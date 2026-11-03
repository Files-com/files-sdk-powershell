using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs SiteSubdomainRedirect operations with the SDK and projects the SDK's SiteSubdomainRedirect into the module's.
    internal static class SiteSubdomainRedirectBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.SiteSubdomainRedirect Project(Sdk.SiteSubdomainRedirect source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.SiteSubdomainRedirect
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Subdomain = source.Subdomain,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.SiteSubdomainRedirect> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.SiteSubdomainRedirects.List(parameters, requestOptions), source => SiteSubdomainRedirectBridge.Project(source, connection, options));
        }

        internal static Task<Models.SiteSubdomainRedirect> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.SiteSubdomainRedirects.FindAsync(id, parameters, requestOptions, token), source => SiteSubdomainRedirectBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.SiteSubdomainRedirect model ? model.DeleteAsync(parameters, token) : client.SiteSubdomainRedirects.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
