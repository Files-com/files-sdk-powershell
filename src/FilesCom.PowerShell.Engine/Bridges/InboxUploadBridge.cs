using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs InboxUpload operations with the SDK and projects the SDK's InboxUpload into the module's.
    internal static class InboxUploadBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.InboxUpload Project(Sdk.InboxUpload source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.InboxUpload
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                InboxRegistration = InboxRegistrationBridge.Project(source.InboxRegistration, connection, options),
                Path = source.Path,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.InboxUpload> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.InboxUploads.List(parameters, requestOptions), source => InboxUploadBridge.Project(source, connection, options));
        }
    }
}
