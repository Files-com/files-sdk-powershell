using System.IO;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Makes a FilesAttachment for a file parameter, such as Set-FilesUser -AvatarFile: a local file, bytes or a stream,
    /// with the name and media type to send it as. Nothing is read or sent.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "FilesAttachment", DefaultParameterSetName = PathSet)]
    [OutputType(typeof(FilesAttachment))]
    public sealed class NewFilesAttachmentCommand : FilesCmdlet
    {
        private const string PathSet = "Path";
        private const string LiteralPathSet = "LiteralPath";
        private const string ContentSet = "Content";
        private const string StreamSet = "Stream";

        /// <summary>The local file to attach. Wildcards are expanded, and must match one file.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = PathSet)]
        [SupportsWildcards]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>The local file to attach, used exactly as written. A file piped from Get-ChildItem binds here.</summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = LiteralPathSet)]
        [Alias("PSPath")]
        [ValidateNotNullOrEmpty]
        public string LiteralPath { get; set; }

        /// <summary>The bytes to attach. They are not copied: leave them unchanged until the command given the attachment ends.</summary>
        [Parameter(Mandatory = true, ParameterSetName = ContentSet)]
        [ValidateNotNull]
        public byte[] Content { get; set; }

        /// <summary>
        /// The stream to attach, from its position when the command given the attachment sends it. It is borrowed: the
        /// command reads it and leaves it open.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = StreamSet)]
        [ValidateNotNull]
        public Stream InputStream { get; set; }

        /// <summary>The name to send the file with, exactly as given. Defaults to the local file's name.</summary>
        [Parameter(ParameterSetName = PathSet)]
        [Parameter(ParameterSetName = LiteralPathSet)]
        [Parameter(Mandatory = true, ParameterSetName = ContentSet)]
        [Parameter(Mandatory = true, ParameterSetName = StreamSet)]
        [ValidateNotNullOrEmpty]
        public string FileName { get; set; }

        /// <summary>The media type to send the file with, such as image/png. Defaults to application/octet-stream.</summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string ContentType { get; set; }

        /// <summary>
        /// For -InputStream, the most bytes a stream that cannot seek may hold, from 1: it is copied to a private
        /// temporary file before the request is sent, and nothing is sent if it holds more. Defaults to 64 MiB. A stream
        /// that can seek is read in place and needs no limit.
        /// </summary>
        [Parameter(ParameterSetName = StreamSet)]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long MaxLength { get; set; }

        /// <summary>Writes the attachment.</summary>
        protected override void Execute()
        {
            switch (ParameterSetName)
            {
                case ContentSet:
                    WriteObject(FilesAttachment.FromBytes(Content, FileName, ContentType));
                    break;
                case StreamSet:
                    WriteObject(FilesAttachment.FromStream(InputStream, FileName, ContentType, IsBound(nameof(MaxLength)) ? MaxLength : (long?)null));
                    break;
                default:
                    string file = LocalPaths.ExistingFile(this, ParameterSetName == PathSet ? Path : LiteralPath, literal: ParameterSetName == LiteralPathSet);
                    WriteObject(FilesAttachment.FromFile(file, FileName, ContentType));
                    break;
            }
        }
    }
}
