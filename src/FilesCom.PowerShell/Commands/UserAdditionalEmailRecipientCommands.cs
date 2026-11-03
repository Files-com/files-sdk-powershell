using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List User Additional Email Recipients, or get one user additional email recipient by Id.
    /// </summary>
    [Cmdlet("Get", "FilesUserAdditionalEmailRecipient", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.UserAdditionalEmailRecipient), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.UserAdditionalEmailRecipient>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.UserAdditionalEmailRecipient), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesUserAdditionalEmailRecipientCommand : FilesListCmdlet<Models.UserAdditionalEmailRecipient>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `email`, `user_id` or `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `email`, `workspace_id` or `user_id`. Valid field combinations are `[ workspace_id, email ]`, `[ user_id, email ]`, `[ workspace_id, user_id ]` or `[ workspace_id, user_id, email ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `email`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// User Additional Email Recipient ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List User Additional Email Recipients, or get one user additional email recipient by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.UserAdditionalEmailRecipient result = Run(connection, token => UserAdditionalEmailRecipientBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => UserAdditionalEmailRecipientBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(FilterPrefix)))
            {
                parameters["filter_prefix"] = RequestValues.QueryObject(FilterPrefix, "FilterPrefix");
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
    /// Create User Additional Email Recipient.
    /// </summary>
    [Cmdlet("New", "FilesUserAdditionalEmailRecipient", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.UserAdditionalEmailRecipient))]
    public sealed class NewFilesUserAdditionalEmailRecipientCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Additional email recipient address
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Email { get; set; }

        /// <summary>Create User Additional Email Recipient.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Email)) ? $"User additional email recipient '{Email}'" : "User additional email recipient"))
            {
                return;
            }
            Models.UserAdditionalEmailRecipient result = Run(connection, token => UserAdditionalEmailRecipientBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Email)))
            {
                parameters["email"] = Email;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update User Additional Email Recipient.
    /// </summary>
    [Cmdlet("Set", "FilesUserAdditionalEmailRecipient", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.UserAdditionalEmailRecipient), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesUserAdditionalEmailRecipientCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User Additional Email Recipient ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Additional email recipient address
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Email { get; set; }

        /// <summary>
        /// The user additional email recipient to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.UserAdditionalEmailRecipient InputObject { get; set; }

        /// <summary>Update User Additional Email Recipient.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"User additional email recipient {id}"))
            {
                return;
            }
            Models.UserAdditionalEmailRecipient result = Run(connection, token => UserAdditionalEmailRecipientBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Email)))
            {
                parameters["email"] = Email;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete User Additional Email Recipient.
    /// </summary>
    [Cmdlet("Remove", "FilesUserAdditionalEmailRecipient", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesUserAdditionalEmailRecipientCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User Additional Email Recipient ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The user additional email recipient to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.UserAdditionalEmailRecipient InputObject { get; set; }

        /// <summary>Delete User Additional Email Recipient.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"User additional email recipient {id}"))
            {
                return;
            }
            Run(connection, token => UserAdditionalEmailRecipientBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
