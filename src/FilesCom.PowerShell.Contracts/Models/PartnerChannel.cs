using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A PartnerChannel defines a structured communication path within a Partner root folder, including directional folder names and partner-scoped routing configuration.
    /// </summary>
    public sealed class PartnerChannel : FilesObject
    {
        internal PartnerChannel()
        {
        }

        /// <summary>
        /// The unique ID of the Partner Channel.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the Workspace associated with this Partner Channel.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Channel directions. `two_way` enables both directions, `to_partner` enables outgoing downloads, and `from_partner` enables incoming uploads.
        /// </summary>
        public string Direction { get; internal set; }

        /// <summary>
        /// Use the Channel folder directly for a one-way exchange. Defaults to false. Cannot be changed after creation. Folder name overrides must be blank when enabled, and the Channel must remain one-way.
        /// </summary>
        public bool UseChannelRoot { get; internal set; }

        /// <summary>
        /// ID of the Partner this Channel belongs to.
        /// </summary>
        public long? PartnerId { get; internal set; }

        /// <summary>
        /// ID of the Partner Channel Template that manages this Channel, if any.
        /// </summary>
        public long? PartnerChannelTemplateId { get; internal set; }

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
        /// Optional route path for files uploaded by the Partner.
        /// </summary>
        public string FromPartnerRoutePath { get; internal set; }

        /// <summary>
        /// Optional route path for files delivered to the Partner.
        /// </summary>
        public string ToPartnerRoutePath { get; internal set; }

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

        /// <summary>
        /// Resolved Channel folder path.
        /// </summary>
        public string ChannelPath { get; internal set; }

        /// <summary>
        /// Resolved to-Partner folder path.
        /// </summary>
        public string ToPartnerFolderPath { get; internal set; }

        /// <summary>
        /// Resolved from-Partner folder path.
        /// </summary>
        public string FromPartnerFolderPath { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(PartnerChannel source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Direction = source.Direction;
            UseChannelRoot = source.UseChannelRoot;
            PartnerId = source.PartnerId;
            PartnerChannelTemplateId = source.PartnerChannelTemplateId;
            Path = source.Path;
            ToPartnerFolderName = source.ToPartnerFolderName;
            FromPartnerFolderName = source.FromPartnerFolderName;
            FromPartnerRoutePath = source.FromPartnerRoutePath;
            ToPartnerRoutePath = source.ToPartnerRoutePath;
            ToPartnerManagedFolderPaths = source.ToPartnerManagedFolderPaths;
            FromPartnerManagedFolderPaths = source.FromPartnerManagedFolderPaths;
            EffectiveToPartnerFolderName = source.EffectiveToPartnerFolderName;
            EffectiveFromPartnerFolderName = source.EffectiveFromPartnerFolderName;
            ChannelPath = source.ChannelPath;
            ToPartnerFolderPath = source.ToPartnerFolderPath;
            FromPartnerFolderPath = source.FromPartnerFolderPath;
        }
    }
}
