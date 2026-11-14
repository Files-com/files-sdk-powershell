using System;
using System.Net;
using System.Net.Http;

namespace FilesCom.PowerShell
{
    // What Connect-FilesAccount asks the engine to make a client with, apart from the credentials. Null values leave the
    // engine's defaults.
    internal sealed class FilesConnectionSettings
    {
        internal string BaseUrl { get; set; }

        internal long? WorkspaceId { get; set; }

        internal string Language { get; set; }

        internal int? MaxNetworkRetries { get; set; }

        internal bool? CheckDownloadModificationTime { get; set; }

        internal TimeSpan? OperationTimeout { get; set; }

        // The engine's network timeouts, in whole seconds.
        internal int? ConnectTimeoutSeconds { get; set; }

        internal int? ReadTimeoutSeconds { get; set; }

        internal int? WriteTimeoutSeconds { get; set; }

        // The engine's waits before retrying a Files.com API request, in seconds.
        internal double? InitialRetryDelaySeconds { get; set; }

        internal double? MaxRetryDelaySeconds { get; set; }

        // A handler the caller owns and shares; the engine sends every request through it and never disposes it.
        internal HttpMessageHandler SharedHandler { get; set; }

        // A proxy for handlers the engine creates and owns.
        internal Uri Proxy { get; set; }

        internal ICredentials ProxyCredentials { get; set; }

        // Whether the engine's DEBUG diagnostics, which include the text of Files.com's responses, reach the Debug
        // stream of the connection's commands.
        internal bool DiagnosticDebug { get; set; }

        // The least severe engine diagnostic the connection's client produces; null for all of them.
        internal FilesLogLevel? LogLevel { get; set; }

        // Whether commands move to the address Files.com names for the site's region; null for Automatic.
        internal FilesRegionRelocationPolicy RegionRelocation { get; set; }
    }
}
