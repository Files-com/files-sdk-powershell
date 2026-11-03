---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Set-FilesConnection
---

# Set-FilesConnection

## SYNOPSIS

Makes a connection the default of the current runspace, or clears the default.

## SYNTAX

### Connection (Default)

```
Set-FilesConnection [-Connection] <FilesConnection> [-PassThru]
```

### Clear

```
Set-FilesConnection -Clear
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Set-FilesConnection chooses the connection FilesCom commands use in the current runspace when they are not given -Connection. The choice is local to the runspace and is never saved. With -Clear, commands need -Connection until a default is set again; the connection itself stays open.

## EXAMPLES

### Example 1: Switch the default connection

```powershell
Set-FilesConnection -Connection $staging
```

Commands in this runspace now use the staging connection by default.

### Example 2: Require an explicit connection

```powershell
Set-FilesConnection -Clear
```

Commands fail with Files.NoConnection unless given -Connection.

## PARAMETERS

### -Clear

Clears the default, so commands need -Connection. The connection itself stays open.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Clear
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Connection

The connection to make the default. It must be open.

```yaml
Type: FilesCom.PowerShell.FilesConnection
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Connection
  Position: 0
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PassThru

Writes the new default connection.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Connection
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

### FilesCom.PowerShell.FilesConnection

The connection to make the default.

## OUTPUTS

### None

Nothing, unless -PassThru is given.

### FilesCom.PowerShell.FilesConnection

With -PassThru, the new default connection.

## NOTES

Setting the default changes nothing on Files.com. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()

