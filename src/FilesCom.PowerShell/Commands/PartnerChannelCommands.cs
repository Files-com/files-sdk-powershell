using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Partner Channels, or get one partner channel by Id.
    /// </summary>
    [Cmdlet("Get", "FilesPartnerChannel", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.PartnerChannel), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.PartnerChannel>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.PartnerChannel), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesPartnerChannelCommand : FilesListCmdlet<Models.PartnerChannel>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `workspace_id`, `path` or `partner_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `partner_id` and `workspace_id`. Valid field combinations are `[ workspace_id, partner_id ]`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// Partner Channel ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Partner Channels, or get one partner channel by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.PartnerChannel result = Run(connection, token => PartnerChannelBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => PartnerChannelBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create Partner Channel.
    /// </summary>
    [Cmdlet("New", "FilesPartnerChannel", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.PartnerChannel))]
    public sealed class NewFilesPartnerChannelCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Channel directions. `two_way` enables both directions, `to_partner` enables outgoing downloads, and `from_partner` enables incoming uploads.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Direction { get; set; }

        /// <summary>
        /// Use the Channel folder directly for a one-way exchange. Defaults to false. Cannot be changed after creation. Folder name overrides must be blank when enabled, and the Channel must remain one-way.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UseChannelRoot { get; set; }

        /// <summary>
        /// Optional Channel-level from-Partner folder name override.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FromPartnerFolderName { get; set; }

        /// <summary>
        /// Managed folder paths inside the from-Partner folder.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] FromPartnerManagedFolderPaths { get; set; }

        /// <summary>
        /// Optional route path for files uploaded by the Partner.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FromPartnerRoutePath { get; set; }

        /// <summary>
        /// Optional Channel-level to-Partner folder name override.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ToPartnerFolderName { get; set; }

        /// <summary>
        /// Managed folder paths inside the to-Partner folder.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string[] ToPartnerManagedFolderPaths { get; set; }

        /// <summary>
        /// Optional route path for files delivered to the Partner.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string ToPartnerRoutePath { get; set; }

        /// <summary>
        /// ID of the Partner this Channel belongs to.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long PartnerId { get; set; }

        /// <summary>
        /// Channel path relative to the Partner root folder.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// ID of the Workspace associated with this Partner Channel.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create Partner Channel.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Partner channel '{Path}'" : "Partner channel"))
            {
                return;
            }
            Models.PartnerChannel result = Run(connection, token => PartnerChannelBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Direction)))
            {
                parameters["direction"] = Direction;
            }
            if (IsBound(nameof(UseChannelRoot)))
            {
                parameters["use_channel_root"] = UseChannelRoot.Value;
            }
            if (IsBound(nameof(FromPartnerFolderName)))
            {
                parameters["from_partner_folder_name"] = FromPartnerFolderName;
            }
            if (IsBound(nameof(FromPartnerManagedFolderPaths)))
            {
                parameters["from_partner_managed_folder_paths"] = RequestValues.Strings(FromPartnerManagedFolderPaths, "FromPartnerManagedFolderPaths");
            }
            if (IsBound(nameof(FromPartnerRoutePath)))
            {
                parameters["from_partner_route_path"] = FromPartnerRoutePath;
            }
            if (IsBound(nameof(ToPartnerFolderName)))
            {
                parameters["to_partner_folder_name"] = ToPartnerFolderName;
            }
            if (IsBound(nameof(ToPartnerManagedFolderPaths)))
            {
                parameters["to_partner_managed_folder_paths"] = RequestValues.Strings(ToPartnerManagedFolderPaths, "ToPartnerManagedFolderPaths");
            }
            if (IsBound(nameof(ToPartnerRoutePath)))
            {
                parameters["to_partner_route_path"] = ToPartnerRoutePath;
            }
            if (IsBound(nameof(PartnerId)))
            {
                parameters["partner_id"] = PartnerId;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Partner Channel.
    /// </summary>
    [Cmdlet("Set", "FilesPartnerChannel", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.PartnerChannel), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesPartnerChannelCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Partner Channel ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Channel directions. `two_way` enables both directions, `to_partner` enables outgoing downloads, and `from_partner` enables incoming uploads.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Direction { get; set; }

        /// <summary>
        /// Use the Channel folder directly for a one-way exchange. Defaults to false. Cannot be changed after creation. Folder name overrides must be blank when enabled, and the Channel must remain one-way.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? UseChannelRoot { get; set; }

        /// <summary>
        /// Optional Channel-level from-Partner folder name override.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FromPartnerFolderName { get; set; }

        /// <summary>
        /// Managed folder paths inside the from-Partner folder.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] FromPartnerManagedFolderPaths { get; set; }

        /// <summary>
        /// Optional route path for files uploaded by the Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string FromPartnerRoutePath { get; set; }

        /// <summary>
        /// Optional Channel-level to-Partner folder name override.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ToPartnerFolderName { get; set; }

        /// <summary>
        /// Managed folder paths inside the to-Partner folder.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string[] ToPartnerManagedFolderPaths { get; set; }

        /// <summary>
        /// Optional route path for files delivered to the Partner.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string ToPartnerRoutePath { get; set; }

        /// <summary>
        /// Channel path relative to the Partner root folder.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// The partner channel to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.PartnerChannel InputObject { get; set; }

        /// <summary>Update Partner Channel.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Partner channel {id}"))
            {
                return;
            }
            Models.PartnerChannel result = Run(connection, token => PartnerChannelBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Direction)))
            {
                parameters["direction"] = Direction;
            }
            if (IsBound(nameof(UseChannelRoot)))
            {
                parameters["use_channel_root"] = UseChannelRoot.Value;
            }
            if (IsBound(nameof(FromPartnerFolderName)))
            {
                parameters["from_partner_folder_name"] = FromPartnerFolderName;
            }
            if (IsBound(nameof(FromPartnerManagedFolderPaths)))
            {
                parameters["from_partner_managed_folder_paths"] = RequestValues.Strings(FromPartnerManagedFolderPaths, "FromPartnerManagedFolderPaths");
            }
            if (IsBound(nameof(FromPartnerRoutePath)))
            {
                parameters["from_partner_route_path"] = FromPartnerRoutePath;
            }
            if (IsBound(nameof(ToPartnerFolderName)))
            {
                parameters["to_partner_folder_name"] = ToPartnerFolderName;
            }
            if (IsBound(nameof(ToPartnerManagedFolderPaths)))
            {
                parameters["to_partner_managed_folder_paths"] = RequestValues.Strings(ToPartnerManagedFolderPaths, "ToPartnerManagedFolderPaths");
            }
            if (IsBound(nameof(ToPartnerRoutePath)))
            {
                parameters["to_partner_route_path"] = ToPartnerRoutePath;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Partner Channel.
    /// </summary>
    [Cmdlet("Remove", "FilesPartnerChannel", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesPartnerChannelCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Partner Channel ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The partner channel to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.PartnerChannel InputObject { get; set; }

        /// <summary>Delete Partner Channel.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Partner channel {id}"))
            {
                return;
            }
            Run(connection, token => PartnerChannelBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
