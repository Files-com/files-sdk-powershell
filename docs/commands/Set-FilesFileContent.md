---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Set-FilesFileContent
---

# Set-FilesFileContent

## SYNOPSIS

Writes a whole remote file from text or bytes.

## SYNTAX

### __AllParameterSets

```
Set-FilesFileContent [-Path] <string> [-Value] <Object> [-MkdirParents]
 [-Connection <FilesConnection>] [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Set-FilesFileContent writes one file at -Path from everything given to -Value, whether piped or passed, and uploads it once, with the engine's upload, after the last value has arrived. Strings are joined into one UTF-8 text without a byte order mark, each string after the first on a new line (the platform's newline, CRLF on Windows), and no newline is added after the last; bytes and byte arrays are written one after another, as they were when given, even if the command that gave an array changes or reuses it afterwards. A single string or byte array is therefore written exactly as given. The file is created, or replaced once the upload completes; until then any file already there is unchanged. Use Send-FilesFile for a large file, a local file or a stream.

Text and bytes together, $null and values of any other type are refused with Files.InvalidArgument before anything is sent. A pipeline ended early by a terminating error or by being stopped, such as with Ctrl+C, sends nothing either. A non-terminating error earlier in the pipeline does not end it, and the values that still arrive are written: when the file must not be written after such an error, make it terminating, for example with -ErrorAction Stop on the command that produces the content. When the pipeline gives no value at all, nothing is written; to write an empty file, give -Value ''.

-WhatIf describes the one upload, with the size of all the content, without contacting Files.com, and -Confirm asks once.

## EXAMPLES

### Example 1: Write a text file

```powershell
@{ enabled = $true } | ConvertTo-Json | Set-FilesFileContent -Path config/settings.json
```

Replaces config/settings.json with the JSON text.

### Example 2: Write bytes into a new folder

```powershell
Set-FilesFileContent -Path exports/2026/data.bin -Value ([System.IO.File]::ReadAllBytes("$PWD/data.bin")) -MkdirParents
```

Creates exports/2026 if missing.

### Example 3: Write lines as one file

```powershell
Get-Content ./hosts.txt | Where-Object { $_ -notmatch '^#' } | Set-FilesFileContent -Path config/hosts.txt
```

Uploads the lines that are not comments as one file, with one upload. To upload a local text file exactly, with its own line endings, give it as one string with Get-Content -Raw, or use Send-FilesFile.

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
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Value

The content: strings, written as UTF-8 with each string after the first on a new line, or bytes and byte arrays, written as they were when given. Every value piped goes into the one file.

```yaml
Type: System.Object
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 1
  IsRequired: true
  ValueFromPipeline: true
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

### System.Object

Strings, or bytes and byte arrays, as -Value; everything piped is written as one file.

## OUTPUTS

### None

Nothing.

## NOTES

The content is held in memory; nothing is uploaded until it has all been given, and the connection's -OperationTimeout counts the time the pipeline takes to give it. Piping a byte array sends it one byte at a time; for large content, give the array to -Value, or use Send-FilesFile. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
