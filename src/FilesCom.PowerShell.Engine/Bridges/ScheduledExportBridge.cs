using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ScheduledExport operations with the SDK and projects the SDK's ScheduledExport into the module's.
    internal static class ScheduledExportBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ScheduledExport Project(Sdk.ScheduledExport source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ScheduledExport
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                ExportType = source.ExportType,
                ReportName = source.ReportName,
                ExportOptions = Values.Json(source.ExportOptions),
                UserId = source.UserId,
                Disabled = source.Disabled,
                Trigger = source.Trigger,
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
                LastExportId = source.LastExportId,
                LastError = source.LastError,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.ScheduledExport> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ScheduledExports.List(parameters, requestOptions), source => ScheduledExportBridge.Project(source, connection, options));
        }

        internal static Task<Models.ScheduledExport> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ScheduledExports.FindAsync(id, parameters, requestOptions, token), source => ScheduledExportBridge.Project(source, connection, options));
        }

        internal static Task<Models.ScheduledExport> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ScheduledExports.CreateAsync(parameters, requestOptions, token), source => ScheduledExportBridge.Project(source, connection, options));
        }

        internal static Task<Models.ScheduledExport> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ScheduledExport model ? model.UpdateAsync(parameters, token) : client.ScheduledExports.UpdateAsync(id, parameters, requestOptions, token), source => ScheduledExportBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.ScheduledExport model ? model.DeleteAsync(parameters, token) : client.ScheduledExports.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
