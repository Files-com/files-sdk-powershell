namespace FilesCom.PowerShell
{
    /// <summary>
    /// What a transfer with a checkpoint does when the transfer the checkpoint records cannot be continued
    /// (<c>-CheckpointRestart</c> of Send-FilesFile, Receive-FilesFile and Receive-FilesDirectory).
    /// </summary>
    public enum FilesCheckpointRestart
    {
        /// <summary>Fail with Files.CheckpointRefused before any part is sent or fetched. The default.</summary>
        Fail,

        /// <summary>
        /// Transfer the file again from its first byte. An upload starts as a new upload: parts of the old upload are never
        /// completed with parts of the new one, and an upload whose final request was sent is never started over. A download
        /// discards only its own stage and checkpoint, never the destination or another transfer's checkpoint.
        /// </summary>
        StartOver,
    }
}
