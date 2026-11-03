using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List SFTP Host Keys, or get one sftp host key by Id.
    /// </summary>
    [Cmdlet("Get", "FilesSftpHostKey", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.SftpHostKey), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.SftpHostKey>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.SftpHostKey), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesSftpHostKeyCommand : FilesListCmdlet<Models.SftpHostKey>
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
        /// Sftp Host Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List SFTP Host Keys, or get one sftp host key by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.SftpHostKey result = Run(connection, token => SftpHostKeyBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => SftpHostKeyBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create SFTP Host Key.
    /// </summary>
    [Cmdlet("New", "FilesSftpHostKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.SftpHostKey))]
    public sealed class NewFilesSftpHostKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// If true, use this SFTP Host Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Active { get; set; }

        /// <summary>
        /// Custom Domain ID. If set, this key is used only for that Custom Domain.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? CustomDomainId { get; set; }

        /// <summary>
        /// The friendly name of this SFTP Host Key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// The private key data.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>Create SFTP Host Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Sftp host key '{Name}'" : "Sftp host key"))
            {
                return;
            }
            Models.SftpHostKey result = Run(connection, token => SftpHostKeyBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Active)))
            {
                parameters["active"] = Active.Value;
            }
            if (IsBound(nameof(CustomDomainId)))
            {
                parameters["custom_domain_id"] = CustomDomainId.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update SFTP Host Key.
    /// </summary>
    [Cmdlet("Set", "FilesSftpHostKey", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.SftpHostKey), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesSftpHostKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Sftp Host Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// If true, use this SFTP Host Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Active { get; set; }

        /// <summary>
        /// Custom Domain ID. If set, this key is used only for that Custom Domain.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? CustomDomainId { get; set; }

        /// <summary>
        /// The friendly name of this SFTP Host Key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// The private key data.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString PrivateKey { get; set; }

        /// <summary>
        /// The sftp host key to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.SftpHostKey InputObject { get; set; }

        /// <summary>Update SFTP Host Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Sftp host key {id}"))
            {
                return;
            }
            Models.SftpHostKey result = Run(connection, token => SftpHostKeyBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Active)))
            {
                parameters["active"] = Active.Value;
            }
            if (IsBound(nameof(CustomDomainId)))
            {
                parameters["custom_domain_id"] = CustomDomainId.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(PrivateKey)))
            {
                parameters["private_key"] = RequestValues.Reveal(PrivateKey);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete SFTP Host Key.
    /// </summary>
    [Cmdlet("Remove", "FilesSftpHostKey", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesSftpHostKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Sftp Host Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The sftp host key to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.SftpHostKey InputObject { get; set; }

        /// <summary>Delete SFTP Host Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Sftp host key {id}"))
            {
                return;
            }
            Run(connection, token => SftpHostKeyBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
