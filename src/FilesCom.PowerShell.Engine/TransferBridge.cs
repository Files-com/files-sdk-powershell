using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FilesCom.Transfers;

namespace FilesCom.PowerShell.Engine
{
    // Uploads and downloads with the SDK's transfer engine. Parts, retries, the shared limiter and the one final request
    // are the SDK's; this bridge only passes the caller's choices in and the results out.
    internal static class TransferBridge
    {
        // A limiter with the given budgets, or the engine's defaults for those not given.
        internal static FilesTransferLimiter NewLimiter(int? maxConcurrentRequests, long? maxBufferedBytes)
        {
            var limiter = new TransferLimiter(
                maxConcurrentRequests ?? TransferLimiter.DefaultMaxConcurrentRequests,
                maxBufferedBytes ?? TransferLimiter.DefaultMaxBufferedBytes);
            return new FilesTransferLimiter(limiter.MaxConcurrentRequests, limiter.MaxBufferedBytes, limiter);
        }

        // Uploads a local file, which the SDK opens once and reads every part through.
        internal static Task<FilesUploadResult> UploadFileAsync(FilesConnection connection, string localPath, string remotePath, FilesUploadSettings settings, IProgress<FilesTransferProgress> progress, IProgress<FilesUploadTrace> trace, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(
                connection,
                options,
                cancellationToken,
                (client, requestOptions, token) => WithinLimits(client.Transfers.UploadFileAsync(localPath, remotePath, UploadOptions(settings, progress, trace, requestOptions), token)),
                result => Project(result, connection, options));
        }

        // Uploads length bytes of a borrowed stream, which stays open, or with a null length everything it gives until
        // it ends.
        internal static Task<FilesUploadResult> UploadStreamAsync(FilesConnection connection, Stream source, long? length, string remotePath, FilesUploadSettings settings, IProgress<FilesTransferProgress> progress, IProgress<FilesUploadTrace> trace, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(
                connection,
                options,
                cancellationToken,
                (client, requestOptions, token) => WithinLimits(length.HasValue
                    ? client.Transfers.UploadStreamAsync(source, length.Value, remotePath, UploadOptions(settings, progress, trace, requestOptions), token)
                    : client.Transfers.UploadStreamAsync(source, remotePath, UploadOptions(settings, progress, trace, requestOptions), token)),
                result => Project(result, connection, options));
        }

        // Downloads into a borrowed stream, which stays open, from its current position, with the engine's transfer
        // download: bound to one version of the file where its storage allows, in checked parts written in order, or in
        // one request. The stream holds only whole parts after a failure; a caller that publishes a file does so only
        // after this completes. The web app's page and a version that changed are refused before bytes are written.
        internal static Task<FilesDownloadResult> DownloadAsync(FilesConnection connection, string remotePath, Stream destination, string localPath, int? maxConcurrency, long? partSize, FilesTransferLimiter limiter, IProgress<FilesTransferProgress> progress, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(
                connection,
                options,
                cancellationToken,
                (client, requestOptions, token) => client.Transfers.DownloadStreamAsync(remotePath, destination, new DownloadOptions
                {
                    MaxConcurrency = maxConcurrency,
                    PartSize = partSize,
                    Limiter = (TransferLimiter)limiter?.EngineLimiter,
                    Progress = progress == null ? null : new DownloadProgressRelay(progress),
                    RequestOptions = requestOptions,
                }, token),
                result => new FilesDownloadResult(remotePath, localPath, result.Size, result.VersionBound, result.Parts, result.ReceivedBytes, result.ResumedBytes, result.Elapsed, false));
        }

        // Downloads to a local file with the engine's own publication: the file is written whole into the engine's private
        // stage beside it and put in place only once complete, where nothing is at its name, or with -Force replacing only
        // a regular file there. With a checkpoint, a download that is stopped or fails keeps what it fetched, and running it
        // again with the same checkpoint continues it once its storage confirms the version is still the file's.
        internal static Task<FilesDownloadResult> DownloadFileAsync(FilesConnection connection, string remotePath, string localPath, FilesDownloadSettings settings, IProgress<FilesTransferProgress> progress, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(
                connection,
                options,
                cancellationToken,
                (client, requestOptions, token) => Publishing(client.Transfers.DownloadFileAsync(remotePath, localPath, new DownloadOptions
                {
                    MaxConcurrency = settings.MaxConcurrency,
                    PartSize = settings.PartSize,
                    Limiter = (TransferLimiter)settings.Limiter?.EngineLimiter,
                    Progress = progress == null ? null : new DownloadProgressRelay(progress),
                    CheckpointPath = settings.CheckpointPath,
                    PublicationPolicy = Policy(settings.ReplaceRegularFile),
                    CheckpointRestartPolicy = Restart(settings.CheckpointRestart),
                    RequestOptions = requestOptions,
                }, token)),
                result => new FilesDownloadResult(remotePath, result.LocalPath ?? localPath, result.Size, result.VersionBound, result.Parts, result.ReceivedBytes, result.ResumedBytes, result.Elapsed, result.CompletedEarlier));
        }

