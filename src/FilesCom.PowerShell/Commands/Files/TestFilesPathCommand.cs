using System.Management.Automation;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell
{
    /// <summary>What Test-FilesPath looks for.</summary>
    public enum FilesPathType
    {
        /// <summary>A file or a folder.</summary>
        Any,

        /// <summary>A file only.</summary>
        File,

        /// <summary>A folder only.</summary>
        Folder,
    }
}

namespace FilesCom.PowerShell.Commands
{
    /// <summary>Tells whether a file or folder is at a remote path: true or false. It changes nothing.</summary>
    [Cmdlet(VerbsDiagnostic.Test, "FilesPath")]
    [OutputType(typeof(bool))]
    public sealed class TestFilesPathCommand : FilesApiCmdlet
    {
        /// <summary>The remote path, used exactly as written.</summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>Whether to look for a file, a folder or either; by default either.</summary>
        [Parameter]
        public FilesPathType PathType { get; set; }

        /// <summary>Tells whether the current record's path exists.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            ErrorTarget = Path;
            bool exists = Run(connection, async token =>
            {
                if (PathType != FilesPathType.Folder && await HelperBridge.FileExistsAsync(connection, Path, CallOptions, token).ConfigureAwait(false))
                {
                    return true;
                }
                return PathType != FilesPathType.File && await HelperBridge.FolderExistsAsync(connection, Path, CallOptions, token).ConfigureAwait(false);
            });
            WriteObject(exists);
        }
    }
}
