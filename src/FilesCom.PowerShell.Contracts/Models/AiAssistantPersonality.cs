using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AI Assistant Personality defines a system prompt used to customize the in-app AI Assistant.
    /// </summary>
    public sealed class AiAssistantPersonality : FilesObject
    {
        internal AiAssistantPersonality()
        {
        }

        /// <summary>
        /// AI Assistant Personality ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// AI Assistant Personality name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// System prompt injected into the in-app AI Assistant.
        /// </summary>
        public string SystemPrompt { get; internal set; }

        /// <summary>
        /// Whether this personality is the default personality for the Workspace.
        /// </summary>
        public bool UseByDefault { get; internal set; }

        /// <summary>
        /// If true, this default-workspace personality can apply to users in all workspaces.
        /// </summary>
        public bool ApplyToAllWorkspaces { get; internal set; }

        /// <summary>
        /// Creation time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Last update time.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(AiAssistantPersonality source)
        {
            Id = source.Id;
            WorkspaceId = source.WorkspaceId;
            Name = source.Name;
            SystemPrompt = source.SystemPrompt;
            UseByDefault = source.UseByDefault;
            ApplyToAllWorkspaces = source.ApplyToAllWorkspaces;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
