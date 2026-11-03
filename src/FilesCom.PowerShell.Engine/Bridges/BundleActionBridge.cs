using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs BundleAction operations with the SDK and projects the SDK's BundleAction into the module's.
    internal static class BundleActionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.BundleAction Project(Sdk.BundleAction source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.BundleAction
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Action = source.Action,
                BundleRegistration = BundleRegistrationBridge.Project(source.BundleRegistration, connection, options),
                CreatedAt = Values.Utc(source.CreatedAt),
                Destination = source.Destination,
                Path = source.Path,
                Source = source.Source,
                BundleRegistrationId = source.BundleRegistrationId,
                WorkspaceId = source.WorkspaceId,
                BundleRegistrationName = source.BundleRegistrationName,
                BundleRegistrationEmail = source.BundleRegistrationEmail,
                BundleRegistrationIp = source.BundleRegistrationIp,
                FormFieldSetId = source.FormFieldSetId,
                FormFieldData = Values.Json(source.FormFieldData),
            };
        }

        internal static FilesPageSource<Models.BundleAction> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.BundleActions.List(parameters, requestOptions), source => BundleActionBridge.Project(source, connection, options));
        }
    }
}
