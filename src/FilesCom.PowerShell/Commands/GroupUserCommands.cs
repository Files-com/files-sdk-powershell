using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Group Users.
    /// </summary>
    [Cmdlet("Get", "FilesGroupUser", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.GroupUser), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.GroupUser>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesGroupUserCommand : FilesListCmdlet<Models.GroupUser>
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
        /// Group ID. If provided, returns memberships of this group. Requires a Site Administrator, a Read-only Administrator, a Workspace Administrator for the group's workspace, or a Group Administrator of this group.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? GroupId { get; set; }

        /// <summary>
        /// User ID.  If provided, will return group_users of this user.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>List Group Users.</summary>
        protected override void Execute()
        {
            WriteList(connection => GroupUserBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId.Value;
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Group User.
    /// </summary>
    [Cmdlet("New", "FilesGroupUser", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.GroupUser))]
    public sealed class NewFilesGroupUserCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Group ID to add user to.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long GroupId { get; set; }

        /// <summary>
        /// User ID to add to group.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long UserId { get; set; }

        /// <summary>
        /// Is the user a group administrator?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Admin { get; set; }

        /// <summary>Create Group User.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Group user"))
            {
                return;
            }
            Models.GroupUser result = Run(connection, token => GroupUserBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId;
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId;
            }
            if (IsBound(nameof(Admin)))
            {
                parameters["admin"] = Admin.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Group User.
    /// </summary>
    [Cmdlet("Set", "FilesGroupUser", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.GroupUser))]
    public sealed class SetFilesGroupUserCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Group User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Group ID to add user to.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long GroupId { get; set; }

        /// <summary>
        /// User ID to add to group.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long UserId { get; set; }

        /// <summary>
        /// Is the user a group administrator?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Admin { get; set; }

        /// <summary>Update Group User.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            long id = Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Group user {id}"))
            {
                return;
            }
            Models.GroupUser result = Run(connection, token => GroupUserBridge.UpdateAsync(connection, id, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId;
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId;
            }
            if (IsBound(nameof(Admin)))
            {
                parameters["admin"] = Admin.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Group User.
    /// </summary>
    [Cmdlet("Remove", "FilesGroupUser", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesGroupUserCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Group User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Group ID from which to remove user.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long GroupId { get; set; }

        /// <summary>
        /// User ID to remove from group.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long UserId { get; set; }

        /// <summary>Delete Group User.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            long id = Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Group user {id}"))
            {
                return;
            }
            Run(connection, token => GroupUserBridge.DeleteAsync(connection, id, parameters, CallOptions, token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId;
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId;
            }
            return parameters;
        }
    }
}
