using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Show Action Notification Export.
    /// </summary>
    [Cmdlet("Get", "FilesActionNotificationExport")]
    [OutputType(typeof(Models.ActionNotificationExport))]
    public sealed class GetFilesActionNotificationExportCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Action Notification Export ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>Show Action Notification Export.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            long id = Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = FindParameters();
            Models.ActionNotificationExport result = Run(connection, token => ActionNotificationExportBridge.FindAsync(connection, id, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Action Notification Export.
    /// </summary>
    [Cmdlet("New", "FilesActionNotificationExport", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.ActionNotificationExport))]
    public sealed class NewFilesActionNotificationExportCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Workspace whose logs are exported. Set to `0` for the default workspace. A null value means a site-wide export.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Start date/time of export range.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? StartAt { get; set; }

        /// <summary>
        /// End date/time of export range.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? EndAt { get; set; }

        /// <summary>
        /// Error message associated with the request, if any.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryMessage { get; set; }

        /// <summary>
        /// The HTTP request method used by the webhook.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryRequestMethod { get; set; }

        /// <summary>
        /// The target webhook URL.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryRequestUrl { get; set; }

        /// <summary>
        /// The HTTP status returned from the server in response to the webhook request.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryStatus { get; set; }

        /// <summary>
        /// true if the webhook request succeeded (i.e. returned a 200 or 204 response status). false otherwise.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? QuerySuccess { get; set; }

        /// <summary>
        /// Return notifications that were triggered by actions on this specific path.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryPath { get; set; }

        /// <summary>
        /// Return notifications that were triggered by actions in this folder.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryFolder { get; set; }

        /// <summary>Create Action Notification Export.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Action notification export"))
            {
                return;
            }
            Models.ActionNotificationExport result = Run(connection, token => ActionNotificationExportBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(StartAt)))
            {
                parameters["start_at"] = RequestValues.DateTime(StartAt.Value);
            }
            if (IsBound(nameof(EndAt)))
            {
                parameters["end_at"] = RequestValues.DateTime(EndAt.Value);
            }
            if (IsBound(nameof(QueryMessage)))
            {
                parameters["query_message"] = QueryMessage;
            }
            if (IsBound(nameof(QueryRequestMethod)))
            {
                parameters["query_request_method"] = QueryRequestMethod;
            }
            if (IsBound(nameof(QueryRequestUrl)))
            {
                parameters["query_request_url"] = QueryRequestUrl;
            }
            if (IsBound(nameof(QueryStatus)))
            {
                parameters["query_status"] = QueryStatus;
            }
            if (IsBound(nameof(QuerySuccess)))
            {
                parameters["query_success"] = QuerySuccess.Value;
            }
            if (IsBound(nameof(QueryPath)))
            {
                parameters["query_path"] = QueryPath;
            }
            if (IsBound(nameof(QueryFolder)))
            {
                parameters["query_folder"] = QueryFolder;
            }
            return parameters;
        }
    }
}
