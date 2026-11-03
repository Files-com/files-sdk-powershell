using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com holiday region.
    /// </summary>
    public sealed class HolidayRegion : FilesObject
    {
        internal HolidayRegion()
        {
        }

        /// <summary>
        /// The code representing a region
        /// </summary>
        public string Code { get; internal set; }

        /// <summary>
        /// The name of the region
        /// </summary>
        public string Name { get; internal set; }
    }
}
