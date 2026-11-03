namespace FilesCom.PowerShell
{
    /// <summary>
    /// Why a downloaded file was not put in place under its name, or was put in place without certainty
    /// (<see cref="FilesException.PublicationProblem"/>). Whether the file is in place is
    /// <see cref="FilesException.DownloadPlaced"/>, which some problems report either way: read it rather than inferring it
    /// from the problem.
    /// </summary>
    public enum FilesPublicationProblem
    {
        /// <summary>Something is already at the file's name, which a download without -Force leaves as it is. Nothing was replaced.</summary>
        DestinationExists = 0,

        /// <summary>A folder, link or other entry that is not a regular file is at the file's name; it is never replaced or followed.</summary>
        DestinationNotRegularFile = 1,

        /// <summary>
        /// The file at that name cannot be replaced keeping who may use it: its owner, group or access control list cannot be
        /// given to the new file, or cannot be read. Nothing was replaced.
        /// </summary>
        AccessNotKept = 2,

        /// <summary>This platform cannot put the file in place as the command requires without a weaker way of doing it, which is not used. Nothing was replaced.</summary>
        NotSupported = 3,

        /// <summary>The file is in place, complete, but its folder's entry could not be flushed to storage, so it may not survive a crash or power loss.</summary>
        PlacedNotDurable = 4,

        /// <summary>
        /// The file is in place, complete, but recording that in the checkpoint failed. Running the command again with the
        /// same checkpoint recognizes it as this download's file.
        /// </summary>
        PlacedNotRecorded = 5,

        /// <summary>
        /// The downloaded file was replaced where it waited in its private staging folder by another program running as you,
        /// which on macOS and Linux can rename it away and put another file at its name. The downloaded file is not in place
        /// and its bytes are not kept. Found before the file was to be put in place, the file at its name is as it was; found
        /// just after, what that program put at the stage's name was put there instead, taking the place of any file of that
        /// name, and the message says so. Nothing that program put anywhere is removed.
        /// </summary>
        StageReplaced = 6,

        /// <summary>
        /// The folder the file was to be put in is no longer at the path given: it was renamed, or replaced by another,
        /// during the download. Found before the file was put in place, nothing was put in place, and with a checkpoint the
        /// downloaded bytes are kept in the folder the download opened, for a later run that finds that folder at the path
        /// again. Found just after, or for a file an earlier run put in place, the file is in place in that folder, wherever
        /// it now is, not at the path, and is not removed. A folder download whose local directory moves while its remote
        /// folder is listed does nothing in it.
        /// </summary>
        DestinationMoved = 7,
    }
}
