using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs EventChannel operations with the SDK and projects the SDK's EventChannel into the module's.
    internal static class EventChannelBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.EventChannel Project(Sdk.EventChannel source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.EventChannel
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                WorkspaceId = source.WorkspaceId,
                Description = source.Description,
                Enabled = source.Enabled,
                DefaultChannel = source.DefaultChannel,
                CreatedAt = Values.Utc(source.CreatedAt),
                UpdatedAt = Values.Utc(source.UpdatedAt),
            };
        }

        internal static FilesPageSource<Models.EventChannel> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.EventChannels.List(parameters, requestOptions), source => EventChannelBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventChannel> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.EventChannels.FindAsync(id, parameters, requestOptions, token), source => EventChannelBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventChannel> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.EventChannels.CreateAsync(parameters, requestOptions, token), source => EventChannelBridge.Project(source, connection, options));
        }

        internal static Task<Models.EventChannel> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.EventChannel model ? model.UpdateAsync(parameters, token) : client.EventChannels.UpdateAsync(id, parameters, requestOptions, token), source => EventChannelBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.EventChannel model ? model.DeleteAsync(parameters, token) : client.EventChannels.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
