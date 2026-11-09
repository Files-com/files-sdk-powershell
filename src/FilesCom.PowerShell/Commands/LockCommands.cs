using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Locks by Path.
    /// </summary>
    [Cmdlet("Get", "FilesPathLock", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Lock), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Lock>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesPathLockCommand : FilesListCmdlet<Models.Lock>
    {
        /// <summary>
        /// Path to operate on.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "List")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Page")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Used for pagination.  When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`.  Send one of those cursor value here to resume an existing list from the next available record.  Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Cursor { get; set; }

        /// <summary>
        /// Number of records to show per page.  (Max: 10000, 1,000 or less is recommended).
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PerPage { get; set; }

        /// <summary>
        /// Include locks from children objects?
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public bool? IncludeChildren { get; set; }

        /// <summary>List Locks by Path.</summary>
        protected override void Execute()
        {
            WriteList(connection => LockBridge.ListFor(connection, Path, ListForParameters(), CallOptions));
        }

        private Dictionary<string, object> ListForParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Cursor)))
            {
                parameters["cursor"] = Cursor;
            }
            if (IsBound(nameof(PerPage)))
            {
                parameters["per_page"] = PerPage.Value;
            }
            if (IsBound(nameof(IncludeChildren)))
            {
                parameters["include_children"] = IncludeChildren.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Lock.
    /// </summary>
    [Cmdlet("New", "FilesLock", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Lock))]
    public sealed class NewFilesLockCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Lock token. With expected_token, use the same value to refresh or a different value to replace the existing token.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Token { get; set; }

        /// <summary>
        /// Require this existing, unexpired token before refreshing or replacing a lock. Set token to the same value to refresh, or a different value to replace.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString ExpectedToken { get; set; }

        /// <summary>
        /// Can lock be modified by users other than its creator?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AllowAccessByAnyUser { get; set; }

        /// <summary>
        /// Is lock exclusive?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Exclusive { get; set; }

        /// <summary>
        /// Does lock apply to subfolders?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Recursive { get; set; }

        /// <summary>
        /// Arbitrary descriptive label for the lock. Does not change the lock creator or permissions.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Owner { get; set; }

        /// <summary>
        /// Lock timeout in seconds
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? Timeout { get; set; }

        /// <summary>Create Lock.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            string path = Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, $"Lock '{path}'"))
            {
                return;
            }
            Models.Lock result = Run(connection, token => LockBridge.CreateAsync(connection, path, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Token)))
            {
                parameters["token"] = RequestValues.Reveal(Token);
            }
            if (IsBound(nameof(ExpectedToken)))
            {
                parameters["expected_token"] = RequestValues.Reveal(ExpectedToken);
            }
            if (IsBound(nameof(AllowAccessByAnyUser)))
            {
                parameters["allow_access_by_any_user"] = AllowAccessByAnyUser.Value;
            }
            if (IsBound(nameof(Exclusive)))
            {
                parameters["exclusive"] = Exclusive.Value;
            }
            if (IsBound(nameof(Recursive)))
            {
                parameters["recursive"] = Recursive.Value;
            }
            if (IsBound(nameof(Owner)))
            {
                parameters["owner"] = Owner;
            }
            if (IsBound(nameof(Timeout)))
            {
                parameters["timeout"] = Timeout.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Lock.
    /// </summary>
    [Cmdlet("Remove", "FilesLock", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesLockCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Path
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Lock token
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [Parameter(Mandatory = true, ParameterSetName = "InputObject")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Token { get; set; }

        /// <summary>
        /// The lock to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Lock InputObject { get; set; }

        /// <summary>Delete Lock.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Lock '{path}'"))
            {
                return;
            }
            Run(connection, token => LockBridge.DeleteAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Token)))
            {
                parameters["token"] = Token;
            }
            return parameters;
        }
    }
}
