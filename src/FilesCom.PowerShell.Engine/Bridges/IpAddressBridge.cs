using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs IpAddress operations with the SDK and projects the SDK's IpAddress into the module's.
    internal static class IpAddressBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.IpAddress Project(Sdk.IpAddress source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.IpAddress
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                AssociatedWith = source.AssociatedWith,
                GroupId = source.GroupId,
                IpAddresses = Values.Copy(source.IpAddresses),
            };
        }

        internal static FilesPageSource<Models.IpAddress> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.IpAddresses.List(parameters, requestOptions), source => IpAddressBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.PublicIpAddress> GetSmartfileReserved(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.IpAddresses.GetSmartfileReserved(parameters, requestOptions), source => PublicIpAddressBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.PublicIpAddress> GetExavaultReserved(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.IpAddresses.GetExavaultReserved(parameters, requestOptions), source => PublicIpAddressBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.PublicIpAddress> GetReserved(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.IpAddresses.GetReserved(parameters, requestOptions), source => PublicIpAddressBridge.Project(source, connection, options));
        }
    }
}
