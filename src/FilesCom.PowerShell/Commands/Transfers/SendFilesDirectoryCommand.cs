using System.IO;
using System.Management.Automation;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Uploads a local folder and everything in it into a remote folder, several files at once. Every regular file is
    /// uploaded to the same relative path, and every folder with nothing inside to upload is created. Symbolic links are
    /// not followed or uploaded. The first failure stops the files in progress and the command fails with it, once they
    /// have stopped; files completed before then stay completed. With -CheckpointDirectory, running the command again
    /// continues it.
    /// </summary>
    [Cmdlet(VerbsCommunications.Send, "FilesDirectory", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(FilesDirectoryTransferResult))]
    public sealed class SendFilesDirectoryCommand : FilesApiCmdlet
    {
        /// <summary>The local folder to upload, used exactly as written.</summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string LiteralPath { get; set; }

        /// <summary>
        /// The remote folder to upload into, created if missing; an empty string for the site's root. Defaults to a folder
        /// named like the local folder in the site's root.
        /// </summary>
        [Parameter(Position = 1)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string RemotePath { get; set; }

        /// <summary>
        /// The most files uploaded at once, from 1 to 64. Unset, the engine's automatic policy decides how many as the
        /// upload goes, within its own limits. Each file sends its own parts within the -Limiter.
        /// </summary>
        [Parameter]
        [ValidateRange(1, 64)]
        public int MaxConcurrentFiles { get; set; }

        /// <summary>
        /// Starts the files whose parts go straight to Amazon S3 with the scaled S3 policy at the policy's 150 parts at
        /// once, together, skipping initial concurrency probes on a known high-throughput network. Without it, or with
        /// -AggressiveStartup:$false, they probe the link from one part at a time and add parts as answered parts show
        /// they help, which can reach the same throughput and concurrency ceiling; only the ramp-up takes longer. Either
        /// way the files then share one target that adapts with the same policy, within the -Limiter. Files to other
        /// destinations start from their own defaults.
        /// </summary>
        [Parameter]
        public SwitchParameter AggressiveStartup { get; set; }

        /// <summary>
        /// A local folder in which the upload records its progress, so that when it is stopped, by Ctrl+C, a failure or
        /// the process ending, running the same command again continues it: a file already completed is skipped while it
        /// still has the bytes that were sent, and a file in progress continues. It is created if missing, for you only,
        /// holds no credentials or signed URLs, and must not be inside the folder uploaded. The engine relies on it only
        /// where no other user can change it, and refuses it otherwise, with Files.CheckpointRefused.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string CheckpointDirectory { get; set; }

        /// <summary>A limiter from New-FilesTransferLimiter whose request slots and buffer space every file of this upload shares, with other transfers too.</summary>
        [Parameter]
        [ValidateNotNull]
        public FilesTransferLimiter Limiter { get; set; }

        /// <summary>Writes the completed upload.</summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>Uploads the folder.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            string local = LocalPaths.ExistingFolder(this, LiteralPath);
            string remote = RemotePath ?? Path.GetFileName(local.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string checkpoint = CheckpointDirectory == null ? null : LocalPaths.CheckpointFolder(this, CheckpointDirectory);
            ErrorTarget = remote;
            string recording = checkpoint == null ? "" : $", recording its progress in '{checkpoint}'";
            if (!ShouldProcessOn(connection, $"'{local}' and everything in it to folder '{remote}'{recording}"))
            {
                return;
            }
            FilesDirectoryTransferResult result = Run(connection, $"Uploading {local}", run => TransferBridge.UploadDirectoryAsync(
                connection,
                local,
                remote,
                IsBound(nameof(MaxConcurrentFiles)) ? MaxConcurrentFiles : (int?)null,
                AggressiveStartup.IsPresent,
                Limiter,
                checkpoint,
                run.Progress,
                CallOptions,
                run.Token));
            if (PassThru.IsPresent)
            {
                WriteObject(result);
            }
        }
    }
}
