using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com auto.
    /// </summary>
    public sealed class Auto : FilesObject
    {
        internal Auto()
        {
        }

        /// <summary>
        /// Dynamic
        /// </summary>
        public object Dynamic { get; internal set; }
    }
}
