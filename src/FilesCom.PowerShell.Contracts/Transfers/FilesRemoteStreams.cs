using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// A remote file opened to read by Open-FilesFileReadStream: a forward-only stream of its bytes, downloaded behind it
    /// by the Files.com engine, bound to the version there when it was opened where its storage allows, as
    /// Receive-FilesFile describes, and read ahead of the caller by at most the parts in progress and 1 MiB.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It goes on reading with the connection and workspace it was opened with after the command that opened it has
    /// returned. A failure of the download reaches the caller as it reads, once it has read everything before it, as a
    /// FilesException; closing the connection stops the download, and a read then fails with Files.ConnectionClosed.
    /// </para>
    /// <para>
    /// It does not seek and has no length or position. Dispose it when done, or before its end to stop the download:
    /// disposing returns once the download has stopped.
    /// </para>
    /// </remarks>
    public sealed class FilesRemoteReadStream : Stream
    {
        private readonly Stream stream;
        private readonly CancellationTokenSource lifetime;
        private readonly Func<Exception, FilesException> engineFailure;
        private int disposed;

        // stream is the engine's; lifetime stops its download, and is cancelled when the connection closes; engineFailure
        // translates the engine's own exceptions, returning null for any other.
        internal FilesRemoteReadStream(string path, FilesConnection connection, Stream stream, CancellationTokenSource lifetime, Func<Exception, FilesException> engineFailure)
        {
            Path = path;
            Connection = connection;
            this.stream = stream;
            this.lifetime = lifetime;
            this.engineFailure = engineFailure;
        }

        /// <summary>The remote file, as it was given.</summary>
        public string Path { get; }

        /// <summary>The connection the file is read with.</summary>
        public FilesConnection Connection { get; }

        /// <inheritdoc/>
        public override bool CanRead => Volatile.Read(ref disposed) == 0;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <summary>Not supported: a remote file is read forward only.</summary>
        public override long Length => throw RemoteStreams.ForwardOnly("read");

        /// <summary>Not supported: a remote file is read forward only.</summary>
        public override long Position
        {
            get => throw RemoteStreams.ForwardOnly("read");
            set => throw RemoteStreams.ForwardOnly("read");
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            try
            {
                return stream.Read(buffer, offset, count);
            }
            catch (Exception failure) when (!RemoteStreams.PassesThrough(failure, CancellationToken.None))
            {
                throw Failure(failure);
            }
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            try
            {
                return await stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception failure) when (!RemoteStreams.PassesThrough(failure, cancellationToken))
            {
                throw Failure(failure);
            }
        }

        /// <summary>Does nothing: a stream opened to read has nothing to flush.</summary>
        public override void Flush()
        {
        }

        /// <summary>Not supported: a remote file is read forward only.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw RemoteStreams.ForwardOnly("read");

        /// <summary>Not supported: a remote file opened to read cannot be changed.</summary>
        public override void SetLength(long value) => throw new NotSupportedException("A remote file opened to read cannot be changed.");

        /// <summary>Not supported: a remote file opened to read cannot be written.</summary>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("A remote file opened to read cannot be written.");

        /// <summary>Stops the download, if it has not ended, and returns once it has stopped.</summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && Interlocked.Exchange(ref disposed, 1) == 0)
                {
                    try
                    {
                        // The engine's stream stops its download; a failure of the download went to reads.
                        stream.Dispose();
                    }
                    catch (Exception failure)
                    {
                        throw RemoteStreams.Translate(failure, engineFailure, Connection, $"The download of '{Path}'");
                    }
                    finally
                    {
                        lifetime.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(FilesRemoteReadStream));
            }
        }

        private Exception Failure(Exception failure)
        {
            // Disposed from another thread while a read waited, which stopped the download.
            if (failure is ObjectDisposedException || (failure is OperationCanceledException && Volatile.Read(ref disposed) != 0))
            {
                return new ObjectDisposedException(nameof(FilesRemoteReadStream), "The stream was closed while it was being read.");
            }
            return RemoteStreams.Translate(failure, engineFailure, Connection, $"The download of '{Path}'");
        }
    }

    /// <summary>
    /// A remote file opened to write by Open-FilesFileWriteStream: a forward-only stream whose bytes the Files.com engine
    /// uploads, while they are written, as a file of unknown length. Closing or disposing it ends the file, waits until
    /// the upload has completed and throws the upload's failure, if it failed. Abandon leaves the file uncompleted.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A write waits while the upload is 1 MiB behind. It goes on uploading with the connection and workspace it was
    /// opened with after the command that opened it has returned. A failure of the upload reaches the caller as a
    /// FilesException, from the write that finds it, which also closes the stream, or from closing it.
    /// </para>
    /// <para>
    /// Closing the stream completes the file with what was written, including when the code writing it failed: to leave
    /// the file as it was, call Abandon first. Closing the connection before then stops the upload and the file is not
    /// completed; closing it while the file is being completed leaves unknown whether it was.
    /// </para>
    /// </remarks>
    public sealed class FilesRemoteWriteStream : Stream
    {
        private readonly Stream stream;
        private readonly CancellationTokenSource lifetime;
        private readonly Func<Exception, FilesException> engineFailure;
        private readonly object gate = new object();
        private int disposed;
        private int abandoned;

        // stream is the engine's; lifetime abandons its upload, and is cancelled when the connection closes; engineFailure
        // translates the engine's own exceptions, returning null for any other.
        internal FilesRemoteWriteStream(string path, FilesConnection connection, Stream stream, CancellationTokenSource lifetime, Func<Exception, FilesException> engineFailure)
        {
            Path = path;
            Connection = connection;
            this.stream = stream;
            this.lifetime = lifetime;
            this.engineFailure = engineFailure;
        }

        /// <summary>The remote file, as it was given, created or replaced once the stream is closed.</summary>
        public string Path { get; }

        /// <summary>The connection the file is written with.</summary>
        public FilesConnection Connection { get; }

        /// <summary>Whether Abandon was called: the file is then not completed.</summary>
        public bool IsAbandoned => Volatile.Read(ref abandoned) != 0;

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => Volatile.Read(ref disposed) == 0;

        /// <summary>Not supported: a remote file is written forward only.</summary>
        public override long Length => throw RemoteStreams.ForwardOnly("written");

        /// <summary>Not supported: a remote file is written forward only.</summary>
        public override long Position
        {
            get => throw RemoteStreams.ForwardOnly("written");
            set => throw RemoteStreams.ForwardOnly("written");
        }

        /// <summary>
        /// Abandons the upload: nothing more is sent, the file is not completed, and closing the stream then returns
        /// without an error, even when the upload had already failed. Call it before closing the stream, such as when the
        /// code writing it fails; once closing the stream has begun it does nothing.
        /// </summary>
        public void Abandon()
        {
            // Under the gate that closing the stream takes first, so that the upload is stopped before closing can find
            // it abandoned.
            lock (gate)
            {
                if (Volatile.Read(ref disposed) != 0 || Interlocked.Exchange(ref abandoned, 1) != 0)
                {
                    return;
                }
                try
                {
                    lifetime.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Ended meanwhile, by a write that found the upload ended.
                }
                catch (AggregateException)
                {
                    // A callback registered on the token failed; the upload is abandoned all the same.
                }
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfCannotWrite();
            try
            {
                stream.Write(buffer, offset, count);
            }
            catch (Exception failure) when (!RemoteStreams.PassesThrough(failure, CancellationToken.None))
            {
                throw WriteFailure(failure);
            }
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfCannotWrite();
            try
            {
                await stream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception failure) when (!RemoteStreams.PassesThrough(failure, cancellationToken))
            {
                throw WriteFailure(failure);
            }
        }

        /// <summary>Does nothing: what is written goes to the upload as it is written.</summary>
        public override void Flush()
        {
        }

        /// <summary>Not supported: a remote file opened to write cannot be read.</summary>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException("A remote file opened to write cannot be read.");

        /// <summary>Not supported: a remote file is written forward only.</summary>
        public override long Seek(long offset, SeekOrigin origin) => throw RemoteStreams.ForwardOnly("written");

        /// <summary>Not supported: a remote file is written forward only.</summary>
        public override void SetLength(long value) => throw RemoteStreams.ForwardOnly("written");

        /// <summary>
        /// Ends the file with what was written and waits until its upload has completed, throwing the upload's failure
        /// if it failed; once the stream is abandoned, returns without completing the file, and without an error.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            bool closing;
            bool abandonedBefore;
            // Abandon stops the upload under the same gate, and does nothing once closing began: an upload abandoned by
            // now was stopped before closing began, and never has its file completed.
            lock (gate)
            {
                closing = disposing && Interlocked.Exchange(ref disposed, 1) == 0;
                abandonedBefore = Volatile.Read(ref abandoned) != 0;
            }
            if (!closing)
            {
                base.Dispose(disposing);
                return;
            }
            bool stoppedBefore = lifetime.IsCancellationRequested;
            try
            {
                try
                {
                    stream.Dispose();
                }
                catch (Exception failure)
                {
                    Exception outcome = Outcome(failure);
                    // An upload that failed before it was abandoned changes nothing the caller asked for: the file is not
                    // completed, as abandoning asks, and closing stays quiet, so that the caller's own failure is the one
                    // reported. Only an outcome that leaves the file's completion unknown is still reported.
                    if (abandonedBefore && !(outcome is FilesException uncertain && uncertain.Kind == FilesErrorKind.TransferCompletionUnknown))
                    {
                        return;
                    }
                    throw outcome;
                }
                // The engine returns quietly from an upload stopped by the token: by Abandon, or by the connection closing.
                if (Volatile.Read(ref abandoned) == 0 && lifetime.IsCancellationRequested)
                {
                    throw stoppedBefore ? ClosedBeforeCompleting() : ClosedWhileCompleting();
                }
            }
            finally
            {
                lifetime.Dispose();
                base.Dispose(disposing);
            }
        }

        // Refuses a write to a closed stream, or to an abandoned upload, whose bytes would never be sent.
        private void ThrowIfCannotWrite()
        {
            if (Volatile.Read(ref disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(FilesRemoteWriteStream));
            }
            if (Volatile.Read(ref abandoned) != 0)
            {
                throw Abandoned();
            }
        }

        // The engine fails a write only once its upload has ended: it failed, was abandoned or lost its connection. The
        // stream then ends too, with the upload's own outcome, which waiting for the ended upload gives; nothing more is
        // sent, and closing the stream afterwards reports nothing more.
        private Exception WriteFailure(Exception failure)
        {
            if (failure is ObjectDisposedException)
            {
                return new ObjectDisposedException(nameof(FilesRemoteWriteStream));
            }
            Exception outcome = failure;
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                try
                {
                    stream.Dispose();
                }
                catch (Exception ended)
                {
                    outcome = ended;
                }
                finally
                {
                    lifetime.Dispose();
                }
            }
            return Outcome(outcome);
        }

        private Exception Outcome(Exception failure)
        {
            // A write waiting for room when the upload stopped reading finds the pipe gone rather than the reason.
            bool stopped = failure is OperationCanceledException || failure is IOException;
            if (stopped && Volatile.Read(ref abandoned) != 0)
            {
                return Abandoned();
            }
            if (stopped && !Connection.IsOpen)
            {
                return ClosedBeforeCompleting();
            }
            return RemoteStreams.Translate(failure, engineFailure, Connection, $"The upload of '{Path}'");
        }

        private FilesException Abandoned()
        {
            return new FilesException(FilesErrorKind.Cancelled, $"The upload of '{Path}' was abandoned: nothing more is sent, and the file is not completed.");
        }

        private FilesException ClosedBeforeCompleting()
        {
            return new FilesException(FilesErrorKind.ConnectionClosed, $"The connection to {Connection.BaseUrl} was closed before '{Path}' was completed, which stopped its upload: the file was not completed.");
        }

        private FilesException ClosedWhileCompleting()
        {
            return new FilesException(FilesErrorKind.ConnectionClosed, $"The connection to {Connection.BaseUrl} was closed while '{Path}' was being completed, so whether the file was completed is unknown. Check the remote file.")
            {
                RequestMayHaveBeenProcessed = true,
            };
        }
    }

    // What both remote file streams do with a failure of the engine's stream after it was opened.
    internal static class RemoteStreams
    {
        internal static NotSupportedException ForwardOnly(string use)
        {
            return new NotSupportedException($"A remote file is {use} forward only: it has no length or position and does not seek.");
        }

        // The stream's own argument and unsupported-member errors, and the caller's own cancellation, reach the caller
        // unchanged; every other failure is translated.
        internal static bool PassesThrough(Exception failure, CancellationToken callerToken)
        {
            return failure is ArgumentException
                || failure is NotSupportedException
                || (failure is OperationCanceledException && callerToken.IsCancellationRequested);
        }

        // An SDK failure as the engine's translation; the transfer stopped by the connection closing, or otherwise
        // cancelled, as such; any other failure of the transfer as a connection failure. FilesException keeps no URL
        // credentials in its text, and no engine exception is kept as an inner exception.
        internal static Exception Translate(Exception failure, Func<Exception, FilesException> engineFailure, FilesConnection connection, string transfer)
        {
            if (failure is OperationCanceledException)
            {
                return connection.IsOpen
                    ? new FilesException(FilesErrorKind.Cancelled, $"{transfer} was cancelled.")
                    : new FilesException(FilesErrorKind.ConnectionClosed, $"{transfer} stopped: the connection to {connection.BaseUrl} was closed.");
            }
            return engineFailure(failure) ?? new FilesException(FilesErrorKind.Connection, $"{transfer} failed: {failure.Message}");
        }
    }
}
