using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs AutomationRun operations with the SDK and projects the SDK's AutomationRun into the module's.
    internal static class AutomationRunBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AutomationRun Project(Sdk.AutomationRun source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AutomationRun
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                AutomationId = source.AutomationId,
                AutomationVersionId = source.AutomationVersionId,
                Version = source.Version,
                WorkspaceId = source.WorkspaceId,
                CancelRequestedAt = Values.Utc(source.CancelRequestedAt),
                CompletedAt = Values.Utc(source.CompletedAt),
                CreatedAt = Values.Utc(source.CreatedAt),
                RetryAt = Values.Utc(source.RetryAt),
                RetriedAt = Values.Utc(source.RetriedAt),
                RetriedInRunId = source.RetriedInRunId,
                RetryOfRunId = source.RetryOfRunId,
                RerunOfRunId = source.RerunOfRunId,
                RerunFromNodeId = source.RerunFromNodeId,
                Runtime = source.Runtime,
                Status = source.Status,
                SuccessfulOperations = source.SuccessfulOperations,
                FailedOperations = source.FailedOperations,
                Definition = Values.Json(source.Definition),
                NodeStates = Values.Json(source.NodeStates),
                ExecutionNodes = Values.Array(source.ExecutionNodes, item => AutomationExecutionNodeBridge.Project(item, connection, options)),
                JournalUrl = source.JournalUrl,
                StatusMessagesUrl = source.StatusMessagesUrl,
            };
        }

        internal static FilesPageSource<Models.AutomationRun> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.AutomationRuns.List(parameters, requestOptions), source => AutomationRunBridge.Project(source, connection, options));
        }

        internal static Task<Models.AutomationRun> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.AutomationRuns.FindAsync(id, parameters, requestOptions, token), source => AutomationRunBridge.Project(source, connection, options));
        }

        internal static Task<Models.AutomationExecutionNode> FindNodeAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.AutomationRuns.FindNodeAsync(id, parameters, requestOptions, token), source => AutomationExecutionNodeBridge.Project(source, connection, options));
        }

        internal static Task<Models.AutomationRun> RerunAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.AutomationRun model ? model.RerunAsync(parameters, token) : client.AutomationRuns.RerunAsync(id, parameters, requestOptions, token), source => AutomationRunBridge.Project(source, connection, options));
        }

        internal static Task<Models.AutomationRun> CancelAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.AutomationRun model ? model.CancelAsync(parameters, token) : client.AutomationRuns.CancelAsync(id, parameters, requestOptions, token), source => AutomationRunBridge.Project(source, connection, options));
        }
    }
}
