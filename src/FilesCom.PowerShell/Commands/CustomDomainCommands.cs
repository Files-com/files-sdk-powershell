using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Custom Domains, or get one custom domain by Id.
    /// </summary>
    [Cmdlet("Get", "FilesCustomDomain", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.CustomDomain), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.CustomDomain>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.CustomDomain), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesCustomDomainCommand : FilesListCmdlet<Models.CustomDomain>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `id` or `available_to_all_workspaces`.
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
        /// Custom Domain ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Custom Domains, or get one custom domain by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.CustomDomain result = Run(connection, token => CustomDomainBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => CustomDomainBridge.List(connection, ListParameters(), CallOptions));
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
    /// Allocate dedicated IP addresses to this Custom Domain.
    /// </summary>
    [Cmdlet("New", "FilesCustomDomainIpAllocation", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.CustomDomain))]
    public sealed class NewFilesCustomDomainIpAllocationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Custom Domain ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Number of dedicated IP addresses to allocate.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long Count { get; set; }

        /// <summary>Allocate dedicated IP addresses to this Custom Domain.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            long id = Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = CreateAllocateIpParameters();
            if (!ShouldProcessOn(connection, $"Custom domain {id}"))
            {
                return;
            }
            Models.CustomDomain result = Run(connection, token => CustomDomainBridge.CreateAllocateIpAsync(connection, id, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateAllocateIpParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Count)))
            {
                parameters["count"] = Count;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Custom Domain.
    /// </summary>
    [Cmdlet("New", "FilesCustomDomain", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.CustomDomain))]
    public sealed class NewFilesCustomDomainCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Allow all workspaces to use this default-workspace Custom Domain.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AvailableToAllWorkspaces { get; set; }

        /// <summary>
        /// Workspace ID (0 for the default workspace).
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Where this custom domain routes. Can be `site_alias`, `public_hosting`, `s3_endpoint`, or `unassigned` (not routing traffic). Set to `unassigned` automatically when a bound `public_hosting` folder behavior is deleted, and can be set manually via the API for any reason.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// Public Hosting behavior ID when this domain routes to a specific Public Hosting behavior.  Preserved as historical context when `destination` becomes `unassigned`.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? FolderBehaviorId { get; set; }

        /// <summary>
        /// Current SSL certificate ID.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? SslCertificateId { get; set; }

        /// <summary>
        /// Customer-owned domain name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Domain { get; set; }

        /// <summary>Create Custom Domain.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Custom domain"))
            {
                return;
            }
            Models.CustomDomain result = Run(connection, token => CustomDomainBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(AvailableToAllWorkspaces)))
            {
                parameters["available_to_all_workspaces"] = AvailableToAllWorkspaces.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(FolderBehaviorId)))
            {
                parameters["folder_behavior_id"] = FolderBehaviorId.Value;
            }
            if (IsBound(nameof(SslCertificateId)))
            {
                parameters["ssl_certificate_id"] = SslCertificateId.Value;
            }
            if (IsBound(nameof(Domain)))
            {
                parameters["domain"] = Domain;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Custom Domain.
    /// </summary>
    [Cmdlet("Set", "FilesCustomDomain", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.CustomDomain), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesCustomDomainCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Custom Domain ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Allow all workspaces to use this default-workspace Custom Domain.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? AvailableToAllWorkspaces { get; set; }

        /// <summary>
        /// Workspace ID (0 for the default workspace).
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>
        /// Where this custom domain routes. Can be `site_alias`, `public_hosting`, `s3_endpoint`, or `unassigned` (not routing traffic). Set to `unassigned` automatically when a bound `public_hosting` folder behavior is deleted, and can be set manually via the API for any reason.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Destination { get; set; }

        /// <summary>
        /// Public Hosting behavior ID when this domain routes to a specific Public Hosting behavior.  Preserved as historical context when `destination` becomes `unassigned`.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? FolderBehaviorId { get; set; }

        /// <summary>
        /// Current SSL certificate ID.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesInteger]
        [ValidateNotNull]
        public long? SslCertificateId { get; set; }

        /// <summary>
        /// Customer-owned domain name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Domain { get; set; }

        /// <summary>
        /// The custom domain to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.CustomDomain InputObject { get; set; }

        /// <summary>Update Custom Domain.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Custom domain {id}"))
            {
                return;
            }
            Models.CustomDomain result = Run(connection, token => CustomDomainBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(AvailableToAllWorkspaces)))
            {
                parameters["available_to_all_workspaces"] = AvailableToAllWorkspaces.Value;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(Destination)))
            {
                parameters["destination"] = Destination;
            }
            if (IsBound(nameof(FolderBehaviorId)))
            {
                parameters["folder_behavior_id"] = FolderBehaviorId.Value;
            }
            if (IsBound(nameof(SslCertificateId)))
            {
                parameters["ssl_certificate_id"] = SslCertificateId.Value;
            }
            if (IsBound(nameof(Domain)))
            {
                parameters["domain"] = Domain;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Custom Domain.
    /// </summary>
    [Cmdlet("Remove", "FilesCustomDomain", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesCustomDomainCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Custom Domain ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The custom domain to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.CustomDomain InputObject { get; set; }

        /// <summary>Delete Custom Domain.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Custom domain {id}"))
            {
                return;
            }
            Run(connection, token => CustomDomainBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
