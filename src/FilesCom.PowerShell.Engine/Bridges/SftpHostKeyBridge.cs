using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SftpHostKey operations with the SDK and projects the SDK's SftpHostKey into the module's.
    internal static class SftpHostKeyBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SftpHostKey Project(Sdk.SftpHostKey source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SftpHostKey
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Active = source.Active,
                CustomDomainId = source.CustomDomainId,
                Id = source.Id,
                Name = source.Name,
                KeyType = source.KeyType,
                FingerprintMd5 = source.FingerprintMd5,
                FingerprintSha256 = source.FingerprintSha256,
            };
        }

        internal static FilesPageSource<Models.SftpHostKey> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SftpHostKeys.List(parameters, requestOptions), source => SftpHostKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.SftpHostKey> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SftpHostKeys.FindAsync(id, parameters, requestOptions, token), source => SftpHostKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.SftpHostKey> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SftpHostKeys.CreateAsync(parameters, requestOptions, token), source => SftpHostKeyBridge.Project(source, connection, options));
        }

        internal static Task<Models.SftpHostKey> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.SftpHostKey model ? model.UpdateAsync(parameters, token) : client.SftpHostKeys.UpdateAsync(id, parameters, requestOptions, token), source => SftpHostKeyBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.SftpHostKey model ? model.DeleteAsync(parameters, token) : client.SftpHostKeys.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
