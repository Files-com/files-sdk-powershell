using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell.Runtime
{
    /// <summary>
    /// The base of FilesCom commands: runs engine operations away from the pipeline thread with cancellation from
    /// Ctrl+C and pipeline stops, and reports failures as Files.* error records.
    /// </summary>
    public abstract class FilesCmdlet : PSCmdlet
    {
        private readonly CancellationTokenSource stopping = new CancellationTokenSource();

        // How long the command has run: a connection's operation timeout bounds the whole command, across every record,
        // page, file and request, not each request on its own.
        private readonly Stopwatch running = Stopwatch.StartNew();

        // The stages of the local files the command writes, made when it writes the first.
        private OwnedFileStages stages;

        /// <summary>The object an error record names for the current record, such as its ID or path.</summary>
        protected object ErrorTarget { get; set; }

        /// <summary>Runs the command for the current record, reporting any failure as a Files.* error record.</summary>
        protected sealed override void ProcessRecord()
        {
            ErrorTarget = null;
            try
            {
                Execute();
            }
            catch (Exception failure) when (!(failure is RuntimeException))
            {
                ErrorRecord record = FilesErrors.Record(failure, ErrorTarget, MyInvocation);
                if (TerminatesOn(failure))
                {
                    FinishStages();
                    ThrowTerminatingError(record);
                }
                WriteError(record);
            }
        }

        internal OwnedFileStages Stages => stages ?? (stages = OwnedFileStages.Create());

        // Ends the command's local stages and warns of each private folder that had to be left in place. A command that
        // writes local files calls it when it ends and from Dispose, where report is false because the pipeline has
        // ended and nothing more can be written.
        internal void FinishStages(bool report = true)
        {
            if (stages == null)
            {
                return;
            }
            IReadOnlyList<string> kept = stages.Finish();
            stages = null;
            if (report)
            {
                foreach (string message in kept)
                {
                    WriteWarning(message);
                }
            }
        }

        /// <summary>Does the command's work for the current record.</summary>
        protected abstract void Execute();

        /// <summary>Whether a failure stops the command rather than only the current record.</summary>
        protected virtual bool TerminatesOn(Exception failure)
        {
            return FilesErrors.IsTerminating(failure);
        }

        /// <summary>Asks the engine operation in progress to stop. Called on another thread; it only signals.</summary>
        protected override void StopProcessing()
        {
            stopping.Cancel();
        }

        internal T Run<T>(FilesConnection connection, string activity, Func<PipelineOperation, Task<T>> operation)
        {
            using (var run = new PipelineOperation(this, connection, connection.OperationTimeout, Remaining(connection.OperationTimeout), connection.DiagnosticDebug, stopping.Token, activity))
            {
                return run.Wait(token => operation(run));
            }
        }

        // Runs an operation that makes a connection, bounded by the timeout the new connection will have and with its
        // diagnostics, or, with no settings, one that no timeout bounds. A connection made with -DiagnosticDebug shows
        // the engine's DEBUG messages of its sign-in too, which may contain sensitive information, such as the request
        // and response bodies with the password or one-time code sent and the session ID received.
        internal T RunWithoutConnection<T>(FilesConnectionSettings settings, Func<CancellationToken, Task<T>> operation)
        {
            TimeSpan? operationTimeout = settings?.OperationTimeout;
            using (var run = new PipelineOperation(this, null, operationTimeout, Remaining(operationTimeout), settings?.DiagnosticDebug ?? false, stopping.Token, MyInvocation.MyCommand.Name))
            {
                return run.Wait(operation);
            }
        }

        // What is left of the command's operation timeout, which may already have passed.
        private TimeSpan? Remaining(TimeSpan? operationTimeout)
        {
            return operationTimeout.HasValue ? operationTimeout.Value - running.Elapsed : (TimeSpan?)null;
        }

        /// <summary>Runs an engine operation with the connection and waits for its result on the pipeline thread.</summary>
        protected T Run<T>(FilesConnection connection, Func<CancellationToken, Task<T>> operation)
        {
            return Run(connection, MyInvocation.MyCommand.Name, run => operation(run.Token));
        }

        /// <summary>Runs an engine operation that returns nothing, and waits for it to finish.</summary>
        protected void Run(FilesConnection connection, Func<CancellationToken, Task> operation)
        {
            Run(connection, MyInvocation.MyCommand.Name, async run =>
            {
                await operation(run.Token).ConfigureAwait(false);
                return true;
            });
        }

        /// <summary>Whether the caller supplied the parameter, including as an explicit $false or empty value.</summary>
        protected bool IsBound(string parameterName)
        {
            return MyInvocation.BoundParameters.ContainsKey(parameterName);
        }

        /// <summary>
        /// Confirms a change with the caller, naming the site it goes to. False under -WhatIf or when the caller
        /// declines; nothing has been sent before this returns.
        /// </summary>
        protected bool ShouldProcessOn(FilesConnection connection, string target)
        {
            return ShouldProcess($"{target} on {connection.BaseUrl}");
        }
    }
}
