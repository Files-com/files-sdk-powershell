using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's AgentPushUpdate into the module's.
    internal static class AgentPushUpdateBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AgentPushUpdate Project(Sdk.AgentPushUpdate source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AgentPushUpdate
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Version = source.Version,
                Message = source.Message,
                CurrentVersion = source.CurrentVersion,
                PendingVersion = source.PendingVersion,
                LastError = source.LastError,
                Error = source.Error,
            };
        }
    }
}
