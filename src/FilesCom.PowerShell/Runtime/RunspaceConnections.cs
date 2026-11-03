using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;

namespace FilesCom.PowerShell.Runtime
{
    // The connections one runspace made and its default connection. Each runspace has its own, so a default chosen
    // in one never affects another, and closing a runspace closes the connections it made.
    internal sealed class RunspaceConnections
    {
        private static readonly ConditionalWeakTable<Runspace, RunspaceConnections> ByRunspace = new ConditionalWeakTable<Runspace, RunspaceConnections>();

        private readonly object gate = new object();
        private readonly List<FilesConnection> made = new List<FilesConnection>();
        private FilesConnection defaultConnection;

        private RunspaceConnections(Runspace runspace)
        {
            runspace.StateChanged += (sender, args) =>
            {
                RunspaceState state = args.RunspaceStateInfo.State;
                if (state == RunspaceState.Closing || state == RunspaceState.Closed || state == RunspaceState.Broken)
                {
                    CloseAll();
                }
            };
        }

        // The connections of the runspace running the current pipeline.
        internal static RunspaceConnections Current => ByRunspace.GetValue(Runspace.DefaultRunspace, runspace => new RunspaceConnections(runspace));

        // The default as it was selected, open or not. Commands use it to report a default another runspace closed.
        internal FilesConnection SelectedDefault
        {
            get
            {
                lock (gate)
                {
                    return defaultConnection;
                }
            }
        }

        internal FilesConnection Default
        {
            get
            {
                lock (gate)
                {
                    if (defaultConnection != null && !defaultConnection.IsOpen)
                    {
                        defaultConnection = null;
                    }
                    return defaultConnection;
                }
            }
            set
            {
                lock (gate)
                {
                    defaultConnection = value;
                }
            }
        }

        internal void Add(FilesConnection connection)
        {
            lock (gate)
            {
                made.RemoveAll(existing => !existing.IsOpen);
                made.Add(connection);
            }
        }

        internal IReadOnlyList<FilesConnection> Open
        {
            get
            {
                lock (gate)
                {
                    return made.FindAll(connection => connection.IsOpen);
                }
            }
        }

        // Closes every connection the runspace made, locally: operations using them stop, and no request is sent.
        internal void CloseAll()
        {
            List<FilesConnection> toClose;
            lock (gate)
            {
                toClose = new List<FilesConnection>(made);
                made.Clear();
                defaultConnection = null;
            }
            foreach (FilesConnection connection in toClose)
            {
                connection.Close();
            }
        }
    }
}
