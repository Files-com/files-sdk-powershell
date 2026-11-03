using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A SIEM HTTP Destination defines where Files.com sends the log types you select.
    /// </summary>
    public sealed class SiemHttpDestination : FilesObject
    {
        internal SiemHttpDestination()
        {
        }

        /// <summary>
        /// SIEM HTTP Destination ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Name for this Destination
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Destination Type
        /// </summary>
        public string DestinationType { get; internal set; }

        /// <summary>
        /// Destination Url
        /// </summary>
        public string DestinationUrl { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: file. Destination folder path on Files.com.
        /// </summary>
        public string FileDestinationPath { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: file. Generated file format.
        /// </summary>
        public string FileFormat { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: file. Interval, in minutes, between file deliveries.
        /// </summary>
        public long? FileIntervalMinutes { get; internal set; }

        /// <summary>
        /// Additional HTTP Headers included in calls to the destination URL
        /// </summary>
        public object AdditionalHeaders { get; internal set; }

        /// <summary>
        /// Whether this SIEM HTTP Destination is currently being sent to or not
        /// </summary>
        public bool SendingActive { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: generic. Indicates the type of HTTP body. Can be json_newline or json_array. json_newline is multiple log entries as JSON separated by newlines. json_array is a single JSON array containing multiple log entries as JSON.
        /// </summary>
        public string GenericPayloadType { get; internal set; }

        /// <summary>
        /// Applicable only for destination types: splunk, splunk_compatible. Authentication token for the destination.
        /// </summary>
        public string SplunkTokenMasked { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: crowdstrike. Authentication token provided by Crowdstrike.
        /// </summary>
        public string CrowdstrikeTokenMasked { get; internal set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Immutable ID of the Data Collection Rule.
        /// </summary>
        public string AzureDcrImmutableId { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: azure. Name of the stream in the DCR that represents the destination table.
        /// </summary>
        public string AzureStreamName { get; internal set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Tenant ID.
        /// </summary>
        public string AzureOauthClientCredentialsTenantId { get; internal set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Client ID.
        /// </summary>
        public string AzureOauthClientCredentialsClientId { get; internal set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Client Secret.
        /// </summary>
        public string AzureOauthClientCredentialsClientSecretMasked { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: qradar. Basic auth username provided by QRadar.
        /// </summary>
        public string QradarUsername { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: qradar. Basic auth password provided by QRadar.
        /// </summary>
        public string QradarPasswordMasked { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: solar_winds. Authentication token provided by Solar Winds.
        /// </summary>
        public string SolarWindsTokenMasked { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: new_relic. API key provided by New Relic.
        /// </summary>
        public string NewRelicApiKeyMasked { get; internal set; }

        /// <summary>
        /// Applicable only for destination type: datadog. API key provided by Datadog.
        /// </summary>
        public string DatadogApiKeyMasked { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for action logs.
        /// </summary>
        public bool ActionSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? ActionEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for sftp_action logs.
        /// </summary>
        public bool SftpActionSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? SftpActionEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for ftp_action logs.
        /// </summary>
        public bool FtpActionSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? FtpActionEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for web_dav_action logs.
        /// </summary>
        public bool WebDavActionSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? WebDavActionEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for sync logs.
        /// </summary>
        public bool SyncSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? SyncEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for outbound_connection logs.
        /// </summary>
        public bool OutboundConnectionSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? OutboundConnectionEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for automation logs.
        /// </summary>
        public bool AutomationSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? AutomationEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for api_request logs.
        /// </summary>
        public bool ApiRequestSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? ApiRequestEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for public_hosting_request logs.
        /// </summary>
        public bool PublicHostingRequestSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? PublicHostingRequestEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for email logs.
        /// </summary>
        public bool EmailSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? EmailEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for exavault_api_request logs.
        /// </summary>
        public bool ExavaultApiRequestSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? ExavaultApiRequestEntriesSent { get; internal set; }

        /// <summary>
        /// Whether or not sending is enabled for settings_change logs.
        /// </summary>
        public bool SettingsChangeSendEnabled { get; internal set; }

        /// <summary>
        /// Number of log entries sent for the lifetime of this destination.
        /// </summary>
        public long? SettingsChangeEntriesSent { get; internal set; }

        /// <summary>
        /// Type of URL that was last called. Can be `destination_url` or `azure_oauth_client_credentials_url`
        /// </summary>
        public string LastHttpCallTargetType { get; internal set; }

        /// <summary>
        /// Was the last HTTP call made successful?
        /// </summary>
        public bool LastHttpCallSuccess { get; internal set; }

        /// <summary>
        /// Last HTTP Call Response Code
        /// </summary>
        public long? LastHttpCallResponseCode { get; internal set; }

        /// <summary>
        /// Last HTTP Call Response Body. Large responses are truncated.
        /// </summary>
        public string LastHttpCallResponseBody { get; internal set; }

        /// <summary>
        /// Last HTTP Call Error Message if applicable
        /// </summary>
        public string LastHttpCallErrorMessage { get; internal set; }

        /// <summary>
        /// Time of Last HTTP Call
        /// </summary>
        public string LastHttpCallTime { get; internal set; }

        /// <summary>
        /// Duration of the last HTTP Call in milliseconds
        /// </summary>
        public long? LastHttpCallDurationMs { get; internal set; }

        /// <summary>
        /// Time of Most Recent Successful HTTP Call
        /// </summary>
        public string MostRecentHttpCallSuccessTime { get; internal set; }

        /// <summary>
        /// Connection Test Entry
        /// </summary>
        public string ConnectionTestEntry { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(SiemHttpDestination source)
        {
            Id = source.Id;
            Name = source.Name;
            DestinationType = source.DestinationType;
            DestinationUrl = source.DestinationUrl;
            FileDestinationPath = source.FileDestinationPath;
            FileFormat = source.FileFormat;
            FileIntervalMinutes = source.FileIntervalMinutes;
            AdditionalHeaders = source.AdditionalHeaders;
            SendingActive = source.SendingActive;
            GenericPayloadType = source.GenericPayloadType;
            SplunkTokenMasked = source.SplunkTokenMasked;
            CrowdstrikeTokenMasked = source.CrowdstrikeTokenMasked;
            AzureDcrImmutableId = source.AzureDcrImmutableId;
            AzureStreamName = source.AzureStreamName;
            AzureOauthClientCredentialsTenantId = source.AzureOauthClientCredentialsTenantId;
            AzureOauthClientCredentialsClientId = source.AzureOauthClientCredentialsClientId;
            AzureOauthClientCredentialsClientSecretMasked = source.AzureOauthClientCredentialsClientSecretMasked;
            QradarUsername = source.QradarUsername;
            QradarPasswordMasked = source.QradarPasswordMasked;
            SolarWindsTokenMasked = source.SolarWindsTokenMasked;
            NewRelicApiKeyMasked = source.NewRelicApiKeyMasked;
            DatadogApiKeyMasked = source.DatadogApiKeyMasked;
            ActionSendEnabled = source.ActionSendEnabled;
            ActionEntriesSent = source.ActionEntriesSent;
            SftpActionSendEnabled = source.SftpActionSendEnabled;
            SftpActionEntriesSent = source.SftpActionEntriesSent;
            FtpActionSendEnabled = source.FtpActionSendEnabled;
            FtpActionEntriesSent = source.FtpActionEntriesSent;
            WebDavActionSendEnabled = source.WebDavActionSendEnabled;
            WebDavActionEntriesSent = source.WebDavActionEntriesSent;
            SyncSendEnabled = source.SyncSendEnabled;
            SyncEntriesSent = source.SyncEntriesSent;
            OutboundConnectionSendEnabled = source.OutboundConnectionSendEnabled;
            OutboundConnectionEntriesSent = source.OutboundConnectionEntriesSent;
            AutomationSendEnabled = source.AutomationSendEnabled;
            AutomationEntriesSent = source.AutomationEntriesSent;
            ApiRequestSendEnabled = source.ApiRequestSendEnabled;
            ApiRequestEntriesSent = source.ApiRequestEntriesSent;
            PublicHostingRequestSendEnabled = source.PublicHostingRequestSendEnabled;
            PublicHostingRequestEntriesSent = source.PublicHostingRequestEntriesSent;
            EmailSendEnabled = source.EmailSendEnabled;
            EmailEntriesSent = source.EmailEntriesSent;
            ExavaultApiRequestSendEnabled = source.ExavaultApiRequestSendEnabled;
            ExavaultApiRequestEntriesSent = source.ExavaultApiRequestEntriesSent;
            SettingsChangeSendEnabled = source.SettingsChangeSendEnabled;
            SettingsChangeEntriesSent = source.SettingsChangeEntriesSent;
            LastHttpCallTargetType = source.LastHttpCallTargetType;
            LastHttpCallSuccess = source.LastHttpCallSuccess;
            LastHttpCallResponseCode = source.LastHttpCallResponseCode;
            LastHttpCallResponseBody = source.LastHttpCallResponseBody;
            LastHttpCallErrorMessage = source.LastHttpCallErrorMessage;
            LastHttpCallTime = source.LastHttpCallTime;
            LastHttpCallDurationMs = source.LastHttpCallDurationMs;
            MostRecentHttpCallSuccessTime = source.MostRecentHttpCallSuccessTime;
            ConnectionTestEntry = source.ConnectionTestEntry;
        }
    }
}
