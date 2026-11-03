using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs History operations with the SDK.
    internal static class HistoryBridge
    {
        internal static FilesPageSource<Models.HistoryAction> ListForFile(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Histories.ListForFile(path, parameters, requestOptions), source => HistoryActionBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.HistoryAction> ListForFolder(FilesConnection connection, string path, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Histories.ListForFolder(path, parameters, requestOptions), source => HistoryActionBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.HistoryAction> ListForUser(FilesConnection connection, long userId, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Histories.ListForUser(userId, parameters, requestOptions), source => HistoryActionBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.HistoryAction> ListLogins(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Histories.ListLogins(parameters, requestOptions), source => HistoryActionBridge.Project(source, connection, options));
        }

        internal static FilesPageSource<Models.HistoryAction> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Histories.List(parameters, requestOptions), source => HistoryActionBridge.Project(source, connection, options));
        }
    }
}
