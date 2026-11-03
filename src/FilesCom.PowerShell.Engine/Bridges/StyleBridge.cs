using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs Style operations with the SDK and projects the SDK's Style into the module's.
    internal static class StyleBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.Style Project(Sdk.Style source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.Style
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Path = source.Path,
                Logo = ImageBridge.Project(source.Logo, connection, options),
                LogoClickHref = source.LogoClickHref,
                Thumbnail = ImageBridge.Project(source.Thumbnail, connection, options),
            };
        }

        internal static Task<Models.Style> FindAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.Styles.FindAsync(path, parameters, requestOptions, token), source => StyleBridge.Project(source, connection, options));
        }

        internal static Task<Models.Style> UpdateAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Style model ? model.UpdateAsync(Attachments.ForEngine(parameters), token) : client.Styles.UpdateAsync(path, Attachments.ForEngine(parameters), requestOptions, token), source => StyleBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.Style model ? model.DeleteAsync(parameters, token) : client.Styles.DeleteAsync(path, parameters, requestOptions, token));
        }
    }
}
