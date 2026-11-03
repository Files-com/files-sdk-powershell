using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Secret operations with the SDK and projects the SDK's Secret into the module's.
    internal static class SecretBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Secret Project(Sdk.Secret source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Secret
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Name = source.Name,
                Description = source.Description,
                SecretType = source.SecretType,
                Metadata = Values.Json(source.Metadata),
                ValueFieldNames = Values.Copy(source.ValueFieldNames),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.Secret> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Secrets.List(parameters, requestOptions), source => SecretBridge.Project(source, connection, options));
        }

        internal static Task<Models.Secret> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Secrets.FindAsync(id, parameters, requestOptions, token), source => SecretBridge.Project(source, connection, options));
        }

        internal static Task<Models.Secret> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Secrets.CreateAsync(parameters, requestOptions, token), source => SecretBridge.Project(source, connection, options));
        }

        internal static Task<Models.Secret> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Secret model ? model.UpdateAsync(parameters, token) : client.Secrets.UpdateAsync(id, parameters, requestOptions, token), source => SecretBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Secret model ? model.DeleteAsync(parameters, token) : client.Secrets.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
