using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs PendingWorkEvent operations with the SDK and projects the SDK's PendingWorkEvent into the module's.
    internal static class PendingWorkEventBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PendingWorkEvent Project(Sdk.PendingWorkEvent source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PendingWorkEvent
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
                FolderBehaviorId = source.FolderBehaviorId,
            };
        }

        internal static FilesPageSource<Models.PendingWorkEvent> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.PendingWorkEvents.List(parameters, requestOptions), source => PendingWorkEventBridge.Project(source, connection, options));
        }

        internal static Task<Models.PendingWorkEvent> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PendingWorkEvents.FindAsync(id, parameters, requestOptions, token), source => PendingWorkEventBridge.Project(source, connection, options));
        }
    }
}
