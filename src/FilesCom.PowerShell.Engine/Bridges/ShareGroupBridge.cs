using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ShareGroup operations with the SDK and projects the SDK's ShareGroup into the module's.
    internal static class ShareGroupBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ShareGroup Project(Sdk.ShareGroup source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ShareGroup
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                Notes = source.Notes,
                UserId = source.UserId,
                Members = Values.Array(source.Members, item => ShareGroupMemberBridge.Project(item, connection, options)),
            };
        }

        internal static FilesPageSource<Models.ShareGroup> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ShareGroups.List(parameters, requestOptions), source => ShareGroupBridge.Project(source, connection, options));
        }

        internal static Task<Models.ShareGroup> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ShareGroups.FindAsync(id, parameters, requestOptions, token), source => ShareGroupBridge.Project(source, connection, options));
        }

        internal static Task<Models.ShareGroup> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ShareGroups.CreateAsync(parameters, requestOptions, token), source => ShareGroupBridge.Project(source, connection, options));
        }

        internal static Task<Models.ShareGroup> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ShareGroup model ? model.UpdateAsync(parameters, token) : client.ShareGroups.UpdateAsync(id, parameters, requestOptions, token), source => ShareGroupBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ShareGroup model ? model.DeleteAsync(parameters, token) : client.ShareGroups.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
