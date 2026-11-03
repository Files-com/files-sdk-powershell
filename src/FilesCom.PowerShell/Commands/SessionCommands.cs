using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Create user session (log in).
    /// </summary>
    [Cmdlet("New", "FilesSession", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.Session))]
    public sealed class NewFilesSessionCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Username to sign in as
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Username { get; set; }

        /// <summary>
        /// Password for sign in
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString Password { get; set; }

        /// <summary>
        /// If this user has a 2FA device, provide its OTP or code here.
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Otp { get; set; }

        /// <summary>
        /// Identifier for a partially-completed login
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public System.Security.SecureString PartialSessionId { get; set; }

        /// <summary>Create user session (log in).</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Username)) ? $"Session '{Username}'" : "Session"))
            {
                return;
            }
            Models.Session result = Run(connection, token => SessionBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Username)))
            {
                parameters["username"] = Username;
            }
            if (IsBound(nameof(Password)))
            {
                parameters["password"] = RequestValues.Reveal(Password);
            }
            if (IsBound(nameof(Otp)))
            {
                parameters["otp"] = Otp;
            }
            if (IsBound(nameof(PartialSessionId)))
            {
                parameters["partial_session_id"] = RequestValues.Reveal(PartialSessionId);
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete user session (log out).
    /// </summary>
    [Cmdlet("Remove", "FilesSession", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesSessionCommand : FilesApiCmdlet
    {

        /// <summary>Delete user session (log out).</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, "Session"))
            {
                return;
            }
            Run(connection, token => SessionBridge.DeleteAsync(connection, parameters, CallOptions, token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
