using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// Expert tuning for uploads whose concurrency is Automatic, made by New-FilesAutomaticConcurrency: how many parts an
    /// upload starts with, how far and how fast it may grow, and how it backs off after failures, throttling or rising
    /// latency. Give it to <c>Send-FilesFile -AutomaticConcurrency</c>.
    /// </summary>
    /// <remarks>
    /// Every setting holds its effective value: the one given to New-FilesAutomaticConcurrency, or the default of
    /// <see cref="Profile"/>. An upload given this tuning uses it whatever the destination; an upload without it keeps
    /// the defaults of the profile chosen for its destination. The object never changes, so one can be shared by many
    /// uploads.
    /// </remarks>
    public sealed class FilesAutomaticConcurrency
    {
        internal FilesAutomaticConcurrency(
            FilesUploadPerformanceProfile profile,
            int startTarget,
            int minTarget,
            int softCeiling,
            int maxTarget,
            long softCeilingUnlockBytes,
            double softCeilingUnlockBytesPerSecond,
            int windowAttempts,
            int growthIntervalWindows,
            int growthStep,
            double probeGainPercent,
            int plateauTarget,
            double probeGainPerAddedPartPercent,
            int probeWindows,
            double shrinkPercent,
            int holdWindows,
            double latencyGrowthQueueLimit,
            double latencyBackoffQueueLimit,
            double backpressureShrinkPercent,
            TimeSpan backpressurePause,
            IReadOnlyList<string> overrides)
        {
            Profile = profile;
            StartTarget = startTarget;
            MinTarget = minTarget;
            SoftCeiling = softCeiling;
            MaxTarget = maxTarget;
            SoftCeilingUnlockBytes = softCeilingUnlockBytes;
            SoftCeilingUnlockBytesPerSecond = softCeilingUnlockBytesPerSecond;
            WindowAttempts = windowAttempts;
            GrowthIntervalWindows = growthIntervalWindows;
            GrowthStep = growthStep;
            ProbeGainPercent = probeGainPercent;
            PlateauTarget = plateauTarget;
            ProbeGainPerAddedPartPercent = probeGainPerAddedPartPercent;
            ProbeWindows = probeWindows;
            ShrinkPercent = shrinkPercent;
            HoldWindows = holdWindows;
            LatencyGrowthQueueLimit = latencyGrowthQueueLimit;
            LatencyBackoffQueueLimit = latencyBackoffQueueLimit;
            BackpressureShrinkPercent = backpressureShrinkPercent;
            BackpressurePause = backpressurePause;
            Overrides = overrides;
        }

        /// <summary>The profile whose defaults the settings not given came from, and which the upload reports.</summary>
        public FilesUploadPerformanceProfile Profile { get; }

        /// <summary>The number of parts at once an upload starts with.</summary>
        public int StartTarget { get; }

        /// <summary>The fewest parts at once failures, throttling or latency can lower it to.</summary>
        public int MinTarget { get; }

        /// <summary>The most parts at once growth reaches until the upload has moved SoftCeilingUnlockBytes fast enough.</summary>
        public int SoftCeiling { get; }

        /// <summary>The most parts at once the upload ever sends. Send-FilesFile -MaxConcurrency, when given, also limits it.</summary>
        public int MaxTarget { get; }

        /// <summary>The bytes of accepted parts an upload must have delivered before it may grow past SoftCeiling.</summary>
        public long SoftCeilingUnlockBytes { get; }

        /// <summary>The throughput, in bytes per second, a measured window must reach before the upload may grow past SoftCeiling.</summary>
        public double SoftCeilingUnlockBytesPerSecond { get; }

        /// <summary>The fewest successful part requests in one measurement window.</summary>
        public int WindowAttempts { get; }

        /// <summary>The number of measured windows at a number of parts at once before it grows.</summary>
        public int GrowthIntervalWindows { get; }

        /// <summary>The number of parts at once each growth adds.</summary>
        public int GrowthStep { get; }

        /// <summary>The throughput gain, in percent, a grown number must show to be kept.</summary>
        public double ProbeGainPercent { get; }

        /// <summary>The number of parts at once above which each added part must earn ProbeGainPerAddedPartPercent.</summary>
        public int PlateauTarget { get; }

        /// <summary>The throughput gain, in percent, each part at once added above PlateauTarget must earn.</summary>
        public double ProbeGainPerAddedPartPercent { get; }

        /// <summary>The number of measured windows a grown number gets to show its gain.</summary>
        public int ProbeWindows { get; }

        /// <summary>How much, in percent, a temporary failure or rising latency lowers the number of parts at once.</summary>
        public double ShrinkPercent { get; }

        /// <summary>The number of measured windows growth waits after the number was lowered or a growth given up.</summary>
        public int HoldWindows { get; }

        /// <summary>The estimated number of part requests queued, rather than moving data, above which growth waits; 0 turns this off.</summary>
        public double LatencyGrowthQueueLimit { get; }

        /// <summary>The estimated number of part requests queued above which, with falling throughput, the number is lowered; 0 turns this off.</summary>
        public double LatencyBackoffQueueLimit { get; }

        /// <summary>How much, in percent, a throttling answer lowers the number of parts at once.</summary>
        public double BackpressureShrinkPercent { get; }

        /// <summary>The shortest time a throttling answer stops the upload from starting part requests, including retries.</summary>
        public TimeSpan BackpressurePause { get; }

        /// <summary>The settings given to New-FilesAutomaticConcurrency; the others are the profile's defaults.</summary>
        public IReadOnlyList<string> Overrides { get; }
    }

    /// <summary>
    /// Expert tuning for uploads whose part sizes are Automatic, made by New-FilesAutomaticPartSizing: the size parts start
    /// with, the bounds the upload keeps them in, and how it tries larger parts. Give it to
    /// <c>Send-FilesFile -AutomaticPartSizing</c>. It applies only where the upload chooses its part sizes: where its
    /// parts go straight to Amazon S3, within the engine's limits.
    /// </summary>
    /// <remarks>
    /// Every setting holds its effective value: the one given to New-FilesAutomaticPartSizing, or the engine's default.
    /// The object never changes, so one can be shared by many uploads.
    /// </remarks>
    public sealed class FilesAutomaticPartSizing
    {
        internal FilesAutomaticPartSizing(
            IReadOnlyList<FilesPartSizeTier> knownSizeTiers,
            long? workloadBytes,
            int workloadParts,
            long unknownLengthStartSize,
            long minPartSize,
            long maxPartSize,
            double growthFactor,
            int growthIntervalWindows,
            double probeGainPercent,
            int probeWindows,
            int holdWindows,
            IReadOnlyList<string> overrides)
        {
            KnownSizeTiers = knownSizeTiers;
            WorkloadBytes = workloadBytes;
            WorkloadParts = workloadParts;
            UnknownLengthStartSize = unknownLengthStartSize;
            MinPartSize = minPartSize;
            MaxPartSize = maxPartSize;
            GrowthFactor = growthFactor;
            GrowthIntervalWindows = growthIntervalWindows;
            ProbeGainPercent = probeGainPercent;
            ProbeWindows = probeWindows;
            HoldWindows = holdWindows;
            Overrides = overrides;
        }

        /// <summary>The part size a file starts with, by the file's size, in order of MaxFileBytes. Files larger than every tier use the last.</summary>
        public IReadOnlyList<FilesPartSizeTier> KnownSizeTiers { get; }

        /// <summary>The size of the whole workload the upload belongs to, in bytes, or null for the file's own size. Only the starting size uses it.</summary>
        public long? WorkloadBytes { get; }

        /// <summary>The fewest parts the workload should be divided into: a smaller workload gets smaller parts.</summary>
        public int WorkloadParts { get; }

        /// <summary>The size, in bytes, the parts of a stream of unknown length start with.</summary>
        public long UnknownLengthStartSize { get; }

        /// <summary>The smallest size, in bytes, the upload chooses for a part other than the last. The engine's minimum applies too.</summary>
        public long MinPartSize { get; }

        /// <summary>The largest size, in bytes, the upload chooses or grows parts to. Parts can be larger when the file needs them to be.</summary>
        public long MaxPartSize { get; }

        /// <summary>How many times larger each growth makes the parts not yet read.</summary>
        public double GrowthFactor { get; }

        /// <summary>The number of measured windows at a size before parts grow.</summary>
        public int GrowthIntervalWindows { get; }

        /// <summary>The throughput gain, in percent, larger parts must show to be kept.</summary>
        public double ProbeGainPercent { get; }

        /// <summary>The number of measured windows larger parts get to show their gain.</summary>
        public int ProbeWindows { get; }

        /// <summary>The number of measured windows growth waits after larger parts were given up.</summary>
        public int HoldWindows { get; }

        /// <summary>The settings given to New-FilesAutomaticPartSizing; the others are the engine's defaults.</summary>
        public IReadOnlyList<string> Overrides { get; }
    }

    /// <summary>A starting part size for files up to a given size, one of <see cref="FilesAutomaticPartSizing.KnownSizeTiers"/>.</summary>
    public sealed class FilesPartSizeTier
    {
        internal FilesPartSizeTier(long maxFileBytes, long partSize)
        {
            MaxFileBytes = maxFileBytes;
            PartSize = partSize;
        }

        /// <summary>The largest file, in bytes, the tier applies to.</summary>
        public long MaxFileBytes { get; }

        /// <summary>The part size, in bytes, uploads of such a file start with.</summary>
        public long PartSize { get; }

        /// <summary>The tier, such as <c>268435456 -> 8388608</c>.</summary>
        public override string ToString() => $"{MaxFileBytes} -> {PartSize}";
    }
}
