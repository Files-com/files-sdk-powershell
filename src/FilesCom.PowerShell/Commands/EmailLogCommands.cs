using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Email Logs.
    /// </summary>
    [Cmdlet("Get", "FilesEmailLog", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.EmailLog), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.EmailLog>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesEmailLogCommand : FilesListCmdlet<Models.EmailLog>
    {
        /// <summary>
        /// Used for pagination.  When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`.  Send one of those cursor value here to resume an existing list from the next available record.  Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Cursor { get; set; }

        /// <summary>
        /// Number of records to show per page.  (Max: 1000, 1,000 or less is recommended).
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PerPage { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `status` and `created_at`. Valid field combinations are `[ status ]`, `[ created_at ]` or `[ status, created_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ status ]`, `[ created_at ]` or `[ status, created_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ status ]`, `[ created_at ]` or `[ status, created_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGteq { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ status ]`, `[ created_at ]` or `[ status, created_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ status ]`, `[ created_at ]` or `[ status, created_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLteq { get; set; }

        /// <summary>List Email Logs.</summary>
        protected override void Execute()
        {
            WriteList(connection => EmailLogBridge.List(connection, ListParameters(), CallOptions));
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
    }
}
