using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// Represents a single in-progress file transfer within a running SyncRun.
    /// </summary>
    public sealed class SyncRunLiveTransfer : FilesObject
    {
        internal SyncRunLiveTransfer()
        {
        }

        /// <summary>
        /// The file path being transferred. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Status of this individual transfer
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Bytes transferred so far
        /// </summary>
        public long? BytesCopied { get; internal set; }

        /// <summary>
        /// Total bytes of the file being transferred
        /// </summary>
        public long? BytesTotal { get; internal set; }

        /// <summary>
        /// Transfer progress from 0.0 to 1.0
        /// </summary>
        public double? Percentage { get; internal set; }

        /// <summary>
        /// Estimated time remaining (human-readable)
        /// </summary>
        public string Eta { get; internal set; }

        /// <summary>
        /// When this individual transfer started
        /// </summary>
        public string StartedAt { get; internal set; }
    }
}
