using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A BundleNotification is an E-Mail sent out to users when certain actions are performed on or within a shared set of files and folders.
    /// </summary>
    public sealed class BundleNotification : FilesObject
    {
        internal BundleNotification()
        {
        }

        /// <summary>
        /// Bundle ID to notify on
        /// </summary>
        public long? BundleId { get; internal set; }

        /// <summary>
        /// Bundle Notification ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Triggers bundle notification when a registration action occurs for it.
        /// </summary>
        public bool NotifyOnRegistration { get; internal set; }

        /// <summary>
        /// Triggers bundle notification when a upload action occurs for it.
        /// </summary>
        public bool NotifyOnUpload { get; internal set; }

        /// <summary>
        /// Is the current user the user to notify?
        /// </summary>
        public bool NotifyCurrentUser { get; internal set; }

        /// <summary>
        /// The id of the user to notify.
        /// </summary>
        public long? NotifyUserId { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(BundleNotification source)
        {
            BundleId = source.BundleId;
            Id = source.Id;
            NotifyOnRegistration = source.NotifyOnRegistration;
            NotifyOnUpload = source.NotifyOnUpload;
            NotifyCurrentUser = source.NotifyCurrentUser;
            NotifyUserId = source.NotifyUserId;
            WorkspaceId = source.WorkspaceId;
        }
    }
}
