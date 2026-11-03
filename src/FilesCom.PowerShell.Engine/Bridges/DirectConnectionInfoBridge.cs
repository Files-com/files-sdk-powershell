using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's DirectConnectionInfo into the module's.
    internal static class DirectConnectionInfoBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.DirectConnectionInfo Project(Sdk.DirectConnectionInfo source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.DirectConnectionInfo
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Version = source.Version,
                ServerName = source.ServerName,
                Addresses = Values.Copy(source.Addresses),
                DirectUri = source.DirectUri,
                CaPem = source.CaPem,
            };
        }
    }
}
