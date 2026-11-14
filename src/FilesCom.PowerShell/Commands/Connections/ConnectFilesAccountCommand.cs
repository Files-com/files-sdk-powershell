using System;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Security;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Connects to a Files.com site with an API key, an existing session ID, or a username and password, and makes the
    /// connection the default of the current runspace. A sign-in that needs a second factor fails with
    /// Files.TwoFactorRequired, whose target is the challenge -Challenge finishes with the user's code.
    /// </summary>
    [Cmdlet(VerbsCommunications.Connect, "FilesAccount", DefaultParameterSetName = ApiKeySet, SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    [OutputType(typeof(FilesConnection))]
    public sealed class ConnectFilesAccountCommand : FilesCmdlet
    {
        private const string ApiKeySet = "ApiKey";
        private const string SessionSet = "Session";
        private const string CredentialSet = "Credential";
        private const string ChallengeSet = "Challenge";
        private const string DefaultBaseUrl = "https://app.files.com/";

        // The longest timeout or delay the engine accepts, in seconds: the longest wait of the runtime's timers.
        private const int LongestTimeoutSeconds = 2147483;
        private const double LongestDelaySeconds = 2147483.0;

        /// <summary>The API key to authenticate with. Disconnecting never revokes it.</summary>
        [Parameter(Mandatory = true, ParameterSetName = ApiKeySet)]
        [ValidateNotNull]
        public SecureString ApiKey { get; set; }

        /// <summary>An existing session ID to authenticate with. Disconnecting never revokes it.</summary>
        [Parameter(Mandatory = true, ParameterSetName = SessionSet)]
        [ValidateNotNull]
        public SecureString SessionId { get; set; }

        /// <summary>
        /// The username and password to sign in with. Signing in creates a session that the connection owns and
        /// Disconnect-FilesAccount revokes.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = CredentialSet)]
        [Credential]
        [ValidateNotNull]
        public PSCredential Credential { get; set; }

        /// <summary>
        /// A sign-in that needs a second factor, from the target of the Files.TwoFactorRequired error of an earlier
        /// Connect-FilesAccount -Credential. It keeps that sign-in's site and settings.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ChallengeSet)]
        [ValidateNotNull]
        public FilesSignInChallenge Challenge { get; set; }

        /// <summary>
        /// The current code from the user's two-factor authentication device or text message: sent with the credential,
        /// or finishing a -Challenge.
        /// </summary>
        [Parameter(ParameterSetName = CredentialSet)]
        [Parameter(Mandatory = true, ParameterSetName = ChallengeSet)]
        [ValidateNotNullOrEmpty]
        public string OneTimePassword { get; set; }

        /// <summary>The address of the site, such as https://mysite.files.com. Defaults to https://app.files.com.</summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateNotNullOrEmpty]
        public string BaseUrl { get; set; } = DefaultBaseUrl;

        /// <summary>The workspace commands run in unless they say otherwise, including workspace 0.</summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [FilesInteger]
        public long WorkspaceId { get; set; }

        /// <summary>The language Files.com writes messages in, such as de or es.</summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateNotNullOrEmpty]
        public string Language { get; set; }

        /// <summary>
        /// How many times the engine retries a request that failed in a way that may be temporary, such as a connection
        /// failure, a timeout or a server error, each retry shown with -Verbose. Defaults to 3; 0 turns the engine's retries
        /// off. Below them, a request that only reads something, such as a download, can be sent again when its connection
        /// closes before any answer. A request that changes something, such as a delete or the one that completes an upload,
        /// is sent once per try, and again only where Files.com redirects it; after a lost or failed answer it is not sent
        /// again.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateRange(0, 100)]
        public int MaxNetworkRetries { get; set; }

        /// <summary>
        /// Whether downloads check Last-Modified timestamps. Defaults to true; false leaves stronger ETag, size
        /// and response checks in place. Receive-FilesFile can override this setting for one download.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        public bool CheckDownloadModificationTime { get; set; } = true;

        /// <summary>
        /// The wait, in seconds, before the first retry of a Files.com API request; each later retry waits twice as long,
        /// up to -MaxRetryDelaySeconds. Defaults to 0.1. A server that says when to retry is waited for instead.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateRange(0.0, LongestDelaySeconds)]
        public double RetryDelaySeconds { get; set; }

        /// <summary>The longest wait, in seconds, before a retry of a Files.com API request. Defaults to 2.</summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateRange(0.0, LongestDelaySeconds)]
        public double MaxRetryDelaySeconds { get; set; }

        /// <summary>
        /// How long, in seconds, to wait to connect to a server. Defaults to 30. It applies on PowerShell 7 to the
        /// connections the engine makes itself, including through -Proxy; Windows PowerShell has no separate connect
        /// timeout, so there a connection that cannot be made ends when -ReadTimeoutSeconds or -WriteTimeoutSeconds runs
        /// out. A -HttpMessageHandler connects as it is configured.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateRange(1, LongestTimeoutSeconds)]
        public int ConnectTimeoutSeconds { get; set; }

        /// <summary>
        /// How long, in seconds, a request may wait for its response once sent, and then between the parts of the
        /// response. Defaults to 80. It limits a stalled connection, not a long download that keeps receiving data. A
        /// request that runs out is retried like other temporary failures, then fails with Files.NetworkTimeout.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateRange(1, LongestTimeoutSeconds)]
        public int ReadTimeoutSeconds { get; set; }

        /// <summary>
        /// How long, in seconds, an upload part may go without sending data. Defaults to 80. It limits a stalled
        /// connection, not a long upload; time spent reading the local file or stream is not counted.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateRange(1, LongestTimeoutSeconds)]
        public int WriteTimeoutSeconds { get; set; }

        /// <summary>
        /// A proxy server, such as http://proxy.example.com:8080, for every request of the connection, to Files.com and
        /// to the storage addresses of uploads and downloads. By default the system's proxy settings apply.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateNotNull]
        public Uri Proxy { get; set; }

        /// <summary>The credential the -Proxy server asks for.</summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [Credential]
        [ValidateNotNull]
        public PSCredential ProxyCredential { get; set; }

        /// <summary>
        /// An HttpMessageHandler of the caller's, such as one with its own TLS or certificate settings, to send every
        /// request of the connection through, to Files.com and to storage addresses. The connection borrows it: closing
        /// the connection never disposes it, so dispose it yourself once every connection using it is closed. It must
        /// not follow redirects itself (AllowAutoRedirect set to $false), since the engine follows them without sending
        /// credentials to other sites, and it must not retry requests itself.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [ValidateNotNull]
        public HttpMessageHandler HttpMessageHandler { get; set; }

        /// <summary>
        /// Lets the engine's DEBUG diagnostics reach the Debug stream, shown with -Debug, of this command's own sign-in
        /// and of the commands that use this connection. DEBUG messages may contain sensitive information: URLs,
        /// request and response bodies, and values such as the password and one-time code a sign-in sends, the session
        /// ID or partial-login identifier it receives, and passwords, API keys or session IDs other commands send or
        /// receive. Neither what they include nor any redaction is guaranteed. Restrict access to them and review them
        /// before sharing. Use it only to diagnose a problem. Other connections' commands and the Verbose stream never
        /// show them. Without it, only the engine's ordinary diagnostics, such as requests and retries, reach the
        /// Verbose stream, without credentials or queries.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        public SwitchParameter DiagnosticDebug { get; set; }

        /// <summary>
        /// The least severe engine diagnostic this connection's commands produce: Debug, Information, Warning, Error or
        /// None. By default the engine produces all of them, and a command shows its ordinary diagnostics on the Verbose
        /// stream, with -Verbose, and its DEBUG diagnostics only with -DiagnosticDebug. Warning, for example, keeps this
        /// connection's commands from writing requests and retries to the Verbose stream, whatever other connections do.
        /// -DiagnosticDebug can be used only with Debug or no -LogLevel.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        public FilesLogLevel LogLevel { get; set; }

        /// <summary>
        /// Whether commands move on to the address Files.com names when it answers that the site is served from another
        /// region: Automatic, the default, moves to a verified HTTPS address on port 443 with a public DNS name; Disabled
        /// never moves, and the command fails with Files.RegionMismatch naming the address; a hashtable of origin pairs,
        /// such as @{ 'https://files.example.com' = 'https://files.example.eu' }, moves only from each key to its value,
        /// and also allows what Automatic never does, such as another port, a name that is not public or plain HTTP, as a
        /// development site may need. A command that moves keeps the new address for its remaining requests, and so do the
        /// objects and pages it writes; the connection's own address is unchanged. Moving takes one of the command's
        /// retries (-MaxNetworkRetries).
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        [FilesRegionRelocation]
        [ArgumentCompleter(typeof(FilesRegionRelocationCompleter))]
        [ValidateNotNull]
        public FilesRegionRelocationPolicy RegionRelocation { get; set; }

        /// <summary>
        /// The longest any one command may run with this connection, from its start to its end, across all the requests,
        /// pages and files it handles. By default commands have no time limit.
        /// </summary>
        [Parameter(ParameterSetName = ApiKeySet)]
        [Parameter(ParameterSetName = SessionSet)]
        [Parameter(ParameterSetName = CredentialSet)]
        public TimeSpan OperationTimeout { get; set; }

        /// <summary>Leaves the runspace's default connection as it is, so commands use this one only when given it with -Connection.</summary>
        [Parameter]
        public SwitchParameter NoDefault { get; set; }

        /// <summary>Makes the connection and writes it.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == ChallengeSet)
            {
                CompleteChallenge();
                return;
            }
            Uri site = SiteAddress();
            if (IsBound(nameof(OperationTimeout)) && OperationTimeout <= TimeSpan.Zero)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-OperationTimeout must be longer than zero.");
            }
            if (DiagnosticDebug.IsPresent && IsBound(nameof(LogLevel)) && LogLevel != FilesLogLevel.Debug)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"-DiagnosticDebug shows DEBUG diagnostics, which -LogLevel {LogLevel} leaves out. Use -LogLevel Debug, or leave it out.");
            }
            var settings = new FilesConnectionSettings
            {
                BaseUrl = site.AbsoluteUri,
                WorkspaceId = IsBound(nameof(WorkspaceId)) ? WorkspaceId : (long?)null,
                Language = Language,
                MaxNetworkRetries = IsBound(nameof(MaxNetworkRetries)) ? MaxNetworkRetries : (int?)null,
                CheckDownloadModificationTime = IsBound(nameof(CheckDownloadModificationTime)) ? CheckDownloadModificationTime : (bool?)null,
                OperationTimeout = IsBound(nameof(OperationTimeout)) ? OperationTimeout : (TimeSpan?)null,
                ConnectTimeoutSeconds = IsBound(nameof(ConnectTimeoutSeconds)) ? ConnectTimeoutSeconds : (int?)null,
                ReadTimeoutSeconds = IsBound(nameof(ReadTimeoutSeconds)) ? ReadTimeoutSeconds : (int?)null,
                WriteTimeoutSeconds = IsBound(nameof(WriteTimeoutSeconds)) ? WriteTimeoutSeconds : (int?)null,
                InitialRetryDelaySeconds = IsBound(nameof(RetryDelaySeconds)) ? RetryDelaySeconds : (double?)null,
                MaxRetryDelaySeconds = IsBound(nameof(MaxRetryDelaySeconds)) ? MaxRetryDelaySeconds : (double?)null,
                SharedHandler = HttpMessageHandler,
                Proxy = ProxyAddress(),
                ProxyCredentials = ProxyCredential?.GetNetworkCredential(),
                DiagnosticDebug = DiagnosticDebug.IsPresent,
                LogLevel = IsBound(nameof(LogLevel)) ? LogLevel : (FilesLogLevel?)null,
                RegionRelocation = RegionRelocation,
            };
            string action = ParameterSetName == CredentialSet ? $"Sign in as {Credential.UserName} and create a session" : "Connect";
            if (!ShouldProcess(site.AbsoluteUri, action))
            {
                return;
            }
            FilesConnection connection;
            switch (ParameterSetName)
            {
                case SessionSet:
                    connection = ConnectionBridge.ConnectWithSession(settings, RequestValues.Reveal(SessionId));
                    break;
                case CredentialSet:
                    NetworkCredential credential = Credential.GetNetworkCredential();
                    connection = RunWithoutConnection(settings, token => ConnectionBridge.SignInAsync(settings, Credential.UserName, credential.Password, OneTimePassword, token));
                    break;
                default:
                    connection = ConnectionBridge.ConnectWithApiKey(settings, RequestValues.Reveal(ApiKey));
                    break;
            }
            Use(connection);
        }

        // Finishes a two-factor sign-in with the challenge's own site and settings. The engine claims the challenge
        // before it sends anything, so this command and another finishing the same challenge cannot both send a code.
        private void CompleteChallenge()
        {
            if (Challenge.IsCompleted)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "This challenge already finished a sign-in. Sign in again with Connect-FilesAccount -Credential.");
            }
            if (!ShouldProcess(Challenge.BaseUrl, $"Finish signing in as {Challenge.Username} and create a session"))
            {
                return;
            }
            Use(RunWithoutConnection(Challenge.Settings, token => ConnectionBridge.CompleteSignInAsync(Challenge, OneTimePassword, token)));
        }

        // The -Proxy address, which must be an absolute http:// or https:// address, used with no handler of the caller's.
        private Uri ProxyAddress()
        {
            if (HttpMessageHandler != null && (Proxy != null || ProxyCredential != null))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-Proxy and -ProxyCredential cannot be used with -HttpMessageHandler; configure the handler's own proxy instead.");
            }
            if (Proxy == null)
            {
                if (ProxyCredential != null)
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, "-ProxyCredential needs -Proxy.");
                }
                return null;
            }
            if (!Proxy.IsAbsoluteUri || (Proxy.Scheme != Uri.UriSchemeHttp && Proxy.Scheme != Uri.UriSchemeHttps) || !string.IsNullOrEmpty(Proxy.UserInfo))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-Proxy must be an absolute http:// or https:// address without credentials, such as http://proxy.example.com:8080. Give its credential with -ProxyCredential.");
            }
            return Proxy;
        }

        private void Use(FilesConnection connection)
        {
            RunspaceConnections connections = RunspaceConnections.Current;
            connections.Add(connection);
            if (!NoDefault.IsPresent)
            {
                connections.Default = connection;
            }
            WriteObject(connection);
        }

        // The site's absolute address. Credentials, a query or a fragment in it are refused, since the address is
        // shown in messages and connection objects.
        private Uri SiteAddress()
        {
            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out Uri site) || (site.Scheme != Uri.UriSchemeHttps && site.Scheme != Uri.UriSchemeHttp))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"-BaseUrl must be an absolute https:// address, such as https://mysite.files.com.");
            }
            if (!string.IsNullOrEmpty(site.UserInfo) || !string.IsNullOrEmpty(site.Query) || !string.IsNullOrEmpty(site.Fragment))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-BaseUrl must not contain credentials, a query or a fragment.");
            }
            if (site.Scheme == Uri.UriSchemeHttp && !site.IsLoopback)
            {
                WriteWarning($"{site.Host} is reached over http://, so credentials and data travel unencrypted. Use https:// unless this is a test server.");
            }
            return site;
        }
    }
}
