using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs FileCommentReaction operations with the SDK and projects the SDK's FileCommentReaction into the module's.
    internal static class FileCommentReactionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FileCommentReaction Project(Sdk.FileCommentReaction source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FileCommentReaction
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Emoji = source.Emoji,
            };
        }

        internal static Task<Models.FileCommentReaction> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FileCommentReactions.CreateAsync(parameters, requestOptions, token), source => FileCommentReactionBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.FileCommentReaction model ? model.DeleteAsync(parameters, token) : client.FileCommentReactions.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
