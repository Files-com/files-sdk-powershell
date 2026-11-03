using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Desktop Configuration Profile centrally defines desktop mount point mappings for users in a Site or Workspace.
    /// </summary>
    public sealed class DesktopConfigurationProfile : FilesObject
    {
        internal DesktopConfigurationProfile()
        {
        }

        /// <summary>
        /// Desktop Configuration Profile ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Profile name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Whether this profile applies to all users in the Workspace by default
        /// </summary>
        public bool UseForAllUsers { get; internal set; }

        /// <summary>
        /// Whether the desktop app should hide drive mounting, prevent new drive mounts, and unmount active drive mounts for users with this profile
        /// </summary>
        public bool DisableDriveMounting { get; internal set; }

        /// <summary>
        /// Mount point mappings for the desktop app. Keys must be a single uppercase Windows drive letter other than A, B, or C, and values are Files.com paths to mount there.
        /// </summary>
        public object MountMappings { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(DesktopConfigurationProfile source)
        {
            Id = source.Id;
            Name = source.Name;
            WorkspaceId = source.WorkspaceId;
            UseForAllUsers = source.UseForAllUsers;
            DisableDriveMounting = source.DisableDriveMounting;
            MountMappings = source.MountMappings;
        }
    }
}
