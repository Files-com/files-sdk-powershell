using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A KeyLifecycleRule represents a rule that applies to API keys, GPG keys, and SSH keys (also called User Public Keys) based on their inactivity or age.
    /// </summary>
    public sealed class KeyLifecycleRule : FilesObject
    {
        internal KeyLifecycleRule()
        {
        }

        /// <summary>
        /// Key Lifecycle Rule ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Key type for which the rule will apply (gpg, ssh, or api).
        /// </summary>
        public string KeyType { get; internal set; }

        /// <summary>
        /// Number of days of inactivity before the rule applies.
        /// </summary>
        public long? InactivityDays { get; internal set; }

        /// <summary>
        /// Number of days after creation before an SSH key expires. Applies only to SSH keys.
        /// </summary>
        public long? ExpirationDays { get; internal set; }

        /// <summary>
        /// If true, a default-workspace rule also applies to keys in all workspaces.
        /// </summary>
        public bool ApplyToAllWorkspaces { get; internal set; }

        /// <summary>
        /// Key Lifecycle Rule name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(KeyLifecycleRule source)
        {
            Id = source.Id;
            KeyType = source.KeyType;
            InactivityDays = source.InactivityDays;
            ExpirationDays = source.ExpirationDays;
            ApplyToAllWorkspaces = source.ApplyToAllWorkspaces;
            Name = source.Name;
            WorkspaceId = source.WorkspaceId;
        }
    }
}
