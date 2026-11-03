using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs UsageDailySnapshot operations with the SDK and projects the SDK's UsageDailySnapshot into the module's.
    internal static class UsageDailySnapshotBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UsageDailySnapshot Project(Sdk.UsageDailySnapshot source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UsageDailySnapshot
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Date = Values.Utc(source.Date),
                ApiUsageAvailable = source.ApiUsageAvailable,
                ReadApiUsage = source.ReadApiUsage,
                WriteApiUsage = source.WriteApiUsage,
                TransformationCreditsUsage = source.TransformationCreditsUsage,
                UserCount = source.UserCount,
                CurrentStorage = source.CurrentStorage,
                DeletedFilesStorage = source.DeletedFilesStorage,
                DeletedFilesCountedInMinimum = source.DeletedFilesCountedInMinimum,
                RootStorage = source.RootStorage,
                UsageByTopLevelDir = Values.Array(source.UsageByTopLevelDir, item => UsageByTopLevelDirBridge.Project(item, connection, options)),
            };
        }

        internal static FilesPageSource<Models.UsageDailySnapshot> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.UsageDailySnapshots.List(parameters, requestOptions), source => UsageDailySnapshotBridge.Project(source, connection, options));
        }
    }
}
