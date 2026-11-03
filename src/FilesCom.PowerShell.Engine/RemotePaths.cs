using System.Collections.Generic;
using System.Globalization;
using FilesCom.Util;

namespace FilesCom.PowerShell.Engine
{
    // Remote path rules the commands need before sending anything, from the SDK, which follows Files.com's own.
    internal static class RemotePaths
    {
        // The key two remote paths share when Files.com treats them as the same path: it compares paths without regard
        // to case, accents or redundant slashes.
        internal static string ComparisonKey(string remotePath)
        {
            return PathUtil.normalize_for_comparison(remotePath);
        }

        // The path of a file sent to an external destination: Files.com's folder for it, "_/<root>/<id>" with a root of
        // ExternalDestinations, then pathWithin. As the SDK's external-destination helpers do, either slash separates
        // folders, and empty, "." and ".." folders and null characters are left out, each folder cleaned by the SDK's own
        // rule, so the path cannot leave the destination. Null when pathWithin names nothing within it.
        internal static string InExternalDestination(string root, long id, string pathWithin)
        {
            var folders = new List<string> { "_", root, id.ToString(CultureInfo.InvariantCulture) };
            foreach (string folder in pathWithin.Replace('\\', '/').Split('/'))
            {
                string clean = PathUtil.cleanpath(folder);
                if (clean.Length > 0)
                {
                    folders.Add(clean);
                }
            }
            return folders.Count > 3 ? string.Join("/", folders) : null;
        }
    }
}
