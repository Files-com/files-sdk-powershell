using System;
using System.Globalization;
using System.Management.Automation;

namespace FilesCom.PowerShell.Binding
{
    /// <summary>
    /// Keeps every digit of a decimal parameter. PowerShell converts text such as "123456789.123456789123" to a
    /// decimal through a double, which keeps only about 15 digits; this reads text as a decimal directly, and a double,
    /// such as the literal 0.1, as the shortest text that is exactly that double.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilesDecimalAttribute : ArgumentTransformationAttribute
    {
        /// <summary>Converts the argument to a decimal without going through a double.</summary>
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            object value = inputData is PSObject wrapped ? wrapped.BaseObject : inputData;
            switch (value)
            {
                case null:
                    return null;
                case decimal exact:
                    return exact;
                case string text when decimal.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsed):
                    return parsed;
                case double number when !double.IsNaN(number) && !double.IsInfinity(number):
                    return Parse(number.ToString("R", CultureInfo.InvariantCulture));
                case float number when !float.IsNaN(number) && !float.IsInfinity(number):
                    return Parse(number.ToString("R", CultureInfo.InvariantCulture));
                case long number:
                    return (decimal)number;
                case int number:
                    return (decimal)number;
                case short number:
                    return (decimal)number;
                case byte number:
                    return (decimal)number;
                default:
                    throw new ArgumentTransformationMetadataException($"'{value}' is not a decimal number.");
            }
        }

        private static decimal Parse(string text)
        {
            if (!decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsed))
            {
                throw new ArgumentTransformationMetadataException($"{text} is outside the range of a decimal number.");
            }
            return parsed;
        }
    }
}
