using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AS2OutgoingMessage is a single record created for each individual AS2 file transfer out to a Partner.
    /// </summary>
    public sealed class As2OutgoingMessage : FilesObject
    {
        internal As2OutgoingMessage()
        {
        }

        /// <summary>
        /// Id of the AS2 Partner.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the Workspace associated with this AS2 Outgoing Message.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Id of the AS2 Partner associated with this message.
        /// </summary>
        public long? As2PartnerId { get; internal set; }

        /// <summary>
        /// Id of the AS2 Station associated with this message.
        /// </summary>
        public long? As2StationId { get; internal set; }

        /// <summary>
        /// UUID assigned to this message.
        /// </summary>
        public string Uuid { get; internal set; }

        /// <summary>
        /// HTTP Headers sent with this message.
        /// </summary>
        public object HttpHeaders { get; internal set; }

        /// <summary>
        /// Result of processing.
        /// </summary>
        public string ProcessingResult { get; internal set; }

        /// <summary>
        /// Result of processing description.
        /// </summary>
        public string ProcessingResultDescription { get; internal set; }

        /// <summary>
        /// AS2 Message Integrity Check SHA1
        /// </summary>
        public string Mic { get; internal set; }

        /// <summary>
        /// AS2 Message Integrity Check SHA256
        /// </summary>
        public string MicSha256 { get; internal set; }

        /// <summary>
        /// AS2 TO
        /// </summary>
        public string As2To { get; internal set; }

        /// <summary>
        /// AS2 FROM
        /// </summary>
        public string As2From { get; internal set; }

        /// <summary>
        /// AS2 Subject
        /// </summary>
        public string As2Subject { get; internal set; }

        /// <summary>
        /// Date Header
        /// </summary>
        public string Date { get; internal set; }

        /// <summary>
        /// AS2 Message Id
        /// </summary>
        public string MessageId { get; internal set; }

        /// <summary>
        /// Encrypted Payload Body Size
        /// </summary>
        public string BodySize { get; internal set; }

        /// <summary>
        /// Filename of the file being sent.
        /// </summary>
        public string AttachmentFilename { get; internal set; }

        /// <summary>
        /// Message creation date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// HTTP Response Code received for this message
        /// </summary>
        public string HttpResponseCode { get; internal set; }

        /// <summary>
        /// HTTP Headers received for this message.
        /// </summary>
        public object HttpResponseHeaders { get; internal set; }

        /// <summary>
        /// HTTP transmission duration in seceonds
        /// </summary>
        public double? HttpTransmissionDuration { get; internal set; }

        /// <summary>
        /// Did the partner give a response body?
        /// </summary>
        public bool MdnReceived { get; internal set; }

        /// <summary>
        /// Is the response in MDN format?
        /// </summary>
        public bool MdnValid { get; internal set; }

        /// <summary>
        /// MDN signature verified?
        /// </summary>
        public bool MdnSignatureVerified { get; internal set; }

        /// <summary>
        /// MDN message id matched?
        /// </summary>
        public bool MdnMessageIdMatched { get; internal set; }

        /// <summary>
        /// MDN MIC matched?
        /// </summary>
        public bool MdnMicMatched { get; internal set; }

        /// <summary>
        /// MDN disposition indicate a successful processing?
        /// </summary>
        public bool MdnProcessingSuccess { get; internal set; }

        /// <summary>
        /// URL to download the original file contents
        /// </summary>
        public string RawUri { get; internal set; }

        /// <summary>
        /// URL to download the file contents encoded as smime
        /// </summary>
        public string SmimeUri { get; internal set; }

        /// <summary>
        /// URL to download the file contents as smime with signature
        /// </summary>
        public string SmimeSignedUri { get; internal set; }

        /// <summary>
        /// URL to download the encrypted signed smime that is to sent as AS2 body
        /// </summary>
        public string EncryptedUri { get; internal set; }

        /// <summary>
        /// URL to download the http response body
        /// </summary>
        public string MdnResponseUri { get; internal set; }
    }
}
