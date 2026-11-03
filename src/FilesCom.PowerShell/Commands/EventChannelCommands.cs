using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Event Channels, or get one event channel by Id.
    /// </summary>
    [Cmdlet("Get", "FilesEventChannel", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.EventChannel), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.EventChannel>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.EventChannel), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesEventChannelCommand : FilesListCmdlet<Models.EventChannel>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `name`, `enabled`, `default_channel` or `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `enabled`, `default_channel` or `workspace_id`. Valid field combinations are `[ workspace_id, enabled ]` and `[ workspace_id, default_channel ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Event Channel ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Event Channels, or get one event channel by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.EventChannel result = Run(connection, token => EventChannelBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => EventChannelBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Event Channel.
    /// </summary>
    [Cmdlet("New", "FilesEventChannel", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.EventChannel))]
    public sealed class NewFilesEventChannelCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Event Channel name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Event Channel description.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Whether this Event Channel can dispatch events.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Whether this Event Channel is the default destination for newly published events.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DefaultChannel { get; set; }

        /// <summary>Create Event Channel.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Event channel '{Name}'" : "Event channel"))
            {
                return;
            }
            Models.EventChannel result = Run(connection, token => EventChannelBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Enabled)))
            {
                parameters["enabled"] = Enabled.Value;
            }
            if (IsBound(nameof(DefaultChannel)))
            {
                parameters["default_channel"] = DefaultChannel.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Event Channel.
    /// </summary>
    [Cmdlet("Set", "FilesEventChannel", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.EventChannel), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesEventChannelCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Event Channel ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Event Channel name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Event Channel description.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Whether this Event Channel can dispatch events.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Whether this Event Channel is the default destination for newly published events.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DefaultChannel { get; set; }

        /// <summary>
        /// The event channel to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.EventChannel InputObject { get; set; }

        /// <summary>Update Event Channel.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Event channel {id}"))
            {
                return;
            }
            Models.EventChannel result = Run(connection, token => EventChannelBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Enabled)))
            {
                parameters["enabled"] = Enabled.Value;
            }
            if (IsBound(nameof(DefaultChannel)))
            {
                parameters["default_channel"] = DefaultChannel.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Event Channel.
    /// </summary>
    [Cmdlet("Remove", "FilesEventChannel", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesEventChannelCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Event Channel ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The event channel to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.EventChannel InputObject { get; set; }

        /// <summary>Delete Event Channel.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Event channel {id}"))
            {
                return;
            }
            Run(connection, token => EventChannelBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
