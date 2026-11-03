using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Behavior is an API resource for what are also known as Folder Settings.
    /// </summary>
    public sealed class Behavior : FilesObject
    {
        internal Behavior()
        {
        }

        /// <summary>
        /// Folder behavior ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Folder path.  Note that Behavior paths cannot be updated once initially set.  You will need to remove and re-create the behavior on the new path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// URL for attached file
        /// </summary>
        public string AttachmentUrl { get; internal set; }

        /// <summary>
        /// Behavior type.
        /// </summary>
        public string BehaviorType { get; internal set; }

        /// <summary>
        /// Name for this behavior.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Description for this behavior.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Settings for this behavior.  See the section above for an example value to provide here.  Formatting is different for each Behavior type.  May be sent as nested JSON or a single JSON-encoded string.  If using XML encoding for the API call, this data must be sent as a JSON-encoded string.
        /// </summary>
        public object Value { get; internal set; }

        /// <summary>
        /// Public URL for this publicly hosted folder when the `Serve Publicly` behavior has a key configured. When a Custom Domain with `public_hosting` destination is attached to this behavior, the URL uses that domain. Otherwise it uses the site's `subdomain.hosted-by-files.com` host, unless `custom_domain_only` is enabled, in which case no URL is returned.
        /// </summary>
        public string PublicHostingUrl { get; internal set; }

        /// <summary>
        /// If true, disables the inherited behavior for this folder and its children. Valid only for behavior types that child folders may override, and requires recursive to be true. Rejected for all other behavior types.
        /// </summary>
        public bool DisableParentFolderBehavior { get; internal set; }

        /// <summary>
        /// Whether this behavior is recursive for this record. `always` behaviors are always `true`, `never` behaviors are always `false`, and `sometimes` behaviors may be either value.
        /// </summary>
        public bool Recursive { get; internal set; }

        /// <summary>
        /// If true, this behavior is inherited from a higher scope rather than owned by the requested workspace.
        /// </summary>
        public bool Inherited { get; internal set; }

        /// <summary>
        /// If true, this behavior is controlled by a parent-site policy and cannot be modified locally.
        /// </summary>
        public bool Managed { get; internal set; }

        /// <summary>
        /// If true, this behavior may only be modified by a site admin because it is at the site root or disables a root behavior.
        /// </summary>
        public bool RootBehaviorSiteAdminOnly { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Behavior source)
        {
            Id = source.Id;
            Path = source.Path;
            AttachmentUrl = source.AttachmentUrl;
            BehaviorType = source.BehaviorType;
            Name = source.Name;
            Description = source.Description;
            Value = source.Value;
            PublicHostingUrl = source.PublicHostingUrl;
            DisableParentFolderBehavior = source.DisableParentFolderBehavior;
            Recursive = source.Recursive;
            Inherited = source.Inherited;
            Managed = source.Managed;
            RootBehaviorSiteAdminOnly = source.RootBehaviorSiteAdminOnly;
        }
    }
}
