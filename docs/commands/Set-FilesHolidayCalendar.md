---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Set-FilesHolidayCalendar
---

# Set-FilesHolidayCalendar

## SYNOPSIS

Update Holiday Calendar.

## SYNTAX

### Id (Default)

```
Set-FilesHolidayCalendar [<parameters of the Id set>]
```

### InputObject

```
Set-FilesHolidayCalendar [<parameters of the InputObject set>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Update Holiday Calendar.

The command accepts holiday calendars from the pipeline as -InputObject, and uses each one's Id and the connection that fetched it.

When given -InputObject, the command updates that object with the values Files.com returns and writes it.

The command supports -WhatIf, which describes the change without sending it to Files.com, and -Confirm, which asks before sending it.

## EXAMPLES

### Example 1: Update Holiday Calendar

```powershell
Set-FilesHolidayCalendar -Id 1 -Definition @{ months = @{ '0' = @(@{ name = 'Good Friday'; function = 'easter(year)'; function_modifier = -2 }); '1' = @(@{ name = 'New Year''s Day'; mday = 1; observed = 'to_weekday_if_weekend(date)' }, @{ name = 'Third Monday'; week = 3; wday = 1 }); '11' = @(@{ name = 'Day after Thanksgiving'; week = 4; wday = 4; day_offset = 1 }); '12' = @(@{ name = 'Christmas Eve Early Close'; mday = 24; start_time = '13:00'; end_time = '17:00'; year_ranges = @{ from = 2026 } }) } } -Name 'Company Holidays'
```

Writes the holiday calendar as Files.com returns it.

### Example 2: Act on holiday calendars from the pipeline

```powershell
Get-FilesHolidayCalendar -Id 1 | Set-FilesHolidayCalendar -Definition @{ months = @{ '0' = @(@{ name = 'Good Friday'; function = 'easter(year)'; function_modifier = -2 }); '1' = @(@{ name = 'New Year''s Day'; mday = 1; observed = 'to_weekday_if_weekend(date)' }, @{ name = 'Third Monday'; week = 3; wday = 1 }); '11' = @(@{ name = 'Day after Thanksgiving'; week = 4; wday = 4; day_offset = 1 }); '12' = @(@{ name = 'Christmas Eve Early Close'; mday = 24; start_time = '13:00'; end_time = '17:00'; year_ranges = @{ from = 2026 } }) } } -Name 'Company Holidays'
```

Takes the holiday calendar from the pipeline and uses its Id and connection.

### Example 3: Preview the change

```powershell
Set-FilesHolidayCalendar -Id 1 -Definition @{ months = @{ '0' = @(@{ name = 'Good Friday'; function = 'easter(year)'; function_modifier = -2 }); '1' = @(@{ name = 'New Year''s Day'; mday = 1; observed = 'to_weekday_if_weekend(date)' }, @{ name = 'Third Monday'; week = 3; wday = 1 }); '11' = @(@{ name = 'Day after Thanksgiving'; week = 4; wday = 4; day_offset = 1 }); '12' = @(@{ name = 'Christmas Eve Early Close'; mday = 24; start_time = '13:00'; end_time = '17:00'; year_ranges = @{ from = 2026 } }) } } -Name 'Company Holidays' -WhatIf
```

Describes the change without sending anything to Files.com.

## PARAMETERS

### -Id

Holiday Calendar ID.

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Id
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Definition

Holiday rules for the calendar. Give a hashtable or PSCustomObject; keys keep their case and values their types.

```yaml
Type: System.Collections.IDictionary
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Id
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

### -Name

Holiday Calendar name.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Id
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

The holiday calendar to act on, such as one written by another FilesCom command. The command uses its Id and the connection that fetched it.

```yaml
Type: FilesCom.PowerShell.Models.HolidayCalendar
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

### FilesCom.PowerShell.Models.HolidayCalendar

A holiday calendar from another FilesCom command, as -InputObject.

## OUTPUTS

### FilesCom.PowerShell.Models.HolidayCalendar

The holiday calendar as Files.com returned it.

## NOTES

Failures are written as error records with IDs such as Files.NotFound or Files.Authentication. See about_FilesCom for connections, paging, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
