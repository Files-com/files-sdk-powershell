using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A SyncLog is an audit log for monitoring and troubleshooting operations made by Remote Server Sync.
    /// </summary>
    public sealed class SyncLog : FilesObject
    {
        internal SyncLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// Sync ID
        /// </summary>
        public long? SyncId { get; internal set; }

        /// <summary>
        /// External Event ID used by legacy syncs
        /// </summary>
        public long? ExternalEventId { get; internal set; }

        /// <summary>
        /// Sync Run ID
        /// </summary>
        public long? SyncRunId { get; internal set; }

        /// <summary>
        /// Error type, if applicable
        /// </summary>
        public string ErrorType { get; internal set; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Operation type
        /// </summary>
        public string Operation { get; internal set; }

        /// <summary>
        /// File path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// File size
        /// </summary>
        public long? Size { get; internal set; }

        /// <summary>
        /// File type
        /// </summary>
        public string FileType { get; internal set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Start Time of Action
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
