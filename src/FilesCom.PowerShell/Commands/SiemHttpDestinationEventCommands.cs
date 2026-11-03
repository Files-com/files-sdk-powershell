using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List SIEM HTTP Destination Events, or get one siem http destination event by Id.
    /// </summary>
    [Cmdlet("Get", "FilesSiemHttpDestinationEvent", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.SiemHttpDestinationEvent), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.SiemHttpDestinationEvent>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.SiemHttpDestinationEvent), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesSiemHttpDestinationEventCommand : FilesListCmdlet<Models.SiemHttpDestinationEvent>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `created_at`, `status` or `siem_http_destination_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `created_at`, `siem_http_destination_id` or `status`. Valid field combinations are `[ siem_http_destination_id, created_at ]`, `[ status, created_at ]`, `[ siem_http_destination_id, status ]` or `[ siem_http_destination_id, status, created_at ]`.
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
        /// Siem Http Destination Event ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List SIEM HTTP Destination Events, or get one siem http destination event by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.SiemHttpDestinationEvent result = Run(connection, token => SiemHttpDestinationEventBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => SiemHttpDestinationEventBridge.List(connection, ListParameters(), CallOptions));
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
}
