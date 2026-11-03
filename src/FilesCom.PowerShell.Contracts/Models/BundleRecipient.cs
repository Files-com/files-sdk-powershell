using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A BundleRecipient is a person who has had a bundle shared with them via email invite.
    /// </summary>
    public sealed class BundleRecipient : FilesObject
    {
        internal BundleRecipient()
        {
        }

        /// <summary>
        /// The recipient's company.
        /// </summary>
        public string Company { get; internal set; }

        /// <summary>
        /// The recipient's name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// A note sent to the recipient with the bundle.
        /// </summary>
        public string Note { get; internal set; }

        /// <summary>
        /// The recipient's email address.
        /// </summary>
        public string Recipient { get; internal set; }

        /// <summary>
        /// When the Bundle was shared with this recipient.
        /// </summary>
        public DateTime? SentAt { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }
    }
}
