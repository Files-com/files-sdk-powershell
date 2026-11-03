using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A SettingsChange is any change in your site caused by any user.
    /// </summary>
    public sealed class SettingsChange : FilesObject
    {
        internal SettingsChange()
        {
        }

        /// <summary>
        /// The API key id responsible for this change.
        /// </summary>
        public long? ApiKeyId { get; internal set; }

        /// <summary>
        /// Markdown-formatted change messages.
        /// </summary>
        public string[] Changes { get; internal set; }

        /// <summary>
        /// The time this change was made.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// The user id responsible for this change.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// true if this change was performed by Files.com support.
        /// </summary>
        public bool UserIsFilesSupport { get; internal set; }

        /// <summary>
        /// true if this change was performed by a user on a parent site.
        /// </summary>
        public bool UserIsFromParentSite { get; internal set; }

        /// <summary>
        /// The username of the user responsible for this change.
        /// </summary>
        public string Username { get; internal set; }
    }
}
