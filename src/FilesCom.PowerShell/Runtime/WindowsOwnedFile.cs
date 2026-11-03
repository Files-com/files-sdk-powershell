using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace FilesCom.PowerShell.Runtime
{
    // OwnedFile on Windows. The destination folder is opened without delete sharing for the whole operation, so no one
    // can rename, delete or replace it meanwhile. Its handle asks only for the right to add a file, which writing the
    // file needs anyway, not for the right to list the folder. The stage is a hidden file created new in that folder and
    // held open, shared with no one, so no one can read, rename, delete or replace it either. Publishing makes the
    // open stage visible, unless it replaces a hidden file, and renames it into the held folder, both through its own
    // handle; an unpublished stage is deleted through its handle. A stage that replaces a file first takes that file's
    // access control list through its handle, so the replacement gives no one access the replaced file did not; when
    // that list cannot be read or set, nothing is replaced.
    internal sealed class WindowsOwnedFile : OwnedFile
    {
        private const uint GenericWrite = 0x40000000;
        private const uint Delete = 0x00010000;
        private const uint AddFile = 0x00000002;
        private const uint ReadAttributes = 0x00000080;
        private const uint Synchronize = 0x00100000;
        private const uint ReadControl = 0x00020000;
        private const uint WriteDac = 0x00040000;
        private const uint ShareDelete = 0x00000004;
        private const uint OpenReparsePoint = 0x00200000;
        private const int FileObject = 1;
        private const uint DaclInformation = 0x00000004;
        private const uint ProtectedDacl = 0x80000000;
        private const uint UnprotectedDacl = 0x20000000;
        private const ushort DaclProtected = 0x1000;
        private const uint ShareRead = 0x00000001;
        private const uint ShareWrite = 0x00000002;
        private const uint CreateNew = 1;
        private const uint OpenExisting = 3;
        private const uint AttributeHidden = 0x00000002;
        private const uint AttributeNormal = 0x00000080;
        private const uint AttributeDirectory = 0x00000010;
        private const uint AttributeReparsePoint = 0x00000400;
        private const uint BackupSemantics = 0x02000000;
        private const uint InvalidAttributes = 0xFFFFFFFF;
        private const int FileBasicInfo = 0;
        private const int FileDispositionInfo = 4;
        // The NtSetInformationFile class of FILE_RENAME_INFORMATION, which has the layout of FILE_RENAME_INFO.
        private const int FileRenameInformation = 10;
        private const int ErrorFileExists = 80;
        private const int ErrorAlreadyExists = 183;

        private readonly SafeFileHandle folder;
        private readonly FileStream stream;

        internal WindowsOwnedFile(string destination) : base(destination)
        {
            try
            {
                // The right to add a file counts, unlike attribute rights alone, when Windows checks sharing, so holding
                // the handle keeps others from renaming or deleting the folder.
                folder = Opened(CreateFileW(Folder, AddFile | Synchronize, ShareRead | ShareWrite, IntPtr.Zero, OpenExisting, BackupSemantics, IntPtr.Zero), $"open the folder {Folder}");
                // Shared with no one: the stage takes the folder's inherited access rules when it is created, and only
                // takes those of the file it replaces as it is published, so until then no other handle may open it,
                // whatever the folder lets its readers see.
                SafeFileHandle stage = Opened(CreateFileW(Path.Combine(Folder, StageName()), GenericWrite | Delete | ReadAttributes | WriteDac, 0, IntPtr.Zero, CreateNew, AttributeHidden, IntPtr.Zero), $"create a file in {Folder}");
                stream = new FileStream(stage, FileAccess.Write, 81920);
            }
            catch
            {
                folder?.Dispose();
                throw;
            }
        }

        internal override Stream Stream => stream;

        protected override void PublishStage(bool replace)
        {
            stream.Flush(flushToDisk: true);
            bool replacesHidden = false;
            if (replace)
            {
                uint attributes = GetFileAttributesW(Destination);
                if (attributes != InvalidAttributes && (attributes & (AttributeDirectory | AttributeReparsePoint)) != 0)
                {
                    throw NotRegular();
                }
                replacesHidden = attributes != InvalidAttributes && (attributes & AttributeHidden) != 0;
                if (attributes != InvalidAttributes)
                {
                    TakeOverAccessRules();
                }
            }
            // The stage was hidden only while it was being written. A file that replaces a hidden one stays hidden, as
            // on Unix a replacing file keeps the permissions of the file it replaces.
            if (!replacesHidden)
            {
                Unhide();
            }
            // Renamed through its handle into the held folder, given as the name's RootDirectory. Only NtSetInformationFile
            // takes a name relative to a RootDirectory: SetFileInformationByHandle refuses one with ERROR_INVALID_PARAMETER,
            // so every publish failed with "The parameter is incorrect" when it was used here. A name that exists, when
            // not replacing, is STATUS_OBJECT_NAME_COLLISION, which maps to ERROR_ALREADY_EXISTS.
            byte[] rename = RenameInformation(replace);
            int status = NtSetInformationFile(stream.SafeFileHandle, out IoStatusBlock _, rename, (uint)rename.Length, FileRenameInformation);
            if (status < 0)
            {
                int error = RtlNtStatusToDosError(status);
                if (error == ErrorFileExists || error == ErrorAlreadyExists)
                {
                    throw Exists();
                }
                throw new IOException($"Could not move the finished file to {Destination}: {new Win32Exception(error).Message}");
            }
            stream.Dispose();
        }

        protected override void Close(bool removeStage)
        {
            if (removeStage && stream != null && !stream.SafeFileHandle.IsClosed)
            {
                // Deletes the stage, through its own handle, when the handle closes.
                SetFileInformationByHandle(stream.SafeFileHandle, FileDispositionInfo, new byte[] { 1 }, 1);
            }
            stream?.Dispose();
            folder?.Dispose();
        }

        // Gives the stage the access control list of the file it replaces, read through a handle to that file that asks
        // only to read its security, does not follow a reparse point and shares everything, and set through the stage's
        // own handle. A list the replaced file inherited is inherited by the stage in the same way from the same folder;
        // a protected list is copied whole.
        private void TakeOverAccessRules()
        {
            using (SafeFileHandle replaced = CreateFileW(Destination, ReadControl | ReadAttributes, ShareRead | ShareWrite | ShareDelete, IntPtr.Zero, OpenExisting, BackupSemantics | OpenReparsePoint, IntPtr.Zero))
            {
                if (replaced.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 2)
                    {
                        // Gone since it was checked: the stage is published as a new file.
                        return;
                    }
                    throw NotCarriedOver(new Win32Exception(error).Message);
                }
                var information = new byte[40];
                if (!GetFileInformationByHandleEx(replaced, FileBasicInfo, information, (uint)information.Length))
                {
                    throw NotCarriedOver(new Win32Exception(Marshal.GetLastWin32Error()).Message);
                }
                if ((BitConverter.ToUInt32(information, 32) & (AttributeDirectory | AttributeReparsePoint)) != 0)
                {
                    throw NotRegular();
                }
                int result = GetSecurityInfo(replaced, FileObject, DaclInformation, out IntPtr _, out IntPtr _, out IntPtr dacl, out IntPtr _, out IntPtr descriptor);
                if (result != 0)
                {
                    throw NotCarriedOver(new Win32Exception(result).Message);
                }
                try
                {
                    if (!GetSecurityDescriptorControl(descriptor, out ushort control, out uint _))
                    {
                        throw NotCarriedOver(new Win32Exception(Marshal.GetLastWin32Error()).Message);
                    }
                    uint which = DaclInformation | ((control & DaclProtected) != 0 ? ProtectedDacl : UnprotectedDacl);
                    result = SetSecurityInfo(stream.SafeFileHandle, FileObject, which, IntPtr.Zero, IntPtr.Zero, dacl, IntPtr.Zero);
                    if (result != 0)
                    {
                        throw NotCarriedOver(new Win32Exception(result).Message);
                    }
                }
                finally
                {
                    LocalFree(descriptor);
                }
            }
        }

        private IOException NotCarriedOver(string reason)
        {
            return new IOException($"Could not give the new file the access rules of the file it replaces, {Destination}: {reason}. Nothing was replaced; the existing file is unchanged.");
        }

        // Clears the stage's hidden attribute through its handle. FILE_BASIC_INFO: four times, left as zero so they do
        // not change, then the attributes, where zero would mean no change and so a file with no others is normal.
        private void Unhide()
        {
            var information = new byte[40];
            if (!GetFileInformationByHandleEx(stream.SafeFileHandle, FileBasicInfo, information, (uint)information.Length))
            {
                throw new IOException($"Could not read the attributes of the file written for {Destination}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
            }
            uint attributes = BitConverter.ToUInt32(information, 32) & ~AttributeHidden;
            var change = new byte[40];
            Array.Copy(BitConverter.GetBytes(attributes == 0 ? AttributeNormal : attributes), 0, change, 32, 4);
            if (!SetFileInformationByHandle(stream.SafeFileHandle, FileBasicInfo, change, (uint)change.Length))
            {
                throw new IOException($"Could not make the file written for {Destination} visible: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
            }
        }

        // FILE_RENAME_INFO naming the destination relative to the held folder: ReplaceIfExists, padded to pointer size,
        // RootDirectory, FileNameLength in bytes, then the name.
        private byte[] RenameInformation(bool replace)
        {
            byte[] name = Encoding.Unicode.GetBytes(Name);
            int handleOffset = IntPtr.Size;
            int lengthOffset = handleOffset + IntPtr.Size;
            int nameOffset = lengthOffset + 4;
            var buffer = new byte[nameOffset + name.Length + 2];
            buffer[0] = replace ? (byte)1 : (byte)0;
            byte[] root = IntPtr.Size == 8 ? BitConverter.GetBytes(folder.DangerousGetHandle().ToInt64()) : BitConverter.GetBytes(folder.DangerousGetHandle().ToInt32());
            Array.Copy(root, 0, buffer, handleOffset, root.Length);
            Array.Copy(BitConverter.GetBytes(name.Length), 0, buffer, lengthOffset, 4);
            Array.Copy(name, 0, buffer, nameOffset, name.Length);
            return buffer;
        }

        private static SafeFileHandle Opened(SafeFileHandle handle, string action)
        {
            if (!handle.IsInvalid)
            {
                return handle;
            }
            int error = Marshal.GetLastWin32Error();
            string message = $"Could not {action}: {new Win32Exception(error).Message}";
            switch (error)
            {
                case 3: throw new DirectoryNotFoundException(message);
                case 5: throw new UnauthorizedAccessException(message);
                default: throw new IOException(message);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFileW(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetFileInformationByHandle(SafeFileHandle handle, int informationClass, byte[] information, uint size);

        [StructLayout(LayoutKind.Sequential)]
        private struct IoStatusBlock
        {
            public IntPtr Status;
            public IntPtr Information;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtSetInformationFile(SafeFileHandle handle, out IoStatusBlock status, byte[] information, uint length, int informationClass);

        [DllImport("ntdll.dll")]
        private static extern int RtlNtStatusToDosError(int status);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetFileAttributesW(string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandleEx(SafeFileHandle handle, int informationClass, byte[] information, uint size);

        [DllImport("advapi32.dll")]
        private static extern int GetSecurityInfo(SafeFileHandle handle, int objectType, uint securityInformation, out IntPtr owner, out IntPtr group, out IntPtr dacl, out IntPtr sacl, out IntPtr securityDescriptor);

        [DllImport("advapi32.dll")]
        private static extern int SetSecurityInfo(SafeFileHandle handle, int objectType, uint securityInformation, IntPtr owner, IntPtr group, IntPtr dacl, IntPtr sacl);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSecurityDescriptorControl(IntPtr securityDescriptor, out ushort control, out uint revision);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr memory);
    }
}
