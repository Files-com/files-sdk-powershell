using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Workspace is a lightweight way to organize related resources inside a single Files.com Site.
    /// </summary>
    public sealed class Workspace : FilesObject
    {
        internal Workspace()
        {
        }

        /// <summary>
        /// Workspace ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace name
        /// </summary>
        public string Name { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Workspace source)
        {
            Id = source.Id;
            Name = source.Name;
        }
    }
}
