using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Schedules, or get one schedule by Id.
    /// </summary>
    [Cmdlet("Get", "FilesSchedule", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Schedule), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Schedule>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Schedule), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesScheduleCommand : FilesListCmdlet<Models.Schedule>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// Schedule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Schedules, or get one schedule by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Schedule result = Run(connection, token => ScheduleBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => ScheduleBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Schedule.
    /// </summary>
    [Cmdlet("New", "FilesSchedule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Schedule))]
    public sealed class NewFilesScheduleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Schedule name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// 0-based weekdays used by the Schedule. 0 is Sunday.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ScheduleDaysOfWeek { get; set; }

        /// <summary>
        /// Times of day in HH:MM format (24-hour).
        /// </summary>
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Time zone for scheduled times. If not set, times are interpreted as UTC.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Optional holiday region on which linked resources do not run.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>Create Schedule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Schedule '{Name}'" : "Schedule"))
            {
                return;
            }
            Models.Schedule result = Run(connection, token => ScheduleBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
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
    /// Update Schedule.
    /// </summary>
    [Cmdlet("Set", "FilesSchedule", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Schedule), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesScheduleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Schedule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Schedule name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// 0-based weekdays used by the Schedule. 0 is Sunday.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ScheduleDaysOfWeek { get; set; }

        /// <summary>
        /// Times of day in HH:MM format (24-hour).
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Time zone for scheduled times. If not set, times are interpreted as UTC.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Optional holiday region on which linked resources do not run.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// The schedule to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Schedule InputObject { get; set; }

        /// <summary>Update Schedule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Schedule {id}"))
            {
                return;
            }
            Models.Schedule result = Run(connection, token => ScheduleBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
    /// Delete Schedule.
    /// </summary>
    [Cmdlet("Remove", "FilesSchedule", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesScheduleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Schedule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The schedule to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Schedule InputObject { get; set; }

        /// <summary>Delete Schedule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Schedule {id}"))
            {
                return;
            }
            Run(connection, token => ScheduleBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
