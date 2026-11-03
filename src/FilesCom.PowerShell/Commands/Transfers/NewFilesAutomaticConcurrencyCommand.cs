using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Creates expert tuning for how automatic uploads choose the number of parts they send at once, for
    /// Send-FilesFile -AutomaticConcurrency. Nothing is sent to Files.com.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "FilesAutomaticConcurrency")]
    [OutputType(typeof(FilesAutomaticConcurrency))]
    public sealed class NewFilesAutomaticConcurrencyCommand : FilesCmdlet
    {
        /// <summary>
        /// The kind of destination whose defaults the settings not given take: Generic, S3 or Agent. An upload given this
        /// tuning uses it whatever the destination.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public FilesUploadPerformanceProfile Profile { get; set; }

        /// <summary>The number of parts at once an upload starts with.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, 1024)]
        public int StartTarget { get; set; }

        /// <summary>The fewest parts at once failures, throttling or latency can lower an upload to.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, 1024)]
        public int MinTarget { get; set; }

        /// <summary>
        /// The most parts at once growth reaches until the upload has delivered -SoftCeilingUnlockBytes and measured
        /// -SoftCeilingUnlockBytesPerSecond. Equal to -MaxTarget for no soft ceiling.
        /// </summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, 1024)]
        public int SoftCeiling { get; set; }

        /// <summary>The most parts at once an upload ever sends. Send-FilesFile -MaxConcurrency, when given, also limits it.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, 1024)]
        public int MaxTarget { get; set; }

        /// <summary>The bytes of accepted parts an upload must have delivered before it may grow past -SoftCeiling.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long SoftCeilingUnlockBytes { get; set; }

        /// <summary>The throughput, in bytes per second, a measured window must reach before an upload may grow past -SoftCeiling.</summary>
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public double SoftCeilingUnlockBytesPerSecond { get; set; }

        /// <summary>The fewest successful part requests in one measurement window.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, int.MaxValue)]
        public int WindowAttempts { get; set; }

        /// <summary>The number of measured windows at a number of parts at once before it grows.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, int.MaxValue)]
        public int GrowthIntervalWindows { get; set; }

        /// <summary>The number of parts at once each growth adds.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, 1024)]
        public int GrowthStep { get; set; }

        /// <summary>The throughput gain, in percent, a grown number of parts at once must show to be kept.</summary>
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public double ProbeGainPercent { get; set; }

        /// <summary>The number of parts at once above which each added part must earn -ProbeGainPerAddedPartPercent.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, 1024)]
        public int PlateauTarget { get; set; }

        /// <summary>The throughput gain, in percent, each part at once added above -PlateauTarget must earn.</summary>
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public double ProbeGainPerAddedPartPercent { get; set; }

        /// <summary>The number of measured windows a grown number of parts at once gets to show its gain.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, int.MaxValue)]
        public int ProbeWindows { get; set; }

        /// <summary>How much, in percent, a temporary failure or rising latency lowers the number of parts at once.</summary>
        [Parameter]
        [ValidateRange(0.0, 100.0)]
        public double ShrinkPercent { get; set; }

        /// <summary>The number of measured windows growth waits after the number was lowered or a growth given up.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(0, int.MaxValue)]
        public int HoldWindows { get; set; }

        /// <summary>The estimated number of part requests queued, rather than moving data, above which growth waits. 0 turns this off.</summary>
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public double LatencyGrowthQueueLimit { get; set; }

        /// <summary>
        /// The estimated number of part requests queued above which, with falling throughput, the number of parts at once
        /// is lowered by -ShrinkPercent. At least -LatencyGrowthQueueLimit; 0 turns this off.
        /// </summary>
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public double LatencyBackoffQueueLimit { get; set; }

        /// <summary>How much, in percent, a throttling answer lowers the number of parts at once.</summary>
        [Parameter]
        [ValidateRange(0.0, 100.0)]
        public double BackpressureShrinkPercent { get; set; }

        /// <summary>
        /// The shortest time, in seconds from 0 to 60, a throttling answer stops the upload from starting part requests,
        /// including retries. A longer Retry-After, up to a minute, is honored instead.
        /// </summary>
        [Parameter]
        [ValidateRange(0.0, 60.0)]
        public double BackpressurePauseSeconds { get; set; }

        /// <summary>Writes the tuning.</summary>
        protected override void Execute()
        {
            var given = new Dictionary<string, object>();
            Add(given, nameof(StartTarget), StartTarget);
            Add(given, nameof(MinTarget), MinTarget);
            Add(given, nameof(SoftCeiling), SoftCeiling);
            Add(given, nameof(MaxTarget), MaxTarget);
            Add(given, nameof(SoftCeilingUnlockBytes), SoftCeilingUnlockBytes);
            Add(given, nameof(SoftCeilingUnlockBytesPerSecond), SoftCeilingUnlockBytesPerSecond);
            Add(given, nameof(WindowAttempts), WindowAttempts);
            Add(given, nameof(GrowthIntervalWindows), GrowthIntervalWindows);
            Add(given, nameof(GrowthStep), GrowthStep);
            Add(given, nameof(ProbeGainPercent), ProbeGainPercent);
            Add(given, nameof(PlateauTarget), PlateauTarget);
            Add(given, nameof(ProbeGainPerAddedPartPercent), ProbeGainPerAddedPartPercent);
            Add(given, nameof(ProbeWindows), ProbeWindows);
            Add(given, nameof(ShrinkPercent), ShrinkPercent);
            Add(given, nameof(HoldWindows), HoldWindows);
            Add(given, nameof(LatencyGrowthQueueLimit), LatencyGrowthQueueLimit);
            Add(given, nameof(LatencyBackoffQueueLimit), LatencyBackoffQueueLimit);
            Add(given, nameof(BackpressureShrinkPercent), BackpressureShrinkPercent);
            if (IsBound(nameof(BackpressurePauseSeconds)))
            {
                given["BackpressurePause"] = TimeSpan.FromTicks((long)Math.Round(BackpressurePauseSeconds * TimeSpan.TicksPerSecond));
            }
            WriteObject(TuningBridge.NewAutomaticConcurrency(Profile, given));
        }

        private void Add(Dictionary<string, object> given, string parameter, object value)
        {
            if (IsBound(parameter))
            {
                given[parameter] = value;
            }
        }
    }
}
