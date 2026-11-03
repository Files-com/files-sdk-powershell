using System;
using FilesCom.PowerShell.Models;

namespace FilesCom.PowerShell
{
    /// <summary>A completed upload, written by <c>Send-FilesFile -PassThru</c>.</summary>
    public sealed class FilesUploadResult
    {
        internal FilesUploadResult(
            string path,
            long size,
            RemoteFile file,
            FilesUploadedPart[] parts,
            bool parallelParts,
            long sentBytes,
            TimeSpan elapsed,
            FilesUploadConcurrencyMode concurrencyMode,
            FilesUploadPerformanceProfile performanceProfile,
            int peakConcurrency,
            FilesUploadSerialReason serialReason,
            int partRetries,
            FilesUploadPartSizingMode partSizingMode,
            FilesPartSizeSource partSizeSource,
            FilesServerPlannedReason serverPlannedReason,
            FilesPartSizeConstraint partSizeConstraint,
            FilesPartSizeLimits partSizeLimits,
            long largestPartSize,
            FilesUploadProfileSource performanceProfileSource,
            FilesUploadMetrics metrics)
        {
            Path = path;
            Size = size;
            File = file;
            Parts = parts;
            ParallelParts = parallelParts;
            SentBytes = sentBytes;
            Elapsed = elapsed;
            ConcurrencyMode = concurrencyMode;
            PerformanceProfile = performanceProfile;
            PeakConcurrency = peakConcurrency;
            SerialReason = serialReason;
            PartRetries = partRetries;
            PartSizingMode = partSizingMode;
            PartSizeSource = partSizeSource;
            ServerPlannedReason = serverPlannedReason;
            PartSizeConstraint = partSizeConstraint;
            PartSizeLimits = partSizeLimits;
            LargestPartSize = largestPartSize;
            PerformanceProfileSource = performanceProfileSource;
            Metrics = metrics;
        }

        /// <summary>The remote path the file was uploaded to.</summary>
        public string Path { get; }

        /// <summary>The number of bytes uploaded.</summary>
        public long Size { get; }

        /// <summary>The file as Files.com described it when the upload completed.</summary>
        public RemoteFile File { get; }

        /// <summary>
        /// The number of parts an earlier run of an upload continued from its checkpoint had sent, which this run did not
        /// send again; 0 for an upload that was not continued.
        /// </summary>
        public int ResumedParts { get; internal set; }

        /// <summary>The bytes of <see cref="ResumedParts"/>.</summary>
        public long ResumedBytes { get; internal set; }

        /// <summary>The parts the file was sent in, by part number.</summary>
        public FilesUploadedPart[] Parts { get; }

        /// <summary>Whether Files.com allowed sending the parts in parallel.</summary>
        public bool ParallelParts { get; }

        /// <summary>The bytes written to the network, counting resent parts again.</summary>
        public long SentBytes { get; }

        /// <summary>How long the upload took.</summary>
        public TimeSpan Elapsed { get; }

        /// <summary>Whether the upload adapted how many parts it sent at once or kept that number fixed.</summary>
        public FilesUploadConcurrencyMode ConcurrencyMode { get; }

        /// <summary>
        /// The profile an automatic upload was tuned with: -PerformanceProfile, or the profile -AutomaticConcurrency started
        /// from, when given; otherwise Agent when Files.com supplied direct connection information for the upload, S3 (with
        /// the scaled S3 policy) for an ordinary multipart upload to Amazon S3, as the negotiated parts showed when the
        /// upload started, and Generic for any other destination. For a static upload, that kind of destination, which
        /// changed nothing it did.
        /// </summary>
        public FilesUploadPerformanceProfile PerformanceProfile { get; }

        /// <summary>
        /// Whether PerformanceProfile came from the destination the negotiated parts Files.com gave showed (Server) or from
        /// -PerformanceProfile or -AutomaticConcurrency (Options).
        /// </summary>
        public FilesUploadProfileSource PerformanceProfileSource { get; }

        /// <summary>The most part requests the upload had in flight at once.</summary>
        public int PeakConcurrency { get; }

        /// <summary>Why the upload sent its parts one at a time, or None.</summary>
        public FilesUploadSerialReason SerialReason { get; }

        /// <summary>The number of times a part was sent again after a temporary failure.</summary>
        public int PartRetries { get; }

        /// <summary>The mode the upload was asked to size its parts with.</summary>
        public FilesUploadPartSizingMode PartSizingMode { get; }

        /// <summary>Where the part sizes came from: Files.com's plan, the upload's own choice, or -PartSize.</summary>
        public FilesPartSizeSource PartSizeSource { get; }

        /// <summary>Why the upload used the part sizes Files.com planned, or None.</summary>
        public FilesServerPlannedReason ServerPlannedReason { get; }

        /// <summary>What most limited the part size the upload started with, or None.</summary>
        public FilesPartSizeConstraint PartSizeConstraint { get; }

        /// <summary>
        /// The engine's limits for choosing part sizes, for an upload whose parts go straight to Amazon S3, even when it
        /// used the sizes Files.com plans; null for any other destination.
        /// </summary>
        public FilesPartSizeLimits PartSizeLimits { get; }

        /// <summary>The size in bytes of the largest part.</summary>
        public long LargestPartSize { get; }

        /// <summary>What the upload measured: waits, read-ahead pressure, its decisions and the process's resource use.</summary>
        public FilesUploadMetrics Metrics { get; }
    }
}
