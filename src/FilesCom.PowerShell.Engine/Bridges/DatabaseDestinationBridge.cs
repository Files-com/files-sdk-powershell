using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs DatabaseDestination operations with the SDK and projects the SDK's DatabaseDestination into the module's.
    internal static class DatabaseDestinationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.DatabaseDestination Project(Sdk.DatabaseDestination source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.DatabaseDestination
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Name = source.Name,
                Provider = source.Provider,
                Config = Values.Json(source.Config),
                SecretId = source.SecretId,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.DatabaseDestination> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.DatabaseDestinations.List(parameters, requestOptions), source => DatabaseDestinationBridge.Project(source, connection, options));
        }

        internal static Task<Models.DatabaseDestination> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.DatabaseDestinations.FindAsync(id, parameters, requestOptions, token), source => DatabaseDestinationBridge.Project(source, connection, options));
        }

        internal static Task<Models.DatabaseDestination> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.DatabaseDestinations.CreateAsync(parameters, requestOptions, token), source => DatabaseDestinationBridge.Project(source, connection, options));
        }

        internal static Task<Models.DatabaseDestination> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.DatabaseDestination model ? model.UpdateAsync(parameters, token) : client.DatabaseDestinations.UpdateAsync(id, parameters, requestOptions, token), source => DatabaseDestinationBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.DatabaseDestination model ? model.DeleteAsync(parameters, token) : client.DatabaseDestinations.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
