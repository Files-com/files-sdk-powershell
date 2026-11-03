using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Files.com user additional email recipient.
    /// </summary>
    public sealed class UserAdditionalEmailRecipient : FilesObject
    {
        internal UserAdditionalEmailRecipient()
        {
        }

        /// <summary>
        /// User additional email recipient ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// User ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Workspace ID (0 for default workspace).
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Additional email recipient address
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// Created at date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(UserAdditionalEmailRecipient source)
        {
            Id = source.Id;
            UserId = source.UserId;
            WorkspaceId = source.WorkspaceId;
            Email = source.Email;
            CreatedAt = source.CreatedAt;
        }
    }
}
