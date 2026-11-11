using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Expectation operations with the SDK and projects the SDK's Expectation into the module's.
    internal static class ExpectationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Expectation Project(Sdk.Expectation source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Expectation
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Name = source.Name,
                Description = source.Description,
                Path = source.Path,
                Source = source.Source,
                ExcludePattern = source.ExcludePattern,
                Disabled = source.Disabled,
                ExpectationsVersion = source.ExpectationsVersion,
                Trigger = source.Trigger,
                Interval = source.Interval,
                RecurringDay = source.RecurringDay,
                RecurringDays = Values.Copy(source.RecurringDays),
                ScheduleId = source.ScheduleId,
                ScheduleDaysOfWeek = Values.Copy(source.ScheduleDaysOfWeek),
                ScheduleTimesOfDay = Values.Copy(source.ScheduleTimesOfDay),
                ScheduleTimeZone = source.ScheduleTimeZone,
                HolidayRegion = source.HolidayRegion,
                LookbackInterval = source.LookbackInterval,
                ScheduleTimeExceptions = Values.JsonArray(source.ScheduleTimeExceptions),
                LateAcceptanceInterval = source.LateAcceptanceInterval,
                InactivityInterval = source.InactivityInterval,
                MaxOpenInterval = source.MaxOpenInterval,
                Criteria = Values.Json(source.Criteria),
                LastEvaluatedAt = Values.Utc(source.LastEvaluatedAt),
                LastSuccessAt = Values.Utc(source.LastSuccessAt),
                LastFailureAt = Values.Utc(source.LastFailureAt),
                LastResult = source.LastResult,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.Expectation> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Expectations.List(parameters, requestOptions), source => ExpectationBridge.Project(source, connection, options));
        }

        internal static Task<Models.Expectation> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Expectations.FindAsync(id, parameters, requestOptions, token), source => ExpectationBridge.Project(source, connection, options));
        }

        internal static Task<Models.Expectation> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Expectations.CreateAsync(parameters, requestOptions, token), source => ExpectationBridge.Project(source, connection, options));
        }

        internal static Task<Models.ExpectationEvaluation> TriggerEvaluationAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Expectation model ? model.TriggerEvaluationAsync(parameters, token) : client.Expectations.TriggerEvaluationAsync(id, parameters, requestOptions, token), source => ExpectationEvaluationBridge.Project(source, connection, options));
        }

        internal static Task<Models.Expectation> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Expectation model ? model.UpdateAsync(parameters, token) : client.Expectations.UpdateAsync(id, parameters, requestOptions, token), source => ExpectationBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Expectation model ? model.DeleteAsync(parameters, token) : client.Expectations.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
