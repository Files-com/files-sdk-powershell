using System.Management.Automation;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Makes a connection the default of the current runspace, which commands use when not given -Connection, or
    /// clears the default.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "FilesConnection", DefaultParameterSetName = "Connection")]
    [OutputType(typeof(FilesConnection))]
    public sealed class SetFilesConnectionCommand : FilesCmdlet
    {
        /// <summary>The connection to make the default.</summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Connection")]
        [ValidateNotNull]
        public FilesConnection Connection { get; set; }

        /// <summary>Clears the default, so commands need -Connection. The connection itself stays open.</summary>
        [Parameter(Mandatory = true, ParameterSetName = "Clear")]
        public SwitchParameter Clear { get; set; }

        /// <summary>Writes the new default connection.</summary>
        [Parameter(ParameterSetName = "Connection")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>Sets or clears the default.</summary>
        protected override void Execute()
        {
            RunspaceConnections connections = RunspaceConnections.Current;
            if (Clear.IsPresent)
            {
                connections.Default = null;
                return;
            }
            Connection.RequireOpen();
            connections.Default = Connection;
            if (PassThru.IsPresent)
            {
                WriteObject(Connection);
            }
        }
    }
}
