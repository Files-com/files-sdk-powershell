using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs HolidayCalendar operations with the SDK and projects the SDK's HolidayCalendar into the module's.
    internal static class HolidayCalendarBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.HolidayCalendar Project(Sdk.HolidayCalendar source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.HolidayCalendar
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                Definition = Values.Json(source.Definition),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.HolidayCalendar> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.HolidayCalendars.List(parameters, requestOptions), source => HolidayCalendarBridge.Project(source, connection, options));
        }

        internal static Task<Models.HolidayCalendar> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.HolidayCalendars.FindAsync(id, parameters, requestOptions, token), source => HolidayCalendarBridge.Project(source, connection, options));
        }

        internal static Task<Models.HolidayCalendar> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.HolidayCalendars.CreateAsync(parameters, requestOptions, token), source => HolidayCalendarBridge.Project(source, connection, options));
        }

        internal static Task<Models.HolidayCalendar> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.HolidayCalendar model ? model.UpdateAsync(parameters, token) : client.HolidayCalendars.UpdateAsync(id, parameters, requestOptions, token), source => HolidayCalendarBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.HolidayCalendar model ? model.DeleteAsync(parameters, token) : client.HolidayCalendars.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
