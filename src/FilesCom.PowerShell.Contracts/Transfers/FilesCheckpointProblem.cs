namespace FilesCom.PowerShell
{
    /// <summary>Why a transfer's checkpoint file cannot be used (<see cref="FilesException.CheckpointProblem"/>).</summary>
    public enum FilesCheckpointProblem
    {
        /// <summary>
        /// It cannot be read, is not a checkpoint the engine, in this format, wrote, or, for a download, its stage no longer
        /// holds the bytes it records.
        /// </summary>
        Unreadable,

        /// <summary>It records another transfer: another site, workspace, remote path, local file or kind of transfer.</summary>
        OtherTransfer,

        /// <summary>
        /// The local file is not the one the upload it records was sending: its length changed, or the bytes of a part
        /// Files.com accepted are no longer the bytes that were sent.
        /// </summary>
        SourceChanged,

        /// <summary>Files.com no longer knows the upload it records, which expired or was cancelled before it was completed.</summary>
        UploadExpired,

        /// <summary>
        /// The upload's parts no longer go where, or within the limits, they did when it started: another S3 upload,
        /// other part limits or parts available, or parts recorded outside the current limits.
        /// </summary>
        GrantChanged,

        /// <summary>
        /// It is kept where another user could have changed it, such as a folder other users can write to, or it is a
        /// link or another user's file, so what it says is not relied on.
        /// </summary>
        Untrusted,
    }
}
