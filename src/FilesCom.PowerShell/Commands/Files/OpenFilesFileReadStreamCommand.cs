using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Opens a remote file to read as a forward-only stream, which the Files.com engine downloads behind it as it is
    /// read, bound to one version of the file where its storage allows and read ahead by at most the parts in progress and
    /// 1 MiB, so that the file is processed without holding it in memory or on disk. The stream goes on reading after
    /// the command returns, with its connection and workspace; dispose it when done.
    /// </summary>
    [Cmdlet(VerbsCommon.Open, "FilesFileReadStream")]
    [OutputType(typeof(FilesRemoteReadStream))]
    public sealed class OpenFilesFileReadStreamCommand : FilesApiCmdlet
    {
        /// <summary>The remote file, used exactly as written.</summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// The most parts fetched at the same time, from 1 to 64. Unset, the engine's automatic policy decides how many as
        /// the download goes, within its own limits. Parts are fetched at once only when the download is bound to a version
        /// of the file.
        /// </summary>
        [Parameter]
        [ValidateRange(1, 64)]
        public int MaxConcurrency { get; set; }

        /// <summary>
        /// The size in bytes of each part fetched, 1 or more; by default 8 MiB. The stream holds at most the parts in
        /// progress and 1 MiB ahead of the reader.
        /// </summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long PartSize { get; set; }

        /// <summary>Opens the current record's file.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            ErrorTarget = Path;
            // The command owns the stream from when it is opened until it reaches the pipeline: a failure between,
            // such as writing the command's diagnostics or the pipeline stopping, stops its download.
            FilesRemoteReadStream opened = null;
            try
            {
                Run(connection, async token => opened = await HelperBridge.OpenReadAsync(
                    connection,
                    Path,
                    IsBound(nameof(MaxConcurrency)) ? MaxConcurrency : (int?)null,
                    IsBound(nameof(PartSize)) ? PartSize : (long?)null,
                    CallOptions,
                    token).ConfigureAwait(false));
                WriteObject(opened);
            }
            catch
            {
                opened?.Dispose();
                throw;
            }
        }
    }
}
