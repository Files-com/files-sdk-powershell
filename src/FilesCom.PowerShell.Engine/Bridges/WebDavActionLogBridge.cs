using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs WebDavActionLog operations with the SDK and projects the SDK's WebDavActionLog into the module's.
    internal static class WebDavActionLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.WebDavActionLog Project(Sdk.WebDavActionLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.WebDavActionLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                RemoteIp = source.RemoteIp,
                ServerIp = source.ServerIp,
                Username = source.Username,
                AuthCiphers = source.AuthCiphers,
                ActionType = source.ActionType,
                Path = source.Path,
                TruePath = source.TruePath,
                Name = source.Name,
                HttpMethod = source.HttpMethod,
                HttpPath = source.HttpPath,
                HttpResponseCode = source.HttpResponseCode,
                Size = source.Size,
                EntriesReturned = source.EntriesReturned,
                Success = source.Success,
                Status = source.Status,
                DurationMs = source.DurationMs,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.WebDavActionLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.WebDavActionLogs.List(parameters, requestOptions), source => WebDavActionLogBridge.Project(source, connection, options));
        }
    }
}
