using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com agent node.
    /// </summary>
    public sealed class AgentNode : FilesObject
    {
        internal AgentNode()
        {
        }

        /// <summary>
        /// Stable Agent installation ID
        /// </summary>
        public string NodeId { get; internal set; }

        /// <summary>
        /// Customer-configured Agent node name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Hostname reported by the Agent
        /// </summary>
        public string Hostname { get; internal set; }

        /// <summary>
        /// Configured traffic preference
        /// </summary>
        public string AvailabilityRole { get; internal set; }

        /// <summary>
        /// Whether this node currently has an available Agent instance
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Whether this node is the current default route for new unscoped work
        /// </summary>
        public bool IsDefault { get; internal set; }

        /// <summary>
        /// Whether the proxy recently validated a direct connection to this Agent node. False means direct transfers are enabled but not currently available; null means disabled or unsupported.
        /// </summary>
        public bool DirectTransferAvailable { get; internal set; }

        /// <summary>
        /// Most recent successful node observation
        /// </summary>
        public DateTime? LastSeenAt { get; internal set; }

        /// <summary>
        /// Current Agent processes for this node
        /// </summary>
        public Models.AgentNodeInstance[] Instances { get; internal set; }
    }
}
