using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Permissions.
    /// </summary>
    [Cmdlet("Get", "FilesPermission", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Permission), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Permission>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesPermissionCommand : FilesListCmdlet<Models.Permission>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `site_id`, `group_id`, `path`, `user_id`, `partner_id` or `id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `path`, `group_id`, `partner_id` or `user_id`. Valid field combinations are `[ group_id, path ]`, `[ partner_id, path ]`, `[ user_id, path ]`, `[ user_id, group_id ]`, `[ user_id, group_id, path ]`, `[ user_id, group_id, partner_id ]` or `[ user_id, group_id, partner_id, path ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `path`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// Permission path.  If provided, will scope all permissions(including upward) to this path.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// If searching by user or group, also include user's permissions that are inherited from its groups?
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? IncludeGroups { get; set; }

        /// <summary>
        /// group_id
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string GroupId { get; set; }

        /// <summary>
        /// partner_id
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string PartnerId { get; set; }

        /// <summary>
        /// user_id
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string UserId { get; set; }

        /// <summary>List Permissions.</summary>
        protected override void Execute()
        {
            WriteList(connection => PermissionBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(IncludeGroups)))
            {
                parameters["include_groups"] = IncludeGroups.Value;
            }
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId;
            }
            if (IsBound(nameof(PartnerId)))
            {
                parameters["partner_id"] = PartnerId;
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Permission.
    /// </summary>
    [Cmdlet("New", "FilesPermission", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.Permission))]
    public sealed class NewFilesPermissionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Folder path
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Group ID. Provide `group_name` or `group_id`
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? GroupId { get; set; }

        /// <summary>
        /// Group IDs when the Permission requires membership in every listed group. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>
        /// Permission type.  Can be `admin`, `full`, `readonly`, `writeonly`, `list`, or `history`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Permission { get; set; }

        /// <summary>
        /// Apply to subfolders recursively? Must be true for `admin` Permissions.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Recursive { get; set; }

        /// <summary>
        /// Partner ID if this Permission belongs to a partner.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PartnerId { get; set; }

        /// <summary>
        /// User ID.  Provide `username` or `user_id`
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// User username.  Provide `username` or `user_id`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Username { get; set; }

        /// <summary>
        /// Group name.  Provide `group_name` or `group_id`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GroupName { get; set; }

        /// <summary>
        /// Site ID. If not provided, will default to current site. Used when creating a permission for a child site.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SiteId { get; set; }

        /// <summary>Create Permission.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Permission '{Path}'" : "Permission"))
            {
                return;
            }
            Models.Permission result = Run(connection, token => PermissionBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId.Value;
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.JoinInt64s(GroupIds);
            }
            if (IsBound(nameof(Permission)))
            {
                parameters["permission"] = Permission;
            }
            if (IsBound(nameof(Recursive)))
            {
                parameters["recursive"] = Recursive.Value;
            }
            if (IsBound(nameof(PartnerId)))
            {
                parameters["partner_id"] = PartnerId.Value;
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            if (IsBound(nameof(GroupName)))
            {
                parameters["group_name"] = GroupName;
            }
            if (IsBound(nameof(SiteId)))
            {
                parameters["site_id"] = SiteId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Permission.
    /// </summary>
    [Cmdlet("Remove", "FilesPermission", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesPermissionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Permission ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The permission to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Permission InputObject { get; set; }

        /// <summary>Delete Permission.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Permission {id}"))
            {
                return;
            }
            Run(connection, token => PermissionBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
