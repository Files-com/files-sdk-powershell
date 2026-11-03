using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SettingsChange operations with the SDK and projects the SDK's SettingsChange into the module's.
    internal static class SettingsChangeBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SettingsChange Project(Sdk.SettingsChange source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SettingsChange
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                ApiKeyId = source.ApiKeyId,
                Changes = Values.Copy(source.Changes),
                CreatedAt = Values.Utc(source.CreatedAt),
                UserId = source.UserId,
                UserIsFilesSupport = source.UserIsFilesSupport,
                UserIsFromParentSite = source.UserIsFromParentSite,
                Username = source.Username,
            };
        }

        internal static FilesPageSource<Models.SettingsChange> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SettingsChanges.List(parameters, requestOptions), source => SettingsChangeBridge.Project(source, connection, options));
        }
    }
}
