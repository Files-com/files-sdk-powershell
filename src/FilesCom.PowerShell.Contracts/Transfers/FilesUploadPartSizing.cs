namespace FilesCom.PowerShell
{
    /// <summary>
    /// How an upload sizes its parts. The upload chooses the sizes only where its parts go straight to Amazon S3 as an
    /// ordinary multipart upload, as the part URLs Files.com gives when the upload starts show, and then within the
    /// engine's own planning limits for S3. Elsewhere every mode uses the sizes Files.com plans, and the result's
    /// ServerPlannedReason says why. How many parts are sent at once is chosen separately, by -ConcurrencyMode.
    /// </summary>
    public enum FilesUploadPartSizingMode
    {
        /// <summary>
        /// Chooses a size from the file's size, and tries larger parts for the parts not yet read when sending more parts
        /// at once no longer raises throughput. The default.
        /// </summary>
        Automatic,

        /// <summary>
        /// Sends every part but the last with -PartSize bytes. Where the upload chooses its sizes, it fails before sending
        /// any part if that size is outside the engine's limits.
        /// </summary>
        Fixed,

        /// <summary>Uses the sizes Files.com plans, even where the upload could choose.</summary>
        ServerPlanned,
    }

    /// <summary>Where an upload from a stream keeps each part until Files.com has accepted it, so that a retry resends the same bytes.</summary>
    public enum FilesStreamPartStorage
    {
        /// <summary>
        /// In memory. The parts held at once stay within the transfer limiter's buffer space, which also limits how
        /// large one part can be. The default.
        /// </summary>
        Memory,

        /// <summary>
        /// In a temporary file of its own, deleted once the part has been accepted or the upload ends. Memory then no
        /// longer limits how large parts can be; the files use disk space for the parts read ahead and being sent.
        /// </summary>
        TemporaryFiles,
    }

    /// <summary>Where an upload's part sizes came from.</summary>
    public enum FilesPartSizeSource
    {
        /// <summary>From Files.com's plan; ServerPlannedReason says why.</summary>
        ServerPlanned,

        /// <summary>Chosen by the upload, as -PartSizingMode Automatic describes.</summary>
        Automatic,

        /// <summary>-PartSize.</summary>
        Fixed,
    }

    /// <summary>Why an upload used the part sizes Files.com plans.</summary>
    public enum FilesServerPlannedReason
    {
        /// <summary>It did not: the upload chose its sizes.</summary>
        None,

        /// <summary>-PartSizingMode ServerPlanned was given.</summary>
        Requested,

        /// <summary>
        /// The upload could not choose part sizes: its parts do not go straight to Amazon S3, or the file does not fit
        /// within the engine's limits for S3.
        /// </summary>
        NotAllowed,

        /// <summary>
        /// The stream's length was not known and its parts do not go straight to Amazon S3, so they were sent as
        /// Files.com planned them, one at a time.
        /// </summary>
        LengthUnknown,

        /// <summary>
        /// The upload could have chosen part sizes for the stream of unknown length, but parts held in memory could not
        /// be large enough for -MaxLength, or the largest file the engine plans for, to fit in the parts available. Its
        /// parts went one at a time, as Files.com planned them. Declare a smaller -MaxLength, or keep parts in
        /// temporary files with -PartStorage TemporaryFiles.
        /// </summary>
        MemoryLimit,
    }

    /// <summary>What most limited the part size an upload started with.</summary>
    public enum FilesPartSizeConstraint
    {
        /// <summary>Nothing: the size is the one preferred for the file's size, or the size given, or Files.com's own.</summary>
        None,

        /// <summary>
        /// The largest size the upload chooses on its own: 256 MiB, or the MaxPartSize of -AutomaticPartSizing. Parts are
        /// larger only where the file needs them to be, to stay within the number of parts available.
        /// </summary>
        MaxPartSize,

        /// <summary>The smallest part other than the last the destination allows: 5 MiB, in the engine's limits for S3.</summary>
        ServerMinimum,

        /// <summary>The largest part the engine plans for the destination: 1 GiB, in its limits for S3.</summary>
        ServerMaximum,

        /// <summary>The number of parts available: 10,000, or fewer where Files.com says so. Smaller parts would need more.</summary>
        PartLimit,

        /// <summary>The buffer space of the transfer limiter, which holds whole parts of a stream in memory until they are accepted.</summary>
        BufferLimit,

        /// <summary>The smallest size the upload chooses for a part other than the last: the MinPartSize of -AutomaticPartSizing.</summary>
        MinPartSize,
    }

    /// <summary>
    /// The limits within which an upload whose parts go straight to Amazon S3 chooses their sizes: the engine's own
    /// planning limits, within S3's rules, and no promise that Files.com or the destination accepts any file within them.
    /// </summary>
    public sealed class FilesPartSizeLimits
    {
        internal FilesPartSizeLimits(long minNonfinalBytes, long maxPartBytes, long maxParts, long maxFileBytes)
        {
            MinNonfinalBytes = minNonfinalBytes;
            MaxPartBytes = maxPartBytes;
            MaxParts = maxParts;
            MaxFileBytes = maxFileBytes;
        }

        /// <summary>The smallest size in bytes of every part except the last.</summary>
        public long MinNonfinalBytes { get; }

        /// <summary>The largest size in bytes of any part.</summary>
        public long MaxPartBytes { get; }

        /// <summary>The most parts.</summary>
        public long MaxParts { get; }

        /// <summary>The largest file, in bytes.</summary>
        public long MaxFileBytes { get; }
    }
}
