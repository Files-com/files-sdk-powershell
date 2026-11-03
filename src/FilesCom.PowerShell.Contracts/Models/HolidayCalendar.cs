using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Holiday Calendar defines site-wide holiday dates and optional partial-day windows that scheduled resources skip.
    /// </summary>
    public sealed class HolidayCalendar : FilesObject
    {
        internal HolidayCalendar()
        {
        }

        /// <summary>
        /// Holiday Calendar ID. Set a scheduled resource's `holiday_region` to `custom_` followed by this ID to make it skip the days in this calendar.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Holiday Calendar name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Holiday rules for the calendar.
        /// </summary>
        public object Definition { get; internal set; }

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
        internal void CopyFrom(HolidayCalendar source)
        {
            Id = source.Id;
            Name = source.Name;
            Definition = source.Definition;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
