using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's AgentNode into the module's.
    internal static class AgentNodeBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AgentNode Project(Sdk.AgentNode source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AgentNode
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                NodeId = source.NodeId,
                Name = source.Name,
                Hostname = source.Hostname,
                AvailabilityRole = source.AvailabilityRole,
                Status = source.Status,
                IsDefault = source.IsDefault,
                DirectTransferAvailable = source.DirectTransferAvailable,
                LastSeenAt = Values.Utc(source.LastSeenAt),
                Instances = Values.Array(source.Instances, item => AgentNodeInstanceBridge.Project(item, connection, options)),
            };
        }
    }
}
