using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An Action is a single record in our history log.
    /// </summary>
    public sealed class HistoryAction : FilesObject
    {
        internal HistoryAction()
        {
        }

        /// <summary>
        /// Action ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Action occurrence date/time
        /// </summary>
        public DateTime? When { get; internal set; }

        /// <summary>
        /// The destination path for this action, if applicable
        /// </summary>
        public string Destination { get; internal set; }

        /// <summary>
        /// Friendly displayed output
        /// </summary>
        public string Display { get; internal set; }

        /// <summary>
        /// IP Address that performed this action
        /// </summary>
        public string Ip { get; internal set; }

        /// <summary>
        /// The source path for this action, if applicable
        /// </summary>
        public string Source { get; internal set; }

        /// <summary>
        /// Targets
        /// </summary>
        public object Targets { get; internal set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// true if this change was performed by a user on a parent site.
        /// </summary>
        public bool UserIsFromParentSite { get; internal set; }

        /// <summary>
        /// Type of action
        /// </summary>
        public string ActionType { get; internal set; }

        /// <summary>
        /// Failure type.  If action was a user login or session failure, why did it fail?
        /// </summary>
        public string FailureType { get; internal set; }

        /// <summary>
        /// Interface on which this action occurred.
        /// </summary>
        public string Interface { get; internal set; }
    }
}
