using System;

namespace FilesCom.PowerShell
{
    // The upload options Send-FilesFile's caller chose, checked against each other before anything is read or sent.
    // Null values leave the engine's defaults.
    internal sealed class FilesUploadSettings
    {
        internal FilesUploadSettings(
            FilesUploadConcurrencyMode? concurrencyMode,
            int? maxConcurrency,
            FilesUploadPerformanceProfile? performanceProfile,
            bool aggressiveStartup,
            FilesUploadPartSizingMode? partSizingMode,
            long? partSize,
            bool? mkdirParents,
            FilesTransferLimiter limiter,
            DateTime? modifiedTime,
            long? maxLength,
            FilesStreamPartStorage? partStorage,
            bool spool,
            string temporaryDirectory,
            FilesAutomaticConcurrency automaticConcurrency,
            FilesAutomaticPartSizing automaticPartSizing,
            int? maxReadyParts,
            long? maxReadyBytes,
            string checkpointPath,
            FilesCheckpointRestart? checkpointRestart)
        {
            ConcurrencyMode = concurrencyMode;
            MaxConcurrency = maxConcurrency;
            PerformanceProfile = performanceProfile;
            AggressiveStartup = aggressiveStartup;
            PartSizingMode = partSizingMode;
            PartSize = partSize;
            MkdirParents = mkdirParents;
            Limiter = limiter;
            ModifiedTime = modifiedTime;
            MaxLength = maxLength;
            PartStorage = partStorage;
            Spool = spool;
            TemporaryDirectory = temporaryDirectory;
            AutomaticConcurrency = automaticConcurrency;
            AutomaticPartSizing = automaticPartSizing;
            MaxReadyParts = maxReadyParts;
            MaxReadyBytes = maxReadyBytes;
            CheckpointPath = checkpointPath;
            CheckpointRestart = checkpointRestart;
        }

        // A local file, as a full path, in which a file upload records its progress so that running it again continues
        // it. Never for a stream.
        internal string CheckpointPath { get; }

        // Set only with CheckpointPath; null for the engine's default, Fail.
        internal FilesCheckpointRestart? CheckpointRestart { get; }

        internal FilesUploadConcurrencyMode? ConcurrencyMode { get; }

        internal int? MaxConcurrency { get; }

        internal FilesUploadPerformanceProfile? PerformanceProfile { get; }

        // Whether the scaled S3 policy the engine chooses starts at its start instead of probing the link first. Never with
        // Static, PerformanceProfile or AutomaticConcurrency.
        internal bool AggressiveStartup { get; }

        internal FilesUploadPartSizingMode? PartSizingMode { get; }

        // Set only with PartSizingMode Fixed.
        internal long? PartSize { get; }

        internal bool? MkdirParents { get; }

        internal FilesTransferLimiter Limiter { get; }

        internal DateTime? ModifiedTime { get; }

        // For a stream of unknown length: the most bytes it may give, and with Spool also the most copied to the spool
        // file.
        internal long? MaxLength { get; }

        internal FilesStreamPartStorage? PartStorage { get; }

        // Copy a stream of unknown length to a temporary file before sending anything. Requires MaxLength.
        internal bool Spool { get; }

        // An existing local folder, as a full path.
        internal string TemporaryDirectory { get; }

        // Tuning for automatic concurrency, which replaces the profile chosen for the destination. Never with Static.
        internal FilesAutomaticConcurrency AutomaticConcurrency { get; }

        // Tuning for automatic part sizes. Never with Fixed or ServerPlanned.
        internal FilesAutomaticPartSizing AutomaticPartSizing { get; }

        // The most parts, and bytes of their buffers, read ahead and waiting to be sent.
        internal int? MaxReadyParts { get; }

        internal long? MaxReadyBytes { get; }
    }
}
