using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An ActionNotificationExport is an operation that provides access to outgoing webhook logs.
    /// </summary>
    public sealed class ActionNotificationExport : FilesObject
    {
        internal ActionNotificationExport()
        {
        }

        /// <summary>
        /// History Export ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace whose logs are exported. Set to `0` for the default workspace. A null value means a site-wide export.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Version of the underlying records for the export.
        /// </summary>
        public string ExportVersion { get; internal set; }

        /// <summary>
        /// Start date/time of export range.
        /// </summary>
        public DateTime? StartAt { get; internal set; }

        /// <summary>
        /// End date/time of export range.
        /// </summary>
        public DateTime? EndAt { get; internal set; }

        /// <summary>
        /// Status of export.  Valid values: `building`, `ready`, or `failed`
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Return notifications that were triggered by actions on this specific path.
        /// </summary>
        public string QueryPath { get; internal set; }

        /// <summary>
        /// Return notifications that were triggered by actions in this folder.
        /// </summary>
        public string QueryFolder { get; internal set; }

        /// <summary>
        /// Error message associated with the request, if any.
        /// </summary>
        public string QueryMessage { get; internal set; }

        /// <summary>
        /// The HTTP request method used by the webhook.
        /// </summary>
        public string QueryRequestMethod { get; internal set; }

        /// <summary>
        /// The target webhook URL.
        /// </summary>
        public string QueryRequestUrl { get; internal set; }

        /// <summary>
        /// The HTTP status returned from the server in response to the webhook request.
        /// </summary>
        public string QueryStatus { get; internal set; }

        /// <summary>
        /// true if the webhook request succeeded (i.e. returned a 200 or 204 response status). false otherwise.
        /// </summary>
        public bool QuerySuccess { get; internal set; }

        /// <summary>
        /// If `status` is `ready`, this will be a URL where all the results can be downloaded at once as a CSV.
        /// </summary>
        public string ResultsUrl { get; internal set; }
    }
}
