using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Share Link Recipients.
    /// </summary>
    [Cmdlet("Get", "FilesBundleRecipient", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.BundleRecipient), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.BundleRecipient>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesBundleRecipientCommand : FilesListCmdlet<Models.BundleRecipient>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`.
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
        /// List recipients for the bundle with this ID.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "List")]
        [Parameter(Mandatory = true, ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long BundleId { get; set; }

        /// <summary>List Share Link Recipients.</summary>
        protected override void Execute()
        {
            WriteList(connection => BundleRecipientBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(BundleId)))
            {
                parameters["bundle_id"] = BundleId;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Share Link Recipient.
    /// </summary>
    [Cmdlet("New", "FilesBundleRecipient", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.BundleRecipient))]
    public sealed class NewFilesBundleRecipientCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// Bundle to share.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long BundleId { get; set; }

        /// <summary>
        /// Email addresses to share this bundle with.
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

        /// <summary>Create Share Link Recipient.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Bundle recipient '{Name}'" : "Bundle recipient"))
            {
                return;
            }
            Models.BundleRecipient result = Run(connection, token => BundleRecipientBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(BundleId)))
            {
                parameters["bundle_id"] = BundleId;
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
