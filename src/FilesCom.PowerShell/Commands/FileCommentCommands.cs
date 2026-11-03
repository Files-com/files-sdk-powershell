using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List File Comments by Path.
    /// </summary>
    [Cmdlet("Get", "FilesFileCommentForPath", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.FileComment), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.FileComment>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesFileCommentForPathCommand : FilesListCmdlet<Models.FileComment>
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
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>List File Comments by Path.</summary>
        protected override void Execute()
        {
            WriteList(connection => FileCommentBridge.ListFor(connection, Path, ListForParameters(), CallOptions));
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
            return parameters;
        }
    }

    /// <summary>
    /// Create File Comment.
    /// </summary>
    [Cmdlet("New", "FilesFileComment", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileComment))]
    public sealed class NewFilesFileCommentCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Comment body.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Body { get; set; }

        /// <summary>
        /// File path.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>Create File Comment.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"File comment '{Path}'" : "File comment"))
            {
                return;
            }
            Models.FileComment result = Run(connection, token => FileCommentBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Body)))
            {
                parameters["body"] = Body;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update File Comment.
    /// </summary>
    [Cmdlet("Set", "FilesFileComment", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileComment), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesFileCommentCommand : FilesApiCmdlet
    {
        /// <summary>
        /// File Comment ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Comment body.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Id")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Body { get; set; }

        /// <summary>
        /// The file comment to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.FileComment InputObject { get; set; }

        /// <summary>Update File Comment.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"File comment {id}"))
            {
                return;
            }
            Models.FileComment result = Run(connection, token => FileCommentBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Body)))
            {
                parameters["body"] = Body;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete File Comment.
    /// </summary>
    [Cmdlet("Remove", "FilesFileComment", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesFileCommentCommand : FilesApiCmdlet
    {
        /// <summary>
        /// File Comment ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The file comment to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.FileComment InputObject { get; set; }

        /// <summary>Delete File Comment.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"File comment {id}"))
            {
                return;
            }
            Run(connection, token => FileCommentBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
