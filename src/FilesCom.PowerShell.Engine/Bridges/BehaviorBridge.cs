using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Behavior operations with the SDK and projects the SDK's Behavior into the module's.
    internal static class BehaviorBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Behavior Project(Sdk.Behavior source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Behavior
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Path = source.Path,
                AttachmentUrl = source.AttachmentUrl,
                BehaviorType = source.BehaviorType,
                Name = source.Name,
                Description = source.Description,
                Value = Values.Json(source.Value),
                PublicHostingUrl = source.PublicHostingUrl,
                DisableParentFolderBehavior = source.DisableParentFolderBehavior,
                Recursive = source.Recursive,
                Inherited = source.Inherited,
                Managed = source.Managed,
                RootBehaviorSiteAdminOnly = source.RootBehaviorSiteAdminOnly,
            };
        }

        internal static FilesPageSource<Models.Behavior> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Behaviors.List(parameters, requestOptions), source => BehaviorBridge.Project(source, connection, options));
        }

        internal static Task<Models.Behavior> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Behaviors.FindAsync(id, parameters, requestOptions, token), source => BehaviorBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.Behavior> ListFor(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Behaviors.ListFor(path, parameters, requestOptions), source => BehaviorBridge.Project(source, connection, options));
        }

        internal static Task<Models.Behavior> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Behaviors.CreateAsync(Attachments.ForEngine(parameters), requestOptions, token), source => BehaviorBridge.Project(source, connection, options));
        }

        internal static Task WebhookTestAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Behaviors.WebhookTestAsync(parameters, requestOptions, token));
        }

        internal static Task<Models.Behavior> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Behavior model ? model.UpdateAsync(Attachments.ForEngine(parameters), token) : client.Behaviors.UpdateAsync(id, Attachments.ForEngine(parameters), requestOptions, token), source => BehaviorBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Behavior model ? model.DeleteAsync(parameters, token) : client.Behaviors.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
