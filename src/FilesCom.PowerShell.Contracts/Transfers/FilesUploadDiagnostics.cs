using System;

namespace FilesCom.PowerShell
{
    /// <summary>Where an automatic upload's concurrency tuning came from.</summary>
    public enum FilesUploadProfileSource
    {
        /// <summary>The tuning for the destination the negotiated parts showed: Agent's defaults when Files.com supplied direct connection information, the scaled S3 policy for an ordinary multipart upload to Amazon S3, Generic's defaults otherwise.</summary>
        Server,

        /// <summary>The caller's -PerformanceProfile or -AutomaticConcurrency.</summary>
        Options,
    }

    /// <summary>A concurrency decision of an automatic upload.</summary>
    public enum FilesConcurrencyDecision
    {
        /// <summary>The number of parts at once grew; the growth is a probe until its windows judge it.</summary>
        Grew,

        /// <summary>A grown number showed its gain and was kept.</summary>
        ProbeAccepted,

        /// <summary>A grown number did not show its gain and went back.</summary>
        ProbeRejected,

        /// <summary>A temporary failure lowered the number.</summary>
        ShrankAfterFailure,

        /// <summary>A throttling answer lowered the number.</summary>
        ShrankAfterThrottling,

        /// <summary>Rising latency with falling throughput lowered the number.</summary>
        ShrankForLatency,

        /// <summary>A throttling answer paused new part requests for Cooldown.</summary>
        CooldownStarted,

        /// <summary>Growth waited because requests were queuing rather than moving data.</summary>
        GrowthHeldForLatency,

        /// <summary>The upload moved enough data fast enough to grow past its soft ceiling.</summary>
        SoftCeilingUnlocked,

        /// <summary>A window did not count, because the upload was below its number for BelowTargetReason.</summary>
        WindowNotMeasured,

        /// <summary>A decision this module does not know.</summary>
        Other,

        /// <summary>
        /// An upload warm-starting with the scaled S3 policy chosen for it raised the number by one part: after the
        /// baseline its start measured once its connections were warm, after the part last added delivered at least half
        /// of what each part delivered before it, or after it rested; and only from a window whose requests were not
        /// queued (a queue estimate under half a request).
        /// </summary>
        WarmStartRaised,

        /// <summary>A window of the warm start's answered parts showed a fast link, and the number went to the policy's start.</summary>
        WarmStartFastLink,

        /// <summary>
        /// Once its connection was warm, the part the warm start last added did not deliver half of what each part
        /// delivered before it, or the window's requests were queued (a queue estimate of half a request or more) whatever
        /// it delivered, so the number went back to where it was before that raise. The warm start goes on: it rests there
        /// for windows of the policy's size before it tries one part more again.
        /// </summary>
        WarmStartSettled,
    }

    /// <summary>Why an upload had fewer part requests in flight than it allowed.</summary>
    public enum FilesBelowTargetReason
    {
        /// <summary>Not below its number, or not a window that was left unmeasured.</summary>
        None,

        /// <summary>Waiting for a shared transfer limiter's request slots.</summary>
        SharedLimiter,

        /// <summary>Waiting for the next part to be read.</summary>
        NoPartReady,

        /// <summary>Sending parts one at a time, as Files.com asked.</summary>
        SerialParts,
    }

    /// <summary>A part size decision of an upload that chooses its part sizes.</summary>
    public enum FilesPartSizeDecision
    {
        /// <summary>The first size was chosen; PartSizeConstraint says what most limited it.</summary>
        Chosen,

        /// <summary>The parts not yet read grew; the growth is a probe until its windows judge it.</summary>
        Grew,

        /// <summary>Larger parts showed their gain and were kept.</summary>
        ProbeAccepted,

        /// <summary>Larger parts did not show their gain, and the size went back.</summary>
        ProbeRejected,

        /// <summary>A decision this module does not know.</summary>
        Other,
    }

    /// <summary>How an upload whose records were captured with <c>Send-FilesFile -Diagnostics</c> ended.</summary>
    public enum FilesUploadTraceOutcome
    {
        /// <summary>The upload completed.</summary>
        Completed,

        /// <summary>The upload failed; the command reports the failure itself.</summary>
        Failed,
    }

