using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Users, or get one user by Id.
    /// </summary>
    [Cmdlet("Get", "FilesUser", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.User), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.User>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.User), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesUserCommand : FilesListCmdlet<Models.User>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `site_id`, `workspace_id`, `company`, `name`, `disabled`, `authenticate_until`, `username`, `email`, `site_admin`, `last_desktop_login_at`, `last_login_at`, `password_validity_days` or `ssl_required`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `username`, `name`, `email`, `company`, `site_admin`, `password_validity_days`, `ssl_required`, `last_login_at`, `authenticate_until`, `not_site_admin`, `disabled`, `partner_id`, `primary_group_id` or `workspace_id`. Valid field combinations are `[ site_admin, username ]`, `[ not_site_admin, username ]`, `[ workspace_id, username ]`, `[ company, name ]`, `[ workspace_id, name ]`, `[ workspace_id, email ]`, `[ workspace_id, company ]`, `[ workspace_id, site_admin ]`, `[ workspace_id, not_site_admin ]`, `[ workspace_id, disabled ]`, `[ workspace_id, partner_id ]`, `[ workspace_id, site_admin, username ]`, `[ workspace_id, not_site_admin, username ]`, `[ workspace_id, disabled, username ]`, `[ workspace_id, partner_id, username ]` or `[ workspace_id, company, name ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than the supplied value. Valid fields are `password_validity_days`, `last_login_at` or `authenticate_until`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `password_validity_days`, `last_login_at` or `authenticate_until`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGteq { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `username`, `name`, `email` or `company`. Valid field combinations are `[ company, name ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than the supplied value. Valid fields are `password_validity_days`, `last_login_at` or `authenticate_until`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than or equal the supplied value. Valid fields are `password_validity_days`, `last_login_at` or `authenticate_until`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLteq { get; set; }

        /// <summary>
        /// comma-separated list of User IDs
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] Ids { get; set; }

        /// <summary>
        /// Include users from the parent site.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? IncludeParentSiteUsers { get; set; }

        /// <summary>
        /// When scoped to a Custom Workspace, include Default Workspace users who have permissions in that Workspace. Available to Site Administrators, and to Workspace Administrators and Folder Admins in that Workspace when the site shares its Default Workspace directory with Workspaces.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? IncludeDefaultWorkspaceUsers { get; set; }

        /// <summary>
        /// Searches for partial matches of name, username, or email.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Search { get; set; }

        /// <summary>
        /// User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Users, or get one user by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.User result = Run(connection, token => UserBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => UserBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(FilterGt)))
            {
                parameters["filter_gt"] = RequestValues.QueryObject(FilterGt, "FilterGt");
            }
            if (IsBound(nameof(FilterGteq)))
            {
                parameters["filter_gteq"] = RequestValues.QueryObject(FilterGteq, "FilterGteq");
            }
            if (IsBound(nameof(FilterPrefix)))
            {
                parameters["filter_prefix"] = RequestValues.QueryObject(FilterPrefix, "FilterPrefix");
            }
            if (IsBound(nameof(FilterLt)))
            {
                parameters["filter_lt"] = RequestValues.QueryObject(FilterLt, "FilterLt");
            }
            if (IsBound(nameof(FilterLteq)))
            {
                parameters["filter_lteq"] = RequestValues.QueryObject(FilterLteq, "FilterLteq");
            }
            if (IsBound(nameof(Ids)))
            {
                parameters["ids"] = RequestValues.JoinInt64s(Ids);
            }
            if (IsBound(nameof(IncludeParentSiteUsers)))
            {
                parameters["include_parent_site_users"] = IncludeParentSiteUsers.Value;
            }
            if (IsBound(nameof(IncludeDefaultWorkspaceUsers)))
            {
                parameters["include_default_workspace_users"] = IncludeDefaultWorkspaceUsers.Value;
            }
            if (IsBound(nameof(Search)))
            {
                parameters["search"] = Search;
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
    /// Create User.
    /// </summary>
    [Cmdlet("New", "FilesUser", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.User))]
    public sealed class NewFilesUserCommand : FilesApiCmdlet
    {
        /// <summary>
        /// An image file for your user avatar.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment AvatarFile { get; set; }

        /// <summary>
        /// If true, the avatar will be deleted.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AvatarDelete { get; set; }

        /// <summary>
        /// Used for changing a password on an existing user.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString ChangePassword { get; set; }

        /// <summary>
        /// Optional, but if provided, we will ensure that it matches the value sent in `change_password`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString ChangePasswordConfirmation { get; set; }

        /// <summary>
        /// User's email.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Email { get; set; }

        /// <summary>
        /// Permission to grant on the User Root upon creation, or on the Workspace root when the User Root is blank. For the Default Workspace, a blank User Root uses the site root. `admin` on a Custom Workspace's root grants Workspace Administrator access; on a subfolder it grants Folder Admin access. Can be blank or `admin`, `full`, `read`, `write`, `list`, `read+write`, `list+write`, or `history`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GrantPermission { get; set; }

        /// <summary>
        /// Group ID to associate this user with.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? GroupId { get; set; }

        /// <summary>
        /// A list of group ids to associate this user with.  Comma delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>
        /// Pre-calculated hash of the user's password. If supplied, this will be used to authenticate the user on first login. Supported hash methods are MD5, SHA1, and SHA256.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString ImportedPasswordHash { get; set; }

        /// <summary>
        /// User password.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Password { get; set; }

        /// <summary>
        /// Optional, but if provided, we will ensure that it matches the value sent in `password`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PasswordConfirmation { get; set; }

        /// <summary>
        /// Signifies that the user has read all the announcements in the UI.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AnnouncementsRead { get; set; }

        /// <summary>
        /// AI Assistant Personality ID assigned directly to this user, if any.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? AiAssistantPersonalityId { get; set; }

        /// <summary>
        /// A list of allowed IPs if applicable.  Newline delimited
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] AllowedIps { get; set; }

        /// <summary>
        /// DEPRECATED: If `true`, the user can user create Bundles (aka Share Links). Use the bundle permission instead.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AttachmentsPermission { get; set; }

        /// <summary>
        /// Scheduled Date/Time at which user will be deactivated
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? AuthenticateUntil { get; set; }

        /// <summary>
        /// How is this user authenticated?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Allow this user to perform operations on the account, payments, and invoices?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BillingPermission { get; set; }

        /// <summary>
        /// Exempt this user from user lifecycle rules?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BypassUserLifecycleRules { get; set; }

        /// <summary>
        /// Allow this user to skip site-wide IP blacklists?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BypassSiteAllowedIps { get; set; }

        /// <summary>
        /// Can the user connect with WebDAV?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DavPermission { get; set; }

        /// <summary>
        /// Desktop Configuration Profile ID assigned directly to this user, if any.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? DesktopConfigurationProfileId { get; set; }

        /// <summary>
        /// Workspace ID the user should land in by default when more than one Workspace is available.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? DefaultWorkspaceId { get; set; }

        /// <summary>
        /// Is user disabled? Disabled users cannot log in, and do not count for billing purposes. Users can be automatically disabled after an inactivity period via a Site setting or schedule to be deactivated after specific date.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// File system layout
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FilesystemLayout { get; set; }

        /// <summary>
        /// Can the user access with FTP/FTPS?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FtpPermission { get; set; }

        /// <summary>
        /// Text to display to the user in the header of the UI
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HeaderText { get; set; }

        /// <summary>
        /// Integration Centric Profile ID assigned directly to this user, if any.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? IntegrationCentricProfileId { get; set; }

        /// <summary>
        /// Preferred language
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Language { get; set; }

        /// <summary>
        /// Hour of the day at which daily notifications should be sent. Can be in range 0 to 23
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? NotificationDailySendTime { get; set; }

        /// <summary>
        /// User's full name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// User's company
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Company { get; set; }

        /// <summary>
        /// Any internal notes on the user
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Notes { get; set; }

        /// <summary>
        /// Enable integration with Office for the web?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? OfficeIntegrationEnabled { get; set; }

        /// <summary>
        /// Is this user a Partner administrator?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PartnerAdmin { get; set; }

        /// <summary>
        /// Partner ID if this user belongs to a Partner
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PartnerId { get; set; }

        /// <summary>
        /// Number of days to allow user to use the same password
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PasswordValidityDays { get; set; }

        /// <summary>
        /// Primary group ID for Group Admin scoping
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PrimaryGroupId { get; set; }

        /// <summary>
        /// Is the user an allowed to view all (non-billing) site configuration for this site?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ReadonlySiteAdmin { get; set; }

        /// <summary>
        /// Deprecated. Use notify_on_all_site_warnings and granular failure notification preferences instead.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ReceiveAdminAlerts { get; set; }

        /// <summary>
        /// Should the user receive site warnings via email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnAllSiteWarnings { get; set; }

        /// <summary>
        /// Should the user receive sso/scim/ldap configuration/sync failures via email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnAllSsoFailures { get; set; }

        /// <summary>
        /// Should the user receive user security events via email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnAllUserSecurityEvents { get; set; }

        /// <summary>
        /// Should the user receive pending work failures via email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnAllPendingWorkFailures { get; set; }

        /// <summary>
        /// Should the user receive siem failures via email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnAllSiemHttpDestinationFailures { get; set; }

        /// <summary>
        /// Should the user receive sync failures via email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnAllSyncFailures { get; set; }

        /// <summary>
        /// Should the user receive automation failures via email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnAllAutomationFailures { get; set; }

        /// <summary>
        /// Should the user receive expectation failures and misses via email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NotifyOnAllExpectationFailures { get; set; }

        /// <summary>
        /// Require user to login by specified date otherwise it will be disabled.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? RequireLoginBy { get; set; }

        /// <summary>
        /// Is a password change required upon next user login?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RequirePasswordChange { get; set; }

        /// <summary>
        /// ID of the internal Group responsible for this Partner User, overriding the Partner default.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ResponsibleGroupId { get; set; }

        /// <summary>
        /// ID of the internal User responsible for this Partner User, overriding the Partner default.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ResponsibleUserId { get; set; }

        /// <summary>
        /// Can this user access the Web app, Desktop app, SDKs, or REST API?  (All of these tools use the API internally, so this is one unified permission set.)
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RestapiPermission { get; set; }

        /// <summary>
        /// Can the user access the S3-compatible endpoint? Defaults to true.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? S3CompatibleEndpointPermission { get; set; }

        /// <summary>
        /// Does this user manage it's own credentials or is it a shared/bot user?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SelfManaged { get; set; }

        /// <summary>
        /// Can the user access with SFTP?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpPermission { get; set; }

        /// <summary>
        /// Is the user an administrator for this site?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SiteAdmin { get; set; }

        /// <summary>
        /// Skip Welcome page in the UI?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SkipWelcomeScreen { get; set; }

        /// <summary>
        /// SSL required setting
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SslRequired { get; set; }

        /// <summary>
        /// FTP TLS session reuse requirement: use_system_setting, always_require, or never_require
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FtpTlsSessionReuseRequired { get; set; }

        /// <summary>
        /// SSO (Single Sign On) strategy ID for the user, if applicable.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SsoStrategyId { get; set; }

        /// <summary>
        /// 2FA required setting. `use_system_setting` uses the site-wide setting, including SSO exemptions. `always_require` and `never_require` override the site-wide setting when user-level overrides are allowed.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Require2fa { get; set; }

        /// <summary>
        /// Comma-separated list of Tags for this user. Tags are used for other features, such as UserLifecycleRules, which can target specific tags.  Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] Tags { get; set; }

        /// <summary>
        /// User time zone
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string TimeZone { get; set; }

        /// <summary>
        /// If filesystem layout is user_root, this path is the root path the user is fixed to for all interfaces. If the filesystem layout is site_root or partner_root, this acts as a root folder only for FTP and SFTP (SFTP applicability also requires a site-wide setting to be set). For partner_root layout, this path is relative to the Partner root folder for all callers and blank opts out of an additional protocol root. In this situation, this path is not applied to the API, Desktop, or Web interface.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UserRoot { get; set; }

        /// <summary>
        /// Home folder for FTP/SFTP. For users with the partner_root filesystem layout, this path is relative to the Partner root folder. In all other cases, it is an absolute path. Only applies to FTP and SFTP, and not any other interface.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UserHome { get; set; }

        /// <summary>
        /// Whether the user is an administrator of their own Custom Workspace. Does not reflect administration granted through Permissions.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WorkspaceAdmin { get; set; }

        /// <summary>
        /// User's username
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Username { get; set; }

        /// <summary>
        /// ID of the Workspace the user belongs to. 0 is the Default Workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create User.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Username)) ? $"User '{Username}'" : "User"))
            {
                return;
            }
            Models.User result = Run(connection, token => UserBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(AvatarFile)))
            {
                parameters["avatar_file"] = RequestValues.Attachment(AvatarFile, "AvatarFile");
            }
            if (IsBound(nameof(AvatarDelete)))
            {
                parameters["avatar_delete"] = AvatarDelete.Value;
            }
            if (IsBound(nameof(ChangePassword)))
            {
                parameters["change_password"] = RequestValues.Reveal(ChangePassword);
            }
            if (IsBound(nameof(ChangePasswordConfirmation)))
            {
                parameters["change_password_confirmation"] = RequestValues.Reveal(ChangePasswordConfirmation);
            }
            if (IsBound(nameof(Email)))
            {
                parameters["email"] = Email;
            }
            if (IsBound(nameof(GrantPermission)))
            {
                parameters["grant_permission"] = GrantPermission;
            }
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId.Value;
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.JoinInt64s(GroupIds);
            }
            if (IsBound(nameof(ImportedPasswordHash)))
            {
                parameters["imported_password_hash"] = RequestValues.Reveal(ImportedPasswordHash);
            }
            if (IsBound(nameof(Password)))
            {
                parameters["password"] = RequestValues.Reveal(Password);
            }
            if (IsBound(nameof(PasswordConfirmation)))
            {
                parameters["password_confirmation"] = RequestValues.Reveal(PasswordConfirmation);
            }
            if (IsBound(nameof(AnnouncementsRead)))
            {
                parameters["announcements_read"] = AnnouncementsRead.Value;
            }
            if (IsBound(nameof(AiAssistantPersonalityId)))
            {
                parameters["ai_assistant_personality_id"] = AiAssistantPersonalityId.Value;
            }
            if (IsBound(nameof(AllowedIps)))
            {
                parameters["allowed_ips"] = RequestValues.Join(AllowedIps, "AllowedIps", "\n");
            }
            if (IsBound(nameof(AttachmentsPermission)))
            {
                parameters["attachments_permission"] = AttachmentsPermission.Value;
            }
            if (IsBound(nameof(AuthenticateUntil)))
            {
                parameters["authenticate_until"] = RequestValues.DateTime(AuthenticateUntil.Value);
            }
            if (IsBound(nameof(AuthenticationMethod)))
            {
                parameters["authentication_method"] = AuthenticationMethod;
            }
            if (IsBound(nameof(BillingPermission)))
            {
                parameters["billing_permission"] = BillingPermission.Value;
            }
            if (IsBound(nameof(BypassUserLifecycleRules)))
            {
                parameters["bypass_user_lifecycle_rules"] = BypassUserLifecycleRules.Value;
            }
            if (IsBound(nameof(BypassSiteAllowedIps)))
            {
                parameters["bypass_site_allowed_ips"] = BypassSiteAllowedIps.Value;
            }
            if (IsBound(nameof(DavPermission)))
            {
                parameters["dav_permission"] = DavPermission.Value;
            }
            if (IsBound(nameof(DesktopConfigurationProfileId)))
            {
                parameters["desktop_configuration_profile_id"] = DesktopConfigurationProfileId.Value;
            }
            if (IsBound(nameof(DefaultWorkspaceId)))
            {
                parameters["default_workspace_id"] = DefaultWorkspaceId.Value;
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(FilesystemLayout)))
            {
                parameters["filesystem_layout"] = FilesystemLayout;
            }
            if (IsBound(nameof(FtpPermission)))
            {
                parameters["ftp_permission"] = FtpPermission.Value;
            }
            if (IsBound(nameof(HeaderText)))
            {
                parameters["header_text"] = HeaderText;
            }
            if (IsBound(nameof(IntegrationCentricProfileId)))
            {
                parameters["integration_centric_profile_id"] = IntegrationCentricProfileId.Value;
            }
            if (IsBound(nameof(Language)))
            {
                parameters["language"] = Language;
            }
            if (IsBound(nameof(NotificationDailySendTime)))
            {
                parameters["notification_daily_send_time"] = NotificationDailySendTime.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Company)))
            {
                parameters["company"] = Company;
            }
            if (IsBound(nameof(Notes)))
            {
                parameters["notes"] = Notes;
            }
            if (IsBound(nameof(OfficeIntegrationEnabled)))
            {
                parameters["office_integration_enabled"] = OfficeIntegrationEnabled.Value;
            }
            if (IsBound(nameof(PartnerAdmin)))
            {
                parameters["partner_admin"] = PartnerAdmin.Value;
            }
            if (IsBound(nameof(PartnerId)))
            {
                parameters["partner_id"] = PartnerId.Value;
            }
            if (IsBound(nameof(PasswordValidityDays)))
            {
                parameters["password_validity_days"] = PasswordValidityDays.Value;
            }
            if (IsBound(nameof(PrimaryGroupId)))
            {
                parameters["primary_group_id"] = PrimaryGroupId.Value;
            }
            if (IsBound(nameof(ReadonlySiteAdmin)))
            {
                parameters["readonly_site_admin"] = ReadonlySiteAdmin.Value;
            }
            if (IsBound(nameof(ReceiveAdminAlerts)))
            {
                parameters["receive_admin_alerts"] = ReceiveAdminAlerts.Value;
            }
            if (IsBound(nameof(NotifyOnAllSiteWarnings)))
            {
                parameters["notify_on_all_site_warnings"] = NotifyOnAllSiteWarnings.Value;
            }
            if (IsBound(nameof(NotifyOnAllSsoFailures)))
            {
                parameters["notify_on_all_sso_failures"] = NotifyOnAllSsoFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllUserSecurityEvents)))
            {
                parameters["notify_on_all_user_security_events"] = NotifyOnAllUserSecurityEvents.Value;
            }
            if (IsBound(nameof(NotifyOnAllPendingWorkFailures)))
            {
                parameters["notify_on_all_pending_work_failures"] = NotifyOnAllPendingWorkFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllSiemHttpDestinationFailures)))
            {
                parameters["notify_on_all_siem_http_destination_failures"] = NotifyOnAllSiemHttpDestinationFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllSyncFailures)))
            {
                parameters["notify_on_all_sync_failures"] = NotifyOnAllSyncFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllAutomationFailures)))
            {
                parameters["notify_on_all_automation_failures"] = NotifyOnAllAutomationFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllExpectationFailures)))
            {
                parameters["notify_on_all_expectation_failures"] = NotifyOnAllExpectationFailures.Value;
            }
            if (IsBound(nameof(RequireLoginBy)))
            {
                parameters["require_login_by"] = RequestValues.DateTime(RequireLoginBy.Value);
            }
            if (IsBound(nameof(RequirePasswordChange)))
            {
                parameters["require_password_change"] = RequirePasswordChange.Value;
            }
            if (IsBound(nameof(ResponsibleGroupId)))
            {
                parameters["responsible_group_id"] = ResponsibleGroupId.Value;
            }
            if (IsBound(nameof(ResponsibleUserId)))
            {
                parameters["responsible_user_id"] = ResponsibleUserId.Value;
            }
            if (IsBound(nameof(RestapiPermission)))
            {
                parameters["restapi_permission"] = RestapiPermission.Value;
            }
            if (IsBound(nameof(S3CompatibleEndpointPermission)))
            {
                parameters["s3_compatible_endpoint_permission"] = S3CompatibleEndpointPermission.Value;
            }
            if (IsBound(nameof(SelfManaged)))
            {
                parameters["self_managed"] = SelfManaged.Value;
            }
            if (IsBound(nameof(SftpPermission)))
            {
                parameters["sftp_permission"] = SftpPermission.Value;
            }
            if (IsBound(nameof(SiteAdmin)))
            {
                parameters["site_admin"] = SiteAdmin.Value;
            }
            if (IsBound(nameof(SkipWelcomeScreen)))
            {
                parameters["skip_welcome_screen"] = SkipWelcomeScreen.Value;
            }
            if (IsBound(nameof(SslRequired)))
            {
                parameters["ssl_required"] = SslRequired;
            }
            if (IsBound(nameof(FtpTlsSessionReuseRequired)))
            {
                parameters["ftp_tls_session_reuse_required"] = FtpTlsSessionReuseRequired;
            }
            if (IsBound(nameof(SsoStrategyId)))
            {
                parameters["sso_strategy_id"] = SsoStrategyId.Value;
            }
            if (IsBound(nameof(Require2fa)))
            {
                parameters["require_2fa"] = Require2fa;
            }
            if (IsBound(nameof(Tags)))
            {
                parameters["tags"] = RequestValues.Join(Tags, "Tags", ",");
            }
            if (IsBound(nameof(TimeZone)))
            {
                parameters["time_zone"] = TimeZone;
            }
            if (IsBound(nameof(UserRoot)))
            {
                parameters["user_root"] = UserRoot;
            }
            if (IsBound(nameof(UserHome)))
            {
                parameters["user_home"] = UserHome;
            }
            if (IsBound(nameof(WorkspaceAdmin)))
            {
                parameters["workspace_admin"] = WorkspaceAdmin.Value;
            }
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Unlock user who has been locked out due to failed logins.
    /// </summary>
    [Cmdlet("Unlock", "FilesUser", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class UnlockFilesUserCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The user to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.User InputObject { get; set; }

        /// <summary>Unlock user who has been locked out due to failed logins.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UnlockParameters();
            if (!ShouldProcessOn(connection, $"User {id}"))
            {
                return;
            }
            Run(connection, token => UserBridge.UnlockAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> UnlockParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Resend user welcome email.
    /// </summary>
    [Cmdlet("Send", "FilesUserWelcomeEmail", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class SendFilesUserWelcomeEmailCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The user to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.User InputObject { get; set; }

        /// <summary>Resend user welcome email.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ResendWelcomeEmailParameters();
            if (!ShouldProcessOn(connection, $"User {id}"))
            {
                return;
            }
            Run(connection, token => UserBridge.ResendWelcomeEmailAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> ResendWelcomeEmailParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Trigger 2FA Reset process for user who has lost access to their existing 2FA methods.
    /// </summary>
    [Cmdlet("Reset", "FilesUserTwoFactorAuthentication", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class ResetFilesUserTwoFactorAuthenticationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The user to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.User InputObject { get; set; }

        /// <summary>Trigger 2FA Reset process for user who has lost access to their existing 2FA methods.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = User2faResetParameters();
            if (!ShouldProcessOn(connection, $"User {id}"))
            {
                return;
            }
            Run(connection, token => UserBridge.User2faResetAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> User2faResetParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update User.
    /// </summary>
    [Cmdlet("Set", "FilesUser", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.User), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesUserCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// An image file for your user avatar.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public FilesAttachment AvatarFile { get; set; }

        /// <summary>
        /// If true, the avatar will be deleted.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AvatarDelete { get; set; }

        /// <summary>
        /// Used for changing a password on an existing user.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString ChangePassword { get; set; }

        /// <summary>
        /// Optional, but if provided, we will ensure that it matches the value sent in `change_password`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString ChangePasswordConfirmation { get; set; }

        /// <summary>
        /// User's email.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Email { get; set; }

        /// <summary>
        /// Permission to grant on the User Root upon creation, or on the Workspace root when the User Root is blank. For the Default Workspace, a blank User Root uses the site root. `admin` on a Custom Workspace's root grants Workspace Administrator access; on a subfolder it grants Folder Admin access. Can be blank or `admin`, `full`, `read`, `write`, `list`, `read+write`, `list+write`, or `history`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string GrantPermission { get; set; }

        /// <summary>
        /// Group ID to associate this user with.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? GroupId { get; set; }

        /// <summary>
        /// A list of group ids to associate this user with.  Comma delimited.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>
        /// Pre-calculated hash of the user's password. If supplied, this will be used to authenticate the user on first login. Supported hash methods are MD5, SHA1, and SHA256.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString ImportedPasswordHash { get; set; }

        /// <summary>
        /// User password.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString Password { get; set; }

        /// <summary>
        /// Optional, but if provided, we will ensure that it matches the value sent in `password`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString PasswordConfirmation { get; set; }

        /// <summary>
        /// Signifies that the user has read all the announcements in the UI.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AnnouncementsRead { get; set; }

        /// <summary>
        /// AI Assistant Personality ID assigned directly to this user, if any.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? AiAssistantPersonalityId { get; set; }

        /// <summary>
        /// A list of allowed IPs if applicable.  Newline delimited
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] AllowedIps { get; set; }

        /// <summary>
        /// DEPRECATED: If `true`, the user can user create Bundles (aka Share Links). Use the bundle permission instead.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AttachmentsPermission { get; set; }

        /// <summary>
        /// Scheduled Date/Time at which user will be deactivated
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? AuthenticateUntil { get; set; }

        /// <summary>
        /// How is this user authenticated?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Allow this user to perform operations on the account, payments, and invoices?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? BillingPermission { get; set; }

        /// <summary>
        /// Exempt this user from user lifecycle rules?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? BypassUserLifecycleRules { get; set; }

        /// <summary>
        /// Allow this user to skip site-wide IP blacklists?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? BypassSiteAllowedIps { get; set; }

        /// <summary>
        /// Can the user connect with WebDAV?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DavPermission { get; set; }

        /// <summary>
        /// Desktop Configuration Profile ID assigned directly to this user, if any.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? DesktopConfigurationProfileId { get; set; }

        /// <summary>
        /// Workspace ID the user should land in by default when more than one Workspace is available.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? DefaultWorkspaceId { get; set; }

        /// <summary>
        /// Is user disabled? Disabled users cannot log in, and do not count for billing purposes. Users can be automatically disabled after an inactivity period via a Site setting or schedule to be deactivated after specific date.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Disabled { get; set; }

        /// <summary>
        /// File system layout
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FilesystemLayout { get; set; }

        /// <summary>
        /// Can the user access with FTP/FTPS?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? FtpPermission { get; set; }

        /// <summary>
        /// Text to display to the user in the header of the UI
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HeaderText { get; set; }

        /// <summary>
        /// Integration Centric Profile ID assigned directly to this user, if any.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? IntegrationCentricProfileId { get; set; }

        /// <summary>
        /// Preferred language
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Language { get; set; }

        /// <summary>
        /// Hour of the day at which daily notifications should be sent. Can be in range 0 to 23
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? NotificationDailySendTime { get; set; }

        /// <summary>
        /// User's full name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// User's company
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Company { get; set; }

        /// <summary>
        /// Any internal notes on the user
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Notes { get; set; }

        /// <summary>
        /// Enable integration with Office for the web?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? OfficeIntegrationEnabled { get; set; }

        /// <summary>
        /// Is this user a Partner administrator?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? PartnerAdmin { get; set; }

        /// <summary>
        /// Partner ID if this user belongs to a Partner
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PartnerId { get; set; }

        /// <summary>
        /// Number of days to allow user to use the same password
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PasswordValidityDays { get; set; }

        /// <summary>
        /// Primary group ID for Group Admin scoping
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PrimaryGroupId { get; set; }

        /// <summary>
        /// Is the user an allowed to view all (non-billing) site configuration for this site?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ReadonlySiteAdmin { get; set; }

        /// <summary>
        /// Deprecated. Use notify_on_all_site_warnings and granular failure notification preferences instead.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ReceiveAdminAlerts { get; set; }

        /// <summary>
        /// Should the user receive site warnings via email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnAllSiteWarnings { get; set; }

        /// <summary>
        /// Should the user receive sso/scim/ldap configuration/sync failures via email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnAllSsoFailures { get; set; }

        /// <summary>
        /// Should the user receive user security events via email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnAllUserSecurityEvents { get; set; }

        /// <summary>
        /// Should the user receive pending work failures via email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnAllPendingWorkFailures { get; set; }

        /// <summary>
        /// Should the user receive siem failures via email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnAllSiemHttpDestinationFailures { get; set; }

        /// <summary>
        /// Should the user receive sync failures via email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnAllSyncFailures { get; set; }

        /// <summary>
        /// Should the user receive automation failures via email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnAllAutomationFailures { get; set; }

        /// <summary>
        /// Should the user receive expectation failures and misses via email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? NotifyOnAllExpectationFailures { get; set; }

        /// <summary>
        /// Require user to login by specified date otherwise it will be disabled.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? RequireLoginBy { get; set; }

        /// <summary>
        /// Is a password change required upon next user login?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? RequirePasswordChange { get; set; }

        /// <summary>
        /// ID of the internal Group responsible for this Partner User, overriding the Partner default.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ResponsibleGroupId { get; set; }

        /// <summary>
        /// ID of the internal User responsible for this Partner User, overriding the Partner default.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ResponsibleUserId { get; set; }

        /// <summary>
        /// Can this user access the Web app, Desktop app, SDKs, or REST API?  (All of these tools use the API internally, so this is one unified permission set.)
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? RestapiPermission { get; set; }

        /// <summary>
        /// Can the user access the S3-compatible endpoint? Defaults to true.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? S3CompatibleEndpointPermission { get; set; }

        /// <summary>
        /// Does this user manage it's own credentials or is it a shared/bot user?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SelfManaged { get; set; }

        /// <summary>
        /// Can the user access with SFTP?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SftpPermission { get; set; }

        /// <summary>
        /// Is the user an administrator for this site?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SiteAdmin { get; set; }

        /// <summary>
        /// Skip Welcome page in the UI?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SkipWelcomeScreen { get; set; }

        /// <summary>
        /// SSL required setting
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SslRequired { get; set; }

        /// <summary>
        /// FTP TLS session reuse requirement: use_system_setting, always_require, or never_require
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FtpTlsSessionReuseRequired { get; set; }

        /// <summary>
        /// SSO (Single Sign On) strategy ID for the user, if applicable.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? SsoStrategyId { get; set; }

        /// <summary>
        /// 2FA required setting. `use_system_setting` uses the site-wide setting, including SSO exemptions. `always_require` and `never_require` override the site-wide setting when user-level overrides are allowed.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Require2fa { get; set; }

        /// <summary>
        /// Comma-separated list of Tags for this user. Tags are used for other features, such as UserLifecycleRules, which can target specific tags.  Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] Tags { get; set; }

        /// <summary>
        /// User time zone
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string TimeZone { get; set; }

        /// <summary>
        /// If filesystem layout is user_root, this path is the root path the user is fixed to for all interfaces. If the filesystem layout is site_root or partner_root, this acts as a root folder only for FTP and SFTP (SFTP applicability also requires a site-wide setting to be set). For partner_root layout, this path is relative to the Partner root folder for all callers and blank opts out of an additional protocol root. In this situation, this path is not applied to the API, Desktop, or Web interface.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string UserRoot { get; set; }

        /// <summary>
        /// Home folder for FTP/SFTP. For users with the partner_root filesystem layout, this path is relative to the Partner root folder. In all other cases, it is an absolute path. Only applies to FTP and SFTP, and not any other interface.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string UserHome { get; set; }

        /// <summary>
        /// Whether the user is an administrator of their own Custom Workspace. Does not reflect administration granted through Permissions.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? WorkspaceAdmin { get; set; }

        /// <summary>
        /// User's username
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Username { get; set; }

        /// <summary>
        /// Workspace ID. Only Site Administrators can change this field. Values supplied by Workspace Administrators, Group Administrators, or other non-Site Administrators using `/user` are ignored.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// If true when changing authentication_method from `password` to `sso`, remove all two-factor methods. Ignored in all other cases.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Clear2fa { get; set; }

        /// <summary>
        /// Required when assigning a Partner to an existing non-Partner user. If true, convert the user by assigning the partner_id provided.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ConvertToPartnerUser { get; set; }

        /// <summary>
        /// The user to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.User InputObject { get; set; }

        /// <summary>Update User.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"User {id}"))
            {
                return;
            }
            Models.User result = Run(connection, token => UserBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(AvatarFile)))
            {
                parameters["avatar_file"] = RequestValues.Attachment(AvatarFile, "AvatarFile");
            }
            if (IsBound(nameof(AvatarDelete)))
            {
                parameters["avatar_delete"] = AvatarDelete.Value;
            }
            if (IsBound(nameof(ChangePassword)))
            {
                parameters["change_password"] = RequestValues.Reveal(ChangePassword);
            }
            if (IsBound(nameof(ChangePasswordConfirmation)))
            {
                parameters["change_password_confirmation"] = RequestValues.Reveal(ChangePasswordConfirmation);
            }
            if (IsBound(nameof(Email)))
            {
                parameters["email"] = Email;
            }
            if (IsBound(nameof(GrantPermission)))
            {
                parameters["grant_permission"] = GrantPermission;
            }
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId.Value;
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.JoinInt64s(GroupIds);
            }
            if (IsBound(nameof(ImportedPasswordHash)))
            {
                parameters["imported_password_hash"] = RequestValues.Reveal(ImportedPasswordHash);
            }
            if (IsBound(nameof(Password)))
            {
                parameters["password"] = RequestValues.Reveal(Password);
            }
            if (IsBound(nameof(PasswordConfirmation)))
            {
                parameters["password_confirmation"] = RequestValues.Reveal(PasswordConfirmation);
            }
            if (IsBound(nameof(AnnouncementsRead)))
            {
                parameters["announcements_read"] = AnnouncementsRead.Value;
            }
            if (IsBound(nameof(AiAssistantPersonalityId)))
            {
                parameters["ai_assistant_personality_id"] = AiAssistantPersonalityId.Value;
            }
            if (IsBound(nameof(AllowedIps)))
            {
                parameters["allowed_ips"] = RequestValues.Join(AllowedIps, "AllowedIps", "\n");
            }
            if (IsBound(nameof(AttachmentsPermission)))
            {
                parameters["attachments_permission"] = AttachmentsPermission.Value;
            }
            if (IsBound(nameof(AuthenticateUntil)))
            {
                parameters["authenticate_until"] = RequestValues.DateTime(AuthenticateUntil.Value);
            }
            if (IsBound(nameof(AuthenticationMethod)))
            {
                parameters["authentication_method"] = AuthenticationMethod;
            }
            if (IsBound(nameof(BillingPermission)))
            {
                parameters["billing_permission"] = BillingPermission.Value;
            }
            if (IsBound(nameof(BypassUserLifecycleRules)))
            {
                parameters["bypass_user_lifecycle_rules"] = BypassUserLifecycleRules.Value;
            }
            if (IsBound(nameof(BypassSiteAllowedIps)))
            {
                parameters["bypass_site_allowed_ips"] = BypassSiteAllowedIps.Value;
            }
            if (IsBound(nameof(DavPermission)))
            {
                parameters["dav_permission"] = DavPermission.Value;
            }
            if (IsBound(nameof(DesktopConfigurationProfileId)))
            {
                parameters["desktop_configuration_profile_id"] = DesktopConfigurationProfileId.Value;
            }
            if (IsBound(nameof(DefaultWorkspaceId)))
            {
                parameters["default_workspace_id"] = DefaultWorkspaceId.Value;
            }
            if (IsBound(nameof(Disabled)))
            {
                parameters["disabled"] = Disabled.Value;
            }
            if (IsBound(nameof(FilesystemLayout)))
            {
                parameters["filesystem_layout"] = FilesystemLayout;
            }
            if (IsBound(nameof(FtpPermission)))
            {
                parameters["ftp_permission"] = FtpPermission.Value;
            }
            if (IsBound(nameof(HeaderText)))
            {
                parameters["header_text"] = HeaderText;
            }
            if (IsBound(nameof(IntegrationCentricProfileId)))
            {
                parameters["integration_centric_profile_id"] = IntegrationCentricProfileId.Value;
            }
            if (IsBound(nameof(Language)))
            {
                parameters["language"] = Language;
            }
            if (IsBound(nameof(NotificationDailySendTime)))
            {
                parameters["notification_daily_send_time"] = NotificationDailySendTime.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Company)))
            {
                parameters["company"] = Company;
            }
            if (IsBound(nameof(Notes)))
            {
                parameters["notes"] = Notes;
            }
            if (IsBound(nameof(OfficeIntegrationEnabled)))
            {
                parameters["office_integration_enabled"] = OfficeIntegrationEnabled.Value;
            }
            if (IsBound(nameof(PartnerAdmin)))
            {
                parameters["partner_admin"] = PartnerAdmin.Value;
            }
            if (IsBound(nameof(PartnerId)))
            {
                parameters["partner_id"] = PartnerId.Value;
            }
            if (IsBound(nameof(PasswordValidityDays)))
            {
                parameters["password_validity_days"] = PasswordValidityDays.Value;
            }
            if (IsBound(nameof(PrimaryGroupId)))
            {
                parameters["primary_group_id"] = PrimaryGroupId.Value;
            }
            if (IsBound(nameof(ReadonlySiteAdmin)))
            {
                parameters["readonly_site_admin"] = ReadonlySiteAdmin.Value;
            }
            if (IsBound(nameof(ReceiveAdminAlerts)))
            {
                parameters["receive_admin_alerts"] = ReceiveAdminAlerts.Value;
            }
            if (IsBound(nameof(NotifyOnAllSiteWarnings)))
            {
                parameters["notify_on_all_site_warnings"] = NotifyOnAllSiteWarnings.Value;
            }
            if (IsBound(nameof(NotifyOnAllSsoFailures)))
            {
                parameters["notify_on_all_sso_failures"] = NotifyOnAllSsoFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllUserSecurityEvents)))
            {
                parameters["notify_on_all_user_security_events"] = NotifyOnAllUserSecurityEvents.Value;
            }
            if (IsBound(nameof(NotifyOnAllPendingWorkFailures)))
            {
                parameters["notify_on_all_pending_work_failures"] = NotifyOnAllPendingWorkFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllSiemHttpDestinationFailures)))
            {
                parameters["notify_on_all_siem_http_destination_failures"] = NotifyOnAllSiemHttpDestinationFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllSyncFailures)))
            {
                parameters["notify_on_all_sync_failures"] = NotifyOnAllSyncFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllAutomationFailures)))
            {
                parameters["notify_on_all_automation_failures"] = NotifyOnAllAutomationFailures.Value;
            }
            if (IsBound(nameof(NotifyOnAllExpectationFailures)))
            {
                parameters["notify_on_all_expectation_failures"] = NotifyOnAllExpectationFailures.Value;
            }
            if (IsBound(nameof(RequireLoginBy)))
            {
                parameters["require_login_by"] = RequestValues.DateTime(RequireLoginBy.Value);
            }
            if (IsBound(nameof(RequirePasswordChange)))
            {
                parameters["require_password_change"] = RequirePasswordChange.Value;
            }
            if (IsBound(nameof(ResponsibleGroupId)))
            {
                parameters["responsible_group_id"] = ResponsibleGroupId.Value;
            }
            if (IsBound(nameof(ResponsibleUserId)))
            {
                parameters["responsible_user_id"] = ResponsibleUserId.Value;
            }
            if (IsBound(nameof(RestapiPermission)))
            {
                parameters["restapi_permission"] = RestapiPermission.Value;
            }
            if (IsBound(nameof(S3CompatibleEndpointPermission)))
            {
                parameters["s3_compatible_endpoint_permission"] = S3CompatibleEndpointPermission.Value;
            }
            if (IsBound(nameof(SelfManaged)))
            {
                parameters["self_managed"] = SelfManaged.Value;
            }
            if (IsBound(nameof(SftpPermission)))
            {
                parameters["sftp_permission"] = SftpPermission.Value;
            }
            if (IsBound(nameof(SiteAdmin)))
            {
                parameters["site_admin"] = SiteAdmin.Value;
            }
            if (IsBound(nameof(SkipWelcomeScreen)))
            {
                parameters["skip_welcome_screen"] = SkipWelcomeScreen.Value;
            }
            if (IsBound(nameof(SslRequired)))
            {
                parameters["ssl_required"] = SslRequired;
            }
            if (IsBound(nameof(FtpTlsSessionReuseRequired)))
            {
                parameters["ftp_tls_session_reuse_required"] = FtpTlsSessionReuseRequired;
            }
            if (IsBound(nameof(SsoStrategyId)))
            {
                parameters["sso_strategy_id"] = SsoStrategyId.Value;
            }
            if (IsBound(nameof(Require2fa)))
            {
                parameters["require_2fa"] = Require2fa;
            }
            if (IsBound(nameof(Tags)))
            {
                parameters["tags"] = RequestValues.Join(Tags, "Tags", ",");
            }
            if (IsBound(nameof(TimeZone)))
            {
                parameters["time_zone"] = TimeZone;
            }
            if (IsBound(nameof(UserRoot)))
            {
                parameters["user_root"] = UserRoot;
            }
            if (IsBound(nameof(UserHome)))
            {
                parameters["user_home"] = UserHome;
            }
            if (IsBound(nameof(WorkspaceAdmin)))
            {
                parameters["workspace_admin"] = WorkspaceAdmin.Value;
            }
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(Clear2fa)))
            {
                parameters["clear_2fa"] = Clear2fa.Value;
            }
            if (IsBound(nameof(ConvertToPartnerUser)))
            {
                parameters["convert_to_partner_user"] = ConvertToPartnerUser.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete User.
    /// </summary>
    [Cmdlet("Remove", "FilesUser", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesUserCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Provide a User ID here to transfer ownership of certain resources such as Automations and Share Links (Bundles) to that new user.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? NewOwnerId { get; set; }

        /// <summary>
        /// The user to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.User InputObject { get; set; }

        /// <summary>Delete User.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"User {id}"))
            {
                return;
            }
            Run(connection, token => UserBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(NewOwnerId)))
            {
                parameters["new_owner_id"] = NewOwnerId.Value;
            }
            return parameters;
        }
    }
}
