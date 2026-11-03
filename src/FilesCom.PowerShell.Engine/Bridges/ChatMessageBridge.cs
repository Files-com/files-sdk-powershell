using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's ChatMessage into the module's.
    internal static class ChatMessageBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ChatMessage Project(Sdk.ChatMessage source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ChatMessage
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Role = source.Role,
                Content = source.Content,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }
    }
}
