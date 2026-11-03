using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A SiteSubdomainRedirect object represents an old Files.com subdomain that continues to work after the site's Files.com subdomain changes.
    /// </summary>
    public sealed class SiteSubdomainRedirect : FilesObject
    {
        internal SiteSubdomainRedirect()
        {
        }

        /// <summary>
        /// Site subdomain redirect ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Files.com subdomain that continues to route to the current site subdomain.
        /// </summary>
        public string Subdomain { get; internal set; }

        /// <summary>
        /// When this redirect was created.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// When this redirect was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }
    }
}
