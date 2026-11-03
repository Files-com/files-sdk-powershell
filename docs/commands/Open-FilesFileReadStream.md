---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Open-FilesFileReadStream
---

# Open-FilesFileReadStream

## SYNOPSIS

Opens a remote file to read as a forward-only stream.

## SYNTAX

### __AllParameterSets

```
Open-FilesFileReadStream [-Path] <string> [-MaxConcurrency <int>] [-PartSize <long>]
 [-Connection <FilesConnection>] [-WorkspaceScope <long>] [-NoWorkspaceScope]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Open-FilesFileReadStream opens the file at -Path and writes a FilesRemoteReadStream, a System.IO.Stream that reads the file forward from its first byte. The Files.com engine downloads the file behind the stream as it is read, bound to the version of the file there when it was opened where its storage allows, as Receive-FilesFile describes, with every part checked, and reads ahead of the reader by at most the parts in progress and 1 MiB: a file larger than memory or the local disk can be processed as it arrives. The command returns once the first bytes can be read, or the file is known to be empty; a failure before then, such as a file that does not exist, is the command's error, and it writes nothing.

The stream goes on after the command returns, with the connection and workspace it was opened with. A failure of the download reaches the caller as it reads, once it has read everything before it, as a FilesException, such as Files.SourceChanged when the file changed during the download. Closing the connection, with Disconnect-FilesAccount or when its runspace closes, stops the download: a read then fails with Files.ConnectionClosed. The stream does not seek and has no length or position; those members throw NotSupportedException, as for other forward-only streams.

Dispose the stream when done, in a finally block: disposing it before its end stops the download, and returns once it has stopped. Ctrl+C does not interrupt a read in progress, which returns when data arrives or the connection's -ReadTimeoutSeconds passes; to download a whole file with Ctrl+C, resuming and progress, use Receive-FilesFile.

## EXAMPLES

### Example 1: Read a large text file line by line

```powershell
$stream = Open-FilesFileReadStream -Path logs/2026/access.log
try {
    $reader = [System.IO.StreamReader]::new($stream)
    while ($null -ne ($line = $reader.ReadLine())) {
        if ($line -match ' 500 ') { $line }
    }
}
finally {
    $stream.Dispose()
}
```

Writes the lines that record a server error, holding only a few of them in memory at a time.

### Example 2: Read the first bytes of a file and stop

```powershell
$stream = Open-FilesFileReadStream -Path media/video.mp4
try {
    $header = [byte[]]::new(16)
    $read = $stream.Read($header, 0, $header.Length)
}
finally {
    $stream.Dispose()
}
```

Reads the first bytes, then stops the download of the rest.

## PARAMETERS

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

### -MaxConcurrency

The most parts fetched at the same time, from 1 to 64. Unset, the engine's automatic policy decides how many as the download goes, within its own limits. Parts are fetched at once only when the download is bound to a version of the file.

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

The size in bytes of each part fetched, 1 or more; by default 8 MiB. The stream holds at most the parts in progress and 1 MiB ahead of the reader.

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

### -Path

The remote file, used exactly as written.

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
  ValueFromPipelineByPropertyName: true
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

### System.String

Remote paths, from the Path property of piped objects.

## OUTPUTS

### FilesCom.PowerShell.FilesRemoteReadStream

The open stream, a System.IO.Stream: its Path is the remote file and its Connection the connection it reads with.

## NOTES

The stream uses the engine's default part size and concurrency unless -PartSize or -MaxConcurrency says otherwise. The connection's -OperationTimeout bounds opening the stream, not reading it after the command returns. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
