using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An InboundS3Log is an audit log for monitoring and troubleshooting S3-compatible API requests to your Files.com site.
    /// </summary>
    public sealed class InboundS3Log : FilesObject
    {
        internal InboundS3Log()
        {
        }

        /// <summary>
        /// Request Path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Client IP
        /// </summary>
        public string ClientIp { get; internal set; }

        /// <summary>
        /// S3 Operation Type
        /// </summary>
        public string Operation { get; internal set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// AWS Access Key ID
        /// </summary>
        public string AwsAccessKey { get; internal set; }

        /// <summary>
        /// Error message, if applicable
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Error type, if applicable
        /// </summary>
        public string ErrorType { get; internal set; }

        /// <summary>
        /// Duration (in milliseconds)
        /// </summary>
        public long? DurationMs { get; internal set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string RequestId { get; internal set; }

        /// <summary>
        /// User Agent
        /// </summary>
        public string UserAgent { get; internal set; }

        /// <summary>
        /// Start Time of Request
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
