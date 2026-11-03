using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A UserCipherUse is a way to see the exact set of encryption ciphers and protocols used by a given user.
    /// </summary>
    public sealed class UserCipherUse : FilesObject
    {
        internal UserCipherUse()
        {
        }

        /// <summary>
        /// UserCipherUse ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the user who performed this access
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Username of the user who performed this access
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// The protocol and cipher employed
        /// </summary>
        public string ProtocolCipher { get; internal set; }

        /// <summary>
        /// The earliest recorded use of this combination of interface and protocol and cipher (for this user)
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Is this cipher considered insecure?
        /// </summary>
        public bool Insecure { get; internal set; }

        /// <summary>
        /// The interface accessed
        /// </summary>
        public string Interface { get; internal set; }

        /// <summary>
        /// The most recent use of this combination of interface and protocol and cipher (for this user)
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }
    }
}
