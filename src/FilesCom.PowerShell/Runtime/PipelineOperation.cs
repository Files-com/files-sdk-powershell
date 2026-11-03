using System;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell.Runtime
{
    // Runs one engine operation for a command. The operation runs on the thread pool, away from the pipeline thread,
    // while the pipeline thread waits for it and is the only thread that writes to the pipeline: progress the
    // operation reports is kept as a single latest snapshot and shown from here, and the engine's diagnostics are
    // collected in the operation's FilesLogSink and written from here to the Verbose and Debug streams. Cancellation
    // reaches the operation through Token, and Wait returns only once the operation has stopped, however it ends,
    // including when writing to the pipeline fails or stops the command: then this operation alone is cancelled, nothing
    // more is written, and the failure is thrown on unchanged once the operation has settled. Until then the event and
    // sink the operation's threads use stay valid; afterwards they ignore those threads. An upload's trace records, when
    // captured, are kept the same way, up to their limit, and written from here to the Information stream.
    internal sealed class PipelineOperation : IProgress<FilesTransferProgress>, IDisposable
    {
        private static readonly TimeSpan ProgressInterval = TimeSpan.FromMilliseconds(250);
        // The tag of the Information records Send-FilesFile -Diagnostics writes.
        private static readonly string[] TraceTags = { "FilesCom.UploadTrace" };

        private readonly FilesCmdlet command;
        private readonly FilesConnection connection;
        private readonly TimeSpan? operationTimeout;
        private readonly CancellationTokenSource timeout;
        private readonly CancellationTokenSource linked;
        private readonly ManualResetEventSlim wake = new ManualResetEventSlim(false);
        private readonly int progressId;
        private readonly string activity;
        // The last record written, which the completed record repeats. Each write is a new record, so a host that
        // keeps them sees every one as it was written.
        private ProgressRecord progressRecord;
        private readonly FilesLogSink log;
        private TraceCapture trace;
        private FilesTransferProgress latestProgress;
        private DateTime lastProgressWrite = DateTime.MinValue;

        // connection is null for an operation that makes one, such as signing in; operationTimeout then comes from
        // the settings of the connection being made. remaining is what the command has left of that timeout: the
        // operation is cancelled when it runs out, at once if it already has.
        // diagnosticDebug lets the engine's DEBUG messages reach the Debug stream.
        internal PipelineOperation(FilesCmdlet command, FilesConnection connection, TimeSpan? operationTimeout, TimeSpan? remaining, bool diagnosticDebug, CancellationToken stopping, string activity)
        {
            this.command = command;
            this.connection = connection;
            this.operationTimeout = operationTimeout;
            timeout = new CancellationTokenSource();
            if (remaining.HasValue && remaining.Value <= TimeSpan.Zero)
            {
                timeout.Cancel();
            }
            else if (remaining.HasValue)
            {
                timeout.CancelAfter(remaining.Value);
            }
            linked = CancellationTokenSource.CreateLinkedTokenSource(stopping, connection?.Lifetime ?? CancellationToken.None, timeout.Token);
            progressId = Math.Abs(activity.GetHashCode() % 10000) + 1;
            this.activity = activity;
            log = new FilesLogSink(diagnosticDebug, Wake);
        }

        // Cancelled when the pipeline stops, the connection closes or the command's operation timeout passes.
        internal CancellationToken Token => linked.Token;

        internal IProgress<FilesTransferProgress> Progress => this;

        // Captures the trace records of the upload to path, at most limit of them, for the pipeline thread to write to
        // the Information stream with a summary once the upload has ended.
        internal IProgress<FilesUploadTrace> CaptureTrace(string path, int limit)
        {
            var capture = new TraceCapture(path, limit, Wake);
            Volatile.Write(ref trace, capture);
            return capture;
        }

        void IProgress<FilesTransferProgress>.Report(FilesTransferProgress value)
        {
            Interlocked.Exchange(ref latestProgress, value);
            Wake();
        }

        internal T Wait<T>(Func<CancellationToken, Task<T>> operation)
        {
            // The sink is set inside the operation's own flow, which every continuation of the operation inherits, and
            // never on the pipeline thread.
            Task<T> task = Task.Run(() =>
            {
                FilesLogSink.Current = log;
                return operation(Token);
            }, Token);
            // Kept, so that Wait ends only once this continuation has run too.
            Task woken = task.ContinueWith(_ => Wake(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            try
            {
                while (!task.IsCompleted)
                {
                    wake.Wait();
                    wake.Reset();
                    WriteLog();
                    WriteTrace();
                    WriteLatestProgress(force: false);
                }
                WriteLog();
                WriteTrace();
                WriteTraceSummary(task);
                CompleteProgress();
            }
            catch
            {
                // A write failed or stopped the command, as a Verbose message does under VerbosePreference Stop, or a
                // later command in the pipeline ended it. Only this operation's own token is cancelled, never the
                // connection or another command's, and the failure goes on unchanged once the operation has ended.
                Settle(task, woken, cancel: true);
                throw;
            }
            Settle(task, woken, cancel: false);
            try
            {
                return task.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) when (Token.IsCancellationRequested)
            {
                throw CancellationFailure();
            }
            catch (ObjectDisposedException) when (connection != null && !connection.IsOpen)
            {
                // The connection closed between the operation taking its client and using it.
                throw Closed();
            }
        }

        // Waits for the operation and the continuation that wakes this thread to end, cancelling the operation first when
        // asked. Nothing is written meanwhile; a failure of the operation is left for the caller to observe, or observed
        // here when the caller is already throwing another. The sink then ignores messages logged later.
        private void Settle(Task task, Task woken, bool cancel)
        {
            if (cancel)
            {
                try
                {
                    linked.Cancel();
                }
                catch (AggregateException)
                {
                    // A callback registered on the token failed; the token is cancelled all the same.
                }
            }
            try
            {
                task.Wait();
            }
            catch (AggregateException)
            {
                // Observed: the caller reports the pipeline's failure, or reads the operation's own from its result.
            }
            woken.Wait();
            log.Close();
            Volatile.Read(ref trace)?.Close();
        }

        // Wakes the pipeline thread, from the operation's threads. After the operation has settled and been disposed, a
        // late call does nothing.
        private void Wake()
        {
            try
            {
                wake.Set();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void WriteLog()
        {
            while (log.TryTake(out FilesLogMessage message))
            {
                if (command.Stopping)
                {
                    continue;
                }
                if (message.Debug)
                {
                    command.WriteDebug(message.Text);
                }
                else
                {
                    command.WriteVerbose(message.Text);
                }
            }
            int dropped = log.TakeDropped();
            if (dropped > 0 && !command.Stopping)
            {
                command.WriteVerbose($"FilesCom: {dropped} more engine messages were left out.");
            }
        }

        // Writes the trace records captured so far. Records are written only while the command runs: once the pipeline
        // is stopping, they are left out, and no summary claims otherwise.
        private void WriteTrace()
        {
            TraceCapture capture = Volatile.Read(ref trace);
            if (capture == null)
            {
                return;
            }
            while (capture.TryTake(out FilesUploadTrace record))
            {
                if (command.Stopping)
                {
                    continue;
                }
                command.WriteInformation(record, TraceTags);
                capture.Written++;
            }
        }

        // The last record of an upload that completed or failed: how many records were written and how many were left
        // out. A failure of the upload is thrown on after it, unchanged.
        private void WriteTraceSummary(Task task)
        {
            TraceCapture capture = Volatile.Read(ref trace);
            if (capture == null || command.Stopping)
            {
                return;
            }
            var outcome = task.Status == TaskStatus.RanToCompletion ? FilesUploadTraceOutcome.Completed : FilesUploadTraceOutcome.Failed;
            command.WriteInformation(new FilesUploadTraceSummary(capture.Path, outcome, capture.Written, capture.Dropped, capture.Limit), TraceTags);
        }

        internal void Wait(Func<CancellationToken, Task> operation)
        {
            Wait(async token =>
            {
                await operation(token).ConfigureAwait(false);
                return true;
            });
        }

        // Why the operation was cancelled, which decides how the command reports it.
        private Exception CancellationFailure()
        {
            if (command.Stopping)
            {
                return new PipelineStoppedException();
            }
            if (connection != null && connection.Lifetime.IsCancellationRequested)
            {
                return Closed();
            }
            if (timeout.IsCancellationRequested)
            {
                return new FilesException(FilesErrorKind.OperationTimeout, $"The command did not finish within the connection's operation timeout of {operationTimeout}.");
            }
            return new FilesException(FilesErrorKind.Cancelled, "The operation was cancelled.");
        }

        private FilesException Closed()
        {
            return new FilesException(FilesErrorKind.ConnectionClosed, $"The connection to {connection.BaseUrl} was closed while the command ran.");
        }

        private void WriteLatestProgress(bool force)
        {
            FilesTransferProgress progress = Volatile.Read(ref latestProgress);
            if (progress == null || command.Stopping)
            {
                return;
            }
            DateTime now = DateTime.UtcNow;
            if (!force && now - lastProgressWrite < ProgressInterval)
            {
                return;
            }
            lastProgressWrite = now;
            // A directory transfer counts its files, of which ActiveParts are being transferred now.
            bool directory = progress.FilesTotal.HasValue;
            string parts = progress.ActiveParts > 1 ? $", {progress.ActiveParts} {(directory ? "files" : "parts")} at once" : "";
            string path = directory ? $"{progress.Path}: {progress.FilesDone} of {progress.FilesTotal} files," : $"{progress.Path}:";
            // A transfer whose total is not known, such as a stream of unknown length until it ends, has no percentage
            // or time remaining: its total so far is only what has been read.
            bool known = progress.TotalBytesKnown && progress.TotalBytes > 0;
            string status;
            if (progress.Spooling)
            {
                status = $"{path} copying the stream to a temporary file before sending it, {progress.SpooledBytes:N0} bytes so far";
            }
            else if (known)
            {
                status = $"{path} {progress.AcknowledgedBytes:N0} of {progress.TotalBytes:N0} bytes{parts}";
            }
            else if (progress.TotalBytes > 0)
            {
                status = $"{path} {progress.AcknowledgedBytes:N0} of {progress.TotalBytes:N0} bytes read so far, total unknown{parts}";
            }
            else
            {
                status = $"{path} {progress.AcknowledgedBytes:N0} bytes{parts}";
            }
            if (progress.CooldownRemaining > TimeSpan.Zero)
            {
                // Files.com's storage asked the upload to slow down; parts already sent go on.
                status += $"; throttled, starting no new part for {progress.CooldownRemaining.TotalSeconds:0.0} s";
            }
            var record = new ProgressRecord(progressId, activity, status)
            {
                PercentComplete = known ? (int)Math.Min(100, progress.AcknowledgedBytes * 100 / progress.TotalBytes) : -1,
            };
            if (known && progress.Elapsed.TotalSeconds >= 1 && progress.AcknowledgedBytes > 0 && progress.TotalBytes > progress.AcknowledgedBytes)
            {
                double bytesPerSecond = progress.AcknowledgedBytes / progress.Elapsed.TotalSeconds;
                record.SecondsRemaining = (int)Math.Min(int.MaxValue, (progress.TotalBytes - progress.AcknowledgedBytes) / bytesPerSecond);
            }
            command.WriteProgress(record);
            progressRecord = record;
        }

        private void CompleteProgress()
        {
            if (Volatile.Read(ref latestProgress) == null || command.Stopping)
            {
                return;
            }
            WriteLatestProgress(force: true);
            command.WriteProgress(new ProgressRecord(progressId, activity, progressRecord.StatusDescription) { RecordType = ProgressRecordType.Completed });
        }

        public void Dispose()
        {
            log.Close();
            Volatile.Read(ref trace)?.Close();
            linked.Dispose();
            timeout.Dispose();
            wake.Dispose();
        }
    }
    // An upload's trace records, from the engine's threads, for the pipeline thread to write. It keeps at most Limit of
    // them for the whole upload, however slowly the pipeline takes them: later ones are counted and left out. Adding
    // never blocks or throws, and after Close records are ignored.
    internal sealed class TraceCapture : IProgress<FilesUploadTrace>
    {
        private readonly ConcurrentQueue<FilesUploadTrace> records = new ConcurrentQueue<FilesUploadTrace>();
        private readonly Action wake;
        private int accepted;
        private int dropped;
        private int closed;

        internal TraceCapture(string path, int limit, Action wake)
        {
            Path = path;
            Limit = limit;
            this.wake = wake;
        }

        internal string Path { get; }

        internal int Limit { get; }

        internal int Dropped => Volatile.Read(ref dropped);

        // Records the pipeline thread has written; only it counts them.
        internal int Written { get; set; }

        public void Report(FilesUploadTrace value)
        {
            if (Volatile.Read(ref closed) != 0)
            {
                return;
            }
            if (Interlocked.Increment(ref accepted) > Limit)
            {
                Interlocked.Decrement(ref accepted);
                Interlocked.Increment(ref dropped);
                return;
            }
            records.Enqueue(value);
            wake();
        }

        internal bool TryTake(out FilesUploadTrace record)
        {
            return records.TryDequeue(out record);
        }

        internal void Close()
        {
            Volatile.Write(ref closed, 1);
        }
    }
}
