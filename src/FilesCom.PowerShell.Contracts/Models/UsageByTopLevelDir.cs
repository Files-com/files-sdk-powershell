using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// Storage Usage - map of root folders to their usage as of end date (not necessarily high water mark, which is used for billing)
    /// </summary>
    public sealed class UsageByTopLevelDir : FilesObject
    {
        internal UsageByTopLevelDir()
        {
        }

        /// <summary>
        /// Directory name
        /// </summary>
        public string Dir { get; internal set; }

        /// <summary>
        /// Usage
        /// </summary>
        public long? Size { get; internal set; }

        /// <summary>
        /// File count
        /// </summary>
        public long? Count { get; internal set; }
    }
}
