using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs HistoryExportResult operations with the SDK and projects the SDK's HistoryExportResult into the module's.
    internal static class HistoryExportResultBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.HistoryExportResult Project(Sdk.HistoryExportResult source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.HistoryExportResult
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                CreatedAtIso8601 = source.CreatedAtIso8601,
                UserId = source.UserId,
                FileId = source.FileId,
                ParentId = source.ParentId,
                Path = source.Path,
                Folder = source.Folder,
                Src = source.Src,
                Destination = source.Destination,
                Ip = source.Ip,
                Username = source.Username,
                UserIsFromParentSite = source.UserIsFromParentSite,
                Action = source.Action,
                FailureType = source.FailureType,
                Interface = source.Interface,
                TargetId = source.TargetId,
                TargetName = source.TargetName,
                TargetPermission = source.TargetPermission,
                TargetRecursive = source.TargetRecursive,
                TargetExpiresAt = source.TargetExpiresAt,
                TargetExpiresAtIso8601 = source.TargetExpiresAtIso8601,
                TargetPermissionSet = source.TargetPermissionSet,
                TargetPlatform = source.TargetPlatform,
                TargetUsername = source.TargetUsername,
                TargetUserId = source.TargetUserId,
            };
        }

        internal static FilesPageSource<Models.HistoryExportResult> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.HistoryExportResults.List(parameters, requestOptions), source => HistoryExportResultBridge.Project(source, connection, options));
        }
    }
}
