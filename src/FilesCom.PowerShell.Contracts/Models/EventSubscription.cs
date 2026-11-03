using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An EventSubscription selects EventRecords for an EventChannel and sends them to one or more EventTargets.
    /// </summary>
    public sealed class EventSubscription : FilesObject
    {
        internal EventSubscription()
        {
        }

        /// <summary>
        /// Event Subscription ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Event Channel ID
        /// </summary>
        public long? EventChannelId { get; internal set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace or site-wide.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// If true, this default-workspace subscription applies to events from all workspaces.
        /// </summary>
        public bool ApplyToAllWorkspaces { get; internal set; }

        /// <summary>
        /// Event Subscription name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Custom subject line to use for notification emails.
        /// </summary>
        public string Subject { get; internal set; }

        /// <summary>
        /// Custom message to include in notification emails.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// If true, notification email bodies contain only the custom message, omitting event details and the review button. Requires a custom message, defaults to false, and does not affect non-email targets.
        /// </summary>
        public bool MessageOnly { get; internal set; }

        /// <summary>
        /// Whether this Event Subscription can dispatch events.
        /// </summary>
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Event type strings matched by this subscription. Blank means all event types. Valid values: `automation_run.canceled.v1`, `automation_run.failure.v1`, `automation_run.failure_will_retry.v1`, `automation_run.partial_failure.v1`, `automation_run.partial_failure_will_retry.v1`, `automation_run.skipped.v1`, `automation_run.success.v1`, `expectation_evaluation.invalid.v1`, `expectation_evaluation.late.v1`, `expectation_evaluation.missing.v1`, `expectation_evaluation.success.v1`, `expectation_incident.acknowledged.v1`, `expectation_incident.open.v1`, `expectation_incident.resolved.v1`, `expectation_incident.snoozed.v1`, `external_event.client_log.failure.v1`, `external_event.client_log.partial_failure.v1`, `external_event.client_log.skipped.v1`, `external_event.client_log.success.v1`, `malware_scan_log.quarantined.v1`, `pending_work_event.failure.v1`, `pending_work_event.partial_failure.v1`, `pending_work_event.skipped.v1`, `pending_work_event.success.v1`, `siem_http_destination_event.failure.v1`, `siem_http_destination_event.partial_failure.v1`, `siem_http_destination_event.skipped.v1`, `siem_http_destination_event.success.v1`, `sso_event.ldap_login.failure.v1`, `sso_event.ldap_login.partial_failure.v1`, `sso_event.ldap_login.skipped.v1`, `sso_event.ldap_login.success.v1`, `sso_event.ldap_sync.failure.v1`, `sso_event.ldap_sync.partial_failure.v1`, `sso_event.ldap_sync.skipped.v1`, `sso_event.ldap_sync.success.v1`, `sso_event.saml_login.failure.v1`, `sso_event.saml_login.partial_failure.v1`, `sso_event.saml_login.skipped.v1`, `sso_event.saml_login.success.v1`, `sync_run.failure.v1`, `sync_run.partial_failure.v1`, `sync_run.skipped.v1`, `sync_run.success.v1`, `user_security_event.lockout.v1`
        /// </summary>
        public string[] EventTypes { get; internal set; }

        /// <summary>
        /// Conditions on event fields that an event must meet to be delivered, keyed by dot-notation field path. All conditions must match.
        /// </summary>
        public object Filter { get; internal set; }

        /// <summary>
        /// Event Subscription delivery policy. Overrides retry settings from each Event Target's delivery policy.
        /// </summary>
        public object DeliveryPolicy { get; internal set; }

        /// <summary>
        /// Event Target IDs this subscription sends to.
        /// </summary>
        public long?[] EventTargetIds { get; internal set; }

        /// <summary>
        /// Event Subscription create date/time.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Event Subscription update date/time.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(EventSubscription source)
        {
            Id = source.Id;
            EventChannelId = source.EventChannelId;
            WorkspaceId = source.WorkspaceId;
            ApplyToAllWorkspaces = source.ApplyToAllWorkspaces;
            Name = source.Name;
            Subject = source.Subject;
            Message = source.Message;
            MessageOnly = source.MessageOnly;
            Enabled = source.Enabled;
            EventTypes = source.EventTypes;
            Filter = source.Filter;
            DeliveryPolicy = source.DeliveryPolicy;
            EventTargetIds = source.EventTargetIds;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
