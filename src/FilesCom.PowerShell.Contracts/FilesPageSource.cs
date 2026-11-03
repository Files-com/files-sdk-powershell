using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell
{
    // One list query that loads its pages in order, one at a time: the engine's side of automatic paging and of
    // FilesContinuation. It never goes back to the first page once the last has loaded.
    internal abstract class FilesPageSource
    {
        protected FilesPageSource(FilesConnection connection)
        {
            Connection = connection;
        }

        internal FilesConnection Connection { get; }

        // The number of pages loaded so far.
        internal abstract int PagesLoaded { get; }
    }

    internal abstract class FilesPageSource<T> : FilesPageSource where T : FilesObject
    {
        protected FilesPageSource(FilesConnection connection) : base(connection)
        {
        }

        // Loads the next page. After a failure the same page can be loaded again.
        internal abstract Task<FilesPageResult<T>> LoadNextPageAsync(CancellationToken cancellationToken);
    }

    internal sealed class FilesPageResult<T> where T : FilesObject
    {
        internal FilesPageResult(T[] items, bool hasMore)
        {
            Items = items;
            HasMore = hasMore;
        }

        internal T[] Items { get; }

        internal bool HasMore { get; }
    }
}