        // Downloads a remote folder and everything in it into a local directory with the engine's directory download: files
        // at most maxConcurrentFiles at once, sharing the limiter, each put in place on its own as a file download is. With
        // a checkpoint folder, running it again skips the files it completed, once their storage confirms their versions, and
        // continues the rest.
        internal static Task<FilesDirectoryTransferResult> DownloadDirectoryAsync(FilesConnection connection, string remoteDirectory, string localDirectory, FilesDirectoryDownloadSettings settings, IProgress<FilesTransferProgress> progress, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(
                connection,
                options,
                cancellationToken,
                (client, requestOptions, token) =>
                {
                    var directoryOptions = new DirectoryDownloadOptions
                    {
                        Limiter = (TransferLimiter)settings.Limiter?.EngineLimiter,
                        CheckpointDirectory = settings.CheckpointDirectory,
                        Progress = progress == null ? null : new DirectoryProgressRelay(progress),
                        FileOptions = new DownloadOptions
                        {
                            MaxConcurrency = settings.MaxConcurrency,
                            PartSize = settings.PartSize,
                            PublicationPolicy = Policy(settings.ReplaceRegularFile),
                            CheckpointRestartPolicy = Restart(settings.CheckpointRestart),
                            RequestOptions = requestOptions,
                        },
                    };
                    if (settings.MaxConcurrentFiles.HasValue)
                    {
                        directoryOptions.MaxConcurrentFiles = settings.MaxConcurrentFiles.Value;
                    }
                    return Publishing(client.Transfers.DownloadDirectoryAsync(remoteDirectory, localDirectory, directoryOptions, token), localDirectory);
                },
                result => Project(result));
        }

        private static bool SamePath(string one, string other)
        {
            return one != null && string.Equals(
                Path.GetFullPath(one).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(other).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.Ordinal);
        }

        // The module never uses the engine's default publication, which would replace any file, or a link, at the name.
        private static DownloadPublicationPolicy Policy(bool replaceRegularFile)
        {
            return replaceRegularFile ? DownloadPublicationPolicy.ReplaceRegularFile : DownloadPublicationPolicy.CreateNew;
        }

        private static CheckpointRestartPolicy Restart(FilesCheckpointRestart? restart)
        {
            return restart == FilesCheckpointRestart.StartOver ? CheckpointRestartPolicy.StartOver : CheckpointRestartPolicy.Fail;
        }

        // A file the engine did not put in place as the module asked, or put in place without certainty, becomes the
        // module's error with the engine's typed reason; the engine reports it as an IOException, not an SDK exception.
        // In a directory download the engine's "nothing was downloaded" about one file is about that file alone: the error
        // says so. A refusal about the local directory itself, such as one that moved while the remote folder was listed,
        // concerns no file.
        private static async Task<T> Publishing<T>(Task<T> download, string localDirectory = null)
        {
            try
            {
                return await download.ConfigureAwait(false);
            }
            catch (DownloadPublicationException refused)
            {
                throw ErrorTranslation.Publication(refused, localDirectory != null && !SamePath(refused.LocalPath, localDirectory));
            }
        }

        // Passes a download's progress on as the module's: the bytes written in order, against the file's size once known.
        private sealed class DownloadProgressRelay : IProgress<DownloadProgress>
        {
            private readonly IProgress<FilesTransferProgress> receiver;

            internal DownloadProgressRelay(IProgress<FilesTransferProgress> receiver)
            {
                this.receiver = receiver;
            }

            public void Report(DownloadProgress value)
            {
                receiver.Report(new FilesTransferProgress(value.Path, value.TotalBytes, value.TotalBytesKnown, false, 0, value.WrittenBytes, value.WrittenBytes, value.Retries, value.Elapsed, value.ActiveParts, 0, TimeSpan.Zero));
            }
        }

