using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A RemoteServerCredential is a way to store a credential for Remote Servers in a centralized vault and then reference it from Remote Server definitions.
    /// </summary>
    public sealed class RemoteServerCredential : FilesObject
    {
        internal RemoteServerCredential()
        {
        }

        /// <summary>
        /// Remote Server Credential ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID (0 for default workspace)
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Internal name for your reference
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Internal description for your reference
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Remote server type.  Remote Server Credentials are only valid for a single type of Remote Server.
        /// </summary>
        public string ServerType { get; internal set; }

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
        /// Google Cloud Storage: S3-compatible Access Key.
        /// </summary>
        public string GoogleCloudStorageS3CompatibleAccessKey { get; internal set; }

        /// <summary>
        /// Wasabi: Access Key.
        /// </summary>
        public string WasabiAccessKey { get; internal set; }

        /// <summary>
        /// S3-compatible: Access Key
        /// </summary>
        public string S3CompatibleAccessKey { get; internal set; }

        /// <summary>
        /// Filebase: Access Key.
        /// </summary>
        public string FilebaseAccessKey { get; internal set; }

        /// <summary>
        /// Cloudflare: Access Key.
        /// </summary>
        public string CloudflareAccessKey { get; internal set; }

        /// <summary>
        /// Linode: Access Key
        /// </summary>
        public string LinodeAccessKey { get; internal set; }

        /// <summary>
        /// SharePoint: Microsoft Entra tenant ID for app-only authentication.
        /// </summary>
        public string SharepointTenantId { get; internal set; }

        /// <summary>
        /// SharePoint: Microsoft Entra application client ID for app-only authentication.
        /// </summary>
        public string SharepointClientId { get; internal set; }

        /// <summary>
        /// SharePoint: App-only credential type. Either secret or certificate.
        /// </summary>
        public string SharepointAppCredentialType { get; internal set; }

        /// <summary>
        /// Remote server username.
        /// </summary>
        public string Username { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(RemoteServerCredential source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Name = source.Name;
            Description = source.Description;
            ServerType = source.ServerType;
            AwsAccessKey = source.AwsAccessKey;
            S3AssumeRoleArn = source.S3AssumeRoleArn;
            S3AssumeRoleDurationSeconds = source.S3AssumeRoleDurationSeconds;
            S3AssumeRoleExternalId = source.S3AssumeRoleExternalId;
            GoogleCloudStorageS3CompatibleAccessKey = source.GoogleCloudStorageS3CompatibleAccessKey;
            WasabiAccessKey = source.WasabiAccessKey;
            S3CompatibleAccessKey = source.S3CompatibleAccessKey;
            FilebaseAccessKey = source.FilebaseAccessKey;
            CloudflareAccessKey = source.CloudflareAccessKey;
            LinodeAccessKey = source.LinodeAccessKey;
            SharepointTenantId = source.SharepointTenantId;
            SharepointClientId = source.SharepointClientId;
            SharepointAppCredentialType = source.SharepointAppCredentialType;
            Username = source.Username;
        }
    }
}
