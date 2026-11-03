using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An ExternalEvent is a log that is sent to the cloud from a client application such as the Files.com CLI.
    /// </summary>
    public sealed class ExternalEvent : FilesObject
    {
        internal ExternalEvent()
        {
        }

        /// <summary>
        /// Event ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Type of event being recorded.
        /// </summary>
        public string EventType { get; internal set; }

        /// <summary>
        /// Status of event.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Event body
        /// </summary>
        public string Body { get; internal set; }

        /// <summary>
        /// External event create date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Link to log file.
        /// </summary>
        public string BodyUrl { get; internal set; }
    }
}
