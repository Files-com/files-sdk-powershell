using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A ChatMessage represents one message within a ChatSession, which is a conversation with the Files.com AI Assistant.
    /// </summary>
    public sealed class ChatMessage : FilesObject
    {
        internal ChatMessage()
        {
        }

        /// <summary>
        /// Chat Message ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Message role.
        /// </summary>
        public string Role { get; internal set; }

        /// <summary>
        /// Message content.
        /// </summary>
        public string Content { get; internal set; }

        /// <summary>
        /// Message creation date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
