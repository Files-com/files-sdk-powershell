using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ApiKey operations with the SDK and projects the SDK's ApiKey into the module's.
    internal static class ApiKeyBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ApiKey Project(Sdk.ApiKey source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ApiKey
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                DescriptiveLabel = source.DescriptiveLabel,
                Description = source.Description,
                CreatedAt = Values.Utc(source.CreatedAt),
                ExpiresAt = Values.Utc(source.ExpiresAt),
                Key = Values.Secret(source.Key),
                AwsStyleCredentials = source.AwsStyleCredentials,
                AwsAccessKeyId = source.AwsAccessKeyId,
                AwsSecretKey = Values.Secret(source.AwsSecretKey),
                LastUseAt = Values.Utc(source.LastUseAt),
                Name = source.Name,
                PermissionSet = source.PermissionSet,
                Platform = source.Platform,
                SiteId = source.SiteId,
                SiteName = source.SiteName,
                Url = source.Url,
                UserId = source.UserId,
                WorkspaceId = source.WorkspaceId,
            };
        }

        internal static FilesPageSource<Models.ApiKey> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ApiKeys.List(parameters, requestOptions), source => ApiKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.ApiKey> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ApiKeys.FindAsync(id, parameters, requestOptions, token), source => ApiKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.ApiKey> FindCurrentAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ApiKeys.FindCurrentAsync(parameters, requestOptions, token), source => ApiKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.ApiKey> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ApiKeys.CreateAsync(parameters, requestOptions, token), source => ApiKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.ApiKey> UpdateCurrentAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ApiKeys.UpdateCurrentAsync(parameters, requestOptions, token), source => ApiKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.ApiKey> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ApiKey model ? model.UpdateAsync(parameters, token) : client.ApiKeys.UpdateAsync(id, parameters, requestOptions, token), source => ApiKeyBridge.Project(source, connection, options));
        }

        internal static Task DeleteCurrentAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ApiKeys.DeleteCurrentAsync(parameters, requestOptions, token));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ApiKey model ? model.DeleteAsync(parameters, token) : client.ApiKeys.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
