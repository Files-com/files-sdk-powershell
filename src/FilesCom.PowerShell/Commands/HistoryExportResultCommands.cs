using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List History Export Results.
    /// </summary>
    [Cmdlet("Get", "FilesHistoryExportResult", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.HistoryExportResult), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.HistoryExportResult>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesHistoryExportResultCommand : FilesListCmdlet<Models.HistoryExportResult>
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
        /// ID of the associated history export.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "List")]
        [Parameter(Mandatory = true, ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long HistoryExportId { get; set; }

        /// <summary>List History Export Results.</summary>
        protected override void Execute()
        {
            WriteList(connection => HistoryExportResultBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(HistoryExportId)))
            {
                parameters["history_export_id"] = HistoryExportId;
            }
            return parameters;
        }
    }
}
