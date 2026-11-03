using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com agent push update.
    /// </summary>
    public sealed class AgentPushUpdate : FilesObject
    {
        internal AgentPushUpdate()
        {
        }

        /// <summary>
        /// Pushed agent version
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// Update accepted or reason
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Installed agent version
        /// </summary>
        public string CurrentVersion { get; internal set; }

        /// <summary>
        /// Pending agent version or null
        /// </summary>
        public string PendingVersion { get; internal set; }

        /// <summary>
        /// Last error message or null
        /// </summary>
        public string LastError { get; internal set; }

        /// <summary>
        /// Error code
        /// </summary>
        public string Error { get; internal set; }
    }
}
