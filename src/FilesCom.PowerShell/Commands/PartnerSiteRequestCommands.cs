using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Partner Site Requests.
    /// </summary>
    [Cmdlet("Get", "FilesPartnerSiteRequest", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.PartnerSiteRequest), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.PartnerSiteRequest>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesPartnerSiteRequestCommand : FilesListCmdlet<Models.PartnerSiteRequest>
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
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are `host_partner_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// If set, return records where the specified field is equal to the supplied value. Valid fields are `host_partner_id`.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Filter { get; set; }

        /// <summary>List Partner Site Requests.</summary>
        protected override void Execute()
        {
            WriteList(connection => PartnerSiteRequestBridge.List(connection, ListParameters(), CallOptions));
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
    }

    /// <summary>
    /// Find partner site request by pairing key.
    /// </summary>
    [Cmdlet("Get", "FilesPartnerSiteRequestByPairingKey")]
    public sealed class GetFilesPartnerSiteRequestByPairingKeyCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Pairing key for the partner site request
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string PairingKey { get; set; }

        /// <summary>Find partner site request by pairing key.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = FindByPairingKeyParameters();
            Run(connection, token => PartnerSiteRequestBridge.FindByPairingKeyAsync(connection, parameters, CallOptions, token));
        }

        private Dictionary<string, object> FindByPairingKeyParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(PairingKey)))
            {
                parameters["pairing_key"] = PairingKey;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Create Partner Site Request.
    /// </summary>
    [Cmdlet("New", "FilesPartnerSiteRequest", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.PartnerSiteRequest))]
    public sealed class NewFilesPartnerSiteRequestCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Host Partner ID to link with
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesInteger]
        [ValidateNotNull]
        public long HostPartnerId { get; set; }

        /// <summary>
        /// Guest Site URL to link to
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string GuestSiteUrl { get; set; }

        /// <summary>Create Partner Site Request.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, "Partner site request"))
            {
                return;
            }
            Models.PartnerSiteRequest result = Run(connection, token => PartnerSiteRequestBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(HostPartnerId)))
            {
                parameters["host_partner_id"] = HostPartnerId;
            }
            if (IsBound(nameof(GuestSiteUrl)))
            {
                parameters["guest_site_url"] = GuestSiteUrl;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Approve partner site request.
    /// </summary>
    [Cmdlet("Approve", "FilesPartnerSiteRequest", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class ApproveFilesPartnerSiteRequestCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Pairing key for the partner site request
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string PairingKey { get; set; }

        /// <summary>
        /// ID of an existing Partner on this site, with the host role, that represents the requesting organization. The connection binds to that Partner and makes it host_and_guest. When omitted, a guest Partner named after the host site is created.
        /// </summary>
        [Parameter()]
        [FilesInteger]
        [ValidateNotNull]
        public long? PartnerId { get; set; }

        /// <summary>Approve partner site request.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = ApproveParameters();
            if (!ShouldProcessOn(connection, "Partner site request"))
            {
                return;
            }
            Run(connection, token => PartnerSiteRequestBridge.ApproveAsync(connection, parameters, CallOptions, token));
        }

        private Dictionary<string, object> ApproveParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(PairingKey)))
            {
                parameters["pairing_key"] = PairingKey;
            }
            if (IsBound(nameof(PartnerId)))
            {
                parameters["partner_id"] = PartnerId.Value;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Reject partner site request.
    /// </summary>
    [Cmdlet("Deny", "FilesPartnerSiteRequest", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class DenyFilesPartnerSiteRequestCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Pairing key for the partner site request
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string PairingKey { get; set; }

        /// <summary>Reject partner site request.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = RejectParameters();
            if (!ShouldProcessOn(connection, "Partner site request"))
            {
                return;
            }
            Run(connection, token => PartnerSiteRequestBridge.RejectAsync(connection, parameters, CallOptions, token));
        }

        private Dictionary<string, object> RejectParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(PairingKey)))
            {
                parameters["pairing_key"] = PairingKey;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Partner Site Request.
    /// </summary>
    [Cmdlet("Remove", "FilesPartnerSiteRequest", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesPartnerSiteRequestCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Partner Site Request ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The partner site request to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.PartnerSiteRequest InputObject { get; set; }

        /// <summary>Delete Partner Site Request.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Partner site request {id}"))
            {
                return;
            }
            Run(connection, token => PartnerSiteRequestBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
