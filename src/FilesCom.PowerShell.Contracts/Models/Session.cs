using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Session is an operation that allows you to make further API calls using our REST API or SDKs as a specific user.
    /// </summary>
    public sealed class Session : FilesObject
    {
        internal Session()
        {
        }

        /// <summary>
        /// Session ID
        /// </summary>
        public FilesSecret Id { get; internal set; }

        /// <summary>
        /// Session language
        /// </summary>
        public string Language { get; internal set; }

        /// <summary>
        /// Is this session read only?
        /// </summary>
        public bool ReadOnly { get; internal set; }

        /// <summary>
        /// Are insecure SFTP ciphers allowed for this user? (If this is set to true, the site administrator has signaled that it is ok to use less secure SSH ciphers for this user.)
        /// </summary>
        public bool SftpInsecureCiphers { get; internal set; }
    }
}
