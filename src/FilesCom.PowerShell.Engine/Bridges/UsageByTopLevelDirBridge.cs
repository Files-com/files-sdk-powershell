using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's UsageByTopLevelDir into the module's.
    internal static class UsageByTopLevelDirBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UsageByTopLevelDir Project(Sdk.UsageByTopLevelDir source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UsageByTopLevelDir
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Dir = source.Dir,
                Size = source.Size,
                Count = source.Count,
            };
        }
    }
}
