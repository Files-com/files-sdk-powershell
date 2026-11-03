using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Folders by Path.
    /// </summary>
    [Cmdlet("Get", "FilesChildItem", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.RemoteFile), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.RemoteFile>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesChildItemCommand : FilesListCmdlet<Models.RemoteFile>
    {
        /// <summary>
        /// Send cursor to resume an existing list from the point at which you left off.  Get a cursor from an existing list via the X-Files-Cursor-Next header or the X-Files-Cursor-Prev header.
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
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Request a preview size.  Can be `small` (default), `large`, `xlarge`, or `pdf`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string PreviewSize { get; set; }

        /// <summary>
        /// Search by field and direction. Valid fields are `path`, `size`, `modified_at_datetime`, `provided_modified_at`.  Valid directions are `asc` and `desc`.  Defaults to `{"path":"asc"}`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If specified, will search the folders/files list by name. Ignores text before last `/`. This is the same API used by the search bar in the web UI when running 'Search This Folder'.  Search results are a best effort, not real time, and not guaranteed to perfectly match the latest folder listing.  Results may be truncated if more than 1,000 possible matches exist.  This field should only be used for ad-hoc (human) searching, and not as part of an automated process.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Search { get; set; }

        /// <summary>
        /// If provided, the search string in `search` will search for files where this custom metadata key matches the value sent in `search`.  Set this to `*` to allow any metadata key to match the value sent in `search`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string SearchCustomMetadataKey { get; set; }

        /// <summary>
        /// Search entire site?  If true, we will search the entire site.  Do not provide a path when using this parameter.  This is the same API used by the search bar in the web UI when running 'Search All Files'.  Search results are a best effort, not real time, and not guaranteed to match every file.  This field should only be used for ad-hoc (human) searching, and not as part of an automated process.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? SearchAll { get; set; }

        /// <summary>
        /// Include file previews?
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? WithPreviews { get; set; }

        /// <summary>
        /// Include file priority color information?
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? WithPriorityColor { get; set; }

        /// <summary>
        /// Type of objects to return.  Can be `folder` or `file`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Type { get; set; }

        /// <summary>
        /// If provided, will only return files/folders modified after this time. Can be used only in combination with `type` filter.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ModifiedAtDatetime { get; set; }

        /// <summary>List Folders by Path.</summary>
        protected override void Execute()
        {
            WriteList(connection => FolderBridge.ListFor(connection, Path, ListForParameters(), CallOptions));
        }

        private Dictionary<string, object> ListForParameters()
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
            if (IsBound(nameof(PreviewSize)))
            {
                parameters["preview_size"] = PreviewSize;
            }
            if (IsBound(nameof(SortBy)))
            {
                parameters["sort_by"] = RequestValues.QueryObject(SortBy, "SortBy");
            }
            if (IsBound(nameof(Search)))
            {
                parameters["search"] = Search;
            }
            if (IsBound(nameof(SearchCustomMetadataKey)))
            {
                parameters["search_custom_metadata_key"] = SearchCustomMetadataKey;
            }
            if (IsBound(nameof(SearchAll)))
            {
                parameters["search_all"] = SearchAll.Value;
            }
            if (IsBound(nameof(WithPreviews)))
            {
                parameters["with_previews"] = WithPreviews.Value;
            }
            if (IsBound(nameof(WithPriorityColor)))
            {
                parameters["with_priority_color"] = WithPriorityColor.Value;
            }
            if (IsBound(nameof(Type)))
            {
                parameters["type"] = Type;
            }
            if (IsBound(nameof(ModifiedAtDatetime)))
            {
                parameters["modified_at_datetime"] = RequestValues.DateTime(ModifiedAtDatetime.Value);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Folder.
    /// </summary>
    [Cmdlet("New", "FilesFolder", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.RemoteFile))]
    public sealed class NewFilesFolderCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Create parent directories if they do not exist?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? MkdirParents { get; set; }

        /// <summary>
        /// User provided modification time.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ProvidedMtime { get; set; }

        /// <summary>Create Folder.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            string path = Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, $"Folder '{path}'"))
            {
                return;
            }
            Models.RemoteFile result = Run(connection, token => FolderBridge.CreateAsync(connection, path, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(MkdirParents)))
            {
                parameters["mkdir_parents"] = MkdirParents.Value;
            }
            if (IsBound(nameof(ProvidedMtime)))
            {
                parameters["provided_mtime"] = RequestValues.DateTime(ProvidedMtime.Value);
            }
            return parameters;
        }
    }
}
