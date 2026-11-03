using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs EventDeliveryAttempt operations with the SDK and projects the SDK's EventDeliveryAttempt into the module's.
    internal static class EventDeliveryAttemptBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.EventDeliveryAttempt Project(Sdk.EventDeliveryAttempt source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.EventDeliveryAttempt
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                EventRecordId = source.EventRecordId,
                EventSubscriptionId = source.EventSubscriptionId,
                EventTargetId = source.EventTargetId,
                ActivityNotificationBatchId = source.ActivityNotificationBatchId,
                WorkspaceId = source.WorkspaceId,
                Status = source.Status,
                AttemptNumber = source.AttemptNumber,
                ResponseCode = source.ResponseCode,
                ErrorMessage = source.ErrorMessage,
                ResponseBody = source.ResponseBody,
                LatencyMs = source.LatencyMs,
                DeliveredAt = Values.Utc(source.DeliveredAt),
                LastAttemptedAt = Values.Utc(source.LastAttemptedAt),
                NextAttemptAt = Values.Utc(source.NextAttemptAt),
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.EventDeliveryAttempt> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.EventDeliveryAttempts.List(parameters, requestOptions), source => EventDeliveryAttemptBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventDeliveryAttempt> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.EventDeliveryAttempts.FindAsync(id, parameters, requestOptions, token), source => EventDeliveryAttemptBridge.Project(source, connection, options));
        }
    }
}
