using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FilesCom.PowerShell
{
    // The engine diagnostics of one command's operation, collected on the engine's threads for the pipeline thread to
    // write: messages at INFO and above, which have lost the credentials a URL in them could carry, to the Verbose
    // stream, and the engine's DEBUG messages, which may contain URLs, request and response bodies and other sensitive
    // information, to the Debug stream only when the connection was made with -DiagnosticDebug. The operation's
    // asynchronous flow carries its sink in Current, so a message logged by a request of another command, runspace or
    // connection never reaches it.
    internal sealed class FilesLogSink
    {
        // Enough for every retry and request of a long command; the pipeline thread takes messages as they come, so
        // the limit only matters when it cannot keep up.
        private const int Capacity = 10000;

        private static readonly AsyncLocal<FilesLogSink> current = new AsyncLocal<FilesLogSink>();

        private readonly ConcurrentQueue<FilesLogMessage> messages = new ConcurrentQueue<FilesLogMessage>();
        private readonly Action wake;
        private int count;
        private int dropped;
        private int closed;

        internal FilesLogSink(bool debug, Action wake)
        {
            Debug = debug;
            this.wake = wake;
        }

        // The sink of the operation whose flow this code runs in, or null.
        internal static FilesLogSink Current
        {
            get => current.Value;
            set => current.Value = value;
        }

        // Whether DEBUG messages are wanted.
        internal bool Debug { get; }

        internal void Add(bool debug, string text)
        {
            if (Volatile.Read(ref closed) != 0)
            {
                return;
            }
            if (Interlocked.Increment(ref count) > Capacity)
            {
                Interlocked.Decrement(ref count);
                Interlocked.Increment(ref dropped);
                return;
            }
            messages.Enqueue(new FilesLogMessage(debug, text));
            wake();
        }

        internal bool TryTake(out FilesLogMessage message)
        {
            if (messages.TryDequeue(out message))
            {
                Interlocked.Decrement(ref count);
                return true;
            }
            return false;
        }

        // Ends the sink once its operation has ended: messages logged afterwards, by work the operation left behind, are
        // ignored rather than kept for a pipeline that no longer reads them.
        internal void Close()
        {
            Volatile.Write(ref closed, 1);
        }

        // How many messages were dropped since the last call because the sink was full.
        internal int TakeDropped()
        {
            return Interlocked.Exchange(ref dropped, 0);
        }
    }

    internal readonly struct FilesLogMessage
    {
        internal FilesLogMessage(bool debug, string text)
        {
            Debug = debug;
            Text = text;
        }

        internal bool Debug { get; }

        internal string Text { get; }
    }
}
