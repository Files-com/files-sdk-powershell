using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs CustomDomain operations with the SDK and projects the SDK's CustomDomain into the module's.
    internal static class CustomDomainBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.CustomDomain Project(Sdk.CustomDomain source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.CustomDomain
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                AvailableToAllWorkspaces = source.AvailableToAllWorkspaces,
                OutboundIpAddresses = Values.Copy(source.OutboundIpAddresses),
                Domain = source.Domain,
                Destination = source.Destination,
                DnsStatus = source.DnsStatus,
                SslCertificateId = source.SslCertificateId,
                BrickManaged = source.BrickManaged,
                FolderBehaviorId = source.FolderBehaviorId,
                IpAddresses = Values.Copy(source.IpAddresses),
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.CustomDomain> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.CustomDomains.List(parameters, requestOptions), source => CustomDomainBridge.Project(source, connection, options));
        }

        internal static Task<Models.CustomDomain> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.CustomDomains.FindAsync(id, parameters, requestOptions, token), source => CustomDomainBridge.Project(source, connection, options));
        }

        internal static Task<Models.CustomDomain> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.CustomDomains.CreateAsync(parameters, requestOptions, token), source => CustomDomainBridge.Project(source, connection, options));
        }

        internal static Task<Models.CustomDomain> CreateAllocateIpAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.CustomDomains.CreateAllocateIpAsync(id, parameters, requestOptions, token), source => CustomDomainBridge.Project(source, connection, options));
        }

        internal static Task<Models.CustomDomain> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.CustomDomain model ? model.UpdateAsync(parameters, token) : client.CustomDomains.UpdateAsync(id, parameters, requestOptions, token), source => CustomDomainBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.CustomDomain model ? model.DeleteAsync(parameters, token) : client.CustomDomains.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
