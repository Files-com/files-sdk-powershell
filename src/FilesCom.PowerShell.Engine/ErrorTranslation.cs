using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using FilesCom.Transfers;

namespace FilesCom.PowerShell.Engine
{
    // Translates the SDK's exceptions to FilesException: the error's message, type, status, title, instance, request ID
    // and validation messages and fields. FilesException makes every one of those texts safe itself, removing the
    // credentials, query and fragment of any URL, since a signed URL keeps its credentials there; the kind comes from
    // the SDK exception's own class and the raw error type, before either is made safe. Response headers other than the
    // request ID and error data are left behind; data can hold a partial session ID, which only a sign-in challenge
    // keeps, out of sight.
    internal static class ErrorTranslation
    {
        // Error types whose cause is the resource's current state rather than the request.
        private static readonly HashSet<string> ConflictTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            "processing-failure/already-completed",
            "processing-failure/destination-exists",
            "processing-failure/destination-parent-conflict",
            "processing-failure/expectation-already-has-open-window",
            "processing-failure/file-locked",
            "processing-failure/folder-locked",
            "processing-failure/folder-not-empty",
            "processing-failure/recipient-already-shared",
            "processing-failure/resource-locked",
            "processing-failure/subfolder-locked",
            "processing-failure/sync-in-progress",
        };

        internal static FilesException Translate(SdkException failure)
        {
            FilesException translated = TranslateKind(failure);
            translated.RequestMayHaveBeenProcessed = failure.RequestMayHaveBeenProcessed;
            return translated;
        }

        // Specific exceptions come before the classes they derive from.
        private static FilesException TranslateKind(SdkException failure)
        {
            switch (failure)
            {
                case TransferCheckpointException checkpoint:
                    return new FilesException(FilesErrorKind.CheckpointRefused, WithoutEngineSettings(checkpoint.Message))
                    {
                        CheckpointProblem = Problem(checkpoint.Problem),
                        CheckpointPath = checkpoint.CheckpointPath,
                    };
                case DownloadSourceChangedException changed:
                    return new FilesException(FilesErrorKind.SourceChanged, changed.Message);
                case FileMigrationFailedException migrationFailed:
                    return new FilesException(FilesErrorKind.FileActionFailed, migrationFailed.Message)
                    {
                        FileMigrationId = migrationFailed.Migration?.Id,
                        FileMigrationStatus = migrationFailed.Migration?.Status,
                    };
                case UploadCompletionUnknownException uncertain:
                    return new FilesException(FilesErrorKind.TransferCompletionUnknown, uncertain.Message, new FilesUncertainUpload(uncertain.Path, uncertain.Size, TransferBridge.Parts(uncertain.Parts)));
                case LockoutRegionMismatchException mismatch:
                    return RegionMismatch(mismatch);
                case ApiException apiFailure:
                    return FromApi(apiFailure);
                case InvalidResponseException invalid:
                    return new FilesException(FilesErrorKind.InvalidResponse, invalid.Message, null, invalid.httpStatus == 0 ? (int?)null : invalid.httpStatus, null, null, RequestId(invalid.httpHeaders), null, null);
                case NetworkTimeoutException timeout:
                    return new FilesException(FilesErrorKind.NetworkTimeout, timeout.Message);
                case ApiConnectionException connection:
                    return new FilesException(FilesErrorKind.Connection, connection.Message);
                case AuthenticationException missingCredentials:
                    return new FilesException(FilesErrorKind.Authentication, missingCredentials.Message);
                case InvalidParameterException invalidParameter:
                    return new FilesException(FilesErrorKind.InvalidArgument, invalidParameter.Message);
                case MissingParameterException missingParameter:
                    return new FilesException(FilesErrorKind.InvalidArgument, missingParameter.Message);
                case FilesCom.NotImplementedException notImplemented:
                    return new FilesException(FilesErrorKind.Unsupported, notImplemented.Message);
                default:
                    return new FilesException(FilesErrorKind.ApiError, failure.Message);
            }
        }

