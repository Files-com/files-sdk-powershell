using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Behaviors, or get one behavior by Id.
    /// </summary>
    [Cmdlet("Get", "FilesBehavior", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Behavior), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Behavior>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Behavior), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesBehaviorCommand : FilesListCmdlet<Models.Behavior>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `behavior`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `clickwrap_id`, `form_field_set_id`, `impacts_ui`, `remote_server_id` or `behavior`. Valid field combinations are `[ impacts_ui, behavior ]` and `[ behavior, remote_server_id ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Behavior ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Behaviors, or get one behavior by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Behavior result = Run(connection, token => BehaviorBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => BehaviorBridge.List(connection, ListParameters(), CallOptions));
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
    /// List Behaviors by Path.
    /// </summary>
    [Cmdlet("Get", "FilesFolderBehavior", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Behavior), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Behavior>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesFolderBehaviorCommand : FilesListCmdlet<Models.Behavior>
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `behavior`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `impacts_ui` and `behavior`. Valid field combinations are `[ impacts_ui, behavior ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If `true`, behaviors above this path are shown.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? AncestorBehaviors { get; set; }

        /// <summary>List Behaviors by Path.</summary>
        protected override void Execute()
        {
            WriteList(connection => BehaviorBridge.ListFor(connection, Path, ListForParameters(), CallOptions));
        }

        private Dictionary<string, object> ListForParameters()
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
            if (IsBound(nameof(AncestorBehaviors)))
            {
                parameters["ancestor_behaviors"] = AncestorBehaviors.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Behavior.
    /// </summary>
    [Cmdlet("New", "FilesBehavior", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Behavior))]
    public sealed class NewFilesBehaviorCommand : FilesApiCmdlet
    {
        /// <summary>
        /// This field stores data specific to the type of behavior. See The Behavior Types section for the accepted value for each type of behavior.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Value { get; set; }

        /// <summary>
        /// Certain behaviors may require a file, for instance, the `watermark` behavior requires a watermark image. Attach that file here.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment AttachmentFile { get; set; }

        /// <summary>
        /// If true, disables the inherited behavior for this folder and its children. Valid only for behavior types that child folders may override, and requires recursive to be true. Rejected for all other behavior types.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DisableParentFolderBehavior { get; set; }

        /// <summary>
        /// Whether the behavior should apply to child folders. This is only configurable for behavior types whose recursion mode is `sometimes`; `always` behaviors stay recursive and `never` behaviors stay non-recursive.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Recursive { get; set; }

        /// <summary>
        /// Name for this behavior.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Description for this behavior.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Path where this behavior should apply.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Behavior type.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Behavior { get; set; }

        /// <summary>Create Behavior.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Behavior '{Path}'" : "Behavior"))
            {
                return;
            }
            Models.Behavior result = Run(connection, token => BehaviorBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Value)))
            {
                parameters["value"] = RequestValues.Object(Value, "Value");
            }
            if (IsBound(nameof(AttachmentFile)))
            {
                parameters["attachment_file"] = RequestValues.Attachment(AttachmentFile, "AttachmentFile");
            }
            if (IsBound(nameof(DisableParentFolderBehavior)))
            {
                parameters["disable_parent_folder_behavior"] = DisableParentFolderBehavior.Value;
            }
            if (IsBound(nameof(Recursive)))
            {
                parameters["recursive"] = Recursive.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(Behavior)))
            {
                parameters["behavior"] = Behavior;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Test Webhook.
    /// </summary>
    [Cmdlet("Test", "FilesBehaviorWebhook", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class TestFilesBehaviorWebhookCommand : FilesApiCmdlet
    {
        /// <summary>
        /// URL for testing the webhook.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Url { get; set; }

        /// <summary>
        /// HTTP request method (GET or POST).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Method { get; set; }

        /// <summary>
        /// Encoding type for the webhook payload. Can be JSON, XML, or RAW (form data).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Encoding { get; set; }

        /// <summary>
        /// Additional request headers to send via HTTP.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Headers { get; set; }

        /// <summary>
        /// Additional body parameters to include in the webhook payload.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Body { get; set; }

        /// <summary>
        /// Action for test body.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Action { get; set; }

        /// <summary>Test Webhook.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = WebhookTestParameters();
            if (!ShouldProcessOn(connection, "Behavior"))
            {
                return;
            }
            Run(connection, token => BehaviorBridge.WebhookTestAsync(connection, parameters, CallOptions, token));
        }

        private Dictionary<string, object> WebhookTestParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Url)))
            {
                parameters["url"] = Url;
            }
            if (IsBound(nameof(Method)))
            {
                parameters["method"] = Method;
            }
            if (IsBound(nameof(Encoding)))
            {
                parameters["encoding"] = Encoding;
            }
            if (IsBound(nameof(Headers)))
            {
                parameters["headers"] = RequestValues.Object(Headers, "Headers");
            }
            if (IsBound(nameof(Body)))
            {
                parameters["body"] = RequestValues.Object(Body, "Body");
            }
            if (IsBound(nameof(Action)))
            {
                parameters["action"] = Action;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Behavior.
    /// </summary>
    [Cmdlet("Set", "FilesBehavior", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Behavior), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesBehaviorCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Behavior ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// This field stores data specific to the type of behavior. See The Behavior Types section for the accepted value for each type of behavior.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Value { get; set; }

        /// <summary>
        /// Certain behaviors may require a file, for instance, the `watermark` behavior requires a watermark image. Attach that file here.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public FilesAttachment AttachmentFile { get; set; }

        /// <summary>
        /// If true, disables the inherited behavior for this folder and its children. Valid only for behavior types that child folders may override, and requires recursive to be true. Rejected for all other behavior types.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DisableParentFolderBehavior { get; set; }

        /// <summary>
        /// Whether the behavior should apply to child folders. This is only configurable for behavior types whose recursion mode is `sometimes`; `always` behaviors stay recursive and `never` behaviors stay non-recursive.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Recursive { get; set; }

        /// <summary>
        /// Name for this behavior.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Description for this behavior.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// If `true`, delete the file stored in `attachment`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AttachmentDelete { get; set; }

        /// <summary>
        /// The behavior to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Behavior InputObject { get; set; }

        /// <summary>Update Behavior.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Behavior {id}"))
            {
                return;
            }
            Models.Behavior result = Run(connection, token => BehaviorBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Value)))
            {
                parameters["value"] = RequestValues.Object(Value, "Value");
            }
            if (IsBound(nameof(AttachmentFile)))
            {
                parameters["attachment_file"] = RequestValues.Attachment(AttachmentFile, "AttachmentFile");
            }
            if (IsBound(nameof(DisableParentFolderBehavior)))
            {
                parameters["disable_parent_folder_behavior"] = DisableParentFolderBehavior.Value;
            }
            if (IsBound(nameof(Recursive)))
            {
                parameters["recursive"] = Recursive.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(AttachmentDelete)))
            {
                parameters["attachment_delete"] = AttachmentDelete.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Behavior.
    /// </summary>
    [Cmdlet("Remove", "FilesBehavior", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesBehaviorCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Behavior ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The behavior to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Behavior InputObject { get; set; }

        /// <summary>Delete Behavior.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Behavior {id}"))
            {
                return;
            }
            Run(connection, token => BehaviorBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
