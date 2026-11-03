using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs EventRecord operations with the SDK and projects the SDK's EventRecord into the module's.
    internal static class EventRecordBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.EventRecord Project(Sdk.EventRecord source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.EventRecord
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                EventUuid = source.EventUuid,
                EventType = source.EventType,
                Severity = source.Severity,
                SourceType = source.SourceType,
                SourceId = source.SourceId,
                OccurredAt = Values.Utc(source.OccurredAt),
                HumanTitle = source.HumanTitle,
                HumanSummary = source.HumanSummary,
                HumanFields = Values.JsonArray(source.HumanFields),
                Actor = Values.Json(source.Actor),
                Resources = Values.JsonArray(source.Resources),
                Payload = Values.Json(source.Payload),
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.EventRecord> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.EventRecords.List(parameters, requestOptions), source => EventRecordBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventRecord> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.EventRecords.FindAsync(id, parameters, requestOptions, token), source => EventRecordBridge.Project(source, connection, options));
        }
    }
}
