using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An Integration Centric Profile defines the Remote Server integrations a user is expected to add and connect during integration-centric onboarding.
    /// </summary>
    public sealed class IntegrationCentricProfile : FilesObject
    {
        internal IntegrationCentricProfile()
        {
        }

        /// <summary>
        /// Integration Centric Profile ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Profile name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Workspace ID
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Whether this profile applies to all users in the Workspace by default
        /// </summary>
        public bool UseForAllUsers { get; internal set; }

        /// <summary>
        /// Remote Server integrations the user is expected to add and connect. Each entry requires `server_type` and may include a display `name`.
        /// </summary>
        public object[] ExpectedRemoteServers { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(IntegrationCentricProfile source)
        {
            Id = source.Id;
            Name = source.Name;
            WorkspaceId = source.WorkspaceId;
            UseForAllUsers = source.UseForAllUsers;
            ExpectedRemoteServers = source.ExpectedRemoteServers;
        }
    }
}
