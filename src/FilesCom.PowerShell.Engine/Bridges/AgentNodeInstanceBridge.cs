using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's AgentNodeInstance into the module's.
    internal static class AgentNodeInstanceBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AgentNodeInstance Project(Sdk.AgentNodeInstance source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AgentNodeInstance
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                InstanceId = source.InstanceId,
                ProcessState = source.ProcessState,
                Status = source.Status,
                IsDefault = source.IsDefault,
                AgentVersion = source.AgentVersion,
                LastSeenAt = Values.Utc(source.LastSeenAt),
                Connections = Values.Array(source.Connections, item => AgentNodeConnectionBridge.Project(item, connection, options)),
            };
        }
    }
}
