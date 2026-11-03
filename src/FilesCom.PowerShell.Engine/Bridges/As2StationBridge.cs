using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs As2Station operations with the SDK and projects the SDK's As2Station into the module's.
    internal static class As2StationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.As2Station Project(Sdk.As2Station source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.As2Station
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Name = source.Name,
                Uri = source.Uri,
                Domain = source.Domain,
                HexPublicCertificateSerial = source.HexPublicCertificateSerial,
                PublicCertificateMd5 = source.PublicCertificateMd5,
                PublicCertificate = source.PublicCertificate,
                PrivateKeyMd5 = source.PrivateKeyMd5,
                PublicCertificateSubject = source.PublicCertificateSubject,
                PublicCertificateIssuer = source.PublicCertificateIssuer,
                PublicCertificateSerial = source.PublicCertificateSerial,
                PublicCertificateNotBefore = source.PublicCertificateNotBefore,
                PublicCertificateNotAfter = source.PublicCertificateNotAfter,
                PrivateKeyPasswordMd5 = source.PrivateKeyPasswordMd5,
            };
        }

        internal static FilesPageSource<Models.As2Station> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.As2Stations.List(parameters, requestOptions), source => As2StationBridge.Project(source, connection, options));
        }

        internal static Task<Models.As2Station> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.As2Stations.FindAsync(id, parameters, requestOptions, token), source => As2StationBridge.Project(source, connection, options));
        }

        internal static Task<Models.As2Station> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.As2Stations.CreateAsync(parameters, requestOptions, token), source => As2StationBridge.Project(source, connection, options));
        }

        internal static Task<Models.As2Station> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.As2Station model ? model.UpdateAsync(parameters, token) : client.As2Stations.UpdateAsync(id, parameters, requestOptions, token), source => As2StationBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.As2Station model ? model.DeleteAsync(parameters, token) : client.As2Stations.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
