using System;
using System.Collections.Generic;
using System.Linq;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// The exception in a FilesCom command's error record. It carries what Files.com reported about the failure, never
    /// credentials, response bodies or signed URLs: every URL in any of its text, including the message, error type,
    /// title, instance, request ID, errors and field names and messages, keeps only its scheme, host, port and path.
    /// </summary>
    public sealed class FilesException : Exception
    {
        private static readonly IReadOnlyList<string> NoErrors = Array.Empty<string>();
        private string requestedOrigin;

        internal FilesException(FilesErrorKind kind, string message)
            : base(SafeText.WithoutUrlCredentials(message))
        {
            Kind = kind;
            Errors = NoErrors;
        }

        internal FilesException(
            FilesErrorKind kind,
            string message,
            string errorType,
            int? httpStatus,
            string title,
            string instance,
            string requestId,
            IReadOnlyList<string> errors,
            IReadOnlyDictionary<string, string[]> modelErrors,
            FilesSignInChallenge challenge = null)
            : base(SafeText.WithoutUrlCredentials(message))
        {
            Challenge = challenge;
            Kind = kind;
            ErrorType = SafeText.WithoutUrlCredentials(errorType);
            HttpStatus = httpStatus;
            Title = SafeText.WithoutUrlCredentials(title);
            Instance = SafeText.WithoutUrlCredentials(instance);
            RequestId = SafeText.WithoutUrlCredentials(requestId);
            Errors = errors?.Select(SafeText.WithoutUrlCredentials).ToArray() ?? NoErrors;
            ModelErrors = SafeFields(modelErrors);
        }

        internal FilesException(FilesErrorKind kind, string message, FilesUncertainUpload uncertainUpload)
            : this(kind, message)
        {
            UncertainUpload = uncertainUpload;
        }

        // Messages by field, with the field names and every message made safe. Fields that become the same name keep all
        // their messages.
        private static IReadOnlyDictionary<string, string[]> SafeFields(IReadOnlyDictionary<string, string[]> fields)
        {
            if (fields == null)
            {
                return null;
            }
            var safe = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string[]> field in fields)
            {
                string name = SafeText.WithoutUrlCredentials(field.Key) ?? "";
                string[] messages = (field.Value ?? Array.Empty<string>()).Select(SafeText.WithoutUrlCredentials).ToArray();
                safe[name] = safe.TryGetValue(name, out string[] earlier) ? earlier.Concat(messages).ToArray() : messages;
            }
            return safe;
        }

        /// <summary>What went wrong.</summary>
        public FilesErrorKind Kind { get; }

        /// <summary>The error type Files.com reported, such as <c>not-found/user-not-found</c>.</summary>
        public string ErrorType { get; }

        /// <summary>The HTTP status of Files.com's error response.</summary>
        public int? HttpStatus { get; }

        /// <summary>The title Files.com gave the error.</summary>
        public string Title { get; }

        /// <summary>The identifier Files.com gave this occurrence of the error.</summary>
        public string Instance { get; }

        /// <summary>The request ID from Files.com's response headers, when it sent one.</summary>
        public string RequestId { get; }

        /// <summary>Further error messages Files.com reported.</summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>Messages about individual fields, keyed by field, when Files.com reported them.</summary>
        public IReadOnlyDictionary<string, string[]> ModelErrors { get; }

        /// <summary>For an upload whose completion is unknown, what Files.com had accepted.</summary>
        public FilesUncertainUpload UncertainUpload { get; }

        /// <summary>For a sign-in that needs a second factor, the challenge that finishes it.</summary>
        public FilesSignInChallenge Challenge { get; }

        /// <summary>For Files.LimitExceeded, the limit the upload needed more than.</summary>
        public FilesUploadLimit? UploadLimit { get; internal set; }

        /// <summary>
        /// For Files.RegionMismatch, the address Files.com named for the site, such as https://files.example.eu, or null
        /// when it named none that is an origin.
        /// </summary>
        public string RequestedOrigin
        {
            get => requestedOrigin;
            internal set => requestedOrigin = SafeText.WithoutUrlCredentials(value);
        }

        /// <summary>For Files.RegionMismatch, why the command did not move to <see cref="RequestedOrigin"/>.</summary>
        public FilesRegionRelocationRefusal? RegionRelocationRefusal { get; internal set; }

        /// <summary>
        /// Whether the request may have reached Files.com and taken effect although no usable answer came back. The engine
        /// does not send such a request that changes something again after that, since that could do it twice: check
        /// whether it took effect before running the command again. False when the request was not sent, only read
        /// something, or can safely be received twice; false alone does not show that nothing changed.
        /// </summary>
        public bool RequestMayHaveBeenProcessed { get; internal set; }

        /// <summary>For Files.CheckpointRefused, why the checkpoint cannot be used.</summary>
        public FilesCheckpointProblem? CheckpointProblem { get; internal set; }

        /// <summary>
        /// For Files.CheckpointRefused, the checkpoint file refused. For a download with a
        /// <see cref="PublicationProblem"/>, the checkpoint file of that file's download, when it has one; in a folder
        /// download, that is the file's own checkpoint in the checkpoint folder. An error about a folder download's local
        /// directory itself, or about a file an earlier run of it completed, names none, though the checkpoint folder
        /// keeps the folder's record.
        /// </summary>
        public string CheckpointPath { get; internal set; }

        /// <summary>For Files.FileActionFailed, the ID of the migration that failed, or null when the action failed without one.</summary>
        public long? FileMigrationId { get; internal set; }

        /// <summary>For Files.FileActionFailed, the migration's status as last checked, such as failed or cancelled.</summary>
        public string FileMigrationStatus { get; internal set; }

        /// <summary>
        /// For a download whose file was not put in place under its name, or was put in place without certainty
        /// (Files.LocalFile, Files.Unsupported or Files.PublicationUncertain), why.
        /// </summary>
        public FilesPublicationProblem? PublicationProblem { get; internal set; }

        /// <summary>
        /// For a download with a <see cref="PublicationProblem"/>, the local file it was to put in place, or, for an
        /// error about a folder download's local directory itself, that directory.
        /// </summary>
        public string LocalPath { get; internal set; }

        /// <summary>
        /// For a download with a <see cref="PublicationProblem"/>, whether the downloaded file is in place: complete but
        /// uncertain, as <see cref="PublicationProblem"/> says, or, for DestinationMoved, in the folder the download opened,
        /// which is no longer at the path given. False when the downloaded file was not put in place; that alone does not
        /// show that no file changed, since another program can have put its own file at that name (StageReplaced).
        /// </summary>
        public bool DownloadPlaced { get; internal set; }

        /// <summary>
        /// For a download with a <see cref="PublicationProblem"/>, whether the downloaded bytes were kept as a private stage
        /// with their checkpoint, so that running the command again with the same checkpoint can put the file in place
        /// without downloading it again, once its storage confirms that version is still the file's (and, for
        /// DestinationMoved, once the folder the download opened is at the path again). False for a file already placed and
        /// for a stage another program replaced, even when a checkpoint or folder record remains.
        /// </summary>
        public bool DownloadRetained { get; internal set; }

        // What to do, when the command that failed knows better than the error's kind, as a wait's own -Timeout does.
        internal string Recommendation { get; set; }
    }
}
