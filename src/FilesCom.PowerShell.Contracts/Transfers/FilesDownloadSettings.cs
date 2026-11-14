namespace FilesCom.PowerShell
{
    // The download options Receive-FilesFile's caller chose for a local file. Null values leave the engine's defaults.
    internal sealed class FilesDownloadSettings
    {
        internal FilesDownloadSettings(int? maxConcurrency, long? partSize, FilesTransferLimiter limiter, bool replaceRegularFile, string checkpointPath, FilesCheckpointRestart? checkpointRestart, bool? checkDownloadModificationTime = null)
        {
            MaxConcurrency = maxConcurrency;
            PartSize = partSize;
            Limiter = limiter;
            ReplaceRegularFile = replaceRegularFile;
            CheckpointPath = checkpointPath;
            CheckpointRestart = checkpointRestart;
            CheckDownloadModificationTime = checkDownloadModificationTime;
        }

        internal bool? CheckDownloadModificationTime { get; }

        internal int? MaxConcurrency { get; }

        internal long? PartSize { get; }

        internal FilesTransferLimiter Limiter { get; }

        // -Force: the finished file may replace a regular file at its name, never a folder or link. Otherwise it is put in
        // place only where nothing is at its name.
        internal bool ReplaceRegularFile { get; }

        // A local file, as a full path, in which the download records its progress so that running it again continues it.
        internal string CheckpointPath { get; }

        // Set only with CheckpointPath; null for the engine's default, Fail.
        internal FilesCheckpointRestart? CheckpointRestart { get; }
    }

    // The options Receive-FilesDirectory's caller chose. Every file of the folder is downloaded with the same per-file
    // choices; the checkpoint folder and limiter are the whole download's.
    internal sealed class FilesDirectoryDownloadSettings
    {
        internal FilesDirectoryDownloadSettings(int? maxConcurrentFiles, int? maxConcurrency, long? partSize, FilesTransferLimiter limiter, bool replaceRegularFile, string checkpointDirectory, FilesCheckpointRestart? checkpointRestart)
        {
            MaxConcurrentFiles = maxConcurrentFiles;
            MaxConcurrency = maxConcurrency;
            PartSize = partSize;
            Limiter = limiter;
            ReplaceRegularFile = replaceRegularFile;
            CheckpointDirectory = checkpointDirectory;
            CheckpointRestart = checkpointRestart;
        }

        internal int? MaxConcurrentFiles { get; }

        // Per file: the most parts fetched at once and their size.
        internal int? MaxConcurrency { get; }

        internal long? PartSize { get; }

        internal FilesTransferLimiter Limiter { get; }

        internal bool ReplaceRegularFile { get; }

        // A local folder, as a full path, in which the download records its progress; created by the engine if missing.
        internal string CheckpointDirectory { get; }

        // Set only with CheckpointDirectory; null for the engine's default, Fail.
        internal FilesCheckpointRestart? CheckpointRestart { get; }
    }
}
