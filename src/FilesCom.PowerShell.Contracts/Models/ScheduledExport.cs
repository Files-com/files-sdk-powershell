using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Scheduled Export defines a recurring schedule for generating one of the built-in CSV exports and e-mailing it to a Site Admin recipient.
    /// </summary>
    public sealed class ScheduledExport : FilesObject
    {
        internal ScheduledExport()
        {
        }

        /// <summary>
        /// Scheduled Export ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Name for this scheduled export.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Export report type. Valid values: folder_size_audit, group_membership_audit, permission_audit, share_link_audit
        /// </summary>
        public string ExportType { get; internal set; }

        /// <summary>
        /// Human-readable report name.
        /// </summary>
        public string ReportName { get; internal set; }

        /// <summary>
        /// Options for the report selected by export_type.
        /// </summary>
        public object ExportOptions { get; internal set; }

        /// <summary>
        /// Site Admin user who receives the completed export e-mail.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// If true, this scheduled export will not run.
        /// </summary>
        public bool Disabled { get; internal set; }

        /// <summary>
        /// Schedule trigger type: `daily` or `custom_schedule`.
        /// </summary>
        public string Trigger { get; internal set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the scheduled export.
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
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the scheduled export's schedule fields.
        /// </summary>
        public long? ScheduleId { get; internal set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the 0-based weekdays used by the schedule.
        /// </summary>
        public long?[] ScheduleDaysOfWeek { get; internal set; }

        /// <summary>
        /// Times of day in HH:MM format for the scheduled export schedule.
        /// </summary>
        public string[] ScheduleTimesOfDay { get; internal set; }

        /// <summary>
        /// Time zone used by the scheduled export schedule.
        /// </summary>
        public string ScheduleTimeZone { get; internal set; }

        /// <summary>
        /// Optional holiday region used by the scheduled export schedule.
        /// </summary>
        public string HolidayRegion { get; internal set; }

        /// <summary>
        /// Human-readable schedule description.
        /// </summary>
        public string HumanReadableSchedule { get; internal set; }

        /// <summary>
        /// Most recent scheduled attempt time, including attempts that failed validation.
        /// </summary>
        public DateTime? LastRunAt { get; internal set; }

        /// <summary>
        /// Most recent Export ID created by this schedule.
        /// </summary>
        public long? LastExportId { get; internal set; }

        /// <summary>
        /// Validation error from the most recent scheduled attempt. The schedule remains enabled and retries at its next scheduled time. Cleared when an export is successfully created; does not describe errors during export generation.
        /// </summary>
        public string LastError { get; internal set; }

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
        internal void CopyFrom(ScheduledExport source)
        {
            Id = source.Id;
            Name = source.Name;
            ExportType = source.ExportType;
            ReportName = source.ReportName;
            ExportOptions = source.ExportOptions;
            UserId = source.UserId;
            Disabled = source.Disabled;
            Trigger = source.Trigger;
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
            LastExportId = source.LastExportId;
            LastError = source.LastError;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
