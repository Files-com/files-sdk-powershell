using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List GPG Keys, or get one gpg key by Id.
    /// </summary>
    [Cmdlet("Get", "FilesGpgKey", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.GpgKey), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.GpgKey>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.GpgKey), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesGpgKeyCommand : FilesListCmdlet<Models.GpgKey>
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `name` or `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id`, `partner_id` or `expires_at`. Valid field combinations are `[ workspace_id, partner_id ]`, `[ workspace_id, expires_at ]`, `[ partner_id, expires_at ]` or `[ workspace_id, partner_id, expires_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than the supplied value. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGteq { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than the supplied value. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than or equal the supplied value. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLteq { get; set; }

        /// <summary>
        /// Gpg Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List GPG Keys, or get one gpg key by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.GpgKey result = Run(connection, token => GpgKeyBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => GpgKeyBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
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
            if (IsBound(nameof(FilterGt)))
            {
                parameters["filter_gt"] = RequestValues.QueryObject(FilterGt, "FilterGt");
            }
            if (IsBound(nameof(FilterGteq)))
            {
                parameters["filter_gteq"] = RequestValues.QueryObject(FilterGteq, "FilterGteq");
            }
            if (IsBound(nameof(FilterLt)))
            {
                parameters["filter_lt"] = RequestValues.QueryObject(FilterLt, "FilterLt");
            }
            if (IsBound(nameof(FilterLteq)))
            {
                parameters["filter_lteq"] = RequestValues.QueryObject(FilterLteq, "FilterLteq");
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
    /// Create GPG Key.
    /// </summary>
    [Cmdlet("New", "FilesGpgKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.GpgKey))]
    public sealed class NewFilesGpgKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Partner ID who owns this GPG Key, if applicable.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PartnerId { get; set; }

        /// <summary>
        /// The GPG public key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PublicKey { get; set; }

        /// <summary>
        /// The GPG private key
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>
        /// The GPG private key password
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKeyPassword { get; set; }

        /// <summary>
        /// GPG key name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Workspace ID (0 for default workspace).
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Expiration date of the key. Used for the generation of the key. Will be ignored if `generate_keypair` is false.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? GenerateExpiresAt { get; set; }

        /// <summary>
        /// If true, generate a new GPG key pair. Can not be used with `public_key`/`private_key`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GenerateKeypair { get; set; }

        /// <summary>
        /// Full name of the key owner. Used for the generation of the key. Will be ignored if `generate_keypair` is false.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GenerateFullName { get; set; }

        /// <summary>
        /// Email address of the key owner. Used for the generation of the key. Will be ignored if `generate_keypair` is false.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GenerateEmail { get; set; }

        /// <summary>Create GPG Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Gpg key '{Name}'" : "Gpg key"))
            {
                return;
            }
            Models.GpgKey result = Run(connection, token => GpgKeyBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(PartnerId)))
            {
                parameters["partner_id"] = PartnerId.Value;
            }
            if (IsBound(nameof(PublicKey)))
            {
                parameters["public_key"] = PublicKey;
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            if (IsBound(nameof(PrivateKeyPassword)))
            {
                parameters["private_key_password"] = RequestValues.Reveal(PrivateKeyPassword);
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(GenerateExpiresAt)))
            {
                parameters["generate_expires_at"] = RequestValues.DateTime(GenerateExpiresAt.Value);
            }
            if (IsBound(nameof(GenerateKeypair)))
            {
                parameters["generate_keypair"] = GenerateKeypair.Value;
            }
            if (IsBound(nameof(GenerateFullName)))
            {
                parameters["generate_full_name"] = GenerateFullName;
            }
            if (IsBound(nameof(GenerateEmail)))
            {
                parameters["generate_email"] = GenerateEmail;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update GPG Key.
    /// </summary>
    [Cmdlet("Set", "FilesGpgKey", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.GpgKey), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesGpgKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Gpg Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Partner ID who owns this GPG Key, if applicable.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PartnerId { get; set; }

        /// <summary>
        /// The GPG public key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PublicKey { get; set; }

        /// <summary>
        /// The GPG private key
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>
        /// The GPG private key password
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKeyPassword { get; set; }

        /// <summary>
        /// GPG key name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// The gpg key to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.GpgKey InputObject { get; set; }

        /// <summary>Update GPG Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Gpg key {id}"))
            {
                return;
            }
            Models.GpgKey result = Run(connection, token => GpgKeyBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(PartnerId)))
            {
                parameters["partner_id"] = PartnerId.Value;
            }
            if (IsBound(nameof(PublicKey)))
            {
                parameters["public_key"] = PublicKey;
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            if (IsBound(nameof(PrivateKeyPassword)))
            {
                parameters["private_key_password"] = RequestValues.Reveal(PrivateKeyPassword);
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete GPG Key.
    /// </summary>
    [Cmdlet("Remove", "FilesGpgKey", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesGpgKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Gpg Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The gpg key to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.GpgKey InputObject { get; set; }

        /// <summary>Delete GPG Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Gpg key {id}"))
            {
                return;
            }
            Run(connection, token => GpgKeyBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
