namespace FilesCom.PowerShell
{
    /// <summary>
    /// The base of the objects FilesCom commands write for Files.com resources, such as
    /// <see cref="Models.User"/>. Each holds what Files.com returned for the resource.
    /// </summary>
    /// <remarks>
    /// An object remembers the connection and the workspace that fetched it, so a command given it as
    /// <c>-InputObject</c> acts on it with the same connection and in the same workspace, including workspace 0 or no
    /// workspace, unless that command is given <c>-WorkspaceScope</c> or <c>-NoWorkspaceScope</c> itself. Commands not
    /// given the object still use their connection's workspace. An object read back from serialized data, such as
    /// <c>Import-Clixml</c> output, is data only: act on its resource by passing its ID or path with <c>-Connection</c>.
    /// </remarks>
    public abstract class FilesObject
    {
        internal FilesObject()
        {
        }

        internal FilesConnection BoundConnection { get; set; }

        // The workspace options of the request that read the object.
        internal FilesCallOptions BoundCallOptions { get; set; }

        // The engine's own object this one was projected from, whose operations keep its connection's client, its
        // options and the address it was read from, such as that of its site's region. Null for an object no engine
        // call made.
        internal object EngineModel { get; set; }
    }
}
