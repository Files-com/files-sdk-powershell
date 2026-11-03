using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An ActionLog is an audit log of file actions performed by users or the system.
    /// </summary>
    public sealed class ActionLog : FilesObject
    {
        internal ActionLog()
        {
        }

        /// <summary>
        /// The type of action performed
        /// </summary>
        public string Action { get; internal set; }

        /// <summary>
        /// Time the action was performed
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Destination path, for moves and copies
        /// </summary>
        public string Destination { get; internal set; }

        /// <summary>
        /// Failure type, if applicable
        /// </summary>
        public string FailureType { get; internal set; }

        /// <summary>
        /// Folder involved in the action
        /// </summary>
        public string Folder { get; internal set; }

        /// <summary>
        /// Interface used to perform the action
        /// </summary>
        public string Interface { get; internal set; }

        /// <summary>
        /// IP address from which the action was performed
        /// </summary>
        public string Ip { get; internal set; }

        /// <summary>
        /// File or Folder ID associated with the action
        /// </summary>
        public long? MetadataDmId { get; internal set; }

        /// <summary>
        /// Parent File or Folder ID associated with the action
        /// </summary>
        public long? ParentMetadataDmId { get; internal set; }

        /// <summary>
        /// File path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Site ID where the action took place
        /// </summary>
        public long? SiteId { get; internal set; }

        /// <summary>
        /// Source path, for moves and copies
        /// </summary>
        public string Src { get; internal set; }

        /// <summary>
        /// User ID who performed the action
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Username who performed the action
        /// </summary>
        public string Username { get; internal set; }
    }
}
