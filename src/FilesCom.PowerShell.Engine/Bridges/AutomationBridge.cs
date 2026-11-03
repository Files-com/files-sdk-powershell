using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Automation operations with the SDK and projects the SDK's Automation into the module's.
    internal static class AutomationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Automation Project(Sdk.Automation source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Automation
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                AlwaysSerializeJobs = source.AlwaysSerializeJobs,
                AlwaysOverwriteSizeMatchingFiles = source.AlwaysOverwriteSizeMatchingFiles,
                AutomationType = source.AutomationType,
                ConvertedAutomationId = source.ConvertedAutomationId,
                Deleted = source.Deleted,
                Description = source.Description,
                Definition = Values.Json(source.Definition),
                DestinationReplaceFrom = source.DestinationReplaceFrom,
                DestinationReplaceTo = source.DestinationReplaceTo,
                Destinations = Values.Copy(source.Destinations),
                Disabled = source.Disabled,
                ExcludePattern = source.ExcludePattern,
                ImportUrls = Values.JsonArray(source.ImportUrls),
                InboundEmailAddress = source.InboundEmailAddress,
                FlattenDestinationStructure = source.FlattenDestinationStructure,
                GroupIds = Values.Copy(source.GroupIds),
                IgnoreLockedFolders = source.IgnoreLockedFolders,
                Interval = source.Interval,
                LastModifiedAt = Values.Utc(source.LastModifiedAt),
                LegacyFolderMatching = source.LegacyFolderMatching,
                Name = source.Name,
                OriginalAutomationId = source.OriginalAutomationId,
                OverwriteFiles = source.OverwriteFiles,
                Path = source.Path,
                PathTimeZone = source.PathTimeZone,
                Version = source.Version,
                RecurringDay = source.RecurringDay,
                RecurringDays = Values.Copy(source.RecurringDays),
                ScheduleId = source.ScheduleId,
                RetryOnFailureIntervalInMinutes = source.RetryOnFailureIntervalInMinutes,
                RetryOnFailureNumberOfAttempts = source.RetryOnFailureNumberOfAttempts,
                Schedule = Values.Json(source.Schedule),
                HumanReadableSchedule = source.HumanReadableSchedule,
                ScheduleDaysOfWeek = Values.Copy(source.ScheduleDaysOfWeek),
                ScheduleTimesOfDay = Values.Copy(source.ScheduleTimesOfDay),
                ScheduleTimeZone = source.ScheduleTimeZone,
                Source = source.Source,
                SyncIds = Values.Copy(source.SyncIds),
                TriggerActions = Values.Copy(source.TriggerActions),
                Trigger = source.Trigger,
                UserId = source.UserId,
                UserIds = Values.Copy(source.UserIds),
                Value = Values.Json(source.Value),
                WebhookUrl = source.WebhookUrl,
                HolidayRegion = source.HolidayRegion,
            };
        }

        internal static FilesPageSource<Models.Automation> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Automations.List(parameters, requestOptions), source => AutomationBridge.Project(source, connection, options));
        }

        internal static Task<Models.Automation> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Automations.FindAsync(id, parameters, requestOptions, token), source => AutomationBridge.Project(source, connection, options));
        }

        internal static Task<Models.AutomationAuthoringSchema> GetAuthoringSchemaAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Automations.GetAuthoringSchemaAsync(parameters, requestOptions, token), source => AutomationAuthoringSchemaBridge.Project(source, connection, options));
        }

        internal static Task<Models.Automation> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Automations.CreateAsync(parameters, requestOptions, token), source => AutomationBridge.Project(source, connection, options));
        }

        internal static Task<Models.Automation> UpgradeAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Automation model ? model.UpgradeAsync(parameters, token) : client.Automations.UpgradeAsync(id, parameters, requestOptions, token), source => AutomationBridge.Project(source, connection, options));
        }

        internal static Task ManualRunAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Automation model ? model.ManualRunAsync(parameters, token) : client.Automations.ManualRunAsync(id, parameters, requestOptions, token));
        }

        internal static Task<Models.Automation> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Automation model ? model.UpdateAsync(parameters, token) : client.Automations.UpdateAsync(id, parameters, requestOptions, token), source => AutomationBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Automation model ? model.DeleteAsync(parameters, token) : client.Automations.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
