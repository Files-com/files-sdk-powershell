using System;

namespace FilesCom.PowerShell
{
    // A snapshot of an upload's progress, from the engine's progress reports. The command shows the latest one with
    // Write-Progress; reports are never written from the engine's threads.
    internal sealed class FilesTransferProgress
    {
        internal FilesTransferProgress(string path, long totalBytes, bool totalBytesKnown, bool spooling, long spooledBytes, long acknowledgedBytes, long sentBytes, int partRetries, TimeSpan elapsed, int activeParts, int concurrencyLimit, TimeSpan cooldownRemaining)
        {
            Path = path;
            TotalBytes = totalBytes;
            TotalBytesKnown = totalBytesKnown;
            Spooling = spooling;
            SpooledBytes = spooledBytes;
            AcknowledgedBytes = acknowledgedBytes;
            SentBytes = sentBytes;
            PartRetries = partRetries;
            Elapsed = elapsed;
            ActiveParts = activeParts;
            ConcurrencyLimit = concurrencyLimit;
            CooldownRemaining = cooldownRemaining;
        }

        internal string Path { get; }

        // The upload's size, or while TotalBytesKnown is false, the bytes read from a stream of unknown length so far.
        internal long TotalBytes { get; }

        internal bool TotalBytesKnown { get; }

        // Whether a stream is still being copied to its spool file, before anything is sent.
        internal bool Spooling { get; }

        internal long SpooledBytes { get; }

        // Bytes Files.com has accepted, each counted once.
        internal long AcknowledgedBytes { get; }

        // Bytes written to the network, counting resent parts again.
        internal long SentBytes { get; }

        internal int PartRetries { get; }

        internal TimeSpan Elapsed { get; }

        // Part requests in flight now, and how many the upload allows at once now.
        internal int ActiveParts { get; }

        internal int ConcurrencyLimit { get; }

        // How long a throttling answer still keeps the upload from starting part requests.
        internal TimeSpan CooldownRemaining { get; }

        // For a directory transfer, the files done, by this run or an earlier one, of every file to transfer; ActiveParts
        // is then the files being transferred now. Null for the transfer of one file.
        internal int? FilesDone { get; set; }

        internal int? FilesTotal { get; set; }
    }
}
