using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SyncLog operations with the SDK and projects the SDK's SyncLog into the module's.
    internal static class SyncLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SyncLog Project(Sdk.SyncLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SyncLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                SyncId = source.SyncId,
                ExternalEventId = source.ExternalEventId,
                SyncRunId = source.SyncRunId,
                ErrorType = source.ErrorType,
                Message = source.Message,
                Operation = source.Operation,
                Path = source.Path,
                Size = source.Size,
                FileType = source.FileType,
                Status = source.Status,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.SyncLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SyncLogs.List(parameters, requestOptions), source => SyncLogBridge.Project(source, connection, options));
        }
    }
}
