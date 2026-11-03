namespace FilesCom.PowerShell
{
    /// <summary>The limit an upload needed more than, reported with Files.LimitExceeded.</summary>
    public enum FilesUploadLimit
    {
        /// <summary>A stream without -Length gave more than it may hold: -MaxLength, or the largest file the engine plans for.</summary>
        StreamLength,

        /// <summary>
        /// The file or stream needs more parts than are available, with parts no larger than the engine's limits allow or,
        /// for a stream's parts in memory, than the transfer limiter's buffer space allows.
        /// </summary>
        PartCount,

        /// <summary>A part must be larger than the transfer limiter may buffer (New-FilesTransferLimiter -MaxBufferedBytes).</summary>
        BufferSpace,

        /// <summary>A part of a stream kept in memory would be larger than a single array can hold.</summary>
        MemoryPart,

        /// <summary>The stream is longer than the -MaxLength -Spool may copy to its temporary file. Nothing was uploaded.</summary>
        SpoolLength,

        /// <summary>
        /// -PartSize cannot be used: it is outside the part sizes the engine's limits allow, more than the transfer limiter
        /// may buffer, or would need more parts than are available.
        /// </summary>
        FixedPartSize,
    }
}
