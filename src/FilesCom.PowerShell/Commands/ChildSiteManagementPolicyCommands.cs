using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Child Site Management Policies. On a Child Site, returns only the policy covering that site. Its value contains managed setting names with null values, or get one child site management policy by Id.
    /// </summary>
    [Cmdlet("Get", "FilesChildSiteManagementPolicy", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.ChildSiteManagementPolicy), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.ChildSiteManagementPolicy>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.ChildSiteManagementPolicy), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesChildSiteManagementPolicyCommand : FilesListCmdlet<Models.ChildSiteManagementPolicy>
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
        /// Child Site Management Policy ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Child Site Management Policies. On a Child Site, returns only the policy covering that site. Its value contains managed setting names with null values, or get one child site management policy by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.ChildSiteManagementPolicy result = Run(connection, token => ChildSiteManagementPolicyBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => ChildSiteManagementPolicyBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Child Site Management Policy.
    /// </summary>
    [Cmdlet("New", "FilesChildSiteManagementPolicy", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.ChildSiteManagementPolicy))]
    public sealed class NewFilesChildSiteManagementPolicyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Policy configuration data. Attributes differ by policy type. For more information, refer to the Value Hash section of the developer documentation.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Value { get; set; }

        /// <summary>
        /// IDs of child sites excluded from this default policy.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] SkipChildSiteIds { get; set; }

        /// <summary>
        /// IDs of child sites explicitly assigned to this non-default policy.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ChildSiteIds { get; set; }

        /// <summary>
        /// Whether this policy applies to child sites not explicitly assigned to another policy.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DefaultPolicy { get; set; }

        /// <summary>
        /// Type of policy.  Valid values: `settings`.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string PolicyType { get; set; }

        /// <summary>
        /// Name for this policy.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Description for this policy.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>Create Child Site Management Policy.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Child site management policy '{Name}'" : "Child site management policy"))
            {
                return;
            }
            Models.ChildSiteManagementPolicy result = Run(connection, token => ChildSiteManagementPolicyBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Value)))
            {
                parameters["value"] = RequestValues.Object(Value, "Value");
            }
            if (IsBound(nameof(SkipChildSiteIds)))
            {
                parameters["skip_child_site_ids"] = RequestValues.Int64s(SkipChildSiteIds);
            }
            if (IsBound(nameof(ChildSiteIds)))
            {
                parameters["child_site_ids"] = RequestValues.Int64s(ChildSiteIds);
            }
            if (IsBound(nameof(DefaultPolicy)))
            {
                parameters["default_policy"] = DefaultPolicy.Value;
            }
            if (IsBound(nameof(PolicyType)))
            {
                parameters["policy_type"] = PolicyType;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Child Site Management Policy.
    /// </summary>
    [Cmdlet("Set", "FilesChildSiteManagementPolicy", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.ChildSiteManagementPolicy), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesChildSiteManagementPolicyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Child Site Management Policy ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Policy configuration data. Attributes differ by policy type. For more information, refer to the Value Hash section of the developer documentation.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Value { get; set; }

        /// <summary>
        /// IDs of child sites excluded from this default policy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] SkipChildSiteIds { get; set; }

        /// <summary>
        /// IDs of child sites explicitly assigned to this non-default policy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long[] ChildSiteIds { get; set; }

        /// <summary>
        /// Whether this policy applies to child sites not explicitly assigned to another policy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DefaultPolicy { get; set; }

        /// <summary>
        /// Type of policy.  Valid values: `settings`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string PolicyType { get; set; }

        /// <summary>
        /// Name for this policy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Description for this policy.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// The child site management policy to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ChildSiteManagementPolicy InputObject { get; set; }

        /// <summary>Update Child Site Management Policy.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Child site management policy {id}"))
            {
                return;
            }
            Models.ChildSiteManagementPolicy result = Run(connection, token => ChildSiteManagementPolicyBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Value)))
            {
                parameters["value"] = RequestValues.Object(Value, "Value");
            }
            if (IsBound(nameof(SkipChildSiteIds)))
            {
                parameters["skip_child_site_ids"] = RequestValues.Int64s(SkipChildSiteIds);
            }
            if (IsBound(nameof(ChildSiteIds)))
            {
                parameters["child_site_ids"] = RequestValues.Int64s(ChildSiteIds);
            }
            if (IsBound(nameof(DefaultPolicy)))
            {
                parameters["default_policy"] = DefaultPolicy.Value;
            }
            if (IsBound(nameof(PolicyType)))
            {
                parameters["policy_type"] = PolicyType;
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Child Site Management Policy.
    /// </summary>
    [Cmdlet("Remove", "FilesChildSiteManagementPolicy", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesChildSiteManagementPolicyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Child Site Management Policy ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The child site management policy to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ChildSiteManagementPolicy InputObject { get; set; }

        /// <summary>Delete Child Site Management Policy.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Child site management policy {id}"))
            {
                return;
            }
            Run(connection, token => ChildSiteManagementPolicyBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
