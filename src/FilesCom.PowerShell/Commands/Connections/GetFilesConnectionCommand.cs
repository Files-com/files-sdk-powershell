using System.Management.Automation;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>Gets the current runspace's default connection, or every open connection it made.</summary>
    [Cmdlet(VerbsCommon.Get, "FilesConnection", DefaultParameterSetName = "Default")]
    [OutputType(typeof(FilesConnection))]
    public sealed class GetFilesConnectionCommand : FilesCmdlet
    {
        /// <summary>Gets every open connection the current runspace made, not only its default.</summary>
        [Parameter(Mandatory = true, ParameterSetName = "All")]
        public SwitchParameter All { get; set; }

        /// <summary>Writes the connections.</summary>
        protected override void Execute()
        {
            RunspaceConnections connections = RunspaceConnections.Current;
            if (All.IsPresent)
            {
                WriteObject(connections.Open, enumerateCollection: true);
                return;
            }
            FilesConnection connection = connections.Default;
            if (connection != null)
            {
                WriteObject(connection);
            }
        }
    }
}
