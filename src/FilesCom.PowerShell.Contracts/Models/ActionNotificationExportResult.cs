using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An ActionNotificationExportResult is a single record containing webhook log information that can be obtained as a part of Action Notification Export request.
    /// </summary>
    public sealed class ActionNotificationExportResult : FilesObject
    {
        internal ActionNotificationExportResult()
        {
        }

        /// <summary>
        /// Notification ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// When the notification was sent.
        /// </summary>
        public long? CreatedAt { get; internal set; }

        /// <summary>
        /// HTTP status code returned in the webhook response.
        /// </summary>
        public long? Status { get; internal set; }

        /// <summary>
        /// A message indicating the overall status of the webhook notification.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// `true` if the webhook succeeded by receiving a 200 or 204 response.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// A JSON-encoded string with headers that were sent with the webhook.
        /// </summary>
        public string RequestHeaders { get; internal set; }

        /// <summary>
        /// The HTTP verb used to perform the webhook.
        /// </summary>
        public string RequestMethod { get; internal set; }

        /// <summary>
        /// The webhook request URL.
        /// </summary>
        public string RequestUrl { get; internal set; }

        /// <summary>
        /// The path to the actual file that triggered this notification. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// The folder associated with the triggering action for this notification.
        /// </summary>
        public string Folder { get; internal set; }
    }
}
