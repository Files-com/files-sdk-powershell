using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs EventSubscription operations with the SDK and projects the SDK's EventSubscription into the module's.
    internal static class EventSubscriptionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.EventSubscription Project(Sdk.EventSubscription source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.EventSubscription
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                EventChannelId = source.EventChannelId,
                WorkspaceId = source.WorkspaceId,
                ApplyToAllWorkspaces = source.ApplyToAllWorkspaces,
                Name = source.Name,
                Subject = source.Subject,
                Message = source.Message,
                MessageOnly = source.MessageOnly,
                Enabled = source.Enabled,
                EventTypes = Values.Copy(source.EventTypes),
                Filter = Values.Json(source.Filter),
                DeliveryPolicy = Values.Json(source.DeliveryPolicy),
                EventTargetIds = Values.Copy(source.EventTargetIds),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.EventSubscription> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.EventSubscriptions.List(parameters, requestOptions), source => EventSubscriptionBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventSubscription> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.EventSubscriptions.FindAsync(id, parameters, requestOptions, token), source => EventSubscriptionBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventSubscription> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.EventSubscriptions.CreateAsync(parameters, requestOptions, token), source => EventSubscriptionBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventSubscription> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.EventSubscription model ? model.UpdateAsync(parameters, token) : client.EventSubscriptions.UpdateAsync(id, parameters, requestOptions, token), source => EventSubscriptionBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.EventSubscription model ? model.DeleteAsync(parameters, token) : client.EventSubscriptions.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
