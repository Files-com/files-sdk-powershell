namespace FilesCom.PowerShell
{
    /// <summary>
    /// What went wrong, as a FilesCom command's error record ID reports it: <c>Files.</c> followed by the name, such as
    /// <c>Files.NotFound</c>.
    /// </summary>
    public enum FilesErrorKind
    {
        /// <summary>An argument or request parameter is not valid.</summary>
        InvalidArgument,

        /// <summary>Files.com could not authenticate the request, or the connection has no credentials.</summary>
        Authentication,

        /// <summary>
        /// Signing in needs a second factor. The error's target is a <see cref="FilesSignInChallenge"/> that
        /// <c>Connect-FilesAccount -Challenge</c> finishes with the user's code.
        /// </summary>
        TwoFactorRequired,

        /// <summary>The credentials are not allowed to do this.</summary>
        PermissionDenied,

        /// <summary>The resource does not exist.</summary>
        NotFound,

        /// <summary>The resource's current state prevents this, such as an existing destination or a lock.</summary>
        Conflict,

        /// <summary>Files.com is limiting the rate of requests.</summary>
        Throttled,

        /// <summary>Files.com could not process the request.</summary>
        ProcessingFailure,

        /// <summary>The site's configuration or account prevents this.</summary>
        SiteConfiguration,

        /// <summary>Files.com is temporarily unavailable.</summary>
        ServiceUnavailable,

        /// <summary>Files.com returned an error of a type this module does not classify.</summary>
        ApiError,

        /// <summary>No reliable response arrived: the request could not be sent or completed.</summary>
        Connection,

        /// <summary>Files.com's response could not be read.</summary>
        InvalidResponse,

        /// <summary>
        /// A transfer's final request failed after it may have reached Files.com, so whether it completed is unknown.
        /// </summary>
        TransferCompletionUnknown,

        /// <summary>The module or the Files.com engine it runs on cannot do this.</summary>
        Unsupported,

        /// <summary>A local file or folder could not be read or written.</summary>
        LocalFile,

        /// <summary>The module could not load one of its own assemblies.</summary>
        ModuleLoad,

        /// <summary>No connection was given and the runspace has no default connection.</summary>
        NoConnection,

        /// <summary>The connection is closed.</summary>
        ConnectionClosed,

        /// <summary>The command ran longer than its connection's operation timeout.</summary>
        OperationTimeout,

        /// <summary>
        /// A request stalled: no response, or no further response data, arrived within the connection's read timeout,
        /// an upload part sent nothing within its write timeout, or a connection could not be made within its connect
        /// timeout. The engine retries such a request as its retry settings allow before reporting this.
        /// </summary>
        NetworkTimeout,

        /// <summary>The operation was cancelled.</summary>
        Cancelled,

        /// <summary>
        /// An upload did not fit its limits: a stream gave more than -MaxLength, the largest file the engine plans for or
        /// what the parts available can hold; a part would not fit the transfer limiter's buffer space; or -PartSize is
        /// outside the engine's limits. The upload was not completed, so the remote file was neither created nor
        /// changed. The exception's UploadLimit says which limit.
        /// </summary>
        LimitExceeded,

        /// <summary>
        /// Files.com answered that the site is served from another region's address, and the connection's
        /// -RegionRelocation did not let the command move there. The exception's RequestedOrigin names the address and
        /// RegionRelocationRefusal says why the command did not move.
        /// </summary>
        RegionMismatch,

        /// <summary>
        /// A transfer's checkpoint file cannot be used, so nothing was transferred. The exception's CheckpointProblem
        /// says why and CheckpointPath names the file.
        /// </summary>
        CheckpointRefused,

        /// <summary>
        /// The version of the remote file a download was bound to can no longer be downloaded: the file changed during
        /// the download, or Files.com can no longer serve that version. Nothing was published; downloading again gets the
        /// file as it is now.
        /// </summary>
        SourceChanged,

        /// <summary>
        /// A copy, move or other action Files.com finishes in the background failed or was cancelled. What it did before
        /// then stays done. The exception's FileMigrationId and FileMigrationStatus name the migration.
        /// </summary>
        FileActionFailed,
        /// <summary>
        /// A downloaded file was put in place, complete, but without certainty that it will survive a crash or that its
        /// checkpoint recorded it, or in the folder the download opened after that folder moved away from the path given.
        /// The file is not removed. The exception's PublicationProblem says which, and LocalPath names the path given.
        /// </summary>
        PublicationUncertain,
    }
}
