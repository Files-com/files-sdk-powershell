using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com zip list entry.
    /// </summary>
    public sealed class ZipListEntry : FilesObject
    {
        internal ZipListEntry()
        {
        }

        /// <summary>
        /// Entry path inside the ZIP. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Uncompressed size in bytes.
        /// </summary>
        public long? Size { get; internal set; }
    }
}
