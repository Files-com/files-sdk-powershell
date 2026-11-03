using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FilesCom.PowerShell.Engine
{
    // Converts SDK model values to the module's: .NET base types only, with every JSON value System.Text.Json left in
    // an object property turned into dictionaries, arrays and plain values.
    internal static class Values
    {
        // A date the SDK read as local time is the same instant in UTC, so every date the module writes is UTC.
        internal static DateTime? Utc(DateTime? value)
        {
            return value.HasValue && value.Value.Kind == DateTimeKind.Local ? value.Value.ToUniversalTime() : value;
        }

        internal static FilesSecret Secret(string value)
        {
            return value == null ? null : new FilesSecret(value);
        }

        internal static T[] Copy<T>(T[] values)
        {
            return values == null ? null : (T[])values.Clone();
        }

        internal static Dictionary<string, string[]> Copy(Dictionary<string, string[]> values)
        {
            return values?.ToDictionary(entry => entry.Key, entry => Copy(entry.Value), StringComparer.Ordinal);
        }

        internal static TResult[] Array<TSource, TResult>(TSource[] values, Func<TSource, TResult> project)
        {
            return values?.Select(project).ToArray();
        }

        internal static object[] JsonArray(object[] values)
        {
            return values?.Select(Json).ToArray();
        }

        // Object properties hold whatever JSON Files.com returned: an object becomes an ordinal dictionary, so keys
        // differing only in case stay distinct; a whole number stays an Int64; other numbers are decimals where exact.
        internal static object Json(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case JsonElement element:
                    return Json(element);
                case Dictionary<string, object> dictionary:
                    return dictionary.ToDictionary(entry => entry.Key, entry => Json(entry.Value), StringComparer.Ordinal);
                case object[] array:
                    return JsonArray(array);
                default:
                    return value;
            }
        }

        private static object Json(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
                    foreach (JsonProperty property in element.EnumerateObject())
                    {
                        dictionary[property.Name] = Json(property.Value);
                    }
                    return dictionary;
                case JsonValueKind.Array:
                    return element.EnumerateArray().Select(Json).ToArray();
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long whole))
                    {
                        return whole;
                    }
                    if (element.TryGetDecimal(out decimal exact))
                    {
                        return exact;
                    }
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                default:
                    return null;
            }
        }
    }
}
