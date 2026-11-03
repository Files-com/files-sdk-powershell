using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A PartnerSiteRequest represents a request for a Guest Partner to add their Files.com Site to their Partnership with the Host Partner.
    /// </summary>
    public sealed class PartnerSiteRequest : FilesObject
    {
        internal PartnerSiteRequest()
        {
        }

        /// <summary>
        /// Partner Site Request ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Host Partner ID
        /// </summary>
        public long? HostPartnerId { get; internal set; }

        /// <summary>
        /// Guest Site URL
        /// </summary>
        public string GuestSiteUrl { get; internal set; }

        /// <summary>
        /// Request status (pending, approved, rejected)
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Host Site Name
        /// </summary>
        public string HostSiteName { get; internal set; }

        /// <summary>
        /// Pairing key used to approve this request on the Guest Site
        /// </summary>
        public FilesSecret PairingKey { get; internal set; }

        /// <summary>
        /// Request creation date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Request last updated date/time
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }
    }
}
