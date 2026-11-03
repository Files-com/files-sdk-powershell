using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Inbox Registrations.
    /// </summary>
    [Cmdlet("Get", "FilesInboxRegistration", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.InboxRegistration), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.InboxRegistration>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesInboxRegistrationCommand : FilesListCmdlet<Models.InboxRegistration>
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
        /// ID of the associated Inbox. This is required if the user is not a site admin.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? FolderBehaviorId { get; set; }

        /// <summary>List Inbox Registrations.</summary>
        protected override void Execute()
        {
            WriteList(connection => InboxRegistrationBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(FolderBehaviorId)))
            {
                parameters["folder_behavior_id"] = FolderBehaviorId.Value;
            }
            return parameters;
        }
    }
}
