using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace FilesCom.PowerShell.Binding
{
    /// <summary>
    /// Accepts a hashtable, other dictionary or PSCustomObject for an object parameter, or an array of them. Property
    /// names keep their case and values keep their types, including $false, 0, empty text and $null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilesDictionaryAttribute : ArgumentTransformationAttribute
    {
        /// <summary>Converts PSCustomObject values to dictionaries.</summary>
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            object value = inputData is PSObject wrapped && !(wrapped.BaseObject is PSCustomObject) ? wrapped.BaseObject : inputData;
            if (value is IDictionary || value == null)
            {
                return value;
            }
            if (IsCustomObject(value))
            {
                return ToDictionary((PSObject)value);
            }
            if (value is IEnumerable sequence && !(value is string))
            {
                var dictionaries = new List<IDictionary>();
                foreach (object item in sequence)
                {
                    dictionaries.Add((IDictionary)Transform(engineIntrinsics, item) ?? throw new ArgumentTransformationMetadataException("An element of the array is null."));
                }
                return dictionaries.ToArray();
            }
            throw new ArgumentTransformationMetadataException($"A hashtable or PSCustomObject is required, not {value.GetType().Name}.");
        }

        internal static bool IsCustomObject(object value)
        {
            return value is PSObject wrapped && wrapped.BaseObject is PSCustomObject;
        }

        // The PSCustomObject's note properties, in order, as an ordinal (case-preserving) dictionary.
        internal static IDictionary ToDictionary(PSObject value)
        {
            var dictionary = new System.Collections.Specialized.OrderedDictionary(StringComparer.Ordinal);
            foreach (PSPropertyInfo property in value.Properties)
            {
                dictionary.Add(property.Name, property.Value);
            }
            return dictionary;
        }
    }
}
