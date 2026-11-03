using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's Preview into the module's.
    internal static class PreviewBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Preview Project(Sdk.Preview source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Preview
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Status = source.Status,
                DownloadUri = source.DownloadUri,
                Type = source.Type,
                Size = source.Size,
            };
        }
    }
}
