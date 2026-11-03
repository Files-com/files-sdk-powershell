using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs GroupUser operations with the SDK and projects the SDK's GroupUser into the module's.
    internal static class GroupUserBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.GroupUser Project(Sdk.GroupUser source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.GroupUser
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                GroupName = source.GroupName,
                GroupId = source.GroupId,
                UserId = source.UserId,
                Admin = source.Admin,
                Username = source.Username,
            };
        }

        internal static FilesPageSource<Models.GroupUser> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.GroupUsers.List(parameters, requestOptions), source => GroupUserBridge.Project(source, connection, options));
        }

        internal static Task<Models.GroupUser> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.GroupUsers.CreateAsync(parameters, requestOptions, token), source => GroupUserBridge.Project(source, connection, options));
        }

        internal static Task<Models.GroupUser> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.GroupUsers.UpdateAsync(id, parameters, requestOptions, token), source => GroupUserBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.GroupUsers.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
