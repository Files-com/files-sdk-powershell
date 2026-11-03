using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs PartnerChannelTemplate operations with the SDK and projects the SDK's PartnerChannelTemplate into the module's.
    internal static class PartnerChannelTemplateBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PartnerChannelTemplate Project(Sdk.PartnerChannelTemplate source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PartnerChannelTemplate
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                Direction = source.Direction,
                UseChannelRoot = source.UseChannelRoot,
                Name = source.Name,
                Path = source.Path,
                ToPartnerFolderName = source.ToPartnerFolderName,
                FromPartnerFolderName = source.FromPartnerFolderName,
                FromPartnerRoutePathPattern = source.FromPartnerRoutePathPattern,
                ToPartnerRoutePathPattern = source.ToPartnerRoutePathPattern,
                ToPartnerManagedFolderPaths = Values.Copy(source.ToPartnerManagedFolderPaths),
                FromPartnerManagedFolderPaths = Values.Copy(source.FromPartnerManagedFolderPaths),
                EffectiveToPartnerFolderName = source.EffectiveToPartnerFolderName,
                EffectiveFromPartnerFolderName = source.EffectiveFromPartnerFolderName,
            };
        }

        internal static FilesPageSource<Models.PartnerChannelTemplate> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.PartnerChannelTemplates.List(parameters, requestOptions), source => PartnerChannelTemplateBridge.Project(source, connection, options));
        }

        internal static Task<Models.PartnerChannelTemplate> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PartnerChannelTemplates.FindAsync(id, parameters, requestOptions, token), source => PartnerChannelTemplateBridge.Project(source, connection, options));
        }

        internal static Task<Models.PartnerChannelTemplate> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.PartnerChannelTemplates.CreateAsync(parameters, requestOptions, token), source => PartnerChannelTemplateBridge.Project(source, connection, options));
        }

        internal static Task<Models.PartnerChannelTemplate> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.PartnerChannelTemplate model ? model.UpdateAsync(parameters, token) : client.PartnerChannelTemplates.UpdateAsync(id, parameters, requestOptions, token), source => PartnerChannelTemplateBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.PartnerChannelTemplate model ? model.DeleteAsync(parameters, token) : client.PartnerChannelTemplates.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
