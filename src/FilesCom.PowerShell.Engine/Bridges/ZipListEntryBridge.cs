using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's ZipListEntry into the module's.
    internal static class ZipListEntryBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ZipListEntry Project(Sdk.ZipListEntry source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ZipListEntry
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Path = source.Path,
                Size = source.Size,
            };
        }
    }
}
