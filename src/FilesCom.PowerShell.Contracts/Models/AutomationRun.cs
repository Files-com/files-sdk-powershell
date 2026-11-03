using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AutomationRun is a record with information about a single execution of a given Automation.
    /// </summary>
    public sealed class AutomationRun : FilesObject
    {
        internal AutomationRun()
        {
        }

        /// <summary>
        /// ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the associated Automation.
        /// </summary>
        public long? AutomationId { get; internal set; }

        /// <summary>
        /// ID of the immutable Automation version pinned by this run.
        /// </summary>
        public long? AutomationVersionId { get; internal set; }

        /// <summary>
        /// Pinned Automation v2 definition version.
        /// </summary>
        public long? Version { get; internal set; }

        /// <summary>
        /// Workspace ID.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Date/time at which cancellation was requested.
        /// </summary>
        public DateTime? CancelRequestedAt { get; internal set; }

        /// <summary>
        /// Automation run completion/failure date/time.
        /// </summary>
        public DateTime? CompletedAt { get; internal set; }

        /// <summary>
        /// Automation run start date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// If set, this automation will be retried at this date/time due to `failure` or `partial_failure`.
        /// </summary>
        public DateTime? RetryAt { get; internal set; }

        /// <summary>
        /// If set, this Automation run was retried due to `failure` or `partial_failure`.
        /// </summary>
        public DateTime? RetriedAt { get; internal set; }

        /// <summary>
        /// ID of the run that is or will be retrying this run.
        /// </summary>
        public long? RetriedInRunId { get; internal set; }

        /// <summary>
        /// ID of the original run that this run is retrying.
        /// </summary>
        public long? RetryOfRunId { get; internal set; }

        /// <summary>
        /// ID of the run whose persisted node outputs this run reused.
        /// </summary>
        public long? RerunOfRunId { get; internal set; }

        /// <summary>
        /// Node at which this run resumed execution.
        /// </summary>
        public string RerunFromNodeId { get; internal set; }

        /// <summary>
        /// Automation run runtime.
        /// </summary>
        public double? Runtime { get; internal set; }

        /// <summary>
        /// The status of the AutomationRun. One of `queued`, `running`, `success`, `partial_failure`, `failure`, `skipped`, or `canceled`.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Count of successful operations.
        /// </summary>
        public long? SuccessfulOperations { get; internal set; }

        /// <summary>
        /// Count of failed operations.
        /// </summary>
        public long? FailedOperations { get; internal set; }

        /// <summary>
        /// Automation definition snapshot pinned by this run. For performance reasons, this is not provided when listing Automation runs.
        /// </summary>
        public object Definition { get; internal set; }

        /// <summary>
        /// Status and execution stage for each node in this run. For performance reasons, this is not provided when listing Automation runs.
        /// </summary>
        public object NodeStates { get; internal set; }

        /// <summary>
        /// Execution status, timing, and bounded output summaries for each node. For performance reasons, this is not provided when listing Automation runs.
        /// </summary>
        public Models.AutomationExecutionNode[] ExecutionNodes { get; internal set; }

        /// <summary>
        /// Link to the run journal artifact.
        /// </summary>
        public string JournalUrl { get; internal set; }

        /// <summary>
        /// Link to status messages log file.
        /// </summary>
        public string StatusMessagesUrl { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(AutomationRun source)
        {
            Id = source.Id;
            AutomationId = source.AutomationId;
            AutomationVersionId = source.AutomationVersionId;
            Version = source.Version;
            WorkspaceId = source.WorkspaceId;
            CancelRequestedAt = source.CancelRequestedAt;
            CompletedAt = source.CompletedAt;
            CreatedAt = source.CreatedAt;
            RetryAt = source.RetryAt;
            RetriedAt = source.RetriedAt;
            RetriedInRunId = source.RetriedInRunId;
            RetryOfRunId = source.RetryOfRunId;
            RerunOfRunId = source.RerunOfRunId;
            RerunFromNodeId = source.RerunFromNodeId;
            Runtime = source.Runtime;
            Status = source.Status;
            SuccessfulOperations = source.SuccessfulOperations;
            FailedOperations = source.FailedOperations;
            Definition = source.Definition;
            NodeStates = source.NodeStates;
            ExecutionNodes = source.ExecutionNodes;
            JournalUrl = source.JournalUrl;
            StatusMessagesUrl = source.StatusMessagesUrl;
        }
    }
}
