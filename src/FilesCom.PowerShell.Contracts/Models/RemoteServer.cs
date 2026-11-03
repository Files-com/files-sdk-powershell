using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A RemoteServer is a specific type of Behavior called `remote_server_sync`.
    /// </summary>
    public sealed class RemoteServer : FilesObject
    {
        internal RemoteServer()
        {
        }

        /// <summary>
        /// Remote Server ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// If true, this Remote Server is disabled. Updating it clears this flag, except for retired Agent v1 records, which remain disabled.
        /// </summary>
        public bool Disabled { get; internal set; }

        /// <summary>
        /// Type of authentication method to use
        /// </summary>
        public string AuthenticationMethod { get; internal set; }

        /// <summary>
        /// Hostname or IP address. For Agent Remote Servers, the Agent reports the hostname it connects from at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not change the running Agent.
        /// </summary>
        public string Hostname { get; internal set; }

        /// <summary>
        /// Initial home folder on remote server
        /// </summary>
        public string RemoteHomePath { get; internal set; }

        /// <summary>
        /// Upload staging path.  Applies to SFTP only.  If a path is provided here, files will first be uploaded to this path on the remote folder and the moved into the final correct path via an SFTP move command.  This is required by some remote MFT systems to emulate atomic uploads, which are otherwise not supoprted by SFTP.
        /// </summary>
        public string UploadStagingPath { get; internal set; }

        /// <summary>
        /// Allow relative paths in SFTP. If true, paths will not be forced to be absolute, allowing operations relative to the user's home directory.
        /// </summary>
        public bool AllowRelativePaths { get; internal set; }

        /// <summary>
        /// Internal name for your reference
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Internal description for your reference
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Port for remote server. For Agent Remote Servers, the Agent reports its downstream proxy port at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not change the running Agent.
        /// </summary>
        public long? Port { get; internal set; }

        /// <summary>
        /// If set to always, uploads to this server will be uploaded first to Files.com before being sent to the remote server. This can improve performance in certain access patterns, such as high-latency connections.  It will cause data to be temporarily stored in Files.com. If set to auto, we will perform this optimization if we believe it to be a benefit in a given situation.
        /// </summary>
        public string BufferUploads { get; internal set; }

        /// <summary>
        /// Max number of parallel connections.  Ignored for S3 connections (we will parallelize these as much as possible).
        /// </summary>
        public long? MaxConnections { get; internal set; }

        /// <summary>
        /// If true, we will ensure that all communications with this remote server are made through the primary region of the site.  This setting can also be overridden by a site-wide setting which will force it to true.
        /// </summary>
        public bool PinToSiteRegion { get; internal set; }

        /// <summary>
        /// If set, all communications with this remote server are made through the provided region.
        /// </summary>
        public string PinnedRegion { get; internal set; }

        /// <summary>
        /// ID of Remote Server Credential, if applicable.
        /// </summary>
        public long? RemoteServerCredentialId { get; internal set; }

        /// <summary>
        /// S3 bucket name
        /// </summary>
        public string S3Bucket { get; internal set; }

        /// <summary>
        /// S3 region
        /// </summary>
        public string S3Region { get; internal set; }

        /// <summary>
        /// ARN of the AWS KMS key that encrypts files written to the bucket (SSE-KMS). Leave blank unless your bucket requires a specific key.
        /// </summary>
        public string S3KmsKeyId { get; internal set; }

        /// <summary>
        /// AWS Access Key.
        /// </summary>
        public string AwsAccessKey { get; internal set; }

        /// <summary>
        /// AWS IAM Role ARN for AssumeRole authentication.
        /// </summary>
        public string S3AssumeRoleArn { get; internal set; }

        /// <summary>
        /// Session duration in seconds for AssumeRole authentication (900-43200).
        /// </summary>
        public long? S3AssumeRoleDurationSeconds { get; internal set; }

        /// <summary>
        /// External ID for AssumeRole authentication.
        /// </summary>
        public string S3AssumeRoleExternalId { get; internal set; }

        /// <summary>
        /// Remote server certificate
        /// </summary>
        public string ServerCertificate { get; internal set; }

        /// <summary>
        /// Pinned SSH host key or OpenSSH host certificate for SFTP. If omitted, Files.com detects and stores a host key, preferring plain keys over certificates. With `server_certificate=require_match` (the default), the server must present the exact pinned key or certificate and prove it holds the matching private key. A pinned certificate is compared in full, so renewal can require updating `server_host_key` even when its underlying key is unchanged. Files.com does not check certificate CA signatures, principals, or validity periods. Certificate expiration alone does not end the pin. Update `server_host_key` to replace the pin.
        /// </summary>
        public string ServerHostKey { get; internal set; }

        /// <summary>
        /// Remote server type.
        /// </summary>
        public string ServerType { get; internal set; }

        /// <summary>
        /// Owner's preference for administrator access to this user-owned Remote Server. Null uses the Site policy's default. Only the owner may change it when the Site policy allows user choice.
        /// </summary>
        public bool SiteAdminAccess { get; internal set; }

        /// <summary>
        /// Whether Site and Workspace Administrators can access this Remote Server under the current Site policy.
        /// </summary>
        public bool EffectiveSiteAdminAccess { get; internal set; }

        /// <summary>
        /// ID of the Integration-Centric user who owns this personal Remote Server, if any. Connections created by Site and Workspace Administrators have no owner.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Workspace ID (0 for default workspace)
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Should we require SSL?
        /// </summary>
        public string Ssl { get; internal set; }

        /// <summary>
        /// Remote server username.
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Google Cloud Storage: Bucket Name
        /// </summary>
        public string GoogleCloudStorageBucket { get; internal set; }

        /// <summary>
        /// Google Cloud Storage: Authentication method. Can be json, hmac, or oauth.
        /// </summary>
        public string GoogleCloudStorageAuthenticationMethod { get; internal set; }

        /// <summary>
        /// Google Cloud Storage: OAuth scope. Can be https://www.googleapis.com/auth/devstorage.read_only or https://www.googleapis.com/auth/devstorage.read_write.
        /// </summary>
        public string GoogleCloudStorageOauthScope { get; internal set; }

        /// <summary>
        /// Google Cloud Storage: Project ID
        /// </summary>
        public string GoogleCloudStorageProjectId { get; internal set; }

        /// <summary>
        /// Google Cloud Storage: S3-compatible Access Key.
        /// </summary>
        public string GoogleCloudStorageS3CompatibleAccessKey { get; internal set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: S3 Endpoint
        /// </summary>
        public string BackblazeB2S3Endpoint { get; internal set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: Bucket name
        /// </summary>
        public string BackblazeB2Bucket { get; internal set; }

        /// <summary>
        /// Wasabi: Bucket name
        /// </summary>
        public string WasabiBucket { get; internal set; }

        /// <summary>
        /// Wasabi: Region
        /// </summary>
        public string WasabiRegion { get; internal set; }

        /// <summary>
        /// Wasabi: Access Key.
        /// </summary>
        public string WasabiAccessKey { get; internal set; }

        /// <summary>
        /// Either `in_setup` or `complete`
        /// </summary>
        public string AuthStatus { get; internal set; }

        /// <summary>
        /// Describes the authorized account
        /// </summary>
        public string AuthAccountName { get; internal set; }

        /// <summary>
        /// OneDrive: Either personal or business_other account types
        /// </summary>
        public string OneDriveAccountType { get; internal set; }

        /// <summary>
        /// SharePoint: Microsoft Entra tenant ID for app-only authentication.
        /// </summary>
        public string SharepointTenantId { get; internal set; }

        /// <summary>
        /// SharePoint: Microsoft Entra application client ID for app-only authentication.
        /// </summary>
        public string SharepointClientId { get; internal set; }

        /// <summary>
        /// SharePoint: If true, this remote server uses Microsoft Entra app-only authentication.
        /// </summary>
        public bool SharepointAppAuthentication { get; internal set; }

        /// <summary>
        /// SharePoint: App-only credential type. Either secret or certificate.
        /// </summary>
        public string SharepointAppCredentialType { get; internal set; }

        /// <summary>
        /// SharePoint: Site URL to scope app-only authentication to a single site. Leave blank to browse all sites.
        /// </summary>
        public string SharepointSiteUrl { get; internal set; }

        /// <summary>
        /// Azure Blob Storage: Account name
        /// </summary>
        public string AzureBlobStorageAccount { get; internal set; }

        /// <summary>
        /// Azure Blob Storage: Container name
        /// </summary>
        public string AzureBlobStorageContainer { get; internal set; }

        /// <summary>
        /// Azure Blob Storage: Does the storage account has hierarchical namespace feature enabled?
        /// </summary>
        public bool AzureBlobStorageHierarchicalNamespace { get; internal set; }

        /// <summary>
        /// Azure Blob Storage: Custom DNS suffix
        /// </summary>
        public string AzureBlobStorageDnsSuffix { get; internal set; }

        /// <summary>
        /// Azure Files: Storage Account name
        /// </summary>
        public string AzureFilesStorageAccount { get; internal set; }

        /// <summary>
        /// Azure Files:  Storage Share name
        /// </summary>
        public string AzureFilesStorageShareName { get; internal set; }

        /// <summary>
        /// Azure Files: Custom DNS suffix
        /// </summary>
        public string AzureFilesStorageDnsSuffix { get; internal set; }

        /// <summary>
        /// S3-compatible: Bucket name
        /// </summary>
        public string S3CompatibleBucket { get; internal set; }

        /// <summary>
        /// S3-compatible: endpoint
        /// </summary>
        public string S3CompatibleEndpoint { get; internal set; }

        /// <summary>
        /// S3-compatible: region
        /// </summary>
        public string S3CompatibleRegion { get; internal set; }

        /// <summary>
        /// S3-compatible: If true, use virtual-hosted-style URLs instead of path-style URLs
        /// </summary>
        public bool S3CompatibleVirtualHostedStyle { get; internal set; }

        /// <summary>
        /// S3-compatible: Access Key
        /// </summary>
        public string S3CompatibleAccessKey { get; internal set; }

        /// <summary>
        /// `true` if remote server only accepts connections from dedicated IPs
        /// </summary>
        public bool EnableDedicatedIps { get; internal set; }

        /// <summary>
        /// Custom Domain ID whose dedicated IP addresses are selected when this Remote Server uses dedicated IPs. Must be available to this Remote Server's workspace. Requires enable_dedicated_ips and cannot be combined with an outbound Agent. Set to null to use the site's default dedicated IPs.
        /// </summary>
        public long? CustomDomainId { get; internal set; }

        /// <summary>
        /// Current eligible public IP addresses for the selected Custom Domain. Any address in this list may originate a connection. Empty when no domain is selected or its configuration is unavailable. Only included in responses for a single Remote Server.
        /// </summary>
        public string[] OutboundIpAddresses { get; internal set; }

        /// <summary>
        /// Agent file permissions: `read_only`, `write_only`, or `read_write`. Files.com writes this value as `permission_set` in the configuration file generated for this Remote Server. Setting it on Create or Update changes the generated file, not a running Agent. The Agent enforces the permissions in its local configuration file, controlled by the host's system administrator, and reports them at registration, replacing the stored value.
        /// </summary>
        public string FilesAgentPermissionSet { get; internal set; }

        /// <summary>
        /// Agent local root path. Files.com writes this value as `root` in the configuration file generated for this Remote Server. Setting it on Create or Update changes the generated file, not a running Agent. The Agent enforces the root in its local configuration file, controlled by the host's system administrator, and reports it at registration, replacing the stored value.
        /// </summary>
        public string FilesAgentRoot { get; internal set; }

        /// <summary>
        /// Files.com Agent version reported by the Agent at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not update the running Agent.
        /// </summary>
        public string FilesAgentVersion { get; internal set; }

        /// <summary>
        /// If true, the Files Agent is up to date.
        /// </summary>
        public bool FilesAgentUpToDate { get; internal set; }

        /// <summary>
        /// Latest available Files Agent version
        /// </summary>
        public string FilesAgentLatestVersion { get; internal set; }

        /// <summary>
        /// Files Agent supports receiving push updates
        /// </summary>
        public bool FilesAgentSupportsPushUpdates { get; internal set; }

        /// <summary>
        /// Whether the Files Agent Proxy recently validated a direct transfer connection. `true` means a direct connection was recently validated (actual availability can vary by client network), `false` means direct transfers are enabled but not currently available, and `null` means direct transfers are disabled or unsupported. Only provided for a connected Files Agent when showing a single Remote Server.
        /// </summary>
        public bool DirectTransferAvailable { get; internal set; }

        /// <summary>
        /// Route traffic to outbound on a files-agent
        /// </summary>
        public long? OutboundAgentId { get; internal set; }

        /// <summary>
        /// Filebase: Bucket name
        /// </summary>
        public string FilebaseBucket { get; internal set; }

        /// <summary>
        /// Filebase: Access Key.
        /// </summary>
        public string FilebaseAccessKey { get; internal set; }

        /// <summary>
        /// Files.com direct link: paired API key prefix.
        /// </summary>
        public string FilesApiKeyPrefix { get; internal set; }

        /// <summary>
        /// Cloudflare: Bucket name
        /// </summary>
        public string CloudflareBucket { get; internal set; }

        /// <summary>
        /// Cloudflare: Access Key.
        /// </summary>
        public string CloudflareAccessKey { get; internal set; }

        /// <summary>
        /// Cloudflare: endpoint
        /// </summary>
        public string CloudflareEndpoint { get; internal set; }

        /// <summary>
        /// Dropbox: If true, list Team folders in root?
        /// </summary>
        public bool DropboxTeams { get; internal set; }

        /// <summary>
        /// Linode: Bucket name
        /// </summary>
        public string LinodeBucket { get; internal set; }

        /// <summary>
        /// Linode: Access Key
        /// </summary>
        public string LinodeAccessKey { get; internal set; }

        /// <summary>
        /// Linode: region
        /// </summary>
        public string LinodeRegion { get; internal set; }

        /// <summary>
        /// If true, this remote server supports file versioning. This value is determined automatically by Files.com.
        /// </summary>
        public bool SupportsVersioning { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(RemoteServer source)
        {
            Id = source.Id;
            Disabled = source.Disabled;
            AuthenticationMethod = source.AuthenticationMethod;
            Hostname = source.Hostname;
            RemoteHomePath = source.RemoteHomePath;
            UploadStagingPath = source.UploadStagingPath;
            AllowRelativePaths = source.AllowRelativePaths;
            Name = source.Name;
            Description = source.Description;
            Port = source.Port;
            BufferUploads = source.BufferUploads;
            MaxConnections = source.MaxConnections;
            PinToSiteRegion = source.PinToSiteRegion;
            PinnedRegion = source.PinnedRegion;
            RemoteServerCredentialId = source.RemoteServerCredentialId;
            S3Bucket = source.S3Bucket;
            S3Region = source.S3Region;
            S3KmsKeyId = source.S3KmsKeyId;
            AwsAccessKey = source.AwsAccessKey;
            S3AssumeRoleArn = source.S3AssumeRoleArn;
            S3AssumeRoleDurationSeconds = source.S3AssumeRoleDurationSeconds;
            S3AssumeRoleExternalId = source.S3AssumeRoleExternalId;
            ServerCertificate = source.ServerCertificate;
            ServerHostKey = source.ServerHostKey;
            ServerType = source.ServerType;
            SiteAdminAccess = source.SiteAdminAccess;
            EffectiveSiteAdminAccess = source.EffectiveSiteAdminAccess;
            UserId = source.UserId;
            WorkspaceId = source.WorkspaceId;
            Ssl = source.Ssl;
            Username = source.Username;
            GoogleCloudStorageBucket = source.GoogleCloudStorageBucket;
            GoogleCloudStorageAuthenticationMethod = source.GoogleCloudStorageAuthenticationMethod;
            GoogleCloudStorageOauthScope = source.GoogleCloudStorageOauthScope;
            GoogleCloudStorageProjectId = source.GoogleCloudStorageProjectId;
            GoogleCloudStorageS3CompatibleAccessKey = source.GoogleCloudStorageS3CompatibleAccessKey;
            BackblazeB2S3Endpoint = source.BackblazeB2S3Endpoint;
            BackblazeB2Bucket = source.BackblazeB2Bucket;
            WasabiBucket = source.WasabiBucket;
            WasabiRegion = source.WasabiRegion;
            WasabiAccessKey = source.WasabiAccessKey;
            AuthStatus = source.AuthStatus;
            AuthAccountName = source.AuthAccountName;
            OneDriveAccountType = source.OneDriveAccountType;
            SharepointTenantId = source.SharepointTenantId;
            SharepointClientId = source.SharepointClientId;
            SharepointAppAuthentication = source.SharepointAppAuthentication;
            SharepointAppCredentialType = source.SharepointAppCredentialType;
            SharepointSiteUrl = source.SharepointSiteUrl;
            AzureBlobStorageAccount = source.AzureBlobStorageAccount;
            AzureBlobStorageContainer = source.AzureBlobStorageContainer;
            AzureBlobStorageHierarchicalNamespace = source.AzureBlobStorageHierarchicalNamespace;
            AzureBlobStorageDnsSuffix = source.AzureBlobStorageDnsSuffix;
            AzureFilesStorageAccount = source.AzureFilesStorageAccount;
            AzureFilesStorageShareName = source.AzureFilesStorageShareName;
            AzureFilesStorageDnsSuffix = source.AzureFilesStorageDnsSuffix;
            S3CompatibleBucket = source.S3CompatibleBucket;
            S3CompatibleEndpoint = source.S3CompatibleEndpoint;
            S3CompatibleRegion = source.S3CompatibleRegion;
            S3CompatibleVirtualHostedStyle = source.S3CompatibleVirtualHostedStyle;
            S3CompatibleAccessKey = source.S3CompatibleAccessKey;
            EnableDedicatedIps = source.EnableDedicatedIps;
            CustomDomainId = source.CustomDomainId;
            OutboundIpAddresses = source.OutboundIpAddresses;
            FilesAgentPermissionSet = source.FilesAgentPermissionSet;
            FilesAgentRoot = source.FilesAgentRoot;
            FilesAgentVersion = source.FilesAgentVersion;
            FilesAgentUpToDate = source.FilesAgentUpToDate;
            FilesAgentLatestVersion = source.FilesAgentLatestVersion;
            FilesAgentSupportsPushUpdates = source.FilesAgentSupportsPushUpdates;
            DirectTransferAvailable = source.DirectTransferAvailable;
            OutboundAgentId = source.OutboundAgentId;
            FilebaseBucket = source.FilebaseBucket;
            FilebaseAccessKey = source.FilebaseAccessKey;
            FilesApiKeyPrefix = source.FilesApiKeyPrefix;
            CloudflareBucket = source.CloudflareBucket;
            CloudflareAccessKey = source.CloudflareAccessKey;
            CloudflareEndpoint = source.CloudflareEndpoint;
            DropboxTeams = source.DropboxTeams;
            LinodeBucket = source.LinodeBucket;
            LinodeAccessKey = source.LinodeAccessKey;
            LinodeRegion = source.LinodeRegion;
            SupportsVersioning = source.SupportsVersioning;
        }
    }
}
