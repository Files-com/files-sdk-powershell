using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Makes the SDK client behind each connection, always with FilesClient.Create and settings built here: never the
    // default client, the application configuration file or environment variables. Each client is disposed when its
    // connection closes; a client made only to sign in is disposed once the sign-in ends.
    internal static class ConnectionBridge
    {
        internal static FilesConnection ConnectWithApiKey(FilesConnectionSettings settings, string apiKey)
        {
            FilesConfiguration configuration = Configuration(settings);
            configuration.ApiKey = apiKey;
            return new FilesConnection(settings, FilesAuthenticationMethod.ApiKey, null, FilesClient.Create(configuration, Transport(settings)));
        }

        internal static FilesConnection ConnectWithSession(FilesConnectionSettings settings, string sessionId)
        {
            FilesConfiguration configuration = Configuration(settings);
            configuration.SessionId = sessionId;
            return new FilesConnection(settings, FilesAuthenticationMethod.Session, null, FilesClient.Create(configuration, Transport(settings)));
        }

        // Signs in, creating a session the connection owns. The one-time password is sent with the username and
        // password when given. When Files.com needs a second factor it answers with a partly completed login, which the
        // failure carries as a FilesSignInChallenge for CompleteSignInAsync.
        internal static Task<FilesConnection> SignInAsync(FilesConnectionSettings settings, string username, string password, string oneTimePassword, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object> { ["username"] = username, ["password"] = password };
            if (oneTimePassword != null)
            {
                parameters["otp"] = oneTimePassword;
            }
            return CreateSessionAsync(settings, username, parameters, cancellationToken);
        }

        // Finishes a sign-in that needed a second factor: the partly completed login and the user's code create the
        // session, which the new connection owns. The challenge is claimed before anything is sent, so a second
        // command finishing it at the same time sends nothing and cannot create a second session. The claim is kept
        // once the session exists, and given back when none was created, so the user can try another code.
        internal static async Task<FilesConnection> CompleteSignInAsync(FilesSignInChallenge challenge, string oneTimePassword, CancellationToken cancellationToken)
        {
            if (!challenge.TryBegin())
            {
                throw new FilesException(
                    FilesErrorKind.InvalidArgument,
                    challenge.IsCompleted
                        ? "This challenge already finished a sign-in. Sign in again with Connect-FilesAccount -Credential."
                        : "Another command is finishing a sign-in with this challenge now. Wait for it to finish.");
            }
            var parameters = new Dictionary<string, object>
            {
                ["username"] = challenge.Username,
                ["partial_session_id"] = challenge.PartialSessionId,
                ["otp"] = oneTimePassword,
            };
            FilesConnection connection;
            try
            {
                connection = await CreateSessionAsync(challenge.Settings, challenge.Username, parameters, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                challenge.Release();
                throw;
            }
            challenge.Complete();
            return connection;
        }

        // The connection's client is made with the settings and transport the sign-in's client was already made with,
        // so nothing between the session's creation and the connection that owns it can fail or be cancelled, and a
        // session created here is never left without its connection.
        private static async Task<FilesConnection> CreateSessionAsync(FilesConnectionSettings settings, string username, Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            HttpTransport transport = Transport(settings);
            Sdk.Session session;
            using (FilesClient signIn = FilesClient.Create(Configuration(settings), transport))
            {
                try
                {
                    session = await signIn.Sessions.CreateAsync(parameters, null, cancellationToken).ConfigureAwait(false);
                }
                catch (ApiException failure) when (PartialSessionId(failure) != null)
                {
                    throw ErrorTranslation.FromApi(failure, new FilesSignInChallenge(settings, username, PartialSessionId(failure), SecondFactors(failure)));
                }
                catch (SdkException failure)
                {
                    throw ErrorTranslation.Translate(failure);
                }
            }
            FilesConfiguration configuration = Configuration(settings);
            configuration.SessionId = session.Id;
            return new FilesConnection(settings, FilesAuthenticationMethod.Credential, username, FilesClient.Create(configuration, transport));
        }

        // The partly completed login a sign-in failure's data holds, when Files.com needs another factor.
        private static string PartialSessionId(ApiException failure)
        {
            return failure.data != null && failure.data.TryGetValue("partial_session_id", out object value) ? Text(value) : null;
        }

        private static IReadOnlyList<string> SecondFactors(ApiException failure)
        {
            var methods = new List<string>();
            if (failure.data != null && failure.data.TryGetValue("two_factor_authentication_methods", out object value))
            {
                if (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    methods.AddRange(element.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()));
                }
                else if (value is IEnumerable<object> items)
                {
                    methods.AddRange(items.Select(Text).Where(item => item != null));
                }
            }
            return methods.AsReadOnly();
        }

        private static string Text(object value)
        {
            if (value is JsonElement element)
            {
                return element.ValueKind == JsonValueKind.String ? element.GetString() : null;
            }
            return value as string;
        }

        // Revokes the session a credential connection created: the request deletes the session it is sent with.
        internal static Task RevokeOwnedSessionAsync(FilesConnection connection, CancellationToken cancellationToken)
        {
            return EngineCall.RunAsync(connection, FilesCallOptions.None, cancellationToken, (client, options, token) => client.Sessions.DeleteAsync(null, options, token));
        }

        // Closes the connection: its client refuses new operations, cancels those it is running, and completes once the
        // engine has finished with what they use (their requests, responses, temporary files and the streams they own)
        // and has closed its network connections. It does not wait for the operations' own tasks, which each command
        // awaits and settles itself, and a stream a command borrowed stays open and its caller's. Completes at once when
        // the connection was already closed.
        internal static Task CloseAsync(FilesConnection connection)
        {
            return connection.Close() is FilesClient client ? client.DisposeAsync().AsTask() : Task.CompletedTask;
        }

        // The handlers of a connection's clients: the caller's shared handler, which no client disposes; handlers
        // each client makes with the proxy and owns; or, with neither, null for the engine's own.
        private static HttpTransport Transport(FilesConnectionSettings settings)
        {
            if (settings.SharedHandler != null)
            {
                try
                {
                    return HttpTransport.FromSharedHandler(settings.SharedHandler);
                }
                // The engine refuses a handler it cannot use with its own usage errors, which the module reports as
                // invalid arguments wherever they come from (ErrorTranslation), or with an ArgumentException.
                catch (Exception failure) when (failure is InvalidParameterException || failure is MissingParameterException || failure is ArgumentException)
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-HttpMessageHandler cannot be used: {failure.Message}");
                }
            }
            if (settings.Proxy != null)
            {
                Uri proxy = settings.Proxy;
                ICredentials credentials = settings.ProxyCredentials;
                return HttpTransport.FromHandlerFactory(() => ProxyHandler(proxy, credentials));
            }
            return null;
        }

        private static HttpMessageHandler ProxyHandler(Uri proxy, ICredentials credentials)
        {
            var webProxy = new WebProxy(proxy) { Credentials = credentials };
#if NET8_0_OR_GREATER
            return new SocketsHttpHandler { Proxy = webProxy, UseProxy = true };
#else
            return new HttpClientHandler { Proxy = webProxy, UseProxy = true };
#endif
        }

        // The module's name and version, sent before the engine's own in every request's User-Agent: the version is the
        // module's (from its _VERSION file), without build metadata.
        internal const string ProductName = "FilesCom-PowerShell";

        internal static readonly string ProductVersion = ModuleVersion();

        private static string ModuleVersion()
        {
            string version = typeof(ConnectionBridge).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .OfType<System.Reflection.AssemblyInformationalVersionAttribute>().Select(attribute => attribute.InformationalVersion).FirstOrDefault()
                ?? typeof(ConnectionBridge).Assembly.GetName().Version.ToString(3);
            int metadata = version.IndexOf('+');
            return metadata < 0 ? version : version.Substring(0, metadata);
        }

        private static FilesConfiguration Configuration(FilesConnectionSettings settings)
        {
            var configuration = new FilesConfiguration
            {
                BaseUrl = settings.BaseUrl,
                LoggerFactory = EngineLogging.Factory,
                MinimumLogLevel = EngineLogging.MinimumLevel(settings.LogLevel),
                ProductName = ProductName,
                ProductVersion = ProductVersion,
            };
            if (settings.WorkspaceId.HasValue)
            {
                configuration.WorkspaceId = settings.WorkspaceId.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (settings.Language != null)
            {
                configuration.Language = settings.Language;
            }
            if (settings.CheckDownloadModificationTime.HasValue)
            {
                configuration.CheckDownloadModificationTime = settings.CheckDownloadModificationTime.Value;
            }
            if (settings.MaxNetworkRetries.HasValue)
            {
                configuration.MaxNetworkRetries = settings.MaxNetworkRetries.Value;
            }
            if (settings.ConnectTimeoutSeconds.HasValue)
            {
                configuration.ConnectTimeout = settings.ConnectTimeoutSeconds.Value;
            }
            if (settings.ReadTimeoutSeconds.HasValue)
            {
                configuration.ReadTimeout = settings.ReadTimeoutSeconds.Value;
            }
            if (settings.WriteTimeoutSeconds.HasValue)
            {
                configuration.WriteTimeout = settings.WriteTimeoutSeconds.Value;
            }
            if (settings.InitialRetryDelaySeconds.HasValue)
            {
                configuration.InitialNetworkRequestDelay = settings.InitialRetryDelaySeconds.Value;
            }
            if (settings.MaxRetryDelaySeconds.HasValue)
            {
                configuration.MaxNetworkRetryDelay = settings.MaxRetryDelaySeconds.Value;
            }
            configuration.RegionRelocation = RegionRelocation(settings.RegionRelocation ?? FilesRegionRelocationPolicy.Automatic);
            return configuration;
        }

        private static RegionRelocationPolicy RegionRelocation(FilesRegionRelocationPolicy policy)
        {
            switch (policy.Kind)
            {
                case FilesRegionRelocation.Disabled:
                    return RegionRelocationPolicy.Disabled;
                case FilesRegionRelocation.Listed:
                    RegionRelocationPolicy listed = RegionRelocationPolicy.OnlyBetween(policy.Pairs[0].From, policy.Pairs[0].To);
                    foreach (FilesOriginPair pair in policy.Pairs.Skip(1))
                    {
                        listed = listed.OrBetween(pair.From, pair.To);
                    }
                    return listed;
                default:
                    return RegionRelocationPolicy.Automatic;
            }
        }
    }
}
