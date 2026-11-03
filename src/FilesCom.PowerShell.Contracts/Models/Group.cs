using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Group is a powerful tool for permissions and user management on Files.com.
    /// </summary>
    public sealed class Group : FilesObject
    {
        internal Group()
        {
        }

        /// <summary>
        /// Group ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Group name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// A list of allowed IPs if applicable.  Newline delimited
        /// </summary>
        public string AllowedIps { get; internal set; }

        /// <summary>
        /// Comma-delimited list of user IDs who are group administrators (separated by commas)
        /// </summary>
        public string AdminIds { get; internal set; }

        /// <summary>
        /// Notes about this group
        /// </summary>
        public string Notes { get; internal set; }

        /// <summary>
        /// Comma-delimited list of user IDs who belong to this group (separated by commas)
        /// </summary>
        public string UserIds { get; internal set; }

        /// <summary>
        /// Comma-delimited list of usernames who belong to this group (separated by commas)
        /// </summary>
        public string Usernames { get; internal set; }

        /// <summary>
        /// AI Assistant Personality ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user or Partner assignment overrides it.
        /// </summary>
        public long? AiAssistantPersonalityId { get; internal set; }

        /// <summary>
        /// If true, users in this group can use FTP to login.  This will override a false value of `ftp_permission` on the user level.
        /// </summary>
        public bool FtpPermission { get; internal set; }

        /// <summary>
        /// If true, users in this group can use SFTP to login.  This will override a false value of `sftp_permission` on the user level.
        /// </summary>
        public bool SftpPermission { get; internal set; }

        /// <summary>
        /// If true, users in this group can use WebDAV to login.  This will override a false value of `dav_permission` on the user level.
        /// </summary>
        public bool DavPermission { get; internal set; }

        /// <summary>
        /// If true, users in this group can use the REST API to login.  This will override a false value of `restapi_permission` on the user level.
        /// </summary>
        public bool RestapiPermission { get; internal set; }

        /// <summary>
        /// If true, users in this group can access the S3-compatible endpoint. This will override a false value of `s3_compatible_endpoint_permission` on the user level. Defaults to false.
        /// </summary>
        public bool S3CompatibleEndpointPermission { get; internal set; }

        /// <summary>
        /// Desktop Configuration Profile ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        public long? DesktopConfigurationProfileId { get; internal set; }

        /// <summary>
        /// Integration Centric Profile ID assigned to this Group, if any. Users in the Group inherit it unless a direct per-user assignment overrides it.
        /// </summary>
        public long? IntegrationCentricProfileId { get; internal set; }

        /// <summary>
        /// Site ID
        /// </summary>
        public long? SiteId { get; internal set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Group source)
        {
            Id = source.Id;
            Name = source.Name;
            AllowedIps = source.AllowedIps;
            AdminIds = source.AdminIds;
            Notes = source.Notes;
            UserIds = source.UserIds;
            Usernames = source.Usernames;
            AiAssistantPersonalityId = source.AiAssistantPersonalityId;
            FtpPermission = source.FtpPermission;
            SftpPermission = source.SftpPermission;
            DavPermission = source.DavPermission;
            RestapiPermission = source.RestapiPermission;
            S3CompatibleEndpointPermission = source.S3CompatibleEndpointPermission;
            DesktopConfigurationProfileId = source.DesktopConfigurationProfileId;
            IntegrationCentricProfileId = source.IntegrationCentricProfileId;
            SiteId = source.SiteId;
            WorkspaceId = source.WorkspaceId;
        }
    }
}
