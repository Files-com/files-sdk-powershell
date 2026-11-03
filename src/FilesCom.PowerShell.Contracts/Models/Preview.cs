using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Preview is an image or PDF preview that is meant to be viewed within the web interface.
    /// </summary>
    public sealed class Preview : FilesObject
    {
        internal Preview()
        {
        }

        /// <summary>
        /// Preview ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Preview status.  Can be invalid, not_generated, generating, complete, or file_too_large
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Link to download preview
        /// </summary>
        public string DownloadUri { get; internal set; }

        /// <summary>
        /// Preview type. Can be image, pdf, pdf_native, video, audio, or text
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// Preview size
        /// </summary>
        public string Size { get; internal set; }
    }
}
