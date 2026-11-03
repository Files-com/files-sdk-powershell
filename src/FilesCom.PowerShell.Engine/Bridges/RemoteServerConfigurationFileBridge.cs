using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's RemoteServerConfigurationFile into the module's.
    internal static class RemoteServerConfigurationFileBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.RemoteServerConfigurationFile Project(Sdk.RemoteServerConfigurationFile source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.RemoteServerConfigurationFile
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                PermissionSet = source.PermissionSet,
                PrivateKey = Values.Secret(source.PrivateKey),
                Subdomain = source.Subdomain,
                Root = source.Root,
                NodeName = source.NodeName,
                FollowLinks = source.FollowLinks,
                PreferProtocol = source.PreferProtocol,
                Dns = source.Dns,
                ProxyAllOutbound = source.ProxyAllOutbound,
                EndpointOverride = source.EndpointOverride,
                LogFile = source.LogFile,
                LogLevel = source.LogLevel,
                LogRotateNum = source.LogRotateNum,
                LogRotateSize = source.LogRotateSize,
                OverrideMaxConcurrentJobs = source.OverrideMaxConcurrentJobs,
                GracefulShutdownTimeout = source.GracefulShutdownTimeout,
                TransferRateLimit = source.TransferRateLimit,
                AutoUpdatePolicy = source.AutoUpdatePolicy,
            };
        }
    }
}
