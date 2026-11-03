using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List AS2 Stations, or get one as2 station by Id.
    /// </summary>
    [Cmdlet("Get", "FilesAs2Station", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.As2Station), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.As2Station>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.As2Station), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesAs2StationCommand : FilesListCmdlet<Models.As2Station>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id` and `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// As2 Station ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List AS2 Stations, or get one as2 station by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.As2Station result = Run(connection, token => As2StationBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => As2StationBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create AS2 Station.
    /// </summary>
    [Cmdlet("New", "FilesAs2Station", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.As2Station))]
    public sealed class NewFilesAs2StationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// The station's formal AS2 name, unique across all Workspaces on the Site.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// ID of the Workspace associated with this AS2 Station.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Public certificate used for message security.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PublicCertificate { get; set; }

        /// <summary>
        /// PEM-encoded private key matching public_certificate.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>
        /// Password for the PEM-encoded private key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKeyPassword { get; set; }

        /// <summary>
        /// Base64-encoded PKCS#12 (.pfx or .p12) file containing the identity's certificate and private key. Provide this instead of public_certificate and private_key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Pkcs12 { get; set; }

        /// <summary>
        /// Password for pkcs12. The file and password are used only for import; the extracted certificate and private key are stored as PEM.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Pkcs12Password { get; set; }

        /// <summary>Create AS2 Station.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"As2 station '{Name}'" : "As2 station"))
            {
                return;
            }
            Models.As2Station result = Run(connection, token => As2StationBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(PublicCertificate)))
            {
                parameters["public_certificate"] = PublicCertificate;
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            if (IsBound(nameof(PrivateKeyPassword)))
            {
                parameters["private_key_password"] = RequestValues.Reveal(PrivateKeyPassword);
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
    /// Update AS2 Station.
    /// </summary>
    [Cmdlet("Set", "FilesAs2Station", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.As2Station), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesAs2StationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// As2 Station ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The station's formal AS2 name, unique across all Workspaces on the Site.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Public certificate used for message security.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PublicCertificate { get; set; }

        /// <summary>
        /// PEM-encoded private key matching public_certificate.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>
        /// Password for the PEM-encoded private key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKeyPassword { get; set; }

        /// <summary>
        /// Base64-encoded PKCS#12 (.pfx or .p12) file containing the identity's replacement certificate and private key. Provide this instead of public_certificate and private_key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString Pkcs12 { get; set; }

        /// <summary>
        /// Password for pkcs12. The file and password are used only for import; the extracted certificate and private key are stored as PEM.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString Pkcs12Password { get; set; }

        /// <summary>
        /// The as2 station to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.As2Station InputObject { get; set; }

        /// <summary>Update AS2 Station.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"As2 station {id}"))
            {
                return;
            }
            Models.As2Station result = Run(connection, token => As2StationBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(PublicCertificate)))
            {
                parameters["public_certificate"] = PublicCertificate;
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            if (IsBound(nameof(PrivateKeyPassword)))
            {
                parameters["private_key_password"] = RequestValues.Reveal(PrivateKeyPassword);
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
    /// Delete AS2 Station.
    /// </summary>
    [Cmdlet("Remove", "FilesAs2Station", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesAs2StationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// As2 Station ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The as2 station to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.As2Station InputObject { get; set; }

        /// <summary>Delete AS2 Station.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"As2 station {id}"))
            {
                return;
            }
            Run(connection, token => As2StationBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
