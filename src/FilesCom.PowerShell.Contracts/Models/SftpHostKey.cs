using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An SFTP Host Key is a cryptographic key used to verify the identity of the server during an SFTP connection.
    /// </summary>
    public sealed class SftpHostKey : FilesObject
    {
        internal SftpHostKey()
        {
        }

        /// <summary>
        /// If true, use this SFTP Host Key.
        /// </summary>
        public bool Active { get; internal set; }

        /// <summary>
        /// Custom Domain ID. If set, this key is used only for that Custom Domain.
        /// </summary>
        public long? CustomDomainId { get; internal set; }

        /// <summary>
        /// SFTP Host Key ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// The friendly name of this SFTP Host Key.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// SSH key type
        /// </summary>
        public string KeyType { get; internal set; }

        /// <summary>
        /// MD5 Fingerprint of the public key
        /// </summary>
        public string FingerprintMd5 { get; internal set; }

        /// <summary>
        /// SHA256 Fingerprint of the public key
        /// </summary>
        public string FingerprintSha256 { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(SftpHostKey source)
        {
            Active = source.Active;
            CustomDomainId = source.CustomDomainId;
            Id = source.Id;
            Name = source.Name;
            KeyType = source.KeyType;
            FingerprintMd5 = source.FingerprintMd5;
            FingerprintSha256 = source.FingerprintSha256;
        }
    }
}
