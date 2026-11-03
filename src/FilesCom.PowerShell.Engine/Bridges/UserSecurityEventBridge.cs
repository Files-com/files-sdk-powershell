using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs UserSecurityEvent operations with the SDK and projects the SDK's UserSecurityEvent into the module's.
    internal static class UserSecurityEventBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UserSecurityEvent Project(Sdk.UserSecurityEvent source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UserSecurityEvent
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                EventType = source.EventType,
                Body = source.Body,
                EventErrors = Values.Copy(source.EventErrors),
                CreatedAt = Values.Utc(source.CreatedAt),
                BodyUrl = source.BodyUrl,
                UserId = source.UserId,
            };
        }

        internal static FilesPageSource<Models.UserSecurityEvent> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.UserSecurityEvents.List(parameters, requestOptions), source => UserSecurityEventBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserSecurityEvent> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.UserSecurityEvents.FindAsync(id, parameters, requestOptions, token), source => UserSecurityEventBridge.Project(source, connection, options));
        }
    }
}
