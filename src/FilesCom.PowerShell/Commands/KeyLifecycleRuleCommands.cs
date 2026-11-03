using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Key Lifecycle Rules, or get one key lifecycle rule by Id.
    /// </summary>
    [Cmdlet("Get", "FilesKeyLifecycleRule", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.KeyLifecycleRule), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.KeyLifecycleRule>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.KeyLifecycleRule), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesKeyLifecycleRuleCommand : FilesListCmdlet<Models.KeyLifecycleRule>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id` and `key_type`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `key_type` and `workspace_id`. Valid field combinations are `[ workspace_id, key_type ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Key Lifecycle Rule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Key Lifecycle Rules, or get one key lifecycle rule by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.KeyLifecycleRule result = Run(connection, token => KeyLifecycleRuleBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => KeyLifecycleRuleBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create Key Lifecycle Rule.
    /// </summary>
    [Cmdlet("New", "FilesKeyLifecycleRule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.KeyLifecycleRule))]
    public sealed class NewFilesKeyLifecycleRuleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// If true, a default-workspace rule also applies to keys in all workspaces.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ApplyToAllWorkspaces { get; set; }

        /// <summary>
        /// Number of days after creation before an SSH key expires. Applies only to SSH keys.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ExpirationDays { get; set; }

        /// <summary>
        /// Key type for which the rule will apply (gpg, ssh, or api).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string KeyType { get; set; }

        /// <summary>
        /// Number of days of inactivity before the rule applies.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? InactivityDays { get; set; }

        /// <summary>
        /// Key Lifecycle Rule name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Key Lifecycle Rule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Key lifecycle rule '{Name}'" : "Key lifecycle rule"))
            {
                return;
            }
            Models.KeyLifecycleRule result = Run(connection, token => KeyLifecycleRuleBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(ApplyToAllWorkspaces)))
            {
                parameters["apply_to_all_workspaces"] = ApplyToAllWorkspaces.Value;
            }
            if (IsBound(nameof(ExpirationDays)))
            {
                parameters["expiration_days"] = ExpirationDays.Value;
            }
            if (IsBound(nameof(KeyType)))
            {
                parameters["key_type"] = KeyType;
            }
            if (IsBound(nameof(InactivityDays)))
            {
                parameters["inactivity_days"] = InactivityDays.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Key Lifecycle Rule.
    /// </summary>
    [Cmdlet("Set", "FilesKeyLifecycleRule", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.KeyLifecycleRule), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesKeyLifecycleRuleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Key Lifecycle Rule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// If true, a default-workspace rule also applies to keys in all workspaces.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ApplyToAllWorkspaces { get; set; }

        /// <summary>
        /// Number of days after creation before an SSH key expires. Applies only to SSH keys.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ExpirationDays { get; set; }

        /// <summary>
        /// Key type for which the rule will apply (gpg, ssh, or api).
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string KeyType { get; set; }

        /// <summary>
        /// Number of days of inactivity before the rule applies.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? InactivityDays { get; set; }

        /// <summary>
        /// Key Lifecycle Rule name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// The key lifecycle rule to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.KeyLifecycleRule InputObject { get; set; }

        /// <summary>Update Key Lifecycle Rule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Key lifecycle rule {id}"))
            {
                return;
            }
            Models.KeyLifecycleRule result = Run(connection, token => KeyLifecycleRuleBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(ApplyToAllWorkspaces)))
            {
                parameters["apply_to_all_workspaces"] = ApplyToAllWorkspaces.Value;
            }
            if (IsBound(nameof(ExpirationDays)))
            {
                parameters["expiration_days"] = ExpirationDays.Value;
            }
            if (IsBound(nameof(KeyType)))
            {
                parameters["key_type"] = KeyType;
            }
            if (IsBound(nameof(InactivityDays)))
            {
                parameters["inactivity_days"] = InactivityDays.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Key Lifecycle Rule.
    /// </summary>
    [Cmdlet("Remove", "FilesKeyLifecycleRule", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesKeyLifecycleRuleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Key Lifecycle Rule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The key lifecycle rule to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.KeyLifecycleRule InputObject { get; set; }

        /// <summary>Delete Key Lifecycle Rule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Key lifecycle rule {id}"))
            {
                return;
            }
            Run(connection, token => KeyLifecycleRuleBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
