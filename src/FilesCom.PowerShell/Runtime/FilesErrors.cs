using System;
using System.IO;
using System.Management.Automation;

namespace FilesCom.PowerShell.Runtime
{
    // Turns a command's failure into its error record: a stable Files.<Kind> ID, a category, a message and, where
    // there is one, what to do about it. Records carry FilesException, whose members are safe to show; the engine never
    // passes on the SDK's own exceptions, which can hold response headers.
    internal static class FilesErrors
    {
        // The invocation, where given, is the failing command's: advice then names only parameters that command takes.
        internal static ErrorRecord Record(Exception failure, object target, InvocationInfo invocation = null)
        {
            if (failure is FilesException filesFailure)
            {
                return Record(filesFailure, Category(filesFailure), (object)filesFailure.Challenge ?? target, invocation);
            }
            if (failure is ArgumentException)
            {
                return Record(new FilesException(FilesErrorKind.InvalidArgument, failure.Message), ErrorCategory.InvalidArgument, target);
            }
            if ((failure is FileNotFoundException || failure is FileLoadException || failure is BadImageFormatException) && failure.Message.StartsWith("Could not load file or assembly", StringComparison.Ordinal))
            {
                return Record(new FilesException(FilesErrorKind.ModuleLoad, failure.Message), ErrorCategory.ResourceUnavailable, target);
            }
            if (failure is FileNotFoundException || failure is DirectoryNotFoundException)
            {
                return Record(new FilesException(FilesErrorKind.LocalFile, failure.Message), ErrorCategory.ObjectNotFound, target);
            }
            if (failure is UnauthorizedAccessException)
            {
                return Record(new FilesException(FilesErrorKind.LocalFile, failure.Message), ErrorCategory.PermissionDenied, target);
            }
            if (failure is IOException)
            {
                return Record(new FilesException(FilesErrorKind.LocalFile, failure.Message), ErrorCategory.WriteError, target);
            }
            if (failure is OperationCanceledException)
            {
                return Record(new FilesException(FilesErrorKind.Cancelled, "The Files.com engine cancelled the operation, for example because a response timed out."), ErrorCategory.OperationStopped, target);
            }
            return new ErrorRecord(failure, "Files.Unexpected", ErrorCategory.NotSpecified, target);
        }

        // Whether the failure stops the command. A failure that concerns only the current object, such as a resource
        // that does not exist, is written as a non-terminating error so the command can go on with the next object;
        // failures of the connection, credentials, local arguments or a transfer's outcome stop it.
        internal static bool IsTerminating(Exception failure)
        {
            if ((failure is IOException && !failure.Message.StartsWith("Could not load file or assembly", StringComparison.Ordinal)) || failure is UnauthorizedAccessException)
            {
                return false;
            }
            if (!(failure is FilesException filesFailure))
            {
                return true;
            }
            switch (filesFailure.Kind)
            {
                case FilesErrorKind.NotFound:
                case FilesErrorKind.PermissionDenied:
                case FilesErrorKind.Conflict:
                case FilesErrorKind.ProcessingFailure:
                case FilesErrorKind.ApiError:
                case FilesErrorKind.LocalFile:
                case FilesErrorKind.FileActionFailed:
                case FilesErrorKind.PublicationUncertain:
                    return false;
                case FilesErrorKind.InvalidArgument:
                    return !filesFailure.HttpStatus.HasValue;
                default:
                    return true;
            }
        }

        private static ErrorRecord Record(FilesException failure, ErrorCategory category, object target, InvocationInfo invocation = null)
        {
            var record = new ErrorRecord(failure, "Files." + failure.Kind, category, target);
            string recommendation = Recommendation(failure, invocation);
            if (recommendation != null)
            {
                record.ErrorDetails = new ErrorDetails(failure.Message) { RecommendedAction = recommendation };
            }
            return record;
        }

