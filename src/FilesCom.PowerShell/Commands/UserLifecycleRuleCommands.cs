using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List User Lifecycle Rules, or get one user lifecycle rule by Id.
    /// </summary>
    [Cmdlet("Get", "FilesUserLifecycleRule", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.UserLifecycleRule), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.UserLifecycleRule>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.UserLifecycleRule), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesUserLifecycleRuleCommand : FilesListCmdlet<Models.UserLifecycleRule>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `site_id` and `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// User Lifecycle Rule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List User Lifecycle Rules, or get one user lifecycle rule by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.UserLifecycleRule result = Run(connection, token => UserLifecycleRuleBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => UserLifecycleRuleBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create User Lifecycle Rule.
    /// </summary>
    [Cmdlet("New", "FilesUserLifecycleRule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.UserLifecycleRule))]
    public sealed class NewFilesUserLifecycleRuleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Action to take on inactive users (disable or delete)
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Action { get; set; }

        /// <summary>
        /// If true, a Default Workspace rule also applies to users in all Custom Workspaces. Can only be enabled when `workspace_id` is `0`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ApplyToAllWorkspaces { get; set; }

        /// <summary>
        /// User authentication method for which the rule will apply. Use `all_non_sso` to target every non-SSO authentication method with one rule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Array of Group IDs to which the rule applies. If empty or not set, the rule applies to all users.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>
        /// Number of days of inactivity before the rule applies
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? InactivityDays { get; set; }

        /// <summary>
        /// If true, the rule includes Site Administrators, who always belong to the Default Workspace. Can only be enabled when `workspace_id` is `0`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? IncludeSiteAdmins { get; set; }

        /// <summary>
        /// If true, the rule will apply to folder admins.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? IncludeFolderAdmins { get; set; }

        /// <summary>
        /// User Lifecycle Rule name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// If true, users will be emailed before the rule disables or deletes them.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyUsers { get; set; }

        /// <summary>
        /// If provided, only users belonging to Partners with this tag at the Partner level will be affected by the rule. Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PartnerTag { get; set; }

        /// <summary>
        /// State of the users to apply the rule to (inactive or disabled)
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UserState { get; set; }

        /// <summary>
        /// If provided, only users with this tag will be affected by the rule. Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UserTag { get; set; }

        /// <summary>
        /// Workspace whose users the rule applies to. `0` means the Default Workspace. A Custom Workspace rule applies only to users who belong to that Workspace, regardless of access granted to other users.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create User Lifecycle Rule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"User lifecycle rule '{Name}'" : "User lifecycle rule"))
            {
                return;
            }
            Models.UserLifecycleRule result = Run(connection, token => UserLifecycleRuleBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Action)))
            {
                parameters["action"] = Action;
            }
            if (IsBound(nameof(ApplyToAllWorkspaces)))
            {
                parameters["apply_to_all_workspaces"] = ApplyToAllWorkspaces.Value;
            }
            if (IsBound(nameof(AuthenticationMethod)))
            {
                parameters["authentication_method"] = AuthenticationMethod;
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.Int64s(GroupIds);
            }
            if (IsBound(nameof(InactivityDays)))
            {
                parameters["inactivity_days"] = InactivityDays.Value;
            }
            if (IsBound(nameof(IncludeSiteAdmins)))
            {
                parameters["include_site_admins"] = IncludeSiteAdmins.Value;
            }
            if (IsBound(nameof(IncludeFolderAdmins)))
            {
                parameters["include_folder_admins"] = IncludeFolderAdmins.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(NotifyUsers)))
            {
                parameters["notify_users"] = NotifyUsers.Value;
            }
            if (IsBound(nameof(PartnerTag)))
            {
                parameters["partner_tag"] = PartnerTag;
            }
            if (IsBound(nameof(UserState)))
            {
                parameters["user_state"] = UserState;
            }
            if (IsBound(nameof(UserTag)))
            {
                parameters["user_tag"] = UserTag;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update User Lifecycle Rule.
    /// </summary>
    [Cmdlet("Set", "FilesUserLifecycleRule", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.UserLifecycleRule), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesUserLifecycleRuleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User Lifecycle Rule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Action to take on inactive users (disable or delete)
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Action { get; set; }

        /// <summary>
        /// If true, a Default Workspace rule also applies to users in all Custom Workspaces. Can only be enabled when `workspace_id` is `0`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ApplyToAllWorkspaces { get; set; }

        /// <summary>
        /// User authentication method for which the rule will apply. Use `all_non_sso` to target every non-SSO authentication method with one rule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Array of Group IDs to which the rule applies. If empty or not set, the rule applies to all users.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>
        /// Number of days of inactivity before the rule applies
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? InactivityDays { get; set; }

        /// <summary>
        /// If true, the rule includes Site Administrators, who always belong to the Default Workspace. Can only be enabled when `workspace_id` is `0`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? IncludeSiteAdmins { get; set; }

        /// <summary>
        /// If true, the rule will apply to folder admins.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? IncludeFolderAdmins { get; set; }

        /// <summary>
        /// User Lifecycle Rule name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// If true, users will be emailed before the rule disables or deletes them.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyUsers { get; set; }

        /// <summary>
        /// If provided, only users belonging to Partners with this tag at the Partner level will be affected by the rule. Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PartnerTag { get; set; }

        /// <summary>
        /// State of the users to apply the rule to (inactive or disabled)
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string UserState { get; set; }

        /// <summary>
        /// If provided, only users with this tag will be affected by the rule. Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string UserTag { get; set; }

        /// <summary>
        /// Workspace whose users the rule applies to. `0` means the Default Workspace. A Custom Workspace rule applies only to users who belong to that Workspace, regardless of access granted to other users.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// The user lifecycle rule to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.UserLifecycleRule InputObject { get; set; }

        /// <summary>Update User Lifecycle Rule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"User lifecycle rule {id}"))
            {
                return;
            }
            Models.UserLifecycleRule result = Run(connection, token => UserLifecycleRuleBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Action)))
            {
                parameters["action"] = Action;
            }
            if (IsBound(nameof(ApplyToAllWorkspaces)))
            {
                parameters["apply_to_all_workspaces"] = ApplyToAllWorkspaces.Value;
            }
            if (IsBound(nameof(AuthenticationMethod)))
            {
                parameters["authentication_method"] = AuthenticationMethod;
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.Int64s(GroupIds);
            }
            if (IsBound(nameof(InactivityDays)))
            {
                parameters["inactivity_days"] = InactivityDays.Value;
            }
            if (IsBound(nameof(IncludeSiteAdmins)))
            {
                parameters["include_site_admins"] = IncludeSiteAdmins.Value;
            }
            if (IsBound(nameof(IncludeFolderAdmins)))
            {
                parameters["include_folder_admins"] = IncludeFolderAdmins.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(NotifyUsers)))
            {
                parameters["notify_users"] = NotifyUsers.Value;
            }
            if (IsBound(nameof(PartnerTag)))
            {
                parameters["partner_tag"] = PartnerTag;
            }
            if (IsBound(nameof(UserState)))
            {
                parameters["user_state"] = UserState;
            }
            if (IsBound(nameof(UserTag)))
            {
                parameters["user_tag"] = UserTag;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete User Lifecycle Rule.
    /// </summary>
    [Cmdlet("Remove", "FilesUserLifecycleRule", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesUserLifecycleRuleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User Lifecycle Rule ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The user lifecycle rule to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.UserLifecycleRule InputObject { get; set; }

        /// <summary>Delete User Lifecycle Rule.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"User lifecycle rule {id}"))
            {
                return;
            }
            Run(connection, token => UserLifecycleRuleBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
