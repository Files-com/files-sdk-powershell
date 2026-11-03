---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Receive-FilesDirectory
---

# Receive-FilesDirectory

## SYNOPSIS

Downloads a remote folder and everything in it into a local directory, several files at once.

## SYNTAX

### RemotePath (Default)

```
Receive-FilesDirectory [-RemotePath] <string> [-LiteralPath] <string> [-Force] [-MaxConcurrentFiles <int>]
 [-MaxConcurrency <int>] [-PartSize <long>] [-Limiter <FilesTransferLimiter>] [-CheckpointDirectory <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>] [-WorkspaceScope <long>]
 [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### InputObject

```
Receive-FilesDirectory [-LiteralPath] <string> -InputObject <RemoteFile> [-Force] [-MaxConcurrentFiles <int>]
 [-MaxConcurrency <int>] [-PartSize <long>] [-Limiter <FilesTransferLimiter>] [-CheckpointDirectory <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>] [-WorkspaceScope <long>]
 [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Receive-FilesDirectory downloads every file under the remote folder -RemotePath, or the folder piped to it, to the same relative path under the local directory -LiteralPath, as Receive-FilesFile downloads a file, and creates every folder that has nothing inside to download. The local directory, and the folders above it, are created if missing when the download starts, and stay whatever follows, as does a checkpoint folder created for it. The remote folder is listed first, a folder at a time, and its files are downloaded at most -MaxConcurrentFiles at once, in the order of their paths, sharing one limiter. A remote name that cannot be a local file's name on this system fails the download before anything is written.

Each file is written into a private stage beside its name and appears under that name only once it is complete: where nothing is there, or with -Force replacing only a regular file. A folder, a symbolic link or another special file at a file's name is never replaced or followed. Local folders are entered and created one name at a time without following symbolic links, so a link where a folder belongs fails the files below it rather than writing where it points. The local directory and the folders above it are held so that they cannot be renamed during the download.

The first failure stops the files in progress, and no other file starts; the command then fails with that error once every file has stopped. The error is the one file's: a file whose name is taken, for example, fails with Files.LocalFile, names that file as its target, and says what is at its name. Files completed before then stay in place, and so does everything else in the local directory: nothing is removed or rolled back. A failure of one piped folder that concerns only its files, such as a name already taken, is written as an error and the command goes on with the next folder; a failure of the connection, the checkpoint or Files.com's answer stops the command.

With -CheckpointDirectory, running the same command again skips each file it completed once its storage confirms that the version it downloaded is still the file's current version and the local file is still the one it wrote, and continues each file in progress from the parts it has, once its storage confirms the file's version; a file whose version changed is downloaded again as it is now. A file skipped because an earlier run completed it is listed with CompletedEarlier set; a file downloaded again is not. Stopping the command, with Ctrl+C or by stopping the pipeline, is pausing it: the checkpoint keeps what was done.

-WhatIf describes the download without contacting Files.com, and creates neither the local directory nor the checkpoint folder.

## EXAMPLES

### Example 1: Download a folder

```powershell
Receive-FilesDirectory -RemotePath reports/2026 -LiteralPath ./reports
```

Downloads reports/2026 and everything in it into ./reports, creating it if missing, and fails at the first file whose name is already taken there.

### Example 2: Download a large folder so that it can be continued

```powershell
Receive-FilesDirectory media ./media -CheckpointDirectory ~/.files-media-download -MaxConcurrentFiles 8 -PassThru |
    Select-Object Bytes, Elapsed, @{ Name = 'Files'; Expression = { $_.Files.Count } }
```

Records its progress in ~/.files-media-download; if the download is stopped, running the same command again skips the files it completed and continues the rest.

### Example 3: Download each subfolder into a directory of its own

```powershell
Get-FilesChildItem -Path projects | Where-Object Type -EQ directory |
    Receive-FilesDirectory -LiteralPath { Join-Path ./projects $_.DisplayName } -Force
```

Downloads every folder in projects into a directory of the same name under ./projects, replacing regular files already there.

## PARAMETERS

### -CheckpointDirectory

A local folder in which the download records its progress, so that when it is stopped, by Ctrl+C, a failure or the process ending, running the same command again continues it: a file already completed is skipped once its storage confirms it is still the version downloaded and the local file is still the one written, and a file in progress continues from the parts it has. It is created if missing, for you only, holds no credentials or signed URLs, and must not be inside the local directory: one given inside it by name is refused before anything is sent, and one inside it through a link when the download starts. Each folder downloaded needs one of its own. The engine relies on it only where no other user can change it, and otherwise refuses it with Files.CheckpointRefused once it has listed the remote folder, before any file is fetched. Once the download completes, the checkpoint records nothing: running the command again then downloads the folder anew. For piped folders, a script block such as { Join-Path ~/.files-downloads $_.DisplayName } gives each its own.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: InputObject
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: RemotePath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -CheckpointRestart

What a file does when the checkpoint records a download of it that cannot be continued, such as one whose downloaded bytes are gone: Fail, the default, fails with Files.CheckpointRefused before anything is fetched; StartOver discards that file's own stage and checkpoint and downloads it again from its first byte, never touching the local file or another transfer's checkpoint. A file whose version changed, or whose stage is gone, starts over whatever this says. Needs -CheckpointDirectory.

```yaml
Type: FilesCom.PowerShell.FilesCheckpointRestart
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Confirm

Prompts you for confirmation before running the cmdlet.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases:
- cf
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Connection

The connection to use, from Connect-FilesAccount. Defaults to the connection of the -InputObject given, and otherwise to the current runspace's default connection.

```yaml
Type: FilesCom.PowerShell.FilesConnection
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Force

Replaces a regular file at a downloaded file's name, giving the new file that file's group, access control list and permissions; nothing is replaced when those cannot be carried over. A folder, a symbolic link or another special file at that name is never replaced or followed, and fails the download with Files.LocalFile. Without -Force, a file whose name is taken fails the download with Files.LocalFile, and the file there stays as it is. Checking that the entry is a regular file and replacing it are two steps, so another process changing the folder in between can have a different entry at that name replaced, never followed.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -InputObject

A folder written by another FilesCom command, such as Get-FilesChildItem or Get-FilesFile. Its path and connection are used, and its requests run in the workspace it was read in unless -WorkspaceScope or -NoWorkspaceScope says otherwise. An object that is not a folder is refused.

```yaml
Type: FilesCom.PowerShell.Models.RemoteFile
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: InputObject
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Limiter

A limiter from New-FilesTransferLimiter whose request slots and buffer space every file of this download shares, with other transfers too.

```yaml
Type: FilesCom.PowerShell.FilesTransferLimiter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -LiteralPath

The local directory to download into, used exactly as written, and created with the folders above it if missing when the download starts; they stay whatever follows. The folder's contents go directly into it. For piped folders, a script block such as { Join-Path ./downloads $_.DisplayName } gives each its own directory; two folders of one command never download into the same one, or one inside the other.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: InputObject
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: RemotePath
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -MaxConcurrency

The most parts of each file fetched at the same time, from 1 to 64. Unset, the engine's automatic policy decides how many as the download goes, within its own limits. Parts are fetched at once only when the file's download is bound to a version, as Receive-FilesFile describes. A -Limiter's request budget also applies.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -MaxConcurrentFiles

The most files downloaded at once, from 1 to 64. Unset, the engine's automatic policy decides how many as the download goes, within its own limits. Each file fetches its own parts within the -Limiter.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -NoWorkspaceScope

Runs this command's requests in no workspace, even if the connection has one. The connection itself is unchanged.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PartSize

The size in bytes of each part fetched, 1 or more; by default 8 MiB. A file whose storage cannot bind its download to a version comes in one request.

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PassThru

Writes the completed download.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -RemotePath

The remote folder to download, used exactly as written; an empty string for the site's root.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: RemotePath
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -WhatIf

Runs the command in a mode that only reports what would happen without performing the actions.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases:
- wi
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -WorkspaceScope

Runs this command's requests in this workspace instead of the connection's or, for an -InputObject, instead of the workspace it was read in, including workspace 0. The connection and the object are unchanged.

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```
### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### FilesCom.PowerShell.Models.RemoteFile

Folders from commands such as Get-FilesChildItem or Get-FilesFile, as -InputObject.

## OUTPUTS

### None

Nothing, unless -PassThru is given.

### FilesCom.PowerShell.FilesDirectoryTransferResult

With -PassThru, each completed download: every file with its relative, local and remote paths, its size and whether an earlier run completed it; the empty folders created; the bytes and the time taken. Excluded is always empty for a download.

## NOTES

Receive-FilesFile downloads one file, and Send-FilesDirectory uploads a folder. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
