using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Permission object represents a grant of access permission on a specific Path to a User or Group.
    /// </summary>
    public sealed class Permission : FilesObject
    {
        internal Permission()
        {
        }

        /// <summary>
        /// Permission ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Folder path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Username (if applicable)
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Group ID
        /// </summary>
        public long? GroupId { get; internal set; }

        /// <summary>
        /// Group name (if applicable)
        /// </summary>
        public string GroupName { get; internal set; }

        /// <summary>
        /// Group IDs when this Permission requires membership in every listed group.
        /// </summary>
        public long?[] GroupIds { get; internal set; }

        /// <summary>
        /// Group names when this permission requires multiple groups
        /// </summary>
        public string[] GroupNames { get; internal set; }

        /// <summary>
        /// Partner ID (if applicable)
        /// </summary>
        public long? PartnerId { get; internal set; }

        /// <summary>
        /// Partner name (if applicable)
        /// </summary>
        public string PartnerName { get; internal set; }

        /// <summary>
        /// Permission type.  See the table referenced in the documentation for an explanation of each permission.
        /// </summary>
        public string PermissionType { get; internal set; }

        /// <summary>
        /// Whether this Permission applies to subfolders. Must be true for `admin` Permissions.
        /// </summary>
        public bool Recursive { get; internal set; }

        /// <summary>
        /// Site ID
        /// </summary>
        public long? SiteId { get; internal set; }
    }
}
