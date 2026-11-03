using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Ai Tasks, or get one ai task by Id.
    /// </summary>
    [Cmdlet("Get", "FilesAiTask", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.AiTask), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.AiTask>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.AiTask), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesAiTaskCommand : FilesListCmdlet<Models.AiTask>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `name`, `disabled` or `updated_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `disabled`, `trigger` or `workspace_id`. Valid field combinations are `[ workspace_id, disabled ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Ai Task ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Ai Tasks, or get one ai task by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.AiTask result = Run(connection, token => AiTaskBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => AiTaskBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create Ai Task.
    /// </summary>
    [Cmdlet("New", "FilesAiTask", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.AiTask))]
    public sealed class NewFilesAiTaskCommand : FilesApiCmdlet
    {
        /// <summary>
        /// AI Task description.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// If true, this AI Task will not run.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// Optional holiday region used by the AI Task schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the AI Task.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Interval { get; set; }

        /// <summary>
        /// AI Task name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Path scope used for action-triggered AI Tasks.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Permissions used by the internal API key for this AI Task. Valid values are `full` and `files_only`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PermissionSet { get; set; }

        /// <summary>
        /// Prompt sent when this AI Task is invoked.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Prompt { get; set; }

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
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the AI Task's schedule fields.
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
        /// Time zone used by the AI Task schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Times of day in HH:MM format for the AI Task schedule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Source glob used with `path` for action-triggered AI Tasks.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Source { get; set; }

        /// <summary>
        /// How this AI Task is triggered.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// If trigger is `action`, the file action types that invoke this AI Task. Valid actions are create, copy, move, archived_delete, update, read, destroy.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] TriggerActions { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Ai Task.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Ai task '{Path}'" : "Ai task"))
            {
                return;
            }
            Models.AiTask result = Run(connection, token => AiTaskBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(HolidayRegion)))
            {
                parameters["holiday_region"] = HolidayRegion;
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(PermissionSet)))
            {
                parameters["permission_set"] = PermissionSet;
            }
            if (IsBound(nameof(Prompt)))
            {
                parameters["prompt"] = Prompt;
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
            if (IsBound(nameof(ScheduleTimeZone)))
            {
                parameters["schedule_time_zone"] = ScheduleTimeZone;
            }
            if (IsBound(nameof(ScheduleTimesOfDay)))
            {
                parameters["schedule_times_of_day"] = RequestValues.Strings(ScheduleTimesOfDay, "ScheduleTimesOfDay");
            }
            if (IsBound(nameof(Source)))
            {
                parameters["source"] = Source;
            }
            if (IsBound(nameof(Trigger)))
            {
                parameters["trigger"] = Trigger;
            }
            if (IsBound(nameof(TriggerActions)))
            {
                parameters["trigger_actions"] = RequestValues.Strings(TriggerActions, "TriggerActions");
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Manually Run AI Task.
    /// </summary>
    [Cmdlet("Start", "FilesAiTask", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class StartFilesAiTaskCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Ai Task ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The ai task to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.AiTask InputObject { get; set; }

        /// <summary>Manually Run AI Task.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ManualRunParameters();
            if (!ShouldProcessOn(connection, $"Ai task {id}"))
            {
                return;
            }
            Run(connection, token => AiTaskBridge.ManualRunAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> ManualRunParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update Ai Task.
    /// </summary>
    [Cmdlet("Set", "FilesAiTask", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.AiTask), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesAiTaskCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Ai Task ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// AI Task description.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// If true, this AI Task will not run.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// Optional holiday region used by the AI Task schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run the AI Task.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Interval { get; set; }

        /// <summary>
        /// AI Task name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Path scope used for action-triggered AI Tasks.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Permissions used by the internal API key for this AI Task. Valid values are `full` and `files_only`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PermissionSet { get; set; }

        /// <summary>
        /// Prompt sent when this AI Task is invoked.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Prompt { get; set; }

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
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the AI Task's schedule fields.
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
        /// Time zone used by the AI Task schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Times of day in HH:MM format for the AI Task schedule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Source glob used with `path` for action-triggered AI Tasks.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Source { get; set; }

        /// <summary>
        /// How this AI Task is triggered.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// If trigger is `action`, the file action types that invoke this AI Task. Valid actions are create, copy, move, archived_delete, update, read, destroy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] TriggerActions { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// The ai task to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.AiTask InputObject { get; set; }

        /// <summary>Update Ai Task.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Ai task {id}"))
            {
                return;
            }
            Models.AiTask result = Run(connection, token => AiTaskBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(HolidayRegion)))
            {
                parameters["holiday_region"] = HolidayRegion;
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(PermissionSet)))
            {
                parameters["permission_set"] = PermissionSet;
            }
            if (IsBound(nameof(Prompt)))
            {
                parameters["prompt"] = Prompt;
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
            if (IsBound(nameof(ScheduleTimeZone)))
            {
                parameters["schedule_time_zone"] = ScheduleTimeZone;
            }
            if (IsBound(nameof(ScheduleTimesOfDay)))
            {
                parameters["schedule_times_of_day"] = RequestValues.Strings(ScheduleTimesOfDay, "ScheduleTimesOfDay");
            }
            if (IsBound(nameof(Source)))
            {
                parameters["source"] = Source;
            }
            if (IsBound(nameof(Trigger)))
            {
                parameters["trigger"] = Trigger;
            }
            if (IsBound(nameof(TriggerActions)))
            {
                parameters["trigger_actions"] = RequestValues.Strings(TriggerActions, "TriggerActions");
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Ai Task.
    /// </summary>
    [Cmdlet("Remove", "FilesAiTask", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesAiTaskCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Ai Task ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The ai task to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.AiTask InputObject { get; set; }

        /// <summary>Delete Ai Task.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Ai task {id}"))
            {
                return;
            }
            Run(connection, token => AiTaskBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
