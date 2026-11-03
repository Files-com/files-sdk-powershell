using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's PublicIpAddress into the module's.
    internal static class PublicIpAddressBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PublicIpAddress Project(Sdk.PublicIpAddress source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PublicIpAddress
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                IpAddress = source.IpAddress,
                ServerName = source.ServerName,
                FtpEnabled = source.FtpEnabled,
                SftpEnabled = source.SftpEnabled,
            };
        }
    }
}
