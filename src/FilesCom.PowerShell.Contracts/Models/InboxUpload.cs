using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An InboxUpload is a log record about upload operations that happened in your Inbox.
    /// </summary>
    public sealed class InboxUpload : FilesObject
    {
        internal InboxUpload()
        {
        }

        /// <summary>
        /// InboxRegistration
        /// </summary>
        public Models.InboxRegistration InboxRegistration { get; internal set; }

        /// <summary>
        /// Upload path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Upload date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
