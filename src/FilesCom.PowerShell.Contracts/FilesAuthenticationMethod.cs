namespace FilesCom.PowerShell
{
    /// <summary>How a <see cref="FilesConnection"/> authenticates.</summary>
    public enum FilesAuthenticationMethod
    {
        /// <summary>An API key the caller supplied. Disconnecting never revokes it.</summary>
        ApiKey,

        /// <summary>A session ID the caller supplied. Disconnecting never revokes it.</summary>
        Session,

        /// <summary>
        /// A session Connect-FilesAccount created by signing in with a username and password. Disconnecting revokes it.
        /// </summary>
        Credential,
    }
}
