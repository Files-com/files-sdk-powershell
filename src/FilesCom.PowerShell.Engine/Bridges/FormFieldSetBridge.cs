using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs FormFieldSet operations with the SDK and projects the SDK's FormFieldSet into the module's.
    internal static class FormFieldSetBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FormFieldSet Project(Sdk.FormFieldSet source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FormFieldSet
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Title = source.Title,
                RequireCompleteMetadataCapture = source.RequireCompleteMetadataCapture,
                FormLayout = Values.Copy(source.FormLayout),
                FormFields = Values.Array(source.FormFields, item => FormFieldBridge.Project(item, connection, options)),
                SkipName = source.SkipName,
                SkipEmail = source.SkipEmail,
                SkipCompany = source.SkipCompany,
                InUse = source.InUse,
            };
        }

        internal static FilesPageSource<Models.FormFieldSet> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.FormFieldSets.List(parameters, requestOptions), source => FormFieldSetBridge.Project(source, connection, options));
        }

        internal static Task<Models.FormFieldSet> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FormFieldSets.FindAsync(id, parameters, requestOptions, token), source => FormFieldSetBridge.Project(source, connection, options));
        }

        internal static Task<Models.FormFieldSet> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.FormFieldSets.CreateAsync(parameters, requestOptions, token), source => FormFieldSetBridge.Project(source, connection, options));
        }

        internal static Task<Models.FormFieldSet> UpdateAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.FormFieldSet model ? model.UpdateAsync(parameters, token) : client.FormFieldSets.UpdateAsync(id, parameters, requestOptions, token), source => FormFieldSetBridge.Project(source, connection, options));
        }

        internal static Task DeleteAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.FormFieldSet model ? model.DeleteAsync(parameters, token) : client.FormFieldSets.DeleteAsync(id, parameters, requestOptions, token));
        }
    }
}
