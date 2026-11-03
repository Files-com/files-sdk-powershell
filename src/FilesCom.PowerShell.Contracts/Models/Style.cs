using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Style is a custom set of branding that can be applied on a per-folder basis.
    /// </summary>
    public sealed class Style : FilesObject
    {
        internal Style()
        {
        }

        /// <summary>
        /// Style ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Folder path. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Logo
        /// </summary>
        public Models.Image Logo { get; internal set; }

        /// <summary>
        /// URL to open when a public visitor clicks the logo
        /// </summary>
        public string LogoClickHref { get; internal set; }

        /// <summary>
        /// Logo thumbnail
        /// </summary>
        public Models.Image Thumbnail { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Style source)
        {
            Id = source.Id;
            Path = source.Path;
            Logo = source.Logo;
            LogoClickHref = source.LogoClickHref;
            Thumbnail = source.Thumbnail;
        }
    }
}
