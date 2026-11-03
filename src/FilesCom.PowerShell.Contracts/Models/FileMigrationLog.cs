using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A FileMigrationLog is an audit log for monitoring and troubleshooting File Migrations in your site.
    /// </summary>
    public sealed class FileMigrationLog : FilesObject
    {
        internal FileMigrationLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// File Migration ID
        /// </summary>
        public long? FileMigrationId { get; internal set; }

        /// <summary>
        /// Destination path, for moves and copies
        /// </summary>
        public string DestPath { get; internal set; }

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
        /// Status
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Start Time of Action
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