        // Uploads a local directory and everything in it into a remote folder with the engine's directory upload: files
        // at most maxConcurrentFiles at once, sharing the limiter, each as a file upload, started aggressively when
        // aggressiveStartup says so and otherwise as the engine starts by default; with a checkpoint directory, running it
        // again continues it.
        internal static Task<FilesDirectoryTransferResult> UploadDirectoryAsync(FilesConnection connection, string localDirectory, string remoteDirectory, int? maxConcurrentFiles, bool aggressiveStartup, FilesTransferLimiter limiter, string checkpointDirectory, IProgress<FilesTransferProgress> progress, FilesCallOptions options, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(
                connection,
                options,
                cancellationToken,
                (client, requestOptions, token) =>
                {
                    var directoryOptions = new DirectoryUploadOptions
                    {
                        Limiter = (TransferLimiter)limiter?.EngineLimiter,
                        CheckpointDirectory = checkpointDirectory,
                        Progress = progress == null ? null : new DirectoryProgressRelay(progress),
                        FileOptions = new UploadOptions { RequestOptions = requestOptions, Startup = Startup(aggressiveStartup) },
                    };
                    if (maxConcurrentFiles.HasValue)
                    {
                        directoryOptions.MaxConcurrentFiles = maxConcurrentFiles.Value;
                    }
                    return client.Transfers.UploadDirectoryAsync(localDirectory, remoteDirectory, directoryOptions, token);
                },
                result => Project(result));
        }

        private static FilesDirectoryTransferResult Project(DirectoryTransferResult result)
        {
            return new FilesDirectoryTransferResult(
                result.LocalDirectory,
                result.RemoteDirectory,
                result.Files.Select(file => new FilesDirectoryTransferEntry(file.RelativePath, file.LocalPath, file.RemotePath, file.Size, file.CompletedEarlier)).ToList().AsReadOnly(),
                result.Directories.ToList().AsReadOnly(),
                result.Excluded.ToList().AsReadOnly(),
                result.Bytes,
                result.Elapsed);
        }

        // Passes a directory transfer's progress on as the module's: bytes done of the whole transfer.
        private sealed class DirectoryProgressRelay : IProgress<DirectoryTransferProgress>
        {
            private readonly IProgress<FilesTransferProgress> receiver;

            internal DirectoryProgressRelay(IProgress<FilesTransferProgress> receiver)
            {
                this.receiver = receiver;
            }

            public void Report(DirectoryTransferProgress value)
            {
                receiver.Report(new FilesTransferProgress(value.RemoteDirectory, value.BytesTotal, true, false, 0, value.BytesDone, value.BytesDone, 0, value.Elapsed, value.FilesActive, 0, TimeSpan.Zero)
                {
                    FilesDone = value.FilesDone,
                    FilesTotal = value.FilesTotal,
                });
            }
        }

        internal static FilesUploadedPart[] Parts(IReadOnlyList<UploadedPart> parts)
        {
            return parts.Select(part => new FilesUploadedPart(part.Number, part.Offset, part.Length, part.ETag, part.Attempts)).ToArray();
        }

