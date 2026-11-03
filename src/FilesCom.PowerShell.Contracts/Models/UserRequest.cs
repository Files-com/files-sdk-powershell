using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A UserRequest is an operation that allows anonymous users to place a request for access on the login screen to the site administrator.
    /// </summary>
    public sealed class UserRequest : FilesObject
    {
        internal UserRequest()
        {
        }

        /// <summary>
        /// ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// User's full name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// Details of the user's request
        /// </summary>
        public string Details { get; internal set; }

        /// <summary>
        /// User's company name
        /// </summary>
        public string Company { get; internal set; }
    }
}
