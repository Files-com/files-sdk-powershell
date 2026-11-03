using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Request operations with the SDK and projects the SDK's Request into the module's.
    internal static class RequestBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Request Project(Sdk.Request source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Request
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Path = source.Path,
                Source = source.Source,
                Destination = source.Destination,
                AutomationId = source.AutomationId,
                UserDisplayName = source.UserDisplayName,
            };
        }

        internal static FilesPageSource<Models.Request> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Requests.List(parameters, requestOptions), source => RequestBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.Request> GetFolder(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Requests.GetFolder(path, parameters, requestOptions), source => RequestBridge.Project(source, connection, options));
        }

        internal static Task<Models.Request> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Requests.CreateAsync(parameters, requestOptions, token), source => RequestBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Request model ? model.DeleteAsync(parameters, token) : client.Requests.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
