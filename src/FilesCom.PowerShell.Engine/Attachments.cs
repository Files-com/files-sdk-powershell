using System.Collections.Generic;

namespace FilesCom.PowerShell.Engine
{
    // File parameters as the engine sends them. Each FilesAttachment of a request becomes the engine's FileAttachment
    // only when the operation runs, after the command was confirmed: the engine opens a local file then, and reads a
    // stream then, borrowing it, and closes only what it opened.
    internal static class Attachments
    {
        internal static Dictionary<string, object> ForEngine(Dictionary<string, object> parameters)
        {
            var result = new Dictionary<string, object>(parameters);
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                if (parameter.Value is FilesAttachment attachment)
                {
                    result[parameter.Key] = Engine(attachment);
                }
            }
            return result;
        }

        private static FileAttachment Engine(FilesAttachment attachment)
        {
            switch (attachment.SourceKind)
            {
                case FilesAttachmentSource.Bytes:
                    return FileAttachment.FromBytes(attachment.Content, attachment.FileName, attachment.ContentType);
                case FilesAttachmentSource.Stream:
                    // The module's documented default applies, whatever the engine's own default becomes.
                    return FileAttachment.FromStream(attachment.Stream, attachment.FileName, attachment.ContentType, attachment.MaxLength ?? FilesAttachment.DefaultMaxLength);
                default:
                    return FileAttachment.FromFile(attachment.LocalPath, attachment.FileName, attachment.ContentType);
            }
        }
    }
}
