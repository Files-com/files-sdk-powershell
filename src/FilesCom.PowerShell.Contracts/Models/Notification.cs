using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A Notification is our feature that sends E-Mails when specific actions occur in the folder.
    /// </summary>
    public sealed class Notification : FilesObject
    {
        internal Notification()
        {
        }

        /// <summary>
        /// Notification ID
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Folder path to notify on. This must be slash-delimited, but it must neither start nor end with a slash. Maximum of 5000 characters.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// ID of Group to receive notifications
        /// </summary>
        public long? GroupId { get; internal set; }

        /// <summary>
        /// Group name, if a Group ID is set
        /// </summary>
        public string GroupName { get; internal set; }

        /// <summary>
        /// Group IDs when the notification requires multiple groups
        /// </summary>
        public long?[] GroupIds { get; internal set; }

        /// <summary>
        /// Group names when the notification requires multiple groups
        /// </summary>
        public string[] GroupNames { get; internal set; }

        /// <summary>
        /// If set, will only notify on actions made by a member of one of the specified groups
        /// </summary>
        public long?[] TriggeringGroupIds { get; internal set; }

        /// <summary>
        /// If set, will only notify on actions made one of the specified users
        /// </summary>
        public long?[] TriggeringUserIds { get; internal set; }

        /// <summary>
        /// Notify when actions are performed by a share recipient?
        /// </summary>
        public bool TriggerByShareRecipients { get; internal set; }

        /// <summary>
        /// If true, will send notifications about a user's own activity to that user.  If false, only activity performed by other users (or anonymous users) will be sent in notifications.
        /// </summary>
        public bool NotifyUserActions { get; internal set; }

        /// <summary>
        /// Trigger on files copied to this path?
        /// </summary>
        public bool NotifyOnCopy { get; internal set; }

        /// <summary>
        /// Trigger on files deleted in this path?
        /// </summary>
        public bool NotifyOnDelete { get; internal set; }

        /// <summary>
        /// Trigger on files downloaded in this path?
        /// </summary>
        public bool NotifyOnDownload { get; internal set; }

        /// <summary>
        /// Trigger on files moved to this path?
        /// </summary>
        public bool NotifyOnMove { get; internal set; }

        /// <summary>
        /// Trigger on files created/uploaded/updated/changed in this path?
        /// </summary>
        public bool NotifyOnUpload { get; internal set; }

        /// <summary>
        /// Apply notification recursively?  This will enable notifications for each subfolder.
        /// </summary>
        public bool Recursive { get; internal set; }

        /// <summary>
        /// The time interval that notifications are aggregated to
        /// </summary>
        public string SendInterval { get; internal set; }

        /// <summary>
        /// Custom subject line to use for notification emails
        /// </summary>
        public string Subject { get; internal set; }

        /// <summary>
        /// Custom message to include in notification emails
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Array of filenames (possibly with wildcards) to scope trigger
        /// </summary>
        public string[] TriggeringFilenames { get; internal set; }

        /// <summary>
        /// Workspace ID. `0` means the default workspace.
        /// </summary>
        public long? WorkspaceId { get; internal set; }

        /// <summary>
        /// Is the user unsubscribed from this notification?
        /// </summary>
        public bool Unsubscribed { get; internal set; }

        /// <summary>
        /// The reason that the user unsubscribed
        /// </summary>
        public string UnsubscribedReason { get; internal set; }

        /// <summary>
        /// Notification user ID
        /// </summary>
        public long? UserId { get; internal set; }

        /// <summary>
        /// Notification username
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// If true, it means that the recipient at this user's email address has manually unsubscribed from all emails, or had their email "hard bounce", which means that we are unable to send mail to this user's current email address. Notifications will resume if the user changes their email address.
        /// </summary>
        public bool SuppressedEmail { get; internal set; }

        // Takes the values Files.com returned for this same resource after a change, so an -InputObject shows its
        // resource as it now is. The object keeps its connection.
        internal void CopyFrom(Notification source)
        {
            Id = source.Id;
            Path = source.Path;
            GroupId = source.GroupId;
            GroupName = source.GroupName;
            GroupIds = source.GroupIds;
            GroupNames = source.GroupNames;
            TriggeringGroupIds = source.TriggeringGroupIds;
            TriggeringUserIds = source.TriggeringUserIds;
            TriggerByShareRecipients = source.TriggerByShareRecipients;
            NotifyUserActions = source.NotifyUserActions;
            NotifyOnCopy = source.NotifyOnCopy;
            NotifyOnDelete = source.NotifyOnDelete;
            NotifyOnDownload = source.NotifyOnDownload;
            NotifyOnMove = source.NotifyOnMove;
            NotifyOnUpload = source.NotifyOnUpload;
            Recursive = source.Recursive;
            SendInterval = source.SendInterval;
            Subject = source.Subject;
            Message = source.Message;
            TriggeringFilenames = source.TriggeringFilenames;
            WorkspaceId = source.WorkspaceId;
            Unsubscribed = source.Unsubscribed;
            UnsubscribedReason = source.UnsubscribedReason;
            UserId = source.UserId;
            Username = source.Username;
            SuppressedEmail = source.SuppressedEmail;
        }
    }
}