        // The engine's settings for the caller's choices. The engine keeps a stream's parts and its spool file, in memory
        // or in its own temporary files, and deletes them however the upload ends; the module holds none of them.
        private static UploadOptions UploadOptions(FilesUploadSettings settings, IProgress<FilesTransferProgress> progress, IProgress<FilesUploadTrace> trace, Dictionary<string, object> requestOptions)
        {
            var options = new UploadOptions
            {
                Limiter = (TransferLimiter)settings.Limiter?.EngineLimiter,
                MkdirParents = settings.MkdirParents,
                ModifiedTime = settings.ModifiedTime,
                Progress = progress == null ? null : new ProgressRelay(progress, settings.Spool),
                Trace = trace == null ? null : new TraceRelay(trace),
                RequestOptions = requestOptions,
                PartSize = settings.PartSize,
                MaxStreamLength = settings.MaxLength,
                SpoolMaxBytes = settings.Spool ? settings.MaxLength : null,
                TemporaryDirectory = settings.TemporaryDirectory,
                CheckpointPath = settings.CheckpointPath,
            };
            // Unset, the engine's own defaults apply: automatic concurrency, within the profile's limits only, with the
            // profile of the destination the part URLs show; automatic part sizes with the engine's tuning; stream parts
            // kept in memory; the engine's read-ahead budget.
            if (settings.ConcurrencyMode.HasValue)
            {
                options.ConcurrencyMode = settings.ConcurrencyMode.Value == FilesUploadConcurrencyMode.Static ? UploadConcurrencyMode.Static : UploadConcurrencyMode.Automatic;
            }
            options.MaxConcurrency = settings.MaxConcurrency;
            if (settings.AutomaticConcurrency != null)
            {
                options.AutomaticConcurrency = Engine(settings.AutomaticConcurrency);
            }
            else if (settings.PerformanceProfile.HasValue)
            {
                options.AutomaticConcurrency = AutomaticConcurrencyOptions.For(Profile(settings.PerformanceProfile.Value));
            }
            options.Startup = Startup(settings.AggressiveStartup);
            if (settings.PartSizingMode.HasValue)
            {
                options.PartSizingMode = SizingMode(settings.PartSizingMode.Value);
            }
            if (settings.AutomaticPartSizing != null)
            {
                options.AutomaticPartSizing = Engine(settings.AutomaticPartSizing);
            }
            if (settings.PartStorage == FilesStreamPartStorage.TemporaryFiles)
            {
                options.StreamPartStorage = StreamPartStorage.TemporaryFiles;
            }
            if (settings.CheckpointRestart.HasValue)
            {
                options.CheckpointRestartPolicy = settings.CheckpointRestart.Value == FilesCheckpointRestart.StartOver ? CheckpointRestartPolicy.StartOver : CheckpointRestartPolicy.Fail;
            }
            if (settings.MaxReadyParts.HasValue)
            {
                options.MaxReadyParts = settings.MaxReadyParts.Value;
            }
            if (settings.MaxReadyBytes.HasValue)
            {
                options.MaxReadyBytes = settings.MaxReadyBytes.Value;
            }
            return options;
        }

        // The engine's tuning for the module's: the profile's defaults with every effective value set, which is the same
        // as the defaults with the settings given changed.
        private static AutomaticConcurrencyOptions Engine(FilesAutomaticConcurrency tuning)
        {
            AutomaticConcurrencyOptions options = AutomaticConcurrencyOptions.For(Profile(tuning.Profile));
            options.StartTarget = tuning.StartTarget;
            options.MinTarget = tuning.MinTarget;
            options.SoftCeiling = tuning.SoftCeiling;
            options.MaxTarget = tuning.MaxTarget;
            options.SoftCeilingUnlockBytes = tuning.SoftCeilingUnlockBytes;
            options.SoftCeilingUnlockBytesPerSecond = tuning.SoftCeilingUnlockBytesPerSecond;
            options.WindowAttempts = tuning.WindowAttempts;
            options.GrowthIntervalWindows = tuning.GrowthIntervalWindows;
            options.GrowthStep = tuning.GrowthStep;
            options.ProbeGainPercent = tuning.ProbeGainPercent;
            options.PlateauTarget = tuning.PlateauTarget;
            options.ProbeGainPerAddedPartPercent = tuning.ProbeGainPerAddedPartPercent;
            options.ProbeWindows = tuning.ProbeWindows;
            options.ShrinkPercent = tuning.ShrinkPercent;
            options.HoldWindows = tuning.HoldWindows;
            options.LatencyGrowthQueueLimit = tuning.LatencyGrowthQueueLimit;
            options.LatencyBackoffQueueLimit = tuning.LatencyBackoffQueueLimit;
            options.BackpressureShrinkPercent = tuning.BackpressureShrinkPercent;
            options.BackpressurePause = tuning.BackpressurePause;
            return options;
        }

        private static AutomaticPartSizingOptions Engine(FilesAutomaticPartSizing tuning)
        {
            return new AutomaticPartSizingOptions
            {
                KnownSizeTiers = tuning.KnownSizeTiers.Select(tier => new PartSizeTier(tier.MaxFileBytes, tier.PartSize)).ToList(),
                WorkloadBytes = tuning.WorkloadBytes,
                WorkloadParts = tuning.WorkloadParts,
                UnknownLengthStartSize = tuning.UnknownLengthStartSize,
                MinPartSize = tuning.MinPartSize,
                MaxPartSize = tuning.MaxPartSize,
                GrowthFactor = tuning.GrowthFactor,
                GrowthIntervalWindows = tuning.GrowthIntervalWindows,
                ProbeGainPercent = tuning.ProbeGainPercent,
                ProbeWindows = tuning.ProbeWindows,
                HoldWindows = tuning.HoldWindows,
            };
        }

