using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Secret stores named, typed secret material for later use by features that reference the Secret by ID.
    /// </summary>
    public sealed class Secret : FilesObject
    {
        internal Secret()
        {
        }

        /// <summary>
        /// Secret ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Secret name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Internal description for your reference.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Secret type.
        /// </summary>
        public string SecretType { get; internal set; }

        /// <summary>
        /// Non-secret metadata for the Secret type.
        /// </summary>
        public object Metadata { get; internal set; }

        /// <summary>
        /// Names of configured secret value fields. Secret values are never returned.
        /// </summary>
        public string[] ValueFieldNames { get; internal set; }

        /// <summary>
        /// Secret create date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Secret update date/time.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Secret source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Name = source.Name;
            Description = source.Description;
            SecretType = source.SecretType;
            Metadata = source.Metadata;
            ValueFieldNames = source.ValueFieldNames;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
