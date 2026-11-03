using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs As2OutgoingMessage operations with the SDK and projects the SDK's As2OutgoingMessage into the module's.
    internal static class As2OutgoingMessageBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.As2OutgoingMessage Project(Sdk.As2OutgoingMessage source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.As2OutgoingMessage
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                WorkspaceId = source.WorkspaceId,
                As2PartnerId = source.As2PartnerId,
                As2StationId = source.As2StationId,
                Uuid = source.Uuid,
                HttpHeaders = Values.Json(source.HttpHeaders),
                ProcessingResult = source.ProcessingResult,
                ProcessingResultDescription = source.ProcessingResultDescription,
                Mic = source.Mic,
                MicSha256 = source.MicSha256,
                As2To = source.As2To,
                As2From = source.As2From,
                As2Subject = source.As2Subject,
                Date = source.Date,
                MessageId = source.MessageId,
                BodySize = source.BodySize,
                AttachmentFilename = source.AttachmentFilename,
                CreatedAt = Values.Utc(source.CreatedAt),
                HttpResponseCode = source.HttpResponseCode,
                HttpResponseHeaders = Values.Json(source.HttpResponseHeaders),
                HttpTransmissionDuration = source.HttpTransmissionDuration,
                MdnReceived = source.MdnReceived,
                MdnValid = source.MdnValid,
                MdnSignatureVerified = source.MdnSignatureVerified,
                MdnMessageIdMatched = source.MdnMessageIdMatched,
                MdnMicMatched = source.MdnMicMatched,
                MdnProcessingSuccess = source.MdnProcessingSuccess,
                RawUri = source.RawUri,
                SmimeUri = source.SmimeUri,
                SmimeSignedUri = source.SmimeSignedUri,
                EncryptedUri = source.EncryptedUri,
                MdnResponseUri = source.MdnResponseUri,
            };
        }

        internal static FilesPageSource<Models.As2OutgoingMessage> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.As2OutgoingMessages.List(parameters, requestOptions), source => As2OutgoingMessageBridge.Project(source, connection, options));
        }
    }
}
