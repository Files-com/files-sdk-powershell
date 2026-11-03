using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs IntegrationCentricProfile operations with the SDK and projects the SDK's IntegrationCentricProfile into the module's.
    internal static class IntegrationCentricProfileBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.IntegrationCentricProfile Project(Sdk.IntegrationCentricProfile source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.IntegrationCentricProfile
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                WorkspaceId = source.WorkspaceId,
                UseForAllUsers = source.UseForAllUsers,
                ExpectedRemoteServers = Values.JsonArray(source.ExpectedRemoteServers),
            };
        }

        internal static FilesPageSource<Models.IntegrationCentricProfile> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.IntegrationCentricProfiles.List(parameters, requestOptions), source => IntegrationCentricProfileBridge.Project(source, connection, options));
        }

        internal static Task<Models.IntegrationCentricProfile> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.IntegrationCentricProfiles.FindAsync(id, parameters, requestOptions, token), source => IntegrationCentricProfileBridge.Project(source, connection, options));
        }

        internal static Task<Models.IntegrationCentricProfile> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.IntegrationCentricProfiles.CreateAsync(parameters, requestOptions, token), source => IntegrationCentricProfileBridge.Project(source, connection, options));
        }

        internal static Task<Models.IntegrationCentricProfile> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.IntegrationCentricProfile model ? model.UpdateAsync(parameters, token) : client.IntegrationCentricProfiles.UpdateAsync(id, parameters, requestOptions, token), source => IntegrationCentricProfileBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.IntegrationCentricProfile model ? model.DeleteAsync(parameters, token) : client.IntegrationCentricProfiles.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
