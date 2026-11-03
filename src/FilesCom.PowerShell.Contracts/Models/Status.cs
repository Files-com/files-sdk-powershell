using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com status.
    /// </summary>
    public sealed class Status : FilesObject
    {
        internal Status()
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
        public string StatusType { get; internal set; }

        /// <summary>
        /// Additional data
        /// </summary>
        public Models.Auto Data { get; internal set; }

        /// <summary>
        /// A list of api errors
        /// </summary>
        public Models.Errors[] Errors { get; internal set; }

        /// <summary>
        /// Required Clickwrap id
        /// </summary>
        public long? ClickwrapId { get; internal set; }

        /// <summary>
        /// Required Clickwrap body
        /// </summary>
        public string ClickwrapBody { get; internal set; }
    }
}
