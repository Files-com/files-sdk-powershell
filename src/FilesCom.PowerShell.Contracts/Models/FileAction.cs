using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A FileAction is an operation status for a copy, move or delete.
    /// </summary>
    public sealed class FileAction : FilesObject
    {
        internal FileAction()
        {
        }

        /// <summary>
        /// Status of file operation.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// If status is pending, this is the id of the File Migration to check for status updates.  Delete actions include it whenever the deletion runs as a File Migration, including while it is processing.
        /// </summary>
        public long? FileMigrationId { get; internal set; }
    }
}
