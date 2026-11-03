using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A ShareGroup is a way for you to store and name groups of email contacts to be used for sending share and inbox invitations.
    /// </summary>
    public sealed class ShareGroup : FilesObject
    {
        internal ShareGroup()
        {
        }

        /// <summary>
        /// Share Group ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Name of the share group
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Additional notes of the share group
        /// </summary>
        public string Notes { get; internal set; }

        /// <summary>
        /// Owner User ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// A list of share group members
        /// </summary>
        public Models.ShareGroupMember[] Members { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(ShareGroup source)
        {
            Id = source.Id;
            Name = source.Name;
            Notes = source.Notes;
            UserId = source.UserId;
            Members = source.Members;
        }
    }
}
