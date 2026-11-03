using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An EventRecord is a durable event emitted by Files.com for routing through Event Channels.
    /// </summary>
    public sealed class EventRecord : FilesObject
    {
        internal EventRecord()
        {
        }

        /// <summary>
        /// Event Record ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace or site-wide.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Stable event UUID.
        /// </summary>
        public string EventUuid { get; internal set; }

        /// <summary>
        /// Versioned event type string. Valid values: `automation_run.canceled.v1`, `automation_run.failure.v1`, `automation_run.failure_will_retry.v1`, `automation_run.partial_failure.v1`, `automation_run.partial_failure_will_retry.v1`, `automation_run.skipped.v1`, `automation_run.success.v1`, `expectation_evaluation.invalid.v1`, `expectation_evaluation.late.v1`, `expectation_evaluation.missing.v1`, `expectation_evaluation.success.v1`, `expectation_incident.acknowledged.v1`, `expectation_incident.open.v1`, `expectation_incident.resolved.v1`, `expectation_incident.snoozed.v1`, `external_event.client_log.failure.v1`, `external_event.client_log.partial_failure.v1`, `external_event.client_log.skipped.v1`, `external_event.client_log.success.v1`, `malware_scan_log.quarantined.v1`, `pending_work_event.failure.v1`, `pending_work_event.partial_failure.v1`, `pending_work_event.skipped.v1`, `pending_work_event.success.v1`, `siem_http_destination_event.failure.v1`, `siem_http_destination_event.partial_failure.v1`, `siem_http_destination_event.skipped.v1`, `siem_http_destination_event.success.v1`, `sso_event.ldap_login.failure.v1`, `sso_event.ldap_login.partial_failure.v1`, `sso_event.ldap_login.skipped.v1`, `sso_event.ldap_login.success.v1`, `sso_event.ldap_sync.failure.v1`, `sso_event.ldap_sync.partial_failure.v1`, `sso_event.ldap_sync.skipped.v1`, `sso_event.ldap_sync.success.v1`, `sso_event.saml_login.failure.v1`, `sso_event.saml_login.partial_failure.v1`, `sso_event.saml_login.skipped.v1`, `sso_event.saml_login.success.v1`, `sync_run.failure.v1`, `sync_run.partial_failure.v1`, `sync_run.skipped.v1`, `sync_run.success.v1`, `user_security_event.lockout.v1`
        /// </summary>
        public string EventType { get; internal set; }

        /// <summary>
        /// Event severity.
        /// </summary>
        public string Severity { get; internal set; }

        /// <summary>
        /// Source record type.
        /// </summary>
        public string SourceType { get; internal set; }

        /// <summary>
        /// Source record ID.
        /// </summary>
        public long? SourceId { get; internal set; }

        /// <summary>
        /// Event occurrence date/time.
        /// </summary>
        public DateTime? OccurredAt { get; internal set; }

        /// <summary>
        /// Human-readable event title.
        /// </summary>
        public string HumanTitle { get; internal set; }

        /// <summary>
        /// Human-readable event summary.
        /// </summary>
        public string HumanSummary { get; internal set; }

        /// <summary>
        /// Human-readable event detail fields.
        /// </summary>
        public object[] HumanFields { get; internal set; }

        /// <summary>
        /// Actor associated with the event.
        /// </summary>
        public object Actor { get; internal set; }

        /// <summary>
        /// Resources associated with the event.
        /// </summary>
        public object[] Resources { get; internal set; }

        /// <summary>
        /// Event payload.
        /// </summary>
        public object Payload { get; internal set; }

        /// <summary>
        /// Event Record create date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
