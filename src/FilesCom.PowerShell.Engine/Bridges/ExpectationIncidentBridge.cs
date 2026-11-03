using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ExpectationIncident operations with the SDK and projects the SDK's ExpectationIncident into the module's.
    internal static class ExpectationIncidentBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ExpectationIncident Project(Sdk.ExpectationIncident source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ExpectationIncident
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                ExpectationId = source.ExpectationId,
                Status = source.Status,
                OpenedAt = Values.Utc(source.OpenedAt),
                LastFailedAt = Values.Utc(source.LastFailedAt),
                AcknowledgedAt = Values.Utc(source.AcknowledgedAt),
                SnoozedUntil = Values.Utc(source.SnoozedUntil),
                ResolvedAt = Values.Utc(source.ResolvedAt),
                OpenedByEvaluationId = source.OpenedByEvaluationId,
                LastEvaluationId = source.LastEvaluationId,
                ResolvedByEvaluationId = source.ResolvedByEvaluationId,
                Summary = Values.Json(source.Summary),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.ExpectationIncident> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ExpectationIncidents.List(parameters, requestOptions), source => ExpectationIncidentBridge.Project(source, connection, options));
        }

        internal static Task<Models.ExpectationIncident> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ExpectationIncidents.FindAsync(id, parameters, requestOptions, token), source => ExpectationIncidentBridge.Project(source, connection, options));
        }

        internal static Task<Models.ExpectationIncident> ResolveAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ExpectationIncident model ? model.ResolveAsync(parameters, token) : client.ExpectationIncidents.ResolveAsync(id, parameters, requestOptions, token), source => ExpectationIncidentBridge.Project(source, connection, options));
        }

        internal static Task<Models.ExpectationIncident> SnoozeAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ExpectationIncident model ? model.SnoozeAsync(parameters, token) : client.ExpectationIncidents.SnoozeAsync(id, parameters, requestOptions, token), source => ExpectationIncidentBridge.Project(source, connection, options));
        }

        internal static Task<Models.ExpectationIncident> AcknowledgeAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ExpectationIncident model ? model.AcknowledgeAsync(parameters, token) : client.ExpectationIncidents.AcknowledgeAsync(id, parameters, requestOptions, token), source => ExpectationIncidentBridge.Project(source, connection, options));
        }
    }
}
