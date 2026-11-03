using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's AutomationExecutionNode into the module's.
    internal static class AutomationExecutionNodeBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AutomationExecutionNode Project(Sdk.AutomationExecutionNode source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AutomationExecutionNode
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                NodeId = source.NodeId,
                NodeType = source.NodeType,
                Status = source.Status,
                RunStage = source.RunStage,
                Reused = source.Reused,
                SuccessfulOperations = source.SuccessfulOperations,
                FailedOperations = source.FailedOperations,
                StartedAt = Values.Utc(source.StartedAt),
                CompletedAt = Values.Utc(source.CompletedAt),
                DurationMs = source.DurationMs,
                Inputs = Values.JsonArray(source.Inputs),
                Outputs = Values.Json(source.Outputs),
                InputItems = Values.Json(source.InputItems),
                OutputItems = Values.Json(source.OutputItems),
            };
        }
    }
}
