using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Creates expert tuning for how uploads choose their part sizes where their parts go straight to Amazon S3, for
    /// Send-FilesFile -AutomaticPartSizing. Nothing is sent to Files.com.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "FilesAutomaticPartSizing")]
    [OutputType(typeof(FilesAutomaticPartSizing))]
    public sealed class NewFilesAutomaticPartSizingCommand : FilesCmdlet
    {
        /// <summary>
        /// The part size a file starts with, by the file's size: a hashtable of file sizes and the part sizes files of up
        /// to that size start with, such as @{ 256MB = 8MB; 4GB = 16MB }. Files larger than every tier use the last one.
        /// </summary>
        [Parameter]
        [FilesPartSizeTiers]
        [ValidateNotNull]
        public FilesPartSizeTier[] KnownSizeTiers { get; set; }

        /// <summary>
        /// The size, in bytes, of the whole workload the upload belongs to, such as a batch of files uploaded together.
        /// Defaults to each file's own size. Only the starting size uses it.
        /// </summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(0, long.MaxValue)]
        public long WorkloadBytes { get; set; }

        /// <summary>The fewest parts the workload should be divided into: a smaller workload gets smaller parts, so it can send several at once.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, int.MaxValue)]
        public int WorkloadParts { get; set; }

        /// <summary>The size, in bytes, the parts of a stream of unknown length start with.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long UnknownLengthStartSize { get; set; }

        /// <summary>The smallest size, in bytes, the upload chooses for a part other than the last. The engine's minimum applies too.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(0, long.MaxValue)]
        public long MinPartSize { get; set; }

        /// <summary>
        /// The largest size, in bytes, the upload chooses or grows parts to, at least -MinPartSize. The engine's maximum
        /// applies too, and parts can be larger when the file needs them to be.
        /// </summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, long.MaxValue)]
        public long MaxPartSize { get; set; }

        /// <summary>How many times larger each growth makes the parts not yet read: more than 1 and at most 16.</summary>
        [Parameter]
        [ValidateRange(0.0, 16.0)]
        public double GrowthFactor { get; set; }

        /// <summary>The number of measured windows at a size before parts grow.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, int.MaxValue)]
        public int GrowthIntervalWindows { get; set; }

        /// <summary>The throughput gain, in percent, larger parts must show to be kept.</summary>
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public double ProbeGainPercent { get; set; }

        /// <summary>The number of measured windows larger parts get to show their gain.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(1, int.MaxValue)]
        public int ProbeWindows { get; set; }

        /// <summary>The number of measured windows growth waits after larger parts were given up.</summary>
        [Parameter]
        [FilesInteger]
        [ValidateRange(0, int.MaxValue)]
        public int HoldWindows { get; set; }

        /// <summary>Writes the tuning.</summary>
        protected override void Execute()
        {
            var given = new Dictionary<string, object>();
            Add(given, nameof(KnownSizeTiers), KnownSizeTiers);
            Add(given, nameof(WorkloadBytes), WorkloadBytes);
            Add(given, nameof(WorkloadParts), WorkloadParts);
            Add(given, nameof(UnknownLengthStartSize), UnknownLengthStartSize);
            Add(given, nameof(MinPartSize), MinPartSize);
            Add(given, nameof(MaxPartSize), MaxPartSize);
            Add(given, nameof(GrowthFactor), GrowthFactor);
            Add(given, nameof(GrowthIntervalWindows), GrowthIntervalWindows);
            Add(given, nameof(ProbeGainPercent), ProbeGainPercent);
            Add(given, nameof(ProbeWindows), ProbeWindows);
            Add(given, nameof(HoldWindows), HoldWindows);
            WriteObject(TuningBridge.NewAutomaticPartSizing(given));
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
