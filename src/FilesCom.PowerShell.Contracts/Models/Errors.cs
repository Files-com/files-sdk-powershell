using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com errors.
    /// </summary>
    public sealed class Errors : FilesObject
    {
        internal Errors()
        {
        }

        /// <summary>
        /// A list of fields where errors occur
        /// </summary>
        public string[] Fields { get; internal set; }

        /// <summary>
        /// A list of error messages
        /// </summary>
        public string[] Messages { get; internal set; }
    }
}
