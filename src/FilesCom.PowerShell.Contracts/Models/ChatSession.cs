using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A ChatSession represents one conversation with the Files.com AI Assistant.
    /// </summary>
    public sealed class ChatSession : FilesObject
    {
        internal ChatSession()
        {
        }

        /// <summary>
        /// Chat Session ID.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Short AI-generated chat title.
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// User ID.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// AI Task ID. Present when the conversation was started by an AI Task.
        /// </summary>
        public long? AiTaskId { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Most recent chat activity date/time.
        /// </summary>
        public DateTime? LastActiveAt { get; internal set; }

        /// <summary>
        /// Chat session creation date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Visible conversation messages in this chat session. For performance reasons, this is not provided when listing chat sessions.
        /// </summary>
        public Models.ChatMessage[] Messages { get; internal set; }
    }
}
