namespace FilesCom.PowerShell.Engine
{
    // The roots of Files.com's folders for the external destinations a file can be sent to, from the generator's shared
    // list that the SDK's own external-destination helpers are generated from.
    internal static class ExternalDestinations
    {
        // The remote server with a given ID is "_/RemoteServers/<id>".
        internal const string RemoteServer = "RemoteServers";
        // The snapshot with a given ID is "_/Snapshots/<id>".
        internal const string Snapshot = "Snapshots";
        // The child site with a given ID is "_/Sites/<id>".
        internal const string ChildSite = "Sites";
    }
}
