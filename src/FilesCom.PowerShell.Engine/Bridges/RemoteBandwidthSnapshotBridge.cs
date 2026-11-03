using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs RemoteBandwidthSnapshot operations with the SDK and projects the SDK's RemoteBandwidthSnapshot into the module's.
    internal static class RemoteBandwidthSnapshotBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.RemoteBandwidthSnapshot Project(Sdk.RemoteBandwidthSnapshot source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.RemoteBandwidthSnapshot
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                SyncBytesReceived = source.SyncBytesReceived,
                SyncBytesSent = source.SyncBytesSent,
                LoggedAt = Values.Utc(source.LoggedAt),
                RemoteServerId = source.RemoteServerId,
            };
        }

        internal static FilesPageSource<Models.RemoteBandwidthSnapshot> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.RemoteBandwidthSnapshots.List(parameters, requestOptions), source => RemoteBandwidthSnapshotBridge.Project(source, connection, options));
        }
    }
}
