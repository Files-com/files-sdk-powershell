using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SyncRun operations with the SDK and projects the SDK's SyncRun into the module's.
    internal static class SyncRunBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SyncRun Project(Sdk.SyncRun source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SyncRun
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Body = source.Body,
                BytesSynced = source.BytesSynced,
                ComparedFiles = source.ComparedFiles,
                ComparedFolders = source.ComparedFolders,
                CompletedAt = Values.Utc(source.CompletedAt),
                CreatedAt = Values.Utc(source.CreatedAt),
                DestRemoteServerType = source.DestRemoteServerType,
                DryRun = source.DryRun,
                ErroredFiles = source.ErroredFiles,
                EstimatedBytesCount = source.EstimatedBytesCount,
                EventErrors = Values.Copy(source.EventErrors),
                LogUrl = source.LogUrl,
                Runtime = source.Runtime,
                SiteId = source.SiteId,
                WorkspaceId = source.WorkspaceId,
                SrcRemoteServerType = source.SrcRemoteServerType,
                Status = source.Status,
                SuccessfulFiles = source.SuccessfulFiles,
                SyncId = source.SyncId,
                SyncName = source.SyncName,
                UpdatedAt = Values.Utc(source.UpdatedAt),
                LiveTransfers = Values.Array(source.LiveTransfers, item => SyncRunLiveTransferBridge.Project(item, connection, options)),
            };
        }

        internal static FilesPageSource<Models.SyncRun> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SyncRuns.List(parameters, requestOptions), source => SyncRunBridge.Project(source, connection, options));
        }

        internal static Task<Models.SyncRun> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SyncRuns.FindAsync(id, parameters, requestOptions, token), source => SyncRunBridge.Project(source, connection, options));
        }
    }
}
