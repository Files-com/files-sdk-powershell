using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AutomationLog is an audit log for monitoring and troubleshooting triggered Automation.
    /// </summary>
    public sealed class AutomationLog : FilesObject
    {
        internal AutomationLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// Automation ID
        /// </summary>
        public long? AutomationId { get; internal set; }

        /// <summary>
        /// Automation Run ID
        /// </summary>
        public long? AutomationRunId { get; internal set; }

        /// <summary>
        /// Logical item correlation ID
        /// </summary>
        public string CorrelationId { get; internal set; }

        /// <summary>
        /// Destination path, for moves and copies
        /// </summary>
        public string DestPath { get; internal set; }

        /// <summary>
        /// Typed error, if applicable
        /// </summary>
        public string ErrorType { get; internal set; }

        /// <summary>
        /// ID of the Automation whose definition owns this node
        /// </summary>
        public long? SourceAutomationId { get; internal set; }

        /// <summary>
        /// Pinned version of the Automation whose definition owns this node
        /// </summary>
        public long? SourceAutomationVersion { get; internal set; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Node ID
        /// </summary>
        public string NodeId { get; internal set; }

        /// <summary>
        /// Node type
        /// </summary>
        public string NodeType { get; internal set; }

        /// <summary>
        /// Operation type
        /// </summary>
        public string Operation { get; internal set; }

        /// <summary>
        /// Node outlet used by the item
        /// </summary>
        public string Output { get; internal set; }

        /// <summary>
        /// File path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Start Time of Action
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
