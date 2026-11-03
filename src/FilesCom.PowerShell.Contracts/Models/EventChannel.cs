using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An EventChannel is a named grouping of EventSubscriptions.
    /// </summary>
    public sealed class EventChannel : FilesObject
    {
        internal EventChannel()
        {
        }

        /// <summary>
        /// Event Channel ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Event Channel name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Event Channel description.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Whether this Event Channel can dispatch events.
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Whether this Event Channel is the default destination for newly published events.
        /// </summary>
        public bool DefaultChannel { get; internal set; }

        /// <summary>
        /// Event Channel create date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Event Channel update date/time.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(EventChannel source)
        {
            Id = source.Id;
            Name = source.Name;
            WorkspaceId = source.WorkspaceId;
            Description = source.Description;
            Enabled = source.Enabled;
            DefaultChannel = source.DefaultChannel;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
