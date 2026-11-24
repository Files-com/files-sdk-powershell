using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Certificates, or get one certificate by Id.
    /// </summary>
    [Cmdlet("Get", "FilesCertificate", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Certificate), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Certificate>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Certificate), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesCertificateCommand : FilesListCmdlet<Models.Certificate>
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
        /// Certificate ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Certificates, or get one certificate by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Certificate result = Run(connection, token => CertificateBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => CertificateBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create CSR or import existing SSL Certificate.
    /// </summary>
    [Cmdlet("New", "FilesCertificate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Certificate))]
    public sealed class NewFilesCertificateCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Internal name of the SSL certificate.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Domain for certificate.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CertificateDomain { get; set; }

        /// <summary>
        /// Additional domains for the certificate CSR.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] CertificateExtraDomains { get; set; }

        /// <summary>
        /// Country.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CertificateCountry { get; set; }

        /// <summary>
        /// State or province.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CertificateStateOrProvince { get; set; }

        /// <summary>
        /// City or locale.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CertificateCityOrLocale { get; set; }

        /// <summary>
        /// Company name.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CertificateCompanyName { get; set; }

        /// <summary>
        /// Department name or organization unit 1
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CsrOu1 { get; set; }

        /// <summary>
        /// Department name or organization unit 2
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CsrOu2 { get; set; }

        /// <summary>
        /// Department name or organization unit 3
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CsrOu3 { get; set; }

        /// <summary>
        /// Email address for the certificate owner.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string CertificateEmailAddress { get; set; }

        /// <summary>
        /// Any supported key type.  Defaults to `RSA-4096`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string KeyType { get; set; }

        /// <summary>
        /// The certificate.  PEM or PKCS7 formats accepted.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Certificate { get; set; }

        /// <summary>
        /// Certificate private key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>
        /// Certificate password.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Password { get; set; }

        /// <summary>
        /// Intermediate certificates.  PEM or PKCS7 formats accepted.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Intermediates { get; set; }

        /// <summary>Create CSR or import existing SSL Certificate.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Certificate '{Name}'" : "Certificate"))
            {
                return;
            }
            Models.Certificate result = Run(connection, token => CertificateBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(CertificateDomain)))
            {
                parameters["certificate_domain"] = CertificateDomain;
            }
            if (IsBound(nameof(CertificateExtraDomains)))
            {
                parameters["certificate_extra_domains"] = RequestValues.Strings(CertificateExtraDomains, "CertificateExtraDomains");
            }
            if (IsBound(nameof(CertificateCountry)))
            {
                parameters["certificate_country"] = CertificateCountry;
            }
            if (IsBound(nameof(CertificateStateOrProvince)))
            {
                parameters["certificate_state_or_province"] = CertificateStateOrProvince;
            }
            if (IsBound(nameof(CertificateCityOrLocale)))
            {
                parameters["certificate_city_or_locale"] = CertificateCityOrLocale;
            }
            if (IsBound(nameof(CertificateCompanyName)))
            {
                parameters["certificate_company_name"] = CertificateCompanyName;
            }
            if (IsBound(nameof(CsrOu1)))
            {
                parameters["csr_ou1"] = CsrOu1;
            }
            if (IsBound(nameof(CsrOu2)))
            {
                parameters["csr_ou2"] = CsrOu2;
            }
            if (IsBound(nameof(CsrOu3)))
            {
                parameters["csr_ou3"] = CsrOu3;
            }
            if (IsBound(nameof(CertificateEmailAddress)))
            {
                parameters["certificate_email_address"] = CertificateEmailAddress;
            }
            if (IsBound(nameof(KeyType)))
            {
                parameters["key_type"] = KeyType;
            }
            if (IsBound(nameof(Certificate)))
            {
                parameters["certificate"] = Certificate;
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            if (IsBound(nameof(Password)))
            {
                parameters["password"] = RequestValues.Reveal(Password);
            }
            if (IsBound(nameof(Intermediates)))
            {
                parameters["intermediates"] = Intermediates;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Activate SSL Certificate.
    /// </summary>
    [Cmdlet("Enable", "FilesCertificate", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class EnableFilesCertificateCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Certificate ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Leave blank or set to `any` to replace any currently active certificate, if applicable.  Set to `new_ips` to activate this certificate as an additional certificate on your Site by allocating a new set of Dedicated IPs (may require a Plan upgrade).  Set to the ID of a currently active certificate to replace that certificate on its set of dedicated IPs.  A certificate belonging to a Custom Domain activates only for that domain: leave blank, use `any`, or specify that domain's current certificate ID.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ReplaceCert { get; set; }

        /// <summary>
        /// The certificate to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Certificate InputObject { get; set; }

        /// <summary>Activate SSL Certificate.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ActivateParameters();
            if (!ShouldProcessOn(connection, $"Certificate {id}"))
            {
                return;
            }
            Run(connection, token => CertificateBridge.ActivateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> ActivateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(ReplaceCert)))
            {
                parameters["replace_cert"] = ReplaceCert;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Deactivate SSL Certificate.
    /// </summary>
    [Cmdlet("Disable", "FilesCertificate", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class DisableFilesCertificateCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Certificate ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The certificate to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Certificate InputObject { get; set; }

        /// <summary>Deactivate SSL Certificate.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeactivateParameters();
            if (!ShouldProcessOn(connection, $"Certificate {id}"))
            {
                return;
            }
            Run(connection, token => CertificateBridge.DeactivateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeactivateParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Replace this active custom SSL Certificate for the Primary Custom Domain with a Files.com SSL Certificate.  The Primary Custom Domain's CNAME record must point at the Site's subdomain.  The Files.com certificate is issued and activated in the background.
    /// </summary>
    [Cmdlet("Invoke", "FilesCertificateReplaceWithFiles", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class InvokeFilesCertificateReplaceWithFilesCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Certificate ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The certificate to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Certificate InputObject { get; set; }

        /// <summary>Replace this active custom SSL Certificate for the Primary Custom Domain with a Files.com SSL Certificate.  The Primary Custom Domain's CNAME record must point at the Site's subdomain.  The Files.com certificate is issued and activated in the background.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ReplaceWithFilesParameters();
            if (!ShouldProcessOn(connection, $"Certificate {id}"))
            {
                return;
            }
            Run(connection, token => CertificateBridge.ReplaceWithFilesAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> ReplaceWithFilesParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update Certificate.
    /// </summary>
    [Cmdlet("Set", "FilesCertificate", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Certificate), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesCertificateCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Certificate ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Internal certificate name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Any intermediate certificates.  PEM or PKCS7 formats accepted.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Intermediates { get; set; }

        /// <summary>
        /// The certificate.  PEM or PKCS7 formats accepted.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Certificate { get; set; }

        /// <summary>
        /// The certificate to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Certificate InputObject { get; set; }

        /// <summary>Update Certificate.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Certificate {id}"))
            {
                return;
            }
            Models.Certificate result = Run(connection, token => CertificateBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Intermediates)))
            {
                parameters["intermediates"] = Intermediates;
            }
            if (IsBound(nameof(Certificate)))
            {
                parameters["certificate"] = Certificate;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Certificate.
    /// </summary>
    [Cmdlet("Remove", "FilesCertificate", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesCertificateCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Certificate ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The certificate to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Certificate InputObject { get; set; }

        /// <summary>Delete Certificate.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Certificate {id}"))
            {
                return;
            }
            Run(connection, token => CertificateBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
