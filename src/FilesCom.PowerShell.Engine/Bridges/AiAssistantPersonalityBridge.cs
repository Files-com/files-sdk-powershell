using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs AiAssistantPersonality operations with the SDK and projects the SDK's AiAssistantPersonality into the module's.
    internal static class AiAssistantPersonalityBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AiAssistantPersonality Project(Sdk.AiAssistantPersonality source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AiAssistantPersonality
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Name = source.Name,
                SystemPrompt = source.SystemPrompt,
                UseByDefault = source.UseByDefault,
                ApplyToAllWorkspaces = source.ApplyToAllWorkspaces,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.AiAssistantPersonality> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.AiAssistantPersonalities.List(parameters, requestOptions), source => AiAssistantPersonalityBridge.Project(source, connection, options));
        }

        internal static Task<Models.AiAssistantPersonality> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.AiAssistantPersonalities.FindAsync(id, parameters, requestOptions, token), source => AiAssistantPersonalityBridge.Project(source, connection, options));
        }

        internal static Task<Models.AiAssistantPersonality> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.AiAssistantPersonalities.CreateAsync(parameters, requestOptions, token), source => AiAssistantPersonalityBridge.Project(source, connection, options));
        }

        internal static Task<Models.AiAssistantPersonality> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.AiAssistantPersonality model ? model.UpdateAsync(parameters, token) : client.AiAssistantPersonalities.UpdateAsync(id, parameters, requestOptions, token), source => AiAssistantPersonalityBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.AiAssistantPersonality model ? model.DeleteAsync(parameters, token) : client.AiAssistantPersonalities.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
