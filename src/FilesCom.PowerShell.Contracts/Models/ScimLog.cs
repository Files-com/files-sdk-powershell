using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A SCIM log entry represents a single SCIM request made to the system.
    /// </summary>
    public sealed class ScimLog : FilesObject
    {
        internal ScimLog()
        {
        }

        /// <summary>
        /// The unique ID of this SCIM request.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// The date and time when this SCIM request occurred.
        /// </summary>
        public string CreatedAt { get; internal set; }

        /// <summary>
        /// The path portion of the URL requested.
        /// </summary>
        public string RequestPath { get; internal set; }

        /// <summary>
        /// The HTTP method used for this request.
        /// </summary>
        public string RequestMethod { get; internal set; }

        /// <summary>
        /// The HTTP response code returned for this request.
        /// </summary>
        public string HttpResponseCode { get; internal set; }

        /// <summary>
        /// The User-Agent header sent with the request.
        /// </summary>
        public string UserAgent { get; internal set; }

        /// <summary>
        /// The JSON payload sent with the request.
        /// </summary>
        public string RequestJson { get; internal set; }

        /// <summary>
        /// The JSON payload returned in the response.
        /// </summary>
        public string ResponseJson { get; internal set; }
    }
}
