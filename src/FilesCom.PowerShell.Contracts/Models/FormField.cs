using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A FormField is a single form field used in Form Field Set.
    /// </summary>
    public sealed class FormField : FilesObject
    {
        internal FormField()
        {
        }

        /// <summary>
        /// Form field id
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Label to be displayed
        /// </summary>
        public string Label { get; internal set; }

        /// <summary>
        /// Is this a required field?
        /// </summary>
        public bool Required { get; internal set; }

        /// <summary>
        /// Help text to be displayed
        /// </summary>
        public string HelpText { get; internal set; }

        /// <summary>
        /// Type of Field
        /// </summary>
        public string FieldType { get; internal set; }

        /// <summary>
        /// Options to display for radio and dropdown
        /// </summary>
        public string[] OptionsForSelect { get; internal set; }

        /// <summary>
        /// Default option for radio and dropdown
        /// </summary>
        public string DefaultOption { get; internal set; }

        /// <summary>
        /// Form field set id
        /// </summary>
        public long? FormFieldSetId { get; internal set; }
    }
}
