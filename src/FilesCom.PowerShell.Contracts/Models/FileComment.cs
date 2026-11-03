using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A FileComment is a comment attached to a file by a user.
    /// </summary>
    public sealed class FileComment : FilesObject
    {
        internal FileComment()
        {
        }

        /// <summary>
        /// File Comment ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Comment body.
        /// </summary>
        public string Body { get; internal set; }

        /// <summary>
        /// Reactions to this comment.
        /// </summary>
        public Models.FileCommentReaction[] Reactions { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(FileComment source)
        {
            Id = source.Id;
            Body = source.Body;
            Reactions = source.Reactions;
        }
    }
}
