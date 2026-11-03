using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs KeyLifecycleRule operations with the SDK and projects the SDK's KeyLifecycleRule into the module's.
    internal static class KeyLifecycleRuleBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.KeyLifecycleRule Project(Sdk.KeyLifecycleRule source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.KeyLifecycleRule
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                KeyType = source.KeyType,
                InactivityDays = source.InactivityDays,
                ExpirationDays = source.ExpirationDays,
                ApplyToAllWorkspaces = source.ApplyToAllWorkspaces,
                Name = source.Name,
                WorkspaceId = source.WorkspaceId,
            };
        }

        internal static FilesPageSource<Models.KeyLifecycleRule> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.KeyLifecycleRules.List(parameters, requestOptions), source => KeyLifecycleRuleBridge.Project(source, connection, options));
        }

        internal static Task<Models.KeyLifecycleRule> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.KeyLifecycleRules.FindAsync(id, parameters, requestOptions, token), source => KeyLifecycleRuleBridge.Project(source, connection, options));
        }

        internal static Task<Models.KeyLifecycleRule> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.KeyLifecycleRules.CreateAsync(parameters, requestOptions, token), source => KeyLifecycleRuleBridge.Project(source, connection, options));
        }

        internal static Task<Models.KeyLifecycleRule> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.KeyLifecycleRule model ? model.UpdateAsync(parameters, token) : client.KeyLifecycleRules.UpdateAsync(id, parameters, requestOptions, token), source => KeyLifecycleRuleBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.KeyLifecycleRule model ? model.DeleteAsync(parameters, token) : client.KeyLifecycleRules.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