    /// <summary>
    /// A detailed record of an upload's work, written to the Information stream by <c>Send-FilesFile -Diagnostics</c> with
    /// the tag FilesCom.UploadTrace: an attempt, a concurrency decision or a part size decision. Records hold only the
    /// remote path, times, decisions and counts: never addresses, headers, credentials or file contents.
    /// </summary>
    public abstract class FilesUploadTrace
    {
        internal FilesUploadTrace(string path, TimeSpan elapsed)
        {
            Path = path;
            Elapsed = elapsed;
        }

        /// <summary>The remote path being uploaded.</summary>
        public string Path { get; }

        /// <summary>The time since the upload started.</summary>
        public TimeSpan Elapsed { get; }
    }

    /// <summary>One request that sent a part, recorded when its answer arrived and before any retry waited.</summary>
    public sealed class FilesUploadAttemptTrace : FilesUploadTrace
    {
        internal FilesUploadAttemptTrace(string path, TimeSpan elapsed, long partNumber, int attempt, bool accepted, int? statusCode, bool throttled, TimeSpan? retryAfter, TimeSpan duration, TimeSpan admissionWait, long partBytes, long sentBytes, int target)
            : base(path, elapsed)
        {
            PartNumber = partNumber;
            Attempt = attempt;
            Accepted = accepted;
            StatusCode = statusCode;
            Throttled = throttled;
            RetryAfter = retryAfter;
            Duration = duration;
            AdmissionWait = admissionWait;
            PartBytes = partBytes;
            SentBytes = sentBytes;
            Target = target;
        }

        /// <summary>The part number, from 1.</summary>
        public long PartNumber { get; }

        /// <summary>Which attempt to send the part this was, from 1.</summary>
        public int Attempt { get; }

        /// <summary>Whether the storage server accepted the part.</summary>
        public bool Accepted { get; }

        /// <summary>The storage server's HTTP status, or null if no answer arrived.</summary>
        public int? StatusCode { get; }

        /// <summary>Whether the answer asked the upload to slow down: HTTP 429 or 503, or Retry-After on a failure.</summary>
        public bool Throttled { get; }

        /// <summary>The server's Retry-After, when it sent one.</summary>
        public TimeSpan? RetryAfter { get; }

        /// <summary>The time from sending the request to its answer, without waits before it.</summary>
        public TimeSpan Duration { get; }

        /// <summary>The time the request waited to be admitted, by the upload's own number and cooldown and by its limiter.</summary>
        public TimeSpan AdmissionWait { get; }

        /// <summary>The bytes in the part.</summary>
        public long PartBytes { get; }

        /// <summary>The bytes this request wrote to the network.</summary>
        public long SentBytes { get; }

        /// <summary>The number of parts at once the upload allowed once this request was taken into account.</summary>
        public int Target { get; }
    }

    /// <summary>A concurrency decision of an automatic upload.</summary>
    public sealed class FilesUploadDecisionTrace : FilesUploadTrace
    {
        internal FilesUploadDecisionTrace(string path, TimeSpan elapsed, FilesConcurrencyDecision decision, int previousTarget, int target, FilesBelowTargetReason belowTargetReason, double throughputBytesPerSecond, double baselineBytesPerSecond, double queueEstimate, TimeSpan cooldown)
            : base(path, elapsed)
        {
            Decision = decision;
            PreviousTarget = previousTarget;
            Target = target;
            BelowTargetReason = belowTargetReason;
            ThroughputBytesPerSecond = throughputBytesPerSecond;
            BaselineBytesPerSecond = baselineBytesPerSecond;
            QueueEstimate = queueEstimate;
            Cooldown = cooldown;
        }

        /// <summary>What the upload decided.</summary>
        public FilesConcurrencyDecision Decision { get; }

        /// <summary>The number of parts at once before the decision.</summary>
        public int PreviousTarget { get; }

        /// <summary>The number of parts at once after the decision.</summary>
        public int Target { get; }

        /// <summary>For WindowNotMeasured, why the upload was below its number; otherwise None.</summary>
        public FilesBelowTargetReason BelowTargetReason { get; }

