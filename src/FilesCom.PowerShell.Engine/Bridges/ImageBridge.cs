using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's Image into the module's.
    internal static class ImageBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Image Project(Sdk.Image source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Image
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Name = source.Name,
                Uri = source.Uri,
            };
        }
    }
}
