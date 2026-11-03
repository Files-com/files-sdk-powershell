using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SiemHttpDestinationEvent operations with the SDK and projects the SDK's SiemHttpDestinationEvent into the module's.
    internal static class SiemHttpDestinationEventBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SiemHttpDestinationEvent Project(Sdk.SiemHttpDestinationEvent source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SiemHttpDestinationEvent
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                EventType = source.EventType,
                Status = source.Status,
                Body = source.Body,
                EventErrors = Values.Copy(source.EventErrors),
                CreatedAt = Values.Utc(source.CreatedAt),
                BodyUrl = source.BodyUrl,
                SiemHttpDestinationId = source.SiemHttpDestinationId,
            };
        }

        internal static FilesPageSource<Models.SiemHttpDestinationEvent> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SiemHttpDestinationEvents.List(parameters, requestOptions), source => SiemHttpDestinationEventBridge.Project(source, connection, options));
        }

        internal static Task<Models.SiemHttpDestinationEvent> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SiemHttpDestinationEvents.FindAsync(id, parameters, requestOptions, token), source => SiemHttpDestinationEventBridge.Project(source, connection, options));
        }
    }
}
