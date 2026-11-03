using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Writes lines of text to a local file in one complete step, such as a report built from Files.com objects. The
    /// lines go to a hidden stage the command creates and owns in the file's folder, and the file appears at its path
    /// only once every input line has arrived: if a command earlier in the pipeline fails or the pipeline stops, the
    /// command removes its stage, and any file already at the path stays as it was.
    /// </summary>
    [Cmdlet(VerbsData.Out, "FilesLocalFile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(FileInfo))]
    public sealed class OutFilesLocalFileCommand : FilesCmdlet, IDisposable
    {
        private OwnedFile file;
        private StreamWriter writer;
        private string destination;
        private bool writing;

        /// <summary>The local file to write, used exactly as written. Its folder must exist.</summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string LiteralPath { get; set; }

        /// <summary>A line of text to write. Each input string becomes one line, in UTF-8 without a byte order mark.</summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [AllowEmptyString]
        [AllowNull]
        public string InputObject { get; set; }

        /// <summary>
        /// Replaces a regular file that already exists at the path, giving the new file that file's group, access control
        /// list and permissions; nothing is replaced when those cannot be carried over. Without it, an existing file makes
        /// the command fail before it reads any input. Another process that changes the folder between the check and the
        /// replacement can have a different entry at that name replaced, never followed.
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }

        /// <summary>Writes the finished file.</summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>Checks the path and opens the stage.</summary>
        protected override void BeginProcessing()
        {
            try
            {
                destination = LocalPaths.NewFileDestination(this, LiteralPath);
                bool replacing = File.Exists(destination);
                if (replacing && !Force.IsPresent)
                {
                    throw new IOException($"The local file {destination} already exists. Use -Force to replace it.");
                }
                if (!ShouldProcess(destination, replacing ? "Replace the file" : "Create the file"))
                {
                    return;
                }
                file = Stages.Stage(destination);
                // The owned file closes the stream: after publishing, the writer must not touch it.
                writer = new StreamWriter(file.Stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 4096, leaveOpen: true);
                writing = true;
            }
            catch (Exception failure) when (!(failure is RuntimeException))
            {
                CloseFile();
                FinishStages();
                ThrowTerminatingError(FilesErrors.Record(failure, LiteralPath));
            }
        }

        /// <summary>Writes the current line to the stage.</summary>
        protected override void Execute()
        {
            if (writing)
            {
                writer.WriteLine(InputObject);
            }
        }

        /// <summary>Every write fails the whole command, so a file is never published without all of its lines.</summary>
        protected override bool TerminatesOn(Exception failure)
        {
            return true;
        }

        /// <summary>Publishes the finished file.</summary>
        protected override void EndProcessing()
        {
            if (!writing)
            {
                return;
            }
            try
            {
                writer.Flush();
                file.Publish(replace: Force.IsPresent);
            }
            catch (Exception failure) when (!(failure is RuntimeException))
            {
                CloseFile();
                FinishStages();
                ThrowTerminatingError(FilesErrors.Record(failure, destination));
            }
            finally
            {
                CloseFile();
            }
            FinishStages();
            if (PassThru.IsPresent)
            {
                WriteObject(new FileInfo(destination));
            }
        }

        /// <summary>Removes the stage unless the file was published, and its private folder when the command ends early.</summary>
        public void Dispose()
        {
            CloseFile();
            FinishStages(report: false);
        }

        private void CloseFile()
        {
            writing = false;
            // The writer leaves the stream open and holds nothing else; the owned file closes the stream, and disposing
            // the writer would flush into a stream that publishing already closed.
            writer = null;
            file?.Dispose();
            file = null;
        }
    }
}
