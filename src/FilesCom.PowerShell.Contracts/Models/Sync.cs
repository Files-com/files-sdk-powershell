using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Sync represents a file synchronization job between two locations (local-remote, remote-remote, local-child_site, etc).
    /// </summary>
    public sealed class Sync : FilesObject
    {
        internal Sync()
        {
        }

        /// <summary>
        /// Sync ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Name for this sync job
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Description for this sync job
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Site ID this sync belongs to
        /// </summary>
        public long? SiteId { get; internal set; }

        /// <summary>
        /// Workspace ID this sync belongs to
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// User who created or owns this sync
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Absolute source path for the sync
        /// </summary>
        public string SrcPath { get; internal set; }

        /// <summary>
        /// Absolute destination path for the sync
        /// </summary>
        public string DestPath { get; internal set; }

        /// <summary>
        /// Remote server ID for the source (if remote)
        /// </summary>
        public long? SrcRemoteServerId { get; internal set; }

        /// <summary>
        /// Remote server ID for the destination (if remote)
        /// </summary>
        public long? DestRemoteServerId { get; internal set; }

        /// <summary>
        /// Source site ID if syncing from a child or partner site
        /// </summary>
        public long? SrcSiteId { get; internal set; }

        /// <summary>
        /// Destination site ID if syncing to a child or partner site
        /// </summary>
        public long? DestSiteId { get; internal set; }

        /// <summary>
        /// Is this a two-way sync?
        /// </summary>
        public bool TwoWay { get; internal set; }

        /// <summary>
        /// Keep files after copying?
        /// </summary>
        public bool KeepAfterCopy { get; internal set; }

        /// <summary>
        /// Delete empty folders after sync?
        /// </summary>
        public bool DeleteEmptyFolders { get; internal set; }

        /// <summary>
        /// Is this sync disabled?
        /// </summary>
        public bool Disabled { get; internal set; }

        /// <summary>
        /// Trigger type: daily, custom_schedule, or manual
        /// </summary>
        public string Trigger { get; internal set; }

        /// <summary>
        /// Some MFT services request an empty file (known as a trigger file) to signal the sync is complete and they can begin further processing. If trigger_file is set, a zero-byte file will be sent at the end of the sync.
        /// </summary>
        public string TriggerFile { get; internal set; }

        /// <summary>
        /// If true, the trigger file will be sent at the end of a successful sync even when no files were transferred.
        /// </summary>
        public bool AlwaysWriteTriggerFile { get; internal set; }

        /// <summary>
        /// Array of glob patterns to include
        /// </summary>
        public string[] IncludePatterns { get; internal set; }

        /// <summary>
        /// Array of glob patterns to exclude
        /// </summary>
        public string[] ExcludePatterns { get; internal set; }

        /// <summary>
        /// When this sync was created
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// When this sync was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        /// <summary>
        /// Frequency in minutes between syncs. If set, this value must be greater than or equal to the `remote_sync_interval` value for the site's plan. If left blank, the plan's `remote_sync_interval` will be used. This setting is only used if `trigger` is empty.
        /// </summary>
        public long? SyncIntervalMinutes { get; internal set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run this sync.  One of: `day`, `week`, `week_end`, `month`, `month_end`, `quarter`, `quarter_end`, `year`, `year_end`
        /// </summary>
        public string Interval { get; internal set; }

        /// <summary>
        /// If trigger type is `daily`, this specifies a day number to run in one of the supported intervals: `week`, `month`, `quarter`, `year`.
        /// </summary>
        public long? RecurringDay { get; internal set; }

        /// <summary>
        /// If trigger type is `daily`, this specifies one or more day numbers to run in one of the supported intervals: `week`, `month`, `quarter`, `year`.
        /// </summary>
        public long?[] RecurringDays { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the sync's schedule fields.
        /// </summary>
        public long? ScheduleId { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, Custom schedule description for when the sync should be run. 0-based days of the week. 0 is Sunday, 1 is Monday, etc.
        /// </summary>
        public long?[] ScheduleDaysOfWeek { get; internal set; }

        /// <summary>
        /// Times of day to run in HH:MM format. For `custom_schedule`, run at these times on specified days of week. For `daily`, run at these times on the scheduled interval date.
        /// </summary>
        public string[] ScheduleTimesOfDay { get; internal set; }

        /// <summary>
        /// Time zone for the schedule. If not set, times are interpreted as UTC.
        /// </summary>
        public string ScheduleTimeZone { get; internal set; }

        /// <summary>
        /// Skip the sync if there is a formal, observed holiday for this region.
        /// </summary>
        public string HolidayRegion { get; internal set; }

        /// <summary>
        /// The latest run of this sync
        /// </summary>
        public Models.SyncRun LatestSyncRun { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Sync source)
        {
            Id = source.Id;
            Name = source.Name;
            Description = source.Description;
            SiteId = source.SiteId;
            WorkspaceId = source.WorkspaceId;
            UserId = source.UserId;
            SrcPath = source.SrcPath;
            DestPath = source.DestPath;
            SrcRemoteServerId = source.SrcRemoteServerId;
            DestRemoteServerId = source.DestRemoteServerId;
            SrcSiteId = source.SrcSiteId;
            DestSiteId = source.DestSiteId;
            TwoWay = source.TwoWay;
            KeepAfterCopy = source.KeepAfterCopy;
            DeleteEmptyFolders = source.DeleteEmptyFolders;
            Disabled = source.Disabled;
            Trigger = source.Trigger;
            TriggerFile = source.TriggerFile;
            AlwaysWriteTriggerFile = source.AlwaysWriteTriggerFile;
            IncludePatterns = source.IncludePatterns;
            ExcludePatterns = source.ExcludePatterns;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
            SyncIntervalMinutes = source.SyncIntervalMinutes;
            Interval = source.Interval;
            RecurringDay = source.RecurringDay;
            RecurringDays = source.RecurringDays;
            ScheduleId = source.ScheduleId;
            ScheduleDaysOfWeek = source.ScheduleDaysOfWeek;
            ScheduleTimesOfDay = source.ScheduleTimesOfDay;
            ScheduleTimeZone = source.ScheduleTimeZone;
            HolidayRegion = source.HolidayRegion;
            LatestSyncRun = source.LatestSyncRun;
        }
    }
}
