using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Clickwrap operations with the SDK and projects the SDK's Clickwrap into the module's.
    internal static class ClickwrapBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Clickwrap Project(Sdk.Clickwrap source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Clickwrap
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                Body = source.Body,
                UseWithUsers = source.UseWithUsers,
                UseWithBundles = source.UseWithBundles,
                UseWithInboxes = source.UseWithInboxes,
            };
        }

        internal static FilesPageSource<Models.Clickwrap> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Clickwraps.List(parameters, requestOptions), source => ClickwrapBridge.Project(source, connection, options));
        }

        internal static Task<Models.Clickwrap> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Clickwraps.FindAsync(id, parameters, requestOptions, token), source => ClickwrapBridge.Project(source, connection, options));
        }

        internal static Task<Models.Clickwrap> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Clickwraps.CreateAsync(parameters, requestOptions, token), source => ClickwrapBridge.Project(source, connection, options));
        }

        internal static Task<Models.Clickwrap> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Clickwrap model ? model.UpdateAsync(parameters, token) : client.Clickwraps.UpdateAsync(id, parameters, requestOptions, token), source => ClickwrapBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Clickwrap model ? model.DeleteAsync(parameters, token) : client.Clickwraps.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
