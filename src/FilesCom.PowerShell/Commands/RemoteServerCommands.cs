using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Remote Servers, or get one remote server by Id.
    /// </summary>
    [Cmdlet("Get", "FilesRemoteServer", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.RemoteServer), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.RemoteServer>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.RemoteServer), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesRemoteServerCommand : FilesListCmdlet<Models.RemoteServer>
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Used for pagination.  When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`.  Send one of those cursor value here to resume an existing list from the next available record.  Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Cursor { get; set; }

        /// <summary>
        /// Number of records to show per page.  (Max: 10000, 1,000 or less is recommended).
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PerPage { get; set; }

        /// <summary>
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `name`, `server_type`, `backblaze_b2_bucket`, `google_cloud_storage_bucket`, `wasabi_bucket`, `s3_bucket`, `azure_blob_storage_container`, `azure_files_storage_share_name`, `s3_compatible_bucket`, `filebase_bucket`, `cloudflare_bucket` or `linode_bucket`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `name`, `server_type`, `workspace_id`, `backblaze_b2_bucket`, `google_cloud_storage_bucket`, `wasabi_bucket`, `s3_bucket`, `azure_blob_storage_container`, `azure_files_storage_share_name`, `s3_compatible_bucket`, `filebase_bucket`, `cloudflare_bucket` or `linode_bucket`. Valid field combinations are `[ server_type, name ]`, `[ workspace_id, name ]`, `[ backblaze_b2_bucket, name ]`, `[ google_cloud_storage_bucket, name ]`, `[ wasabi_bucket, name ]`, `[ s3_bucket, name ]`, `[ azure_blob_storage_container, name ]`, `[ azure_files_storage_share_name, name ]`, `[ s3_compatible_bucket, name ]`, `[ filebase_bucket, name ]`, `[ cloudflare_bucket, name ]`, `[ linode_bucket, name ]`, `[ workspace_id, server_type ]` or `[ workspace_id, server_type, name ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `name`, `backblaze_b2_bucket`, `google_cloud_storage_bucket`, `wasabi_bucket`, `s3_bucket`, `azure_blob_storage_container`, `azure_files_storage_share_name`, `s3_compatible_bucket`, `filebase_bucket`, `cloudflare_bucket` or `linode_bucket`. Valid field combinations are `[ backblaze_b2_bucket, name ]`, `[ google_cloud_storage_bucket, name ]`, `[ wasabi_bucket, name ]`, `[ s3_bucket, name ]`, `[ azure_blob_storage_container, name ]`, `[ azure_files_storage_share_name, name ]`, `[ s3_compatible_bucket, name ]`, `[ filebase_bucket, name ]`, `[ cloudflare_bucket, name ]` or `[ linode_bucket, name ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// Remote Server ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Remote Servers, or get one remote server by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.RemoteServer result = Run(connection, token => RemoteServerBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => RemoteServerBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Cursor)))
            {
                parameters["cursor"] = Cursor;
            }
            if (IsBound(nameof(PerPage)))
            {
                parameters["per_page"] = PerPage.Value;
            }
            if (IsBound(nameof(SortBy)))
            {
                parameters["sort_by"] = RequestValues.QueryObject(SortBy, "SortBy");
            }
            if (IsBound(nameof(Filter)))
            {
                parameters["filter"] = RequestValues.QueryObject(Filter, "Filter");
            }
            if (IsBound(nameof(FilterPrefix)))
            {
                parameters["filter_prefix"] = RequestValues.QueryObject(FilterPrefix, "FilterPrefix");
            }
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// List Files.com Agent nodes.
    /// </summary>
    [Cmdlet("Get", "FilesRemoteServerAgentNode", DefaultParameterSetName = "Id")]
    [OutputType(typeof(Models.AgentNode), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class GetFilesRemoteServerAgentNodeCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Server ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The remote server to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteServer InputObject { get; set; }

        /// <summary>List Files.com Agent nodes.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = AgentNodesParameters();
            Models.AgentNode result = Run(connection, token => RemoteServerBridge.AgentNodesAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> AgentNodesParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Download configuration file (required for some Remote Server integrations, such as the Files.com Agent).
    /// </summary>
    [Cmdlet("Get", "FilesRemoteServerConfigurationFile")]
    [OutputType(typeof(Models.RemoteServerConfigurationFile))]
    public sealed class GetFilesRemoteServerConfigurationFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Server ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>Download configuration file (required for some Remote Server integrations, such as the Files.com Agent).</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            long id = Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = FindConfigurationFileParameters();
            Models.RemoteServerConfigurationFile result = Run(connection, token => RemoteServerBridge.FindConfigurationFileAsync(connection, id, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> FindConfigurationFileParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Remote Server.
    /// </summary>
    [Cmdlet("New", "FilesRemoteServer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.RemoteServer))]
    public sealed class NewFilesRemoteServerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Password, if needed.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Password { get; set; }

        /// <summary>
        /// Private key, if needed.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>
        /// Passphrase for private key if needed.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKeyPassphrase { get; set; }

        /// <summary>
        /// Reset authenticated account?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ResetAuthentication { get; set; }

        /// <summary>
        /// SharePoint: PEM-encoded certificate and unencrypted private key for app-only authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString SharepointClientCertificate { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra application client secret for app-only authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString SharepointClientSecret { get; set; }

        /// <summary>
        /// SSL client certificate.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SslCertificate { get; set; }

        /// <summary>
        /// AWS: secret key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString AwsSecretKey { get; set; }

        /// <summary>
        /// Azure Blob Storage: Access Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString AzureBlobStorageAccessKey { get; set; }

        /// <summary>
        /// Azure Blob Storage: Shared Access Signature (SAS) token
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString AzureBlobStorageSasToken { get; set; }

        /// <summary>
        /// Azure File Storage: Access Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString AzureFilesStorageAccessKey { get; set; }

        /// <summary>
        /// Azure File Storage: Shared Access Signature (SAS) token
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString AzureFilesStorageSasToken { get; set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: applicationKey
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString BackblazeB2ApplicationKey { get; set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: keyID
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string BackblazeB2KeyId { get; set; }

        /// <summary>
        /// Cloudflare: Secret Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString CloudflareSecretKey { get; set; }

        /// <summary>
        /// Filebase: Secret Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString FilebaseSecretKey { get; set; }

        /// <summary>
        /// Google Cloud Storage: JSON file that contains the private key. To generate see https://cloud.google.com/storage/docs/json_api/v1/how-tos/authorizing#APIKey
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString GoogleCloudStorageCredentialsJson { get; set; }

        /// <summary>
        /// Google Cloud Storage: S3-compatible secret key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString GoogleCloudStorageS3CompatibleSecretKey { get; set; }

        /// <summary>
        /// Linode: Secret Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString LinodeSecretKey { get; set; }

        /// <summary>
        /// S3-compatible: Secret Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString S3CompatibleSecretKey { get; set; }

        /// <summary>
        /// Wasabi: Secret Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString WasabiSecretKey { get; set; }

        /// <summary>
        /// Allow relative paths in SFTP. If true, paths will not be forced to be absolute, allowing operations relative to the user's home directory.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowRelativePaths { get; set; }

        /// <summary>
        /// AWS Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AwsAccessKey { get; set; }

        /// <summary>
        /// Azure Blob Storage: Account name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureBlobStorageAccount { get; set; }

        /// <summary>
        /// Azure Blob Storage: Container name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureBlobStorageContainer { get; set; }

        /// <summary>
        /// Azure Blob Storage: Custom DNS suffix
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureBlobStorageDnsSuffix { get; set; }

        /// <summary>
        /// Azure Blob Storage: Does the storage account has hierarchical namespace feature enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AzureBlobStorageHierarchicalNamespace { get; set; }

        /// <summary>
        /// Azure Files: Storage Account name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureFilesStorageAccount { get; set; }

        /// <summary>
        /// Azure Files: Custom DNS suffix
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureFilesStorageDnsSuffix { get; set; }

        /// <summary>
        /// Azure Files:  Storage Share name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureFilesStorageShareName { get; set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: Bucket name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string BackblazeB2Bucket { get; set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: S3 Endpoint
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string BackblazeB2S3Endpoint { get; set; }

        /// <summary>
        /// If set to always, uploads to this server will be uploaded first to Files.com before being sent to the remote server. This can improve performance in certain access patterns, such as high-latency connections.  It will cause data to be temporarily stored in Files.com. If set to auto, we will perform this optimization if we believe it to be a benefit in a given situation.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string BufferUploads { get; set; }

        /// <summary>
        /// Cloudflare: Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CloudflareAccessKey { get; set; }

        /// <summary>
        /// Cloudflare: Bucket name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CloudflareBucket { get; set; }

        /// <summary>
        /// Cloudflare: endpoint
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CloudflareEndpoint { get; set; }

        /// <summary>
        /// Internal description for your reference
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Dropbox: If true, list Team folders in root?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DropboxTeams { get; set; }

        /// <summary>
        /// `true` if remote server only accepts connections from dedicated IPs
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? EnableDedicatedIps { get; set; }

        /// <summary>
        /// Filebase: Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FilebaseAccessKey { get; set; }

        /// <summary>
        /// Filebase: Bucket name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FilebaseBucket { get; set; }

        /// <summary>
        /// Files.com direct link: API key used once to pair the remote server.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString FilesApiKey { get; set; }

        /// <summary>
        /// Agent file permissions: `read_only`, `write_only`, or `read_write`. Files.com writes this value as `permission_set` in the configuration file generated for this Remote Server. Setting it on Create or Update changes the generated file, not a running Agent. The Agent enforces the permissions in its local configuration file, controlled by the host's system administrator, and reports them at registration, replacing the stored value.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FilesAgentPermissionSet { get; set; }

        /// <summary>
        /// Agent local root path. Files.com writes this value as `root` in the configuration file generated for this Remote Server. Setting it on Create or Update changes the generated file, not a running Agent. The Agent enforces the root in its local configuration file, controlled by the host's system administrator, and reports it at registration, replacing the stored value.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FilesAgentRoot { get; set; }

        /// <summary>
        /// Files.com Agent version reported by the Agent at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not update the running Agent.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FilesAgentVersion { get; set; }

        /// <summary>
        /// Route traffic to outbound on a files-agent
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? OutboundAgentId { get; set; }

        /// <summary>
        /// Custom Domain ID whose dedicated IP addresses are selected when this Remote Server uses dedicated IPs. Must be available to this Remote Server's workspace. Requires enable_dedicated_ips and cannot be combined with an outbound Agent. Set to null to use the site's default dedicated IPs.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? CustomDomainId { get; set; }

        /// <summary>
        /// Google Cloud Storage: Authentication method. Can be json, hmac, or oauth.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GoogleCloudStorageAuthenticationMethod { get; set; }

        /// <summary>
        /// Google Cloud Storage: Bucket Name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GoogleCloudStorageBucket { get; set; }

        /// <summary>
        /// Google Cloud Storage: OAuth scope. Can be https://www.googleapis.com/auth/devstorage.read_only or https://www.googleapis.com/auth/devstorage.read_write.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GoogleCloudStorageOauthScope { get; set; }

        /// <summary>
        /// Google Cloud Storage: Project ID
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GoogleCloudStorageProjectId { get; set; }

        /// <summary>
        /// Google Cloud Storage: S3-compatible Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GoogleCloudStorageS3CompatibleAccessKey { get; set; }

        /// <summary>
        /// Hostname or IP address. For Agent Remote Servers, the Agent reports the hostname it connects from at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not change the running Agent.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Hostname { get; set; }

        /// <summary>
        /// Linode: Access Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string LinodeAccessKey { get; set; }

        /// <summary>
        /// Linode: Bucket name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string LinodeBucket { get; set; }

        /// <summary>
        /// Linode: region
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string LinodeRegion { get; set; }

        /// <summary>
        /// Max number of parallel connections.  Ignored for S3 connections (we will parallelize these as much as possible).
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? MaxConnections { get; set; }

        /// <summary>
        /// Internal name for your reference
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// OneDrive: Either personal or business_other account types
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string OneDriveAccountType { get; set; }

        /// <summary>
        /// If true, we will ensure that all communications with this remote server are made through the primary region of the site.  This setting can also be overridden by a site-wide setting which will force it to true.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PinToSiteRegion { get; set; }

        /// <summary>
        /// Port for remote server. For Agent Remote Servers, the Agent reports its downstream proxy port at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not change the running Agent.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Port { get; set; }

        /// <summary>
        /// Upload staging path.  Applies to SFTP only.  If a path is provided here, files will first be uploaded to this path on the remote folder and the moved into the final correct path via an SFTP move command.  This is required by some remote MFT systems to emulate atomic uploads, which are otherwise not supoprted by SFTP.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UploadStagingPath { get; set; }

        /// <summary>
        /// ID of Remote Server Credential, if applicable.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? RemoteServerCredentialId { get; set; }

        /// <summary>
        /// AWS IAM Role ARN for AssumeRole authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3AssumeRoleArn { get; set; }

        /// <summary>
        /// Session duration in seconds for AssumeRole authentication (900-43200).
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? S3AssumeRoleDurationSeconds { get; set; }

        /// <summary>
        /// S3 bucket name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3Bucket { get; set; }

        /// <summary>
        /// S3-compatible: Access Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3CompatibleAccessKey { get; set; }

        /// <summary>
        /// S3-compatible: Bucket name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3CompatibleBucket { get; set; }

        /// <summary>
        /// S3-compatible: endpoint
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3CompatibleEndpoint { get; set; }

        /// <summary>
        /// S3-compatible: region
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3CompatibleRegion { get; set; }

        /// <summary>
        /// S3-compatible: If true, use virtual-hosted-style URLs instead of path-style URLs
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? S3CompatibleVirtualHostedStyle { get; set; }

        /// <summary>
        /// ARN of the AWS KMS key that encrypts files written to the bucket (SSE-KMS). Leave blank unless your bucket requires a specific key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3KmsKeyId { get; set; }

        /// <summary>
        /// S3 region
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3Region { get; set; }

        /// <summary>
        /// Remote server certificate
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ServerCertificate { get; set; }

        /// <summary>
        /// Pinned SSH host key or OpenSSH host certificate for SFTP. If omitted, Files.com detects and stores a host key, preferring plain keys over certificates. With `server_certificate=require_match` (the default), the server must present the exact pinned key or certificate and prove it holds the matching private key. A pinned certificate is compared in full, so renewal can require updating `server_host_key` even when its underlying key is unchanged. Files.com does not check certificate CA signatures, principals, or validity periods. Certificate expiration alone does not end the pin. Update `server_host_key` to replace the pin.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ServerHostKey { get; set; }

        /// <summary>
        /// Remote server type.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ServerType { get; set; }

        /// <summary>
        /// Owner's preference for administrator access to this user-owned Remote Server. Null uses the Site policy's default. Only the owner may change it when the Site policy allows user choice.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SiteAdminAccess { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra application client ID for app-only authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SharepointClientId { get; set; }

        /// <summary>
        /// SharePoint: Site URL to scope app-only authentication to a single site. Leave blank to browse all sites.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SharepointSiteUrl { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra tenant ID for app-only authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SharepointTenantId { get; set; }

        /// <summary>
        /// Should we require SSL?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Ssl { get; set; }

        /// <summary>
        /// Remote server username.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Username { get; set; }

        /// <summary>
        /// Wasabi: Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string WasabiAccessKey { get; set; }

        /// <summary>
        /// Wasabi: Bucket name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string WasabiBucket { get; set; }

        /// <summary>
        /// Wasabi: Region
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string WasabiRegion { get; set; }

        /// <summary>
        /// Workspace ID (0 for default workspace)
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Remote Server.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Username)) ? $"Remote server '{Username}'" : "Remote server"))
            {
                return;
            }
            Models.RemoteServer result = Run(connection, token => RemoteServerBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Password)))
            {
                parameters["password"] = RequestValues.Reveal(Password);
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            if (IsBound(nameof(PrivateKeyPassphrase)))
            {
                parameters["private_key_passphrase"] = RequestValues.Reveal(PrivateKeyPassphrase);
            }
            if (IsBound(nameof(ResetAuthentication)))
            {
                parameters["reset_authentication"] = ResetAuthentication.Value;
            }
            if (IsBound(nameof(SharepointClientCertificate)))
            {
                parameters["sharepoint_client_certificate"] = RequestValues.Reveal(SharepointClientCertificate);
            }
            if (IsBound(nameof(SharepointClientSecret)))
            {
                parameters["sharepoint_client_secret"] = RequestValues.Reveal(SharepointClientSecret);
            }
            if (IsBound(nameof(SslCertificate)))
            {
                parameters["ssl_certificate"] = SslCertificate;
            }
            if (IsBound(nameof(AwsSecretKey)))
            {
                parameters["aws_secret_key"] = RequestValues.Reveal(AwsSecretKey);
            }
            if (IsBound(nameof(AzureBlobStorageAccessKey)))
            {
                parameters["azure_blob_storage_access_key"] = RequestValues.Reveal(AzureBlobStorageAccessKey);
            }
            if (IsBound(nameof(AzureBlobStorageSasToken)))
            {
                parameters["azure_blob_storage_sas_token"] = RequestValues.Reveal(AzureBlobStorageSasToken);
            }
            if (IsBound(nameof(AzureFilesStorageAccessKey)))
            {
                parameters["azure_files_storage_access_key"] = RequestValues.Reveal(AzureFilesStorageAccessKey);
            }
            if (IsBound(nameof(AzureFilesStorageSasToken)))
            {
                parameters["azure_files_storage_sas_token"] = RequestValues.Reveal(AzureFilesStorageSasToken);
            }
            if (IsBound(nameof(BackblazeB2ApplicationKey)))
            {
                parameters["backblaze_b2_application_key"] = RequestValues.Reveal(BackblazeB2ApplicationKey);
            }
            if (IsBound(nameof(BackblazeB2KeyId)))
            {
                parameters["backblaze_b2_key_id"] = BackblazeB2KeyId;
            }
            if (IsBound(nameof(CloudflareSecretKey)))
            {
                parameters["cloudflare_secret_key"] = RequestValues.Reveal(CloudflareSecretKey);
            }
            if (IsBound(nameof(FilebaseSecretKey)))
            {
                parameters["filebase_secret_key"] = RequestValues.Reveal(FilebaseSecretKey);
            }
            if (IsBound(nameof(GoogleCloudStorageCredentialsJson)))
            {
                parameters["google_cloud_storage_credentials_json"] = RequestValues.Reveal(GoogleCloudStorageCredentialsJson);
            }
            if (IsBound(nameof(GoogleCloudStorageS3CompatibleSecretKey)))
            {
                parameters["google_cloud_storage_s3_compatible_secret_key"] = RequestValues.Reveal(GoogleCloudStorageS3CompatibleSecretKey);
            }
            if (IsBound(nameof(LinodeSecretKey)))
            {
                parameters["linode_secret_key"] = RequestValues.Reveal(LinodeSecretKey);
            }
            if (IsBound(nameof(S3CompatibleSecretKey)))
            {
                parameters["s3_compatible_secret_key"] = RequestValues.Reveal(S3CompatibleSecretKey);
            }
            if (IsBound(nameof(WasabiSecretKey)))
            {
                parameters["wasabi_secret_key"] = RequestValues.Reveal(WasabiSecretKey);
            }
            if (IsBound(nameof(AllowRelativePaths)))
            {
                parameters["allow_relative_paths"] = AllowRelativePaths.Value;
            }
            if (IsBound(nameof(AwsAccessKey)))
            {
                parameters["aws_access_key"] = AwsAccessKey;
            }
            if (IsBound(nameof(AzureBlobStorageAccount)))
            {
                parameters["azure_blob_storage_account"] = AzureBlobStorageAccount;
            }
            if (IsBound(nameof(AzureBlobStorageContainer)))
            {
                parameters["azure_blob_storage_container"] = AzureBlobStorageContainer;
            }
            if (IsBound(nameof(AzureBlobStorageDnsSuffix)))
            {
                parameters["azure_blob_storage_dns_suffix"] = AzureBlobStorageDnsSuffix;
            }
            if (IsBound(nameof(AzureBlobStorageHierarchicalNamespace)))
            {
                parameters["azure_blob_storage_hierarchical_namespace"] = AzureBlobStorageHierarchicalNamespace.Value;
            }
            if (IsBound(nameof(AzureFilesStorageAccount)))
            {
                parameters["azure_files_storage_account"] = AzureFilesStorageAccount;
            }
            if (IsBound(nameof(AzureFilesStorageDnsSuffix)))
            {
                parameters["azure_files_storage_dns_suffix"] = AzureFilesStorageDnsSuffix;
            }
            if (IsBound(nameof(AzureFilesStorageShareName)))
            {
                parameters["azure_files_storage_share_name"] = AzureFilesStorageShareName;
            }
            if (IsBound(nameof(BackblazeB2Bucket)))
            {
                parameters["backblaze_b2_bucket"] = BackblazeB2Bucket;
            }
            if (IsBound(nameof(BackblazeB2S3Endpoint)))
            {
                parameters["backblaze_b2_s3_endpoint"] = BackblazeB2S3Endpoint;
            }
            if (IsBound(nameof(BufferUploads)))
            {
                parameters["buffer_uploads"] = BufferUploads;
            }
            if (IsBound(nameof(CloudflareAccessKey)))
            {
                parameters["cloudflare_access_key"] = CloudflareAccessKey;
            }
            if (IsBound(nameof(CloudflareBucket)))
            {
                parameters["cloudflare_bucket"] = CloudflareBucket;
            }
            if (IsBound(nameof(CloudflareEndpoint)))
            {
                parameters["cloudflare_endpoint"] = CloudflareEndpoint;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(DropboxTeams)))
            {
                parameters["dropbox_teams"] = DropboxTeams.Value;
            }
            if (IsBound(nameof(EnableDedicatedIps)))
            {
                parameters["enable_dedicated_ips"] = EnableDedicatedIps.Value;
            }
            if (IsBound(nameof(FilebaseAccessKey)))
            {
                parameters["filebase_access_key"] = FilebaseAccessKey;
            }
            if (IsBound(nameof(FilebaseBucket)))
            {
                parameters["filebase_bucket"] = FilebaseBucket;
            }
            if (IsBound(nameof(FilesApiKey)))
            {
                parameters["files_api_key"] = RequestValues.Reveal(FilesApiKey);
            }
            if (IsBound(nameof(FilesAgentPermissionSet)))
            {
                parameters["files_agent_permission_set"] = FilesAgentPermissionSet;
            }
            if (IsBound(nameof(FilesAgentRoot)))
            {
                parameters["files_agent_root"] = FilesAgentRoot;
            }
            if (IsBound(nameof(FilesAgentVersion)))
            {
                parameters["files_agent_version"] = FilesAgentVersion;
            }
            if (IsBound(nameof(OutboundAgentId)))
            {
                parameters["outbound_agent_id"] = OutboundAgentId.Value;
            }
            if (IsBound(nameof(CustomDomainId)))
            {
                parameters["custom_domain_id"] = CustomDomainId.Value;
            }
            if (IsBound(nameof(GoogleCloudStorageAuthenticationMethod)))
            {
                parameters["google_cloud_storage_authentication_method"] = GoogleCloudStorageAuthenticationMethod;
            }
            if (IsBound(nameof(GoogleCloudStorageBucket)))
            {
                parameters["google_cloud_storage_bucket"] = GoogleCloudStorageBucket;
            }
            if (IsBound(nameof(GoogleCloudStorageOauthScope)))
            {
                parameters["google_cloud_storage_oauth_scope"] = GoogleCloudStorageOauthScope;
            }
            if (IsBound(nameof(GoogleCloudStorageProjectId)))
            {
                parameters["google_cloud_storage_project_id"] = GoogleCloudStorageProjectId;
            }
            if (IsBound(nameof(GoogleCloudStorageS3CompatibleAccessKey)))
            {
                parameters["google_cloud_storage_s3_compatible_access_key"] = GoogleCloudStorageS3CompatibleAccessKey;
            }
            if (IsBound(nameof(Hostname)))
            {
                parameters["hostname"] = Hostname;
            }
            if (IsBound(nameof(LinodeAccessKey)))
            {
                parameters["linode_access_key"] = LinodeAccessKey;
            }
            if (IsBound(nameof(LinodeBucket)))
            {
                parameters["linode_bucket"] = LinodeBucket;
            }
            if (IsBound(nameof(LinodeRegion)))
            {
                parameters["linode_region"] = LinodeRegion;
            }
            if (IsBound(nameof(MaxConnections)))
            {
                parameters["max_connections"] = MaxConnections.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(OneDriveAccountType)))
            {
                parameters["one_drive_account_type"] = OneDriveAccountType;
            }
            if (IsBound(nameof(PinToSiteRegion)))
            {
                parameters["pin_to_site_region"] = PinToSiteRegion.Value;
            }
            if (IsBound(nameof(Port)))
            {
                parameters["port"] = Port.Value;
            }
            if (IsBound(nameof(UploadStagingPath)))
            {
                parameters["upload_staging_path"] = UploadStagingPath;
            }
            if (IsBound(nameof(RemoteServerCredentialId)))
            {
                parameters["remote_server_credential_id"] = RemoteServerCredentialId.Value;
            }
            if (IsBound(nameof(S3AssumeRoleArn)))
            {
                parameters["s3_assume_role_arn"] = S3AssumeRoleArn;
            }
            if (IsBound(nameof(S3AssumeRoleDurationSeconds)))
            {
                parameters["s3_assume_role_duration_seconds"] = S3AssumeRoleDurationSeconds.Value;
            }
            if (IsBound(nameof(S3Bucket)))
            {
                parameters["s3_bucket"] = S3Bucket;
            }
            if (IsBound(nameof(S3CompatibleAccessKey)))
            {
                parameters["s3_compatible_access_key"] = S3CompatibleAccessKey;
            }
            if (IsBound(nameof(S3CompatibleBucket)))
            {
                parameters["s3_compatible_bucket"] = S3CompatibleBucket;
            }
            if (IsBound(nameof(S3CompatibleEndpoint)))
            {
                parameters["s3_compatible_endpoint"] = S3CompatibleEndpoint;
            }
            if (IsBound(nameof(S3CompatibleRegion)))
            {
                parameters["s3_compatible_region"] = S3CompatibleRegion;
            }
            if (IsBound(nameof(S3CompatibleVirtualHostedStyle)))
            {
                parameters["s3_compatible_virtual_hosted_style"] = S3CompatibleVirtualHostedStyle.Value;
            }
            if (IsBound(nameof(S3KmsKeyId)))
            {
                parameters["s3_kms_key_id"] = S3KmsKeyId;
            }
            if (IsBound(nameof(S3Region)))
            {
                parameters["s3_region"] = S3Region;
            }
            if (IsBound(nameof(ServerCertificate)))
            {
                parameters["server_certificate"] = ServerCertificate;
            }
            if (IsBound(nameof(ServerHostKey)))
            {
                parameters["server_host_key"] = ServerHostKey;
            }
            if (IsBound(nameof(ServerType)))
            {
                parameters["server_type"] = ServerType;
            }
            if (IsBound(nameof(SiteAdminAccess)))
            {
                parameters["site_admin_access"] = SiteAdminAccess.Value;
            }
            if (IsBound(nameof(SharepointClientId)))
            {
                parameters["sharepoint_client_id"] = SharepointClientId;
            }
            if (IsBound(nameof(SharepointSiteUrl)))
            {
                parameters["sharepoint_site_url"] = SharepointSiteUrl;
            }
            if (IsBound(nameof(SharepointTenantId)))
            {
                parameters["sharepoint_tenant_id"] = SharepointTenantId;
            }
            if (IsBound(nameof(Ssl)))
            {
                parameters["ssl"] = Ssl;
            }
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            if (IsBound(nameof(WasabiAccessKey)))
            {
                parameters["wasabi_access_key"] = WasabiAccessKey;
            }
            if (IsBound(nameof(WasabiBucket)))
            {
                parameters["wasabi_bucket"] = WasabiBucket;
            }
            if (IsBound(nameof(WasabiRegion)))
            {
                parameters["wasabi_region"] = WasabiRegion;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Push update to Files Agent.
    /// </summary>
    [Cmdlet("Update", "FilesRemoteServerAgent", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.AgentPushUpdate), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class UpdateFilesRemoteServerAgentCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Server ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The remote server to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteServer InputObject { get; set; }

        /// <summary>Push update to Files Agent.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = AgentPushUpdateParameters();
            if (!ShouldProcessOn(connection, $"Remote server {id}"))
            {
                return;
            }
            Models.AgentPushUpdate result = Run(connection, token => RemoteServerBridge.AgentPushUpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> AgentPushUpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update Remote Server.
    /// </summary>
    [Cmdlet("Set", "FilesRemoteServer", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.RemoteServer), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesRemoteServerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Server ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Password, if needed.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString Password { get; set; }

        /// <summary>
        /// Private key, if needed.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>
        /// Passphrase for private key if needed.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKeyPassphrase { get; set; }

        /// <summary>
        /// Reset authenticated account?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ResetAuthentication { get; set; }

        /// <summary>
        /// SharePoint: PEM-encoded certificate and unencrypted private key for app-only authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString SharepointClientCertificate { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra application client secret for app-only authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString SharepointClientSecret { get; set; }

        /// <summary>
        /// SSL client certificate.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SslCertificate { get; set; }

        /// <summary>
        /// AWS: secret key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString AwsSecretKey { get; set; }

        /// <summary>
        /// Azure Blob Storage: Access Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString AzureBlobStorageAccessKey { get; set; }

        /// <summary>
        /// Azure Blob Storage: Shared Access Signature (SAS) token
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString AzureBlobStorageSasToken { get; set; }

        /// <summary>
        /// Azure File Storage: Access Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString AzureFilesStorageAccessKey { get; set; }

        /// <summary>
        /// Azure File Storage: Shared Access Signature (SAS) token
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString AzureFilesStorageSasToken { get; set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: applicationKey
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString BackblazeB2ApplicationKey { get; set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: keyID
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string BackblazeB2KeyId { get; set; }

        /// <summary>
        /// Cloudflare: Secret Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString CloudflareSecretKey { get; set; }

        /// <summary>
        /// Filebase: Secret Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString FilebaseSecretKey { get; set; }

        /// <summary>
        /// Google Cloud Storage: JSON file that contains the private key. To generate see https://cloud.google.com/storage/docs/json_api/v1/how-tos/authorizing#APIKey
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString GoogleCloudStorageCredentialsJson { get; set; }

        /// <summary>
        /// Google Cloud Storage: S3-compatible secret key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString GoogleCloudStorageS3CompatibleSecretKey { get; set; }

        /// <summary>
        /// Linode: Secret Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString LinodeSecretKey { get; set; }

        /// <summary>
        /// S3-compatible: Secret Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString S3CompatibleSecretKey { get; set; }

        /// <summary>
        /// Wasabi: Secret Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString WasabiSecretKey { get; set; }

        /// <summary>
        /// Allow relative paths in SFTP. If true, paths will not be forced to be absolute, allowing operations relative to the user's home directory.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AllowRelativePaths { get; set; }

        /// <summary>
        /// AWS Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AwsAccessKey { get; set; }

        /// <summary>
        /// Azure Blob Storage: Account name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureBlobStorageAccount { get; set; }

        /// <summary>
        /// Azure Blob Storage: Container name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureBlobStorageContainer { get; set; }

        /// <summary>
        /// Azure Blob Storage: Custom DNS suffix
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureBlobStorageDnsSuffix { get; set; }

        /// <summary>
        /// Azure Blob Storage: Does the storage account has hierarchical namespace feature enabled?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AzureBlobStorageHierarchicalNamespace { get; set; }

        /// <summary>
        /// Azure Files: Storage Account name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureFilesStorageAccount { get; set; }

        /// <summary>
        /// Azure Files: Custom DNS suffix
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureFilesStorageDnsSuffix { get; set; }

        /// <summary>
        /// Azure Files:  Storage Share name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureFilesStorageShareName { get; set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: Bucket name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string BackblazeB2Bucket { get; set; }

        /// <summary>
        /// Backblaze B2 Cloud Storage: S3 Endpoint
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string BackblazeB2S3Endpoint { get; set; }

        /// <summary>
        /// If set to always, uploads to this server will be uploaded first to Files.com before being sent to the remote server. This can improve performance in certain access patterns, such as high-latency connections.  It will cause data to be temporarily stored in Files.com. If set to auto, we will perform this optimization if we believe it to be a benefit in a given situation.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string BufferUploads { get; set; }

        /// <summary>
        /// Cloudflare: Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string CloudflareAccessKey { get; set; }

        /// <summary>
        /// Cloudflare: Bucket name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string CloudflareBucket { get; set; }

        /// <summary>
        /// Cloudflare: endpoint
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string CloudflareEndpoint { get; set; }

        /// <summary>
        /// Internal description for your reference
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Dropbox: If true, list Team folders in root?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DropboxTeams { get; set; }

        /// <summary>
        /// `true` if remote server only accepts connections from dedicated IPs
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? EnableDedicatedIps { get; set; }

        /// <summary>
        /// Filebase: Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FilebaseAccessKey { get; set; }

        /// <summary>
        /// Filebase: Bucket name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FilebaseBucket { get; set; }

        /// <summary>
        /// Files.com direct link: API key used once to pair the remote server.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString FilesApiKey { get; set; }

        /// <summary>
        /// Agent file permissions: `read_only`, `write_only`, or `read_write`. Files.com writes this value as `permission_set` in the configuration file generated for this Remote Server. Setting it on Create or Update changes the generated file, not a running Agent. The Agent enforces the permissions in its local configuration file, controlled by the host's system administrator, and reports them at registration, replacing the stored value.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FilesAgentPermissionSet { get; set; }

        /// <summary>
        /// Agent local root path. Files.com writes this value as `root` in the configuration file generated for this Remote Server. Setting it on Create or Update changes the generated file, not a running Agent. The Agent enforces the root in its local configuration file, controlled by the host's system administrator, and reports it at registration, replacing the stored value.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FilesAgentRoot { get; set; }

        /// <summary>
        /// Files.com Agent version reported by the Agent at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not update the running Agent.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FilesAgentVersion { get; set; }

        /// <summary>
        /// Route traffic to outbound on a files-agent
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? OutboundAgentId { get; set; }

        /// <summary>
        /// Custom Domain ID whose dedicated IP addresses are selected when this Remote Server uses dedicated IPs. Must be available to this Remote Server's workspace. Requires enable_dedicated_ips and cannot be combined with an outbound Agent. Set to null to use the site's default dedicated IPs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? CustomDomainId { get; set; }

        /// <summary>
        /// Google Cloud Storage: Authentication method. Can be json, hmac, or oauth.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string GoogleCloudStorageAuthenticationMethod { get; set; }

        /// <summary>
        /// Google Cloud Storage: Bucket Name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string GoogleCloudStorageBucket { get; set; }

        /// <summary>
        /// Google Cloud Storage: OAuth scope. Can be https://www.googleapis.com/auth/devstorage.read_only or https://www.googleapis.com/auth/devstorage.read_write.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string GoogleCloudStorageOauthScope { get; set; }

        /// <summary>
        /// Google Cloud Storage: Project ID
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string GoogleCloudStorageProjectId { get; set; }

        /// <summary>
        /// Google Cloud Storage: S3-compatible Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string GoogleCloudStorageS3CompatibleAccessKey { get; set; }

        /// <summary>
        /// Hostname or IP address. For Agent Remote Servers, the Agent reports the hostname it connects from at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not change the running Agent.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Hostname { get; set; }

        /// <summary>
        /// Linode: Access Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string LinodeAccessKey { get; set; }

        /// <summary>
        /// Linode: Bucket name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string LinodeBucket { get; set; }

        /// <summary>
        /// Linode: region
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string LinodeRegion { get; set; }

        /// <summary>
        /// Max number of parallel connections.  Ignored for S3 connections (we will parallelize these as much as possible).
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? MaxConnections { get; set; }

        /// <summary>
        /// Internal name for your reference
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// OneDrive: Either personal or business_other account types
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string OneDriveAccountType { get; set; }

        /// <summary>
        /// If true, we will ensure that all communications with this remote server are made through the primary region of the site.  This setting can also be overridden by a site-wide setting which will force it to true.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? PinToSiteRegion { get; set; }

        /// <summary>
        /// Port for remote server. For Agent Remote Servers, the Agent reports its downstream proxy port at each registration, replacing the stored value. This field is not included in the generated Agent configuration file. Setting it on Create or Update does not change the running Agent.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Port { get; set; }

        /// <summary>
        /// Upload staging path.  Applies to SFTP only.  If a path is provided here, files will first be uploaded to this path on the remote folder and the moved into the final correct path via an SFTP move command.  This is required by some remote MFT systems to emulate atomic uploads, which are otherwise not supoprted by SFTP.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string UploadStagingPath { get; set; }

        /// <summary>
        /// ID of Remote Server Credential, if applicable.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? RemoteServerCredentialId { get; set; }

        /// <summary>
        /// AWS IAM Role ARN for AssumeRole authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3AssumeRoleArn { get; set; }

        /// <summary>
        /// Session duration in seconds for AssumeRole authentication (900-43200).
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? S3AssumeRoleDurationSeconds { get; set; }

        /// <summary>
        /// S3 bucket name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3Bucket { get; set; }

        /// <summary>
        /// S3-compatible: Access Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3CompatibleAccessKey { get; set; }

        /// <summary>
        /// S3-compatible: Bucket name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3CompatibleBucket { get; set; }

        /// <summary>
        /// S3-compatible: endpoint
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3CompatibleEndpoint { get; set; }

        /// <summary>
        /// S3-compatible: region
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3CompatibleRegion { get; set; }

        /// <summary>
        /// S3-compatible: If true, use virtual-hosted-style URLs instead of path-style URLs
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? S3CompatibleVirtualHostedStyle { get; set; }

        /// <summary>
        /// ARN of the AWS KMS key that encrypts files written to the bucket (SSE-KMS). Leave blank unless your bucket requires a specific key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3KmsKeyId { get; set; }

        /// <summary>
        /// S3 region
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3Region { get; set; }

        /// <summary>
        /// Remote server certificate
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ServerCertificate { get; set; }

        /// <summary>
        /// Pinned SSH host key or OpenSSH host certificate for SFTP. If omitted, Files.com detects and stores a host key, preferring plain keys over certificates. With `server_certificate=require_match` (the default), the server must present the exact pinned key or certificate and prove it holds the matching private key. A pinned certificate is compared in full, so renewal can require updating `server_host_key` even when its underlying key is unchanged. Files.com does not check certificate CA signatures, principals, or validity periods. Certificate expiration alone does not end the pin. Update `server_host_key` to replace the pin.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ServerHostKey { get; set; }

        /// <summary>
        /// Remote server type.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ServerType { get; set; }

        /// <summary>
        /// Owner's preference for administrator access to this user-owned Remote Server. Null uses the Site policy's default. Only the owner may change it when the Site policy allows user choice.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SiteAdminAccess { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra application client ID for app-only authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SharepointClientId { get; set; }

        /// <summary>
        /// SharePoint: Site URL to scope app-only authentication to a single site. Leave blank to browse all sites.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SharepointSiteUrl { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra tenant ID for app-only authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SharepointTenantId { get; set; }

        /// <summary>
        /// Should we require SSL?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Ssl { get; set; }

        /// <summary>
        /// Remote server username.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Username { get; set; }

        /// <summary>
        /// Wasabi: Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string WasabiAccessKey { get; set; }

        /// <summary>
        /// Wasabi: Bucket name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string WasabiBucket { get; set; }

        /// <summary>
        /// Wasabi: Region
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string WasabiRegion { get; set; }

        /// <summary>
        /// The remote server to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteServer InputObject { get; set; }

        /// <summary>Update Remote Server.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Remote server {id}"))
            {
                return;
            }
            Models.RemoteServer result = Run(connection, token => RemoteServerBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Password)))
            {
                parameters["password"] = RequestValues.Reveal(Password);
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            if (IsBound(nameof(PrivateKeyPassphrase)))
            {
                parameters["private_key_passphrase"] = RequestValues.Reveal(PrivateKeyPassphrase);
            }
            if (IsBound(nameof(ResetAuthentication)))
            {
                parameters["reset_authentication"] = ResetAuthentication.Value;
            }
            if (IsBound(nameof(SharepointClientCertificate)))
            {
                parameters["sharepoint_client_certificate"] = RequestValues.Reveal(SharepointClientCertificate);
            }
            if (IsBound(nameof(SharepointClientSecret)))
            {
                parameters["sharepoint_client_secret"] = RequestValues.Reveal(SharepointClientSecret);
            }
            if (IsBound(nameof(SslCertificate)))
            {
                parameters["ssl_certificate"] = SslCertificate;
            }
            if (IsBound(nameof(AwsSecretKey)))
            {
                parameters["aws_secret_key"] = RequestValues.Reveal(AwsSecretKey);
            }
            if (IsBound(nameof(AzureBlobStorageAccessKey)))
            {
                parameters["azure_blob_storage_access_key"] = RequestValues.Reveal(AzureBlobStorageAccessKey);
            }
            if (IsBound(nameof(AzureBlobStorageSasToken)))
            {
                parameters["azure_blob_storage_sas_token"] = RequestValues.Reveal(AzureBlobStorageSasToken);
            }
            if (IsBound(nameof(AzureFilesStorageAccessKey)))
            {
                parameters["azure_files_storage_access_key"] = RequestValues.Reveal(AzureFilesStorageAccessKey);
            }
            if (IsBound(nameof(AzureFilesStorageSasToken)))
            {
                parameters["azure_files_storage_sas_token"] = RequestValues.Reveal(AzureFilesStorageSasToken);
            }
            if (IsBound(nameof(BackblazeB2ApplicationKey)))
            {
                parameters["backblaze_b2_application_key"] = RequestValues.Reveal(BackblazeB2ApplicationKey);
            }
            if (IsBound(nameof(BackblazeB2KeyId)))
            {
                parameters["backblaze_b2_key_id"] = BackblazeB2KeyId;
            }
            if (IsBound(nameof(CloudflareSecretKey)))
            {
                parameters["cloudflare_secret_key"] = RequestValues.Reveal(CloudflareSecretKey);
            }
            if (IsBound(nameof(FilebaseSecretKey)))
            {
                parameters["filebase_secret_key"] = RequestValues.Reveal(FilebaseSecretKey);
            }
            if (IsBound(nameof(GoogleCloudStorageCredentialsJson)))
            {
                parameters["google_cloud_storage_credentials_json"] = RequestValues.Reveal(GoogleCloudStorageCredentialsJson);
            }
            if (IsBound(nameof(GoogleCloudStorageS3CompatibleSecretKey)))
            {
                parameters["google_cloud_storage_s3_compatible_secret_key"] = RequestValues.Reveal(GoogleCloudStorageS3CompatibleSecretKey);
            }
            if (IsBound(nameof(LinodeSecretKey)))
            {
                parameters["linode_secret_key"] = RequestValues.Reveal(LinodeSecretKey);
            }
            if (IsBound(nameof(S3CompatibleSecretKey)))
            {
                parameters["s3_compatible_secret_key"] = RequestValues.Reveal(S3CompatibleSecretKey);
            }
            if (IsBound(nameof(WasabiSecretKey)))
            {
                parameters["wasabi_secret_key"] = RequestValues.Reveal(WasabiSecretKey);
            }
            if (IsBound(nameof(AllowRelativePaths)))
            {
                parameters["allow_relative_paths"] = AllowRelativePaths.Value;
            }
            if (IsBound(nameof(AwsAccessKey)))
            {
                parameters["aws_access_key"] = AwsAccessKey;
            }
            if (IsBound(nameof(AzureBlobStorageAccount)))
            {
                parameters["azure_blob_storage_account"] = AzureBlobStorageAccount;
            }
            if (IsBound(nameof(AzureBlobStorageContainer)))
            {
                parameters["azure_blob_storage_container"] = AzureBlobStorageContainer;
            }
            if (IsBound(nameof(AzureBlobStorageDnsSuffix)))
            {
                parameters["azure_blob_storage_dns_suffix"] = AzureBlobStorageDnsSuffix;
            }
            if (IsBound(nameof(AzureBlobStorageHierarchicalNamespace)))
            {
                parameters["azure_blob_storage_hierarchical_namespace"] = AzureBlobStorageHierarchicalNamespace.Value;
            }
            if (IsBound(nameof(AzureFilesStorageAccount)))
            {
                parameters["azure_files_storage_account"] = AzureFilesStorageAccount;
            }
            if (IsBound(nameof(AzureFilesStorageDnsSuffix)))
            {
                parameters["azure_files_storage_dns_suffix"] = AzureFilesStorageDnsSuffix;
            }
            if (IsBound(nameof(AzureFilesStorageShareName)))
            {
                parameters["azure_files_storage_share_name"] = AzureFilesStorageShareName;
            }
            if (IsBound(nameof(BackblazeB2Bucket)))
            {
                parameters["backblaze_b2_bucket"] = BackblazeB2Bucket;
            }
            if (IsBound(nameof(BackblazeB2S3Endpoint)))
            {
                parameters["backblaze_b2_s3_endpoint"] = BackblazeB2S3Endpoint;
            }
            if (IsBound(nameof(BufferUploads)))
            {
                parameters["buffer_uploads"] = BufferUploads;
            }
            if (IsBound(nameof(CloudflareAccessKey)))
            {
                parameters["cloudflare_access_key"] = CloudflareAccessKey;
            }
            if (IsBound(nameof(CloudflareBucket)))
            {
                parameters["cloudflare_bucket"] = CloudflareBucket;
            }
            if (IsBound(nameof(CloudflareEndpoint)))
            {
                parameters["cloudflare_endpoint"] = CloudflareEndpoint;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(DropboxTeams)))
            {
                parameters["dropbox_teams"] = DropboxTeams.Value;
            }
            if (IsBound(nameof(EnableDedicatedIps)))
            {
                parameters["enable_dedicated_ips"] = EnableDedicatedIps.Value;
            }
            if (IsBound(nameof(FilebaseAccessKey)))
            {
                parameters["filebase_access_key"] = FilebaseAccessKey;
            }
            if (IsBound(nameof(FilebaseBucket)))
            {
                parameters["filebase_bucket"] = FilebaseBucket;
            }
            if (IsBound(nameof(FilesApiKey)))
            {
                parameters["files_api_key"] = RequestValues.Reveal(FilesApiKey);
            }
            if (IsBound(nameof(FilesAgentPermissionSet)))
            {
                parameters["files_agent_permission_set"] = FilesAgentPermissionSet;
            }
            if (IsBound(nameof(FilesAgentRoot)))
            {
                parameters["files_agent_root"] = FilesAgentRoot;
            }
            if (IsBound(nameof(FilesAgentVersion)))
            {
                parameters["files_agent_version"] = FilesAgentVersion;
            }
            if (IsBound(nameof(OutboundAgentId)))
            {
                parameters["outbound_agent_id"] = OutboundAgentId.Value;
            }
            if (IsBound(nameof(CustomDomainId)))
            {
                parameters["custom_domain_id"] = CustomDomainId.Value;
            }
            if (IsBound(nameof(GoogleCloudStorageAuthenticationMethod)))
            {
                parameters["google_cloud_storage_authentication_method"] = GoogleCloudStorageAuthenticationMethod;
            }
            if (IsBound(nameof(GoogleCloudStorageBucket)))
            {
                parameters["google_cloud_storage_bucket"] = GoogleCloudStorageBucket;
            }
            if (IsBound(nameof(GoogleCloudStorageOauthScope)))
            {
                parameters["google_cloud_storage_oauth_scope"] = GoogleCloudStorageOauthScope;
            }
            if (IsBound(nameof(GoogleCloudStorageProjectId)))
            {
                parameters["google_cloud_storage_project_id"] = GoogleCloudStorageProjectId;
            }
            if (IsBound(nameof(GoogleCloudStorageS3CompatibleAccessKey)))
            {
                parameters["google_cloud_storage_s3_compatible_access_key"] = GoogleCloudStorageS3CompatibleAccessKey;
            }
            if (IsBound(nameof(Hostname)))
            {
                parameters["hostname"] = Hostname;
            }
            if (IsBound(nameof(LinodeAccessKey)))
            {
                parameters["linode_access_key"] = LinodeAccessKey;
            }
            if (IsBound(nameof(LinodeBucket)))
            {
                parameters["linode_bucket"] = LinodeBucket;
            }
            if (IsBound(nameof(LinodeRegion)))
            {
                parameters["linode_region"] = LinodeRegion;
            }
            if (IsBound(nameof(MaxConnections)))
            {
                parameters["max_connections"] = MaxConnections.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(OneDriveAccountType)))
            {
                parameters["one_drive_account_type"] = OneDriveAccountType;
            }
            if (IsBound(nameof(PinToSiteRegion)))
            {
                parameters["pin_to_site_region"] = PinToSiteRegion.Value;
            }
            if (IsBound(nameof(Port)))
            {
                parameters["port"] = Port.Value;
            }
            if (IsBound(nameof(UploadStagingPath)))
            {
                parameters["upload_staging_path"] = UploadStagingPath;
            }
            if (IsBound(nameof(RemoteServerCredentialId)))
            {
                parameters["remote_server_credential_id"] = RemoteServerCredentialId.Value;
            }
            if (IsBound(nameof(S3AssumeRoleArn)))
            {
                parameters["s3_assume_role_arn"] = S3AssumeRoleArn;
            }
            if (IsBound(nameof(S3AssumeRoleDurationSeconds)))
            {
                parameters["s3_assume_role_duration_seconds"] = S3AssumeRoleDurationSeconds.Value;
            }
            if (IsBound(nameof(S3Bucket)))
            {
                parameters["s3_bucket"] = S3Bucket;
            }
            if (IsBound(nameof(S3CompatibleAccessKey)))
            {
                parameters["s3_compatible_access_key"] = S3CompatibleAccessKey;
            }
            if (IsBound(nameof(S3CompatibleBucket)))
            {
                parameters["s3_compatible_bucket"] = S3CompatibleBucket;
            }
            if (IsBound(nameof(S3CompatibleEndpoint)))
            {
                parameters["s3_compatible_endpoint"] = S3CompatibleEndpoint;
            }
            if (IsBound(nameof(S3CompatibleRegion)))
            {
                parameters["s3_compatible_region"] = S3CompatibleRegion;
            }
            if (IsBound(nameof(S3CompatibleVirtualHostedStyle)))
            {
                parameters["s3_compatible_virtual_hosted_style"] = S3CompatibleVirtualHostedStyle.Value;
            }
            if (IsBound(nameof(S3KmsKeyId)))
            {
                parameters["s3_kms_key_id"] = S3KmsKeyId;
            }
            if (IsBound(nameof(S3Region)))
            {
                parameters["s3_region"] = S3Region;
            }
            if (IsBound(nameof(ServerCertificate)))
            {
                parameters["server_certificate"] = ServerCertificate;
            }
            if (IsBound(nameof(ServerHostKey)))
            {
                parameters["server_host_key"] = ServerHostKey;
            }
            if (IsBound(nameof(ServerType)))
            {
                parameters["server_type"] = ServerType;
            }
            if (IsBound(nameof(SiteAdminAccess)))
            {
                parameters["site_admin_access"] = SiteAdminAccess.Value;
            }
            if (IsBound(nameof(SharepointClientId)))
            {
                parameters["sharepoint_client_id"] = SharepointClientId;
            }
            if (IsBound(nameof(SharepointSiteUrl)))
            {
                parameters["sharepoint_site_url"] = SharepointSiteUrl;
            }
            if (IsBound(nameof(SharepointTenantId)))
            {
                parameters["sharepoint_tenant_id"] = SharepointTenantId;
            }
            if (IsBound(nameof(Ssl)))
            {
                parameters["ssl"] = Ssl;
            }
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            if (IsBound(nameof(WasabiAccessKey)))
            {
                parameters["wasabi_access_key"] = WasabiAccessKey;
            }
            if (IsBound(nameof(WasabiBucket)))
            {
                parameters["wasabi_bucket"] = WasabiBucket;
            }
            if (IsBound(nameof(WasabiRegion)))
            {
                parameters["wasabi_region"] = WasabiRegion;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Remote Server.
    /// </summary>
    [Cmdlet("Remove", "FilesRemoteServer", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesRemoteServerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Server ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The remote server to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteServer InputObject { get; set; }

        /// <summary>Delete Remote Server.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Remote server {id}"))
            {
                return;
            }
            Run(connection, token => RemoteServerBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
