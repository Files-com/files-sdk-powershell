using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Form Field Set is a custom form to be used for bundle and inbox registrations.
    /// </summary>
    public sealed class FormFieldSet : FilesObject
    {
        internal FormFieldSet()
        {
        }

        /// <summary>
        /// Form field set id
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Title to be displayed
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Reject answers over 1024 characters or more than 32 answered custom fields to ensure complete file metadata capture
        /// </summary>
        public bool RequireCompleteMetadataCapture { get; internal set; }

        /// <summary>
        /// Layout of the form
        /// </summary>
        public long?[] FormLayout { get; internal set; }

        /// <summary>
        /// Associated form field definitions; authenticated form field set responses include historical definitions, while form_layout identifies current fields
        /// </summary>
        public Models.FormField[] FormFields { get; internal set; }

        /// <summary>
        /// Any associated InboxRegistrations or BundleRegistrations can be saved without providing name
        /// </summary>
        public bool SkipName { get; internal set; }

        /// <summary>
        /// Any associated InboxRegistrations or BundleRegistrations can be saved without providing email
        /// </summary>
        public bool SkipEmail { get; internal set; }

        /// <summary>
        /// Any associated InboxRegistrations or BundleRegistrations can be saved without providing company
        /// </summary>
        public bool SkipCompany { get; internal set; }

        /// <summary>
        /// Form Field Set is in use by an active Inbox / Bundle / Inbox Registration / Bundle Registration
        /// </summary>
        public bool InUse { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(FormFieldSet source)
        {
            Id = source.Id;
            Title = source.Title;
            RequireCompleteMetadataCapture = source.RequireCompleteMetadataCapture;
            FormLayout = source.FormLayout;
            FormFields = source.FormFields;
            SkipName = source.SkipName;
            SkipEmail = source.SkipEmail;
            SkipCompany = source.SkipCompany;
            InUse = source.InUse;
        }
    }
}
