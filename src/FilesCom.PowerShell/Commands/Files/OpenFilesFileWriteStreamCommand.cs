using System.Management.Automation;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Opens a remote file to write as a forward-only stream, whose bytes the Files.com engine uploads as they are
    /// written, so that a file is produced without holding it in memory or on disk. Closing the stream completes the
    /// file, creating it or replacing it, and waits for the upload; Abandon, before closing, leaves the file as it was.
    /// The stream goes on writing after the command returns, with its connection and workspace.
    /// </summary>
    [Cmdlet(VerbsCommon.Open, "FilesFileWriteStream", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(FilesRemoteWriteStream))]
    public sealed class OpenFilesFileWriteStreamCommand : FilesApiCmdlet
    {
        /// <summary>The remote file, used exactly as written.</summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>Creates missing folders above the file.</summary>
        [Parameter]
        public SwitchParameter MkdirParents { get; set; }

        /// <summary>Opens the file, once the change is confirmed.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            ErrorTarget = Path;
            if (!ShouldProcessOn(connection, $"file '{Path}', creating or replacing it with what is written once the stream is closed"))
            {
                return;
            }
            // The command owns the stream from when it is opened until it reaches the pipeline: a failure between,
            // such as writing the command's diagnostics or the pipeline stopping, abandons its upload, and nothing is
            // completed.
            FilesRemoteWriteStream opened = null;
            try
            {
                Run(connection, async token => opened = await HelperBridge.OpenWriteAsync(connection, Path, IsBound(nameof(MkdirParents)) ? MkdirParents.IsPresent : (bool?)null, CallOptions, token).ConfigureAwait(false));
                WriteObject(opened);
            }
            catch
            {
                if (opened != null)
                {
                    opened.Abandon();
                    try
                    {
                        opened.Dispose();
                    }
                    catch (FilesException)
                    {
                        // Closing an abandoned stream reports only an unknown completion, which cannot follow here; the
                        // failure that stopped the command is the one reported.
                    }
                }
                throw;
            }
        }
    }
}
