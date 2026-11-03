using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs FileMigration operations with the SDK and projects the SDK's FileMigration into the module's.
    internal static class FileMigrationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FileMigration Project(Sdk.FileMigration source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FileMigration
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Path = source.Path,
                DestPath = source.DestPath,
                FailureMessage = source.FailureMessage,
                FilesMoved = source.FilesMoved,
                FilesTotal = source.FilesTotal,
                Operation = source.Operation,
                Region = source.Region,
                Status = source.Status,
                LogUrl = source.LogUrl,
            };
        }

        internal static Task<Models.FileMigration> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FileMigrations.FindAsync(id, parameters, requestOptions, token), source => FileMigrationBridge.Project(source, connection, options));
        }
    }
}
