using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Workspace operations with the SDK and projects the SDK's Workspace into the module's.
    internal static class WorkspaceBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Workspace Project(Sdk.Workspace source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Workspace
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
            };
        }

        internal static FilesPageSource<Models.Workspace> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Workspaces.List(parameters, requestOptions), source => WorkspaceBridge.Project(source, connection, options));
        }

        internal static Task<Models.Workspace> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Workspaces.FindAsync(id, parameters, requestOptions, token), source => WorkspaceBridge.Project(source, connection, options));
        }

        internal static Task<Models.Workspace> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Workspaces.CreateAsync(parameters, requestOptions, token), source => WorkspaceBridge.Project(source, connection, options));
        }

        internal static Task<Models.Workspace> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Workspace model ? model.UpdateAsync(parameters, token) : client.Workspaces.UpdateAsync(id, parameters, requestOptions, token), source => WorkspaceBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Workspace model ? model.DeleteAsync(parameters, token) : client.Workspaces.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
