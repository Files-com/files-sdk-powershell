using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Share Links, or get one bundle by Id.
    /// </summary>
    [Cmdlet("Get", "FilesBundle", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.Bundle), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.Bundle>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.Bundle), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesBundleCommand : FilesListCmdlet<Models.Bundle>
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `created_at`, `expires_at`, `code`, `group_id`, `user_id` or `bypasses_site_expiration_rules`. Valid field combinations are `[ group_id, expires_at ]` and `[ user_id, expires_at ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than the supplied value. Valid fields are `created_at` and `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `created_at` and `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGteq { get; set; }

        /// <summary>
        /// If set, return records where the specified field is prefixed by the supplied value. Valid fields are `code`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterPrefix { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than the supplied value. Valid fields are `created_at` and `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than or equal the supplied value. Valid fields are `created_at` and `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLteq { get; set; }

        /// <summary>
        /// If true, only list deleted Share Links.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [Parameter(ParameterSetName = "Id")]
        [ValidateNotNull]
        public bool? Deleted { get; set; }

        /// <summary>
        /// Bundle ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Share Links, or get one bundle by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.Bundle result = Run(connection, token => BundleBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => BundleBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
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
            if (IsBound(nameof(FilterGt)))
            {
                parameters["filter_gt"] = RequestValues.QueryObject(FilterGt, "FilterGt");
            }
            if (IsBound(nameof(FilterGteq)))
            {
                parameters["filter_gteq"] = RequestValues.QueryObject(FilterGteq, "FilterGteq");
            }
            if (IsBound(nameof(FilterPrefix)))
            {
                parameters["filter_prefix"] = RequestValues.QueryObject(FilterPrefix, "FilterPrefix");
            }
            if (IsBound(nameof(FilterLt)))
            {
                parameters["filter_lt"] = RequestValues.QueryObject(FilterLt, "FilterLt");
            }
            if (IsBound(nameof(FilterLteq)))
            {
                parameters["filter_lteq"] = RequestValues.QueryObject(FilterLteq, "FilterLteq");
            }
            if (IsBound(nameof(Deleted)))
            {
                parameters["deleted"] = Deleted.Value;
            }
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Deleted)))
            {
                parameters["deleted"] = Deleted.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Share Link.
    /// </summary>
    [Cmdlet("New", "FilesBundle", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Bundle))]
    public sealed class NewFilesBundleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// A list of paths to include in this bundle.
        /// </summary>
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public string[] Paths { get; set; }

        /// <summary>
        /// Password for this bundle.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Password { get; set; }

        /// <summary>
        /// If true, this Share Link bypasses site-wide expiration rules. Only site admins may set this.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? BypassesSiteExpirationRules { get; set; }

        /// <summary>
        /// Id of Form Field Set to use with this bundle
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? FormFieldSetId { get; set; }

        /// <summary>
        /// If true, create a snapshot of this bundle's contents.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? CreateSnapshot { get; set; }

        /// <summary>
        /// Do not create subfolders for files uploaded to this share. Note: there are subtle security pitfalls with allowing anonymous uploads from multiple users to live in the same folder. We strongly discourage use of this option unless absolutely required.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DontSeparateSubmissionsByFolder { get; set; }

        /// <summary>
        /// Explicit Bundle expiration date/time. If not set, the site-wide expiration setting may apply.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// If true, finalize the snapshot of this bundle's contents. Note that `create_snapshot` must also be true.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FinalizeSnapshot { get; set; }

        /// <summary>
        /// Maximum number of times bundle can be accessed
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? MaxUses { get; set; }

        /// <summary>
        /// Owning group ID. If set, members of this group can view, edit, and share this Share Link.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? GroupId { get; set; }

        /// <summary>
        /// Internal name for identifying this Share Link.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string InternalName { get; set; }

        /// <summary>
        /// Public description
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Bundle internal note
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Note { get; set; }

        /// <summary>
        /// Bundle code.  This code forms the end part of the Public URL.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Code { get; set; }

        /// <summary>
        /// Template for creating submission subfolders. Can use the uploader's name, email address, ip, company, `strftime` directives, and any custom form data.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PathTemplate { get; set; }

        /// <summary>
        /// Timezone to use when rendering timestamps in path templates.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PathTemplateTimeZone { get; set; }

        /// <summary>
        /// Permissions that apply to Folders in this Share Link.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Permissions { get; set; }

        /// <summary>
        /// Show a registration page that captures the downloader's name and email address?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RequireRegistration { get; set; }

        /// <summary>
        /// ID of the clickwrap to use with this bundle.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? ClickwrapId { get; set; }

        /// <summary>
        /// ID of the associated inbox, if available.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? InboxId { get; set; }

        /// <summary>
        /// Only allow access to recipients who have explicitly received the share via an email sent through the Files.com UI?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? RequireShareRecipient { get; set; }

        /// <summary>
        /// If true, require_share_recipient bundles will send a one-time password to the recipient when they register. Cannot be enabled if the bundle has a password set.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SendOneTimePasswordToRecipientAtRegistration { get; set; }

        /// <summary>
        /// Send delivery receipt to the uploader. Note: For writable share only
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SendEmailReceiptToUploader { get; set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing email?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SkipEmail { get; set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing name?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SkipName { get; set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing company?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? SkipCompany { get; set; }

        /// <summary>
        /// Date when share will start to be accessible. If `nil` access granted right after create.
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? StartAccessOnDate { get; set; }

        /// <summary>
        /// ID of the snapshot containing this bundle's contents.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SnapshotId { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Preview watermark image applied to all bundle items.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public FilesAttachment WatermarkAttachmentFile { get; set; }

        /// <summary>
        /// Preview watermark settings applied to all bundle items. Uses the same keys as Behavior.value
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary WatermarkValue { get; set; }

        /// <summary>Create Share Link.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Bundle"))
            {
                return;
            }
            Models.Bundle result = Run(connection, token => BundleBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Paths)))
            {
                parameters["paths"] = RequestValues.Strings(Paths, "Paths");
            }
            if (IsBound(nameof(Password)))
            {
                parameters["password"] = RequestValues.Reveal(Password);
            }
            if (IsBound(nameof(BypassesSiteExpirationRules)))
            {
                parameters["bypasses_site_expiration_rules"] = BypassesSiteExpirationRules.Value;
            }
            if (IsBound(nameof(FormFieldSetId)))
            {
                parameters["form_field_set_id"] = FormFieldSetId.Value;
            }
            if (IsBound(nameof(CreateSnapshot)))
            {
                parameters["create_snapshot"] = CreateSnapshot.Value;
            }
            if (IsBound(nameof(DontSeparateSubmissionsByFolder)))
            {
                parameters["dont_separate_submissions_by_folder"] = DontSeparateSubmissionsByFolder.Value;
            }
            if (IsBound(nameof(ExpiresAt)))
            {
                parameters["expires_at"] = RequestValues.DateTime(ExpiresAt.Value);
            }
            if (IsBound(nameof(FinalizeSnapshot)))
            {
                parameters["finalize_snapshot"] = FinalizeSnapshot.Value;
            }
            if (IsBound(nameof(MaxUses)))
            {
                parameters["max_uses"] = MaxUses.Value;
            }
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId.Value;
            }
            if (IsBound(nameof(InternalName)))
            {
                parameters["internal_name"] = InternalName;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(Note)))
            {
                parameters["note"] = Note;
            }
            if (IsBound(nameof(Code)))
            {
                parameters["code"] = Code;
            }
            if (IsBound(nameof(PathTemplate)))
            {
                parameters["path_template"] = PathTemplate;
            }
            if (IsBound(nameof(PathTemplateTimeZone)))
            {
                parameters["path_template_time_zone"] = PathTemplateTimeZone;
            }
            if (IsBound(nameof(Permissions)))
            {
                parameters["permissions"] = Permissions;
            }
            if (IsBound(nameof(RequireRegistration)))
            {
                parameters["require_registration"] = RequireRegistration.Value;
            }
            if (IsBound(nameof(ClickwrapId)))
            {
                parameters["clickwrap_id"] = ClickwrapId.Value;
            }
            if (IsBound(nameof(InboxId)))
            {
                parameters["inbox_id"] = InboxId.Value;
            }
            if (IsBound(nameof(RequireShareRecipient)))
            {
                parameters["require_share_recipient"] = RequireShareRecipient.Value;
            }
            if (IsBound(nameof(SendOneTimePasswordToRecipientAtRegistration)))
            {
                parameters["send_one_time_password_to_recipient_at_registration"] = SendOneTimePasswordToRecipientAtRegistration.Value;
            }
            if (IsBound(nameof(SendEmailReceiptToUploader)))
            {
                parameters["send_email_receipt_to_uploader"] = SendEmailReceiptToUploader.Value;
            }
            if (IsBound(nameof(SkipEmail)))
            {
                parameters["skip_email"] = SkipEmail.Value;
            }
            if (IsBound(nameof(SkipName)))
            {
                parameters["skip_name"] = SkipName.Value;
            }
            if (IsBound(nameof(SkipCompany)))
            {
                parameters["skip_company"] = SkipCompany.Value;
            }
            if (IsBound(nameof(StartAccessOnDate)))
            {
                parameters["start_access_on_date"] = RequestValues.DateTime(StartAccessOnDate.Value);
            }
            if (IsBound(nameof(SnapshotId)))
            {
                parameters["snapshot_id"] = SnapshotId.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(WatermarkAttachmentFile)))
            {
                parameters["watermark_attachment_file"] = RequestValues.Attachment(WatermarkAttachmentFile, "WatermarkAttachmentFile");
            }
            if (IsBound(nameof(WatermarkValue)))
            {
                parameters["watermark_value"] = RequestValues.Object(WatermarkValue, "WatermarkValue");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Send email(s) with a link to bundle.
    /// </summary>
    [Cmdlet("Send", "FilesBundleShare", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class SendFilesBundleShareCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Bundle ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// A list of email addresses to share this bundle with. Required unless `recipients` is used.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] To { get; set; }

        /// <summary>
        /// Note to include in email.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Note { get; set; }

        /// <summary>
        /// A list of recipients to share this bundle with. Required unless `to` is used.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary[] Recipients { get; set; }

        /// <summary>
        /// The bundle to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Bundle InputObject { get; set; }

        /// <summary>Send email(s) with a link to bundle.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = ShareParameters();
            if (!ShouldProcessOn(connection, $"Bundle {id}"))
            {
                return;
            }
            Run(connection, token => BundleBridge.ShareAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> ShareParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(To)))
            {
                parameters["to"] = RequestValues.Strings(To, "To");
            }
            if (IsBound(nameof(Note)))
            {
                parameters["note"] = Note;
            }
            if (IsBound(nameof(Recipients)))
            {
                parameters["recipients"] = RequestValues.Objects(Recipients, "Recipients");
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Share Link.
    /// </summary>
    [Cmdlet("Set", "FilesBundle", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Bundle), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesBundleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Bundle ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// A list of paths to include in this bundle.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] Paths { get; set; }

        /// <summary>
        /// Password for this bundle.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public System.Security.SecureString Password { get; set; }

        /// <summary>
        /// If true, this Share Link bypasses site-wide expiration rules. Only site admins may set this.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? BypassesSiteExpirationRules { get; set; }

        /// <summary>
        /// Id of Form Field Set to use with this bundle
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? FormFieldSetId { get; set; }

        /// <summary>
        /// ID of the clickwrap to use with this bundle.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? ClickwrapId { get; set; }

        /// <summary>
        /// Bundle code.  This code forms the end part of the Public URL.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Code { get; set; }

        /// <summary>
        /// If true, create a snapshot of this bundle's contents.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? CreateSnapshot { get; set; }

        /// <summary>
        /// Public description
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// Do not create subfolders for files uploaded to this share. Note: there are subtle security pitfalls with allowing anonymous uploads from multiple users to live in the same folder. We strongly discourage use of this option unless absolutely required.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DontSeparateSubmissionsByFolder { get; set; }

        /// <summary>
        /// Explicit Bundle expiration date/time. If not set, the site-wide expiration setting may apply.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// If true, finalize the snapshot of this bundle's contents. Note that `create_snapshot` must also be true.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? FinalizeSnapshot { get; set; }

        /// <summary>
        /// ID of the associated inbox, if available.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? InboxId { get; set; }

        /// <summary>
        /// Maximum number of times bundle can be accessed
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? MaxUses { get; set; }

        /// <summary>
        /// Owning group ID. If set, members of this group can view, edit, and share this Share Link.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? GroupId { get; set; }

        /// <summary>
        /// Internal name for identifying this Share Link.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string InternalName { get; set; }

        /// <summary>
        /// Bundle internal note
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Note { get; set; }

        /// <summary>
        /// Template for creating submission subfolders. Can use the uploader's name, email address, ip, company, `strftime` directives, and any custom form data.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PathTemplate { get; set; }

        /// <summary>
        /// Timezone to use when rendering timestamps in path templates.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PathTemplateTimeZone { get; set; }

        /// <summary>
        /// Permissions that apply to Folders in this Share Link.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Permissions { get; set; }

        /// <summary>
        /// Show a registration page that captures the downloader's name and email address?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? RequireRegistration { get; set; }

        /// <summary>
        /// Only allow access to recipients who have explicitly received the share via an email sent through the Files.com UI?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? RequireShareRecipient { get; set; }

        /// <summary>
        /// If true, require_share_recipient bundles will send a one-time password to the recipient when they register. Cannot be enabled if the bundle has a password set.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SendOneTimePasswordToRecipientAtRegistration { get; set; }

        /// <summary>
        /// Send delivery receipt to the uploader. Note: For writable share only
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SendEmailReceiptToUploader { get; set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing company?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SkipCompany { get; set; }

        /// <summary>
        /// Date when share will start to be accessible. If `nil` access granted right after create.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? StartAccessOnDate { get; set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing email?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SkipEmail { get; set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing name?
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? SkipName { get; set; }

        /// <summary>
        /// The owning user id. Only site admins can set this.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// If true, will delete the file stored in watermark_attachment
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? WatermarkAttachmentDelete { get; set; }

        /// <summary>
        /// Preview watermark image applied to all bundle items.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public FilesAttachment WatermarkAttachmentFile { get; set; }

        /// <summary>
        /// Preview watermark settings applied to all bundle items. Uses the same keys as Behavior.value
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary WatermarkValue { get; set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// The bundle to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Bundle InputObject { get; set; }

        /// <summary>Update Share Link.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Bundle {id}"))
            {
                return;
            }
            Models.Bundle result = Run(connection, token => BundleBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Paths)))
            {
                parameters["paths"] = RequestValues.Strings(Paths, "Paths");
            }
            if (IsBound(nameof(Password)))
            {
                parameters["password"] = RequestValues.Reveal(Password);
            }
            if (IsBound(nameof(BypassesSiteExpirationRules)))
            {
                parameters["bypasses_site_expiration_rules"] = BypassesSiteExpirationRules.Value;
            }
            if (IsBound(nameof(FormFieldSetId)))
            {
                parameters["form_field_set_id"] = FormFieldSetId.Value;
            }
            if (IsBound(nameof(ClickwrapId)))
            {
                parameters["clickwrap_id"] = ClickwrapId.Value;
            }
            if (IsBound(nameof(Code)))
            {
                parameters["code"] = Code;
            }
            if (IsBound(nameof(CreateSnapshot)))
            {
                parameters["create_snapshot"] = CreateSnapshot.Value;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(DontSeparateSubmissionsByFolder)))
            {
                parameters["dont_separate_submissions_by_folder"] = DontSeparateSubmissionsByFolder.Value;
            }
            if (IsBound(nameof(ExpiresAt)))
            {
                parameters["expires_at"] = RequestValues.DateTime(ExpiresAt.Value);
            }
            if (IsBound(nameof(FinalizeSnapshot)))
            {
                parameters["finalize_snapshot"] = FinalizeSnapshot.Value;
            }
            if (IsBound(nameof(InboxId)))
            {
                parameters["inbox_id"] = InboxId.Value;
            }
            if (IsBound(nameof(MaxUses)))
            {
                parameters["max_uses"] = MaxUses.Value;
            }
            if (IsBound(nameof(GroupId)))
            {
                parameters["group_id"] = GroupId.Value;
            }
            if (IsBound(nameof(InternalName)))
            {
                parameters["internal_name"] = InternalName;
            }
            if (IsBound(nameof(Note)))
            {
                parameters["note"] = Note;
            }
            if (IsBound(nameof(PathTemplate)))
            {
                parameters["path_template"] = PathTemplate;
            }
            if (IsBound(nameof(PathTemplateTimeZone)))
            {
                parameters["path_template_time_zone"] = PathTemplateTimeZone;
            }
            if (IsBound(nameof(Permissions)))
            {
                parameters["permissions"] = Permissions;
            }
            if (IsBound(nameof(RequireRegistration)))
            {
                parameters["require_registration"] = RequireRegistration.Value;
            }
            if (IsBound(nameof(RequireShareRecipient)))
            {
                parameters["require_share_recipient"] = RequireShareRecipient.Value;
            }
            if (IsBound(nameof(SendOneTimePasswordToRecipientAtRegistration)))
            {
                parameters["send_one_time_password_to_recipient_at_registration"] = SendOneTimePasswordToRecipientAtRegistration.Value;
            }
            if (IsBound(nameof(SendEmailReceiptToUploader)))
            {
                parameters["send_email_receipt_to_uploader"] = SendEmailReceiptToUploader.Value;
            }
            if (IsBound(nameof(SkipCompany)))
            {
                parameters["skip_company"] = SkipCompany.Value;
            }
            if (IsBound(nameof(StartAccessOnDate)))
            {
                parameters["start_access_on_date"] = RequestValues.DateTime(StartAccessOnDate.Value);
            }
            if (IsBound(nameof(SkipEmail)))
            {
                parameters["skip_email"] = SkipEmail.Value;
            }
            if (IsBound(nameof(SkipName)))
            {
                parameters["skip_name"] = SkipName.Value;
            }
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(WatermarkAttachmentDelete)))
            {
                parameters["watermark_attachment_delete"] = WatermarkAttachmentDelete.Value;
            }
            if (IsBound(nameof(WatermarkAttachmentFile)))
            {
                parameters["watermark_attachment_file"] = RequestValues.Attachment(WatermarkAttachmentFile, "WatermarkAttachmentFile");
            }
            if (IsBound(nameof(WatermarkValue)))
            {
                parameters["watermark_value"] = RequestValues.Object(WatermarkValue, "WatermarkValue");
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Share Link.
    /// </summary>
    [Cmdlet("Remove", "FilesBundle", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesBundleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Bundle ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The bundle to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Bundle InputObject { get; set; }

        /// <summary>Delete Share Link.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Bundle {id}"))
            {
                return;
            }
            Run(connection, token => BundleBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
