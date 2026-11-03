using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Permission operations with the SDK and projects the SDK's Permission into the module's.
    internal static class PermissionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Permission Project(Sdk.Permission source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Permission
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Path = source.Path,
                UserId = source.UserId,
                Username = source.Username,
                GroupId = source.GroupId,
                GroupName = source.GroupName,
                GroupIds = Values.Copy(source.GroupIds),
                GroupNames = Values.Copy(source.GroupNames),
                PartnerId = source.PartnerId,
                PartnerName = source.PartnerName,
                PermissionType = source.PermissionType,
                Recursive = source.Recursive,
                SiteId = source.SiteId,
            };
        }

        internal static FilesPageSource<Models.Permission> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Permissions.List(parameters, requestOptions), source => PermissionBridge.Project(source, connection, options));
        }

        internal static Task<Models.Permission> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Permissions.CreateAsync(parameters, requestOptions, token), source => PermissionBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Permission model ? model.DeleteAsync(parameters, token) : client.Permissions.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
