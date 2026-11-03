using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Clickwrap is a legal agreement (such as an NDA or Terms of Use) that your Users and/or Bundle/Inbox participants will need to agree to via a "Clickwrap" UI before accessing the site, bundle, or inbox.
    /// </summary>
    public sealed class Clickwrap : FilesObject
    {
        internal Clickwrap()
        {
        }

        /// <summary>
        /// Clickwrap ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Name of the Clickwrap agreement (used when selecting from multiple Clickwrap agreements.)
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Body text of Clickwrap (supports Markdown formatting).
        /// </summary>
        public string Body { get; internal set; }

        /// <summary>
        /// Use this Clickwrap for Users?  Values: `none`, `require` (new user signup via email invitation only), `require_all_users_once` (show to all users at their next web login; once accepted, not shown again), `require_all_users_always` (show to all users on every web login).
        /// </summary>
        public string UseWithUsers { get; internal set; }

        /// <summary>
        /// Use this Clickwrap for Bundles?
        /// </summary>
        public string UseWithBundles { get; internal set; }

        /// <summary>
        /// Use this Clickwrap for Inboxes?
        /// </summary>
        public string UseWithInboxes { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Clickwrap source)
        {
            Id = source.Id;
            Name = source.Name;
            Body = source.Body;
            UseWithUsers = source.UseWithUsers;
            UseWithBundles = source.UseWithBundles;
            UseWithInboxes = source.UseWithInboxes;
        }
    }
}
