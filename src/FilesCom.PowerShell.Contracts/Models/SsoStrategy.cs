using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An SSOStrategy is a way for users to sign in via another identity provider, such as Okta or Auth0.
    /// </summary>
    public sealed class SsoStrategy : FilesObject
    {
        internal SsoStrategy()
        {
        }

        /// <summary>
        /// SSO Protocol
        /// </summary>
        public string Protocol { get; internal set; }

        /// <summary>
        /// Provider name
        /// </summary>
        public string Provider { get; internal set; }

        /// <summary>
        /// Custom label for the SSO provider on the login page.
        /// </summary>
        public string Label { get; internal set; }

        /// <summary>
        /// URL holding a custom logo for the SSO provider on the login page.
        /// </summary>
        public string LogoUrl { get; internal set; }

        /// <summary>
        /// ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Is strategy enabled?  This may become automatically set to `false` after a high number and duration of failures.
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Count of users with this SSO Strategy
        /// </summary>
        public long? UserCount { get; internal set; }

        /// <summary>
        /// Identity provider sha256 cert fingerprint if saml_provider_metadata_url is not available.
        /// </summary>
        public string SamlProviderCertFingerprint { get; internal set; }

        /// <summary>
        /// Identity provider issuer url
        /// </summary>
        public string SamlProviderIssuerUrl { get; internal set; }

        /// <summary>
        /// Custom identity provider metadata
        /// </summary>
        public string SamlProviderMetadataContent { get; internal set; }

        /// <summary>
        /// Metadata URL for the SAML identity provider
        /// </summary>
        public string SamlProviderMetadataUrl { get; internal set; }

        /// <summary>
        /// Identity provider SLO endpoint
        /// </summary>
        public string SamlProviderSloTargetUrl { get; internal set; }

        /// <summary>
        /// Identity provider SSO endpoint if saml_provider_metadata_url is not available.
        /// </summary>
        public string SamlProviderSsoTargetUrl { get; internal set; }

        /// <summary>
        /// SCIM authentication type.
        /// </summary>
        public string ScimAuthenticationMethod { get; internal set; }

        /// <summary>
        /// SCIM username.
        /// </summary>
        public string ScimUsername { get; internal set; }

        /// <summary>
        /// SCIM OAuth Access Token. Only returned when the token is first issued or reset.
        /// </summary>
        public FilesSecret ScimOauthAccessToken { get; internal set; }

        /// <summary>
        /// SCIM OAuth Access Token Expiration Time.
        /// </summary>
        public string ScimOauthAccessTokenExpiresAt { get; internal set; }

        /// <summary>
        /// Subdomain or domain name for your auth provider.   Example: `https://[subdomain].okta.com/`
        /// </summary>
        public string Subdomain { get; internal set; }

        /// <summary>
        /// Auto-provision users?
        /// </summary>
        public bool ProvisionUsers { get; internal set; }

        /// <summary>
        /// Auto-provision group membership based on group memberships on the SSO side?
        /// </summary>
        public bool ProvisionGroups { get; internal set; }

        /// <summary>
        /// Auto-deprovision users?
        /// </summary>
        public bool DeprovisionUsers { get; internal set; }

        /// <summary>
        /// Auto-deprovision group membership based on group memberships on the SSO side?
        /// </summary>
        public bool DeprovisionGroups { get; internal set; }

        /// <summary>
        /// Method used for deprovisioning users.
        /// </summary>
        public string DeprovisionBehavior { get; internal set; }

        /// <summary>
        /// Comma-separated list of group names for groups to automatically add all auto-provisioned users to.
        /// </summary>
        public string ProvisionGroupDefault { get; internal set; }

        /// <summary>
        /// Comma-separated list of group names for groups (with optional wildcards) that will be excluded from auto-provisioning.
        /// </summary>
        public string ProvisionGroupExclusion { get; internal set; }

        /// <summary>
        /// Comma-separated list of group names for groups (with optional wildcards) that will be auto-provisioned.
        /// </summary>
        public string ProvisionGroupInclusion { get; internal set; }

        /// <summary>
        /// Comma or newline separated list of group names (with optional wildcards) to require membership for user provisioning.
        /// </summary>
        public string ProvisionGroupRequired { get; internal set; }

        /// <summary>
        /// Comma-separated list of group names whose members will be created with email_signup authentication.
        /// </summary>
        public string ProvisionEmailSignupGroups { get; internal set; }

        /// <summary>
        /// Comma-separated list of group names used to grant or remove Read-Only Site Admin access for users on this SSO provider. When this list is nonempty and a directory sync changes a user's group membership, membership in at least one listed group grants this access; membership in none removes it, including access granted manually. Directory syncs include SCIM requests from any provider on the site, LDAP syncs, and sign-ins that set the user's groups from the identity provider. Membership changes made any other way, including through the web app, API, SDKs, or CLI, do not affect administrator access.
        /// </summary>
        public string ProvisionReadonlySiteAdminGroups { get; internal set; }

        /// <summary>
        /// Comma-separated list of group names used to grant or remove Site Admin access for users on this SSO provider. When this list is nonempty and a directory sync changes a user's group membership, membership in at least one listed group grants this access; membership in none removes it, including access granted manually. Directory syncs include SCIM requests from any provider on the site, LDAP syncs, and sign-ins that set the user's groups from the identity provider. Membership changes made any other way, including through the web app, API, SDKs, or CLI, do not affect administrator access.
        /// </summary>
        public string ProvisionSiteAdminGroups { get; internal set; }

        /// <summary>
        /// Comma-separated list of group names whose members will be provisioned as Group Admins.
        /// </summary>
        public string ProvisionGroupAdminGroups { get; internal set; }

        /// <summary>
        /// ProvisionAttachmentsPermission
        /// </summary>
        public bool ProvisionAttachmentsPermission { get; internal set; }

        /// <summary>
        /// Auto-provisioned users get WebDAV permission?
        /// </summary>
        public bool ProvisionDavPermission { get; internal set; }

        /// <summary>
        /// Auto-provisioned users get FTP permission?
        /// </summary>
        public bool ProvisionFtpPermission { get; internal set; }

        /// <summary>
        /// Auto-provisioned users get SFTP permission?
        /// </summary>
        public bool ProvisionSftpPermission { get; internal set; }

        /// <summary>
        /// Default time zone for auto provisioned users.
        /// </summary>
        public string ProvisionTimeZone { get; internal set; }

        /// <summary>
        /// Default company for auto provisioned users.
        /// </summary>
        public string ProvisionCompany { get; internal set; }

        /// <summary>
        /// 2FA required setting for auto provisioned users. `use_system_setting` uses the site-wide setting, including SSO exemptions. `always_require` and `never_require` override the site-wide setting when user-level overrides are allowed.
        /// </summary>
        public string ProvisionRequire2fa { get; internal set; }

        /// <summary>
        /// File System layout to use for auto provisioned users.
        /// </summary>
        public string ProvisionFilesystemLayout { get; internal set; }

        /// <summary>
        /// URL-friendly, unique identifier for Azure SAML configuration
        /// </summary>
        public string ProviderIdentifier { get; internal set; }

        /// <summary>
        /// Base DN for looking up users in LDAP server
        /// </summary>
        public string LdapBaseDn { get; internal set; }

        /// <summary>
        /// Domain name that will be appended to LDAP usernames
        /// </summary>
        public string LdapDomain { get; internal set; }

        /// <summary>
        /// Should this strategy be displayed on the login page?
        /// </summary>
        public bool DisplayOnLoginPage { get; internal set; }

        /// <summary>
        /// LDAP host
        /// </summary>
        public string LdapHost { get; internal set; }

        /// <summary>
        /// LDAP backup host
        /// </summary>
        public string LdapHost2 { get; internal set; }

        /// <summary>
        /// LDAP backup host
        /// </summary>
        public string LdapHost3 { get; internal set; }

        /// <summary>
        /// LDAP port
        /// </summary>
        public long? LdapPort { get; internal set; }

        /// <summary>
        /// Use LDAP server settings for scheduled provisioning while using this SSO provider for authentication?
        /// </summary>
        public bool LdapProvisioningEnabled { get; internal set; }

        /// <summary>
        /// Use secure LDAP?
        /// </summary>
        public bool LdapSecure { get; internal set; }

        /// <summary>
        /// How to validate the LDAP server certificate. `require_match` validates the certificate chain and hostname; `allow_any` disables certificate validation.
        /// </summary>
        public string LdapServerCertificate { get; internal set; }

        /// <summary>
        /// LDAP server type
        /// </summary>
        public string LdapType { get; internal set; }

        /// <summary>
        /// Username for signing in to LDAP server.
        /// </summary>
        public string LdapUsername { get; internal set; }

        /// <summary>
        /// LDAP username field
        /// </summary>
        public string LdapUsernameField { get; internal set; }
    }
}
