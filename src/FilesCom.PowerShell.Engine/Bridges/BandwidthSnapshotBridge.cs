using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs BandwidthSnapshot operations with the SDK and projects the SDK's BandwidthSnapshot into the module's.
    internal static class BandwidthSnapshotBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.BandwidthSnapshot Project(Sdk.BandwidthSnapshot source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.BandwidthSnapshot
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                BytesReceived = source.BytesReceived,
                BytesSent = source.BytesSent,
                SyncBytesReceived = source.SyncBytesReceived,
                SyncBytesSent = source.SyncBytesSent,
                RequestsGet = source.RequestsGet,
                RequestsPut = source.RequestsPut,
                RequestsOther = source.RequestsOther,
                LoggedAt = Values.Utc(source.LoggedAt),
            };
        }

        internal static FilesPageSource<Models.BandwidthSnapshot> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.BandwidthSnapshots.List(parameters, requestOptions), source => BandwidthSnapshotBridge.Project(source, connection, options));
        }
    }
}
