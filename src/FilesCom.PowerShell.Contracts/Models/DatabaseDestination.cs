using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Database Destination lets an Automation append rows to an existing table.
    /// </summary>
    public sealed class DatabaseDestination : FilesObject
    {
        internal DatabaseDestination()
        {
        }

        /// <summary>
        /// Database Destination ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Name of the destination.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Database provider.
        /// </summary>
        public string Provider { get; internal set; }

        /// <summary>
        /// Provider settings for the destination. Use the schema matching provider. The connection address and table are fixed after creation.
        /// </summary>
        public object Config { get; internal set; }

        /// <summary>
        /// ID of a Secret in the same Workspace. Use a Token Secret for Snowflake, a service account Key/Value Secret for BigQuery, or a Username/Password Secret for a database login.
        /// </summary>
        public long? SecretId { get; internal set; }

        /// <summary>
        /// When the destination was created.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// When the destination was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(DatabaseDestination source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Name = source.Name;
            Provider = source.Provider;
            Config = source.Config;
            SecretId = source.SecretId;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
