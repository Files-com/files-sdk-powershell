using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AI Task defines a Files.com AI prompt that can run on a schedule or in response to file actions.
    /// </summary>
    public sealed class AiTask : FilesObject
    {
        internal AiTask()
        {
        }

        /// <summary>
        /// AI Task ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// AI Task name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// AI Task description.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Prompt sent when this AI Task is invoked.
        /// </summary>
        public string Prompt { get; internal set; }

        /// <summary>
        /// Permissions used by the internal API key for this AI Task. Valid values are `full` and `files_only`.
        /// </summary>
        public string PermissionSet { get; internal set; }

        /// <summary>
        /// Path scope used for action-triggered AI Tasks. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Source glob used with `path` for action-triggered AI Tasks.
        /// </summary>
        public string Source { get; internal set; }

        /// <summary>
        /// If true, this AI Task will not run.
        /// </summary>
        public bool Disabled { get; internal set; }

        /// <summary>
        /// How this AI Task is triggered.
        /// </summary>
        public string Trigger { get; internal set; }

        /// <summary>
        /// If trigger is `action`, the file action types that invoke this AI Task. Valid actions are create, copy, move, archived_delete, update, read, destroy.
        /// </summary>
        public string[] TriggerActions { get; internal set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the AI Task.
        /// </summary>
        public string Interval { get; internal set; }

        /// <summary>
        /// If trigger is `daily`, this selects the day number inside the chosen interval.
        /// </summary>
        public long? RecurringDay { get; internal set; }

        /// <summary>
        /// If trigger is `daily`, this selects one or more day numbers inside a `week`, `month`, `quarter`, or `year` interval.
        /// </summary>
        public long?[] RecurringDays { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the AI Task's schedule fields.
        /// </summary>
        public long? ScheduleId { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the 0-based weekdays used by the schedule.
        /// </summary>
        public long?[] ScheduleDaysOfWeek { get; internal set; }

        /// <summary>
        /// Times of day in HH:MM format for the AI Task schedule.
        /// </summary>
        public string[] ScheduleTimesOfDay { get; internal set; }

        /// <summary>
        /// Time zone used by the AI Task schedule.
        /// </summary>
        public string ScheduleTimeZone { get; internal set; }

        /// <summary>
        /// Optional holiday region used by the AI Task schedule.
        /// </summary>
        public string HolidayRegion { get; internal set; }

        /// <summary>
        /// Human-readable schedule description.
        /// </summary>
        public string HumanReadableSchedule { get; internal set; }

        /// <summary>
        /// Most recent successful invocation time.
        /// </summary>
        public DateTime? LastRunAt { get; internal set; }

        /// <summary>
        /// Master User ID used for AI Task invocations.
        /// </summary>
        public long? MasterAdminUserId { get; internal set; }

        /// <summary>
        /// Creation time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Last update time.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(AiTask source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Name = source.Name;
            Description = source.Description;
            Prompt = source.Prompt;
            PermissionSet = source.PermissionSet;
            Path = source.Path;
            Source = source.Source;
            Disabled = source.Disabled;
            Trigger = source.Trigger;
            TriggerActions = source.TriggerActions;
            Interval = source.Interval;
            RecurringDay = source.RecurringDay;
            RecurringDays = source.RecurringDays;
            ScheduleId = source.ScheduleId;
            ScheduleDaysOfWeek = source.ScheduleDaysOfWeek;
            ScheduleTimesOfDay = source.ScheduleTimesOfDay;
            ScheduleTimeZone = source.ScheduleTimeZone;
            HolidayRegion = source.HolidayRegion;
            HumanReadableSchedule = source.HumanReadableSchedule;
            LastRunAt = source.LastRunAt;
            MasterAdminUserId = source.MasterAdminUserId;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
