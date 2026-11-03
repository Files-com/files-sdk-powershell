using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Folder operations with the SDK.
    internal static class FolderBridge
    {
        internal static FilesPageSource<Models.RemoteFile> ListFor(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Folders.ListFor(path, parameters, requestOptions), source => RemoteFileBridge.Project(source, connection, options));
        }

        internal static Task<Models.RemoteFile> CreateAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Folders.CreateAsync(path, parameters, requestOptions, token), source => RemoteFileBridge.Project(source, connection, options));
        }
    }
}
