using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An OutboundConnectionLog is an audit log for monitoring and troubleshooting actions performed by your Remote Servers and Files.com on-premise Agents.
    /// </summary>
    public sealed class OutboundConnectionLog : FilesObject
    {
        internal OutboundConnectionLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// Remote Path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// End User IP
        /// </summary>
        public string ClientIp { get; internal set; }

        /// <summary>
        /// Source Remote Server ID
        /// </summary>
        public long? SrcRemoteServerId { get; internal set; }

        /// <summary>
        /// Source Remote Server name
        /// </summary>
        public string SrcRemoteServerName { get; internal set; }

        /// <summary>
        /// Destination Remote Server ID
        /// </summary>
        public long? DestRemoteServerId { get; internal set; }

        /// <summary>
        /// Destination Remote Server name
        /// </summary>
        public string DestRemoteServerName { get; internal set; }

        /// <summary>
        /// Operation Type
        /// </summary>
        public string Operation { get; internal set; }

        /// <summary>
        /// Error message, if applicable
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Error operation, if applicable
        /// </summary>
        public string ErrorOperation { get; internal set; }

        /// <summary>
        /// Error type, if applicable
        /// </summary>
        public string ErrorType { get; internal set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Duration (in milliseconds)
        /// </summary>
        public long? DurationMs { get; internal set; }

        /// <summary>
        /// Data Length in Bytes. Present for upload actions that transfer data.
        /// </summary>
        public long? BytesUploaded { get; internal set; }

        /// <summary>
        /// Data Length in Bytes. Present for download actions that transfer data.
        /// </summary>
        public long? BytesDownloaded { get; internal set; }

        /// <summary>
        /// Number of entries returned for a folder list action.
        /// </summary>
        public long? ListCount { get; internal set; }

        /// <summary>
        /// Start Time of Action
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
