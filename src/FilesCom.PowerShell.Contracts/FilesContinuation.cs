using System.Threading;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// The position after one page of a list, which the same command's <c>-Continuation</c> parameter continues from.
    /// </summary>
    /// <remarks>
    /// A continuation belongs to the command, query and connection that produced it and exists only in this session;
    /// it cannot be saved and resumed later. Each continuation loads its page once: a page that loaded successfully
    /// gives a new continuation for the page after it, and the used one cannot load again. After a failure the same
    /// continuation can be tried again.
    /// </remarks>
    public sealed class FilesContinuation
    {
        private const int Unused = 0;
        private const int Loading = 1;
        private const int Used = 2;
        private int state;

        internal FilesContinuation(FilesPageSource source, string commandName, int pageNumber)
        {
            Source = source;
            CommandName = commandName;
            PageNumber = pageNumber;
        }

        /// <summary>The command that continues from here.</summary>
        public string CommandName { get; }

        /// <summary>The number of the page this continuation loads, counting the first page as 1.</summary>
        public int PageNumber { get; }

        /// <summary>Whether this continuation has already loaded its page.</summary>
        public bool IsUsed => Volatile.Read(ref state) == Used;

        internal FilesPageSource Source { get; }

        // Claims the continuation for one load. False when it is loading elsewhere or already used.
        internal bool TryBeginLoad()
        {
            return Interlocked.CompareExchange(ref state, Loading, Unused) == Unused;
        }

        internal void CompleteLoad()
        {
            Volatile.Write(ref state, Used);
        }

        internal void AbandonLoad()
        {
            Volatile.Write(ref state, Unused);
        }

        /// <summary>Describes the continuation.</summary>
        public override string ToString()
        {
            return $"{CommandName} page {PageNumber}{(IsUsed ? " (used)" : "")}";
        }
    }
}
