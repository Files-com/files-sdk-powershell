using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's ShareGroupMember into the module's.
    internal static class ShareGroupMemberBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.ShareGroupMember Project(Sdk.ShareGroupMember source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.ShareGroupMember
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Name = source.Name,
                Company = source.Company,
                Email = source.Email,
            };
        }
    }
}