        // A downloaded file the engine did not put in place as asked, or put in place without certainty. A refusal is a
        // local file error, like the early refusal of an existing file; a placed but uncertain file is its own kind, so
        // that it is never mistaken for either success or a file that is not there. Whether the file is placed comes from
        // the engine's own Placed, never from the problem: DestinationMoved is reported both ways.
        internal static FilesException Publication(DownloadPublicationException refused, bool inDirectory = false)
        {
            FilesPublicationProblem? problem = Problem(refused.Problem);
            FilesErrorKind kind = refused.Placed
                ? FilesErrorKind.PublicationUncertain
                : problem == FilesPublicationProblem.NotSupported ? FilesErrorKind.Unsupported : FilesErrorKind.LocalFile;
            string message = inDirectory ? refused.Message + " Files of the folder completed before it stay in place; files in progress were stopped, and no other file was started." : refused.Message;
            return new FilesException(kind, message)
            {
                PublicationProblem = problem,
                LocalPath = refused.LocalPath,
                DownloadPlaced = refused.Placed,
                DownloadRetained = refused.Retained,
                CheckpointPath = refused.CheckpointPath,
            };
        }

        // An engine problem this module does not know stays unnamed rather than being passed off as another.
        private static FilesPublicationProblem? Problem(DownloadPublicationProblem problem)
        {
            switch (problem)
            {
                case DownloadPublicationProblem.NotSupported:
                    return FilesPublicationProblem.NotSupported;
                case DownloadPublicationProblem.DestinationExists:
                    return FilesPublicationProblem.DestinationExists;
                case DownloadPublicationProblem.DestinationNotRegularFile:
                    return FilesPublicationProblem.DestinationNotRegularFile;
                case DownloadPublicationProblem.AccessNotKept:
                    return FilesPublicationProblem.AccessNotKept;
                case DownloadPublicationProblem.PlacedNotDurable:
                    return FilesPublicationProblem.PlacedNotDurable;
                case DownloadPublicationProblem.PlacedNotRecorded:
                    return FilesPublicationProblem.PlacedNotRecorded;
                case DownloadPublicationProblem.StageReplaced:
                    return FilesPublicationProblem.StageReplaced;
                case DownloadPublicationProblem.DestinationMoved:
                    return FilesPublicationProblem.DestinationMoved;
                default:
                    return null;
            }
        }

        private static FilesCheckpointProblem Problem(TransferCheckpointProblem problem)
        {
            switch (problem)
            {
                case TransferCheckpointProblem.OtherTransfer:
                    return FilesCheckpointProblem.OtherTransfer;
                case TransferCheckpointProblem.SourceChanged:
                    return FilesCheckpointProblem.SourceChanged;
                case TransferCheckpointProblem.UploadExpired:
                    return FilesCheckpointProblem.UploadExpired;
                case TransferCheckpointProblem.GrantChanged:
                    return FilesCheckpointProblem.GrantChanged;
                case TransferCheckpointProblem.Untrusted:
                    return FilesCheckpointProblem.Untrusted;
                default:
                    return FilesCheckpointProblem.Unreadable;
            }
        }

        // The engine's message without its sentences about its own settings, such as UploadOptions.CheckpointRestartPolicy:
        // the error's recommended action names the command's parameter instead.
        private static string WithoutEngineSettings(string message)
        {
            string[] sentences = Regex.Split(message, @"(?<=\.) ");
            string kept = string.Join(" ", sentences.Where(sentence => !Regex.IsMatch(sentence, @"\b\w+Options\.\w+|CheckpointRestartPolicy")));
            return kept.Length == 0 ? message : kept;
        }

