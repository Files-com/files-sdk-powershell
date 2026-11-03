using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An Automation is an automated process of controlling workflows on your Files.com site.
    /// </summary>
    public sealed class Automation : FilesObject
    {
        internal Automation()
        {
        }

        /// <summary>
        /// Automation ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Ordinarily, we will allow automation runs to run in parallel for non-scheduled automations. If this flag is `true` we will force automation runs to be serialized (run one at a time, one after another). This can resolve some issues with race conditions on remote systems at the cost of some performance.
        /// </summary>
        public bool AlwaysSerializeJobs { get; internal set; }

        /// <summary>
        /// Ordinarily, files with identical size in the source and destination will be skipped from copy operations to prevent wasted transfer.  If this flag is `true` we will overwrite the destination file always.  Note that this may cause large amounts of wasted transfer usage.  This setting has no effect unless `overwrite_files` is also set to `true`.
        /// </summary>
        public bool AlwaysOverwriteSizeMatchingFiles { get; internal set; }

        /// <summary>
        /// Automation type
        /// </summary>
        public string AutomationType { get; internal set; }

        /// <summary>
        /// ID of this v1 Automation's converted v2 copy, when present.
        /// </summary>
        public long? ConvertedAutomationId { get; internal set; }

        /// <summary>
        /// Indicates if the automation has been deleted.
        /// </summary>
        public bool Deleted { get; internal set; }

        /// <summary>
        /// Description for the this Automation.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Automation v2 graph definition.
        /// </summary>
        public object Definition { get; internal set; }

        /// <summary>
        /// If set, this string in the destination path will be replaced with the value in `destination_replace_to`.
        /// </summary>
        public string DestinationReplaceFrom { get; internal set; }

        /// <summary>
        /// If set, this string will replace the value `destination_replace_from` in the destination filename. You can use special patterns here.
        /// </summary>
        public string DestinationReplaceTo { get; internal set; }

        /// <summary>
        /// Destination Paths
        /// </summary>
        public string[] Destinations { get; internal set; }

        /// <summary>
        /// If true, this automation will not run.
        /// </summary>
        public bool Disabled { get; internal set; }

        /// <summary>
        /// If set, this glob pattern will exclude files from the automation. Supports globs, except on remote mounts.
        /// </summary>
        public string ExcludePattern { get; internal set; }

        /// <summary>
        /// List of URLs to be imported and names to be used.
        /// </summary>
        public object[] ImportUrls { get; internal set; }

        /// <summary>
        /// If trigger is `email`, this is the address that triggers the Automation.
        /// </summary>
        public string InboundEmailAddress { get; internal set; }

        /// <summary>
        /// Normally copy and move automations that use globs will implicitly preserve the source folder structure in the destination.  If this flag is `true`, the source folder structure will be flattened in the destination.  This is useful for copying or moving files from multiple folders into a single destination folder.
        /// </summary>
        public bool FlattenDestinationStructure { get; internal set; }

        /// <summary>
        /// IDs of Groups for the Automation (i.e. who to Request File from)
        /// </summary>
        public long?[] GroupIds { get; internal set; }

        /// <summary>
        /// If true, the Lock Folders behavior will be disregarded for automated actions.
        /// </summary>
        public bool IgnoreLockedFolders { get; internal set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run this automation.  One of: `day`, `week`, `week_end`, `month`, `month_end`, `quarter`, `quarter_end`, `year`, `year_end`
        /// </summary>
        public string Interval { get; internal set; }

        /// <summary>
        /// Time when automation was last modified. Does not change for name or description updates.
        /// </summary>
        public DateTime? LastModifiedAt { get; internal set; }

        /// <summary>
        /// If `true`, use the legacy behavior for this automation, where it can operate on folders in addition to just files.  This behavior no longer works and should not be used.
        /// </summary>
        public bool LegacyFolderMatching { get; internal set; }

        /// <summary>
        /// Name for this automation.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// ID of the retained v1 Automation for a converted Automation. Re-enable that v1 Automation to pause this copy and revert the conversion.
        /// </summary>
        public long? OriginalAutomationId { get; internal set; }

        /// <summary>
        /// If true, existing files will be overwritten with new files on Move/Copy automations.  Note: by default files will not be overwritten on Copy automations if they appear to be the same file size as the newly incoming file.  Use the `always_overwrite_size_matching_files` option in conjunction with `overwrite_files` to override this behavior and overwrite files no matter what.
        /// </summary>
        public bool OverwriteFiles { get; internal set; }

        /// <summary>
        /// Path on which this Automation runs.  Supports globs, except on remote mounts. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Timezone to use when rendering timestamps in paths.
        /// </summary>
        public string PathTimeZone { get; internal set; }

        /// <summary>
        /// Current Automation v2 definition version.
        /// </summary>
        public long? Version { get; internal set; }

        /// <summary>
        /// If trigger type is `daily`, this specifies a day number to run in one of the supported intervals: `week`, `month`, `quarter`, `year`.
        /// </summary>
        public long? RecurringDay { get; internal set; }

        /// <summary>
        /// If trigger type is `daily`, this specifies one or more day numbers to run in one of the supported intervals: `week`, `month`, `quarter`, `year`.
        /// </summary>
        public long?[] RecurringDays { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the automation's schedule fields.
        /// </summary>
        public long? ScheduleId { get; internal set; }

        /// <summary>
        /// If the Automation fails, retry at this interval (in minutes).  Acceptable values are 5 through 1440 (one day).  Set to null to disable.
        /// </summary>
        public long? RetryOnFailureIntervalInMinutes { get; internal set; }

        /// <summary>
        /// If the Automation fails, retry at most this many times.  Maximum allowed value: 10.  Set to null to disable.
        /// </summary>
        public long? RetryOnFailureNumberOfAttempts { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, Custom schedule description for when the automation should be run in json format.
        /// </summary>
        public object Schedule { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule` or `daily` with times, Human readable schedule description for when the automation should be run.
        /// </summary>
        public string HumanReadableSchedule { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, Custom schedule description for when the automation should be run. 0 is Sunday, 1 is Monday, etc.
        /// </summary>
        public long?[] ScheduleDaysOfWeek { get; internal set; }

        /// <summary>
        /// Times of day to run in HH:MM format (24-hour). For `custom_schedule`, run at these times on specified days of week. For `daily`, run at these times on the scheduled interval date.
        /// </summary>
        public string[] ScheduleTimesOfDay { get; internal set; }

        /// <summary>
        /// Time zone for the schedule. If not set, times are interpreted as UTC.
        /// </summary>
        public string ScheduleTimeZone { get; internal set; }

        /// <summary>
        /// Source path/glob.  See Automation docs for exact description, but this is used to filter for files in the `path` to find files to operate on. Supports globs, except on remote mounts.
        /// </summary>
        public string Source { get; internal set; }

        /// <summary>
        /// IDs of Syncs to run by this Automation.
        /// </summary>
        public long?[] SyncIds { get; internal set; }

        /// <summary>
        /// If trigger is `action`, this is the list of action types on which to trigger the automation. Valid actions are create, copy, move, archived_delete, update, read, destroy
        /// </summary>
        public string[] TriggerActions { get; internal set; }

        /// <summary>
        /// How this automation is triggered to run.
        /// </summary>
        public string Trigger { get; internal set; }

        /// <summary>
        /// User ID of the Automation's creator.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// IDs of Users for the Automation (i.e. who to Request File from)
        /// </summary>
        public long?[] UserIds { get; internal set; }

        /// <summary>
        /// A Hash of attributes specific to the automation type.
        /// </summary>
        public object Value { get; internal set; }

        /// <summary>
        /// If trigger is `webhook`, this is the URL of the webhook to trigger the Automation.
        /// </summary>
        public string WebhookUrl { get; internal set; }

        /// <summary>
        /// Skip the automation if there is a formal, observed holiday for this region.
        /// </summary>
        public string HolidayRegion { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Automation source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            AlwaysSerializeJobs = source.AlwaysSerializeJobs;
            AlwaysOverwriteSizeMatchingFiles = source.AlwaysOverwriteSizeMatchingFiles;
            AutomationType = source.AutomationType;
            ConvertedAutomationId = source.ConvertedAutomationId;
            Deleted = source.Deleted;
            Description = source.Description;
            Definition = source.Definition;
            DestinationReplaceFrom = source.DestinationReplaceFrom;
            DestinationReplaceTo = source.DestinationReplaceTo;
            Destinations = source.Destinations;
            Disabled = source.Disabled;
            ExcludePattern = source.ExcludePattern;
            ImportUrls = source.ImportUrls;
            InboundEmailAddress = source.InboundEmailAddress;
            FlattenDestinationStructure = source.FlattenDestinationStructure;
            GroupIds = source.GroupIds;
            IgnoreLockedFolders = source.IgnoreLockedFolders;
            Interval = source.Interval;
            LastModifiedAt = source.LastModifiedAt;
            LegacyFolderMatching = source.LegacyFolderMatching;
            Name = source.Name;
            OriginalAutomationId = source.OriginalAutomationId;
            OverwriteFiles = source.OverwriteFiles;
            Path = source.Path;
            PathTimeZone = source.PathTimeZone;
            Version = source.Version;
            RecurringDay = source.RecurringDay;
            RecurringDays = source.RecurringDays;
            ScheduleId = source.ScheduleId;
            RetryOnFailureIntervalInMinutes = source.RetryOnFailureIntervalInMinutes;
            RetryOnFailureNumberOfAttempts = source.RetryOnFailureNumberOfAttempts;
            Schedule = source.Schedule;
            HumanReadableSchedule = source.HumanReadableSchedule;
            ScheduleDaysOfWeek = source.ScheduleDaysOfWeek;
            ScheduleTimesOfDay = source.ScheduleTimesOfDay;
            ScheduleTimeZone = source.ScheduleTimeZone;
            Source = source.Source;
            SyncIds = source.SyncIds;
            TriggerActions = source.TriggerActions;
            Trigger = source.Trigger;
            UserId = source.UserId;
            UserIds = source.UserIds;
            Value = source.Value;
            WebhookUrl = source.WebhookUrl;
            HolidayRegion = source.HolidayRegion;
        }
    }
}
