using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ExavaultApiRequestLog operations with the SDK and projects the SDK's ExavaultApiRequestLog into the module's.
    internal static class ExavaultApiRequestLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ExavaultApiRequestLog Project(Sdk.ExavaultApiRequestLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ExavaultApiRequestLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                Endpoint = source.Endpoint,
                Version = source.Version,
                RequestIp = source.RequestIp,
                RequestMethod = source.RequestMethod,
                ErrorType = source.ErrorType,
                ErrorMessage = source.ErrorMessage,
                UserAgent = source.UserAgent,
                ResponseCode = source.ResponseCode,
                Success = source.Success,
                DurationMs = source.DurationMs,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.ExavaultApiRequestLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ExavaultApiRequestLogs.List(parameters, requestOptions), source => ExavaultApiRequestLogBridge.Project(source, connection, options));
        }
    }
}