        // The site is served from another region's address, and the command did not move there: the address and the
        // reason are the engine's own, and its message names the module's parameter rather than the engine's setting.
        private static FilesException RegionMismatch(LockoutRegionMismatchException failure)
        {
            return new FilesException(
                FilesErrorKind.RegionMismatch,
                RegionTerms(failure.Message),
                failure.type,
                failure.httpStatus == 0 ? (int?)null : failure.httpStatus,
                failure.title,
                failure.instance,
                RequestId(failure.httpHeaders),
                failure.errors,
                failure.modelErrors)
            {
                RequestedOrigin = failure.RequestedOrigin,
                RegionRelocationRefusal = Refusal(failure.Refusal),
            };
        }

        private static string RegionTerms(string message)
        {
            string terms = message
                .Replace("FilesConfiguration.RegionRelocation is Disabled", "the connection's -RegionRelocation is Disabled")
                .Replace("FilesConfiguration.RegionRelocation does not allow it", "the connection's -RegionRelocation does not allow it")
                .Replace("RegionRelocationPolicy.OnlyBetween allows exact pairs of origins", "Connect-FilesAccount -RegionRelocation with a hashtable of origin pairs allows exact pairs");
            return Regex.Replace(terms, "RegionRelocationPolicy\\.OnlyBetween\\(\"([^\"]*)\", \"([^\"]*)\"\\)", "Connect-FilesAccount -RegionRelocation @{ '$1' = '$2' }");
        }

        private static FilesRegionRelocationRefusal? Refusal(RegionRelocationRefusal? refusal)
        {
            switch (refusal)
            {
                case RegionRelocationRefusal.Disabled:
                    return FilesRegionRelocationRefusal.Disabled;
                case RegionRelocationRefusal.AlreadyRelocated:
                    return FilesRegionRelocationRefusal.AlreadyRelocated;
                case RegionRelocationRefusal.UntrustedIssuer:
                    return FilesRegionRelocationRefusal.UntrustedIssuer;
                case RegionRelocationRefusal.InvalidHost:
                    return FilesRegionRelocationRefusal.InvalidHost;
                case RegionRelocationRefusal.SameOrigin:
                    return FilesRegionRelocationRefusal.SameOrigin;
                case RegionRelocationRefusal.NotAllowed:
                    return FilesRegionRelocationRefusal.NotAllowed;
                case RegionRelocationRefusal.RetriesExhausted:
                    return FilesRegionRelocationRefusal.RetriesExhausted;
                default:
                    return null;
            }
        }

        internal static FilesException FromApi(ApiException failure, FilesSignInChallenge challenge = null)
        {
            return new FilesException(
                challenge != null ? FilesErrorKind.TwoFactorRequired : Kind(failure),
                failure.Message,
                failure.type,
                failure.httpStatus == 0 ? (int?)null : failure.httpStatus,
                failure.title,
                failure.instance,
                RequestId(failure.httpHeaders),
                failure.errors,
                failure.modelErrors,
                challenge);
        }

        // The kind follows the error type's group, which the SDK's exception class names; the HTTP status plays no part.
        private static FilesErrorKind Kind(ApiException failure)
        {
            switch (failure)
            {
                case BadRequestException _:
                    return FilesErrorKind.InvalidArgument;
                case NotAuthenticatedException _:
                    return FilesErrorKind.Authentication;
                case NotAuthorizedException _:
                    return FilesErrorKind.PermissionDenied;
                case NotFoundException _:
                    return FilesErrorKind.NotFound;
                case ProcessingFailureException _:
                    return ConflictTypes.Contains(failure.type ?? "") ? FilesErrorKind.Conflict : FilesErrorKind.ProcessingFailure;
                case RateLimitedException _:
                    return FilesErrorKind.Throttled;
                case ServiceUnavailableException _:
                    return FilesErrorKind.ServiceUnavailable;
                case SiteConfigurationException _:
                    return FilesErrorKind.SiteConfiguration;
                default:
                    return FilesErrorKind.ApiError;
            }
        }

        private static string RequestId(HttpHeaders headers)
        {
            return headers != null && headers.TryGetValues("X-Request-Id", out IEnumerable<string> values) ? values.FirstOrDefault() : null;
        }
    }
}
