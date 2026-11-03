using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs DnsRecord operations with the SDK and projects the SDK's DnsRecord into the module's.
    internal static class DnsRecordBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.DnsRecord Project(Sdk.DnsRecord source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.DnsRecord
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Domain = source.Domain,
                Rrtype = source.Rrtype,
                Value = source.Value,
            };
        }

        internal static FilesPageSource<Models.DnsRecord> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.DnsRecords.List(parameters, requestOptions), source => DnsRecordBridge.Project(source, connection, options));
        }
    }
}
