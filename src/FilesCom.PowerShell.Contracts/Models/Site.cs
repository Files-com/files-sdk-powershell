using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Site is the place you'll come to update site settings, as well as manage site-wide API keys.
    /// </summary>
    public sealed class Site : FilesObject
    {
        internal Site()
        {
        }

        /// <summary>
        /// Site Id
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Site name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Additional extensions that are considered text files
        /// </summary>
        public string[] AdditionalTextFileTypes { get; internal set; }

        /// <summary>
        /// Availability settings for AI features. Each feature requires the site_admins, workspace_admins, folder_admins, and all_users keys. Optional selected_group_members defaults to false; when true, members of at least one group in group_ids get access regardless of the other options. All availability options are additive: any enabled option matching the user grants access. Optional group_ids is an array of integer IDs of active groups on this site, from any workspace. Omitted or empty group_ids grants no access through selected_group_members and does not affect other options. Disabling all AI features overrides these settings.
        /// </summary>
        public object AiFeatureAvailability { get; internal set; }

        /// <summary>
        /// Is SMS two factor authentication allowed?
        /// </summary>
        public bool Allowed2faMethodSms { get; internal set; }

        /// <summary>
        /// Is TOTP two factor authentication allowed?
        /// </summary>
        public bool Allowed2faMethodTotp { get; internal set; }

        /// <summary>
        /// Is WebAuthn two factor authentication allowed?
        /// </summary>
        public bool Allowed2faMethodWebauthn { get; internal set; }

        /// <summary>
        /// Is yubikey two factor authentication allowed?
        /// </summary>
        public bool Allowed2faMethodYubi { get; internal set; }

        /// <summary>
        /// Is OTP via email two factor authentication allowed?
        /// </summary>
        public bool Allowed2faMethodEmail { get; internal set; }

        /// <summary>
        /// Is OTP via static codes for two factor authentication allowed?
        /// </summary>
        public bool Allowed2faMethodStatic { get; internal set; }

        /// <summary>
        /// Are users allowed to configure their two factor authentication to be bypassed for FTP/SFTP/WebDAV?
        /// </summary>
        public bool Allowed2faMethodBypassForFtpSftpDav { get; internal set; }

        /// <summary>
        /// User ID for the main site administrator
        /// </summary>
        public long? AdminUserId { get; internal set; }

        /// <summary>
        /// Allow admins to bypass the locked subfolders setting.
        /// </summary>
        public bool AdminsBypassLockedSubfolders { get; internal set; }

        /// <summary>
        /// Are manual Bundle names allowed?
        /// </summary>
        public bool AllowBundleNames { get; internal set; }

        /// <summary>
        /// Allow the site-wide two-factor authentication requirement to be overriden on a per-user-basis?
        /// </summary>
        public bool AllowUserLevel2faOverride { get; internal set; }

        /// <summary>
        /// Allow the site-wide allowed IP restriction to be overriden on a per-user-basis?
        /// </summary>
        public bool AllowUserLevelAllowedIpOverride { get; internal set; }

        /// <summary>
        /// Allow per-user overrides of the FTP TLS session reuse requirement?
        /// </summary>
        public bool AllowUserLevelFtpTlsSessionReuseOverride { get; internal set; }

        /// <summary>
        /// Allow the site-wide FTP SSL requirement to be overriden on a per-user-basis?
        /// </summary>
        public bool AllowUserLevelSslOverride { get; internal set; }

        /// <summary>
        /// Comma separated list of allowed Country codes
        /// </summary>
        public string AllowedCountries { get; internal set; }

        /// <summary>
        /// List of allowed IP addresses
        /// </summary>
        public string AllowedIps { get; internal set; }

        /// <summary>
        /// Require Parent Site users entering this Child Site to match its allowed IP addresses in addition to their existing IP restrictions. Applies to all Workspaces. Defaults to false.
        /// </summary>
        public bool EnforceParentSiteUserIpRestrictions { get; internal set; }

        /// <summary>
        /// Create parent directories if they do not exist during uploads?  This is primarily used to work around broken upload clients that assume servers will perform this step.
        /// </summary>
        public bool AlwaysMkdirParents { get; internal set; }

        /// <summary>
        /// Number of days to retain AS2 messages (incoming and outgoing).
        /// </summary>
        public long? As2MessageRetentionDays { get; internal set; }

        /// <summary>
        /// If false, rename conflicting files instead of asking for overwrite confirmation.  Only applies to web interface.
        /// </summary>
        public bool AskAboutOverwrites { get; internal set; }

        /// <summary>
        /// Do Bundle owners receive activity notifications?
        /// </summary>
        public string BundleActivityNotifications { get; internal set; }

        /// <summary>
        /// Site-wide Bundle expiration in days
        /// </summary>
        public long? BundleExpiration { get; internal set; }

        /// <summary>
        /// Custom error message to show when bundle is not found.
        /// </summary>
        public string BundleNotFoundMessage { get; internal set; }

        /// <summary>
        /// Do Bundles require password protection?
        /// </summary>
        public bool BundlePasswordRequired { get; internal set; }

        /// <summary>
        /// If true, new Share Links created by a user with a primary group will default to that group as owner.
        /// </summary>
        public bool BundlesDefaultOwnedByPrimaryGroup { get; internal set; }

        /// <summary>
        /// List of email domains to disallow when entering a Bundle/Inbox recipients
        /// </summary>
        public string[] BundleRecipientBlacklistDomains { get; internal set; }

        /// <summary>
        /// Disallow free email domains for Bundle/Inbox recipients?
        /// </summary>
        public bool BundleRecipientBlacklistFreeEmailDomains { get; internal set; }

        /// <summary>
        /// Do Bundle owners receive registration notification?
        /// </summary>
        public string BundleRegistrationNotifications { get; internal set; }

        /// <summary>
        /// Do Bundles require registration?
        /// </summary>
        public bool BundleRequireRegistration { get; internal set; }

        /// <summary>
        /// Do Bundles require recipients for sharing?
        /// </summary>
        public bool BundleRequireShareRecipient { get; internal set; }

        /// <summary>
        /// If true, new Share Links must send a one-time password to the recipient when they register. Requires bundle_require_share_recipient and cannot be enabled with bundle_password_required.
        /// </summary>
        public bool BundleSendOneTimePasswordToRecipientAtRegistration { get; internal set; }

        /// <summary>
        /// Do Bundles require internal notes?
        /// </summary>
        public bool BundleRequireNote { get; internal set; }

        /// <summary>
        /// Do Bundle creators receive receipts of invitations?
        /// </summary>
        public bool BundleSendSharedReceipts { get; internal set; }

        /// <summary>
        /// Do Bundle uploaders receive upload confirmation notifications?
        /// </summary>
        public string BundleUploadReceiptNotifications { get; internal set; }

        /// <summary>
        /// Preview watermark image applied to all bundle items.
        /// </summary>
        public Models.Image BundleWatermarkAttachment { get; internal set; }

        /// <summary>
        /// Preview watermark settings applied to all bundle items. Uses the same keys as Behavior.value
        /// </summary>
        public object BundleWatermarkValue { get; internal set; }

        /// <summary>
        /// Calculate CRC32 checksums for files?
        /// </summary>
        public bool CalculateFileChecksumsCrc32 { get; internal set; }

        /// <summary>
        /// Calculate MD5 checksums for files?
        /// </summary>
        public bool CalculateFileChecksumsMd5 { get; internal set; }

        /// <summary>
        /// Calculate SHA1 checksums for files?
        /// </summary>
        public bool CalculateFileChecksumsSha1 { get; internal set; }

        /// <summary>
        /// Calculate SHA256 checksums for files?
        /// </summary>
        public bool CalculateFileChecksumsSha256 { get; internal set; }

        /// <summary>
        /// If true, administrators on this site's Child Sites, including Folder Admins, can list this site's Default Workspace users and groups and grant them folder permissions and notifications on Child Site paths.
        /// </summary>
        public bool ChildSiteFolderAdminsCanSeeParentSiteDirectory { get; internal set; }

        /// <summary>
        /// If true, Workspace Administrators and Folder Admins in a Custom Workspace can list the Default Workspace users and groups that have permissions in that Workspace and choose them as Email Notification recipients.
        /// </summary>
        public bool WorkspaceFolderAdminsCanSeeDefaultWorkspaceDirectory { get; internal set; }

        /// <summary>
        /// Require email authentication, virus, and spam checks for incoming emails to Inboxes and Incoming Email Automations in every Workspace on this site?
        /// </summary>
        public bool UploadsViaEmailAuthentication { get; internal set; }

        /// <summary>
        /// Page link and button color
        /// </summary>
        public string Color2Left { get; internal set; }

        /// <summary>
        /// Top bar link color
        /// </summary>
        public string Color2Link { get; internal set; }

        /// <summary>
        /// Page link and button color
        /// </summary>
        public string Color2Text { get; internal set; }

        /// <summary>
        /// Top bar background color
        /// </summary>
        public string Color2Top { get; internal set; }

        /// <summary>
        /// Top bar text color
        /// </summary>
        public string Color2TopText { get; internal set; }

        /// <summary>
        /// Site main contact name
        /// </summary>
        public string ContactName { get; internal set; }

        /// <summary>
        /// Time this site was created
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Preferred currency
        /// </summary>
        public string Currency { get; internal set; }

        /// <summary>
        /// Is this site using a custom namespace for users?
        /// </summary>
        public bool CustomNamespace { get; internal set; }

        /// <summary>
        /// Is WebDAV enabled?
        /// </summary>
        public bool DavEnabled { get; internal set; }

        /// <summary>
        /// Use user FTP roots also for WebDAV?
        /// </summary>
        public bool DavUserRootEnabled { get; internal set; }

        /// <summary>
        /// Number of days to keep deleted files
        /// </summary>
        public long? DaysToRetainBackups { get; internal set; }

        /// <summary>
        /// If true, allow public viewers of Bundles with full permissions to use document editing integrations.
        /// </summary>
        public bool DocumentEditsInBundleAllowed { get; internal set; }

        /// <summary>
        /// Site default time zone
        /// </summary>
        public string DefaultTimeZone { get; internal set; }

        /// <summary>
        /// Is the desktop app enabled?
        /// </summary>
        public bool DesktopApp { get; internal set; }

        /// <summary>
        /// Is desktop app session IP pinning enabled?
        /// </summary>
        public bool DesktopAppSessionIpPinning { get; internal set; }

        /// <summary>
        /// Desktop app session lifetime (in hours)
        /// </summary>
        public long? DesktopAppSessionLifetime { get; internal set; }

        /// <summary>
        /// Use legacy checksums mode?
        /// </summary>
        public bool LegacyChecksumsMode { get; internal set; }

        /// <summary>
        /// Allow legacy app pairing in all workspaces? Disabled for new sites; existing sites remain enabled until changed. Disabling blocks new pairing exchanges without revoking existing app credentials or affecting OAuth sign-in. Site administrators can re-enable it.
        /// </summary>
        public bool LegacyPairingEnabled { get; internal set; }

        /// <summary>
        /// Time of the last completed legacy app pairing across all workspaces. Failed or incomplete attempts do not count. Read-only; empty until a pairing completes after tracking begins.
        /// </summary>
        public DateTime? LastPairingAt { get; internal set; }

        /// <summary>
        /// If true, we will migrate all remote server syncs to the new Sync model.
        /// </summary>
        public bool MigrateRemoteServerSyncToSync { get; internal set; }

        /// <summary>
        /// Is OAuth DCR (dynamic client registration) for MCP enabled?
        /// </summary>
        public bool McpDcrEnabled { get; internal set; }

        /// <summary>
        /// Is the mobile app enabled?
        /// </summary>
        public bool MobileApp { get; internal set; }

        /// <summary>
        /// Is mobile app session IP pinning enabled?
        /// </summary>
        public bool MobileAppSessionIpPinning { get; internal set; }

        /// <summary>
        /// Mobile app session lifetime (in hours)
        /// </summary>
        public long? MobileAppSessionLifetime { get; internal set; }

        /// <summary>
        /// Comma separated list of disallowed Country codes
        /// </summary>
        public string DisallowedCountries { get; internal set; }

        /// <summary>
        /// If true, all AI features are disabled for this site.
        /// </summary>
        public bool DisableAllAiFeatures { get; internal set; }

        /// <summary>
        /// If set, Files.com will not set the CAA records required to generate future SSL certificates for this domain.
        /// </summary>
        public bool DisableFilesCertificateGeneration { get; internal set; }

        /// <summary>
        /// Are notifications disabled?
        /// </summary>
        public bool DisableNotifications { get; internal set; }

        /// <summary>
        /// If true, file operations produce file_action.activity.v1 EventChannel events in all workspaces. Defaults to false; subscription filters and workspace access still control delivery.
        /// </summary>
        public bool FileActivityEventsEnabled { get; internal set; }

        /// <summary>
        /// Is password reset disabled?
        /// </summary>
        public bool DisablePasswordReset { get; internal set; }

        /// <summary>
        /// Custom domain
        /// </summary>
        public string Domain { get; internal set; }

        /// <summary>
        /// Send HSTS (HTTP Strict Transport Security) header when visitors access the site via a custom domain?
        /// </summary>
        public bool DomainHstsHeader { get; internal set; }

        /// <summary>
        /// Letsencrypt chain to use when registering SSL Certificate for domain. No longer used as of 2026.
        /// </summary>
        public string DomainLetsencryptChain { get; internal set; }

        /// <summary>
        /// Main email for this site
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// Are FedRAMP security restrictions enabled for this site?
        /// </summary>
        public bool Fedramp { get; internal set; }

        /// <summary>
        /// Allow other Files.com sites to use this site's API keys for native Files.com Remote Server connections? Defaults to true and applies to all keys and workspaces on this site. When false, new pairings and access through existing connections are rejected without revoking keys or deleting connections. Re-enabling permits access again with usable keys. Does not disable this site's connections to other sites, ordinary API access, or Connected Sites.
        /// </summary>
        public bool FilesComRemoteServerEnabled { get; internal set; }

        /// <summary>
        /// Is FTP enabled?
        /// </summary>
        public bool FtpEnabled { get; internal set; }

        /// <summary>
        /// Reply-to email for this site
        /// </summary>
        public string ReplyToEmail { get; internal set; }

        /// <summary>
        /// If true, groups can be manually created / modified / deleted by Site Admins. Otherwise, groups can only be managed via your SSO provider.
        /// </summary>
        public bool NonSsoGroupsAllowed { get; internal set; }

        /// <summary>
        /// If true, users can be manually created / modified / deleted by Site Admins. Otherwise, users can only be managed via your SSO provider.
        /// </summary>
        public bool NonSsoUsersAllowed { get; internal set; }

        /// <summary>
        /// If true, permissions for this site must be bound to a group (not a user).
        /// </summary>
        public bool FolderPermissionsGroupsOnly { get; internal set; }

        /// <summary>
        /// Allow group admins to create users in their groups
        /// </summary>
        public bool GroupAdminsCanAddUsers { get; internal set; }

        /// <summary>
        /// Allow group admins to add or remove existing users in their groups
        /// </summary>
        public bool GroupAdminsCanManageGroupMemberships { get; internal set; }

        /// <summary>
        /// Allow group admins to delete users in their groups
        /// </summary>
        public bool GroupAdminsCanDeleteUsers { get; internal set; }

        /// <summary>
        /// Allow group admins to enable or disable users in their groups
        /// </summary>
        public bool GroupAdminsCanEnableDisableUsers { get; internal set; }

        /// <summary>
        /// Allow group admins to modify users in their groups
        /// </summary>
        public bool GroupAdminsCanModifyUsers { get; internal set; }

        /// <summary>
        /// Allow group admins to exempt users in their groups from lifecycle rules
        /// </summary>
        public bool GroupAdminsCanBypassUserLifecycleRules { get; internal set; }

        /// <summary>
        /// Allow group admins to reset passwords for users in their groups
        /// </summary>
        public bool GroupAdminsCanResetPasswords { get; internal set; }

        /// <summary>
        /// Allow group admins to set password authentication method
        /// </summary>
        public bool GroupAdminsCanSetUserPassword { get; internal set; }

        /// <summary>
        /// Is there a signed HIPAA BAA between Files.com and this site?
        /// </summary>
        public bool Hipaa { get; internal set; }

        /// <summary>
        /// Branded icon 128x128
        /// </summary>
        public Models.Image Icon128 { get; internal set; }

        /// <summary>
        /// Branded icon 16x16
        /// </summary>
        public Models.Image Icon16 { get; internal set; }

        /// <summary>
        /// Branded icon 32x32
        /// </summary>
        public Models.Image Icon32 { get; internal set; }

        /// <summary>
        /// Branded icon 48x48
        /// </summary>
        public Models.Image Icon48 { get; internal set; }

        /// <summary>
        /// Can files be modified?
        /// </summary>
        public DateTime? ImmutableFilesSetAt { get; internal set; }

        /// <summary>
        /// Administrator access to user-owned Remote Servers in every Workspace: always_visible (default), never_visible, user_choice_default_visible, or user_choice_default_personal. Forced modes override owner preferences; choice modes use the preference or their default.
        /// </summary>
        public string IntegrationCentricRemoteServerAdminAccess { get; internal set; }

        /// <summary>
        /// Include password in emails to new users?
        /// </summary>
        public bool IncludePasswordInWelcomeEmail { get; internal set; }

        /// <summary>
        /// Site default language
        /// </summary>
        public string Language { get; internal set; }

        /// <summary>
        /// Login help text
        /// </summary>
        public string LoginHelpText { get; internal set; }

        /// <summary>
        /// Branded logo
        /// </summary>
        public Models.Image Logo { get; internal set; }

        /// <summary>
        /// Branded login page background
        /// </summary>
        public Models.Image LoginPageBackgroundImage { get; internal set; }

        /// <summary>
        /// Number of prior passwords to disallow
        /// </summary>
        public long? MaxPriorPasswords { get; internal set; }

        /// <summary>
        /// Site settings managed by the parent site. Each key identifies a managed setting, with a null value. Non-secret effective values are returned in their own Site settings fields.
        /// </summary>
        public object ManagedSiteSettings { get; internal set; }

        /// <summary>
        /// A message to show users when they connect via FTP or SFTP.
        /// </summary>
        public string MotdText { get; internal set; }

        /// <summary>
        /// Show message to users connecting via FTP
        /// </summary>
        public bool MotdUseForFtp { get; internal set; }

        /// <summary>
        /// Show message to users connecting via SFTP
        /// </summary>
        public bool MotdUseForSftp { get; internal set; }

        /// <summary>
        /// Next billing amount
        /// </summary>
        public decimal? NextBillingAmount { get; internal set; }

        /// <summary>
        /// Next billing date
        /// </summary>
        public string NextBillingDate { get; internal set; }

        /// <summary>
        /// If true, allows users to use a document editing integration.
        /// </summary>
        public bool OfficeIntegrationAvailable { get; internal set; }

        /// <summary>
        /// Which document editing integration to support. Files.com Editor or Microsoft Office for the Web.
        /// </summary>
        public string OfficeIntegrationType { get; internal set; }

        /// <summary>
        /// Link to scheduling a meeting with our Sales team
        /// </summary>
        public string OncehubLink { get; internal set; }

        /// <summary>
        /// Use servers in the USA only?
        /// </summary>
        public bool OptOutGlobal { get; internal set; }

        /// <summary>
        /// Is this site's billing overdue?
        /// </summary>
        public bool Overdue { get; internal set; }

        /// <summary>
        /// Shortest password length for users
        /// </summary>
        public long? PasswordMinLength { get; internal set; }

        /// <summary>
        /// Require a letter in passwords?
        /// </summary>
        public bool PasswordRequireLetter { get; internal set; }

        /// <summary>
        /// Require lower and upper case letters in passwords?
        /// </summary>
        public bool PasswordRequireMixed { get; internal set; }

        /// <summary>
        /// Require a number in passwords?
        /// </summary>
        public bool PasswordRequireNumber { get; internal set; }

        /// <summary>
        /// Require special characters in password?
        /// </summary>
        public bool PasswordRequireSpecial { get; internal set; }

        /// <summary>
        /// Require passwords that have not been previously breached? (see https://haveibeenpwned.com/)
        /// </summary>
        public bool PasswordRequireUnbreached { get; internal set; }

        /// <summary>
        /// Require bundles' passwords, and passwords for other items (inboxes, public shares, etc.) to conform to the same requirements as users' passwords?
        /// </summary>
        public bool PasswordRequirementsApplyToBundles { get; internal set; }

        /// <summary>
        /// Number of days password is valid
        /// </summary>
        public long? PasswordValidityDays { get; internal set; }

        /// <summary>
        /// Site phone number
        /// </summary>
        public string Phone { get; internal set; }

        /// <summary>
        /// If true, we will ensure that all internal communications with any remote server are made through the primary region of the site. This setting overrides individual remote server settings.
        /// </summary>
        public bool PinAllRemoteServersToSiteRegion { get; internal set; }

        /// <summary>
        /// If true, we will prevent non-administrators from receiving any permissions directly on the root folder.  This is commonly used to prevent the accidental application of permissions.
        /// </summary>
        public bool PreventRootPermissionsForNonSiteAdmins { get; internal set; }

        /// <summary>
        /// If true, protocol access permissions on users will be ignored, and only protocol access permissions set on Groups will be honored.  Make sure that your current user is a member of a group with API permission when changing this value to avoid locking yourself out of your site.
        /// </summary>
        public bool ProtocolAccessGroupsOnly { get; internal set; }

        /// <summary>
        /// Require two-factor authentication for all users?
        /// </summary>
        public bool Require2fa { get; internal set; }

        /// <summary>
        /// If true, only site admins may create, modify, or delete any behavior at the site root, or a skip that would disable one.
        /// </summary>
        public bool RestrictRootFolderBehaviorsToSiteAdmins { get; internal set; }

        /// <summary>
        /// If true, supported protective behaviors at the site root also apply within named workspaces. Requires restrict_root_folder_behaviors_to_site_admins to be enabled.
        /// </summary>
        public bool RootFolderBehaviorsApplyToWorkspaces { get; internal set; }

        /// <summary>
        /// If true, SSO users using the default user-level two-factor authentication setting are exempt from the site-wide two-factor authentication requirement.
        /// </summary>
        public bool Require2faExemptAllSsoUsers { get; internal set; }

        /// <summary>
        /// If set, requirement for two-factor authentication has been scheduled to end on this date-time.
        /// </summary>
        public DateTime? Require2faStopTime { get; internal set; }

        /// <summary>
        /// Auto-removes bundles for disabled/deleted users and enforces bundle expiry within user access period.
        /// </summary>
        public bool RevokeBundleAccessOnDisableOrDelete { get; internal set; }

        /// <summary>
        /// What type of user is required to use two-factor authentication (when require_2fa is set to `true` for this site)?
        /// </summary>
        public string Require2faUserType { get; internal set; }

        /// <summary>
        /// If true, we will hide the 'Remember Me' box on Inbox and Bundle registration pages, requiring that the user logout and log back in every time they visit the page.
        /// </summary>
        public bool RequireLogoutFromBundlesAndInboxes { get; internal set; }

        /// <summary>
        /// Current session
        /// </summary>
        public Models.Session Session { get; internal set; }

        /// <summary>
        /// Is the S3-compatible endpoint enabled for all users and workspaces on this site? Defaults to true. When false, user and group S3 permissions do not allow access.
        /// </summary>
        public bool S3CompatibleEndpointEnabled { get; internal set; }

        /// <summary>
        /// Is SFTP enabled?
        /// </summary>
        public bool SftpEnabled { get; internal set; }

        /// <summary>
        /// Finalize partial SFTP uploads from interrupted connections? Default: true.
        /// </summary>
        public bool SftpFinalizePartialUploads { get; internal set; }

        /// <summary>
        /// Sftp Host Key Type
        /// </summary>
        public string SftpHostKeyType { get; internal set; }

        /// <summary>
        /// Id of the currently selected custom SFTP Host Key
        /// </summary>
        public long? ActiveSftpHostKeyId { get; internal set; }

        /// <summary>
        /// Ids of the selected custom SFTP Host Keys
        /// </summary>
        public long?[] ActiveSftpHostKeyIds { get; internal set; }

        /// <summary>
        /// If true, we will allow weak and known insecure ciphers to be used for SFTP connections.  Enabling this setting severely weakens the security of your site and it is not recommend, except as a last resort for compatibility.
        /// </summary>
        public bool SftpInsecureCiphers { get; internal set; }

        /// <summary>
        /// If true, we will allow weak Diffie Hellman parameters to be used within ciphers for SFTP that are otherwise on our secure list.  This has the effect of making the cipher weaker than our normal threshold for security, but is required to support certain legacy or broken SSH and MFT clients.  Enabling this weakens security, but not nearly as much as enabling the full `sftp_insecure_ciphers` option.
        /// </summary>
        public bool SftpInsecureDiffieHellman { get; internal set; }

        /// <summary>
        /// Use user FTP roots also for SFTP?
        /// </summary>
        public bool SftpUserRootEnabled { get; internal set; }

        /// <summary>
        /// Allow bundle creation
        /// </summary>
        public bool SharingEnabled { get; internal set; }

        /// <summary>
        /// Show log in link in user notifications?
        /// </summary>
        public bool ShowUserNotificationsLogInLink { get; internal set; }

        /// <summary>
        /// Show request access link for users without access?  Currently unused.
        /// </summary>
        public bool ShowRequestAccessLink { get; internal set; }

        /// <summary>
        /// Custom site footer text for authenticated pages
        /// </summary>
        public string SiteFooter { get; internal set; }

        /// <summary>
        /// Custom site header text for authenticated pages
        /// </summary>
        public string SiteHeader { get; internal set; }

        /// <summary>
        /// Custom site footer text for public pages
        /// </summary>
        public string SitePublicFooter { get; internal set; }

        /// <summary>
        /// Custom site header text for public pages
        /// </summary>
        public string SitePublicHeader { get; internal set; }

        /// <summary>
        /// SMTP server hostname or IP
        /// </summary>
        public string SmtpAddress { get; internal set; }

        /// <summary>
        /// SMTP server authentication type
        /// </summary>
        public string SmtpAuthentication { get; internal set; }

        /// <summary>
        /// From address to use when mailing through custom SMTP
        /// </summary>
        public string SmtpFrom { get; internal set; }

        /// <summary>
        /// SMTP server port
        /// </summary>
        public long? SmtpPort { get; internal set; }

        /// <summary>
        /// Custom SMTP encryption mode: if_available (default) uses STARTTLS when offered and otherwise sends credentials and messages unencrypted. A certificate-verified STARTTLS connection automatically changes if_available to require unless smtp_ssl is managed by a parent policy. require requires STARTTLS before authentication; require_implicit uses TLS from connection start; never disables TLS. TLS verifies the server certificate against smtp_address. Encryption is never automatically downgraded.
        /// </summary>
        public string SmtpSsl { get; internal set; }

        /// <summary>
        /// SMTP server username
        /// </summary>
        public string SmtpUsername { get; internal set; }

        /// <summary>
        /// Session expiry in minutes
        /// </summary>
        public long? SessionExpiryMinutes { get; internal set; }

        /// <summary>
        /// Allow snapshot share links creation
        /// </summary>
        public bool SnapshotSharingEnabled { get; internal set; }

        /// <summary>
        /// Require TLS-protected FTP data connections to reuse the control connection TLS session? Applies to all workspaces.
        /// </summary>
        public bool FtpTlsSessionReuseRequired { get; internal set; }

        /// <summary>
        /// Is SSL required?  Disabling this is insecure.
        /// </summary>
        public bool SslRequired { get; internal set; }

        /// <summary>
        /// Site subdomain
        /// </summary>
        public string Subdomain { get; internal set; }

        /// <summary>
        /// If switching plans, when does the new plan take effect?
        /// </summary>
        public DateTime? SwitchToPlanDate { get; internal set; }

        /// <summary>
        /// Number of days left in trial
        /// </summary>
        public long? TrialDaysLeft { get; internal set; }

        /// <summary>
        /// When does this Site trial expire?
        /// </summary>
        public DateTime? TrialUntil { get; internal set; }

        /// <summary>
        /// If using custom SMTP, should we use dedicated IPs to deliver emails?
        /// </summary>
        public bool UseDedicatedIpsForSmtp { get; internal set; }

        /// <summary>
        /// Allow uploaders to set `provided_modified_at` for uploaded files?
        /// </summary>
        public bool UseProvidedModifiedAt { get; internal set; }

        /// <summary>
        /// User of current session
        /// </summary>
        public Models.User User { get; internal set; }

        /// <summary>
        /// Will users be locked out after incorrect login attempts?
        /// </summary>
        public bool UserLockout { get; internal set; }

        /// <summary>
        /// How many hours to lock user out for failed password?
        /// </summary>
        public long? UserLockoutLockPeriod { get; internal set; }

        /// <summary>
        /// Number of login tries within `user_lockout_within` hours before users are locked out
        /// </summary>
        public long? UserLockoutTries { get; internal set; }

        /// <summary>
        /// Number of hours for user lockout window
        /// </summary>
        public long? UserLockoutWithin { get; internal set; }

        /// <summary>
        /// Enable User Requests feature
        /// </summary>
        public bool UserRequestsEnabled { get; internal set; }

        /// <summary>
        /// Send email to site admins when a user request is received?
        /// </summary>
        public bool UserRequestsNotifyAdmins { get; internal set; }

        /// <summary>
        /// Allow users to create their own API keys?
        /// </summary>
        public bool UsersCanCreateApiKeys { get; internal set; }

        /// <summary>
        /// Allow users to create their own SSH keys?
        /// </summary>
        public bool UsersCanCreateSshKeys { get; internal set; }

        /// <summary>
        /// How usernames are displayed in the web UI. Can be `username_only`, `full_name_only`, `full_name_username`, `full_name_company`, or `full_name_username_company`.
        /// </summary>
        public string UsernameDisplay { get; internal set; }

        /// <summary>
        /// Custom text send in user welcome email
        /// </summary>
        public string WelcomeCustomText { get; internal set; }

        /// <summary>
        /// Custom footer text for system-generated emails (as Markdown). Supports standard strftime date/time patterns like %Y (4-digit year), %m (month), %d (day).
        /// </summary>
        public string EmailFooterCustomText { get; internal set; }

        /// <summary>
        /// Include this email in welcome emails if enabled
        /// </summary>
        public string WelcomeEmailCc { get; internal set; }

        /// <summary>
        /// Include this email subject in welcome emails if enabled
        /// </summary>
        public string WelcomeEmailSubject { get; internal set; }

        /// <summary>
        /// Will the welcome email be sent to new users?
        /// </summary>
        public bool WelcomeEmailEnabled { get; internal set; }

        /// <summary>
        /// Does the welcome screen appear?
        /// </summary>
        public string WelcomeScreen { get; internal set; }

        /// <summary>
        /// Does FTP user Windows emulation mode?
        /// </summary>
        public bool WindowsModeFtp { get; internal set; }
    }
}
