using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs As2Partner operations with the SDK and projects the SDK's As2Partner into the module's.
    internal static class As2PartnerBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.As2Partner Project(Sdk.As2Partner source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.As2Partner
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                As2StationId = source.As2StationId,
                Name = source.Name,
                Uri = source.Uri,
                ServerCertificate = source.ServerCertificate,
                HttpAuthUsername = source.HttpAuthUsername,
                AdditionalHttpHeaders = Values.Json(source.AdditionalHttpHeaders),
                DefaultMimeType = source.DefaultMimeType,
                MdnValidationLevel = source.MdnValidationLevel,
                SignatureValidationLevel = source.SignatureValidationLevel,
                EnableDedicatedIps = source.EnableDedicatedIps,
                HexPublicCertificateSerial = source.HexPublicCertificateSerial,
                PublicCertificate = source.PublicCertificate,
                PublicCertificateMd5 = source.PublicCertificateMd5,
                PublicCertificateSubject = source.PublicCertificateSubject,
                PublicCertificateIssuer = source.PublicCertificateIssuer,
                PublicCertificateSerial = source.PublicCertificateSerial,
                PublicCertificateNotBefore = source.PublicCertificateNotBefore,
                PublicCertificateNotAfter = source.PublicCertificateNotAfter,
            };
        }

        internal static FilesPageSource<Models.As2Partner> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.As2Partners.List(parameters, requestOptions), source => As2PartnerBridge.Project(source, connection, options));
        }

        internal static Task<Models.As2Partner> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.As2Partners.FindAsync(id, parameters, requestOptions, token), source => As2PartnerBridge.Project(source, connection, options));
        }

        internal static Task<Models.As2Partner> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.As2Partners.CreateAsync(parameters, requestOptions, token), source => As2PartnerBridge.Project(source, connection, options));
        }

        internal static Task<Models.As2Partner> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.As2Partner model ? model.UpdateAsync(parameters, token) : client.As2Partners.UpdateAsync(id, parameters, requestOptions, token), source => As2PartnerBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.As2Partner model ? model.DeleteAsync(parameters, token) : client.As2Partners.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
