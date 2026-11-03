using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs EmailLog operations with the SDK and projects the SDK's EmailLog into the module's.
    internal static class EmailLogBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.EmailLog Project(Sdk.EmailLog source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.EmailLog
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                Timestamp = Values.Utc(source.Timestamp),
                Message = source.Message,
                Status = source.Status,
                Subject = source.Subject,
                To = source.To,
                Cc = source.Cc,
                DeliveryMethod = source.DeliveryMethod,
                SmtpHostname = source.SmtpHostname,
                SmtpIp = source.SmtpIp,
                CreatedAt = Values.Utc(source.CreatedAt),
            };
        }

        internal static FilesPageSource<Models.EmailLog> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.EmailLogs.List(parameters, requestOptions), source => EmailLogBridge.Project(source, connection, options));
        }
    }
}
