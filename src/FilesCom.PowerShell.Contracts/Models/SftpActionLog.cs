using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An SFTPActionLog is an audit log for monitoring and troubleshooting operations made to your site through SFTP protocol.
    /// </summary>
    public sealed class SftpActionLog : FilesObject
    {
        internal SftpActionLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// IP Address of SFTP Client
        /// </summary>
        public string RemoteIp { get; internal set; }

        /// <summary>
        /// IP Address of SFTP Server
        /// </summary>
        public string ServerIp { get; internal set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Name of the SFTP Client provided at initial connection
        /// </summary>
        public string SshClientIdentification { get; internal set; }

        /// <summary>
        /// Unique ID of the Session
        /// </summary>
        public string SessionUuid { get; internal set; }

        /// <summary>
        /// SFTP Sequence ID
        /// </summary>
        public long? SeqId { get; internal set; }

        /// <summary>
        /// Authentication Method
        /// </summary>
        public string AuthMethod { get; internal set; }

        /// <summary>
        /// Authentication Ciphers
        /// </summary>
        public string AuthCiphers { get; internal set; }

        /// <summary>
        /// Action Type
        /// </summary>
        public string ActionType { get; internal set; }

        /// <summary>
        /// Path as sent by the Client (may not match Files.com path due to user root folders for SFTP). This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Path on Files.com
        /// </summary>
        public string TruePath { get; internal set; }

        /// <summary>
        /// Name of File
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// SFTP Response Code Returned to the Client.
        /// </summary>
        public string SftpResponseCode { get; internal set; }

        /// <summary>
        /// SFTP Response Message Returned to the Client.
        /// </summary>
        public string SftpResponseMessage { get; internal set; }

        /// <summary>
        /// MD5 Hash of the file. Not always available.
        /// </summary>
        public string Md5 { get; internal set; }

        /// <summary>
        /// Size of File That was Uploaded or Downloaded.
        /// </summary>
        public long? Size { get; internal set; }

        /// <summary>
        /// Data Length in Bytes. Present for actions that transfer data.
        /// </summary>
        public long? DataLength { get; internal set; }

        /// <summary>
        /// The number of bytes transferred for uploads and downloads.
        /// </summary>
        public long? BytesTransferred { get; internal set; }

        /// <summary>
        /// Number of entries returned when listing files and folders
        /// </summary>
        public long? EntriesReturned { get; internal set; }

        /// <summary>
        /// Whether SFTP Action was successful.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// SFTP Action status.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Duration (in milliseconds)
        /// </summary>
        public long? DurationMs { get; internal set; }

        /// <summary>
        /// Start Time of Action
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
