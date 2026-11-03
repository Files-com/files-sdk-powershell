using System;
using FilesCom.PowerShell.Models;

namespace FilesCom.PowerShell
{
    /// <summary>A completed download, written by <c>Receive-FilesFile -PassThru</c>.</summary>
    public sealed class FilesDownloadResult
    {
        internal FilesDownloadResult(string remotePath, string localPath, long size, bool versionBound, long parts, long receivedBytes, long resumedBytes, TimeSpan elapsed, bool completedEarlier)
        {
            RemotePath = remotePath;
            LocalPath = localPath;
            Size = size;
            VersionBound = versionBound;
            Parts = parts;
            ReceivedBytes = receivedBytes;
            ResumedBytes = resumedBytes;
            Elapsed = elapsed;
            CompletedEarlier = completedEarlier;
        }

        /// <summary>The remote path of the downloaded file.</summary>
        public string RemotePath { get; }

        /// <summary>The local file written, or null when the download went to a stream.</summary>
        public string LocalPath { get; }

        /// <summary>The number of bytes in the file, all of which were written.</summary>
        public long Size { get; }

        /// <summary>
        /// Whether the download was bound to one version of the remote file, so that every part written is of that
        /// version; false when the file came in one request, which is never split or continued.
        /// </summary>
        public bool VersionBound { get; }

        /// <summary>The number of parts the file was fetched in: 1 for a file that came in one request, 0 for an empty one.</summary>
        public long Parts { get; }

        /// <summary>The bytes received from the network, including parts fetched more than once.</summary>
        public long ReceivedBytes { get; }

        /// <summary>The bytes a continued download already had; 0 for a download that was not continued.</summary>
        public long ResumedBytes { get; }

        /// <summary>How long the download took.</summary>
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Whether an earlier run with the same -CheckpointPath had already put this very file in place (the same file on
        /// storage, of the same length), and this run only finished what that run did not, without fetching it again.
        /// Files.com was not asked again whether it is still the current version. False for a file this run put in place.
        /// </summary>
        public bool CompletedEarlier { get; }

        /// <summary>
        /// Not set: always null. The engine's version-bound download does not return the file's details, and details read
        /// separately would not show which version was downloaded; Get-FilesFile -Path reads the file's current details.
        /// The property remains so that scripts reading it get null rather than an error.
        /// </summary>
        public RemoteFile File => null;
    }
}
