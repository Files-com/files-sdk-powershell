using System.Management.Automation;
using System.Text;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Reads a whole remote file: as text, decoded as UTF-8 (a UTF-8 byte order mark is dropped), or with -AsByteStream
    /// as its bytes. The file is downloaded with the engine's download, bound to one version where its storage allows, and
    /// held in memory; use Receive-FilesFile for a file too large for that.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "FilesFileContent")]
    [OutputType(typeof(string), typeof(byte[]))]
    public sealed class GetFilesFileContentCommand : FilesApiCmdlet
    {
        /// <summary>The remote file, used exactly as written.</summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>Writes the file's bytes, as one byte array, instead of its text.</summary>
        [Parameter]
        public SwitchParameter AsByteStream { get; set; }

        /// <summary>Reads the current record's file.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            ErrorTarget = Path;
            byte[] bytes = Run(connection, token => HelperBridge.ReadAllBytesAsync(connection, Path, CallOptions, token));
            if (AsByteStream.IsPresent)
            {
                WriteObject(bytes, enumerateCollection: false);
                return;
            }
            int start = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            WriteObject(new UTF8Encoding(false).GetString(bytes, start, bytes.Length - start));
        }
    }
}
