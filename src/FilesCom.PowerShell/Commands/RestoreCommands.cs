using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Restores.
    /// </summary>
    [Cmdlet("Get", "FilesRestore", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Restore), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Restore>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesRestoreCommand : FilesListCmdlet<Models.Restore>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `restoration_type`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>List Restores.</summary>
        protected override void Execute()
        {
            WriteList(connection => RestoreBridge.List(connection, ListParameters(), CallOptions));
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
    }

    /// <summary>
    /// Create Restore.
    /// </summary>
    [Cmdlet("New", "FilesRestore", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Restore))]
    public sealed class NewFilesRestoreCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Restore files or users deleted on or after this date/time. Don't set this earlier than you need. Can not be greater than 365 days prior to the restore request.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset EarliestDate { get; set; }

        /// <summary>
        /// Prefix of the files/folders to restore, or a case-insensitive username prefix for a user restore. A trailing slash selects that folder and its contents only; without it, the prefix matches any path beginning with that text. Do not use a leading slash. To restore all deleted items of the selected restoration type within the selected site or workspace scope, specify an empty string (`''`) or omit the field.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Prefix { get; set; }

        /// <summary>
        /// Type of restoration to perform. `files` restores deleted filesystem items. `users` restores deleted users and associated authentication records, plus permissions when `restore_deleted_permissions=true`. A 5-minute grace period allows for batch deletion processes: associated records deleted within the 5 minutes before the user are included, even if removed independently. Records deleted earlier, or before `earliest_date`, remain deleted.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string RestorationType { get; set; }

        /// <summary>
        /// If true, a user restore restores permissions deleted along with the selected users, including permissions removed independently within the 5 minutes before user deletion. Permissions deleted earlier, or before `earliest_date`, remain deleted. An in-place file restore restores user and group permissions on returned folders and their subfolders that were deleted from the time of the folder's deletion through 24 hours afterward, including permissions removed independently within that window. File restores into a new folder do not restore permissions.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RestoreDeletedPermissions { get; set; }

        /// <summary>
        /// If true, we will restore the files in place (into their original paths). If false, we will create a new restoration folder in the root and restore files there.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RestoreInPlace { get; set; }

        /// <summary>
        /// If true, we will update the last modified timestamp of restored files to today's date. If false, we might trigger File Expiration to delete the file again.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UpdateTimestamps { get; set; }

        /// <summary>
        /// Workspace ID for a workspace-scoped restore. `0` means the default site-wide scope.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Restore.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Restore"))
            {
                return;
            }
            Models.Restore result = Run(connection, token => RestoreBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(EarliestDate)))
            {
                parameters["earliest_date"] = RequestValues.DateTime(EarliestDate);
            }
            if (IsBound(nameof(Prefix)))
            {
                parameters["prefix"] = Prefix;
            }
            if (IsBound(nameof(RestorationType)))
            {
                parameters["restoration_type"] = RestorationType;
            }
            if (IsBound(nameof(RestoreDeletedPermissions)))
            {
                parameters["restore_deleted_permissions"] = RestoreDeletedPermissions.Value;
            }
            if (IsBound(nameof(RestoreInPlace)))
            {
                parameters["restore_in_place"] = RestoreInPlace.Value;
            }
            if (IsBound(nameof(UpdateTimestamps)))
            {
                parameters["update_timestamps"] = UpdateTimestamps.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }
}
