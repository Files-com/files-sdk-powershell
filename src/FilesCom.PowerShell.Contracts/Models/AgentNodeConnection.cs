using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com agent node connection.
    /// </summary>
    public sealed class AgentNodeConnection : FilesObject
    {
        internal AgentNodeConnection()
        {
        }

        /// <summary>
        /// How the Agent process uses this proxy connection
        /// </summary>
        public string Mode { get; internal set; }

        /// <summary>
        /// Whether this connection was observed recently and has not disconnected
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Most recent successful observation for this connection
        /// </summary>
        public DateTime? LastSeenAt { get; internal set; }
    }
}
