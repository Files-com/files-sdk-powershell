using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List API Keys, or get one api key by Id.
    /// </summary>
    [Cmdlet("Get", "FilesApiKey", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.ApiKey), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.ApiKey>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.ApiKey), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesApiKeyCommand : FilesListCmdlet<Models.ApiKey>
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `site_id` and `workspace_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `aws_style_credentials` and `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than the supplied value. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterGteq { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than the supplied value. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLt { get; set; }

        /// <summary>
        /// If set, return records where the specified field is less than or equal the supplied value. Valid fields are `expires_at`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary FilterLteq { get; set; }

        /// <summary>
        /// Api Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List API Keys, or get one api key by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.ApiKey result = Run(connection, token => ApiKeyBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => ApiKeyBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
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
            if (IsBound(nameof(FilterGt)))
            {
                parameters["filter_gt"] = RequestValues.QueryObject(FilterGt, "FilterGt");
            }
            if (IsBound(nameof(FilterGteq)))
            {
                parameters["filter_gteq"] = RequestValues.QueryObject(FilterGteq, "FilterGteq");
            }
            if (IsBound(nameof(FilterLt)))
            {
                parameters["filter_lt"] = RequestValues.QueryObject(FilterLt, "FilterLt");
            }
            if (IsBound(nameof(FilterLteq)))
            {
                parameters["filter_lteq"] = RequestValues.QueryObject(FilterLteq, "FilterLteq");
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
    /// Show information about current API key.  (Requires current API connection to be using an API key.).
    /// </summary>
    [Cmdlet("Get", "FilesCurrentApiKey")]
    [OutputType(typeof(Models.ApiKey))]
    public sealed class GetFilesCurrentApiKeyCommand : FilesApiCmdlet
    {

        /// <summary>Show information about current API key.  (Requires current API connection to be using an API key.).</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = FindCurrentParameters();
            Models.ApiKey result = Run(connection, token => ApiKeyBridge.FindCurrentAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> FindCurrentParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create API Key.
    /// </summary>
    [Cmdlet("New", "FilesApiKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.ApiKey))]
    public sealed class NewFilesApiKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// User ID.  Provide a value of `0` to operate the current session's user.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? UserId { get; set; }

        /// <summary>
        /// User-supplied description of API key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// API Key expiration date
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// Internal name for the API Key.  For your use.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// If `true`, this API key will be usable with AWS-compatible endpoints, such as our Inbound S3-compatible endpoint.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? AwsStyleCredentials { get; set; }

        /// <summary>
        /// Restricts the file and folder operations made with this key, meaning the files, folders, and file_actions endpoints, to the specified folder and its descendants, including copy and move destinations. For `GET /file_migrations/{id}`, a key with a path can read only migrations it started. Does not grant access beyond the owning user's permissions. Optional for `files_only` keys and required for `office_integration` keys, which need a path the owning user can read. Keys with any other permission set cannot have a path.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Permissions for this API Key. Keys with the `desktop_app` permission set only have the ability to do the functions provided in our Desktop App (File and Share Link operations). Keys with the `mobile_app` permission set can only perform the functions provided by our Mobile App (File and Share Link operations). Keys with the `office_integration` permission set are auto generated, and automatically expire, to allow users to interact with office integration platforms. Keys with the `files_only` permission set can use only the files, folders, and file_actions endpoints, excluding the GPG Encrypt and GPG Decrypt file actions, where they perform file operations as a full-access file user in the key's workspace scope, along with `GET /file_migrations/{id}` and `GET /api_key`. On the migration lookup, any key with a `path` can read only migrations it started. Without a `path`, user-owned keys retain user-scoped access; site-wide Workspace-bound keys can read only migrations whose source and destination are both in their Workspace. Every `files_only` key is Workspace-bound, including Workspace `0`, as is a full-access key in a named Workspace. A full-access site-wide key in the default Workspace retains Site-wide migration access. Migrations outside the caller's visibility return `not-found`. Keys with `files_only` cannot use site admin, workspace admin, folder admin, group admin, partner admin, or billing privileges from the owning user. Both GPG file actions and every other endpoint deny them with `not-authorized/api-key-only-for-file-operations`. Folder-configured GPG encryption and decryption still apply to files uploaded with a `files_only` key.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string PermissionSet { get; set; }

        /// <summary>
        /// Workspace ID for this API Key. `0` means the default workspace.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? WorkspaceId { get; set; }

        /// <summary>Create API Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Path)) ? $"Api key '{Path}'" : "Api key"))
            {
                return;
            }
            Models.ApiKey result = Run(connection, token => ApiKeyBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(UserId)))
            {
                parameters["user_id"] = UserId.Value;
            }
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(ExpiresAt)))
            {
                parameters["expires_at"] = RequestValues.DateTime(ExpiresAt.Value);
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            if (IsBound(nameof(AwsStyleCredentials)))
            {
                parameters["aws_style_credentials"] = AwsStyleCredentials.Value;
            }
            if (IsBound(nameof(Path)))
            {
                parameters["path"] = Path;
            }
            if (IsBound(nameof(PermissionSet)))
            {
                parameters["permission_set"] = PermissionSet;
            }
            if (IsBound(nameof(WorkspaceId)))
            {
                parameters["workspace_id"] = WorkspaceId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update current API key.  (Requires current API connection to be using an API key.).
    /// </summary>
    [Cmdlet("Set", "FilesCurrentApiKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.ApiKey))]
    public sealed class SetFilesCurrentApiKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// API Key expiration date
        /// </summary>
        [Parameter()]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// Internal name for the API Key.  For your use.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>Update current API key.  (Requires current API connection to be using an API key.).</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = UpdateCurrentParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Api key '{Name}'" : "Api key"))
            {
                return;
            }
            Models.ApiKey result = Run(connection, token => ApiKeyBridge.UpdateCurrentAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateCurrentParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(ExpiresAt)))
            {
                parameters["expires_at"] = RequestValues.DateTime(ExpiresAt.Value);
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update API Key.
    /// </summary>
    [Cmdlet("Set", "FilesApiKey", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(Models.ApiKey), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesApiKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Api Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// User-supplied description of API key.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Description { get; set; }

        /// <summary>
        /// API Key expiration date
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDateTime]
        [ValidateNotNull]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// Internal name for the API Key.  For your use.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// The api key to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ApiKey InputObject { get; set; }

        /// <summary>Update API Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Api key {id}"))
            {
                return;
            }
            Models.ApiKey result = Run(connection, token => ApiKeyBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(Description)))
            {
                parameters["description"] = Description;
            }
            if (IsBound(nameof(ExpiresAt)))
            {
                parameters["expires_at"] = RequestValues.DateTime(ExpiresAt.Value);
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete current API key.  (Requires current API connection to be using an API key.).
    /// </summary>
    [Cmdlet("Remove", "FilesCurrentApiKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesCurrentApiKeyCommand : FilesApiCmdlet
    {

        /// <summary>Delete current API key.  (Requires current API connection to be using an API key.).</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = DeleteCurrentParameters();
            if (!ShouldProcessOn(connection, "Api key"))
            {
                return;
            }
            Run(connection, token => ApiKeyBridge.DeleteCurrentAsync(connection, parameters, CallOptions, token));
        }

        private Dictionary<string, object> DeleteCurrentParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Delete API Key.
    /// </summary>
    [Cmdlet("Remove", "FilesApiKey", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesApiKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Api Key ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The api key to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.ApiKey InputObject { get; set; }

        /// <summary>Delete API Key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Api key {id}"))
            {
                return;
            }
            Run(connection, token => ApiKeyBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
