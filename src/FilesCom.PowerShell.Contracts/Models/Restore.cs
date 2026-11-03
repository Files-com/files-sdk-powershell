using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Restore kicks off a process to restore deleted data for your Site.
    /// </summary>
    public sealed class Restore : FilesObject
    {
        internal Restore()
        {
        }

        /// <summary>
        /// Restore files or users deleted on or after this date/time. Don't set this earlier than you need. Can not be greater than 365 days prior to the restore request.
        /// </summary>
        public DateTime? EarliestDate { get; internal set; }

        /// <summary>
        /// Restore Record ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Number of directories that were successfully restored.
        /// </summary>
        public long? DirsRestored { get; internal set; }

        /// <summary>
        /// Number of directories that were not able to be restored.
        /// </summary>
        public long? DirsErrored { get; internal set; }

        /// <summary>
        /// Total number of directories processed.
        /// </summary>
        public long? DirsTotal { get; internal set; }

        /// <summary>
        /// Number of files successfully restored.
        /// </summary>
        public long? FilesRestored { get; internal set; }

        /// <summary>
        /// Number of files that were not able to be restored.
        /// </summary>
        public long? FilesErrored { get; internal set; }

        /// <summary>
        /// Total number of files processed.
        /// </summary>
        public long? FilesTotal { get; internal set; }

        /// <summary>
        /// Prefix of the files/folders to restore, or a case-insensitive username prefix for a user restore. A trailing slash selects that folder and its contents only; without it, the prefix matches any path beginning with that text. Do not use a leading slash. To restore all deleted items of the selected restoration type within the selected site or workspace scope, specify an empty string (`''`) or omit the field.
        /// </summary>
        public string Prefix { get; internal set; }

        /// <summary>
        /// Type of restoration to perform. `files` restores deleted filesystem items. `users` restores deleted users and associated authentication records, plus permissions when `restore_deleted_permissions=true`. A 5-minute grace period allows for batch deletion processes: associated records deleted within the 5 minutes before the user are included, even if removed independently. Records deleted earlier, or before `earliest_date`, remain deleted.
        /// </summary>
        public string RestorationType { get; internal set; }

        /// <summary>
        /// If true, we will restore the files in place (into their original paths). If false, we will create a new restoration folder in the root and restore files there.
        /// </summary>
        public bool RestoreInPlace { get; internal set; }

        /// <summary>
        /// If true, a user restore restores permissions deleted along with the selected users, including permissions removed independently within the 5 minutes before user deletion. Permissions deleted earlier, or before `earliest_date`, remain deleted. An in-place file restore restores user and group permissions on returned folders and their subfolders that were deleted from the time of the folder's deletion through 24 hours afterward, including permissions removed independently within that window. File restores into a new folder do not restore permissions.
        /// </summary>
        public bool RestoreDeletedPermissions { get; internal set; }

        /// <summary>
        /// Number of users successfully restored (only present for `restoration_type=users`).
        /// </summary>
        public long? UsersRestored { get; internal set; }

        /// <summary>
        /// Number of users that failed to restore (only present for `restoration_type=users`).
        /// </summary>
        public long? UsersErrored { get; internal set; }

        /// <summary>
        /// Total number of users processed (only present for `restoration_type=users`).
        /// </summary>
        public long? UsersTotal { get; internal set; }

        /// <summary>
        /// Number of API keys restored (only present for `restoration_type=users`).
        /// </summary>
        public long? ApiKeysRestored { get; internal set; }

        /// <summary>
        /// Number of public keys restored (only present for `restoration_type=users`).
        /// </summary>
        public long? PublicKeysRestored { get; internal set; }

        /// <summary>
        /// Number of two factor authentication methods restored (only present for `restoration_type=users`).
        /// </summary>
        public long? TwoFactorAuthenticationMethodsRestored { get; internal set; }

        /// <summary>
        /// Status of the restoration process.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// If true, we will update the last modified timestamp of restored files to today's date. If false, we might trigger File Expiration to delete the file again.
        /// </summary>
        public bool UpdateTimestamps { get; internal set; }

        /// <summary>
        /// Workspace ID for a workspace-scoped restore. `0` means the default site-wide scope.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Error messages received while restoring files and/or directories. Only present if there were errors.
        /// </summary>
        public string[] ErrorMessages { get; internal set; }
    }
}
