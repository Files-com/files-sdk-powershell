using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs FileComment operations with the SDK and projects the SDK's FileComment into the module's.
    internal static class FileCommentBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FileComment Project(Sdk.FileComment source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FileComment
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Body = source.Body,
                Reactions = Values.Array(source.Reactions, item => FileCommentReactionBridge.Project(item, connection, options)),
            };
        }

        internal static FilesPageSource<Models.FileComment> ListFor(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.FileComments.ListFor(path, parameters, requestOptions), source => FileCommentBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileComment> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FileComments.CreateAsync(parameters, requestOptions, token), source => FileCommentBridge.Project(source, connection, options));
        }

        internal static Task<Models.FileComment> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.FileComment model ? model.UpdateAsync(parameters, token) : client.FileComments.UpdateAsync(id, parameters, requestOptions, token), source => FileCommentBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.FileComment model ? model.DeleteAsync(parameters, token) : client.FileComments.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
