using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's PartnerConnection into the module's.
    internal static class PartnerConnectionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PartnerConnection Project(Sdk.PartnerConnection source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PartnerConnection
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Role = source.Role,
                SiteId = source.SiteId,
                SiteName = source.SiteName,
                MountPath = source.MountPath,
            };
        }
    }
}
