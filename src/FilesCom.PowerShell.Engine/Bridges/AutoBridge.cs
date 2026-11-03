using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's Auto into the module's.
    internal static class AutoBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Auto Project(Sdk.Auto source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Auto
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Dynamic = Values.Json(source.Dynamic),
            };
        }
    }
}
