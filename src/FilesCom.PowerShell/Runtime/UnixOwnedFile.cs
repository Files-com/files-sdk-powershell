using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FilesCom.PowerShell.Runtime
{
    // OwnedFile on macOS and Linux. The stages for one destination folder are files in one private folder the command
    // creates there, with mode 0700 and, on macOS, no access control entries, so that no other user can enter it, add to
    // it or change what it holds. Every step names entries relative to descriptors: the destination folder's, opened once
    // and for search only, since creating a file there needs write and search permission but not permission to list it,
    // so replacing the folder, or any folder above it, cannot redirect the file; and the private folder's. Each file is
    // created new in the private folder, and publishing renames it into the destination folder, checking its device and
    // inode numbers before and after.
    //
    // Removing the private folder is the one step that names it in the destination folder, and a name there can be given
    // to something else by anyone allowed to rename entries in that folder: no system call removes a folder only if it
    // is a given one. So the command removes it only when no one but its user and the superuser can rename entries
    // there, since then the name still leads to the folder the command made: when the destination folder belongs to the
    // user and no one else may write to it, or is sticky and belongs to the user or the superuser, and on macOS has no
    // access control entry that lets others change it. Only its owner or the superuser can change those facts. In any
    // other folder the command leaves the empty private folder in place and reports it, rather than risk removing
    // something another user put under its name.
    //
    // A file that replaces another gives no one access the replaced file did not, apart from its new owner, the user:
    // before any permission bit grants the group or others access, the stage takes the replaced file's group and access
    // control list, through its own descriptor. When either cannot be carried over, nothing is replaced.
    internal sealed class UnixOwnedFile : OwnedFile
    {
        private const int PrivateDirectoryMode = 0x1C0; // 0700
        private const int PrivateFileMode = 0x180; // 0600

        private static readonly Posix Os = Posix.Current;

        private readonly StageFolder stage;
        private readonly string dataName;
        private readonly Identity dataIdentity;
        private readonly FileStream stream;
        private bool moved;

        private UnixOwnedFile(string destination, StageFolder stage) : base(destination)
        {
            this.stage = stage;
            dataName = StageName();
            int data = Os.Check(Os.OpenAt(stage.Descriptor, dataName, Os.WriteOnly | Os.Create | Os.Exclusive | Os.NoFollow | Os.CloseOnExec, PrivateFileMode), $"create a file in {Folder}");
            try
            {
                stream = new FileStream(new SafeFileHandle(new IntPtr(data), ownsHandle: true), FileAccess.Write, 81920);
            }
            catch
            {
                // Nothing has had the file's name but this command, in a folder only its user can change.
                Os.Close(data);
                Os.UnlinkAt(stage.Descriptor, dataName, 0);
                throw;
            }
            try
            {
                // The mode argument of openat does not reach macOS on every processor, so it is set explicitly.
                Os.Check(Os.ChangeMode(data, PrivateFileMode), $"protect the file being written in {Folder}");
                dataIdentity = Os.Stat(data);
            }
            catch
            {
                Close(removeStage: true);
                throw;
            }
        }

        internal override Stream Stream => stream;

        protected override void PublishStage(bool replace)
        {
            stream.Flush(flushToDisk: true);
            int data = stream.SafeFileHandle.DangerousGetHandle().ToInt32();
            Identity? existing = replace ? Os.StatAt(stage.Folder, Name) : null;
            if (existing.HasValue && !existing.Value.IsRegular)
            {
                throw NotRegular();
            }
            if (existing.HasValue)
            {
                TakeOverPermissions(data, existing.Value);
            }
            else
            {
                // A new file keeps the group it got in the folder, and gets the permissions any new file gets from this
                // process's umask.
                Os.Check(Os.ChangeMode(data, 0x1B6 & ~stage.Umask()), $"set the permissions of {Destination}");
            }
            stream.Dispose();
            stage.CheckFolder();
            if (Os.StatAt(stage.Descriptor, dataName) != dataIdentity)
            {
                throw new IOException($"The file being written for {Destination} was replaced. Nothing was published.");
            }
            if (replace)
            {
                Os.Check(Os.RenameAt(stage.Descriptor, dataName, stage.Folder, Name), $"move the finished file to {Destination}");
                moved = true;
            }
            else
            {
                MoveWithoutReplacing();
            }
            if (Os.StatAt(stage.Folder, Name) != dataIdentity)
            {
                throw new IOException($"{Destination} was replaced by another process right after it was written.");
            }
        }

        // Gives the private stage the principals of the file it is about to replace, through its own descriptor: that
        // file's group, then its access control list, then its permission bits. When that file is the user's, all its
        // bits; otherwise the replacement is necessarily the user's, so the owner bits a new file gets with that file's
        // group and other bits. Special bits are never copied. The group and access list are set while the stage still
        // grants nothing to anyone but its owner, and the result is read back. Throws, leaving the replaced file as it
        // is, when any of it cannot be carried over, as when the user may not give a file that group.
        private void TakeOverPermissions(int data, Identity replaced)
        {
            string path = Path.Combine(Folder, Name);
            AccessRules rules;
            try
            {
                rules = Os.ReadAccessRules(path);
            }
            catch (IOException failure)
            {
                throw NotCarriedOver("its access rules", failure.Message);
            }
            // The access list is read by path, so the entry at that path must still be the file checked, and the name
            // in the folder the command holds must lead to it too.
            Identity? byName = Os.StatAt(stage.Folder, Name);
            Identity? byPath = Os.LinkStatPath(path);
            if (!byName.HasValue || !byName.Value.SameAttributes(replaced) || !byPath.HasValue || !byPath.Value.SameAttributes(replaced))
            {
                throw new IOException($"{Destination} changed while the command was checking it. Nothing was replaced.");
            }
            if (Os.Stat(data).Group != replaced.Group && Os.ChangeGroup(data, replaced.Group) != 0)
            {
                throw NotCarriedOver($"its group (ID {replaced.Group})", Os.ErrorText(Os.LastError()));
            }
            try
            {
                Os.ApplyAccessRules(data, rules);
            }
            catch (IOException failure)
            {
                throw NotCarriedOver("its access rules", failure.Message);
            }
            int mode = replaced.Owner == Os.EffectiveUser()
                ? (int)(replaced.Mode & 0x1FF)
                : ((0x1B6 & ~stage.Umask()) & 0x1C0) | (int)(replaced.Mode & 0x3F);
            Os.Check(Os.ChangeMode(data, mode), $"set the permissions of {Destination}");
            Identity result = Os.Stat(data);
            if (result.Group != replaced.Group || (result.Mode & 0x1FF) != mode || !Os.HasAccessRules(data, rules))
            {
                throw NotCarriedOver("its group, access rules and permissions", "they did not all take effect");
            }
        }

        private IOException NotCarriedOver(string what, string reason)
        {
            return new IOException($"Could not give the new file the {what} of the file it replaces, {Destination}: {reason}. Nothing was replaced; the existing file is unchanged.");
        }

        // Moves the stage to the destination unless a file already exists there, in one step where the file system
        // allows it; otherwise by linking the destination to the stage, which also fails when it exists, then removing
        // the stage's own name.
        private void MoveWithoutReplacing()
        {
            int result = Os.RenameAtExclusive(stage.Descriptor, dataName, stage.Folder, Name, out bool supported);
            if (supported)
            {
                if (result != 0 && Os.LastError() == Os.AlreadyExists)
                {
                    throw Exists();
                }
                Os.Check(result, $"move the finished file to {Destination}");
                moved = true;
                return;
            }
            if (Os.LinkAt(stage.Descriptor, dataName, stage.Folder, Name) != 0)
            {
                if (Os.LastError() == Os.AlreadyExists)
                {
                    throw Exists();
                }
                Os.Check(-1, $"move the finished file to {Destination}");
            }
            moved = true;
            Os.UnlinkAt(stage.Descriptor, dataName, 0);
        }

        // Removes an unpublished stage by its name in the private folder, where only this user can change what a name
        // leads to, and only while it leads to the file the command created.
        protected override void Close(bool removeStage)
        {
            stream?.Dispose();
            if (removeStage && !moved && dataIdentity.Inode != 0 && Os.StatAt(stage.Descriptor, dataName) == dataIdentity)
            {
                Os.UnlinkAt(stage.Descriptor, dataName, 0);
            }
        }

        // The private folders of one command, by the device and inode numbers of their destination folders.
        internal sealed class Stages : OwnedFileStages
        {
            private readonly Dictionary<Identity, StageFolder> folders = new Dictionary<Identity, StageFolder>();

            protected override OwnedFile Open(string destination)
            {
                string path = Path.GetDirectoryName(destination);
                int folder = Os.OpenFolder(path);
                Identity identity;
                try
                {
                    identity = Os.Stat(folder);
                }
                catch
                {
                    Os.Close(folder);
                    throw;
                }
                if (folders.TryGetValue(identity, out StageFolder stage))
                {
                    Os.Close(folder);
                }
                else
                {
                    stage = new StageFolder(path, folder, identity);
                    folders.Add(identity, stage);
                }
                return new UnixOwnedFile(destination, stage);
            }

            protected override IReadOnlyList<string> FinishShared()
            {
                var kept = new List<string>();
                foreach (StageFolder stage in folders.Values)
                {
                    string message = stage.Finish();
                    if (message != null)
                    {
                        kept.Add(message);
                    }
                }
                folders.Clear();
                return kept;
            }
        }

        // A private folder in one destination folder, and the descriptors of both.
        private sealed class StageFolder
        {
            private readonly string path;
            private readonly Identity folderIdentity;
            private readonly string name;
            private readonly Identity identity;
            private int umask = -1;
            private bool created;
            private bool finished;

            // Takes over folder, the destination folder's descriptor, and closes it when it fails.
            internal StageFolder(string path, int folder, Identity folderIdentity)
            {
                this.path = path;
                this.folderIdentity = folderIdentity;
                Folder = folder;
                try
                {
                    name = StageName();
                    Os.Check(Os.MakeDirectoryAt(folder, name, PrivateDirectoryMode), $"create a private folder in {path}");
                    created = true;
                    Descriptor = Os.Check(Os.OpenAt(folder, name, Os.ReadOnly | Os.Directory | Os.NoFollow | Os.CloseOnExec, 0), $"open the private folder in {path}");
                    identity = Os.Stat(Descriptor);
                    // A folder another user put in place of the one just created would not be this user's.
                    if (!identity.IsDirectory || identity.Owner != Os.EffectiveUser())
                    {
                        throw new IOException($"The private folder the command created in {path} was replaced before it could be used. Nothing was written.");
                    }
                    // Access control entries the destination folder passes on to new folders would let others in.
                    Os.MakePrivate(Descriptor, path);
                    if ((Os.Stat(Descriptor).Mode & 0x1FF & ~PrivateDirectoryMode) != 0)
                    {
                        throw new IOException($"The file system of {path} does not keep the private folder the command created there private, so the command does not write there.");
                    }
                }
                catch (Exception failure)
                {
                    string kept = Finish();
                    if (kept != null)
                    {
                        throw new IOException($"{failure.Message} {kept}", failure);
                    }
                    throw;
                }
            }

            // The destination folder's descriptor, opened for search only.
            internal int Folder { get; }

            // The private folder's descriptor.
            internal int Descriptor { get; } = -1;

            // Fails when the destination folder's path no longer leads to the folder the command opened, since a
            // file published there would not appear at the path the caller gave.
            internal void CheckFolder()
            {
                if (Os.StatPath(path) != folderIdentity)
                {
                    throw new IOException($"The folder {path} was replaced while the file was being written. Nothing was published.");
                }
            }

            // This process's umask, read without changing it: the permissions a new folder, asked for with all of
            // them, gets.
            internal int Umask()
            {
                if (umask >= 0)
                {
                    return umask;
                }
                const string Probe = "umask";
                Os.Check(Os.MakeDirectoryAt(Descriptor, Probe, 0x1FF), $"create a folder in the private folder in {path}");
                try
                {
                    Identity? probe = Os.StatAt(Descriptor, Probe);
                    umask = probe.HasValue ? (int)(~probe.Value.Mode & 0x1FF) : 0x12;
                    return umask;
                }
                finally
                {
                    Os.UnlinkAt(Descriptor, Probe, Os.RemoveDirectory);
                }
            }

            // Removes the private folder when no one else can have given its name to something else, and closes the
            // descriptors. Returns why the folder was left in place, or null.
            internal string Finish()
            {
                if (finished)
                {
                    return null;
                }
                finished = true;
                string message = null;
                try
                {
                    if (created)
                    {
                        message = Remove();
                    }
                }
                finally
                {
                    if (Descriptor >= 0)
                    {
                        Os.Close(Descriptor);
                    }
                    Os.Close(Folder);
                }
                return message;
            }

            private string Remove()
            {
                if (!OnlyUserCanRename())
                {
                    return $"Left the empty private folder {name} in {path}: other users can rename entries in that folder, so the command could not be sure the name still led to its own folder. Remove it once no other process is using it.";
                }
                // Here the name can only have moved by this user's or the superuser's own doing.
                if (Os.StatAt(Folder, name) != identity)
                {
                    return $"The private folder {name} the command created in {path} was moved, so the command left it in place.";
                }
                if (Os.UnlinkAt(Folder, name, Os.RemoveDirectory) != 0)
                {
                    return $"Could not remove the private folder {name} the command created in {path}: {Os.ErrorText(Os.LastError())}.";
                }
                return null;
            }

            // Whether no one but this user and the superuser can rename or remove entries in the destination folder.
            private bool OnlyUserCanRename()
            {
                Identity current = Os.Stat(Folder);
                uint user = Os.EffectiveUser();
                bool ownedAndUnshared = current.Owner == user && (current.Mode & 0x12) == 0;
                bool sticky = (current.Mode & 0x200) != 0 && (current.Owner == user || current.Owner == 0);
                return (ownedAndUnshared || sticky) && !Os.AllowsOthersToChange(Folder);
            }
        }

        internal struct Identity : IEquatable<Identity>
        {
            internal Identity(ulong device, ulong inode, uint mode, uint owner, uint group)
            {
                Device = device;
                Inode = inode;
                Mode = mode;
                Owner = owner;
                Group = group;
            }

            internal ulong Device { get; }

            internal ulong Inode { get; }

            internal uint Mode { get; }

            internal uint Owner { get; }

            internal uint Group { get; }

            internal bool IsDirectory => (Mode & 0xF000) == 0x4000;

            internal bool IsRegular => (Mode & 0xF000) == 0x8000;

            public static bool operator ==(Identity left, Identity right) => left.Equals(right);

            public static bool operator !=(Identity left, Identity right) => !left.Equals(right);

            public bool Equals(Identity other) => Device == other.Device && Inode == other.Inode;

            public override bool Equals(object obj) => obj is Identity other && Equals(other);

            public override int GetHashCode() => (Device ^ Inode).GetHashCode();

            // The same file, with the same type, permissions, owner and group.
            internal bool SameAttributes(Identity other) => Equals(other) && Mode == other.Mode && Owner == other.Owner && Group == other.Group;
        }

        // The access control list of a file beyond its permission bits: its text on macOS, the raw
        // system.posix_acl_access attribute on Linux, or neither when it has none.
        private sealed class AccessRules
        {
            internal static readonly AccessRules None = new AccessRules(null, null);

            internal AccessRules(string text, byte[] bytes)
            {
                Text = text;
                Bytes = bytes;
            }

            internal string Text { get; }

            internal byte[] Bytes { get; }
        }

        // The C library calls and constants, which differ between macOS and Linux and, on Linux, between processors.
        // Only the fields of struct stat this class reads are decoded: device, inode, mode, owner and group, which
        // directly follows the owner on every supported platform.
        private abstract class Posix
        {
            internal static readonly Posix Current = Choose();

            internal int ReadOnly => 0;

            internal int WriteOnly => 1;

            internal abstract int Create { get; }

            internal abstract int Exclusive { get; }

            internal abstract int NoFollow { get; }

            internal abstract int Directory { get; }

            internal abstract int CloseOnExec { get; }

            // Opens a folder only to name entries in it, which needs search permission but not read permission.
            internal abstract int Search { get; }

            internal abstract int SymlinkNoFollow { get; }

            internal abstract int RemoveDirectory { get; }

            internal int AlreadyExists => 17;

            private static Posix Choose()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? (Posix)new MacArm64() : new MacX64();
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    switch (RuntimeInformation.ProcessArchitecture)
                    {
                        case Architecture.X64: return new LinuxX64();
                        case Architecture.Arm64: return new LinuxArm64();
                    }
                }
                return new Unsupported();
            }

            internal abstract int Open(string path, int flags);

            internal abstract Identity Stat(int descriptor);

            internal abstract Identity? StatAt(int directory, string name);

            internal abstract Identity? StatPath(string path);

            // The entry at path itself, not what a symbolic link there leads to.
            internal abstract Identity? LinkStatPath(string path);

            internal abstract AccessRules ReadAccessRules(string path);

            // Gives the file of descriptor exactly the access rules given, removing any others it has.
            internal abstract void ApplyAccessRules(int descriptor, AccessRules rules);

            internal abstract bool HasAccessRules(int descriptor, AccessRules rules);

            internal abstract int RenameAtExclusive(int fromDirectory, string from, int toDirectory, string to, out bool supported);

            internal int OpenAt(int directory, string name, int flags, int mode) => Native.openat(directory, name, flags, mode);

            internal int MakeDirectoryAt(int directory, string name, int mode) => Native.mkdirat(directory, name, mode);

            internal int ChangeMode(int descriptor, int mode) => Native.fchmod(descriptor, mode);

            // Changes only the group of the file of descriptor; the owner stays as it is.
            internal int ChangeGroup(int descriptor, uint group) => Native.fchown(descriptor, uint.MaxValue, group);

            internal int RenameAt(int fromDirectory, string from, int toDirectory, string to) => Native.renameat(fromDirectory, from, toDirectory, to);

            internal int LinkAt(int fromDirectory, string from, int toDirectory, string to) => Native.linkat(fromDirectory, from, toDirectory, to, 0);

            internal int UnlinkAt(int directory, string name, int flags) => Native.unlinkat(directory, name, flags);

            internal void Close(int descriptor) => Native.close(descriptor);

            internal string ErrorText(int error) => Marshal.PtrToStringAnsi(Native.strerror(error));

            // A folder's descriptor for naming entries in it; opened for reading where the system has no search-only
            // opens, which reject the flag.
            internal int OpenFolder(string path)
            {
                int folder = Open(path, Search | CloseOnExec);
                if (folder < 0 && LastError() == 22)
                {
                    folder = Open(path, ReadOnly | Directory | CloseOnExec);
                }
                return Check(folder, $"open the folder {path}");
            }

            // Removes what a new folder took from its parent that could let other users in besides its mode.
            internal virtual void MakePrivate(int directory, string path)
            {
            }

            // Whether the folder has access rules beyond its mode that let other users add, rename or remove its
            // entries, or change its rules. On Linux its mode already bounds what its access control list allows.
            internal virtual bool AllowsOthersToChange(int directory) => false;

            internal uint EffectiveUser() => Native.geteuid();

            internal int LastError() => Marshal.GetLastWin32Error();

            // The result of a call that returns -1 on failure, or the failure as the matching .NET exception.
            internal int Check(int result, string action)
            {
                if (result >= 0)
                {
                    return result;
                }
                int error = LastError();
                string message = $"Could not {action}: {ErrorText(error)}.";
                switch (error)
                {
                    case 2: throw new DirectoryNotFoundException(message);
                    case 1:
                    case 13: throw new UnauthorizedAccessException(message);
                    default: throw new IOException(message);
                }
            }

            protected Identity Decode(byte[] stat, int modeOffset, bool shortMode, int ownerOffset, bool wideDevice)
            {
                ulong device = wideDevice ? BitConverter.ToUInt64(stat, 0) : BitConverter.ToUInt32(stat, 0);
                uint mode = shortMode ? BitConverter.ToUInt16(stat, modeOffset) : BitConverter.ToUInt32(stat, modeOffset);
                return new Identity(device, BitConverter.ToUInt64(stat, 8), mode, BitConverter.ToUInt32(stat, ownerOffset), BitConverter.ToUInt32(stat, ownerOffset + 4));
            }

            // stat structures are at most 144 bytes on the supported platforms.
            protected static byte[] Buffer() => new byte[256];

            protected Identity Required(int result, byte[] stat, Func<byte[], Identity> decode)
            {
                Check(result, "read a file's identity");
                return decode(stat);
            }

            protected Identity? Optional(int result, byte[] stat, Func<byte[], Identity> decode)
            {
                if (result == 0)
                {
                    return decode(stat);
                }
                if (LastError() == 2)
                {
                    return null;
                }
                Check(result, "read a file's identity");
                return null;
            }
        }

        private abstract class Mac : Posix
        {
            internal override int Create => 0x200;

            internal override int Exclusive => 0x800;

            internal override int NoFollow => 0x100;

            internal override int Directory => 0x100000;

            internal override int CloseOnExec => 0x1000000;

            // O_SEARCH: O_EXEC with O_DIRECTORY.
            internal override int Search => 0x40000000 | Directory;

            internal override int SymlinkNoFollow => 0x20;

            internal override int RemoveDirectory => 0x80;

            private const int ExtendedAcl = 0x100;
            private const int NoSuchEntry = 2;
            private const int NotSupported = 45;
            private const int OperationNotSupported = 102;

            // The permissions that let whoever an access control entry allows them to add, rename or remove a folder's
            // entries, or to change who may. For folders, write is add_file and append add_subdirectory.
            private static readonly HashSet<string> ChangingPermissions = new HashSet<string>(StringComparer.Ordinal)
            {
                "write", "append", "delete", "delete_child", "add_file", "add_subdirectory", "writesecurity", "chown",
            };

            internal override int Open(string path, int flags) => Native.openat(-2, path, flags, 0);

            // Replaces the entries the folder inherited from its parent with none, then checks that none are left. A
            // file system without access control lists has none to remove.
            internal override void MakePrivate(int directory, string path)
            {
                IntPtr empty = Native.acl_init(0);
                if (empty == IntPtr.Zero)
                {
                    Check(-1, $"protect the private folder in {path}");
                }
                int result = Native.acl_set_fd_np(directory, empty, ExtendedAcl);
                int error = result == 0 ? 0 : LastError();
                Native.acl_free(empty);
                if (result != 0 && error != NotSupported && error != OperationNotSupported)
                {
                    throw new IOException($"Could not protect the private folder in {path}: {ErrorText(error)}.");
                }
                if (AccessControlText(directory, out _))
                {
                    throw new IOException($"Could not remove the access rules the private folder in {path} inherited. Nothing was written.");
                }
            }

            // Reads access control entries in the form acl_to_text writes them, one to a line after a header:
            // tag:uuid:name:id:allow-or-deny[,flags]:permissions. A list that cannot be read counts as allowing changes.
            internal override bool AllowsOthersToChange(int directory)
            {
                string text;
                try
                {
                    if (!AccessControlText(directory, out text))
                    {
                        return false;
                    }
                }
                catch (IOException)
                {
                    return true;
                }
                foreach (string line in text.Split('\n'))
                {
                    string[] fields = line.Split(':');
                    if (fields.Length < 6 || fields[4].Split(',')[0] != "allow")
                    {
                        continue;
                    }
                    foreach (string permission in fields[5].Split(','))
                    {
                        if (ChangingPermissions.Contains(permission.Trim()))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            // Reads the list of the entry at path itself, which needs no permission to read the file.
            internal override AccessRules ReadAccessRules(string path)
            {
                return AccessControlText(Native.acl_get_link_np(path, ExtendedAcl), out string text) ? new AccessRules(text, null) : AccessRules.None;
            }

            // The private stage has no list of its own, since its folder's was removed, so there is none to remove.
            internal override void ApplyAccessRules(int descriptor, AccessRules rules)
            {
                if (rules.Text == null)
                {
                    return;
                }
                IntPtr acl = Native.acl_from_text(rules.Text);
                if (acl == IntPtr.Zero)
                {
                    throw new IOException($"its access rules could not be read back: {ErrorText(LastError())}");
                }
                try
                {
                    if (Native.acl_set_fd_np(descriptor, acl, ExtendedAcl) != 0)
                    {
                        throw new IOException(ErrorText(LastError()));
                    }
                }
                finally
                {
                    Native.acl_free(acl);
                }
            }

            internal override bool HasAccessRules(int descriptor, AccessRules rules)
            {
                string current = AccessControlText(Native.acl_get_fd_np(descriptor, ExtendedAcl), out string text) ? text : null;
                return current == rules.Text;
            }

            // Whether the folder has an access control list, and its text.
            private bool AccessControlText(int directory, out string text)
            {
                return AccessControlText(Native.acl_get_fd_np(directory, ExtendedAcl), out text);
            }

            // The text of a list acl_get_*_np returned, freeing it, or false when that call found none.
            private bool AccessControlText(IntPtr acl, out string text)
            {
                text = null;
                if (acl == IntPtr.Zero)
                {
                    int error = LastError();
                    if (error == NoSuchEntry || error == NotSupported || error == OperationNotSupported)
                    {
                        return false;
                    }
                    throw new IOException($"Could not read access rules: {ErrorText(error)}.");
                }
                try
                {
                    IntPtr written = Native.acl_to_text(acl, IntPtr.Zero);
                    if (written == IntPtr.Zero)
                    {
                        throw new IOException("Could not read access rules.");
                    }
                    text = Marshal.PtrToStringAnsi(written);
                    Native.acl_free(written);
                    return true;
                }
                finally
                {
                    Native.acl_free(acl);
                }
            }

            internal override int RenameAtExclusive(int fromDirectory, string from, int toDirectory, string to, out bool supported)
            {
                // RENAME_EXCL: fail with EEXIST rather than replace. Unsupported by the file system: ENOTSUP.
                int result = Native.renameatx_np(fromDirectory, from, toDirectory, to, 0x4);
                supported = result == 0 || LastError() != 45;
                return result;
            }

            protected Identity DecodeMac(byte[] stat) => Decode(stat, 4, shortMode: true, 16, wideDevice: false);
        }

        private sealed class MacArm64 : Mac
        {
            internal override Identity Stat(int descriptor)
            {
                byte[] stat = Buffer();
                return Required(Native.fstat(descriptor, stat), stat, DecodeMac);
            }

            internal override Identity? StatAt(int directory, string name)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat(directory, name, stat, SymlinkNoFollow), stat, DecodeMac);
            }

            internal override Identity? StatPath(string path)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat(-2, path, stat, 0), stat, DecodeMac);
            }

            internal override Identity? LinkStatPath(string path)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat(-2, path, stat, SymlinkNoFollow), stat, DecodeMac);
            }
        }

        // On Intel Macs the struct stat with 64-bit inode numbers comes from the $INODE64 variants.
        private sealed class MacX64 : Mac
        {
            internal override Identity Stat(int descriptor)
            {
                byte[] stat = Buffer();
                return Required(Native.fstat_inode64(descriptor, stat), stat, DecodeMac);
            }

            internal override Identity? StatAt(int directory, string name)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat_inode64(directory, name, stat, SymlinkNoFollow), stat, DecodeMac);
            }

            internal override Identity? StatPath(string path)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat_inode64(-2, path, stat, 0), stat, DecodeMac);
            }

            internal override Identity? LinkStatPath(string path)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat_inode64(-2, path, stat, SymlinkNoFollow), stat, DecodeMac);
            }
        }

        private abstract class Linux : Posix
        {
            internal override int Create => 0x40;

            internal override int Exclusive => 0x80;

            internal override int CloseOnExec => 0x80000;

            // O_PATH, the same on x64 and Arm64. An O_PATH descriptor can name entries in *at calls and be read by fstat.
            internal override int Search => 0x200000 | Directory;

            internal override int SymlinkNoFollow => 0x100;

            internal override int RemoveDirectory => 0x200;

            internal override int Open(string path, int flags) => Native.openat(-100, path, flags, 0);

            internal override int RenameAtExclusive(int fromDirectory, string from, int toDirectory, string to, out bool supported)
            {
                try
                {
                    // RENAME_NOREPLACE: fail with EEXIST rather than replace. Unsupported: EINVAL or ENOSYS.
                    int result = Native.renameat2(fromDirectory, from, toDirectory, to, 1);
                    int error = result == 0 ? 0 : LastError();
                    supported = error != 22 && error != 38;
                    return result;
                }
                catch (EntryPointNotFoundException)
                {
                    supported = false;
                    return -1;
                }
            }

            protected abstract Identity DecodeLinux(byte[] stat);

            internal override Identity Stat(int descriptor)
            {
                byte[] stat = Buffer();
                return Required(Native.fstat(descriptor, stat), stat, DecodeLinux);
            }

            internal override Identity? StatAt(int directory, string name)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat(directory, name, stat, SymlinkNoFollow), stat, DecodeLinux);
            }

            internal override Identity? StatPath(string path)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat(-100, path, stat, 0), stat, DecodeLinux);
            }

            internal override Identity? LinkStatPath(string path)
            {
                byte[] stat = Buffer();
                return Optional(Native.fstatat(-100, path, stat, SymlinkNoFollow), stat, DecodeLinux);
            }

            // A POSIX access control list is the extended attribute system.posix_acl_access; the file's group bits then
            // show its mask, which the permission bits set later keep.
            private const string AccessAcl = "system.posix_acl_access";
            private const int NoAttribute = 61;
            private const int NotSupported = 95;

            internal override AccessRules ReadAccessRules(string path)
            {
                long size = (long)Native.lgetxattr(path, AccessAcl, null, UIntPtr.Zero);
                if (size < 0)
                {
                    int error = LastError();
                    if (error == NoAttribute || error == NotSupported)
                    {
                        return AccessRules.None;
                    }
                    throw new IOException($"Could not read access rules: {ErrorText(error)}.");
                }
                var bytes = new byte[size];
                long read = (long)Native.lgetxattr(path, AccessAcl, bytes, (UIntPtr)bytes.Length);
                if (read != size)
                {
                    throw new IOException("Could not read access rules: they changed while being read.");
                }
                return new AccessRules(null, bytes);
            }

            // The stage may have inherited a list from its folder's default list; one the replaced file did not have is
            // removed.
            internal override void ApplyAccessRules(int descriptor, AccessRules rules)
            {
                if (rules.Bytes != null)
                {
                    if (Native.fsetxattr(descriptor, AccessAcl, rules.Bytes, (UIntPtr)rules.Bytes.Length, 0) != 0)
                    {
                        throw new IOException(ErrorText(LastError()));
                    }
                    return;
                }
                if (Native.fremovexattr(descriptor, AccessAcl) != 0)
                {
                    int error = LastError();
                    if (error != NoAttribute && error != NotSupported)
                    {
                        throw new IOException(ErrorText(error));
                    }
                }
            }

            // Whether the file has a list exactly when the replaced file had one. The permission bits set afterwards
            // rewrite the owner, mask and other entries of a list, so its bytes are not compared.
            internal override bool HasAccessRules(int descriptor, AccessRules rules)
            {
                long size = (long)Native.fgetxattr(descriptor, AccessAcl, null, UIntPtr.Zero);
                if (size < 0)
                {
                    int error = LastError();
                    if (error != NoAttribute && error != NotSupported)
                    {
                        throw new IOException($"Could not read access rules: {ErrorText(error)}.");
                    }
                    return rules.Bytes == null;
                }
                return rules.Bytes != null;
            }
        }

        private sealed class LinuxX64 : Linux
        {
            internal override int NoFollow => 0x20000;

            internal override int Directory => 0x10000;

            protected override Identity DecodeLinux(byte[] stat) => Decode(stat, 24, shortMode: false, 28, wideDevice: true);
        }

        private sealed class LinuxArm64 : Linux
        {
            internal override int NoFollow => 0x8000;

            internal override int Directory => 0x4000;

            protected override Identity DecodeLinux(byte[] stat) => Decode(stat, 16, shortMode: false, 24, wideDevice: true);
        }

        private sealed class Unsupported : Posix
        {
            internal override int Create => throw Failure();

            internal override int Exclusive => throw Failure();

            internal override int NoFollow => throw Failure();

            internal override int Directory => throw Failure();

            internal override int CloseOnExec => throw Failure();

            internal override int Search => throw Failure();

            internal override int SymlinkNoFollow => throw Failure();

            internal override int RemoveDirectory => throw Failure();

            internal override int Open(string path, int flags) => throw Failure();

            internal override Identity Stat(int descriptor) => throw Failure();

            internal override Identity? StatAt(int directory, string name) => throw Failure();

            internal override Identity? StatPath(string path) => throw Failure();

            internal override Identity? LinkStatPath(string path) => throw Failure();

            internal override AccessRules ReadAccessRules(string path) => throw Failure();

            internal override void ApplyAccessRules(int descriptor, AccessRules rules) => throw Failure();

            internal override bool HasAccessRules(int descriptor, AccessRules rules) => throw Failure();

            internal override int RenameAtExclusive(int fromDirectory, string from, int toDirectory, string to, out bool supported) => throw Failure();

            private static PlatformNotSupportedException Failure()
            {
                return new PlatformNotSupportedException($"Writing local files safely is supported on macOS and on Linux for x64 and Arm64, not on {RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture}.");
            }
        }

        private static class Native
        {
            private const string Libc = "libc";

            [DllImport(Libc, SetLastError = true)]
            internal static extern int openat(int directory, string name, int flags, int mode);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int mkdirat(int directory, string name, int mode);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int fchmod(int descriptor, int mode);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int fchown(int descriptor, uint owner, uint group);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int renameat(int fromDirectory, string from, int toDirectory, string to);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int renameatx_np(int fromDirectory, string from, int toDirectory, string to, uint flags);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int renameat2(int fromDirectory, string from, int toDirectory, string to, uint flags);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int linkat(int fromDirectory, string from, int toDirectory, string to, int flags);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int unlinkat(int directory, string name, int flags);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int close(int descriptor);

            [DllImport(Libc)]
            internal static extern uint geteuid();

            [DllImport(Libc)]
            internal static extern IntPtr strerror(int error);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int fstat(int descriptor, byte[] stat);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int fstatat(int directory, string name, byte[] stat, int flags);

            [DllImport(Libc, EntryPoint = "fstat$INODE64", SetLastError = true)]
            internal static extern int fstat_inode64(int descriptor, byte[] stat);

            [DllImport(Libc, EntryPoint = "fstatat$INODE64", SetLastError = true)]
            internal static extern int fstatat_inode64(int directory, string name, byte[] stat, int flags);

            // macOS access control lists.
            [DllImport(Libc, SetLastError = true)]
            internal static extern IntPtr acl_init(int count);

            [DllImport(Libc, SetLastError = true)]
            internal static extern IntPtr acl_get_fd_np(int descriptor, int type);

            [DllImport(Libc, SetLastError = true)]
            internal static extern IntPtr acl_get_link_np(string path, int type);

            [DllImport(Libc, SetLastError = true)]
            internal static extern IntPtr acl_from_text(string text);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int acl_set_fd_np(int descriptor, IntPtr acl, int type);

            [DllImport(Libc, SetLastError = true)]
            internal static extern IntPtr acl_to_text(IntPtr acl, IntPtr length);

            [DllImport(Libc)]
            internal static extern int acl_free(IntPtr item);

            // Linux extended attributes.
            [DllImport(Libc, SetLastError = true)]
            internal static extern IntPtr lgetxattr(string path, string name, byte[] value, UIntPtr size);

            [DllImport(Libc, SetLastError = true)]
            internal static extern IntPtr fgetxattr(int descriptor, string name, byte[] value, UIntPtr size);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int fsetxattr(int descriptor, string name, byte[] value, UIntPtr size, int flags);

            [DllImport(Libc, SetLastError = true)]
            internal static extern int fremovexattr(int descriptor, string name);
        }
    }
}
