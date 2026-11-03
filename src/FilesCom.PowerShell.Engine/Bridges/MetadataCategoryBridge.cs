using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs MetadataCategory operations with the SDK and projects the SDK's MetadataCategory into the module's.
    internal static class MetadataCategoryBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.MetadataCategory Project(Sdk.MetadataCategory source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.MetadataCategory
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Name = source.Name,
                Definitions = Values.Copy(source.Definitions),
                DefaultColumns = Values.Copy(source.DefaultColumns),
            };
        }

        internal static FilesPageSource<Models.MetadataCategory> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.MetadataCategories.List(parameters, requestOptions), source => MetadataCategoryBridge.Project(source, connection, options));
        }

        internal static Task<Models.MetadataCategory> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.MetadataCategories.FindAsync(id, parameters, requestOptions, token), source => MetadataCategoryBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.MetadataCategory> ListFor(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.MetadataCategories.ListFor(path, parameters, requestOptions), source => MetadataCategoryBridge.Project(source, connection, options));
        }

        internal static Task<Models.MetadataCategory> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.MetadataCategories.CreateAsync(parameters, requestOptions, token), source => MetadataCategoryBridge.Project(source, connection, options));
        }

        internal static Task<Models.MetadataCategory> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.MetadataCategory model ? model.UpdateAsync(parameters, token) : client.MetadataCategories.UpdateAsync(id, parameters, requestOptions, token), source => MetadataCategoryBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.MetadataCategory model ? model.DeleteAsync(parameters, token) : client.MetadataCategories.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
