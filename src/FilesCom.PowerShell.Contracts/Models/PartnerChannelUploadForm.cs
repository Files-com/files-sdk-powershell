using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Partner Channel Upload Form attaches a Form Field Set to one From Partner upload destination.
    /// </summary>
    public sealed class PartnerChannelUploadForm : FilesObject
    {
        internal PartnerChannelUploadForm()
        {
        }

        /// <summary>
        /// Upload destination path relative to the From Partner folder. Must be a configured leaf managed folder path, or an empty string for the From Partner folder when no managed folders are configured. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// ID of the Form Field Set answered for uploads to this destination. Must be in the same Workspace and contain at least one custom field.
        /// </summary>
        public long? FormFieldSetId { get; internal set; }
    }
}
