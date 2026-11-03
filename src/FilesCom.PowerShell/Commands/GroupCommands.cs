using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Groups, or get one group by Id.
    /// </summary>
    [Cmdlet("Get", "FilesGroup", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Group), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Group>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Group), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesGroupCommand : FilesListCmdlet<Models.Group>
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
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `name` and `workspace_id`. Valid field combinations are `[ workspace_id, name ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// Comma-separated list of group ids to include in results.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] Ids { get; set; }

        /// <summary>
        /// Include groups from the parent site.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? IncludeParentSiteGroups { get; set; }

        /// <summary>
        /// When scoped to a Custom Workspace, include Default Workspace groups that have permissions in that Workspace. Available to Site Administrators, and to Workspace Administrators and Folder Admins in that Workspace when the site shares its Default Workspace directory with Workspaces.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? IncludeDefaultWorkspaceGroups { get; set; }

        /// <summary>
        /// Group ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Groups, or get one group by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Group result = Run(connection, token => GroupBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => GroupBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(Ids)))
            {
                parameters["ids"] = RequestValues.JoinInt64s(Ids);
            }
            if (IsBound(nameof(IncludeParentSiteGroups)))
            {
                parameters["include_parent_site_groups"] = IncludeParentSiteGroups.Value;
            }
            if (IsBound(nameof(IncludeDefaultWorkspaceGroups)))
            {
                parameters["include_default_workspace_groups"] = IncludeDefaultWorkspaceGroups.Value;
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
    /// Create Group.
    /// </summary>
    [Cmdlet("New", "FilesGroup", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.Group))]
    public sealed class NewFilesGroupCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Group notes.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Notes { get; set; }

        /// <summary>
        /// A list of user ids. If sent as a string, should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] UserIds { get; set; }

        /// <summary>
        /// A list of group admin user ids. If sent as a string, should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] AdminIds { get; set; }

        /// <summary>
        /// AI Assistant Personality ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user or Partner assignment overrides it.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? AiAssistantPersonalityId { get; set; }

        /// <summary>
        /// If true, users in this group can use FTP to login.  This will override a false value of `ftp_permission` on the user level.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FtpPermission { get; set; }

        /// <summary>
        /// If true, users in this group can use SFTP to login.  This will override a false value of `sftp_permission` on the user level.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpPermission { get; set; }

        /// <summary>
        /// If true, users in this group can use WebDAV to login.  This will override a false value of `dav_permission` on the user level.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DavPermission { get; set; }

        /// <summary>
        /// If true, users in this group can use the REST API to login.  This will override a false value of `restapi_permission` on the user level.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RestapiPermission { get; set; }

        /// <summary>
        /// If true, users in this group can access the S3-compatible endpoint. This will override a false value of `s3_compatible_endpoint_permission` on the user level. Defaults to false.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? S3CompatibleEndpointPermission { get; set; }

        /// <summary>
        /// Desktop Configuration Profile ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? DesktopConfigurationProfileId { get; set; }

        /// <summary>
        /// Integration Centric Profile ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? IntegrationCentricProfileId { get; set; }

        /// <summary>
        /// A list of allowed IPs if applicable.  Newline delimited
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] AllowedIps { get; set; }

        /// <summary>
        /// Group name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Group.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Group '{Name}'" : "Group"))
            {
                return;
            }
            Models.Group result = Run(connection, token => GroupBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Notes)))
            {
                parameters["notes"] = Notes;
            }
            if (IsBound(nameof(UserIds)))
            {
                parameters["user_ids"] = RequestValues.JoinInt64s(UserIds);
            }
            if (IsBound(nameof(AdminIds)))
            {
                parameters["admin_ids"] = RequestValues.JoinInt64s(AdminIds);
            }
            if (IsBound(nameof(AiAssistantPersonalityId)))
            {
                parameters["ai_assistant_personality_id"] = AiAssistantPersonalityId.Value;
            }
            if (IsBound(nameof(FtpPermission)))
            {
                parameters["ftp_permission"] = FtpPermission.Value;
            }
            if (IsBound(nameof(SftpPermission)))
            {
                parameters["sftp_permission"] = SftpPermission.Value;
            }
            if (IsBound(nameof(DavPermission)))
            {
                parameters["dav_permission"] = DavPermission.Value;
            }
            if (IsBound(nameof(RestapiPermission)))
            {
                parameters["restapi_permission"] = RestapiPermission.Value;
            }
            if (IsBound(nameof(S3CompatibleEndpointPermission)))
            {
                parameters["s3_compatible_endpoint_permission"] = S3CompatibleEndpointPermission.Value;
            }
            if (IsBound(nameof(DesktopConfigurationProfileId)))
            {
                parameters["desktop_configuration_profile_id"] = DesktopConfigurationProfileId.Value;
            }
            if (IsBound(nameof(IntegrationCentricProfileId)))
            {
                parameters["integration_centric_profile_id"] = IntegrationCentricProfileId.Value;
            }
            if (IsBound(nameof(AllowedIps)))
            {
                parameters["allowed_ips"] = RequestValues.Join(AllowedIps, "AllowedIps", "\n");
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
    /// Update Group.
    /// </summary>
    [Cmdlet("Set", "FilesGroup", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.Group), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesGroupCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Group ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Group notes.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Notes { get; set; }

        /// <summary>
        /// A list of user ids. If sent as a string, should be comma-delimited.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] UserIds { get; set; }

        /// <summary>
        /// A list of group admin user ids. If sent as a string, should be comma-delimited.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] AdminIds { get; set; }

        /// <summary>
        /// AI Assistant Personality ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user or Partner assignment overrides it.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? AiAssistantPersonalityId { get; set; }

        /// <summary>
        /// If true, users in this group can use FTP to login.  This will override a false value of `ftp_permission` on the user level.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? FtpPermission { get; set; }

        /// <summary>
        /// If true, users in this group can use SFTP to login.  This will override a false value of `sftp_permission` on the user level.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SftpPermission { get; set; }

        /// <summary>
        /// If true, users in this group can use WebDAV to login.  This will override a false value of `dav_permission` on the user level.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DavPermission { get; set; }

        /// <summary>
        /// If true, users in this group can use the REST API to login.  This will override a false value of `restapi_permission` on the user level.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? RestapiPermission { get; set; }

        /// <summary>
        /// If true, users in this group can access the S3-compatible endpoint. This will override a false value of `s3_compatible_endpoint_permission` on the user level. Defaults to false.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? S3CompatibleEndpointPermission { get; set; }

        /// <summary>
        /// Desktop Configuration Profile ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? DesktopConfigurationProfileId { get; set; }

        /// <summary>
        /// Integration Centric Profile ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? IntegrationCentricProfileId { get; set; }

        /// <summary>
        /// A list of allowed IPs if applicable.  Newline delimited
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] AllowedIps { get; set; }

        /// <summary>
        /// Group name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// The group to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Group InputObject { get; set; }

        /// <summary>Update Group.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Group {id}"))
            {
                return;
            }
            Models.Group result = Run(connection, token => GroupBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Notes)))
            {
                parameters["notes"] = Notes;
            }
            if (IsBound(nameof(UserIds)))
            {
                parameters["user_ids"] = RequestValues.JoinInt64s(UserIds);
            }
            if (IsBound(nameof(AdminIds)))
            {
                parameters["admin_ids"] = RequestValues.JoinInt64s(AdminIds);
            }
            if (IsBound(nameof(AiAssistantPersonalityId)))
            {
                parameters["ai_assistant_personality_id"] = AiAssistantPersonalityId.Value;
            }
            if (IsBound(nameof(FtpPermission)))
            {
                parameters["ftp_permission"] = FtpPermission.Value;
            }
            if (IsBound(nameof(SftpPermission)))
            {
                parameters["sftp_permission"] = SftpPermission.Value;
            }
            if (IsBound(nameof(DavPermission)))
            {
                parameters["dav_permission"] = DavPermission.Value;
            }
            if (IsBound(nameof(RestapiPermission)))
            {
                parameters["restapi_permission"] = RestapiPermission.Value;
            }
            if (IsBound(nameof(S3CompatibleEndpointPermission)))
            {
                parameters["s3_compatible_endpoint_permission"] = S3CompatibleEndpointPermission.Value;
            }
            if (IsBound(nameof(DesktopConfigurationProfileId)))
            {
                parameters["desktop_configuration_profile_id"] = DesktopConfigurationProfileId.Value;
            }
            if (IsBound(nameof(IntegrationCentricProfileId)))
            {
                parameters["integration_centric_profile_id"] = IntegrationCentricProfileId.Value;
            }
            if (IsBound(nameof(AllowedIps)))
            {
                parameters["allowed_ips"] = RequestValues.Join(AllowedIps, "AllowedIps", "\n");
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Group.
    /// </summary>
    [Cmdlet("Remove", "FilesGroup", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesGroupCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Group ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The group to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Group InputObject { get; set; }

        /// <summary>Delete Group.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Group {id}"))
            {
                return;
            }
            Run(connection, token => GroupBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
