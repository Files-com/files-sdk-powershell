using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs User operations with the SDK and projects the SDK's User into the module's.
    internal static class UserBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.User Project(Sdk.User source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.User
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Username = source.Username,
                AdminGroupIds = Values.Copy(source.AdminGroupIds),
                AllowedIps = source.AllowedIps,
                AttachmentsPermission = source.AttachmentsPermission,
                ApiKeysCount = source.ApiKeysCount,
                AuthenticateUntil = Values.Utc(source.AuthenticateUntil),
                AuthenticationMethod = source.AuthenticationMethod,
                AvatarUrl = source.AvatarUrl,
                Billable = source.Billable,
                BillingPermission = source.BillingPermission,
                BypassSiteAllowedIps = source.BypassSiteAllowedIps,
                BypassUserLifecycleRules = source.BypassUserLifecycleRules,
                CreatedAt = Values.Utc(source.CreatedAt),
                DavPermission = source.DavPermission,
                Disabled = source.Disabled,
                DisabledExpiredOrInactive = source.DisabledExpiredOrInactive,
                AiAssistantPersonalityId = source.AiAssistantPersonalityId,
                DesktopConfigurationProfileId = source.DesktopConfigurationProfileId,
                IntegrationCentricProfileId = source.IntegrationCentricProfileId,
                Email = source.Email,
                FilesystemLayout = source.FilesystemLayout,
                FirstLoginAt = Values.Utc(source.FirstLoginAt),
                FtpPermission = source.FtpPermission,
                GroupIds = source.GroupIds,
                HeaderText = source.HeaderText,
                Language = source.Language,
                LastLoginAt = Values.Utc(source.LastLoginAt),
                LastWebLoginAt = Values.Utc(source.LastWebLoginAt),
                LastFtpLoginAt = Values.Utc(source.LastFtpLoginAt),
                LastSftpLoginAt = Values.Utc(source.LastSftpLoginAt),
                LastDavLoginAt = Values.Utc(source.LastDavLoginAt),
                LastDesktopLoginAt = Values.Utc(source.LastDesktopLoginAt),
                LastRestapiLoginAt = Values.Utc(source.LastRestapiLoginAt),
                LastApiUseAt = Values.Utc(source.LastApiUseAt),
                LastActiveAt = Values.Utc(source.LastActiveAt),
                LastProtocolCipher = source.LastProtocolCipher,
                LockoutExpires = Values.Utc(source.LockoutExpires),
                Name = source.Name,
                Company = source.Company,
                Notes = source.Notes,
                NotificationDailySendTime = source.NotificationDailySendTime,
                OfficeIntegrationEnabled = source.OfficeIntegrationEnabled,
                PartnerAdmin = source.PartnerAdmin,
                PartnerId = source.PartnerId,
                PartnerName = source.PartnerName,
                PasswordSetAt = Values.Utc(source.PasswordSetAt),
                PasswordValidityDays = source.PasswordValidityDays,
                PrimaryGroupId = source.PrimaryGroupId,
                PublicKeysCount = source.PublicKeysCount,
                ReceiveAdminAlerts = source.ReceiveAdminAlerts,
                NotifyOnAllSiteWarnings = source.NotifyOnAllSiteWarnings,
                NotifyOnAllSsoFailures = source.NotifyOnAllSsoFailures,
                NotifyOnAllUserSecurityEvents = source.NotifyOnAllUserSecurityEvents,
                NotifyOnAllPendingWorkFailures = source.NotifyOnAllPendingWorkFailures,
                NotifyOnAllSiemHttpDestinationFailures = source.NotifyOnAllSiemHttpDestinationFailures,
                NotifyOnAllSyncFailures = source.NotifyOnAllSyncFailures,
                NotifyOnAllAutomationFailures = source.NotifyOnAllAutomationFailures,
                NotifyOnAllExpectationFailures = source.NotifyOnAllExpectationFailures,
                Require2fa = source.Require2fa,
                RequireLoginBy = Values.Utc(source.RequireLoginBy),
                Active2fa = source.Active2fa,
                RequirePasswordChange = source.RequirePasswordChange,
                PasswordExpired = source.PasswordExpired,
                ResponsibleGroupId = source.ResponsibleGroupId,
                ResponsibleUserId = source.ResponsibleUserId,
                ReadonlySiteAdmin = source.ReadonlySiteAdmin,
                RestapiPermission = source.RestapiPermission,
                S3CompatibleEndpointPermission = source.S3CompatibleEndpointPermission,
                SelfManaged = source.SelfManaged,
                SftpPermission = source.SftpPermission,
                SiteAdmin = source.SiteAdmin,
                WorkspaceAdmin = source.WorkspaceAdmin,
                SiteId = source.SiteId,
                WorkspaceId = source.WorkspaceId,
                DefaultWorkspaceId = source.DefaultWorkspaceId,
                SkipWelcomeScreen = source.SkipWelcomeScreen,
                FtpTlsSessionReuseRequired = source.FtpTlsSessionReuseRequired,
                SslRequired = source.SslRequired,
                SsoStrategyId = source.SsoStrategyId,
                ExternallyManaged = source.ExternallyManaged,
                Tags = source.Tags,
                TimeZone = source.TimeZone,
                TypeOf2fa = source.TypeOf2fa,
                TypeOf2faForDisplay = source.TypeOf2faForDisplay,
                UserRoot = source.UserRoot,
                UserHome = source.UserHome,
                DaysRemainingUntilPasswordExpire = source.DaysRemainingUntilPasswordExpire,
                PasswordExpireAt = Values.Utc(source.PasswordExpireAt),
            };
        }

        internal static FilesPageSource<Models.User> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Users.List(parameters, requestOptions), source => UserBridge.Project(source, connection, options));
        }

        internal static Task<Models.User> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Users.FindAsync(id, parameters, requestOptions, token), source => UserBridge.Project(source, connection, options));
        }

        internal static Task<Models.User> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Users.CreateAsync(Attachments.ForEngine(parameters), requestOptions, token), source => UserBridge.Project(source, connection, options));
        }

        internal static Task User2faResetAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.User model ? model.User2faResetAsync(parameters, token) : client.Users.User2faResetAsync(id, parameters, requestOptions, token));
        }

        internal static Task ResendWelcomeEmailAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.User model ? model.ResendWelcomeEmailAsync(parameters, token) : client.Users.ResendWelcomeEmailAsync(id, parameters, requestOptions, token));
        }

        internal static Task UnlockAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.User model ? model.UnlockAsync(parameters, token) : client.Users.UnlockAsync(id, parameters, requestOptions, token));
        }

        internal static Task<Models.User> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.User model ? model.UpdateAsync(Attachments.ForEngine(parameters), token) : client.Users.UpdateAsync(id, Attachments.ForEngine(parameters), requestOptions, token), source => UserBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.User model ? model.DeleteAsync(parameters, token) : client.Users.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
