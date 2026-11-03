using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Bundle is the API/SDK term for the feature called Share Links in the web interface.
    /// </summary>
    public sealed class Bundle : FilesObject
    {
        internal Bundle()
        {
        }

        /// <summary>
        /// Bundle code.  This code forms the end part of the Public URL.
        /// </summary>
        public string Code { get; internal set; }

        /// <summary>
        /// Page link and button color
        /// </summary>
        public string ColorLeft { get; internal set; }

        /// <summary>
        /// Top bar link color
        /// </summary>
        public string ColorLink { get; internal set; }

        /// <summary>
        /// Page link and button color
        /// </summary>
        public string ColorText { get; internal set; }

        /// <summary>
        /// Top bar background color
        /// </summary>
        public string ColorTop { get; internal set; }

        /// <summary>
        /// Top bar text color
        /// </summary>
        public string ColorTopText { get; internal set; }

        /// <summary>
        /// Public URL of Share Link
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Public description
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Explicit Bundle expiration date/time. If not set, the site-wide expiration setting may apply.
        /// </summary>
        public DateTime? ExpiresAt { get; internal set; }

        /// <summary>
        /// Is this bundle password protected?
        /// </summary>
        public bool PasswordProtected { get; internal set; }

        /// <summary>
        /// Permissions that apply to Folders in this Share Link.
        /// </summary>
        public string Permissions { get; internal set; }

        /// <summary>
        /// PreviewOnly
        /// </summary>
        public bool PreviewOnly { get; internal set; }

        /// <summary>
        /// Show a registration page that captures the downloader's name and email address?
        /// </summary>
        public bool RequireRegistration { get; internal set; }

        /// <summary>
        /// Only allow access to recipients who have explicitly received the share via an email sent through the Files.com UI?
        /// </summary>
        public bool RequireShareRecipient { get; internal set; }

        /// <summary>
        /// If true, we will hide the 'Remember Me' box on the Bundle registration page, requiring that the user logout and log back in every time they visit the page.
        /// </summary>
        public bool RequireLogout { get; internal set; }

        /// <summary>
        /// Legal text that must be agreed to prior to accessing Bundle.
        /// </summary>
        public string ClickwrapBody { get; internal set; }

        /// <summary>
        /// Custom Form to use
        /// </summary>
        public Models.FormFieldSet FormFieldSet { get; internal set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing name?
        /// </summary>
        public bool SkipName { get; internal set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing email?
        /// </summary>
        public bool SkipEmail { get; internal set; }

        /// <summary>
        /// Date when share will start to be accessible. If `nil` access granted right after create.
        /// </summary>
        public DateTime? StartAccessOnDate { get; internal set; }

        /// <summary>
        /// BundleRegistrations can be saved without providing company?
        /// </summary>
        public bool SkipCompany { get; internal set; }

        /// <summary>
        /// Bundle ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// If true, this Share Link bypasses site-wide expiration rules. Only site admins may set this.
        /// </summary>
        public bool BypassesSiteExpirationRules { get; internal set; }

        /// <summary>
        /// Bundle created at date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Indicates if the bundle has been deleted.
        /// </summary>
        public bool Deleted { get; internal set; }

        /// <summary>
        /// Bundle deleted at date/time
        /// </summary>
        public DateTime? DeletedAt { get; internal set; }

        /// <summary>
        /// Do not create subfolders for files uploaded to this share. Note: there are subtle security pitfalls with allowing anonymous uploads from multiple users to live in the same folder. We strongly discourage use of this option unless absolutely required.
        /// </summary>
        public bool DontSeparateSubmissionsByFolder { get; internal set; }

        /// <summary>
        /// Read-only expiration date/time, using the explicit expiration or the site-wide setting when applicable. Null when the Share Link does not expire.
        /// </summary>
        public DateTime? EffectiveExpiresAt { get; internal set; }

        /// <summary>
        /// Maximum number of times bundle can be accessed
        /// </summary>
        public long? MaxUses { get; internal set; }

        /// <summary>
        /// Internal name for identifying this Share Link.
        /// </summary>
        public string InternalName { get; internal set; }

        /// <summary>
        /// Bundle internal note
        /// </summary>
        public string Note { get; internal set; }

        /// <summary>
        /// Template for creating submission subfolders. Can use the uploader's name, email address, ip, company, `strftime` directives, and any custom form data.
        /// </summary>
        public string PathTemplate { get; internal set; }

        /// <summary>
        /// Timezone to use when rendering timestamps in path templates.
        /// </summary>
        public string PathTemplateTimeZone { get; internal set; }

        /// <summary>
        /// Send delivery receipt to the uploader. Note: For writable share only
        /// </summary>
        public bool SendEmailReceiptToUploader { get; internal set; }

        /// <summary>
        /// ID of the snapshot containing this bundle's contents.
        /// </summary>
        public long? SnapshotId { get; internal set; }

        /// <summary>
        /// Bundle creator user ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Bundle creator username
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// Owning group ID. If set, members of this group can view, edit, and share this Share Link.
        /// </summary>
        public long? GroupId { get; internal set; }

        /// <summary>
        /// ID of the clickwrap to use with this bundle.
        /// </summary>
        public long? ClickwrapId { get; internal set; }

        /// <summary>
        /// ID of the associated inbox, if available.
        /// </summary>
        public long? InboxId { get; internal set; }

        /// <summary>
        /// Preview watermark image applied to all bundle items.
        /// </summary>
        public Models.Image WatermarkAttachment { get; internal set; }

        /// <summary>
        /// Preview watermark settings applied to all bundle items. Uses the same keys as Behavior.value
        /// </summary>
        public object WatermarkValue { get; internal set; }

        /// <summary>
        /// If true, require_share_recipient bundles will send a one-time password to the recipient when they register. Cannot be enabled if the bundle has a password set.
        /// </summary>
        public bool SendOneTimePasswordToRecipientAtRegistration { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Does this bundle have an associated inbox?
        /// </summary>
        public bool HasInbox { get; internal set; }

        /// <summary>
        /// Should folder uploads be prevented?
        /// </summary>
        public bool DontAllowFoldersInUploads { get; internal set; }

        /// <summary>
        /// Upload slots requested by the associated Inbox. Each slot contains a name used as its label and destination subfolder name.
        /// </summary>
        public object[] RequestedUploadSlots { get; internal set; }

        /// <summary>
        /// A list of paths in this bundle.  For performance reasons, this is not provided when listing bundles.
        /// </summary>
        public string[] Paths { get; internal set; }

        /// <summary>
        /// A list of bundlepaths in this bundle.  For performance reasons, this is not provided when listing bundles.
        /// </summary>
        public Models.BundlePath[] Bundlepaths { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Bundle source)
        {
            Code = source.Code;
            ColorLeft = source.ColorLeft;
            ColorLink = source.ColorLink;
            ColorText = source.ColorText;
            ColorTop = source.ColorTop;
            ColorTopText = source.ColorTopText;
            Url = source.Url;
            Description = source.Description;
            ExpiresAt = source.ExpiresAt;
            PasswordProtected = source.PasswordProtected;
            Permissions = source.Permissions;
            PreviewOnly = source.PreviewOnly;
            RequireRegistration = source.RequireRegistration;
            RequireShareRecipient = source.RequireShareRecipient;
            RequireLogout = source.RequireLogout;
            ClickwrapBody = source.ClickwrapBody;
            FormFieldSet = source.FormFieldSet;
            SkipName = source.SkipName;
            SkipEmail = source.SkipEmail;
            StartAccessOnDate = source.StartAccessOnDate;
            SkipCompany = source.SkipCompany;
            Id = source.Id;
            BypassesSiteExpirationRules = source.BypassesSiteExpirationRules;
            CreatedAt = source.CreatedAt;
            Deleted = source.Deleted;
            DeletedAt = source.DeletedAt;
            DontSeparateSubmissionsByFolder = source.DontSeparateSubmissionsByFolder;
            EffectiveExpiresAt = source.EffectiveExpiresAt;
            MaxUses = source.MaxUses;
            InternalName = source.InternalName;
            Note = source.Note;
            PathTemplate = source.PathTemplate;
            PathTemplateTimeZone = source.PathTemplateTimeZone;
            SendEmailReceiptToUploader = source.SendEmailReceiptToUploader;
            SnapshotId = source.SnapshotId;
            UserId = source.UserId;
            Username = source.Username;
            GroupId = source.GroupId;
            ClickwrapId = source.ClickwrapId;
            InboxId = source.InboxId;
            WatermarkAttachment = source.WatermarkAttachment;
            WatermarkValue = source.WatermarkValue;
            SendOneTimePasswordToRecipientAtRegistration = source.SendOneTimePasswordToRecipientAtRegistration;
            WorkspaceId = source.WorkspaceId;
            HasInbox = source.HasInbox;
            DontAllowFoldersInUploads = source.DontAllowFoldersInUploads;
            RequestedUploadSlots = source.RequestedUploadSlots;
            Paths = source.Paths;
            Bundlepaths = source.Bundlepaths;
        }
    }
}
