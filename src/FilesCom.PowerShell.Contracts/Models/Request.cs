using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Request is a file that *should* be uploaded by a specific user or group.
    /// </summary>
    public sealed class Request : FilesObject
    {
        internal Request()
        {
        }

        /// <summary>
        /// Request ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Folder path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Source filename, if applicable
        /// </summary>
        public string Source { get; internal set; }

        /// <summary>
        /// Destination filename
        /// </summary>
        public string Destination { get; internal set; }

        /// <summary>
        /// ID of automation that created request
        /// </summary>
        public long? AutomationId { get; internal set; }

        /// <summary>
        /// User making the request (if applicable)
        /// </summary>
        public string UserDisplayName { get; internal set; }
    }
}
