using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ScimLog operations with the SDK and projects the SDK's ScimLog into the module's.
    internal static class ScimLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ScimLog Project(Sdk.ScimLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ScimLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                RequestPath = source.RequestPath,
                RequestMethod = source.RequestMethod,
                HttpResponseCode = source.HttpResponseCode,
                UserAgent = source.UserAgent,
                RequestJson = source.RequestJson,
                ResponseJson = source.ResponseJson,
            };
        }

        internal static FilesPageSource<Models.ScimLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ScimLogs.List(parameters, requestOptions), source => ScimLogBridge.Project(source, connection, options));
        }

        internal static Task<Models.ScimLog> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ScimLogs.FindAsync(id, parameters, requestOptions, token), source => ScimLogBridge.Project(source, connection, options));
        }
    }
}
