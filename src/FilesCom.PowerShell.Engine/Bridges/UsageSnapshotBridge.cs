using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs UsageSnapshot operations with the SDK and projects the SDK's UsageSnapshot into the module's.
    internal static class UsageSnapshotBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UsageSnapshot Project(Sdk.UsageSnapshot source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UsageSnapshot
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                StartAt = Values.Utc(source.StartAt),
                EndAt = Values.Utc(source.EndAt),
                HighWaterUserCount = source.HighWaterUserCount,
                TransformationCreditsUsage = source.TransformationCreditsUsage,
                CurrentStorage = source.CurrentStorage,
                HighWaterStorage = source.HighWaterStorage,
                RootStorage = source.RootStorage,
                DeletedFilesCountedInMinimum = source.DeletedFilesCountedInMinimum,
                DeletedFilesStorage = source.DeletedFilesStorage,
                TotalBillableUsage = source.TotalBillableUsage,
                TotalBillableTransferUsage = source.TotalBillableTransferUsage,
                BytesSent = source.BytesSent,
                SyncBytesReceived = source.SyncBytesReceived,
                SyncBytesSent = source.SyncBytesSent,
                UsageByTopLevelDir = Values.Array(source.UsageByTopLevelDir, item => UsageByTopLevelDirBridge.Project(item, connection, options)),
            };
        }

        internal static FilesPageSource<Models.UsageSnapshot> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.UsageSnapshots.List(parameters, requestOptions), source => UsageSnapshotBridge.Project(source, connection, options));
        }
    }
}