        // An upload that does not fit a limit, which the engine alone reports as UploadLimitException, is LimitExceeded,
        // with the limit and with the module's parameters in place of the engine's names for its settings. Everything
        // else goes on as it was thrown: a failure of the caller's stream, whatever its type, and any other failure of the
        // engine, which the command reports by its own kind.
        private static async Task<UploadResult> WithinLimits(Task<UploadResult> upload)
        {
            try
            {
                return await upload.ConfigureAwait(false);
            }
            catch (UploadLimitException failure)
            {
                throw new FilesException(FilesErrorKind.LimitExceeded, InModuleTerms(failure.Message)) { UploadLimit = Limit(failure.Limit) };
            }
        }

        // Null for a limit this module does not know, whose message still says what it is.
        private static FilesUploadLimit? Limit(UploadLimit limit)
        {
            switch (limit)
            {
                case UploadLimit.StreamLength:
                    return FilesUploadLimit.StreamLength;
                case UploadLimit.PartCount:
                    return FilesUploadLimit.PartCount;
                case UploadLimit.BufferSpace:
                    return FilesUploadLimit.BufferSpace;
                case UploadLimit.MemoryPart:
                    return FilesUploadLimit.MemoryPart;
                case UploadLimit.SpoolLength:
                    return FilesUploadLimit.SpoolLength;
                case UploadLimit.FixedPartSize:
                    return FilesUploadLimit.FixedPartSize;
                default:
                    return null;
            }
        }

        private static string InModuleTerms(string message)
        {
            return message
                .Replace("UploadOptions.MaxStreamLength", "-MaxLength")
                .Replace("keep parts in StreamPartStorage.TemporaryFiles", "keep parts in temporary files with -PartStorage TemporaryFiles")
                .Replace("TransferLimiter.MaxBufferedBytes", "New-FilesTransferLimiter -MaxBufferedBytes");
        }

        private static UploadPartSizingMode SizingMode(FilesUploadPartSizingMode mode)
        {
            switch (mode)
            {
                case FilesUploadPartSizingMode.Fixed:
                    return UploadPartSizingMode.Fixed;
                case FilesUploadPartSizingMode.ServerPlanned:
                    return UploadPartSizingMode.ServerPlanned;
                default:
                    return UploadPartSizingMode.Automatic;
            }
        }

        // The engine's startup: Aggressive with -AggressiveStartup, and otherwise its default, which probes the link first.
        private static UploadStartup Startup(bool aggressive)
        {
            return aggressive ? UploadStartup.Aggressive : UploadStartup.Default;
        }

        internal static UploadPerformanceProfile Profile(FilesUploadPerformanceProfile profile)
        {
            switch (profile)
            {
                case FilesUploadPerformanceProfile.S3:
                    return UploadPerformanceProfile.S3;
                case FilesUploadPerformanceProfile.Agent:
                    return UploadPerformanceProfile.Agent;
                default:
                    return UploadPerformanceProfile.Generic;
            }
        }

        private static FilesUploadResult Project(UploadResult result, FilesConnection connection, FilesCallOptions options)
        {
            UploadConcurrency concurrency = result.FinalProgress?.Concurrency;
            UploadPartSizing sizing = result.FinalProgress?.PartSizing;
            PartSizeLimits limits = sizing?.Limits;
            return new FilesUploadResult(
                result.Path,
                result.Size,
                RemoteFileBridge.Project(result.File, connection, options),
                Parts(result.Parts),
                result.ParallelParts,
                result.SentBytes,
                result.Elapsed,
                concurrency?.Mode == UploadConcurrencyMode.Static ? FilesUploadConcurrencyMode.Static : FilesUploadConcurrencyMode.Automatic,
                ModuleProfile(concurrency?.Profile ?? UploadPerformanceProfile.Generic),
                concurrency?.PeakActiveAttempts ?? 0,
                SerialReason(concurrency?.SerialReason ?? UploadSerialReason.None),
                result.FinalProgress?.PartRetries ?? 0,
                ModuleSizingMode(sizing?.Mode ?? UploadPartSizingMode.Automatic),
                SizeSource(sizing?.Source ?? PartSizeSource.ServerPlanned),
                PlannedReason(sizing?.ServerPlannedReason ?? ServerPlannedReason.None),
                Constraint(sizing?.StartConstraint ?? PartSizeConstraint.None),
                limits == null ? null : new FilesPartSizeLimits(limits.MinNonfinalBytes, limits.MaxPartBytes, limits.MaxParts, limits.MaxFileBytes),
                sizing?.LargestPartSize ?? 0,
                concurrency?.ProfileSource == UploadProfileSource.Options ? FilesUploadProfileSource.Options : FilesUploadProfileSource.Server,
                Metrics(result))
            {
                ResumedParts = result.ResumedParts,
                ResumedBytes = result.ResumedBytes,
            };
        }

