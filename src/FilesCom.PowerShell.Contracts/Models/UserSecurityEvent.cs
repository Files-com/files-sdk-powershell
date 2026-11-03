using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A UserSecurityEvent is a log record for user security activity such as user lockouts.
    /// </summary>
    public sealed class UserSecurityEvent : FilesObject
    {
        internal UserSecurityEvent()
        {
        }

        /// <summary>
        /// Event ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Type of user security event being recorded.
        /// </summary>
        public string EventType { get; internal set; }

        /// <summary>
        /// Event body.
        /// </summary>
        public string Body { get; internal set; }

        /// <summary>
        /// Event errors.
        /// </summary>
        public string[] EventErrors { get; internal set; }

        /// <summary>
        /// Event create date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Link to log file.
        /// </summary>
        public string BodyUrl { get; internal set; }

        /// <summary>
        /// User ID.
        /// </summary>
        public long? UserId { get; internal set; }
    }
}
