using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SsoEvent operations with the SDK and projects the SDK's SsoEvent into the module's.
    internal static class SsoEventBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SsoEvent Project(Sdk.SsoEvent source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SsoEvent
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
                UserId = source.UserId,
                Username = source.Username,
                IdpUid = source.IdpUid,
                Provider = source.Provider,
                ProviderLabel = source.ProviderLabel,
                Ip = source.Ip,
                Region = source.Region,
            };
        }

        internal static FilesPageSource<Models.SsoEvent> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SsoEvents.List(parameters, requestOptions), source => SsoEventBridge.Project(source, connection, options));
        }

        internal static Task<Models.SsoEvent> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SsoEvents.FindAsync(id, parameters, requestOptions, token), source => SsoEventBridge.Project(source, connection, options));
        }
    }
}
