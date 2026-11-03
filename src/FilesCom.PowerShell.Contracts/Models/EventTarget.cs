using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An EventTarget is a delivery destination for EventRecords.
    /// </summary>
    public sealed class EventTarget : FilesObject
    {
        internal EventTarget()
        {
        }

        /// <summary>
        /// Event Target ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Event Target name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Event Target type.
        /// </summary>
        public string TargetType { get; internal set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace or site-wide.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// If true, this default-workspace target can receive events from all workspaces.
        /// </summary>
        public bool ApplyToAllWorkspaces { get; internal set; }

        /// <summary>
        /// Whether this Event Target can receive events.
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Event Target configuration. Fields depend on target_type.
        /// </summary>
        public object Config { get; internal set; }

        /// <summary>
        /// Event Target delivery policy. Email and folder targets support batch_interval in seconds, between 600 and 86400. Slack and Teams support delivery_mode (immediate or subfolder_summary), summary_path (empty string means root), and summary_delay (15–300 seconds, default 60). The summary applies to matching file activity under summary_path; other matching events retain immediate delivery.
        /// </summary>
        public object DeliveryPolicy { get; internal set; }

        /// <summary>
        /// Event Target create date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Event Target update date/time.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(EventTarget source)
        {
            Id = source.Id;
            Name = source.Name;
            TargetType = source.TargetType;
            WorkspaceId = source.WorkspaceId;
            ApplyToAllWorkspaces = source.ApplyToAllWorkspaces;
            Enabled = source.Enabled;
            Config = source.Config;
            DeliveryPolicy = source.DeliveryPolicy;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
