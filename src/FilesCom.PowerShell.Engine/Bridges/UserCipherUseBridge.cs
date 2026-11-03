using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs UserCipherUse operations with the SDK and projects the SDK's UserCipherUse into the module's.
    internal static class UserCipherUseBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.UserCipherUse Project(Sdk.UserCipherUse source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.UserCipherUse
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                UserId = source.UserId,
                Username = source.Username,
                ProtocolCipher = source.ProtocolCipher,
                CreatedAt = Values.Utc(source.CreatedAt),
                Insecure = source.Insecure,
                Interface = source.Interface,
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.UserCipherUse> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.UserCipherUses.List(parameters, requestOptions), source => UserCipherUseBridge.Project(source, connection, options));
        }
    }
}
