using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An APIKey is a key that allows programmatic access to your Site.
    /// </summary>
    public sealed class ApiKey : FilesObject
    {
        internal ApiKey()
        {
        }

        /// <summary>
        /// API Key ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Unique label that describes this API key.  Useful for external systems where you may have API keys from multiple accounts and want a human-readable label for each key.
        /// </summary>
        public string DescriptiveLabel { get; internal set; }

        /// <summary>
        /// User-supplied description of API key.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Time which API Key was created
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// API Key expiration date
        /// </summary>
        public DateTime? ExpiresAt { get; internal set; }

        /// <summary>
        /// API Key actual key string
        /// </summary>
        public FilesSecret Key { get; internal set; }

        /// <summary>
        /// If `true`, this API key will be usable with AWS-compatible endpoints, such as our Inbound S3-compatible endpoint.
        /// </summary>
        public bool AwsStyleCredentials { get; internal set; }

        /// <summary>
        /// AWS Access Key ID to use with AWS-compatible endpoints, such as our Inbound S3-compatible endpoint.
        /// </summary>
        public string AwsAccessKeyId { get; internal set; }

        /// <summary>
        /// AWS Secret Key to use with AWS-compatible endpoints, such as our Inbound S3-compatible endpoint.
        /// </summary>
        public FilesSecret AwsSecretKey { get; internal set; }

        /// <summary>
        /// API Key last used - note this value is only updated once per 3 hour period, so the 'actual' time of last use may be up to 3 hours later than this timestamp.
        /// </summary>
        public DateTime? LastUseAt { get; internal set; }

        /// <summary>
        /// Internal name for the API Key.  For your use.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Permissions for this API Key. Keys with the `desktop_app` permission set only have the ability to do the functions provided in our Desktop App (File and Share Link operations). Keys with the `mobile_app` permission set can only perform the functions provided by our Mobile App (File and Share Link operations). Keys with the `office_integration` permission set are auto generated, and automatically expire, to allow users to interact with office integration platforms. Keys with the `files_only` permission set can use only the files, folders, and file_actions endpoints, excluding the GPG Encrypt and GPG Decrypt file actions, where they perform file operations as a full-access file user in the key's workspace scope, along with `GET /file_migrations/{id}` and `GET /api_key`. On the migration lookup, any key with a `path` can read only migrations it started. Without a `path`, user-owned keys retain user-scoped access; site-wide Workspace-bound keys can read only migrations whose source and destination are both in their Workspace. Every `files_only` key is Workspace-bound, including Workspace `0`, as is a full-access key in a named Workspace. A full-access site-wide key in the default Workspace retains Site-wide migration access. Migrations outside the caller's visibility return `not-found`. Keys with `files_only` cannot use site admin, workspace admin, folder admin, group admin, partner admin, or billing privileges from the owning user. Both GPG file actions and every other endpoint deny them with `not-authorized/api-key-only-for-file-operations`. Folder-configured GPG encryption and decryption still apply to files uploaded with a `files_only` key.
        /// </summary>
        public string PermissionSet { get; internal set; }

        /// <summary>
        /// If this API key represents a Desktop app, what platform was it created on?
        /// </summary>
        public string Platform { get; internal set; }

        /// <summary>
        /// Site ID
        /// </summary>
        public long? SiteId { get; internal set; }

        /// <summary>
        /// Site Name
        /// </summary>
        public string SiteName { get; internal set; }

        /// <summary>
        /// URL for API host.
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// User ID for the owner of this API Key.  May be blank for Site-wide API Keys.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Workspace ID for this API Key. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(ApiKey source)
        {
            Id = source.Id;
            DescriptiveLabel = source.DescriptiveLabel;
            Description = source.Description;
            CreatedAt = source.CreatedAt;
            ExpiresAt = source.ExpiresAt;
            Key = source.Key;
            AwsStyleCredentials = source.AwsStyleCredentials;
            AwsAccessKeyId = source.AwsAccessKeyId;
            AwsSecretKey = source.AwsSecretKey;
            LastUseAt = source.LastUseAt;
            Name = source.Name;
            PermissionSet = source.PermissionSet;
            Platform = source.Platform;
            SiteId = source.SiteId;
            SiteName = source.SiteName;
            Url = source.Url;
            UserId = source.UserId;
            WorkspaceId = source.WorkspaceId;
        }
    }
}
