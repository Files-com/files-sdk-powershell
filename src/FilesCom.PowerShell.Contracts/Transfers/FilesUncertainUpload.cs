namespace FilesCom.PowerShell
{
    /// <summary>
    /// An upload whose final request failed after it may have reached Files.com, so the file may or may not have been
    /// completed. Check the remote file before uploading it again.
    /// </summary>
    public sealed class FilesUncertainUpload
    {
        internal FilesUncertainUpload(string path, long size, FilesUploadedPart[] parts)
        {
            Path = path;
            Size = size;
            Parts = parts;
        }

        /// <summary>The remote path of the upload.</summary>
        public string Path { get; }

        /// <summary>The number of bytes sent.</summary>
        public long Size { get; }

        /// <summary>The parts Files.com accepted before the final request.</summary>
        public FilesUploadedPart[] Parts { get; }

        /// <summary>Describes the upload.</summary>
        public override string ToString()
        {
            return $"{Path} ({Size} bytes, {Parts.Length} parts accepted)";
        }
    }
}
