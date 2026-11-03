namespace FilesCom.PowerShell
{
    // Options one command applies to its own requests, leaving its connection unchanged.
    internal sealed class FilesCallOptions
    {
        internal static readonly FilesCallOptions None = new FilesCallOptions(null, clearWorkspace: false);

        internal FilesCallOptions(long? workspace, bool clearWorkspace)
        {
            Workspace = workspace;
            ClearWorkspace = clearWorkspace;
        }

        // The workspace to run in instead of the connection's, including 0.
        internal long? Workspace { get; }

        // Run in no workspace, even if the connection has one.
        internal bool ClearWorkspace { get; }
    }
}
