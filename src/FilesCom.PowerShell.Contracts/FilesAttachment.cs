using System;
using System.IO;
using System.Net.Http.Headers;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// A file to send in a file parameter, such as <c>New-FilesUser -AvatarFile</c>: a local file, bytes or a stream,
    /// with the name and media type to send it as. New-FilesAttachment makes one.
    /// </summary>
    /// <remarks>
    /// PowerShell converts a <see cref="FileInfo"/>, such as <c>Get-Item logo.png</c>, to an attachment directly.
    /// Making an attachment reads nothing: a command opens its local file, or reads its stream, only once its change is
    /// confirmed, and sends the same bytes if it sends the request again. A stream is borrowed: commands read it and
    /// leave it open. Bytes are not copied, so leave them unchanged until the command ends.
    /// </remarks>
    public sealed class FilesAttachment
    {
        /// <summary>The most bytes read from a stream that cannot seek, unless another MaxLength is given: 64 MiB.</summary>
        public const long DefaultMaxLength = 64L * 1024 * 1024;

        /// <summary>Attaches the local file, sent under its own name.</summary>
        public FilesAttachment(FileInfo file)
            : this(CheckedFileName(file?.Name ?? throw new ArgumentNullException(nameof(file)), nameof(file)), null, FilesAttachmentSource.File)
        {
            LocalPath = file.FullName;
        }

        private FilesAttachment(string fileName, string contentType, FilesAttachmentSource sourceKind)
        {
            FileName = CheckedFileName(fileName, nameof(fileName));
            ContentType = CheckedContentType(contentType);
            SourceKind = sourceKind;
        }

        /// <summary>
        /// Attaches the local file at <paramref name="path"/>, a full path, sent as <paramref name="fileName"/> or else
        /// under its own name.
        /// </summary>
        public static FilesAttachment FromFile(string path, string fileName = null, string contentType = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("An attachment needs the path of its file.", nameof(path));
            }
            return new FilesAttachment(fileName ?? Path.GetFileName(path), contentType, FilesAttachmentSource.File) { LocalPath = path };
        }

        /// <summary>Attaches <paramref name="content"/>, sent as a file named <paramref name="fileName"/>.</summary>
        public static FilesAttachment FromBytes(byte[] content, string fileName, string contentType = null)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            return new FilesAttachment(fileName, contentType, FilesAttachmentSource.Bytes) { Content = content, Length = content.LongLength };
        }

        /// <summary>
        /// Attaches what <paramref name="stream"/> holds from its position when the command sends it, as a file named
        /// <paramref name="fileName"/>. The stream is borrowed and left open. A stream that can seek is read in place,
        /// from that position each time the request is sent. One that cannot is first copied to a private temporary
        /// file, which is deleted when the command ends; if it holds more than <paramref name="maxLength"/> bytes,
        /// <see cref="DefaultMaxLength"/> by default, nothing is sent.
        /// </summary>
        public static FilesAttachment FromStream(Stream stream, string fileName, string contentType = null, long? maxLength = null)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException("The stream cannot be read.", nameof(stream));
            }
            if (maxLength < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "An attachment's MaxLength must be at least 1.");
            }
            return new FilesAttachment(fileName, contentType, FilesAttachmentSource.Stream) { Stream = stream, MaxLength = maxLength };
        }

        /// <summary>The file name sent with the content, exactly as given.</summary>
        public string FileName { get; }

        /// <summary>The media type sent with the content, or null to send it as application/octet-stream.</summary>
        public string ContentType { get; }

        /// <summary>Where the content comes from.</summary>
        public FilesAttachmentSource SourceKind { get; }

        /// <summary>The full path of a local file attachment; otherwise null.</summary>
        public string LocalPath { get; private set; }

        /// <summary>The number of bytes of an attachment of bytes; otherwise null, since a file or stream is read when sent.</summary>
        public long? Length { get; private set; }

        /// <summary>For a stream, the most bytes it may hold if it cannot seek, or null for <see cref="DefaultMaxLength"/>.</summary>
        public long? MaxLength { get; private set; }

        internal byte[] Content { get; private set; }

        internal Stream Stream { get; private set; }

        // The engine's rules for a file name it sends: not empty, and no control character, which no request can carry.
        private static string CheckedFileName(string fileName, string argument)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("An attachment needs a file name.", argument);
            }
            foreach (char c in fileName)
            {
                if (char.IsControl(c))
                {
                    throw new ArgumentException("An attachment's file name cannot hold a control character, which no request can carry.", argument);
                }
            }
            return fileName;
        }

        private static string CheckedContentType(string contentType)
        {
            if (contentType != null && !MediaTypeHeaderValue.TryParse(contentType, out _))
            {
                throw new ArgumentException($"\"{contentType}\" is not a media type, such as image/png.", nameof(contentType));
            }
            return contentType;
        }
    }

    /// <summary>Where a <see cref="FilesAttachment"/> reads its content from.</summary>
    public enum FilesAttachmentSource
    {
        /// <summary>A local file.</summary>
        File,

        /// <summary>Bytes in memory.</summary>
        Bytes,

        /// <summary>A borrowed stream.</summary>
        Stream,
    }
}
