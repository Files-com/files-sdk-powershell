using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Notifications, or get one notification by Id.
    /// </summary>
    [Cmdlet("Get", "FilesNotification", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Notification), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Notification>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Notification), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesNotificationCommand : FilesListCmdlet<Models.Notification>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `path`, `user_id` or `group_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `path`, `user_id`, `workspace_id` or `group_id`. Valid field combinations are `[ workspace_id, path ]`, `[ workspace_id, user_id ]`, `[ workspace_id, group_id ]` or `[ workspace_id, user_id, path ]`.
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
        /// Show notifications for this Path.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// If `include_ancestors` is `true` and `path` is specified, include notifications for any parent paths. Ignored if `path` is not specified.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? IncludeAncestors { get; set; }

        /// <summary>
        /// group_id
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string GroupId { get; set; }

        /// <summary>
        /// Notification ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Notifications, or get one notification by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Notification result = Run(connection, token => NotificationBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => NotificationBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(IncludeAncestors)))
            {
                parameters["include_ancestors"] = IncludeAncestors.Value;
            }
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId;
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
    /// Create Notification.
    /// </summary>
    [Cmdlet("New", "FilesNotification", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Notification))]
    public sealed class NewFilesNotificationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// The id of the user to notify. Provide `user_id`, `username` or `group_id`.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// If `true`, copying or moving resources into this path will trigger a notification, in addition to just uploads.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnCopy { get; set; }

        /// <summary>
        /// Trigger on files deleted in this path?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnDelete { get; set; }

        /// <summary>
        /// Trigger on files downloaded in this path?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnDownload { get; set; }

        /// <summary>
        /// Trigger on files moved to this path?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnMove { get; set; }

        /// <summary>
        /// Trigger on files created/uploaded/updated/changed in this path?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnUpload { get; set; }

        /// <summary>
        /// If `true` actions initiated by the user will still result in a notification
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyUserActions { get; set; }

        /// <summary>
        /// If `true`, enable notifications for each subfolder in this path
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Recursive { get; set; }

        /// <summary>
        /// The time interval that notifications are aggregated by.  Can be `five_minutes`, `fifteen_minutes`, `hourly`, or `daily`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SendInterval { get; set; }

        /// <summary>
        /// Custom subject line to use for notification emails
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Subject { get; set; }

        /// <summary>
        /// Custom message to include in notification emails
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Message { get; set; }

        /// <summary>
        /// Array of filenames (possibly with wildcards) to scope trigger
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] TriggeringFilenames { get; set; }

        /// <summary>
        /// If set, will only notify on actions made by a member of one of the specified groups
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] TriggeringGroupIds { get; set; }

        /// <summary>
        /// If set, will only notify on actions made one of the specified users
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] TriggeringUserIds { get; set; }

        /// <summary>
        /// Notify when actions are performed by a share recipient?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? TriggerByShareRecipients { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// The ID of the group to notify.  Provide `user_id`, `username` or `group_id`.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? GroupId { get; set; }

        /// <summary>
        /// Group IDs when the notification requires multiple groups. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>
        /// Path
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// The username of the user to notify.  Provide `user_id`, `username` or `group_id`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Username { get; set; }

        /// <summary>Create Notification.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Notification '{Path}'" : "Notification"))
            {
                return;
            }
            Models.Notification result = Run(connection, token => NotificationBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(NotifyOnCopy)))
            {
                parameters["notify_on_copy"] = NotifyOnCopy.Value;
            }
            if (IsBound(nameof(NotifyOnDelete)))
            {
                parameters["notify_on_delete"] = NotifyOnDelete.Value;
            }
            if (IsBound(nameof(NotifyOnDownload)))
            {
                parameters["notify_on_download"] = NotifyOnDownload.Value;
            }
            if (IsBound(nameof(NotifyOnMove)))
            {
                parameters["notify_on_move"] = NotifyOnMove.Value;
            }
            if (IsBound(nameof(NotifyOnUpload)))
            {
                parameters["notify_on_upload"] = NotifyOnUpload.Value;
            }
            if (IsBound(nameof(NotifyUserActions)))
            {
                parameters["notify_user_actions"] = NotifyUserActions.Value;
            }
            if (IsBound(nameof(Recursive)))
            {
                parameters["recursive"] = Recursive.Value;
            }
            if (IsBound(nameof(SendInterval)))
            {
                parameters["send_interval"] = SendInterval;
            }
            if (IsBound(nameof(Subject)))
            {
                parameters["subject"] = Subject;
            }
            if (IsBound(nameof(Message)))
            {
                parameters["message"] = Message;
            }
            if (IsBound(nameof(TriggeringFilenames)))
            {
                parameters["triggering_filenames"] = RequestValues.Strings(TriggeringFilenames, "TriggeringFilenames");
            }
            if (IsBound(nameof(TriggeringGroupIds)))
            {
                parameters["triggering_group_ids"] = RequestValues.Int64s(TriggeringGroupIds);
            }
            if (IsBound(nameof(TriggeringUserIds)))
            {
                parameters["triggering_user_ids"] = RequestValues.Int64s(TriggeringUserIds);
            }
            if (IsBound(nameof(TriggerByShareRecipients)))
            {
                parameters["trigger_by_share_recipients"] = TriggerByShareRecipients.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId.Value;
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.JoinInt64s(GroupIds);
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Notification.
    /// </summary>
    [Cmdlet("Set", "FilesNotification", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Notification), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesNotificationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Notification ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// If `true`, copying or moving resources into this path will trigger a notification, in addition to just uploads.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnCopy { get; set; }

        /// <summary>
        /// Trigger on files deleted in this path?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnDelete { get; set; }

        /// <summary>
        /// Trigger on files downloaded in this path?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnDownload { get; set; }

        /// <summary>
        /// Trigger on files moved to this path?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnMove { get; set; }

        /// <summary>
        /// Trigger on files created/uploaded/updated/changed in this path?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnUpload { get; set; }

        /// <summary>
        /// If `true` actions initiated by the user will still result in a notification
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyUserActions { get; set; }

        /// <summary>
        /// If `true`, enable notifications for each subfolder in this path
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Recursive { get; set; }

        /// <summary>
        /// The time interval that notifications are aggregated by.  Can be `five_minutes`, `fifteen_minutes`, `hourly`, or `daily`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SendInterval { get; set; }

        /// <summary>
        /// Custom subject line to use for notification emails
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Subject { get; set; }

        /// <summary>
        /// Custom message to include in notification emails
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Message { get; set; }

        /// <summary>
        /// Array of filenames (possibly with wildcards) to scope trigger
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] TriggeringFilenames { get; set; }

        /// <summary>
        /// If set, will only notify on actions made by a member of one of the specified groups
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] TriggeringGroupIds { get; set; }

        /// <summary>
        /// If set, will only notify on actions made one of the specified users
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] TriggeringUserIds { get; set; }

        /// <summary>
        /// Notify when actions are performed by a share recipient?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? TriggerByShareRecipients { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// The notification to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Notification InputObject { get; set; }

        /// <summary>Update Notification.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Notification {id}"))
            {
                return;
            }
            Models.Notification result = Run(connection, token => NotificationBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(NotifyOnCopy)))
            {
                parameters["notify_on_copy"] = NotifyOnCopy.Value;
            }
            if (IsBound(nameof(NotifyOnDelete)))
            {
                parameters["notify_on_delete"] = NotifyOnDelete.Value;
            }
            if (IsBound(nameof(NotifyOnDownload)))
            {
                parameters["notify_on_download"] = NotifyOnDownload.Value;
            }
            if (IsBound(nameof(NotifyOnMove)))
            {
                parameters["notify_on_move"] = NotifyOnMove.Value;
            }
            if (IsBound(nameof(NotifyOnUpload)))
            {
                parameters["notify_on_upload"] = NotifyOnUpload.Value;
            }
            if (IsBound(nameof(NotifyUserActions)))
            {
                parameters["notify_user_actions"] = NotifyUserActions.Value;
            }
            if (IsBound(nameof(Recursive)))
            {
                parameters["recursive"] = Recursive.Value;
            }
            if (IsBound(nameof(SendInterval)))
            {
                parameters["send_interval"] = SendInterval;
            }
            if (IsBound(nameof(Subject)))
            {
                parameters["subject"] = Subject;
            }
            if (IsBound(nameof(Message)))
            {
                parameters["message"] = Message;
            }
            if (IsBound(nameof(TriggeringFilenames)))
            {
                parameters["triggering_filenames"] = RequestValues.Strings(TriggeringFilenames, "TriggeringFilenames");
            }
            if (IsBound(nameof(TriggeringGroupIds)))
            {
                parameters["triggering_group_ids"] = RequestValues.Int64s(TriggeringGroupIds);
            }
            if (IsBound(nameof(TriggeringUserIds)))
            {
                parameters["triggering_user_ids"] = RequestValues.Int64s(TriggeringUserIds);
            }
            if (IsBound(nameof(TriggerByShareRecipients)))
            {
                parameters["trigger_by_share_recipients"] = TriggerByShareRecipients.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Notification.
    /// </summary>
    [Cmdlet("Remove", "FilesNotification", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesNotificationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Notification ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The notification to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Notification InputObject { get; set; }

        /// <summary>Delete Notification.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Notification {id}"))
            {
                return;
            }
            Run(connection, token => NotificationBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
