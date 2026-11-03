using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List SIEM HTTP Destinations, or get one siem http destination by Id.
    /// </summary>
    [Cmdlet("Get", "FilesSiemHttpDestination", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.SiemHttpDestination), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.SiemHttpDestination>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.SiemHttpDestination), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesSiemHttpDestinationCommand : FilesListCmdlet<Models.SiemHttpDestination>
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
        /// Siem Http Destination ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List SIEM HTTP Destinations, or get one siem http destination by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.SiemHttpDestination result = Run(connection, token => SiemHttpDestinationBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => SiemHttpDestinationBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create SIEM HTTP Destination.
    /// </summary>
    [Cmdlet("New", "FilesSiemHttpDestination", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.SiemHttpDestination))]
    public sealed class NewFilesSiemHttpDestinationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Name for this Destination
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Additional HTTP Headers included in calls to the destination URL
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary AdditionalHeaders { get; set; }

        /// <summary>
        /// Whether this SIEM HTTP Destination is currently being sent to or not
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SendingActive { get; set; }

        /// <summary>
        /// Applicable only for destination type: generic. Indicates the type of HTTP body. Can be json_newline or json_array. json_newline is multiple log entries as JSON separated by newlines. json_array is a single JSON array containing multiple log entries as JSON.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GenericPayloadType { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Destination folder path on Files.com.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FileDestinationPath { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Generated file format.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FileFormat { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Interval, in minutes, between file deliveries. Valid values are 5, 10, 15, 20, 30, 60, 90, 180, 240, 360.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? FileIntervalMinutes { get; set; }

        /// <summary>
        /// Applicable only for destination types: splunk, splunk_compatible. Authentication token for the destination.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString SplunkToken { get; set; }

        /// <summary>
        /// Applicable only for destination type: crowdstrike. Authentication token provided by Crowdstrike.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString CrowdstrikeToken { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Immutable ID of the Data Collection Rule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureDcrImmutableId { get; set; }

        /// <summary>
        /// Applicable only for destination type: azure. Name of the stream in the DCR that represents the destination table.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureStreamName { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Tenant ID.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureOauthClientCredentialsTenantId { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Client ID.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureOauthClientCredentialsClientId { get; set; }

        /// <summary>
        /// Applicable only for destination type: azure. Client Credentials OAuth Client Secret.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString AzureOauthClientCredentialsClientSecret { get; set; }

        /// <summary>
        /// Applicable only for destination type: qradar. Basic auth username provided by QRadar.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QradarUsername { get; set; }

        /// <summary>
        /// Applicable only for destination type: qradar. Basic auth password provided by QRadar.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString QradarPassword { get; set; }

        /// <summary>
        /// Applicable only for destination type: solar_winds. Authentication token provided by Solar Winds.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString SolarWindsToken { get; set; }

        /// <summary>
        /// Applicable only for destination type: new_relic. API key provided by New Relic.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString NewRelicApiKey { get; set; }

        /// <summary>
        /// Applicable only for destination type: datadog. API key provided by Datadog.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString DatadogApiKey { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for action logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for sftp_action logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for ftp_action logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FtpActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for web_dav_action logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WebDavActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for sync logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SyncSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for outbound_connection logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? OutboundConnectionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for automation logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AutomationSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for api_request logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ApiRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for public_hosting_request logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PublicHostingRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for email logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? EmailSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for exavault_api_request logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ExavaultApiRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for settings_change logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SettingsChangeSendEnabled { get; set; }

        /// <summary>
        /// Destination Type
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string DestinationType { get; set; }

        /// <summary>
        /// Destination Url
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DestinationUrl { get; set; }

        /// <summary>Create SIEM HTTP Destination.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Siem http destination '{Name}'" : "Siem http destination"))
            {
                return;
            }
            Models.SiemHttpDestination result = Run(connection, token => SiemHttpDestinationBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(AdditionalHeaders)))
            {
                parameters["additional_headers"] = RequestValues.Object(AdditionalHeaders, "AdditionalHeaders");
            }
            if (IsBound(nameof(SendingActive)))
            {
                parameters["sending_active"] = SendingActive.Value;
            }
            if (IsBound(nameof(GenericPayloadType)))
            {
                parameters["generic_payload_type"] = GenericPayloadType;
            }
            if (IsBound(nameof(FileDestinationPath)))
            {
                parameters["file_destination_path"] = FileDestinationPath;
            }
            if (IsBound(nameof(FileFormat)))
            {
                parameters["file_format"] = FileFormat;
            }
            if (IsBound(nameof(FileIntervalMinutes)))
            {
                parameters["file_interval_minutes"] = FileIntervalMinutes.Value;
            }
            if (IsBound(nameof(SplunkToken)))
            {
                parameters["splunk_token"] = RequestValues.Reveal(SplunkToken);
            }
            if (IsBound(nameof(CrowdstrikeToken)))
            {
                parameters["crowdstrike_token"] = RequestValues.Reveal(CrowdstrikeToken);
            }
            if (IsBound(nameof(AzureDcrImmutableId)))
            {
                parameters["azure_dcr_immutable_id"] = AzureDcrImmutableId;
            }
            if (IsBound(nameof(AzureStreamName)))
            {
                parameters["azure_stream_name"] = AzureStreamName;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsTenantId)))
            {
                parameters["azure_oauth_client_credentials_tenant_id"] = AzureOauthClientCredentialsTenantId;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsClientId)))
            {
                parameters["azure_oauth_client_credentials_client_id"] = AzureOauthClientCredentialsClientId;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsClientSecret)))
            {
                parameters["azure_oauth_client_credentials_client_secret"] = RequestValues.Reveal(AzureOauthClientCredentialsClientSecret);
            }
            if (IsBound(nameof(QradarUsername)))
            {
                parameters["qradar_username"] = QradarUsername;
            }
            if (IsBound(nameof(QradarPassword)))
            {
                parameters["qradar_password"] = RequestValues.Reveal(QradarPassword);
            }
            if (IsBound(nameof(SolarWindsToken)))
            {
                parameters["solar_winds_token"] = RequestValues.Reveal(SolarWindsToken);
            }
            if (IsBound(nameof(NewRelicApiKey)))
            {
                parameters["new_relic_api_key"] = RequestValues.Reveal(NewRelicApiKey);
            }
            if (IsBound(nameof(DatadogApiKey)))
            {
                parameters["datadog_api_key"] = RequestValues.Reveal(DatadogApiKey);
            }
            if (IsBound(nameof(ActionSendEnabled)))
            {
                parameters["action_send_enabled"] = ActionSendEnabled.Value;
            }
            if (IsBound(nameof(SftpActionSendEnabled)))
            {
                parameters["sftp_action_send_enabled"] = SftpActionSendEnabled.Value;
            }
            if (IsBound(nameof(FtpActionSendEnabled)))
            {
                parameters["ftp_action_send_enabled"] = FtpActionSendEnabled.Value;
            }
            if (IsBound(nameof(WebDavActionSendEnabled)))
            {
                parameters["web_dav_action_send_enabled"] = WebDavActionSendEnabled.Value;
            }
            if (IsBound(nameof(SyncSendEnabled)))
            {
                parameters["sync_send_enabled"] = SyncSendEnabled.Value;
            }
            if (IsBound(nameof(OutboundConnectionSendEnabled)))
            {
                parameters["outbound_connection_send_enabled"] = OutboundConnectionSendEnabled.Value;
            }
            if (IsBound(nameof(AutomationSendEnabled)))
            {
                parameters["automation_send_enabled"] = AutomationSendEnabled.Value;
            }
            if (IsBound(nameof(ApiRequestSendEnabled)))
            {
                parameters["api_request_send_enabled"] = ApiRequestSendEnabled.Value;
            }
            if (IsBound(nameof(PublicHostingRequestSendEnabled)))
            {
                parameters["public_hosting_request_send_enabled"] = PublicHostingRequestSendEnabled.Value;
            }
            if (IsBound(nameof(EmailSendEnabled)))
            {
                parameters["email_send_enabled"] = EmailSendEnabled.Value;
            }
            if (IsBound(nameof(ExavaultApiRequestSendEnabled)))
            {
                parameters["exavault_api_request_send_enabled"] = ExavaultApiRequestSendEnabled.Value;
            }
            if (IsBound(nameof(SettingsChangeSendEnabled)))
            {
                parameters["settings_change_send_enabled"] = SettingsChangeSendEnabled.Value;
            }
            if (IsBound(nameof(DestinationType)))
            {
                parameters["destination_type"] = DestinationType;
            }
            if (IsBound(nameof(DestinationUrl)))
            {
                parameters["destination_url"] = DestinationUrl;
            }
            return parameters;
        }
    }

    /// <summary>
    /// send_test_entry SIEM HTTP Destination.
    /// </summary>
    [Cmdlet("Send", "FilesSiemHttpDestinationTestEntry", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class SendFilesSiemHttpDestinationTestEntryCommand : FilesApiCmdlet
    {
        /// <summary>
        /// SIEM HTTP Destination ID
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SiemHttpDestinationId { get; set; }

        /// <summary>
        /// Destination Type
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DestinationType { get; set; }

        /// <summary>
        /// Destination Url
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DestinationUrl { get; set; }

        /// <summary>
        /// Name for this Destination
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Additional HTTP Headers included in calls to the destination URL
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary AdditionalHeaders { get; set; }

        /// <summary>
        /// Whether this SIEM HTTP Destination is currently being sent to or not
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SendingActive { get; set; }

        /// <summary>
        /// Applicable only for destination type: generic. Indicates the type of HTTP body. Can be json_newline or json_array. json_newline is multiple log entries as JSON separated by newlines. json_array is a single JSON array containing multiple log entries as JSON.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GenericPayloadType { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Destination folder path on Files.com.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FileDestinationPath { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Generated file format.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FileFormat { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Interval, in minutes, between file deliveries. Valid values are 5, 10, 15, 20, 30, 60, 90, 180, 240, 360.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? FileIntervalMinutes { get; set; }

        /// <summary>
        /// Applicable only for destination types: splunk, splunk_compatible. Authentication token for the destination.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString SplunkToken { get; set; }

        /// <summary>
        /// Applicable only for destination type: crowdstrike. Authentication token provided by Crowdstrike.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString CrowdstrikeToken { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Immutable ID of the Data Collection Rule.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureDcrImmutableId { get; set; }

        /// <summary>
        /// Applicable only for destination type: azure. Name of the stream in the DCR that represents the destination table.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureStreamName { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Tenant ID.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureOauthClientCredentialsTenantId { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Client ID.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string AzureOauthClientCredentialsClientId { get; set; }

        /// <summary>
        /// Applicable only for destination type: azure. Client Credentials OAuth Client Secret.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString AzureOauthClientCredentialsClientSecret { get; set; }

        /// <summary>
        /// Applicable only for destination type: qradar. Basic auth username provided by QRadar.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string QradarUsername { get; set; }

        /// <summary>
        /// Applicable only for destination type: qradar. Basic auth password provided by QRadar.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString QradarPassword { get; set; }

        /// <summary>
        /// Applicable only for destination type: solar_winds. Authentication token provided by Solar Winds.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString SolarWindsToken { get; set; }

        /// <summary>
        /// Applicable only for destination type: new_relic. API key provided by New Relic.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString NewRelicApiKey { get; set; }

        /// <summary>
        /// Applicable only for destination type: datadog. API key provided by Datadog.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString DatadogApiKey { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for action logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for sftp_action logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SftpActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for ftp_action logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FtpActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for web_dav_action logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WebDavActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for sync logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SyncSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for outbound_connection logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? OutboundConnectionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for automation logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AutomationSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for api_request logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ApiRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for public_hosting_request logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? PublicHostingRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for email logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? EmailSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for exavault_api_request logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ExavaultApiRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for settings_change logs.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SettingsChangeSendEnabled { get; set; }

        /// <summary>send_test_entry SIEM HTTP Destination.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = SendTestEntryParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Siem http destination '{Name}'" : "Siem http destination"))
            {
                return;
            }
            Run(connection, token => SiemHttpDestinationBridge.SendTestEntryAsync(connection, parameters, CallOptions, token));
        }

        private Dictionary<string, object> SendTestEntryParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(SiemHttpDestinationId)))
            {
                parameters["siem_http_destination_id"] = SiemHttpDestinationId.Value;
            }
            if (IsBound(nameof(DestinationType)))
            {
                parameters["destination_type"] = DestinationType;
            }
            if (IsBound(nameof(DestinationUrl)))
            {
                parameters["destination_url"] = DestinationUrl;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(AdditionalHeaders)))
            {
                parameters["additional_headers"] = RequestValues.Object(AdditionalHeaders, "AdditionalHeaders");
            }
            if (IsBound(nameof(SendingActive)))
            {
                parameters["sending_active"] = SendingActive.Value;
            }
            if (IsBound(nameof(GenericPayloadType)))
            {
                parameters["generic_payload_type"] = GenericPayloadType;
            }
            if (IsBound(nameof(FileDestinationPath)))
            {
                parameters["file_destination_path"] = FileDestinationPath;
            }
            if (IsBound(nameof(FileFormat)))
            {
                parameters["file_format"] = FileFormat;
            }
            if (IsBound(nameof(FileIntervalMinutes)))
            {
                parameters["file_interval_minutes"] = FileIntervalMinutes.Value;
            }
            if (IsBound(nameof(SplunkToken)))
            {
                parameters["splunk_token"] = RequestValues.Reveal(SplunkToken);
            }
            if (IsBound(nameof(CrowdstrikeToken)))
            {
                parameters["crowdstrike_token"] = RequestValues.Reveal(CrowdstrikeToken);
            }
            if (IsBound(nameof(AzureDcrImmutableId)))
            {
                parameters["azure_dcr_immutable_id"] = AzureDcrImmutableId;
            }
            if (IsBound(nameof(AzureStreamName)))
            {
                parameters["azure_stream_name"] = AzureStreamName;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsTenantId)))
            {
                parameters["azure_oauth_client_credentials_tenant_id"] = AzureOauthClientCredentialsTenantId;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsClientId)))
            {
                parameters["azure_oauth_client_credentials_client_id"] = AzureOauthClientCredentialsClientId;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsClientSecret)))
            {
                parameters["azure_oauth_client_credentials_client_secret"] = RequestValues.Reveal(AzureOauthClientCredentialsClientSecret);
            }
            if (IsBound(nameof(QradarUsername)))
            {
                parameters["qradar_username"] = QradarUsername;
            }
            if (IsBound(nameof(QradarPassword)))
            {
                parameters["qradar_password"] = RequestValues.Reveal(QradarPassword);
            }
            if (IsBound(nameof(SolarWindsToken)))
            {
                parameters["solar_winds_token"] = RequestValues.Reveal(SolarWindsToken);
            }
            if (IsBound(nameof(NewRelicApiKey)))
            {
                parameters["new_relic_api_key"] = RequestValues.Reveal(NewRelicApiKey);
            }
            if (IsBound(nameof(DatadogApiKey)))
            {
                parameters["datadog_api_key"] = RequestValues.Reveal(DatadogApiKey);
            }
            if (IsBound(nameof(ActionSendEnabled)))
            {
                parameters["action_send_enabled"] = ActionSendEnabled.Value;
            }
            if (IsBound(nameof(SftpActionSendEnabled)))
            {
                parameters["sftp_action_send_enabled"] = SftpActionSendEnabled.Value;
            }
            if (IsBound(nameof(FtpActionSendEnabled)))
            {
                parameters["ftp_action_send_enabled"] = FtpActionSendEnabled.Value;
            }
            if (IsBound(nameof(WebDavActionSendEnabled)))
            {
                parameters["web_dav_action_send_enabled"] = WebDavActionSendEnabled.Value;
            }
            if (IsBound(nameof(SyncSendEnabled)))
            {
                parameters["sync_send_enabled"] = SyncSendEnabled.Value;
            }
            if (IsBound(nameof(OutboundConnectionSendEnabled)))
            {
                parameters["outbound_connection_send_enabled"] = OutboundConnectionSendEnabled.Value;
            }
            if (IsBound(nameof(AutomationSendEnabled)))
            {
                parameters["automation_send_enabled"] = AutomationSendEnabled.Value;
            }
            if (IsBound(nameof(ApiRequestSendEnabled)))
            {
                parameters["api_request_send_enabled"] = ApiRequestSendEnabled.Value;
            }
            if (IsBound(nameof(PublicHostingRequestSendEnabled)))
            {
                parameters["public_hosting_request_send_enabled"] = PublicHostingRequestSendEnabled.Value;
            }
            if (IsBound(nameof(EmailSendEnabled)))
            {
                parameters["email_send_enabled"] = EmailSendEnabled.Value;
            }
            if (IsBound(nameof(ExavaultApiRequestSendEnabled)))
            {
                parameters["exavault_api_request_send_enabled"] = ExavaultApiRequestSendEnabled.Value;
            }
            if (IsBound(nameof(SettingsChangeSendEnabled)))
            {
                parameters["settings_change_send_enabled"] = SettingsChangeSendEnabled.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update SIEM HTTP Destination.
    /// </summary>
    [Cmdlet("Set", "FilesSiemHttpDestination", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.SiemHttpDestination), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesSiemHttpDestinationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Siem Http Destination ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Name for this Destination
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Additional HTTP Headers included in calls to the destination URL
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary AdditionalHeaders { get; set; }

        /// <summary>
        /// Whether this SIEM HTTP Destination is currently being sent to or not
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SendingActive { get; set; }

        /// <summary>
        /// Applicable only for destination type: generic. Indicates the type of HTTP body. Can be json_newline or json_array. json_newline is multiple log entries as JSON separated by newlines. json_array is a single JSON array containing multiple log entries as JSON.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string GenericPayloadType { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Destination folder path on Files.com.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FileDestinationPath { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Generated file format.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FileFormat { get; set; }

        /// <summary>
        /// Applicable only for destination type: file. Interval, in minutes, between file deliveries. Valid values are 5, 10, 15, 20, 30, 60, 90, 180, 240, 360.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? FileIntervalMinutes { get; set; }

        /// <summary>
        /// Applicable only for destination types: splunk, splunk_compatible. Authentication token for the destination.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString SplunkToken { get; set; }

        /// <summary>
        /// Applicable only for destination type: crowdstrike. Authentication token provided by Crowdstrike.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString CrowdstrikeToken { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Immutable ID of the Data Collection Rule.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureDcrImmutableId { get; set; }

        /// <summary>
        /// Applicable only for destination type: azure. Name of the stream in the DCR that represents the destination table.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureStreamName { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Tenant ID.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureOauthClientCredentialsTenantId { get; set; }

        /// <summary>
        /// Applicable only for destination types: azure, azure_legacy. Client Credentials OAuth Client ID.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string AzureOauthClientCredentialsClientId { get; set; }

        /// <summary>
        /// Applicable only for destination type: azure. Client Credentials OAuth Client Secret.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString AzureOauthClientCredentialsClientSecret { get; set; }

        /// <summary>
        /// Applicable only for destination type: qradar. Basic auth username provided by QRadar.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string QradarUsername { get; set; }

        /// <summary>
        /// Applicable only for destination type: qradar. Basic auth password provided by QRadar.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString QradarPassword { get; set; }

        /// <summary>
        /// Applicable only for destination type: solar_winds. Authentication token provided by Solar Winds.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString SolarWindsToken { get; set; }

        /// <summary>
        /// Applicable only for destination type: new_relic. API key provided by New Relic.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString NewRelicApiKey { get; set; }

        /// <summary>
        /// Applicable only for destination type: datadog. API key provided by Datadog.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString DatadogApiKey { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for action logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for sftp_action logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SftpActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for ftp_action logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? FtpActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for web_dav_action logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? WebDavActionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for sync logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SyncSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for outbound_connection logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? OutboundConnectionSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for automation logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AutomationSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for api_request logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ApiRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for public_hosting_request logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? PublicHostingRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for email logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? EmailSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for exavault_api_request logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ExavaultApiRequestSendEnabled { get; set; }

        /// <summary>
        /// Whether or not sending is enabled for settings_change logs.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SettingsChangeSendEnabled { get; set; }

        /// <summary>
        /// Destination Type
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string DestinationType { get; set; }

        /// <summary>
        /// Destination Url
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string DestinationUrl { get; set; }

        /// <summary>
        /// The siem http destination to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.SiemHttpDestination InputObject { get; set; }

        /// <summary>Update SIEM HTTP Destination.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Siem http destination {id}"))
            {
                return;
            }
            Models.SiemHttpDestination result = Run(connection, token => SiemHttpDestinationBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(AdditionalHeaders)))
            {
                parameters["additional_headers"] = RequestValues.Object(AdditionalHeaders, "AdditionalHeaders");
            }
            if (IsBound(nameof(SendingActive)))
            {
                parameters["sending_active"] = SendingActive.Value;
            }
            if (IsBound(nameof(GenericPayloadType)))
            {
                parameters["generic_payload_type"] = GenericPayloadType;
            }
            if (IsBound(nameof(FileDestinationPath)))
            {
                parameters["file_destination_path"] = FileDestinationPath;
            }
            if (IsBound(nameof(FileFormat)))
            {
                parameters["file_format"] = FileFormat;
            }
            if (IsBound(nameof(FileIntervalMinutes)))
            {
                parameters["file_interval_minutes"] = FileIntervalMinutes.Value;
            }
            if (IsBound(nameof(SplunkToken)))
            {
                parameters["splunk_token"] = RequestValues.Reveal(SplunkToken);
            }
            if (IsBound(nameof(CrowdstrikeToken)))
            {
                parameters["crowdstrike_token"] = RequestValues.Reveal(CrowdstrikeToken);
            }
            if (IsBound(nameof(AzureDcrImmutableId)))
            {
                parameters["azure_dcr_immutable_id"] = AzureDcrImmutableId;
            }
            if (IsBound(nameof(AzureStreamName)))
            {
                parameters["azure_stream_name"] = AzureStreamName;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsTenantId)))
            {
                parameters["azure_oauth_client_credentials_tenant_id"] = AzureOauthClientCredentialsTenantId;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsClientId)))
            {
                parameters["azure_oauth_client_credentials_client_id"] = AzureOauthClientCredentialsClientId;
            }
            if (IsBound(nameof(AzureOauthClientCredentialsClientSecret)))
            {
                parameters["azure_oauth_client_credentials_client_secret"] = RequestValues.Reveal(AzureOauthClientCredentialsClientSecret);
            }
            if (IsBound(nameof(QradarUsername)))
            {
                parameters["qradar_username"] = QradarUsername;
            }
            if (IsBound(nameof(QradarPassword)))
            {
                parameters["qradar_password"] = RequestValues.Reveal(QradarPassword);
            }
            if (IsBound(nameof(SolarWindsToken)))
            {
                parameters["solar_winds_token"] = RequestValues.Reveal(SolarWindsToken);
            }
            if (IsBound(nameof(NewRelicApiKey)))
            {
                parameters["new_relic_api_key"] = RequestValues.Reveal(NewRelicApiKey);
            }
            if (IsBound(nameof(DatadogApiKey)))
            {
                parameters["datadog_api_key"] = RequestValues.Reveal(DatadogApiKey);
            }
            if (IsBound(nameof(ActionSendEnabled)))
            {
                parameters["action_send_enabled"] = ActionSendEnabled.Value;
            }
            if (IsBound(nameof(SftpActionSendEnabled)))
            {
                parameters["sftp_action_send_enabled"] = SftpActionSendEnabled.Value;
            }
            if (IsBound(nameof(FtpActionSendEnabled)))
            {
                parameters["ftp_action_send_enabled"] = FtpActionSendEnabled.Value;
            }
            if (IsBound(nameof(WebDavActionSendEnabled)))
            {
                parameters["web_dav_action_send_enabled"] = WebDavActionSendEnabled.Value;
            }
            if (IsBound(nameof(SyncSendEnabled)))
            {
                parameters["sync_send_enabled"] = SyncSendEnabled.Value;
            }
            if (IsBound(nameof(OutboundConnectionSendEnabled)))
            {
                parameters["outbound_connection_send_enabled"] = OutboundConnectionSendEnabled.Value;
            }
            if (IsBound(nameof(AutomationSendEnabled)))
            {
                parameters["automation_send_enabled"] = AutomationSendEnabled.Value;
            }
            if (IsBound(nameof(ApiRequestSendEnabled)))
            {
                parameters["api_request_send_enabled"] = ApiRequestSendEnabled.Value;
            }
            if (IsBound(nameof(PublicHostingRequestSendEnabled)))
            {
                parameters["public_hosting_request_send_enabled"] = PublicHostingRequestSendEnabled.Value;
            }
            if (IsBound(nameof(EmailSendEnabled)))
            {
                parameters["email_send_enabled"] = EmailSendEnabled.Value;
            }
            if (IsBound(nameof(ExavaultApiRequestSendEnabled)))
            {
                parameters["exavault_api_request_send_enabled"] = ExavaultApiRequestSendEnabled.Value;
            }
            if (IsBound(nameof(SettingsChangeSendEnabled)))
            {
                parameters["settings_change_send_enabled"] = SettingsChangeSendEnabled.Value;
            }
            if (IsBound(nameof(DestinationType)))
            {
                parameters["destination_type"] = DestinationType;
            }
            if (IsBound(nameof(DestinationUrl)))
            {
                parameters["destination_url"] = DestinationUrl;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete SIEM HTTP Destination.
    /// </summary>
    [Cmdlet("Remove", "FilesSiemHttpDestination", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesSiemHttpDestinationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Siem Http Destination ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The siem http destination to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.SiemHttpDestination InputObject { get; set; }

        /// <summary>Delete SIEM HTTP Destination.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Siem http destination {id}"))
            {
                return;
            }
            Run(connection, token => SiemHttpDestinationBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
