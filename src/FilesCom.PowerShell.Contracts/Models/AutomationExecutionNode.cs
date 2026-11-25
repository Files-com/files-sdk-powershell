using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com automation execution node.
    /// </summary>
    public sealed class AutomationExecutionNode : FilesObject
    {
        internal AutomationExecutionNode()
        {
        }

        /// <summary>
        /// Node ID from the pinned Automation definition.
        /// </summary>
        public string NodeId { get; internal set; }

        /// <summary>
        /// Node type.
        /// </summary>
        public string NodeType { get; internal set; }

        /// <summary>
        /// Node status.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Current node execution stage.
        /// </summary>
        public string RunStage { get; internal set; }

        /// <summary>
        /// Whether this node reused persisted output from an earlier run.
        /// </summary>
        public bool Reused { get; internal set; }

        /// <summary>
        /// Count of successful operations in this node.
        /// </summary>
        public long? SuccessfulOperations { get; internal set; }

        /// <summary>
        /// Count of failed operations in this node.
        /// </summary>
        public long? FailedOperations { get; internal set; }

        /// <summary>
        /// When this node started.
        /// </summary>
        public DateTime? StartedAt { get; internal set; }

        /// <summary>
        /// When this node completed.
        /// </summary>
        public DateTime? CompletedAt { get; internal set; }

        /// <summary>
        /// Node runtime in milliseconds.
        /// </summary>
        public long? DurationMs { get; internal set; }

        /// <summary>
        /// Ordered inbound edge references.
        /// </summary>
        public object[] Inputs { get; internal set; }

        /// <summary>
        /// Output counts, item kinds, and typed-error summaries by outlet.
        /// </summary>
        public object Outputs { get; internal set; }

        /// <summary>
        /// Available input items grouped by inlet. Resolve large values with item_refs and item_values.
        /// </summary>
        public object InputItems { get; internal set; }

        /// <summary>
        /// Output items grouped by outlet. Resolve large values with item_refs and item_values.
        /// </summary>
        public object OutputItems { get; internal set; }

        /// <summary>
        /// References with a path from the response root and a digest. Replace the null at each path with the value for that digest. Path segments are literal object keys or zero-based array indexes.
        /// </summary>
        public object[] ItemRefs { get; internal set; }

        /// <summary>
        /// Ordered entries with a digest, a value, and optional item_refs relative to that value. References inside entries point to earlier entries in this array.
        /// </summary>
        public object[] ItemValues { get; internal set; }
    }
}
