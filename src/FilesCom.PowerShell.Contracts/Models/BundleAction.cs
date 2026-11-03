using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A BundleAction is an action performed on a bundle.
    /// </summary>
    public sealed class BundleAction : FilesObject
    {
        internal BundleAction()
        {
        }

        /// <summary>
        /// Type of action
        /// </summary>
        public string Action { get; internal set; }

        /// <summary>
        /// Object that contains bundle registration information
        /// </summary>
        public Models.BundleRegistration BundleRegistration { get; internal set; }

        /// <summary>
        /// Action occurrence date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// The destination path for this bundle action, if applicable
        /// </summary>
        public string Destination { get; internal set; }

        /// <summary>
        /// Path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// The source path for this bundle action, if applicable
        /// </summary>
        public string Source { get; internal set; }

        /// <summary>
        /// Identifier of the associated bundle registration
        /// </summary>
        public long? BundleRegistrationId { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Name of the registrant who performed the action
        /// </summary>
        public string BundleRegistrationName { get; internal set; }

        /// <summary>
        /// Email of the registrant
        /// </summary>
        public string BundleRegistrationEmail { get; internal set; }

        /// <summary>
        /// IP address of the registrant
        /// </summary>
        public string BundleRegistrationIp { get; internal set; }

        /// <summary>
        /// Id of associated form field set
        /// </summary>
        public long? FormFieldSetId { get; internal set; }

        /// <summary>
        /// Data for form field set with form field ids as keys and user data as values
        /// </summary>
        public object FormFieldData { get; internal set; }
    }
}
