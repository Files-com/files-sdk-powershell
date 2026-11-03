using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Downloads a remote folder and everything in it into a local directory, several files at once. Every file is
    /// downloaded to the same relative path, as Receive-FilesFile downloads a file, and every empty folder is created.
    /// Each file appears under its name only once it is complete, where nothing is there, or with -Force replacing only
    /// a regular file. The first failure stops the files in progress and the command fails with it, once they have
    /// stopped; files completed before then stay in place, and nothing else in the local directory is touched. With
    /// -CheckpointDirectory, running the command again continues it.
    /// </summary>
    [Cmdlet(VerbsCommunications.Receive, "FilesDirectory", DefaultParameterSetName = RemotePathSet, SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(FilesDirectoryTransferResult))]
    public sealed class ReceiveFilesDirectoryCommand : FilesApiCmdlet
    {
        private const string RemotePathSet = "RemotePath";
        private const string InputObjectSet = "InputObject";

        // The local directories and checkpoint folders earlier folders of this command were given, so that no later
        // folder downloads over one of them or reuses another download's record.
        private readonly List<string> localDirectories = new List<string>();
        private readonly List<string> checkpointDirectories = new List<string>();

        /// <summary>The remote folder to download, used exactly as written; an empty string for the site's root.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = RemotePathSet)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string RemotePath { get; set; }

        /// <summary>A folder written by another FilesCom command, such as Get-FilesChildItem; its path, connection and workspace are used.</summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = InputObjectSet)]
        [ValidateNotNull]
        public Models.RemoteFile InputObject { get; set; }

        /// <summary>
        /// The local directory to download into, used exactly as written, and created with the folders above it if
        /// missing when the download starts; they stay whatever follows. The folder's contents go directly into it. For
        /// piped folders, a script block such as { Join-Path ./downloads $_.DisplayName } gives each its own directory;
        /// two folders of one command never download into the same one, or one inside the other.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = RemotePathSet)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = InputObjectSet)]
        [ValidateNotNullOrEmpty]
        public string LiteralPath { get; set; }

        /// <summary>
        /// Replaces a regular file at a downloaded file's name, giving the new file that file's group, access control list
        /// and permissions; nothing is replaced when those cannot be carried over. A folder, symbolic link or other special
        /// file is never replaced or followed. Without it, a file whose name is taken fails the download and the file there
        /// stays as it is.
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// The most files downloaded at once, from 1 to 64. Unset, the engine's automatic policy decides how many as the
        /// download goes, within its own limits. Each file fetches its own parts within the -Limiter.
        /// </summary>
        [Parameter]
        [ValidateRange(1, 64)]
        public int MaxConcurrentFiles { get; set; }

        /// <summary>
        /// The most parts of each file fetched at the same time, from 1 to 64. Unset, the engine's automatic policy
        /// decides how many as the download goes, within its own limits. Parts are fetched at once only when the file's
        /// download is bound to a version, as Receive-FilesFile describes. A -Limiter's request budget also applies.
        /// </summary>
        [Parameter]
        [ValidateRange(1, 64)]
        public int MaxConcurrency { get; set; }

        /// <summary>The size in bytes of each part fetched, 1 or more; by default 8 MiB. A file not bound to a version comes in one request.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long PartSize { get; set; }

        /// <summary>A limiter from New-FilesTransferLimiter whose request slots and buffer space every file of this download shares, with other transfers too.</summary>
        [Parameter]
        [ValidateNotNull]
        public FilesTransferLimiter Limiter { get; set; }

        /// <summary>
        /// A local folder in which the download records its progress, so that when it is stopped, by Ctrl+C, a failure or
        /// the process ending, running the same command again continues it: a file already completed is skipped once
        /// its storage confirms it is still the version downloaded and the local file is still the one written, and a file
        /// in progress continues from the parts it has. It is created if missing, for you only, holds no credentials or
        /// signed URLs, and must not be inside the local directory. Each folder downloaded needs one of its own. The
        /// engine relies on it only where no other user can change it, and otherwise refuses it with
        /// Files.CheckpointRefused once it has listed the remote folder, before any file is fetched.
        /// </summary>
        [Parameter(ParameterSetName = RemotePathSet)]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = InputObjectSet)]
        [ValidateNotNullOrEmpty]
        public string CheckpointDirectory { get; set; }

        /// <summary>
        /// What a file does when the checkpoint records a download of it that cannot be continued, such as one whose
        /// downloaded bytes are gone: Fail, the default, fails with Files.CheckpointRefused before anything is fetched;
        /// StartOver discards that file's own stage and checkpoint and downloads it again from its first byte, never
        /// touching the local file or another transfer's checkpoint. Needs -CheckpointDirectory.
        /// </summary>
        [Parameter]
        public FilesCheckpointRestart CheckpointRestart { get; set; }

        /// <summary>Writes the completed download.</summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>Downloads the current record's folder.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            FilesCallOptions options = CallOptionsFor(InputObject);
            string remote = RemoteFolder();
            ErrorTarget = remote;
            if (IsBound(nameof(CheckpointRestart)) && CheckpointDirectory == null)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-CheckpointRestart says what to do with a checkpoint, and -CheckpointDirectory was not given.");
            }
            string local = LocalPaths.DownloadDirectory(this, LiteralPath);
            string checkpoint = CheckpointDirectory == null ? null : LocalPaths.CheckpointFolderOutside(this, CheckpointDirectory, local);
            ClaimForThisFolder(remote, local, checkpoint);

            string what = remote.Length == 0 ? "the site's root folder" : $"folder '{remote}'";
            string replacing = Force.IsPresent ? ", replacing regular files there" : "";
            string recording = checkpoint == null ? "" : $", recording its progress in '{checkpoint}'";
            if (!ShouldProcessOn(connection, $"{what} and everything in it to '{local}'{replacing}{recording}"))
            {
                return;
            }
            var settings = new FilesDirectoryDownloadSettings(
                IsBound(nameof(MaxConcurrentFiles)) ? MaxConcurrentFiles : (int?)null,
                IsBound(nameof(MaxConcurrency)) ? MaxConcurrency : (int?)null,
                IsBound(nameof(PartSize)) ? PartSize : (long?)null,
                Limiter,
                Force.IsPresent,
                checkpoint,
                IsBound(nameof(CheckpointRestart)) ? CheckpointRestart : (FilesCheckpointRestart?)null);
            FilesDirectoryTransferResult result;
            try
            {
                result = Run(connection, remote.Length == 0 ? "Downloading the site's root folder" : $"Downloading {remote}", run => TransferBridge.DownloadDirectoryAsync(
                    connection,
                    remote,
                    local,
                    settings,
                    run.Progress,
                    options,
                    run.Token));
            }
            catch (FilesException failure) when (failure.LocalPath != null)
            {
                // The error is about the one file that was not put in place; the files completed before it stay.
                ErrorTarget = failure.LocalPath;
                throw;
            }
            if (PassThru.IsPresent)
            {
                WriteObject(result);
            }
        }

        private string RemoteFolder()
        {
            if (InputObject == null)
            {
                return RemotePath;
            }
            if (InputObject.Path == null)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.");
            }
            if (InputObject.Type != "directory")
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"'{InputObject.Path}' is not a folder{(InputObject.Type == null ? "" : $" but a {InputObject.Type}")}. Receive-FilesFile downloads a file.");
            }
            return InputObject.Path;
        }

        // Refuses a local directory that is, holds or is inside one an earlier folder of this command downloads into,
        // since a file of one could take the name of the other's, and likewise a checkpoint folder, since it records one
        // download. Both are refused before anything is sent for this folder.
        private void ClaimForThisFolder(string remote, string local, string checkpoint)
        {
            string taken = localDirectories.Find(earlier => Overlap(local, earlier));
            if (taken != null)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"'{remote}' would download into '{local}', and an earlier folder of this command downloads into '{taken}'. '{remote}' and any later folder were not downloaded. Give each folder a directory of its own, for example with a script block such as {{ Join-Path ./downloads $_.DisplayName }}.");
            }
            string used = checkpoint == null ? null : checkpointDirectories.Find(earlier => Overlap(checkpoint, earlier));
            if (used != null)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"-CheckpointDirectory '{checkpoint}' records one folder's download, and an earlier folder of this command records its own in '{used}'. '{remote}' and any later folder were not downloaded. Give each folder a checkpoint folder of its own.");
            }
            localDirectories.Add(local);
            if (checkpoint != null)
            {
                checkpointDirectories.Add(checkpoint);
            }
        }

        private static bool Overlap(string one, string other)
        {
            return LocalPaths.IsSameOrInside(one, other) || LocalPaths.IsSameOrInside(other, one);
        }
    }
}
