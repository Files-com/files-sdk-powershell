using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A PartnerChannelTemplate defines reusable Partner Channel configuration that can be applied to Partners.
    /// </summary>
    public sealed class PartnerChannelTemplate : FilesObject
    {
        internal PartnerChannelTemplate()
        {
        }

        /// <summary>
        /// The unique ID of the Partner Channel Template.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the Workspace associated with this Partner Channel Template.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Channel directions. `two_way` enables both directions, `to_partner` enables outgoing downloads, and `from_partner` enables incoming uploads.
        /// </summary>
        public string Direction { get; internal set; }

        /// <summary>
        /// Use the Channel folder directly for a one-way exchange. Defaults to false. Cannot be changed after creation. Folder name overrides must be blank when enabled, and the Template must remain one-way.
        /// </summary>
        public bool UseChannelRoot { get; internal set; }

        /// <summary>
        /// The name of the Partner Channel Template.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Channel path relative to the Partner root folder. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Optional Channel-level to-Partner folder name override.
        /// </summary>
        public string ToPartnerFolderName { get; internal set; }

        /// <summary>
        /// Optional Channel-level from-Partner folder name override.
        /// </summary>
        public string FromPartnerFolderName { get; internal set; }

        /// <summary>
        /// Optional route path pattern for files uploaded by the Partner. Supports {{partner_name}}.
        /// </summary>
        public string FromPartnerRoutePathPattern { get; internal set; }

        /// <summary>
        /// Optional route path pattern for files delivered to the Partner. Supports {{partner_name}}.
        /// </summary>
        public string ToPartnerRoutePathPattern { get; internal set; }

        /// <summary>
        /// Managed folder paths inside the to-Partner folder.
        /// </summary>
        public string[] ToPartnerManagedFolderPaths { get; internal set; }

        /// <summary>
        /// Managed folder paths inside the from-Partner folder.
        /// </summary>
        public string[] FromPartnerManagedFolderPaths { get; internal set; }

        /// <summary>
        /// Resolved to-Partner subfolder name. Null when the direction is disabled or uses the Channel folder directly.
        /// </summary>
        public string EffectiveToPartnerFolderName { get; internal set; }

        /// <summary>
        /// Resolved from-Partner subfolder name. Null when the direction is disabled or uses the Channel folder directly.
        /// </summary>
        public string EffectiveFromPartnerFolderName { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(PartnerChannelTemplate source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Direction = source.Direction;
            UseChannelRoot = source.UseChannelRoot;
            Name = source.Name;
            Path = source.Path;
            ToPartnerFolderName = source.ToPartnerFolderName;
            FromPartnerFolderName = source.FromPartnerFolderName;
            FromPartnerRoutePathPattern = source.FromPartnerRoutePathPattern;
            ToPartnerRoutePathPattern = source.ToPartnerRoutePathPattern;
            ToPartnerManagedFolderPaths = source.ToPartnerManagedFolderPaths;
            FromPartnerManagedFolderPaths = source.FromPartnerManagedFolderPaths;
            EffectiveToPartnerFolderName = source.EffectiveToPartnerFolderName;
            EffectiveFromPartnerFolderName = source.EffectiveFromPartnerFolderName;
        }
    }
}
