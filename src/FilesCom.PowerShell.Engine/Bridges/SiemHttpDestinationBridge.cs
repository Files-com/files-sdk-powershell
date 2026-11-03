using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SiemHttpDestination operations with the SDK and projects the SDK's SiemHttpDestination into the module's.
    internal static class SiemHttpDestinationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SiemHttpDestination Project(Sdk.SiemHttpDestination source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SiemHttpDestination
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                DestinationType = source.DestinationType,
                DestinationUrl = source.DestinationUrl,
                FileDestinationPath = source.FileDestinationPath,
                FileFormat = source.FileFormat,
                FileIntervalMinutes = source.FileIntervalMinutes,
                AdditionalHeaders = Values.Json(source.AdditionalHeaders),
                SendingActive = source.SendingActive,
                GenericPayloadType = source.GenericPayloadType,
                SplunkTokenMasked = source.SplunkTokenMasked,
                CrowdstrikeTokenMasked = source.CrowdstrikeTokenMasked,
                AzureDcrImmutableId = source.AzureDcrImmutableId,
                AzureStreamName = source.AzureStreamName,
                AzureOauthClientCredentialsTenantId = source.AzureOauthClientCredentialsTenantId,
                AzureOauthClientCredentialsClientId = source.AzureOauthClientCredentialsClientId,
                AzureOauthClientCredentialsClientSecretMasked = source.AzureOauthClientCredentialsClientSecretMasked,
                QradarUsername = source.QradarUsername,
                QradarPasswordMasked = source.QradarPasswordMasked,
                SolarWindsTokenMasked = source.SolarWindsTokenMasked,
                NewRelicApiKeyMasked = source.NewRelicApiKeyMasked,
                DatadogApiKeyMasked = source.DatadogApiKeyMasked,
                ActionSendEnabled = source.ActionSendEnabled,
                ActionEntriesSent = source.ActionEntriesSent,
                SftpActionSendEnabled = source.SftpActionSendEnabled,
                SftpActionEntriesSent = source.SftpActionEntriesSent,
                FtpActionSendEnabled = source.FtpActionSendEnabled,
                FtpActionEntriesSent = source.FtpActionEntriesSent,
                WebDavActionSendEnabled = source.WebDavActionSendEnabled,
                WebDavActionEntriesSent = source.WebDavActionEntriesSent,
                SyncSendEnabled = source.SyncSendEnabled,
                SyncEntriesSent = source.SyncEntriesSent,
                OutboundConnectionSendEnabled = source.OutboundConnectionSendEnabled,
                OutboundConnectionEntriesSent = source.OutboundConnectionEntriesSent,
                AutomationSendEnabled = source.AutomationSendEnabled,
                AutomationEntriesSent = source.AutomationEntriesSent,
                ApiRequestSendEnabled = source.ApiRequestSendEnabled,
                ApiRequestEntriesSent = source.ApiRequestEntriesSent,
                PublicHostingRequestSendEnabled = source.PublicHostingRequestSendEnabled,
                PublicHostingRequestEntriesSent = source.PublicHostingRequestEntriesSent,
                EmailSendEnabled = source.EmailSendEnabled,
                EmailEntriesSent = source.EmailEntriesSent,
                ExavaultApiRequestSendEnabled = source.ExavaultApiRequestSendEnabled,
                ExavaultApiRequestEntriesSent = source.ExavaultApiRequestEntriesSent,
                SettingsChangeSendEnabled = source.SettingsChangeSendEnabled,
                SettingsChangeEntriesSent = source.SettingsChangeEntriesSent,
                LastHttpCallTargetType = source.LastHttpCallTargetType,
                LastHttpCallSuccess = source.LastHttpCallSuccess,
                LastHttpCallResponseCode = source.LastHttpCallResponseCode,
                LastHttpCallResponseBody = source.LastHttpCallResponseBody,
                LastHttpCallErrorMessage = source.LastHttpCallErrorMessage,
                LastHttpCallTime = source.LastHttpCallTime,
                LastHttpCallDurationMs = source.LastHttpCallDurationMs,
                MostRecentHttpCallSuccessTime = source.MostRecentHttpCallSuccessTime,
                ConnectionTestEntry = source.ConnectionTestEntry,
            };
        }

        internal static FilesPageSource<Models.SiemHttpDestination> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SiemHttpDestinations.List(parameters, requestOptions), source => SiemHttpDestinationBridge.Project(source, connection, options));
        }

        internal static Task<Models.SiemHttpDestination> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SiemHttpDestinations.FindAsync(id, parameters, requestOptions, token), source => SiemHttpDestinationBridge.Project(source, connection, options));
        }

        internal static Task<Models.SiemHttpDestination> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SiemHttpDestinations.CreateAsync(parameters, requestOptions, token), source => SiemHttpDestinationBridge.Project(source, connection, options));
        }

        internal static Task SendTestEntryAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SiemHttpDestinations.SendTestEntryAsync(parameters, requestOptions, token));
        }

        internal static Task<Models.SiemHttpDestination> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.SiemHttpDestination model ? model.UpdateAsync(parameters, token) : client.SiemHttpDestinations.UpdateAsync(id, parameters, requestOptions, token), source => SiemHttpDestinationBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.SiemHttpDestination model ? model.DeleteAsync(parameters, token) : client.SiemHttpDestinations.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
