using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A User represents a human or system/service user with the ability to connect to Files.com via any of the available connectivity methods (unless restricted to specific protocols).
    /// </summary>
    public sealed class User : FilesObject
    {
        internal User()
        {
        }

        /// <summary>
        /// User ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// User's username
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// List of group IDs of which this user is an administrator
        /// </summary>
        public long?[] AdminGroupIds { get; internal set; }

        /// <summary>
        /// A list of allowed IPs if applicable.  Newline delimited
        /// </summary>
        public string AllowedIps { get; internal set; }

        /// <summary>
        /// If `true`, the user can user create Bundles (aka Share Links). Use the bundle permission instead.
        /// </summary>
        public bool AttachmentsPermission { get; internal set; }

        /// <summary>
        /// Number of API keys associated with this user
        /// </summary>
        public long? ApiKeysCount { get; internal set; }

        /// <summary>
        /// Scheduled Date/Time at which user will be deactivated
        /// </summary>
        public DateTime? AuthenticateUntil { get; internal set; }

        /// <summary>
        /// How is this user authenticated?
        /// </summary>
        public string AuthenticationMethod { get; internal set; }

        /// <summary>
        /// URL holding the user's avatar
        /// </summary>
        public string AvatarUrl { get; internal set; }

        /// <summary>
        /// Is this a billable user record?
        /// </summary>
        public bool Billable { get; internal set; }

        /// <summary>
        /// Allow this user to perform operations on the account, payments, and invoices?
        /// </summary>
        public bool BillingPermission { get; internal set; }

        /// <summary>
        /// Allow this user to skip site-wide IP blacklists?
        /// </summary>
        public bool BypassSiteAllowedIps { get; internal set; }

        /// <summary>
        /// Exempt this user from user lifecycle rules?
        /// </summary>
        public bool BypassUserLifecycleRules { get; internal set; }

        /// <summary>
        /// When this user was created
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Can the user connect with WebDAV?
        /// </summary>
        public bool DavPermission { get; internal set; }

        /// <summary>
        /// Is user disabled? Disabled users cannot log in, and do not count for billing purposes. Users can be automatically disabled after an inactivity period via a Site setting or schedule to be deactivated after specific date.
        /// </summary>
        public bool Disabled { get; internal set; }

        /// <summary>
        /// Computed property that returns true if user disabled or expired or inactive.
        /// </summary>
        public bool DisabledExpiredOrInactive { get; internal set; }

        /// <summary>
        /// AI Assistant Personality ID assigned directly to this user, if any.
        /// </summary>
        public long? AiAssistantPersonalityId { get; internal set; }

        /// <summary>
        /// Desktop Configuration Profile ID assigned directly to this user, if any.
        /// </summary>
        public long? DesktopConfigurationProfileId { get; internal set; }

        /// <summary>
        /// Integration Centric Profile ID assigned directly to this user, if any.
        /// </summary>
        public long? IntegrationCentricProfileId { get; internal set; }

        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// File system layout
        /// </summary>
        public string FilesystemLayout { get; internal set; }

        /// <summary>
        /// User's first login time
        /// </summary>
        public DateTime? FirstLoginAt { get; internal set; }

        /// <summary>
        /// Can the user access with FTP/FTPS?
        /// </summary>
        public bool FtpPermission { get; internal set; }

        /// <summary>
        /// Comma-separated list of group IDs of which this user is a member
        /// </summary>
        public string GroupIds { get; internal set; }

        /// <summary>
        /// Text to display to the user in the header of the UI
        /// </summary>
        public string HeaderText { get; internal set; }

        /// <summary>
        /// Preferred language
        /// </summary>
        public string Language { get; internal set; }

        /// <summary>
        /// User's most recent login time via any protocol
        /// </summary>
        public DateTime? LastLoginAt { get; internal set; }

        /// <summary>
        /// User's most recent login time via web
        /// </summary>
        public DateTime? LastWebLoginAt { get; internal set; }

        /// <summary>
        /// User's most recent login time via FTP
        /// </summary>
        public DateTime? LastFtpLoginAt { get; internal set; }

        /// <summary>
        /// User's most recent login time via SFTP
        /// </summary>
        public DateTime? LastSftpLoginAt { get; internal set; }

        /// <summary>
        /// User's most recent login time via WebDAV
        /// </summary>
        public DateTime? LastDavLoginAt { get; internal set; }

        /// <summary>
        /// User's most recent login time via Desktop app
        /// </summary>
        public DateTime? LastDesktopLoginAt { get; internal set; }

        /// <summary>
        /// User's most recent login time via Rest API
        /// </summary>
        public DateTime? LastRestapiLoginAt { get; internal set; }

        /// <summary>
        /// User's most recent API use time
        /// </summary>
        public DateTime? LastApiUseAt { get; internal set; }

        /// <summary>
        /// User's most recent activity time, which is the latest of most recent login, most recent API use, enablement, or creation
        /// </summary>
        public DateTime? LastActiveAt { get; internal set; }

        /// <summary>
        /// The most recent protocol and cipher used
        /// </summary>
        public string LastProtocolCipher { get; internal set; }

        /// <summary>
        /// Time in the future that the user will no longer be locked out if applicable
        /// </summary>
        public DateTime? LockoutExpires { get; internal set; }

        /// <summary>
        /// User's full name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// User's company
        /// </summary>
        public string Company { get; internal set; }

        /// <summary>
        /// Any internal notes on the user
        /// </summary>
        public string Notes { get; internal set; }

        /// <summary>
        /// Hour of the day at which daily notifications should be sent. Can be in range 0 to 23
        /// </summary>
        public long? NotificationDailySendTime { get; internal set; }

        /// <summary>
        /// Enable integration with Office for the web?
        /// </summary>
        public bool OfficeIntegrationEnabled { get; internal set; }

        /// <summary>
        /// Is this user a Partner administrator?
        /// </summary>
        public bool PartnerAdmin { get; internal set; }

        /// <summary>
        /// Partner ID if this user belongs to a Partner
        /// </summary>
        public long? PartnerId { get; internal set; }

        /// <summary>
        /// Name of the Partner if this user belongs to a Partner
        /// </summary>
        public string PartnerName { get; internal set; }

        /// <summary>
        /// Last time the user's password was set
        /// </summary>
        public DateTime? PasswordSetAt { get; internal set; }

        /// <summary>
        /// Number of days to allow user to use the same password
        /// </summary>
        public long? PasswordValidityDays { get; internal set; }

        /// <summary>
        /// Primary group ID for Group Admin scoping
        /// </summary>
        public long? PrimaryGroupId { get; internal set; }

        /// <summary>
        /// Number of public keys associated with this user
        /// </summary>
        public long? PublicKeysCount { get; internal set; }

        /// <summary>
        /// Deprecated. Use notify_on_all_site_warnings and granular failure notification preferences instead.
        /// </summary>
        public bool ReceiveAdminAlerts { get; internal set; }

        /// <summary>
        /// Should the user receive site warnings via email?
        /// </summary>
        public bool NotifyOnAllSiteWarnings { get; internal set; }

        /// <summary>
        /// Should the user receive sso/scim/ldap configuration/sync failures via email?
        /// </summary>
        public bool NotifyOnAllSsoFailures { get; internal set; }

        /// <summary>
        /// Should the user receive user security events via email?
        /// </summary>
        public bool NotifyOnAllUserSecurityEvents { get; internal set; }

        /// <summary>
        /// Should the user receive pending work failures via email?
        /// </summary>
        public bool NotifyOnAllPendingWorkFailures { get; internal set; }

        /// <summary>
        /// Should the user receive siem failures via email?
        /// </summary>
        public bool NotifyOnAllSiemHttpDestinationFailures { get; internal set; }

        /// <summary>
        /// Should the user receive sync failures via email?
        /// </summary>
        public bool NotifyOnAllSyncFailures { get; internal set; }

        /// <summary>
        /// Should the user receive automation failures via email?
        /// </summary>
        public bool NotifyOnAllAutomationFailures { get; internal set; }

        /// <summary>
        /// Should the user receive expectation failures and misses via email?
        /// </summary>
        public bool NotifyOnAllExpectationFailures { get; internal set; }

        /// <summary>
        /// 2FA required setting. `use_system_setting` uses the site-wide setting, including SSO exemptions. `always_require` and `never_require` override the site-wide setting when user-level overrides are allowed.
        /// </summary>
        public string Require2fa { get; internal set; }

        /// <summary>
        /// Require user to login by specified date otherwise it will be disabled.
        /// </summary>
        public DateTime? RequireLoginBy { get; internal set; }

        /// <summary>
        /// Is 2fa active for the user?
        /// </summary>
        public bool Active2fa { get; internal set; }

        /// <summary>
        /// Is a password change required upon next user login?
        /// </summary>
        public bool RequirePasswordChange { get; internal set; }

        /// <summary>
        /// Is user's password expired?
        /// </summary>
        public bool PasswordExpired { get; internal set; }

        /// <summary>
        /// ID of the internal Group responsible for this Partner User, overriding the Partner default.
        /// </summary>
        public long? ResponsibleGroupId { get; internal set; }

        /// <summary>
        /// ID of the internal User responsible for this Partner User, overriding the Partner default.
        /// </summary>
        public long? ResponsibleUserId { get; internal set; }

        /// <summary>
        /// Is the user an allowed to view all (non-billing) site configuration for this site?
        /// </summary>
        public bool ReadonlySiteAdmin { get; internal set; }

        /// <summary>
        /// Can this user access the Web app, Desktop app, SDKs, or REST API?  (All of these tools use the API internally, so this is one unified permission set.)
        /// </summary>
        public bool RestapiPermission { get; internal set; }

        /// <summary>
        /// Can the user access the S3-compatible endpoint? Defaults to true.
        /// </summary>
        public bool S3CompatibleEndpointPermission { get; internal set; }

        /// <summary>
        /// Does this user manage it's own credentials or is it a shared/bot user?
        /// </summary>
        public bool SelfManaged { get; internal set; }

        /// <summary>
        /// Can the user access with SFTP?
        /// </summary>
        public bool SftpPermission { get; internal set; }

        /// <summary>
        /// Is the user an administrator for this site?
        /// </summary>
        public bool SiteAdmin { get; internal set; }

        /// <summary>
        /// Whether the user is an administrator of their own Custom Workspace. Does not reflect administration granted through Permissions.
        /// </summary>
        public bool WorkspaceAdmin { get; internal set; }

        /// <summary>
        /// Site ID
        /// </summary>
        public long? SiteId { get; internal set; }

        /// <summary>
        /// ID of the Workspace the user belongs to. 0 is the Default Workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Workspace ID the user should land in by default when more than one Workspace is available.
        /// </summary>
        public long? DefaultWorkspaceId { get; internal set; }

        /// <summary>
        /// Skip Welcome page in the UI?
        /// </summary>
        public bool SkipWelcomeScreen { get; internal set; }

        /// <summary>
        /// FTP TLS session reuse requirement: use_system_setting, always_require, or never_require
        /// </summary>
        public string FtpTlsSessionReuseRequired { get; internal set; }

        /// <summary>
        /// SSL required setting
        /// </summary>
        public string SslRequired { get; internal set; }

        /// <summary>
        /// SSO (Single Sign On) strategy ID for the user, if applicable.
        /// </summary>
        public long? SsoStrategyId { get; internal set; }

        /// <summary>
        /// Is this user managed by a SsoStrategy?
        /// </summary>
        public bool ExternallyManaged { get; internal set; }

        /// <summary>
        /// Comma-separated list of Tags for this user. Tags are used for other features, such as UserLifecycleRules, which can target specific tags.  Tags must only contain lowercase letters, numbers, and hyphens.
        /// </summary>
        public string Tags { get; internal set; }

        /// <summary>
        /// User time zone
        /// </summary>
        public string TimeZone { get; internal set; }

        /// <summary>
        /// Type(s) of 2FA methods in use, for programmatic use.  Will be either `sms`, `totp`, `webauthn`, `yubi`, `email`, or multiple values sorted alphabetically and joined by an underscore.  Does not specify whether user has more than one of a given method.
        /// </summary>
        public string TypeOf2fa { get; internal set; }

        /// <summary>
        /// Type(s) of 2FA methods in use, formatted for displaying in the UI.  Unlike `type_of_2fa`, this value will make clear when a user has more than 1 of the same type of method.
        /// </summary>
        public string TypeOf2faForDisplay { get; internal set; }

        /// <summary>
        /// If filesystem layout is user_root, this path is the root path the user is fixed to for all interfaces. If the filesystem layout is site_root or partner_root, this acts as a root folder only for FTP and SFTP (SFTP applicability also requires a site-wide setting to be set). For partner_root layout, this path is relative to the Partner root folder for all callers and blank opts out of an additional protocol root. In this situation, this path is not applied to the API, Desktop, or Web interface.
        /// </summary>
        public string UserRoot { get; internal set; }

        /// <summary>
        /// Home folder for FTP/SFTP. For users with the partner_root filesystem layout, this path is relative to the Partner root folder. In all other cases, it is an absolute path. Only applies to FTP and SFTP, and not any other interface.
        /// </summary>
        public string UserHome { get; internal set; }

        /// <summary>
        /// Number of days remaining until password expires
        /// </summary>
        public long? DaysRemainingUntilPasswordExpire { get; internal set; }

        /// <summary>
        /// Password expiration datetime
        /// </summary>
        public DateTime? PasswordExpireAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(User source)
        {
            Id = source.Id;
            Username = source.Username;
            AdminGroupIds = source.AdminGroupIds;
            AllowedIps = source.AllowedIps;
            AttachmentsPermission = source.AttachmentsPermission;
            ApiKeysCount = source.ApiKeysCount;
            AuthenticateUntil = source.AuthenticateUntil;
            AuthenticationMethod = source.AuthenticationMethod;
            AvatarUrl = source.AvatarUrl;
            Billable = source.Billable;
            BillingPermission = source.BillingPermission;
            BypassSiteAllowedIps = source.BypassSiteAllowedIps;
            BypassUserLifecycleRules = source.BypassUserLifecycleRules;
            CreatedAt = source.CreatedAt;
            DavPermission = source.DavPermission;
            Disabled = source.Disabled;
            DisabledExpiredOrInactive = source.DisabledExpiredOrInactive;
            AiAssistantPersonalityId = source.AiAssistantPersonalityId;
            DesktopConfigurationProfileId = source.DesktopConfigurationProfileId;
            IntegrationCentricProfileId = source.IntegrationCentricProfileId;
            Email = source.Email;
            FilesystemLayout = source.FilesystemLayout;
            FirstLoginAt = source.FirstLoginAt;
            FtpPermission = source.FtpPermission;
            GroupIds = source.GroupIds;
            HeaderText = source.HeaderText;
            Language = source.Language;
            LastLoginAt = source.LastLoginAt;
            LastWebLoginAt = source.LastWebLoginAt;
            LastFtpLoginAt = source.LastFtpLoginAt;
            LastSftpLoginAt = source.LastSftpLoginAt;
            LastDavLoginAt = source.LastDavLoginAt;
            LastDesktopLoginAt = source.LastDesktopLoginAt;
            LastRestapiLoginAt = source.LastRestapiLoginAt;
            LastApiUseAt = source.LastApiUseAt;
            LastActiveAt = source.LastActiveAt;
            LastProtocolCipher = source.LastProtocolCipher;
            LockoutExpires = source.LockoutExpires;
            Name = source.Name;
            Company = source.Company;
            Notes = source.Notes;
            NotificationDailySendTime = source.NotificationDailySendTime;
            OfficeIntegrationEnabled = source.OfficeIntegrationEnabled;
            PartnerAdmin = source.PartnerAdmin;
            PartnerId = source.PartnerId;
            PartnerName = source.PartnerName;
            PasswordSetAt = source.PasswordSetAt;
            PasswordValidityDays = source.PasswordValidityDays;
            PrimaryGroupId = source.PrimaryGroupId;
            PublicKeysCount = source.PublicKeysCount;
            ReceiveAdminAlerts = source.ReceiveAdminAlerts;
            NotifyOnAllSiteWarnings = source.NotifyOnAllSiteWarnings;
            NotifyOnAllSsoFailures = source.NotifyOnAllSsoFailures;
            NotifyOnAllUserSecurityEvents = source.NotifyOnAllUserSecurityEvents;
            NotifyOnAllPendingWorkFailures = source.NotifyOnAllPendingWorkFailures;
            NotifyOnAllSiemHttpDestinationFailures = source.NotifyOnAllSiemHttpDestinationFailures;
            NotifyOnAllSyncFailures = source.NotifyOnAllSyncFailures;
            NotifyOnAllAutomationFailures = source.NotifyOnAllAutomationFailures;
            NotifyOnAllExpectationFailures = source.NotifyOnAllExpectationFailures;
            Require2fa = source.Require2fa;
            RequireLoginBy = source.RequireLoginBy;
            Active2fa = source.Active2fa;
            RequirePasswordChange = source.RequirePasswordChange;
            PasswordExpired = source.PasswordExpired;
            ResponsibleGroupId = source.ResponsibleGroupId;
            ResponsibleUserId = source.ResponsibleUserId;
            ReadonlySiteAdmin = source.ReadonlySiteAdmin;
            RestapiPermission = source.RestapiPermission;
            S3CompatibleEndpointPermission = source.S3CompatibleEndpointPermission;
            SelfManaged = source.SelfManaged;
            SftpPermission = source.SftpPermission;
            SiteAdmin = source.SiteAdmin;
            WorkspaceAdmin = source.WorkspaceAdmin;
            SiteId = source.SiteId;
            WorkspaceId = source.WorkspaceId;
            DefaultWorkspaceId = source.DefaultWorkspaceId;
            SkipWelcomeScreen = source.SkipWelcomeScreen;
            FtpTlsSessionReuseRequired = source.FtpTlsSessionReuseRequired;
            SslRequired = source.SslRequired;
            SsoStrategyId = source.SsoStrategyId;
            ExternallyManaged = source.ExternallyManaged;
            Tags = source.Tags;
            TimeZone = source.TimeZone;
            TypeOf2fa = source.TypeOf2fa;
            TypeOf2faForDisplay = source.TypeOf2faForDisplay;
            UserRoot = source.UserRoot;
            UserHome = source.UserHome;
            DaysRemainingUntilPasswordExpire = source.DaysRemainingUntilPasswordExpire;
            PasswordExpireAt = source.PasswordExpireAt;
        }
    }
}
