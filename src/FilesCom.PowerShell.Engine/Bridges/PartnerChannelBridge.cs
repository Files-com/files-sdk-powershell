using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs PartnerChannel operations with the SDK and projects the SDK's PartnerChannel into the module's.
    internal static class PartnerChannelBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PartnerChannel Project(Sdk.PartnerChannel source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PartnerChannel
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Direction = source.Direction,
                UseChannelRoot = source.UseChannelRoot,
                PartnerId = source.PartnerId,
                PartnerChannelTemplateId = source.PartnerChannelTemplateId,
                Path = source.Path,
                ToPartnerFolderName = source.ToPartnerFolderName,
                FromPartnerFolderName = source.FromPartnerFolderName,
                FromPartnerRoutePath = source.FromPartnerRoutePath,
                ToPartnerRoutePath = source.ToPartnerRoutePath,
                ToPartnerManagedFolderPaths = Values.Copy(source.ToPartnerManagedFolderPaths),
                FromPartnerManagedFolderPaths = Values.Copy(source.FromPartnerManagedFolderPaths),
                EffectiveToPartnerFolderName = source.EffectiveToPartnerFolderName,
                EffectiveFromPartnerFolderName = source.EffectiveFromPartnerFolderName,
                ChannelPath = source.ChannelPath,
                ToPartnerFolderPath = source.ToPartnerFolderPath,
                FromPartnerFolderPath = source.FromPartnerFolderPath,
            };
        }

        internal static FilesPageSource<Models.PartnerChannel> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.PartnerChannels.List(parameters, requestOptions), source => PartnerChannelBridge.Project(source, connection, options));
        }

        internal static Task<Models.PartnerChannel> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PartnerChannels.FindAsync(id, parameters, requestOptions, token), source => PartnerChannelBridge.Project(source, connection, options));
        }

        internal static Task<Models.PartnerChannel> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PartnerChannels.CreateAsync(parameters, requestOptions, token), source => PartnerChannelBridge.Project(source, connection, options));
        }

        internal static Task<Models.PartnerChannel> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.PartnerChannel model ? model.UpdateAsync(parameters, token) : client.PartnerChannels.UpdateAsync(id, parameters, requestOptions, token), source => PartnerChannelBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.PartnerChannel model ? model.DeleteAsync(parameters, token) : client.PartnerChannels.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
