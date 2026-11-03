using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A File object represents a file or folder on your Files.com site.
    /// </summary>
    public sealed class RemoteFile : FilesObject
    {
        internal RemoteFile()
        {
        }

        /// <summary>
        /// File/Folder path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// User ID of the User who created the file/folder
        /// </summary>
        public long? CreatedById { get; internal set; }

        /// <summary>
        /// ID of the API key that created the file/folder
        /// </summary>
        public long? CreatedByApiKeyId { get; internal set; }

        /// <summary>
        /// ID of the AS2 Incoming Message that created the file/folder
        /// </summary>
        public long? CreatedByAs2IncomingMessageId { get; internal set; }

        /// <summary>
        /// ID of the Automation that created the file/folder
        /// </summary>
        public long? CreatedByAutomationId { get; internal set; }

        /// <summary>
        /// ID of the Bundle Registration that created the file/folder
        /// </summary>
        public long? CreatedByBundleRegistrationId { get; internal set; }

        /// <summary>
        /// ID of the Inbox that created the file/folder
        /// </summary>
        public long? CreatedByInboxId { get; internal set; }

        /// <summary>
        /// ID of the Remote Server that created the file/folder
        /// </summary>
        public long? CreatedByRemoteServerId { get; internal set; }

        /// <summary>
        /// ID of the Sync that created the file/folder
        /// </summary>
        public long? CreatedBySyncId { get; internal set; }

        /// <summary>
        /// Custom metadata map of keys and values. Limited to 32 keys, 256 characters per key and 1024 characters per value.
        /// </summary>
        public object CustomMetadata { get; internal set; }

        /// <summary>
        /// File/Folder display name
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Type: `directory` or `file`.
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// File/Folder size
        /// </summary>
        public long? Size { get; internal set; }

        /// <summary>
        /// File created date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// User ID of the User who last modified the file/folder
        /// </summary>
        public long? LastModifiedById { get; internal set; }

        /// <summary>
        /// ID of the API key that last modified the file/folder
        /// </summary>
        public long? LastModifiedByApiKeyId { get; internal set; }

        /// <summary>
        /// ID of the Automation that last modified the file/folder
        /// </summary>
        public long? LastModifiedByAutomationId { get; internal set; }

        /// <summary>
        /// ID of the Bundle Registration that last modified the file/folder
        /// </summary>
        public long? LastModifiedByBundleRegistrationId { get; internal set; }

        /// <summary>
        /// ID of the Remote Server that last modified the file/folder
        /// </summary>
        public long? LastModifiedByRemoteServerId { get; internal set; }

        /// <summary>
        /// ID of the Sync that last modified the file/folder
        /// </summary>
        public long? LastModifiedBySyncId { get; internal set; }

        /// <summary>
        /// File last modified date/time, according to the server.  This is the timestamp of the last Files.com operation of the file, regardless of what modified timestamp was sent.
        /// </summary>
        public DateTime? Mtime { get; internal set; }

        /// <summary>
        /// File last modified date/time, according to the client who set it.  Files.com allows desktop, FTP, SFTP, and WebDAV clients to set modified at times.  This allows Desktop&lt;-&gt;Cloud syncing to preserve modified at times.
        /// </summary>
        public DateTime? ProvidedMtime { get; internal set; }

        /// <summary>
        /// File CRC32 checksum. This is sometimes delayed, so if you get a blank response, wait and try again.
        /// </summary>
        public string Crc32 { get; internal set; }

        /// <summary>
        /// File MD5 checksum. This is sometimes delayed, so if you get a blank response, wait and try again.
        /// </summary>
        public string Md5 { get; internal set; }

        /// <summary>
        /// File SHA1 checksum. This is sometimes delayed, so if you get a blank response, wait and try again.
        /// </summary>
        public string Sha1 { get; internal set; }

        /// <summary>
        /// File SHA256 checksum. This is sometimes delayed, so if you get a blank response, wait and try again.
        /// </summary>
        public string Sha256 { get; internal set; }

        /// <summary>
        /// MIME Type.  This is determined by the filename extension and is not stored separately internally.
        /// </summary>
        public string MimeType { get; internal set; }

        /// <summary>
        /// Region location
        /// </summary>
        public string Region { get; internal set; }

        /// <summary>
        /// A short string representing the current user's permissions.  Can be `r` (Read),`w` (Write),`d` (Delete), `l` (List) or any combination
        /// </summary>
        public string Permissions { get; internal set; }

        /// <summary>
        /// Are subfolders locked and unable to be modified?
        /// </summary>
        public bool SubfoldersLocked { get; internal set; }

        /// <summary>
        /// Is this folder locked and unable to be modified?
        /// </summary>
        public bool IsLocked { get; internal set; }

        /// <summary>
        /// Link to download file. Provided only in response to a download request.
        /// </summary>
        public string DownloadUri { get; internal set; }

        /// <summary>
        /// Optional direct connection information for direct Agent transfer attempts
        /// </summary>
        public Models.DirectConnectionInfo DirectConnectionInfo { get; internal set; }

        /// <summary>
        /// Bookmark/priority color of file/folder
        /// </summary>
        public string PriorityColor { get; internal set; }

        /// <summary>
        /// File preview ID
        /// </summary>
        public long? PreviewId { get; internal set; }

        /// <summary>
        /// File preview
        /// </summary>
        public Models.Preview Preview { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(RemoteFile source)
        {
            Path = source.Path;
            CreatedById = source.CreatedById;
            CreatedByApiKeyId = source.CreatedByApiKeyId;
            CreatedByAs2IncomingMessageId = source.CreatedByAs2IncomingMessageId;
            CreatedByAutomationId = source.CreatedByAutomationId;
            CreatedByBundleRegistrationId = source.CreatedByBundleRegistrationId;
            CreatedByInboxId = source.CreatedByInboxId;
            CreatedByRemoteServerId = source.CreatedByRemoteServerId;
            CreatedBySyncId = source.CreatedBySyncId;
            CustomMetadata = source.CustomMetadata;
            DisplayName = source.DisplayName;
            Type = source.Type;
            Size = source.Size;
            CreatedAt = source.CreatedAt;
            LastModifiedById = source.LastModifiedById;
            LastModifiedByApiKeyId = source.LastModifiedByApiKeyId;
            LastModifiedByAutomationId = source.LastModifiedByAutomationId;
            LastModifiedByBundleRegistrationId = source.LastModifiedByBundleRegistrationId;
            LastModifiedByRemoteServerId = source.LastModifiedByRemoteServerId;
            LastModifiedBySyncId = source.LastModifiedBySyncId;
            Mtime = source.Mtime;
            ProvidedMtime = source.ProvidedMtime;
            Crc32 = source.Crc32;
            Md5 = source.Md5;
            Sha1 = source.Sha1;
            Sha256 = source.Sha256;
            MimeType = source.MimeType;
            Region = source.Region;
            Permissions = source.Permissions;
            SubfoldersLocked = source.SubfoldersLocked;
            IsLocked = source.IsLocked;
            DownloadUri = source.DownloadUri;
            DirectConnectionInfo = source.DirectConnectionInfo;
            PriorityColor = source.PriorityColor;
            PreviewId = source.PreviewId;
            Preview = source.Preview;
        }
    }
}
