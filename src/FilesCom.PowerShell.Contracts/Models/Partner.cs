using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Partner is a first-class entity that cleanly represents an external organization, enables delegated administration, and enforces strict boundaries.
    /// </summary>
    public sealed class Partner : FilesObject
    {
        internal Partner()
        {
        }

        /// <summary>
        /// Allow Partner Admins to change Two-Factor Authentication requirements for Partner Users.
        /// </summary>
        public bool AllowBypassing2faPolicies { get; internal set; }

        /// <summary>
        /// A list of allowed IPs for this Partner. Newline delimited. Partner User IP access is allowed when the IP matches the Partner, User, or Site allowed IP lists.
        /// </summary>
        public string AllowedIps { get; internal set; }

        /// <summary>
        /// Allow Partner Admins to change or reset credentials for users belonging to this Partner.
        /// </summary>
        public bool AllowCredentialChanges { get; internal set; }

        /// <summary>
        /// Allow Partner Admins to provide GPG keys.
        /// </summary>
        public bool AllowProvidingGpgKeys { get; internal set; }

        /// <summary>
        /// Allow Partner Admins to create users.
        /// </summary>
        public bool AllowUserCreation { get; internal set; }

        /// <summary>
        /// When `true`, emails sent to Partner users are copied to the responsible User or Group.
        /// </summary>
        public bool CcEmailsToResponsibleParty { get; internal set; }

        /// <summary>
        /// Approved Connected Sites relationships for this Partner, in both directions. Empty when this Partner has no connections. Read-only.
        /// </summary>
        public Models.PartnerConnection[] Connections { get; internal set; }

        /// <summary>
        /// The unique ID of the Partner.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// AI Assistant Personality ID assigned to this Partner, if any. Users in the Partner inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        public long? AiAssistantPersonalityId { get; internal set; }

        /// <summary>
        /// ID of the Workspace associated with this Partner.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// The name of the Partner.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Notes about this Partner.
        /// </summary>
        public string Notes { get; internal set; }

        /// <summary>
        /// Array of User IDs that are Partner Admins for this Partner.
        /// </summary>
        public long?[] PartnerAdminIds { get; internal set; }

        /// <summary>
        /// ID of the Partner Channel Template assigned to this Partner.
        /// </summary>
        public long? PartnerChannelTemplateId { get; internal set; }

        /// <summary>
        /// This site's role for this Partner in Connected Sites relationships. `host` is a Partner this site configured. `guest` is a Partner created by approving another site's connection request; it has no root folder and cannot hold users, permissions, or Partner Channels, or host a connection. `host_and_guest` is a configured Partner that is also the guest side of a connection. Promote a `guest` Partner by setting this to `host_and_guest` together with a `root_folder`.
        /// </summary>
        public string PartnershipRole { get; internal set; }

        /// <summary>
        /// ID of the Group responsible for this Partner.
        /// </summary>
        public long? ResponsibleGroupId { get; internal set; }

        /// <summary>
        /// ID of the User responsible for this Partner.
        /// </summary>
        public long? ResponsibleUserId { get; internal set; }

        /// <summary>
        /// The root folder path for this Partner.
        /// </summary>
        public string RootFolder { get; internal set; }

        /// <summary>
        /// Show Partner users a simplified home page built from this Partner's Channels.
        /// </summary>
        public bool ShowPartnerChannelHomePage { get; internal set; }

        /// <summary>
        /// Comma-separated list of Tags for this Partner. Tags are used for other features, such as UserLifecycleRules, which can target specific tags.  Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        public string Tags { get; internal set; }

        /// <summary>
        /// Array of User IDs that belong to this Partner.
        /// </summary>
        public long?[] UserIds { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Partner source)
        {
            AllowBypassing2faPolicies = source.AllowBypassing2faPolicies;
            AllowedIps = source.AllowedIps;
            AllowCredentialChanges = source.AllowCredentialChanges;
            AllowProvidingGpgKeys = source.AllowProvidingGpgKeys;
            AllowUserCreation = source.AllowUserCreation;
            CcEmailsToResponsibleParty = source.CcEmailsToResponsibleParty;
            Connections = source.Connections;
            Id = source.Id;
            AiAssistantPersonalityId = source.AiAssistantPersonalityId;
            WorkspaceId = source.WorkspaceId;
            Name = source.Name;
            Notes = source.Notes;
            PartnerAdminIds = source.PartnerAdminIds;
            PartnerChannelTemplateId = source.PartnerChannelTemplateId;
            PartnershipRole = source.PartnershipRole;
            ResponsibleGroupId = source.ResponsibleGroupId;
            ResponsibleUserId = source.ResponsibleUserId;
            RootFolder = source.RootFolder;
            ShowPartnerChannelHomePage = source.ShowPartnerChannelHomePage;
            Tags = source.Tags;
            UserIds = source.UserIds;
        }
    }
}
