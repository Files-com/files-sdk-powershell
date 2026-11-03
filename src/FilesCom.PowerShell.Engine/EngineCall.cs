using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell.Engine
{
    // Runs one SDK operation with a connection's client: the request options for the command's workspace, and the
    // SDK's exceptions translated to FilesException so no SDK type reaches the commands.
    internal static class EngineCall
    {
        internal static FilesClient Client(FilesConnection connection)
        {
            return (FilesClient)connection.EngineClient;
        }

        internal static async Task<TResult> RunAsync<TSdk, TResult>(
            FilesConnection connection,
            FilesCallOptions options,
            CancellationToken cancellationToken,
            Func<FilesClient, Dictionary<string, object>, CancellationToken, Task<TSdk>> operation,
            Func<TSdk, TResult> project)
        {
            FilesClient client = Client(connection);
            try
            {
                return project(await operation(client, RequestOptions(options), cancellationToken).ConfigureAwait(false));
            }
            catch (SdkException failure)
            {
                throw ErrorTranslation.Translate(failure);
            }
        }

        internal static async Task RunAsync(
            FilesConnection connection,
            FilesCallOptions options,
            CancellationToken cancellationToken,
            Func<FilesClient, Dictionary<string, object>, CancellationToken, Task> operation)
        {
            FilesClient client = Client(connection);
            try
            {
                await operation(client, RequestOptions(options), cancellationToken).ConfigureAwait(false);
            }
            catch (SdkException failure)
            {
                throw ErrorTranslation.Translate(failure);
            }
        }

        // A list query's pages, loaded one at a time with the connection's client.
        internal static FilesPageSource<TResult> Pages<TSdk, TResult>(
            FilesConnection connection,
            FilesCallOptions options,
            Func<FilesClient, Dictionary<string, object>, FilesList<TSdk>> open,
            Func<TSdk, TResult> project)
            where TResult : FilesObject
        {
            FilesClient client = Client(connection);
            try
            {
                return new PageSource<TSdk, TResult>(connection, open(client, RequestOptions(options)), project);
            }
            catch (SdkException failure)
            {
                throw ErrorTranslation.Translate(failure);
            }
        }

        // The SDK's request options for a command's workspace choice; null to use the connection's.
        internal static Dictionary<string, object> RequestOptions(FilesCallOptions options)
        {
            if (options.ClearWorkspace)
            {
                return new Dictionary<string, object> { ["workspace_id"] = "" };
            }
            if (options.Workspace.HasValue)
            {
                return new Dictionary<string, object> { ["workspace_id"] = options.Workspace.Value.ToString(CultureInfo.InvariantCulture) };
            }
            return null;
        }
    }
}
