using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Numerics;

namespace FilesCom.PowerShell.Binding
{
    /// <summary>
    /// Accepts only whole numbers for an Int64 parameter, or each element of an Int64 array. PowerShell would
    /// otherwise round 2.5 to 2 or accept "2.5" as 2 before the command could see the value; an ID or count that
    /// is not a whole number, or does not fit in an Int64, is refused instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilesIntegerAttribute : ArgumentTransformationAttribute
    {
        /// <summary>Converts the argument to Int64 values without rounding.</summary>
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            object value = Unwrap(inputData);
            if (value == null || value is string || !(value is IEnumerable sequence))
            {
                return value == null ? null : (object)ToInt64(value);
            }
            var values = new List<long>();
            foreach (object item in sequence)
            {
                values.Add(ToInt64(Unwrap(item)));
            }
            return values.ToArray();
        }

        internal static object Unwrap(object value)
        {
            return value is PSObject wrapped ? wrapped.BaseObject : value;
        }

        internal static long ToInt64(object value)
        {
            switch (value)
            {
                case long exact:
                    return exact;
                case int number:
                    return number;
                case short number:
                    return number;
                case byte number:
                    return number;
                case sbyte number:
                    return number;
                case ushort number:
                    return number;
                case uint number:
                    return number;
                case ulong number when number <= long.MaxValue:
                    return (long)number;
                case BigInteger number when number >= long.MinValue && number <= long.MaxValue:
                    return (long)number;
                case decimal number when decimal.Truncate(number) == number && number >= long.MinValue && number <= long.MaxValue:
                    return (long)number;
                case double number when Math.Truncate(number) == number && number >= -9223372036854775808.0 && number < 9223372036854775808.0:
                    return (long)number;
                case float number when Math.Truncate(number) == number && number >= -9223372036854775808.0f && number < 9223372036854775808.0f:
                    return (long)number;
                case string text when long.TryParse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long parsed):
                    return parsed;
                case null:
                    throw new ArgumentTransformationMetadataException("A whole number is required, but the value is null.");
                default:
                    throw new ArgumentTransformationMetadataException($"'{value}' is not a whole number that fits in an Int64.");
            }
        }
    }
}
