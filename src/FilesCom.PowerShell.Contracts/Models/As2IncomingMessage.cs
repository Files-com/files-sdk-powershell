using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AS2IncomingMessage is a single record created for each individual AS2 file transfer incoming from a Partner.
    /// </summary>
    public sealed class As2IncomingMessage : FilesObject
    {
        internal As2IncomingMessage()
        {
        }

        /// <summary>
        /// ID of this AS2 incoming message record.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// ID of the incoming message record this message was replayed from. Empty for messages received directly from a trading partner.
        /// </summary>
        public long? ReplayedFromMessageId { get; internal set; }

        /// <summary>
        /// ID of the Workspace associated with this AS2 Incoming Message.
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
        /// Content Type header of the incoming message.
        /// </summary>
        public string ContentType { get; internal set; }

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
        /// AS2 Message Integrity Check
        /// </summary>
        public string Mic { get; internal set; }

        /// <summary>
        /// AS2 Message Integrity Check Algorithm Used
        /// </summary>
        public string MicAlgo { get; internal set; }

        /// <summary>
        /// AS2 TO header of message
        /// </summary>
        public string As2To { get; internal set; }

        /// <summary>
        /// AS2 FROM header of message
        /// </summary>
        public string As2From { get; internal set; }

        /// <summary>
        /// AS2 Message Id
        /// </summary>
        public string MessageId { get; internal set; }

        /// <summary>
        /// AS2 Subject Header
        /// </summary>
        public string Subject { get; internal set; }

        /// <summary>
        /// Date Header
        /// </summary>
        public string Date { get; internal set; }

        /// <summary>
        /// Encrypted Payload Body Size
        /// </summary>
        public string BodySize { get; internal set; }

        /// <summary>
        /// Filename of the file being received.
        /// </summary>
        public string AttachmentFilename { get; internal set; }

        /// <summary>
        /// IP Address of the Sender
        /// </summary>
        public string Ip { get; internal set; }

        /// <summary>
        /// Message creation date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// HTTP Response Code sent for this message
        /// </summary>
        public string HttpResponseCode { get; internal set; }

        /// <summary>
        /// HTTP Headers sent for this message.
        /// </summary>
        public object HttpResponseHeaders { get; internal set; }

        /// <summary>
        /// Incoming Message Recipient(the Client Cert used to encrypt this message)'s serial
        /// </summary>
        public string RecipientSerial { get; internal set; }

        /// <summary>
        /// Incoming Message Recipient(the Client Cert used to encrypt this message)'s serial in hex format.
        /// </summary>
        public string HexRecipientSerial { get; internal set; }

        /// <summary>
        /// Incoming Message Recipient(the Client Cert used to encrypt this message)'s issuer
        /// </summary>
        public string RecipientIssuer { get; internal set; }

        /// <summary>
        /// Message body received?
        /// </summary>
        public bool MessageReceived { get; internal set; }

        /// <summary>
        /// Message decrypted successfully?
        /// </summary>
        public bool MessageDecrypted { get; internal set; }

        /// <summary>
        /// Message signature verified?
        /// </summary>
        public bool MessageSignatureVerified { get; internal set; }

        /// <summary>
        /// Message processed successfully?
        /// </summary>
        public bool MessageProcessingSuccess { get; internal set; }

        /// <summary>
        /// MDN returned?
        /// </summary>
        public bool MessageMdnReturned { get; internal set; }

        /// <summary>
        /// URL to download the encrypted signed smime that is to sent as AS2 body
        /// </summary>
        public string EncryptedUri { get; internal set; }

        /// <summary>
        /// URL to download the file contents as smime with signature
        /// </summary>
        public string SmimeSignedUri { get; internal set; }

        /// <summary>
        /// URL to download the file contents encoded as smime
        /// </summary>
        public string SmimeUri { get; internal set; }

        /// <summary>
        /// URL to download the original file contents
        /// </summary>
        public string RawUri { get; internal set; }

        /// <summary>
        /// URL to download the http response body
        /// </summary>
        public string MdnResponseUri { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(As2IncomingMessage source)
        {
            Id = source.Id;
            ReplayedFromMessageId = source.ReplayedFromMessageId;
            WorkspaceId = source.WorkspaceId;
            As2PartnerId = source.As2PartnerId;
            As2StationId = source.As2StationId;
            Uuid = source.Uuid;
            ContentType = source.ContentType;
            HttpHeaders = source.HttpHeaders;
            ProcessingResult = source.ProcessingResult;
            ProcessingResultDescription = source.ProcessingResultDescription;
            Mic = source.Mic;
            MicAlgo = source.MicAlgo;
            As2To = source.As2To;
            As2From = source.As2From;
            MessageId = source.MessageId;
            Subject = source.Subject;
            Date = source.Date;
            BodySize = source.BodySize;
            AttachmentFilename = source.AttachmentFilename;
            Ip = source.Ip;
            CreatedAt = source.CreatedAt;
            HttpResponseCode = source.HttpResponseCode;
            HttpResponseHeaders = source.HttpResponseHeaders;
            RecipientSerial = source.RecipientSerial;
            HexRecipientSerial = source.HexRecipientSerial;
            RecipientIssuer = source.RecipientIssuer;
            MessageReceived = source.MessageReceived;
            MessageDecrypted = source.MessageDecrypted;
            MessageSignatureVerified = source.MessageSignatureVerified;
            MessageProcessingSuccess = source.MessageProcessingSuccess;
            MessageMdnReturned = source.MessageMdnReturned;
            EncryptedUri = source.EncryptedUri;
            SmimeSignedUri = source.SmimeSignedUri;
            SmimeUri = source.SmimeUri;
            RawUri = source.RawUri;
            MdnResponseUri = source.MdnResponseUri;
        }
    }
}
