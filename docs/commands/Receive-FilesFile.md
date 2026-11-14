---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Receive-FilesFile
---

# Receive-FilesFile

## SYNOPSIS

Downloads a file from Files.com to a local file or into a stream.

## SYNTAX

### LiteralPath (Default)

```
Receive-FilesFile [-RemotePath] <string> [-LiteralPath] <string> [-Force] [-MaxConcurrency <int>] [-CheckDownloadModificationTime <bool>]
 [-PartSize <long>] [-Limiter <FilesTransferLimiter>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### Stream

```
Receive-FilesFile [-RemotePath] <string> -OutputStream <Stream> [-MaxConcurrency <int>] [-CheckDownloadModificationTime <bool>]
 [-PartSize <long>] [-Limiter <FilesTransferLimiter>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### InputObject

```
Receive-FilesFile [-LiteralPath] <string> -InputObject <RemoteFile> [-Force] [-MaxConcurrency <int>] [-CheckDownloadModificationTime <bool>]
 [-PartSize <long>] [-Limiter <FilesTransferLimiter>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Receive-FilesFile downloads the file at a remote path to a local file, or writes it into a stream you provide. Remote paths are used exactly as written; the local path is used exactly as written too, or, when it names an existing folder, the file is written into it under its remote name.

Where the file's download URL is an HTTPS URL of an object at an Amazon S3 general-purpose endpoint, which answers for the version of the very object it names, the download is bound to the version of the file there when it starts: the first part is asked for alone, and the strong entity tag and size its answer gives are the version. The rest is fetched in parts of -PartSize bytes, up to -MaxConcurrency at once, each asking the storage for that version, and every part must come back as exactly those bytes of it, or the download fails with Files.SourceChanged, so bytes of two versions are never put together. A part that fails in a way that may be temporary is fetched again alone, and a download address about to expire, or one the storage refuses as expired, is renewed for the same object; any other refusal, such as an ordinary 403, fails the download. From any other storage, such as an S3-compatible service, another kind of S3 endpoint or storage Files.com serves itself, the file comes in one request, bound to no version, which starts again from the first byte if it fails partway. An answer from the Files.com web app, such as a page for an expired link, is refused with Files.InvalidResponse before any of it is written. Progress shows the file's size as soon as Files.com or the storage server states it. Into a stream, parts are written in order, each kept in memory, within the -Limiter's buffer space, until those before it are written; a stream therefore holds only whole parts of the file after a failure.

A local file is written by the Files.com engine into a stage, a file in a private folder the engine creates beside the destination, named like .files-download-0123456789abcdef, and put in place under its name only once every byte has been written and checked. Until then the name is untouched: a download that fails or is stopped leaves no partial file there, and without -CheckpointPath the private folder is removed however the download ends. Without -Force the file is put in place only where nothing is at its name: an existing file, folder or link, even a link to nothing, is left as it is and the download fails with Files.LocalFile, whose PublicationProblem is DestinationExists. With -Force it replaces a regular file only: a folder, a link or any other special file is never replaced or followed (DestinationNotRegularFile). Something already at the name is refused before anything is sent or confirmed, even under -WhatIf; something that appears there during the download is refused when the file is put in place, and nothing is replaced. With -Force, checking that the entry is a regular file and replacing it are two steps, so another program changing the folder at that moment can have a different entry replaced, never followed.

A file that replaces another takes that file's group, access control list and permissions, and belongs to you: a writable file of another user's is replaced by one of yours, whose owner permissions are yours and whose group and other permissions stay as they were. If the new file cannot be given that group or access control list, for example because you are not a member of the group, nothing is replaced and the download fails with Files.LocalFile (AccessNotKept) once the file has been downloaded; the existing file stays exactly as it was. On Windows the new file takes the replaced file's access control list, and a hidden file it replaces stays hidden. A new file gets the group and permissions any new file in the folder gets, and is never hidden.

The stage stays open while it is written, so no other process can rename or delete it, and the destination folder and the folders above it are held so that they cannot be renamed during the download.

With -CheckpointPath, a download bound to a version keeps its stage when it is stopped, by Ctrl+C or the pipeline stopping, when it fails, or when its process ends, and the checkpoint records the version and the parts written. Running the same command again with the same checkpoint continues it once the storage confirms that version is still the file's, by answering for its first byte asked for as that version: only the parts not yet written are fetched, and the result's ResumedBytes counts the bytes earlier runs had written. If the version is gone, the download starts over from the first byte, as the file is now, and nothing of the old version is put in place. A download not bound to a version always starts over. When the finished file is refused at its name, such as by a file that appeared there during the download, its bytes are kept with the checkpoint (the error's DownloadRetained is true): running the command again, with -Force or once the name is free, puts them in place without fetching them again, once the storage confirms the version. A run that finds the very file an earlier run put in place, which that run could not record, completes without fetching it or asking about it again, and its result's CompletedEarlier is true; for this, a file at the name is left for the engine to judge whenever the checkpoint exists, rather than refused before anything is sent. The checkpoint is deleted once the file is in place. A checkpoint that cannot be continued, such as one whose stage no longer holds what it records, is refused with Files.CheckpointRefused before anything is fetched, unless -CheckpointRestart StartOver says to discard that download's own stage and checkpoint; a file that is not a checkpoint of this download is never discarded. Each download needs a checkpoint of its own: a later file piped to the same command with the same -CheckpointPath is refused with Files.InvalidArgument.

A stream is borrowed: the command writes to it from its current position and leaves it open. A download into a stream is never continued, so it takes no -CheckpointPath or -Force.

## EXAMPLES

### Example 1: Download a file

```powershell
Receive-FilesFile -RemotePath reports/2026/report.csv -LiteralPath ./report.csv
```

Writes report.csv, failing if it already exists.

### Example 2: Download every file in a folder

```powershell
Get-FilesChildItem -Path reports/2026 | Where-Object Type -EQ file | Receive-FilesFile -LiteralPath ./downloads -Force
```

Writes each file of the remote folder into ./downloads, replacing files already there.

### Example 3: Download a large file that can be continued

```powershell
Receive-FilesFile -RemotePath backups/2026/db.tar -LiteralPath ./db.tar -CheckpointPath ./db.tar.checkpoint
```

If a download bound to a version is stopped or fails, running the same command again fetches only the parts not yet written, once the storage confirms the file has not changed, and puts db.tar in place when it is complete.

### Example 4: Download into memory

```powershell
$buffer = [System.IO.MemoryStream]::new()
Receive-FilesFile -RemotePath config/settings.json -OutputStream $buffer
[System.Text.Encoding]::UTF8.GetString($buffer.ToArray())
```

Writes the file into a memory stream and reads it as text.

## PARAMETERS

### -CheckpointPath

A local file in which the download records its progress, so that when it is stopped, by Ctrl+C, a failure or the process ending, running the same command again with the same connection, remote path, local file and checkpoint continues it: once the storage confirms the version is still the file's, only the parts not yet written are fetched; a version that is gone starts over from the first byte. A download to a file bound to no version always starts over. The engine writes the checkpoint for you only, without credentials or signed URLs, and deletes it once the file is in place. Its folder must exist and be one that only you, or the system, can change, and each download needs a checkpoint of its own: it is refused, with Files.CheckpointRefused, when it records another transfer or cannot be continued, unless -CheckpointRestart StartOver says to download the file again. Not with -OutputStream.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: LiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: InputObject
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

What the download does when its checkpoint records a download that cannot be continued, such as one whose stage no longer holds what it records: Fail, the default, fails with Files.CheckpointRefused before anything is fetched; StartOver discards that download's own stage and checkpoint and downloads the file again from its first byte. A file that is not a checkpoint of this download, or one kept where another user could have changed it, is refused either way. Needs -CheckpointPath.

```yaml
Type: FilesCom.PowerShell.FilesCheckpointRestart
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: LiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: InputObject
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues:
- Fail
- StartOver
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

Replaces a regular file that already exists at the destination, giving the new file that file's group, access control list and permissions as described above; nothing is replaced when those cannot be carried over. A folder, a link or any other entry that is not a regular file is never replaced or followed. Without it, anything at the name is left untouched and the download fails with Files.LocalFile. Another process that changes the folder between the check and the replacement can have a different entry at that name replaced, never followed.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: LiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: InputObject
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

A file written by another FilesCom command, such as Get-FilesChildItem. Its path and connection are used.

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

### -CheckDownloadModificationTime

Whether this download checks Last-Modified timestamps. Unset inherits the connection setting, which defaults to true. Set -CheckDownloadModificationTime:$false to suppress automatic timestamp headers and comparisons, including for -OutputStream. Source changes detectable only by timestamps can then go unnoticed; stronger ETag, size, concurrency and checkpoint checks still apply. The value is fixed when the download starts.

```yaml
Type: System.Boolean
DefaultValue: 'connection setting'
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

### -Limiter

A limiter from New-FilesTransferLimiter whose request slots and buffer space this download shares with other transfers.

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

The local file to write, used exactly as written, or an existing folder to write the file into under its remote name. The folder must exist.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: LiteralPath
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: InputObject
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -MaxConcurrency

The most parts fetched at the same time, from 1 to 64. Unset, the engine's automatic policy decides how many as the download goes, within its own limits. Parts are fetched at once only when the download is bound to a version of the file. A -Limiter's request budget also applies.

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

### -OutputStream

The stream to write to, from its current position. It is borrowed: the command writes to it and leaves it open. A download into a stream cannot be continued, so it takes no -CheckpointPath.

```yaml
Type: System.IO.Stream
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Stream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PartSize

The size in bytes of each part fetched, 1 or more; by default 8 MiB. Into a stream, each part waits in memory until the parts before it have been written, so a part is no larger than the -Limiter's buffer space.

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

Writes each completed download as a FilesDownloadResult.

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

The remote path of the file, used exactly as written.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: LiteralPath
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Stream
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

Runs this command's requests in this workspace instead of the connection's, including workspace 0. The connection itself is unchanged.

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

Files from commands such as Get-FilesChildItem, as -InputObject.

## OUTPUTS

### None

Nothing, unless -PassThru is given.

### FilesCom.PowerShell.FilesDownloadResult

With -PassThru, each completed download: the remote and local paths, the file's size, whether the download was bound to one version, the parts fetched and the bytes received by this run, the bytes earlier runs with the same checkpoint had written (ResumedBytes), whether an earlier run had already put the file in place (CompletedEarlier), and the time taken. Its File property is always null: the version-bound download does not return the file's details, which Get-FilesFile reads.

## NOTES

An interrupted download to a local file continues with -CheckpointPath (see its description); a download into a stream cannot be continued. Get-FilesDownloadInfo returns a file's download metadata, such as its download URL, without downloading it. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()

