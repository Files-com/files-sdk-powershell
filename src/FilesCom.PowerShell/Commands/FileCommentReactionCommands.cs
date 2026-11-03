using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Create File Comment Reaction.
    /// </summary>
    [Cmdlet("New", "FilesFileCommentReaction", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.FileCommentReaction))]
    public sealed class NewFilesFileCommentReactionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// ID of file comment to attach reaction to.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long FileCommentId { get; set; }

        /// <summary>
        /// Emoji to react with.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Emoji { get; set; }

        /// <summary>Create File Comment Reaction.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "File comment reaction"))
            {
                return;
            }
            Models.FileCommentReaction result = Run(connection, token => FileCommentReactionBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(FileCommentId)))
            {
                parameters["file_comment_id"] = FileCommentId;
            }
            if (IsBound(nameof(Emoji)))
            {
                parameters["emoji"] = Emoji;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete File Comment Reaction.
    /// </summary>
    [Cmdlet("Remove", "FilesFileCommentReaction", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesFileCommentReactionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// File Comment Reaction ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The file comment reaction to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.FileCommentReaction InputObject { get; set; }

        /// <summary>Delete File Comment Reaction.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"File comment reaction {id}"))
            {
                return;
            }
            Run(connection, token => FileCommentReactionBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
