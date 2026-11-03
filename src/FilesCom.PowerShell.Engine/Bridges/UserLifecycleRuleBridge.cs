using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs UserLifecycleRule operations with the SDK and projects the SDK's UserLifecycleRule into the module's.
    internal static class UserLifecycleRuleBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UserLifecycleRule Project(Sdk.UserLifecycleRule source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UserLifecycleRule
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                AuthenticationMethod = source.AuthenticationMethod,
                GroupIds = Values.Copy(source.GroupIds),
                Action = source.Action,
                InactivityDays = source.InactivityDays,
                IncludeFolderAdmins = source.IncludeFolderAdmins,
                IncludeSiteAdmins = source.IncludeSiteAdmins,
                ApplyToAllWorkspaces = source.ApplyToAllWorkspaces,
                Name = source.Name,
                NotifyUsers = source.NotifyUsers,
                PartnerTag = source.PartnerTag,
                SiteId = source.SiteId,
                WorkspaceId = source.WorkspaceId,
                UserState = source.UserState,
                UserTag = source.UserTag,
            };
        }

        internal static FilesPageSource<Models.UserLifecycleRule> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.UserLifecycleRules.List(parameters, requestOptions), source => UserLifecycleRuleBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserLifecycleRule> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.UserLifecycleRules.FindAsync(id, parameters, requestOptions, token), source => UserLifecycleRuleBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserLifecycleRule> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.UserLifecycleRules.CreateAsync(parameters, requestOptions, token), source => UserLifecycleRuleBridge.Project(source, connection, options));
        }

        internal static Task<Models.UserLifecycleRule> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.UserLifecycleRule model ? model.UpdateAsync(parameters, token) : client.UserLifecycleRules.UpdateAsync(id, parameters, requestOptions, token), source => UserLifecycleRuleBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.UserLifecycleRule model ? model.DeleteAsync(parameters, token) : client.UserLifecycleRules.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
