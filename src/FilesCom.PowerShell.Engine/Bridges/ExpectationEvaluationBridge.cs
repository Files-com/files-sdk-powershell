using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ExpectationEvaluation operations with the SDK and projects the SDK's ExpectationEvaluation into the module's.
    internal static class ExpectationEvaluationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ExpectationEvaluation Project(Sdk.ExpectationEvaluation source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ExpectationEvaluation
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                ExpectationId = source.ExpectationId,
                Status = source.Status,
                OpenedVia = source.OpenedVia,
                OpenedAt = Values.Utc(source.OpenedAt),
                WindowStartAt = Values.Utc(source.WindowStartAt),
                WindowEndAt = Values.Utc(source.WindowEndAt),
                DeadlineAt = Values.Utc(source.DeadlineAt),
                LateAcceptanceCutoffAt = Values.Utc(source.LateAcceptanceCutoffAt),
                HardCloseAt = Values.Utc(source.HardCloseAt),
                ClosedAt = Values.Utc(source.ClosedAt),
                MatchedFiles = Values.JsonArray(source.MatchedFiles),
                MissingFiles = Values.JsonArray(source.MissingFiles),
                CriteriaErrors = Values.Copy(source.CriteriaErrors),
                Summary = Values.Json(source.Summary),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.ExpectationEvaluation> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ExpectationEvaluations.List(parameters, requestOptions), source => ExpectationEvaluationBridge.Project(source, connection, options));
        }

        internal static Task<Models.ExpectationEvaluation> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ExpectationEvaluations.FindAsync(id, parameters, requestOptions, token), source => ExpectationEvaluationBridge.Project(source, connection, options));
        }
    }
}
