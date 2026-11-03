using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Lock can be used by your custom-developed applications to implement file locking and concurrency features.
    /// </summary>
    public sealed class Lock : FilesObject
    {
        internal Lock()
        {
        }

        /// <summary>
        /// Path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Lock timeout in seconds
        /// </summary>
        public long? Timeout { get; internal set; }

        /// <summary>
        /// Depth
        /// </summary>
        public string Depth { get; internal set; }

        /// <summary>
        /// Does lock apply to subfolders?
        /// </summary>
        public bool Recursive { get; internal set; }

        /// <summary>
        /// Arbitrary descriptive label for the lock. Does not change the lock creator or permissions.
        /// </summary>
        public string Owner { get; internal set; }

        /// <summary>
        /// Scope
        /// </summary>
        public string Scope { get; internal set; }

        /// <summary>
        /// Is lock exclusive?
        /// </summary>
        public bool Exclusive { get; internal set; }

        /// <summary>
        /// Lock token.  Use to release lock.
        /// </summary>
        public string Token { get; internal set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// Can lock be modified by users other than its creator?
        /// </summary>
        public bool AllowAccessByAnyUser { get; internal set; }

        /// <summary>
        /// Lock creator user ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Lock creator username
        /// </summary>
        public string Username { get; internal set; }
    }
}
