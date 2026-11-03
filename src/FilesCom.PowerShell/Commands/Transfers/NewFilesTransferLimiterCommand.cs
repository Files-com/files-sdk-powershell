using System.Management.Automation;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Creates a budget of concurrent part requests and buffered bytes for uploads to share, such as uploads running in
    /// ForEach-Object -Parallel. Nothing is sent to Files.com.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "FilesTransferLimiter")]
    [OutputType(typeof(FilesTransferLimiter))]
    public sealed class NewFilesTransferLimiterCommand : FilesCmdlet
    {
        /// <summary>The most part requests the sharing uploads send at the same time. Defaults to the engine's default, 8.</summary>
        [Parameter]
        [ValidateRange(1, int.MaxValue)]
        public int MaxConcurrentRequests { get; set; }

        /// <summary>The most bytes of file contents the sharing uploads hold in memory at once. Defaults to the engine's default, 128 MiB.</summary>
        [Parameter]
        [ValidateRange(1, long.MaxValue)]
        public long MaxBufferedBytes { get; set; }

        /// <summary>Writes the new limiter.</summary>
        protected override void Execute()
        {
            WriteObject(TransferBridge.NewLimiter(
                IsBound(nameof(MaxConcurrentRequests)) ? MaxConcurrentRequests : (int?)null,
                IsBound(nameof(MaxBufferedBytes)) ? MaxBufferedBytes : (long?)null));
        }
    }
}
