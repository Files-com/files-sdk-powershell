using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A PendingWorkEvent is a log record for pending file work failures, such as GPG Encryption and GPG Decryption.
    /// </summary>
    public sealed class PendingWorkEvent : FilesObject
    {
        internal PendingWorkEvent()
        {
        }

        /// <summary>
        /// Event ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Type of pending work event being recorded.
        /// </summary>
        public string EventType { get; internal set; }

        /// <summary>
        /// Status of event.
        /// </summary>
        public string Status { get; internal set; }

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
        /// Folder Behavior ID.
        /// </summary>
        public long? FolderBehaviorId { get; internal set; }
    }
}
