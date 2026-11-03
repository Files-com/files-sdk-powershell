using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SsoStrategy operations with the SDK and projects the SDK's SsoStrategy into the module's.
    internal static class SsoStrategyBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SsoStrategy Project(Sdk.SsoStrategy source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SsoStrategy
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Protocol = source.Protocol,
                Provider = source.Provider,
                Label = source.Label,
                LogoUrl = source.LogoUrl,
                Id = source.Id,
                Enabled = source.Enabled,
                UserCount = source.UserCount,
                SamlProviderCertFingerprint = source.SamlProviderCertFingerprint,
                SamlProviderIssuerUrl = source.SamlProviderIssuerUrl,
                SamlProviderMetadataContent = source.SamlProviderMetadataContent,
                SamlProviderMetadataUrl = source.SamlProviderMetadataUrl,
                SamlProviderSloTargetUrl = source.SamlProviderSloTargetUrl,
                SamlProviderSsoTargetUrl = source.SamlProviderSsoTargetUrl,
                ScimAuthenticationMethod = source.ScimAuthenticationMethod,
                ScimUsername = source.ScimUsername,
                ScimOauthAccessToken = Values.Secret(source.ScimOauthAccessToken),
                ScimOauthAccessTokenExpiresAt = source.ScimOauthAccessTokenExpiresAt,
                Subdomain = source.Subdomain,
                ProvisionUsers = source.ProvisionUsers,
                ProvisionGroups = source.ProvisionGroups,
                DeprovisionUsers = source.DeprovisionUsers,
                DeprovisionGroups = source.DeprovisionGroups,
                DeprovisionBehavior = source.DeprovisionBehavior,
                ProvisionGroupDefault = source.ProvisionGroupDefault,
                ProvisionGroupExclusion = source.ProvisionGroupExclusion,
                ProvisionGroupInclusion = source.ProvisionGroupInclusion,
                ProvisionGroupRequired = source.ProvisionGroupRequired,
                ProvisionEmailSignupGroups = source.ProvisionEmailSignupGroups,
                ProvisionReadonlySiteAdminGroups = source.ProvisionReadonlySiteAdminGroups,
                ProvisionSiteAdminGroups = source.ProvisionSiteAdminGroups,
                ProvisionGroupAdminGroups = source.ProvisionGroupAdminGroups,
                ProvisionAttachmentsPermission = source.ProvisionAttachmentsPermission,
                ProvisionDavPermission = source.ProvisionDavPermission,
                ProvisionFtpPermission = source.ProvisionFtpPermission,
                ProvisionSftpPermission = source.ProvisionSftpPermission,
                ProvisionTimeZone = source.ProvisionTimeZone,
                ProvisionCompany = source.ProvisionCompany,
                ProvisionRequire2fa = source.ProvisionRequire2fa,
                ProvisionFilesystemLayout = source.ProvisionFilesystemLayout,
                ProviderIdentifier = source.ProviderIdentifier,
                LdapBaseDn = source.LdapBaseDn,
                LdapDomain = source.LdapDomain,
                DisplayOnLoginPage = source.DisplayOnLoginPage,
                LdapHost = source.LdapHost,
                LdapHost2 = source.LdapHost2,
                LdapHost3 = source.LdapHost3,
                LdapPort = source.LdapPort,
                LdapProvisioningEnabled = source.LdapProvisioningEnabled,
                LdapSecure = source.LdapSecure,
                LdapServerCertificate = source.LdapServerCertificate,
                LdapType = source.LdapType,
                LdapUsername = source.LdapUsername,
                LdapUsernameField = source.LdapUsernameField,
            };
        }

        internal static FilesPageSource<Models.SsoStrategy> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SsoStrategies.List(parameters, requestOptions), source => SsoStrategyBridge.Project(source, connection, options));
        }

        internal static Task<Models.SsoStrategy> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SsoStrategies.FindAsync(id, parameters, requestOptions, token), source => SsoStrategyBridge.Project(source, connection, options));
        }

        internal static Task SyncAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.SsoStrategy model ? model.SyncAsync(parameters, token) : client.SsoStrategies.SyncAsync(id, parameters, requestOptions, token));
        }
    }
}
