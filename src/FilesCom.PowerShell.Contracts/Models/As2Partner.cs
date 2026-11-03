using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AS2Partner is a counterparty of the Files.com site's AS2 connectivity.
    /// </summary>
    public sealed class As2Partner : FilesObject
    {
        internal As2Partner()
        {
        }

        /// <summary>
        /// ID of the AS2 Partner.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the Workspace associated with this AS2 Partner.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// ID of the AS2 Station associated with this partner.
        /// </summary>
        public long? As2StationId { get; internal set; }

        /// <summary>
        /// The partner's formal AS2 name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Public URI where we will send the AS2 messages (via HTTP/HTTPS).
        /// </summary>
        public string Uri { get; internal set; }

        /// <summary>
        /// Should we require that the remote HTTP server have a valid SSL Certificate for HTTPS? (This only applies to Outgoing AS2 message from Files.com to a Partner.)
        /// </summary>
        public string ServerCertificate { get; internal set; }

        /// <summary>
        /// Username to send to server for HTTP Authentication.
        /// </summary>
        public string HttpAuthUsername { get; internal set; }

        /// <summary>
        /// Additional HTTP Headers for outgoing message sent to this partner.
        /// </summary>
        public object AdditionalHttpHeaders { get; internal set; }

        /// <summary>
        /// Default mime type of the file attached to the encrypted message
        /// </summary>
        public string DefaultMimeType { get; internal set; }

        /// <summary>
        /// How should Files.com evaluate message transfer success based on a partner's MDN response?  This setting does not affect MDN storage; all MDNs received from a partner are always stored. `none`: MDN is stored for informational purposes only, a successful HTTPS transfer is a successful AS2 transfer. `weak`: Inspect the MDN for MIC and Disposition only. `normal`: `weak` plus validate MDN signature matches body, `strict`: `normal` but do not allow signatures from self-signed or incorrectly purposed certificates. `auto`: Automatically set the correct value for this setting based on next mdn received.
        /// </summary>
        public string MdnValidationLevel { get; internal set; }

        /// <summary>
        /// Should Files.com require signatures on incoming AS2 messages?  `normal`: require that incoming messages are signed with a valid matching signature. `none`: Unsigned incoming messages are allowed. `auto`: A one-time starting state. The next message received is always accepted and switches this to `normal` if it carries a valid signature, or `none` if it does not.
        /// </summary>
        public string SignatureValidationLevel { get; internal set; }

        /// <summary>
        /// If `true`, we will use your site's dedicated IPs for all outbound connections to this AS2 Partner.
        /// </summary>
        public bool EnableDedicatedIps { get; internal set; }

        /// <summary>
        /// Serial of public certificate used for message security in hex format.
        /// </summary>
        public string HexPublicCertificateSerial { get; internal set; }

        /// <summary>
        /// Public certificate used for message security.
        /// </summary>
        public string PublicCertificate { get; internal set; }

        /// <summary>
        /// MD5 hash of public certificate used for message security.
        /// </summary>
        public string PublicCertificateMd5 { get; internal set; }

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

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(As2Partner source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            As2StationId = source.As2StationId;
            Name = source.Name;
            Uri = source.Uri;
            ServerCertificate = source.ServerCertificate;
            HttpAuthUsername = source.HttpAuthUsername;
            AdditionalHttpHeaders = source.AdditionalHttpHeaders;
            DefaultMimeType = source.DefaultMimeType;
            MdnValidationLevel = source.MdnValidationLevel;
            SignatureValidationLevel = source.SignatureValidationLevel;
            EnableDedicatedIps = source.EnableDedicatedIps;
            HexPublicCertificateSerial = source.HexPublicCertificateSerial;
            PublicCertificate = source.PublicCertificate;
            PublicCertificateMd5 = source.PublicCertificateMd5;
            PublicCertificateSubject = source.PublicCertificateSubject;
            PublicCertificateIssuer = source.PublicCertificateIssuer;
            PublicCertificateSerial = source.PublicCertificateSerial;
            PublicCertificateNotBefore = source.PublicCertificateNotBefore;
            PublicCertificateNotAfter = source.PublicCertificateNotAfter;
        }
    }
}
