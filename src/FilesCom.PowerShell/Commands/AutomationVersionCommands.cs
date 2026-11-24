using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Automation Versions, or get one automation version by Id.
    /// </summary>
    [Cmdlet("Get", "FilesAutomationVersion", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.AutomationVersion), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.AutomationVersion>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.AutomationVersion), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesAutomationVersionCommand : FilesListCmdlet<Models.AutomationVersion>
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
        /// ID of the Automation that owns this revision.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "List")]
        [Parameter(Mandatory = true, ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long AutomationId { get; set; }

        /// <summary>
        /// Automation Version ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Automation Versions, or get one automation version by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.AutomationVersion result = Run(connection, token => AutomationVersionBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => AutomationVersionBridge.List(connection, ListParameters(), CallOptions));
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
    /// Restore an Automation v2 definition as a new current version.
    /// </summary>
    [Cmdlet("Restore", "FilesAutomationVersion", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Automation), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class RestoreFilesAutomationVersionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Automation Version ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The automation version to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.AutomationVersion InputObject { get; set; }

        /// <summary>Restore an Automation v2 definition as a new current version.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = RestoreParameters();
            if (!ShouldProcessOn(connection, $"Automation version {id}"))
            {
                return;
            }
            Models.Automation result = Run(connection, token => AutomationVersionBridge.RestoreAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> RestoreParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
