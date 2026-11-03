using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FilesCom.Transfers;

namespace FilesCom.PowerShell.Engine
{
    // The expert tuning New-FilesAutomaticConcurrency and New-FilesAutomaticPartSizing make: the engine's own defaults,
    // with the settings given changed, checked by the rules the engine checks them by when an upload starts, so that a
    // setting it would refuse is refused here, with the module's parameter names, before any upload is attempted. The
    // engine checks them again when an upload starts, before it reads or sends anything.
    internal static class TuningBridge
    {
        internal static FilesAutomaticConcurrency NewAutomaticConcurrency(FilesUploadPerformanceProfile profile, IReadOnlyDictionary<string, object> given)
        {
            AutomaticConcurrencyOptions options = AutomaticConcurrencyOptions.For(TransferBridge.Profile(profile));
            foreach (KeyValuePair<string, object> setting in given)
            {
                switch (setting.Key)
                {
                    case nameof(options.StartTarget): options.StartTarget = (int)setting.Value; break;
                    case nameof(options.MinTarget): options.MinTarget = (int)setting.Value; break;
                    case nameof(options.SoftCeiling): options.SoftCeiling = (int)setting.Value; break;
                    case nameof(options.MaxTarget): options.MaxTarget = (int)setting.Value; break;
                    case nameof(options.SoftCeilingUnlockBytes): options.SoftCeilingUnlockBytes = (long)setting.Value; break;
                    case nameof(options.SoftCeilingUnlockBytesPerSecond): options.SoftCeilingUnlockBytesPerSecond = (double)setting.Value; break;
                    case nameof(options.WindowAttempts): options.WindowAttempts = (int)setting.Value; break;
                    case nameof(options.GrowthIntervalWindows): options.GrowthIntervalWindows = (int)setting.Value; break;
                    case nameof(options.GrowthStep): options.GrowthStep = (int)setting.Value; break;
                    case nameof(options.ProbeGainPercent): options.ProbeGainPercent = (double)setting.Value; break;
                    case nameof(options.PlateauTarget): options.PlateauTarget = (int)setting.Value; break;
                    case nameof(options.ProbeGainPerAddedPartPercent): options.ProbeGainPerAddedPartPercent = (double)setting.Value; break;
                    case nameof(options.ProbeWindows): options.ProbeWindows = (int)setting.Value; break;
                    case nameof(options.ShrinkPercent): options.ShrinkPercent = (double)setting.Value; break;
                    case nameof(options.HoldWindows): options.HoldWindows = (int)setting.Value; break;
                    case nameof(options.LatencyGrowthQueueLimit): options.LatencyGrowthQueueLimit = (double)setting.Value; break;
                    case nameof(options.LatencyBackoffQueueLimit): options.LatencyBackoffQueueLimit = (double)setting.Value; break;
                    case nameof(options.BackpressureShrinkPercent): options.BackpressureShrinkPercent = (double)setting.Value; break;
                    case nameof(options.BackpressurePause): options.BackpressurePause = (TimeSpan)setting.Value; break;
                    default: throw new ArgumentException($"Unknown concurrency setting {setting.Key}.", nameof(given));
                }
            }
            var check = new Check(given, profile.ToString());
            check.AtLeast(nameof(options.MinTarget), options.MinTarget, 1);
            check.AtMost(nameof(options.MaxTarget), options.MaxTarget, 1024);
            check.Ordered(nameof(options.MinTarget), options.MinTarget, nameof(options.StartTarget), options.StartTarget);
            check.Ordered(nameof(options.StartTarget), options.StartTarget, nameof(options.SoftCeiling), options.SoftCeiling);
            check.Ordered(nameof(options.SoftCeiling), options.SoftCeiling, nameof(options.MaxTarget), options.MaxTarget);
            check.Ordered(nameof(options.MinTarget), options.MinTarget, nameof(options.PlateauTarget), options.PlateauTarget);
            check.Ordered(nameof(options.PlateauTarget), options.PlateauTarget, nameof(options.MaxTarget), options.MaxTarget);
            check.AtLeast(nameof(options.SoftCeilingUnlockBytes), options.SoftCeilingUnlockBytes, 1);
            check.Finite(nameof(options.SoftCeilingUnlockBytesPerSecond), options.SoftCeilingUnlockBytesPerSecond, double.MaxValue);
            check.AtLeast(nameof(options.WindowAttempts), options.WindowAttempts, 1);
            check.AtLeast(nameof(options.GrowthIntervalWindows), options.GrowthIntervalWindows, 1);
            check.AtLeast(nameof(options.GrowthStep), options.GrowthStep, 1);
            check.AtLeast(nameof(options.ProbeWindows), options.ProbeWindows, 1);
            check.AtLeast(nameof(options.HoldWindows), options.HoldWindows, 0);
            check.Finite(nameof(options.ProbeGainPercent), options.ProbeGainPercent, double.MaxValue);
            check.Finite(nameof(options.ProbeGainPerAddedPartPercent), options.ProbeGainPerAddedPartPercent, double.MaxValue);
            check.Finite(nameof(options.ShrinkPercent), options.ShrinkPercent, 100);
            check.Finite(nameof(options.BackpressureShrinkPercent), options.BackpressureShrinkPercent, 100);
            check.Finite(nameof(options.LatencyGrowthQueueLimit), options.LatencyGrowthQueueLimit, double.MaxValue);
            check.Finite(nameof(options.LatencyBackoffQueueLimit), options.LatencyBackoffQueueLimit, double.MaxValue);
            if (options.LatencyGrowthQueueLimit > 0 && options.LatencyBackoffQueueLimit > 0 && options.LatencyBackoffQueueLimit < options.LatencyGrowthQueueLimit)
            {
                throw Refused($"{check.Describe(nameof(options.LatencyBackoffQueueLimit), options.LatencyBackoffQueueLimit)} cannot be below {check.Describe(nameof(options.LatencyGrowthQueueLimit), options.LatencyGrowthQueueLimit).TrimEnd(',')}: the upload would lower its number of parts at a latency that does not yet stop it growing.");
            }
            if (options.BackpressurePause < TimeSpan.Zero || options.BackpressurePause > TimeSpan.FromMinutes(1))
            {
                throw Refused($"{check.Describe(nameof(options.BackpressurePause), options.BackpressurePause.TotalSeconds)} must be from 0 to 60 seconds.");
            }
            return new FilesAutomaticConcurrency(
                profile,
                options.StartTarget,
                options.MinTarget,
                options.SoftCeiling,
                options.MaxTarget,
                options.SoftCeilingUnlockBytes,
                options.SoftCeilingUnlockBytesPerSecond,
                options.WindowAttempts,
                options.GrowthIntervalWindows,
                options.GrowthStep,
                options.ProbeGainPercent,
                options.PlateauTarget,
                options.ProbeGainPerAddedPartPercent,
                options.ProbeWindows,
                options.ShrinkPercent,
                options.HoldWindows,
                options.LatencyGrowthQueueLimit,
                options.LatencyBackoffQueueLimit,
                options.BackpressureShrinkPercent,
                options.BackpressurePause,
                Array.AsReadOnly(given.Keys.ToArray()));
        }

