using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ExternalEvent operations with the SDK and projects the SDK's ExternalEvent into the module's.
    internal static class ExternalEventBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ExternalEvent Project(Sdk.ExternalEvent source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ExternalEvent
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                EventType = source.EventType,
                Status = source.Status,
                Body = source.Body,
                CreatedAt = Values.Utc(source.CreatedAt),
                BodyUrl = source.BodyUrl,
            };
        }

        internal static FilesPageSource<Models.ExternalEvent> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ExternalEvents.List(parameters, requestOptions), source => ExternalEventBridge.Project(source, connection, options));
        }

        internal static Task<Models.ExternalEvent> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ExternalEvents.FindAsync(id, parameters, requestOptions, token), source => ExternalEventBridge.Project(source, connection, options));
        }

        internal static Task<Models.ExternalEvent> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ExternalEvents.CreateAsync(parameters, requestOptions, token), source => ExternalEventBridge.Project(source, connection, options));
        }
    }
}
