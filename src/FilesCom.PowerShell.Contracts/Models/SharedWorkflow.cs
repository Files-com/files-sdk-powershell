using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com shared workflow.
    /// </summary>
    public sealed class SharedWorkflow : FilesObject
    {
        internal SharedWorkflow()
        {
        }

        /// <summary>
        /// Shared Workflow ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Name displayed to recipients.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Description displayed to recipients.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Revision required when submitting a Shared Workflow run.
        /// </summary>
        public string SubmissionRevision { get; internal set; }

        /// <summary>
        /// Current custom questions in display order.
        /// </summary>
        public Models.FormField[] FormFields { get; internal set; }
    }
}
