using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Requests.
    /// </summary>
    [Cmdlet("Get", "FilesRequest", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Request), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Request>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesRequestCommand : FilesListCmdlet<Models.Request>
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
        /// Only show requests of the current user?  (Defaults to true if current user is not a site admin.)
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? Mine { get; set; }

        /// <summary>
        /// Path to show requests for.  If omitted, shows all paths. Send `/` to represent the root directory.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>List Requests.</summary>
        protected override void Execute()
        {
            WriteList(connection => RequestBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(Mine)))
            {
                parameters["mine"] = Mine.Value;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            return parameters;
        }
    }

    /// <summary>
    /// List Requests.
    /// </summary>
    [Cmdlet("Get", "FilesFolderRequest", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Request), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Request>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesFolderRequestCommand : FilesListCmdlet<Models.Request>
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
        /// Only show requests of the current user?  (Defaults to true if current user is not a site admin.)
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? Mine { get; set; }

        /// <summary>
        /// Path to show requests for.  If omitted, shows all paths. Send `/` to represent the root directory.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>List Requests.</summary>
        protected override void Execute()
        {
            WriteList(connection => RequestBridge.GetFolder(connection, Path, GetFolderParameters(), CallOptions));
        }

        private Dictionary<string, object> GetFolderParameters()
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
            if (IsBound(nameof(Mine)))
            {
                parameters["mine"] = Mine.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Request.
    /// </summary>
    [Cmdlet("New", "FilesRequest", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Request))]
    public sealed class NewFilesRequestCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Folder path on which to request the file.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Destination filename (without extension) to request.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// A list of user IDs to request the file from. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] UserIds { get; set; }

        /// <summary>
        /// A list of group IDs to request the file from. If sent as a string, it should be comma-delimited.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GroupIds { get; set; }

        /// <summary>Create Request.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Request '{Path}'" : "Request"))
            {
                return;
            }
            Models.Request result = Run(connection, token => RequestBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(UserIds)))
            {
                parameters["user_ids"] = RequestValues.JoinInt64s(UserIds);
            }
            if (IsBound(nameof(GroupIds)))
            {
                parameters["group_ids"] = RequestValues.JoinInt64s(GroupIds);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Request.
    /// </summary>
    [Cmdlet("Remove", "FilesRequest", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesRequestCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Request ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The request to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Request InputObject { get; set; }

        /// <summary>Delete Request.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Request {id}"))
            {
                return;
            }
            Run(connection, token => RequestBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
