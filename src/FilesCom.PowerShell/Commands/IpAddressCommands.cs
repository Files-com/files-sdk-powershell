using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List IP Addresses associated with the current site.
    /// </summary>
    [Cmdlet("Get", "FilesIpAddress", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.IpAddress), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.IpAddress>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesIpAddressCommand : FilesListCmdlet<Models.IpAddress>
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

        /// <summary>List IP Addresses associated with the current site.</summary>
        protected override void Execute()
        {
            WriteList(connection => IpAddressBridge.List(connection, ListParameters(), CallOptions));
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
            return parameters;
        }
    }

    /// <summary>
    /// List all possible public SmartFile IP addresses.
    /// </summary>
    [Cmdlet("Get", "FilesSmartfileReservedIpAddress", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.PublicIpAddress), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.PublicIpAddress>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesSmartfileReservedIpAddressCommand : FilesListCmdlet<Models.PublicIpAddress>
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

        /// <summary>List all possible public SmartFile IP addresses.</summary>
        protected override void Execute()
        {
            WriteList(connection => IpAddressBridge.GetSmartfileReserved(connection, GetSmartfileReservedParameters(), CallOptions));
        }

        private Dictionary<string, object> GetSmartfileReservedParameters()
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
            return parameters;
        }
    }

    /// <summary>
    /// List all possible public ExaVault IP addresses.
    /// </summary>
    [Cmdlet("Get", "FilesExavaultReservedIpAddress", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.PublicIpAddress), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.PublicIpAddress>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesExavaultReservedIpAddressCommand : FilesListCmdlet<Models.PublicIpAddress>
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

        /// <summary>List all possible public ExaVault IP addresses.</summary>
        protected override void Execute()
        {
            WriteList(connection => IpAddressBridge.GetExavaultReserved(connection, GetExavaultReservedParameters(), CallOptions));
        }

        private Dictionary<string, object> GetExavaultReservedParameters()
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
            return parameters;
        }
    }

    /// <summary>
    /// List all possible public IP addresses.
    /// </summary>
    [Cmdlet("Get", "FilesReservedIpAddress", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.PublicIpAddress), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.PublicIpAddress>), ParameterSetName = new[] { "Page", "Continuation" })]
    public sealed class GetFilesReservedIpAddressCommand : FilesListCmdlet<Models.PublicIpAddress>
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

        /// <summary>List all possible public IP addresses.</summary>
        protected override void Execute()
        {
            WriteList(connection => IpAddressBridge.GetReserved(connection, GetReservedParameters(), CallOptions));
        }

        private Dictionary<string, object> GetReservedParameters()
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
            return parameters;
        }
    }
}
