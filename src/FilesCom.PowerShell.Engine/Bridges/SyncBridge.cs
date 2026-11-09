using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Sync operations with the SDK and projects the SDK's Sync into the module's.
    internal static class SyncBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Sync Project(Sdk.Sync source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Sync
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                SiteId = source.SiteId,
                WorkspaceId = source.WorkspaceId,
                UserId = source.UserId,
                SrcPath = source.SrcPath,
                DestPath = source.DestPath,
                SrcRemoteServerId = source.SrcRemoteServerId,
                DestRemoteServerId = source.DestRemoteServerId,
                SrcSiteId = source.SrcSiteId,
                DestSiteId = source.DestSiteId,
                TwoWay = source.TwoWay,
                KeepAfterCopy = source.KeepAfterCopy,
                DeleteEmptyFolders = source.DeleteEmptyFolders,
                Disabled = source.Disabled,
                Trigger = source.Trigger,
                TriggerFile = source.TriggerFile,
                AlwaysWriteTriggerFile = source.AlwaysWriteTriggerFile,
                IncludePatterns = Values.Copy(source.IncludePatterns),
                ExcludePatterns = Values.Copy(source.ExcludePatterns),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
                SyncIntervalMinutes = source.SyncIntervalMinutes,
                Interval = source.Interval,
                RecurringDay = source.RecurringDay,
                RecurringDays = Values.Copy(source.RecurringDays),
                ScheduleId = source.ScheduleId,
                ScheduleDaysOfWeek = Values.Copy(source.ScheduleDaysOfWeek),
                ScheduleTimesOfDay = Values.Copy(source.ScheduleTimesOfDay),
                ScheduleTimeZone = source.ScheduleTimeZone,
                HolidayRegion = source.HolidayRegion,
                LatestSyncRun = SyncRunBridge.Project(source.LatestSyncRun, connection, options),
            };
        }

        internal static FilesPageSource<Models.Sync> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Syncs.List(parameters, requestOptions), source => SyncBridge.Project(source, connection, options));
        }

        internal static Task<Models.Sync> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Syncs.FindAsync(id, parameters, requestOptions, token), source => SyncBridge.Project(source, connection, options));
        }

        internal static Task<Models.Sync> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Syncs.CreateAsync(parameters, requestOptions, token), source => SyncBridge.Project(source, connection, options));
        }

        internal static Task ManualRunAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Sync model ? model.ManualRunAsync(parameters, token) : client.Syncs.ManualRunAsync(id, parameters, requestOptions, token));
        }

        internal static Task DryRunAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Sync model ? model.DryRunAsync(parameters, token) : client.Syncs.DryRunAsync(id, parameters, requestOptions, token));
        }

        internal static Task<Models.Sync> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Sync model ? model.UpdateAsync(parameters, token) : client.Syncs.UpdateAsync(id, parameters, requestOptions, token), source => SyncBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Sync model ? model.DeleteAsync(parameters, token) : client.Syncs.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
