using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Automations, or get one automation by Id.
    /// </summary>
    [Cmdlet("Get", "FilesAutomation", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Automation), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Automation>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Automation), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesAutomationCommand : FilesListCmdlet<Models.Automation>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `name`, `automation`, `last_modified_at` or `disabled`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `disabled`, `last_modified_at`, `workspace_id` or `automation`. Valid field combinations are `[ disabled, last_modified_at ]`, `[ workspace_id, disabled ]`, `[ disabled, automation ]`, `[ workspace_id, last_modified_at ]`, `[ automation, last_modified_at ]`, `[ workspace_id, automation ]`, `[ workspace_id, disabled, last_modified_at ]`, `[ disabled, automation, last_modified_at ]`, `[ workspace_id, disabled, automation ]`, `[ workspace_id, automation, last_modified_at ]` or `[ workspace_id, disabled, automation, last_modified_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than the supplied value. Valid fields are `last_modified_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `last_modified_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGteq { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than the supplied value. Valid fields are `last_modified_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than or equal the supplied value. Valid fields are `last_modified_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLteq { get; set; }

        /// <summary>
        /// Automation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Automations, or get one automation by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Automation result = Run(connection, token => AutomationBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => AutomationBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(FilterGt)))
            {
                parameters["filter_gt"] = RequestValues.QueryObject(FilterGt, "FilterGt");
            }
            if (IsBound(nameof(FilterGteq)))
            {
                parameters["filter_gteq"] = RequestValues.QueryObject(FilterGteq, "FilterGteq");
            }
            if (IsBound(nameof(FilterLt)))
            {
                parameters["filter_lt"] = RequestValues.QueryObject(FilterLt, "FilterLt");
            }
            if (IsBound(nameof(FilterLteq)))
            {
                parameters["filter_lteq"] = RequestValues.QueryObject(FilterLteq, "FilterLteq");
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
    /// Show the Automation v2 authoring schema and active node catalog.
    /// </summary>
    [Cmdlet("Get", "FilesAutomationAuthoringSchema")]
    [OutputType(typeof(Models.AutomationAuthoringSchema))]
    public sealed class GetFilesAutomationAuthoringSchemaCommand : FilesApiCmdlet
    {

        /// <summary>Show the Automation v2 authoring schema and active node catalog.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = GetAuthoringSchemaParameters();
            Models.AutomationAuthoringSchema result = Run(connection, token => AutomationBridge.GetAuthoringSchemaAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> GetAuthoringSchemaParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Automation.
    /// </summary>
    [Cmdlet("New", "FilesAutomation", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Automation))]
    public sealed class NewFilesAutomationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Source path/glob.  See Automation docs for exact description, but this is used to filter for files in the `path` to find files to operate on. Supports globs, except on remote mounts.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Source { get; set; }

        /// <summary>
        /// A list of destination paths. Use a trailing slash for folder destinations and omit it for file destinations.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] Destinations { get; set; }

        /// <summary>
        /// If set, this string in the destination path will be replaced with the value in `destination_replace_to`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DestinationReplaceFrom { get; set; }

        /// <summary>
        /// If set, this string will replace the value `destination_replace_from` in the destination filename. You can use special patterns here.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DestinationReplaceTo { get; set; }

        /// <summary>
        /// How often to run this automation? One of: `day`, `week`, `week_end`, `month`, `month_end`, `quarter`, `quarter_end`, `year`, `year_end`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Interval { get; set; }

        /// <summary>
        /// Path on which this Automation runs.  Supports globs, except on remote mounts.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// A list of sync IDs the automation is associated with. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] SyncIds { get; set; }

        /// <summary>
        /// A list of user IDs the automation is associated with. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] UserIds { get; set; }

        /// <summary>
        /// A list of group IDs the automation is associated with. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the automation's schedule fields.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ScheduleId { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule` without `schedule_id`, a list of days of the week to run this automation. 0 is Sunday, 1 is Monday, etc.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ScheduleDaysOfWeek { get; set; }

        /// <summary>
        /// Times of day to run in HH:MM format (24-hour). Required for `custom_schedule` without `schedule_id`. Optional for `daily` triggers; if not set, runs at midnight UTC.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Time zone for schedule fields. Optional for `custom_schedule` without `schedule_id` and for `daily`. If not set, times are interpreted as UTC.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Skip automation on holidays in this region. Optional for `custom_schedule` without `schedule_id` and for `daily`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// Ordinarily, files with identical size in the source and destination will be skipped from copy operations to prevent wasted transfer.  If this flag is `true` we will overwrite the destination file always.  Note that this may cause large amounts of wasted transfer usage.  This setting has no effect unless `overwrite_files` is also set to `true`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AlwaysOverwriteSizeMatchingFiles { get; set; }

        /// <summary>
        /// Ordinarily, we will allow automation runs to run in parallel for non-scheduled automations. If this flag is `true` we will force automation runs to be serialized (run one at a time, one after another). This can resolve some issues with race conditions on remote systems at the cost of some performance.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AlwaysSerializeJobs { get; set; }

        /// <summary>
        /// Description for the this Automation.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Automation v2 graph definition.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Definition { get; set; }

        /// <summary>
        /// If true, this automation will not run.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// If set, this glob pattern will exclude files from the automation. Supports globs, except on remote mounts.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ExcludePattern { get; set; }

        /// <summary>
        /// List of URLs to be imported and names to be used.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary[] ImportUrls { get; set; }

        /// <summary>
        /// Normally copy and move automations that use globs will implicitly preserve the source folder structure in the destination.  If this flag is `true`, the source folder structure will be flattened in the destination.  This is useful for copying or moving files from multiple folders into a single destination folder.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FlattenDestinationStructure { get; set; }

        /// <summary>
        /// If true, the Lock Folders behavior will be disregarded for automated actions.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? IgnoreLockedFolders { get; set; }

        /// <summary>
        /// DEPRECATED: If `true`, use the legacy behavior for this automation, where it can operate on folders in addition to just files.  This behavior no longer works and should not be used.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? LegacyFolderMatching { get; set; }

        /// <summary>
        /// Name for this automation.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// If true, existing files will be overwritten with new files on Move/Copy automations.  Note: by default files will not be overwritten on Copy automations if they appear to be the same file size as the newly incoming file.  Use the `always_overwrite_size_matching_files` option in conjunction with `overwrite_files` to override this behavior and overwrite files no matter what.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? OverwriteFiles { get; set; }

        /// <summary>
        /// Timezone to use when rendering timestamps in paths.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PathTimeZone { get; set; }

        /// <summary>
        /// If the Automation fails, retry at this interval (in minutes).  Acceptable values are 5 through 1440 (one day).  Set to null to disable.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? RetryOnFailureIntervalInMinutes { get; set; }

        /// <summary>
        /// If the Automation fails, retry at most this many times.  Maximum allowed value: 10.  Set to null to disable.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? RetryOnFailureNumberOfAttempts { get; set; }

        /// <summary>
        /// How this automation is triggered to run.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// If trigger is `action`, this is the list of action types on which to trigger the automation. Valid actions are create, copy, move, archived_delete, update, read, destroy
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] TriggerActions { get; set; }

        /// <summary>
        /// A Hash of attributes specific to the automation type.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Value { get; set; }

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
        /// Automation type
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Automation { get; set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Automation.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Automation '{Path}'" : "Automation"))
            {
                return;
            }
            Models.Automation result = Run(connection, token => AutomationBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Source)))
            {
                parameters["source"] = Source;
            }
            if (IsBound(nameof(Destinations)))
            {
                parameters["destinations"] = RequestValues.Strings(Destinations, "Destinations");
            }
            if (IsBound(nameof(DestinationReplaceFrom)))
            {
                parameters["destination_replace_from"] = DestinationReplaceFrom;
            }
            if (IsBound(nameof(DestinationReplaceTo)))
            {
                parameters["destination_replace_to"] = DestinationReplaceTo;
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(SyncIds)))
            {
                parameters["sync_ids"] = RequestValues.JoinInt64s(SyncIds);
            }
            if (IsBound(nameof(UserIds)))
            {
                parameters["user_ids"] = RequestValues.JoinInt64s(UserIds);
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.JoinInt64s(GroupIds);
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
            if (IsBound(nameof(AlwaysOverwriteSizeMatchingFiles)))
            {
                parameters["always_overwrite_size_matching_files"] = AlwaysOverwriteSizeMatchingFiles.Value;
            }
            if (IsBound(nameof(AlwaysSerializeJobs)))
            {
                parameters["always_serialize_jobs"] = AlwaysSerializeJobs.Value;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Definition)))
            {
                parameters["definition"] = RequestValues.Object(Definition, "Definition");
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(ExcludePattern)))
            {
                parameters["exclude_pattern"] = ExcludePattern;
            }
            if (IsBound(nameof(ImportUrls)))
            {
                parameters["import_urls"] = RequestValues.Objects(ImportUrls, "ImportUrls");
            }
            if (IsBound(nameof(FlattenDestinationStructure)))
            {
                parameters["flatten_destination_structure"] = FlattenDestinationStructure.Value;
            }
            if (IsBound(nameof(IgnoreLockedFolders)))
            {
                parameters["ignore_locked_folders"] = IgnoreLockedFolders.Value;
            }
            if (IsBound(nameof(LegacyFolderMatching)))
            {
                parameters["legacy_folder_matching"] = LegacyFolderMatching.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(OverwriteFiles)))
            {
                parameters["overwrite_files"] = OverwriteFiles.Value;
            }
            if (IsBound(nameof(PathTimeZone)))
            {
                parameters["path_time_zone"] = PathTimeZone;
            }
            if (IsBound(nameof(RetryOnFailureIntervalInMinutes)))
            {
                parameters["retry_on_failure_interval_in_minutes"] = RetryOnFailureIntervalInMinutes.Value;
            }
            if (IsBound(nameof(RetryOnFailureNumberOfAttempts)))
            {
                parameters["retry_on_failure_number_of_attempts"] = RetryOnFailureNumberOfAttempts.Value;
            }
            if (IsBound(nameof(Trigger)))
            {
                parameters["trigger"] = Trigger;
            }
            if (IsBound(nameof(TriggerActions)))
            {
                parameters["trigger_actions"] = RequestValues.Strings(TriggerActions, "TriggerActions");
            }
            if (IsBound(nameof(Value)))
            {
                parameters["value"] = RequestValues.Object(Value, "Value");
            }
            if (IsBound(nameof(RecurringDay)))
            {
                parameters["recurring_day"] = RecurringDay.Value;
            }
            if (IsBound(nameof(RecurringDays)))
            {
                parameters["recurring_days"] = RequestValues.Int64s(RecurringDays);
            }
            if (IsBound(nameof(Automation)))
            {
                parameters["automation"] = Automation;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Upgrade a legacy Automation to Automation v2.
    /// </summary>
    [Cmdlet("Update", "FilesAutomation", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Automation), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class UpdateFilesAutomationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Automation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The automation to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Automation InputObject { get; set; }

        /// <summary>Upgrade a legacy Automation to Automation v2.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpgradeParameters();
            if (!ShouldProcessOn(connection, $"Automation {id}"))
            {
                return;
            }
            Models.Automation result = Run(connection, token => AutomationBridge.UpgradeAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpgradeParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Manually Run Automation.
    /// </summary>
    [Cmdlet("Start", "FilesAutomation", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class StartFilesAutomationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Automation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Initial items for a v2 manual trigger. Each item contains exactly one `file` path or `data` object.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary[] Items { get; set; }

        /// <summary>
        /// The automation to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Automation InputObject { get; set; }

        /// <summary>Manually Run Automation.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ManualRunParameters();
            if (!ShouldProcessOn(connection, $"Automation {id}"))
            {
                return;
            }
            Run(connection, token => AutomationBridge.ManualRunAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> ManualRunParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Items)))
            {
                parameters["items"] = RequestValues.Objects(Items, "Items");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Automation.
    /// </summary>
    [Cmdlet("Set", "FilesAutomation", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Automation), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesAutomationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Automation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Source path/glob.  See Automation docs for exact description, but this is used to filter for files in the `path` to find files to operate on. Supports globs, except on remote mounts.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Source { get; set; }

        /// <summary>
        /// A list of destination paths. Use a trailing slash for folder destinations and omit it for file destinations.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] Destinations { get; set; }

        /// <summary>
        /// If set, this string in the destination path will be replaced with the value in `destination_replace_to`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string DestinationReplaceFrom { get; set; }

        /// <summary>
        /// If set, this string will replace the value `destination_replace_from` in the destination filename. You can use special patterns here.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string DestinationReplaceTo { get; set; }

        /// <summary>
        /// How often to run this automation? One of: `day`, `week`, `week_end`, `month`, `month_end`, `quarter`, `quarter_end`, `year`, `year_end`
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Interval { get; set; }

        /// <summary>
        /// Path on which this Automation runs.  Supports globs, except on remote mounts.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// A list of sync IDs the automation is associated with. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] SyncIds { get; set; }

        /// <summary>
        /// A list of user IDs the automation is associated with. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] UserIds { get; set; }

        /// <summary>
        /// A list of group IDs the automation is associated with. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule`, the reusable Schedule used instead of the automation's schedule fields.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ScheduleId { get; set; }

        /// <summary>
        /// If trigger is `custom_schedule` without `schedule_id`, a list of days of the week to run this automation. 0 is Sunday, 1 is Monday, etc.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ScheduleDaysOfWeek { get; set; }

        /// <summary>
        /// Times of day to run in HH:MM format (24-hour). Required for `custom_schedule` without `schedule_id`. Optional for `daily` triggers; if not set, runs at midnight UTC.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] ScheduleTimesOfDay { get; set; }

        /// <summary>
        /// Time zone for schedule fields. Optional for `custom_schedule` without `schedule_id` and for `daily`. If not set, times are interpreted as UTC.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ScheduleTimeZone { get; set; }

        /// <summary>
        /// Skip automation on holidays in this region. Optional for `custom_schedule` without `schedule_id` and for `daily`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HolidayRegion { get; set; }

        /// <summary>
        /// Ordinarily, files with identical size in the source and destination will be skipped from copy operations to prevent wasted transfer.  If this flag is `true` we will overwrite the destination file always.  Note that this may cause large amounts of wasted transfer usage.  This setting has no effect unless `overwrite_files` is also set to `true`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AlwaysOverwriteSizeMatchingFiles { get; set; }

        /// <summary>
        /// Ordinarily, we will allow automation runs to run in parallel for non-scheduled automations. If this flag is `true` we will force automation runs to be serialized (run one at a time, one after another). This can resolve some issues with race conditions on remote systems at the cost of some performance.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AlwaysSerializeJobs { get; set; }

        /// <summary>
        /// Description for the this Automation.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Automation v2 graph definition.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Definition { get; set; }

        /// <summary>
        /// If true, this automation will not run.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// If set, this glob pattern will exclude files from the automation. Supports globs, except on remote mounts.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ExcludePattern { get; set; }

        /// <summary>
        /// List of URLs to be imported and names to be used.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary[] ImportUrls { get; set; }

        /// <summary>
        /// Normally copy and move automations that use globs will implicitly preserve the source folder structure in the destination.  If this flag is `true`, the source folder structure will be flattened in the destination.  This is useful for copying or moving files from multiple folders into a single destination folder.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? FlattenDestinationStructure { get; set; }

        /// <summary>
        /// If true, the Lock Folders behavior will be disregarded for automated actions.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? IgnoreLockedFolders { get; set; }

        /// <summary>
        /// DEPRECATED: If `true`, use the legacy behavior for this automation, where it can operate on folders in addition to just files.  This behavior no longer works and should not be used.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? LegacyFolderMatching { get; set; }

        /// <summary>
        /// Name for this automation.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// If true, existing files will be overwritten with new files on Move/Copy automations.  Note: by default files will not be overwritten on Copy automations if they appear to be the same file size as the newly incoming file.  Use the `always_overwrite_size_matching_files` option in conjunction with `overwrite_files` to override this behavior and overwrite files no matter what.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? OverwriteFiles { get; set; }

        /// <summary>
        /// Timezone to use when rendering timestamps in paths.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PathTimeZone { get; set; }

        /// <summary>
        /// If the Automation fails, retry at this interval (in minutes).  Acceptable values are 5 through 1440 (one day).  Set to null to disable.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? RetryOnFailureIntervalInMinutes { get; set; }

        /// <summary>
        /// If the Automation fails, retry at most this many times.  Maximum allowed value: 10.  Set to null to disable.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? RetryOnFailureNumberOfAttempts { get; set; }

        /// <summary>
        /// How this automation is triggered to run.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Trigger { get; set; }

        /// <summary>
        /// If trigger is `action`, this is the list of action types on which to trigger the automation. Valid actions are create, copy, move, archived_delete, update, read, destroy
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] TriggerActions { get; set; }

        /// <summary>
        /// A Hash of attributes specific to the automation type.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Value { get; set; }

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
        /// Automation type
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Automation { get; set; }

        /// <summary>
        /// The automation to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Automation InputObject { get; set; }

        /// <summary>Update Automation.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Automation {id}"))
            {
                return;
            }
            Models.Automation result = Run(connection, token => AutomationBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Source)))
            {
                parameters["source"] = Source;
            }
            if (IsBound(nameof(Destinations)))
            {
                parameters["destinations"] = RequestValues.Strings(Destinations, "Destinations");
            }
            if (IsBound(nameof(DestinationReplaceFrom)))
            {
                parameters["destination_replace_from"] = DestinationReplaceFrom;
            }
            if (IsBound(nameof(DestinationReplaceTo)))
            {
                parameters["destination_replace_to"] = DestinationReplaceTo;
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(SyncIds)))
            {
                parameters["sync_ids"] = RequestValues.JoinInt64s(SyncIds);
            }
            if (IsBound(nameof(UserIds)))
            {
                parameters["user_ids"] = RequestValues.JoinInt64s(UserIds);
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.JoinInt64s(GroupIds);
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
            if (IsBound(nameof(AlwaysOverwriteSizeMatchingFiles)))
            {
                parameters["always_overwrite_size_matching_files"] = AlwaysOverwriteSizeMatchingFiles.Value;
            }
            if (IsBound(nameof(AlwaysSerializeJobs)))
            {
                parameters["always_serialize_jobs"] = AlwaysSerializeJobs.Value;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Definition)))
            {
                parameters["definition"] = RequestValues.Object(Definition, "Definition");
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(ExcludePattern)))
            {
                parameters["exclude_pattern"] = ExcludePattern;
            }
            if (IsBound(nameof(ImportUrls)))
            {
                parameters["import_urls"] = RequestValues.Objects(ImportUrls, "ImportUrls");
            }
            if (IsBound(nameof(FlattenDestinationStructure)))
            {
                parameters["flatten_destination_structure"] = FlattenDestinationStructure.Value;
            }
            if (IsBound(nameof(IgnoreLockedFolders)))
            {
                parameters["ignore_locked_folders"] = IgnoreLockedFolders.Value;
            }
            if (IsBound(nameof(LegacyFolderMatching)))
            {
                parameters["legacy_folder_matching"] = LegacyFolderMatching.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(OverwriteFiles)))
            {
                parameters["overwrite_files"] = OverwriteFiles.Value;
            }
            if (IsBound(nameof(PathTimeZone)))
            {
                parameters["path_time_zone"] = PathTimeZone;
            }
            if (IsBound(nameof(RetryOnFailureIntervalInMinutes)))
            {
                parameters["retry_on_failure_interval_in_minutes"] = RetryOnFailureIntervalInMinutes.Value;
            }
            if (IsBound(nameof(RetryOnFailureNumberOfAttempts)))
            {
                parameters["retry_on_failure_number_of_attempts"] = RetryOnFailureNumberOfAttempts.Value;
            }
            if (IsBound(nameof(Trigger)))
            {
                parameters["trigger"] = Trigger;
            }
            if (IsBound(nameof(TriggerActions)))
            {
                parameters["trigger_actions"] = RequestValues.Strings(TriggerActions, "TriggerActions");
            }
            if (IsBound(nameof(Value)))
            {
                parameters["value"] = RequestValues.Object(Value, "Value");
            }
            if (IsBound(nameof(RecurringDay)))
            {
                parameters["recurring_day"] = RecurringDay.Value;
            }
            if (IsBound(nameof(RecurringDays)))
            {
                parameters["recurring_days"] = RequestValues.Int64s(RecurringDays);
            }
            if (IsBound(nameof(Automation)))
            {
                parameters["automation"] = Automation;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Automation.
    /// </summary>
    [Cmdlet("Remove", "FilesAutomation", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesAutomationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Automation ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The automation to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Automation InputObject { get; set; }

        /// <summary>Delete Automation.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Automation {id}"))
            {
                return;
            }
            Run(connection, token => AutomationBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
