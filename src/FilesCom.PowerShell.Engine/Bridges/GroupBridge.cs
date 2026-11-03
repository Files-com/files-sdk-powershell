using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Group operations with the SDK and projects the SDK's Group into the module's.
    internal static class GroupBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Group Project(Sdk.Group source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Group
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                AllowedIps = source.AllowedIps,
                AdminIds = source.AdminIds,
                Notes = source.Notes,
                UserIds = source.UserIds,
                Usernames = source.Usernames,
                AiAssistantPersonalityId = source.AiAssistantPersonalityId,
                FtpPermission = source.FtpPermission,
                SftpPermission = source.SftpPermission,
                DavPermission = source.DavPermission,
                RestapiPermission = source.RestapiPermission,
                S3CompatibleEndpointPermission = source.S3CompatibleEndpointPermission,
                DesktopConfigurationProfileId = source.DesktopConfigurationProfileId,
                IntegrationCentricProfileId = source.IntegrationCentricProfileId,
                SiteId = source.SiteId,
                WorkspaceId = source.WorkspaceId,
            };
        }

        internal static FilesPageSource<Models.Group> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Groups.List(parameters, requestOptions), source => GroupBridge.Project(source, connection, options));
        }

        internal static Task<Models.Group> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Groups.FindAsync(id, parameters, requestOptions, token), source => GroupBridge.Project(source, connection, options));
        }

        internal static Task<Models.Group> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Groups.CreateAsync(parameters, requestOptions, token), source => GroupBridge.Project(source, connection, options));
        }

        internal static Task<Models.Group> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Group model ? model.UpdateAsync(parameters, token) : client.Groups.UpdateAsync(id, parameters, requestOptions, token), source => GroupBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Group model ? model.DeleteAsync(parameters, token) : client.Groups.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
