using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An ExpectationIncident groups ongoing failure behavior for an Expectation over time.
    /// </summary>
    public sealed class ExpectationIncident : FilesObject
    {
        internal ExpectationIncident()
        {
        }

        /// <summary>
        /// Expectation Incident ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Expectation ID.
        /// </summary>
        public long? ExpectationId { get; internal set; }

        /// <summary>
        /// Incident status.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// When the incident was opened.
        /// </summary>
        public DateTime? OpenedAt { get; internal set; }

        /// <summary>
        /// When the most recent failing evaluation contributing to the incident occurred.
        /// </summary>
        public DateTime? LastFailedAt { get; internal set; }

        /// <summary>
        /// When the incident was acknowledged.
        /// </summary>
        public DateTime? AcknowledgedAt { get; internal set; }

        /// <summary>
        /// When the current snooze expires.
        /// </summary>
        public DateTime? SnoozedUntil { get; internal set; }

        /// <summary>
        /// When the incident was resolved.
        /// </summary>
        public DateTime? ResolvedAt { get; internal set; }

        /// <summary>
        /// Evaluation that first opened the incident.
        /// </summary>
        public long? OpenedByEvaluationId { get; internal set; }

        /// <summary>
        /// Most recent evaluation linked to the incident.
        /// </summary>
        public long? LastEvaluationId { get; internal set; }

        /// <summary>
        /// Evaluation that resolved the incident.
        /// </summary>
        public long? ResolvedByEvaluationId { get; internal set; }

        /// <summary>
        /// Compact incident summary payload.
        /// </summary>
        public object Summary { get; internal set; }

        /// <summary>
        /// Creation time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Last update time.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(ExpectationIncident source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            ExpectationId = source.ExpectationId;
            Status = source.Status;
            OpenedAt = source.OpenedAt;
            LastFailedAt = source.LastFailedAt;
            AcknowledgedAt = source.AcknowledgedAt;
            SnoozedUntil = source.SnoozedUntil;
            ResolvedAt = source.ResolvedAt;
            OpenedByEvaluationId = source.OpenedByEvaluationId;
            LastEvaluationId = source.LastEvaluationId;
            ResolvedByEvaluationId = source.ResolvedByEvaluationId;
            Summary = source.Summary;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
