using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An App represents one of the various integrations provided by Files.com.
    /// </summary>
    public sealed class App : FilesObject
    {
        internal App()
        {
        }

        /// <summary>
        /// The type of the App
        /// </summary>
        public string AppType { get; internal set; }

        /// <summary>
        /// Collection of named links to documentation
        /// </summary>
        public object DocumentationLinks { get; internal set; }

        /// <summary>
        /// Long description for the in-App landing page
        /// </summary>
        public string ExtendedDescription { get; internal set; }

        /// <summary>
        /// Long form description of the App
        /// </summary>
        public string ExtendedDescriptionForMarketingSite { get; internal set; }

        /// <summary>
        /// Link to external homepage
        /// </summary>
        public string ExternalHomepageUrl { get; internal set; }

        /// <summary>
        /// Is featured on the App listing?
        /// </summary>
        public bool Featured { get; internal set; }

        /// <summary>
        /// Associated Folder Behavior type, if any
        /// </summary>
        public string FolderBehaviorType { get; internal set; }

        /// <summary>
        /// App icon
        /// </summary>
        public string IconUrl { get; internal set; }

        /// <summary>
        /// Logo thumbnail for the App
        /// </summary>
        public string LogoThumbnailUrl { get; internal set; }

        /// <summary>
        /// Full size logo for the App
        /// </summary>
        public string LogoUrl { get; internal set; }

        /// <summary>
        /// Marketing introdution of the App
        /// </summary>
        public string MarketingIntro { get; internal set; }

        /// <summary>
        /// Marketing video page
        /// </summary>
        public string MarketingYoutubeUrl { get; internal set; }

        /// <summary>
        /// Name of the App
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Package manager install command
        /// </summary>
        public string PackageManagerInstallCommand { get; internal set; }

        /// <summary>
        /// Associated Remote Server type, if any
        /// </summary>
        public string RemoteServerType { get; internal set; }

        /// <summary>
        /// Screenshots of the App
        /// </summary>
        public string[] ScreenshotListUrls { get; internal set; }

        /// <summary>
        /// Link to SDK installation instructions
        /// </summary>
        public string SdkInstallationInstructionsLink { get; internal set; }

        /// <summary>
        /// Short description of the App
        /// </summary>
        public string ShortDescription { get; internal set; }

        /// <summary>
        /// Associated SSO Strategy type, if any
        /// </summary>
        public string SsoStrategyType { get; internal set; }

        /// <summary>
        /// Associated SIEM type, if any
        /// </summary>
        public string SiemType { get; internal set; }

        /// <summary>
        /// Tutorial video page
        /// </summary>
        public string TutorialYoutubeUrl { get; internal set; }
    }
}
