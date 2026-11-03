using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An EventDeliveryAttempt records delivery state for an EventRecord and EventTarget.
    /// </summary>
    public sealed class EventDeliveryAttempt : FilesObject
    {
        internal EventDeliveryAttempt()
        {
        }

        /// <summary>
        /// Event Delivery Attempt ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Event Record ID
        /// </summary>
        public long? EventRecordId { get; internal set; }

        /// <summary>
        /// Event Subscription ID
        /// </summary>
        public long? EventSubscriptionId { get; internal set; }

        /// <summary>
        /// Event Target ID
        /// </summary>
        public long? EventTargetId { get; internal set; }

        /// <summary>
        /// Shared summary ID when this event is delivered in a Slack or Teams subfolder summary. All events in a summary share its delivery outcome.
        /// </summary>
        public long? ActivityNotificationBatchId { get; internal set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace or site-wide.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Delivery status.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Number of delivery attempts made.
        /// </summary>
        public long? AttemptNumber { get; internal set; }

        /// <summary>
        /// HTTP response code, if applicable.
        /// </summary>
        public long? ResponseCode { get; internal set; }

        /// <summary>
        /// Delivery error message, if applicable.
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        /// Delivery response body, if applicable.
        /// </summary>
        public string ResponseBody { get; internal set; }

        /// <summary>
        /// Delivery latency in milliseconds.
        /// </summary>
        public long? LatencyMs { get; internal set; }

        /// <summary>
        /// Successful delivery date/time.
        /// </summary>
        public DateTime? DeliveredAt { get; internal set; }

        /// <summary>
        /// Most recent attempt date/time.
        /// </summary>
        public DateTime? LastAttemptedAt { get; internal set; }

        /// <summary>
        /// Next scheduled attempt date/time.
        /// </summary>
        public DateTime? NextAttemptAt { get; internal set; }

        /// <summary>
        /// Delivery Attempt create date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
