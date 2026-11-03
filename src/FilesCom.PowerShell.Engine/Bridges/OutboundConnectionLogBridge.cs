using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs OutboundConnectionLog operations with the SDK and projects the SDK's OutboundConnectionLog into the module's.
    internal static class OutboundConnectionLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.OutboundConnectionLog Project(Sdk.OutboundConnectionLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.OutboundConnectionLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                Path = source.Path,
                ClientIp = source.ClientIp,
                SrcRemoteServerId = source.SrcRemoteServerId,
                SrcRemoteServerName = source.SrcRemoteServerName,
                DestRemoteServerId = source.DestRemoteServerId,
                DestRemoteServerName = source.DestRemoteServerName,
                Operation = source.Operation,
                ErrorMessage = source.ErrorMessage,
                ErrorOperation = source.ErrorOperation,
                ErrorType = source.ErrorType,
                Status = source.Status,
                DurationMs = source.DurationMs,
                BytesUploaded = source.BytesUploaded,
                BytesDownloaded = source.BytesDownloaded,
                ListCount = source.ListCount,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.OutboundConnectionLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.OutboundConnectionLogs.List(parameters, requestOptions), source => OutboundConnectionLogBridge.Project(source, connection, options));
        }
    }
}
