using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace FilesCom.PowerShell.Binding
{
    /// <summary>
    /// Accepts part size tiers as a hashtable of file sizes and the part sizes files of up to that size start with, such
    /// as <c>@{ 256MB = 8MB; 4GB = 16MB }</c>, or as tiers, such as another tuning's KnownSizeTiers. Sizes are whole
    /// numbers of bytes, never rounded; the tiers are put in order of file size.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilesPartSizeTiersAttribute : ArgumentTransformationAttribute
    {
        /// <summary>Converts the argument to tiers in order of file size.</summary>
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            object value = FilesIntegerAttribute.Unwrap(inputData);
            var tiers = new List<FilesPartSizeTier>();
            switch (value)
            {
                case IDictionary sizes:
                    foreach (DictionaryEntry entry in sizes)
                    {
                        tiers.Add(new FilesPartSizeTier(FilesIntegerAttribute.ToInt64(FilesIntegerAttribute.Unwrap(entry.Key)), FilesIntegerAttribute.ToInt64(FilesIntegerAttribute.Unwrap(entry.Value))));
                    }
                    break;
                case FilesPartSizeTier tier:
                    tiers.Add(tier);
                    break;
                case IEnumerable sequence when !(value is string):
                    foreach (object item in sequence)
                    {
                        if (!(FilesIntegerAttribute.Unwrap(item) is FilesPartSizeTier tier))
                        {
                            throw new ArgumentTransformationMetadataException($"Part size tiers are a hashtable of file sizes and part sizes, such as @{{ 256MB = 8MB }}, or tiers, not {item?.GetType().Name ?? "null"}.");
                        }
                        tiers.Add(tier);
                    }
                    break;
                default:
                    throw new ArgumentTransformationMetadataException($"Part size tiers are a hashtable of file sizes and part sizes, such as @{{ 256MB = 8MB }}, not {value?.GetType().Name ?? "null"}.");
            }
            return tiers.OrderBy(tier => tier.MaxFileBytes).ToArray();
        }
    }
}
