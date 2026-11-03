using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A SyncRun represents a single execution (run) of a Sync job.
    /// </summary>
    public sealed class SyncRun : FilesObject
    {
        internal SyncRun()
        {
        }

        /// <summary>
        /// SyncRun ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Log or summary body for this run
        /// </summary>
        public string Body { get; internal set; }

        /// <summary>
        /// Total bytes synced in this run
        /// </summary>
        public long? BytesSynced { get; internal set; }

        /// <summary>
        /// Number of files compared
        /// </summary>
        public long? ComparedFiles { get; internal set; }

        /// <summary>
        /// Number of folders compared
        /// </summary>
        public long? ComparedFolders { get; internal set; }

        /// <summary>
        /// When this run was completed
        /// </summary>
        public DateTime? CompletedAt { get; internal set; }

        /// <summary>
        /// When this run was created
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Destination remote server type, if any
        /// </summary>
        public string DestRemoteServerType { get; internal set; }

        /// <summary>
        /// Whether this run was a dry run (no actual changes made)
        /// </summary>
        public bool DryRun { get; internal set; }

        /// <summary>
        /// Number of files that errored
        /// </summary>
        public long? ErroredFiles { get; internal set; }

        /// <summary>
        /// Estimated bytes count for this run
        /// </summary>
        public long? EstimatedBytesCount { get; internal set; }

        /// <summary>
        /// Array of errors encountered during the run
        /// </summary>
        public string[] EventErrors { get; internal set; }

        /// <summary>
        /// Link to external log file.
        /// </summary>
        public string LogUrl { get; internal set; }

        /// <summary>
        /// Total runtime in seconds
        /// </summary>
        public double? Runtime { get; internal set; }

        /// <summary>
        /// Site ID
        /// </summary>
        public long? SiteId { get; internal set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Source remote server type, if any
        /// </summary>
        public string SrcRemoteServerType { get; internal set; }

        /// <summary>
        /// Status of the sync run (success, failure, partial_failure, in_progress, skipped)
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Number of files successfully synced
        /// </summary>
        public long? SuccessfulFiles { get; internal set; }

        /// <summary>
        /// ID of the Sync this run belongs to
        /// </summary>
        public long? SyncId { get; internal set; }

        /// <summary>
        /// Name of the Sync this run belongs to
        /// </summary>
        public string SyncName { get; internal set; }

        /// <summary>
        /// When this run was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        /// <summary>
        /// Array of in-progress file transfers with progress data. Only present when the sync run status is in_progress.
        /// </summary>
        public Models.SyncRunLiveTransfer[] LiveTransfers { get; internal set; }
    }
}
