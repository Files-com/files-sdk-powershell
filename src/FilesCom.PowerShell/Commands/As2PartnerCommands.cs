using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List AS2 Partners, or get one as2 partner by Id.
    /// </summary>
    [Cmdlet("Get", "FilesAs2Partner", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.As2Partner), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.As2Partner>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.As2Partner), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesAs2PartnerCommand : FilesListCmdlet<Models.As2Partner>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `as2_station_id` or `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `as2_station_id` and `workspace_id`. Valid field combinations are `[ workspace_id, as2_station_id ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// As2 Partner ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List AS2 Partners, or get one as2 partner by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.As2Partner result = Run(connection, token => As2PartnerBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => As2PartnerBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create AS2 Partner.
    /// </summary>
    [Cmdlet("New", "FilesAs2Partner", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.As2Partner))]
    public sealed class NewFilesAs2PartnerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// If `true`, we will use your site's dedicated IPs for all outbound connections to this AS2 Partner.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? EnableDedicatedIps { get; set; }

        /// <summary>
        /// Username to send to server for HTTP Authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HttpAuthUsername { get; set; }

        /// <summary>
        /// Password to send to server for HTTP Authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString HttpAuthPassword { get; set; }

        /// <summary>
        /// How should Files.com evaluate message transfer success based on a partner's MDN response?  This setting does not affect MDN storage; all MDNs received from a partner are always stored. `none`: MDN is stored for informational purposes only, a successful HTTPS transfer is a successful AS2 transfer. `weak`: Inspect the MDN for MIC and Disposition only. `normal`: `weak` plus validate MDN signature matches body, `strict`: `normal` but do not allow signatures from self-signed or incorrectly purposed certificates. `auto`: Automatically set the correct value for this setting based on next mdn received.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string MdnValidationLevel { get; set; }

        /// <summary>
        /// Should Files.com require signatures on incoming AS2 messages?  `normal`: require that incoming messages are signed with a valid matching signature. `none`: Unsigned incoming messages are allowed. `auto`: A one-time starting state. The next message received is always accepted and switches this to `normal` if it carries a valid signature, or `none` if it does not.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string SignatureValidationLevel { get; set; }

        /// <summary>
        /// Should we require that the remote HTTP server have a valid SSL Certificate for HTTPS? (This only applies to Outgoing AS2 message from Files.com to a Partner.)
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ServerCertificate { get; set; }

        /// <summary>
        /// Default mime type of the file attached to the encrypted message
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string DefaultMimeType { get; set; }

        /// <summary>
        /// Additional HTTP Headers for outgoing message sent to this partner.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary AdditionalHttpHeaders { get; set; }

        /// <summary>
        /// ID of the AS2 Station associated with this partner.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long As2StationId { get; set; }

        /// <summary>
        /// The partner's formal AS2 name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Public URI where we will send the AS2 messages (via HTTP/HTTPS).
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Uri { get; set; }

        /// <summary>
        /// Public certificate for AS2 Partner.  Note: This is the certificate for AS2 message security, not a certificate used for HTTPS authentication.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PublicCertificate { get; set; }

        /// <summary>
        /// Base64-encoded PKCS#12 (.pfx or .p12) file containing the partner's public certificate. Provide this instead of public_certificate. Any private key in the file is discarded. A public-only file must contain exactly one certificate.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Pkcs12 { get; set; }

        /// <summary>
        /// Password for pkcs12. The file and password are used only for import; the extracted public certificate is stored as PEM.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Pkcs12Password { get; set; }

        /// <summary>Create AS2 Partner.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"As2 partner '{Name}'" : "As2 partner"))
            {
                return;
            }
            Models.As2Partner result = Run(connection, token => As2PartnerBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(EnableDedicatedIps)))
            {
                parameters["enable_dedicated_ips"] = EnableDedicatedIps.Value;
            }
            if (IsBound(nameof(HttpAuthUsername)))
            {
                parameters["http_auth_username"] = HttpAuthUsername;
            }
            if (IsBound(nameof(HttpAuthPassword)))
            {
                parameters["http_auth_password"] = RequestValues.Reveal(HttpAuthPassword);
            }
            if (IsBound(nameof(MdnValidationLevel)))
            {
                parameters["mdn_validation_level"] = MdnValidationLevel;
            }
            if (IsBound(nameof(SignatureValidationLevel)))
            {
                parameters["signature_validation_level"] = SignatureValidationLevel;
            }
            if (IsBound(nameof(ServerCertificate)))
            {
                parameters["server_certificate"] = ServerCertificate;
            }
            if (IsBound(nameof(DefaultMimeType)))
            {
                parameters["default_mime_type"] = DefaultMimeType;
            }
            if (IsBound(nameof(AdditionalHttpHeaders)))
            {
                parameters["additional_http_headers"] = RequestValues.Object(AdditionalHttpHeaders, "AdditionalHttpHeaders");
            }
            if (IsBound(nameof(As2StationId)))
            {
                parameters["as2_station_id"] = As2StationId;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Uri)))
            {
                parameters["uri"] = Uri;
            }
            if (IsBound(nameof(PublicCertificate)))
            {
                parameters["public_certificate"] = PublicCertificate;
            }
            if (IsBound(nameof(Pkcs12)))
            {
                parameters["pkcs12"] = RequestValues.Reveal(Pkcs12);
            }
            if (IsBound(nameof(Pkcs12Password)))
            {
                parameters["pkcs12_password"] = RequestValues.Reveal(Pkcs12Password);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update AS2 Partner.
    /// </summary>
    [Cmdlet("Set", "FilesAs2Partner", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.As2Partner), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesAs2PartnerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// As2 Partner ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// If `true`, we will use your site's dedicated IPs for all outbound connections to this AS2 Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? EnableDedicatedIps { get; set; }

        /// <summary>
        /// Username to send to server for HTTP Authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HttpAuthUsername { get; set; }

        /// <summary>
        /// Password to send to server for HTTP Authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString HttpAuthPassword { get; set; }

        /// <summary>
        /// How should Files.com evaluate message transfer success based on a partner's MDN response?  This setting does not affect MDN storage; all MDNs received from a partner are always stored. `none`: MDN is stored for informational purposes only, a successful HTTPS transfer is a successful AS2 transfer. `weak`: Inspect the MDN for MIC and Disposition only. `normal`: `weak` plus validate MDN signature matches body, `strict`: `normal` but do not allow signatures from self-signed or incorrectly purposed certificates. `auto`: Automatically set the correct value for this setting based on next mdn received.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string MdnValidationLevel { get; set; }

        /// <summary>
        /// Should Files.com require signatures on incoming AS2 messages?  `normal`: require that incoming messages are signed with a valid matching signature. `none`: Unsigned incoming messages are allowed. `auto`: A one-time starting state. The next message received is always accepted and switches this to `normal` if it carries a valid signature, or `none` if it does not.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SignatureValidationLevel { get; set; }

        /// <summary>
        /// Should we require that the remote HTTP server have a valid SSL Certificate for HTTPS? (This only applies to Outgoing AS2 message from Files.com to a Partner.)
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ServerCertificate { get; set; }

        /// <summary>
        /// Default mime type of the file attached to the encrypted message
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string DefaultMimeType { get; set; }

        /// <summary>
        /// Additional HTTP Headers for outgoing message sent to this partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary AdditionalHttpHeaders { get; set; }

        /// <summary>
        /// The partner's formal AS2 name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Public URI where we will send the AS2 messages (via HTTP/HTTPS).
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Uri { get; set; }

        /// <summary>
        /// Public certificate for AS2 Partner.  Note: This is the certificate for AS2 message security, not a certificate used for HTTPS authentication.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PublicCertificate { get; set; }

        /// <summary>
        /// Base64-encoded PKCS#12 (.pfx or .p12) file containing the partner's public certificate. Provide this instead of public_certificate. Any private key in the file is discarded. A public-only file must contain exactly one certificate.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString Pkcs12 { get; set; }

        /// <summary>
        /// Password for pkcs12. The file and password are used only for import; the extracted public certificate is stored as PEM.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString Pkcs12Password { get; set; }

        /// <summary>
        /// The as2 partner to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.As2Partner InputObject { get; set; }

        /// <summary>Update AS2 Partner.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"As2 partner {id}"))
            {
                return;
            }
            Models.As2Partner result = Run(connection, token => As2PartnerBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(EnableDedicatedIps)))
            {
                parameters["enable_dedicated_ips"] = EnableDedicatedIps.Value;
            }
            if (IsBound(nameof(HttpAuthUsername)))
            {
                parameters["http_auth_username"] = HttpAuthUsername;
            }
            if (IsBound(nameof(HttpAuthPassword)))
            {
                parameters["http_auth_password"] = RequestValues.Reveal(HttpAuthPassword);
            }
            if (IsBound(nameof(MdnValidationLevel)))
            {
                parameters["mdn_validation_level"] = MdnValidationLevel;
            }
            if (IsBound(nameof(SignatureValidationLevel)))
            {
                parameters["signature_validation_level"] = SignatureValidationLevel;
            }
            if (IsBound(nameof(ServerCertificate)))
            {
                parameters["server_certificate"] = ServerCertificate;
            }
            if (IsBound(nameof(DefaultMimeType)))
            {
                parameters["default_mime_type"] = DefaultMimeType;
            }
            if (IsBound(nameof(AdditionalHttpHeaders)))
            {
                parameters["additional_http_headers"] = RequestValues.Object(AdditionalHttpHeaders, "AdditionalHttpHeaders");
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Uri)))
            {
                parameters["uri"] = Uri;
            }
            if (IsBound(nameof(PublicCertificate)))
            {
                parameters["public_certificate"] = PublicCertificate;
            }
            if (IsBound(nameof(Pkcs12)))
            {
                parameters["pkcs12"] = RequestValues.Reveal(Pkcs12);
            }
            if (IsBound(nameof(Pkcs12Password)))
            {
                parameters["pkcs12_password"] = RequestValues.Reveal(Pkcs12Password);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete AS2 Partner.
    /// </summary>
    [Cmdlet("Remove", "FilesAs2Partner", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesAs2PartnerCommand : FilesApiCmdlet
    {
        /// <summary>
        /// As2 Partner ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The as2 partner to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.As2Partner InputObject { get; set; }

        /// <summary>Delete AS2 Partner.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"As2 partner {id}"))
            {
                return;
            }
            Run(connection, token => As2PartnerBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
