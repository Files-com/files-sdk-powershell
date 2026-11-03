using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Event Subscriptions, or get one event subscription by Id.
    /// </summary>
    [Cmdlet("Get", "FilesEventSubscription", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.EventSubscription), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.EventSubscription>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.EventSubscription), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesEventSubscriptionCommand : FilesListCmdlet<Models.EventSubscription>
    {
        /// <summary>
        /// Used for pagination.  When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`.  Send one of those cursor value here to resume an existing list from the next available record.  Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Cursor { get; set; }

        /// <summary>
        /// Number of records to show per page.  (Max: 10000, 1,000 or less is recommended).
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PerPage { get; set; }

        /// <summary>
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `name`, `enabled`, `event_channel_id` or `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `enabled`, `event_channel_id` or `workspace_id`. Valid field combinations are `[ enabled, event_channel_id ]`, `[ workspace_id, enabled ]`, `[ workspace_id, event_channel_id ]` or `[ workspace_id, enabled, event_channel_id ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Event Subscription ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Event Subscriptions, or get one event subscription by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.EventSubscription result = Run(connection, token => EventSubscriptionBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => EventSubscriptionBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Cursor)))
            {
                parameters["cursor"] = Cursor;
            }
            if (IsBound(nameof(PerPage)))
            {
                parameters["per_page"] = PerPage.Value;
            }
            if (IsBound(nameof(SortBy)))
            {
                parameters["sort_by"] = RequestValues.QueryObject(SortBy, "SortBy");
            }
            if (IsBound(nameof(Filter)))
            {
                parameters["filter"] = RequestValues.QueryObject(Filter, "Filter");
            }
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Event Subscription.
    /// </summary>
    [Cmdlet("New", "FilesEventSubscription", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.EventSubscription))]
    public sealed class NewFilesEventSubscriptionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Event Channel ID
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? EventChannelId { get; set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace or site-wide.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// If true, this default-workspace subscription applies to events from all workspaces.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? ApplyToAllWorkspaces { get; set; }

        /// <summary>
        /// Event Subscription name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Custom subject line to use for notification emails.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Subject { get; set; }

        /// <summary>
        /// Custom message to include in notification emails.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Message { get; set; }

        /// <summary>
        /// If true, notification email bodies contain only the custom message, omitting event details and the review button. Requires a custom message, defaults to false, and does not affect non-email targets.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? MessageOnly { get; set; }

        /// <summary>
        /// Whether this Event Subscription can dispatch events.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Event type strings matched by this subscription. Blank means all event types. Valid values: `automation_run.canceled.v1`, `automation_run.failure.v1`, `automation_run.failure_will_retry.v1`, `automation_run.partial_failure.v1`, `automation_run.partial_failure_will_retry.v1`, `automation_run.skipped.v1`, `automation_run.success.v1`, `expectation_evaluation.invalid.v1`, `expectation_evaluation.late.v1`, `expectation_evaluation.missing.v1`, `expectation_evaluation.success.v1`, `expectation_incident.acknowledged.v1`, `expectation_incident.open.v1`, `expectation_incident.resolved.v1`, `expectation_incident.snoozed.v1`, `external_event.client_log.failure.v1`, `external_event.client_log.partial_failure.v1`, `external_event.client_log.skipped.v1`, `external_event.client_log.success.v1`, `malware_scan_log.quarantined.v1`, `pending_work_event.failure.v1`, `pending_work_event.partial_failure.v1`, `pending_work_event.skipped.v1`, `pending_work_event.success.v1`, `siem_http_destination_event.failure.v1`, `siem_http_destination_event.partial_failure.v1`, `siem_http_destination_event.skipped.v1`, `siem_http_destination_event.success.v1`, `sso_event.ldap_login.failure.v1`, `sso_event.ldap_login.partial_failure.v1`, `sso_event.ldap_login.skipped.v1`, `sso_event.ldap_login.success.v1`, `sso_event.ldap_sync.failure.v1`, `sso_event.ldap_sync.partial_failure.v1`, `sso_event.ldap_sync.skipped.v1`, `sso_event.ldap_sync.success.v1`, `sso_event.saml_login.failure.v1`, `sso_event.saml_login.partial_failure.v1`, `sso_event.saml_login.skipped.v1`, `sso_event.saml_login.success.v1`, `sync_run.failure.v1`, `sync_run.partial_failure.v1`, `sync_run.skipped.v1`, `sync_run.success.v1`, `user_security_event.lockout.v1`
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] EventTypes { get; set; }

        /// <summary>
        /// Conditions on event fields that an event must meet to be delivered, keyed by dot-notation field path. All conditions must match.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Event Subscription delivery policy. Overrides retry settings from each Event Target's delivery policy.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary DeliveryPolicy { get; set; }

        /// <summary>
        /// Event Target IDs this subscription sends to.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] EventTargetIds { get; set; }

        /// <summary>Create Event Subscription.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Event subscription '{Name}'" : "Event subscription"))
            {
                return;
            }
            Models.EventSubscription result = Run(connection, token => EventSubscriptionBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(EventChannelId)))
            {
                parameters["event_channel_id"] = EventChannelId.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(ApplyToAllWorkspaces)))
            {
                parameters["apply_to_all_workspaces"] = ApplyToAllWorkspaces.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Subject)))
            {
                parameters["subject"] = Subject;
            }
            if (IsBound(nameof(Message)))
            {
                parameters["message"] = Message;
            }
            if (IsBound(nameof(MessageOnly)))
            {
                parameters["message_only"] = MessageOnly.Value;
            }
            if (IsBound(nameof(Enabled)))
            {
                parameters["enabled"] = Enabled.Value;
            }
            if (IsBound(nameof(EventTypes)))
            {
                parameters["event_types"] = RequestValues.Strings(EventTypes, "EventTypes");
            }
            if (IsBound(nameof(Filter)))
            {
                parameters["filter"] = RequestValues.Object(Filter, "Filter");
            }
            if (IsBound(nameof(DeliveryPolicy)))
            {
                parameters["delivery_policy"] = RequestValues.Object(DeliveryPolicy, "DeliveryPolicy");
            }
            if (IsBound(nameof(EventTargetIds)))
            {
                parameters["event_target_ids"] = RequestValues.Int64s(EventTargetIds);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Event Subscription.
    /// </summary>
    [Cmdlet("Set", "FilesEventSubscription", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.EventSubscription), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesEventSubscriptionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Event Subscription ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Event Channel ID
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? EventChannelId { get; set; }

        /// <summary>
        /// Workspace ID. 0 means the default workspace or site-wide.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// If true, this default-workspace subscription applies to events from all workspaces.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? ApplyToAllWorkspaces { get; set; }

        /// <summary>
        /// Event Subscription name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Custom subject line to use for notification emails.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Subject { get; set; }

        /// <summary>
        /// Custom message to include in notification emails.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Message { get; set; }

        /// <summary>
        /// If true, notification email bodies contain only the custom message, omitting event details and the review button. Requires a custom message, defaults to false, and does not affect non-email targets.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? MessageOnly { get; set; }

        /// <summary>
        /// Whether this Event Subscription can dispatch events.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Event type strings matched by this subscription. Blank means all event types. Valid values: `automation_run.canceled.v1`, `automation_run.failure.v1`, `automation_run.failure_will_retry.v1`, `automation_run.partial_failure.v1`, `automation_run.partial_failure_will_retry.v1`, `automation_run.skipped.v1`, `automation_run.success.v1`, `expectation_evaluation.invalid.v1`, `expectation_evaluation.late.v1`, `expectation_evaluation.missing.v1`, `expectation_evaluation.success.v1`, `expectation_incident.acknowledged.v1`, `expectation_incident.open.v1`, `expectation_incident.resolved.v1`, `expectation_incident.snoozed.v1`, `external_event.client_log.failure.v1`, `external_event.client_log.partial_failure.v1`, `external_event.client_log.skipped.v1`, `external_event.client_log.success.v1`, `malware_scan_log.quarantined.v1`, `pending_work_event.failure.v1`, `pending_work_event.partial_failure.v1`, `pending_work_event.skipped.v1`, `pending_work_event.success.v1`, `siem_http_destination_event.failure.v1`, `siem_http_destination_event.partial_failure.v1`, `siem_http_destination_event.skipped.v1`, `siem_http_destination_event.success.v1`, `sso_event.ldap_login.failure.v1`, `sso_event.ldap_login.partial_failure.v1`, `sso_event.ldap_login.skipped.v1`, `sso_event.ldap_login.success.v1`, `sso_event.ldap_sync.failure.v1`, `sso_event.ldap_sync.partial_failure.v1`, `sso_event.ldap_sync.skipped.v1`, `sso_event.ldap_sync.success.v1`, `sso_event.saml_login.failure.v1`, `sso_event.saml_login.partial_failure.v1`, `sso_event.saml_login.skipped.v1`, `sso_event.saml_login.success.v1`, `sync_run.failure.v1`, `sync_run.partial_failure.v1`, `sync_run.skipped.v1`, `sync_run.success.v1`, `user_security_event.lockout.v1`
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] EventTypes { get; set; }

        /// <summary>
        /// Conditions on event fields that an event must meet to be delivered, keyed by dot-notation field path. All conditions must match.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Event Subscription delivery policy. Overrides retry settings from each Event Target's delivery policy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary DeliveryPolicy { get; set; }

        /// <summary>
        /// Event Target IDs this subscription sends to.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] EventTargetIds { get; set; }

        /// <summary>
        /// The event subscription to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.EventSubscription InputObject { get; set; }

        /// <summary>Update Event Subscription.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Event subscription {id}"))
            {
                return;
            }
            Models.EventSubscription result = Run(connection, token => EventSubscriptionBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(EventChannelId)))
            {
                parameters["event_channel_id"] = EventChannelId.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(ApplyToAllWorkspaces)))
            {
                parameters["apply_to_all_workspaces"] = ApplyToAllWorkspaces.Value;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Subject)))
            {
                parameters["subject"] = Subject;
            }
            if (IsBound(nameof(Message)))
            {
                parameters["message"] = Message;
            }
            if (IsBound(nameof(MessageOnly)))
            {
                parameters["message_only"] = MessageOnly.Value;
            }
            if (IsBound(nameof(Enabled)))
            {
                parameters["enabled"] = Enabled.Value;
            }
            if (IsBound(nameof(EventTypes)))
            {
                parameters["event_types"] = RequestValues.Strings(EventTypes, "EventTypes");
            }
            if (IsBound(nameof(Filter)))
            {
                parameters["filter"] = RequestValues.Object(Filter, "Filter");
            }
            if (IsBound(nameof(DeliveryPolicy)))
            {
                parameters["delivery_policy"] = RequestValues.Object(DeliveryPolicy, "DeliveryPolicy");
            }
            if (IsBound(nameof(EventTargetIds)))
            {
                parameters["event_target_ids"] = RequestValues.Int64s(EventTargetIds);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Event Subscription.
    /// </summary>
    [Cmdlet("Remove", "FilesEventSubscription", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesEventSubscriptionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Event Subscription ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The event subscription to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.EventSubscription InputObject { get; set; }

        /// <summary>Delete Event Subscription.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Event subscription {id}"))
            {
                return;
            }
            Run(connection, token => EventSubscriptionBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
