using System;
using System.Collections.Generic;
using System.Management.Automation;
using FilesCom.PowerShell.Binding;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// List Holiday Calendars, or get one holiday calendar by Id.
    /// </summary>
    [Cmdlet("Get", "FilesHolidayCalendar", DefaultParameterSetName = "List", SupportsPaging = true)]
    [OutputType(typeof(Models.HolidayCalendar), ParameterSetName = new[] { "List" })]
    [OutputType(typeof(FilesPage<Models.HolidayCalendar>), ParameterSetName = new[] { "Page", "Continuation" })]
    [OutputType(typeof(Models.HolidayCalendar), ParameterSetName = new[] { "Id" })]
    public sealed class GetFilesHolidayCalendarCommand : FilesListCmdlet<Models.HolidayCalendar>
    {
        /// <summary>
        /// Used for pagination.  When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`.  Send one of those cursor value here to resume an existing list from the next available record.  Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [ValidateNotNull]
        public string Cursor { get; set; }

        /// <summary>
        /// Number of records to show per page.  (Max: 10000, 1,000 or less is recommended).
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesInteger]
        [ValidateNotNull]
        public long? PerPage { get; set; }

        /// <summary>
        /// If set, sort records by the specified field in either `asc` or `desc` direction. Valid fields are .
        /// </summary>
        [Parameter(ParameterSetName = "List")]
        [Parameter(ParameterSetName = "Page")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary SortBy { get; set; }

        /// <summary>
        /// Holiday Calendar ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>List Holiday Calendars, or get one holiday calendar by Id.</summary>
        protected override void Execute()
        {
            if (ParameterSetName == "Id")
            {
                FilesConnection connection = ResolveConnection();
                long id = Id;
                ErrorTarget = id;
                Dictionary<string, object> parameters = FindParameters();
                Models.HolidayCalendar result = Run(connection, token => HolidayCalendarBridge.FindAsync(connection, id, parameters, CallOptions, token));
                WriteRecord(result);
                return;
            }
            WriteList(connection => HolidayCalendarBridge.List(connection, ListParameters(), CallOptions));
        }

        private Dictionary<string, object> ListParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Cursor)))
            {
                parameters["cursor"] = Cursor;
            }
            if (IsBound(nameof(PerPage)))
            {
                parameters["per_page"] = PerPage.Value;
            }
            if (IsBound(nameof(SortBy)))
            {
                parameters["sort_by"] = RequestValues.QueryObject(SortBy, "SortBy");
            }
            return parameters;
        }

        private Dictionary<string, object> FindParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }

    /// <summary>
    /// Create Holiday Calendar.
    /// </summary>
    [Cmdlet("New", "FilesHolidayCalendar", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.HolidayCalendar))]
    public sealed class NewFilesHolidayCalendarCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Holiday rules for the calendar.
        /// </summary>
        [Parameter(Mandatory = true)]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Definition { get; set; }

        /// <summary>
        /// Holiday Calendar name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [AllowEmptyString]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>Create Holiday Calendar.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection();
            Dictionary<string, object> parameters = CreateParameters();
            if (!ShouldProcessOn(connection, IsBound(nameof(Name)) ? $"Holiday calendar '{Name}'" : "Holiday calendar"))
            {
                return;
            }
            Models.HolidayCalendar result = Run(connection, token => HolidayCalendarBridge.CreateAsync(connection, parameters, CallOptions, token));
            WriteObject(result);
        }

        private Dictionary<string, object> CreateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Definition)))
            {
                parameters["definition"] = RequestValues.Object(Definition, "Definition");
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Update Holiday Calendar.
    /// </summary>
    [Cmdlet("Set", "FilesHolidayCalendar", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Models.HolidayCalendar), ParameterSetName = new[] { "Id", "InputObject" })]
    public sealed class SetFilesHolidayCalendarCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Holiday Calendar ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// Holiday rules for the calendar.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [FilesDictionary]
        [ValidateNotNull]
        public System.Collections.IDictionary Definition { get; set; }

        /// <summary>
        /// Holiday Calendar name.
        /// </summary>
        [Parameter(ParameterSetName = "Id")]
        [Parameter(ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public string Name { get; set; }

        /// <summary>
        /// The holiday calendar to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.HolidayCalendar InputObject { get; set; }

        /// <summary>Update Holiday Calendar.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = UpdateParameters();
            if (!ShouldProcessOn(connection, $"Holiday calendar {id}"))
            {
                return;
            }
            Models.HolidayCalendar result = Run(connection, token => HolidayCalendarBridge.UpdateAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
            if (InputObject != null)
            {
                InputObject.CopyFrom(result);
                result = InputObject;
            }
            WriteObject(result);
        }

        private Dictionary<string, object> UpdateParameters()
        {
            var parameters = new Dictionary<string, object>();
            if (IsBound(nameof(Definition)))
            {
                parameters["definition"] = RequestValues.Object(Definition, "Definition");
            }
            if (IsBound(nameof(Name)))
            {
                parameters["name"] = Name;
            }
            return parameters;
        }
    }

    /// <summary>
    /// Delete Holiday Calendar.
    /// </summary>
    [Cmdlet("Remove", "FilesHolidayCalendar", DefaultParameterSetName = "Id", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveFilesHolidayCalendarCommand : FilesApiCmdlet
    {
        /// <summary>
        /// Holiday Calendar ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Id")]
        [FilesInteger]
        [ValidateNotNull]
        public long Id { get; set; }

        /// <summary>
        /// The holiday calendar to act on, such as one written by another FilesCom command. The command uses its
        /// Id and the connection that fetched it.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "InputObject")]
        [ValidateNotNull]
        public Models.HolidayCalendar InputObject { get; set; }

        /// <summary>Delete Holiday Calendar.</summary>
        protected override void Execute()
        {
            FilesConnection connection = ResolveConnection(InputObject);
            long id = InputObject != null ? InputObject.Id ?? throw new FilesException(FilesErrorKind.InvalidArgument, "The input object has no Id.") : Id;
            ErrorTarget = id;
            Dictionary<string, object> parameters = DeleteParameters();
            if (!ShouldProcessOn(connection, $"Holiday calendar {id}"))
            {
                return;
            }
            Run(connection, token => HolidayCalendarBridge.DeleteAsync(connection, id, parameters, CallOptionsFor(InputObject), RetainedModel(InputObject), token));
        }

        private Dictionary<string, object> DeleteParameters()
        {
            var parameters = new Dictionary<string, object>();
            return parameters;
        }
    }
}