        private static ErrorCategory Category(FilesException failure)
        {
            switch (failure.Kind)
            {
                case FilesErrorKind.InvalidArgument: return ErrorCategory.InvalidArgument;
                case FilesErrorKind.Authentication: return ErrorCategory.AuthenticationError;
                case FilesErrorKind.TwoFactorRequired: return ErrorCategory.AuthenticationError;
                case FilesErrorKind.PermissionDenied: return ErrorCategory.PermissionDenied;
                case FilesErrorKind.NotFound: return ErrorCategory.ObjectNotFound;
                case FilesErrorKind.Conflict: return failure.ErrorType != null && failure.ErrorType.EndsWith("exists", StringComparison.Ordinal) ? ErrorCategory.ResourceExists : ErrorCategory.ResourceBusy;
                case FilesErrorKind.Throttled: return ErrorCategory.LimitsExceeded;
                case FilesErrorKind.ProcessingFailure: return ErrorCategory.InvalidOperation;
                case FilesErrorKind.SiteConfiguration: return ErrorCategory.InvalidOperation;
                case FilesErrorKind.ServiceUnavailable: return ErrorCategory.ResourceUnavailable;
                case FilesErrorKind.Connection: return ErrorCategory.ConnectionError;
                case FilesErrorKind.InvalidResponse: return ErrorCategory.InvalidResult;
                case FilesErrorKind.TransferCompletionUnknown: return ErrorCategory.OperationStopped;
                case FilesErrorKind.Unsupported: return ErrorCategory.NotImplemented;
                case FilesErrorKind.LocalFile: return PublicationCategory(failure.PublicationProblem);
                case FilesErrorKind.ModuleLoad: return ErrorCategory.ResourceUnavailable;
                case FilesErrorKind.NoConnection: return ErrorCategory.InvalidOperation;
                case FilesErrorKind.ConnectionClosed: return ErrorCategory.ResourceUnavailable;
                case FilesErrorKind.OperationTimeout: return ErrorCategory.OperationTimeout;
                case FilesErrorKind.NetworkTimeout: return ErrorCategory.OperationTimeout;
                case FilesErrorKind.Cancelled: return ErrorCategory.OperationStopped;
                case FilesErrorKind.LimitExceeded: return ErrorCategory.LimitsExceeded;
                case FilesErrorKind.RegionMismatch: return ErrorCategory.ResourceUnavailable;
                case FilesErrorKind.CheckpointRefused: return ErrorCategory.InvalidData;
                case FilesErrorKind.SourceChanged: return ErrorCategory.ResourceUnavailable;
                case FilesErrorKind.FileActionFailed: return ErrorCategory.InvalidOperation;
                case FilesErrorKind.PublicationUncertain: return ErrorCategory.WriteError;
                default: return ErrorCategory.NotSpecified;
            }
        }

        private static ErrorCategory PublicationCategory(FilesPublicationProblem? problem)
        {
            switch (problem)
            {
                case FilesPublicationProblem.DestinationExists: return ErrorCategory.ResourceExists;
                case FilesPublicationProblem.DestinationNotRegularFile: return ErrorCategory.InvalidType;
                case FilesPublicationProblem.AccessNotKept: return ErrorCategory.PermissionDenied;
                case FilesPublicationProblem.DestinationMoved: return ErrorCategory.ResourceUnavailable;
                default: return ErrorCategory.WriteError;
            }
        }

        // What to do about the failure, from its kind and, where the engine gave one, its typed reason.
        private static string LimitRecommendation(FilesUploadLimit? limit)
        {
            switch (limit)
            {
                case FilesUploadLimit.StreamLength: return "Declare a -MaxLength that covers the whole stream, or upload the data as a file.";
                case FilesUploadLimit.PartCount: return "Upload from a file, or keep a stream's parts in temporary files with -PartStorage TemporaryFiles, so that parts can be as large as the file needs; the message says which limit applies.";
                case FilesUploadLimit.BufferSpace: return "Share a limiter with more -MaxBufferedBytes, keep the stream's parts in temporary files with -PartStorage TemporaryFiles, or upload from a file.";
                case FilesUploadLimit.MemoryPart: return "Keep the stream's parts in temporary files with -PartStorage TemporaryFiles, or upload from a file.";
                case FilesUploadLimit.SpoolLength: return "Give -Spool a -MaxLength that covers the whole stream.";
                case FilesUploadLimit.FixedPartSize: return "Give a -PartSize within the limits the message names, or leave it out for automatic sizing.";
                default: return "Read the message for the limit and what to change.";
            }
        }

        private static string RegionRecommendation(FilesRegionRelocationRefusal? refusal)
        {
            const string direct = "Connect with -BaseUrl set to the site's address, the error's RequestedOrigin";
            switch (refusal)
            {
                case FilesRegionRelocationRefusal.Disabled: return direct + ", or with -RegionRelocation Automatic.";
                case FilesRegionRelocationRefusal.NotAllowed:
                case FilesRegionRelocationRefusal.UntrustedIssuer: return direct + ", or, if both addresses serve Files.com for these credentials, allow the move with -RegionRelocation @{ '<connection address>' = '<site address>' }.";
                case FilesRegionRelocationRefusal.RetriesExhausted: return direct + ", or with a -MaxNetworkRetries of at least 1: moving takes one of the command's retries.";
                default: return direct + ", when it names one; otherwise check the site's address with Files.com.";
            }
        }

