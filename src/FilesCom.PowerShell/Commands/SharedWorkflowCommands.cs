using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Shared Workflows, or get one shared workflow by Id.
    /// </summary>
    [Cmdlet("Get", "FilesSharedWorkflow", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.SharedWorkflow), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.SharedWorkflow>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.SharedWorkflow), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesSharedWorkflowCommand : FilesListCmdlet<Models.SharedWorkflow>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are .
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// Shared Workflow ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Shared Workflows, or get one shared workflow by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.SharedWorkflow result = Run(connection, token => SharedWorkflowBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => SharedWorkflowBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Run a Shared Workflow.
    /// </summary>
    [Cmdlet("Start", "FilesSharedWorkflow", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.AutomationRun), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class StartFilesSharedWorkflowCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Shared Workflow ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Revision returned when opening the Shared Workflow.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Id")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string SubmissionRevision { get; set; }

        /// <summary>
        /// Client-generated UUID used to retry the same submission without starting another run.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Id")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string SubmissionId { get; set; }

        /// <summary>
        /// Path of a file the recipient can read, including a file uploaded to a Snapshot created for this workflow.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Id")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string FilePath { get; set; }

        /// <summary>
        /// The shared workflow to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.SharedWorkflow InputObject { get; set; }

        /// <summary>Run a Shared Workflow.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = RunWorkflowParameters();
            if (!ShouldProcessOn(connection, $"Shared workflow {id}"))
            {
                return;
            }
            Models.AutomationRun result = Run(connection, token => SharedWorkflowBridge.RunWorkflowAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> RunWorkflowParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(SubmissionRevision)))
            {
                parameters["submission_revision"] = SubmissionRevision;
            }
            if (IsBound(nameof(SubmissionId)))
            {
                parameters["submission_id"] = SubmissionId;
            }
            if (IsBound(nameof(FilePath)))
            {
                parameters["file_path"] = FilePath;
            }
            return parameters;
        }
    }
}
