using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A UsageSnapshot is a site-wide utilization report of storage, bandwidth, APIs, and user accounts of your Files.com site for the given time period.
    /// </summary>
    public sealed class UsageSnapshot : FilesObject
    {
        internal UsageSnapshot()
        {
        }

        /// <summary>
        /// Usage snapshot ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Usage snapshot start date/time
        /// </summary>
        public DateTime? StartAt { get; internal set; }

        /// <summary>
        /// Usage snapshot end date/time
        /// </summary>
        public DateTime? EndAt { get; internal set; }

        /// <summary>
        /// Highest user count number in time period
        /// </summary>
        public long? HighWaterUserCount { get; internal set; }

        /// <summary>
        /// Total Transformation and AI credits used in this usage period. Updated daily. Does not include credits consumed today.
        /// </summary>
        public long? TransformationCreditsUsage { get; internal set; }

        /// <summary>
        /// Current total Storage Usage GB as of end date (not necessarily high water mark, which is used for billing)
        /// </summary>
        public long? CurrentStorage { get; internal set; }

        /// <summary>
        /// Highest Storage Usage GB recorded in time period (used for billing)
        /// </summary>
        public long? HighWaterStorage { get; internal set; }

        /// <summary>
        /// Storage Usage for root folder as of end date (not necessarily high water mark, which is used for billing)
        /// </summary>
        public long? RootStorage { get; internal set; }

        /// <summary>
        /// Storage Usage for files that are deleted but uploaded within last 30 days as of end date (not necessarily high water mark, which is used for billing)
        /// </summary>
        public long? DeletedFilesCountedInMinimum { get; internal set; }

        /// <summary>
        /// Storage Usage for files that are deleted but retained as backups as of end date (not necessarily high water mark, which is used for billing)
        /// </summary>
        public long? DeletedFilesStorage { get; internal set; }

        /// <summary>
        /// Storage + Transfer Usage - Total Billable amount
        /// </summary>
        public long? TotalBillableUsage { get; internal set; }

        /// <summary>
        /// Transfer usage for period - Total Billable amount
        /// </summary>
        public long? TotalBillableTransferUsage { get; internal set; }

        /// <summary>
        /// Transfer Usage for period - Outbound GB from Files Native Storage
        /// </summary>
        public long? BytesSent { get; internal set; }

        /// <summary>
        /// Transfer Usage for period - Inbound GB to Remote Servers (Sync/Mount)
        /// </summary>
        public long? SyncBytesReceived { get; internal set; }

        /// <summary>
        /// Transfer Usage for period - Outbound GB from Remote Servers (Sync/Mount)
        /// </summary>
        public long? SyncBytesSent { get; internal set; }

        /// <summary>
        /// Storage Usage - map of root folders to their usage as of end date (not necessarily high water mark, which is used for billing)
        /// </summary>
        public Models.UsageByTopLevelDir[] UsageByTopLevelDir { get; internal set; }
    }
}