        // Whether the failing command takes the parameter. Without the command, advice names none of a transfer's own.
        private static bool Takes(InvocationInfo invocation, string parameter)
        {
            return invocation != null && invocation.MyCommand.Parameters.ContainsKey(parameter);
        }

        // The failing command's checkpoint parameter as advice names it, or "checkpoint" when that is not known.
        private static string Checkpoint(InvocationInfo invocation)
        {
            return Takes(invocation, "CheckpointDirectory") ? "-CheckpointDirectory" : Takes(invocation, "CheckpointPath") ? "-CheckpointPath" : "checkpoint";
        }

        // Whether the failing command was given a checkpoint, without which running it again recognizes nothing.
        private static bool GivenCheckpoint(InvocationInfo invocation)
        {
            return invocation != null && (invocation.BoundParameters.ContainsKey("CheckpointDirectory") || invocation.BoundParameters.ContainsKey("CheckpointPath"));
        }

        private static string CheckpointRecommendation(FilesCheckpointProblem? problem, InvocationInfo invocation)
        {
            bool restarts = Takes(invocation, "CheckpointRestart");
            switch (problem)
            {
                case FilesCheckpointProblem.SourceChanged:
                case FilesCheckpointProblem.UploadExpired:
                case FilesCheckpointProblem.GrantChanged:
                    return restarts
                        ? "Run the upload again with -CheckpointRestart StartOver to upload the file again from its first byte, as a new upload."
                        : "Delete the checkpoint file the error's CheckpointPath names, then run the same command again, which uploads that file again from its first byte, as a new upload.";
                case FilesCheckpointProblem.Untrusted:
                    return "Keep the checkpoint in a folder only you can change, such as one in your home folder, and not as a link or another user's file.";
                default:
                    string other = $"file that is not a checkpoint of this transfer is never discarded, so delete it or give another {Checkpoint(invocation)}.";
                    return restarts
                        ? "Give each transfer a checkpoint of its own. A download whose checkpoint or stage cannot be continued starts again from its first byte with -CheckpointRestart StartOver, which discards only that download's own state; a " + other
                        : "Give each transfer a checkpoint of its own. A " + other;
            }
        }

        // What to do about a download the engine did not put in place as asked. The advice follows what the engine reports,
        // placed and retained, not the problem alone, and never suggests that nothing changed or that -Force is the fix.
        private static string PublicationRecommendation(FilesException failure, InvocationInfo invocation)
        {
            string kept = failure.DownloadRetained
                ? $" The downloaded bytes are kept with the checkpoint: running the same command again with the same {Checkpoint(invocation)} puts the file in place without downloading it again, once its storage confirms that version is still the file's."
                : "";
            switch (failure.PublicationProblem)
            {
                case FilesPublicationProblem.DestinationExists: return "If the file at that name is to be replaced, run the command again with -Force, which replaces only a regular file; otherwise give another -LiteralPath." + kept;
                case FilesPublicationProblem.DestinationNotRegularFile: return "A folder, link or other special file at that name is never replaced: move it, or give another -LiteralPath." + kept;
                case FilesPublicationProblem.AccessNotKept: return "The file at that name cannot be replaced keeping who may use it: give another -LiteralPath, or have its owner replace or remove it." + kept;
                case FilesPublicationProblem.NotSupported: return "This platform cannot put the file in place without a weaker way of doing it; download to a folder on a local volume" + (Takes(invocation, "OutputStream") ? ", or into a stream with -OutputStream." : ".");
                case FilesPublicationProblem.PlacedNotDurable: return "The file is in place and complete; after a crash or power loss, check it before relying on it.";
                case FilesPublicationProblem.PlacedNotRecorded: return $"The file is in place and complete; run the same command again with the same {Checkpoint(invocation)} to finish its record, which recognizes this file without downloading it again.";
                case FilesPublicationProblem.StageReplaced: return "Another program running as you replaced the downloaded file while it waited to be put in place, so it was not used and its bytes were not kept; as the message says, that program's file may have been put at the name instead. Nothing it put anywhere was removed: check the file at that name and find out what changed the folder before downloading again.";
                case FilesPublicationProblem.DestinationMoved: return MovedRecommendation(failure, invocation);
                default: return null;
            }
        }

