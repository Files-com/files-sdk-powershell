using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Remote Server Credentials, or get one remote server credential by Id.
    /// </summary>
    [Cmdlet("Get", "FilesRemoteServerCredential", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.RemoteServerCredential), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.RemoteServerCredential>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.RemoteServerCredential), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesRemoteServerCredentialCommand : FilesListCmdlet<Models.RemoteServerCredential>
    {
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id` and `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id` and `name`. Valid field combinations are `[ workspace_id, name ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// Remote Server Credential ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Remote Server Credentials, or get one remote server credential by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.RemoteServerCredential result = Run(connection, token => RemoteServerCredentialBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => RemoteServerCredentialBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
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
    /// Create Remote Server Credential.
    /// </summary>
    [Cmdlet("New", "FilesRemoteServerCredential", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.RemoteServerCredential))]
    public sealed class NewFilesRemoteServerCredentialCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Internal name for your reference
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Internal description for your reference
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Remote server type.  Remote Server Credentials are only valid for a single type of Remote Server.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ServerType { get; set; }

        /// <summary>
        /// AWS Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AwsAccessKey { get; set; }

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
        /// Cloudflare: Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CloudflareAccessKey { get; set; }

        /// <summary>
        /// Filebase: Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FilebaseAccessKey { get; set; }

        /// <summary>
        /// Google Cloud Storage: S3-compatible Access Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GoogleCloudStorageS3CompatibleAccessKey { get; set; }

        /// <summary>
        /// Linode: Access Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string LinodeAccessKey { get; set; }

        /// <summary>
        /// S3-compatible: Access Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string S3CompatibleAccessKey { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra application client ID for app-only authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SharepointClientId { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra tenant ID for app-only authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SharepointTenantId { get; set; }

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
        /// Wasabi: Secret Key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString WasabiSecretKey { get; set; }

        /// <summary>
        /// Workspace ID (0 for default workspace)
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// ID of Remote Server Credential to copy omitted values from.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? CopyValuesFromCredentialId { get; set; }

        /// <summary>Create Remote Server Credential.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Username)) ? $"Remote server credential '{Username}'" : "Remote server credential"))
            {
                return;
            }
            Models.RemoteServerCredential result = Run(connection, token => RemoteServerCredentialBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(ServerType)))
            {
                parameters["server_type"] = ServerType;
            }
            if (IsBound(nameof(AwsAccessKey)))
            {
                parameters["aws_access_key"] = AwsAccessKey;
            }
            if (IsBound(nameof(S3AssumeRoleArn)))
            {
                parameters["s3_assume_role_arn"] = S3AssumeRoleArn;
            }
            if (IsBound(nameof(S3AssumeRoleDurationSeconds)))
            {
                parameters["s3_assume_role_duration_seconds"] = S3AssumeRoleDurationSeconds.Value;
            }
            if (IsBound(nameof(CloudflareAccessKey)))
            {
                parameters["cloudflare_access_key"] = CloudflareAccessKey;
            }
            if (IsBound(nameof(FilebaseAccessKey)))
            {
                parameters["filebase_access_key"] = FilebaseAccessKey;
            }
            if (IsBound(nameof(GoogleCloudStorageS3CompatibleAccessKey)))
            {
                parameters["google_cloud_storage_s3_compatible_access_key"] = GoogleCloudStorageS3CompatibleAccessKey;
            }
            if (IsBound(nameof(LinodeAccessKey)))
            {
                parameters["linode_access_key"] = LinodeAccessKey;
            }
            if (IsBound(nameof(S3CompatibleAccessKey)))
            {
                parameters["s3_compatible_access_key"] = S3CompatibleAccessKey;
            }
            if (IsBound(nameof(SharepointClientId)))
            {
                parameters["sharepoint_client_id"] = SharepointClientId;
            }
            if (IsBound(nameof(SharepointTenantId)))
            {
                parameters["sharepoint_tenant_id"] = SharepointTenantId;
            }
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            if (IsBound(nameof(WasabiAccessKey)))
            {
                parameters["wasabi_access_key"] = WasabiAccessKey;
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
            if (IsBound(nameof(SharepointClientCertificate)))
            {
                parameters["sharepoint_client_certificate"] = RequestValues.Reveal(SharepointClientCertificate);
            }
            if (IsBound(nameof(SharepointClientSecret)))
            {
                parameters["sharepoint_client_secret"] = RequestValues.Reveal(SharepointClientSecret);
            }
            if (IsBound(nameof(WasabiSecretKey)))
            {
                parameters["wasabi_secret_key"] = RequestValues.Reveal(WasabiSecretKey);
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(CopyValuesFromCredentialId)))
            {
                parameters["copy_values_from_credential_id"] = CopyValuesFromCredentialId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Remote Server Credential.
    /// </summary>
    [Cmdlet("Set", "FilesRemoteServerCredential", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.RemoteServerCredential), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesRemoteServerCredentialCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Server Credential ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Internal name for your reference
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Internal description for your reference
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Remote server type.  Remote Server Credentials are only valid for a single type of Remote Server.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ServerType { get; set; }

        /// <summary>
        /// AWS Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AwsAccessKey { get; set; }

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
        /// Cloudflare: Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string CloudflareAccessKey { get; set; }

        /// <summary>
        /// Filebase: Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FilebaseAccessKey { get; set; }

        /// <summary>
        /// Google Cloud Storage: S3-compatible Access Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string GoogleCloudStorageS3CompatibleAccessKey { get; set; }

        /// <summary>
        /// Linode: Access Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string LinodeAccessKey { get; set; }

        /// <summary>
        /// S3-compatible: Access Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string S3CompatibleAccessKey { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra application client ID for app-only authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SharepointClientId { get; set; }

        /// <summary>
        /// SharePoint: Microsoft Entra tenant ID for app-only authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SharepointTenantId { get; set; }

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
        /// Wasabi: Secret Key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString WasabiSecretKey { get; set; }

        /// <summary>
        /// The remote server credential to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteServerCredential InputObject { get; set; }

        /// <summary>Update Remote Server Credential.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Remote server credential {id}"))
            {
                return;
            }
            Models.RemoteServerCredential result = Run(connection, token => RemoteServerCredentialBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(ServerType)))
            {
                parameters["server_type"] = ServerType;
            }
            if (IsBound(nameof(AwsAccessKey)))
            {
                parameters["aws_access_key"] = AwsAccessKey;
            }
            if (IsBound(nameof(S3AssumeRoleArn)))
            {
                parameters["s3_assume_role_arn"] = S3AssumeRoleArn;
            }
            if (IsBound(nameof(S3AssumeRoleDurationSeconds)))
            {
                parameters["s3_assume_role_duration_seconds"] = S3AssumeRoleDurationSeconds.Value;
            }
            if (IsBound(nameof(CloudflareAccessKey)))
            {
                parameters["cloudflare_access_key"] = CloudflareAccessKey;
            }
            if (IsBound(nameof(FilebaseAccessKey)))
            {
                parameters["filebase_access_key"] = FilebaseAccessKey;
            }
            if (IsBound(nameof(GoogleCloudStorageS3CompatibleAccessKey)))
            {
                parameters["google_cloud_storage_s3_compatible_access_key"] = GoogleCloudStorageS3CompatibleAccessKey;
            }
            if (IsBound(nameof(LinodeAccessKey)))
            {
                parameters["linode_access_key"] = LinodeAccessKey;
            }
            if (IsBound(nameof(S3CompatibleAccessKey)))
            {
                parameters["s3_compatible_access_key"] = S3CompatibleAccessKey;
            }
            if (IsBound(nameof(SharepointClientId)))
            {
                parameters["sharepoint_client_id"] = SharepointClientId;
            }
            if (IsBound(nameof(SharepointTenantId)))
            {
                parameters["sharepoint_tenant_id"] = SharepointTenantId;
            }
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            if (IsBound(nameof(WasabiAccessKey)))
            {
                parameters["wasabi_access_key"] = WasabiAccessKey;
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
            if (IsBound(nameof(SharepointClientCertificate)))
            {
                parameters["sharepoint_client_certificate"] = RequestValues.Reveal(SharepointClientCertificate);
            }
            if (IsBound(nameof(SharepointClientSecret)))
            {
                parameters["sharepoint_client_secret"] = RequestValues.Reveal(SharepointClientSecret);
            }
            if (IsBound(nameof(WasabiSecretKey)))
            {
                parameters["wasabi_secret_key"] = RequestValues.Reveal(WasabiSecretKey);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Remote Server Credential.
    /// </summary>
    [Cmdlet("Remove", "FilesRemoteServerCredential", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesRemoteServerCredentialCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Server Credential ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The remote server credential to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteServerCredential InputObject { get; set; }

        /// <summary>Delete Remote Server Credential.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Remote server credential {id}"))
            {
                return;
            }
            Run(connection, token => RemoteServerCredentialBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
