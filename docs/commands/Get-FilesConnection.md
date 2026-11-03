---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Get-FilesConnection
---

# Get-FilesConnection

## SYNOPSIS

Gets the current runspace's default connection, or every open connection it made.

## SYNTAX

### Default (Default)

```
Get-FilesConnection
```

### All

```
Get-FilesConnection -All
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Get-FilesConnection writes the connection FilesCom commands use by default in the current runspace, if there is one. With -All it writes every open connection the runspace made. Connection objects show safe metadata only, never credentials.

## EXAMPLES

### Example 1: Show the default connection

```powershell
Get-FilesConnection
```

Writes the default connection, or nothing when there is none.

### Example 2: List every open connection

```powershell
Get-FilesConnection -All | Format-Table BaseUrl, AuthenticationMethod, Username
```

Writes each open connection the runspace made.

## PARAMETERS

### -All

Writes every open connection the current runspace made, not only its default.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: All
  Position: Named
  IsRequired: true
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

The command takes no pipeline input.

## OUTPUTS

### FilesCom.PowerShell.FilesConnection

The default connection, or with -All every open connection.

## NOTES

Each runspace has its own default connection. A default chosen in one runspace never affects another. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()

