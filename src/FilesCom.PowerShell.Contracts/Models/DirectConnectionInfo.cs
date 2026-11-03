using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com direct connection info.
    /// </summary>
    public sealed class DirectConnectionInfo : FilesObject
    {
        internal DirectConnectionInfo()
        {
        }

        /// <summary>
        /// Direct connection information schema version.
        /// </summary>
        public long? Version { get; internal set; }

        /// <summary>
        /// TLS server name (SNI and Host header) for the Agent's direct transfer listener.
        /// </summary>
        public string ServerName { get; internal set; }

        /// <summary>
        /// Validated ip:port candidates that may be dialed over TCP+TLS for this transfer.
        /// </summary>
        public string[] Addresses { get; internal set; }

        /// <summary>
        /// Signed HTTPS URI for direct Agent transfer traffic.
        /// </summary>
        public string DirectUri { get; internal set; }

        /// <summary>
        /// PEM-encoded CA certificate used to verify the Agent's direct transfer TLS certificate.
        /// </summary>
        public string CaPem { get; internal set; }
    }
}
