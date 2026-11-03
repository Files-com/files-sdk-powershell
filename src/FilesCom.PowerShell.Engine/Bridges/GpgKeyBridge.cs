using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs GpgKey operations with the SDK and projects the SDK's GpgKey into the module's.
    internal static class GpgKeyBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.GpgKey Project(Sdk.GpgKey source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.GpgKey
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                ExpiresAt = Values.Utc(source.ExpiresAt),
                Name = source.Name,
                PartnerId = source.PartnerId,
                PartnerName = source.PartnerName,
                UserId = source.UserId,
                PublicKeyMd5 = source.PublicKeyMd5,
                PrivateKeyMd5 = source.PrivateKeyMd5,
                GeneratedPublicKey = source.GeneratedPublicKey,
                GeneratedPrivateKey = Values.Secret(source.GeneratedPrivateKey),
                PrivateKeyPasswordMd5 = source.PrivateKeyPasswordMd5,
            };
        }

        internal static FilesPageSource<Models.GpgKey> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.GpgKeys.List(parameters, requestOptions), source => GpgKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.GpgKey> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.GpgKeys.FindAsync(id, parameters, requestOptions, token), source => GpgKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.GpgKey> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.GpgKeys.CreateAsync(parameters, requestOptions, token), source => GpgKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.GpgKey> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.GpgKey model ? model.UpdateAsync(parameters, token) : client.GpgKeys.UpdateAsync(id, parameters, requestOptions, token), source => GpgKeyBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.GpgKey model ? model.DeleteAsync(parameters, token) : client.GpgKeys.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
