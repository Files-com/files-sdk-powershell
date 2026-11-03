using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An SsoEvent is a log record for SSO-related activity such as LDAP syncs and SSO login attempts.
    /// </summary>
    public sealed class SsoEvent : FilesObject
    {
        internal SsoEvent()
        {
        }

        /// <summary>
        /// Event ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Type of SSO event being recorded.
        /// </summary>
        public string EventType { get; internal set; }

        /// <summary>
        /// Status of event.
        /// </summary>
        public string Status { get; internal set; }

        /// <summary>
        /// Event body.
        /// </summary>
        public string Body { get; internal set; }

        /// <summary>
        /// Event errors.
        /// </summary>
        public string[] EventErrors { get; internal set; }

        /// <summary>
        /// Event create date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Link to log file.
        /// </summary>
        public string BodyUrl { get; internal set; }

        /// <summary>
        /// User ID.
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Username on Files.com for the SSO login attempt.
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Identity Provider UID for the SSO login attempt.
        /// </summary>
        public string IdpUid { get; internal set; }

        /// <summary>
        /// SSO provider for the SSO login attempt.
        /// </summary>
        public string Provider { get; internal set; }

        /// <summary>
        /// SSO provider label for the SSO login attempt.
        /// </summary>
        public string ProviderLabel { get; internal set; }

        /// <summary>
        /// IP address for the SSO login attempt.
        /// </summary>
        public string Ip { get; internal set; }

        /// <summary>
        /// Region for the SSO login attempt.
        /// </summary>
        public string Region { get; internal set; }
    }
}
