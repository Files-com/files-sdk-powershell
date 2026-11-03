using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Remote Mount Backends, or get one remote mount backend by Id.
    /// </summary>
    [Cmdlet("Get", "FilesRemoteMountBackend", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.RemoteMountBackend), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.RemoteMountBackend>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.RemoteMountBackend), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesRemoteMountBackendCommand : FilesListCmdlet<Models.RemoteMountBackend>
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
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `remote_server_mount_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Remote Mount Backend ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Remote Mount Backends, or get one remote mount backend by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.RemoteMountBackend result = Run(connection, token => RemoteMountBackendBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => RemoteMountBackendBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create Remote Mount Backend.
    /// </summary>
    [Cmdlet("New", "FilesRemoteMountBackend", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.RemoteMountBackend))]
    public sealed class NewFilesRemoteMountBackendCommand : FilesApiCmdlet
    {
        /// <summary>
        /// True if this backend is enabled.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Number of consecutive failures before considering the backend unhealthy.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Fall { get; set; }

        /// <summary>
        /// True if health checks are enabled for this backend.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? HealthCheckEnabled { get; set; }

        /// <summary>
        /// Type of health check to perform.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string HealthCheckType { get; set; }

        /// <summary>
        /// Interval in seconds between health checks.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Interval { get; set; }

        /// <summary>
        /// Minimum free CPU percentage required for this backend to be considered healthy.
        /// </summary>
        [Parameter()]
        [FilesDecimal]
        [ValidateNotNull]
        public decimal? MinFreeCpu { get; set; }

        /// <summary>
        /// Minimum free memory percentage required for this backend to be considered healthy.
        /// </summary>
        [Parameter()]
        [FilesDecimal]
        [ValidateNotNull]
        public decimal? MinFreeMem { get; set; }

        /// <summary>
        /// Priority of this backend.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Priority { get; set; }

        /// <summary>
        /// Path on the remote server to treat as the root of this mount.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string RemotePath { get; set; }

        /// <summary>
        /// Number of consecutive successes before considering the backend healthy.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Rise { get; set; }

        /// <summary>
        /// Path to the canary file used for health checks.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string CanaryFilePath { get; set; }

        /// <summary>
        /// The mount ID of the Remote Server Mount that this backend is associated with.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long RemoteServerMountId { get; set; }

        /// <summary>
        /// The remote server that this backend is associated with.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long RemoteServerId { get; set; }

        /// <summary>Create Remote Mount Backend.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Remote mount backend"))
            {
                return;
            }
            Models.RemoteMountBackend result = Run(connection, token => RemoteMountBackendBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Enabled)))
            {
                parameters["enabled"] = Enabled.Value;
            }
            if (IsBound(nameof(Fall)))
            {
                parameters["fall"] = Fall.Value;
            }
            if (IsBound(nameof(HealthCheckEnabled)))
            {
                parameters["health_check_enabled"] = HealthCheckEnabled.Value;
            }
            if (IsBound(nameof(HealthCheckType)))
            {
                parameters["health_check_type"] = HealthCheckType;
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval.Value;
            }
            if (IsBound(nameof(MinFreeCpu)))
            {
                parameters["min_free_cpu"] = MinFreeCpu.Value;
            }
            if (IsBound(nameof(MinFreeMem)))
            {
                parameters["min_free_mem"] = MinFreeMem.Value;
            }
            if (IsBound(nameof(Priority)))
            {
                parameters["priority"] = Priority.Value;
            }
            if (IsBound(nameof(RemotePath)))
            {
                parameters["remote_path"] = RemotePath;
            }
            if (IsBound(nameof(Rise)))
            {
                parameters["rise"] = Rise.Value;
            }
            if (IsBound(nameof(CanaryFilePath)))
            {
                parameters["canary_file_path"] = CanaryFilePath;
            }
            if (IsBound(nameof(RemoteServerMountId)))
            {
                parameters["remote_server_mount_id"] = RemoteServerMountId;
            }
            if (IsBound(nameof(RemoteServerId)))
            {
                parameters["remote_server_id"] = RemoteServerId;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Reset backend status to healthy.
    /// </summary>
    [Cmdlet("Reset", "FilesRemoteMountBackendStatus", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class ResetFilesRemoteMountBackendStatusCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Mount Backend ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The remote mount backend to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteMountBackend InputObject { get; set; }

        /// <summary>Reset backend status to healthy.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ResetStatusParameters();
            if (!ShouldProcessOn(connection, $"Remote mount backend {id}"))
            {
                return;
            }
            Run(connection, token => RemoteMountBackendBridge.ResetStatusAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> ResetStatusParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update Remote Mount Backend.
    /// </summary>
    [Cmdlet("Set", "FilesRemoteMountBackend", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.RemoteMountBackend), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesRemoteMountBackendCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Mount Backend ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// True if this backend is enabled.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Number of consecutive failures before considering the backend unhealthy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Fall { get; set; }

        /// <summary>
        /// True if health checks are enabled for this backend.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? HealthCheckEnabled { get; set; }

        /// <summary>
        /// Type of health check to perform.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string HealthCheckType { get; set; }

        /// <summary>
        /// Interval in seconds between health checks.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Interval { get; set; }

        /// <summary>
        /// Minimum free CPU percentage required for this backend to be considered healthy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDecimal]
        [ValidateNotNull]
        public decimal? MinFreeCpu { get; set; }

        /// <summary>
        /// Minimum free memory percentage required for this backend to be considered healthy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDecimal]
        [ValidateNotNull]
        public decimal? MinFreeMem { get; set; }

        /// <summary>
        /// Priority of this backend.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Priority { get; set; }

        /// <summary>
        /// Path on the remote server to treat as the root of this mount.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string RemotePath { get; set; }

        /// <summary>
        /// Number of consecutive successes before considering the backend healthy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Rise { get; set; }

        /// <summary>
        /// Path to the canary file used for health checks.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string CanaryFilePath { get; set; }

        /// <summary>
        /// The remote server that this backend is associated with.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? RemoteServerId { get; set; }

        /// <summary>
        /// The remote mount backend to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteMountBackend InputObject { get; set; }

        /// <summary>Update Remote Mount Backend.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Remote mount backend {id}"))
            {
                return;
            }
            Models.RemoteMountBackend result = Run(connection, token => RemoteMountBackendBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Enabled)))
            {
                parameters["enabled"] = Enabled.Value;
            }
            if (IsBound(nameof(Fall)))
            {
                parameters["fall"] = Fall.Value;
            }
            if (IsBound(nameof(HealthCheckEnabled)))
            {
                parameters["health_check_enabled"] = HealthCheckEnabled.Value;
            }
            if (IsBound(nameof(HealthCheckType)))
            {
                parameters["health_check_type"] = HealthCheckType;
            }
            if (IsBound(nameof(Interval)))
            {
                parameters["interval"] = Interval.Value;
            }
            if (IsBound(nameof(MinFreeCpu)))
            {
                parameters["min_free_cpu"] = MinFreeCpu.Value;
            }
            if (IsBound(nameof(MinFreeMem)))
            {
                parameters["min_free_mem"] = MinFreeMem.Value;
            }
            if (IsBound(nameof(Priority)))
            {
                parameters["priority"] = Priority.Value;
            }
            if (IsBound(nameof(RemotePath)))
            {
                parameters["remote_path"] = RemotePath;
            }
            if (IsBound(nameof(Rise)))
            {
                parameters["rise"] = Rise.Value;
            }
            if (IsBound(nameof(CanaryFilePath)))
            {
                parameters["canary_file_path"] = CanaryFilePath;
            }
            if (IsBound(nameof(RemoteServerId)))
            {
                parameters["remote_server_id"] = RemoteServerId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Remote Mount Backend.
    /// </summary>
    [Cmdlet("Remove", "FilesRemoteMountBackend", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesRemoteMountBackendCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Remote Mount Backend ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The remote mount backend to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteMountBackend InputObject { get; set; }

        /// <summary>Delete Remote Mount Backend.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Remote mount backend {id}"))
            {
                return;
            }
            Run(connection, token => RemoteMountBackendBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
