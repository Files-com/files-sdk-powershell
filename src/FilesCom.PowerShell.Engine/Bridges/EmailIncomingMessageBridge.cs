using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs EmailIncomingMessage operations with the SDK and projects the SDK's EmailIncomingMessage into the module's.
    internal static class EmailIncomingMessageBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.EmailIncomingMessage Project(Sdk.EmailIncomingMessage source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.EmailIncomingMessage
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                InboxId = source.InboxId,
                Sender = source.Sender,
                SenderName = source.SenderName,
                Status = source.Status,
                Body = source.Body,
                Message = source.Message,
                CreatedAt = Values.Utc(source.CreatedAt),
                InboxTitle = source.InboxTitle,
            };
        }

        internal static FilesPageSource<Models.EmailIncomingMessage> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.EmailIncomingMessages.List(parameters, requestOptions), source => EmailIncomingMessageBridge.Project(source, connection, options));
        }
    }
}
