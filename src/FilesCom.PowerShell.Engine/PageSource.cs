using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell.Engine
{
    // Pages of one SDK list, loaded in order with FilesList.LoadNextPageAsync. One page loads at a time, even when a
    // continuation is used from several runspaces, and after the last page the source never starts over: FilesList
    // itself would go back to the first page.
    internal sealed class PageSource<TSdk, TResult> : FilesPageSource<TResult> where TResult : FilesObject
    {
        private readonly FilesList<TSdk> list;
        private readonly Func<TSdk, TResult> project;
        private readonly SemaphoreSlim oneAtATime = new SemaphoreSlim(1, 1);
        private int pagesLoaded;
        private bool finished;

        internal PageSource(FilesConnection connection, FilesList<TSdk> list, Func<TSdk, TResult> project) : base(connection)
        {
            this.list = list;
            this.project = project;
        }

        internal override int PagesLoaded => Volatile.Read(ref pagesLoaded);

        internal override async Task<FilesPageResult<TResult>> LoadNextPageAsync(CancellationToken cancellationToken)
        {
            await oneAtATime.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (finished)
                {
                    throw new InvalidOperationException("The list has no more pages.");
                }
                await list.LoadNextPageAsync(cancellationToken).ConfigureAwait(false);
                finished = !list.HasNextPage;
                Interlocked.Increment(ref pagesLoaded);
                return new FilesPageResult<TResult>(list.data.Select(project).ToArray(), !finished);
            }
            catch (SdkException failure)
            {
                throw ErrorTranslation.Translate(failure);
            }
            finally
            {
                oneAtATime.Release();
            }
        }
    }
}
