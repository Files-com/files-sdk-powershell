using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs AutomationVersion operations with the SDK and projects the SDK's AutomationVersion into the module's.
    internal static class AutomationVersionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AutomationVersion Project(Sdk.AutomationVersion source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AutomationVersion
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                AutomationId = source.AutomationId,
                WorkspaceId = source.WorkspaceId,
                Version = source.Version,
                CreatedAt = Values.Utc(source.CreatedAt),
                UserId = source.UserId,
                Username = source.Username,
                ApiKeyId = source.ApiKeyId,
                Definition = Values.Json(source.Definition),
            };
        }

        internal static FilesPageSource<Models.AutomationVersion> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.AutomationVersions.List(parameters, requestOptions), source => AutomationVersionBridge.Project(source, connection, options));
        }

        internal static Task<Models.AutomationVersion> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.AutomationVersions.FindAsync(id, parameters, requestOptions, token), source => AutomationVersionBridge.Project(source, connection, options));
        }

        internal static Task<Models.Automation> RestoreAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.AutomationVersion model ? model.RestoreAsync(parameters, token) : client.AutomationVersions.RestoreAsync(id, parameters, requestOptions, token), source => AutomationBridge.Project(source, connection, options));
        }
    }
}
