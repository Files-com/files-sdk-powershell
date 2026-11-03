using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Create Webhook Test.
    /// </summary>
    [Cmdlet("New", "FilesWebhookTest", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.WebhookTest))]
    public sealed class NewFilesWebhookTestCommand : FilesApiCmdlet
    {
        /// <summary>
        /// URL for testing the webhook.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Url { get; set; }

        /// <summary>
        /// HTTP method(GET or POST).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Method { get; set; }

        /// <summary>
        /// HTTP encoding method.  Can be JSON, XML, or RAW (form data).
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Encoding { get; set; }

        /// <summary>
        /// Additional request headers.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Headers { get; set; }

        /// <summary>
        /// Additional body parameters.
        /// </summary>
        [Parameter()]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Body { get; set; }

        /// <summary>
        /// raw body text
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string RawBody { get; set; }

        /// <summary>
        /// Send the file data as the request body?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? FileAsBody { get; set; }

        /// <summary>
        /// Send the file data as a named parameter in the request POST body
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string FileFormField { get; set; }

        /// <summary>
        /// action for test body
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public string Action { get; set; }

        /// <summary>
        /// Use dedicated IPs for sending the webhook?
        /// </summary>
        [Parameter()]
        [ValidateNotNull]
        public bool? UseDedicatedIps { get; set; }

        /// <summary>Create Webhook Test.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Webhook test"))
            {
                return;
            }
            Models.WebhookTest result = Run(connection, token => WebhookTestBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Url)))
            {
                parameters["url"] = Url;
            }
            if (IsBound(nameof(Method)))
            {
                parameters["method"] = Method;
            }
            if (IsBound(nameof(Encoding)))
            {
                parameters["encoding"] = Encoding;
            }
            if (IsBound(nameof(Headers)))
            {
                parameters["headers"] = RequestValues.Object(Headers, "Headers");
            }
            if (IsBound(nameof(Body)))
            {
                parameters["body"] = RequestValues.Object(Body, "Body");
            }
            if (IsBound(nameof(RawBody)))
            {
                parameters["raw_body"] = RawBody;
            }
            if (IsBound(nameof(FileAsBody)))
            {
                parameters["file_as_body"] = FileAsBody.Value;
            }
            if (IsBound(nameof(FileFormField)))
            {
                parameters["file_form_field"] = FileFormField;
            }
            if (IsBound(nameof(Action)))
            {
                parameters["action"] = Action;
            }
            if (IsBound(nameof(UseDedicatedIps)))
            {
                parameters["use_dedicated_ips"] = UseDedicatedIps.Value;
            }
            return parameters;
        }
    }
}
