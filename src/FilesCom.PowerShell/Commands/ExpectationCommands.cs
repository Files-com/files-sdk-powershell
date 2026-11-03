using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Expectations, or get one expectation by Id.
    /// </summary>
    [Cmdlet("Get", "FilesExpectation", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Expectation), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Expectation>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Expectation), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesExpectationCommand : FilesListCmdlet<Models.Expectation>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `name` or `disabled`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `disabled` and `workspace_id`. Valid field combinations are `[ workspace_id, disabled ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Expectation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Expectations, or get one expectation by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Expectation result = Run(connection, token => ExpectationBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => ExpectationBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Expectation.
    /// </summary>
    [Cmdlet("New", "FilesExpectation", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Expectation))]
    public sealed class NewFilesExpectationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Expectation name.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Expectation description.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Path scope for the expectation. Supports workspace-relative presentation.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Source glob used to select candidate files.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Source { get; set; }

        /// <summary>
        /// Optional source exclusion glob.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ExcludePattern { get; set; }

        /// <summary>
        /// If true, the expectation is disabled.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// How this expectation opens windows.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the expectation.
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
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the Expectation's schedule fields.
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
        /// Times of day in HH:MM format for the Expectation schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Time zone used by the Expectation schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Optional holiday region used by the Expectation schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// How many seconds before the due boundary the window starts.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? LookbackInterval { get; set; }

        /// <summary>
        /// How many seconds a schedule-driven window may remain eligible to close as late.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? LateAcceptanceInterval { get; set; }

        /// <summary>
        /// How many quiet seconds are required before final closure.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? InactivityInterval { get; set; }

        /// <summary>
        /// Hard-stop duration in seconds for unscheduled expectations.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? MaxOpenInterval { get; set; }

        /// <summary>
        /// Versioned success criteria definition for the expectation, including optional Files Transform Script content validation in criteria v2.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Criteria { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Expectation.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Expectation '{Path}'" : "Expectation"))
            {
                return;
            }
            Models.Expectation result = Run(connection, token => ExpectationBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(Source)))
            {
                parameters["source"] = Source;
            }
            if (IsBound(nameof(ExcludePattern)))
            {
                parameters["exclude_pattern"] = ExcludePattern;
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
            if (IsBound(nameof(LookbackInterval)))
            {
                parameters["lookback_interval"] = LookbackInterval.Value;
            }
            if (IsBound(nameof(LateAcceptanceInterval)))
            {
                parameters["late_acceptance_interval"] = LateAcceptanceInterval.Value;
            }
            if (IsBound(nameof(InactivityInterval)))
            {
                parameters["inactivity_interval"] = InactivityInterval.Value;
            }
            if (IsBound(nameof(MaxOpenInterval)))
            {
                parameters["max_open_interval"] = MaxOpenInterval.Value;
            }
            if (IsBound(nameof(Criteria)))
            {
                parameters["criteria"] = RequestValues.Object(Criteria, "Criteria");
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Manually open an Expectation window.
    /// </summary>
    [Cmdlet("Start", "FilesExpectationEvaluation", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.ExpectationEvaluation), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class StartFilesExpectationEvaluationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Expectation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The expectation to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Expectation InputObject { get; set; }

        /// <summary>Manually open an Expectation window.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = TriggerEvaluationParameters();
            if (!ShouldProcessOn(connection, $"Expectation {id}"))
            {
                return;
            }
            Models.ExpectationEvaluation result = Run(connection, token => ExpectationBridge.TriggerEvaluationAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> TriggerEvaluationParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update Expectation.
    /// </summary>
    [Cmdlet("Set", "FilesExpectation", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Expectation), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesExpectationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Expectation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Expectation name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Expectation description.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Path scope for the expectation. Supports workspace-relative presentation.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Source glob used to select candidate files.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Source { get; set; }

        /// <summary>
        /// Optional source exclusion glob.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ExcludePattern { get; set; }

        /// <summary>
        /// If true, the expectation is disabled.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// How this expectation opens windows.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the expectation.
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
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the Expectation's schedule fields.
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
        /// Times of day in HH:MM format for the Expectation schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Time zone used by the Expectation schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Optional holiday region used by the Expectation schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// How many seconds before the due boundary the window starts.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? LookbackInterval { get; set; }

        /// <summary>
        /// How many seconds a schedule-driven window may remain eligible to close as late.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? LateAcceptanceInterval { get; set; }

        /// <summary>
        /// How many quiet seconds are required before final closure.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? InactivityInterval { get; set; }

        /// <summary>
        /// Hard-stop duration in seconds for unscheduled expectations.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? MaxOpenInterval { get; set; }

        /// <summary>
        /// Versioned success criteria definition for the expectation, including optional Files Transform Script content validation in criteria v2.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Criteria { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// The expectation to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Expectation InputObject { get; set; }

        /// <summary>Update Expectation.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Expectation {id}"))
            {
                return;
            }
            Models.Expectation result = Run(connection, token => ExpectationBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(Source)))
            {
                parameters["source"] = Source;
            }
            if (IsBound(nameof(ExcludePattern)))
            {
                parameters["exclude_pattern"] = ExcludePattern;
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
            if (IsBound(nameof(LookbackInterval)))
            {
                parameters["lookback_interval"] = LookbackInterval.Value;
            }
            if (IsBound(nameof(LateAcceptanceInterval)))
            {
                parameters["late_acceptance_interval"] = LateAcceptanceInterval.Value;
            }
            if (IsBound(nameof(InactivityInterval)))
            {
                parameters["inactivity_interval"] = InactivityInterval.Value;
            }
            if (IsBound(nameof(MaxOpenInterval)))
            {
                parameters["max_open_interval"] = MaxOpenInterval.Value;
            }
            if (IsBound(nameof(Criteria)))
            {
                parameters["criteria"] = RequestValues.Object(Criteria, "Criteria");
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Expectation.
    /// </summary>
    [Cmdlet("Remove", "FilesExpectation", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesExpectationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Expectation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The expectation to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Expectation InputObject { get; set; }

        /// <summary>Delete Expectation.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Expectation {id}"))
            {
                return;
            }
            Run(connection, token => ExpectationBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
