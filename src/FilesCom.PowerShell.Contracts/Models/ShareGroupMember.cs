using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A ShareGroupMember is a single user record that belongs to a ShareGroup.
    /// </summary>
    public sealed class ShareGroupMember : FilesObject
    {
        internal ShareGroupMember()
        {
        }

        /// <summary>
        /// Name of the share group member
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Company of the share group member
        /// </summary>
        public string Company { get; internal set; }

        /// <summary>
        /// Email of the share group member
        /// </summary>
        public string Email { get; internal set; }
    }
}
