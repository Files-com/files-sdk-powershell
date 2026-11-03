using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Partners, or get one partner by Id.
    /// </summary>
    [Cmdlet("Get", "FilesPartner", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Partner), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Partner>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Partner), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesPartnerCommand : FilesListCmdlet<Models.Partner>
    {
        /// <summary>
        /// Used for pagination.  When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`.  Send one of those cursor value here to resume an existing list from the next available record.  Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Cursor { get; set; }

        /// <summary>
        /// Number of records to show per page.  (Max: 10000, 1,000 or less is recommended).
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PerPage { get; set; }

        /// <summary>
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id` and `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Partner ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Partners, or get one partner by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Partner result = Run(connection, token => PartnerBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => PartnerBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Cursor)))
            {
                parameters["cursor"] = Cursor;
            }
            if (IsBound(nameof(PerPage)))
            {
                parameters["per_page"] = PerPage.Value;
            }
            if (IsBound(nameof(SortBy)))
            {
                parameters["sort_by"] = RequestValues.QueryObject(SortBy, "SortBy");
            }
            if (IsBound(nameof(Filter)))
            {
                parameters["filter"] = RequestValues.QueryObject(Filter, "Filter");
            }
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Partner.
    /// </summary>
    [Cmdlet("New", "FilesPartner", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.Partner))]
    public sealed class NewFilesPartnerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// AI Assistant Personality ID assigned to this Partner, if any. Users in the Partner inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? AiAssistantPersonalityId { get; set; }

        /// <summary>
        /// A list of allowed IPs for this Partner. Newline delimited. Partner User IP access is allowed when the IP matches the Partner, User, or Site allowed IP lists.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] AllowedIps { get; set; }

        /// <summary>
        /// Allow Partner Admins to change Two-Factor Authentication requirements for Partner Users.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowBypassing2faPolicies { get; set; }

        /// <summary>
        /// Allow Partner Admins to change or reset credentials for users belonging to this Partner.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowCredentialChanges { get; set; }

        /// <summary>
        /// Allow Partner Admins to provide GPG keys.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowProvidingGpgKeys { get; set; }

        /// <summary>
        /// Allow Partner Admins to create users.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowUserCreation { get; set; }

        /// <summary>
        /// When `true`, emails sent to Partner users are copied to the responsible User or Group.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? CcEmailsToResponsibleParty { get; set; }

        /// <summary>
        /// Notes about this Partner.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Notes { get; set; }

        /// <summary>
        /// ID of the Partner Channel Template assigned to this Partner.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PartnerChannelTemplateId { get; set; }

        /// <summary>
        /// ID of the Group responsible for this Partner.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ResponsibleGroupId { get; set; }

        /// <summary>
        /// ID of the User responsible for this Partner.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ResponsibleUserId { get; set; }

        /// <summary>
        /// Show Partner users a simplified home page built from this Partner's Channels.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ShowPartnerChannelHomePage { get; set; }

        /// <summary>
        /// Comma-separated list of Tags for this Partner. Tags are used for other features, such as UserLifecycleRules, which can target specific tags.  Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] Tags { get; set; }

        /// <summary>
        /// The name of the Partner.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// The root folder path for this Partner.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string RootFolder { get; set; }

        /// <summary>
        /// ID of the Workspace associated with this Partner.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Partner.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Partner '{Name}'" : "Partner"))
            {
                return;
            }
            Models.Partner result = Run(connection, token => PartnerBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(AiAssistantPersonalityId)))
            {
                parameters["ai_assistant_personality_id"] = AiAssistantPersonalityId.Value;
            }
            if (IsBound(nameof(AllowedIps)))
            {
                parameters["allowed_ips"] = RequestValues.Join(AllowedIps, "AllowedIps", "\n");
            }
            if (IsBound(nameof(AllowBypassing2faPolicies)))
            {
                parameters["allow_bypassing_2fa_policies"] = AllowBypassing2faPolicies.Value;
            }
            if (IsBound(nameof(AllowCredentialChanges)))
            {
                parameters["allow_credential_changes"] = AllowCredentialChanges.Value;
            }
            if (IsBound(nameof(AllowProvidingGpgKeys)))
            {
                parameters["allow_providing_gpg_keys"] = AllowProvidingGpgKeys.Value;
            }
            if (IsBound(nameof(AllowUserCreation)))
            {
                parameters["allow_user_creation"] = AllowUserCreation.Value;
            }
            if (IsBound(nameof(CcEmailsToResponsibleParty)))
            {
                parameters["cc_emails_to_responsible_party"] = CcEmailsToResponsibleParty.Value;
            }
            if (IsBound(nameof(Notes)))
            {
                parameters["notes"] = Notes;
            }
            if (IsBound(nameof(PartnerChannelTemplateId)))
            {
                parameters["partner_channel_template_id"] = PartnerChannelTemplateId.Value;
            }
            if (IsBound(nameof(ResponsibleGroupId)))
            {
                parameters["responsible_group_id"] = ResponsibleGroupId.Value;
            }
            if (IsBound(nameof(ResponsibleUserId)))
            {
                parameters["responsible_user_id"] = ResponsibleUserId.Value;
            }
            if (IsBound(nameof(ShowPartnerChannelHomePage)))
            {
                parameters["show_partner_channel_home_page"] = ShowPartnerChannelHomePage.Value;
            }
            if (IsBound(nameof(Tags)))
            {
                parameters["tags"] = RequestValues.Join(Tags, "Tags", ",");
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(RootFolder)))
            {
                parameters["root_folder"] = RootFolder;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Partner.
    /// </summary>
    [Cmdlet("Set", "FilesPartner", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.Partner), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesPartnerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Partner ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// AI Assistant Personality ID assigned to this Partner, if any. Users in the Partner inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? AiAssistantPersonalityId { get; set; }

        /// <summary>
        /// A list of allowed IPs for this Partner. Newline delimited. Partner User IP access is allowed when the IP matches the Partner, User, or Site allowed IP lists.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] AllowedIps { get; set; }

        /// <summary>
        /// Allow Partner Admins to change Two-Factor Authentication requirements for Partner Users.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AllowBypassing2faPolicies { get; set; }

        /// <summary>
        /// Allow Partner Admins to change or reset credentials for users belonging to this Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AllowCredentialChanges { get; set; }

        /// <summary>
        /// Allow Partner Admins to provide GPG keys.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AllowProvidingGpgKeys { get; set; }

        /// <summary>
        /// Allow Partner Admins to create users.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AllowUserCreation { get; set; }

        /// <summary>
        /// When `true`, emails sent to Partner users are copied to the responsible User or Group.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? CcEmailsToResponsibleParty { get; set; }

        /// <summary>
        /// Notes about this Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Notes { get; set; }

        /// <summary>
        /// ID of the Partner Channel Template assigned to this Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PartnerChannelTemplateId { get; set; }

        /// <summary>
        /// ID of the Group responsible for this Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ResponsibleGroupId { get; set; }

        /// <summary>
        /// ID of the User responsible for this Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ResponsibleUserId { get; set; }

        /// <summary>
        /// Show Partner users a simplified home page built from this Partner's Channels.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ShowPartnerChannelHomePage { get; set; }

        /// <summary>
        /// Comma-separated list of Tags for this Partner. Tags are used for other features, such as UserLifecycleRules, which can target specific tags.  Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] Tags { get; set; }

        /// <summary>
        /// The name of the Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// This site's role for this Partner in Connected Sites relationships. `host` is a Partner this site configured. `guest` is a Partner created by approving another site's connection request; it has no root folder and cannot hold users, permissions, or Partner Channels, or host a connection. `host_and_guest` is a configured Partner that is also the guest side of a connection. Promote a `guest` Partner by setting this to `host_and_guest` together with a `root_folder`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PartnershipRole { get; set; }

        /// <summary>
        /// The root folder path for this Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string RootFolder { get; set; }

        /// <summary>
        /// The partner to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Partner InputObject { get; set; }

        /// <summary>Update Partner.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Partner {id}"))
            {
                return;
            }
            Models.Partner result = Run(connection, token => PartnerBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(AiAssistantPersonalityId)))
            {
                parameters["ai_assistant_personality_id"] = AiAssistantPersonalityId.Value;
            }
            if (IsBound(nameof(AllowedIps)))
            {
                parameters["allowed_ips"] = RequestValues.Join(AllowedIps, "AllowedIps", "\n");
            }
            if (IsBound(nameof(AllowBypassing2faPolicies)))
            {
                parameters["allow_bypassing_2fa_policies"] = AllowBypassing2faPolicies.Value;
            }
            if (IsBound(nameof(AllowCredentialChanges)))
            {
                parameters["allow_credential_changes"] = AllowCredentialChanges.Value;
            }
            if (IsBound(nameof(AllowProvidingGpgKeys)))
            {
                parameters["allow_providing_gpg_keys"] = AllowProvidingGpgKeys.Value;
            }
            if (IsBound(nameof(AllowUserCreation)))
            {
                parameters["allow_user_creation"] = AllowUserCreation.Value;
            }
            if (IsBound(nameof(CcEmailsToResponsibleParty)))
            {
                parameters["cc_emails_to_responsible_party"] = CcEmailsToResponsibleParty.Value;
            }
            if (IsBound(nameof(Notes)))
            {
                parameters["notes"] = Notes;
            }
            if (IsBound(nameof(PartnerChannelTemplateId)))
            {
                parameters["partner_channel_template_id"] = PartnerChannelTemplateId.Value;
            }
            if (IsBound(nameof(ResponsibleGroupId)))
            {
                parameters["responsible_group_id"] = ResponsibleGroupId.Value;
            }
            if (IsBound(nameof(ResponsibleUserId)))
            {
                parameters["responsible_user_id"] = ResponsibleUserId.Value;
            }
            if (IsBound(nameof(ShowPartnerChannelHomePage)))
            {
                parameters["show_partner_channel_home_page"] = ShowPartnerChannelHomePage.Value;
            }
            if (IsBound(nameof(Tags)))
            {
                parameters["tags"] = RequestValues.Join(Tags, "Tags", ",");
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(PartnershipRole)))
            {
                parameters["partnership_role"] = PartnershipRole;
            }
            if (IsBound(nameof(RootFolder)))
            {
                parameters["root_folder"] = RootFolder;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Partner.
    /// </summary>
    [Cmdlet("Remove", "FilesPartner", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesPartnerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Partner ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The partner to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Partner InputObject { get; set; }

        /// <summary>Delete Partner.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Partner {id}"))
            {
                return;
            }
            Run(connection, token => PartnerBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
