using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs BundleDownload operations with the SDK and projects the SDK's BundleDownload into the module's.
    internal static class BundleDownloadBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.BundleDownload Project(Sdk.BundleDownload source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.BundleDownload
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                BundleRegistration = BundleRegistrationBridge.Project(source.BundleRegistration, connection, options),
                DownloadMethod = source.DownloadMethod,
                Path = source.Path,
                WorkspaceId = source.WorkspaceId,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.BundleDownload> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.BundleDownloads.List(parameters, requestOptions), source => BundleDownloadBridge.Project(source, connection, options));
        }
    }
}