        /// <summary>The throughput of the window behind the decision, in bytes per second, or 0.</summary>
        public double ThroughputBytesPerSecond { get; }

        /// <summary>The throughput the decision compared it with, in bytes per second, or 0.</summary>
        public double BaselineBytesPerSecond { get; }

        /// <summary>The estimated number of requests queued in the window behind the decision.</summary>
        public double QueueEstimate { get; }

        /// <summary>For CooldownStarted, how long new part requests are paused.</summary>
        public TimeSpan Cooldown { get; }
    }

    /// <summary>A part size decision of an upload that chooses its part sizes.</summary>
    public sealed class FilesUploadSizingTrace : FilesUploadTrace
    {
        internal FilesUploadSizingTrace(string path, TimeSpan elapsed, FilesPartSizeDecision decision, long previousSize, long size, FilesPartSizeConstraint constraint, double throughputBytesPerSecond, double baselineBytesPerSecond)
            : base(path, elapsed)
        {
            Decision = decision;
            PreviousSize = previousSize;
            Size = size;
            Constraint = constraint;
            ThroughputBytesPerSecond = throughputBytesPerSecond;
            BaselineBytesPerSecond = baselineBytesPerSecond;
        }

        /// <summary>What the upload decided.</summary>
        public FilesPartSizeDecision Decision { get; }

        /// <summary>The size before the decision, in bytes, or 0 for the first.</summary>
        public long PreviousSize { get; }

        /// <summary>The size of the parts read from now on, in bytes, unless a part holds the rest of the file.</summary>
        public long Size { get; }

        /// <summary>For Chosen, what most limited the size.</summary>
        public FilesPartSizeConstraint Constraint { get; }

        /// <summary>The throughput of the window behind the decision, in bytes per second, or 0.</summary>
        public double ThroughputBytesPerSecond { get; }

        /// <summary>The throughput the decision compared it with, in bytes per second, or 0.</summary>
        public double BaselineBytesPerSecond { get; }
    }

    /// <summary>
    /// The last Information record <c>Send-FilesFile -Diagnostics</c> writes for an upload that completed or failed: how
    /// many of its records were written, how many were left out, and whether the records are complete. An upload whose
    /// pipeline was stopped writes none.
    /// </summary>
    public sealed class FilesUploadTraceSummary
    {
        internal FilesUploadTraceSummary(string path, FilesUploadTraceOutcome outcome, int recordsWritten, int recordsDropped, int limit)
        {
            Path = path;
            Outcome = outcome;
            RecordsWritten = recordsWritten;
            RecordsDropped = recordsDropped;
            Limit = limit;
        }

        /// <summary>The remote path of the upload.</summary>
        public string Path { get; }

        /// <summary>Whether the upload completed or failed.</summary>
        public FilesUploadTraceOutcome Outcome { get; }

        /// <summary>The records written for the upload.</summary>
        public int RecordsWritten { get; }

        /// <summary>The records left out because the upload made more than -DiagnosticLimit.</summary>
        public int RecordsDropped { get; }

        /// <summary>The most records the upload could write, from -DiagnosticLimit.</summary>
        public int Limit { get; }

        /// <summary>Whether every record the upload made was written.</summary>
        public bool Complete => RecordsDropped == 0;
    }

    /// <summary>
    /// What an upload measured, as it reported when it ended: waits, read-ahead pressure, its concurrency and sizing
    /// decisions, and the process's CPU and memory use. <see cref="FilesUploadResult.Metrics"/>.
    /// </summary>
    /// <remarks>
    /// These describe the connection the upload had; measurements against a local test server say nothing about
    /// production throughput.
    /// </remarks>
    public sealed class FilesUploadMetrics
    {
        internal FilesUploadMetrics() { }

        /// <summary>The upload's size divided by its elapsed time, in bytes per second.</summary>
        public double ThroughputBytesPerSecond { get; internal set; }

        /// <summary>The number of parts Files.com accepted.</summary>
        public int PartsAcknowledged { get; internal set; }

        /// <summary>The total time part requests waited to be admitted, by the upload's own number and cooldown and by its limiter.</summary>
        public TimeSpan AdmissionWait { get; internal set; }

        /// <summary>The total time part requests waited for a request slot from the limiter.</summary>
        public TimeSpan RequestWait { get; internal set; }

