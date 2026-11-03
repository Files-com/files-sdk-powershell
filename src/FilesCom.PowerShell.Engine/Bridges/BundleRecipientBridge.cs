using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs BundleRecipient operations with the SDK and projects the SDK's BundleRecipient into the module's.
    internal static class BundleRecipientBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.BundleRecipient Project(Sdk.BundleRecipient source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.BundleRecipient
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Company = source.Company,
                Name = source.Name,
                Note = source.Note,
                Recipient = source.Recipient,
                SentAt = Values.Utc(source.SentAt),
                WorkspaceId = source.WorkspaceId,
            };
        }

        internal static FilesPageSource<Models.BundleRecipient> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.BundleRecipients.List(parameters, requestOptions), source => BundleRecipientBridge.Project(source, connection, options));
        }

        internal static Task<Models.BundleRecipient> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.BundleRecipients.CreateAsync(parameters, requestOptions, token), source => BundleRecipientBridge.Project(source, connection, options));
        }
    }
}
