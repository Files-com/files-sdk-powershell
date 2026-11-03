using System;
using System.IO;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Downloads a file from Files.com to a local file or into a stream. Where its storage is an Amazon S3
    /// general-purpose endpoint over HTTPS, the download is bound to the version its first part's answer names and
    /// fetched in checked parts, several at once, so bytes of two versions are never put together; from any other
    /// storage the file comes in one request. A local file is written by the engine
    /// into a private stage beside it and put in place under its name only once the download has completed: where
    /// nothing is at that name, or with -Force replacing only a regular file. With -CheckpointPath, a version-bound
    /// download that is stopped or fails keeps what it fetched, and running the command again continues it.
    /// </summary>
    [Cmdlet(VerbsCommunications.Receive, "FilesFile", DefaultParameterSetName = LiteralPathSet, SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(FilesDownloadResult))]
    public sealed class ReceiveFilesFileCommand : FilesApiCmdlet
    {
        private const string LiteralPathSet = "LiteralPath";
        private const string StreamSet = "Stream";
        private const string InputObjectSet = "InputObject";

        /// <summary>The remote path of the file, used exactly as written.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = LiteralPathSet)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = StreamSet)]
        [ValidateNotNullOrEmpty]
        public string RemotePath { get; set; }

        /// <summary>A file written by another FilesCom command, such as Get-FilesChildItem; its path and connection are used.</summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = InputObjectSet)]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>
        /// The local file to write, used exactly as written, or an existing folder to write the file into under its
        /// remote name. The folder must exist.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = LiteralPathSet)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = InputObjectSet)]
        [ValidateNotNullOrEmpty]
        public string LiteralPath { get; set; }

        /// <summary>
        /// The stream to write to from its current position. It is borrowed: the command writes to it and leaves it open.
        /// A download into a stream cannot be continued, so it takes no -CheckpointPath.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = StreamSet)]
        [ValidateNotNull]
        public Stream OutputStream { get; set; }

        /// <summary>
        /// Replaces a regular file that already exists, giving the new file that file's group, access control list and
        /// permissions; nothing is replaced when those cannot be carried over. A folder, a link or any other entry that is
        /// not a regular file is never replaced or followed. Without it, anything at that name is left untouched and the
        /// download fails.
        /// </summary>
        [Parameter(ParameterSetName = LiteralPathSet)]
        [Parameter(ParameterSetName = InputObjectSet)]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// The most parts fetched at the same time, from 1 to 64. Unset, the engine's automatic policy decides how many as
        /// the download goes, within its own limits. Parts are fetched at once only when the download is bound to a version
        /// of the file. A -Limiter's request budget also applies.
        /// </summary>
        [Parameter]
        [ValidateRange(1, 64)]
        public int MaxConcurrency { get; set; }

        /// <summary>
        /// The size in bytes of each part fetched, 1 or more; by default 8 MiB. Into a stream, each part waits in memory
        /// until the parts before it have been written, so a part is no larger than the -Limiter's buffer space.
        /// </summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long PartSize { get; set; }

        /// <summary>A limiter from New-FilesTransferLimiter whose request slots and buffer space this download shares with other transfers.</summary>
        [Parameter]
        [ValidateNotNull]
        public FilesTransferLimiter Limiter { get; set; }

        /// <summary>
        /// A local file in which the download records its progress, so that when it is stopped, by Ctrl+C, a failure or
        /// the process ending, running the same command again with the same connection, remote path, local file and
        /// checkpoint continues it: once its storage confirms the version is still the file's, only the parts not yet
        /// written are fetched; a version that is gone starts over from the first byte. A download to a file bound to no
        /// version always starts over. The engine writes the checkpoint for you only, without credentials or signed URLs,
        /// and deletes it once the file is in place. Its folder must exist and be one that only you, or the system, can
        /// change, and each download needs a checkpoint of its own: it is refused, with Files.CheckpointRefused, when it
        /// records another transfer or cannot be continued, unless -CheckpointRestart StartOver says to download the file
        /// again.
        /// </summary>
        [Parameter(ParameterSetName = LiteralPathSet)]
        [Parameter(ParameterSetName = InputObjectSet)]
        [ValidateNotNullOrEmpty]
        public string CheckpointPath { get; set; }

        /// <summary>
        /// What the download does when its checkpoint records a download that cannot be continued, such as one whose
        /// stage no longer holds what it records: Fail, the default, fails with Files.CheckpointRefused before anything is
        /// fetched; StartOver discards that download's own stage and checkpoint and downloads the file again from its
        /// first byte. Needs -CheckpointPath.
        /// </summary>
        [Parameter(ParameterSetName = LiteralPathSet)]
        [Parameter(ParameterSetName = InputObjectSet)]
        public FilesCheckpointRestart CheckpointRestart { get; set; }

        /// <summary>Writes the completed download.</summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        // Set once a checkpoint has been given to a download, so that no other file of this command reuses it.
        private bool checkpointGiven;

        /// <summary>Downloads the current record's file.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string remotePath = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : RemotePath;
            ErrorTarget = remotePath;
            if (ParameterSetName == StreamSet)
            {
                DownloadToStream(connection, remotePath);
                return;
            }
            DownloadToFile(connection, remotePath);
        }

        private void DownloadToStream(FilesConnection connection, string remotePath)
        {
            if (!OutputStream.CanWrite)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-OutputStream cannot be written.");
            }
            if (!ShouldProcessOn(connection, $"file '{remotePath}' to a stream"))
            {
                return;
            }
            FilesDownloadResult result = Run(connection, $"Downloading {remotePath}", run => TransferBridge.DownloadAsync(
                connection,
                remotePath,
                OutputStream,
                null,
                IsBound(nameof(MaxConcurrency)) ? MaxConcurrency : (int?)null,
                IsBound(nameof(PartSize)) ? PartSize : (long?)null,
                Limiter,
                run.Progress,
                CallOptionsFor(InputObject),
                run.Token));
            WritePassThru(result);
        }

        // Every local file goes through the engine's file download, which stages it and puts it in place as the policy
        // says: the module neither writes nor publishes the file itself.
        private void DownloadToFile(FilesConnection connection, string remotePath)
        {
            FilesDownloadSettings settings = Settings();
            if (settings.CheckpointPath != null && checkpointGiven)
            {
                // An earlier input of this pipeline had the checkpoint, and its download may have completed.
                throw new FilesException(FilesErrorKind.InvalidArgument, $"-CheckpointPath records one download, and an earlier file of this command already used it; that download may have completed. '{remotePath}' and any later file were not downloaded. Download each file with a checkpoint of its own.");
            }
            string destination = LocalPaths.DownloadDestination(this, LiteralPath, remotePath);
            bool replacing = CheckDestination(destination, settings);
            string checkpoint = settings.CheckpointPath == null ? "" : $", recording its progress in '{settings.CheckpointPath}'";
            checkpointGiven |= settings.CheckpointPath != null;
            if (!ShouldProcessOn(connection, $"file '{remotePath}' to '{destination}'{(replacing ? ", replacing it" : "")}{checkpoint}"))
            {
                return;
            }
            FilesDownloadResult result = Run(connection, $"Downloading {remotePath}", run => TransferBridge.DownloadFileAsync(connection, remotePath, destination, settings, run.Progress, CallOptionsFor(InputObject), run.Token));
            WritePassThru(result);
        }

        // The caller's choices for a local file, checked before anything is read or sent.
        private FilesDownloadSettings Settings()
        {
            if (IsBound(nameof(CheckpointRestart)) && CheckpointPath == null)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-CheckpointRestart says what to do with a checkpoint, and -CheckpointPath was not given.");
            }
            return new FilesDownloadSettings(
                IsBound(nameof(MaxConcurrency)) ? MaxConcurrency : (int?)null,
                IsBound(nameof(PartSize)) ? PartSize : (long?)null,
                Limiter,
                Force.IsPresent,
                CheckpointPath == null ? null : LocalPaths.CheckpointFile(this, CheckpointPath),
                IsBound(nameof(CheckpointRestart)) ? CheckpointRestart : (FilesCheckpointRestart?)null);
        }

        // Refuses, before confirming or sending anything, a destination the engine's policy would refuse: without -Force
        // anything at the name, even a link to nothing; with it, a folder or a link. The engine refuses such a destination
        // as well, and anything else it finds there when it puts the file in place. An existing checkpoint can record
        // this download's own work, a stage kept for a file refused at this name, or a file an earlier run already put
        // there, which only the engine can recognize, so then the engine alone decides. Returns whether -Force is to
        // replace a regular file there.
        private static bool CheckDestination(string destination, FilesDownloadSettings settings)
        {
            FileAttributes? entry = EntryAttributes(destination);
            if (!entry.HasValue)
            {
                return false;
            }
            bool regular = (entry.Value & (FileAttributes.Directory | FileAttributes.ReparsePoint)) == 0;
            if (settings.CheckpointPath != null && File.Exists(settings.CheckpointPath))
            {
                return regular && settings.ReplaceRegularFile;
            }
            if (!settings.ReplaceRegularFile)
            {
                throw Refused(destination, settings, FilesPublicationProblem.DestinationExists, $"{destination} exists, and is left as it is: nothing was downloaded.");
            }
            if (!regular)
            {
                throw Refused(destination, settings, FilesPublicationProblem.DestinationNotRegularFile, $"{destination} is not a regular file, so it is not replaced: nothing was downloaded.");
            }
            return true;
        }

        // The attributes of whatever is at path, without following a link, or null when nothing is there or that cannot
        // be told, which the engine then finds out.
        private static FileAttributes? EntryAttributes(string path)
        {
            try
            {
                return File.GetAttributes(path);
            }
            catch (Exception failure) when (failure is IOException || failure is UnauthorizedAccessException)
            {
                return null;
            }
        }

        private static FilesException Refused(string destination, FilesDownloadSettings settings, FilesPublicationProblem problem, string message)
        {
            return new FilesException(FilesErrorKind.LocalFile, message)
            {
                PublicationProblem = problem,
                LocalPath = destination,
                CheckpointPath = settings.CheckpointPath,
            };
        }

        private void WritePassThru(FilesDownloadResult result)
        {
            if (PassThru.IsPresent)
            {
                WriteObject(result);
            }
        }
    }
}
