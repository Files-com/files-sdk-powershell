using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's FormField into the module's.
    internal static class FormFieldBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.FormField Project(Sdk.FormField source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.FormField
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Id = source.Id,
                Label = source.Label,
                Required = source.Required,
                HelpText = source.HelpText,
                FieldType = source.FieldType,
                OptionsForSelect = Values.Copy(source.OptionsForSelect),
                DefaultOption = source.DefaultOption,
                FormFieldSetId = source.FormFieldSetId,
            };
        }
    }
}
