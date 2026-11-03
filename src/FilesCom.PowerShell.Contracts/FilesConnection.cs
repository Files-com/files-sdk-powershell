using System;
using System.Threading;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// A connection to a Files.com site, made by Connect-FilesAccount. It holds the site's address and credentials,
    /// and every command that uses it runs with exactly those settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The object shows only safe metadata; the credentials stay inside the module. Pass it to any command with
    /// <c>-Connection</c>, or make it the default of the current runspace with Set-FilesConnection.
    /// </para>
    /// <para>
    /// A connection belongs to the runspace that made it and closes when that runspace closes, when the module is
    /// removed, or when Disconnect-FilesAccount closes it. Closing it stops the operations using it and disposes the
    /// engine's client, which closes the network connections it made; a handler given with <c>-HttpMessageHandler</c>
    /// is never disposed. Other runspaces, such as <c>ForEach-Object -Parallel</c> script blocks given it with
    /// <c>$using:</c>, may use it while it is open.
    /// </para>
    /// </remarks>
    public sealed class FilesConnection
    {
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private object engineClient;
        private int closed;

        internal FilesConnection(FilesConnectionSettings settings, FilesAuthenticationMethod authenticationMethod, string username, object engineClient)
        {
            Id = Guid.NewGuid();
            BaseUrl = settings.BaseUrl;
            AuthenticationMethod = authenticationMethod;
            Username = username;
            WorkspaceId = settings.WorkspaceId;
            OperationTimeout = settings.OperationTimeout;
            MaxNetworkRetries = settings.MaxNetworkRetries;
            ConnectTimeout = Seconds(settings.ConnectTimeoutSeconds);
            ReadTimeout = Seconds(settings.ReadTimeoutSeconds);
            WriteTimeout = Seconds(settings.WriteTimeoutSeconds);
            Proxy = settings.Proxy;
            UsesSharedHandler = settings.SharedHandler != null;
            DiagnosticDebug = settings.DiagnosticDebug;
            LogLevel = settings.LogLevel;
            RegionRelocation = settings.RegionRelocation ?? FilesRegionRelocationPolicy.Automatic;
            ConnectedAt = DateTime.UtcNow;
            this.engineClient = engineClient;
        }

        /// <summary>Identifies this connection among others in the session.</summary>
        public Guid Id { get; }

        /// <summary>The address of the Files.com site.</summary>
        public string BaseUrl { get; }

        /// <summary>How the connection authenticates.</summary>
        public FilesAuthenticationMethod AuthenticationMethod { get; }

        /// <summary>The username that signed in, for a connection made with a credential; otherwise null.</summary>
        public string Username { get; }

        /// <summary>The workspace commands run in unless a command says otherwise, or null for none.</summary>
        public long? WorkspaceId { get; }

        /// <summary>The longest any one command may run with this connection, or null for no limit.</summary>
        public TimeSpan? OperationTimeout { get; }

        /// <summary>How many times the engine retries a request that failed in a way that may be temporary, or null for its default of 3.</summary>
        public int? MaxNetworkRetries { get; }

        /// <summary>How long the engine waits to connect to a server, or null for its default of 30 seconds (on .NET 8 and later).</summary>
        public TimeSpan? ConnectTimeout { get; }

        /// <summary>How long a request may wait for response data before it stalls, or null for the engine's default of 80 seconds.</summary>
        public TimeSpan? ReadTimeout { get; }

        /// <summary>How long an upload part may go without sending data before it stalls, or null for the engine's default of 80 seconds.</summary>
        public TimeSpan? WriteTimeout { get; }

        /// <summary>The proxy the connection's requests go through, or null when it uses the system's settings or a handler of the caller's.</summary>
        public Uri Proxy { get; }

        /// <summary>Whether the connection sends its requests through an HttpMessageHandler the caller owns.</summary>
        public bool UsesSharedHandler { get; }

        /// <summary>Whether the engine's DEBUG diagnostics, which include Files.com's responses, reach the Debug stream of this connection's commands.</summary>
        public bool DiagnosticDebug { get; }

        /// <summary>The least severe engine diagnostic the connection's commands produce, or null for all of them.</summary>
        public FilesLogLevel? LogLevel { get; }

        /// <summary>
        /// Whether the connection's commands move on to the address Files.com names when it answers that the site is
        /// served from another region: Automatic, Disabled, or only between listed pairs of origins.
        /// </summary>
        public FilesRegionRelocationPolicy RegionRelocation { get; }

        /// <summary>When the connection was made, in UTC.</summary>
        public DateTime ConnectedAt { get; }

        /// <summary>Whether the connection is open. A closed connection cannot run commands.</summary>
        public bool IsOpen => Volatile.Read(ref closed) == 0;

        /// <summary>Whether disconnecting revokes the connection's session on Files.com.</summary>
        public bool OwnsSession => AuthenticationMethod == FilesAuthenticationMethod.Credential;

        // Cancelled when the connection closes, which stops the operations using it.
        internal CancellationToken Lifetime => lifetime.Token;

        // The engine's client, which holds the credentials. Released when the connection closes.
        internal object EngineClient
        {
            get
            {
                object client = Volatile.Read(ref engineClient);
                if (client == null)
                {
                    throw ClosedFailure();
                }
                return client;
            }
        }

        // Refuses a connection that is already closed, as a command does before it asks to confirm anything or sends a
        // request. A connection that closes while a command runs is reported by that command as closing meanwhile.
        internal void RequireOpen()
        {
            if (!IsOpen)
            {
                throw ClosedFailure();
            }
        }

        private FilesException ClosedFailure()
        {
            return new FilesException(FilesErrorKind.ConnectionClosed, $"The connection to {BaseUrl} is closed. Connect again with Connect-FilesAccount.");
        }

        // Closes the connection locally: stops its operations and starts disposing its client, which returns at once and
        // releases the client's network connections once its operations have ended. Returns the client, whose disposal
        // a caller may wait for, or null when the connection was already closed. Revoking an owned session is the
        // caller's separate step, before this.
        internal object Close()
        {
            if (Interlocked.Exchange(ref closed, 1) != 0)
            {
                return null;
            }
            lifetime.Cancel();
            object client = Interlocked.Exchange(ref engineClient, null);
            (client as IDisposable)?.Dispose();
            return client;
        }

        private static TimeSpan? Seconds(int? seconds)
        {
            return seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : (TimeSpan?)null;
        }

        /// <summary>Describes the connection without its credentials.</summary>
        public override string ToString()
        {
            return $"{BaseUrl} ({AuthenticationMethod}{(IsOpen ? "" : ", closed")})";
        }
    }
}
