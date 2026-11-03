using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs EventTarget operations with the SDK and projects the SDK's EventTarget into the module's.
    internal static class EventTargetBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.EventTarget Project(Sdk.EventTarget source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.EventTarget
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                TargetType = source.TargetType,
                WorkspaceId = source.WorkspaceId,
                ApplyToAllWorkspaces = source.ApplyToAllWorkspaces,
                Enabled = source.Enabled,
                Config = Values.Json(source.Config),
                DeliveryPolicy = Values.Json(source.DeliveryPolicy),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.EventTarget> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.EventTargets.List(parameters, requestOptions), source => EventTargetBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventTarget> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.EventTargets.FindAsync(id, parameters, requestOptions, token), source => EventTargetBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventTarget> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.EventTargets.CreateAsync(parameters, requestOptions, token), source => EventTargetBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventTarget> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.EventTarget model ? model.UpdateAsync(parameters, token) : client.EventTargets.UpdateAsync(id, parameters, requestOptions, token), source => EventTargetBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.EventTarget model ? model.DeleteAsync(parameters, token) : client.EventTargets.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
