using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs AiTask operations with the SDK and projects the SDK's AiTask into the module's.
    internal static class AiTaskBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AiTask Project(Sdk.AiTask source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AiTask
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Name = source.Name,
                Description = source.Description,
                Prompt = source.Prompt,
                PermissionSet = source.PermissionSet,
                Path = source.Path,
                Source = source.Source,
                Disabled = source.Disabled,
                Trigger = source.Trigger,
                TriggerActions = Values.Copy(source.TriggerActions),
                Interval = source.Interval,
                RecurringDay = source.RecurringDay,
                RecurringDays = Values.Copy(source.RecurringDays),
                ScheduleId = source.ScheduleId,
                ScheduleDaysOfWeek = Values.Copy(source.ScheduleDaysOfWeek),
                ScheduleTimesOfDay = Values.Copy(source.ScheduleTimesOfDay),
                ScheduleTimeZone = source.ScheduleTimeZone,
                HolidayRegion = source.HolidayRegion,
                HumanReadableSchedule = source.HumanReadableSchedule,
                LastRunAt = Values.Utc(source.LastRunAt),
                MasterAdminUserId = source.MasterAdminUserId,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.AiTask> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.AiTasks.List(parameters, requestOptions), source => AiTaskBridge.Project(source, connection, options));
        }

        internal static Task<Models.AiTask> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.AiTasks.FindAsync(id, parameters, requestOptions, token), source => AiTaskBridge.Project(source, connection, options));
        }

        internal static Task<Models.AiTask> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.AiTasks.CreateAsync(parameters, requestOptions, token), source => AiTaskBridge.Project(source, connection, options));
        }

        internal static Task ManualRunAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.AiTask model ? model.ManualRunAsync(parameters, token) : client.AiTasks.ManualRunAsync(id, parameters, requestOptions, token));
        }

        internal static Task<Models.AiTask> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.AiTask model ? model.UpdateAsync(parameters, token) : client.AiTasks.UpdateAsync(id, parameters, requestOptions, token), source => AiTaskBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.AiTask model ? model.DeleteAsync(parameters, token) : client.AiTasks.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
