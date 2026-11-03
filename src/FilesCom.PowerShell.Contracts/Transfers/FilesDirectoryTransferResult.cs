using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell
{
    /// <summary>A completed directory transfer, written by <c>Send-FilesDirectory -PassThru</c> and <c>Receive-FilesDirectory -PassThru</c>.</summary>
    public sealed class FilesDirectoryTransferResult
    {
        internal FilesDirectoryTransferResult(string localDirectory, string remoteDirectory, IReadOnlyList<FilesDirectoryTransferEntry> files, IReadOnlyList<string> directories, IReadOnlyList<string> excluded, long bytes, TimeSpan elapsed)
        {
            LocalDirectory = localDirectory;
            RemoteDirectory = remoteDirectory;
            Files = files;
            Directories = directories;
            Excluded = excluded;
            Bytes = bytes;
            Elapsed = elapsed;
        }

        /// <summary>The local directory uploaded, or downloaded into.</summary>
        public string LocalDirectory { get; }

        /// <summary>The remote folder uploaded into, or downloaded.</summary>
        public string RemoteDirectory { get; }

        /// <summary>Every file transferred, or completed by an earlier run of the transfer, in the order of their relative paths.</summary>
        public IReadOnlyList<FilesDirectoryTransferEntry> Files { get; }

        /// <summary>The relative paths of the folders created because they had nothing inside to transfer.</summary>
        public IReadOnlyList<string> Directories { get; }

        /// <summary>
        /// The relative paths of local entries that were not uploaded: symbolic links, and anything else that is neither
        /// a regular file nor a folder. Always empty for a download.
        /// </summary>
        public IReadOnlyList<string> Excluded { get; }

        /// <summary>The bytes of every file in <see cref="Files"/>.</summary>
        public long Bytes { get; }

        /// <summary>How long the transfer took.</summary>
        public TimeSpan Elapsed { get; }
    }

    /// <summary>One file of a completed directory transfer.</summary>
    public sealed class FilesDirectoryTransferEntry
    {
        internal FilesDirectoryTransferEntry(string relativePath, string localPath, string remotePath, long size, bool completedEarlier)
        {
            RelativePath = relativePath;
            LocalPath = localPath;
            RemotePath = remotePath;
            Size = size;
            CompletedEarlier = completedEarlier;
        }

        /// <summary>The file's path inside the transferred directory, with / between names.</summary>
        public string RelativePath { get; }

        /// <summary>The local file.</summary>
        public string LocalPath { get; }

        /// <summary>The remote file.</summary>
        public string RemotePath { get; }

        /// <summary>The file's bytes.</summary>
        public long Size { get; }

        /// <summary>
        /// Whether an earlier run of the transfer, with the same checkpoint folder, completed the file, which this run did
        /// not transfer again: for an upload, the local file still has the bytes that were sent; for a download, Files.com
        /// confirmed the version written is still the file's current one, and the local file is still the one written.
        /// </summary>
        public bool CompletedEarlier { get; }
    }
}
