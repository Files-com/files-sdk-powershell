using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs PartnerSiteRequest operations with the SDK and projects the SDK's PartnerSiteRequest into the module's.
    internal static class PartnerSiteRequestBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PartnerSiteRequest Project(Sdk.PartnerSiteRequest source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PartnerSiteRequest
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                HostPartnerId = source.HostPartnerId,
                GuestSiteUrl = source.GuestSiteUrl,
                Status = source.Status,
                HostSiteName = source.HostSiteName,
                PairingKey = Values.Secret(source.PairingKey),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.PartnerSiteRequest> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.PartnerSiteRequests.List(parameters, requestOptions), source => PartnerSiteRequestBridge.Project(source, connection, options));
        }

        internal static Task FindByPairingKeyAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PartnerSiteRequests.FindByPairingKeyAsync(parameters, requestOptions, token));
        }

        internal static Task<Models.PartnerSiteRequest> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PartnerSiteRequests.CreateAsync(parameters, requestOptions, token), source => PartnerSiteRequestBridge.Project(source, connection, options));
        }

        internal static Task RejectAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PartnerSiteRequests.RejectAsync(parameters, requestOptions, token));
        }

        internal static Task ApproveAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PartnerSiteRequests.ApproveAsync(parameters, requestOptions, token));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.PartnerSiteRequest model ? model.DeleteAsync(parameters, token) : client.PartnerSiteRequests.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
