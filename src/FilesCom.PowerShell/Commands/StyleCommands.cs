using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Show Style.
    /// </summary>
    [Cmdlet("Get", "FilesStyle")]
    [OutputType(typeof(Models.Style))]
    public sealed class GetFilesStyleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Style path.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>Show Style.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            string path = Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = FindParameters();
            Models.Style result = Run(connection, token => StyleBridge.FindAsync(connection, path, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Update Style.
    /// </summary>
    [Cmdlet("Set", "FilesStyle", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Style), ParameterSetName = new[] { "Path", "InputObject" })]
    public sealed class SetFilesStyleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Style path.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// Logo for custom branding. Required when creating a new style.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public FilesAttachment File { get; set; }

        /// <summary>
        /// URL to open when a public visitor clicks the logo.
        /// </summary>
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string LogoClickHref { get; set; }

        /// <summary>
        /// The style to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Style InputObject { get; set; }

        /// <summary>Update Style.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Style '{path}'"))
            {
                return;
            }
            Models.Style result = Run(connection, token => StyleBridge.UpdateAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
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
            if (IsBound(nameof(File)))
            {
                parameters["file"] = RequestValues.Attachment(File, "File");
            }
            if (IsBound(nameof(LogoClickHref)))
            {
                parameters["logo_click_href"] = LogoClickHref;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Style.
    /// </summary>
    [Cmdlet("Remove", "FilesStyle", DefaultParameterSetName = "Path", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesStyleCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Style path.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Path { get; set; }

        /// <summary>
        /// The style to act on, such as one written by another FilesCom command. The command uses its
        /// Path and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.Style InputObject { get; set; }

        /// <summary>Delete Style.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            string path = InputObject != null ? InputObject.Path ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Path.") : Path;
            ErrorTarget = path;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Style '{path}'"))
            {
                return;
            }
            Run(connection, token => StyleBridge.DeleteAsync(connection, path, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
