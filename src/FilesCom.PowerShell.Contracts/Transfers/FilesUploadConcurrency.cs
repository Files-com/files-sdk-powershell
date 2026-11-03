namespace FilesCom.PowerShell
{
    /// <summary>How an upload decides how many of its parts to send at the same time.</summary>
    public enum FilesUploadConcurrencyMode
    {
        /// <summary>
        /// Adjusts the number to what the upload measures: it adds parts while they raise throughput and sends fewer
        /// after failures, throttling or rising latency, within -MaxConcurrency when that is given. The default.
        /// </summary>
        Automatic,

        /// <summary>Sends -MaxConcurrency parts at once, or 4 when it is not given, whatever the upload measures.</summary>
        Static,
    }

    /// <summary>
    /// The kind of destination an automatic upload's starting point and limits are tuned for. It is only a performance
    /// preference: it never changes the parts Files.com asks for, and never sends parts in parallel when Files.com says
    /// they must be sent one at a time. Without -PerformanceProfile, an automatic upload uses Agent when Files.com
    /// supplies direct connection information for it (a Files.com Agent or Integration Worker destination), the scaled S3
    /// policy where its parts go straight to Amazon S3 as an ordinary multipart upload, as the negotiated parts show, and
    /// Generic elsewhere; given, it is used as it is, and S3 names the conservative S3 preset.
    /// </summary>
    public enum FilesUploadPerformanceProfile
    {
        /// <summary>Any destination: favors stability. Used for every destination other than an ordinary multipart upload to Amazon S3.</summary>
        Generic,

        /// <summary>Tuned for Amazon S3: starts with more parts at once and can grow further on fast uploads.</summary>
        S3,

        /// <summary>Files.com Agent and Integration Worker destinations, which have their own limits: starts lower and stays lower.</summary>
        Agent,
    }

    /// <summary>Why an upload sent its parts one at a time.</summary>
    public enum FilesUploadSerialReason
    {
        /// <summary>It did not: Files.com allowed parallel parts throughout.</summary>
        None,

        /// <summary>Files.com did not allow parallel parts for the upload's first part.</summary>
        ServerRequiresSerialParts,

        /// <summary>Files.com withdrew permission to send parts in parallel for a later part.</summary>
        ServerWithdrewParallelParts,

        /// <summary>
        /// The stream's length was not known, so each part was read, and sent, only once the one before it had been:
        /// only then was it known where the part starts, and whether the stream had ended.
        /// </summary>
        LengthUnknown,
    }
}
