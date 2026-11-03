using System.Management.Automation;
using FilesCom.PowerShell.Binding;

namespace FilesCom.PowerShell.Runtime
{
    /// <summary>
    /// The base of commands that call Files.com: the connection they use and the workspace their requests run in.
    /// </summary>
    public abstract class FilesApiCmdlet : FilesCmdlet
    {
        private FilesCallOptions callOptions;

        /// <summary>
        /// The connection to use. Defaults to the connection of the -InputObject or -Continuation given, and otherwise
        /// to the current runspace's default connection.
        /// </summary>
        [Parameter]
        [ValidateNotNull]
        public FilesConnection Connection { get; set; }

        /// <summary>
        /// Runs this command's requests in this workspace, including workspace 0, instead of the connection's or, for an
        /// -InputObject, instead of the workspace the object was read in. The connection and the object are unchanged.
        /// </summary>
        [Parameter]
        [FilesInteger]
        public long WorkspaceScope { get; set; }

        /// <summary>Runs this command's requests in no workspace, even if the connection has one.</summary>
        [Parameter]
        public SwitchParameter NoWorkspaceScope { get; set; }

        /// <summary>The workspace options for this command's requests.</summary>
        internal FilesCallOptions CallOptions
        {
            get
            {
                if (callOptions == null)
                {
                    if (IsBound(nameof(WorkspaceScope)) && NoWorkspaceScope.IsPresent)
                    {
                        throw new FilesException(FilesErrorKind.InvalidArgument, "Use either -WorkspaceScope or -NoWorkspaceScope, not both.");
                    }
                    callOptions = IsBound(nameof(WorkspaceScope)) || NoWorkspaceScope.IsPresent
                        ? new FilesCallOptions(IsBound(nameof(WorkspaceScope)) ? WorkspaceScope : (long?)null, NoWorkspaceScope.IsPresent)
                        : FilesCallOptions.None;
                }
                return callOptions;
            }
        }

        /// <summary>
        /// The workspace options for a request about <paramref name="inputObject"/>: -WorkspaceScope or -NoWorkspaceScope
        /// when given, else the options the object was read with, so it is acted on in the workspace it came from.
        /// </summary>
        internal FilesCallOptions CallOptionsFor(FilesObject inputObject)
        {
            FilesCallOptions given = CallOptions;
            if (inputObject == null || !ReferenceEquals(given, FilesCallOptions.None))
            {
                return given;
            }
            return inputObject.BoundCallOptions ?? FilesCallOptions.None;
        }

        /// <summary>
        /// The engine's object <paramref name="inputObject"/> was read as, for the command to run that object's own
        /// operation: it keeps the object's options and goes where the object was read from, such as the address of its
        /// site's region. Null without an object, or when -WorkspaceScope or -NoWorkspaceScope asks for other options,
        /// which only the connection's own operations take.
        /// </summary>
        internal object RetainedModel(FilesObject inputObject)
        {
            return inputObject != null && ReferenceEquals(CallOptions, FilesCallOptions.None) ? inputObject.EngineModel : null;
        }

        /// <summary>
        /// The connection for the current record: the input object's own, which -Connection may only confirm, else
        /// -Connection, else the runspace default. A connection that is already closed is refused here, before the
        /// command asks to confirm anything or sends a request.
        /// </summary>
        internal FilesConnection ResolveConnection(FilesObject inputObject = null)
        {
            if (inputObject != null)
            {
                return Owned(inputObject.BoundConnection, "object");
            }
            // The default as selected, even when another runspace has closed it since, so that is what the error says.
            FilesConnection connection = Connection ?? RunspaceConnections.Current.SelectedDefault;
            if (connection == null)
            {
                throw new FilesException(FilesErrorKind.NoConnection, "There is no Files.com connection. Connect with Connect-FilesAccount, then pass the connection with -Connection or make it the default with Set-FilesConnection.");
            }
            connection.RequireOpen();
            return connection;
        }

        // The connection an object or continuation belongs to. Specifying a different -Connection is refused rather
        // than mixing one connection's object with another's credentials, and a closed one is refused, never replaced.
        internal FilesConnection Owned(FilesConnection owner, string what)
        {
            if (owner == null)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The {what} has no connection. Pass its ID or path with -Connection instead.");
            }
            if (Connection != null && !ReferenceEquals(Connection, owner))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The {what} belongs to the connection to {owner.BaseUrl} ({owner.Id}), not the one given with -Connection.");
            }
            owner.RequireOpen();
            return owner;
        }
    }
}
