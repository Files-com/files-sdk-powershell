using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs RemoteServer operations with the SDK and projects the SDK's RemoteServer into the module's.
    internal static class RemoteServerBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.RemoteServer Project(Sdk.RemoteServer source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.RemoteServer
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Disabled = source.Disabled,
                AuthenticationMethod = source.AuthenticationMethod,
                Hostname = source.Hostname,
                RemoteHomePath = source.RemoteHomePath,
                UploadStagingPath = source.UploadStagingPath,
                AllowRelativePaths = source.AllowRelativePaths,
                Name = source.Name,
                Description = source.Description,
                Port = source.Port,
                BufferUploads = source.BufferUploads,
                MaxConnections = source.MaxConnections,
                PinToSiteRegion = source.PinToSiteRegion,
                PinnedRegion = source.PinnedRegion,
                RemoteServerCredentialId = source.RemoteServerCredentialId,
                S3Bucket = source.S3Bucket,
                S3Region = source.S3Region,
                S3KmsKeyId = source.S3KmsKeyId,
                AwsAccessKey = source.AwsAccessKey,
                S3AssumeRoleArn = source.S3AssumeRoleArn,
                S3AssumeRoleDurationSeconds = source.S3AssumeRoleDurationSeconds,
                S3AssumeRoleExternalId = source.S3AssumeRoleExternalId,
                ServerCertificate = source.ServerCertificate,
                ServerHostKey = source.ServerHostKey,
                ServerType = source.ServerType,
                SiteAdminAccess = source.SiteAdminAccess,
                EffectiveSiteAdminAccess = source.EffectiveSiteAdminAccess,
                UserId = source.UserId,
                WorkspaceId = source.WorkspaceId,
                Ssl = source.Ssl,
                Username = source.Username,
                GoogleCloudStorageBucket = source.GoogleCloudStorageBucket,
                GoogleCloudStorageAuthenticationMethod = source.GoogleCloudStorageAuthenticationMethod,
                GoogleCloudStorageOauthScope = source.GoogleCloudStorageOauthScope,
                GoogleCloudStorageProjectId = source.GoogleCloudStorageProjectId,
                GoogleCloudStorageS3CompatibleAccessKey = source.GoogleCloudStorageS3CompatibleAccessKey,
                BackblazeB2S3Endpoint = source.BackblazeB2S3Endpoint,
                BackblazeB2Bucket = source.BackblazeB2Bucket,
                WasabiBucket = source.WasabiBucket,
                WasabiRegion = source.WasabiRegion,
                WasabiAccessKey = source.WasabiAccessKey,
                AuthStatus = source.AuthStatus,
                AuthAccountName = source.AuthAccountName,
                OneDriveAccountType = source.OneDriveAccountType,
                SharepointTenantId = source.SharepointTenantId,
                SharepointClientId = source.SharepointClientId,
                SharepointAppAuthentication = source.SharepointAppAuthentication,
                SharepointAppCredentialType = source.SharepointAppCredentialType,
                SharepointSiteUrl = source.SharepointSiteUrl,
                AzureBlobStorageAccount = source.AzureBlobStorageAccount,
                AzureBlobStorageContainer = source.AzureBlobStorageContainer,
                AzureBlobStorageHierarchicalNamespace = source.AzureBlobStorageHierarchicalNamespace,
                AzureBlobStorageDnsSuffix = source.AzureBlobStorageDnsSuffix,
                AzureFilesStorageAccount = source.AzureFilesStorageAccount,
                AzureFilesStorageShareName = source.AzureFilesStorageShareName,
                AzureFilesStorageDnsSuffix = source.AzureFilesStorageDnsSuffix,
                S3CompatibleBucket = source.S3CompatibleBucket,
                S3CompatibleEndpoint = source.S3CompatibleEndpoint,
                S3CompatibleRegion = source.S3CompatibleRegion,
                S3CompatibleVirtualHostedStyle = source.S3CompatibleVirtualHostedStyle,
                S3CompatibleAccessKey = source.S3CompatibleAccessKey,
                EnableDedicatedIps = source.EnableDedicatedIps,
                CustomDomainId = source.CustomDomainId,
                OutboundIpAddresses = Values.Copy(source.OutboundIpAddresses),
                FilesAgentPermissionSet = source.FilesAgentPermissionSet,
                FilesAgentRoot = source.FilesAgentRoot,
                FilesAgentVersion = source.FilesAgentVersion,
                FilesAgentUpToDate = source.FilesAgentUpToDate,
                FilesAgentLatestVersion = source.FilesAgentLatestVersion,
                FilesAgentSupportsPushUpdates = source.FilesAgentSupportsPushUpdates,
                DirectTransferAvailable = source.DirectTransferAvailable,
                OutboundAgentId = source.OutboundAgentId,
                FilebaseBucket = source.FilebaseBucket,
                FilebaseAccessKey = source.FilebaseAccessKey,
                FilesApiKeyPrefix = source.FilesApiKeyPrefix,
                CloudflareBucket = source.CloudflareBucket,
                CloudflareAccessKey = source.CloudflareAccessKey,
                CloudflareEndpoint = source.CloudflareEndpoint,
                DropboxTeams = source.DropboxTeams,
                LinodeBucket = source.LinodeBucket,
                LinodeAccessKey = source.LinodeAccessKey,
                LinodeRegion = source.LinodeRegion,
                SupportsVersioning = source.SupportsVersioning,
            };
        }

        internal static FilesPageSource<Models.RemoteServer> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.RemoteServers.List(parameters, requestOptions), source => RemoteServerBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteServer> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteServers.FindAsync(id, parameters, requestOptions, token), source => RemoteServerBridge.Project(source, connection, options));
        }

        internal static Task<Models.AgentNode> AgentNodesAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteServer model ? model.AgentNodesAsync(parameters, token) : client.RemoteServers.AgentNodesAsync(id, parameters, requestOptions, token), source => AgentNodeBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteServerConfigurationFile> FindConfigurationFileAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteServers.FindConfigurationFileAsync(id, parameters, requestOptions, token), source => RemoteServerConfigurationFileBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteServer> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.RemoteServers.CreateAsync(parameters, requestOptions, token), source => RemoteServerBridge.Project(source, connection, options));
        }

        internal static Task<Models.AgentPushUpdate> AgentPushUpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteServer model ? model.AgentPushUpdateAsync(parameters, token) : client.RemoteServers.AgentPushUpdateAsync(id, parameters, requestOptions, token), source => AgentPushUpdateBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteServer> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteServer model ? model.UpdateAsync(parameters, token) : client.RemoteServers.UpdateAsync(id, parameters, requestOptions, token), source => RemoteServerBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.RemoteServer model ? model.DeleteAsync(parameters, token) : client.RemoteServers.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
