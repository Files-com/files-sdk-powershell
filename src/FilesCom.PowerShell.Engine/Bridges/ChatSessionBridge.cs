using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ChatSession operations with the SDK and projects the SDK's ChatSession into the module's.
    internal static class ChatSessionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ChatSession Project(Sdk.ChatSession source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ChatSession
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Title = source.Title,
                UserId = source.UserId,
                AiTaskId = source.AiTaskId,
                WorkspaceId = source.WorkspaceId,
                LastActiveAt = Values.Utc(source.LastActiveAt),
                CreatedAt = Values.Utc(source.CreatedAt),
                Messages = Values.Array(source.Messages, item => ChatMessageBridge.Project(item, connection, options)),
            };
        }

        internal static FilesPageSource<Models.ChatSession> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ChatSessions.List(parameters, requestOptions), source => ChatSessionBridge.Project(source, connection, options));
        }

        internal static Task<Models.ChatSession> FindAsync(FilesConnection connection, string id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ChatSessions.FindAsync(id, parameters, requestOptions, token), source => ChatSessionBridge.Project(source, connection, options));
        }
    }
}
