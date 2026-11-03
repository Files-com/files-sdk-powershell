using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's HistoryAction into the module's.
    internal static class HistoryActionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.HistoryAction Project(Sdk.HistoryAction source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.HistoryAction
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Path = source.Path,
                When = Values.Utc(source.When),
                Destination = source.Destination,
                Display = source.Display,
                Ip = source.Ip,
                Source = source.Source,
                Targets = Values.Json(source.Targets),
                UserId = source.UserId,
                Username = source.Username,
                UserIsFromParentSite = source.UserIsFromParentSite,
                ActionType = source.ActionType,
                FailureType = source.FailureType,
                Interface = source.Interface,
            };
        }
    }
}
