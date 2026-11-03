using System;
using System.IO;
using System.Threading;

namespace FilesCom.PowerShell.Tests
{
    /// <summary>
    /// A stream that gives its content forward only, as a process's output does: it cannot seek or tell its length or
    /// position, and each read returns at most chunkSize bytes, after readDelayMilliseconds when given. Once failAfter
    /// bytes have been read, when it is not negative, reads fail with failure, or an IOException, as a broken source
    /// would.
    /// </summary>
    public sealed class ForwardOnlyStream : Stream
    {
        private readonly byte[] content;
        private readonly int chunkSize;
        private readonly long failAfter;
        private readonly int readDelayMilliseconds;
        private readonly Exception failure;
        private long read;
        private int disposed;
        private int canReadCalls;
        private int seekingCalls;

        public ForwardOnlyStream(byte[] content, int chunkSize = 1000, long failAfter = -1, int readDelayMilliseconds = 0, Exception failure = null)
        {
            this.content = content;
            this.chunkSize = chunkSize;
            this.failAfter = failAfter;
            this.readDelayMilliseconds = readDelayMilliseconds;
            this.failure = failure;
        }

        /// <summary>The bytes read so far.</summary>
        public long BytesRead => Interlocked.Read(ref read);

        /// <summary>Whether the stream has been disposed.</summary>
        public bool IsDisposed => Volatile.Read(ref disposed) == 1;

        /// <summary>When set, CanRead throws it once it has answered <see cref="CanReadCallsBeforeFailure"/> times.</summary>
        public Exception CanReadFailure { get; set; }

        /// <summary>How many calls of CanRead succeed before <see cref="CanReadFailure"/> is thrown.</summary>
        public int CanReadCallsBeforeFailure { get; set; }

        /// <summary>The number of times CanRead was asked.</summary>
        public int CanReadCalls => Volatile.Read(ref canReadCalls);

        /// <summary>The number of times Length, Position, CanSeek or Seek was used, which a forward-only reader never needs.</summary>
        public int SeekingCalls => Volatile.Read(ref seekingCalls);

        public override bool CanRead
        {
            get
            {
                int call = Interlocked.Increment(ref canReadCalls);
                if (CanReadFailure != null && call > CanReadCallsBeforeFailure)
                {
                    throw CanReadFailure;
                }
                return !IsDisposed;
            }
        }

        public override bool CanSeek
        {
            get
            {
                Interlocked.Increment(ref seekingCalls);
                return false;
            }
        }

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                Interlocked.Increment(ref seekingCalls);
                throw new NotSupportedException("The stream's length is not known.");
            }
        }

        public override long Position
        {
            get
            {
                Interlocked.Increment(ref seekingCalls);
                throw new NotSupportedException("The stream cannot tell its position.");
            }

            set
            {
                Interlocked.Increment(ref seekingCalls);
                throw new NotSupportedException("The stream cannot seek.");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ForwardOnlyStream));
            }
            if (readDelayMilliseconds > 0)
            {
                Thread.Sleep(readDelayMilliseconds);
            }
            long position = BytesRead;
            if (failAfter >= 0 && position >= failAfter)
            {
                throw failure ?? new IOException("The source failed.");
            }
            int available = (int)Math.Min(Math.Min(count, chunkSize), content.Length - position);
            if (failAfter >= 0)
            {
                available = (int)Math.Min(available, failAfter - position);
            }
            Array.Copy(content, position, buffer, offset, available);
            Interlocked.Add(ref read, available);
            return available;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Interlocked.Increment(ref seekingCalls);
            throw new NotSupportedException("The stream cannot seek.");
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            Interlocked.Exchange(ref disposed, 1);
            base.Dispose(disposing);
        }
    }
}
