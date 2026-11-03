using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ChildSiteManagementPolicy operations with the SDK and projects the SDK's ChildSiteManagementPolicy into the module's.
    internal static class ChildSiteManagementPolicyBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ChildSiteManagementPolicy Project(Sdk.ChildSiteManagementPolicy source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ChildSiteManagementPolicy
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                PolicyType = source.PolicyType,
                Name = source.Name,
                Description = source.Description,
                Value = Values.Json(source.Value),
                AppliedChildSiteIds = Values.Copy(source.AppliedChildSiteIds),
                SkipChildSiteIds = Values.Copy(source.SkipChildSiteIds),
                ChildSiteIds = Values.Copy(source.ChildSiteIds),
                DefaultPolicy = source.DefaultPolicy,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.ChildSiteManagementPolicy> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ChildSiteManagementPolicies.List(parameters, requestOptions), source => ChildSiteManagementPolicyBridge.Project(source, connection, options));
        }

        internal static Task<Models.ChildSiteManagementPolicy> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ChildSiteManagementPolicies.FindAsync(id, parameters, requestOptions, token), source => ChildSiteManagementPolicyBridge.Project(source, connection, options));
        }

        internal static Task<Models.ChildSiteManagementPolicy> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ChildSiteManagementPolicies.CreateAsync(parameters, requestOptions, token), source => ChildSiteManagementPolicyBridge.Project(source, connection, options));
        }

        internal static Task<Models.ChildSiteManagementPolicy> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ChildSiteManagementPolicy model ? model.UpdateAsync(parameters, token) : client.ChildSiteManagementPolicies.UpdateAsync(id, parameters, requestOptions, token), source => ChildSiteManagementPolicyBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ChildSiteManagementPolicy model ? model.DeleteAsync(parameters, token) : client.ChildSiteManagementPolicies.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
