using System;
using System.Threading;

namespace FilesCom.PowerShell.Tests
{
    /// <summary>
    /// A receiver of events, such as a PowerShell stream's DataAdding, that fails as a host or pipeline failing to take a
    /// record would: the first call says it has arrived and waits until <see cref="Release"/>, and then it and every later
    /// call throw an InvalidOperationException with the given message. A call waits at most 30 seconds.
    /// </summary>
    public sealed class FailingReceiver
    {
        private readonly ManualResetEventSlim reached = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim released = new ManualResetEventSlim(false);
        private readonly string message;

        public FailingReceiver(string message)
        {
            this.message = message;
        }

        /// <summary>Whether a call has arrived.</summary>
        public bool IsReached => reached.IsSet;

        /// <summary>Lets the waiting call, and every later one, throw.</summary>
        public void Release()
        {
            released.Set();
        }

        /// <summary>A handler for events whose arguments are of type TArgs, failing as described.</summary>
        public EventHandler<TArgs> Handler<TArgs>()
        {
            return (sender, args) =>
            {
                reached.Set();
                released.Wait(TimeSpan.FromSeconds(30));
                throw new InvalidOperationException(message);
            };
        }
    }
}
