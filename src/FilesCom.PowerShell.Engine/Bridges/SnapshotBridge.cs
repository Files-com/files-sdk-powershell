using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Snapshot operations with the SDK and projects the SDK's Snapshot into the module's.
    internal static class SnapshotBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Snapshot Project(Sdk.Snapshot source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Snapshot
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                ExpiresAt = Values.Utc(source.ExpiresAt),
                FinalizedAt = Values.Utc(source.FinalizedAt),
                Name = source.Name,
                UserId = source.UserId,
                BundleId = source.BundleId,
                WorkspaceId = source.WorkspaceId,
            };
        }

        internal static FilesPageSource<Models.Snapshot> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Snapshots.List(parameters, requestOptions), source => SnapshotBridge.Project(source, connection, options));
        }

        internal static Task<Models.Snapshot> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Snapshots.FindAsync(id, parameters, requestOptions, token), source => SnapshotBridge.Project(source, connection, options));
        }

        internal static Task<Models.Snapshot> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Snapshots.CreateAsync(parameters, requestOptions, token), source => SnapshotBridge.Project(source, connection, options));
        }

        internal static Task FinalizeAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Snapshot model ? model.FinalizeAsync(parameters, token) : client.Snapshots.FinalizeAsync(id, parameters, requestOptions, token));
        }

        internal static Task<Models.Snapshot> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Snapshot model ? model.UpdateAsync(parameters, token) : client.Snapshots.UpdateAsync(id, parameters, requestOptions, token), source => SnapshotBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Snapshot model ? model.DeleteAsync(parameters, token) : client.Snapshots.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
