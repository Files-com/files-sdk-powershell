---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Get-FilesWebDavActionLog
---

# Get-FilesWebDavActionLog

## SYNOPSIS

List WebDAV Action Logs.

## SYNTAX

### List (Default)

```
Get-FilesWebDavActionLog [<parameters of the List set>]
```

### Page

```
Get-FilesWebDavActionLog [<parameters of the Page set>]
```

### Continuation

```
Get-FilesWebDavActionLog [<parameters of the Continuation set>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

List WebDAV Action Logs.

Without -Page or -Continuation, the command writes each web dav action log of every page, loading one page at a time as the pipeline consumes them. -First and -Skip bound what it writes, and stopping the pipeline early, as Select-Object -First does, loads no further page. A failure on any page stops the command, so a partial listing is never taken for a complete one.

## EXAMPLES

### Example 1: List all web dav action logs

```powershell
Get-FilesWebDavActionLog
```

Writes every web dav action log, loading one page at a time as the pipeline consumes them.

### Example 2: Get one page at a time

```powershell
$page = Get-FilesWebDavActionLog -Page
$next = Get-FilesWebDavActionLog -Continuation $page.Continuation
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

If set, return records where the specified field is equal to the supplied value. Valid fields are `path`, `true_path`, `remote_ip`, `success`, `status`, `action_type`, `username` or `created_at`. Valid field combinations are `[ path ]`, `[ true_path ]`, `[ remote_ip ]`, `[ success ]`, `[ status ]`, `[ action_type ]`, `[ username ]`, `[ created_at ]`, `[ path, true_path ]`, `[ path, remote_ip ]`, `[ path, success ]`, `[ path, status ]`, `[ path, action_type ]`, `[ path, username ]`, `[ path, created_at ]`, `[ true_path, remote_ip ]`, `[ true_path, success ]`, `[ true_path, status ]`, `[ true_path, action_type ]`, `[ true_path, username ]`, `[ true_path, created_at ]`, `[ remote_ip, success ]`, `[ remote_ip, status ]`, `[ remote_ip, action_type ]`, `[ remote_ip, username ]`, `[ remote_ip, created_at ]`, `[ success, status ]`, `[ success, action_type ]`, `[ success, username ]`, `[ success, created_at ]`, `[ status, action_type ]`, `[ status, username ]`, `[ status, created_at ]`, `[ action_type, username ]`, `[ action_type, created_at ]`, `[ username, created_at ]`, `[ path, true_path, remote_ip ]`, `[ path, true_path, success ]`, `[ path, true_path, status ]`, `[ path, true_path, action_type ]`, `[ path, true_path, username ]`, `[ path, true_path, created_at ]`, `[ path, remote_ip, success ]`, `[ path, remote_ip, status ]`, `[ path, remote_ip, action_type ]`, `[ path, remote_ip, username ]`, `[ path, remote_ip, created_at ]`, `[ path, success, status ]`, `[ path, success, action_type ]`, `[ path, success, username ]`, `[ path, success, created_at ]`, `[ path, status, action_type ]`, `[ path, status, username ]`, `[ path, status, created_at ]`, `[ path, action_type, username ]`, `[ path, action_type, created_at ]`, `[ path, username, created_at ]`, `[ true_path, remote_ip, success ]`, `[ true_path, remote_ip, status ]`, `[ true_path, remote_ip, action_type ]`, `[ true_path, remote_ip, username ]`, `[ true_path, remote_ip, created_at ]`, `[ true_path, success, status ]`, `[ true_path, success, action_type ]`, `[ true_path, success, username ]`, `[ true_path, success, created_at ]`, `[ true_path, status, action_type ]`, `[ true_path, status, username ]`, `[ true_path, status, created_at ]`, `[ true_path, action_type, username ]`, `[ true_path, action_type, created_at ]`, `[ true_path, username, created_at ]`, `[ remote_ip, success, status ]`, `[ remote_ip, success, action_type ]`, `[ remote_ip, success, username ]`, `[ remote_ip, success, created_at ]`, `[ remote_ip, status, action_type ]`, `[ remote_ip, status, username ]`, `[ remote_ip, status, created_at ]`, `[ remote_ip, action_type, username ]`, `[ remote_ip, action_type, created_at ]`, `[ remote_ip, username, created_at ]`, `[ success, status, action_type ]`, `[ success, status, username ]`, `[ success, status, created_at ]`, `[ success, action_type, username ]`, `[ success, action_type, created_at ]`, `[ success, username, created_at ]`, `[ status, action_type, username ]`, `[ status, action_type, created_at ]`, `[ status, username, created_at ]`, `[ action_type, username, created_at ]`, `[ path, true_path, remote_ip, success ]`, `[ path, true_path, remote_ip, status ]`, `[ path, true_path, remote_ip, action_type ]`, `[ path, true_path, remote_ip, username ]`, `[ path, true_path, remote_ip, created_at ]`, `[ path, true_path, success, status ]`, `[ path, true_path, success, action_type ]`, `[ path, true_path, success, username ]`, `[ path, true_path, success, created_at ]`, `[ path, true_path, status, action_type ]`, `[ path, true_path, status, username ]`, `[ path, true_path, status, created_at ]`, `[ path, true_path, action_type, username ]`, `[ path, true_path, action_type, created_at ]`, `[ path, true_path, username, created_at ]`, `[ path, remote_ip, success, status ]`, `[ path, remote_ip, success, action_type ]`, `[ path, remote_ip, success, username ]`, `[ path, remote_ip, success, created_at ]`, `[ path, remote_ip, status, action_type ]`, `[ path, remote_ip, status, username ]`, `[ path, remote_ip, status, created_at ]`, `[ path, remote_ip, action_type, username ]`, `[ path, remote_ip, action_type, created_at ]`, `[ path, remote_ip, username, created_at ]`, `[ path, success, status, action_type ]`, `[ path, success, status, username ]`, `[ path, success, status, created_at ]`, `[ path, success, action_type, username ]`, `[ path, success, action_type, created_at ]`, `[ path, success, username, created_at ]`, `[ path, status, action_type, username ]`, `[ path, status, action_type, created_at ]`, `[ path, status, username, created_at ]`, `[ path, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status ]`, `[ true_path, remote_ip, success, action_type ]`, `[ true_path, remote_ip, success, username ]`, `[ true_path, remote_ip, success, created_at ]`, `[ true_path, remote_ip, status, action_type ]`, `[ true_path, remote_ip, status, username ]`, `[ true_path, remote_ip, status, created_at ]`, `[ true_path, remote_ip, action_type, username ]`, `[ true_path, remote_ip, action_type, created_at ]`, `[ true_path, remote_ip, username, created_at ]`, `[ true_path, success, status, action_type ]`, `[ true_path, success, status, username ]`, `[ true_path, success, status, created_at ]`, `[ true_path, success, action_type, username ]`, `[ true_path, success, action_type, created_at ]`, `[ true_path, success, username, created_at ]`, `[ true_path, status, action_type, username ]`, `[ true_path, status, action_type, created_at ]`, `[ true_path, status, username, created_at ]`, `[ true_path, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type ]`, `[ remote_ip, success, status, username ]`, `[ remote_ip, success, status, created_at ]`, `[ remote_ip, success, action_type, username ]`, `[ remote_ip, success, action_type, created_at ]`, `[ remote_ip, success, username, created_at ]`, `[ remote_ip, status, action_type, username ]`, `[ remote_ip, status, action_type, created_at ]`, `[ remote_ip, status, username, created_at ]`, `[ remote_ip, action_type, username, created_at ]`, `[ success, status, action_type, username ]`, `[ success, status, action_type, created_at ]`, `[ success, status, username, created_at ]`, `[ success, action_type, username, created_at ]`, `[ status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status ]`, `[ path, true_path, remote_ip, success, action_type ]`, `[ path, true_path, remote_ip, success, username ]`, `[ path, true_path, remote_ip, success, created_at ]`, `[ path, true_path, remote_ip, status, action_type ]`, `[ path, true_path, remote_ip, status, username ]`, `[ path, true_path, remote_ip, status, created_at ]`, `[ path, true_path, remote_ip, action_type, username ]`, `[ path, true_path, remote_ip, action_type, created_at ]`, `[ path, true_path, remote_ip, username, created_at ]`, `[ path, true_path, success, status, action_type ]`, `[ path, true_path, success, status, username ]`, `[ path, true_path, success, status, created_at ]`, `[ path, true_path, success, action_type, username ]`, `[ path, true_path, success, action_type, created_at ]`, `[ path, true_path, success, username, created_at ]`, `[ path, true_path, status, action_type, username ]`, `[ path, true_path, status, action_type, created_at ]`, `[ path, true_path, status, username, created_at ]`, `[ path, true_path, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type ]`, `[ path, remote_ip, success, status, username ]`, `[ path, remote_ip, success, status, created_at ]`, `[ path, remote_ip, success, action_type, username ]`, `[ path, remote_ip, success, action_type, created_at ]`, `[ path, remote_ip, success, username, created_at ]`, `[ path, remote_ip, status, action_type, username ]`, `[ path, remote_ip, status, action_type, created_at ]`, `[ path, remote_ip, status, username, created_at ]`, `[ path, remote_ip, action_type, username, created_at ]`, `[ path, success, status, action_type, username ]`, `[ path, success, status, action_type, created_at ]`, `[ path, success, status, username, created_at ]`, `[ path, success, action_type, username, created_at ]`, `[ path, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type ]`, `[ true_path, remote_ip, success, status, username ]`, `[ true_path, remote_ip, success, status, created_at ]`, `[ true_path, remote_ip, success, action_type, username ]`, `[ true_path, remote_ip, success, action_type, created_at ]`, `[ true_path, remote_ip, success, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username ]`, `[ true_path, remote_ip, status, action_type, created_at ]`, `[ true_path, remote_ip, status, username, created_at ]`, `[ true_path, remote_ip, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username ]`, `[ true_path, success, status, action_type, created_at ]`, `[ true_path, success, status, username, created_at ]`, `[ true_path, success, action_type, username, created_at ]`, `[ true_path, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username ]`, `[ remote_ip, success, status, action_type, created_at ]`, `[ remote_ip, success, status, username, created_at ]`, `[ remote_ip, success, action_type, username, created_at ]`, `[ remote_ip, status, action_type, username, created_at ]`, `[ success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type ]`, `[ path, true_path, remote_ip, success, status, username ]`, `[ path, true_path, remote_ip, success, status, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username ]`, `[ path, true_path, remote_ip, success, action_type, created_at ]`, `[ path, true_path, remote_ip, success, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username ]`, `[ path, true_path, remote_ip, status, action_type, created_at ]`, `[ path, true_path, remote_ip, status, username, created_at ]`, `[ path, true_path, remote_ip, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username ]`, `[ path, true_path, success, status, action_type, created_at ]`, `[ path, true_path, success, status, username, created_at ]`, `[ path, true_path, success, action_type, username, created_at ]`, `[ path, true_path, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username ]`, `[ path, remote_ip, success, status, action_type, created_at ]`, `[ path, remote_ip, success, status, username, created_at ]`, `[ path, remote_ip, success, action_type, username, created_at ]`, `[ path, remote_ip, status, action_type, username, created_at ]`, `[ path, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username ]`, `[ true_path, remote_ip, success, status, action_type, created_at ]`, `[ true_path, remote_ip, success, status, username, created_at ]`, `[ true_path, remote_ip, success, action_type, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type, username ]`, `[ path, true_path, remote_ip, success, status, action_type, created_at ]`, `[ path, true_path, remote_ip, success, status, username, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username, created_at ]` or `[ path, true_path, remote_ip, success, status, action_type, username, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is greater than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ path ]`, `[ true_path ]`, `[ remote_ip ]`, `[ success ]`, `[ status ]`, `[ action_type ]`, `[ username ]`, `[ created_at ]`, `[ path, true_path ]`, `[ path, remote_ip ]`, `[ path, success ]`, `[ path, status ]`, `[ path, action_type ]`, `[ path, username ]`, `[ path, created_at ]`, `[ true_path, remote_ip ]`, `[ true_path, success ]`, `[ true_path, status ]`, `[ true_path, action_type ]`, `[ true_path, username ]`, `[ true_path, created_at ]`, `[ remote_ip, success ]`, `[ remote_ip, status ]`, `[ remote_ip, action_type ]`, `[ remote_ip, username ]`, `[ remote_ip, created_at ]`, `[ success, status ]`, `[ success, action_type ]`, `[ success, username ]`, `[ success, created_at ]`, `[ status, action_type ]`, `[ status, username ]`, `[ status, created_at ]`, `[ action_type, username ]`, `[ action_type, created_at ]`, `[ username, created_at ]`, `[ path, true_path, remote_ip ]`, `[ path, true_path, success ]`, `[ path, true_path, status ]`, `[ path, true_path, action_type ]`, `[ path, true_path, username ]`, `[ path, true_path, created_at ]`, `[ path, remote_ip, success ]`, `[ path, remote_ip, status ]`, `[ path, remote_ip, action_type ]`, `[ path, remote_ip, username ]`, `[ path, remote_ip, created_at ]`, `[ path, success, status ]`, `[ path, success, action_type ]`, `[ path, success, username ]`, `[ path, success, created_at ]`, `[ path, status, action_type ]`, `[ path, status, username ]`, `[ path, status, created_at ]`, `[ path, action_type, username ]`, `[ path, action_type, created_at ]`, `[ path, username, created_at ]`, `[ true_path, remote_ip, success ]`, `[ true_path, remote_ip, status ]`, `[ true_path, remote_ip, action_type ]`, `[ true_path, remote_ip, username ]`, `[ true_path, remote_ip, created_at ]`, `[ true_path, success, status ]`, `[ true_path, success, action_type ]`, `[ true_path, success, username ]`, `[ true_path, success, created_at ]`, `[ true_path, status, action_type ]`, `[ true_path, status, username ]`, `[ true_path, status, created_at ]`, `[ true_path, action_type, username ]`, `[ true_path, action_type, created_at ]`, `[ true_path, username, created_at ]`, `[ remote_ip, success, status ]`, `[ remote_ip, success, action_type ]`, `[ remote_ip, success, username ]`, `[ remote_ip, success, created_at ]`, `[ remote_ip, status, action_type ]`, `[ remote_ip, status, username ]`, `[ remote_ip, status, created_at ]`, `[ remote_ip, action_type, username ]`, `[ remote_ip, action_type, created_at ]`, `[ remote_ip, username, created_at ]`, `[ success, status, action_type ]`, `[ success, status, username ]`, `[ success, status, created_at ]`, `[ success, action_type, username ]`, `[ success, action_type, created_at ]`, `[ success, username, created_at ]`, `[ status, action_type, username ]`, `[ status, action_type, created_at ]`, `[ status, username, created_at ]`, `[ action_type, username, created_at ]`, `[ path, true_path, remote_ip, success ]`, `[ path, true_path, remote_ip, status ]`, `[ path, true_path, remote_ip, action_type ]`, `[ path, true_path, remote_ip, username ]`, `[ path, true_path, remote_ip, created_at ]`, `[ path, true_path, success, status ]`, `[ path, true_path, success, action_type ]`, `[ path, true_path, success, username ]`, `[ path, true_path, success, created_at ]`, `[ path, true_path, status, action_type ]`, `[ path, true_path, status, username ]`, `[ path, true_path, status, created_at ]`, `[ path, true_path, action_type, username ]`, `[ path, true_path, action_type, created_at ]`, `[ path, true_path, username, created_at ]`, `[ path, remote_ip, success, status ]`, `[ path, remote_ip, success, action_type ]`, `[ path, remote_ip, success, username ]`, `[ path, remote_ip, success, created_at ]`, `[ path, remote_ip, status, action_type ]`, `[ path, remote_ip, status, username ]`, `[ path, remote_ip, status, created_at ]`, `[ path, remote_ip, action_type, username ]`, `[ path, remote_ip, action_type, created_at ]`, `[ path, remote_ip, username, created_at ]`, `[ path, success, status, action_type ]`, `[ path, success, status, username ]`, `[ path, success, status, created_at ]`, `[ path, success, action_type, username ]`, `[ path, success, action_type, created_at ]`, `[ path, success, username, created_at ]`, `[ path, status, action_type, username ]`, `[ path, status, action_type, created_at ]`, `[ path, status, username, created_at ]`, `[ path, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status ]`, `[ true_path, remote_ip, success, action_type ]`, `[ true_path, remote_ip, success, username ]`, `[ true_path, remote_ip, success, created_at ]`, `[ true_path, remote_ip, status, action_type ]`, `[ true_path, remote_ip, status, username ]`, `[ true_path, remote_ip, status, created_at ]`, `[ true_path, remote_ip, action_type, username ]`, `[ true_path, remote_ip, action_type, created_at ]`, `[ true_path, remote_ip, username, created_at ]`, `[ true_path, success, status, action_type ]`, `[ true_path, success, status, username ]`, `[ true_path, success, status, created_at ]`, `[ true_path, success, action_type, username ]`, `[ true_path, success, action_type, created_at ]`, `[ true_path, success, username, created_at ]`, `[ true_path, status, action_type, username ]`, `[ true_path, status, action_type, created_at ]`, `[ true_path, status, username, created_at ]`, `[ true_path, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type ]`, `[ remote_ip, success, status, username ]`, `[ remote_ip, success, status, created_at ]`, `[ remote_ip, success, action_type, username ]`, `[ remote_ip, success, action_type, created_at ]`, `[ remote_ip, success, username, created_at ]`, `[ remote_ip, status, action_type, username ]`, `[ remote_ip, status, action_type, created_at ]`, `[ remote_ip, status, username, created_at ]`, `[ remote_ip, action_type, username, created_at ]`, `[ success, status, action_type, username ]`, `[ success, status, action_type, created_at ]`, `[ success, status, username, created_at ]`, `[ success, action_type, username, created_at ]`, `[ status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status ]`, `[ path, true_path, remote_ip, success, action_type ]`, `[ path, true_path, remote_ip, success, username ]`, `[ path, true_path, remote_ip, success, created_at ]`, `[ path, true_path, remote_ip, status, action_type ]`, `[ path, true_path, remote_ip, status, username ]`, `[ path, true_path, remote_ip, status, created_at ]`, `[ path, true_path, remote_ip, action_type, username ]`, `[ path, true_path, remote_ip, action_type, created_at ]`, `[ path, true_path, remote_ip, username, created_at ]`, `[ path, true_path, success, status, action_type ]`, `[ path, true_path, success, status, username ]`, `[ path, true_path, success, status, created_at ]`, `[ path, true_path, success, action_type, username ]`, `[ path, true_path, success, action_type, created_at ]`, `[ path, true_path, success, username, created_at ]`, `[ path, true_path, status, action_type, username ]`, `[ path, true_path, status, action_type, created_at ]`, `[ path, true_path, status, username, created_at ]`, `[ path, true_path, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type ]`, `[ path, remote_ip, success, status, username ]`, `[ path, remote_ip, success, status, created_at ]`, `[ path, remote_ip, success, action_type, username ]`, `[ path, remote_ip, success, action_type, created_at ]`, `[ path, remote_ip, success, username, created_at ]`, `[ path, remote_ip, status, action_type, username ]`, `[ path, remote_ip, status, action_type, created_at ]`, `[ path, remote_ip, status, username, created_at ]`, `[ path, remote_ip, action_type, username, created_at ]`, `[ path, success, status, action_type, username ]`, `[ path, success, status, action_type, created_at ]`, `[ path, success, status, username, created_at ]`, `[ path, success, action_type, username, created_at ]`, `[ path, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type ]`, `[ true_path, remote_ip, success, status, username ]`, `[ true_path, remote_ip, success, status, created_at ]`, `[ true_path, remote_ip, success, action_type, username ]`, `[ true_path, remote_ip, success, action_type, created_at ]`, `[ true_path, remote_ip, success, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username ]`, `[ true_path, remote_ip, status, action_type, created_at ]`, `[ true_path, remote_ip, status, username, created_at ]`, `[ true_path, remote_ip, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username ]`, `[ true_path, success, status, action_type, created_at ]`, `[ true_path, success, status, username, created_at ]`, `[ true_path, success, action_type, username, created_at ]`, `[ true_path, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username ]`, `[ remote_ip, success, status, action_type, created_at ]`, `[ remote_ip, success, status, username, created_at ]`, `[ remote_ip, success, action_type, username, created_at ]`, `[ remote_ip, status, action_type, username, created_at ]`, `[ success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type ]`, `[ path, true_path, remote_ip, success, status, username ]`, `[ path, true_path, remote_ip, success, status, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username ]`, `[ path, true_path, remote_ip, success, action_type, created_at ]`, `[ path, true_path, remote_ip, success, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username ]`, `[ path, true_path, remote_ip, status, action_type, created_at ]`, `[ path, true_path, remote_ip, status, username, created_at ]`, `[ path, true_path, remote_ip, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username ]`, `[ path, true_path, success, status, action_type, created_at ]`, `[ path, true_path, success, status, username, created_at ]`, `[ path, true_path, success, action_type, username, created_at ]`, `[ path, true_path, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username ]`, `[ path, remote_ip, success, status, action_type, created_at ]`, `[ path, remote_ip, success, status, username, created_at ]`, `[ path, remote_ip, success, action_type, username, created_at ]`, `[ path, remote_ip, status, action_type, username, created_at ]`, `[ path, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username ]`, `[ true_path, remote_ip, success, status, action_type, created_at ]`, `[ true_path, remote_ip, success, status, username, created_at ]`, `[ true_path, remote_ip, success, action_type, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type, username ]`, `[ path, true_path, remote_ip, success, status, action_type, created_at ]`, `[ path, true_path, remote_ip, success, status, username, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username, created_at ]` or `[ path, true_path, remote_ip, success, status, action_type, username, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ path ]`, `[ true_path ]`, `[ remote_ip ]`, `[ success ]`, `[ status ]`, `[ action_type ]`, `[ username ]`, `[ created_at ]`, `[ path, true_path ]`, `[ path, remote_ip ]`, `[ path, success ]`, `[ path, status ]`, `[ path, action_type ]`, `[ path, username ]`, `[ path, created_at ]`, `[ true_path, remote_ip ]`, `[ true_path, success ]`, `[ true_path, status ]`, `[ true_path, action_type ]`, `[ true_path, username ]`, `[ true_path, created_at ]`, `[ remote_ip, success ]`, `[ remote_ip, status ]`, `[ remote_ip, action_type ]`, `[ remote_ip, username ]`, `[ remote_ip, created_at ]`, `[ success, status ]`, `[ success, action_type ]`, `[ success, username ]`, `[ success, created_at ]`, `[ status, action_type ]`, `[ status, username ]`, `[ status, created_at ]`, `[ action_type, username ]`, `[ action_type, created_at ]`, `[ username, created_at ]`, `[ path, true_path, remote_ip ]`, `[ path, true_path, success ]`, `[ path, true_path, status ]`, `[ path, true_path, action_type ]`, `[ path, true_path, username ]`, `[ path, true_path, created_at ]`, `[ path, remote_ip, success ]`, `[ path, remote_ip, status ]`, `[ path, remote_ip, action_type ]`, `[ path, remote_ip, username ]`, `[ path, remote_ip, created_at ]`, `[ path, success, status ]`, `[ path, success, action_type ]`, `[ path, success, username ]`, `[ path, success, created_at ]`, `[ path, status, action_type ]`, `[ path, status, username ]`, `[ path, status, created_at ]`, `[ path, action_type, username ]`, `[ path, action_type, created_at ]`, `[ path, username, created_at ]`, `[ true_path, remote_ip, success ]`, `[ true_path, remote_ip, status ]`, `[ true_path, remote_ip, action_type ]`, `[ true_path, remote_ip, username ]`, `[ true_path, remote_ip, created_at ]`, `[ true_path, success, status ]`, `[ true_path, success, action_type ]`, `[ true_path, success, username ]`, `[ true_path, success, created_at ]`, `[ true_path, status, action_type ]`, `[ true_path, status, username ]`, `[ true_path, status, created_at ]`, `[ true_path, action_type, username ]`, `[ true_path, action_type, created_at ]`, `[ true_path, username, created_at ]`, `[ remote_ip, success, status ]`, `[ remote_ip, success, action_type ]`, `[ remote_ip, success, username ]`, `[ remote_ip, success, created_at ]`, `[ remote_ip, status, action_type ]`, `[ remote_ip, status, username ]`, `[ remote_ip, status, created_at ]`, `[ remote_ip, action_type, username ]`, `[ remote_ip, action_type, created_at ]`, `[ remote_ip, username, created_at ]`, `[ success, status, action_type ]`, `[ success, status, username ]`, `[ success, status, created_at ]`, `[ success, action_type, username ]`, `[ success, action_type, created_at ]`, `[ success, username, created_at ]`, `[ status, action_type, username ]`, `[ status, action_type, created_at ]`, `[ status, username, created_at ]`, `[ action_type, username, created_at ]`, `[ path, true_path, remote_ip, success ]`, `[ path, true_path, remote_ip, status ]`, `[ path, true_path, remote_ip, action_type ]`, `[ path, true_path, remote_ip, username ]`, `[ path, true_path, remote_ip, created_at ]`, `[ path, true_path, success, status ]`, `[ path, true_path, success, action_type ]`, `[ path, true_path, success, username ]`, `[ path, true_path, success, created_at ]`, `[ path, true_path, status, action_type ]`, `[ path, true_path, status, username ]`, `[ path, true_path, status, created_at ]`, `[ path, true_path, action_type, username ]`, `[ path, true_path, action_type, created_at ]`, `[ path, true_path, username, created_at ]`, `[ path, remote_ip, success, status ]`, `[ path, remote_ip, success, action_type ]`, `[ path, remote_ip, success, username ]`, `[ path, remote_ip, success, created_at ]`, `[ path, remote_ip, status, action_type ]`, `[ path, remote_ip, status, username ]`, `[ path, remote_ip, status, created_at ]`, `[ path, remote_ip, action_type, username ]`, `[ path, remote_ip, action_type, created_at ]`, `[ path, remote_ip, username, created_at ]`, `[ path, success, status, action_type ]`, `[ path, success, status, username ]`, `[ path, success, status, created_at ]`, `[ path, success, action_type, username ]`, `[ path, success, action_type, created_at ]`, `[ path, success, username, created_at ]`, `[ path, status, action_type, username ]`, `[ path, status, action_type, created_at ]`, `[ path, status, username, created_at ]`, `[ path, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status ]`, `[ true_path, remote_ip, success, action_type ]`, `[ true_path, remote_ip, success, username ]`, `[ true_path, remote_ip, success, created_at ]`, `[ true_path, remote_ip, status, action_type ]`, `[ true_path, remote_ip, status, username ]`, `[ true_path, remote_ip, status, created_at ]`, `[ true_path, remote_ip, action_type, username ]`, `[ true_path, remote_ip, action_type, created_at ]`, `[ true_path, remote_ip, username, created_at ]`, `[ true_path, success, status, action_type ]`, `[ true_path, success, status, username ]`, `[ true_path, success, status, created_at ]`, `[ true_path, success, action_type, username ]`, `[ true_path, success, action_type, created_at ]`, `[ true_path, success, username, created_at ]`, `[ true_path, status, action_type, username ]`, `[ true_path, status, action_type, created_at ]`, `[ true_path, status, username, created_at ]`, `[ true_path, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type ]`, `[ remote_ip, success, status, username ]`, `[ remote_ip, success, status, created_at ]`, `[ remote_ip, success, action_type, username ]`, `[ remote_ip, success, action_type, created_at ]`, `[ remote_ip, success, username, created_at ]`, `[ remote_ip, status, action_type, username ]`, `[ remote_ip, status, action_type, created_at ]`, `[ remote_ip, status, username, created_at ]`, `[ remote_ip, action_type, username, created_at ]`, `[ success, status, action_type, username ]`, `[ success, status, action_type, created_at ]`, `[ success, status, username, created_at ]`, `[ success, action_type, username, created_at ]`, `[ status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status ]`, `[ path, true_path, remote_ip, success, action_type ]`, `[ path, true_path, remote_ip, success, username ]`, `[ path, true_path, remote_ip, success, created_at ]`, `[ path, true_path, remote_ip, status, action_type ]`, `[ path, true_path, remote_ip, status, username ]`, `[ path, true_path, remote_ip, status, created_at ]`, `[ path, true_path, remote_ip, action_type, username ]`, `[ path, true_path, remote_ip, action_type, created_at ]`, `[ path, true_path, remote_ip, username, created_at ]`, `[ path, true_path, success, status, action_type ]`, `[ path, true_path, success, status, username ]`, `[ path, true_path, success, status, created_at ]`, `[ path, true_path, success, action_type, username ]`, `[ path, true_path, success, action_type, created_at ]`, `[ path, true_path, success, username, created_at ]`, `[ path, true_path, status, action_type, username ]`, `[ path, true_path, status, action_type, created_at ]`, `[ path, true_path, status, username, created_at ]`, `[ path, true_path, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type ]`, `[ path, remote_ip, success, status, username ]`, `[ path, remote_ip, success, status, created_at ]`, `[ path, remote_ip, success, action_type, username ]`, `[ path, remote_ip, success, action_type, created_at ]`, `[ path, remote_ip, success, username, created_at ]`, `[ path, remote_ip, status, action_type, username ]`, `[ path, remote_ip, status, action_type, created_at ]`, `[ path, remote_ip, status, username, created_at ]`, `[ path, remote_ip, action_type, username, created_at ]`, `[ path, success, status, action_type, username ]`, `[ path, success, status, action_type, created_at ]`, `[ path, success, status, username, created_at ]`, `[ path, success, action_type, username, created_at ]`, `[ path, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type ]`, `[ true_path, remote_ip, success, status, username ]`, `[ true_path, remote_ip, success, status, created_at ]`, `[ true_path, remote_ip, success, action_type, username ]`, `[ true_path, remote_ip, success, action_type, created_at ]`, `[ true_path, remote_ip, success, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username ]`, `[ true_path, remote_ip, status, action_type, created_at ]`, `[ true_path, remote_ip, status, username, created_at ]`, `[ true_path, remote_ip, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username ]`, `[ true_path, success, status, action_type, created_at ]`, `[ true_path, success, status, username, created_at ]`, `[ true_path, success, action_type, username, created_at ]`, `[ true_path, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username ]`, `[ remote_ip, success, status, action_type, created_at ]`, `[ remote_ip, success, status, username, created_at ]`, `[ remote_ip, success, action_type, username, created_at ]`, `[ remote_ip, status, action_type, username, created_at ]`, `[ success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type ]`, `[ path, true_path, remote_ip, success, status, username ]`, `[ path, true_path, remote_ip, success, status, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username ]`, `[ path, true_path, remote_ip, success, action_type, created_at ]`, `[ path, true_path, remote_ip, success, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username ]`, `[ path, true_path, remote_ip, status, action_type, created_at ]`, `[ path, true_path, remote_ip, status, username, created_at ]`, `[ path, true_path, remote_ip, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username ]`, `[ path, true_path, success, status, action_type, created_at ]`, `[ path, true_path, success, status, username, created_at ]`, `[ path, true_path, success, action_type, username, created_at ]`, `[ path, true_path, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username ]`, `[ path, remote_ip, success, status, action_type, created_at ]`, `[ path, remote_ip, success, status, username, created_at ]`, `[ path, remote_ip, success, action_type, username, created_at ]`, `[ path, remote_ip, status, action_type, username, created_at ]`, `[ path, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username ]`, `[ true_path, remote_ip, success, status, action_type, created_at ]`, `[ true_path, remote_ip, success, status, username, created_at ]`, `[ true_path, remote_ip, success, action_type, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type, username ]`, `[ path, true_path, remote_ip, success, status, action_type, created_at ]`, `[ path, true_path, remote_ip, success, status, username, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username, created_at ]` or `[ path, true_path, remote_ip, success, status, action_type, username, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is prefixed by the supplied value. Valid fields are `path`, `true_path`, `action_type` or `username`. Valid field combinations are `[ path ]`, `[ true_path ]`, `[ remote_ip ]`, `[ success ]`, `[ status ]`, `[ action_type ]`, `[ username ]`, `[ created_at ]`, `[ path, true_path ]`, `[ path, remote_ip ]`, `[ path, success ]`, `[ path, status ]`, `[ path, action_type ]`, `[ path, username ]`, `[ path, created_at ]`, `[ true_path, remote_ip ]`, `[ true_path, success ]`, `[ true_path, status ]`, `[ true_path, action_type ]`, `[ true_path, username ]`, `[ true_path, created_at ]`, `[ remote_ip, success ]`, `[ remote_ip, status ]`, `[ remote_ip, action_type ]`, `[ remote_ip, username ]`, `[ remote_ip, created_at ]`, `[ success, status ]`, `[ success, action_type ]`, `[ success, username ]`, `[ success, created_at ]`, `[ status, action_type ]`, `[ status, username ]`, `[ status, created_at ]`, `[ action_type, username ]`, `[ action_type, created_at ]`, `[ username, created_at ]`, `[ path, true_path, remote_ip ]`, `[ path, true_path, success ]`, `[ path, true_path, status ]`, `[ path, true_path, action_type ]`, `[ path, true_path, username ]`, `[ path, true_path, created_at ]`, `[ path, remote_ip, success ]`, `[ path, remote_ip, status ]`, `[ path, remote_ip, action_type ]`, `[ path, remote_ip, username ]`, `[ path, remote_ip, created_at ]`, `[ path, success, status ]`, `[ path, success, action_type ]`, `[ path, success, username ]`, `[ path, success, created_at ]`, `[ path, status, action_type ]`, `[ path, status, username ]`, `[ path, status, created_at ]`, `[ path, action_type, username ]`, `[ path, action_type, created_at ]`, `[ path, username, created_at ]`, `[ true_path, remote_ip, success ]`, `[ true_path, remote_ip, status ]`, `[ true_path, remote_ip, action_type ]`, `[ true_path, remote_ip, username ]`, `[ true_path, remote_ip, created_at ]`, `[ true_path, success, status ]`, `[ true_path, success, action_type ]`, `[ true_path, success, username ]`, `[ true_path, success, created_at ]`, `[ true_path, status, action_type ]`, `[ true_path, status, username ]`, `[ true_path, status, created_at ]`, `[ true_path, action_type, username ]`, `[ true_path, action_type, created_at ]`, `[ true_path, username, created_at ]`, `[ remote_ip, success, status ]`, `[ remote_ip, success, action_type ]`, `[ remote_ip, success, username ]`, `[ remote_ip, success, created_at ]`, `[ remote_ip, status, action_type ]`, `[ remote_ip, status, username ]`, `[ remote_ip, status, created_at ]`, `[ remote_ip, action_type, username ]`, `[ remote_ip, action_type, created_at ]`, `[ remote_ip, username, created_at ]`, `[ success, status, action_type ]`, `[ success, status, username ]`, `[ success, status, created_at ]`, `[ success, action_type, username ]`, `[ success, action_type, created_at ]`, `[ success, username, created_at ]`, `[ status, action_type, username ]`, `[ status, action_type, created_at ]`, `[ status, username, created_at ]`, `[ action_type, username, created_at ]`, `[ path, true_path, remote_ip, success ]`, `[ path, true_path, remote_ip, status ]`, `[ path, true_path, remote_ip, action_type ]`, `[ path, true_path, remote_ip, username ]`, `[ path, true_path, remote_ip, created_at ]`, `[ path, true_path, success, status ]`, `[ path, true_path, success, action_type ]`, `[ path, true_path, success, username ]`, `[ path, true_path, success, created_at ]`, `[ path, true_path, status, action_type ]`, `[ path, true_path, status, username ]`, `[ path, true_path, status, created_at ]`, `[ path, true_path, action_type, username ]`, `[ path, true_path, action_type, created_at ]`, `[ path, true_path, username, created_at ]`, `[ path, remote_ip, success, status ]`, `[ path, remote_ip, success, action_type ]`, `[ path, remote_ip, success, username ]`, `[ path, remote_ip, success, created_at ]`, `[ path, remote_ip, status, action_type ]`, `[ path, remote_ip, status, username ]`, `[ path, remote_ip, status, created_at ]`, `[ path, remote_ip, action_type, username ]`, `[ path, remote_ip, action_type, created_at ]`, `[ path, remote_ip, username, created_at ]`, `[ path, success, status, action_type ]`, `[ path, success, status, username ]`, `[ path, success, status, created_at ]`, `[ path, success, action_type, username ]`, `[ path, success, action_type, created_at ]`, `[ path, success, username, created_at ]`, `[ path, status, action_type, username ]`, `[ path, status, action_type, created_at ]`, `[ path, status, username, created_at ]`, `[ path, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status ]`, `[ true_path, remote_ip, success, action_type ]`, `[ true_path, remote_ip, success, username ]`, `[ true_path, remote_ip, success, created_at ]`, `[ true_path, remote_ip, status, action_type ]`, `[ true_path, remote_ip, status, username ]`, `[ true_path, remote_ip, status, created_at ]`, `[ true_path, remote_ip, action_type, username ]`, `[ true_path, remote_ip, action_type, created_at ]`, `[ true_path, remote_ip, username, created_at ]`, `[ true_path, success, status, action_type ]`, `[ true_path, success, status, username ]`, `[ true_path, success, status, created_at ]`, `[ true_path, success, action_type, username ]`, `[ true_path, success, action_type, created_at ]`, `[ true_path, success, username, created_at ]`, `[ true_path, status, action_type, username ]`, `[ true_path, status, action_type, created_at ]`, `[ true_path, status, username, created_at ]`, `[ true_path, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type ]`, `[ remote_ip, success, status, username ]`, `[ remote_ip, success, status, created_at ]`, `[ remote_ip, success, action_type, username ]`, `[ remote_ip, success, action_type, created_at ]`, `[ remote_ip, success, username, created_at ]`, `[ remote_ip, status, action_type, username ]`, `[ remote_ip, status, action_type, created_at ]`, `[ remote_ip, status, username, created_at ]`, `[ remote_ip, action_type, username, created_at ]`, `[ success, status, action_type, username ]`, `[ success, status, action_type, created_at ]`, `[ success, status, username, created_at ]`, `[ success, action_type, username, created_at ]`, `[ status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status ]`, `[ path, true_path, remote_ip, success, action_type ]`, `[ path, true_path, remote_ip, success, username ]`, `[ path, true_path, remote_ip, success, created_at ]`, `[ path, true_path, remote_ip, status, action_type ]`, `[ path, true_path, remote_ip, status, username ]`, `[ path, true_path, remote_ip, status, created_at ]`, `[ path, true_path, remote_ip, action_type, username ]`, `[ path, true_path, remote_ip, action_type, created_at ]`, `[ path, true_path, remote_ip, username, created_at ]`, `[ path, true_path, success, status, action_type ]`, `[ path, true_path, success, status, username ]`, `[ path, true_path, success, status, created_at ]`, `[ path, true_path, success, action_type, username ]`, `[ path, true_path, success, action_type, created_at ]`, `[ path, true_path, success, username, created_at ]`, `[ path, true_path, status, action_type, username ]`, `[ path, true_path, status, action_type, created_at ]`, `[ path, true_path, status, username, created_at ]`, `[ path, true_path, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type ]`, `[ path, remote_ip, success, status, username ]`, `[ path, remote_ip, success, status, created_at ]`, `[ path, remote_ip, success, action_type, username ]`, `[ path, remote_ip, success, action_type, created_at ]`, `[ path, remote_ip, success, username, created_at ]`, `[ path, remote_ip, status, action_type, username ]`, `[ path, remote_ip, status, action_type, created_at ]`, `[ path, remote_ip, status, username, created_at ]`, `[ path, remote_ip, action_type, username, created_at ]`, `[ path, success, status, action_type, username ]`, `[ path, success, status, action_type, created_at ]`, `[ path, success, status, username, created_at ]`, `[ path, success, action_type, username, created_at ]`, `[ path, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type ]`, `[ true_path, remote_ip, success, status, username ]`, `[ true_path, remote_ip, success, status, created_at ]`, `[ true_path, remote_ip, success, action_type, username ]`, `[ true_path, remote_ip, success, action_type, created_at ]`, `[ true_path, remote_ip, success, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username ]`, `[ true_path, remote_ip, status, action_type, created_at ]`, `[ true_path, remote_ip, status, username, created_at ]`, `[ true_path, remote_ip, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username ]`, `[ true_path, success, status, action_type, created_at ]`, `[ true_path, success, status, username, created_at ]`, `[ true_path, success, action_type, username, created_at ]`, `[ true_path, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username ]`, `[ remote_ip, success, status, action_type, created_at ]`, `[ remote_ip, success, status, username, created_at ]`, `[ remote_ip, success, action_type, username, created_at ]`, `[ remote_ip, status, action_type, username, created_at ]`, `[ success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type ]`, `[ path, true_path, remote_ip, success, status, username ]`, `[ path, true_path, remote_ip, success, status, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username ]`, `[ path, true_path, remote_ip, success, action_type, created_at ]`, `[ path, true_path, remote_ip, success, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username ]`, `[ path, true_path, remote_ip, status, action_type, created_at ]`, `[ path, true_path, remote_ip, status, username, created_at ]`, `[ path, true_path, remote_ip, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username ]`, `[ path, true_path, success, status, action_type, created_at ]`, `[ path, true_path, success, status, username, created_at ]`, `[ path, true_path, success, action_type, username, created_at ]`, `[ path, true_path, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username ]`, `[ path, remote_ip, success, status, action_type, created_at ]`, `[ path, remote_ip, success, status, username, created_at ]`, `[ path, remote_ip, success, action_type, username, created_at ]`, `[ path, remote_ip, status, action_type, username, created_at ]`, `[ path, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username ]`, `[ true_path, remote_ip, success, status, action_type, created_at ]`, `[ true_path, remote_ip, success, status, username, created_at ]`, `[ true_path, remote_ip, success, action_type, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type, username ]`, `[ path, true_path, remote_ip, success, status, action_type, created_at ]`, `[ path, true_path, remote_ip, success, status, username, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username, created_at ]` or `[ path, true_path, remote_ip, success, status, action_type, username, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is less than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ path ]`, `[ true_path ]`, `[ remote_ip ]`, `[ success ]`, `[ status ]`, `[ action_type ]`, `[ username ]`, `[ created_at ]`, `[ path, true_path ]`, `[ path, remote_ip ]`, `[ path, success ]`, `[ path, status ]`, `[ path, action_type ]`, `[ path, username ]`, `[ path, created_at ]`, `[ true_path, remote_ip ]`, `[ true_path, success ]`, `[ true_path, status ]`, `[ true_path, action_type ]`, `[ true_path, username ]`, `[ true_path, created_at ]`, `[ remote_ip, success ]`, `[ remote_ip, status ]`, `[ remote_ip, action_type ]`, `[ remote_ip, username ]`, `[ remote_ip, created_at ]`, `[ success, status ]`, `[ success, action_type ]`, `[ success, username ]`, `[ success, created_at ]`, `[ status, action_type ]`, `[ status, username ]`, `[ status, created_at ]`, `[ action_type, username ]`, `[ action_type, created_at ]`, `[ username, created_at ]`, `[ path, true_path, remote_ip ]`, `[ path, true_path, success ]`, `[ path, true_path, status ]`, `[ path, true_path, action_type ]`, `[ path, true_path, username ]`, `[ path, true_path, created_at ]`, `[ path, remote_ip, success ]`, `[ path, remote_ip, status ]`, `[ path, remote_ip, action_type ]`, `[ path, remote_ip, username ]`, `[ path, remote_ip, created_at ]`, `[ path, success, status ]`, `[ path, success, action_type ]`, `[ path, success, username ]`, `[ path, success, created_at ]`, `[ path, status, action_type ]`, `[ path, status, username ]`, `[ path, status, created_at ]`, `[ path, action_type, username ]`, `[ path, action_type, created_at ]`, `[ path, username, created_at ]`, `[ true_path, remote_ip, success ]`, `[ true_path, remote_ip, status ]`, `[ true_path, remote_ip, action_type ]`, `[ true_path, remote_ip, username ]`, `[ true_path, remote_ip, created_at ]`, `[ true_path, success, status ]`, `[ true_path, success, action_type ]`, `[ true_path, success, username ]`, `[ true_path, success, created_at ]`, `[ true_path, status, action_type ]`, `[ true_path, status, username ]`, `[ true_path, status, created_at ]`, `[ true_path, action_type, username ]`, `[ true_path, action_type, created_at ]`, `[ true_path, username, created_at ]`, `[ remote_ip, success, status ]`, `[ remote_ip, success, action_type ]`, `[ remote_ip, success, username ]`, `[ remote_ip, success, created_at ]`, `[ remote_ip, status, action_type ]`, `[ remote_ip, status, username ]`, `[ remote_ip, status, created_at ]`, `[ remote_ip, action_type, username ]`, `[ remote_ip, action_type, created_at ]`, `[ remote_ip, username, created_at ]`, `[ success, status, action_type ]`, `[ success, status, username ]`, `[ success, status, created_at ]`, `[ success, action_type, username ]`, `[ success, action_type, created_at ]`, `[ success, username, created_at ]`, `[ status, action_type, username ]`, `[ status, action_type, created_at ]`, `[ status, username, created_at ]`, `[ action_type, username, created_at ]`, `[ path, true_path, remote_ip, success ]`, `[ path, true_path, remote_ip, status ]`, `[ path, true_path, remote_ip, action_type ]`, `[ path, true_path, remote_ip, username ]`, `[ path, true_path, remote_ip, created_at ]`, `[ path, true_path, success, status ]`, `[ path, true_path, success, action_type ]`, `[ path, true_path, success, username ]`, `[ path, true_path, success, created_at ]`, `[ path, true_path, status, action_type ]`, `[ path, true_path, status, username ]`, `[ path, true_path, status, created_at ]`, `[ path, true_path, action_type, username ]`, `[ path, true_path, action_type, created_at ]`, `[ path, true_path, username, created_at ]`, `[ path, remote_ip, success, status ]`, `[ path, remote_ip, success, action_type ]`, `[ path, remote_ip, success, username ]`, `[ path, remote_ip, success, created_at ]`, `[ path, remote_ip, status, action_type ]`, `[ path, remote_ip, status, username ]`, `[ path, remote_ip, status, created_at ]`, `[ path, remote_ip, action_type, username ]`, `[ path, remote_ip, action_type, created_at ]`, `[ path, remote_ip, username, created_at ]`, `[ path, success, status, action_type ]`, `[ path, success, status, username ]`, `[ path, success, status, created_at ]`, `[ path, success, action_type, username ]`, `[ path, success, action_type, created_at ]`, `[ path, success, username, created_at ]`, `[ path, status, action_type, username ]`, `[ path, status, action_type, created_at ]`, `[ path, status, username, created_at ]`, `[ path, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status ]`, `[ true_path, remote_ip, success, action_type ]`, `[ true_path, remote_ip, success, username ]`, `[ true_path, remote_ip, success, created_at ]`, `[ true_path, remote_ip, status, action_type ]`, `[ true_path, remote_ip, status, username ]`, `[ true_path, remote_ip, status, created_at ]`, `[ true_path, remote_ip, action_type, username ]`, `[ true_path, remote_ip, action_type, created_at ]`, `[ true_path, remote_ip, username, created_at ]`, `[ true_path, success, status, action_type ]`, `[ true_path, success, status, username ]`, `[ true_path, success, status, created_at ]`, `[ true_path, success, action_type, username ]`, `[ true_path, success, action_type, created_at ]`, `[ true_path, success, username, created_at ]`, `[ true_path, status, action_type, username ]`, `[ true_path, status, action_type, created_at ]`, `[ true_path, status, username, created_at ]`, `[ true_path, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type ]`, `[ remote_ip, success, status, username ]`, `[ remote_ip, success, status, created_at ]`, `[ remote_ip, success, action_type, username ]`, `[ remote_ip, success, action_type, created_at ]`, `[ remote_ip, success, username, created_at ]`, `[ remote_ip, status, action_type, username ]`, `[ remote_ip, status, action_type, created_at ]`, `[ remote_ip, status, username, created_at ]`, `[ remote_ip, action_type, username, created_at ]`, `[ success, status, action_type, username ]`, `[ success, status, action_type, created_at ]`, `[ success, status, username, created_at ]`, `[ success, action_type, username, created_at ]`, `[ status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status ]`, `[ path, true_path, remote_ip, success, action_type ]`, `[ path, true_path, remote_ip, success, username ]`, `[ path, true_path, remote_ip, success, created_at ]`, `[ path, true_path, remote_ip, status, action_type ]`, `[ path, true_path, remote_ip, status, username ]`, `[ path, true_path, remote_ip, status, created_at ]`, `[ path, true_path, remote_ip, action_type, username ]`, `[ path, true_path, remote_ip, action_type, created_at ]`, `[ path, true_path, remote_ip, username, created_at ]`, `[ path, true_path, success, status, action_type ]`, `[ path, true_path, success, status, username ]`, `[ path, true_path, success, status, created_at ]`, `[ path, true_path, success, action_type, username ]`, `[ path, true_path, success, action_type, created_at ]`, `[ path, true_path, success, username, created_at ]`, `[ path, true_path, status, action_type, username ]`, `[ path, true_path, status, action_type, created_at ]`, `[ path, true_path, status, username, created_at ]`, `[ path, true_path, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type ]`, `[ path, remote_ip, success, status, username ]`, `[ path, remote_ip, success, status, created_at ]`, `[ path, remote_ip, success, action_type, username ]`, `[ path, remote_ip, success, action_type, created_at ]`, `[ path, remote_ip, success, username, created_at ]`, `[ path, remote_ip, status, action_type, username ]`, `[ path, remote_ip, status, action_type, created_at ]`, `[ path, remote_ip, status, username, created_at ]`, `[ path, remote_ip, action_type, username, created_at ]`, `[ path, success, status, action_type, username ]`, `[ path, success, status, action_type, created_at ]`, `[ path, success, status, username, created_at ]`, `[ path, success, action_type, username, created_at ]`, `[ path, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type ]`, `[ true_path, remote_ip, success, status, username ]`, `[ true_path, remote_ip, success, status, created_at ]`, `[ true_path, remote_ip, success, action_type, username ]`, `[ true_path, remote_ip, success, action_type, created_at ]`, `[ true_path, remote_ip, success, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username ]`, `[ true_path, remote_ip, status, action_type, created_at ]`, `[ true_path, remote_ip, status, username, created_at ]`, `[ true_path, remote_ip, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username ]`, `[ true_path, success, status, action_type, created_at ]`, `[ true_path, success, status, username, created_at ]`, `[ true_path, success, action_type, username, created_at ]`, `[ true_path, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username ]`, `[ remote_ip, success, status, action_type, created_at ]`, `[ remote_ip, success, status, username, created_at ]`, `[ remote_ip, success, action_type, username, created_at ]`, `[ remote_ip, status, action_type, username, created_at ]`, `[ success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type ]`, `[ path, true_path, remote_ip, success, status, username ]`, `[ path, true_path, remote_ip, success, status, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username ]`, `[ path, true_path, remote_ip, success, action_type, created_at ]`, `[ path, true_path, remote_ip, success, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username ]`, `[ path, true_path, remote_ip, status, action_type, created_at ]`, `[ path, true_path, remote_ip, status, username, created_at ]`, `[ path, true_path, remote_ip, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username ]`, `[ path, true_path, success, status, action_type, created_at ]`, `[ path, true_path, success, status, username, created_at ]`, `[ path, true_path, success, action_type, username, created_at ]`, `[ path, true_path, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username ]`, `[ path, remote_ip, success, status, action_type, created_at ]`, `[ path, remote_ip, success, status, username, created_at ]`, `[ path, remote_ip, success, action_type, username, created_at ]`, `[ path, remote_ip, status, action_type, username, created_at ]`, `[ path, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username ]`, `[ true_path, remote_ip, success, status, action_type, created_at ]`, `[ true_path, remote_ip, success, status, username, created_at ]`, `[ true_path, remote_ip, success, action_type, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type, username ]`, `[ path, true_path, remote_ip, success, status, action_type, created_at ]`, `[ path, true_path, remote_ip, success, status, username, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username, created_at ]` or `[ path, true_path, remote_ip, success, status, action_type, username, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is less than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ path ]`, `[ true_path ]`, `[ remote_ip ]`, `[ success ]`, `[ status ]`, `[ action_type ]`, `[ username ]`, `[ created_at ]`, `[ path, true_path ]`, `[ path, remote_ip ]`, `[ path, success ]`, `[ path, status ]`, `[ path, action_type ]`, `[ path, username ]`, `[ path, created_at ]`, `[ true_path, remote_ip ]`, `[ true_path, success ]`, `[ true_path, status ]`, `[ true_path, action_type ]`, `[ true_path, username ]`, `[ true_path, created_at ]`, `[ remote_ip, success ]`, `[ remote_ip, status ]`, `[ remote_ip, action_type ]`, `[ remote_ip, username ]`, `[ remote_ip, created_at ]`, `[ success, status ]`, `[ success, action_type ]`, `[ success, username ]`, `[ success, created_at ]`, `[ status, action_type ]`, `[ status, username ]`, `[ status, created_at ]`, `[ action_type, username ]`, `[ action_type, created_at ]`, `[ username, created_at ]`, `[ path, true_path, remote_ip ]`, `[ path, true_path, success ]`, `[ path, true_path, status ]`, `[ path, true_path, action_type ]`, `[ path, true_path, username ]`, `[ path, true_path, created_at ]`, `[ path, remote_ip, success ]`, `[ path, remote_ip, status ]`, `[ path, remote_ip, action_type ]`, `[ path, remote_ip, username ]`, `[ path, remote_ip, created_at ]`, `[ path, success, status ]`, `[ path, success, action_type ]`, `[ path, success, username ]`, `[ path, success, created_at ]`, `[ path, status, action_type ]`, `[ path, status, username ]`, `[ path, status, created_at ]`, `[ path, action_type, username ]`, `[ path, action_type, created_at ]`, `[ path, username, created_at ]`, `[ true_path, remote_ip, success ]`, `[ true_path, remote_ip, status ]`, `[ true_path, remote_ip, action_type ]`, `[ true_path, remote_ip, username ]`, `[ true_path, remote_ip, created_at ]`, `[ true_path, success, status ]`, `[ true_path, success, action_type ]`, `[ true_path, success, username ]`, `[ true_path, success, created_at ]`, `[ true_path, status, action_type ]`, `[ true_path, status, username ]`, `[ true_path, status, created_at ]`, `[ true_path, action_type, username ]`, `[ true_path, action_type, created_at ]`, `[ true_path, username, created_at ]`, `[ remote_ip, success, status ]`, `[ remote_ip, success, action_type ]`, `[ remote_ip, success, username ]`, `[ remote_ip, success, created_at ]`, `[ remote_ip, status, action_type ]`, `[ remote_ip, status, username ]`, `[ remote_ip, status, created_at ]`, `[ remote_ip, action_type, username ]`, `[ remote_ip, action_type, created_at ]`, `[ remote_ip, username, created_at ]`, `[ success, status, action_type ]`, `[ success, status, username ]`, `[ success, status, created_at ]`, `[ success, action_type, username ]`, `[ success, action_type, created_at ]`, `[ success, username, created_at ]`, `[ status, action_type, username ]`, `[ status, action_type, created_at ]`, `[ status, username, created_at ]`, `[ action_type, username, created_at ]`, `[ path, true_path, remote_ip, success ]`, `[ path, true_path, remote_ip, status ]`, `[ path, true_path, remote_ip, action_type ]`, `[ path, true_path, remote_ip, username ]`, `[ path, true_path, remote_ip, created_at ]`, `[ path, true_path, success, status ]`, `[ path, true_path, success, action_type ]`, `[ path, true_path, success, username ]`, `[ path, true_path, success, created_at ]`, `[ path, true_path, status, action_type ]`, `[ path, true_path, status, username ]`, `[ path, true_path, status, created_at ]`, `[ path, true_path, action_type, username ]`, `[ path, true_path, action_type, created_at ]`, `[ path, true_path, username, created_at ]`, `[ path, remote_ip, success, status ]`, `[ path, remote_ip, success, action_type ]`, `[ path, remote_ip, success, username ]`, `[ path, remote_ip, success, created_at ]`, `[ path, remote_ip, status, action_type ]`, `[ path, remote_ip, status, username ]`, `[ path, remote_ip, status, created_at ]`, `[ path, remote_ip, action_type, username ]`, `[ path, remote_ip, action_type, created_at ]`, `[ path, remote_ip, username, created_at ]`, `[ path, success, status, action_type ]`, `[ path, success, status, username ]`, `[ path, success, status, created_at ]`, `[ path, success, action_type, username ]`, `[ path, success, action_type, created_at ]`, `[ path, success, username, created_at ]`, `[ path, status, action_type, username ]`, `[ path, status, action_type, created_at ]`, `[ path, status, username, created_at ]`, `[ path, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status ]`, `[ true_path, remote_ip, success, action_type ]`, `[ true_path, remote_ip, success, username ]`, `[ true_path, remote_ip, success, created_at ]`, `[ true_path, remote_ip, status, action_type ]`, `[ true_path, remote_ip, status, username ]`, `[ true_path, remote_ip, status, created_at ]`, `[ true_path, remote_ip, action_type, username ]`, `[ true_path, remote_ip, action_type, created_at ]`, `[ true_path, remote_ip, username, created_at ]`, `[ true_path, success, status, action_type ]`, `[ true_path, success, status, username ]`, `[ true_path, success, status, created_at ]`, `[ true_path, success, action_type, username ]`, `[ true_path, success, action_type, created_at ]`, `[ true_path, success, username, created_at ]`, `[ true_path, status, action_type, username ]`, `[ true_path, status, action_type, created_at ]`, `[ true_path, status, username, created_at ]`, `[ true_path, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type ]`, `[ remote_ip, success, status, username ]`, `[ remote_ip, success, status, created_at ]`, `[ remote_ip, success, action_type, username ]`, `[ remote_ip, success, action_type, created_at ]`, `[ remote_ip, success, username, created_at ]`, `[ remote_ip, status, action_type, username ]`, `[ remote_ip, status, action_type, created_at ]`, `[ remote_ip, status, username, created_at ]`, `[ remote_ip, action_type, username, created_at ]`, `[ success, status, action_type, username ]`, `[ success, status, action_type, created_at ]`, `[ success, status, username, created_at ]`, `[ success, action_type, username, created_at ]`, `[ status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status ]`, `[ path, true_path, remote_ip, success, action_type ]`, `[ path, true_path, remote_ip, success, username ]`, `[ path, true_path, remote_ip, success, created_at ]`, `[ path, true_path, remote_ip, status, action_type ]`, `[ path, true_path, remote_ip, status, username ]`, `[ path, true_path, remote_ip, status, created_at ]`, `[ path, true_path, remote_ip, action_type, username ]`, `[ path, true_path, remote_ip, action_type, created_at ]`, `[ path, true_path, remote_ip, username, created_at ]`, `[ path, true_path, success, status, action_type ]`, `[ path, true_path, success, status, username ]`, `[ path, true_path, success, status, created_at ]`, `[ path, true_path, success, action_type, username ]`, `[ path, true_path, success, action_type, created_at ]`, `[ path, true_path, success, username, created_at ]`, `[ path, true_path, status, action_type, username ]`, `[ path, true_path, status, action_type, created_at ]`, `[ path, true_path, status, username, created_at ]`, `[ path, true_path, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type ]`, `[ path, remote_ip, success, status, username ]`, `[ path, remote_ip, success, status, created_at ]`, `[ path, remote_ip, success, action_type, username ]`, `[ path, remote_ip, success, action_type, created_at ]`, `[ path, remote_ip, success, username, created_at ]`, `[ path, remote_ip, status, action_type, username ]`, `[ path, remote_ip, status, action_type, created_at ]`, `[ path, remote_ip, status, username, created_at ]`, `[ path, remote_ip, action_type, username, created_at ]`, `[ path, success, status, action_type, username ]`, `[ path, success, status, action_type, created_at ]`, `[ path, success, status, username, created_at ]`, `[ path, success, action_type, username, created_at ]`, `[ path, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type ]`, `[ true_path, remote_ip, success, status, username ]`, `[ true_path, remote_ip, success, status, created_at ]`, `[ true_path, remote_ip, success, action_type, username ]`, `[ true_path, remote_ip, success, action_type, created_at ]`, `[ true_path, remote_ip, success, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username ]`, `[ true_path, remote_ip, status, action_type, created_at ]`, `[ true_path, remote_ip, status, username, created_at ]`, `[ true_path, remote_ip, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username ]`, `[ true_path, success, status, action_type, created_at ]`, `[ true_path, success, status, username, created_at ]`, `[ true_path, success, action_type, username, created_at ]`, `[ true_path, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username ]`, `[ remote_ip, success, status, action_type, created_at ]`, `[ remote_ip, success, status, username, created_at ]`, `[ remote_ip, success, action_type, username, created_at ]`, `[ remote_ip, status, action_type, username, created_at ]`, `[ success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type ]`, `[ path, true_path, remote_ip, success, status, username ]`, `[ path, true_path, remote_ip, success, status, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username ]`, `[ path, true_path, remote_ip, success, action_type, created_at ]`, `[ path, true_path, remote_ip, success, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username ]`, `[ path, true_path, remote_ip, status, action_type, created_at ]`, `[ path, true_path, remote_ip, status, username, created_at ]`, `[ path, true_path, remote_ip, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username ]`, `[ path, true_path, success, status, action_type, created_at ]`, `[ path, true_path, success, status, username, created_at ]`, `[ path, true_path, success, action_type, username, created_at ]`, `[ path, true_path, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username ]`, `[ path, remote_ip, success, status, action_type, created_at ]`, `[ path, remote_ip, success, status, username, created_at ]`, `[ path, remote_ip, success, action_type, username, created_at ]`, `[ path, remote_ip, status, action_type, username, created_at ]`, `[ path, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username ]`, `[ true_path, remote_ip, success, status, action_type, created_at ]`, `[ true_path, remote_ip, success, status, username, created_at ]`, `[ true_path, remote_ip, success, action_type, username, created_at ]`, `[ true_path, remote_ip, status, action_type, username, created_at ]`, `[ true_path, success, status, action_type, username, created_at ]`, `[ remote_ip, success, status, action_type, username, created_at ]`, `[ path, true_path, remote_ip, success, status, action_type, username ]`, `[ path, true_path, remote_ip, success, status, action_type, created_at ]`, `[ path, true_path, remote_ip, success, status, username, created_at ]`, `[ path, true_path, remote_ip, success, action_type, username, created_at ]`, `[ path, true_path, remote_ip, status, action_type, username, created_at ]`, `[ path, true_path, success, status, action_type, username, created_at ]`, `[ path, remote_ip, success, status, action_type, username, created_at ]`, `[ true_path, remote_ip, success, status, action_type, username, created_at ]` or `[ path, true_path, remote_ip, success, status, action_type, username, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

### FilesCom.PowerShell.Models.WebDavActionLog

The web dav action log as Files.com returned it.

### FilesCom.PowerShell.FilesPage`1

With -Page or -Continuation, one page of results and the continuation for the next.

## NOTES

Failures are written as error records with IDs such as Files.NotFound or Files.Authentication. See about_FilesCom for connections, paging, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
