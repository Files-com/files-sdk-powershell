using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A HistoryExportResult is a single record with historical information about actions that happened in your site.
    /// </summary>
    public sealed class HistoryExportResult : FilesObject
    {
        internal HistoryExportResult()
        {
        }

        /// <summary>
        /// Action ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// When the action happened
        /// </summary>
        public long? CreatedAt { get; internal set; }

        /// <summary>
        /// When the action happened, in ISO8601 format.
        /// </summary>
        public string CreatedAtIso8601 { get; internal set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// File ID related to the action
        /// </summary>
        public long? FileId { get; internal set; }

        /// <summary>
        /// ID of the parent folder
        /// </summary>
        public long? ParentId { get; internal set; }

        /// <summary>
        /// Path of the related action. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Folder in which the action occurred
        /// </summary>
        public string Folder { get; internal set; }

        /// <summary>
        /// File move originated from this path
        /// </summary>
        public string Src { get; internal set; }

        /// <summary>
        /// File moved to this destination folder
        /// </summary>
        public string Destination { get; internal set; }

        /// <summary>
        /// Client IP that performed the action
        /// </summary>
        public string Ip { get; internal set; }

        /// <summary>
        /// Username of the user that performed the action
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// true if this change was performed by a user on a parent site.
        /// </summary>
        public bool UserIsFromParentSite { get; internal set; }

        /// <summary>
        /// What action was taken. Valid values: `create`, `read`, `update`, `destroy`, `move`, `login`, `failedlogin`, `copy`, `user_create`, `user_update`, `user_destroy`, `group_create`, `group_update`, `group_destroy`, `permission_create`, `permission_destroy`, `api_key_create`, `api_key_update`, `api_key_destroy`, `archived_delete`
        /// </summary>
        public string Action { get; internal set; }

        /// <summary>
        /// The type of login failure, if applicable.  Valid values: `expired_trial`, `account_overdue`, `locked_out`, `ip_mismatch`, `password_mismatch`, `site_mismatch`, `username_not_found`, `none`, `no_ftp_permission`, `no_web_permission`, `no_directory`, `errno_enoent`, `no_sftp_permission`, `no_dav_permission`, `no_restapi_permission`, `key_mismatch`, `region_mismatch`, `expired_access`, `desktop_ip_mismatch`, `desktop_api_key_not_used_quickly_enough`, `disabled`, `country_mismatch`, `insecure_ftp`, `insecure_cipher`, `rate_limited`, `no_s3_compatible_endpoint_permission`, `mobile_ip_mismatch`, `mobile_api_key_not_used_quickly_enough`, `desktop_app_disabled`, `mobile_app_disabled`
        /// </summary>
        public string FailureType { get; internal set; }

        /// <summary>
        /// Interface through which the action was taken. Valid values: `web`, `ftp`, `robot`, `jsapi`, `webdesktopapi`, `sftp`, `dav`, `desktop`, `restapi`, `scim`, `office`, `mobile`, `as2`, `inbound_email`, `remote`, `inbound_s3`
        /// </summary>
        public string Interface { get; internal set; }

        /// <summary>
        /// ID of the object (such as Users, or API Keys) on which the action was taken
        /// </summary>
        public long? TargetId { get; internal set; }

        /// <summary>
        /// Name of the User, Group or other object with a name related to this action
        /// </summary>
        public string TargetName { get; internal set; }

        /// <summary>
        /// Permission level of the action
        /// </summary>
        public string TargetPermission { get; internal set; }

        /// <summary>
        /// Whether or not the action was recursive
        /// </summary>
        public bool TargetRecursive { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this is when the API key will expire. Represented as a Unix timestamp.
        /// </summary>
        public long? TargetExpiresAt { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this is when the API key will expire. Represented in ISO8601 format.
        /// </summary>
        public string TargetExpiresAtIso8601 { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this represents the permission set of the associated  API key
        /// </summary>
        public string TargetPermissionSet { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this is the platform on which the action was taken
        /// </summary>
        public string TargetPlatform { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this is the username on which the action was taken
        /// </summary>
        public string TargetUsername { get; internal set; }

        /// <summary>
        /// If searching for Histories about API keys, this is the User ID on which the action was taken
        /// </summary>
        public long? TargetUserId { get; internal set; }
    }
}
