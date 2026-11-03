using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Clickwraps, or get one clickwrap by Id.
    /// </summary>
    [Cmdlet("Get", "FilesClickwrap", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Clickwrap), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Clickwrap>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Clickwrap), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesClickwrapCommand : FilesListCmdlet<Models.Clickwrap>
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
        /// Clickwrap ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Clickwraps, or get one clickwrap by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Clickwrap result = Run(connection, token => ClickwrapBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => ClickwrapBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create Clickwrap.
    /// </summary>
    [Cmdlet("New", "FilesClickwrap", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Clickwrap))]
    public sealed class NewFilesClickwrapCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Name of the Clickwrap agreement (used when selecting from multiple Clickwrap agreements.)
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Body text of Clickwrap (supports Markdown formatting).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Body { get; set; }

        /// <summary>
        /// Use this Clickwrap for Bundles?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UseWithBundles { get; set; }

        /// <summary>
        /// Use this Clickwrap for Inboxes?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UseWithInboxes { get; set; }

        /// <summary>
        /// Use this Clickwrap for Users?  Values: `none`, `require` (new user signup via email invitation only), `require_all_users_once` (show to all users at their next web login; once accepted, not shown again), `require_all_users_always` (show to all users on every web login).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string UseWithUsers { get; set; }

        /// <summary>Create Clickwrap.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Clickwrap '{Name}'" : "Clickwrap"))
            {
                return;
            }
            Models.Clickwrap result = Run(connection, token => ClickwrapBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Body)))
            {
                parameters["body"] = Body;
            }
            if (IsBound(nameof(UseWithBundles)))
            {
                parameters["use_with_bundles"] = UseWithBundles;
            }
            if (IsBound(nameof(UseWithInboxes)))
            {
                parameters["use_with_inboxes"] = UseWithInboxes;
            }
            if (IsBound(nameof(UseWithUsers)))
            {
                parameters["use_with_users"] = UseWithUsers;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Clickwrap.
    /// </summary>
    [Cmdlet("Set", "FilesClickwrap", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Clickwrap), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesClickwrapCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Clickwrap ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Name of the Clickwrap agreement (used when selecting from multiple Clickwrap agreements.)
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Body text of Clickwrap (supports Markdown formatting).
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Body { get; set; }

        /// <summary>
        /// Use this Clickwrap for Bundles?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string UseWithBundles { get; set; }

        /// <summary>
        /// Use this Clickwrap for Inboxes?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string UseWithInboxes { get; set; }

        /// <summary>
        /// Use this Clickwrap for Users?  Values: `none`, `require` (new user signup via email invitation only), `require_all_users_once` (show to all users at their next web login; once accepted, not shown again), `require_all_users_always` (show to all users on every web login).
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string UseWithUsers { get; set; }

        /// <summary>
        /// The clickwrap to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Clickwrap InputObject { get; set; }

        /// <summary>Update Clickwrap.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Clickwrap {id}"))
            {
                return;
            }
            Models.Clickwrap result = Run(connection, token => ClickwrapBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Body)))
            {
                parameters["body"] = Body;
            }
            if (IsBound(nameof(UseWithBundles)))
            {
                parameters["use_with_bundles"] = UseWithBundles;
            }
            if (IsBound(nameof(UseWithInboxes)))
            {
                parameters["use_with_inboxes"] = UseWithInboxes;
            }
            if (IsBound(nameof(UseWithUsers)))
            {
                parameters["use_with_users"] = UseWithUsers;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Clickwrap.
    /// </summary>
    [Cmdlet("Remove", "FilesClickwrap", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesClickwrapCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Clickwrap ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The clickwrap to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Clickwrap InputObject { get; set; }

        /// <summary>Delete Clickwrap.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Clickwrap {id}"))
            {
                return;
            }
            Run(connection, token => ClickwrapBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
