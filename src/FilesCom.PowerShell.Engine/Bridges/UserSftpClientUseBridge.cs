using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs UserSftpClientUse operations with the SDK and projects the SDK's UserSftpClientUse into the module's.
    internal static class UserSftpClientUseBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UserSftpClientUse Project(Sdk.UserSftpClientUse source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UserSftpClientUse
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                SftpClient = source.SftpClient,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
                UserId = source.UserId,
            };
        }

        internal static FilesPageSource<Models.UserSftpClientUse> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.UserSftpClientUses.List(parameters, requestOptions), source => UserSftpClientUseBridge.Project(source, connection, options));
        }
    }
}
