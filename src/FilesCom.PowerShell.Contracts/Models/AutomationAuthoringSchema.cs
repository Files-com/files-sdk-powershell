using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com automation authoring schema.
    /// </summary>
    public sealed class AutomationAuthoringSchema : FilesObject
    {
        internal AutomationAuthoringSchema()
        {
        }

        /// <summary>
        /// JSON Schema for active Automation v2 graph definitions.
        /// </summary>
        public object DefinitionSchema { get; internal set; }

        /// <summary>
        /// Typed error families accepted by Automation v2 on_error rules.
        /// </summary>
        public object[] ErrorFamilies { get; internal set; }

        /// <summary>
        /// Active Automation v2 node authoring metadata.
        /// </summary>
        public object[] Nodes { get; internal set; }

        /// <summary>
        /// Stable public URL for the Automation v2 graph definition JSON Schema.
        /// </summary>
        public string SchemaUrl { get; internal set; }
    }
}
