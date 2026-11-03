using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs AutomationLog operations with the SDK and projects the SDK's AutomationLog into the module's.
    internal static class AutomationLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AutomationLog Project(Sdk.AutomationLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AutomationLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                AutomationId = source.AutomationId,
                AutomationRunId = source.AutomationRunId,
                CorrelationId = source.CorrelationId,
                DestPath = source.DestPath,
                ErrorType = source.ErrorType,
                SourceAutomationId = source.SourceAutomationId,
                SourceAutomationVersion = source.SourceAutomationVersion,
                Message = source.Message,
                NodeId = source.NodeId,
                NodeType = source.NodeType,
                Operation = source.Operation,
                Output = source.Output,
                Path = source.Path,
                Status = source.Status,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.AutomationLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.AutomationLogs.List(parameters, requestOptions), source => AutomationLogBridge.Project(source, connection, options));
        }
    }
}
