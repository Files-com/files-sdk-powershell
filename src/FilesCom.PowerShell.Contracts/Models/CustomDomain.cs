using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A CustomDomain object represents an additional customer-owned domain that routes to a Files.com site.
    /// </summary>
    public sealed class CustomDomain : FilesObject
    {
        internal CustomDomain()
        {
        }

        /// <summary>
        /// Custom Domain ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID (0 for the default workspace).
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Allow all workspaces to use this default-workspace Custom Domain.
        /// </summary>
        public bool AvailableToAllWorkspaces { get; internal set; }

        /// <summary>
        /// Eligible public IP addresses for Remote Server outbound connections. Empty when this Custom Domain is not eligible for outbound selection.
        /// </summary>
        public string[] OutboundIpAddresses { get; internal set; }

        /// <summary>
        /// Customer-owned domain name.
        /// </summary>
        public string Domain { get; internal set; }

        /// <summary>
        /// Where this custom domain routes. Can be `site_alias`, `public_hosting`, `s3_endpoint`, or `unassigned` (not routing traffic). Set to `unassigned` automatically when a bound `public_hosting` folder behavior is deleted, and can be set manually via the API for any reason.
        /// </summary>
        public string Destination { get; internal set; }

        /// <summary>
        /// Current DNS verification status.
        /// </summary>
        public string DnsStatus { get; internal set; }

        /// <summary>
        /// Current SSL certificate ID.
        /// </summary>
        public long? SslCertificateId { get; internal set; }

        /// <summary>
        /// Is this domain's SSL certificate automatically managed and renewed by Files.com?
        /// </summary>
        public bool BrickManaged { get; internal set; }

        /// <summary>
        /// Public Hosting behavior ID when this domain routes to a specific Public Hosting behavior.  Preserved as historical context when `destination` becomes `unassigned`.
        /// </summary>
        public long? FolderBehaviorId { get; internal set; }

        /// <summary>
        /// Dedicated public IP addresses allocated to this Custom Domain.
        /// </summary>
        public string[] IpAddresses { get; internal set; }

        /// <summary>
        /// When this Custom Domain was created.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// When this Custom Domain was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(CustomDomain source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            AvailableToAllWorkspaces = source.AvailableToAllWorkspaces;
            OutboundIpAddresses = source.OutboundIpAddresses;
            Domain = source.Domain;
            Destination = source.Destination;
            DnsStatus = source.DnsStatus;
            SslCertificateId = source.SslCertificateId;
            BrickManaged = source.BrickManaged;
            FolderBehaviorId = source.FolderBehaviorId;
            IpAddresses = source.IpAddresses;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
