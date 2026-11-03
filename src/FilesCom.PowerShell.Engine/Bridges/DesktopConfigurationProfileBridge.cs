using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs DesktopConfigurationProfile operations with the SDK and projects the SDK's DesktopConfigurationProfile into the module's.
    internal static class DesktopConfigurationProfileBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.DesktopConfigurationProfile Project(Sdk.DesktopConfigurationProfile source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.DesktopConfigurationProfile
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                WorkspaceId = source.WorkspaceId,
                UseForAllUsers = source.UseForAllUsers,
                DisableDriveMounting = source.DisableDriveMounting,
                MountMappings = Values.Json(source.MountMappings),
            };
        }

        internal static FilesPageSource<Models.DesktopConfigurationProfile> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.DesktopConfigurationProfiles.List(parameters, requestOptions), source => DesktopConfigurationProfileBridge.Project(source, connection, options));
        }

        internal static Task<Models.DesktopConfigurationProfile> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.DesktopConfigurationProfiles.FindAsync(id, parameters, requestOptions, token), source => DesktopConfigurationProfileBridge.Project(source, connection, options));
        }

        internal static Task<Models.DesktopConfigurationProfile> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.DesktopConfigurationProfiles.CreateAsync(parameters, requestOptions, token), source => DesktopConfigurationProfileBridge.Project(source, connection, options));
        }

        internal static Task<Models.DesktopConfigurationProfile> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.DesktopConfigurationProfile model ? model.UpdateAsync(parameters, token) : client.DesktopConfigurationProfiles.UpdateAsync(id, parameters, requestOptions, token), source => DesktopConfigurationProfileBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.DesktopConfigurationProfile model ? model.DeleteAsync(parameters, token) : client.DesktopConfigurationProfiles.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
