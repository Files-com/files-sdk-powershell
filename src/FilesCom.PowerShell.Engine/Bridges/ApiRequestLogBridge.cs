using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ApiRequestLog operations with the SDK and projects the SDK's ApiRequestLog into the module's.
    internal static class ApiRequestLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ApiRequestLog Project(Sdk.ApiRequestLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ApiRequestLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                ApiKeyId = source.ApiKeyId,
                ApiKeyPrefix = source.ApiKeyPrefix,
                UserId = source.UserId,
                Username = source.Username,
                UserIsFromParentSite = source.UserIsFromParentSite,
                Interface = source.Interface,
                RequestMethod = source.RequestMethod,
                RequestPath = source.RequestPath,
                RequestIp = source.RequestIp,
                RequestHost = source.RequestHost,
                RequestId = source.RequestId,
                ApiName = source.ApiName,
                UserAgent = source.UserAgent,
                ErrorType = source.ErrorType,
                ErrorMessage = source.ErrorMessage,
                ResponseCode = source.ResponseCode,
                Success = source.Success,
                DurationMs = source.DurationMs,
                ImpersonatorUserId = source.ImpersonatorUserId,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.ApiRequestLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ApiRequestLogs.List(parameters, requestOptions), source => ApiRequestLogBridge.Project(source, connection, options));
        }
    }
}
