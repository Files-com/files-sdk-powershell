using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A WebDAVActionLog is an audit log for monitoring and troubleshooting operations made to your site through WebDAV protocol.
    /// </summary>
    public sealed class WebDavActionLog : FilesObject
    {
        internal WebDavActionLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// IP Address of WebDAV Client
        /// </summary>
        public string RemoteIp { get; internal set; }

        /// <summary>
        /// IP Address of WebDAV Server
        /// </summary>
        public string ServerIp { get; internal set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Authentication Ciphers
        /// </summary>
        public string AuthCiphers { get; internal set; }

        /// <summary>
        /// Action Type
        /// </summary>
        public string ActionType { get; internal set; }

        /// <summary>
        /// Path as sent by the Client (may not match Files.com path due to user root folders for WebDAV). This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
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
        /// Method of the HTTP call.
        /// </summary>
        public string HttpMethod { get; internal set; }

        /// <summary>
        /// Path of the HTTP call.
        /// </summary>
        public string HttpPath { get; internal set; }

        /// <summary>
        /// HTTP Response Code returned to the WebDAV Client.
        /// </summary>
        public long? HttpResponseCode { get; internal set; }

        /// <summary>
        /// Size of File That was Uploaded or Downloaded.
        /// </summary>
        public long? Size { get; internal set; }

        /// <summary>
        /// Number of entries returned when listing files and folders
        /// </summary>
        public long? EntriesReturned { get; internal set; }

        /// <summary>
        /// Whether WebDAV Action was successful.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// WebDAV Action status.
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
