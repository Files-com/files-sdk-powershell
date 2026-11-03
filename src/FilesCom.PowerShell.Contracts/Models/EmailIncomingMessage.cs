using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An EmailIncomingMessage is an audit log designed to track status of email to inbox uploads.
    /// </summary>
    public sealed class EmailIncomingMessage : FilesObject
    {
        internal EmailIncomingMessage()
        {
        }

        /// <summary>
        /// Id of the Email Incoming Message
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Id of the Inbox associated with this message
        /// </summary>
        public long? InboxId { get; internal set; }

        /// <summary>
        /// Sender of the email
        /// </summary>
        public string Sender { get; internal set; }

        /// <summary>
        /// Sender name
        /// </summary>
        public string SenderName { get; internal set; }

        /// <summary>
        /// Status of the message
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Body of the email
        /// </summary>
        public string Body { get; internal set; }

        /// <summary>
        /// Message describing the failure
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Message creation date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Title of the Inbox associated with this message
        /// </summary>
        public string InboxTitle { get; internal set; }
    }
}
