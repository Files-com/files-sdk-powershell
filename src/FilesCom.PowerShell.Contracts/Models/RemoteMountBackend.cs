using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Remote Mount Backend is used to provide high availability for a Remote Server Mount Folder Behavior.
    /// </summary>
    public sealed class RemoteMountBackend : FilesObject
    {
        internal RemoteMountBackend()
        {
        }

        /// <summary>
        /// Path to the canary file used for health checks.
        /// </summary>
        public string CanaryFilePath { get; internal set; }

        /// <summary>
        /// True if this backend is enabled.
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Number of consecutive failures before considering the backend unhealthy.
        /// </summary>
        public long? Fall { get; internal set; }

        /// <summary>
        /// True if health checks are enabled for this backend.
        /// </summary>
        public bool HealthCheckEnabled { get; internal set; }

        /// <summary>
        /// Array of recent health check results.
        /// </summary>
        public object[] HealthCheckResults { get; internal set; }

        /// <summary>
        /// Type of health check to perform.
        /// </summary>
        public string HealthCheckType { get; internal set; }

        /// <summary>
        /// Unique identifier for this backend.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Interval in seconds between health checks.
        /// </summary>
        public long? Interval { get; internal set; }

        /// <summary>
        /// Minimum free CPU percentage required for this backend to be considered healthy.
        /// </summary>
        public decimal? MinFreeCpu { get; internal set; }

        /// <summary>
        /// Minimum free memory percentage required for this backend to be considered healthy.
        /// </summary>
        public decimal? MinFreeMem { get; internal set; }

        /// <summary>
        /// Priority of this backend.
        /// </summary>
        public long? Priority { get; internal set; }

        /// <summary>
        /// Path on the remote server to treat as the root of this mount.
        /// </summary>
        public string RemotePath { get; internal set; }

        /// <summary>
        /// The remote server that this backend is associated with.
        /// </summary>
        public long? RemoteServerId { get; internal set; }

        /// <summary>
        /// The mount ID of the Remote Server Mount that this backend is associated with.
        /// </summary>
        public long? RemoteServerMountId { get; internal set; }

        /// <summary>
        /// Number of consecutive successes before considering the backend healthy.
        /// </summary>
        public long? Rise { get; internal set; }

        /// <summary>
        /// Status of this backend.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// True if this backend is undergoing maintenance.
        /// </summary>
        public bool UndergoingMaintenance { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(RemoteMountBackend source)
        {
            CanaryFilePath = source.CanaryFilePath;
            Enabled = source.Enabled;
            Fall = source.Fall;
            HealthCheckEnabled = source.HealthCheckEnabled;
            HealthCheckResults = source.HealthCheckResults;
            HealthCheckType = source.HealthCheckType;
            Id = source.Id;
            Interval = source.Interval;
            MinFreeCpu = source.MinFreeCpu;
            MinFreeMem = source.MinFreeMem;
            Priority = source.Priority;
            RemotePath = source.RemotePath;
            RemoteServerId = source.RemoteServerId;
            RemoteServerMountId = source.RemoteServerMountId;
            Rise = source.Rise;
            Status = source.Status;
            UndergoingMaintenance = source.UndergoingMaintenance;
        }
    }
}
