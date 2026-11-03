using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs BundleRegistration operations with the SDK and projects the SDK's BundleRegistration into the module's.
    internal static class BundleRegistrationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.BundleRegistration Project(Sdk.BundleRegistration source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.BundleRegistration
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Code = source.Code,
                Name = source.Name,
                Company = source.Company,
                Email = source.Email,
                Ip = source.Ip,
                InboxCode = source.InboxCode,
                ClickwrapBody = source.ClickwrapBody,
                FormFieldSetId = source.FormFieldSetId,
                FormFieldData = Values.Json(source.FormFieldData),
                BundleCode = source.BundleCode,
                BundleId = source.BundleId,
                BundleRecipientId = source.BundleRecipientId,
                WorkspaceId = source.WorkspaceId,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.BundleRegistration> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.BundleRegistrations.List(parameters, requestOptions), source => BundleRegistrationBridge.Project(source, connection, options));
        }
    }
}
