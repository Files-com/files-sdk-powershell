using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An FtpActionLog is an audit log for monitoring and troubleshooting operations made to your site through FTP protocol.
    /// </summary>
    public sealed class FtpActionLog : FilesObject
    {
        internal FtpActionLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// IP Address of FTP Client
        /// </summary>
        public string RemoteIp { get; internal set; }

        /// <summary>
        /// IP Address of FTP Server
        /// </summary>
        public string ServerIp { get; internal set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Unique ID of the Session
        /// </summary>
        public string SessionUuid { get; internal set; }

        /// <summary>
        /// FTP Sequence ID
        /// </summary>
        public long? SeqId { get; internal set; }

        /// <summary>
        /// Authentication Ciphers
        /// </summary>
        public string AuthCiphers { get; internal set; }

        /// <summary>
        /// Action Type
        /// </summary>
        public string ActionType { get; internal set; }

        /// <summary>
        /// Path as sent by the Client (may not match Files.com path due to user root folders for FTP). This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
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
        /// FTP Command Name
        /// </summary>
        public string Cmd { get; internal set; }

        /// <summary>
        /// FTP Command Param
        /// </summary>
        public string Param { get; internal set; }

        /// <summary>
        /// FTP Response Code
        /// </summary>
        public long? ResponseCode { get; internal set; }

        /// <summary>
        /// FTP Response Message
        /// </summary>
        public string ResponseMessage { get; internal set; }

        /// <summary>
        /// Number of entries returned when listing files and folders
        /// </summary>
        public long? EntriesReturned { get; internal set; }

        /// <summary>
        /// Whether FTP Action was successful.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// FTP Action status.
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
