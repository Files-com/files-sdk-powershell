using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's Status into the module's.
    internal static class StatusBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Status Project(Sdk.Status source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Status
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Code = source.Code,
                Message = source.Message,
                StatusType = source.StatusType,
                Data = AutoBridge.Project(source.Data, connection, options),
                Errors = Values.Array(source.Errors, item => ErrorsBridge.Project(item, connection, options)),
                ClickwrapId = source.ClickwrapId,
                ClickwrapBody = source.ClickwrapBody,
            };
        }
    }
}
