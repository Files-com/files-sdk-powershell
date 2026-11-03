using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Integration Centric Profiles, or get one integration centric profile by Id.
    /// </summary>
    [Cmdlet("Get", "FilesIntegrationCentricProfile", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.IntegrationCentricProfile), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.IntegrationCentricProfile>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.IntegrationCentricProfile), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesIntegrationCentricProfileCommand : FilesListCmdlet<Models.IntegrationCentricProfile>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id` and `name`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Integration Centric Profile ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Integration Centric Profiles, or get one integration centric profile by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.IntegrationCentricProfile result = Run(connection, token => IntegrationCentricProfileBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => IntegrationCentricProfileBridge.List(connection, ListParameters(), CallOptions));
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
            if (IsBound(nameof(SortBy)))
            {
                parameters["sort_by"] = RequestValues.QueryObject(SortBy, "SortBy");
            }
            if (IsBound(nameof(Filter)))
            {
                parameters["filter"] = RequestValues.QueryObject(Filter, "Filter");
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
    /// Create Integration Centric Profile.
    /// </summary>
    [Cmdlet("New", "FilesIntegrationCentricProfile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.IntegrationCentricProfile))]
    public sealed class NewFilesIntegrationCentricProfileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Profile name
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Remote Server integrations the user is expected to add and connect. Each entry requires `server_type` and may include a display `name`.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary[] ExpectedRemoteServers { get; set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Whether this profile applies to all users in the Workspace by default
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UseForAllUsers { get; set; }

        /// <summary>Create Integration Centric Profile.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Integration centric profile '{Name}'" : "Integration centric profile"))
            {
                return;
            }
            Models.IntegrationCentricProfile result = Run(connection, token => IntegrationCentricProfileBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(ExpectedRemoteServers)))
            {
                parameters["expected_remote_servers"] = RequestValues.Objects(ExpectedRemoteServers, "ExpectedRemoteServers");
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(UseForAllUsers)))
            {
                parameters["use_for_all_users"] = UseForAllUsers.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Integration Centric Profile.
    /// </summary>
    [Cmdlet("Set", "FilesIntegrationCentricProfile", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.IntegrationCentricProfile), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesIntegrationCentricProfileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Integration Centric Profile ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Profile name
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Remote Server integrations the user is expected to add and connect. Each entry requires `server_type` and may include a display `name`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary[] ExpectedRemoteServers { get; set; }

        /// <summary>
        /// Whether this profile applies to all users in the Workspace by default
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? UseForAllUsers { get; set; }

        /// <summary>
        /// The integration centric profile to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.IntegrationCentricProfile InputObject { get; set; }

        /// <summary>Update Integration Centric Profile.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Integration centric profile {id}"))
            {
                return;
            }
            Models.IntegrationCentricProfile result = Run(connection, token => IntegrationCentricProfileBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(ExpectedRemoteServers)))
            {
                parameters["expected_remote_servers"] = RequestValues.Objects(ExpectedRemoteServers, "ExpectedRemoteServers");
            }
            if (IsBound(nameof(UseForAllUsers)))
            {
                parameters["use_for_all_users"] = UseForAllUsers.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Integration Centric Profile.
    /// </summary>
    [Cmdlet("Remove", "FilesIntegrationCentricProfile", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesIntegrationCentricProfileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Integration Centric Profile ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The integration centric profile to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.IntegrationCentricProfile InputObject { get; set; }

        /// <summary>Delete Integration Centric Profile.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Integration centric profile {id}"))
            {
                return;
            }
            Run(connection, token => IntegrationCentricProfileBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
