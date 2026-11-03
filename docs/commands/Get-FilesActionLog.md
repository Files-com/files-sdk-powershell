---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Get-FilesActionLog
---

# Get-FilesActionLog

## SYNOPSIS

List Action Logs.

## SYNTAX

### List (Default)

```
Get-FilesActionLog [<parameters of the List set>]
```

### Page

```
Get-FilesActionLog [<parameters of the Page set>]
```

### Continuation

```
Get-FilesActionLog [<parameters of the Continuation set>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

List Action Logs.

Without -Page or -Continuation, the command writes each action log of every page, loading one page at a time as the pipeline consumes them. -First and -Skip bound what it writes, and stopping the pipeline early, as Select-Object -First does, loads no further page. A failure on any page stops the command, so a partial listing is never taken for a complete one.

## EXAMPLES

### Example 1: List all action logs

```powershell
Get-FilesActionLog
```

Writes every action log, loading one page at a time as the pipeline consumes them.

### Example 2: Get one page at a time

```powershell
$page = Get-FilesActionLog -Page
$next = Get-FilesActionLog -Continuation $page.Continuation
```

Writes the first page as a FilesPage object, then the next page from its continuation.

## PARAMETERS

### -Cursor

Used for pagination. When a list request has more records available, cursors are provided in the response headers `X-Files-Cursor-Next` and `X-Files-Cursor-Prev`. Send one of those cursor value here to resume an existing list from the next available record. Note: many of our SDKs have iterator methods that will automatically handle cursor-based pagination.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: List
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Page
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PerPage

Number of records to show per page. (Max: 1000, 1,000 or less is recommended).

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: List
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Page
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Filter

If set, return records where the specified field is equal to the supplied value. Valid fields are `folder`, `path`, `src`, `destination`, `created_at`, `user_id` or `username`. Valid field combinations are `[ folder ]`, `[ path ]`, `[ src ]`, `[ destination ]`, `[ created_at ]`, `[ user_id ]`, `[ username ]`, `[ folder, path ]`, `[ folder, src ]`, `[ folder, destination ]`, `[ folder, created_at ]`, `[ folder, user_id ]`, `[ folder, username ]`, `[ path, src ]`, `[ path, destination ]`, `[ path, created_at ]`, `[ path, user_id ]`, `[ path, username ]`, `[ src, destination ]`, `[ src, created_at ]`, `[ src, user_id ]`, `[ src, username ]`, `[ destination, created_at ]`, `[ destination, user_id ]`, `[ destination, username ]`, `[ created_at, user_id ]`, `[ created_at, username ]`, `[ user_id, username ]`, `[ folder, path, src ]`, `[ folder, path, destination ]`, `[ folder, path, created_at ]`, `[ folder, path, user_id ]`, `[ folder, path, username ]`, `[ folder, src, destination ]`, `[ folder, src, created_at ]`, `[ folder, src, user_id ]`, `[ folder, src, username ]`, `[ folder, destination, created_at ]`, `[ folder, destination, user_id ]`, `[ folder, destination, username ]`, `[ folder, created_at, user_id ]`, `[ folder, created_at, username ]`, `[ folder, user_id, username ]`, `[ path, src, destination ]`, `[ path, src, created_at ]`, `[ path, src, user_id ]`, `[ path, src, username ]`, `[ path, destination, created_at ]`, `[ path, destination, user_id ]`, `[ path, destination, username ]`, `[ path, created_at, user_id ]`, `[ path, created_at, username ]`, `[ path, user_id, username ]`, `[ src, destination, created_at ]`, `[ src, destination, user_id ]`, `[ src, destination, username ]`, `[ src, created_at, user_id ]`, `[ src, created_at, username ]`, `[ src, user_id, username ]`, `[ destination, created_at, user_id ]`, `[ destination, created_at, username ]`, `[ destination, user_id, username ]`, `[ created_at, user_id, username ]`, `[ folder, path, src, destination ]`, `[ folder, path, src, created_at ]`, `[ folder, path, src, user_id ]`, `[ folder, path, src, username ]`, `[ folder, path, destination, created_at ]`, `[ folder, path, destination, user_id ]`, `[ folder, path, destination, username ]`, `[ folder, path, created_at, user_id ]`, `[ folder, path, created_at, username ]`, `[ folder, path, user_id, username ]`, `[ folder, src, destination, created_at ]`, `[ folder, src, destination, user_id ]`, `[ folder, src, destination, username ]`, `[ folder, src, created_at, user_id ]`, `[ folder, src, created_at, username ]`, `[ folder, src, user_id, username ]`, `[ folder, destination, created_at, user_id ]`, `[ folder, destination, created_at, username ]`, `[ folder, destination, user_id, username ]`, `[ folder, created_at, user_id, username ]`, `[ path, src, destination, created_at ]`, `[ path, src, destination, user_id ]`, `[ path, src, destination, username ]`, `[ path, src, created_at, user_id ]`, `[ path, src, created_at, username ]`, `[ path, src, user_id, username ]`, `[ path, destination, created_at, user_id ]`, `[ path, destination, created_at, username ]`, `[ path, destination, user_id, username ]`, `[ path, created_at, user_id, username ]`, `[ src, destination, created_at, user_id ]`, `[ src, destination, created_at, username ]`, `[ src, destination, user_id, username ]`, `[ src, created_at, user_id, username ]`, `[ destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at ]`, `[ folder, path, src, destination, user_id ]`, `[ folder, path, src, destination, username ]`, `[ folder, path, src, created_at, user_id ]`, `[ folder, path, src, created_at, username ]`, `[ folder, path, src, user_id, username ]`, `[ folder, path, destination, created_at, user_id ]`, `[ folder, path, destination, created_at, username ]`, `[ folder, path, destination, user_id, username ]`, `[ folder, path, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id ]`, `[ folder, src, destination, created_at, username ]`, `[ folder, src, destination, user_id, username ]`, `[ folder, src, created_at, user_id, username ]`, `[ folder, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id ]`, `[ path, src, destination, created_at, username ]`, `[ path, src, destination, user_id, username ]`, `[ path, src, created_at, user_id, username ]`, `[ path, destination, created_at, user_id, username ]`, `[ src, destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at, user_id ]`, `[ folder, path, src, destination, created_at, username ]`, `[ folder, path, src, destination, user_id, username ]`, `[ folder, path, src, created_at, user_id, username ]`, `[ folder, path, destination, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id, username ]` or `[ folder, path, src, destination, created_at, user_id, username ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

```yaml
Type: System.Collections.IDictionary
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: List
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Page
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -FilterGt

If set, return records where the specified field is greater than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ folder ]`, `[ path ]`, `[ src ]`, `[ destination ]`, `[ created_at ]`, `[ user_id ]`, `[ username ]`, `[ folder, path ]`, `[ folder, src ]`, `[ folder, destination ]`, `[ folder, created_at ]`, `[ folder, user_id ]`, `[ folder, username ]`, `[ path, src ]`, `[ path, destination ]`, `[ path, created_at ]`, `[ path, user_id ]`, `[ path, username ]`, `[ src, destination ]`, `[ src, created_at ]`, `[ src, user_id ]`, `[ src, username ]`, `[ destination, created_at ]`, `[ destination, user_id ]`, `[ destination, username ]`, `[ created_at, user_id ]`, `[ created_at, username ]`, `[ user_id, username ]`, `[ folder, path, src ]`, `[ folder, path, destination ]`, `[ folder, path, created_at ]`, `[ folder, path, user_id ]`, `[ folder, path, username ]`, `[ folder, src, destination ]`, `[ folder, src, created_at ]`, `[ folder, src, user_id ]`, `[ folder, src, username ]`, `[ folder, destination, created_at ]`, `[ folder, destination, user_id ]`, `[ folder, destination, username ]`, `[ folder, created_at, user_id ]`, `[ folder, created_at, username ]`, `[ folder, user_id, username ]`, `[ path, src, destination ]`, `[ path, src, created_at ]`, `[ path, src, user_id ]`, `[ path, src, username ]`, `[ path, destination, created_at ]`, `[ path, destination, user_id ]`, `[ path, destination, username ]`, `[ path, created_at, user_id ]`, `[ path, created_at, username ]`, `[ path, user_id, username ]`, `[ src, destination, created_at ]`, `[ src, destination, user_id ]`, `[ src, destination, username ]`, `[ src, created_at, user_id ]`, `[ src, created_at, username ]`, `[ src, user_id, username ]`, `[ destination, created_at, user_id ]`, `[ destination, created_at, username ]`, `[ destination, user_id, username ]`, `[ created_at, user_id, username ]`, `[ folder, path, src, destination ]`, `[ folder, path, src, created_at ]`, `[ folder, path, src, user_id ]`, `[ folder, path, src, username ]`, `[ folder, path, destination, created_at ]`, `[ folder, path, destination, user_id ]`, `[ folder, path, destination, username ]`, `[ folder, path, created_at, user_id ]`, `[ folder, path, created_at, username ]`, `[ folder, path, user_id, username ]`, `[ folder, src, destination, created_at ]`, `[ folder, src, destination, user_id ]`, `[ folder, src, destination, username ]`, `[ folder, src, created_at, user_id ]`, `[ folder, src, created_at, username ]`, `[ folder, src, user_id, username ]`, `[ folder, destination, created_at, user_id ]`, `[ folder, destination, created_at, username ]`, `[ folder, destination, user_id, username ]`, `[ folder, created_at, user_id, username ]`, `[ path, src, destination, created_at ]`, `[ path, src, destination, user_id ]`, `[ path, src, destination, username ]`, `[ path, src, created_at, user_id ]`, `[ path, src, created_at, username ]`, `[ path, src, user_id, username ]`, `[ path, destination, created_at, user_id ]`, `[ path, destination, created_at, username ]`, `[ path, destination, user_id, username ]`, `[ path, created_at, user_id, username ]`, `[ src, destination, created_at, user_id ]`, `[ src, destination, created_at, username ]`, `[ src, destination, user_id, username ]`, `[ src, created_at, user_id, username ]`, `[ destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at ]`, `[ folder, path, src, destination, user_id ]`, `[ folder, path, src, destination, username ]`, `[ folder, path, src, created_at, user_id ]`, `[ folder, path, src, created_at, username ]`, `[ folder, path, src, user_id, username ]`, `[ folder, path, destination, created_at, user_id ]`, `[ folder, path, destination, created_at, username ]`, `[ folder, path, destination, user_id, username ]`, `[ folder, path, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id ]`, `[ folder, src, destination, created_at, username ]`, `[ folder, src, destination, user_id, username ]`, `[ folder, src, created_at, user_id, username ]`, `[ folder, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id ]`, `[ path, src, destination, created_at, username ]`, `[ path, src, destination, user_id, username ]`, `[ path, src, created_at, user_id, username ]`, `[ path, destination, created_at, user_id, username ]`, `[ src, destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at, user_id ]`, `[ folder, path, src, destination, created_at, username ]`, `[ folder, path, src, destination, user_id, username ]`, `[ folder, path, src, created_at, user_id, username ]`, `[ folder, path, destination, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id, username ]` or `[ folder, path, src, destination, created_at, user_id, username ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

```yaml
Type: System.Collections.IDictionary
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: List
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Page
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -FilterGteq

If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ folder ]`, `[ path ]`, `[ src ]`, `[ destination ]`, `[ created_at ]`, `[ user_id ]`, `[ username ]`, `[ folder, path ]`, `[ folder, src ]`, `[ folder, destination ]`, `[ folder, created_at ]`, `[ folder, user_id ]`, `[ folder, username ]`, `[ path, src ]`, `[ path, destination ]`, `[ path, created_at ]`, `[ path, user_id ]`, `[ path, username ]`, `[ src, destination ]`, `[ src, created_at ]`, `[ src, user_id ]`, `[ src, username ]`, `[ destination, created_at ]`, `[ destination, user_id ]`, `[ destination, username ]`, `[ created_at, user_id ]`, `[ created_at, username ]`, `[ user_id, username ]`, `[ folder, path, src ]`, `[ folder, path, destination ]`, `[ folder, path, created_at ]`, `[ folder, path, user_id ]`, `[ folder, path, username ]`, `[ folder, src, destination ]`, `[ folder, src, created_at ]`, `[ folder, src, user_id ]`, `[ folder, src, username ]`, `[ folder, destination, created_at ]`, `[ folder, destination, user_id ]`, `[ folder, destination, username ]`, `[ folder, created_at, user_id ]`, `[ folder, created_at, username ]`, `[ folder, user_id, username ]`, `[ path, src, destination ]`, `[ path, src, created_at ]`, `[ path, src, user_id ]`, `[ path, src, username ]`, `[ path, destination, created_at ]`, `[ path, destination, user_id ]`, `[ path, destination, username ]`, `[ path, created_at, user_id ]`, `[ path, created_at, username ]`, `[ path, user_id, username ]`, `[ src, destination, created_at ]`, `[ src, destination, user_id ]`, `[ src, destination, username ]`, `[ src, created_at, user_id ]`, `[ src, created_at, username ]`, `[ src, user_id, username ]`, `[ destination, created_at, user_id ]`, `[ destination, created_at, username ]`, `[ destination, user_id, username ]`, `[ created_at, user_id, username ]`, `[ folder, path, src, destination ]`, `[ folder, path, src, created_at ]`, `[ folder, path, src, user_id ]`, `[ folder, path, src, username ]`, `[ folder, path, destination, created_at ]`, `[ folder, path, destination, user_id ]`, `[ folder, path, destination, username ]`, `[ folder, path, created_at, user_id ]`, `[ folder, path, created_at, username ]`, `[ folder, path, user_id, username ]`, `[ folder, src, destination, created_at ]`, `[ folder, src, destination, user_id ]`, `[ folder, src, destination, username ]`, `[ folder, src, created_at, user_id ]`, `[ folder, src, created_at, username ]`, `[ folder, src, user_id, username ]`, `[ folder, destination, created_at, user_id ]`, `[ folder, destination, created_at, username ]`, `[ folder, destination, user_id, username ]`, `[ folder, created_at, user_id, username ]`, `[ path, src, destination, created_at ]`, `[ path, src, destination, user_id ]`, `[ path, src, destination, username ]`, `[ path, src, created_at, user_id ]`, `[ path, src, created_at, username ]`, `[ path, src, user_id, username ]`, `[ path, destination, created_at, user_id ]`, `[ path, destination, created_at, username ]`, `[ path, destination, user_id, username ]`, `[ path, created_at, user_id, username ]`, `[ src, destination, created_at, user_id ]`, `[ src, destination, created_at, username ]`, `[ src, destination, user_id, username ]`, `[ src, created_at, user_id, username ]`, `[ destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at ]`, `[ folder, path, src, destination, user_id ]`, `[ folder, path, src, destination, username ]`, `[ folder, path, src, created_at, user_id ]`, `[ folder, path, src, created_at, username ]`, `[ folder, path, src, user_id, username ]`, `[ folder, path, destination, created_at, user_id ]`, `[ folder, path, destination, created_at, username ]`, `[ folder, path, destination, user_id, username ]`, `[ folder, path, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id ]`, `[ folder, src, destination, created_at, username ]`, `[ folder, src, destination, user_id, username ]`, `[ folder, src, created_at, user_id, username ]`, `[ folder, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id ]`, `[ path, src, destination, created_at, username ]`, `[ path, src, destination, user_id, username ]`, `[ path, src, created_at, user_id, username ]`, `[ path, destination, created_at, user_id, username ]`, `[ src, destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at, user_id ]`, `[ folder, path, src, destination, created_at, username ]`, `[ folder, path, src, destination, user_id, username ]`, `[ folder, path, src, created_at, user_id, username ]`, `[ folder, path, destination, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id, username ]` or `[ folder, path, src, destination, created_at, user_id, username ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

```yaml
Type: System.Collections.IDictionary
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: List
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Page
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -FilterPrefix

If set, return records where the specified field is prefixed by the supplied value. Valid fields are `folder`, `path`, `src`, `destination` or `username`. Valid field combinations are `[ folder ]`, `[ path ]`, `[ src ]`, `[ destination ]`, `[ created_at ]`, `[ user_id ]`, `[ username ]`, `[ folder, path ]`, `[ folder, src ]`, `[ folder, destination ]`, `[ folder, created_at ]`, `[ folder, user_id ]`, `[ folder, username ]`, `[ path, src ]`, `[ path, destination ]`, `[ path, created_at ]`, `[ path, user_id ]`, `[ path, username ]`, `[ src, destination ]`, `[ src, created_at ]`, `[ src, user_id ]`, `[ src, username ]`, `[ destination, created_at ]`, `[ destination, user_id ]`, `[ destination, username ]`, `[ created_at, user_id ]`, `[ created_at, username ]`, `[ user_id, username ]`, `[ folder, path, src ]`, `[ folder, path, destination ]`, `[ folder, path, created_at ]`, `[ folder, path, user_id ]`, `[ folder, path, username ]`, `[ folder, src, destination ]`, `[ folder, src, created_at ]`, `[ folder, src, user_id ]`, `[ folder, src, username ]`, `[ folder, destination, created_at ]`, `[ folder, destination, user_id ]`, `[ folder, destination, username ]`, `[ folder, created_at, user_id ]`, `[ folder, created_at, username ]`, `[ folder, user_id, username ]`, `[ path, src, destination ]`, `[ path, src, created_at ]`, `[ path, src, user_id ]`, `[ path, src, username ]`, `[ path, destination, created_at ]`, `[ path, destination, user_id ]`, `[ path, destination, username ]`, `[ path, created_at, user_id ]`, `[ path, created_at, username ]`, `[ path, user_id, username ]`, `[ src, destination, created_at ]`, `[ src, destination, user_id ]`, `[ src, destination, username ]`, `[ src, created_at, user_id ]`, `[ src, created_at, username ]`, `[ src, user_id, username ]`, `[ destination, created_at, user_id ]`, `[ destination, created_at, username ]`, `[ destination, user_id, username ]`, `[ created_at, user_id, username ]`, `[ folder, path, src, destination ]`, `[ folder, path, src, created_at ]`, `[ folder, path, src, user_id ]`, `[ folder, path, src, username ]`, `[ folder, path, destination, created_at ]`, `[ folder, path, destination, user_id ]`, `[ folder, path, destination, username ]`, `[ folder, path, created_at, user_id ]`, `[ folder, path, created_at, username ]`, `[ folder, path, user_id, username ]`, `[ folder, src, destination, created_at ]`, `[ folder, src, destination, user_id ]`, `[ folder, src, destination, username ]`, `[ folder, src, created_at, user_id ]`, `[ folder, src, created_at, username ]`, `[ folder, src, user_id, username ]`, `[ folder, destination, created_at, user_id ]`, `[ folder, destination, created_at, username ]`, `[ folder, destination, user_id, username ]`, `[ folder, created_at, user_id, username ]`, `[ path, src, destination, created_at ]`, `[ path, src, destination, user_id ]`, `[ path, src, destination, username ]`, `[ path, src, created_at, user_id ]`, `[ path, src, created_at, username ]`, `[ path, src, user_id, username ]`, `[ path, destination, created_at, user_id ]`, `[ path, destination, created_at, username ]`, `[ path, destination, user_id, username ]`, `[ path, created_at, user_id, username ]`, `[ src, destination, created_at, user_id ]`, `[ src, destination, created_at, username ]`, `[ src, destination, user_id, username ]`, `[ src, created_at, user_id, username ]`, `[ destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at ]`, `[ folder, path, src, destination, user_id ]`, `[ folder, path, src, destination, username ]`, `[ folder, path, src, created_at, user_id ]`, `[ folder, path, src, created_at, username ]`, `[ folder, path, src, user_id, username ]`, `[ folder, path, destination, created_at, user_id ]`, `[ folder, path, destination, created_at, username ]`, `[ folder, path, destination, user_id, username ]`, `[ folder, path, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id ]`, `[ folder, src, destination, created_at, username ]`, `[ folder, src, destination, user_id, username ]`, `[ folder, src, created_at, user_id, username ]`, `[ folder, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id ]`, `[ path, src, destination, created_at, username ]`, `[ path, src, destination, user_id, username ]`, `[ path, src, created_at, user_id, username ]`, `[ path, destination, created_at, user_id, username ]`, `[ src, destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at, user_id ]`, `[ folder, path, src, destination, created_at, username ]`, `[ folder, path, src, destination, user_id, username ]`, `[ folder, path, src, created_at, user_id, username ]`, `[ folder, path, destination, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id, username ]` or `[ folder, path, src, destination, created_at, user_id, username ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

```yaml
Type: System.Collections.IDictionary
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: List
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Page
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -FilterLt

If set, return records where the specified field is less than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ folder ]`, `[ path ]`, `[ src ]`, `[ destination ]`, `[ created_at ]`, `[ user_id ]`, `[ username ]`, `[ folder, path ]`, `[ folder, src ]`, `[ folder, destination ]`, `[ folder, created_at ]`, `[ folder, user_id ]`, `[ folder, username ]`, `[ path, src ]`, `[ path, destination ]`, `[ path, created_at ]`, `[ path, user_id ]`, `[ path, username ]`, `[ src, destination ]`, `[ src, created_at ]`, `[ src, user_id ]`, `[ src, username ]`, `[ destination, created_at ]`, `[ destination, user_id ]`, `[ destination, username ]`, `[ created_at, user_id ]`, `[ created_at, username ]`, `[ user_id, username ]`, `[ folder, path, src ]`, `[ folder, path, destination ]`, `[ folder, path, created_at ]`, `[ folder, path, user_id ]`, `[ folder, path, username ]`, `[ folder, src, destination ]`, `[ folder, src, created_at ]`, `[ folder, src, user_id ]`, `[ folder, src, username ]`, `[ folder, destination, created_at ]`, `[ folder, destination, user_id ]`, `[ folder, destination, username ]`, `[ folder, created_at, user_id ]`, `[ folder, created_at, username ]`, `[ folder, user_id, username ]`, `[ path, src, destination ]`, `[ path, src, created_at ]`, `[ path, src, user_id ]`, `[ path, src, username ]`, `[ path, destination, created_at ]`, `[ path, destination, user_id ]`, `[ path, destination, username ]`, `[ path, created_at, user_id ]`, `[ path, created_at, username ]`, `[ path, user_id, username ]`, `[ src, destination, created_at ]`, `[ src, destination, user_id ]`, `[ src, destination, username ]`, `[ src, created_at, user_id ]`, `[ src, created_at, username ]`, `[ src, user_id, username ]`, `[ destination, created_at, user_id ]`, `[ destination, created_at, username ]`, `[ destination, user_id, username ]`, `[ created_at, user_id, username ]`, `[ folder, path, src, destination ]`, `[ folder, path, src, created_at ]`, `[ folder, path, src, user_id ]`, `[ folder, path, src, username ]`, `[ folder, path, destination, created_at ]`, `[ folder, path, destination, user_id ]`, `[ folder, path, destination, username ]`, `[ folder, path, created_at, user_id ]`, `[ folder, path, created_at, username ]`, `[ folder, path, user_id, username ]`, `[ folder, src, destination, created_at ]`, `[ folder, src, destination, user_id ]`, `[ folder, src, destination, username ]`, `[ folder, src, created_at, user_id ]`, `[ folder, src, created_at, username ]`, `[ folder, src, user_id, username ]`, `[ folder, destination, created_at, user_id ]`, `[ folder, destination, created_at, username ]`, `[ folder, destination, user_id, username ]`, `[ folder, created_at, user_id, username ]`, `[ path, src, destination, created_at ]`, `[ path, src, destination, user_id ]`, `[ path, src, destination, username ]`, `[ path, src, created_at, user_id ]`, `[ path, src, created_at, username ]`, `[ path, src, user_id, username ]`, `[ path, destination, created_at, user_id ]`, `[ path, destination, created_at, username ]`, `[ path, destination, user_id, username ]`, `[ path, created_at, user_id, username ]`, `[ src, destination, created_at, user_id ]`, `[ src, destination, created_at, username ]`, `[ src, destination, user_id, username ]`, `[ src, created_at, user_id, username ]`, `[ destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at ]`, `[ folder, path, src, destination, user_id ]`, `[ folder, path, src, destination, username ]`, `[ folder, path, src, created_at, user_id ]`, `[ folder, path, src, created_at, username ]`, `[ folder, path, src, user_id, username ]`, `[ folder, path, destination, created_at, user_id ]`, `[ folder, path, destination, created_at, username ]`, `[ folder, path, destination, user_id, username ]`, `[ folder, path, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id ]`, `[ folder, src, destination, created_at, username ]`, `[ folder, src, destination, user_id, username ]`, `[ folder, src, created_at, user_id, username ]`, `[ folder, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id ]`, `[ path, src, destination, created_at, username ]`, `[ path, src, destination, user_id, username ]`, `[ path, src, created_at, user_id, username ]`, `[ path, destination, created_at, user_id, username ]`, `[ src, destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at, user_id ]`, `[ folder, path, src, destination, created_at, username ]`, `[ folder, path, src, destination, user_id, username ]`, `[ folder, path, src, created_at, user_id, username ]`, `[ folder, path, destination, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id, username ]` or `[ folder, path, src, destination, created_at, user_id, username ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

```yaml
Type: System.Collections.IDictionary
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: List
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Page
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -FilterLteq

If set, return records where the specified field is less than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ folder ]`, `[ path ]`, `[ src ]`, `[ destination ]`, `[ created_at ]`, `[ user_id ]`, `[ username ]`, `[ folder, path ]`, `[ folder, src ]`, `[ folder, destination ]`, `[ folder, created_at ]`, `[ folder, user_id ]`, `[ folder, username ]`, `[ path, src ]`, `[ path, destination ]`, `[ path, created_at ]`, `[ path, user_id ]`, `[ path, username ]`, `[ src, destination ]`, `[ src, created_at ]`, `[ src, user_id ]`, `[ src, username ]`, `[ destination, created_at ]`, `[ destination, user_id ]`, `[ destination, username ]`, `[ created_at, user_id ]`, `[ created_at, username ]`, `[ user_id, username ]`, `[ folder, path, src ]`, `[ folder, path, destination ]`, `[ folder, path, created_at ]`, `[ folder, path, user_id ]`, `[ folder, path, username ]`, `[ folder, src, destination ]`, `[ folder, src, created_at ]`, `[ folder, src, user_id ]`, `[ folder, src, username ]`, `[ folder, destination, created_at ]`, `[ folder, destination, user_id ]`, `[ folder, destination, username ]`, `[ folder, created_at, user_id ]`, `[ folder, created_at, username ]`, `[ folder, user_id, username ]`, `[ path, src, destination ]`, `[ path, src, created_at ]`, `[ path, src, user_id ]`, `[ path, src, username ]`, `[ path, destination, created_at ]`, `[ path, destination, user_id ]`, `[ path, destination, username ]`, `[ path, created_at, user_id ]`, `[ path, created_at, username ]`, `[ path, user_id, username ]`, `[ src, destination, created_at ]`, `[ src, destination, user_id ]`, `[ src, destination, username ]`, `[ src, created_at, user_id ]`, `[ src, created_at, username ]`, `[ src, user_id, username ]`, `[ destination, created_at, user_id ]`, `[ destination, created_at, username ]`, `[ destination, user_id, username ]`, `[ created_at, user_id, username ]`, `[ folder, path, src, destination ]`, `[ folder, path, src, created_at ]`, `[ folder, path, src, user_id ]`, `[ folder, path, src, username ]`, `[ folder, path, destination, created_at ]`, `[ folder, path, destination, user_id ]`, `[ folder, path, destination, username ]`, `[ folder, path, created_at, user_id ]`, `[ folder, path, created_at, username ]`, `[ folder, path, user_id, username ]`, `[ folder, src, destination, created_at ]`, `[ folder, src, destination, user_id ]`, `[ folder, src, destination, username ]`, `[ folder, src, created_at, user_id ]`, `[ folder, src, created_at, username ]`, `[ folder, src, user_id, username ]`, `[ folder, destination, created_at, user_id ]`, `[ folder, destination, created_at, username ]`, `[ folder, destination, user_id, username ]`, `[ folder, created_at, user_id, username ]`, `[ path, src, destination, created_at ]`, `[ path, src, destination, user_id ]`, `[ path, src, destination, username ]`, `[ path, src, created_at, user_id ]`, `[ path, src, created_at, username ]`, `[ path, src, user_id, username ]`, `[ path, destination, created_at, user_id ]`, `[ path, destination, created_at, username ]`, `[ path, destination, user_id, username ]`, `[ path, created_at, user_id, username ]`, `[ src, destination, created_at, user_id ]`, `[ src, destination, created_at, username ]`, `[ src, destination, user_id, username ]`, `[ src, created_at, user_id, username ]`, `[ destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at ]`, `[ folder, path, src, destination, user_id ]`, `[ folder, path, src, destination, username ]`, `[ folder, path, src, created_at, user_id ]`, `[ folder, path, src, created_at, username ]`, `[ folder, path, src, user_id, username ]`, `[ folder, path, destination, created_at, user_id ]`, `[ folder, path, destination, created_at, username ]`, `[ folder, path, destination, user_id, username ]`, `[ folder, path, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id ]`, `[ folder, src, destination, created_at, username ]`, `[ folder, src, destination, user_id, username ]`, `[ folder, src, created_at, user_id, username ]`, `[ folder, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id ]`, `[ path, src, destination, created_at, username ]`, `[ path, src, destination, user_id, username ]`, `[ path, src, created_at, user_id, username ]`, `[ path, destination, created_at, user_id, username ]`, `[ src, destination, created_at, user_id, username ]`, `[ folder, path, src, destination, created_at, user_id ]`, `[ folder, path, src, destination, created_at, username ]`, `[ folder, path, src, destination, user_id, username ]`, `[ folder, path, src, created_at, user_id, username ]`, `[ folder, path, destination, created_at, user_id, username ]`, `[ folder, src, destination, created_at, user_id, username ]`, `[ path, src, destination, created_at, user_id, username ]` or `[ folder, path, src, destination, created_at, user_id, username ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

```yaml
Type: System.Collections.IDictionary
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: List
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Page
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

### -Page

Writes the first page as one FilesPage object instead of writing every object. Pass its Continuation to -Continuation for the next page.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Page
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Continuation

Writes the page after the one this continuation came from, as one FilesPage object. A continuation loads its page once and belongs to the command, query and connection that produced it.

```yaml
Type: FilesCom.PowerShell.FilesContinuation
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Continuation
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -First

Writes at most this many objects. Once they are written, no further page is loaded.

```yaml
Type: System.UInt64
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

### -Skip

Skips this many objects before writing any. The skipped objects are still loaded.

```yaml
Type: System.UInt64
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

### -IncludeTotalCount

Writes a total count first. Files.com does not report list totals, so the count is marked as unknown.

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

The command takes no pipeline input.

## OUTPUTS

### FilesCom.PowerShell.Models.ActionLog

The action log as Files.com returned it.

### FilesCom.PowerShell.FilesPage`1

With -Page or -Continuation, one page of results and the continuation for the next.

## NOTES

Failures are written as error records with IDs such as Files.NotFound or Files.Authentication. See about_FilesCom for connections, paging, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
