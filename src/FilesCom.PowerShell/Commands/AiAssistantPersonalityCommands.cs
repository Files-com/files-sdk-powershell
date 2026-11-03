using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Ai Assistant Personalities, or get one ai assistant personality by Id.
    /// </summary>
    [Cmdlet("Get", "FilesAiAssistantPersonality", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.AiAssistantPersonality), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.AiAssistantPersonality>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.AiAssistantPersonality), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesAiAssistantPersonalityCommand : FilesListCmdlet<Models.AiAssistantPersonality>
    {
        /// <summary>
        /// Used for pagination.  When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`.  Send one of those cursor value here to resume an existing list from the next available record.  Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Cursor { get; set; }

        /// <summary>
        /// Number of records to show per page.  (Max: 10000, 1,000 or less is recommended).
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PerPage { get; set; }

        /// <summary>
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id` and `id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Ai Assistant Personality ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Ai Assistant Personalities, or get one ai assistant personality by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.AiAssistantPersonality result = Run(connection, token => AiAssistantPersonalityBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => AiAssistantPersonalityBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Cursor)))
            {
                parameters["cursor"] = Cursor;
            }
            if (IsBound(nameof(PerPage)))
            {
                parameters["per_page"] = PerPage.Value;
            }
            if (IsBound(nameof(SortBy)))
            {
                parameters["sort_by"] = RequestValues.QueryObject(SortBy, "SortBy");
            }
            if (IsBound(nameof(Filter)))
            {
                parameters["filter"] = RequestValues.QueryObject(Filter, "Filter");
            }
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Ai Assistant Personality.
    /// </summary>
    [Cmdlet("New", "FilesAiAssistantPersonality", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.AiAssistantPersonality))]
    public sealed class NewFilesAiAssistantPersonalityCommand : FilesApiCmdlet
    {
        /// <summary>
        /// If true, this default-workspace personality can apply to users in all workspaces.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ApplyToAllWorkspaces { get; set; }

        /// <summary>
        /// AI Assistant Personality name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// System prompt injected into the in-app AI Assistant.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string SystemPrompt { get; set; }

        /// <summary>
        /// Whether this personality is the default personality for the Workspace.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UseByDefault { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Ai Assistant Personality.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Ai assistant personality '{Name}'" : "Ai assistant personality"))
            {
                return;
            }
            Models.AiAssistantPersonality result = Run(connection, token => AiAssistantPersonalityBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(ApplyToAllWorkspaces)))
            {
                parameters["apply_to_all_workspaces"] = ApplyToAllWorkspaces.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(SystemPrompt)))
            {
                parameters["system_prompt"] = SystemPrompt;
            }
            if (IsBound(nameof(UseByDefault)))
            {
                parameters["use_by_default"] = UseByDefault.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Ai Assistant Personality.
    /// </summary>
    [Cmdlet("Set", "FilesAiAssistantPersonality", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.AiAssistantPersonality), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesAiAssistantPersonalityCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Ai Assistant Personality ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// If true, this default-workspace personality can apply to users in all workspaces.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ApplyToAllWorkspaces { get; set; }

        /// <summary>
        /// AI Assistant Personality name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// System prompt injected into the in-app AI Assistant.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string SystemPrompt { get; set; }

        /// <summary>
        /// Whether this personality is the default personality for the Workspace.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? UseByDefault { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// The ai assistant personality to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.AiAssistantPersonality InputObject { get; set; }

        /// <summary>Update Ai Assistant Personality.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Ai assistant personality {id}"))
            {
                return;
            }
            Models.AiAssistantPersonality result = Run(connection, token => AiAssistantPersonalityBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(ApplyToAllWorkspaces)))
            {
                parameters["apply_to_all_workspaces"] = ApplyToAllWorkspaces.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(SystemPrompt)))
            {
                parameters["system_prompt"] = SystemPrompt;
            }
            if (IsBound(nameof(UseByDefault)))
            {
                parameters["use_by_default"] = UseByDefault.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Ai Assistant Personality.
    /// </summary>
    [Cmdlet("Remove", "FilesAiAssistantPersonality", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesAiAssistantPersonalityCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Ai Assistant Personality ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The ai assistant personality to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.AiAssistantPersonality InputObject { get; set; }

        /// <summary>Delete Ai Assistant Personality.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Ai assistant personality {id}"))
            {
                return;
            }
            Run(connection, token => AiAssistantPersonalityBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
