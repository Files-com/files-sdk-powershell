using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Snapshots, or get one snapshot by Id.
    /// </summary>
    [Cmdlet("Get", "FilesSnapshot", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Snapshot), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Snapshot>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Snapshot), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesSnapshotCommand : FilesListCmdlet<Models.Snapshot>
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
        /// Snapshot ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Snapshots, or get one snapshot by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Snapshot result = Run(connection, token => SnapshotBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => SnapshotBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Snapshot.
    /// </summary>
    [Cmdlet("New", "FilesSnapshot", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Snapshot))]
    public sealed class NewFilesSnapshotCommand : FilesApiCmdlet
    {
        /// <summary>
        /// When the snapshot expires.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// A name for the snapshot.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// An array of paths to add to the snapshot.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] Paths { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Snapshot.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Snapshot '{Name}'" : "Snapshot"))
            {
                return;
            }
            Models.Snapshot result = Run(connection, token => SnapshotBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(ExpiresAt)))
            {
                parameters["expires_at"] = RequestValues.DateTime(ExpiresAt.Value);
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Paths)))
            {
                parameters["paths"] = RequestValues.Strings(Paths, "Paths");
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Finalize Snapshot.
    /// </summary>
    [Cmdlet("Complete", "FilesSnapshot", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class CompleteFilesSnapshotCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Snapshot ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The snapshot to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Snapshot InputObject { get; set; }

        /// <summary>Finalize Snapshot.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = FinalizeParameters();
            if (!ShouldProcessOn(connection, $"Snapshot {id}"))
            {
                return;
            }
            Run(connection, token => SnapshotBridge.FinalizeAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> FinalizeParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update Snapshot.
    /// </summary>
    [Cmdlet("Set", "FilesSnapshot", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Snapshot), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesSnapshotCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Snapshot ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// When the snapshot expires.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// A name for the snapshot.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// An array of paths to add to the snapshot.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] Paths { get; set; }

        /// <summary>
        /// The snapshot to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Snapshot InputObject { get; set; }

        /// <summary>Update Snapshot.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Snapshot {id}"))
            {
                return;
            }
            Models.Snapshot result = Run(connection, token => SnapshotBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(ExpiresAt)))
            {
                parameters["expires_at"] = RequestValues.DateTime(ExpiresAt.Value);
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Paths)))
            {
                parameters["paths"] = RequestValues.Strings(Paths, "Paths");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Snapshot.
    /// </summary>
    [Cmdlet("Remove", "FilesSnapshot", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesSnapshotCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Snapshot ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The snapshot to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Snapshot InputObject { get; set; }

        /// <summary>Delete Snapshot.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Snapshot {id}"))
            {
                return;
            }
            Run(connection, token => SnapshotBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
