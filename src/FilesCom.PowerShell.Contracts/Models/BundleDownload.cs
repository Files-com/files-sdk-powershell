using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A BundleDownload is a record of the download action that happened in the bundle.
    /// </summary>
    public sealed class BundleDownload : FilesObject
    {
        internal BundleDownload()
        {
        }

        /// <summary>
        /// BundleRegistration
        /// </summary>
        public Models.BundleRegistration BundleRegistration { get; internal set; }

        /// <summary>
        /// Download method (file or full_zip)
        /// </summary>
        public string DownloadMethod { get; internal set; }

        /// <summary>
        /// Download path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Download date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
