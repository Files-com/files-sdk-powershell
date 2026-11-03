using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A GroupUser is a record about membership of a User within a Group.
    /// </summary>
    public sealed class GroupUser : FilesObject
    {
        internal GroupUser()
        {
        }

        /// <summary>
        /// Group name
        /// </summary>
        public string GroupName { get; internal set; }

        /// <summary>
        /// Group ID
        /// </summary>
        public long? GroupId { get; internal set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Is this user an administrator of this group?
        /// </summary>
        public bool Admin { get; internal set; }

        /// <summary>
        /// Username of the user
        /// </summary>
        public string Username { get; internal set; }
    }
}
