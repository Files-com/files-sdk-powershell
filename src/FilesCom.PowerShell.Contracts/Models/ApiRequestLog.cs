using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An ApiRequestLog is an audit log for monitoring and troubleshooting requests made via API to your site.
    /// </summary>
    public sealed class ApiRequestLog : FilesObject
    {
        internal ApiRequestLog()
        {
        }

        /// <summary>
        /// Start Time of Action. Deprecrated: Use created_at.
        /// </summary>
        public DateTime? Timestamp { get; internal set; }

        /// <summary>
        /// API Key ID, if applicable
        /// </summary>
        public long? ApiKeyId { get; internal set; }

        /// <summary>
        /// API Key Prefix, if applicable
        /// </summary>
        public string ApiKeyPrefix { get; internal set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// true if this change was performed by a user on a parent site.
        /// </summary>
        public bool UserIsFromParentSite { get; internal set; }

        /// <summary>
        /// API Interface
        /// </summary>
        public string Interface { get; internal set; }

        /// <summary>
        /// HTTP Method
        /// </summary>
        public string RequestMethod { get; internal set; }

        /// <summary>
        /// Request path
        /// </summary>
        public string RequestPath { get; internal set; }

        /// <summary>
        /// IP of requesting client
        /// </summary>
        public string RequestIp { get; internal set; }

        /// <summary>
        /// Hostname the request was sent to
        /// </summary>
        public string RequestHost { get; internal set; }

        /// <summary>
        /// Unique Id of the Request
        /// </summary>
        public string RequestId { get; internal set; }

        /// <summary>
        /// Name of API Endpoint
        /// </summary>
        public string ApiName { get; internal set; }

        /// <summary>
        /// User-Agent
        /// </summary>
        public string UserAgent { get; internal set; }

        /// <summary>
        /// Error type, if applicable
        /// </summary>
        public string ErrorType { get; internal set; }

        /// <summary>
        /// Error message, if applicable
        /// </summary>
        public string ErrorMessage { get; internal set; }

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
        /// User ID of Site Admin user impersonating this user via a Read-Only session.
        /// </summary>
        public long? ImpersonatorUserId { get; internal set; }

        /// <summary>
        /// Start Time of Action
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