        internal static FilesAutomaticPartSizing NewAutomaticPartSizing(IReadOnlyDictionary<string, object> given)
        {
            var options = new AutomaticPartSizingOptions();
            IReadOnlyList<FilesPartSizeTier> tiers = Array.AsReadOnly(options.KnownSizeTiers.Select(tier => new FilesPartSizeTier(tier.MaxFileBytes, tier.PartSize)).ToArray());
            foreach (KeyValuePair<string, object> setting in given)
            {
                switch (setting.Key)
                {
                    case nameof(options.KnownSizeTiers): tiers = Array.AsReadOnly(((IReadOnlyList<FilesPartSizeTier>)setting.Value).ToArray()); break;
                    case nameof(options.WorkloadBytes): options.WorkloadBytes = (long)setting.Value; break;
                    case nameof(options.WorkloadParts): options.WorkloadParts = (int)setting.Value; break;
                    case nameof(options.UnknownLengthStartSize): options.UnknownLengthStartSize = (long)setting.Value; break;
                    case nameof(options.MinPartSize): options.MinPartSize = (long)setting.Value; break;
                    case nameof(options.MaxPartSize): options.MaxPartSize = (long)setting.Value; break;
                    case nameof(options.GrowthFactor): options.GrowthFactor = (double)setting.Value; break;
                    case nameof(options.GrowthIntervalWindows): options.GrowthIntervalWindows = (int)setting.Value; break;
                    case nameof(options.ProbeGainPercent): options.ProbeGainPercent = (double)setting.Value; break;
                    case nameof(options.ProbeWindows): options.ProbeWindows = (int)setting.Value; break;
                    case nameof(options.HoldWindows): options.HoldWindows = (int)setting.Value; break;
                    default: throw new ArgumentException($"Unknown part sizing setting {setting.Key}.", nameof(given));
                }
            }
            var check = new Check(given, null);
            if (tiers.Count == 0)
            {
                throw Refused("-KnownSizeTiers needs at least one tier, such as @{ 256MB = 8MB }.");
            }
            for (int i = 0; i < tiers.Count; i++)
            {
                if (tiers[i].PartSize < 1 || tiers[i].MaxFileBytes < 0)
                {
                    throw Refused($"-KnownSizeTiers tier {tiers[i]} needs a part size of at least 1 byte and a file size of at least 0.");
                }
                if (i > 0 && tiers[i].MaxFileBytes <= tiers[i - 1].MaxFileBytes)
                {
                    throw Refused($"-KnownSizeTiers has two tiers for files of up to {tiers[i].MaxFileBytes} bytes; give each file size once.");
                }
            }
            if (options.WorkloadBytes.HasValue)
            {
                check.AtLeast(nameof(options.WorkloadBytes), options.WorkloadBytes.Value, 0);
            }
            check.AtLeast(nameof(options.WorkloadParts), options.WorkloadParts, 1);
            check.AtLeast(nameof(options.UnknownLengthStartSize), options.UnknownLengthStartSize, 1);
            check.AtLeast(nameof(options.MinPartSize), options.MinPartSize, 0);
            check.AtLeast(nameof(options.MaxPartSize), options.MaxPartSize, 1);
            if (options.MaxPartSize < options.MinPartSize)
            {
                throw Refused($"{check.Describe(nameof(options.MinPartSize), options.MinPartSize)} cannot be above {check.Describe(nameof(options.MaxPartSize), options.MaxPartSize).TrimEnd(',')}.");
            }
            if (double.IsNaN(options.GrowthFactor) || options.GrowthFactor <= 1 || options.GrowthFactor > 16)
            {
                throw Refused($"{check.Describe(nameof(options.GrowthFactor), options.GrowthFactor)} must be more than 1 and at most 16.");
            }
            check.AtLeast(nameof(options.GrowthIntervalWindows), options.GrowthIntervalWindows, 1);
            check.Finite(nameof(options.ProbeGainPercent), options.ProbeGainPercent, double.MaxValue);
            check.AtLeast(nameof(options.ProbeWindows), options.ProbeWindows, 1);
            check.AtLeast(nameof(options.HoldWindows), options.HoldWindows, 0);
            return new FilesAutomaticPartSizing(
                tiers,
                options.WorkloadBytes,
                options.WorkloadParts,
                options.UnknownLengthStartSize,
                options.MinPartSize,
                options.MaxPartSize,
                options.GrowthFactor,
                options.GrowthIntervalWindows,
                options.ProbeGainPercent,
                options.ProbeWindows,
                options.HoldWindows,
                Array.AsReadOnly(given.Keys.ToArray()));
        }

