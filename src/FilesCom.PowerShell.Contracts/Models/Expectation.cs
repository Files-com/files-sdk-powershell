using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// Expectations let your Files.com site define what “correct” file delivery looks like, continuously evaluate whether it happened, and keep history when it did not.
    /// </summary>
    public sealed class Expectation : FilesObject
    {
        internal Expectation()
        {
        }

        /// <summary>
        /// Expectation ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Expectation name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Expectation description.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Path scope for the expectation. Supports workspace-relative presentation. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Source glob used to select candidate files.
        /// </summary>
        public string Source { get; internal set; }

        /// <summary>
        /// Optional source exclusion glob.
        /// </summary>
        public string ExcludePattern { get; internal set; }

        /// <summary>
        /// If true, the expectation is disabled.
        /// </summary>
        public bool Disabled { get; internal set; }

        /// <summary>
        /// Criteria schema version for this expectation.
        /// </summary>
        public long? ExpectationsVersion { get; internal set; }

        /// <summary>
        /// How this expectation opens windows.
        /// </summary>
        public string Trigger { get; internal set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the expectation.
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
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the Expectation's schedule fields.
        /// </summary>
        public long? ScheduleId { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the 0-based weekdays used by the schedule.
        /// </summary>
        public long?[] ScheduleDaysOfWeek { get; internal set; }

        /// <summary>
        /// Times of day in HH:MM format for the Expectation schedule.
        /// </summary>
        public string[] ScheduleTimesOfDay { get; internal set; }

        /// <summary>
        /// Time zone used by the Expectation schedule.
        /// </summary>
        public string ScheduleTimeZone { get; internal set; }

        /// <summary>
        /// Optional holiday region used by the Expectation schedule.
        /// </summary>
        public string HolidayRegion { get; internal set; }

        /// <summary>
        /// How many seconds before the due boundary the window starts.
        /// </summary>
        public long? LookbackInterval { get; internal set; }

        /// <summary>
        /// Calendar-based replacement due times. Matching dates replace all normal times; holiday exclusions still apply. Calendars specifying different times on the same date conflict.
        /// </summary>
        public object[] ScheduleTimeExceptions { get; internal set; }

        /// <summary>
        /// How many seconds a schedule-driven window may remain eligible to close as late.
        /// </summary>
        public long? LateAcceptanceInterval { get; internal set; }

        /// <summary>
        /// How many quiet seconds are required before final closure.
        /// </summary>
        public long? InactivityInterval { get; internal set; }

        /// <summary>
        /// Hard-stop duration in seconds for unscheduled expectations.
        /// </summary>
        public long? MaxOpenInterval { get; internal set; }

        /// <summary>
        /// Versioned success criteria definition for the expectation. Criteria v2 supports optional FTS content validation.
        /// </summary>
        public object Criteria { get; internal set; }

        /// <summary>
        /// Last time this expectation was evaluated.
        /// </summary>
        public DateTime? LastEvaluatedAt { get; internal set; }

        /// <summary>
        /// Last time this expectation closed successfully.
        /// </summary>
        public DateTime? LastSuccessAt { get; internal set; }

        /// <summary>
        /// Last time this expectation closed with a failure result.
        /// </summary>
        public DateTime? LastFailureAt { get; internal set; }

        /// <summary>
        /// Most recent terminal result for this expectation.
        /// </summary>
        public string LastResult { get; internal set; }

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
        internal void CopyFrom(Expectation source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Name = source.Name;
            Description = source.Description;
            Path = source.Path;
            Source = source.Source;
            ExcludePattern = source.ExcludePattern;
            Disabled = source.Disabled;
            ExpectationsVersion = source.ExpectationsVersion;
            Trigger = source.Trigger;
            Interval = source.Interval;
            RecurringDay = source.RecurringDay;
            RecurringDays = source.RecurringDays;
            ScheduleId = source.ScheduleId;
            ScheduleDaysOfWeek = source.ScheduleDaysOfWeek;
            ScheduleTimesOfDay = source.ScheduleTimesOfDay;
            ScheduleTimeZone = source.ScheduleTimeZone;
            HolidayRegion = source.HolidayRegion;
            LookbackInterval = source.LookbackInterval;
            ScheduleTimeExceptions = source.ScheduleTimeExceptions;
            LateAcceptanceInterval = source.LateAcceptanceInterval;
            InactivityInterval = source.InactivityInterval;
            MaxOpenInterval = source.MaxOpenInterval;
            Criteria = source.Criteria;
            LastEvaluatedAt = source.LastEvaluatedAt;
            LastSuccessAt = source.LastSuccessAt;
            LastFailureAt = source.LastFailureAt;
            LastResult = source.LastResult;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
