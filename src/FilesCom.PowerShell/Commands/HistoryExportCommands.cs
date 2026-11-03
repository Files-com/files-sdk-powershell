using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Show History Export.
    /// </summary>
    [Cmdlet("Get", "FilesHistoryExport")]
    [OutputType(typeof(Models.HistoryExport))]
    public sealed class GetFilesHistoryExportCommand : FilesApiCmdlet
    {
        /// <summary>
        /// History Export ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>Show History Export.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            long id = Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = FindParameters();
            Models.HistoryExport result = Run(connection, token => HistoryExportBridge.FindAsync(connection, id, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create History Export.
    /// </summary>
    [Cmdlet("New", "FilesHistoryExport", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.HistoryExport))]
    public sealed class NewFilesHistoryExportCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

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
        /// Filter results by this this action type. Valid values: `create`, `read`, `update`, `destroy`, `move`, `login`, `failedlogin`, `copy`, `user_create`, `user_update`, `user_destroy`, `group_create`, `group_update`, `group_destroy`, `permission_create`, `permission_destroy`, `api_key_create`, `api_key_update`, `api_key_destroy`, `archived_delete`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryAction { get; set; }

        /// <summary>
        /// Filter results by this this interface type. Valid values: `web`, `ftp`, `robot`, `jsapi`, `webdesktopapi`, `sftp`, `dav`, `desktop`, `restapi`, `scim`, `office`, `mobile`, `as2`, `inbound_email`, `remote`, `inbound_s3`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryInterface { get; set; }

        /// <summary>
        /// Return results that are actions performed by the user indicated by this User ID
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryUserId { get; set; }

        /// <summary>
        /// Return results that are file actions related to the file indicated by this File ID
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryFileId { get; set; }

        /// <summary>
        /// Return results that are file actions inside the parent folder specified by this folder ID
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryParentId { get; set; }

        /// <summary>
        /// Return results that are file actions related to paths matching this pattern.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryPath { get; set; }

        /// <summary>
        /// Return results that are file actions related to files or folders inside folder paths matching this pattern.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryFolder { get; set; }

        /// <summary>
        /// Return results that are file moves originating from paths matching this pattern.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QuerySrc { get; set; }

        /// <summary>
        /// Return results that are file moves with paths matching this pattern as destination.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryDestination { get; set; }

        /// <summary>
        /// Filter results by this IP address.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryIp { get; set; }

        /// <summary>
        /// Filter results by this username.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryUsername { get; set; }

        /// <summary>
        /// If searching for Histories about login failures, this parameter restricts results to failures of this specific type.  Valid values: `expired_trial`, `account_overdue`, `locked_out`, `ip_mismatch`, `password_mismatch`, `site_mismatch`, `username_not_found`, `none`, `no_ftp_permission`, `no_web_permission`, `no_directory`, `errno_enoent`, `no_sftp_permission`, `no_dav_permission`, `no_restapi_permission`, `key_mismatch`, `region_mismatch`, `expired_access`, `desktop_ip_mismatch`, `desktop_api_key_not_used_quickly_enough`, `disabled`, `country_mismatch`, `insecure_ftp`, `insecure_cipher`, `rate_limited`, `no_s3_compatible_endpoint_permission`, `mobile_ip_mismatch`, `mobile_api_key_not_used_quickly_enough`, `desktop_app_disabled`, `mobile_app_disabled`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryFailureType { get; set; }

        /// <summary>
        /// If searching for Histories about specific objects (such as Users, or API Keys), this parameter restricts results to objects that match this ID.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryTargetId { get; set; }

        /// <summary>
        /// If searching for Histories about Users, Groups or other objects with names, this parameter restricts results to objects with this name/username.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryTargetName { get; set; }

        /// <summary>
        /// If searching for Histories about Permissions, this parameter restricts results to permissions of this level.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryTargetPermission { get; set; }

        /// <summary>
        /// If searching for Histories about API keys, this parameter restricts results to API keys created by/for this user ID.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryTargetUserId { get; set; }

        /// <summary>
        /// If searching for Histories about API keys, this parameter restricts results to API keys created by/for this username.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryTargetUsername { get; set; }

        /// <summary>
        /// If searching for Histories about API keys, this parameter restricts results to API keys associated with this platform.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryTargetPlatform { get; set; }

        /// <summary>
        /// If searching for Histories about API keys, this parameter restricts results to API keys with this permission set.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QueryTargetPermissionSet { get; set; }

        /// <summary>Create History Export.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "History export"))
            {
                return;
            }
            Models.HistoryExport result = Run(connection, token => HistoryExportBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(StartAt)))
            {
                parameters["start_at"] = RequestValues.DateTime(StartAt.Value);
            }
            if (IsBound(nameof(EndAt)))
            {
                parameters["end_at"] = RequestValues.DateTime(EndAt.Value);
            }
            if (IsBound(nameof(QueryAction)))
            {
                parameters["query_action"] = QueryAction;
            }
            if (IsBound(nameof(QueryInterface)))
            {
                parameters["query_interface"] = QueryInterface;
            }
            if (IsBound(nameof(QueryUserId)))
            {
                parameters["query_user_id"] = QueryUserId;
            }
            if (IsBound(nameof(QueryFileId)))
            {
                parameters["query_file_id"] = QueryFileId;
            }
            if (IsBound(nameof(QueryParentId)))
            {
                parameters["query_parent_id"] = QueryParentId;
            }
            if (IsBound(nameof(QueryPath)))
            {
                parameters["query_path"] = QueryPath;
            }
            if (IsBound(nameof(QueryFolder)))
            {
                parameters["query_folder"] = QueryFolder;
            }
            if (IsBound(nameof(QuerySrc)))
            {
                parameters["query_src"] = QuerySrc;
            }
            if (IsBound(nameof(QueryDestination)))
            {
                parameters["query_destination"] = QueryDestination;
            }
            if (IsBound(nameof(QueryIp)))
            {
                parameters["query_ip"] = QueryIp;
            }
            if (IsBound(nameof(QueryUsername)))
            {
                parameters["query_username"] = QueryUsername;
            }
            if (IsBound(nameof(QueryFailureType)))
            {
                parameters["query_failure_type"] = QueryFailureType;
            }
            if (IsBound(nameof(QueryTargetId)))
            {
                parameters["query_target_id"] = QueryTargetId;
            }
            if (IsBound(nameof(QueryTargetName)))
            {
                parameters["query_target_name"] = QueryTargetName;
            }
            if (IsBound(nameof(QueryTargetPermission)))
            {
                parameters["query_target_permission"] = QueryTargetPermission;
            }
            if (IsBound(nameof(QueryTargetUserId)))
            {
                parameters["query_target_user_id"] = QueryTargetUserId;
            }
            if (IsBound(nameof(QueryTargetUsername)))
            {
                parameters["query_target_username"] = QueryTargetUsername;
            }
            if (IsBound(nameof(QueryTargetPlatform)))
            {
                parameters["query_target_platform"] = QueryTargetPlatform;
            }
            if (IsBound(nameof(QueryTargetPermissionSet)))
            {
                parameters["query_target_permission_set"] = QueryTargetPermissionSet;
            }
            return parameters;
        }
    }
}