        // Retained bytes need the checkpoint they are kept with, so the advice names the one the failing command takes, and
        // offers recognition by a checkpoint only to a command that was given one.
        private static string MovedRecommendation(FilesException failure, InvocationInfo invocation)
        {
            if (failure.DownloadPlaced)
            {
                string recognized = GivenCheckpoint(invocation) ? $", or move the folder back and run the same command again with the same {Checkpoint(invocation)}, which then recognizes it without downloading it again." : ".";
                return "The file is in place, complete, in the folder the download opened, which is no longer at the path given; it was not removed. Find it where that folder now is" + recognized;
            }
            if (failure.DownloadRetained)
            {
                return $"The folder was moved or replaced during the download, so the file was not put in place at the path. The downloaded bytes are kept, privately, in the folder the download opened: once that folder is at the path again, running the same command with the same {Checkpoint(invocation)} puts the file in place without downloading it again; otherwise running it again downloads the file anew.";
            }
            string completed = GivenCheckpoint(invocation) && Takes(invocation, "CheckpointDirectory") ? " The same -CheckpointDirectory recognizes the files an earlier run completed only once their folder is back at the path." : "";
            return "The folder at the path was moved or replaced during the download, so the download stopped without putting anything more in it. Check which folder is at the path now: running the command again downloads into that folder." + completed;
        }

        private static string Recommendation(FilesException failure, InvocationInfo invocation)
        {
            if (failure.Recommendation != null)
            {
                return failure.Recommendation;
            }
            if (failure.RequestMayHaveBeenProcessed)
            {
                return "The request may have reached Files.com and taken effect. Check whether it did before running the command again.";
            }
            switch (failure.Kind)
            {
                case FilesErrorKind.Authentication: return "Check the connection's credentials, or connect again with Connect-FilesAccount.";
                case FilesErrorKind.TwoFactorRequired: return "Finish signing in with Connect-FilesAccount -Challenge, giving it this error's TargetObject and the code from the user's second factor with -OneTimePassword.";
                case FilesErrorKind.ModuleLoad: return "Reinstall the FilesCom module. If another module loaded a different version of this assembly first, import FilesCom first in a new session.";
                case FilesErrorKind.NoConnection: return "Connect with Connect-FilesAccount, which makes the new connection the default of this runspace, or pass a connection with -Connection.";
                case FilesErrorKind.ConnectionClosed: return "Connect again with Connect-FilesAccount, and pass the new connection with -Connection or make it the default with Set-FilesConnection.";
                case FilesErrorKind.Connection: return "Check that this computer can reach the site, then run the command again.";
                case FilesErrorKind.Throttled: return "Wait before sending more requests, and reduce how many run at once.";
                case FilesErrorKind.ServiceUnavailable: return "Run the command again later.";
                case FilesErrorKind.TransferCompletionUnknown: return "Check the remote file, for example with Get-FilesFile, before uploading it again.";
                case FilesErrorKind.OperationTimeout: return "Connect with a longer -OperationTimeout, or split the work into smaller commands.";
                case FilesErrorKind.NetworkTimeout: return "Check that this computer can reach the site. On a slow network, connect with a longer -ReadTimeoutSeconds, -WriteTimeoutSeconds or -ConnectTimeoutSeconds.";
                case FilesErrorKind.LimitExceeded: return LimitRecommendation(failure.UploadLimit);
                case FilesErrorKind.RegionMismatch: return RegionRecommendation(failure.RegionRelocationRefusal);
                case FilesErrorKind.CheckpointRefused: return CheckpointRecommendation(failure.CheckpointProblem, invocation);
                case FilesErrorKind.SourceChanged: return "Download the file again to get it as it is now.";
                case FilesErrorKind.LocalFile:
                case FilesErrorKind.PublicationUncertain:
                case FilesErrorKind.Unsupported: return failure.PublicationProblem.HasValue ? PublicationRecommendation(failure, invocation) : null;
                case FilesErrorKind.FileActionFailed: return failure.FileMigrationId.HasValue ? $"Get-FilesFileMigration -Id {failure.FileMigrationId} shows the migration; what it did before it stopped stays done." : "Check the files the action concerned; what it did before it failed stays done.";
                default: return null;
            }
        }
    }
}
