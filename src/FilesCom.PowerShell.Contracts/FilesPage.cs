using System;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// One page of a list, written by a list command's <c>-Page</c> and <c>-Continuation</c> parameter sets.
    /// </summary>
    /// <typeparam name="T">The type of the listed objects.</typeparam>
    /// <remarks>
    /// Pass <see cref="Continuation"/> to the same command's <c>-Continuation</c> parameter to get the next page.
    /// Without <c>-Page</c>, list commands write the objects of every page instead.
    /// </remarks>
    public sealed class FilesPage<T> where T : FilesObject
    {
        internal FilesPage(T[] items, FilesContinuation continuation)
        {
            Items = items ?? Array.Empty<T>();
            Continuation = continuation;
        }

        /// <summary>The objects on this page, which may be none even when more pages follow.</summary>
        public T[] Items { get; }

        /// <summary>Whether another page follows.</summary>
        public bool HasMore => Continuation != null;

        /// <summary>Gets the next page when given to <c>-Continuation</c>; null after the last page.</summary>
        public FilesContinuation Continuation { get; }
    }
}
