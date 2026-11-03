using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A BundlePath is a single bundlepath in the associated Bundle.
    /// </summary>
    public sealed class BundlePath : FilesObject
    {
        internal BundlePath()
        {
        }

        /// <summary>
        /// Allow access to subfolders content?
        /// </summary>
        public bool Recursive { get; internal set; }

        /// <summary>
        /// The path to the resource relative to filesystem. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }
    }
}
