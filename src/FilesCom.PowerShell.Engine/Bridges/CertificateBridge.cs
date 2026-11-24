using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Certificate operations with the SDK and projects the SDK's Certificate into the module's.
    internal static class CertificateBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Certificate Project(Sdk.Certificate source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Certificate
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                CustomDomainId = source.CustomDomainId,
                CertificateType = source.CertificateType,
                CreatedAt = Values.Utc(source.CreatedAt),
                DisplayStatus = source.DisplayStatus,
                Domains = Values.Copy(source.Domains),
                ExpiresAt = Values.Utc(source.ExpiresAt),
                BrickManaged = source.BrickManaged,
                Intermediates = source.Intermediates,
                IpAddresses = Values.Copy(source.IpAddresses),
                Issuer = source.Issuer,
                KeyType = source.KeyType,
                Request = source.Request,
                Status = source.Status,
                Subject = source.Subject,
            };
        }

        internal static FilesPageSource<Models.Certificate> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Certificates.List(parameters, requestOptions), source => CertificateBridge.Project(source, connection, options));
        }

        internal static Task<Models.Certificate> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Certificates.FindAsync(id, parameters, requestOptions, token), source => CertificateBridge.Project(source, connection, options));
        }

        internal static Task<Models.Certificate> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Certificates.CreateAsync(parameters, requestOptions, token), source => CertificateBridge.Project(source, connection, options));
        }

        internal static Task ActivateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Certificate model ? model.ActivateAsync(parameters, token) : client.Certificates.ActivateAsync(id, parameters, requestOptions, token));
        }

        internal static Task DeactivateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Certificate model ? model.DeactivateAsync(parameters, token) : client.Certificates.DeactivateAsync(id, parameters, requestOptions, token));
        }

        internal static Task ReplaceWithFilesAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Certificate model ? model.ReplaceWithFilesAsync(parameters, token) : client.Certificates.ReplaceWithFilesAsync(id, parameters, requestOptions, token));
        }

        internal static Task<Models.Certificate> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Certificate model ? model.UpdateAsync(parameters, token) : client.Certificates.UpdateAsync(id, parameters, requestOptions, token), source => CertificateBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Certificate model ? model.DeleteAsync(parameters, token) : client.Certificates.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
