using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's FileAction into the module's.
    internal static class FileActionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FileAction Project(Sdk.FileAction source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FileAction
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Status = source.Status,
                FileMigrationId = source.FileMigrationId,
            };
        }
    }
}
