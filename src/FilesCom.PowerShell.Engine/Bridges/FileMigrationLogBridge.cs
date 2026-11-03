using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs FileMigrationLog operations with the SDK and projects the SDK's FileMigrationLog into the module's.
    internal static class FileMigrationLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FileMigrationLog Project(Sdk.FileMigrationLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FileMigrationLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                FileMigrationId = source.FileMigrationId,
                DestPath = source.DestPath,
                ErrorType = source.ErrorType,
                Message = source.Message,
                Operation = source.Operation,
                Path = source.Path,
                Status = source.Status,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.FileMigrationLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.FileMigrationLogs.List(parameters, requestOptions), source => FileMigrationLogBridge.Project(source, connection, options));
        }
    }
}
