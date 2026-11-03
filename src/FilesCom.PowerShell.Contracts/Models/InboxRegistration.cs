using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An InboxRegistration is created when a user fills out the form to access the inbox.
    /// </summary>
    public sealed class InboxRegistration : FilesObject
    {
        internal InboxRegistration()
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
        /// Id of associated inbox
        /// </summary>
        public long? InboxId { get; internal set; }

        /// <summary>
        /// Id of associated inbox recipient
        /// </summary>
        public long? InboxRecipientId { get; internal set; }

        /// <summary>
        /// Title of associated inbox
        /// </summary>
        public string InboxTitle { get; internal set; }

        /// <summary>
        /// Registration creation date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }
    }
}
