using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk = FilesCom.Models;

namespace FilesCom.PowerShell.Engine
{
    // Runs App operations with the SDK and projects the SDK's App into the module's.
    internal static class AppBridge
    {
        // The projected object keeps the connection and workspace options it was read with, for commands given it later.
        internal static Models.App Project(Sdk.App source, FilesConnection connection, FilesCallOptions options)
        {
            if (source == null)
            {
                return null;
            }
            return new Models.App
            {
                BoundConnection = connection,
                BoundCallOptions = options,
                EngineModel = source,
                AppType = source.AppType,
                DocumentationLinks = Values.Json(source.DocumentationLinks),
                ExtendedDescription = source.ExtendedDescription,
                ExtendedDescriptionForMarketingSite = source.ExtendedDescriptionForMarketingSite,
                ExternalHomepageUrl = source.ExternalHomepageUrl,
                Featured = source.Featured,
                FolderBehaviorType = source.FolderBehaviorType,
                IconUrl = source.IconUrl,
                LogoThumbnailUrl = source.LogoThumbnailUrl,
                LogoUrl = source.LogoUrl,
                MarketingIntro = source.MarketingIntro,
                MarketingYoutubeUrl = source.MarketingYoutubeUrl,
                Name = source.Name,
                PackageManagerInstallCommand = source.PackageManagerInstallCommand,
                RemoteServerType = source.RemoteServerType,
                ScreenshotListUrls = Values.Copy(source.ScreenshotListUrls),
                SdkInstallationInstructionsLink = source.SdkInstallationInstructionsLink,
                ShortDescription = source.ShortDescription,
                SsoStrategyType = source.SsoStrategyType,
                SiemType = source.SiemType,
                TutorialYoutubeUrl = source.TutorialYoutubeUrl,
            };
        }

        internal static FilesPageSource<Models.App> List(FilesConnection connection, Dictionary<string, object> parameters, FilesCallOptions options)
        {
            return EngineCall.Pages(connection, options, (client, requestOptions) => client.Apps.List(parameters, requestOptions), source => AppBridge.Project(source, connection, options));
        }
    }
}
