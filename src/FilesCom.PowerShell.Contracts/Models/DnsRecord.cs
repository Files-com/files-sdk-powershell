using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A DNSRecord is a custom DNS configuration record for a Site.
    /// </summary>
    public sealed class DnsRecord : FilesObject
    {
        internal DnsRecord()
        {
        }

        /// <summary>
        /// Unique label for DNS record; used by Zapier and other integrations.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// DNS record domain name
        /// </summary>
        public string Domain { get; internal set; }

        /// <summary>
        /// DNS record type
        /// </summary>
        public string Rrtype { get; internal set; }

        /// <summary>
        /// DNS record value
        /// </summary>
        public string Value { get; internal set; }
    }
}
