using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Schedule is a named, reusable weekday-and-time schedule shared by scheduled resources across a Site.
    /// </summary>
    public sealed class Schedule : FilesObject
    {
        internal Schedule()
        {
        }

        /// <summary>
        /// Schedule ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Schedule name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 0-based weekdays used by the Schedule. 0 is Sunday.
        /// </summary>
        public long?[] ScheduleDaysOfWeek { get; internal set; }

        /// <summary>
        /// Times of day in HH:MM format (24-hour).
        /// </summary>
        public string[] ScheduleTimesOfDay { get; internal set; }

        /// <summary>
        /// Time zone for scheduled times. If not set, times are interpreted as UTC.
        /// </summary>
        public string ScheduleTimeZone { get; internal set; }

        /// <summary>
        /// Optional holiday region on which linked resources do not run.
        /// </summary>
        public string HolidayRegion { get; internal set; }

        /// <summary>
        /// Human-readable Schedule description.
        /// </summary>
        public string HumanReadableSchedule { get; internal set; }

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
        internal void CopyFrom(Schedule source)
        {
            Id = source.Id;
            Name = source.Name;
            ScheduleDaysOfWeek = source.ScheduleDaysOfWeek;
            ScheduleTimesOfDay = source.ScheduleTimesOfDay;
            ScheduleTimeZone = source.ScheduleTimeZone;
            HolidayRegion = source.HolidayRegion;
            HumanReadableSchedule = source.HumanReadableSchedule;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
