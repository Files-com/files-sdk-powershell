using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A UsageDailySnapshot is a detailed site-wide usage report for the day.
    /// </summary>
    public sealed class UsageDailySnapshot : FilesObject
    {
        internal UsageDailySnapshot()
        {
        }

        /// <summary>
        /// ID of the usage record
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// The date of this usage record
        /// </summary>
        public DateTime? Date { get; internal set; }

        /// <summary>
        /// True if the API usage fields `read_api_usage` and `write_api_usage` can be relied upon.  If this is false, we suggest hiding that value from any UI.
        /// </summary>
        public bool ApiUsageAvailable { get; internal set; }

        /// <summary>
        /// Read API Calls used on this day. Note: only updated for days before the current day.
        /// </summary>
        public long? ReadApiUsage { get; internal set; }

        /// <summary>
        /// Write API Calls used on this day. Note: only updated for days before the current day.
        /// </summary>
        public long? WriteApiUsage { get; internal set; }

        /// <summary>
        /// Transformation and AI credits used on this day. Note: only updated for days before the current day.
        /// </summary>
        public long? TransformationCreditsUsage { get; internal set; }

        /// <summary>
        /// Number of billable users as of this day.
        /// </summary>
        public long? UserCount { get; internal set; }

        /// <summary>
        /// GB of Files Native Storage used on this day.
        /// </summary>
        public long? CurrentStorage { get; internal set; }

        /// <summary>
        /// GB of Files Native Storage used on this day for files that have been deleted and are stored as backups.
        /// </summary>
        public long? DeletedFilesStorage { get; internal set; }

        /// <summary>
        /// GB of Files Native Storage used on this day for files that have been permanently deleted but were uploaded less than 30 days ago, and are still billable.
        /// </summary>
        public long? DeletedFilesCountedInMinimum { get; internal set; }

        /// <summary>
        /// GB of Files Native Storage used for the root folder.  Included here because this value will not be part of `usage_by_top_level_dir`
        /// </summary>
        public long? RootStorage { get; internal set; }

        /// <summary>
        /// Usage broken down by each top-level folder
        /// </summary>
        public Models.UsageByTopLevelDir[] UsageByTopLevelDir { get; internal set; }
    }
}
