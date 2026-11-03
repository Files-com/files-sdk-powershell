using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A GPGKey object on Files.com is used to securely store both the private and public keys associated with a GPG (GNU Privacy Guard) encryption key pair.
    /// </summary>
    public sealed class GpgKey : FilesObject
    {
        internal GpgKey()
        {
        }

        /// <summary>
        /// GPG key ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID (0 for default workspace).
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// GPG key expiration date.
        /// </summary>
        public DateTime? ExpiresAt { get; internal set; }

        /// <summary>
        /// GPG key name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Partner ID who owns this GPG Key, if applicable.
        /// </summary>
        public long? PartnerId { get; internal set; }

        /// <summary>
        /// Name of the Partner who owns this GPG Key, if applicable.
        /// </summary>
        public string PartnerName { get; internal set; }

        /// <summary>
        /// User ID who owns this GPG Key, if applicable.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// MD5 hash of the GPG public key
        /// </summary>
        public string PublicKeyMd5 { get; internal set; }

        /// <summary>
        /// MD5 hash of the GPG private key.
        /// </summary>
        public string PrivateKeyMd5 { get; internal set; }

        /// <summary>
        /// GPG public key
        /// </summary>
        public string GeneratedPublicKey { get; internal set; }

        /// <summary>
        /// GPG private key.
        /// </summary>
        public FilesSecret GeneratedPrivateKey { get; internal set; }

        /// <summary>
        /// GPG private key password. Only required for password protected keys.
        /// </summary>
        public string PrivateKeyPasswordMd5 { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(GpgKey source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            ExpiresAt = source.ExpiresAt;
            Name = source.Name;
            PartnerId = source.PartnerId;
            PartnerName = source.PartnerName;
            UserId = source.UserId;
            PublicKeyMd5 = source.PublicKeyMd5;
            PrivateKeyMd5 = source.PrivateKeyMd5;
            GeneratedPublicKey = source.GeneratedPublicKey;
            GeneratedPrivateKey = source.GeneratedPrivateKey;
            PrivateKeyPasswordMd5 = source.PrivateKeyPasswordMd5;
        }
    }
}
