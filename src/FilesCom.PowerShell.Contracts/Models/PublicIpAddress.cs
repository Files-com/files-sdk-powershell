using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A PublicIPAddress is an IP address that `app.files.com` (or `*.files.com`) may potentially resolve to over the next 30 days.
    /// </summary>
    public sealed class PublicIpAddress : FilesObject
    {
        internal PublicIpAddress()
        {
        }

        /// <summary>
        /// The public IP address.
        /// </summary>
        public string IpAddress { get; internal set; }

        /// <summary>
        /// The name of the frontend server.
        /// </summary>
        public string ServerName { get; internal set; }

        /// <summary>
        /// FtpEnabled
        /// </summary>
        public bool FtpEnabled { get; internal set; }

        /// <summary>
        /// SftpEnabled
        /// </summary>
        public bool SftpEnabled { get; internal set; }
    }
}
