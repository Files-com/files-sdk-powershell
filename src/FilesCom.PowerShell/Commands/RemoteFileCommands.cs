using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Download File.
    /// </summary>
    [Cmdlet("Get", "FilesDownloadInfo", DefaultParameterSetName = "Path")]
    [OutputType(typeof(Models.RemoteFile), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class GetFilesDownloadInfoCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Can be blank, `redirect` or `stat`.  If set to `stat`, we will return file information but without a download URL, and without logging a download.  If set to `redirect` we will serve a 302 redirect directly to the file.  This is used for integrations with Zapier, and is not recommended for most integrations.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Action { get; set; }

        /// <summary>
        /// Request a preview size.  Can be `small` (default), `large`, `xlarge`, or `pdf`.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PreviewSize { get; set; }

        /// <summary>
        /// Include file preview information?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? WithPreviews { get; set; }

        /// <summary>
        /// Include file priority color information?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? WithPriorityColor { get; set; }

        /// <summary>
        /// Include optional direct connection information for a direct Agent transfer attempt?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? WithDirectConnectionInfo { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Download File.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = DownloadParameters();
            Models.RemoteFile result = Run(connection, token => RemoteFileBridge.DownloadAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> DownloadParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Action)))
            {
                parameters["action"] = Action;
            }
            if (IsBound(nameof(PreviewSize)))
            {
                parameters["preview_size"] = PreviewSize;
            }
            if (IsBound(nameof(WithPreviews)))
            {
                parameters["with_previews"] = WithPreviews.Value;
            }
            if (IsBound(nameof(WithPriorityColor)))
            {
                parameters["with_priority_color"] = WithPriorityColor.Value;
            }
            if (IsBound(nameof(WithDirectConnectionInfo)))
            {
                parameters["with_direct_connection_info"] = WithDirectConnectionInfo.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Upload File.
    /// </summary>
    [Cmdlet("New", "FilesFile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.RemoteFile))]
    public sealed class NewFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// The action to perform.  Can be `append`, `attachment`, `end`, `upload`, `put`, or may not exist
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Action { get; set; }

        /// <summary>
        /// Custom metadata map to save when `action=end` completes the upload.  Replaces existing metadata; an empty map clears it.  No separate metadata-edit permission is required.  Supported on native files and configured remote mounts, excluding remote server automount paths.  Limited to 32 keys, 256 characters per key and 1024 characters per value.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary CustomMetadata { get; set; }

        /// <summary>
        /// etag identifier.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] EtagsEtag { get; set; }

        /// <summary>
        /// Part number.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] EtagsPart { get; set; }

        /// <summary>
        /// Length of file.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Length { get; set; }

        /// <summary>
        /// Create parent directories if they do not exist?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? MkdirParents { get; set; }

        /// <summary>
        /// Part if uploading a part.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Part { get; set; }

        /// <summary>
        /// How many parts to fetch?
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Parts { get; set; }

        /// <summary>
        /// User provided modification time.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ProvidedMtime { get; set; }

        /// <summary>
        /// ref
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Ref { get; set; }

        /// <summary>
        /// File byte offset to restart from.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Restart { get; set; }

        /// <summary>
        /// Size of file.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Size { get; set; }

        /// <summary>
        /// If copying a folder, also copy supported behaviors, email notification subscriptions, and per-folder branding to the destination folder tree?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? CopyBehaviors { get; set; }

        /// <summary>
        /// If copying folder, copy just the structure?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Structure { get; set; }

        /// <summary>
        /// Allow file rename instead of overwrite?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WithRename { get; set; }

        /// <summary>
        /// If true, and the path refers to a destination not stored on Files.com (such as a remote server mount), the upload will be uploaded first to Files.com before being sent to the remote server mount. This can allow clients to upload using parallel parts to a remote server destination that does not offer parallel parts support natively.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BufferedUpload { get; set; }

        /// <summary>
        /// Include optional direct connection information for a direct Agent transfer attempt?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WithDirectConnectionInfo { get; set; }

        /// <summary>Upload File.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            string path = Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.RemoteFile result = Run(connection, token => RemoteFileBridge.CreateAsync(connection, path, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Action)))
            {
                parameters["action"] = Action;
            }
            if (IsBound(nameof(CustomMetadata)))
            {
                parameters["custom_metadata"] = RequestValues.Object(CustomMetadata, "CustomMetadata");
            }
            if (IsBound(nameof(EtagsEtag)))
            {
                parameters["etags[etag]"] = RequestValues.Strings(EtagsEtag, "EtagsEtag");
            }
            if (IsBound(nameof(EtagsPart)))
            {
                parameters["etags[part]"] = RequestValues.Int64s(EtagsPart);
            }
            if (IsBound(nameof(Length)))
            {
                parameters["length"] = Length.Value;
            }
            if (IsBound(nameof(MkdirParents)))
            {
                parameters["mkdir_parents"] = MkdirParents.Value;
            }
            if (IsBound(nameof(Part)))
            {
                parameters["part"] = Part.Value;
            }
            if (IsBound(nameof(Parts)))
            {
                parameters["parts"] = Parts.Value;
            }
            if (IsBound(nameof(ProvidedMtime)))
            {
                parameters["provided_mtime"] = RequestValues.DateTime(ProvidedMtime.Value);
            }
            if (IsBound(nameof(Ref)))
            {
                parameters["ref"] = Ref;
            }
            if (IsBound(nameof(Restart)))
            {
                parameters["restart"] = Restart.Value;
            }
            if (IsBound(nameof(Size)))
            {
                parameters["size"] = Size.Value;
            }
            if (IsBound(nameof(CopyBehaviors)))
            {
                parameters["copy_behaviors"] = CopyBehaviors.Value;
            }
            if (IsBound(nameof(Structure)))
            {
                parameters["structure"] = Structure;
            }
            if (IsBound(nameof(WithRename)))
            {
                parameters["with_rename"] = WithRename.Value;
            }
            if (IsBound(nameof(BufferedUpload)))
            {
                parameters["buffered_upload"] = BufferedUpload.Value;
            }
            if (IsBound(nameof(WithDirectConnectionInfo)))
            {
                parameters["with_direct_connection_info"] = WithDirectConnectionInfo.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update File/Folder Metadata.
    /// </summary>
    [Cmdlet("Set", "FilesFile", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.RemoteFile), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class SetFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Custom metadata map. Replaces existing metadata; an empty map clears it. Requires Full or Admin permission. Uploaders with Write permission can supply custom metadata when completing an upload instead.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary CustomMetadata { get; set; }

        /// <summary>
        /// Modified time of file.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ProvidedMtime { get; set; }

        /// <summary>
        /// Priority/Bookmark color of file.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PriorityColor { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Update File/Folder Metadata.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.RemoteFile result = Run(connection, token => RemoteFileBridge.UpdateAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(CustomMetadata)))
            {
                parameters["custom_metadata"] = RequestValues.Object(CustomMetadata, "CustomMetadata");
            }
            if (IsBound(nameof(ProvidedMtime)))
            {
                parameters["provided_mtime"] = RequestValues.DateTime(ProvidedMtime.Value);
            }
            if (IsBound(nameof(PriorityColor)))
            {
                parameters["priority_color"] = PriorityColor;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete File/Folder.
    /// </summary>
    [Cmdlet("Remove", "FilesFile", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// If true, will recursively delete folders.  Otherwise, will error on non-empty folders.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Recursive { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Delete File/Folder.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Run(connection, token => RemoteFileBridge.DeleteAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Recursive)))
            {
                parameters["recursive"] = Recursive.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// List the contents of a ZIP file.
    /// </summary>
    [Cmdlet("Get", "FilesArchiveEntry", DefaultParameterSetName = "Path")]
    [OutputType(typeof(Models.ZipListEntry), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class GetFilesArchiveEntryCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>List the contents of a ZIP file.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = ZipListContentsParameters();
            foreach (Models.ZipListEntry item in Run(connection, token => RemoteFileBridge.ZipListContentsAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token)))
            {
                WriteObject(item);
            }
        }

        private Dictionary<string, object> ZipListContentsParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Find File/Folder by Path.
    /// </summary>
    [Cmdlet("Get", "FilesFile")]
    [OutputType(typeof(Models.RemoteFile))]
    public sealed class GetFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Request a preview size.  Can be `small` (default), `large`, `xlarge`, or `pdf`.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PreviewSize { get; set; }

        /// <summary>
        /// Include file preview information?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WithPreviews { get; set; }

        /// <summary>
        /// Include file priority color information?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? WithPriorityColor { get; set; }

        /// <summary>Find File/Folder by Path.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            string path = Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = FindParameters();
            Models.RemoteFile result = Run(connection, token => RemoteFileBridge.FindAsync(connection, path, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(PreviewSize)))
            {
                parameters["preview_size"] = PreviewSize;
            }
            if (IsBound(nameof(WithPreviews)))
            {
                parameters["with_previews"] = WithPreviews.Value;
            }
            if (IsBound(nameof(WithPriorityColor)))
            {
                parameters["with_priority_color"] = WithPriorityColor.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create a ZIP from one or more paths and save it to a destination path.
    /// </summary>
    [Cmdlet("Compress", "FilesArchive", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileAction))]
    public sealed class CompressFilesArchiveCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Paths to include in the ZIP.
        /// </summary>
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public string[] Paths { get; set; }

        /// <summary>
        /// Destination file path for the ZIP.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// Archive format to create.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ArchiveFormat { get; set; }

        /// <summary>
        /// Overwrite existing file in the destination?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Overwrite { get; set; }

        /// <summary>Create a ZIP from one or more paths and save it to a destination path.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = ZipParameters();
            if (!ShouldProcessOn(connection, "File"))
            {
                return;
            }
            Models.FileAction result = Run(connection, token => RemoteFileBridge.ZipAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> ZipParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Paths)))
            {
                parameters["paths"] = RequestValues.Strings(Paths, "Paths");
            }
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(ArchiveFormat)))
            {
                parameters["archive_format"] = ArchiveFormat;
            }
            if (IsBound(nameof(Overwrite)))
            {
                parameters["overwrite"] = Overwrite.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Extract a ZIP file to a destination folder.
    /// </summary>
    [Cmdlet("Expand", "FilesArchive", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileAction), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class ExpandFilesArchiveCommand : FilesApiCmdlet
    {
        /// <summary>
        /// ZIP file path to extract.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Destination folder path for extracted files.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// Optional single entry filename to extract.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Filename { get; set; }

        /// <summary>
        /// Overwrite existing files in the destination?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Overwrite { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Extract a ZIP file to a destination folder.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = UnzipParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.FileAction result = Run(connection, token => RemoteFileBridge.UnzipAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> UnzipParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(Filename)))
            {
                parameters["filename"] = Filename;
            }
            if (IsBound(nameof(Overwrite)))
            {
                parameters["overwrite"] = Overwrite.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Encrypt a file with GPG and save it to a destination path.
    /// </summary>
    [Cmdlet("Protect", "FilesFile", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileAction), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class ProtectFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Destination file path for the encrypted file.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// GPG Key IDs to encrypt with.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GpgKeyIds { get; set; }

        /// <summary>
        /// Partner ID whose GPG keys should be used for encryption.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? GpgKeyPartnerId { get; set; }

        /// <summary>
        /// Optional GPG Key ID to sign with.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? SigningKeyId { get; set; }

        /// <summary>
        /// Output ASCII-armored encrypted data.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Armor { get; set; }

        /// <summary>
        /// Overwrite existing file in the destination?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Overwrite { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Encrypt a file with GPG and save it to a destination path.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = GpgEncryptParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.FileAction result = Run(connection, token => RemoteFileBridge.GpgEncryptAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> GpgEncryptParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(GpgKeyIds)))
            {
                parameters["gpg_key_ids"] = RequestValues.Int64s(GpgKeyIds);
            }
            if (IsBound(nameof(GpgKeyPartnerId)))
            {
                parameters["gpg_key_partner_id"] = GpgKeyPartnerId.Value;
            }
            if (IsBound(nameof(SigningKeyId)))
            {
                parameters["signing_key_id"] = SigningKeyId.Value;
            }
            if (IsBound(nameof(Armor)))
            {
                parameters["armor"] = Armor.Value;
            }
            if (IsBound(nameof(Overwrite)))
            {
                parameters["overwrite"] = Overwrite.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Decrypt a GPG-encrypted file and save it to a destination path.
    /// </summary>
    [Cmdlet("Unprotect", "FilesFile", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileAction), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class UnprotectFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Destination file path for the decrypted file.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// GPG Key IDs to decrypt with. If omitted, every accessible private GPG key in the source workspace is used.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] GpgKeyIds { get; set; }

        /// <summary>
        /// Partner ID whose GPG keys should be used for decryption.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? GpgKeyPartnerId { get; set; }

        /// <summary>
        /// Use every accessible private GPG key in the source workspace for decryption.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? UseAllPrivateKeys { get; set; }

        /// <summary>
        /// Ignore errors from the MDC (modification detection code) check.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? IgnoreMdcError { get; set; }

        /// <summary>
        /// Overwrite existing file in the destination?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Overwrite { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Decrypt a GPG-encrypted file and save it to a destination path.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = GpgDecryptParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.FileAction result = Run(connection, token => RemoteFileBridge.GpgDecryptAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> GpgDecryptParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(GpgKeyIds)))
            {
                parameters["gpg_key_ids"] = RequestValues.Int64s(GpgKeyIds);
            }
            if (IsBound(nameof(GpgKeyPartnerId)))
            {
                parameters["gpg_key_partner_id"] = GpgKeyPartnerId.Value;
            }
            if (IsBound(nameof(UseAllPrivateKeys)))
            {
                parameters["use_all_private_keys"] = UseAllPrivateKeys.Value;
            }
            if (IsBound(nameof(IgnoreMdcError)))
            {
                parameters["ignore_mdc_error"] = IgnoreMdcError.Value;
            }
            if (IsBound(nameof(Overwrite)))
            {
                parameters["overwrite"] = Overwrite.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Transform a file and save the output to a destination path.
    /// </summary>
    [Cmdlet("Convert", "FilesFile", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileAction), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class ConvertFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Destination file path for the transformed output.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// Transform type. Supported values are `image_convert`, `document_convert`, and `files_transform_script_execute`.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string TransformType { get; set; }

        /// <summary>
        /// Destination format to create.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string TargetFormat { get; set; }

        /// <summary>
        /// Files TransformScript source. Required when transform_type is `files_transform_script_execute`.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Script { get; set; }

        /// <summary>
        /// Maximum output width for image_convert.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Width { get; set; }

        /// <summary>
        /// Maximum output height for image_convert.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Height { get; set; }

        /// <summary>
        /// Overwrite existing file in the destination?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Overwrite { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Transform a file and save the output to a destination path.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = TransformParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.FileAction result = Run(connection, token => RemoteFileBridge.TransformAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> TransformParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(TransformType)))
            {
                parameters["transform_type"] = TransformType;
            }
            if (IsBound(nameof(TargetFormat)))
            {
                parameters["target_format"] = TargetFormat;
            }
            if (IsBound(nameof(Script)))
            {
                parameters["script"] = Script;
            }
            if (IsBound(nameof(Width)))
            {
                parameters["width"] = Width.Value;
            }
            if (IsBound(nameof(Height)))
            {
                parameters["height"] = Height.Value;
            }
            if (IsBound(nameof(Overwrite)))
            {
                parameters["overwrite"] = Overwrite.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Move File/Folder.
    /// </summary>
    [Cmdlet("Move", "FilesFile", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileAction), ParameterSetName = new[] { "Path", "InputObject", "RemoteServer", "RemoteServerInputObject", "Snapshot", "SnapshotInputObject", "ChildSite", "ChildSiteInputObject" })]
    public sealed class MoveFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "RemoteServer")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Snapshot")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ChildSite")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Move destination path.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// Overwrite existing file(s) in the destination?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [Parameter(ParameterSetName = "RemoteServer")]
        [Parameter(ParameterSetName = "RemoteServerInputObject")]
        [Parameter(ParameterSetName = "Snapshot")]
        [Parameter(ParameterSetName = "SnapshotInputObject")]
        [Parameter(ParameterSetName = "ChildSite")]
        [Parameter(ParameterSetName = "ChildSiteInputObject")]
        [ValidateNotNull]
        public bool? Overwrite { get; set; }

        /// <summary>
        /// Sends the result to the remote server with this ID instead of a folder of this site.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "RemoteServer")]
        [Parameter(Mandatory = true, ParameterSetName = "RemoteServerInputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long RemoteServerId { get; set; }

        /// <summary>
        /// Sends the result to the snapshot with this ID instead of a folder of this site.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Snapshot")]
        [Parameter(Mandatory = true, ParameterSetName = "SnapshotInputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long SnapshotId { get; set; }

        /// <summary>
        /// Sends the result to the child site with this ID instead of a folder of this site.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ChildSite")]
        [Parameter(Mandatory = true, ParameterSetName = "ChildSiteInputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long ChildSiteId { get; set; }

        /// <summary>
        /// The path within the remote server, snapshot or child site to send to. Defaults to its root folder.
        /// </summary>
        [Parameter(ParameterSetName = "RemoteServer")]
        [Parameter(ParameterSetName = "RemoteServerInputObject")]
        [Parameter(ParameterSetName = "Snapshot")]
        [Parameter(ParameterSetName = "SnapshotInputObject")]
        [Parameter(ParameterSetName = "ChildSite")]
        [Parameter(ParameterSetName = "ChildSiteInputObject")]
        [ValidateNotNull]
        public string DestinationPath { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "RemoteServerInputObject")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "SnapshotInputObject")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ChildSiteInputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Move File/Folder.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = MoveParameters();
            if (IsBound(nameof(RemoteServerId)))
            {
                if (!ShouldProcessOn(connection, $"File '{path}' to remote server {RemoteServerId}"))
                {
                    return;
                }
                WriteObject(Run(connection, token => RemoteFileBridge.MoveToRemoteServerAsync(connection, path, RemoteServerId, DestinationPath, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token)));
                return;
            }
            if (IsBound(nameof(SnapshotId)))
            {
                if (!ShouldProcessOn(connection, $"File '{path}' to snapshot {SnapshotId}"))
                {
                    return;
                }
                WriteObject(Run(connection, token => RemoteFileBridge.MoveToSnapshotAsync(connection, path, SnapshotId, DestinationPath, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token)));
                return;
            }
            if (IsBound(nameof(ChildSiteId)))
            {
                if (!ShouldProcessOn(connection, $"File '{path}' to child site {ChildSiteId}"))
                {
                    return;
                }
                WriteObject(Run(connection, token => RemoteFileBridge.MoveToChildSiteAsync(connection, path, ChildSiteId, DestinationPath, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token)));
                return;
            }
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.FileAction result = Run(connection, token => RemoteFileBridge.MoveAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> MoveParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(Overwrite)))
            {
                parameters["overwrite"] = Overwrite.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Copy File/Folder.
    /// </summary>
    [Cmdlet("Copy", "FilesFile", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileAction), ParameterSetName = new[] { "Path", "InputObject", "RemoteServer", "RemoteServerInputObject", "Snapshot", "SnapshotInputObject", "ChildSite", "ChildSiteInputObject" })]
    public sealed class CopyFilesFileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "RemoteServer")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Snapshot")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ChildSite")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Copy destination path.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// If copying a folder, also copy supported behaviors, email notification subscriptions, and per-folder branding to the destination folder tree?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [Parameter(ParameterSetName = "RemoteServer")]
        [Parameter(ParameterSetName = "RemoteServerInputObject")]
        [Parameter(ParameterSetName = "Snapshot")]
        [Parameter(ParameterSetName = "SnapshotInputObject")]
        [Parameter(ParameterSetName = "ChildSite")]
        [Parameter(ParameterSetName = "ChildSiteInputObject")]
        [ValidateNotNull]
        public bool? CopyBehaviors { get; set; }

        /// <summary>
        /// Copy structure only?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [Parameter(ParameterSetName = "RemoteServer")]
        [Parameter(ParameterSetName = "RemoteServerInputObject")]
        [Parameter(ParameterSetName = "Snapshot")]
        [Parameter(ParameterSetName = "SnapshotInputObject")]
        [Parameter(ParameterSetName = "ChildSite")]
        [Parameter(ParameterSetName = "ChildSiteInputObject")]
        [ValidateNotNull]
        public bool? Structure { get; set; }

        /// <summary>
        /// Overwrite existing file(s) in the destination?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [Parameter(ParameterSetName = "RemoteServer")]
        [Parameter(ParameterSetName = "RemoteServerInputObject")]
        [Parameter(ParameterSetName = "Snapshot")]
        [Parameter(ParameterSetName = "SnapshotInputObject")]
        [Parameter(ParameterSetName = "ChildSite")]
        [Parameter(ParameterSetName = "ChildSiteInputObject")]
        [ValidateNotNull]
        public bool? Overwrite { get; set; }

        /// <summary>
        /// Sends the result to the remote server with this ID instead of a folder of this site.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "RemoteServer")]
        [Parameter(Mandatory = true, ParameterSetName = "RemoteServerInputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long RemoteServerId { get; set; }

        /// <summary>
        /// Sends the result to the snapshot with this ID instead of a folder of this site.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Snapshot")]
        [Parameter(Mandatory = true, ParameterSetName = "SnapshotInputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long SnapshotId { get; set; }

        /// <summary>
        /// Sends the result to the child site with this ID instead of a folder of this site.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ChildSite")]
        [Parameter(Mandatory = true, ParameterSetName = "ChildSiteInputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long ChildSiteId { get; set; }

        /// <summary>
        /// The path within the remote server, snapshot or child site to send to. Defaults to its root folder.
        /// </summary>
        [Parameter(ParameterSetName = "RemoteServer")]
        [Parameter(ParameterSetName = "RemoteServerInputObject")]
        [Parameter(ParameterSetName = "Snapshot")]
        [Parameter(ParameterSetName = "SnapshotInputObject")]
        [Parameter(ParameterSetName = "ChildSite")]
        [Parameter(ParameterSetName = "ChildSiteInputObject")]
        [ValidateNotNull]
        public string DestinationPath { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "RemoteServerInputObject")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "SnapshotInputObject")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ChildSiteInputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Copy File/Folder.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = CopyParameters();
            if (IsBound(nameof(RemoteServerId)))
            {
                if (!ShouldProcessOn(connection, $"File '{path}' to remote server {RemoteServerId}"))
                {
                    return;
                }
                WriteObject(Run(connection, token => RemoteFileBridge.CopyToRemoteServerAsync(connection, path, RemoteServerId, DestinationPath, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token)));
                return;
            }
            if (IsBound(nameof(SnapshotId)))
            {
                if (!ShouldProcessOn(connection, $"File '{path}' to snapshot {SnapshotId}"))
                {
                    return;
                }
                WriteObject(Run(connection, token => RemoteFileBridge.CopyToSnapshotAsync(connection, path, SnapshotId, DestinationPath, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token)));
                return;
            }
            if (IsBound(nameof(ChildSiteId)))
            {
                if (!ShouldProcessOn(connection, $"File '{path}' to child site {ChildSiteId}"))
                {
                    return;
                }
                WriteObject(Run(connection, token => RemoteFileBridge.CopyToChildSiteAsync(connection, path, ChildSiteId, DestinationPath, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token)));
                return;
            }
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.FileAction result = Run(connection, token => RemoteFileBridge.CopyAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> CopyParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(CopyBehaviors)))
            {
                parameters["copy_behaviors"] = CopyBehaviors.Value;
            }
            if (IsBound(nameof(Structure)))
            {
                parameters["structure"] = Structure.Value;
            }
            if (IsBound(nameof(Overwrite)))
            {
                parameters["overwrite"] = Overwrite.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete File/Folder and return its File Action.  A pending or processing deletion continues in the background after this returns; check its File Migration for completion.  Waiting for the File Migration does not cancel the deletion.
    /// </summary>
    [Cmdlet("Start", "FilesFileDeletion", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.FileAction), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class StartFilesFileDeletionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// If true, will recursively delete folders.  Otherwise, will error on non-empty folders.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Recursive { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Delete File/Folder and return its File Action.  A pending or processing deletion continues in the background after this returns; check its File Migration for completion.  Waiting for the File Migration does not cancel the deletion.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = BeginDeleteParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            Models.FileAction result = Run(connection, token => RemoteFileBridge.BeginDeleteAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            WriteObject(result);
        }

        private Dictionary<string, object> BeginDeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Recursive)))
            {
                parameters["recursive"] = Recursive.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Begin File Upload.
    /// </summary>
    [Cmdlet("Start", "FilesUploadSession", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileUploadPart), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class StartFilesUploadSessionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Create parent directories if they do not exist?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? MkdirParents { get; set; }

        /// <summary>
        /// Part if uploading a part.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Part { get; set; }

        /// <summary>
        /// How many parts to fetch?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Parts { get; set; }

        /// <summary>
        /// ref
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Ref { get; set; }

        /// <summary>
        /// File byte offset to restart from.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Restart { get; set; }

        /// <summary>
        /// Total bytes of file being uploaded (include bytes being retained if appending/restarting).
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? Size { get; set; }

        /// <summary>
        /// Allow file rename instead of overwrite?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? WithRename { get; set; }

        /// <summary>
        /// If true, and the path refers to a destination not stored on Files.com (such as a remote server mount), the upload will be uploaded first to Files.com before being sent to the remote server mount. This can allow clients to upload using parallel parts to a remote server destination that does not offer parallel parts support natively.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? BufferedUpload { get; set; }

        /// <summary>
        /// Include optional direct connection information for a direct Agent transfer attempt?
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? WithDirectConnectionInfo { get; set; }

        /// <summary>
        /// The file to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>Begin File Upload.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = BeginUploadParameters();
            if (!ShouldProcessOn(connection, $"File '{path}'"))
            {
                return;
            }
            foreach (Models.FileUploadPart item in Run(connection, token => RemoteFileBridge.BeginUploadAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token)))
            {
                WriteObject(item);
            }
        }

        private Dictionary<string, object> BeginUploadParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(MkdirParents)))
            {
                parameters["mkdir_parents"] = MkdirParents.Value;
            }
            if (IsBound(nameof(Part)))
            {
                parameters["part"] = Part.Value;
            }
            if (IsBound(nameof(Parts)))
            {
                parameters["parts"] = Parts.Value;
            }
            if (IsBound(nameof(Ref)))
            {
                parameters["ref"] = Ref;
            }
            if (IsBound(nameof(Restart)))
            {
                parameters["restart"] = Restart.Value;
            }
            if (IsBound(nameof(Size)))
            {
                parameters["size"] = Size.Value;
            }
            if (IsBound(nameof(WithRename)))
            {
                parameters["with_rename"] = WithRename.Value;
            }
            if (IsBound(nameof(BufferedUpload)))
            {
                parameters["buffered_upload"] = BufferedUpload.Value;
            }
            if (IsBound(nameof(WithDirectConnectionInfo)))
            {
                parameters["with_direct_connection_info"] = WithDirectConnectionInfo.Value;
            }
            return parameters;
        }
    }
}
