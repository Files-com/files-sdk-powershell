using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An InboxRecipient is a person who has had an inbox shared with them via email invitation.
    /// </summary>
    public sealed class InboxRecipient : FilesObject
    {
        internal InboxRecipient()
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
        /// A note sent to the recipient with the inbox.
        /// </summary>
        public string Note { get; internal set; }

        /// <summary>
        /// The recipient's email address.
        /// </summary>
        public string Recipient { get; internal set; }

        /// <summary>
        /// When the Inbox was shared with this recipient.
        /// </summary>
        public DateTime? SentAt { get; internal set; }
    }
}
