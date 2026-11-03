using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs InboxRecipient operations with the SDK and projects the SDK's InboxRecipient into the module's.
    internal static class InboxRecipientBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.InboxRecipient Project(Sdk.InboxRecipient source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.InboxRecipient
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Company = source.Company,
                Name = source.Name,
                Note = source.Note,
                Recipient = source.Recipient,
                SentAt = Values.Utc(source.SentAt),
            };
        }

        internal static FilesPageSource<Models.InboxRecipient> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.InboxRecipients.List(parameters, requestOptions), source => InboxRecipientBridge.Project(source, connection, options));
        }

        internal static Task<Models.InboxRecipient> CreateAsync(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.InboxRecipients.CreateAsync(parameters, requestOptions, token), source => InboxRecipientBridge.Project(source, connection, options));
        }
    }
}
