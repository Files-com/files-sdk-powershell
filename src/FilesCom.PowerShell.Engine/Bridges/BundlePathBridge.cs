using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's BundlePath into the module's.
    internal static class BundlePathBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.BundlePath Project(Sdk.BundlePath source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.BundlePath
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Recursive = source.Recursive,
                Path = source.Path,
            };
        }
    }
}
