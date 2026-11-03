using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A History Export is a resource on the API that is used to export historical action (history) logs.
    /// </summary>
    public sealed class HistoryExport : FilesObject
    {
        internal HistoryExport()
        {
        }

        /// <summary>
        /// Workspace of the export. 0 represents the default workspace. A null value means a site-wide export.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// History Export ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Version of the history for the export.
        /// </summary>
        public string HistoryVersion { get; internal set; }

        /// <summary>
        /// Start date/time of export range.
        /// </summary>
        public DateTime? StartAt { get; internal set; }

        /// <summary>
        /// End date/time of export range.
        /// </summary>
        public DateTime? EndAt { get; internal set; }

        /// <summary>
        /// Status of export.  Will be: `building`, `ready`, or `failed`
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Filter results by this this action type. Valid values: `create`, `read`, `update`, `destroy`, `move`, `login`, `failedlogin`, `copy`, `user_create`, `user_update`, `user_destroy`, `group_create`, `group_update`, `group_destroy`, `permission_create`, `permission_destroy`, `api_key_create`, `api_key_update`, `api_key_destroy`, `archived_delete`
        /// </summary>
        public string QueryAction { get; internal set; }

        /// <summary>
        /// Filter results by this this interface type. Valid values: `web`, `ftp`, `robot`, `jsapi`, `webdesktopapi`, `sftp`, `dav`, `desktop`, `restapi`, `scim`, `office`, `mobile`, `as2`, `inbound_email`, `remote`, `inbound_s3`
        /// </summary>
        public string QueryInterface { get; internal set; }

        /// <summary>
        /// Return results that are actions performed by the user indicated by this User ID
        /// </summary>
        public string QueryUserId { get; internal set; }

        /// <summary>
        /// Return results that are file actions related to the file indicated by this File ID
        /// </summary>
        public string QueryFileId { get; internal set; }

        /// <summary>
        /// Return results that are file actions inside the parent folder specified by this folder ID
        /// </summary>
        public string QueryParentId { get; internal set; }

        /// <summary>
        /// Return results that are file actions related to paths matching this pattern.
        /// </summary>
        public string QueryPath { get; internal set; }

        /// <summary>
        /// Return results that are file actions related to files or folders inside folder paths matching this pattern.
        /// </summary>
        public string QueryFolder { get; internal set; }

        /// <summary>
        /// Return results that are file moves originating from paths matching this pattern.
        /// </summary>
        public string QuerySrc { get; internal set; }

        /// <summary>
        /// Return results that are file moves with paths matching this pattern as destination.
        /// </summary>
        public string QueryDestination { get; internal set; }

        /// <summary>
        /// Filter results by this IP address.
        /// </summary>
        public string QueryIp { get; internal set; }

        /// <summary>
        /// Filter results by this username.
        /// </summary>
        public string QueryUsername { get; internal set; }

        /// <summary>
        /// If searching for Histories about login failures, this parameter restricts results to failures of this specific type.  Valid values: `expired_trial`, `account_overdue`, `locked_out`, `ip_mismatch`, `password_mismatch`, `site_mismatch`, `username_not_found`, `none`, `no_ftp_permission`, `no_web_permission`, `no_directory`, `errno_enoent`, `no_sftp_permission`, `no_dav_permission`, `no_restapi_permission`, `key_mismatch`, `region_mismatch`, `expired_access`, `desktop_ip_mismatch`, `desktop_api_key_not_used_quickly_enough`, `disabled`, `country_mismatch`, `insecure_ftp`, `insecure_cipher`, `rate_limited`, `no_s3_compatible_endpoint_permission`, `mobile_ip_mismatch`, `mobile_api_key_not_used_quickly_enough`, `desktop_app_disabled`, `mobile_app_disabled`
        /// </summary>
        public string QueryFailureType { get; internal set; }

        /// <summary>
        /// If searching for Histories about specific objects (such as Users, or API Keys), this parameter restricts results to objects that match this ID.
        /// </summary>
        public string QueryTargetId { get; internal set; }

        /// <summary>
        /// If searching for Histories about Users, Groups or other objects with names, this parameter restricts results to objects with this name/username.
        /// </summary>
        public string QueryTargetName { get; internal set; }

        /// <summary>
        /// If searching for Histories about Permissions, this parameter restricts results to permissions of this level.
        /// </summary>
        public string QueryTargetPermission { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this parameter restricts results to API keys created by/for this user ID.
        /// </summary>
        public string QueryTargetUserId { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this parameter restricts results to API keys created by/for this username.
        /// </summary>
        public string QueryTargetUsername { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this parameter restricts results to API keys associated with this platform.
        /// </summary>
        public string QueryTargetPlatform { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this parameter restricts results to API keys with this permission set.
        /// </summary>
        public string QueryTargetPermissionSet { get; internal set; }

        /// <summary>
        /// If `status` is `ready`, this will be a URL where all the results can be downloaded at once as a CSV.
        /// </summary>
        public string ResultsUrl { get; internal set; }
    }
}
