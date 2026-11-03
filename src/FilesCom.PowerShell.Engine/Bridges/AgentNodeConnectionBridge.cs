using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's AgentNodeConnection into the module's.
    internal static class AgentNodeConnectionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AgentNodeConnection Project(Sdk.AgentNodeConnection source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AgentNodeConnection
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Mode = source.Mode,
                Status = source.Status,
                LastSeenAt = Values.Utc(source.LastSeenAt),
            };
        }
    }
}
