using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Share Link Notifications, or get one bundle notification by Id.
    /// </summary>
    [Cmdlet("Get", "FilesBundleNotification", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.BundleNotification), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.BundleNotification>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.BundleNotification), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesBundleNotificationCommand : FilesListCmdlet<Models.BundleNotification>
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id` and `bundle_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `bundle_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Bundle ID
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? BundleId { get; set; }

        /// <summary>
        /// Bundle Notification ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Share Link Notifications, or get one bundle notification by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.BundleNotification result = Run(connection, token => BundleNotificationBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => BundleNotificationBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
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
            if (IsBound(nameof(BundleId)))
            {
                parameters["bundle_id"] = BundleId.Value;
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
    /// Create Share Link Notification.
    /// </summary>
    [Cmdlet("New", "FilesBundleNotification", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.BundleNotification))]
    public sealed class NewFilesBundleNotificationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Bundle ID to notify on
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long BundleId { get; set; }

        /// <summary>
        /// The id of the user to notify.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? NotifyUserId { get; set; }

        /// <summary>
        /// Triggers bundle notification when a registration action occurs for it.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnRegistration { get; set; }

        /// <summary>
        /// Triggers bundle notification when a upload action occurs for it.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnUpload { get; set; }

        /// <summary>Create Share Link Notification.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Bundle notification"))
            {
                return;
            }
            Models.BundleNotification result = Run(connection, token => BundleNotificationBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(BundleId)))
            {
                parameters["bundle_id"] = BundleId;
            }
            if (IsBound(nameof(NotifyUserId)))
            {
                parameters["notify_user_id"] = NotifyUserId.Value;
            }
            if (IsBound(nameof(NotifyOnRegistration)))
            {
                parameters["notify_on_registration"] = NotifyOnRegistration.Value;
            }
            if (IsBound(nameof(NotifyOnUpload)))
            {
                parameters["notify_on_upload"] = NotifyOnUpload.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Share Link Notification.
    /// </summary>
    [Cmdlet("Set", "FilesBundleNotification", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.BundleNotification), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesBundleNotificationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Bundle Notification ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Triggers bundle notification when a registration action occurs for it.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnRegistration { get; set; }

        /// <summary>
        /// Triggers bundle notification when a upload action occurs for it.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnUpload { get; set; }

        /// <summary>
        /// The bundle notification to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.BundleNotification InputObject { get; set; }

        /// <summary>Update Share Link Notification.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Bundle notification {id}"))
            {
                return;
            }
            Models.BundleNotification result = Run(connection, token => BundleNotificationBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(NotifyOnRegistration)))
            {
                parameters["notify_on_registration"] = NotifyOnRegistration.Value;
            }
            if (IsBound(nameof(NotifyOnUpload)))
            {
                parameters["notify_on_upload"] = NotifyOnUpload.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Share Link Notification.
    /// </summary>
    [Cmdlet("Remove", "FilesBundleNotification", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesBundleNotificationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Bundle Notification ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The bundle notification to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.BundleNotification InputObject { get; set; }

        /// <summary>Delete Share Link Notification.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Bundle notification {id}"))
            {
                return;
            }
            Run(connection, token => BundleNotificationBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
