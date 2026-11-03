using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A MetadataCategory defines a reusable set of Custom Metadata rules that can be assigned to folders via a folder behavior.
    /// </summary>
    public sealed class MetadataCategory : FilesObject
    {
        internal MetadataCategory()
        {
        }

        /// <summary>
        /// Metadata Category ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Name of the metadata category.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Map of key names to arrays of allowed values. An empty array means free-form text.
        /// </summary>
        public Dictionary<string, string[]> Definitions { get; internal set; }

        /// <summary>
        /// Metadata keys that should appear as columns in the UI by default.
        /// </summary>
        public string[] DefaultColumns { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(MetadataCategory source)
        {
            Id = source.Id;
            Name = source.Name;
            Definitions = source.Definitions;
            DefaultColumns = source.DefaultColumns;
        }
    }
}
