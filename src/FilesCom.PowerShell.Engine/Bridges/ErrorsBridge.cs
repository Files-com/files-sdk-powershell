using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's Errors into the module's.
    internal static class ErrorsBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Errors Project(Sdk.Errors source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Errors
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Fields = Values.Copy(source.Fields),
                Messages = Values.Copy(source.Messages),
            };
        }
    }
}
