---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Open-FilesFileWriteStream
---

# Open-FilesFileWriteStream

## SYNOPSIS

Opens a remote file to write as a forward-only stream.

## SYNTAX

### __AllParameterSets

```
Open-FilesFileWriteStream [-Path] <string> [-MkdirParents] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Open-FilesFileWriteStream writes a FilesRemoteWriteStream, a System.IO.Stream whose bytes the Files.com engine uploads to -Path as they are written, as a file of unknown length: a file larger than memory or the local disk can be produced as it is made. A write waits while the upload is 1 MiB behind. Nothing is sent to Files.com unless the command goes ahead with the change; under -WhatIf, or when a requested confirmation is declined, the command opens nothing and writes nothing.

Closing the stream, with Dispose or Close, ends the file and waits until its upload has completed: the file is then created, or it replaces the file already there, which is unchanged until then. Closing it completes the file with what was written, including when the code writing it failed: to leave the remote file as it was, call the stream's Abandon method before closing it, as in a catch block. Abandon sends nothing more, and closing the stream then returns without an error, even when the upload had already failed, so that the error of the code writing it is the one reported.

A failure of the upload reaches the caller as a FilesException: from the write that finds it, which also closes the stream, or from closing it. A final request that failed after it may have reached Files.com is Files.TransferCompletionUnknown: whether the file was completed is unknown. Closing the connection, with Disconnect-FilesAccount or when its runspace closes, stops the upload: closing the stream then fails with Files.ConnectionClosed, and the file is not completed, unless the connection closed while the file was being completed, when whether it was is unknown. The stream goes on after the command returns, with the connection and workspace it was opened with. It does not seek, read or append, and has no length or position; those members throw NotSupportedException.

Ctrl+C does not interrupt a write in progress, which returns when the upload takes the bytes or the connection's -WriteTimeoutSeconds passes. To upload a local file or a stream in one command, with Ctrl+C, resuming and progress, use Send-FilesFile.

## EXAMPLES

### Example 1: Write a report as it is produced

```powershell
$stream = Open-FilesFileWriteStream -Path reports/processes.csv -MkdirParents
try {
    $writer = [System.IO.StreamWriter]::new($stream, [System.Text.UTF8Encoding]::new($false))
    foreach ($process in Get-Process) {
        $writer.WriteLine("$($process.Name),$($process.Id)")
    }
    $writer.Flush()
}
catch {
    $stream.Abandon()
    throw
}
finally {
    $stream.Dispose()
}
```

Uploads the lines as they are written and completes the file when the stream is disposed; if producing the report fails, Abandon leaves the remote file as it was.

### Example 2: Copy a stream into a remote file

```powershell
$source = [System.IO.File]::OpenRead("$PWD/archive.tar")
try {
    $stream = Open-FilesFileWriteStream -Path backups/archive.tar -ErrorAction Stop
    if ($stream) {
        try {
            $source.CopyTo($stream)
        }
        catch {
            $stream.Abandon()
            throw
        }
        finally {
            $stream.Dispose()
        }
    }
}
finally {
    $source.Dispose()
}
```

Copies the bytes with bounded memory. The local file is closed however the copy ends: when the remote file cannot be opened, when copying fails, which abandons the upload, and when completing the upload fails, whose error is the one reported. Under -WhatIf nothing is opened and nothing is copied. For a local file, Send-FilesFile uploads it in parallel parts with resuming.

## PARAMETERS

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

### -MkdirParents

Creates missing folders above the file.

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

### -Path

The remote file, used exactly as written, created or replaced once the stream is closed.

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

### None

This cmdlet takes no pipeline input.

## OUTPUTS

### FilesCom.PowerShell.FilesRemoteWriteStream

The open stream, a System.IO.Stream: its Path is the remote file, its Connection the connection it writes with, and IsAbandoned whether Abandon was called. Nothing, under -WhatIf or when the change is declined.

## NOTES

The upload sizes its parts itself, as for a stream of unknown length. The connection's -OperationTimeout bounds opening the stream, not writing it after the command returns. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
