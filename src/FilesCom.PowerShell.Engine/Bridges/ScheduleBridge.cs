using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Schedule operations with the SDK and projects the SDK's Schedule into the module's.
    internal static class ScheduleBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Schedule Project(Sdk.Schedule source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Schedule
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                ScheduleDaysOfWeek = Values.Copy(source.ScheduleDaysOfWeek),
                ScheduleTimesOfDay = Values.Copy(source.ScheduleTimesOfDay),
                ScheduleTimeZone = source.ScheduleTimeZone,
                HolidayRegion = source.HolidayRegion,
                HumanReadableSchedule = source.HumanReadableSchedule,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.Schedule> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Schedules.List(parameters, requestOptions), source => ScheduleBridge.Project(source, connection, options));
        }

        internal static Task<Models.Schedule> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Schedules.FindAsync(id, parameters, requestOptions, token), source => ScheduleBridge.Project(source, connection, options));
        }

        internal static Task<Models.Schedule> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Schedules.CreateAsync(parameters, requestOptions, token), source => ScheduleBridge.Project(source, connection, options));
        }

        internal static Task<Models.Schedule> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Schedule model ? model.UpdateAsync(parameters, token) : client.Schedules.UpdateAsync(id, parameters, requestOptions, token), source => ScheduleBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Schedule model ? model.DeleteAsync(parameters, token) : client.Schedules.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
