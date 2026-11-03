using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AS2Station is a remote AS2 server that can send data into Files.com and receive data from Files.com.
    /// </summary>
    public sealed class As2Station : FilesObject
    {
        internal As2Station()
        {
        }

        /// <summary>
        /// Id of the AS2 Station.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the Workspace associated with this AS2 Station.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// The station's formal AS2 name, unique across all Workspaces on the Site.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Public URI for sending AS2 message to.
        /// </summary>
        public string Uri { get; internal set; }

        /// <summary>
        /// The station's AS2 domain name.
        /// </summary>
        public string Domain { get; internal set; }

        /// <summary>
        /// Serial of public certificate used for message security in hex format.
        /// </summary>
        public string HexPublicCertificateSerial { get; internal set; }

        /// <summary>
        /// MD5 hash of public certificate used for message security.
        /// </summary>
        public string PublicCertificateMd5 { get; internal set; }

        /// <summary>
        /// Public certificate used for message security.
        /// </summary>
        public string PublicCertificate { get; internal set; }

        /// <summary>
        /// MD5 hash of private key used for message security.
        /// </summary>
        public string PrivateKeyMd5 { get; internal set; }

        /// <summary>
        /// Subject of public certificate used for message security.
        /// </summary>
        public string PublicCertificateSubject { get; internal set; }

        /// <summary>
        /// Issuer of public certificate used for message security.
        /// </summary>
        public string PublicCertificateIssuer { get; internal set; }

        /// <summary>
        /// Serial of public certificate used for message security.
        /// </summary>
        public string PublicCertificateSerial { get; internal set; }

        /// <summary>
        /// Not before value of public certificate used for message security.
        /// </summary>
        public string PublicCertificateNotBefore { get; internal set; }

        /// <summary>
        /// Not after value of public certificate used for message security.
        /// </summary>
        public string PublicCertificateNotAfter { get; internal set; }

        /// <summary>
        /// MD5 hash of private key password used for message security.
        /// </summary>
        public string PrivateKeyPasswordMd5 { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(As2Station source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Name = source.Name;
            Uri = source.Uri;
            Domain = source.Domain;
            HexPublicCertificateSerial = source.HexPublicCertificateSerial;
            PublicCertificateMd5 = source.PublicCertificateMd5;
            PublicCertificate = source.PublicCertificate;
            PrivateKeyMd5 = source.PrivateKeyMd5;
            PublicCertificateSubject = source.PublicCertificateSubject;
            PublicCertificateIssuer = source.PublicCertificateIssuer;
            PublicCertificateSerial = source.PublicCertificateSerial;
            PublicCertificateNotBefore = source.PublicCertificateNotBefore;
            PublicCertificateNotAfter = source.PublicCertificateNotAfter;
            PrivateKeyPasswordMd5 = source.PrivateKeyPasswordMd5;
        }
    }
}
