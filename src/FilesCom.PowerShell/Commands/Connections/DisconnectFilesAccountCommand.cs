using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Closes a connection. A session that Connect-FilesAccount created by signing in is revoked on Files.com first;
    /// API keys and session IDs the caller supplied are never revoked. Operations still using the connection are
    /// cancelled, and the command returns once the engine has finished with what they use and the connection's network
    /// connections are closed; it does not wait for each command to end.
    /// </summary>
    [Cmdlet(VerbsCommunications.Disconnect, "FilesAccount", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(FilesConnection))]
    public sealed class DisconnectFilesAccountCommand : FilesCmdlet
    {
        /// <summary>The connection to close. Defaults to the current runspace's default connection.</summary>
        [Parameter(Position = 0, ValueFromPipeline = true)]
        [ValidateNotNull]
        public FilesConnection Connection { get; set; }

        /// <summary>Closes the connection without revoking a session it created, which then stays valid until it expires.</summary>
        [Parameter]
        public SwitchParameter KeepSession { get; set; }

        /// <summary>Writes the closed connection.</summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Revokes an owned session, then closes the connection locally whatever the revocation's outcome: a failed
        /// revocation is reported as an error once the connection is closed. A stop (Ctrl+C) while the revocation is
        /// pending leaves the connection open, since nothing could be reported after it: the session may still be valid,
        /// and the connection, still the default if it was, is disconnected again later.
        /// </summary>
        protected override void Execute()
        {
            RunspaceConnections connections = RunspaceConnections.Current;
            FilesConnection connection = Connection ?? connections.Default ?? throw new FilesException(FilesErrorKind.NoConnection, "There is no default connection to disconnect.");
            ErrorTarget = connection;
            bool revoke = connection.IsOpen && connection.OwnsSession && !KeepSession.IsPresent;
            if (connection.IsOpen && !ShouldProcess(connection.ToString(), revoke ? "Revoke the session and disconnect" : "Disconnect"))
            {
                return;
            }
            FilesException revocationFailure = null;
            Task closed;
            bool stopped = false;
            try
            {
                if (revoke)
                {
                    try
                    {
                        Run(connection, token => ConnectionBridge.RevokeOwnedSessionAsync(connection, token));
                    }
                    catch (FilesException failure)
                    {
                        revocationFailure = failure;
                    }
                    catch (PipelineStoppedException)
                    {
                        stopped = true;
                        throw;
                    }
                }
            }
            finally
            {
                if (stopped)
                {
                    closed = null;
                }
                else
                {
                    closed = ConnectionBridge.CloseAsync(connection);
                    if (ReferenceEquals(connections.Default, connection))
                    {
                        connections.Default = null;
                    }
                }
            }
            // Stopping the command stops only this wait; the connection is closed either way.
            RunWithoutConnection(null, token => WaitAsync(closed, token));
            if (revocationFailure != null)
            {
                WriteError(RevocationFailed(revocationFailure, connection));
            }
            if (PassThru.IsPresent)
            {
                WriteObject(connection);
            }
        }

        private static async Task<bool> WaitAsync(Task closed, CancellationToken cancellationToken)
        {
            await Task.WhenAny(closed, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await closed.ConfigureAwait(false);
            return true;
        }

        // Local cleanup finished; only the remote revocation failed or is uncertain, which the record says separately.
        private static ErrorRecord RevocationFailed(FilesException failure, FilesConnection connection)
        {
            ErrorRecord record = FilesErrors.Record(failure, connection);
            record.ErrorDetails = new ErrorDetails($"The connection to {connection.BaseUrl} is closed, but Files.com may not have revoked its session: {failure.Message}")
            {
                RecommendedAction = "The session stays valid until it expires. Sign in to Files.com and end it there if it must stop now.",
            };
            return record;
        }
    }
}
