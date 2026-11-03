using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs PublicKey operations with the SDK and projects the SDK's PublicKey into the module's.
    internal static class PublicKeyBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PublicKey Project(Sdk.PublicKey source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PublicKey
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Title = source.Title,
                CreatedAt = Values.Utc(source.CreatedAt),
                ExpiresAt = Values.Utc(source.ExpiresAt),
                Expired = source.Expired,
                Fingerprint = source.Fingerprint,
                FingerprintSha256 = source.FingerprintSha256,
                Status = source.Status,
                LastLoginAt = Values.Utc(source.LastLoginAt),
                GeneratedPrivateKey = Values.Secret(source.GeneratedPrivateKey),
                GeneratedPublicKey = source.GeneratedPublicKey,
                Username = source.Username,
                UserId = source.UserId,
            };
        }

        internal static FilesPageSource<Models.PublicKey> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.PublicKeys.List(parameters, requestOptions), source => PublicKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.PublicKey> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PublicKeys.FindAsync(id, parameters, requestOptions, token), source => PublicKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.PublicKey> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PublicKeys.CreateAsync(parameters, requestOptions, token), source => PublicKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.PublicKey> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.PublicKey model ? model.UpdateAsync(parameters, token) : client.PublicKeys.UpdateAsync(id, parameters, requestOptions, token), source => PublicKeyBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.PublicKey model ? model.DeleteAsync(parameters, token) : client.PublicKeys.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
