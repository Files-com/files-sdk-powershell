using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A FileMigration is a background operation on one or more files, such as a copy or a region migration.
    /// </summary>
    public sealed class FileMigration : FilesObject
    {
        internal FileMigration()
        {
        }

        /// <summary>
        /// File migration ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Source path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Destination path
        /// </summary>
        public string DestPath { get; internal set; }

        /// <summary>
        /// Reason for the failure, if applicable.
        /// </summary>
        public string FailureMessage { get; internal set; }

        /// <summary>
        /// Number of files processed
        /// </summary>
        public long? FilesMoved { get; internal set; }

        /// <summary>
        /// FilesTotal
        /// </summary>
        public long? FilesTotal { get; internal set; }

        /// <summary>
        /// The type of operation
        /// </summary>
        public string Operation { get; internal set; }

        /// <summary>
        /// Region
        /// </summary>
        public string Region { get; internal set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Link to download the log file for this migration.
        /// </summary>
        public string LogUrl { get; internal set; }
    }
}
