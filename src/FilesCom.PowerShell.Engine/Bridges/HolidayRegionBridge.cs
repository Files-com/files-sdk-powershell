using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs HolidayRegion operations with the SDK and projects the SDK's HolidayRegion into the module's.
    internal static class HolidayRegionBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.HolidayRegion Project(Sdk.HolidayRegion source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.HolidayRegion
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Code = source.Code,
                Name = source.Name,
            };
        }

        internal static FilesPageSource<Models.HolidayRegion> GetSupported(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.HolidayRegions.GetSupported(parameters, requestOptions), source => HolidayRegionBridge.Project(source, connection, options));
        }
    }
}
