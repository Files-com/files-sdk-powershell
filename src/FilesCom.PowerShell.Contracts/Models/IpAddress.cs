using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An IPAddress is a record of IP addresses that you can use to automate keeping your firewall's configuration up to date.
    /// </summary>
    public sealed class IpAddress : FilesObject
    {
        internal IpAddress()
        {
        }

        /// <summary>
        /// Unique label for list; used by Zapier and other integrations.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// The object that this public IP address list is associated with.
        /// </summary>
        public string AssociatedWith { get; internal set; }

        /// <summary>
        /// Group ID
        /// </summary>
        public long? GroupId { get; internal set; }

        /// <summary>
        /// A list of IP addresses.
        /// </summary>
        public string[] IpAddresses { get; internal set; }
    }
}
