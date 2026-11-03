using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A UserSftpClientUse is a way to see the exact set of SFTP clients used by a given user.
    /// </summary>
    public sealed class UserSftpClientUse : FilesObject
    {
        internal UserSftpClientUse()
        {
        }

        /// <summary>
        /// UserSftpClientUse ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// The SFTP client used
        /// </summary>
        public string SftpClient { get; internal set; }

        /// <summary>
        /// The earliest recorded use of this SFTP client (for this user)
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// The most recent use of this SFTP client (for this user)
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        /// <summary>
        /// ID of the user who performed this access
        /// </summary>
        public long? UserId { get; internal set; }
    }
}
