using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FilesCom.Transfers;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // The engine's helpers for waiting on background file actions and for using remote files the way local ones are:
    // each is one public SDK operation, which does its own requests, retries and polling.
    internal static class HelperBridge
    {
        // Waits until the copy or move a FileAction describes has finished, checking with the client and at the address
        // the action came from, in the workspace its command ran in. Returns the completed migration, or null when the
        // action was complete when Files.com answered.
        internal static Task<Models.FileMigration> WaitForActionAsync(FilesConnection connection, object retainedAction, TimeSpan? pollInterval, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            if (!(retainedAction is Sdk.FileAction action))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "The action was not written by a FilesCom command in this session, such as Copy-FilesFile or Move-FilesFile, so there is nothing to wait for.");
            }
            return WithTimeout(timeout, "The action did not finish within -Timeout. It goes on at Files.com.", cancellationToken, token => EngineCall.RunAsync(
                connection,
                FilesCallOptions.None,
                token,
                (client, requestOptions, callToken) => action.WaitAsync(pollInterval, null, callToken),
                migration => migration == null ? null : FileMigrationBridge.Project(migration, connection, FilesCallOptions.None)));
        }

        // Waits until Files.com has nothing at a path, as after deleting a folder with its contents.
        internal static Task WaitUntilRemovedAsync(FilesConnection connection, string path, TimeSpan? pollInterval, TimeSpan? timeout, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return WithTimeout(timeout, $"Files.com still had something at '{path}' after -Timeout.", cancellationToken, async token =>
            {
                await EngineCall.RunAsync(connection, options, token, (client, requestOptions, callToken) => client.RemoteFiles.WaitUntilGoneAsync(path, pollInterval, requestOptions, callToken)).ConfigureAwait(false);
                return true;
            });
        }

        internal static Task<bool> FileExistsAsync(FilesConnection connection, string path, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FileSystem.FileExistsAsync(path, requestOptions, token), exists => exists);
        }

        internal static Task<bool> FolderExistsAsync(FilesConnection connection, string path, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FileSystem.DirectoryExistsAsync(path, requestOptions, token), exists => exists);
        }

        // Reads a whole remote file with the engine's download, bound to one version of it where its storage allows.
        internal static Task<byte[]> ReadAllBytesAsync(FilesConnection connection, string path, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FileSystem.ReadAllBytesAsync(path, new DownloadOptions { RequestOptions = requestOptions }, token), bytes => bytes);
        }

        // Writes bytes as a whole remote file, creating or replacing it, with the engine's upload.
        internal static Task WriteAllBytesAsync(FilesConnection connection, string path, byte[] bytes, bool? mkdirParents, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FileSystem.WriteAllBytesAsync(path, bytes, new UploadOptions { RequestOptions = requestOptions, MkdirParents = mkdirParents }, token));
        }

        // Opens a remote file to read as a stream that outlives the command: its download runs with the connection's
        // client and the command's workspace until the stream is disposed or the connection closes, and what the engine
        // throws after opening is translated as the stream is read. Stopping the command while it opens stops the download.
        internal static async Task<FilesRemoteReadStream> OpenReadAsync(FilesConnection connection, string path, int? maxConcurrency, long? partSize, FilesCallOptions options, CancellationToken cancellationToken)
        {
            var lifetime = CancellationTokenSource.CreateLinkedTokenSource(connection.Lifetime);
            try
            {
                Stream stream = await Opening(lifetime, cancellationToken, () => EngineCall.RunAsync(
                    connection,
                    options,
                    lifetime.Token,
                    (client, requestOptions, token) => client.FileSystem.OpenReadAsync(path, new DownloadOptions { RequestOptions = requestOptions, MaxConcurrency = maxConcurrency, PartSize = partSize }, token),
                    opened => opened)).ConfigureAwait(false);
                return new FilesRemoteReadStream(path, connection, stream, lifetime, StreamFailure);
            }
            catch
            {
                lifetime.Dispose();
                throw;
            }
        }

        // Opens a remote file to write as a stream that outlives the command, uploading with the connection's client and
        // the command's workspace. Nothing reaches Files.com before this is called; cancelling lifetime, which Abandon and
        // the connection closing do, abandons the upload.
        internal static async Task<FilesRemoteWriteStream> OpenWriteAsync(FilesConnection connection, string path, bool? mkdirParents, FilesCallOptions options, CancellationToken cancellationToken)
        {
            var lifetime = CancellationTokenSource.CreateLinkedTokenSource(connection.Lifetime);
            try
            {
                Stream stream = await Opening(lifetime, cancellationToken, () => EngineCall.RunAsync(
                    connection,
                    options,
                    lifetime.Token,
                    (client, requestOptions, token) => client.FileSystem.OpenWriteAsync(path, new UploadOptions { RequestOptions = requestOptions, MkdirParents = mkdirParents }, token),
                    opened => opened)).ConfigureAwait(false);
                return new FilesRemoteWriteStream(path, connection, stream, lifetime, StreamFailure);
            }
            catch
            {
                lifetime.Dispose();
                throw;
            }
        }

        // Opens a stream whose transfer lifetime stops, with the command's cancellation reaching it only while it opens.
        // A stream opened as the command was cancelled is disposed, its transfer already stopped, rather than returned.
        private static async Task<Stream> Opening(CancellationTokenSource lifetime, CancellationToken cancellationToken, Func<Task<Stream>> open)
        {
            Stream stream;
            using (cancellationToken.Register(lifetime.Cancel))
            {
                stream = await open().ConfigureAwait(false);
            }
            if (lifetime.IsCancellationRequested)
            {
                try
                {
                    // Its transfer is already stopped: a stream opened to write is abandoned, not completed.
                    stream.Dispose();
                }
                catch (Exception)
                {
                    // The cancellation is what the command reports.
                }
                throw new OperationCanceledException(lifetime.Token);
            }
            return stream;
        }

        // What the engine's streams throw after opening: the SDK's exceptions translated, or null for any other.
        private static FilesException StreamFailure(Exception failure)
        {
            return failure is SdkException sdk ? ErrorTranslation.Translate(sdk) : null;
        }

        // Runs an operation that ends with OperationTimeout, rather than as cancelled, when its own timeout passes first.
        private static async Task<T> WithTimeout<T>(TimeSpan? timeout, string message, CancellationToken cancellationToken, Func<CancellationToken, Task<T>> operation)
        {
            if (!timeout.HasValue)
            {
                return await operation(cancellationToken).ConfigureAwait(false);
            }
            using (var timer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                timer.CancelAfter(timeout.Value);
                try
                {
                    return await operation(timer.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (timer.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    throw new FilesException(FilesErrorKind.OperationTimeout, message) { Recommendation = "Run Wait-FilesFileAction again, with a longer -Timeout, to go on waiting." };
                }
            }
        }
    }
}
