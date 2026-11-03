namespace FilesCom.PowerShell
{
    /// <summary>One part of an upload that Files.com accepted.</summary>
    public sealed class FilesUploadedPart
    {
        internal FilesUploadedPart(long number, long offset, long length, string etag, int attempts)
        {
            Number = number;
            Offset = offset;
            Length = length;
            // Sent by the storage server; made safe like every other text a server sent.
            ETag = SafeText.WithoutUrlCredentials(etag);
            Attempts = attempts;
        }

        /// <summary>The part number, from 1.</summary>
        public long Number { get; }

        /// <summary>The position of the part's first byte in the file.</summary>
        public long Offset { get; }

        /// <summary>The number of bytes in the part.</summary>
        public long Length { get; }

        /// <summary>The ETag the storage server returned for the part, or null when it returned none.</summary>
        public string ETag { get; }

        /// <summary>How many times the part was sent: 1 unless it was retried.</summary>
        public int Attempts { get; }
    }
}
