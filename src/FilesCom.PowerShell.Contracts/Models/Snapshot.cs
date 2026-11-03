using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// Snapshots allow you to create a read-only archive of files at a specific point in time.
    /// </summary>
    public sealed class Snapshot : FilesObject
    {
        internal Snapshot()
        {
        }

        /// <summary>
        /// The snapshot's unique ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// When the snapshot expires.
        /// </summary>
        public DateTime? ExpiresAt { get; internal set; }

        /// <summary>
        /// When the snapshot was finalized.
        /// </summary>
        public DateTime? FinalizedAt { get; internal set; }

        /// <summary>
        /// A name for the snapshot.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The user that created this snapshot, if applicable.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// The bundle using this snapshot, if applicable.
        /// </summary>
        public long? BundleId { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Snapshot source)
        {
            Id = source.Id;
            ExpiresAt = source.ExpiresAt;
            FinalizedAt = source.FinalizedAt;
            Name = source.Name;
            UserId = source.UserId;
            BundleId = source.BundleId;
            WorkspaceId = source.WorkspaceId;
        }
    }
}
