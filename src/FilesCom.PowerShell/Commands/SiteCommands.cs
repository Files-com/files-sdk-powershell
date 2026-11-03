using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Show Site Settings.
    /// </summary>
    [Cmdlet("Get", "FilesSite")]
    [OutputType(typeof(Models.Site))]
    public sealed class GetFilesSiteCommand : FilesApiCmdlet
    {

        /// <summary>Show Site Settings.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = GetParameters();
            Models.Site result = Run(connection, token => SiteBridge.GetAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> GetParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Get the most recent usage snapshot (usage data for billing purposes) for a Site.
    /// </summary>
    [Cmdlet("Get", "FilesSiteUsage")]
    [OutputType(typeof(Models.UsageSnapshot))]
    public sealed class GetFilesSiteUsageCommand : FilesApiCmdlet
    {

        /// <summary>Get the most recent usage snapshot (usage data for billing purposes) for a Site.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = GetUsageParameters();
            Models.UsageSnapshot result = Run(connection, token => SiteBridge.GetUsageAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> GetUsageParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Extend the Automations conversion window by up to six weeks beyond its originally announced end date.
    /// </summary>
    [Cmdlet("Invoke", "FilesSiteExtendAutomationMigrationWindow", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class InvokeFilesSiteExtendAutomationMigrationWindowCommand : FilesApiCmdlet
    {
        /// <summary>
        /// New conversion-window end date in YYYY-MM-DD format, in Eastern Time (EST/EDT). Must be a Tuesday, Wednesday, or Thursday. Can only extend the current end date, within six weeks of the originally announced end date.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string EndDate { get; set; }

        /// <summary>Extend the Automations conversion window by up to six weeks beyond its originally announced end date.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = ExtendAutomationMigrationWindowParameters();
            if (!ShouldProcessOn(connection, "Site"))
            {
                return;
            }
            Run(connection, token => SiteBridge.ExtendAutomationMigrationWindowAsync(connection, parameters, CallOptions, token));
        }

        private Dictionary<string, object> ExtendAutomationMigrationWindowParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(EndDate)))
            {
                parameters["end_date"] = EndDate;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Site Settings.
    /// </summary>
    [Cmdlet("Set", "FilesSite", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.Site))]
    public sealed class SetFilesSiteCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Site name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Site subdomain
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Subdomain { get; set; }

        /// <summary>
        /// Custom domain
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Domain { get; set; }

        /// <summary>
        /// Send HSTS (HTTP Strict Transport Security) header when visitors access the site via a custom domain?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DomainHstsHeader { get; set; }

        /// <summary>
        /// Letsencrypt chain to use when registering SSL Certificate for domain. No longer used as of 2026.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DomainLetsencryptChain { get; set; }

        /// <summary>
        /// Main email for this site
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Email { get; set; }

        /// <summary>
        /// Reply-to email for this site
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Are manual Bundle names allowed?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowBundleNames { get; set; }

        /// <summary>
        /// Site-wide Bundle expiration in days
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? BundleExpiration { get; set; }

        /// <summary>
        /// Will the welcome email be sent to new users?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WelcomeEmailEnabled { get; set; }

        /// <summary>
        /// If false, rename conflicting files instead of asking for overwrite confirmation.  Only applies to web interface.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AskAboutOverwrites { get; set; }

        /// <summary>
        /// Show request access link for users without access?  Currently unused.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ShowRequestAccessLink { get; set; }

        /// <summary>
        /// Create parent directories if they do not exist during uploads?  This is primarily used to work around broken upload clients that assume servers will perform this step.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AlwaysMkdirParents { get; set; }

        /// <summary>
        /// Include this email in welcome emails if enabled
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string WelcomeEmailCc { get; set; }

        /// <summary>
        /// Include this email subject in welcome emails if enabled
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string WelcomeEmailSubject { get; set; }

        /// <summary>
        /// Custom text send in user welcome email
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string WelcomeCustomText { get; set; }

        /// <summary>
        /// Site default language
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Language { get; set; }

        /// <summary>
        /// Does FTP user Windows emulation mode?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WindowsModeFtp { get; set; }

        /// <summary>
        /// Site default time zone
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DefaultTimeZone { get; set; }

        /// <summary>
        /// Is the desktop app enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DesktopApp { get; set; }

        /// <summary>
        /// Is desktop app session IP pinning enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DesktopAppSessionIpPinning { get; set; }

        /// <summary>
        /// Desktop app session lifetime (in hours)
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? DesktopAppSessionLifetime { get; set; }

        /// <summary>
        /// Is the mobile app enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? MobileApp { get; set; }

        /// <summary>
        /// Is mobile app session IP pinning enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? MobileAppSessionIpPinning { get; set; }

        /// <summary>
        /// Mobile app session lifetime (in hours)
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? MobileAppSessionLifetime { get; set; }

        /// <summary>
        /// If true, permissions for this site must be bound to a group (not a user).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FolderPermissionsGroupsOnly { get; set; }

        /// <summary>
        /// Does the welcome screen appear?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string WelcomeScreen { get; set; }

        /// <summary>
        /// If true, allows users to use a document editing integration.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? OfficeIntegrationAvailable { get; set; }

        /// <summary>
        /// Which document editing integration to support. Files.com Editor or Microsoft Office for the Web.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string OfficeIntegrationType { get; set; }

        /// <summary>
        /// If true, we will ensure that all internal communications with any remote server are made through the primary region of the site. This setting overrides individual remote server settings.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PinAllRemoteServersToSiteRegion { get; set; }

        /// <summary>
        /// A message to show users when they connect via FTP or SFTP.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string MotdText { get; set; }

        /// <summary>
        /// Show message to users connecting via FTP
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? MotdUseForFtp { get; set; }

        /// <summary>
        /// Show message to users connecting via SFTP
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? MotdUseForSftp { get; set; }

        /// <summary>
        /// Visibility settings for account navigation
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary LeftNavigationVisibility { get; set; }

        /// <summary>
        /// If true, all AI features are disabled for this site.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DisableAllAiFeatures { get; set; }

        /// <summary>
        /// Availability settings for AI features. Each feature requires the site_admins, workspace_admins, folder_admins, and all_users keys. Optional selected_group_members defaults to false; when true, members of at least one group in group_ids get access regardless of the other options. All availability options are additive: any enabled option matching the user grants access. Optional group_ids is an array of integer IDs of active groups on this site, from any workspace. Omitted or empty group_ids grants no access through selected_group_members and does not affect other options. Disabling all AI features overrides these settings.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary AiFeatureAvailability { get; set; }

        /// <summary>
        /// Is OAuth DCR (dynamic client registration) for MCP enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? McpDcrEnabled { get; set; }

        /// <summary>
        /// Additional extensions that are considered text files
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] AdditionalTextFileTypes { get; set; }

        /// <summary>
        /// Do Bundles require internal notes?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundleRequireNote { get; set; }

        /// <summary>
        /// Do Bundle creators receive receipts of invitations?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundleSendSharedReceipts { get; set; }

        /// <summary>
        /// If true, new Share Links created by a user with a primary group will default to that group as owner.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundlesDefaultOwnedByPrimaryGroup { get; set; }

        /// <summary>
        /// Calculate CRC32 checksums for files?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? CalculateFileChecksumsCrc32 { get; set; }

        /// <summary>
        /// Calculate MD5 checksums for files?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? CalculateFileChecksumsMd5 { get; set; }

        /// <summary>
        /// Calculate SHA1 checksums for files?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? CalculateFileChecksumsSha1 { get; set; }

        /// <summary>
        /// Calculate SHA256 checksums for files?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? CalculateFileChecksumsSha256 { get; set; }

        /// <summary>
        /// Use legacy checksums mode?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? LegacyChecksumsMode { get; set; }

        /// <summary>
        /// If true, we will migrate all remote server syncs to the new Sync model.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? MigrateRemoteServerSyncToSync { get; set; }

        /// <summary>
        /// Number of days to retain AS2 messages (incoming and outgoing).
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? As2MessageRetentionDays { get; set; }

        /// <summary>
        /// If true, file operations produce file_action.activity.v1 EventChannel events in all workspaces. Defaults to false; subscription filters and workspace access still control delivery.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FileActivityEventsEnabled { get; set; }

        /// <summary>
        /// How usernames are displayed in the web UI. Can be `username_only`, `full_name_only`, `full_name_username`, `full_name_company`, or `full_name_username_company`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UsernameDisplay { get; set; }

        /// <summary>
        /// Administrator access to user-owned Remote Servers in every Workspace: always_visible (default), never_visible, user_choice_default_visible, or user_choice_default_personal. Forced modes override owner preferences; choice modes use the preference or their default.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string IntegrationCentricRemoteServerAdminAccess { get; set; }

        /// <summary>
        /// Session expiry in minutes
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SessionExpiryMinutes { get; set; }

        /// <summary>
        /// Allow legacy app pairing in all workspaces? Disabled for new sites; existing sites remain enabled until changed. Disabling blocks new pairing exchanges without revoking existing app credentials or affecting OAuth sign-in. Site administrators can re-enable it.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? LegacyPairingEnabled { get; set; }

        /// <summary>
        /// Is SSL required?  Disabling this is insecure.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SslRequired { get; set; }

        /// <summary>
        /// Require TLS-protected FTP data connections to reuse the control connection TLS session? Applies to all workspaces.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FtpTlsSessionReuseRequired { get; set; }

        /// <summary>
        /// If true, we will allow weak and known insecure ciphers to be used for SFTP connections.  Enabling this setting severely weakens the security of your site and it is not recommend, except as a last resort for compatibility.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpInsecureCiphers { get; set; }

        /// <summary>
        /// If true, we will allow weak Diffie Hellman parameters to be used within ciphers for SFTP that are otherwise on our secure list.  This has the effect of making the cipher weaker than our normal threshold for security, but is required to support certain legacy or broken SSH and MFT clients.  Enabling this weakens security, but not nearly as much as enabling the full `sftp_insecure_ciphers` option.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpInsecureDiffieHellman { get; set; }

        /// <summary>
        /// If set, Files.com will not set the CAA records required to generate future SSL certificates for this domain.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DisableFilesCertificateGeneration { get; set; }

        /// <summary>
        /// Are FedRAMP security restrictions enabled for this site?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Fedramp { get; set; }

        /// <summary>
        /// Will users be locked out after incorrect login attempts?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UserLockout { get; set; }

        /// <summary>
        /// Number of login tries within `user_lockout_within` hours before users are locked out
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserLockoutTries { get; set; }

        /// <summary>
        /// Number of hours for user lockout window
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserLockoutWithin { get; set; }

        /// <summary>
        /// How many hours to lock user out for failed password?
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserLockoutLockPeriod { get; set; }

        /// <summary>
        /// Include password in emails to new users?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? IncludePasswordInWelcomeEmail { get; set; }

        /// <summary>
        /// Comma separated list of allowed Country codes
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] AllowedCountries { get; set; }

        /// <summary>
        /// List of allowed IP addresses
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AllowedIps { get; set; }

        /// <summary>
        /// Allow the site-wide two-factor authentication requirement to be overriden on a per-user-basis?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowUserLevel2faOverride { get; set; }

        /// <summary>
        /// Allow the site-wide allowed IP restriction to be overriden on a per-user-basis?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowUserLevelAllowedIpOverride { get; set; }

        /// <summary>
        /// Allow the site-wide FTP SSL requirement to be overriden on a per-user-basis?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowUserLevelSslOverride { get; set; }

        /// <summary>
        /// Allow per-user overrides of the FTP TLS session reuse requirement?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowUserLevelFtpTlsSessionReuseOverride { get; set; }

        /// <summary>
        /// Require Parent Site users entering this Child Site to match its allowed IP addresses in addition to their existing IP restrictions. Applies to all Workspaces. Defaults to false.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? EnforceParentSiteUserIpRestrictions { get; set; }

        /// <summary>
        /// Comma separated list of disallowed Country codes
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] DisallowedCountries { get; set; }

        /// <summary>
        /// Number of days to keep deleted files
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? DaysToRetainBackups { get; set; }

        /// <summary>
        /// Number of prior passwords to disallow
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? MaxPriorPasswords { get; set; }

        /// <summary>
        /// Number of days password is valid
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PasswordValidityDays { get; set; }

        /// <summary>
        /// Shortest password length for users
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PasswordMinLength { get; set; }

        /// <summary>
        /// Require a letter in passwords?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PasswordRequireLetter { get; set; }

        /// <summary>
        /// Require lower and upper case letters in passwords?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PasswordRequireMixed { get; set; }

        /// <summary>
        /// Require special characters in password?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PasswordRequireSpecial { get; set; }

        /// <summary>
        /// Require a number in passwords?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PasswordRequireNumber { get; set; }

        /// <summary>
        /// Require passwords that have not been previously breached? (see https://haveibeenpwned.com/)
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PasswordRequireUnbreached { get; set; }

        /// <summary>
        /// If true, we will hide the 'Remember Me' box on Inbox and Bundle registration pages, requiring that the user logout and log back in every time they visit the page.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RequireLogoutFromBundlesAndInboxes { get; set; }

        /// <summary>
        /// Use user FTP roots also for WebDAV?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DavUserRootEnabled { get; set; }

        /// <summary>
        /// Use user FTP roots also for SFTP?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpUserRootEnabled { get; set; }

        /// <summary>
        /// Is password reset disabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DisablePasswordReset { get; set; }

        /// <summary>
        /// Are files protected from modification?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ImmutableFiles { get; set; }

        /// <summary>
        /// Custom error message to show when bundle is not found.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string BundleNotFoundMessage { get; set; }

        /// <summary>
        /// Do Bundles require password protection?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundlePasswordRequired { get; set; }

        /// <summary>
        /// Do Bundles require registration?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundleRequireRegistration { get; set; }

        /// <summary>
        /// Do Bundles require recipients for sharing?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundleRequireShareRecipient { get; set; }

        /// <summary>
        /// If true, new Share Links must send a one-time password to the recipient when they register. Requires bundle_require_share_recipient and cannot be enabled with bundle_password_required.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundleSendOneTimePasswordToRecipientAtRegistration { get; set; }

        /// <summary>
        /// Do Bundle owners receive registration notification?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string BundleRegistrationNotifications { get; set; }

        /// <summary>
        /// Do Bundle owners receive activity notifications?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string BundleActivityNotifications { get; set; }

        /// <summary>
        /// Do Bundle uploaders receive upload confirmation notifications?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string BundleUploadReceiptNotifications { get; set; }

        /// <summary>
        /// If true, allow public viewers of Bundles with full permissions to use document editing integrations.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DocumentEditsInBundleAllowed { get; set; }

        /// <summary>
        /// Require bundles' passwords, and passwords for other items (inboxes, public shares, etc.) to conform to the same requirements as users' passwords?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PasswordRequirementsApplyToBundles { get; set; }

        /// <summary>
        /// If true, we will prevent non-administrators from receiving any permissions directly on the root folder.  This is commonly used to prevent the accidental application of permissions.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PreventRootPermissionsForNonSiteAdmins { get; set; }

        /// <summary>
        /// If true, only site admins may create, modify, or delete any behavior at the site root, or a skip that would disable one.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RestrictRootFolderBehaviorsToSiteAdmins { get; set; }

        /// <summary>
        /// If true, supported protective behaviors at the site root also apply within named workspaces. Requires restrict_root_folder_behaviors_to_site_admins to be enabled.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RootFolderBehaviorsApplyToWorkspaces { get; set; }

        /// <summary>
        /// Use servers in the USA only?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? OptOutGlobal { get; set; }

        /// <summary>
        /// Allow uploaders to set `provided_modified_at` for uploaded files?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UseProvidedModifiedAt { get; set; }

        /// <summary>
        /// Is this site using a custom namespace for users?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? CustomNamespace { get; set; }

        /// <summary>
        /// If true, groups can be manually created / modified / deleted by Site Admins. Otherwise, groups can only be managed via your SSO provider.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NonSsoGroupsAllowed { get; set; }

        /// <summary>
        /// If true, users can be manually created / modified / deleted by Site Admins. Otherwise, users can only be managed via your SSO provider.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? NonSsoUsersAllowed { get; set; }

        /// <summary>
        /// Allow bundle creation
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SharingEnabled { get; set; }

        /// <summary>
        /// Allow snapshot share links creation
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SnapshotSharingEnabled { get; set; }

        /// <summary>
        /// Enable User Requests feature
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UserRequestsEnabled { get; set; }

        /// <summary>
        /// Send email to site admins when a user request is received?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UserRequestsNotifyAdmins { get; set; }

        /// <summary>
        /// Is WebDAV enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DavEnabled { get; set; }

        /// <summary>
        /// Allow other Files.com sites to use this site's API keys for native Files.com Remote Server connections? Defaults to true and applies to all keys and workspaces on this site. When false, new pairings and access through existing connections are rejected without revoking keys or deleting connections. Re-enabling permits access again with usable keys. Does not disable this site's connections to other sites, ordinary API access, or Connected Sites.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FilesComRemoteServerEnabled { get; set; }

        /// <summary>
        /// Is FTP enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FtpEnabled { get; set; }

        /// <summary>
        /// Is the S3-compatible endpoint enabled for all users and workspaces on this site? Defaults to true. When false, user and group S3 permissions do not allow access.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? S3CompatibleEndpointEnabled { get; set; }

        /// <summary>
        /// Is SFTP enabled?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpEnabled { get; set; }

        /// <summary>
        /// Finalize partial SFTP uploads from interrupted connections? Default: true.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpFinalizePartialUploads { get; set; }

        /// <summary>
        /// Allow users to create their own API keys?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UsersCanCreateApiKeys { get; set; }

        /// <summary>
        /// Allow users to create their own SSH keys?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UsersCanCreateSshKeys { get; set; }

        /// <summary>
        /// Show log in link in user notifications?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ShowUserNotificationsLogInLink { get; set; }

        /// <summary>
        /// Sftp Host Key Type
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SftpHostKeyType { get; set; }

        /// <summary>
        /// Id of the currently selected custom SFTP Host Key
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ActiveSftpHostKeyId { get; set; }

        /// <summary>
        /// Ids of the selected custom SFTP Host Keys
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ActiveSftpHostKeyIds { get; set; }

        /// <summary>
        /// If true, protocol access permissions on users will be ignored, and only protocol access permissions set on Groups will be honored.  Make sure that your current user is a member of a group with API permission when changing this value to avoid locking yourself out of your site.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ProtocolAccessGroupsOnly { get; set; }

        /// <summary>
        /// Auto-removes bundles for disabled/deleted users and enforces bundle expiry within user access period.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RevokeBundleAccessOnDisableOrDelete { get; set; }

        /// <summary>
        /// Allow group admins to create users in their groups
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GroupAdminsCanAddUsers { get; set; }

        /// <summary>
        /// Allow group admins to add or remove existing users in their groups
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GroupAdminsCanManageGroupMemberships { get; set; }

        /// <summary>
        /// Allow group admins to delete users in their groups
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GroupAdminsCanDeleteUsers { get; set; }

        /// <summary>
        /// Allow group admins to enable or disable users in their groups
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GroupAdminsCanEnableDisableUsers { get; set; }

        /// <summary>
        /// Allow group admins to modify users in their groups
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GroupAdminsCanModifyUsers { get; set; }

        /// <summary>
        /// Allow group admins to exempt users in their groups from lifecycle rules
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GroupAdminsCanBypassUserLifecycleRules { get; set; }

        /// <summary>
        /// Allow group admins to reset passwords for users in their groups
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GroupAdminsCanResetPasswords { get; set; }

        /// <summary>
        /// Allow group admins to set password authentication method
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GroupAdminsCanSetUserPassword { get; set; }

        /// <summary>
        /// If true, administrators on this site's Child Sites, including Folder Admins, can list this site's Default Workspace users and groups and grant them folder permissions and notifications on Child Site paths.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ChildSiteFolderAdminsCanSeeParentSiteDirectory { get; set; }

        /// <summary>
        /// If true, Workspace Administrators and Folder Admins in a Custom Workspace can list the Default Workspace users and groups that have permissions in that Workspace and choose them as Email Notification recipients.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WorkspaceFolderAdminsCanSeeDefaultWorkspaceDirectory { get; set; }

        /// <summary>
        /// Disallow free email domains for Bundle/Inbox recipients?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundleRecipientBlacklistFreeEmailDomains { get; set; }

        /// <summary>
        /// List of email domains to disallow when entering a Bundle/Inbox recipients
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] BundleRecipientBlacklistDomains { get; set; }

        /// <summary>
        /// Allow admins to bypass the locked subfolders setting.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AdminsBypassLockedSubfolders { get; set; }

        /// <summary>
        /// Is SMS two factor authentication allowed?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Allowed2faMethodSms { get; set; }

        /// <summary>
        /// Is TOTP two factor authentication allowed?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Allowed2faMethodTotp { get; set; }

        /// <summary>
        /// Is WebAuthn two factor authentication allowed?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Allowed2faMethodWebauthn { get; set; }

        /// <summary>
        /// Is yubikey two factor authentication allowed?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Allowed2faMethodYubi { get; set; }

        /// <summary>
        /// Is OTP via email two factor authentication allowed?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Allowed2faMethodEmail { get; set; }

        /// <summary>
        /// Is OTP via static codes for two factor authentication allowed?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Allowed2faMethodStatic { get; set; }

        /// <summary>
        /// Are users allowed to configure their two factor authentication to be bypassed for FTP/SFTP/WebDAV?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Allowed2faMethodBypassForFtpSftpDav { get; set; }

        /// <summary>
        /// Require two-factor authentication for all users?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Require2fa { get; set; }

        /// <summary>
        /// If true, SSO users using the default user-level two-factor authentication setting are exempt from the site-wide two-factor authentication requirement.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Require2faExemptAllSsoUsers { get; set; }

        /// <summary>
        /// What type of user is required to use two-factor authentication (when require_2fa is set to `true` for this site)?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Require2faUserType { get; set; }

        /// <summary>
        /// Top bar background color
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Color2Top { get; set; }

        /// <summary>
        /// Page link and button color
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Color2Left { get; set; }

        /// <summary>
        /// Top bar link color
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Color2Link { get; set; }

        /// <summary>
        /// Page link and button color
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Color2Text { get; set; }

        /// <summary>
        /// Top bar text color
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Color2TopText { get; set; }

        /// <summary>
        /// Custom site header text for authenticated pages
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SiteHeader { get; set; }

        /// <summary>
        /// Custom site footer text for authenticated pages
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SiteFooter { get; set; }

        /// <summary>
        /// Custom site header text for public pages
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SitePublicHeader { get; set; }

        /// <summary>
        /// Custom site footer text for public pages
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SitePublicFooter { get; set; }

        /// <summary>
        /// Login help text
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string LoginHelpText { get; set; }

        /// <summary>
        /// If using custom SMTP, should we use dedicated IPs to deliver emails?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UseDedicatedIpsForSmtp { get; set; }

        /// <summary>
        /// Custom footer text for system-generated emails (as Markdown). Supports standard strftime date/time patterns like %Y (4-digit year), %m (month), %d (day).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string EmailFooterCustomText { get; set; }

        /// <summary>
        /// SMTP server hostname or IP
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SmtpAddress { get; set; }

        /// <summary>
        /// SMTP server authentication type
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SmtpAuthentication { get; set; }

        /// <summary>
        /// From address to use when mailing through custom SMTP
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SmtpFrom { get; set; }

        /// <summary>
        /// SMTP server username
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SmtpUsername { get; set; }

        /// <summary>
        /// SMTP server port
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SmtpPort { get; set; }

        /// <summary>
        /// Custom SMTP encryption mode: if_available (default) uses STARTTLS when offered and otherwise sends credentials and messages unencrypted. A certificate-verified STARTTLS connection automatically changes if_available to require unless smtp_ssl is managed by a parent policy. require requires STARTTLS before authentication; require_implicit uses TLS from connection start; never disables TLS. TLS verifies the server certificate against smtp_address. Encryption is never automatically downgraded.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SmtpSsl { get; set; }

        /// <summary>
        /// Require email authentication, virus, and spam checks for incoming emails to Inboxes and Incoming Email Automations in every Workspace on this site?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UploadsViaEmailAuthentication { get; set; }

        /// <summary>
        /// Preview watermark settings applied to all bundle items. Uses the same keys as Behavior.value
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary BundleWatermarkValue { get; set; }

        /// <summary>
        /// icon16_file
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment Icon16File { get; set; }

        /// <summary>
        /// If true, will delete the file stored in icon16
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Icon16Delete { get; set; }

        /// <summary>
        /// icon32_file
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment Icon32File { get; set; }

        /// <summary>
        /// If true, will delete the file stored in icon32
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Icon32Delete { get; set; }

        /// <summary>
        /// icon48_file
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment Icon48File { get; set; }

        /// <summary>
        /// If true, will delete the file stored in icon48
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Icon48Delete { get; set; }

        /// <summary>
        /// icon128_file
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment Icon128File { get; set; }

        /// <summary>
        /// If true, will delete the file stored in icon128
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Icon128Delete { get; set; }

        /// <summary>
        /// logo_file
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment LogoFile { get; set; }

        /// <summary>
        /// If true, will delete the file stored in logo
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? LogoDelete { get; set; }

        /// <summary>
        /// bundle_watermark_attachment_file
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment BundleWatermarkAttachmentFile { get; set; }

        /// <summary>
        /// If true, will delete the file stored in bundle_watermark_attachment
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BundleWatermarkAttachmentDelete { get; set; }

        /// <summary>
        /// login_page_background_image_file
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment LoginPageBackgroundImageFile { get; set; }

        /// <summary>
        /// If true, will delete the file stored in login_page_background_image
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? LoginPageBackgroundImageDelete { get; set; }

        /// <summary>
        /// If set to true, we will begin the process of disabling 2FA on this site.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Disable2faWithDelay { get; set; }

        /// <summary>
        /// If true, and if changing the site subdomain, then create a redirect from the previous Files.com subdomain to the new Files.com subdomain.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RedirectOldSubdomain { get; set; }

        /// <summary>
        /// Password for SMTP server.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString SmtpPassword { get; set; }

        /// <summary>Update Site Settings.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Site '{Name}'" : "Site"))
            {
                return;
            }
            Models.Site result = Run(connection, token => SiteBridge.UpdateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Subdomain)))
            {
                parameters["subdomain"] = Subdomain;
            }
            if (IsBound(nameof(Domain)))
            {
                parameters["domain"] = Domain;
            }
            if (IsBound(nameof(DomainHstsHeader)))
            {
                parameters["domain_hsts_header"] = DomainHstsHeader.Value;
            }
            if (IsBound(nameof(DomainLetsencryptChain)))
            {
                parameters["domain_letsencrypt_chain"] = DomainLetsencryptChain;
            }
            if (IsBound(nameof(Email)))
            {
                parameters["email"] = Email;
            }
            if (IsBound(nameof(ReplyToEmail)))
            {
                parameters["reply_to_email"] = ReplyToEmail;
            }
            if (IsBound(nameof(AllowBundleNames)))
            {
                parameters["allow_bundle_names"] = AllowBundleNames.Value;
            }
            if (IsBound(nameof(BundleExpiration)))
            {
                parameters["bundle_expiration"] = BundleExpiration.Value;
            }
            if (IsBound(nameof(WelcomeEmailEnabled)))
            {
                parameters["welcome_email_enabled"] = WelcomeEmailEnabled.Value;
            }
            if (IsBound(nameof(AskAboutOverwrites)))
            {
                parameters["ask_about_overwrites"] = AskAboutOverwrites.Value;
            }
            if (IsBound(nameof(ShowRequestAccessLink)))
            {
                parameters["show_request_access_link"] = ShowRequestAccessLink.Value;
            }
            if (IsBound(nameof(AlwaysMkdirParents)))
            {
                parameters["always_mkdir_parents"] = AlwaysMkdirParents.Value;
            }
            if (IsBound(nameof(WelcomeEmailCc)))
            {
                parameters["welcome_email_cc"] = WelcomeEmailCc;
            }
            if (IsBound(nameof(WelcomeEmailSubject)))
            {
                parameters["welcome_email_subject"] = WelcomeEmailSubject;
            }
            if (IsBound(nameof(WelcomeCustomText)))
            {
                parameters["welcome_custom_text"] = WelcomeCustomText;
            }
            if (IsBound(nameof(Language)))
            {
                parameters["language"] = Language;
            }
            if (IsBound(nameof(WindowsModeFtp)))
            {
                parameters["windows_mode_ftp"] = WindowsModeFtp.Value;
            }
            if (IsBound(nameof(DefaultTimeZone)))
            {
                parameters["default_time_zone"] = DefaultTimeZone;
            }
            if (IsBound(nameof(DesktopApp)))
            {
                parameters["desktop_app"] = DesktopApp.Value;
            }
            if (IsBound(nameof(DesktopAppSessionIpPinning)))
            {
                parameters["desktop_app_session_ip_pinning"] = DesktopAppSessionIpPinning.Value;
            }
            if (IsBound(nameof(DesktopAppSessionLifetime)))
            {
                parameters["desktop_app_session_lifetime"] = DesktopAppSessionLifetime.Value;
            }
            if (IsBound(nameof(MobileApp)))
            {
                parameters["mobile_app"] = MobileApp.Value;
            }
            if (IsBound(nameof(MobileAppSessionIpPinning)))
            {
                parameters["mobile_app_session_ip_pinning"] = MobileAppSessionIpPinning.Value;
            }
            if (IsBound(nameof(MobileAppSessionLifetime)))
            {
                parameters["mobile_app_session_lifetime"] = MobileAppSessionLifetime.Value;
            }
            if (IsBound(nameof(FolderPermissionsGroupsOnly)))
            {
                parameters["folder_permissions_groups_only"] = FolderPermissionsGroupsOnly.Value;
            }
            if (IsBound(nameof(WelcomeScreen)))
            {
                parameters["welcome_screen"] = WelcomeScreen;
            }
            if (IsBound(nameof(OfficeIntegrationAvailable)))
            {
                parameters["office_integration_available"] = OfficeIntegrationAvailable.Value;
            }
            if (IsBound(nameof(OfficeIntegrationType)))
            {
                parameters["office_integration_type"] = OfficeIntegrationType;
            }
            if (IsBound(nameof(PinAllRemoteServersToSiteRegion)))
            {
                parameters["pin_all_remote_servers_to_site_region"] = PinAllRemoteServersToSiteRegion.Value;
            }
            if (IsBound(nameof(MotdText)))
            {
                parameters["motd_text"] = MotdText;
            }
            if (IsBound(nameof(MotdUseForFtp)))
            {
                parameters["motd_use_for_ftp"] = MotdUseForFtp.Value;
            }
            if (IsBound(nameof(MotdUseForSftp)))
            {
                parameters["motd_use_for_sftp"] = MotdUseForSftp.Value;
            }
            if (IsBound(nameof(LeftNavigationVisibility)))
            {
                parameters["left_navigation_visibility"] = RequestValues.Object(LeftNavigationVisibility, "LeftNavigationVisibility");
            }
            if (IsBound(nameof(DisableAllAiFeatures)))
            {
                parameters["disable_all_ai_features"] = DisableAllAiFeatures.Value;
            }
            if (IsBound(nameof(AiFeatureAvailability)))
            {
                parameters["ai_feature_availability"] = RequestValues.Object(AiFeatureAvailability, "AiFeatureAvailability");
            }
            if (IsBound(nameof(McpDcrEnabled)))
            {
                parameters["mcp_dcr_enabled"] = McpDcrEnabled.Value;
            }
            if (IsBound(nameof(AdditionalTextFileTypes)))
            {
                parameters["additional_text_file_types"] = RequestValues.Strings(AdditionalTextFileTypes, "AdditionalTextFileTypes");
            }
            if (IsBound(nameof(BundleRequireNote)))
            {
                parameters["bundle_require_note"] = BundleRequireNote.Value;
            }
            if (IsBound(nameof(BundleSendSharedReceipts)))
            {
                parameters["bundle_send_shared_receipts"] = BundleSendSharedReceipts.Value;
            }
            if (IsBound(nameof(BundlesDefaultOwnedByPrimaryGroup)))
            {
                parameters["bundles_default_owned_by_primary_group"] = BundlesDefaultOwnedByPrimaryGroup.Value;
            }
            if (IsBound(nameof(CalculateFileChecksumsCrc32)))
            {
                parameters["calculate_file_checksums_crc32"] = CalculateFileChecksumsCrc32.Value;
            }
            if (IsBound(nameof(CalculateFileChecksumsMd5)))
            {
                parameters["calculate_file_checksums_md5"] = CalculateFileChecksumsMd5.Value;
            }
            if (IsBound(nameof(CalculateFileChecksumsSha1)))
            {
                parameters["calculate_file_checksums_sha1"] = CalculateFileChecksumsSha1.Value;
            }
            if (IsBound(nameof(CalculateFileChecksumsSha256)))
            {
                parameters["calculate_file_checksums_sha256"] = CalculateFileChecksumsSha256.Value;
            }
            if (IsBound(nameof(LegacyChecksumsMode)))
            {
                parameters["legacy_checksums_mode"] = LegacyChecksumsMode.Value;
            }
            if (IsBound(nameof(MigrateRemoteServerSyncToSync)))
            {
                parameters["migrate_remote_server_sync_to_sync"] = MigrateRemoteServerSyncToSync.Value;
            }
            if (IsBound(nameof(As2MessageRetentionDays)))
            {
                parameters["as2_message_retention_days"] = As2MessageRetentionDays.Value;
            }
            if (IsBound(nameof(FileActivityEventsEnabled)))
            {
                parameters["file_activity_events_enabled"] = FileActivityEventsEnabled.Value;
            }
            if (IsBound(nameof(UsernameDisplay)))
            {
                parameters["username_display"] = UsernameDisplay;
            }
            if (IsBound(nameof(IntegrationCentricRemoteServerAdminAccess)))
            {
                parameters["integration_centric_remote_server_admin_access"] = IntegrationCentricRemoteServerAdminAccess;
            }
            if (IsBound(nameof(SessionExpiryMinutes)))
            {
                parameters["session_expiry_minutes"] = SessionExpiryMinutes.Value;
            }
            if (IsBound(nameof(LegacyPairingEnabled)))
            {
                parameters["legacy_pairing_enabled"] = LegacyPairingEnabled.Value;
            }
            if (IsBound(nameof(SslRequired)))
            {
                parameters["ssl_required"] = SslRequired.Value;
            }
            if (IsBound(nameof(FtpTlsSessionReuseRequired)))
            {
                parameters["ftp_tls_session_reuse_required"] = FtpTlsSessionReuseRequired.Value;
            }
            if (IsBound(nameof(SftpInsecureCiphers)))
            {
                parameters["sftp_insecure_ciphers"] = SftpInsecureCiphers.Value;
            }
            if (IsBound(nameof(SftpInsecureDiffieHellman)))
            {
                parameters["sftp_insecure_diffie_hellman"] = SftpInsecureDiffieHellman.Value;
            }
            if (IsBound(nameof(DisableFilesCertificateGeneration)))
            {
                parameters["disable_files_certificate_generation"] = DisableFilesCertificateGeneration.Value;
            }
            if (IsBound(nameof(Fedramp)))
            {
                parameters["fedramp"] = Fedramp.Value;
            }
            if (IsBound(nameof(UserLockout)))
            {
                parameters["user_lockout"] = UserLockout.Value;
            }
            if (IsBound(nameof(UserLockoutTries)))
            {
                parameters["user_lockout_tries"] = UserLockoutTries.Value;
            }
            if (IsBound(nameof(UserLockoutWithin)))
            {
                parameters["user_lockout_within"] = UserLockoutWithin.Value;
            }
            if (IsBound(nameof(UserLockoutLockPeriod)))
            {
                parameters["user_lockout_lock_period"] = UserLockoutLockPeriod.Value;
            }
            if (IsBound(nameof(IncludePasswordInWelcomeEmail)))
            {
                parameters["include_password_in_welcome_email"] = IncludePasswordInWelcomeEmail.Value;
            }
            if (IsBound(nameof(AllowedCountries)))
            {
                parameters["allowed_countries"] = RequestValues.Join(AllowedCountries, "AllowedCountries", ",");
            }
            if (IsBound(nameof(AllowedIps)))
            {
                parameters["allowed_ips"] = AllowedIps;
            }
            if (IsBound(nameof(AllowUserLevel2faOverride)))
            {
                parameters["allow_user_level_2fa_override"] = AllowUserLevel2faOverride.Value;
            }
            if (IsBound(nameof(AllowUserLevelAllowedIpOverride)))
            {
                parameters["allow_user_level_allowed_ip_override"] = AllowUserLevelAllowedIpOverride.Value;
            }
            if (IsBound(nameof(AllowUserLevelSslOverride)))
            {
                parameters["allow_user_level_ssl_override"] = AllowUserLevelSslOverride.Value;
            }
            if (IsBound(nameof(AllowUserLevelFtpTlsSessionReuseOverride)))
            {
                parameters["allow_user_level_ftp_tls_session_reuse_override"] = AllowUserLevelFtpTlsSessionReuseOverride.Value;
            }
            if (IsBound(nameof(EnforceParentSiteUserIpRestrictions)))
            {
                parameters["enforce_parent_site_user_ip_restrictions"] = EnforceParentSiteUserIpRestrictions.Value;
            }
            if (IsBound(nameof(DisallowedCountries)))
            {
                parameters["disallowed_countries"] = RequestValues.Join(DisallowedCountries, "DisallowedCountries", ",");
            }
            if (IsBound(nameof(DaysToRetainBackups)))
            {
                parameters["days_to_retain_backups"] = DaysToRetainBackups.Value;
            }
            if (IsBound(nameof(MaxPriorPasswords)))
            {
                parameters["max_prior_passwords"] = MaxPriorPasswords.Value;
            }
            if (IsBound(nameof(PasswordValidityDays)))
            {
                parameters["password_validity_days"] = PasswordValidityDays.Value;
            }
            if (IsBound(nameof(PasswordMinLength)))
            {
                parameters["password_min_length"] = PasswordMinLength.Value;
            }
            if (IsBound(nameof(PasswordRequireLetter)))
            {
                parameters["password_require_letter"] = PasswordRequireLetter.Value;
            }
            if (IsBound(nameof(PasswordRequireMixed)))
            {
                parameters["password_require_mixed"] = PasswordRequireMixed.Value;
            }
            if (IsBound(nameof(PasswordRequireSpecial)))
            {
                parameters["password_require_special"] = PasswordRequireSpecial.Value;
            }
            if (IsBound(nameof(PasswordRequireNumber)))
            {
                parameters["password_require_number"] = PasswordRequireNumber.Value;
            }
            if (IsBound(nameof(PasswordRequireUnbreached)))
            {
                parameters["password_require_unbreached"] = PasswordRequireUnbreached.Value;
            }
            if (IsBound(nameof(RequireLogoutFromBundlesAndInboxes)))
            {
                parameters["require_logout_from_bundles_and_inboxes"] = RequireLogoutFromBundlesAndInboxes.Value;
            }
            if (IsBound(nameof(DavUserRootEnabled)))
            {
                parameters["dav_user_root_enabled"] = DavUserRootEnabled.Value;
            }
            if (IsBound(nameof(SftpUserRootEnabled)))
            {
                parameters["sftp_user_root_enabled"] = SftpUserRootEnabled.Value;
            }
            if (IsBound(nameof(DisablePasswordReset)))
            {
                parameters["disable_password_reset"] = DisablePasswordReset.Value;
            }
            if (IsBound(nameof(ImmutableFiles)))
            {
                parameters["immutable_files"] = ImmutableFiles.Value;
            }
            if (IsBound(nameof(BundleNotFoundMessage)))
            {
                parameters["bundle_not_found_message"] = BundleNotFoundMessage;
            }
            if (IsBound(nameof(BundlePasswordRequired)))
            {
                parameters["bundle_password_required"] = BundlePasswordRequired.Value;
            }
            if (IsBound(nameof(BundleRequireRegistration)))
            {
                parameters["bundle_require_registration"] = BundleRequireRegistration.Value;
            }
            if (IsBound(nameof(BundleRequireShareRecipient)))
            {
                parameters["bundle_require_share_recipient"] = BundleRequireShareRecipient.Value;
            }
            if (IsBound(nameof(BundleSendOneTimePasswordToRecipientAtRegistration)))
            {
                parameters["bundle_send_one_time_password_to_recipient_at_registration"] = BundleSendOneTimePasswordToRecipientAtRegistration.Value;
            }
            if (IsBound(nameof(BundleRegistrationNotifications)))
            {
                parameters["bundle_registration_notifications"] = BundleRegistrationNotifications;
            }
            if (IsBound(nameof(BundleActivityNotifications)))
            {
                parameters["bundle_activity_notifications"] = BundleActivityNotifications;
            }
            if (IsBound(nameof(BundleUploadReceiptNotifications)))
            {
                parameters["bundle_upload_receipt_notifications"] = BundleUploadReceiptNotifications;
            }
            if (IsBound(nameof(DocumentEditsInBundleAllowed)))
            {
                parameters["document_edits_in_bundle_allowed"] = DocumentEditsInBundleAllowed.Value;
            }
            if (IsBound(nameof(PasswordRequirementsApplyToBundles)))
            {
                parameters["password_requirements_apply_to_bundles"] = PasswordRequirementsApplyToBundles.Value;
            }
            if (IsBound(nameof(PreventRootPermissionsForNonSiteAdmins)))
            {
                parameters["prevent_root_permissions_for_non_site_admins"] = PreventRootPermissionsForNonSiteAdmins.Value;
            }
            if (IsBound(nameof(RestrictRootFolderBehaviorsToSiteAdmins)))
            {
                parameters["restrict_root_folder_behaviors_to_site_admins"] = RestrictRootFolderBehaviorsToSiteAdmins.Value;
            }
            if (IsBound(nameof(RootFolderBehaviorsApplyToWorkspaces)))
            {
                parameters["root_folder_behaviors_apply_to_workspaces"] = RootFolderBehaviorsApplyToWorkspaces.Value;
            }
            if (IsBound(nameof(OptOutGlobal)))
            {
                parameters["opt_out_global"] = OptOutGlobal.Value;
            }
            if (IsBound(nameof(UseProvidedModifiedAt)))
            {
                parameters["use_provided_modified_at"] = UseProvidedModifiedAt.Value;
            }
            if (IsBound(nameof(CustomNamespace)))
            {
                parameters["custom_namespace"] = CustomNamespace.Value;
            }
            if (IsBound(nameof(NonSsoGroupsAllowed)))
            {
                parameters["non_sso_groups_allowed"] = NonSsoGroupsAllowed.Value;
            }
            if (IsBound(nameof(NonSsoUsersAllowed)))
            {
                parameters["non_sso_users_allowed"] = NonSsoUsersAllowed.Value;
            }
            if (IsBound(nameof(SharingEnabled)))
            {
                parameters["sharing_enabled"] = SharingEnabled.Value;
            }
            if (IsBound(nameof(SnapshotSharingEnabled)))
            {
                parameters["snapshot_sharing_enabled"] = SnapshotSharingEnabled.Value;
            }
            if (IsBound(nameof(UserRequestsEnabled)))
            {
                parameters["user_requests_enabled"] = UserRequestsEnabled.Value;
            }
            if (IsBound(nameof(UserRequestsNotifyAdmins)))
            {
                parameters["user_requests_notify_admins"] = UserRequestsNotifyAdmins.Value;
            }
            if (IsBound(nameof(DavEnabled)))
            {
                parameters["dav_enabled"] = DavEnabled.Value;
            }
            if (IsBound(nameof(FilesComRemoteServerEnabled)))
            {
                parameters["files_com_remote_server_enabled"] = FilesComRemoteServerEnabled.Value;
            }
            if (IsBound(nameof(FtpEnabled)))
            {
                parameters["ftp_enabled"] = FtpEnabled.Value;
            }
            if (IsBound(nameof(S3CompatibleEndpointEnabled)))
            {
                parameters["s3_compatible_endpoint_enabled"] = S3CompatibleEndpointEnabled.Value;
            }
            if (IsBound(nameof(SftpEnabled)))
            {
                parameters["sftp_enabled"] = SftpEnabled.Value;
            }
            if (IsBound(nameof(SftpFinalizePartialUploads)))
            {
                parameters["sftp_finalize_partial_uploads"] = SftpFinalizePartialUploads.Value;
            }
            if (IsBound(nameof(UsersCanCreateApiKeys)))
            {
                parameters["users_can_create_api_keys"] = UsersCanCreateApiKeys.Value;
            }
            if (IsBound(nameof(UsersCanCreateSshKeys)))
            {
                parameters["users_can_create_ssh_keys"] = UsersCanCreateSshKeys.Value;
            }
            if (IsBound(nameof(ShowUserNotificationsLogInLink)))
            {
                parameters["show_user_notifications_log_in_link"] = ShowUserNotificationsLogInLink.Value;
            }
            if (IsBound(nameof(SftpHostKeyType)))
            {
                parameters["sftp_host_key_type"] = SftpHostKeyType;
            }
            if (IsBound(nameof(ActiveSftpHostKeyId)))
            {
                parameters["active_sftp_host_key_id"] = ActiveSftpHostKeyId.Value;
            }
            if (IsBound(nameof(ActiveSftpHostKeyIds)))
            {
                parameters["active_sftp_host_key_ids"] = RequestValues.Int64s(ActiveSftpHostKeyIds);
            }
            if (IsBound(nameof(ProtocolAccessGroupsOnly)))
            {
                parameters["protocol_access_groups_only"] = ProtocolAccessGroupsOnly.Value;
            }
            if (IsBound(nameof(RevokeBundleAccessOnDisableOrDelete)))
            {
                parameters["revoke_bundle_access_on_disable_or_delete"] = RevokeBundleAccessOnDisableOrDelete.Value;
            }
            if (IsBound(nameof(GroupAdminsCanAddUsers)))
            {
                parameters["group_admins_can_add_users"] = GroupAdminsCanAddUsers.Value;
            }
            if (IsBound(nameof(GroupAdminsCanManageGroupMemberships)))
            {
                parameters["group_admins_can_manage_group_memberships"] = GroupAdminsCanManageGroupMemberships.Value;
            }
            if (IsBound(nameof(GroupAdminsCanDeleteUsers)))
            {
                parameters["group_admins_can_delete_users"] = GroupAdminsCanDeleteUsers.Value;
            }
            if (IsBound(nameof(GroupAdminsCanEnableDisableUsers)))
            {
                parameters["group_admins_can_enable_disable_users"] = GroupAdminsCanEnableDisableUsers.Value;
            }
            if (IsBound(nameof(GroupAdminsCanModifyUsers)))
            {
                parameters["group_admins_can_modify_users"] = GroupAdminsCanModifyUsers.Value;
            }
            if (IsBound(nameof(GroupAdminsCanBypassUserLifecycleRules)))
            {
                parameters["group_admins_can_bypass_user_lifecycle_rules"] = GroupAdminsCanBypassUserLifecycleRules.Value;
            }
            if (IsBound(nameof(GroupAdminsCanResetPasswords)))
            {
                parameters["group_admins_can_reset_passwords"] = GroupAdminsCanResetPasswords.Value;
            }
            if (IsBound(nameof(GroupAdminsCanSetUserPassword)))
            {
                parameters["group_admins_can_set_user_password"] = GroupAdminsCanSetUserPassword.Value;
            }
            if (IsBound(nameof(ChildSiteFolderAdminsCanSeeParentSiteDirectory)))
            {
                parameters["child_site_folder_admins_can_see_parent_site_directory"] = ChildSiteFolderAdminsCanSeeParentSiteDirectory.Value;
            }
            if (IsBound(nameof(WorkspaceFolderAdminsCanSeeDefaultWorkspaceDirectory)))
            {
                parameters["workspace_folder_admins_can_see_default_workspace_directory"] = WorkspaceFolderAdminsCanSeeDefaultWorkspaceDirectory.Value;
            }
            if (IsBound(nameof(BundleRecipientBlacklistFreeEmailDomains)))
            {
                parameters["bundle_recipient_blacklist_free_email_domains"] = BundleRecipientBlacklistFreeEmailDomains.Value;
            }
            if (IsBound(nameof(BundleRecipientBlacklistDomains)))
            {
                parameters["bundle_recipient_blacklist_domains"] = RequestValues.Strings(BundleRecipientBlacklistDomains, "BundleRecipientBlacklistDomains");
            }
            if (IsBound(nameof(AdminsBypassLockedSubfolders)))
            {
                parameters["admins_bypass_locked_subfolders"] = AdminsBypassLockedSubfolders.Value;
            }
            if (IsBound(nameof(Allowed2faMethodSms)))
            {
                parameters["allowed_2fa_method_sms"] = Allowed2faMethodSms.Value;
            }
            if (IsBound(nameof(Allowed2faMethodTotp)))
            {
                parameters["allowed_2fa_method_totp"] = Allowed2faMethodTotp.Value;
            }
            if (IsBound(nameof(Allowed2faMethodWebauthn)))
            {
                parameters["allowed_2fa_method_webauthn"] = Allowed2faMethodWebauthn.Value;
            }
            if (IsBound(nameof(Allowed2faMethodYubi)))
            {
                parameters["allowed_2fa_method_yubi"] = Allowed2faMethodYubi.Value;
            }
            if (IsBound(nameof(Allowed2faMethodEmail)))
            {
                parameters["allowed_2fa_method_email"] = Allowed2faMethodEmail.Value;
            }
            if (IsBound(nameof(Allowed2faMethodStatic)))
            {
                parameters["allowed_2fa_method_static"] = Allowed2faMethodStatic.Value;
            }
            if (IsBound(nameof(Allowed2faMethodBypassForFtpSftpDav)))
            {
                parameters["allowed_2fa_method_bypass_for_ftp_sftp_dav"] = Allowed2faMethodBypassForFtpSftpDav.Value;
            }
            if (IsBound(nameof(Require2fa)))
            {
                parameters["require_2fa"] = Require2fa.Value;
            }
            if (IsBound(nameof(Require2faExemptAllSsoUsers)))
            {
                parameters["require_2fa_exempt_all_sso_users"] = Require2faExemptAllSsoUsers.Value;
            }
            if (IsBound(nameof(Require2faUserType)))
            {
                parameters["require_2fa_user_type"] = Require2faUserType;
            }
            if (IsBound(nameof(Color2Top)))
            {
                parameters["color2_top"] = Color2Top;
            }
            if (IsBound(nameof(Color2Left)))
            {
                parameters["color2_left"] = Color2Left;
            }
            if (IsBound(nameof(Color2Link)))
            {
                parameters["color2_link"] = Color2Link;
            }
            if (IsBound(nameof(Color2Text)))
            {
                parameters["color2_text"] = Color2Text;
            }
            if (IsBound(nameof(Color2TopText)))
            {
                parameters["color2_top_text"] = Color2TopText;
            }
            if (IsBound(nameof(SiteHeader)))
            {
                parameters["site_header"] = SiteHeader;
            }
            if (IsBound(nameof(SiteFooter)))
            {
                parameters["site_footer"] = SiteFooter;
            }
            if (IsBound(nameof(SitePublicHeader)))
            {
                parameters["site_public_header"] = SitePublicHeader;
            }
            if (IsBound(nameof(SitePublicFooter)))
            {
                parameters["site_public_footer"] = SitePublicFooter;
            }
            if (IsBound(nameof(LoginHelpText)))
            {
                parameters["login_help_text"] = LoginHelpText;
            }
            if (IsBound(nameof(UseDedicatedIpsForSmtp)))
            {
                parameters["use_dedicated_ips_for_smtp"] = UseDedicatedIpsForSmtp.Value;
            }
            if (IsBound(nameof(EmailFooterCustomText)))
            {
                parameters["email_footer_custom_text"] = EmailFooterCustomText;
            }
            if (IsBound(nameof(SmtpAddress)))
            {
                parameters["smtp_address"] = SmtpAddress;
            }
            if (IsBound(nameof(SmtpAuthentication)))
            {
                parameters["smtp_authentication"] = SmtpAuthentication;
            }
            if (IsBound(nameof(SmtpFrom)))
            {
                parameters["smtp_from"] = SmtpFrom;
            }
            if (IsBound(nameof(SmtpUsername)))
            {
                parameters["smtp_username"] = SmtpUsername;
            }
            if (IsBound(nameof(SmtpPort)))
            {
                parameters["smtp_port"] = SmtpPort.Value;
            }
            if (IsBound(nameof(SmtpSsl)))
            {
                parameters["smtp_ssl"] = SmtpSsl;
            }
            if (IsBound(nameof(UploadsViaEmailAuthentication)))
            {
                parameters["uploads_via_email_authentication"] = UploadsViaEmailAuthentication.Value;
            }
            if (IsBound(nameof(BundleWatermarkValue)))
            {
                parameters["bundle_watermark_value"] = RequestValues.Object(BundleWatermarkValue, "BundleWatermarkValue");
            }
            if (IsBound(nameof(Icon16File)))
            {
                parameters["icon16_file"] = RequestValues.Attachment(Icon16File, "Icon16File");
            }
            if (IsBound(nameof(Icon16Delete)))
            {
                parameters["icon16_delete"] = Icon16Delete.Value;
            }
            if (IsBound(nameof(Icon32File)))
            {
                parameters["icon32_file"] = RequestValues.Attachment(Icon32File, "Icon32File");
            }
            if (IsBound(nameof(Icon32Delete)))
            {
                parameters["icon32_delete"] = Icon32Delete.Value;
            }
            if (IsBound(nameof(Icon48File)))
            {
                parameters["icon48_file"] = RequestValues.Attachment(Icon48File, "Icon48File");
            }
            if (IsBound(nameof(Icon48Delete)))
            {
                parameters["icon48_delete"] = Icon48Delete.Value;
            }
            if (IsBound(nameof(Icon128File)))
            {
                parameters["icon128_file"] = RequestValues.Attachment(Icon128File, "Icon128File");
            }
            if (IsBound(nameof(Icon128Delete)))
            {
                parameters["icon128_delete"] = Icon128Delete.Value;
            }
            if (IsBound(nameof(LogoFile)))
            {
                parameters["logo_file"] = RequestValues.Attachment(LogoFile, "LogoFile");
            }
            if (IsBound(nameof(LogoDelete)))
            {
                parameters["logo_delete"] = LogoDelete.Value;
            }
            if (IsBound(nameof(BundleWatermarkAttachmentFile)))
            {
                parameters["bundle_watermark_attachment_file"] = RequestValues.Attachment(BundleWatermarkAttachmentFile, "BundleWatermarkAttachmentFile");
            }
            if (IsBound(nameof(BundleWatermarkAttachmentDelete)))
            {
                parameters["bundle_watermark_attachment_delete"] = BundleWatermarkAttachmentDelete.Value;
            }
            if (IsBound(nameof(LoginPageBackgroundImageFile)))
            {
                parameters["login_page_background_image_file"] = RequestValues.Attachment(LoginPageBackgroundImageFile, "LoginPageBackgroundImageFile");
            }
            if (IsBound(nameof(LoginPageBackgroundImageDelete)))
            {
                parameters["login_page_background_image_delete"] = LoginPageBackgroundImageDelete.Value;
            }
            if (IsBound(nameof(Disable2faWithDelay)))
            {
                parameters["disable_2fa_with_delay"] = Disable2faWithDelay.Value;
            }
            if (IsBound(nameof(RedirectOldSubdomain)))
            {
                parameters["redirect_old_subdomain"] = RedirectOldSubdomain.Value;
            }
            if (IsBound(nameof(SmtpPassword)))
            {
                parameters["smtp_password"] = RequestValues.Reveal(SmtpPassword);
            }
            return parameters;
        }
    }
}
