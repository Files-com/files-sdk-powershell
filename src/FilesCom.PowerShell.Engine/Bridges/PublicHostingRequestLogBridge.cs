using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs PublicHostingRequestLog operations with the SDK and projects the SDK's PublicHostingRequestLog into the module's.
    internal static class PublicHostingRequestLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PublicHostingRequestLog Project(Sdk.PublicHostingRequestLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PublicHostingRequestLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                RemoteIp = source.RemoteIp,
                ServerIp = source.ServerIp,
                Hostname = source.Hostname,
                Path = source.Path,
                ResponseCode = source.ResponseCode,
                Success = source.Success,
                DurationMs = source.DurationMs,
                CreatedAt = Values.Utc(source.CreatedAt),
                BytesTransferred = source.BytesTransferred,
                HttpMethod = source.HttpMethod,
            };
        }

        internal static FilesPageSource<Models.PublicHostingRequestLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.PublicHostingRequestLogs.List(parameters, requestOptions), source => PublicHostingRequestLogBridge.Project(source, connection, options));
        }
    }
}
