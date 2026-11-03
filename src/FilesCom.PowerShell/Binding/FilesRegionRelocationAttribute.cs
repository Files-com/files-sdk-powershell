using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace FilesCom.PowerShell.Binding
{
    /// <summary>
    /// Accepts a region relocation policy as the text Automatic or Disabled, or as a hashtable of origin pairs, each key
    /// the origin Files.com's answer must come from and each value the origin, or origins, it may name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilesRegionRelocationAttribute : ArgumentTransformationAttribute
    {
        /// <summary>Converts the argument to a <see cref="FilesRegionRelocationPolicy"/>.</summary>
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            object value = inputData is PSObject wrapped ? wrapped.BaseObject : inputData;
            try
            {
                switch (value)
                {
                    case FilesRegionRelocationPolicy policy:
                        return policy;
                    case string text:
                        return FilesRegionRelocationPolicy.Parse(text);
                    case IDictionary pairs:
                        return FilesRegionRelocationPolicy.Between(pairs);
                    default:
                        throw new ArgumentException($"A region relocation policy is Automatic, Disabled or a hashtable of origin pairs, not {value?.GetType().Name ?? "null"}.");
                }
            }
            catch (ArgumentException failure)
            {
                throw new ArgumentTransformationMetadataException(failure.Message, failure);
            }
        }
    }

    /// <summary>Completes -RegionRelocation with Automatic and Disabled.</summary>
    public sealed class FilesRegionRelocationCompleter : IArgumentCompleter
    {
        private static readonly string[] Policies = { "Automatic", "Disabled" };

        /// <summary>The policies whose names start with what was typed.</summary>
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            string typed = (wordToComplete ?? string.Empty).Trim('\'', '"');
            return Policies
                .Where(policy => policy.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
                .Select(policy => new CompletionResult(policy, policy, CompletionResultType.ParameterValue, policy));
        }
    }
}
