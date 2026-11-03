using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Public Keys, or get one public key by Id.
    /// </summary>
    [Cmdlet("Get", "FilesPublicKey", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.PublicKey), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.PublicKey>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.PublicKey), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesPublicKeyCommand : FilesListCmdlet<Models.PublicKey>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `user_id`, `title` or `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `created_at` and `workspace_id`. Valid field combinations are `[ workspace_id, created_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than the supplied value. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGteq { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than the supplied value. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than or equal the supplied value. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLteq { get; set; }

        /// <summary>
        /// Public Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Public Keys, or get one public key by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.PublicKey result = Run(connection, token => PublicKeyBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => PublicKeyBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create Public Key.
    /// </summary>
    [Cmdlet("New", "FilesPublicKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.PublicKey))]
    public sealed class NewFilesPublicKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Internal reference for key.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Title { get; set; }

        /// <summary>
        /// Actual contents of SSH key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PublicKey { get; set; }

        /// <summary>
        /// If true, generate a new SSH key pair. Can not be used with `public_key`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? GenerateKeypair { get; set; }

        /// <summary>
        /// Password for the private key. Used for the generation of the key. Will be ignored if `generate_keypair` is false.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString GeneratePrivateKeyPassword { get; set; }

        /// <summary>
        /// Type of key to generate.  One of rsa, dsa, ecdsa, ed25519. Used for the generation of the key. Will be ignored if `generate_keypair` is false.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string GenerateAlgorithm { get; set; }

        /// <summary>
        /// Length of key to generate. If algorithm is ecdsa, this is the signature size. Used for the generation of the key. Will be ignored if `generate_keypair` is false.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? GenerateLength { get; set; }

        /// <summary>Create Public Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Title)) ? $"Public key '{Title}'" : "Public key"))
            {
                return;
            }
            Models.PublicKey result = Run(connection, token => PublicKeyBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Title)))
            {
                parameters["title"] = Title;
            }
            if (IsBound(nameof(PublicKey)))
            {
                parameters["public_key"] = PublicKey;
            }
            if (IsBound(nameof(GenerateKeypair)))
            {
                parameters["generate_keypair"] = GenerateKeypair.Value;
            }
            if (IsBound(nameof(GeneratePrivateKeyPassword)))
            {
                parameters["generate_private_key_password"] = RequestValues.Reveal(GeneratePrivateKeyPassword);
            }
            if (IsBound(nameof(GenerateAlgorithm)))
            {
                parameters["generate_algorithm"] = GenerateAlgorithm;
            }
            if (IsBound(nameof(GenerateLength)))
            {
                parameters["generate_length"] = GenerateLength.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Public Key.
    /// </summary>
    [Cmdlet("Set", "FilesPublicKey", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.PublicKey), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesPublicKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Public Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Internal reference for key.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Id")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Title { get; set; }

        /// <summary>
        /// The public key to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.PublicKey InputObject { get; set; }

        /// <summary>Update Public Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Public key {id}"))
            {
                return;
            }
            Models.PublicKey result = Run(connection, token => PublicKeyBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Title)))
            {
                parameters["title"] = Title;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Public Key.
    /// </summary>
    [Cmdlet("Remove", "FilesPublicKey", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesPublicKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Public Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The public key to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.PublicKey InputObject { get; set; }

        /// <summary>Delete Public Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Public key {id}"))
            {
                return;
            }
            Run(connection, token => PublicKeyBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
