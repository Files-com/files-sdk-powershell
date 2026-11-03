---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Get-FilesFileContent
---

# Get-FilesFileContent

## SYNOPSIS

Reads a whole remote file as text or as bytes.

## SYNTAX

### __AllParameterSets

```
Get-FilesFileContent [-Path] <string> [-AsByteStream] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Get-FilesFileContent downloads the file at -Path into memory with the engine's download, bound to one version of the file where its storage allows, and writes it as one string, decoded as UTF-8 (a UTF-8 byte order mark is dropped), or with -AsByteStream as one byte array. The web app's page and a file that changed during the download are refused rather than returned. Use Receive-FilesFile for a file too large to hold in memory, or to write it to a local file or stream.

## EXAMPLES

### Example 1: Read a small text file

```powershell
$settings = Get-FilesFileContent -Path config/settings.json | ConvertFrom-Json
```

Reads and parses a JSON file.

### Example 2: Read a file's bytes

```powershell
[byte[]] $bytes = Get-FilesFileContent -Path images/logo.png -AsByteStream
```

Keeps the bytes as they are.

## PARAMETERS

### -AsByteStream

Writes the file's bytes, as one byte array, instead of its text.

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

### System.String

The file's text.

### System.Byte[]

With -AsByteStream, the file's bytes.

## NOTES

The whole file is held in memory. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
