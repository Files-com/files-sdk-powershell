using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Security;

namespace FilesCom.PowerShell.Binding
{
    /// <summary>
    /// Converts bound PowerShell arguments to the values the engine sends. Values keep their type and exact text;
    /// nothing is rounded, trimmed, case-folded or split.
    /// </summary>
    internal static class RequestValues
    {
        // The SecureString's text, made plain only here, where the request needs it.
        internal static string Reveal(SecureString value)
        {
            return new NetworkCredential(string.Empty, value).Password;
        }

        // ISO 8601 text keeping the instant and offset: Z for UTC, the offset otherwise, and fractional seconds only
        // when there are some, as the .NET SDK writes dates.
        internal static string DateTime(DateTimeOffset value)
        {
            return value.Offset == TimeSpan.Zero
                ? value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", CultureInfo.InvariantCulture)
                : value.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz", CultureInfo.InvariantCulture);
        }

        // An object parameter sent in a request body: nested dictionaries, PSCustomObjects and arrays, with every
        // key's case and every value, including $false, 0, empty text and $null.
        internal static Dictionary<string, object> Object(IDictionary value, string parameter)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in value)
            {
                string key = Key(entry.Key, parameter);
                if (result.ContainsKey(key))
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-{parameter} has the key '{key}' twice.");
                }
                result[key] = BodyValue(entry.Value, parameter);
            }
            return result;
        }

        internal static object[] Objects(IDictionary[] values, string parameter)
        {
            return values.Select(value => (object)Object(value ?? throw Null(parameter), parameter)).ToArray();
        }

        // An object parameter of a GET request, sent as parameter[key]=value pairs of text, which is all a query
        // string holds. Its values must be single values.
        internal static Dictionary<string, string> QueryObject(IDictionary value, string parameter)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in value)
            {
                string key = Key(entry.Key, parameter);
                if (result.ContainsKey(key))
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-{parameter} has the key '{key}' twice.");
                }
                result[key] = QueryText(Unwrap(entry.Value), parameter, key);
            }
            return result;
        }

        internal static string[] Strings(string[] values, string parameter)
        {
            if (values.Any(value => value == null))
            {
                throw Null(parameter);
            }
            return values;
        }

        internal static long?[] Int64s(long[] values)
        {
            return values.Select(value => (long?)value).ToArray();
        }

        // An ID list the API takes as comma-delimited text.
        internal static string JoinInt64s(long[] values)
        {
            return string.Join(",", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
        }

        // A list the API takes as text joined by its documented delimiter.
        internal static string Join(string[] values, string parameter, string delimiter)
        {
            return string.Join(delimiter, Strings(values, parameter));
        }

        // A file parameter's attachment, checked before the command asks to confirm anything: its local file must exist
        // and its stream be readable. Nothing is opened or read here; the engine does that when the request is sent.
        internal static object Attachment(FilesAttachment value, string parameter)
        {
            if (value == null)
            {
                throw Null(parameter);
            }
            if (value.SourceKind == FilesAttachmentSource.File && !File.Exists(value.LocalPath))
            {
                throw new FileNotFoundException($"The local file {value.LocalPath} for -{parameter} does not exist.", value.LocalPath);
            }
            if (value.SourceKind == FilesAttachmentSource.Stream && !value.Stream.CanRead)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The stream for -{parameter} cannot be read.");
            }
            return value;
        }

        private static string Key(object key, string parameter)
        {
            object unwrapped = Unwrap(key);
            if (!(unwrapped is string text))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"-{parameter} keys must be text, not {unwrapped?.GetType().Name ?? "null"}.");
            }
            return text;
        }

        private static object BodyValue(object value, string parameter)
        {
            object unwrapped = FilesDictionaryAttribute.IsCustomObject(value) ? FilesDictionaryAttribute.ToDictionary((PSObject)value) : Unwrap(value);
            switch (unwrapped)
            {
                case null:
                case string _:
                    return unwrapped;
                case SwitchParameter flag:
                    return flag.IsPresent;
                case IDictionary nested:
                    return Object(nested, parameter);
                case IEnumerable sequence:
                    return sequence.Cast<object>().Select(item => BodyValue(item, parameter)).ToArray();
                default:
                    return unwrapped;
            }
        }

        private static string QueryText(object value, string parameter, string key)
        {
            switch (value)
            {
                case null:
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-{parameter} has no value for '{key}'; a query value cannot be null.");
                case string text:
                    return text;
                case bool flag:
                    return flag ? "true" : "false";
                case SwitchParameter flag:
                    return flag.IsPresent ? "true" : "false";
                case DateTimeOffset offset:
                    return DateTime(offset);
                case System.DateTime dateTime when dateTime.Kind != DateTimeKind.Unspecified:
                    return DateTime(new DateTimeOffset(dateTime));
                case IFormattable number when IsNumber(number):
                    return number.ToString(null, CultureInfo.InvariantCulture);
                default:
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-{parameter} value for '{key}' must be text, a number, a Boolean or a date with a time zone, not {value.GetType().Name}.");
            }
        }

        private static bool IsNumber(object value)
        {
            return value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint
                || value is long || value is ulong || value is float || value is double || value is decimal;
        }

        private static object Unwrap(object value)
        {
            return value is PSObject wrapped ? wrapped.BaseObject : value;
        }

        private static FilesException Null(string parameter)
        {
            return new FilesException(FilesErrorKind.InvalidArgument, $"-{parameter} contains a null element.");
        }
    }
}
