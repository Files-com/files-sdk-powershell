using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Certificate is a way to store your custom SSL certificate for your Files.com site.
    /// </summary>
    public sealed class Certificate : FilesObject
    {
        internal Certificate()
        {
        }

        /// <summary>
        /// Certificate ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Descriptive name of certificate
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Custom Domain that owns this certificate, including while the certificate is inactive.
        /// </summary>
        public long? CustomDomainId { get; internal set; }

        /// <summary>
        /// Full text of SSL certificate
        /// </summary>
        public string CertificateType { get; internal set; }

        /// <summary>
        /// Certificate created at
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Certificate status. (One of `Request Generated`, `New`, `Active`, `Active/Expired`, `Expired`, or `Available`)
        /// </summary>
        public string DisplayStatus { get; internal set; }

        /// <summary>
        /// Domains on this certificate other than files.com domains
        /// </summary>
        public string[] Domains { get; internal set; }

        /// <summary>
        /// Certificate expiration date/time
        /// </summary>
        public DateTime? ExpiresAt { get; internal set; }

        /// <summary>
        /// Is this certificate automatically managed and renewed by Files.com?
        /// </summary>
        public bool BrickManaged { get; internal set; }

        /// <summary>
        /// Intermediate certificates
        /// </summary>
        public string Intermediates { get; internal set; }

        /// <summary>
        /// A list of IP addresses associated with this SSL Certificate
        /// </summary>
        public string[] IpAddresses { get; internal set; }

        /// <summary>
        /// X509 issuer
        /// </summary>
        public string Issuer { get; internal set; }

        /// <summary>
        /// Type of key
        /// </summary>
        public string KeyType { get; internal set; }

        /// <summary>
        /// Certificate signing request text
        /// </summary>
        public string Request { get; internal set; }

        /// <summary>
        /// Certificate status (Request Generated, New, Active, Active/Expired, Expired, or Available)
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// X509 Subject name
        /// </summary>
        public string Subject { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Certificate source)
        {
            Id = source.Id;
            Name = source.Name;
            CustomDomainId = source.CustomDomainId;
            CertificateType = source.CertificateType;
            CreatedAt = source.CreatedAt;
            DisplayStatus = source.DisplayStatus;
            Domains = source.Domains;
            ExpiresAt = source.ExpiresAt;
            BrickManaged = source.BrickManaged;
            Intermediates = source.Intermediates;
            IpAddresses = source.IpAddresses;
            Issuer = source.Issuer;
            KeyType = source.KeyType;
            Request = source.Request;
            Status = source.Status;
            Subject = source.Subject;
        }
    }
}
