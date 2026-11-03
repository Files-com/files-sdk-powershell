---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Test-FilesPath
---

# Test-FilesPath

## SYNOPSIS

Tells whether a file or folder is at a remote path.

## SYNTAX

### __AllParameterSets

```
Test-FilesPath [-Path] <string> [-PathType <FilesPathType>] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Test-FilesPath returns $true when Files.com has a file or folder at -Path, and $false when it has nothing there, or, with -PathType, nothing of that kind. It changes nothing. Any other error, such as missing permission, fails the command rather than returning $false.

## EXAMPLES

### Example 1: Check for a file before downloading it

```powershell
if (Test-FilesPath -Path reports/daily.csv -PathType File) {
    Receive-FilesFile -RemotePath reports/daily.csv -LiteralPath ./daily.csv
}
```

Downloads the report only if it exists.

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

The remote path, used exactly as written.

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

### -PathType

Whether to look for a file, a folder or either; by default either.

```yaml
Type: FilesCom.PowerShell.FilesPathType
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

### System.Boolean

Whether something of the kind asked for is at the path.

## NOTES

Any checks for a file first and then, if there is none, for a folder: two requests when nothing is there. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
