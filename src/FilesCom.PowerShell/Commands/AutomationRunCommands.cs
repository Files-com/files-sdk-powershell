using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Automation Runs, or get one automation run by Id.
    /// </summary>
    [Cmdlet("Get", "FilesAutomationRun", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.AutomationRun), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.AutomationRun>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.AutomationRun), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesAutomationRunCommand : FilesListCmdlet<Models.AutomationRun>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `automation_id`, `created_at` or `status`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `status`, `workspace_id` or `automation_id`. Valid field combinations are `[ workspace_id, status ]`, `[ automation_id, status ]`, `[ workspace_id, automation_id ]` or `[ workspace_id, automation_id, status ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// ID of the associated Automation.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "List")]
        [Parameter(Mandatory = true, ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long AutomationId { get; set; }

        /// <summary>
        /// Automation Run ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Automation Runs, or get one automation run by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.AutomationRun result = Run(connection, token => AutomationRunBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => AutomationRunBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(AutomationId)))
            {
                parameters["automation_id"] = AutomationId;
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
    /// Show Automation Run Node.
    /// </summary>
    [Cmdlet("Get", "FilesAutomationRunNode")]
    [OutputType(typeof(Models.AutomationExecutionNode))]
    public sealed class GetFilesAutomationRunNodeCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Automation Run ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Node ID from the pinned Automation definition.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string NodeId { get; set; }

        /// <summary>Show Automation Run Node.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            long id = Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = FindNodeParameters();
            Models.AutomationExecutionNode result = Run(connection, token => AutomationRunBridge.FindNodeAsync(connection, id, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> FindNodeParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(NodeId)))
            {
                parameters["node_id"] = NodeId;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Cancel Automation Run.
    /// </summary>
    [Cmdlet("Stop", "FilesAutomationRun", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.AutomationRun), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class StopFilesAutomationRunCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Automation Run ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The automation run to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.AutomationRun InputObject { get; set; }

        /// <summary>Cancel Automation Run.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = CancelParameters();
            if (!ShouldProcessOn(connection, $"Automation run {id}"))
            {
                return;
            }
            Models.AutomationRun result = Run(connection, token => AutomationRunBridge.CancelAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> CancelParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Re-run Automation from Node.
    /// </summary>
    [Cmdlet("Restart", "FilesAutomationRun", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.AutomationRun), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class RestartFilesAutomationRunCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Automation Run ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Node ID at which execution should resume.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Id")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string NodeId { get; set; }

        /// <summary>
        /// The automation run to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.AutomationRun InputObject { get; set; }

        /// <summary>Re-run Automation from Node.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = RerunParameters();
            if (!ShouldProcessOn(connection, $"Automation run {id}"))
            {
                return;
            }
            Models.AutomationRun result = Run(connection, token => AutomationRunBridge.RerunAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> RerunParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(NodeId)))
            {
                parameters["node_id"] = NodeId;
            }
            return parameters;
        }
    }
}
