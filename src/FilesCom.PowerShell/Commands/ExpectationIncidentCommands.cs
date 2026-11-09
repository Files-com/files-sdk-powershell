using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Expectation Incidents, or get one expectation incident by Id.
    /// </summary>
    [Cmdlet("Get", "FilesExpectationIncident", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.ExpectationIncident), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.ExpectationIncident>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.ExpectationIncident), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesExpectationIncidentCommand : FilesListCmdlet<Models.ExpectationIncident>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `created_at` or `expectation_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `expectation_id` and `workspace_id`. Valid field combinations are `[ workspace_id, expectation_id ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Expectation Incident ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Expectation Incidents, or get one expectation incident by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.ExpectationIncident result = Run(connection, token => ExpectationIncidentBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => ExpectationIncidentBridge.List(connection, ListParameters(), CallOptions));
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
    /// Acknowledge an expectation incident.
    /// </summary>
    [Cmdlet("Set", "FilesExpectationIncidentAcknowledgement", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.ExpectationIncident), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesExpectationIncidentAcknowledgementCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Expectation Incident ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The expectation incident to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ExpectationIncident InputObject { get; set; }

        /// <summary>Acknowledge an expectation incident.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = AcknowledgeParameters();
            if (!ShouldProcessOn(connection, $"Expectation incident {id}"))
            {
                return;
            }
            Models.ExpectationIncident result = Run(connection, token => ExpectationIncidentBridge.AcknowledgeAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> AcknowledgeParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Snooze an expectation incident until a specified time.
    /// </summary>
    [Cmdlet("Suspend", "FilesExpectationIncident", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.ExpectationIncident), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SuspendFilesExpectationIncidentCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Expectation Incident ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Time until which the incident should remain snoozed.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Id")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset SnoozedUntil { get; set; }

        /// <summary>
        /// The expectation incident to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ExpectationIncident InputObject { get; set; }

        /// <summary>Snooze an expectation incident until a specified time.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = SnoozeParameters();
            if (!ShouldProcessOn(connection, $"Expectation incident {id}"))
            {
                return;
            }
            Models.ExpectationIncident result = Run(connection, token => ExpectationIncidentBridge.SnoozeAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> SnoozeParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(SnoozedUntil)))
            {
                parameters["snoozed_until"] = RequestValues.DateTime(SnoozedUntil);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Resolve an expectation incident.
    /// </summary>
    [Cmdlet("Resolve", "FilesExpectationIncident", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.ExpectationIncident), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class ResolveFilesExpectationIncidentCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Expectation Incident ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The expectation incident to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ExpectationIncident InputObject { get; set; }

        /// <summary>Resolve an expectation incident.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ResolveParameters();
            if (!ShouldProcessOn(connection, $"Expectation incident {id}"))
            {
                return;
            }
            Models.ExpectationIncident result = Run(connection, token => ExpectationIncidentBridge.ResolveAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> ResolveParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