        /// <summary>The total time spent waiting for buffer space from the limiter before reading parts.</summary>
        public TimeSpan BufferWait { get; internal set; }

        /// <summary>The number of times reading ahead stopped at -MaxReadyParts or -MaxReadyBytes.</summary>
        public int ReadyLimitWaits { get; internal set; }

        /// <summary>The number of parts at once the upload allowed when it ended.</summary>
        public int Target { get; internal set; }

        /// <summary>The fewest parts at once the upload could have gone down to.</summary>
        public int MinTarget { get; internal set; }

        /// <summary>The most parts at once growth could reach before the soft ceiling was unlocked.</summary>
        public int SoftCeiling { get; internal set; }

        /// <summary>The most parts at once the upload could reach.</summary>
        public int MaxTarget { get; internal set; }

        /// <summary>Whether the upload grew past its soft ceiling.</summary>
        public bool SoftCeilingUnlocked { get; internal set; }

        /// <summary>The number of times the number of parts at once grew.</summary>
        public int Grows { get; internal set; }

        /// <summary>The number of times it was lowered.</summary>
        public int Shrinks { get; internal set; }

        /// <summary>The number of growths that showed their gain and were kept.</summary>
        public int ProbesAccepted { get; internal set; }

        /// <summary>The number of growths that went back.</summary>
        public int ProbesRejected { get; internal set; }

        /// <summary>The measurement windows that counted.</summary>
        public int MeasuredWindows { get; internal set; }

        /// <summary>The windows that did not count, because the upload was below its number.</summary>
        public int UnmeasuredWindows { get; internal set; }

        /// <summary>The throttling answers the upload received.</summary>
        public int Throttles { get; internal set; }

        /// <summary>The temporary failures the upload received.</summary>
        public int TransientFailures { get; internal set; }

        /// <summary>The last concurrency decision, or null if there was none.</summary>
        public FilesConcurrencyDecision? LastDecision { get; internal set; }

        /// <summary>The throughput of the last measured window, in bytes per second.</summary>
        public double LastThroughputBytesPerSecond { get; internal set; }

        /// <summary>The best throughput of a measured window, in bytes per second.</summary>
        public double BestThroughputBytesPerSecond { get; internal set; }

        /// <summary>The estimated number of requests queued in the last measured window.</summary>
        public double QueueEstimate { get; internal set; }

        /// <summary>The time the upload kept its number of parts in flight.</summary>
        public TimeSpan TimeAtTarget { get; internal set; }

        /// <summary>The time it waited for its limiter.</summary>
        public TimeSpan TimeWaitingForLimiter { get; internal set; }

        /// <summary>The time it waited for a part to be read.</summary>
        public TimeSpan TimeWithoutReadyPart { get; internal set; }

        /// <summary>The time it sent parts one at a time.</summary>
        public TimeSpan TimeSendingSerially { get; internal set; }

        /// <summary>The number of times parts grew.</summary>
        public int PartSizeGrows { get; internal set; }

        /// <summary>The number of larger sizes that showed their gain and were kept.</summary>
        public int PartSizeProbesAccepted { get; internal set; }

        /// <summary>The number of larger sizes given up.</summary>
        public int PartSizeProbesRejected { get; internal set; }

        /// <summary>The last part size decision, or null if there was none.</summary>
        public FilesPartSizeDecision? LastPartSizeDecision { get; internal set; }

        /// <summary>Whether the upload sent each part's offset. Always false: every part goes to its URL exactly as Files.com gave it.</summary>
        public bool SendsPartOffsets { get; internal set; }

        /// <summary>The process's CPU use, in percent of all processors, over the last sample, or null where not reported.</summary>
        public double? ProcessorPercent { get; internal set; }

        /// <summary>The physical memory the whole process used, in bytes, or null where not reported.</summary>
        public long? WorkingSetBytes { get; internal set; }

        /// <summary>The bytes the process's managed heap held, or null where not reported.</summary>
        public long? ManagedHeapBytes { get; internal set; }

        /// <summary>When the process was sampled, or null.</summary>
        public DateTimeOffset? ProcessSampledAt { get; internal set; }
    }
}
