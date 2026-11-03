using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com partner connection.
    /// </summary>
    public sealed class PartnerConnection : FilesObject
    {
        internal PartnerConnection()
        {
        }

        /// <summary>
        /// Relationship ID used with DELETE /partner_sites/:id to disconnect.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// This Partner's role in this connection. A host shares local files with the connected site; a guest accesses files shared by the connected site.
        /// </summary>
        public string Role { get; internal set; }

        /// <summary>
        /// ID of the connected site.
        /// </summary>
        public long? SiteId { get; internal set; }

        /// <summary>
        /// Name of the connected site.
        /// </summary>
        public string SiteName { get; internal set; }

        /// <summary>
        /// File API path to the connected Host's mount on this site when role is guest. Null when role is host. File access remains subject to the caller's permissions and the Host Partner's grants.
        /// </summary>
        public string MountPath { get; internal set; }
    }
}
