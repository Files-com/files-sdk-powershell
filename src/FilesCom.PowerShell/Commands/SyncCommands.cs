using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Syncs, or get one sync by Id.
    /// </summary>
    [Cmdlet("Get", "FilesSync", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Sync), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Sync>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Sync), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesSyncCommand : FilesListCmdlet<Models.Sync>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `site_id`, `workspace_id` or `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id`, `disabled`, `src_remote_server_id` or `dest_remote_server_id`. Valid field combinations are `[ workspace_id, disabled ]`, `[ workspace_id, src_remote_server_id ]`, `[ workspace_id, dest_remote_server_id ]`, `[ disabled, src_remote_server_id ]`, `[ disabled, dest_remote_server_id ]`, `[ workspace_id, disabled, src_remote_server_id ]` or `[ workspace_id, disabled, dest_remote_server_id ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Sync ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Syncs, or get one sync by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Sync result = Run(connection, token => SyncBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => SyncBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create Sync.
    /// </summary>
    [Cmdlet("New", "FilesSync", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Sync))]
    public sealed class NewFilesSyncCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Delete empty folders after sync?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DeleteEmptyFolders { get; set; }

        /// <summary>
        /// Description for this sync job
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Absolute destination path for the sync
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DestPath { get; set; }

        /// <summary>
        /// Remote server ID for the destination (if remote)
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? DestRemoteServerId { get; set; }

        /// <summary>
        /// Is this sync disabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// Array of glob patterns to exclude
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] ExcludePatterns { get; set; }

        /// <summary>
        /// Skip the sync if there is a formal, observed holiday for this region.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// Array of glob patterns to include
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] IncludePatterns { get; set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run this sync.  One of: `day`, `week`, `week_end`, `month`, `month_end`, `quarter`, `quarter_end`, `year`, `year_end`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Interval { get; set; }

        /// <summary>
        /// Keep files after copying?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? KeepAfterCopy { get; set; }

        /// <summary>
        /// Name for this sync job
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// If trigger type is `daily`, this specifies a day number to run in one of the supported intervals: `week`, `month`, `quarter`, `year`.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? RecurringDay { get; set; }

        /// <summary>
        /// If trigger type is `daily`, this specifies one or more day numbers to run in one of the supported intervals: `week`, `month`, `quarter`, `year`.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] RecurringDays { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the sync's schedule fields.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ScheduleId { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, Custom schedule description for when the sync should be run. 0-based days of the week. 0 is Sunday, 1 is Monday, etc.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ScheduleDaysOfWeek { get; set; }

        /// <summary>
        /// Time zone for the schedule. If not set, times are interpreted as UTC.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Times of day to run in HH:MM format. For `custom_schedule`, run at these times on specified days of week. For `daily`, run at these times on the scheduled interval date.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Absolute source path for the sync
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SrcPath { get; set; }

        /// <summary>
        /// Remote server ID for the source (if remote)
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SrcRemoteServerId { get; set; }

        /// <summary>
        /// Frequency in minutes between syncs. If set, this value must be greater than or equal to the `remote_sync_interval` value for the site's plan. If left blank, the plan's `remote_sync_interval` will be used. This setting is only used if `trigger` is empty.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SyncIntervalMinutes { get; set; }

        /// <summary>
        /// Trigger type: daily, custom_schedule, or manual
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// Some MFT services request an empty file (known as a trigger file) to signal the sync is complete and they can begin further processing. If trigger_file is set, a zero-byte file will be sent at the end of the sync.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string TriggerFile { get; set; }

        /// <summary>
        /// If true, the trigger file will be sent at the end of a successful sync even when no files were transferred.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AlwaysWriteTriggerFile { get; set; }

        /// <summary>
        /// Workspace ID this sync belongs to
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Sync.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Sync '{Name}'" : "Sync"))
            {
                return;
            }
            Models.Sync result = Run(connection, token => SyncBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(DeleteEmptyFolders)))
            {
                parameters["delete_empty_folders"] = DeleteEmptyFolders.Value;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(DestPath)))
            {
                parameters["dest_path"] = DestPath;
            }
            if (IsBound(nameof(DestRemoteServerId)))
            {
                parameters["dest_remote_server_id"] = DestRemoteServerId.Value;
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(ExcludePatterns)))
            {
                parameters["exclude_patterns"] = RequestValues.Strings(ExcludePatterns, "ExcludePatterns");
            }
            if (IsBound(nameof(HolidayRegion)))
            {
                parameters["holiday_region"] = HolidayRegion;
            }
            if (IsBound(nameof(IncludePatterns)))
            {
                parameters["include_patterns"] = RequestValues.Strings(IncludePatterns, "IncludePatterns");
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval;
            }
            if (IsBound(nameof(KeepAfterCopy)))
            {
                parameters["keep_after_copy"] = KeepAfterCopy.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
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
            if (IsBound(nameof(SrcPath)))
            {
                parameters["src_path"] = SrcPath;
            }
            if (IsBound(nameof(SrcRemoteServerId)))
            {
                parameters["src_remote_server_id"] = SrcRemoteServerId.Value;
            }
            if (IsBound(nameof(SyncIntervalMinutes)))
            {
                parameters["sync_interval_minutes"] = SyncIntervalMinutes.Value;
            }
            if (IsBound(nameof(Trigger)))
            {
                parameters["trigger"] = Trigger;
            }
            if (IsBound(nameof(TriggerFile)))
            {
                parameters["trigger_file"] = TriggerFile;
            }
            if (IsBound(nameof(AlwaysWriteTriggerFile)))
            {
                parameters["always_write_trigger_file"] = AlwaysWriteTriggerFile.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Dry Run Sync.
    /// </summary>
    [Cmdlet("Test", "FilesSync", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class TestFilesSyncCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Sync ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The sync to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Sync InputObject { get; set; }

        /// <summary>Dry Run Sync.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DryRunParameters();
            if (!ShouldProcessOn(connection, $"Sync {id}"))
            {
                return;
            }
            Run(connection, token => SyncBridge.DryRunAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DryRunParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Manually Run Sync.
    /// </summary>
    [Cmdlet("Start", "FilesSync", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class StartFilesSyncCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Sync ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The sync to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Sync InputObject { get; set; }

        /// <summary>Manually Run Sync.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ManualRunParameters();
            if (!ShouldProcessOn(connection, $"Sync {id}"))
            {
                return;
            }
            Run(connection, token => SyncBridge.ManualRunAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> ManualRunParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update Sync.
    /// </summary>
    [Cmdlet("Set", "FilesSync", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Sync), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesSyncCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Sync ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Delete empty folders after sync?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DeleteEmptyFolders { get; set; }

        /// <summary>
        /// Description for this sync job
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Absolute destination path for the sync
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string DestPath { get; set; }

        /// <summary>
        /// Remote server ID for the destination (if remote)
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? DestRemoteServerId { get; set; }

        /// <summary>
        /// Is this sync disabled?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// Array of glob patterns to exclude
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] ExcludePatterns { get; set; }

        /// <summary>
        /// Skip the sync if there is a formal, observed holiday for this region.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// Array of glob patterns to include
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] IncludePatterns { get; set; }

        /// <summary>
        /// If trigger is `daily`, this specifies how often to run this sync.  One of: `day`, `week`, `week_end`, `month`, `month_end`, `quarter`, `quarter_end`, `year`, `year_end`
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Interval { get; set; }

        /// <summary>
        /// Keep files after copying?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? KeepAfterCopy { get; set; }

        /// <summary>
        /// Name for this sync job
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// If trigger type is `daily`, this specifies a day number to run in one of the supported intervals: `week`, `month`, `quarter`, `year`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? RecurringDay { get; set; }

        /// <summary>
        /// If trigger type is `daily`, this specifies one or more day numbers to run in one of the supported intervals: `week`, `month`, `quarter`, `year`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] RecurringDays { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the sync's schedule fields.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ScheduleId { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, Custom schedule description for when the sync should be run. 0-based days of the week. 0 is Sunday, 1 is Monday, etc.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ScheduleDaysOfWeek { get; set; }

        /// <summary>
        /// Time zone for the schedule. If not set, times are interpreted as UTC.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Times of day to run in HH:MM format. For `custom_schedule`, run at these times on specified days of week. For `daily`, run at these times on the scheduled interval date.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Absolute source path for the sync
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SrcPath { get; set; }

        /// <summary>
        /// Remote server ID for the source (if remote)
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? SrcRemoteServerId { get; set; }

        /// <summary>
        /// Frequency in minutes between syncs. If set, this value must be greater than or equal to the `remote_sync_interval` value for the site's plan. If left blank, the plan's `remote_sync_interval` will be used. This setting is only used if `trigger` is empty.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? SyncIntervalMinutes { get; set; }

        /// <summary>
        /// Trigger type: daily, custom_schedule, or manual
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// Some MFT services request an empty file (known as a trigger file) to signal the sync is complete and they can begin further processing. If trigger_file is set, a zero-byte file will be sent at the end of the sync.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string TriggerFile { get; set; }

        /// <summary>
        /// If true, the trigger file will be sent at the end of a successful sync even when no files were transferred.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AlwaysWriteTriggerFile { get; set; }

        /// <summary>
        /// The sync to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Sync InputObject { get; set; }

        /// <summary>Update Sync.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Sync {id}"))
            {
                return;
            }
            Models.Sync result = Run(connection, token => SyncBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(DeleteEmptyFolders)))
            {
                parameters["delete_empty_folders"] = DeleteEmptyFolders.Value;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(DestPath)))
            {
                parameters["dest_path"] = DestPath;
            }
            if (IsBound(nameof(DestRemoteServerId)))
            {
                parameters["dest_remote_server_id"] = DestRemoteServerId.Value;
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(ExcludePatterns)))
            {
                parameters["exclude_patterns"] = RequestValues.Strings(ExcludePatterns, "ExcludePatterns");
            }
            if (IsBound(nameof(HolidayRegion)))
            {
                parameters["holiday_region"] = HolidayRegion;
            }
            if (IsBound(nameof(IncludePatterns)))
            {
                parameters["include_patterns"] = RequestValues.Strings(IncludePatterns, "IncludePatterns");
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval;
            }
            if (IsBound(nameof(KeepAfterCopy)))
            {
                parameters["keep_after_copy"] = KeepAfterCopy.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
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
            if (IsBound(nameof(SrcPath)))
            {
                parameters["src_path"] = SrcPath;
            }
            if (IsBound(nameof(SrcRemoteServerId)))
            {
                parameters["src_remote_server_id"] = SrcRemoteServerId.Value;
            }
            if (IsBound(nameof(SyncIntervalMinutes)))
            {
                parameters["sync_interval_minutes"] = SyncIntervalMinutes.Value;
            }
            if (IsBound(nameof(Trigger)))
            {
                parameters["trigger"] = Trigger;
            }
            if (IsBound(nameof(TriggerFile)))
            {
                parameters["trigger_file"] = TriggerFile;
            }
            if (IsBound(nameof(AlwaysWriteTriggerFile)))
            {
                parameters["always_write_trigger_file"] = AlwaysWriteTriggerFile.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Sync.
    /// </summary>
    [Cmdlet("Remove", "FilesSync", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesSyncCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Sync ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The sync to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Sync InputObject { get; set; }

        /// <summary>Delete Sync.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Sync {id}"))
            {
                return;
            }
            Run(connection, token => SyncBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
