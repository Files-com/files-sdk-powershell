using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List history for specific file.
    /// </summary>
    [Cmdlet("Get", "FilesFileHistory", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.HistoryAction), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.HistoryAction>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesFileHistoryCommand : FilesListCmdlet<Models.HistoryAction>
    {
        /// <summary>
        /// Leave blank or set to a date/time to filter earlier entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? StartAt { get; set; }

        /// <summary>
        /// Leave blank or set to a date/time to filter later entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? EndAt { get; set; }

        /// <summary>
        /// Display format. Leave blank or set to `full` or `parent`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Display { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>List history for specific file.</summary>
        protected override void Execute()
        {
            WriteList(connection => HistoryBridge.ListForFile(connection, Path, ListForFileParameters(), CallOptions));
        }

        private Dictionary<string, object> ListForFileParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(StartAt)))
            {
                parameters["start_at"] = RequestValues.DateTime(StartAt.Value);
            }
            if (IsBound(nameof(EndAt)))
            {
                parameters["end_at"] = RequestValues.DateTime(EndAt.Value);
            }
            if (IsBound(nameof(Display)))
            {
                parameters["display"] = Display;
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
            return parameters;
        }
    }

    /// <summary>
    /// List history for specific folder.
    /// </summary>
    [Cmdlet("Get", "FilesFolderHistory", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.HistoryAction), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.HistoryAction>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesFolderHistoryCommand : FilesListCmdlet<Models.HistoryAction>
    {
        /// <summary>
        /// Leave blank or set to a date/time to filter earlier entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? StartAt { get; set; }

        /// <summary>
        /// Leave blank or set to a date/time to filter later entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? EndAt { get; set; }

        /// <summary>
        /// Display format. Leave blank or set to `full` or `parent`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Display { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>List history for specific folder.</summary>
        protected override void Execute()
        {
            WriteList(connection => HistoryBridge.ListForFolder(connection, Path, ListForFolderParameters(), CallOptions));
        }

        private Dictionary<string, object> ListForFolderParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(StartAt)))
            {
                parameters["start_at"] = RequestValues.DateTime(StartAt.Value);
            }
            if (IsBound(nameof(EndAt)))
            {
                parameters["end_at"] = RequestValues.DateTime(EndAt.Value);
            }
            if (IsBound(nameof(Display)))
            {
                parameters["display"] = Display;
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
            return parameters;
        }
    }

    /// <summary>
    /// List history for specific user.
    /// </summary>
    [Cmdlet("Get", "FilesUserHistory", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.HistoryAction), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.HistoryAction>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesUserHistoryCommand : FilesListCmdlet<Models.HistoryAction>
    {
        /// <summary>
        /// Leave blank or set to a date/time to filter earlier entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? StartAt { get; set; }

        /// <summary>
        /// Leave blank or set to a date/time to filter later entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? EndAt { get; set; }

        /// <summary>
        /// Display format. Leave blank or set to `full` or `parent`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Display { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// User ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long UserId { get; set; }

        /// <summary>List history for specific user.</summary>
        protected override void Execute()
        {
            WriteList(connection => HistoryBridge.ListForUser(connection, UserId, ListForUserParameters(), CallOptions));
        }

        private Dictionary<string, object> ListForUserParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(StartAt)))
            {
                parameters["start_at"] = RequestValues.DateTime(StartAt.Value);
            }
            if (IsBound(nameof(EndAt)))
            {
                parameters["end_at"] = RequestValues.DateTime(EndAt.Value);
            }
            if (IsBound(nameof(Display)))
            {
                parameters["display"] = Display;
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
            return parameters;
        }
    }

    /// <summary>
    /// List site login history.
    /// </summary>
    [Cmdlet("Get", "FilesLoginHistory", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.HistoryAction), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.HistoryAction>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesLoginHistoryCommand : FilesListCmdlet<Models.HistoryAction>
    {
        /// <summary>
        /// Leave blank or set to a date/time to filter earlier entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? StartAt { get; set; }

        /// <summary>
        /// Leave blank or set to a date/time to filter later entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? EndAt { get; set; }

        /// <summary>
        /// Display format. Leave blank or set to `full` or `parent`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Display { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>List site login history.</summary>
        protected override void Execute()
        {
            WriteList(connection => HistoryBridge.ListLogins(connection, ListLoginsParameters(), CallOptions));
        }

        private Dictionary<string, object> ListLoginsParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(StartAt)))
            {
                parameters["start_at"] = RequestValues.DateTime(StartAt.Value);
            }
            if (IsBound(nameof(EndAt)))
            {
                parameters["end_at"] = RequestValues.DateTime(EndAt.Value);
            }
            if (IsBound(nameof(Display)))
            {
                parameters["display"] = Display;
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
            return parameters;
        }
    }

    /// <summary>
    /// List site full action history.
    /// </summary>
    [Cmdlet("Get", "FilesHistory", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.HistoryAction), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.HistoryAction>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesHistoryCommand : FilesListCmdlet<Models.HistoryAction>
    {
        /// <summary>
        /// Leave blank or set to a date/time to filter earlier entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? StartAt { get; set; }

        /// <summary>
        /// Leave blank or set to a date/time to filter later entries.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? EndAt { get; set; }

        /// <summary>
        /// Display format. Leave blank or set to `full` or `parent`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Display { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `created_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `user_id`, `folder` or `path`. Valid field combinations are `[  ]`, `[ path ]`, `[ path ]` or `[ path ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `path`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>List site full action history.</summary>
        protected override void Execute()
        {
            WriteList(connection => HistoryBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(StartAt)))
            {
                parameters["start_at"] = RequestValues.DateTime(StartAt.Value);
            }
            if (IsBound(nameof(EndAt)))
            {
                parameters["end_at"] = RequestValues.DateTime(EndAt.Value);
            }
            if (IsBound(nameof(Display)))
            {
                parameters["display"] = Display;
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
            if (IsBound(nameof(FilterPrefix)))
            {
                parameters["filter_prefix"] = RequestValues.QueryObject(FilterPrefix, "FilterPrefix");
            }
            return parameters;
        }
    }
}