        // An explicit selection of the final report's measurements: numbers, times and decisions only.
        private static FilesUploadMetrics Metrics(UploadResult result)
        {
            TransferProgress final = result.FinalProgress;
            UploadConcurrency concurrency = final?.Concurrency;
            UploadPartSizing sizing = final?.PartSizing;
            ProcessResourceSample process = final?.Process;
            return new FilesUploadMetrics
            {
                ThroughputBytesPerSecond = result.Elapsed > TimeSpan.Zero ? result.Size / result.Elapsed.TotalSeconds : 0,
                PartsAcknowledged = final?.PartsAcknowledged ?? result.Parts.Count,
                AdmissionWait = final?.AdmissionWait ?? TimeSpan.Zero,
                RequestWait = final?.RequestWait ?? TimeSpan.Zero,
                BufferWait = final?.BufferWait ?? TimeSpan.Zero,
                ReadyLimitWaits = final?.ReadyLimitWaits ?? 0,
                Target = concurrency?.Target ?? 0,
                MinTarget = concurrency?.MinTarget ?? 0,
                SoftCeiling = concurrency?.SoftCeiling ?? 0,
                MaxTarget = concurrency?.MaxTarget ?? 0,
                SoftCeilingUnlocked = concurrency?.SoftCeilingUnlocked ?? false,
                Grows = concurrency?.Grows ?? 0,
                Shrinks = concurrency?.Shrinks ?? 0,
                ProbesAccepted = concurrency?.ProbesAccepted ?? 0,
                ProbesRejected = concurrency?.ProbesRejected ?? 0,
                MeasuredWindows = concurrency?.MeasuredWindows ?? 0,
                UnmeasuredWindows = concurrency?.UnmeasuredWindows ?? 0,
                Throttles = concurrency?.Throttles ?? 0,
                TransientFailures = concurrency?.TransientFailures ?? 0,
                LastDecision = concurrency?.LastDecision == null ? (FilesConcurrencyDecision?)null : Decision(concurrency.LastDecision.Value),
                LastThroughputBytesPerSecond = concurrency?.LastThroughputBytesPerSecond ?? 0,
                BestThroughputBytesPerSecond = concurrency?.BestThroughputBytesPerSecond ?? 0,
                QueueEstimate = concurrency?.QueueEstimate ?? 0,
                TimeAtTarget = concurrency?.TimeAtTarget ?? TimeSpan.Zero,
                TimeWaitingForLimiter = concurrency?.TimeWaitingForLimiter ?? TimeSpan.Zero,
                TimeWithoutReadyPart = concurrency?.TimeWithoutReadyPart ?? TimeSpan.Zero,
                TimeSendingSerially = concurrency?.TimeSendingSerially ?? TimeSpan.Zero,
                PartSizeGrows = sizing?.Grows ?? 0,
                PartSizeProbesAccepted = sizing?.ProbesAccepted ?? 0,
                PartSizeProbesRejected = sizing?.ProbesRejected ?? 0,
                LastPartSizeDecision = sizing?.LastDecision == null ? (FilesPartSizeDecision?)null : SizeDecision(sizing.LastDecision.Value),
                SendsPartOffsets = sizing?.SendsPartOffsets ?? false,
                ProcessorPercent = process?.ProcessorPercent,
                WorkingSetBytes = process?.WorkingSetBytes,
                ManagedHeapBytes = process?.ManagedHeapBytes,
                ProcessSampledAt = process?.SampledAt,
            };
        }

