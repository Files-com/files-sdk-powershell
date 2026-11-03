using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs ActionLog operations with the SDK and projects the SDK's ActionLog into the module's.
    internal static class ActionLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ActionLog Project(Sdk.ActionLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ActionLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Action = source.Action,
                CreatedAt = Values.Utc(source.CreatedAt),
                Destination = source.Destination,
                FailureType = source.FailureType,
                Folder = source.Folder,
                Interface = source.Interface,
                Ip = source.Ip,
                MetadataDmId = source.MetadataDmId,
                ParentMetadataDmId = source.ParentMetadataDmId,
                Path = source.Path,
                SiteId = source.SiteId,
                Src = source.Src,
                UserId = source.UserId,
                Username = source.Username,
            };
        }

        internal static FilesPageSource<Models.ActionLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.ActionLogs.List(parameters, requestOptions), source => ActionLogBridge.Project(source, connection, options));
        }
    }
}
