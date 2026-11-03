using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs RemoteServerCredential operations with the SDK and projects the SDK's RemoteServerCredential into the module's.
    internal static class RemoteServerCredentialBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.RemoteServerCredential Project(Sdk.RemoteServerCredential source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.RemoteServerCredential
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Name = source.Name,
                Description = source.Description,
                ServerType = source.ServerType,
                AwsAccessKey = source.AwsAccessKey,
                S3AssumeRoleArn = source.S3AssumeRoleArn,
                S3AssumeRoleDurationSeconds = source.S3AssumeRoleDurationSeconds,
                S3AssumeRoleExternalId = source.S3AssumeRoleExternalId,
                GoogleCloudStorageS3CompatibleAccessKey = source.GoogleCloudStorageS3CompatibleAccessKey,
                WasabiAccessKey = source.WasabiAccessKey,
                S3CompatibleAccessKey = source.S3CompatibleAccessKey,
                FilebaseAccessKey = source.FilebaseAccessKey,
                CloudflareAccessKey = source.CloudflareAccessKey,
                LinodeAccessKey = source.LinodeAccessKey,
                SharepointTenantId = source.SharepointTenantId,
                SharepointClientId = source.SharepointClientId,
                SharepointAppCredentialType = source.SharepointAppCredentialType,
                Username = source.Username,
            };
        }

        internal static FilesPageSource<Models.RemoteServerCredential> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.RemoteServerCredentials.List(parameters, requestOptions), source => RemoteServerCredentialBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteServerCredential> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteServerCredentials.FindAsync(id, parameters, requestOptions, token), source => RemoteServerCredentialBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteServerCredential> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteServerCredentials.CreateAsync(parameters, requestOptions, token), source => RemoteServerCredentialBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteServerCredential> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteServerCredential model ? model.UpdateAsync(parameters, token) : client.RemoteServerCredentials.UpdateAsync(id, parameters, requestOptions, token), source => RemoteServerCredentialBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteServerCredential model ? model.DeleteAsync(parameters, token) : client.RemoteServerCredentials.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
