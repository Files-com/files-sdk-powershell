using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An EmailLog is a record of the email notifications that were sent.
    /// </summary>
    public sealed class EmailLog : FilesObject
    {
        internal EmailLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// Log Message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Status of E-Mail delivery
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Subject line of E-Mail
        /// </summary>
        public string Subject { get; internal set; }

        /// <summary>
        /// To field of E-Mail
        /// </summary>
        public string To { get; internal set; }

        /// <summary>
        /// CC field of E-Mail
        /// </summary>
        public string Cc { get; internal set; }

        /// <summary>
        /// How was the email delivered?  `customer_smtp` or `files.com`
        /// </summary>
        public string DeliveryMethod { get; internal set; }

        /// <summary>
        /// Customer SMTP Hostname used.
        /// </summary>
        public string SmtpHostname { get; internal set; }

        /// <summary>
        /// Customer SMTP IP address as resolved for use (useful for troubleshooting DNS issues with customer SMTP).
        /// </summary>
        public string SmtpIp { get; internal set; }

        /// <summary>
        /// Start Time of Action
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
