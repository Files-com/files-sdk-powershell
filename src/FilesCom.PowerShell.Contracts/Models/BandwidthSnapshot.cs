using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A BandwidthSnapshot is information about site bandwidth usage for the given date.
    /// </summary>
    public sealed class BandwidthSnapshot : FilesObject
    {
        internal BandwidthSnapshot()
        {
        }

        /// <summary>
        /// Site bandwidth ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Site bandwidth report bytes received
        /// </summary>
        public long? BytesReceived { get; internal set; }

        /// <summary>
        /// Site bandwidth report bytes sent
        /// </summary>
        public long? BytesSent { get; internal set; }

        /// <summary>
        /// Site sync bandwidth report bytes received
        /// </summary>
        public long? SyncBytesReceived { get; internal set; }

        /// <summary>
        /// Site sync bandwidth report bytes sent
        /// </summary>
        public long? SyncBytesSent { get; internal set; }

        /// <summary>
        /// Site bandwidth report get requests
        /// </summary>
        public long? RequestsGet { get; internal set; }

        /// <summary>
        /// Site bandwidth report put requests
        /// </summary>
        public long? RequestsPut { get; internal set; }

        /// <summary>
        /// Site bandwidth report other requests
        /// </summary>
        public long? RequestsOther { get; internal set; }

        /// <summary>
        /// Time the site bandwidth report was logged
        /// </summary>
        public DateTime? LoggedAt { get; internal set; }
    }
}
