using System;
using System.Management.Automation;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Waits until Files.com has finished a copy or move it does in the background, or until nothing is left at a path
    /// after a delete. Copy-FilesFile, Move-FilesFile and Remove-FilesFile return once Files.com accepts the request;
    /// this waits, checking at an interval, until it is done, failed or cancelled. Stopping the command, or -Timeout,
    /// stops only the wait: the action goes on at Files.com.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Wait, "FilesFileAction", DefaultParameterSetName = ActionSet)]
    [OutputType(typeof(Models.FileMigration), ParameterSetName = new[] { ActionSet })]
    public sealed class WaitFilesFileActionCommand : FilesApiCmdlet
    {
        private const string ActionSet = "Action";
        private const string RemovedSet = "Removed";

        /// <summary>
        /// A copy or move written by Copy-FilesFile or Move-FilesFile. It is checked with the connection, the address and
        /// the workspace its command used.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ActionSet)]
        [ValidateNotNull]
        public Models.FileAction InputObject { get; set; }

        /// <summary>The remote path to wait on, such as a folder deleted with its contents, which Files.com can finish after it answers.</summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = RemovedSet)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>Waits until Files.com has nothing at -Path.</summary>
        [Parameter(Mandatory = true, ParameterSetName = RemovedSet)]
        public SwitchParameter Removed { get; set; }

        /// <summary>How long to wait between checks, in seconds; by default 1. Each check is one request.</summary>
        [Parameter]
        [ValidateRange(0.001, 86400)]
        public double PollIntervalSeconds { get; set; }

        /// <summary>The longest to wait. The command then fails with Files.OperationTimeout, and the action goes on at Files.com.</summary>
        [Parameter]
        public TimeSpan Timeout { get; set; }

        /// <summary>Waits for the current record's action or path.</summary>
        protected override void Execute()
        {
            TimeSpan? interval = IsBound(nameof(PollIntervalSeconds)) ? TimeSpan.FromSeconds(PollIntervalSeconds) : (TimeSpan?)null;
            TimeSpan? timeout = null;
            if (IsBound(nameof(Timeout)))
            {
                if (Timeout <= TimeSpan.Zero)
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, "-Timeout must be longer than zero.");
                }
                timeout = Timeout;
            }
            if (ParameterSetName == RemovedSet)
            {
                FilesConnection pathConnection = ResolveConnection();
                ErrorTarget = Path;
                Run(pathConnection, token => HelperBridge.WaitUntilRemovedAsync(pathConnection, Path, interval, timeout, CallOptions, token));
                return;
            }
            if (!ReferenceEquals(CallOptions, FilesCallOptions.None))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "An action is checked in the workspace its command ran in; -WorkspaceScope and -NoWorkspaceScope apply only with -Path.");
            }
            FilesConnection connection = ResolveConnection(InputObject);
            ErrorTarget = InputObject;
            Models.FileMigration migration = Run(connection, token => HelperBridge.WaitForActionAsync(connection, RetainedModel(InputObject), interval, timeout, token));
            if (migration != null)
            {
                WriteObject(migration);
            }
        }
    }
}
