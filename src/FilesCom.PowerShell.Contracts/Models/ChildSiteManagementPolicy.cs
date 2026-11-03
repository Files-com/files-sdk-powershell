using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Child Site Management Policy is a centralized policy defined by a parent site to enforce consistent configurations across child sites.
    /// </summary>
    public sealed class ChildSiteManagementPolicy : FilesObject
    {
        internal ChildSiteManagementPolicy()
        {
        }

        /// <summary>
        /// Policy ID.
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Type of policy.  Valid values: `settings`.
        /// </summary>
        public string PolicyType { get; internal set; }

        /// <summary>
        /// Name for this policy.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Description for this policy.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Policy configuration data. On a Child Site, contains only managed setting names with null values. Settings policies accept site settings plus an optional `folder_behaviors` array for parent-managed root behaviors on child sites. For more information, refer to the Value Hash section of the developer documentation.
        /// </summary>
        public object Value { get; internal set; }

        /// <summary>
        /// IDs of child sites that this policy has been applied to. This field is read-only.
        /// </summary>
        public long?[] AppliedChildSiteIds { get; internal set; }

        /// <summary>
        /// IDs of child sites excluded from this default policy.
        /// </summary>
        public long?[] SkipChildSiteIds { get; internal set; }

        /// <summary>
        /// IDs of child sites explicitly assigned to this non-default policy.
        /// </summary>
        public long?[] ChildSiteIds { get; internal set; }

        /// <summary>
        /// Whether this policy applies to child sites not explicitly assigned to another policy.
        /// </summary>
        public bool DefaultPolicy { get; internal set; }

        /// <summary>
        /// When this policy was created.
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// When this policy was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(ChildSiteManagementPolicy source)
        {
            Id = source.Id;
            PolicyType = source.PolicyType;
            Name = source.Name;
            Description = source.Description;
            Value = source.Value;
            AppliedChildSiteIds = source.AppliedChildSiteIds;
            SkipChildSiteIds = source.SkipChildSiteIds;
            ChildSiteIds = source.ChildSiteIds;
            DefaultPolicy = source.DefaultPolicy;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }
    }
}
