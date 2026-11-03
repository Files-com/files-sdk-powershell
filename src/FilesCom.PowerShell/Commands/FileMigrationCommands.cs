using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Show File Migration.
    /// </summary>
    [Cmdlet("Get", "FilesFileMigration")]
    [OutputType(typeof(Models.FileMigration))]
    public sealed class GetFilesFileMigrationCommand : FilesApiCmdlet
    {
        /// <summary>
        /// File Migration ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>Show File Migration.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            long id = Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = FindParameters();
            Models.FileMigration result = Run(connection, token => FileMigrationBridge.FindAsync(connection, id, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
