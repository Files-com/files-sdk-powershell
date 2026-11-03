using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs InboxRegistration operations with the SDK and projects the SDK's InboxRegistration into the module's.
    internal static class InboxRegistrationBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.InboxRegistration Project(Sdk.InboxRegistration source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.InboxRegistration
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Code = source.Code,
                Name = source.Name,
                Company = source.Company,
                Email = source.Email,
                Ip = source.Ip,
                ClickwrapBody = source.ClickwrapBody,
                FormFieldSetId = source.FormFieldSetId,
                FormFieldData = Values.Json(source.FormFieldData),
                InboxId = source.InboxId,
                InboxRecipientId = source.InboxRecipientId,
                InboxTitle = source.InboxTitle,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.InboxRegistration> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.InboxRegistrations.List(parameters, requestOptions), source => InboxRegistrationBridge.Project(source, connection, options));
        }
    }
}
