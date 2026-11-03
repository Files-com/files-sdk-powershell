using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A BundleRegistration is a registration record when a user fills out the form to access the bundle.
    /// </summary>
    public sealed class BundleRegistration : FilesObject
    {
        internal BundleRegistration()
        {
        }

        /// <summary>
        /// Registration cookie code
        /// </summary>
        public string Code { get; internal set; }

        /// <summary>
        /// Registrant name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Registrant company name
        /// </summary>
        public string Company { get; internal set; }

        /// <summary>
        /// Registrant email address
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// Registrant IP Address
        /// </summary>
        public string Ip { get; internal set; }

        /// <summary>
        /// InboxRegistration cookie code, if there is an associated InboxRegistration
        /// </summary>
        public string InboxCode { get; internal set; }

        /// <summary>
        /// Clickwrap text that was shown to the registrant
        /// </summary>
        public string ClickwrapBody { get; internal set; }

        /// <summary>
        /// Id of associated form field set
        /// </summary>
        public long? FormFieldSetId { get; internal set; }

        /// <summary>
        /// Data for form field set with form field ids as keys and user data as values
        /// </summary>
        public object FormFieldData { get; internal set; }

        /// <summary>
        /// Bundle URL code
        /// </summary>
        public string BundleCode { get; internal set; }

        /// <summary>
        /// Id of associated bundle
        /// </summary>
        public long? BundleId { get; internal set; }

        /// <summary>
        /// Id of associated bundle recipient
        /// </summary>
        public long? BundleRecipientId { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Registration creation date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
