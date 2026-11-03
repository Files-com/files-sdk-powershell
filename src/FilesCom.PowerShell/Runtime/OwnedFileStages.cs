using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FilesCom.PowerShell.Runtime
{
    // The stages of the local files one command writes. On Unix the stages for one destination folder share a private
    // folder there, which Finish removes when it may, so a command that cannot remove it safely leaves one empty folder
    // for each destination folder, not one for each file. On Windows each stage is a file of its own and nothing is
    // left. A command finishes its stages when it ends, however it ends.
    internal abstract class OwnedFileStages : IDisposable
    {
        private readonly List<OwnedFile> files = new List<OwnedFile>();

        internal static OwnedFileStages Create()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? (OwnedFileStages)new WindowsStages() : new UnixOwnedFile.Stages();
        }

        // Opens a new stage for destination, whose folder must exist.
        internal OwnedFile Stage(string destination)
        {
            OwnedFile file = Open(destination);
            files.Add(file);
            return file;
        }

        // Ends the stages: first the files, removing any not published, then what they shared. Returns a message for
        // each private folder that had to be left in place, for the command to write as a warning.
        internal IReadOnlyList<string> Finish()
        {
            foreach (OwnedFile file in files)
            {
                file.Dispose();
            }
            files.Clear();
            return FinishShared();
        }

        public void Dispose()
        {
            Finish();
        }

        protected abstract OwnedFile Open(string destination);

        protected abstract IReadOnlyList<string> FinishShared();

        private sealed class WindowsStages : OwnedFileStages
        {
            protected override OwnedFile Open(string destination)
            {
                return new WindowsOwnedFile(destination);
            }

            protected override IReadOnlyList<string> FinishShared()
            {
                return Array.Empty<string>();
            }
        }
    }
}
