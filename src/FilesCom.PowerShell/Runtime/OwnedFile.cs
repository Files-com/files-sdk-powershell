using System;
using System.IO;
using System.Security.Cryptography;

namespace FilesCom.PowerShell.Runtime
{
    // A local file a command writes in full before anyone sees it at its destination. The bytes go to a stage the
    // command creates and owns, in the destination's folder so publishing stays on one volume; Publish moves that same
    // stage to the destination, and disposing an unpublished stage removes it. Publishing and cleanup act on the stage
    // the command created, never on whatever a path names by then, and the destination folder is the one the command
    // resolved at the start, so neither another process renaming or replacing entries in that folder nor a replaced
    // folder can make the command publish or delete a file it did not write. The destination folder needs only the
    // permissions creating a file there needs: on Unix write and search, not read.
    //
    // Without replace, an existing destination is never overwritten. With it, an entry that is not a regular file when
    // the command checks is refused; the replacement itself is one rename of the command's own file onto the name, which
    // never follows a symbolic link and never replaces a folder. Another process that can change the folder could put a
    // different entry there between that check and the rename, which the rename then replaces: something that process
    // could remove itself. No platform offers a rename that replaces only a regular file. A replacement gives no one
    // access the replaced file did not, apart from its new owner, the user: it takes that file's group and access control
    // list before it grants anyone else anything, or nothing is replaced.
    //
    // On Unix the stage is a file inside a private folder (mode 0700, and on macOS no access control entries) that only
    // the command's user can enter, one for each destination folder the command writes to, and every step works
    // relative to folder descriptors opened at the start, checking device and inode numbers; see UnixOwnedFile for when
    // the private folder is left behind. On Windows the stage file stays open without delete sharing, so no one can
    // rename or delete it, and is renamed into the destination folder, held open for the same reason, through its handle.
    internal abstract class OwnedFile : IDisposable
    {
        private bool published;
        private bool disposed;

        protected OwnedFile(string destination)
        {
            Destination = destination;
            Folder = Path.GetDirectoryName(destination);
            Name = Path.GetFileName(destination);
        }

        // The stream the command writes the file's bytes to. Publish closes it.
        internal abstract Stream Stream { get; }

        internal string Destination { get; }

        protected string Folder { get; }

        protected string Name { get; }

        // Makes the written stage the destination file. Fails, leaving any existing destination as it was, when one
        // exists and replace is false, or when it is not a regular file.
        internal void Publish(bool replace)
        {
            if (published || disposed)
            {
                throw new InvalidOperationException("The file has already been published or abandoned.");
            }
            Stream.Flush();
            PublishStage(replace);
            published = true;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            Close(removeStage: !published);
        }

        protected abstract void PublishStage(bool replace);

        // Releases the stage's handles, removing the stage first when it was not published.
        protected abstract void Close(bool removeStage);

        // A random name for a stage, hidden where the platform hides dot names.
        protected static string StageName()
        {
            var random = new byte[8];
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(random);
            }
            return "." + BitConverter.ToString(random).Replace("-", "").ToLowerInvariant() + ".filescom-partial";
        }

        protected IOException Exists()
        {
            return new IOException($"The local file {Destination} already exists. Use -Force to replace it.");
        }

        protected IOException NotRegular()
        {
            return new IOException($"{Destination} is not a regular file, so the command does not replace it.");
        }
    }
}
