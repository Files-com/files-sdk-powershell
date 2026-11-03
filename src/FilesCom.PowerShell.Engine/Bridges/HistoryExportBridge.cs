using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs HistoryExport operations with the SDK and projects the SDK's HistoryExport into the module's.
    internal static class HistoryExportBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.HistoryExport Project(Sdk.HistoryExport source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.HistoryExport
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                WorkspaceId = source.WorkspaceId,
                Id = source.Id,
                HistoryVersion = source.HistoryVersion,
                StartAt = Values.Utc(source.StartAt),
                EndAt = Values.Utc(source.EndAt),
                Status = source.Status,
                QueryAction = source.QueryAction,
                QueryInterface = source.QueryInterface,
                QueryUserId = source.QueryUserId,
                QueryFileId = source.QueryFileId,
                QueryParentId = source.QueryParentId,
                QueryPath = source.QueryPath,
                QueryFolder = source.QueryFolder,
                QuerySrc = source.QuerySrc,
                QueryDestination = source.QueryDestination,
                QueryIp = source.QueryIp,
                QueryUsername = source.QueryUsername,
                QueryFailureType = source.QueryFailureType,
                QueryTargetId = source.QueryTargetId,
                QueryTargetName = source.QueryTargetName,
                QueryTargetPermission = source.QueryTargetPermission,
                QueryTargetUserId = source.QueryTargetUserId,
                QueryTargetUsername = source.QueryTargetUsername,
                QueryTargetPlatform = source.QueryTargetPlatform,
                QueryTargetPermissionSet = source.QueryTargetPermissionSet,
                ResultsUrl = source.ResultsUrl,
            };
        }

        internal static Task<Models.HistoryExport> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.HistoryExports.FindAsync(id, parameters, requestOptions, token), source => HistoryExportBridge.Project(source, connection, options));
        }

        internal static Task<Models.HistoryExport> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.HistoryExports.CreateAsync(parameters, requestOptions, token), source => HistoryExportBridge.Project(source, connection, options));
        }
    }
}
