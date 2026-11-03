using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ActionNotificationExport operations with the SDK and projects the SDK's ActionNotificationExport into the module's.
    internal static class ActionNotificationExportBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ActionNotificationExport Project(Sdk.ActionNotificationExport source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ActionNotificationExport
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                ExportVersion = source.ExportVersion,
                StartAt = Values.Utc(source.StartAt),
                EndAt = Values.Utc(source.EndAt),
                Status = source.Status,
                QueryPath = source.QueryPath,
                QueryFolder = source.QueryFolder,
                QueryMessage = source.QueryMessage,
                QueryRequestMethod = source.QueryRequestMethod,
                QueryRequestUrl = source.QueryRequestUrl,
                QueryStatus = source.QueryStatus,
                QuerySuccess = source.QuerySuccess,
                ResultsUrl = source.ResultsUrl,
            };
        }

        internal static Task<Models.ActionNotificationExport> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ActionNotificationExports.FindAsync(id, parameters, requestOptions, token), source => ActionNotificationExportBridge.Project(source, connection, options));
        }

        internal static Task<Models.ActionNotificationExport> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.ActionNotificationExports.CreateAsync(parameters, requestOptions, token), source => ActionNotificationExportBridge.Project(source, connection, options));
        }
    }
}
