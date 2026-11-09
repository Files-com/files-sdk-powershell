using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Metadata Categories, or get one metadata category by Id.
    /// </summary>
    [Cmdlet("Get", "FilesMetadataCategory", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.MetadataCategory), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.MetadataCategory>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.MetadataCategory), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesMetadataCategoryCommand : FilesListCmdlet<Models.MetadataCategory>
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
        /// Metadata Category ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Metadata Categories, or get one metadata category by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.MetadataCategory result = Run(connection, token => MetadataCategoryBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => MetadataCategoryBridge.List(connection, ListParameters(), CallOptions));
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
    /// List Metadata Categories by Path.
    /// </summary>
    [Cmdlet("Get", "FilesPathMetadataCategory", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.MetadataCategory), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.MetadataCategory>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesPathMetadataCategoryCommand : FilesListCmdlet<Models.MetadataCategory>
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

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

        /// <summary>List Metadata Categories by Path.</summary>
        protected override void Execute()
        {
            WriteList(connection => MetadataCategoryBridge.ListFor(connection, Path, ListForParameters(), CallOptions));
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
            return parameters;
        }
    }

    /// <summary>
    /// Create Metadata Category.
    /// </summary>
    [Cmdlet("New", "FilesMetadataCategory", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.MetadataCategory))]
    public sealed class NewFilesMetadataCategoryCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Name of the metadata category.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Metadata keys that should appear as columns in the UI by default.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] DefaultColumns { get; set; }

        /// <summary>Create Metadata Category.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Metadata category '{Name}'" : "Metadata category"))
            {
                return;
            }
            Models.MetadataCategory result = Run(connection, token => MetadataCategoryBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(DefaultColumns)))
            {
                parameters["default_columns"] = RequestValues.Strings(DefaultColumns, "DefaultColumns");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Metadata Category.
    /// </summary>
    [Cmdlet("Set", "FilesMetadataCategory", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.MetadataCategory), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesMetadataCategoryCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Metadata Category ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Name of the metadata category.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Metadata keys that should appear as columns in the UI by default.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] DefaultColumns { get; set; }

        /// <summary>
        /// The metadata category to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.MetadataCategory InputObject { get; set; }

        /// <summary>Update Metadata Category.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Metadata category {id}"))
            {
                return;
            }
            Models.MetadataCategory result = Run(connection, token => MetadataCategoryBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(DefaultColumns)))
            {
                parameters["default_columns"] = RequestValues.Strings(DefaultColumns, "DefaultColumns");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Metadata Category.
    /// </summary>
    [Cmdlet("Remove", "FilesMetadataCategory", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesMetadataCategoryCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Metadata Category ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The metadata category to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.MetadataCategory InputObject { get; set; }

        /// <summary>Delete Metadata Category.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Metadata category {id}"))
            {
                return;
            }
            Run(connection, token => MetadataCategoryBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
