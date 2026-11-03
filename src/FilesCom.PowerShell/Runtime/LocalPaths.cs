using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace FilesCom.PowerShell.Runtime
{
    // Resolves the local paths transfer commands read and write through PowerShell's FileSystem provider, so relative
    // paths follow the current location and PSDrives work. Only -Path expands wildcards; a -LiteralPath is used as
    // written. Remote paths never pass through here: they are Files.com names, used exactly as given.
    internal static class LocalPaths
    {
        internal static IReadOnlyList<string> ExistingFiles(PSCmdlet command, string path, bool literal)
        {
            ProviderInfo provider;
            List<string> files;
            if (literal)
            {
                files = new List<string> { command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out PSDriveInfo _) };
            }
            else
            {
                files = new List<string>(command.SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider));
            }
            RequireFileSystem(provider, path);
            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    throw new FilesException(FilesErrorKind.InvalidArgument, $"{file} is a folder. Send-FilesFile uploads files; send the files in a folder by piping Get-ChildItem -File to it.");
                }
                if (!File.Exists(file))
                {
                    throw new FileNotFoundException($"The local file {file} does not exist.", file);
                }
            }
            return files;
        }

        // The local file a download writes: -LiteralPath as written, or, when it names an existing folder, the remote
        // file's name inside it. The remote name must be a plain file name, so it cannot reach outside that folder.
        internal static string DownloadDestination(PSCmdlet command, string literalPath, string remotePath)
        {
            string destination = command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(literalPath, out ProviderInfo provider, out PSDriveInfo _);
            RequireFileSystem(provider, literalPath);
            if (Directory.Exists(destination))
            {
                destination = Path.Combine(destination, SafeFileName(remotePath));
            }
            string folder = Path.GetDirectoryName(destination);
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException($"The folder to download into, {folder}, does not exist.");
            }
            return destination;
        }

        // The local file a command creates: -LiteralPath as written, which must not be a folder and whose folder must
        // exist.
        internal static string NewFileDestination(PSCmdlet command, string literalPath)
        {
            string destination = command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(literalPath, out ProviderInfo provider, out PSDriveInfo _);
            RequireFileSystem(provider, literalPath);
            if (Directory.Exists(destination))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"{destination} is a folder.");
            }
            string folder = Path.GetDirectoryName(destination);
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException($"The folder to write into, {folder}, does not exist.");
            }
            return destination;
        }

        // The one existing local file a path names, with wildcards expanded unless literal, for a command that takes a
        // single file, such as New-FilesAttachment.
        internal static string ExistingFile(PSCmdlet command, string path, bool literal)
        {
            ProviderInfo provider;
            IReadOnlyList<string> files = literal
                ? new List<string> { command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out PSDriveInfo _) }
                : command.SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider);
            RequireFileSystem(provider, path);
            if (files.Count != 1)
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"{path} matches {files.Count} files; give one file, or use -LiteralPath.");
            }
            string file = files[0];
            if (Directory.Exists(file))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"{file} is a folder; give one file.");
            }
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"The local file {file} does not exist.", file);
            }
            return file;
        }

        // An existing local folder, named as written, as the full path the engine needs.
        internal static string ExistingFolder(PSCmdlet command, string literalPath)
        {
            string folder = command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(literalPath, out ProviderInfo provider, out PSDriveInfo _);
            RequireFileSystem(provider, literalPath);
            if (!Directory.Exists(folder))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The folder {folder} does not exist.");
            }
            return folder;
        }

        // A checkpoint file, named as written, as a full path: it need not exist, but its folder must, and it must not be a
        // folder. Whether the folder may be trusted is the engine's decision, made when the transfer starts.
        internal static string CheckpointFile(PSCmdlet command, string literalPath)
        {
            string file = command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(literalPath, out ProviderInfo provider, out PSDriveInfo _);
            RequireFileSystem(provider, literalPath);
            if (Directory.Exists(file))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The checkpoint {file} is a folder. Give the path of a file in it.");
            }
            string folder = Path.GetDirectoryName(file);
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The folder of the checkpoint {file} does not exist.");
            }
            return file;
        }

        // A checkpoint folder, named as written, as a full path. It need not exist: the engine creates it, and decides
        // whether it may be trusted, when the transfer starts.
        internal static string CheckpointFolder(PSCmdlet command, string literalPath)
        {
            string folder = command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(literalPath, out ProviderInfo provider, out PSDriveInfo _);
            RequireFileSystem(provider, literalPath);
            return folder;
        }

        // The local directory a directory download writes into, named as written, as a full path. It need not exist: the
        // engine creates it. A file at that name is refused here, before anything is asked of Files.com.
        internal static string DownloadDirectory(PSCmdlet command, string literalPath)
        {
            string folder = FullPath(command, literalPath);
            if (File.Exists(folder))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"{folder} is a file. Give the local directory to download into; it is created if missing.");
            }
            return folder;
        }

        // A checkpoint folder, named as written, as a full path, which must not be the local directory transferred or be
        // inside it by its name. The engine refuses one inside it through a link too, and decides whether it may be
        // trusted, when the transfer starts.
        internal static string CheckpointFolderOutside(PSCmdlet command, string literalPath, string directory)
        {
            string folder = FullPath(command, literalPath);
            if (IsSameOrInside(folder, directory))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The checkpoint folder {folder} is inside the local directory {directory}, or is that directory. Keep it outside the directory transferred.");
            }
            return folder;
        }

        // Whether a full path is folder itself or below it, by name. Names are compared as the platform's usual file
        // systems compare them: ignoring case on Windows and macOS.
        internal static bool IsSameOrInside(string path, string folder)
        {
            StringComparison comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            string inner = Trimmed(path);
            string outer = Trimmed(folder);
            string prefix = IsSeparator(outer[outer.Length - 1]) ? outer : outer + Path.DirectorySeparatorChar;
            return string.Equals(inner, outer, comparison) || inner.StartsWith(prefix, comparison);
        }

        private static string FullPath(PSCmdlet command, string literalPath)
        {
            string path = command.SessionState.Path.GetUnresolvedProviderPathFromPSPath(literalPath, out ProviderInfo provider, out PSDriveInfo _);
            RequireFileSystem(provider, literalPath);
            return Path.GetFullPath(path);
        }

        // A full path without separators at its end, except a root's own.
        private static string Trimmed(string path)
        {
            string root = Path.GetPathRoot(path) ?? "";
            string trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return trimmed.Length < root.Length ? root : trimmed;
        }

        private static bool IsSeparator(char character)
        {
            return character == Path.DirectorySeparatorChar || character == Path.AltDirectorySeparatorChar;
        }

        private static string SafeFileName(string remotePath)
        {
            string name = remotePath.Substring(remotePath.LastIndexOf('/') + 1);
            if (name.Length == 0 || name == "." || name == ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.IndexOf('\\') >= 0 || IsWindowsAlias(name))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"The remote name '{name}' cannot be used as a local file name. Give the whole local file path with -LiteralPath.");
            }
            return name;
        }

        // A name Windows would turn into another file's: one ending in a dot or a space, which it drops, so that two
        // remote names land on one local file, or a reserved device name such as CON, NUL or COM1, with any extension,
        // which names a device rather than a file. Checked as the engine checks the names of a folder download.
        private static bool IsWindowsAlias(string name)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }
            if (name.EndsWith(".", StringComparison.Ordinal) || name.EndsWith(" ", StringComparison.Ordinal))
            {
                return true;
            }
            string stem = name.Split('.')[0].TrimEnd(' ').ToUpperInvariant();
            return stem == "CON" || stem == "PRN" || stem == "AUX" || stem == "NUL"
                || (stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal))
                    && ((stem[3] >= '0' && stem[3] <= '9') || stem[3] == '¹' || stem[3] == '²' || stem[3] == '³'));
        }

        private static void RequireFileSystem(ProviderInfo provider, string path)
        {
            if (provider.ImplementingType != typeof(Microsoft.PowerShell.Commands.FileSystemProvider))
            {
                throw new FilesException(FilesErrorKind.InvalidArgument, $"{path} is not a file system path.");
            }
        }
    }
}
