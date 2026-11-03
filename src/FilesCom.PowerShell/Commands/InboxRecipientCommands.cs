using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Inbox Recipients.
    /// </summary>
    [Cmdlet("Get", "FilesInboxRecipient", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.InboxRecipient), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.InboxRecipient>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesInboxRecipientCommand : FilesListCmdlet<Models.InboxRecipient>
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
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `has_registrations`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// List recipients for the inbox with this ID.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "List")]
        [Parameter(Mandatory = true, ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long InboxId { get; set; }

        /// <summary>List Inbox Recipients.</summary>
        protected override void Execute()
        {
            WriteList(connection => InboxRecipientBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(InboxId)))
            {
                parameters["inbox_id"] = InboxId;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Inbox Recipient.
    /// </summary>
    [Cmdlet("New", "FilesInboxRecipient", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.InboxRecipient))]
    public sealed class NewFilesInboxRecipientCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Inbox to share.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long InboxId { get; set; }

        /// <summary>
        /// Email address to share this inbox with.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Recipient { get; set; }

        /// <summary>
        /// Name of recipient.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Company of recipient.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Company { get; set; }

        /// <summary>
        /// Note to include in email.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Note { get; set; }

        /// <summary>
        /// Set to true to share the link with the recipient upon creation.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ShareAfterCreate { get; set; }

        /// <summary>Create Inbox Recipient.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Inbox recipient '{Name}'" : "Inbox recipient"))
            {
                return;
            }
            Models.InboxRecipient result = Run(connection, token => InboxRecipientBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(InboxId)))
            {
                parameters["inbox_id"] = InboxId;
            }
            if (IsBound(nameof(Recipient)))
            {
                parameters["recipient"] = Recipient;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Company)))
            {
                parameters["company"] = Company;
            }
            if (IsBound(nameof(Note)))
            {
                parameters["note"] = Note;
            }
            if (IsBound(nameof(ShareAfterCreate)))
            {
                parameters["share_after_create"] = ShareAfterCreate.Value;
            }
            return parameters;
        }
    }
}
