using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com agent node instance.
    /// </summary>
    public sealed class AgentNodeInstance : FilesObject
    {
        internal AgentNodeInstance()
        {
        }

        /// <summary>
        /// Ephemeral ID for this running Agent process
        /// </summary>
        public string InstanceId { get; internal set; }

        /// <summary>
        /// Role of this process during an Agent update
        /// </summary>
        public string ProcessState { get; internal set; }

        /// <summary>
        /// Whether this process has an available proxy connection
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Whether this process receives new unscoped work for its node
        /// </summary>
        public bool IsDefault { get; internal set; }

        /// <summary>
        /// Agent version reported by this process
        /// </summary>
        public string AgentVersion { get; internal set; }

        /// <summary>
        /// Most recent successful observation for this process
        /// </summary>
        public DateTime? LastSeenAt { get; internal set; }

        /// <summary>
        /// Proxy connections observed for this process
        /// </summary>
        public Models.AgentNodeConnection[] Connections { get; internal set; }
    }
}
