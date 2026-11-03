using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs As2IncomingMessage operations with the SDK and projects the SDK's As2IncomingMessage into the module's.
    internal static class As2IncomingMessageBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.As2IncomingMessage Project(Sdk.As2IncomingMessage source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.As2IncomingMessage
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                ReplayedFromMessageId = source.ReplayedFromMessageId,
                WorkspaceId = source.WorkspaceId,
                As2PartnerId = source.As2PartnerId,
                As2StationId = source.As2StationId,
                Uuid = source.Uuid,
                ContentType = source.ContentType,
                HttpHeaders = Values.Json(source.HttpHeaders),
                ProcessingResult = source.ProcessingResult,
                ProcessingResultDescription = source.ProcessingResultDescription,
                Mic = source.Mic,
                MicAlgo = source.MicAlgo,
                As2To = source.As2To,
                As2From = source.As2From,
                MessageId = source.MessageId,
                Subject = source.Subject,
                Date = source.Date,
                BodySize = source.BodySize,
                AttachmentFilename = source.AttachmentFilename,
                Ip = source.Ip,
                CreatedAt = Values.Utc(source.CreatedAt),
                HttpResponseCode = source.HttpResponseCode,
                HttpResponseHeaders = Values.Json(source.HttpResponseHeaders),
                RecipientSerial = source.RecipientSerial,
                HexRecipientSerial = source.HexRecipientSerial,
                RecipientIssuer = source.RecipientIssuer,
                MessageReceived = source.MessageReceived,
                MessageDecrypted = source.MessageDecrypted,
                MessageSignatureVerified = source.MessageSignatureVerified,
                MessageProcessingSuccess = source.MessageProcessingSuccess,
                MessageMdnReturned = source.MessageMdnReturned,
                EncryptedUri = source.EncryptedUri,
                SmimeSignedUri = source.SmimeSignedUri,
                SmimeUri = source.SmimeUri,
                RawUri = source.RawUri,
                MdnResponseUri = source.MdnResponseUri,
            };
        }

        internal static FilesPageSource<Models.As2IncomingMessage> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.As2IncomingMessages.List(parameters, requestOptions), source => As2IncomingMessageBridge.Project(source, connection, options));
        }

        internal static Task<Models.As2IncomingMessage> FindAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => client.As2IncomingMessages.FindAsync(id, parameters, requestOptions, token), source => As2IncomingMessageBridge.Project(source, connection, options));
        }

        internal static Task<Models.As2IncomingMessage> ReplayAsync(FilesConnection connection, long id, Dictionary<string, object> parameters, FilesCallOptions options, object retained, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, options, cancellationToken, (client, requestOptions, token) => retained is Sdk.As2IncomingMessage model ? model.ReplayAsync(parameters, token) : client.As2IncomingMessages.ReplayAsync(id, parameters, requestOptions, token), source => As2IncomingMessageBridge.Project(source, connection, options));
        }
    }
}
