---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Send-FilesDirectory
---

# Send-FilesDirectory

## SYNOPSIS

Uploads a local folder and everything in it into a remote folder, several files at once.

## SYNTAX

### __AllParameterSets

```
Send-FilesDirectory [-LiteralPath] <string> [[-RemotePath] <string>] [-MaxConcurrentFiles <int>]
 [-AggressiveStartup] [-CheckpointDirectory <string>] [-Limiter <FilesTransferLimiter>]
 [-PassThru] [-Connection <FilesConnection>] [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf]
 [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Send-FilesDirectory uploads every regular file under -LiteralPath to the same relative path under -RemotePath, as Send-FilesFile uploads a file, and creates every folder that has nothing inside to upload. The folder is listed first, and its files are uploaded at most -MaxConcurrentFiles at once, in the order of their paths, sharing one limiter. Symbolic links are not followed: they, and anything that is neither a regular file nor a folder, are not uploaded, and -PassThru lists them in Excluded.

The automatic upload policy also shares a learned part-request target across files using the same destination profile. Each file's completed parts help decide that target, and later files retain what earlier files learned. The target applies across those files together; adding a file does not give it another full set of requests. Part-size adjustment remains specific to each file. When the module selects the default scaled S3 policy, without explicit automatic tuning, the target warm-starts once for all the files, at 1 part at a time, as Send-FilesFile describes for one file, and part sizes change only once that warm start is over; with -AggressiveStartup it starts at the policy's 150 parts at once instead, on a known high-throughput network, and adapts from there. Files start as the target allows, and more, one at a time, while too few of their parts are ready to send, within -MaxConcurrentFiles. Once the target reaches the scaled policy's 150, files of one part each, at most 128 at once, cannot fill it, so their parts alone no longer raise it or lower it for latency.

-MaxConcurrentFiles limits active files, while -Limiter bounds their combined part requests and buffer space. A shared limiter is a hard budget; independent commands using it do not share the automatic policy's learned state. Use Send-FilesDirectory for this batch behavior. Several paths or piped files passed to Send-FilesFile are uploaded individually in sequence.

The first failure stops the files in progress, and no other file starts; the command then fails with that error once every file has stopped. Files completed before then stay completed. With -CheckpointDirectory, running the same command again skips each file it completed while that file still has the bytes it sent, and continues each file in progress.

-WhatIf describes the upload without reading the folder or contacting Files.com.

## EXAMPLES

### Example 1: Upload a folder

```powershell
Send-FilesDirectory ./reports -RemotePath archive/reports
```

Uploads ./reports and everything in it into archive/reports, creating that folder if missing.

### Example 2: Upload a large folder so that it can be continued

```powershell
Send-FilesDirectory ./media -RemotePath media -CheckpointDirectory ~/.files-media-upload -MaxConcurrentFiles 8 -PassThru |
    Select-Object Bytes, Elapsed, @{ Name = 'Files'; Expression = { $_.Files.Count } }
```

Records its progress in ~/.files-media-upload; if the upload is stopped, running the same command again continues it.

### Example 3: Skip startup probes on a high-throughput network

```powershell
Send-FilesDirectory ./media -RemotePath media -AggressiveStartup
```

Where the files' parts go straight to Amazon S3, starts their shared target at the scaled S3 policy's 150 parts at once rather than probing the link from 1, and adapts from there.

## PARAMETERS

### -AggressiveStartup

Starts the files whose parts go straight to Amazon S3 as ordinary multipart uploads with their shared target at the scaled S3 policy's 150 parts at once, within the -Limiter, skipping the initial concurrency probes, and starts as many files as that target allows, within -MaxConcurrentFiles. Use it on a known high-throughput network, such as a high-capacity cloud VM. Without it, or with -AggressiveStartup:$false, the files probe the link from 1 part at a time, once, together, and add parts as the parts S3 has answered show they help, as Send-FilesFile describes. Either way the files then share one target that adapts with the same policy, and nothing carries over to another command. Files to other destinations start from their own defaults.

The default start is not a lower performance level: it can reach the same eventual throughput and concurrency ceiling, and only its ramp-up takes longer because it probes first. Leave -AggressiveStartup off when gradual startup helps avoid overwhelming a residential or constrained connection. Both starts keep -MaxConcurrentFiles, the -Limiter and adaptive backoff.

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

### -CheckpointDirectory

A local folder in which the upload records its progress, so that when it is stopped, by Ctrl+C, a failure or the process ending, running the same command again continues it: a file already completed is skipped while it still has the bytes that were sent, and a file in progress continues. It is created if missing, for you only, holds no credentials or signed URLs, and must not be inside the folder uploaded. The engine relies on it only where no other user can change it, and refuses it otherwise, with Files.CheckpointRefused.

```yaml
Type: System.String
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

The connection to use, from Connect-FilesAccount. Defaults to the current runspace's default connection.

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

### -Limiter

A limiter from New-FilesTransferLimiter whose request slots and buffer space every file of this upload shares, with other transfers too.

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

The local folder to upload, used exactly as written.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -MaxConcurrentFiles

The most files uploaded at once, from 1 to 64. Unset, the engine's automatic policy decides how many as the upload goes, within its own limits. This limits files rather than part requests. Their requests share the automatic target and the -Limiter; the limit is not multiplied by the number of files.

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

### -PassThru

Writes the completed upload.

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

The remote folder to upload into, created if missing; an empty string for the site's root. Defaults to a folder named like the local folder in the site's root.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 1
  IsRequired: false
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

## OUTPUTS

### None

Nothing, unless -PassThru is given.

### FilesCom.PowerShell.FilesDirectoryTransferResult

With -PassThru, the completed upload: every file with its relative, local and remote paths, its size and whether an earlier run completed it; the folders created; the entries excluded; the bytes and the time taken.

## NOTES

Receive-FilesDirectory downloads a whole folder, and continues the same way with -CheckpointDirectory. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
