using System;
using System.Globalization;
using System.Management.Automation;

namespace FilesCom.PowerShell.Binding
{
    /// <summary>
    /// Accepts a date and time that identifies an instant: a DateTimeOffset, a DateTime whose Kind is Utc or Local
    /// (such as Get-Date returns), or ISO 8601 text with an offset or Z. A DateTime of unspecified kind, or text
    /// without an offset, is refused rather than guessed to be local time or UTC.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilesDateTimeAttribute : ArgumentTransformationAttribute
    {
        private static readonly string[] Iso8601Formats =
        {
            "yyyy-MM-dd'T'HH:mm:ssK",
            "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
            "yyyy-MM-dd'T'HH:mmK",
        };

        /// <summary>Converts the argument to a DateTimeOffset that keeps its instant and offset.</summary>
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            object value = inputData is PSObject wrapped ? wrapped.BaseObject : inputData;
            switch (value)
            {
                case null:
                    return null;
                case DateTimeOffset offset:
                    return offset;
                case DateTime dateTime when dateTime.Kind == DateTimeKind.Unspecified:
                    throw new ArgumentTransformationMetadataException($"The date {dateTime:o} does not say whether it is UTC or local time. Use Get-Date, [datetime]::SpecifyKind, or ISO 8601 text with an offset such as 2026-01-31T09:00:00-05:00.");
                case DateTime dateTime:
                    return new DateTimeOffset(dateTime);
                case string text when DateTimeOffset.TryParseExact(text.Trim(), Iso8601Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset parsed) && HasOffset(text):
                    return parsed;
                default:
                    throw new ArgumentTransformationMetadataException($"'{value}' is not a date and time with an offset. Use ISO 8601 text such as 2026-01-31T14:00:00Z, a DateTimeOffset, or a DateTime from Get-Date.");
            }
        }

        // DateTimeOffset parsing assumes local time when the text has no offset, which the attribute refuses.
        private static bool HasOffset(string text)
        {
            string trimmed = text.Trim();
            if (trimmed.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            int timeStart = trimmed.IndexOf('T');
            return timeStart >= 0 && (trimmed.IndexOf('+', timeStart) >= 0 || trimmed.IndexOf('-', timeStart) >= 0);
        }
    }
}