        private static FilesException Refused(string message)
        {
            return new FilesException(FilesErrorKind.InvalidArgument, message);
        }

        // Names a setting as the caller knows it: the parameter and value given, or the default it came from.
        private sealed class Check
        {
            private readonly IReadOnlyDictionary<string, object> given;
            private readonly string profile;

            internal Check(IReadOnlyDictionary<string, object> given, string profile)
            {
                this.given = given;
                this.profile = profile;
            }

            internal string Describe(string setting, object value)
            {
                string text = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (given.ContainsKey(setting))
                {
                    return setting == nameof(AutomaticConcurrencyOptions.BackpressurePause) ? $"-BackpressurePauseSeconds {text}" : $"-{setting} {text}";
                }
                return profile == null ? $"{setting} {text}, the default," : $"{setting} {text}, the {profile} default,";
            }

            internal void AtLeast(string setting, long value, long minimum)
            {
                if (value < minimum)
                {
                    throw Refused($"{Describe(setting, value)} must be at least {minimum}.");
                }
            }

            internal void AtMost(string setting, long value, long maximum)
            {
                if (value > maximum)
                {
                    throw Refused($"{Describe(setting, value)} must be at most {maximum}.");
                }
            }

            internal void Ordered(string low, int lowValue, string high, int highValue)
            {
                if (lowValue > highValue)
                {
                    throw Refused($"{Describe(low, lowValue)} cannot be above {Describe(high, highValue).TrimEnd(',')}. Give values in the order MinTarget, StartTarget, SoftCeiling, MaxTarget, with PlateauTarget from MinTarget to MaxTarget.");
                }
            }

            internal void Finite(string setting, double value, double maximum)
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > maximum)
                {
                    throw Refused(maximum == double.MaxValue ? $"{Describe(setting, value)} must be a finite number, at least 0." : $"{Describe(setting, value)} must be from 0 to {maximum}.");
                }
            }
        }
    }
}
