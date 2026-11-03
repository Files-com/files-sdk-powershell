using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A UserLifecycleRule represents a rule that applies to users based on their inactivity, state and authentication method.
    /// </summary>
    public sealed class UserLifecycleRule : FilesObject
    {
        internal UserLifecycleRule()
        {
        }

        /// <summary>
        /// User Lifecycle Rule ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// User authentication method for which the rule will apply. Use `all_non_sso` to target every non-SSO authentication method with one rule.
        /// </summary>
        public string AuthenticationMethod { get; internal set; }

        /// <summary>
        /// Array of Group IDs to which the rule applies. If empty or not set, the rule applies to all users.
        /// </summary>
        public long?[] GroupIds { get; internal set; }

        /// <summary>
        /// Action to take on inactive users (disable or delete)
        /// </summary>
        public string Action { get; internal set; }

        /// <summary>
        /// Number of days of inactivity before the rule applies
        /// </summary>
        public long? InactivityDays { get; internal set; }

        /// <summary>
        /// If true, the rule will apply to folder admins.
        /// </summary>
        public bool IncludeFolderAdmins { get; internal set; }

        /// <summary>
        /// If true, the rule includes Site Administrators, who always belong to the Default Workspace. Can only be enabled when `workspace_id` is `0`.
        /// </summary>
        public bool IncludeSiteAdmins { get; internal set; }

        /// <summary>
        /// If true, a Default Workspace rule also applies to users in all Custom Workspaces. Can only be enabled when `workspace_id` is `0`.
        /// </summary>
        public bool ApplyToAllWorkspaces { get; internal set; }

        /// <summary>
        /// User Lifecycle Rule name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// If true, users will be emailed before the rule disables or deletes them.
        /// </summary>
        public bool NotifyUsers { get; internal set; }

        /// <summary>
        /// If provided, only users belonging to Partners with this tag at the Partner level will be affected by the rule. Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        public string PartnerTag { get; internal set; }

        /// <summary>
        /// Site ID
        /// </summary>
        public long? SiteId { get; internal set; }

        /// <summary>
        /// Workspace whose users the rule applies to. `0` means the Default Workspace. A Custom Workspace rule applies only to users who belong to that Workspace, regardless of access granted to other users.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// State of the users to apply the rule to (inactive or disabled)
        /// </summary>
        public string UserState { get; internal set; }

        /// <summary>
        /// If provided, only users with this tag will be affected by the rule. Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        public string UserTag { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(UserLifecycleRule source)
        {
            Id = source.Id;
            AuthenticationMethod = source.AuthenticationMethod;
            GroupIds = source.GroupIds;
            Action = source.Action;
            InactivityDays = source.InactivityDays;
            IncludeFolderAdmins = source.IncludeFolderAdmins;
            IncludeSiteAdmins = source.IncludeSiteAdmins;
            ApplyToAllWorkspaces = source.ApplyToAllWorkspaces;
            Name = source.Name;
            NotifyUsers = source.NotifyUsers;
            PartnerTag = source.PartnerTag;
            SiteId = source.SiteId;
            WorkspaceId = source.WorkspaceId;
            UserState = source.UserState;
            UserTag = source.UserTag;
        }
    }
}
