using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An ExavaultApiRequestLog is an audit log for monitoring and troubleshooting requests to the ExaVault API.
    /// </summary>
    public sealed class ExavaultApiRequestLog : FilesObject
    {
        internal ExavaultApiRequestLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// Name of API Endpoint
        /// </summary>
        public string Endpoint { get; internal set; }

        /// <summary>
        /// Exavault API Version
        /// </summary>
        public double? Version { get; internal set; }

        /// <summary>
        /// IP of requesting client
        /// </summary>
        public string RequestIp { get; internal set; }

        /// <summary>
        /// HTTP Method
        /// </summary>
        public string RequestMethod { get; internal set; }

        /// <summary>
        /// Error type, if applicable
        /// </summary>
        public string ErrorType { get; internal set; }

        /// <summary>
        /// Error message, if applicable
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// User-Agent
        /// </summary>
        public string UserAgent { get; internal set; }

        /// <summary>
        /// HTTP Response Code
        /// </summary>
        public long? ResponseCode { get; internal set; }

        /// <summary>
        /// `false` if HTTP Response Code is 4xx or 5xx
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// Duration (in milliseconds)
        /// </summary>
        public long? DurationMs { get; internal set; }

        /// <summary>
        /// Start Time of Action
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
