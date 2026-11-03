using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A PublicHostingRequestLog is an audit log for monitoring requests we received to access your publicly served folders.
    /// </summary>
    public sealed class PublicHostingRequestLog : FilesObject
    {
        internal PublicHostingRequestLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// IP Address of Public Hosting Client.
        /// </summary>
        public string RemoteIp { get; internal set; }

        /// <summary>
        /// IP Address of Public Hosting Server.
        /// </summary>
        public string ServerIp { get; internal set; }

        /// <summary>
        /// HTTP Request Hostname.
        /// </summary>
        public string Hostname { get; internal set; }

        /// <summary>
        /// HTTP Request Path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// HTTP Response Code.
        /// </summary>
        public long? ResponseCode { get; internal set; }

        /// <summary>
        /// Whether SFTP Action was successful.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// Duration (in milliseconds).
        /// </summary>
        public long? DurationMs { get; internal set; }

        /// <summary>
        /// Start Time of Action.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// The number of bytes transferred for file downloads.
        /// </summary>
        public long? BytesTransferred { get; internal set; }

        /// <summary>
        /// Method of the HTTP call.
        /// </summary>
        public string HttpMethod { get; internal set; }
    }
}
