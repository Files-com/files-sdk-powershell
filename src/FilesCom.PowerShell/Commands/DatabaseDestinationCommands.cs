using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Database Destinations, or get one database destination by Id.
    /// </summary>
    [Cmdlet("Get", "FilesDatabaseDestination", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.DatabaseDestination), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.DatabaseDestination>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.DatabaseDestination), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesDatabaseDestinationCommand : FilesListCmdlet<Models.DatabaseDestination>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id` and `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id` and `name`. Valid field combinations are `[ workspace_id, name ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// Database Destination ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Database Destinations, or get one database destination by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.DatabaseDestination result = Run(connection, token => DatabaseDestinationBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => DatabaseDestinationBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(FilterPrefix)))
            {
                parameters["filter_prefix"] = RequestValues.QueryObject(FilterPrefix, "FilterPrefix");
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
    /// Create Database Destination.
    /// </summary>
    [Cmdlet("New", "FilesDatabaseDestination", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.DatabaseDestination))]
    public sealed class NewFilesDatabaseDestinationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Name of the destination.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Database provider.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Provider { get; set; }

        /// <summary>
        /// Provider settings for the destination. Use the schema matching provider. The connection address and table are fixed after creation.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Config { get; set; }

        /// <summary>
        /// ID of a Secret in the same Workspace. Use a Token Secret for Snowflake, a service account Key/Value Secret for BigQuery, or a Username/Password Secret for a database login.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long SecretId { get; set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Database Destination.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Database destination '{Name}'" : "Database destination"))
            {
                return;
            }
            Models.DatabaseDestination result = Run(connection, token => DatabaseDestinationBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Provider)))
            {
                parameters["provider"] = Provider;
            }
            if (IsBound(nameof(Config)))
            {
                parameters["config"] = RequestValues.Object(Config, "Config");
            }
            if (IsBound(nameof(SecretId)))
            {
                parameters["secret_id"] = SecretId;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Database Destination.
    /// </summary>
    [Cmdlet("Set", "FilesDatabaseDestination", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.DatabaseDestination), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesDatabaseDestinationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Database Destination ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Name of the destination.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// ID of a Secret in the same Workspace. Use a Token Secret for Snowflake, a service account Key/Value Secret for BigQuery, or a Username/Password Secret for a database login.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? SecretId { get; set; }

        /// <summary>
        /// Provider settings for the destination. Use the schema matching provider. The connection address and table are fixed after creation.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Config { get; set; }

        /// <summary>
        /// The database destination to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.DatabaseDestination InputObject { get; set; }

        /// <summary>Update Database Destination.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Database destination {id}"))
            {
                return;
            }
            Models.DatabaseDestination result = Run(connection, token => DatabaseDestinationBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(SecretId)))
            {
                parameters["secret_id"] = SecretId.Value;
            }
            if (IsBound(nameof(Config)))
            {
                parameters["config"] = RequestValues.Object(Config, "Config");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Database Destination.
    /// </summary>
    [Cmdlet("Remove", "FilesDatabaseDestination", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesDatabaseDestinationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Database Destination ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The database destination to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.DatabaseDestination InputObject { get; set; }

        /// <summary>Delete Database Destination.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Database destination {id}"))
            {
                return;
            }
            Run(connection, token => DatabaseDestinationBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
