using System;
using System.Management.Automation;

namespace FilesCom.PowerShell.Runtime
{
    /// <summary>
    /// The base of commands that list a paged collection. By default they write every object, loading one page at a
    /// time as the pipeline consumes them; <c>-First</c> and <c>-Skip</c> bound what is written, and stopping the
    /// pipeline early, as <c>Select-Object -First</c> does, loads no further page. <c>-Page</c> writes the first page
    /// as a <see cref="FilesPage{T}"/>, and <c>-Continuation</c> writes the page after an earlier one.
    /// </summary>
    /// <typeparam name="T">The type of the listed objects.</typeparam>
    public abstract class FilesListCmdlet<T> : FilesApiCmdlet where T : FilesObject
    {
        /// <summary>The parameter set that writes every object.</summary>
        protected const string ListSet = "List";

        /// <summary>The parameter set that writes the first page.</summary>
        protected const string PageSet = "Page";

        /// <summary>The parameter set that writes the page after a continuation.</summary>
        protected const string ContinuationSet = "Continuation";

        private ulong skipped;
        private ulong written;

        /// <summary>Writes the first page as one FilesPage object, whose Continuation gets the next.</summary>
        [Parameter(Mandatory = true, ParameterSetName = PageSet)]
        public SwitchParameter Page { get; set; }

        /// <summary>Writes the page after the one this continuation came from, as one FilesPage object.</summary>
        [Parameter(Mandatory = true, ParameterSetName = ContinuationSet)]
        [ValidateNotNull]
        public FilesContinuation Continuation { get; set; }

        /// <summary>
        /// Every failure of a list stops it, even for one page after others were written, so a partial listing is
        /// never taken for a complete one. Writing a single object by ID fails per record as usual.
        /// </summary>
        protected override bool TerminatesOn(Exception failure)
        {
            return IsListSet || base.TerminatesOn(failure);
        }

        private bool IsListSet => ParameterSetName == ListSet || ParameterSetName == PageSet || ParameterSetName == ContinuationSet;

        /// <summary>
        /// Writes the list the current parameter set asks for: every object of <paramref name="openList"/>'s query,
        /// its first page, or the next page of -Continuation's query.
        /// </summary>
        internal void WriteList(Func<FilesConnection, FilesPageSource<T>> openList)
        {
            if (ParameterSetName == ContinuationSet)
            {
                WriteContinuedPage();
                return;
            }

            RejectPagingParameters(ParameterSetName == PageSet);
            FilesConnection connection = ResolveConnection();
            FilesPageSource<T> source = openList(connection);
            if (ParameterSetName == PageSet)
            {
                WriteObject(LoadPage(source));
                return;
            }

            if (PagingParameters.IncludeTotalCount)
            {
                WriteObject(PagingParameters.NewTotalCount(0, 0.0));
            }
            bool hasMore = true;
            while (hasMore && !FirstReached)
            {
                WriteVerbose($"Loading page {source.PagesLoaded + 1}.");
                FilesPageResult<T> page = Run(connection, token => source.LoadNextPageAsync(token));
                foreach (T item in page.Items)
                {
                    WriteRecord(item);
                    if (FirstReached)
                    {
                        break;
                    }
                }
                hasMore = page.HasMore;
            }
        }

        /// <summary>Writes one object, within the -Skip and -First bounds.</summary>
        protected void WriteRecord(T item)
        {
            if (skipped < PagingParameters.Skip)
            {
                skipped++;
                return;
            }
            if (FirstReached)
            {
                return;
            }
            written++;
            WriteObject(item);
        }

        private bool FirstReached => written >= PagingParameters.First;

        private void WriteContinuedPage()
        {
            RejectPagingParameters(pageSet: true);
            FilesContinuation continuation = Continuation;
            // Checked before the continuation is claimed, so the command it belongs to can still use it.
            if (!(continuation.Source is FilesPageSource<T> source) || !string.Equals(continuation.CommandName, MyInvocation.MyCommand.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"This continuation belongs to {continuation.CommandName}, not {MyInvocation.MyCommand.Name}.");
            }
            FilesConnection connection = Owned(source.Connection, "continuation");
            if (!continuation.TryBeginLoad())
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"This continuation of {continuation.CommandName} has already loaded page {continuation.PageNumber}, or is loading it now. Continue from the Continuation of the page it returned.");
            }
            FilesPage<T> page;
            try
            {
                page = LoadPage(source, connection);
            }
            catch
            {
                continuation.AbandonLoad();
                throw;
            }
            continuation.CompleteLoad();
            WriteObject(page);
        }

        private FilesPage<T> LoadPage(FilesPageSource<T> source, FilesConnection connection = null)
        {
            FilesPageResult<T> result = Run(connection ?? source.Connection, token => source.LoadNextPageAsync(token));
            FilesContinuation next = result.HasMore ? new FilesContinuation(source, MyInvocation.MyCommand.Name, source.PagesLoaded + 1) : null;
            return new FilesPage<T>(result.Items, next);
        }

        private void RejectPagingParameters(bool pageSet)
        {
            if (pageSet && (IsBound("First") || IsBound("Skip") || IsBound("IncludeTotalCount")))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-First, -Skip and -IncludeTotalCount apply to the objects a list writes, not to -Page or -Continuation.");
            }
        }
    }
}
