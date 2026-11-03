namespace FilesCom.PowerShell
{
    /// <summary>
    /// A budget of concurrent part requests and buffered bytes that uploads share, made by New-FilesTransferLimiter.
    /// Give the same limiter to several <c>Send-FilesFile</c> commands, including ones in other runspaces or with other
    /// connections, to have them take turns within one budget.
    /// </summary>
    public sealed class FilesTransferLimiter
    {
        internal FilesTransferLimiter(int maxConcurrentRequests, long maxBufferedBytes, object engineLimiter)
        {
            MaxConcurrentRequests = maxConcurrentRequests;
            MaxBufferedBytes = maxBufferedBytes;
            EngineLimiter = engineLimiter;
        }

        /// <summary>The most part requests the uploads send at the same time.</summary>
        public int MaxConcurrentRequests { get; }

        /// <summary>The most bytes of file contents the uploads hold in memory at the same time.</summary>
        public long MaxBufferedBytes { get; }

        internal object EngineLimiter { get; }
    }
}
