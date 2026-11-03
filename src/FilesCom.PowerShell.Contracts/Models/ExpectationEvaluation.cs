using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An ExpectationEvaluation records one open or closed window for an Expectation.
    /// </summary>
    public sealed class ExpectationEvaluation : FilesObject
    {
        internal ExpectationEvaluation()
        {
        }

        /// <summary>
        /// ExpectationEvaluation ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Expectation ID.
        /// </summary>
        public long? ExpectationId { get; internal set; }

        /// <summary>
        /// Evaluation status.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// How the evaluation window was opened.
        /// </summary>
        public string OpenedVia { get; internal set; }

        /// <summary>
        /// When the evaluation row was opened.
        /// </summary>
        public DateTime? OpenedAt { get; internal set; }

        /// <summary>
        /// Logical start of the candidate window.
        /// </summary>
        public DateTime? WindowStartAt { get; internal set; }

        /// <summary>
        /// Actual candidate cutoff boundary for the window.
        /// </summary>
        public DateTime? WindowEndAt { get; internal set; }

        /// <summary>
        /// Logical due boundary for schedule-driven windows.
        /// </summary>
        public DateTime? DeadlineAt { get; internal set; }

        /// <summary>
        /// Logical cutoff for late acceptance, when present.
        /// </summary>
        public DateTime? LateAcceptanceCutoffAt { get; internal set; }

        /// <summary>
        /// Hard stop after which the window may not remain open.
        /// </summary>
        public DateTime? HardCloseAt { get; internal set; }

        /// <summary>
        /// When the evaluation row was finalized.
        /// </summary>
        public DateTime? ClosedAt { get; internal set; }

        /// <summary>
        /// Captured evidence for files that matched the window.
        /// </summary>
        public object[] MatchedFiles { get; internal set; }

        /// <summary>
        /// Captured evidence for required files that were missing.
        /// </summary>
        public object[] MissingFiles { get; internal set; }

        /// <summary>
        /// Captured criteria failures for the window, including file-scoped FTS errors and returned validation details.
        /// </summary>
        public string[] CriteriaErrors { get; internal set; }

        /// <summary>
        /// Compact evaluator summary payload.
        /// </summary>
        public object Summary { get; internal set; }

        /// <summary>
        /// Creation time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Last update time.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }
    }
}
