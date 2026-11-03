using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Text;
using FilesCom.PowerShell.Engine;
using FilesCom.PowerShell.Runtime;

namespace FilesCom.PowerShell.Commands
{
    /// <summary>
    /// Writes a whole remote file from everything it is given: strings, as UTF-8 text without a byte order mark with each
    /// string after the first on a new line, or bytes, as they are. The file is uploaded once, after the last value has
    /// arrived, creating the file or replacing it once the upload completes. The content is held in memory; use
    /// Send-FilesFile for a large file or a stream.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "FilesFileContent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class SetFilesFileContentCommand : FilesApiCmdlet
    {
        // The largest byte array .NET allows, and so the most content the command can hold.
        private const long MaxContentLength = 0x7FFFFFC7;

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private FilesConnection connection;

        // Whether a value arrived, even an empty one: with none, the pipeline gave nothing to write.
        private bool given;

        // Whether the content is text or bytes, once a value says which.
        private bool? text;

        // The content while it is one value; once a second value arrives, all of it.
        private byte[] single;
        private MemoryStream joined;
        private long length;
        private int pieces;

        /// <summary>The remote file, used exactly as written.</summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// The content: strings, written as UTF-8 with each string after the first on a new line, or bytes and byte arrays,
        /// written as they were when given. Every value piped goes into the one file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        [AllowEmptyString]
        [AllowEmptyCollection]
        [AllowNull]
        public object Value { get; set; }

        /// <summary>Creates missing folders above the file.</summary>
        [Parameter]
        public SwitchParameter MkdirParents { get; set; }

        /// <summary>Adds the current value to the content, refusing one that cannot be written before anything is sent.</summary>
        protected override void Execute()
        {
            connection = connection ?? ResolveConnection();
            ErrorTarget = Path;
            given = true;
            object value = Unwrap(Value);
            // A list given to -Value, such as several strings, is its values; a byte array is one value.
            if (value is IList values && !(value is byte[]))
            {
                foreach (object item in values)
                {
                    Add(Unwrap(item));
                }
                return;
            }
            Add(value);
        }

        /// <summary>Writes the content once every value has arrived; with no value piped, writes nothing.</summary>
        protected override void EndProcessing()
        {
            if (!given)
            {
                return;
            }
            try
            {
                // Asked before the content is put together, so -WhatIf or a declined -Confirm allocates nothing more.
                if (!ShouldProcessOn(connection, $"{length} bytes to file '{Path}', creating or replacing it"))
                {
                    return;
                }
                byte[] content = joined != null ? joined.ToArray() : single ?? new byte[0];
                joined = null;
                single = null;
                Run(connection, token => HelperBridge.WriteAllBytesAsync(connection, Path, content, IsBound(nameof(MkdirParents)) ? MkdirParents.IsPresent : (bool?)null, CallOptions, token));
            }
            catch (Exception failure) when (!(failure is RuntimeException))
            {
                ErrorRecord record = FilesErrors.Record(failure, Path, MyInvocation);
                if (TerminatesOn(failure))
                {
                    ThrowTerminatingError(record);
                }
                WriteError(record);
            }
        }

        private static object Unwrap(object value)
        {
            return value is PSObject wrapped ? wrapped.BaseObject : value;
        }

        private void Add(object value)
        {
            switch (value)
            {
                case string line:
                    Expect(isText: true);
                    if (pieces > 0)
                    {
                        Append(Utf8.GetBytes(Environment.NewLine));
                    }
                    Append(Utf8.GetBytes(line));
                    break;
                case byte[] bytes:
                    Expect(isText: false);
                    Append(bytes, borrowed: true);
                    break;
                case byte one:
                    Expect(isText: false);
                    Append(new[] { one });
                    break;
                case null:
                    throw new FilesException(FilesErrorKind.InvalidArgument, "-Value was given $null, which is neither text nor bytes; give '' to write an empty file. Nothing was sent.");
                default:
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"-Value must be strings, or bytes and byte arrays, not {value.GetType().Name}. Nothing was sent.");
            }
            pieces++;
        }

        // Text and bytes together have no one meaning: bytes are not text, and converting either would change the file.
        private void Expect(bool isText)
        {
            if (text.HasValue && text.Value != isText)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, "-Value was given both text and bytes; Set-FilesFileContent writes one file, from strings or from bytes. Nothing was sent.");
            }
            text = isText;
        }

        // Adds bytes to the content. A caller's array is borrowed: once the command returns to the producer, the producer
        // may change or reuse it before the upload at the end, so its bytes are copied as they are taken.
        private void Append(byte[] bytes, bool borrowed = false)
        {
            if (length + bytes.Length > MaxContentLength)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The content given is more than the {MaxContentLength} bytes Set-FilesFileContent can hold. Nothing was sent.")
                {
                    Recommendation = "Upload it from a file or stream with Send-FilesFile.",
                };
            }
            length += bytes.Length;
            if (single == null && joined == null)
            {
                single = borrowed ? (byte[])bytes.Clone() : bytes;
                return;
            }
            if (joined == null)
            {
                joined = new MemoryStream();
                joined.Write(single, 0, single.Length);
                single = null;
            }
            joined.Write(bytes, 0, bytes.Length);
        }
    }
}