        private static FilesConcurrencyDecision Decision(ConcurrencyDecision decision)
        {
            switch (decision)
            {
                case ConcurrencyDecision.Grew: return FilesConcurrencyDecision.Grew;
                case ConcurrencyDecision.ProbeAccepted: return FilesConcurrencyDecision.ProbeAccepted;
                case ConcurrencyDecision.ProbeRejected: return FilesConcurrencyDecision.ProbeRejected;
                case ConcurrencyDecision.ShrankAfterFailure: return FilesConcurrencyDecision.ShrankAfterFailure;
                case ConcurrencyDecision.ShrankAfterThrottling: return FilesConcurrencyDecision.ShrankAfterThrottling;
                case ConcurrencyDecision.ShrankForLatency: return FilesConcurrencyDecision.ShrankForLatency;
                case ConcurrencyDecision.CooldownStarted: return FilesConcurrencyDecision.CooldownStarted;
                case ConcurrencyDecision.GrowthHeldForLatency: return FilesConcurrencyDecision.GrowthHeldForLatency;
                case ConcurrencyDecision.SoftCeilingUnlocked: return FilesConcurrencyDecision.SoftCeilingUnlocked;
                case ConcurrencyDecision.WindowNotMeasured: return FilesConcurrencyDecision.WindowNotMeasured;
            }
            // The warm start's decisions, by name: the package engine.json pins for local checks has no such members.
            switch (decision.ToString())
            {
                case "WarmStartRaised": return FilesConcurrencyDecision.WarmStartRaised;
                case "WarmStartFastLink": return FilesConcurrencyDecision.WarmStartFastLink;
                case "WarmStartSettled": return FilesConcurrencyDecision.WarmStartSettled;
                default: return FilesConcurrencyDecision.Other;
            }
        }

        private static FilesPartSizeDecision SizeDecision(PartSizeDecision decision)
        {
            switch (decision)
            {
                case PartSizeDecision.Chosen: return FilesPartSizeDecision.Chosen;
                case PartSizeDecision.Grew: return FilesPartSizeDecision.Grew;
                case PartSizeDecision.ProbeAccepted: return FilesPartSizeDecision.ProbeAccepted;
                case PartSizeDecision.ProbeRejected: return FilesPartSizeDecision.ProbeRejected;
                default: return FilesPartSizeDecision.Other;
            }
        }

        private static FilesBelowTargetReason BelowTarget(BelowTargetReason reason)
        {
            switch (reason)
            {
                case BelowTargetReason.SharedLimiter: return FilesBelowTargetReason.SharedLimiter;
                case BelowTargetReason.NoPartReady: return FilesBelowTargetReason.NoPartReady;
                case BelowTargetReason.SerialParts: return FilesBelowTargetReason.SerialParts;
                default: return FilesBelowTargetReason.None;
            }
        }

        private static FilesUploadPartSizingMode ModuleSizingMode(UploadPartSizingMode mode)
        {
            switch (mode)
            {
                case UploadPartSizingMode.Fixed:
                    return FilesUploadPartSizingMode.Fixed;
                case UploadPartSizingMode.ServerPlanned:
                    return FilesUploadPartSizingMode.ServerPlanned;
                default:
                    return FilesUploadPartSizingMode.Automatic;
            }
        }

        private static FilesPartSizeSource SizeSource(PartSizeSource source)
        {
            switch (source)
            {
                case PartSizeSource.Automatic:
                    return FilesPartSizeSource.Automatic;
                case PartSizeSource.Fixed:
                    return FilesPartSizeSource.Fixed;
                default:
                    return FilesPartSizeSource.ServerPlanned;
            }
        }

        private static FilesServerPlannedReason PlannedReason(ServerPlannedReason reason)
        {
            switch (reason)
            {
                case ServerPlannedReason.Requested:
                    return FilesServerPlannedReason.Requested;
                case ServerPlannedReason.NotAllowed:
                    return FilesServerPlannedReason.NotAllowed;
                case ServerPlannedReason.LengthUnknown:
                    return FilesServerPlannedReason.LengthUnknown;
                case ServerPlannedReason.MemoryLimit:
                    return FilesServerPlannedReason.MemoryLimit;
                default:
                    return FilesServerPlannedReason.None;
            }
        }

