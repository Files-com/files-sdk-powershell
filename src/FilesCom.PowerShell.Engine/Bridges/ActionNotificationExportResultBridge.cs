using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ActionNotificationExportResult operations with the SDK and projects the SDK's ActionNotificationExportResult into the module's.
    internal static class ActionNotificationExportResultBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ActionNotificationExportResult Project(Sdk.ActionNotificationExportResult source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ActionNotificationExportResult
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                Status = source.Status,
                Message = source.Message,
                Success = source.Success,
                RequestHeaders = source.RequestHeaders,
                RequestMethod = source.RequestMethod,
                RequestUrl = source.RequestUrl,
                Path = source.Path,
                Folder = source.Folder,
            };
        }

        internal static FilesPageSource<Models.ActionNotificationExportResult> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ActionNotificationExportResults.List(parameters, requestOptions), source => ActionNotificationExportResultBridge.Project(source, connection, options));
        }
    }
}
