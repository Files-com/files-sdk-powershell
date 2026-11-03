---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: New-FilesHolidayCalendar
---

# New-FilesHolidayCalendar

## SYNOPSIS

Create Holiday Calendar.

## SYNTAX

### __AllParameterSets

```
New-FilesHolidayCalendar [<parameters>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Create Holiday Calendar.

The command supports -WhatIf, which describes the change without sending it to Files.com, and -Confirm, which asks before sending it.

## EXAMPLES

### Example 1: Create Holiday Calendar

```powershell
New-FilesHolidayCalendar -Definition @{ months = @{ '0' = @(@{ name = 'Good Friday'; function = 'easter(year)'; function_modifier = -2 }); '1' = @(@{ name = 'New Year''s Day'; mday = 1; observed = 'to_weekday_if_weekend(date)' }, @{ name = 'Third Monday'; week = 3; wday = 1 }); '11' = @(@{ name = 'Thanksgiving'; week = 4; wday = 4 }); '12' = @(@{ name = 'Christmas Eve Early Close'; mday = 24; start_time = '13:00'; end_time = '17:00'; year_ranges = @{ from = 2026 } }) } } -Name 'Company Holidays'
```

Writes the holiday calendar returned.

### Example 2: Preview the change

```powershell
New-FilesHolidayCalendar -Definition @{ months = @{ '0' = @(@{ name = 'Good Friday'; function = 'easter(year)'; function_modifier = -2 }); '1' = @(@{ name = 'New Year''s Day'; mday = 1; observed = 'to_weekday_if_weekend(date)' }, @{ name = 'Third Monday'; week = 3; wday = 1 }); '11' = @(@{ name = 'Thanksgiving'; week = 4; wday = 4 }); '12' = @(@{ name = 'Christmas Eve Early Close'; mday = 24; start_time = '13:00'; end_time = '17:00'; year_ranges = @{ from = 2026 } }) } } -Name 'Company Holidays' -WhatIf
```

Describes the change without sending anything to Files.com.

## PARAMETERS

### -Definition

Holiday rules for the calendar. Give a hashtable or PSCustomObject; keys keep their case and values their types.

```yaml
Type: System.Collections.IDictionary
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Name

Holiday Calendar name.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
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

The connection to use, from Connect-FilesAccount. Defaults to the connection of the -InputObject or -Continuation given, and otherwise to the current runspace's default connection.

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

### -WorkspaceScope

Runs this command's requests in this workspace, including workspace 0, instead of the connection's or, for an -InputObject, instead of the workspace the object was read in. The connection and the object are unchanged.

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

### -WhatIf

Shows what the cmdlet would change without sending anything to Files.com.

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

The command takes no pipeline input.

## OUTPUTS

### FilesCom.PowerShell.Models.HolidayCalendar

The holiday calendar as Files.com returned it.

## NOTES

Failures are written as error records with IDs such as Files.NotFound or Files.Authentication. See about_FilesCom for connections, paging, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
