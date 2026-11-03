using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A RemoteBandwidthSnapshot is a remote bandwidth report for the given date.
    /// </summary>
    public sealed class RemoteBandwidthSnapshot : FilesObject
    {
        internal RemoteBandwidthSnapshot()
        {
        }

        /// <summary>
        /// Site bandwidth ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Site sync bandwidth report bytes received
        /// </summary>
        public long? SyncBytesReceived { get; internal set; }

        /// <summary>
        /// Site sync bandwidth report bytes sent
        /// </summary>
        public long? SyncBytesSent { get; internal set; }

        /// <summary>
        /// Time the site bandwidth report was logged
        /// </summary>
        public DateTime? LoggedAt { get; internal set; }

        /// <summary>
        /// ID of related Remote Server
        /// </summary>
        public long? RemoteServerId { get; internal set; }
    }
}
