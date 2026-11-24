using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AutomationVersion is an immutable revision of an Automation v2 graph.
    /// </summary>
    public sealed class AutomationVersion : FilesObject
    {
        internal AutomationVersion()
        {
        }

        /// <summary>
        /// Automation revision ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the Automation that owns this revision.
        /// </summary>
        public long? AutomationId { get; internal set; }

        /// <summary>
        /// Workspace that owns the Automation. Zero is the Default Workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Revision number within the Automation.
        /// </summary>
        public long? Version { get; internal set; }

        /// <summary>
        /// Time the revision was saved.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// ID of the user who saved the revision, when available.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Username recorded when the revision was saved, when available.
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// ID of the API key used to save the revision, when available.
        /// </summary>
        public long? ApiKeyId { get; internal set; }

        /// <summary>
        /// Saved Automation v2 graph definition. Included when retrieving a single revision.
        /// </summary>
        public object Definition { get; internal set; }
    }
}
