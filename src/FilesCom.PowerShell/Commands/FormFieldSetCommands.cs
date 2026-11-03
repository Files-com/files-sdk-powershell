using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Form Field Sets, or get one form field set by Id.
    /// </summary>
    [Cmdlet("Get", "FilesFormFieldSet", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.FormFieldSet), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.FormFieldSet>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.FormFieldSet), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesFormFieldSetCommand : FilesListCmdlet<Models.FormFieldSet>
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
        /// Form Field Set ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Form Field Sets, or get one form field set by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.FormFieldSet result = Run(connection, token => FormFieldSetBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => FormFieldSetBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Form Field Set.
    /// </summary>
    [Cmdlet("New", "FilesFormFieldSet", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FormFieldSet))]
    public sealed class NewFilesFormFieldSetCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Title to be displayed
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Title { get; set; }

        /// <summary>
        /// Reject answers over 1024 characters or more than 32 answered custom fields to ensure complete file metadata capture
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RequireCompleteMetadataCapture { get; set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Skip validating form email
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SkipEmail { get; set; }

        /// <summary>
        /// Skip validating form name
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SkipName { get; set; }

        /// <summary>
        /// Skip validating company
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SkipCompany { get; set; }

        /// <summary>
        /// form_fields
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary[] FormFields { get; set; }

        /// <summary>Create Form Field Set.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Title)) ? $"Form field set '{Title}'" : "Form field set"))
            {
                return;
            }
            Models.FormFieldSet result = Run(connection, token => FormFieldSetBridge.CreateAsync(connection, parameters, CallOptions, token));
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
            if (IsBound(nameof(RequireCompleteMetadataCapture)))
            {
                parameters["require_complete_metadata_capture"] = RequireCompleteMetadataCapture.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(SkipEmail)))
            {
                parameters["skip_email"] = SkipEmail.Value;
            }
            if (IsBound(nameof(SkipName)))
            {
                parameters["skip_name"] = SkipName.Value;
            }
            if (IsBound(nameof(SkipCompany)))
            {
                parameters["skip_company"] = SkipCompany.Value;
            }
            if (IsBound(nameof(FormFields)))
            {
                parameters["form_fields"] = RequestValues.Objects(FormFields, "FormFields");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Form Field Set.
    /// </summary>
    [Cmdlet("Set", "FilesFormFieldSet", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FormFieldSet), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesFormFieldSetCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Form Field Set ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Title to be displayed
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Title { get; set; }

        /// <summary>
        /// Reject answers over 1024 characters or more than 32 answered custom fields to ensure complete file metadata capture
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? RequireCompleteMetadataCapture { get; set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Skip validating form email
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SkipEmail { get; set; }

        /// <summary>
        /// Skip validating form name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SkipName { get; set; }

        /// <summary>
        /// Skip validating company
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SkipCompany { get; set; }

        /// <summary>
        /// form_fields
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary[] FormFields { get; set; }

        /// <summary>
        /// The form field set to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.FormFieldSet InputObject { get; set; }

        /// <summary>Update Form Field Set.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Form field set {id}"))
            {
                return;
            }
            Models.FormFieldSet result = Run(connection, token => FormFieldSetBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(RequireCompleteMetadataCapture)))
            {
                parameters["require_complete_metadata_capture"] = RequireCompleteMetadataCapture.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(SkipEmail)))
            {
                parameters["skip_email"] = SkipEmail.Value;
            }
            if (IsBound(nameof(SkipName)))
            {
                parameters["skip_name"] = SkipName.Value;
            }
            if (IsBound(nameof(SkipCompany)))
            {
                parameters["skip_company"] = SkipCompany.Value;
            }
            if (IsBound(nameof(FormFields)))
            {
                parameters["form_fields"] = RequestValues.Objects(FormFields, "FormFields");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Form Field Set.
    /// </summary>
    [Cmdlet("Remove", "FilesFormFieldSet", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesFormFieldSetCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Form Field Set ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The form field set to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.FormFieldSet InputObject { get; set; }

        /// <summary>Delete Form Field Set.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Form field set {id}"))
            {
                return;
            }
            Run(connection, token => FormFieldSetBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
