using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com image.
    /// </summary>
    public sealed class Image : FilesObject
    {
        internal Image()
        {
        }

        /// <summary>
        /// Image name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Image URI
        /// </summary>
        public string Uri { get; internal set; }
    }
}
