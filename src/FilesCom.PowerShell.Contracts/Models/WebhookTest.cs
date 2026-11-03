using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A WebhookTest is an object that can be sent to your Webhook endpoint for connection and response tests.
    /// </summary>
    public sealed class WebhookTest : FilesObject
    {
        internal WebhookTest()
        {
        }

        /// <summary>
        /// Status HTTP code
        /// </summary>
        public long? Code { get; internal set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Status message
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Additional data
        /// </summary>
        public Models.Auto Data { get; internal set; }

        /// <summary>
        /// The success status of the webhook test
        /// </summary>
        public bool Success { get; internal set; }
    }
}