        private static FilesPartSizeConstraint Constraint(PartSizeConstraint constraint)
        {
            switch (constraint)
            {
                case PartSizeConstraint.MinPartSize:
                    return FilesPartSizeConstraint.MinPartSize;
                case PartSizeConstraint.MaxPartSize:
                    return FilesPartSizeConstraint.MaxPartSize;
                case PartSizeConstraint.ServerMinimum:
                    return FilesPartSizeConstraint.ServerMinimum;
                case PartSizeConstraint.ServerMaximum:
                    return FilesPartSizeConstraint.ServerMaximum;
                case PartSizeConstraint.PartLimit:
                    return FilesPartSizeConstraint.PartLimit;
                case PartSizeConstraint.BufferLimit:
                    return FilesPartSizeConstraint.BufferLimit;
                default:
                    return FilesPartSizeConstraint.None;
            }
        }

        private static FilesUploadPerformanceProfile ModuleProfile(UploadPerformanceProfile profile)
        {
            switch (profile)
            {
                case UploadPerformanceProfile.S3:
                    return FilesUploadPerformanceProfile.S3;
                case UploadPerformanceProfile.Agent:
                    return FilesUploadPerformanceProfile.Agent;
                default:
                    return FilesUploadPerformanceProfile.Generic;
            }
        }

        private static FilesUploadSerialReason SerialReason(UploadSerialReason reason)
        {
            switch (reason)
            {
                case UploadSerialReason.ServerRequiresSerialParts:
                    return FilesUploadSerialReason.ServerRequiresSerialParts;
                case UploadSerialReason.ServerWithdrewParallelParts:
                    return FilesUploadSerialReason.ServerWithdrewParallelParts;
                case UploadSerialReason.LengthUnknown:
                    return FilesUploadSerialReason.LengthUnknown;
                default:
                    return FilesUploadSerialReason.None;
            }
        }

        // Passes the engine's progress reports on as the module's. The receiver only keeps the latest report, so this
        // returns at once to the upload thread that reported. A spooled stream's size is known once its copy is
        // complete, so until then it is being copied.
        private sealed class ProgressRelay : IProgress<TransferProgress>
        {
            private readonly IProgress<FilesTransferProgress> receiver;
            private readonly bool spooled;

            internal ProgressRelay(IProgress<FilesTransferProgress> receiver, bool spooled)
            {
                this.receiver = receiver;
                this.spooled = spooled;
            }

            public void Report(TransferProgress value)
            {
                receiver.Report(new FilesTransferProgress(value.Path, value.TotalBytes, value.TotalBytesKnown, spooled && !value.TotalBytesKnown, value.SpooledBytes, value.AcknowledgedBytes, value.SentBytes, value.PartRetries, value.Elapsed, value.ActiveParts, value.ConcurrencyLimit, value.Concurrency?.CooldownRemaining ?? TimeSpan.Zero));
            }
        }

        // Passes the engine's trace records on as the module's, field by field, on the upload thread that reported. The
        // receiver keeps at most its limit of records and never blocks or throws, so the upload never waits for, or fails
        // because of, the pipeline. A record of a kind this module does not know is left out.
        private sealed class TraceRelay : IProgress<UploadTraceEvent>
        {
            private readonly IProgress<FilesUploadTrace> receiver;

            internal TraceRelay(IProgress<FilesUploadTrace> receiver)
            {
                this.receiver = receiver;
            }

            public void Report(UploadTraceEvent value)
            {
                FilesUploadTrace record;
                switch (value)
                {
                    case UploadAttemptTrace attempt:
                        record = new FilesUploadAttemptTrace(attempt.Path, attempt.Elapsed, attempt.PartNumber, attempt.Attempt, attempt.Accepted, attempt.StatusCode, attempt.Throttled, attempt.RetryAfter, attempt.Duration, attempt.AdmissionWait, attempt.PartBytes, attempt.SentBytes, attempt.Target);
                        break;
                    case UploadDecisionTrace decision:
                        record = new FilesUploadDecisionTrace(decision.Path, decision.Elapsed, Decision(decision.Decision), decision.PreviousTarget, decision.Target, BelowTarget(decision.BelowTargetReason), decision.ThroughputBytesPerSecond, decision.BaselineBytesPerSecond, decision.QueueEstimate, decision.Cooldown);
                        break;
                    case UploadSizingTrace sizing:
                        record = new FilesUploadSizingTrace(sizing.Path, sizing.Elapsed, SizeDecision(sizing.Decision), sizing.PreviousSize, sizing.Size, Constraint(sizing.Constraint), sizing.ThroughputBytesPerSecond, sizing.BaselineBytesPerSecond);
                        break;
                    default:
                        return;
                }
                receiver.Report(record);
            }
        }
    }
}
