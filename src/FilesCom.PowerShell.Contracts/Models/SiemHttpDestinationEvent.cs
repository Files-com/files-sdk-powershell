using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A SiemHttpDestinationEvent is a log record for SIEM publishing failures and recoveries.
    /// </summary>
    public sealed class SiemHttpDestinationEvent : FilesObject
    {
        internal SiemHttpDestinationEvent()
        {
        }

        /// <summary>
        /// Event ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Type of SIEM event being recorded.
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
        /// SIEM ID.
        /// </summary>
        public long? SiemHttpDestinationId { get; internal set; }
    }
}
