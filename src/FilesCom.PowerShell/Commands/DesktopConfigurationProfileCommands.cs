using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Desktop Configuration Profiles, or get one desktop configuration profile by Id.
    /// </summary>
    [Cmdlet("Get", "FilesDesktopConfigurationProfile", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.DesktopConfigurationProfile), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.DesktopConfigurationProfile>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.DesktopConfigurationProfile), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesDesktopConfigurationProfileCommand : FilesListCmdlet<Models.DesktopConfigurationProfile>
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
        /// Desktop Configuration Profile ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Desktop Configuration Profiles, or get one desktop configuration profile by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.DesktopConfigurationProfile result = Run(connection, token => DesktopConfigurationProfileBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => DesktopConfigurationProfileBridge.List(connection, ListParameters(), CallOptions));
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
    /// Create Desktop Configuration Profile.
    /// </summary>
    [Cmdlet("New", "FilesDesktopConfigurationProfile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.DesktopConfigurationProfile))]
    public sealed class NewFilesDesktopConfigurationProfileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Profile name
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// Mount point mappings for the desktop app. Keys must be a single uppercase Windows drive letter other than A, B, or C, and values are Files.com paths to mount there.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary MountMappings { get; set; }

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

        /// <summary>
        /// Whether the desktop app should hide drive mounting, prevent new drive mounts, and unmount active drive mounts for users with this profile
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? DisableDriveMounting { get; set; }

        /// <summary>Create Desktop Configuration Profile.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Desktop configuration profile '{Name}'" : "Desktop configuration profile"))
            {
                return;
            }
            Models.DesktopConfigurationProfile result = Run(connection, token => DesktopConfigurationProfileBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(MountMappings)))
            {
                parameters["mount_mappings"] = RequestValues.Object(MountMappings, "MountMappings");
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            if (IsBound(nameof(UseForAllUsers)))
            {
                parameters["use_for_all_users"] = UseForAllUsers.Value;
            }
            if (IsBound(nameof(DisableDriveMounting)))
            {
                parameters["disable_drive_mounting"] = DisableDriveMounting.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Desktop Configuration Profile.
    /// </summary>
    [Cmdlet("Set", "FilesDesktopConfigurationProfile", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.DesktopConfigurationProfile), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesDesktopConfigurationProfileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Desktop Configuration Profile ID.
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
        /// Mount point mappings for the desktop app. Keys must be a single uppercase Windows drive letter other than A, B, or C, and values are Files.com paths to mount there.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary MountMappings { get; set; }

        /// <summary>
        /// Whether this profile applies to all users in the Workspace by default
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? UseForAllUsers { get; set; }

        /// <summary>
        /// Whether the desktop app should hide drive mounting, prevent new drive mounts, and unmount active drive mounts for users with this profile
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public bool? DisableDriveMounting { get; set; }

        /// <summary>
        /// The desktop configuration profile to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.DesktopConfigurationProfile InputObject { get; set; }

        /// <summary>Update Desktop Configuration Profile.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Desktop configuration profile {id}"))
            {
                return;
            }
            Models.DesktopConfigurationProfile result = Run(connection, token => DesktopConfigurationProfileBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(MountMappings)))
            {
                parameters["mount_mappings"] = RequestValues.Object(MountMappings, "MountMappings");
            }
            if (IsBound(nameof(UseForAllUsers)))
            {
                parameters["use_for_all_users"] = UseForAllUsers.Value;
            }
            if (IsBound(nameof(DisableDriveMounting)))
            {
                parameters["disable_drive_mounting"] = DisableDriveMounting.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Desktop Configuration Profile.
    /// </summary>
    [Cmdlet("Remove", "FilesDesktopConfigurationProfile", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesDesktopConfigurationProfileCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Desktop Configuration Profile ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The desktop configuration profile to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.DesktopConfigurationProfile InputObject { get; set; }

        /// <summary>Delete Desktop Configuration Profile.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Desktop configuration profile {id}"))
            {
                return;
            }
            Run(connection, token => DesktopConfigurationProfileBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
