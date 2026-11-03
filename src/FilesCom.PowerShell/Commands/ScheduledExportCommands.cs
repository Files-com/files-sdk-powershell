using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Scheduled Exports, or get one scheduled export by Id.
    /// </summary>
    [Cmdlet("Get", "FilesScheduledExport", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.ScheduledExport), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.ScheduledExport>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.ScheduledExport), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesScheduledExportCommand : FilesListCmdlet<Models.ScheduledExport>
    {
        /// <summary>
        /// Used for pagination.  When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`.  Send one of those cursor value here to resume an existing list from the next available record.  Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Cursor { get; set; }

        /// <summary>
        /// Number of records to show per page.  (Max: 10000, 1,000 or less is recommended).
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PerPage { get; set; }

        /// <summary>
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `name`, `export_type` or `disabled`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `disabled` and `export_type`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `export_type`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// Scheduled Export ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Scheduled Exports, or get one scheduled export by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.ScheduledExport result = Run(connection, token => ScheduledExportBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => ScheduledExportBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Cursor)))
            {
                parameters["cursor"] = Cursor;
            }
            if (IsBound(nameof(PerPage)))
            {
                parameters["per_page"] = PerPage.Value;
            }
            if (IsBound(nameof(SortBy)))
            {
                parameters["sort_by"] = RequestValues.QueryObject(SortBy, "SortBy");
            }
            if (IsBound(nameof(Filter)))
            {
                parameters["filter"] = RequestValues.QueryObject(Filter, "Filter");
            }
            if (IsBound(nameof(FilterPrefix)))
            {
                parameters["filter_prefix"] = RequestValues.QueryObject(FilterPrefix, "FilterPrefix");
            }
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Scheduled Export.
    /// </summary>
    [Cmdlet("New", "FilesScheduledExport", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.ScheduledExport))]
    public sealed class NewFilesScheduledExportCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Name for this scheduled export.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Export report type. Valid values: folder_size_audit, group_membership_audit, permission_audit, share_link_audit
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string ExportType { get; set; }

        /// <summary>
        /// Options for the report selected by export_type.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary ExportOptions { get; set; }

        /// <summary>
        /// Site Admin user who receives the completed export e-mail.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// If true, this scheduled export will not run.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// Schedule trigger type: `daily` or `custom_schedule`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the scheduled export.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Interval { get; set; }

        /// <summary>
        /// If trigger is `daily`, this selects the day number inside the chosen interval.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? RecurringDay { get; set; }

        /// <summary>
        /// If trigger is `daily`, this selects one or more day numbers inside a `week`, `month`, `quarter`, or `year` interval.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] RecurringDays { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the scheduled export's schedule fields.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ScheduleId { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the 0-based weekdays used by the schedule.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ScheduleDaysOfWeek { get; set; }

        /// <summary>
        /// Times of day in HH:MM format for the scheduled export schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Time zone used by the scheduled export schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Optional holiday region used by the scheduled export schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>Create Scheduled Export.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Scheduled export '{Name}'" : "Scheduled export"))
            {
                return;
            }
            Models.ScheduledExport result = Run(connection, token => ScheduledExportBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(ExportType)))
            {
                parameters["export_type"] = ExportType;
            }
            if (IsBound(nameof(ExportOptions)))
            {
                parameters["export_options"] = RequestValues.Object(ExportOptions, "ExportOptions");
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(Trigger)))
            {
                parameters["trigger"] = Trigger;
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval;
            }
            if (IsBound(nameof(RecurringDay)))
            {
                parameters["recurring_day"] = RecurringDay.Value;
            }
            if (IsBound(nameof(RecurringDays)))
            {
                parameters["recurring_days"] = RequestValues.Int64s(RecurringDays);
            }
            if (IsBound(nameof(ScheduleId)))
            {
                parameters["schedule_id"] = ScheduleId.Value;
            }
            if (IsBound(nameof(ScheduleDaysOfWeek)))
            {
                parameters["schedule_days_of_week"] = RequestValues.Int64s(ScheduleDaysOfWeek);
            }
            if (IsBound(nameof(ScheduleTimesOfDay)))
            {
                parameters["schedule_times_of_day"] = RequestValues.Strings(ScheduleTimesOfDay, "ScheduleTimesOfDay");
            }
            if (IsBound(nameof(ScheduleTimeZone)))
            {
                parameters["schedule_time_zone"] = ScheduleTimeZone;
            }
            if (IsBound(nameof(HolidayRegion)))
            {
                parameters["holiday_region"] = HolidayRegion;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Scheduled Export.
    /// </summary>
    [Cmdlet("Set", "FilesScheduledExport", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.ScheduledExport), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesScheduledExportCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Scheduled Export ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Name for this scheduled export.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Export report type. Valid values: folder_size_audit, group_membership_audit, permission_audit, share_link_audit
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ExportType { get; set; }

        /// <summary>
        /// Options for the report selected by export_type.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary ExportOptions { get; set; }

        /// <summary>
        /// Site Admin user who receives the completed export e-mail.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// If true, this scheduled export will not run.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// Schedule trigger type: `daily` or `custom_schedule`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the scheduled export.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Interval { get; set; }

        /// <summary>
        /// If trigger is `daily`, this selects the day number inside the chosen interval.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? RecurringDay { get; set; }

        /// <summary>
        /// If trigger is `daily`, this selects one or more day numbers inside a `week`, `month`, `quarter`, or `year` interval.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] RecurringDays { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the scheduled export's schedule fields.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ScheduleId { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the 0-based weekdays used by the schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ScheduleDaysOfWeek { get; set; }

        /// <summary>
        /// Times of day in HH:MM format for the scheduled export schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Time zone used by the scheduled export schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Optional holiday region used by the scheduled export schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// The scheduled export to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ScheduledExport InputObject { get; set; }

        /// <summary>Update Scheduled Export.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Scheduled export {id}"))
            {
                return;
            }
            Models.ScheduledExport result = Run(connection, token => ScheduledExportBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(ExportType)))
            {
                parameters["export_type"] = ExportType;
            }
            if (IsBound(nameof(ExportOptions)))
            {
                parameters["export_options"] = RequestValues.Object(ExportOptions, "ExportOptions");
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(Trigger)))
            {
                parameters["trigger"] = Trigger;
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval;
            }
            if (IsBound(nameof(RecurringDay)))
            {
                parameters["recurring_day"] = RecurringDay.Value;
            }
            if (IsBound(nameof(RecurringDays)))
            {
                parameters["recurring_days"] = RequestValues.Int64s(RecurringDays);
            }
            if (IsBound(nameof(ScheduleId)))
            {
                parameters["schedule_id"] = ScheduleId.Value;
            }
            if (IsBound(nameof(ScheduleDaysOfWeek)))
            {
                parameters["schedule_days_of_week"] = RequestValues.Int64s(ScheduleDaysOfWeek);
            }
            if (IsBound(nameof(ScheduleTimesOfDay)))
            {
                parameters["schedule_times_of_day"] = RequestValues.Strings(ScheduleTimesOfDay, "ScheduleTimesOfDay");
            }
            if (IsBound(nameof(ScheduleTimeZone)))
            {
                parameters["schedule_time_zone"] = ScheduleTimeZone;
            }
            if (IsBound(nameof(HolidayRegion)))
            {
                parameters["holiday_region"] = HolidayRegion;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Scheduled Export.
    /// </summary>
    [Cmdlet("Remove", "FilesScheduledExport", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesScheduledExportCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Scheduled Export ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The scheduled export to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ScheduledExport InputObject { get; set; }

        /// <summary>Delete Scheduled Export.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Scheduled export {id}"))
            {
                return;
            }
            Run(connection, token => ScheduledExportBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
