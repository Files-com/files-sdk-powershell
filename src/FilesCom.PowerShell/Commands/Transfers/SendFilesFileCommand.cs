using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Uploads local files, or bytes from a stream, to Files.com, or to a remote server, snapshot or child site through
    /// it. Parts are sent in parallel when Files.com allows it, retried individually when they fail, and the upload
    /// completes with one final request.
    /// </summary>
    [Cmdlet(VerbsCommunications.Send, "FilesFile", DefaultParameterSetName = PathSet, SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(FilesUploadResult))]
    public sealed class SendFilesFileCommand : FilesApiCmdlet
    {
        // Each source, local files by -Path or -LiteralPath or a stream, uploads to a folder of this site or, in a set of
        // its own, to one external destination.
        private const string PathSet = "Path";
        private const string LiteralPathSet = "LiteralPath";
        private const string StreamSet = "Stream";
        private const string RemoteServerPathSet = "RemoteServerPath";
        private const string RemoteServerLiteralPathSet = "RemoteServerLiteralPath";
        private const string RemoteServerStreamSet = "RemoteServerStream";
        private const string SnapshotPathSet = "SnapshotPath";
        private const string SnapshotLiteralPathSet = "SnapshotLiteralPath";
        private const string SnapshotStreamSet = "SnapshotStream";
        private const string ChildSitePathSet = "ChildSitePath";
        private const string ChildSiteLiteralPathSet = "ChildSiteLiteralPath";
        private const string ChildSiteStreamSet = "ChildSiteStream";
        private const int DefaultDiagnosticLimit = 1000;

        /// <summary>The local files to upload. Wildcards are expanded.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = PathSet)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = RemoteServerPathSet)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = SnapshotPathSet)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ChildSitePathSet)]
        [SupportsWildcards]
        [ValidateNotNullOrEmpty]
        public string[] Path { get; set; }

        /// <summary>The local files to upload, used exactly as written. Files piped from Get-ChildItem bind here.</summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = LiteralPathSet)]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = RemoteServerLiteralPathSet)]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = SnapshotLiteralPathSet)]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ChildSiteLiteralPathSet)]
        [Alias("PSPath")]
        [ValidateNotNullOrEmpty]
        public string[] LiteralPath { get; set; }

        /// <summary>
        /// The stream to upload from its current position. It is borrowed: the command reads it and leaves it open. It
        /// is read only after confirmation, never seeked, and may be forward-only, such as a process's output.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = StreamSet)]
        [Parameter(Mandatory = true, ParameterSetName = RemoteServerStreamSet)]
        [Parameter(Mandatory = true, ParameterSetName = SnapshotStreamSet)]
        [Parameter(Mandatory = true, ParameterSetName = ChildSiteStreamSet)]
        [ValidateNotNull]
        public Stream InputStream { get; set; }

        /// <summary>
        /// The number of bytes to upload from -InputStream. Without it, the command uploads everything the stream gives
        /// until it ends.
        /// </summary>
        [Parameter(ParameterSetName = StreamSet)]
        [Parameter(ParameterSetName = RemoteServerStreamSet)]
        [Parameter(ParameterSetName = SnapshotStreamSet)]
        [Parameter(ParameterSetName = ChildSiteStreamSet)]
        [FilesInteger]
        [ValidateRange(0, long.MaxValue)]
        public long Length { get; set; }

        /// <summary>
        /// For -InputStream without -Length, the most bytes the stream may give. A longer stream stops the upload, which
        /// is not completed. Where the upload chooses its part sizes, parts are sized so that this many bytes fit in the
        /// parts available. With -Spool, it is also the most bytes copied to the temporary file.
        /// </summary>
        [Parameter(ParameterSetName = StreamSet)]
        [Parameter(ParameterSetName = RemoteServerStreamSet)]
        [Parameter(ParameterSetName = SnapshotStreamSet)]
        [Parameter(ParameterSetName = ChildSiteStreamSet)]
        [FilesInteger]
        [ValidateRange(0, long.MaxValue)]
        public long MaxLength { get; set; }

        /// <summary>
        /// Where the upload keeps each part of -InputStream until Files.com accepts it, so that a retry resends the same
        /// bytes: Memory, the default, within the transfer limiter's buffer space, or TemporaryFiles, one private file
        /// per part, deleted once the part is accepted or the upload ends.
        /// </summary>
        [Parameter(ParameterSetName = StreamSet)]
        [Parameter(ParameterSetName = RemoteServerStreamSet)]
        [Parameter(ParameterSetName = SnapshotStreamSet)]
        [Parameter(ParameterSetName = ChildSiteStreamSet)]
        public FilesStreamPartStorage PartStorage { get; set; }

        /// <summary>
        /// Copies all of -InputStream, which has no -Length, to a private temporary file of at most -MaxLength bytes
        /// before sending anything, then uploads that file with its size known and deletes it. A longer stream, a read
        /// failure or cancellation during the copy ends the command before anything is sent.
        /// </summary>
        [Parameter(ParameterSetName = StreamSet)]
        [Parameter(ParameterSetName = RemoteServerStreamSet)]
        [Parameter(ParameterSetName = SnapshotStreamSet)]
        [Parameter(ParameterSetName = ChildSiteStreamSet)]
        public SwitchParameter Spool { get; set; }

        /// <summary>
        /// The existing local folder for the temporary files of -Spool or -PartStorage TemporaryFiles. Defaults to the
        /// user's temporary folder.
        /// </summary>
        [Parameter(ParameterSetName = StreamSet)]
        [Parameter(ParameterSetName = RemoteServerStreamSet)]
        [Parameter(ParameterSetName = SnapshotStreamSet)]
        [Parameter(ParameterSetName = ChildSiteStreamSet)]
        [ValidateNotNullOrEmpty]
        public string TemporaryDirectory { get; set; }

        /// <summary>Uploads to the remote server with this ID instead of a folder of this site.</summary>
        [Parameter(Mandatory = true, ParameterSetName = RemoteServerPathSet)]
        [Parameter(Mandatory = true, ParameterSetName = RemoteServerLiteralPathSet)]
        [Parameter(Mandatory = true, ParameterSetName = RemoteServerStreamSet)]
        [FilesInteger]
        [ValidateNotNull]
        public long RemoteServerId { get; set; }

        /// <summary>Uploads to the snapshot with this ID instead of a folder of this site.</summary>
        [Parameter(Mandatory = true, ParameterSetName = SnapshotPathSet)]
        [Parameter(Mandatory = true, ParameterSetName = SnapshotLiteralPathSet)]
        [Parameter(Mandatory = true, ParameterSetName = SnapshotStreamSet)]
        [FilesInteger]
        [ValidateNotNull]
        public long SnapshotId { get; set; }

        /// <summary>Uploads to the child site with this ID instead of a folder of this site.</summary>
        [Parameter(Mandatory = true, ParameterSetName = ChildSitePathSet)]
        [Parameter(Mandatory = true, ParameterSetName = ChildSiteLiteralPathSet)]
        [Parameter(Mandatory = true, ParameterSetName = ChildSiteStreamSet)]
        [FilesInteger]
        [ValidateNotNull]
        public long ChildSiteId { get; set; }

        /// <summary>
        /// The remote path to upload to, used exactly as written. Defaults to the local file's name in the root folder.
        /// With -RemoteServerId, -SnapshotId or -ChildSiteId, the path within that destination, by default the local
        /// file's name in its root folder, which cannot leave it: either slash separates folders, and empty, "." and ".."
        /// folders are left out. For piped files, a script block such as { "reports/" + $_.Name } gives each its own path.
        /// </summary>
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = PathSet)]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = RemoteServerPathSet)]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = SnapshotPathSet)]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = ChildSitePathSet)]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = LiteralPathSet)]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = RemoteServerLiteralPathSet)]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = SnapshotLiteralPathSet)]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = ChildSiteLiteralPathSet)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = StreamSet)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = RemoteServerStreamSet)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = SnapshotStreamSet)]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ChildSiteStreamSet)]
        [ValidateNotNullOrEmpty]
        public string RemotePath { get; set; }

        /// <summary>Creates missing parent folders of the remote path. -MkdirParents:$false asks Files.com explicitly not to.</summary>
        [Parameter]
        public SwitchParameter MkdirParents { get; set; }

        /// <summary>
        /// How each upload decides how many parts to send at once when Files.com allows parallel parts: Automatic, the
        /// default, adapts the number to the throughput, failures and throttling it measures; Static keeps it fixed at
        /// -MaxConcurrency, or 4.
        /// </summary>
        [Parameter]
        public FilesUploadConcurrencyMode ConcurrencyMode { get; set; }

        /// <summary>
        /// The most parts of each upload to send at the same time, from 1 to 1024: the limit an automatic upload stays
        /// within, or the number a static upload sends. By default an automatic upload has only its profile's limit, and
        /// a static upload sends 4. A -Limiter's request budget applies as well.
        /// </summary>
        [Parameter]
        [ValidateRange(1, 1024)]
        public int MaxConcurrency { get; set; }

        /// <summary>
        /// The kind of destination an automatic upload's starting point and limits are tuned for: Generic, S3 or Agent.
        /// Without it, Agent when Files.com supplies direct connection information for the upload, the scaled S3 policy
        /// where the parts go straight to Amazon S3 and Generic elsewhere; given, S3 is the conservative S3 preset. It never
        /// changes the parts Files.com asks for.
        /// </summary>
        [Parameter]
        public FilesUploadPerformanceProfile PerformanceProfile { get; set; }

        /// <summary>
        /// Starts an automatic upload whose parts go straight to Amazon S3 with the scaled S3 policy at the policy's 150
        /// parts at once, within -MaxConcurrency, skipping initial concurrency probes on a known high-throughput network.
        /// Without it, or with -AggressiveStartup:$false, the upload probes the link from one part at a time and adds
        /// parts as answered parts show they help, which can reach the same throughput and concurrency ceiling; only its
        /// ramp-up takes longer. Either way the upload then adapts with the same policy. Uploads to other destinations
        /// start from their own defaults. Not with -ConcurrencyMode Static, -PerformanceProfile or -AutomaticConcurrency,
        /// which choose their own start.
        /// </summary>
        [Parameter]
        public SwitchParameter AggressiveStartup { get; set; }

        /// <summary>
        /// How the upload sizes its parts where they go straight to Amazon S3, within the engine's limits for S3:
        /// Automatic, the default, chooses from the file's size and grows parts while that raises throughput; Fixed
        /// sends every part but the last with -PartSize bytes; ServerPlanned uses the sizes Files.com plans. Elsewhere
        /// every mode uses the sizes Files.com plans.
        /// </summary>
        [Parameter]
        public FilesUploadPartSizingMode PartSizingMode { get; set; }

        /// <summary>
        /// The size in bytes of every part but the last, which makes the upload's part sizes Fixed. Where the parts go
        /// straight to Amazon S3, the upload fails before sending any part if this size is outside the engine's limits.
        /// </summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long PartSize { get; set; }

        /// <summary>
        /// Expert tuning, from New-FilesAutomaticConcurrency, for how an automatic upload chooses the number of parts it
        /// sends at once. It replaces the defaults of the profile chosen for the destination with its own, whatever the
        /// destination.
        /// Not with -PerformanceProfile, whose defaults it starts from itself, or -ConcurrencyMode Static.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        public FilesAutomaticConcurrency AutomaticConcurrency { get; set; }

        /// <summary>
        /// Expert tuning, from New-FilesAutomaticPartSizing, for the part sizes an upload chooses where its parts go
        /// straight to Amazon S3.
        /// Not with -PartSize or -PartSizingMode Fixed or ServerPlanned, which do not choose sizes.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        public FilesAutomaticPartSizing AutomaticPartSizing { get; set; }

        /// <summary>
        /// The most parts read ahead and waiting to be sent, at least 1. Defaults to the engine's default, 2. However many
        /// parts an automatic upload sends at once, no more than this many wait ready.
        /// </summary>
        [Parameter]
        [ValidateRange(1, int.MaxValue)]
        public int MaxReadyParts { get; set; }

        /// <summary>
        /// The most bytes of buffers the parts read ahead may hold, at least 1. Defaults to the engine's default, 64 MiB. A
        /// part larger than this is still read ahead once no other part is waiting, so it is a budget for waiting parts, not
        /// a limit on the size of one.
        /// </summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long MaxReadyBytes { get; set; }

        /// <summary>
        /// Writes each upload's detailed records, of every part request and every concurrency and part size decision, to
        /// the Information stream with the tag FilesCom.UploadTrace, and a summary record once the upload has completed or
        /// failed. Collect them with -InformationVariable. Records hold the remote path, times, decisions and counts only.
        /// </summary>
        [Parameter]
        public SwitchParameter Diagnostics { get; set; }

        /// <summary>
        /// With -Diagnostics, the most records one upload writes, from 1 to 100000. Defaults to 1000. Records beyond it are
        /// left out and counted in the summary, whose Complete is then false.
        /// </summary>
        [Parameter]
        [ValidateRange(1, 100000)]
        public int DiagnosticLimit { get; set; }

        /// <summary>A limiter, from New-FilesTransferLimiter, whose request and memory budget this upload shares with others.</summary>
        [Parameter]
        [ValidateNotNull]
        public FilesTransferLimiter Limiter { get; set; }

        /// <summary>The modification time to record. Defaults to the local file's last write time, or for a stream the time the upload completes.</summary>
        [Parameter]
        [FilesDateTime]
        public DateTimeOffset ModifiedTime { get; set; }

        /// <summary>
        /// A local file in which the upload records its progress, so that when it is stopped, by Ctrl+C, a failure or the
        /// process ending, running the same command again with the same connection, remote path and checkpoint continues
        /// it: parts Files.com accepted, whose bytes are still what was sent, are not sent again. The engine writes the
        /// file for you only, without credentials or signed URLs, and deletes it once the upload completes. Its folder
        /// must exist and be one that only you, or the system, can change, and each upload needs a checkpoint of its own:
        /// it is refused, with Files.CheckpointRefused, when it records another upload, the file changed, or Files.com no
        /// longer knows the upload, unless -CheckpointRestart StartOver says to upload the file again. Only a local file
        /// can be continued: a checkpoint cannot be used with -InputStream, or with more than one file.
        /// </summary>
        [Parameter(ParameterSetName = PathSet)]
        [Parameter(ParameterSetName = LiteralPathSet)]
        [Parameter(ParameterSetName = RemoteServerPathSet)]
        [Parameter(ParameterSetName = RemoteServerLiteralPathSet)]
        [Parameter(ParameterSetName = SnapshotPathSet)]
        [Parameter(ParameterSetName = SnapshotLiteralPathSet)]
        [Parameter(ParameterSetName = ChildSitePathSet)]
        [Parameter(ParameterSetName = ChildSiteLiteralPathSet)]
        [ValidateNotNullOrEmpty]
        public string CheckpointPath { get; set; }

        /// <summary>
        /// What the upload does when its checkpoint records an upload that cannot be continued, because the file changed,
        /// Files.com no longer knows the upload, or it now allows other part limits: Fail, the default, fails with
        /// Files.CheckpointRefused before any part is sent; StartOver uploads the file again from its first byte as a new
        /// upload. An upload whose final request was sent is never started over. Needs -CheckpointPath.
        /// </summary>
        [Parameter(ParameterSetName = PathSet)]
        [Parameter(ParameterSetName = LiteralPathSet)]
        [Parameter(ParameterSetName = RemoteServerPathSet)]
        [Parameter(ParameterSetName = RemoteServerLiteralPathSet)]
        [Parameter(ParameterSetName = SnapshotPathSet)]
        [Parameter(ParameterSetName = SnapshotLiteralPathSet)]
        [Parameter(ParameterSetName = ChildSitePathSet)]
        [Parameter(ParameterSetName = ChildSiteLiteralPathSet)]
        public FilesCheckpointRestart CheckpointRestart { get; set; }

        /// <summary>Writes the completed upload.</summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        // Set once a checkpoint has been given to an upload, so that no other upload of this command reuses it.
        private bool checkpointGiven;

        /// <summary>Uploads the current record's files or stream.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            FilesUploadSettings settings = Settings();
            ExternalDestination destination = ChosenDestination();
            if (InputStream != null)
            {
                UploadStream(connection, settings, destination);
                return;
            }
            List<KeyValuePair<string, string>> uploads = Uploads(destination);
            if (settings.CheckpointPath != null && checkpointGiven && uploads.Count > 0)
            {
                // An earlier input of this pipeline had the checkpoint, and its upload may have completed.
                throw new FilesException(FilesErrorKind.InvalidArgument, $"-CheckpointPath records one upload, and an earlier file of this command already used it; that upload may have completed. '{uploads[0].Key}' and any other file of this input were not uploaded. Upload each file with a checkpoint of its own.");
            }
            if (settings.CheckpointPath != null && uploads.Count > 1)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-CheckpointPath records one upload, and this command was given more than one file. Upload each file with a checkpoint of its own. Nothing was uploaded.");
            }
            foreach (KeyValuePair<string, string> upload in uploads)
            {
                checkpointGiven |= settings.CheckpointPath != null;
                UploadFile(connection, settings, destination, upload.Key, upload.Value);
            }
        }

        // The remote server, snapshot or child site the parameter set uploads to, or null for a folder of this site.
        private ExternalDestination ChosenDestination()
        {
            if (IsBound(nameof(RemoteServerId)))
            {
                return new ExternalDestination(ExternalDestinations.RemoteServer, RemoteServerId, $"remote server {RemoteServerId}");
            }
            if (IsBound(nameof(SnapshotId)))
            {
                return new ExternalDestination(ExternalDestinations.Snapshot, SnapshotId, $"snapshot {SnapshotId}");
            }
            if (IsBound(nameof(ChildSiteId)))
            {
                return new ExternalDestination(ExternalDestinations.ChildSite, ChildSiteId, $"child site {ChildSiteId}");
            }
            return null;
        }

        // Every local file of the current record with the remote path it goes to, resolved and checked before the first
        // upload starts: one -RemotePath names one file, and no two files may go to what Files.com treats as the same
        // remote path, since the later would silently replace the earlier.
        private List<KeyValuePair<string, string>> Uploads(ExternalDestination destination)
        {
            var uploads = new List<KeyValuePair<string, string>>();
            bool literal = LiteralPath != null;
            foreach (string path in literal ? LiteralPath : Path)
            {
                foreach (string file in LocalPaths.ExistingFiles(this, path, literal))
                {
                    string remotePath = RemotePath ?? System.IO.Path.GetFileName(file);
                    uploads.Add(new KeyValuePair<string, string>(file, destination == null ? remotePath : destination.PathOf(remotePath)));
                }
            }
            if (RemotePath != null && uploads.Count > 1)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The paths given match {uploads.Count} files, but -RemotePath names one. Omit -RemotePath, or give each file its own with a script block. Nothing was uploaded.");
            }
            IGrouping<string, KeyValuePair<string, string>> collision = uploads.GroupBy(upload => RemotePaths.ComparisonKey(upload.Value), StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
            if (collision != null)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"{string.Join(" and ", collision.Select(upload => upload.Key))} would all upload to '{collision.First().Value}'. Give each file its own remote path. Nothing was uploaded.");
            }
            return uploads;
        }

        private void UploadFile(FilesConnection connection, FilesUploadSettings settings, ExternalDestination destination, string localPath, string remotePath)
        {
            ErrorTarget = remotePath;
            string checkpoint = settings.CheckpointPath == null ? "" : $", recording its progress in '{settings.CheckpointPath}'";
            if (!ShouldProcessOn(connection, $"'{localPath}' {Target(destination, remotePath)}{checkpoint}"))
            {
                return;
            }
            FilesUploadResult result = Run(connection, $"Uploading {remotePath}", run => TransferBridge.UploadFileAsync(connection, localPath, remotePath, settings, run.Progress, Trace(run, remotePath), CallOptions, run.Token));
            WriteResult(result);
        }

        private void UploadStream(FilesConnection connection, FilesUploadSettings settings, ExternalDestination destination)
        {
            if (!InputStream.CanRead)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-InputStream cannot be read.");
            }
            string remotePath = destination == null ? RemotePath : destination.PathOf(RemotePath);
            ErrorTarget = remotePath;
            long? length = IsBound(nameof(Length)) ? Length : (long?)null;
            if (!ShouldProcessOn(connection, $"{StreamDescription(length, settings)} {Target(destination, remotePath)}"))
            {
                return;
            }
            FilesUploadResult result = Run(connection, $"Uploading {remotePath}", run => TransferBridge.UploadStreamAsync(connection, InputStream, length, remotePath, settings, run.Progress, Trace(run, remotePath), CallOptions, run.Token));
            WriteResult(result);
        }

        // What an upload changes, as confirmation names it: the remote file, and the external destination it is in.
        private static string Target(ExternalDestination destination, string remotePath)
        {
            return destination == null ? $"to file '{remotePath}'" : $"to {destination.Name}, as file '{remotePath}'";
        }

        // Where the upload's trace records go: the operation's capture with -Diagnostics, or nowhere, so that the engine
        // makes none.
        private IProgress<FilesUploadTrace> Trace(PipelineOperation run, string remotePath)
        {
            return Diagnostics.IsPresent ? run.CaptureTrace(remotePath, IsBound(nameof(DiagnosticLimit)) ? DiagnosticLimit : DefaultDiagnosticLimit) : null;
        }

        private static string StreamDescription(long? length, FilesUploadSettings settings)
        {
            if (length.HasValue)
            {
                return $"{length} bytes from a stream";
            }
            if (settings.Spool)
            {
                return $"a stream of at most {settings.MaxLength} bytes, copied to a temporary file first,";
            }
            return settings.MaxLength.HasValue ? $"a stream of at most {settings.MaxLength} bytes" : "a stream of unknown length";
        }

        // The caller's choices, checked against each other before anything is read or sent: a setting that would change
        // nothing is refused rather than ignored.
        private FilesUploadSettings Settings()
        {
            bool staticConcurrency = IsBound(nameof(ConcurrencyMode)) && ConcurrencyMode == FilesUploadConcurrencyMode.Static;
            if (IsBound(nameof(PerformanceProfile)) && staticConcurrency)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-PerformanceProfile tunes automatic uploads only; a static upload sends -MaxConcurrency parts at once.");
            }
            if (AutomaticConcurrency != null)
            {
                if (staticConcurrency)
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, "-AutomaticConcurrency tunes automatic uploads only; a static upload sends -MaxConcurrency parts at once.");
                }
                if (IsBound(nameof(PerformanceProfile)))
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-AutomaticConcurrency already starts from the {AutomaticConcurrency.Profile} profile's defaults. Give one or the other; New-FilesAutomaticConcurrency -Profile chooses the profile.");
                }
            }
            // -AggressiveStartup:$false asks for the default start, which every other setting allows.
            if (AggressiveStartup.IsPresent)
            {
                if (staticConcurrency)
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, "-AggressiveStartup starts an automatic upload at the scaled S3 policy's start; a static upload sends -MaxConcurrency parts at once from the start.");
                }
                if (AutomaticConcurrency != null || IsBound(nameof(PerformanceProfile)))
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-AggressiveStartup starts the scaled S3 policy the engine picks for Amazon S3 at its start, and {(AutomaticConcurrency != null ? "-AutomaticConcurrency" : "-PerformanceProfile")} replaces that policy with tuning that starts at its own StartTarget. Give one or the other.");
                }
            }
            if (IsBound(nameof(DiagnosticLimit)) && !Diagnostics.IsPresent)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-DiagnosticLimit bounds the records of -Diagnostics, which was not given.");
            }
            FilesUploadPartSizingMode? sizingMode = IsBound(nameof(PartSizingMode)) ? PartSizingMode : (FilesUploadPartSizingMode?)null;
            if (IsBound(nameof(PartSize)))
            {
                if (sizingMode.HasValue && sizingMode != FilesUploadPartSizingMode.Fixed)
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-PartSize fixes the part sizes, which -PartSizingMode {sizingMode} does not. Give one or the other.");
                }
                sizingMode = FilesUploadPartSizingMode.Fixed;
            }
            else if (sizingMode == FilesUploadPartSizingMode.Fixed)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-PartSizingMode Fixed needs -PartSize, the size in bytes of every part but the last.");
            }
            if (AutomaticPartSizing != null && sizingMode.HasValue && sizingMode != FilesUploadPartSizingMode.Automatic)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, IsBound(nameof(PartSize))
                    ? "-AutomaticPartSizing tunes the sizes an upload chooses, and -PartSize fixes them. Give one or the other."
                    : $"-AutomaticPartSizing tunes the sizes an upload chooses, which -PartSizingMode {sizingMode} does not. Give one or the other.");
            }
            CheckStreamSettings();
            if (IsBound(nameof(CheckpointRestart)) && CheckpointPath == null)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-CheckpointRestart says what to do with a checkpoint, and -CheckpointPath was not given.");
            }
            return new FilesUploadSettings(
                IsBound(nameof(ConcurrencyMode)) ? ConcurrencyMode : (FilesUploadConcurrencyMode?)null,
                IsBound(nameof(MaxConcurrency)) ? MaxConcurrency : (int?)null,
                IsBound(nameof(PerformanceProfile)) ? PerformanceProfile : (FilesUploadPerformanceProfile?)null,
                AggressiveStartup.IsPresent,
                sizingMode,
                IsBound(nameof(PartSize)) ? PartSize : (long?)null,
                IsBound(nameof(MkdirParents)) ? MkdirParents.IsPresent : (bool?)null,
                Limiter,
                IsBound(nameof(ModifiedTime)) ? ModifiedTime.UtcDateTime : (DateTime?)null,
                IsBound(nameof(MaxLength)) ? MaxLength : (long?)null,
                IsBound(nameof(PartStorage)) ? PartStorage : (FilesStreamPartStorage?)null,
                Spool.IsPresent,
                TemporaryDirectory == null ? null : LocalPaths.ExistingFolder(this, TemporaryDirectory),
                AutomaticConcurrency,
                AutomaticPartSizing,
                IsBound(nameof(MaxReadyParts)) ? MaxReadyParts : (int?)null,
                IsBound(nameof(MaxReadyBytes)) ? MaxReadyBytes : (long?)null,
                CheckpointPath == null ? null : LocalPaths.CheckpointFile(this, CheckpointPath),
                IsBound(nameof(CheckpointRestart)) ? CheckpointRestart : (FilesCheckpointRestart?)null);
        }

        // The stream parameters bind only with -InputStream; these are the combinations the parameter sets cannot refuse.
        private void CheckStreamSettings()
        {
            bool sized = IsBound(nameof(Length));
            if (sized && IsBound(nameof(MaxLength)))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-MaxLength bounds a stream uploaded without -Length; with -Length, exactly that many bytes are uploaded.");
            }
            if (Spool.IsPresent)
            {
                if (sized)
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, "-Spool copies a stream uploaded without -Length; with -Length, the stream is uploaded as it is read.");
                }
                if (!IsBound(nameof(MaxLength)) || MaxLength < 1)
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, "-Spool needs a -MaxLength of at least 1, the most bytes to copy to the temporary file.");
                }
                if (IsBound(nameof(PartStorage)))
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, "-PartStorage applies to a stream uploaded as it is read; with -Spool, every part is read back from the temporary file.");
                }
            }
            if (TemporaryDirectory != null && !Spool.IsPresent && PartStorage != FilesStreamPartStorage.TemporaryFiles)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-TemporaryDirectory holds the temporary files of -Spool or -PartStorage TemporaryFiles, and neither was given.");
            }
        }

        private void WriteResult(FilesUploadResult result)
        {
            if (PassThru.IsPresent)
            {
                WriteObject(result);
            }
        }

        // A remote server, snapshot or child site: Files.com's folder for it, whose files the upload's remote paths are in.
        private sealed class ExternalDestination
        {
            private readonly string root;
            private readonly long id;

            internal ExternalDestination(string root, long id, string name)
            {
                this.root = root;
                this.id = id;
                Name = name;
            }

            // The destination as confirmation and errors name it, such as "remote server 5".
            internal string Name { get; }

            // The remote path of the file at pathWithin the destination, which cannot leave it.
            internal string PathOf(string pathWithin)
            {
                return RemotePaths.InExternalDestination(root, id, pathWithin)
                    ?? throw new FilesException(FilesErrorKind.InvalidArgument, $"'{pathWithin}' names no file within {Name}: without its empty, \".\" and \"..\" folders nothing is left. Give -RemotePath a file name. Nothing was uploaded.");
            }
        }
    }
}
