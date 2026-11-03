using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A RemoteServerConfigurationFile is a configuration file (required for some Remote Server integrations, such as the Files.com Agent).
    /// </summary>
    public sealed class RemoteServerConfigurationFile : FilesObject
    {
        internal RemoteServerConfigurationFile()
        {
        }

        /// <summary>
        /// The remote server ID of the agent
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// The permission set for the agent ['read_write', 'read_only', 'write_only']
        /// </summary>
        public string PermissionSet { get; internal set; }

        /// <summary>
        /// The private key for the agent
        /// </summary>
        public FilesSecret PrivateKey { get; internal set; }

        /// <summary>
        /// Files.com subdomain site name
        /// </summary>
        public string Subdomain { get; internal set; }

        /// <summary>
        /// The root directory for the agent
        /// </summary>
        public string Root { get; internal set; }

        /// <summary>
        /// Optional customer-facing name for this Agent installation
        /// </summary>
        public string NodeName { get; internal set; }

        /// <summary>
        /// Follow symlinks when traversing directories
        /// </summary>
        public bool FollowLinks { get; internal set; }

        /// <summary>
        /// Preferred network protocol ['udp', 'tcp'] (default udp)
        /// </summary>
        public string PreferProtocol { get; internal set; }

        /// <summary>
        /// DNS lookup method ['auto','doh','system'] (default auto)
        /// </summary>
        public string Dns { get; internal set; }

        /// <summary>
        /// Proxy all outbound traffic through files.com proxy server
        /// </summary>
        public bool ProxyAllOutbound { get; internal set; }

        /// <summary>
        /// Custom site endpoint URL
        /// </summary>
        public string EndpointOverride { get; internal set; }

        /// <summary>
        /// Log file name and location
        /// </summary>
        public string LogFile { get; internal set; }

        /// <summary>
        /// Log level for the agent logs ['debug', 'info', 'warn', 'error', 'fatal'] (default info)
        /// </summary>
        public string LogLevel { get; internal set; }

        /// <summary>
        /// Log route for agent logs. (default 5)
        /// </summary>
        public long? LogRotateNum { get; internal set; }

        /// <summary>
        /// Log route size in MB for agent logs. (default 20)
        /// </summary>
        public long? LogRotateSize { get; internal set; }

        /// <summary>
        /// Maximum number of concurrent jobs (default 500)
        /// </summary>
        public long? OverrideMaxConcurrentJobs { get; internal set; }

        /// <summary>
        /// Graceful shutdown timeout in seconds (default 15)
        /// </summary>
        public long? GracefulShutdownTimeout { get; internal set; }

        /// <summary>
        /// File transfer (upload/download) rate limit
        ///  `&lt;limit&gt;-&lt;period&gt;`, with the given periods:
        /// * 'S': second
        /// * 'M': minute
        /// * 'H': hour
        /// * 'D': day
        /// Examples:
        /// * 5 requests/second: '5-S'
        /// * 10 requests/minute: '10-M'
        /// * 1000 requests/hour: '1000-H'
        /// * 2000 requests/day: '2000-D'
        /// </summary>
        public string TransferRateLimit { get; internal set; }

        /// <summary>
        /// Auto update policy ['manual_trigger', 'critical_only', 'always', 'never'] (default always)
        /// </summary>
        public string AutoUpdatePolicy { get; internal set; }
    }
}
