using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Projects the SDK's PartnerChannelUploadForm into the module's.
    internal static class PartnerChannelUploadFormBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.PartnerChannelUploadForm Project(Sdk.PartnerChannelUploadForm source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.PartnerChannelUploadForm
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Path = source.Path,
                FormFieldSetId = source.FormFieldSetId,
            };
        }
    }
}
