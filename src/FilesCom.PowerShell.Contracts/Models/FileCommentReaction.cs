using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A FileCommentReaction is a reaction that is attached to a comment on a file.
    /// </summary>
    public sealed class FileCommentReaction : FilesObject
    {
        internal FileCommentReaction()
        {
        }

        /// <summary>
        /// Reaction ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Emoji used in the reaction.
        /// </summary>
        public string Emoji { get; internal set; }
    }
}
