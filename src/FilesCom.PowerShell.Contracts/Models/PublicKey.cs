using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A PublicKey is used to authenticate to Files.com via SFTP (SSH File Transfer Protocol).
    /// </summary>
    public sealed class PublicKey : FilesObject
    {
        internal PublicKey()
        {
        }

        /// <summary>
        /// Public key ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID (0 for default workspace).
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Public key title
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Public key created at date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Public key expiration date/time
        /// </summary>
        public DateTime? ExpiresAt { get; internal set; }

        /// <summary>
        /// Is this public key expired?
        /// </summary>
        public bool Expired { get; internal set; }

        /// <summary>
        /// Public key fingerprint (MD5)
        /// </summary>
        public string Fingerprint { get; internal set; }

        /// <summary>
        /// Public key fingerprint (SHA256)
        /// </summary>
        public string FingerprintSha256 { get; internal set; }

        /// <summary>
        /// Only returned when generating keys. Can be invalid, not_generated, generating, complete
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Key's most recent login time via SFTP
        /// </summary>
        public DateTime? LastLoginAt { get; internal set; }

        /// <summary>
        /// Only returned when generating keys. Private key generated for the user.
        /// </summary>
        public FilesSecret GeneratedPrivateKey { get; internal set; }

        /// <summary>
        /// Only returned when generating keys. Public key generated for the user.
        /// </summary>
        public string GeneratedPublicKey { get; internal set; }

        /// <summary>
        /// Username of the user this public key is associated with
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// User ID this public key is associated with
        /// </summary>
        public long? UserId { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(PublicKey source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Title = source.Title;
            CreatedAt = source.CreatedAt;
            ExpiresAt = source.ExpiresAt;
            Expired = source.Expired;
            Fingerprint = source.Fingerprint;
            FingerprintSha256 = source.FingerprintSha256;
            Status = source.Status;
            LastLoginAt = source.LastLoginAt;
            GeneratedPrivateKey = source.GeneratedPrivateKey;
            GeneratedPublicKey = source.GeneratedPublicKey;
            Username = source.Username;
            UserId = source.UserId;
        }
    }
}
