using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's AutomationAuthoringSchema into the module's.
    internal static class AutomationAuthoringSchemaBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.AutomationAuthoringSchema Project(Sdk.AutomationAuthoringSchema source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.AutomationAuthoringSchema
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                DefinitionSchema = Values.Json(source.DefinitionSchema),
                ErrorFamilies = Values.JsonArray(source.ErrorFamilies),
                Nodes = Values.JsonArray(source.Nodes),
                SchemaUrl = source.SchemaUrl,
            };
        }
    }
}
