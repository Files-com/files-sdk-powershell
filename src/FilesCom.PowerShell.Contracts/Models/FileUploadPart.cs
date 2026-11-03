using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A FileUploadPart is a result of a begin upload operation which is the first step in our upload process.
    /// </summary>
    public sealed class FileUploadPart : FilesObject
    {
        internal FileUploadPart()
        {
        }

        /// <summary>
        /// Content-Type and File to send
        /// </summary>
        public object Send { get; internal set; }

        /// <summary>
        /// Type of upload
        /// </summary>
        public string Action { get; internal set; }

        /// <summary>
        /// If `true`, this file exists and you may wish to ask the user for overwrite confirmation
        /// </summary>
        public bool AskAboutOverwrites { get; internal set; }

        /// <summary>
        /// Number of parts in the upload
        /// </summary>
        public long? AvailableParts { get; internal set; }

        /// <summary>
        /// Date/time of when this Upload part expires and the URL cannot be used any more
        /// </summary>
        public string Expires { get; internal set; }

        /// <summary>
        /// Additional upload headers to provide as part of the upload
        /// </summary>
        public object Headers { get; internal set; }

        /// <summary>
        /// HTTP Method to use for uploading the part, usually `PUT`
        /// </summary>
        public string HttpMethod { get; internal set; }

        /// <summary>
        /// Size in bytes for this part
        /// </summary>
        public long? NextPartsize { get; internal set; }

        /// <summary>
        /// If `true`, multiple parts may be uploaded in parallel.  If `false`, be sure to only upload one part at a time, in order.
        /// </summary>
        public bool ParallelParts { get; internal set; }

        /// <summary>
        /// If `true`, parts may be retried. If `false`, a part cannot be retried and the upload should be restarted.
        /// </summary>
        public bool RetryParts { get; internal set; }

        /// <summary>
        /// Additional HTTP parameters to send with the upload
        /// </summary>
        public object Parameters { get; internal set; }

        /// <summary>
        /// Number of this upload part
        /// </summary>
        public long? PartNumber { get; internal set; }

        /// <summary>
        /// Size in bytes for the next upload part
        /// </summary>
        public long? Partsize { get; internal set; }

        /// <summary>
        /// New file path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Reference name for this upload part
        /// </summary>
        public string Ref { get; internal set; }

        /// <summary>
        /// URI to upload this part to
        /// </summary>
        public string UploadUri { get; internal set; }

        /// <summary>
        /// Optional direct connection information for direct Agent transfer attempts
        /// </summary>
        public Models.DirectConnectionInfo DirectConnectionInfo { get; internal set; }
    }
}
