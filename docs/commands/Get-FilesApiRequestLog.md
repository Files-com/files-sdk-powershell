---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Get-FilesApiRequestLog
---

# Get-FilesApiRequestLog

## SYNOPSIS

List API Request Logs.

## SYNTAX

### List (Default)

```
Get-FilesApiRequestLog [<parameters of the List set>]
```

### Page

```
Get-FilesApiRequestLog [<parameters of the Page set>]
```

### Continuation

```
Get-FilesApiRequestLog [<parameters of the Continuation set>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

List API Request Logs.

Without -Page or -Continuation, the command writes each api request log of every page, loading one page at a time as the pipeline consumes them. -First and -Skip bound what it writes, and stopping the pipeline early, as Select-Object -First does, loads no further page. A failure on any page stops the command, so a partial listing is never taken for a complete one.

## EXAMPLES

### Example 1: List all api request logs

```powershell
Get-FilesApiRequestLog
```

Writes every api request log, loading one page at a time as the pipeline consumes them.

### Example 2: Get one page at a time

```powershell
$page = Get-FilesApiRequestLog -Page
$next = Get-FilesApiRequestLog -Continuation $page.Continuation
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

If set, return records where the specified field is equal to the supplied value. Valid fields are `api_key_id`, `interface`, `request_id`, `request_ip`, `request_method`, `success`, `user_id` or `created_at`. Valid field combinations are `[ api_key_id ]`, `[ interface ]`, `[ request_id ]`, `[ request_ip ]`, `[ request_method ]`, `[ success ]`, `[ user_id ]`, `[ created_at ]`, `[ api_key_id, interface ]`, `[ api_key_id, request_id ]`, `[ api_key_id, request_ip ]`, `[ api_key_id, request_method ]`, `[ api_key_id, success ]`, `[ api_key_id, user_id ]`, `[ api_key_id, created_at ]`, `[ interface, request_id ]`, `[ interface, request_ip ]`, `[ interface, request_method ]`, `[ interface, success ]`, `[ interface, user_id ]`, `[ interface, created_at ]`, `[ request_id, request_ip ]`, `[ request_id, request_method ]`, `[ request_id, success ]`, `[ request_id, user_id ]`, `[ request_id, created_at ]`, `[ request_ip, request_method ]`, `[ request_ip, success ]`, `[ request_ip, user_id ]`, `[ request_ip, created_at ]`, `[ request_method, success ]`, `[ request_method, user_id ]`, `[ request_method, created_at ]`, `[ success, user_id ]`, `[ success, created_at ]`, `[ user_id, created_at ]`, `[ api_key_id, interface, request_id ]`, `[ api_key_id, interface, request_ip ]`, `[ api_key_id, interface, request_method ]`, `[ api_key_id, interface, success ]`, `[ api_key_id, interface, user_id ]`, `[ api_key_id, interface, created_at ]`, `[ api_key_id, request_id, request_ip ]`, `[ api_key_id, request_id, request_method ]`, `[ api_key_id, request_id, success ]`, `[ api_key_id, request_id, user_id ]`, `[ api_key_id, request_id, created_at ]`, `[ api_key_id, request_ip, request_method ]`, `[ api_key_id, request_ip, success ]`, `[ api_key_id, request_ip, user_id ]`, `[ api_key_id, request_ip, created_at ]`, `[ api_key_id, request_method, success ]`, `[ api_key_id, request_method, user_id ]`, `[ api_key_id, request_method, created_at ]`, `[ api_key_id, success, user_id ]`, `[ api_key_id, success, created_at ]`, `[ api_key_id, user_id, created_at ]`, `[ interface, request_id, request_ip ]`, `[ interface, request_id, request_method ]`, `[ interface, request_id, success ]`, `[ interface, request_id, user_id ]`, `[ interface, request_id, created_at ]`, `[ interface, request_ip, request_method ]`, `[ interface, request_ip, success ]`, `[ interface, request_ip, user_id ]`, `[ interface, request_ip, created_at ]`, `[ interface, request_method, success ]`, `[ interface, request_method, user_id ]`, `[ interface, request_method, created_at ]`, `[ interface, success, user_id ]`, `[ interface, success, created_at ]`, `[ interface, user_id, created_at ]`, `[ request_id, request_ip, request_method ]`, `[ request_id, request_ip, success ]`, `[ request_id, request_ip, user_id ]`, `[ request_id, request_ip, created_at ]`, `[ request_id, request_method, success ]`, `[ request_id, request_method, user_id ]`, `[ request_id, request_method, created_at ]`, `[ request_id, success, user_id ]`, `[ request_id, success, created_at ]`, `[ request_id, user_id, created_at ]`, `[ request_ip, request_method, success ]`, `[ request_ip, request_method, user_id ]`, `[ request_ip, request_method, created_at ]`, `[ request_ip, success, user_id ]`, `[ request_ip, success, created_at ]`, `[ request_ip, user_id, created_at ]`, `[ request_method, success, user_id ]`, `[ request_method, success, created_at ]`, `[ request_method, user_id, created_at ]`, `[ success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip ]`, `[ api_key_id, interface, request_id, request_method ]`, `[ api_key_id, interface, request_id, success ]`, `[ api_key_id, interface, request_id, user_id ]`, `[ api_key_id, interface, request_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method ]`, `[ api_key_id, interface, request_ip, success ]`, `[ api_key_id, interface, request_ip, user_id ]`, `[ api_key_id, interface, request_ip, created_at ]`, `[ api_key_id, interface, request_method, success ]`, `[ api_key_id, interface, request_method, user_id ]`, `[ api_key_id, interface, request_method, created_at ]`, `[ api_key_id, interface, success, user_id ]`, `[ api_key_id, interface, success, created_at ]`, `[ api_key_id, interface, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method ]`, `[ api_key_id, request_id, request_ip, success ]`, `[ api_key_id, request_id, request_ip, user_id ]`, `[ api_key_id, request_id, request_ip, created_at ]`, `[ api_key_id, request_id, request_method, success ]`, `[ api_key_id, request_id, request_method, user_id ]`, `[ api_key_id, request_id, request_method, created_at ]`, `[ api_key_id, request_id, success, user_id ]`, `[ api_key_id, request_id, success, created_at ]`, `[ api_key_id, request_id, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success ]`, `[ api_key_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_ip, success, user_id ]`, `[ api_key_id, request_ip, success, created_at ]`, `[ api_key_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id ]`, `[ api_key_id, request_method, success, created_at ]`, `[ api_key_id, request_method, user_id, created_at ]`, `[ api_key_id, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method ]`, `[ interface, request_id, request_ip, success ]`, `[ interface, request_id, request_ip, user_id ]`, `[ interface, request_id, request_ip, created_at ]`, `[ interface, request_id, request_method, success ]`, `[ interface, request_id, request_method, user_id ]`, `[ interface, request_id, request_method, created_at ]`, `[ interface, request_id, success, user_id ]`, `[ interface, request_id, success, created_at ]`, `[ interface, request_id, user_id, created_at ]`, `[ interface, request_ip, request_method, success ]`, `[ interface, request_ip, request_method, user_id ]`, `[ interface, request_ip, request_method, created_at ]`, `[ interface, request_ip, success, user_id ]`, `[ interface, request_ip, success, created_at ]`, `[ interface, request_ip, user_id, created_at ]`, `[ interface, request_method, success, user_id ]`, `[ interface, request_method, success, created_at ]`, `[ interface, request_method, user_id, created_at ]`, `[ interface, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success ]`, `[ request_id, request_ip, request_method, user_id ]`, `[ request_id, request_ip, request_method, created_at ]`, `[ request_id, request_ip, success, user_id ]`, `[ request_id, request_ip, success, created_at ]`, `[ request_id, request_ip, user_id, created_at ]`, `[ request_id, request_method, success, user_id ]`, `[ request_id, request_method, success, created_at ]`, `[ request_id, request_method, user_id, created_at ]`, `[ request_id, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id ]`, `[ request_ip, request_method, success, created_at ]`, `[ request_ip, request_method, user_id, created_at ]`, `[ request_ip, success, user_id, created_at ]`, `[ request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method ]`, `[ api_key_id, interface, request_id, request_ip, success ]`, `[ api_key_id, interface, request_id, request_ip, user_id ]`, `[ api_key_id, interface, request_id, request_ip, created_at ]`, `[ api_key_id, interface, request_id, request_method, success ]`, `[ api_key_id, interface, request_id, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_method, created_at ]`, `[ api_key_id, interface, request_id, success, user_id ]`, `[ api_key_id, interface, request_id, success, created_at ]`, `[ api_key_id, interface, request_id, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success ]`, `[ api_key_id, interface, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id ]`, `[ api_key_id, interface, request_ip, success, created_at ]`, `[ api_key_id, interface, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id ]`, `[ api_key_id, interface, request_method, success, created_at ]`, `[ api_key_id, interface, request_method, user_id, created_at ]`, `[ api_key_id, interface, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success ]`, `[ api_key_id, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id ]`, `[ api_key_id, request_id, request_ip, success, created_at ]`, `[ api_key_id, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id ]`, `[ api_key_id, request_id, request_method, success, created_at ]`, `[ api_key_id, request_id, request_method, user_id, created_at ]`, `[ api_key_id, request_id, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success ]`, `[ interface, request_id, request_ip, request_method, user_id ]`, `[ interface, request_id, request_ip, request_method, created_at ]`, `[ interface, request_id, request_ip, success, user_id ]`, `[ interface, request_id, request_ip, success, created_at ]`, `[ interface, request_id, request_ip, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id ]`, `[ interface, request_id, request_method, success, created_at ]`, `[ interface, request_id, request_method, user_id, created_at ]`, `[ interface, request_id, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id ]`, `[ interface, request_ip, request_method, success, created_at ]`, `[ interface, request_ip, request_method, user_id, created_at ]`, `[ interface, request_ip, success, user_id, created_at ]`, `[ interface, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id ]`, `[ request_id, request_ip, request_method, success, created_at ]`, `[ request_id, request_ip, request_method, user_id, created_at ]`, `[ request_id, request_ip, success, user_id, created_at ]`, `[ request_id, request_method, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id ]`, `[ interface, request_id, request_ip, request_method, success, created_at ]`, `[ interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ interface, request_id, request_ip, success, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id, created_at ]` or `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is greater than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ api_key_id ]`, `[ interface ]`, `[ request_id ]`, `[ request_ip ]`, `[ request_method ]`, `[ success ]`, `[ user_id ]`, `[ created_at ]`, `[ api_key_id, interface ]`, `[ api_key_id, request_id ]`, `[ api_key_id, request_ip ]`, `[ api_key_id, request_method ]`, `[ api_key_id, success ]`, `[ api_key_id, user_id ]`, `[ api_key_id, created_at ]`, `[ interface, request_id ]`, `[ interface, request_ip ]`, `[ interface, request_method ]`, `[ interface, success ]`, `[ interface, user_id ]`, `[ interface, created_at ]`, `[ request_id, request_ip ]`, `[ request_id, request_method ]`, `[ request_id, success ]`, `[ request_id, user_id ]`, `[ request_id, created_at ]`, `[ request_ip, request_method ]`, `[ request_ip, success ]`, `[ request_ip, user_id ]`, `[ request_ip, created_at ]`, `[ request_method, success ]`, `[ request_method, user_id ]`, `[ request_method, created_at ]`, `[ success, user_id ]`, `[ success, created_at ]`, `[ user_id, created_at ]`, `[ api_key_id, interface, request_id ]`, `[ api_key_id, interface, request_ip ]`, `[ api_key_id, interface, request_method ]`, `[ api_key_id, interface, success ]`, `[ api_key_id, interface, user_id ]`, `[ api_key_id, interface, created_at ]`, `[ api_key_id, request_id, request_ip ]`, `[ api_key_id, request_id, request_method ]`, `[ api_key_id, request_id, success ]`, `[ api_key_id, request_id, user_id ]`, `[ api_key_id, request_id, created_at ]`, `[ api_key_id, request_ip, request_method ]`, `[ api_key_id, request_ip, success ]`, `[ api_key_id, request_ip, user_id ]`, `[ api_key_id, request_ip, created_at ]`, `[ api_key_id, request_method, success ]`, `[ api_key_id, request_method, user_id ]`, `[ api_key_id, request_method, created_at ]`, `[ api_key_id, success, user_id ]`, `[ api_key_id, success, created_at ]`, `[ api_key_id, user_id, created_at ]`, `[ interface, request_id, request_ip ]`, `[ interface, request_id, request_method ]`, `[ interface, request_id, success ]`, `[ interface, request_id, user_id ]`, `[ interface, request_id, created_at ]`, `[ interface, request_ip, request_method ]`, `[ interface, request_ip, success ]`, `[ interface, request_ip, user_id ]`, `[ interface, request_ip, created_at ]`, `[ interface, request_method, success ]`, `[ interface, request_method, user_id ]`, `[ interface, request_method, created_at ]`, `[ interface, success, user_id ]`, `[ interface, success, created_at ]`, `[ interface, user_id, created_at ]`, `[ request_id, request_ip, request_method ]`, `[ request_id, request_ip, success ]`, `[ request_id, request_ip, user_id ]`, `[ request_id, request_ip, created_at ]`, `[ request_id, request_method, success ]`, `[ request_id, request_method, user_id ]`, `[ request_id, request_method, created_at ]`, `[ request_id, success, user_id ]`, `[ request_id, success, created_at ]`, `[ request_id, user_id, created_at ]`, `[ request_ip, request_method, success ]`, `[ request_ip, request_method, user_id ]`, `[ request_ip, request_method, created_at ]`, `[ request_ip, success, user_id ]`, `[ request_ip, success, created_at ]`, `[ request_ip, user_id, created_at ]`, `[ request_method, success, user_id ]`, `[ request_method, success, created_at ]`, `[ request_method, user_id, created_at ]`, `[ success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip ]`, `[ api_key_id, interface, request_id, request_method ]`, `[ api_key_id, interface, request_id, success ]`, `[ api_key_id, interface, request_id, user_id ]`, `[ api_key_id, interface, request_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method ]`, `[ api_key_id, interface, request_ip, success ]`, `[ api_key_id, interface, request_ip, user_id ]`, `[ api_key_id, interface, request_ip, created_at ]`, `[ api_key_id, interface, request_method, success ]`, `[ api_key_id, interface, request_method, user_id ]`, `[ api_key_id, interface, request_method, created_at ]`, `[ api_key_id, interface, success, user_id ]`, `[ api_key_id, interface, success, created_at ]`, `[ api_key_id, interface, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method ]`, `[ api_key_id, request_id, request_ip, success ]`, `[ api_key_id, request_id, request_ip, user_id ]`, `[ api_key_id, request_id, request_ip, created_at ]`, `[ api_key_id, request_id, request_method, success ]`, `[ api_key_id, request_id, request_method, user_id ]`, `[ api_key_id, request_id, request_method, created_at ]`, `[ api_key_id, request_id, success, user_id ]`, `[ api_key_id, request_id, success, created_at ]`, `[ api_key_id, request_id, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success ]`, `[ api_key_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_ip, success, user_id ]`, `[ api_key_id, request_ip, success, created_at ]`, `[ api_key_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id ]`, `[ api_key_id, request_method, success, created_at ]`, `[ api_key_id, request_method, user_id, created_at ]`, `[ api_key_id, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method ]`, `[ interface, request_id, request_ip, success ]`, `[ interface, request_id, request_ip, user_id ]`, `[ interface, request_id, request_ip, created_at ]`, `[ interface, request_id, request_method, success ]`, `[ interface, request_id, request_method, user_id ]`, `[ interface, request_id, request_method, created_at ]`, `[ interface, request_id, success, user_id ]`, `[ interface, request_id, success, created_at ]`, `[ interface, request_id, user_id, created_at ]`, `[ interface, request_ip, request_method, success ]`, `[ interface, request_ip, request_method, user_id ]`, `[ interface, request_ip, request_method, created_at ]`, `[ interface, request_ip, success, user_id ]`, `[ interface, request_ip, success, created_at ]`, `[ interface, request_ip, user_id, created_at ]`, `[ interface, request_method, success, user_id ]`, `[ interface, request_method, success, created_at ]`, `[ interface, request_method, user_id, created_at ]`, `[ interface, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success ]`, `[ request_id, request_ip, request_method, user_id ]`, `[ request_id, request_ip, request_method, created_at ]`, `[ request_id, request_ip, success, user_id ]`, `[ request_id, request_ip, success, created_at ]`, `[ request_id, request_ip, user_id, created_at ]`, `[ request_id, request_method, success, user_id ]`, `[ request_id, request_method, success, created_at ]`, `[ request_id, request_method, user_id, created_at ]`, `[ request_id, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id ]`, `[ request_ip, request_method, success, created_at ]`, `[ request_ip, request_method, user_id, created_at ]`, `[ request_ip, success, user_id, created_at ]`, `[ request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method ]`, `[ api_key_id, interface, request_id, request_ip, success ]`, `[ api_key_id, interface, request_id, request_ip, user_id ]`, `[ api_key_id, interface, request_id, request_ip, created_at ]`, `[ api_key_id, interface, request_id, request_method, success ]`, `[ api_key_id, interface, request_id, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_method, created_at ]`, `[ api_key_id, interface, request_id, success, user_id ]`, `[ api_key_id, interface, request_id, success, created_at ]`, `[ api_key_id, interface, request_id, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success ]`, `[ api_key_id, interface, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id ]`, `[ api_key_id, interface, request_ip, success, created_at ]`, `[ api_key_id, interface, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id ]`, `[ api_key_id, interface, request_method, success, created_at ]`, `[ api_key_id, interface, request_method, user_id, created_at ]`, `[ api_key_id, interface, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success ]`, `[ api_key_id, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id ]`, `[ api_key_id, request_id, request_ip, success, created_at ]`, `[ api_key_id, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id ]`, `[ api_key_id, request_id, request_method, success, created_at ]`, `[ api_key_id, request_id, request_method, user_id, created_at ]`, `[ api_key_id, request_id, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success ]`, `[ interface, request_id, request_ip, request_method, user_id ]`, `[ interface, request_id, request_ip, request_method, created_at ]`, `[ interface, request_id, request_ip, success, user_id ]`, `[ interface, request_id, request_ip, success, created_at ]`, `[ interface, request_id, request_ip, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id ]`, `[ interface, request_id, request_method, success, created_at ]`, `[ interface, request_id, request_method, user_id, created_at ]`, `[ interface, request_id, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id ]`, `[ interface, request_ip, request_method, success, created_at ]`, `[ interface, request_ip, request_method, user_id, created_at ]`, `[ interface, request_ip, success, user_id, created_at ]`, `[ interface, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id ]`, `[ request_id, request_ip, request_method, success, created_at ]`, `[ request_id, request_ip, request_method, user_id, created_at ]`, `[ request_id, request_ip, success, user_id, created_at ]`, `[ request_id, request_method, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id ]`, `[ interface, request_id, request_ip, request_method, success, created_at ]`, `[ interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ interface, request_id, request_ip, success, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id, created_at ]` or `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ api_key_id ]`, `[ interface ]`, `[ request_id ]`, `[ request_ip ]`, `[ request_method ]`, `[ success ]`, `[ user_id ]`, `[ created_at ]`, `[ api_key_id, interface ]`, `[ api_key_id, request_id ]`, `[ api_key_id, request_ip ]`, `[ api_key_id, request_method ]`, `[ api_key_id, success ]`, `[ api_key_id, user_id ]`, `[ api_key_id, created_at ]`, `[ interface, request_id ]`, `[ interface, request_ip ]`, `[ interface, request_method ]`, `[ interface, success ]`, `[ interface, user_id ]`, `[ interface, created_at ]`, `[ request_id, request_ip ]`, `[ request_id, request_method ]`, `[ request_id, success ]`, `[ request_id, user_id ]`, `[ request_id, created_at ]`, `[ request_ip, request_method ]`, `[ request_ip, success ]`, `[ request_ip, user_id ]`, `[ request_ip, created_at ]`, `[ request_method, success ]`, `[ request_method, user_id ]`, `[ request_method, created_at ]`, `[ success, user_id ]`, `[ success, created_at ]`, `[ user_id, created_at ]`, `[ api_key_id, interface, request_id ]`, `[ api_key_id, interface, request_ip ]`, `[ api_key_id, interface, request_method ]`, `[ api_key_id, interface, success ]`, `[ api_key_id, interface, user_id ]`, `[ api_key_id, interface, created_at ]`, `[ api_key_id, request_id, request_ip ]`, `[ api_key_id, request_id, request_method ]`, `[ api_key_id, request_id, success ]`, `[ api_key_id, request_id, user_id ]`, `[ api_key_id, request_id, created_at ]`, `[ api_key_id, request_ip, request_method ]`, `[ api_key_id, request_ip, success ]`, `[ api_key_id, request_ip, user_id ]`, `[ api_key_id, request_ip, created_at ]`, `[ api_key_id, request_method, success ]`, `[ api_key_id, request_method, user_id ]`, `[ api_key_id, request_method, created_at ]`, `[ api_key_id, success, user_id ]`, `[ api_key_id, success, created_at ]`, `[ api_key_id, user_id, created_at ]`, `[ interface, request_id, request_ip ]`, `[ interface, request_id, request_method ]`, `[ interface, request_id, success ]`, `[ interface, request_id, user_id ]`, `[ interface, request_id, created_at ]`, `[ interface, request_ip, request_method ]`, `[ interface, request_ip, success ]`, `[ interface, request_ip, user_id ]`, `[ interface, request_ip, created_at ]`, `[ interface, request_method, success ]`, `[ interface, request_method, user_id ]`, `[ interface, request_method, created_at ]`, `[ interface, success, user_id ]`, `[ interface, success, created_at ]`, `[ interface, user_id, created_at ]`, `[ request_id, request_ip, request_method ]`, `[ request_id, request_ip, success ]`, `[ request_id, request_ip, user_id ]`, `[ request_id, request_ip, created_at ]`, `[ request_id, request_method, success ]`, `[ request_id, request_method, user_id ]`, `[ request_id, request_method, created_at ]`, `[ request_id, success, user_id ]`, `[ request_id, success, created_at ]`, `[ request_id, user_id, created_at ]`, `[ request_ip, request_method, success ]`, `[ request_ip, request_method, user_id ]`, `[ request_ip, request_method, created_at ]`, `[ request_ip, success, user_id ]`, `[ request_ip, success, created_at ]`, `[ request_ip, user_id, created_at ]`, `[ request_method, success, user_id ]`, `[ request_method, success, created_at ]`, `[ request_method, user_id, created_at ]`, `[ success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip ]`, `[ api_key_id, interface, request_id, request_method ]`, `[ api_key_id, interface, request_id, success ]`, `[ api_key_id, interface, request_id, user_id ]`, `[ api_key_id, interface, request_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method ]`, `[ api_key_id, interface, request_ip, success ]`, `[ api_key_id, interface, request_ip, user_id ]`, `[ api_key_id, interface, request_ip, created_at ]`, `[ api_key_id, interface, request_method, success ]`, `[ api_key_id, interface, request_method, user_id ]`, `[ api_key_id, interface, request_method, created_at ]`, `[ api_key_id, interface, success, user_id ]`, `[ api_key_id, interface, success, created_at ]`, `[ api_key_id, interface, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method ]`, `[ api_key_id, request_id, request_ip, success ]`, `[ api_key_id, request_id, request_ip, user_id ]`, `[ api_key_id, request_id, request_ip, created_at ]`, `[ api_key_id, request_id, request_method, success ]`, `[ api_key_id, request_id, request_method, user_id ]`, `[ api_key_id, request_id, request_method, created_at ]`, `[ api_key_id, request_id, success, user_id ]`, `[ api_key_id, request_id, success, created_at ]`, `[ api_key_id, request_id, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success ]`, `[ api_key_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_ip, success, user_id ]`, `[ api_key_id, request_ip, success, created_at ]`, `[ api_key_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id ]`, `[ api_key_id, request_method, success, created_at ]`, `[ api_key_id, request_method, user_id, created_at ]`, `[ api_key_id, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method ]`, `[ interface, request_id, request_ip, success ]`, `[ interface, request_id, request_ip, user_id ]`, `[ interface, request_id, request_ip, created_at ]`, `[ interface, request_id, request_method, success ]`, `[ interface, request_id, request_method, user_id ]`, `[ interface, request_id, request_method, created_at ]`, `[ interface, request_id, success, user_id ]`, `[ interface, request_id, success, created_at ]`, `[ interface, request_id, user_id, created_at ]`, `[ interface, request_ip, request_method, success ]`, `[ interface, request_ip, request_method, user_id ]`, `[ interface, request_ip, request_method, created_at ]`, `[ interface, request_ip, success, user_id ]`, `[ interface, request_ip, success, created_at ]`, `[ interface, request_ip, user_id, created_at ]`, `[ interface, request_method, success, user_id ]`, `[ interface, request_method, success, created_at ]`, `[ interface, request_method, user_id, created_at ]`, `[ interface, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success ]`, `[ request_id, request_ip, request_method, user_id ]`, `[ request_id, request_ip, request_method, created_at ]`, `[ request_id, request_ip, success, user_id ]`, `[ request_id, request_ip, success, created_at ]`, `[ request_id, request_ip, user_id, created_at ]`, `[ request_id, request_method, success, user_id ]`, `[ request_id, request_method, success, created_at ]`, `[ request_id, request_method, user_id, created_at ]`, `[ request_id, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id ]`, `[ request_ip, request_method, success, created_at ]`, `[ request_ip, request_method, user_id, created_at ]`, `[ request_ip, success, user_id, created_at ]`, `[ request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method ]`, `[ api_key_id, interface, request_id, request_ip, success ]`, `[ api_key_id, interface, request_id, request_ip, user_id ]`, `[ api_key_id, interface, request_id, request_ip, created_at ]`, `[ api_key_id, interface, request_id, request_method, success ]`, `[ api_key_id, interface, request_id, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_method, created_at ]`, `[ api_key_id, interface, request_id, success, user_id ]`, `[ api_key_id, interface, request_id, success, created_at ]`, `[ api_key_id, interface, request_id, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success ]`, `[ api_key_id, interface, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id ]`, `[ api_key_id, interface, request_ip, success, created_at ]`, `[ api_key_id, interface, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id ]`, `[ api_key_id, interface, request_method, success, created_at ]`, `[ api_key_id, interface, request_method, user_id, created_at ]`, `[ api_key_id, interface, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success ]`, `[ api_key_id, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id ]`, `[ api_key_id, request_id, request_ip, success, created_at ]`, `[ api_key_id, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id ]`, `[ api_key_id, request_id, request_method, success, created_at ]`, `[ api_key_id, request_id, request_method, user_id, created_at ]`, `[ api_key_id, request_id, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success ]`, `[ interface, request_id, request_ip, request_method, user_id ]`, `[ interface, request_id, request_ip, request_method, created_at ]`, `[ interface, request_id, request_ip, success, user_id ]`, `[ interface, request_id, request_ip, success, created_at ]`, `[ interface, request_id, request_ip, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id ]`, `[ interface, request_id, request_method, success, created_at ]`, `[ interface, request_id, request_method, user_id, created_at ]`, `[ interface, request_id, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id ]`, `[ interface, request_ip, request_method, success, created_at ]`, `[ interface, request_ip, request_method, user_id, created_at ]`, `[ interface, request_ip, success, user_id, created_at ]`, `[ interface, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id ]`, `[ request_id, request_ip, request_method, success, created_at ]`, `[ request_id, request_ip, request_method, user_id, created_at ]`, `[ request_id, request_ip, success, user_id, created_at ]`, `[ request_id, request_method, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id ]`, `[ interface, request_id, request_ip, request_method, success, created_at ]`, `[ interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ interface, request_id, request_ip, success, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id, created_at ]` or `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is prefixed by the supplied value. Valid fields are `interface`, `request_id`, `request_ip` or `request_method`. Valid field combinations are `[ api_key_id ]`, `[ interface ]`, `[ request_id ]`, `[ request_ip ]`, `[ request_method ]`, `[ success ]`, `[ user_id ]`, `[ created_at ]`, `[ api_key_id, interface ]`, `[ api_key_id, request_id ]`, `[ api_key_id, request_ip ]`, `[ api_key_id, request_method ]`, `[ api_key_id, success ]`, `[ api_key_id, user_id ]`, `[ api_key_id, created_at ]`, `[ interface, request_id ]`, `[ interface, request_ip ]`, `[ interface, request_method ]`, `[ interface, success ]`, `[ interface, user_id ]`, `[ interface, created_at ]`, `[ request_id, request_ip ]`, `[ request_id, request_method ]`, `[ request_id, success ]`, `[ request_id, user_id ]`, `[ request_id, created_at ]`, `[ request_ip, request_method ]`, `[ request_ip, success ]`, `[ request_ip, user_id ]`, `[ request_ip, created_at ]`, `[ request_method, success ]`, `[ request_method, user_id ]`, `[ request_method, created_at ]`, `[ success, user_id ]`, `[ success, created_at ]`, `[ user_id, created_at ]`, `[ api_key_id, interface, request_id ]`, `[ api_key_id, interface, request_ip ]`, `[ api_key_id, interface, request_method ]`, `[ api_key_id, interface, success ]`, `[ api_key_id, interface, user_id ]`, `[ api_key_id, interface, created_at ]`, `[ api_key_id, request_id, request_ip ]`, `[ api_key_id, request_id, request_method ]`, `[ api_key_id, request_id, success ]`, `[ api_key_id, request_id, user_id ]`, `[ api_key_id, request_id, created_at ]`, `[ api_key_id, request_ip, request_method ]`, `[ api_key_id, request_ip, success ]`, `[ api_key_id, request_ip, user_id ]`, `[ api_key_id, request_ip, created_at ]`, `[ api_key_id, request_method, success ]`, `[ api_key_id, request_method, user_id ]`, `[ api_key_id, request_method, created_at ]`, `[ api_key_id, success, user_id ]`, `[ api_key_id, success, created_at ]`, `[ api_key_id, user_id, created_at ]`, `[ interface, request_id, request_ip ]`, `[ interface, request_id, request_method ]`, `[ interface, request_id, success ]`, `[ interface, request_id, user_id ]`, `[ interface, request_id, created_at ]`, `[ interface, request_ip, request_method ]`, `[ interface, request_ip, success ]`, `[ interface, request_ip, user_id ]`, `[ interface, request_ip, created_at ]`, `[ interface, request_method, success ]`, `[ interface, request_method, user_id ]`, `[ interface, request_method, created_at ]`, `[ interface, success, user_id ]`, `[ interface, success, created_at ]`, `[ interface, user_id, created_at ]`, `[ request_id, request_ip, request_method ]`, `[ request_id, request_ip, success ]`, `[ request_id, request_ip, user_id ]`, `[ request_id, request_ip, created_at ]`, `[ request_id, request_method, success ]`, `[ request_id, request_method, user_id ]`, `[ request_id, request_method, created_at ]`, `[ request_id, success, user_id ]`, `[ request_id, success, created_at ]`, `[ request_id, user_id, created_at ]`, `[ request_ip, request_method, success ]`, `[ request_ip, request_method, user_id ]`, `[ request_ip, request_method, created_at ]`, `[ request_ip, success, user_id ]`, `[ request_ip, success, created_at ]`, `[ request_ip, user_id, created_at ]`, `[ request_method, success, user_id ]`, `[ request_method, success, created_at ]`, `[ request_method, user_id, created_at ]`, `[ success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip ]`, `[ api_key_id, interface, request_id, request_method ]`, `[ api_key_id, interface, request_id, success ]`, `[ api_key_id, interface, request_id, user_id ]`, `[ api_key_id, interface, request_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method ]`, `[ api_key_id, interface, request_ip, success ]`, `[ api_key_id, interface, request_ip, user_id ]`, `[ api_key_id, interface, request_ip, created_at ]`, `[ api_key_id, interface, request_method, success ]`, `[ api_key_id, interface, request_method, user_id ]`, `[ api_key_id, interface, request_method, created_at ]`, `[ api_key_id, interface, success, user_id ]`, `[ api_key_id, interface, success, created_at ]`, `[ api_key_id, interface, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method ]`, `[ api_key_id, request_id, request_ip, success ]`, `[ api_key_id, request_id, request_ip, user_id ]`, `[ api_key_id, request_id, request_ip, created_at ]`, `[ api_key_id, request_id, request_method, success ]`, `[ api_key_id, request_id, request_method, user_id ]`, `[ api_key_id, request_id, request_method, created_at ]`, `[ api_key_id, request_id, success, user_id ]`, `[ api_key_id, request_id, success, created_at ]`, `[ api_key_id, request_id, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success ]`, `[ api_key_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_ip, success, user_id ]`, `[ api_key_id, request_ip, success, created_at ]`, `[ api_key_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id ]`, `[ api_key_id, request_method, success, created_at ]`, `[ api_key_id, request_method, user_id, created_at ]`, `[ api_key_id, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method ]`, `[ interface, request_id, request_ip, success ]`, `[ interface, request_id, request_ip, user_id ]`, `[ interface, request_id, request_ip, created_at ]`, `[ interface, request_id, request_method, success ]`, `[ interface, request_id, request_method, user_id ]`, `[ interface, request_id, request_method, created_at ]`, `[ interface, request_id, success, user_id ]`, `[ interface, request_id, success, created_at ]`, `[ interface, request_id, user_id, created_at ]`, `[ interface, request_ip, request_method, success ]`, `[ interface, request_ip, request_method, user_id ]`, `[ interface, request_ip, request_method, created_at ]`, `[ interface, request_ip, success, user_id ]`, `[ interface, request_ip, success, created_at ]`, `[ interface, request_ip, user_id, created_at ]`, `[ interface, request_method, success, user_id ]`, `[ interface, request_method, success, created_at ]`, `[ interface, request_method, user_id, created_at ]`, `[ interface, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success ]`, `[ request_id, request_ip, request_method, user_id ]`, `[ request_id, request_ip, request_method, created_at ]`, `[ request_id, request_ip, success, user_id ]`, `[ request_id, request_ip, success, created_at ]`, `[ request_id, request_ip, user_id, created_at ]`, `[ request_id, request_method, success, user_id ]`, `[ request_id, request_method, success, created_at ]`, `[ request_id, request_method, user_id, created_at ]`, `[ request_id, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id ]`, `[ request_ip, request_method, success, created_at ]`, `[ request_ip, request_method, user_id, created_at ]`, `[ request_ip, success, user_id, created_at ]`, `[ request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method ]`, `[ api_key_id, interface, request_id, request_ip, success ]`, `[ api_key_id, interface, request_id, request_ip, user_id ]`, `[ api_key_id, interface, request_id, request_ip, created_at ]`, `[ api_key_id, interface, request_id, request_method, success ]`, `[ api_key_id, interface, request_id, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_method, created_at ]`, `[ api_key_id, interface, request_id, success, user_id ]`, `[ api_key_id, interface, request_id, success, created_at ]`, `[ api_key_id, interface, request_id, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success ]`, `[ api_key_id, interface, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id ]`, `[ api_key_id, interface, request_ip, success, created_at ]`, `[ api_key_id, interface, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id ]`, `[ api_key_id, interface, request_method, success, created_at ]`, `[ api_key_id, interface, request_method, user_id, created_at ]`, `[ api_key_id, interface, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success ]`, `[ api_key_id, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id ]`, `[ api_key_id, request_id, request_ip, success, created_at ]`, `[ api_key_id, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id ]`, `[ api_key_id, request_id, request_method, success, created_at ]`, `[ api_key_id, request_id, request_method, user_id, created_at ]`, `[ api_key_id, request_id, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success ]`, `[ interface, request_id, request_ip, request_method, user_id ]`, `[ interface, request_id, request_ip, request_method, created_at ]`, `[ interface, request_id, request_ip, success, user_id ]`, `[ interface, request_id, request_ip, success, created_at ]`, `[ interface, request_id, request_ip, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id ]`, `[ interface, request_id, request_method, success, created_at ]`, `[ interface, request_id, request_method, user_id, created_at ]`, `[ interface, request_id, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id ]`, `[ interface, request_ip, request_method, success, created_at ]`, `[ interface, request_ip, request_method, user_id, created_at ]`, `[ interface, request_ip, success, user_id, created_at ]`, `[ interface, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id ]`, `[ request_id, request_ip, request_method, success, created_at ]`, `[ request_id, request_ip, request_method, user_id, created_at ]`, `[ request_id, request_ip, success, user_id, created_at ]`, `[ request_id, request_method, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id ]`, `[ interface, request_id, request_ip, request_method, success, created_at ]`, `[ interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ interface, request_id, request_ip, success, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id, created_at ]` or `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is less than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ api_key_id ]`, `[ interface ]`, `[ request_id ]`, `[ request_ip ]`, `[ request_method ]`, `[ success ]`, `[ user_id ]`, `[ created_at ]`, `[ api_key_id, interface ]`, `[ api_key_id, request_id ]`, `[ api_key_id, request_ip ]`, `[ api_key_id, request_method ]`, `[ api_key_id, success ]`, `[ api_key_id, user_id ]`, `[ api_key_id, created_at ]`, `[ interface, request_id ]`, `[ interface, request_ip ]`, `[ interface, request_method ]`, `[ interface, success ]`, `[ interface, user_id ]`, `[ interface, created_at ]`, `[ request_id, request_ip ]`, `[ request_id, request_method ]`, `[ request_id, success ]`, `[ request_id, user_id ]`, `[ request_id, created_at ]`, `[ request_ip, request_method ]`, `[ request_ip, success ]`, `[ request_ip, user_id ]`, `[ request_ip, created_at ]`, `[ request_method, success ]`, `[ request_method, user_id ]`, `[ request_method, created_at ]`, `[ success, user_id ]`, `[ success, created_at ]`, `[ user_id, created_at ]`, `[ api_key_id, interface, request_id ]`, `[ api_key_id, interface, request_ip ]`, `[ api_key_id, interface, request_method ]`, `[ api_key_id, interface, success ]`, `[ api_key_id, interface, user_id ]`, `[ api_key_id, interface, created_at ]`, `[ api_key_id, request_id, request_ip ]`, `[ api_key_id, request_id, request_method ]`, `[ api_key_id, request_id, success ]`, `[ api_key_id, request_id, user_id ]`, `[ api_key_id, request_id, created_at ]`, `[ api_key_id, request_ip, request_method ]`, `[ api_key_id, request_ip, success ]`, `[ api_key_id, request_ip, user_id ]`, `[ api_key_id, request_ip, created_at ]`, `[ api_key_id, request_method, success ]`, `[ api_key_id, request_method, user_id ]`, `[ api_key_id, request_method, created_at ]`, `[ api_key_id, success, user_id ]`, `[ api_key_id, success, created_at ]`, `[ api_key_id, user_id, created_at ]`, `[ interface, request_id, request_ip ]`, `[ interface, request_id, request_method ]`, `[ interface, request_id, success ]`, `[ interface, request_id, user_id ]`, `[ interface, request_id, created_at ]`, `[ interface, request_ip, request_method ]`, `[ interface, request_ip, success ]`, `[ interface, request_ip, user_id ]`, `[ interface, request_ip, created_at ]`, `[ interface, request_method, success ]`, `[ interface, request_method, user_id ]`, `[ interface, request_method, created_at ]`, `[ interface, success, user_id ]`, `[ interface, success, created_at ]`, `[ interface, user_id, created_at ]`, `[ request_id, request_ip, request_method ]`, `[ request_id, request_ip, success ]`, `[ request_id, request_ip, user_id ]`, `[ request_id, request_ip, created_at ]`, `[ request_id, request_method, success ]`, `[ request_id, request_method, user_id ]`, `[ request_id, request_method, created_at ]`, `[ request_id, success, user_id ]`, `[ request_id, success, created_at ]`, `[ request_id, user_id, created_at ]`, `[ request_ip, request_method, success ]`, `[ request_ip, request_method, user_id ]`, `[ request_ip, request_method, created_at ]`, `[ request_ip, success, user_id ]`, `[ request_ip, success, created_at ]`, `[ request_ip, user_id, created_at ]`, `[ request_method, success, user_id ]`, `[ request_method, success, created_at ]`, `[ request_method, user_id, created_at ]`, `[ success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip ]`, `[ api_key_id, interface, request_id, request_method ]`, `[ api_key_id, interface, request_id, success ]`, `[ api_key_id, interface, request_id, user_id ]`, `[ api_key_id, interface, request_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method ]`, `[ api_key_id, interface, request_ip, success ]`, `[ api_key_id, interface, request_ip, user_id ]`, `[ api_key_id, interface, request_ip, created_at ]`, `[ api_key_id, interface, request_method, success ]`, `[ api_key_id, interface, request_method, user_id ]`, `[ api_key_id, interface, request_method, created_at ]`, `[ api_key_id, interface, success, user_id ]`, `[ api_key_id, interface, success, created_at ]`, `[ api_key_id, interface, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method ]`, `[ api_key_id, request_id, request_ip, success ]`, `[ api_key_id, request_id, request_ip, user_id ]`, `[ api_key_id, request_id, request_ip, created_at ]`, `[ api_key_id, request_id, request_method, success ]`, `[ api_key_id, request_id, request_method, user_id ]`, `[ api_key_id, request_id, request_method, created_at ]`, `[ api_key_id, request_id, success, user_id ]`, `[ api_key_id, request_id, success, created_at ]`, `[ api_key_id, request_id, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success ]`, `[ api_key_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_ip, success, user_id ]`, `[ api_key_id, request_ip, success, created_at ]`, `[ api_key_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id ]`, `[ api_key_id, request_method, success, created_at ]`, `[ api_key_id, request_method, user_id, created_at ]`, `[ api_key_id, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method ]`, `[ interface, request_id, request_ip, success ]`, `[ interface, request_id, request_ip, user_id ]`, `[ interface, request_id, request_ip, created_at ]`, `[ interface, request_id, request_method, success ]`, `[ interface, request_id, request_method, user_id ]`, `[ interface, request_id, request_method, created_at ]`, `[ interface, request_id, success, user_id ]`, `[ interface, request_id, success, created_at ]`, `[ interface, request_id, user_id, created_at ]`, `[ interface, request_ip, request_method, success ]`, `[ interface, request_ip, request_method, user_id ]`, `[ interface, request_ip, request_method, created_at ]`, `[ interface, request_ip, success, user_id ]`, `[ interface, request_ip, success, created_at ]`, `[ interface, request_ip, user_id, created_at ]`, `[ interface, request_method, success, user_id ]`, `[ interface, request_method, success, created_at ]`, `[ interface, request_method, user_id, created_at ]`, `[ interface, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success ]`, `[ request_id, request_ip, request_method, user_id ]`, `[ request_id, request_ip, request_method, created_at ]`, `[ request_id, request_ip, success, user_id ]`, `[ request_id, request_ip, success, created_at ]`, `[ request_id, request_ip, user_id, created_at ]`, `[ request_id, request_method, success, user_id ]`, `[ request_id, request_method, success, created_at ]`, `[ request_id, request_method, user_id, created_at ]`, `[ request_id, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id ]`, `[ request_ip, request_method, success, created_at ]`, `[ request_ip, request_method, user_id, created_at ]`, `[ request_ip, success, user_id, created_at ]`, `[ request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method ]`, `[ api_key_id, interface, request_id, request_ip, success ]`, `[ api_key_id, interface, request_id, request_ip, user_id ]`, `[ api_key_id, interface, request_id, request_ip, created_at ]`, `[ api_key_id, interface, request_id, request_method, success ]`, `[ api_key_id, interface, request_id, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_method, created_at ]`, `[ api_key_id, interface, request_id, success, user_id ]`, `[ api_key_id, interface, request_id, success, created_at ]`, `[ api_key_id, interface, request_id, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success ]`, `[ api_key_id, interface, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id ]`, `[ api_key_id, interface, request_ip, success, created_at ]`, `[ api_key_id, interface, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id ]`, `[ api_key_id, interface, request_method, success, created_at ]`, `[ api_key_id, interface, request_method, user_id, created_at ]`, `[ api_key_id, interface, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success ]`, `[ api_key_id, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id ]`, `[ api_key_id, request_id, request_ip, success, created_at ]`, `[ api_key_id, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id ]`, `[ api_key_id, request_id, request_method, success, created_at ]`, `[ api_key_id, request_id, request_method, user_id, created_at ]`, `[ api_key_id, request_id, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success ]`, `[ interface, request_id, request_ip, request_method, user_id ]`, `[ interface, request_id, request_ip, request_method, created_at ]`, `[ interface, request_id, request_ip, success, user_id ]`, `[ interface, request_id, request_ip, success, created_at ]`, `[ interface, request_id, request_ip, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id ]`, `[ interface, request_id, request_method, success, created_at ]`, `[ interface, request_id, request_method, user_id, created_at ]`, `[ interface, request_id, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id ]`, `[ interface, request_ip, request_method, success, created_at ]`, `[ interface, request_ip, request_method, user_id, created_at ]`, `[ interface, request_ip, success, user_id, created_at ]`, `[ interface, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id ]`, `[ request_id, request_ip, request_method, success, created_at ]`, `[ request_id, request_ip, request_method, user_id, created_at ]`, `[ request_id, request_ip, success, user_id, created_at ]`, `[ request_id, request_method, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id ]`, `[ interface, request_id, request_ip, request_method, success, created_at ]`, `[ interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ interface, request_id, request_ip, success, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id, created_at ]` or `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is less than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ api_key_id ]`, `[ interface ]`, `[ request_id ]`, `[ request_ip ]`, `[ request_method ]`, `[ success ]`, `[ user_id ]`, `[ created_at ]`, `[ api_key_id, interface ]`, `[ api_key_id, request_id ]`, `[ api_key_id, request_ip ]`, `[ api_key_id, request_method ]`, `[ api_key_id, success ]`, `[ api_key_id, user_id ]`, `[ api_key_id, created_at ]`, `[ interface, request_id ]`, `[ interface, request_ip ]`, `[ interface, request_method ]`, `[ interface, success ]`, `[ interface, user_id ]`, `[ interface, created_at ]`, `[ request_id, request_ip ]`, `[ request_id, request_method ]`, `[ request_id, success ]`, `[ request_id, user_id ]`, `[ request_id, created_at ]`, `[ request_ip, request_method ]`, `[ request_ip, success ]`, `[ request_ip, user_id ]`, `[ request_ip, created_at ]`, `[ request_method, success ]`, `[ request_method, user_id ]`, `[ request_method, created_at ]`, `[ success, user_id ]`, `[ success, created_at ]`, `[ user_id, created_at ]`, `[ api_key_id, interface, request_id ]`, `[ api_key_id, interface, request_ip ]`, `[ api_key_id, interface, request_method ]`, `[ api_key_id, interface, success ]`, `[ api_key_id, interface, user_id ]`, `[ api_key_id, interface, created_at ]`, `[ api_key_id, request_id, request_ip ]`, `[ api_key_id, request_id, request_method ]`, `[ api_key_id, request_id, success ]`, `[ api_key_id, request_id, user_id ]`, `[ api_key_id, request_id, created_at ]`, `[ api_key_id, request_ip, request_method ]`, `[ api_key_id, request_ip, success ]`, `[ api_key_id, request_ip, user_id ]`, `[ api_key_id, request_ip, created_at ]`, `[ api_key_id, request_method, success ]`, `[ api_key_id, request_method, user_id ]`, `[ api_key_id, request_method, created_at ]`, `[ api_key_id, success, user_id ]`, `[ api_key_id, success, created_at ]`, `[ api_key_id, user_id, created_at ]`, `[ interface, request_id, request_ip ]`, `[ interface, request_id, request_method ]`, `[ interface, request_id, success ]`, `[ interface, request_id, user_id ]`, `[ interface, request_id, created_at ]`, `[ interface, request_ip, request_method ]`, `[ interface, request_ip, success ]`, `[ interface, request_ip, user_id ]`, `[ interface, request_ip, created_at ]`, `[ interface, request_method, success ]`, `[ interface, request_method, user_id ]`, `[ interface, request_method, created_at ]`, `[ interface, success, user_id ]`, `[ interface, success, created_at ]`, `[ interface, user_id, created_at ]`, `[ request_id, request_ip, request_method ]`, `[ request_id, request_ip, success ]`, `[ request_id, request_ip, user_id ]`, `[ request_id, request_ip, created_at ]`, `[ request_id, request_method, success ]`, `[ request_id, request_method, user_id ]`, `[ request_id, request_method, created_at ]`, `[ request_id, success, user_id ]`, `[ request_id, success, created_at ]`, `[ request_id, user_id, created_at ]`, `[ request_ip, request_method, success ]`, `[ request_ip, request_method, user_id ]`, `[ request_ip, request_method, created_at ]`, `[ request_ip, success, user_id ]`, `[ request_ip, success, created_at ]`, `[ request_ip, user_id, created_at ]`, `[ request_method, success, user_id ]`, `[ request_method, success, created_at ]`, `[ request_method, user_id, created_at ]`, `[ success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip ]`, `[ api_key_id, interface, request_id, request_method ]`, `[ api_key_id, interface, request_id, success ]`, `[ api_key_id, interface, request_id, user_id ]`, `[ api_key_id, interface, request_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method ]`, `[ api_key_id, interface, request_ip, success ]`, `[ api_key_id, interface, request_ip, user_id ]`, `[ api_key_id, interface, request_ip, created_at ]`, `[ api_key_id, interface, request_method, success ]`, `[ api_key_id, interface, request_method, user_id ]`, `[ api_key_id, interface, request_method, created_at ]`, `[ api_key_id, interface, success, user_id ]`, `[ api_key_id, interface, success, created_at ]`, `[ api_key_id, interface, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method ]`, `[ api_key_id, request_id, request_ip, success ]`, `[ api_key_id, request_id, request_ip, user_id ]`, `[ api_key_id, request_id, request_ip, created_at ]`, `[ api_key_id, request_id, request_method, success ]`, `[ api_key_id, request_id, request_method, user_id ]`, `[ api_key_id, request_id, request_method, created_at ]`, `[ api_key_id, request_id, success, user_id ]`, `[ api_key_id, request_id, success, created_at ]`, `[ api_key_id, request_id, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success ]`, `[ api_key_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_ip, success, user_id ]`, `[ api_key_id, request_ip, success, created_at ]`, `[ api_key_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id ]`, `[ api_key_id, request_method, success, created_at ]`, `[ api_key_id, request_method, user_id, created_at ]`, `[ api_key_id, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method ]`, `[ interface, request_id, request_ip, success ]`, `[ interface, request_id, request_ip, user_id ]`, `[ interface, request_id, request_ip, created_at ]`, `[ interface, request_id, request_method, success ]`, `[ interface, request_id, request_method, user_id ]`, `[ interface, request_id, request_method, created_at ]`, `[ interface, request_id, success, user_id ]`, `[ interface, request_id, success, created_at ]`, `[ interface, request_id, user_id, created_at ]`, `[ interface, request_ip, request_method, success ]`, `[ interface, request_ip, request_method, user_id ]`, `[ interface, request_ip, request_method, created_at ]`, `[ interface, request_ip, success, user_id ]`, `[ interface, request_ip, success, created_at ]`, `[ interface, request_ip, user_id, created_at ]`, `[ interface, request_method, success, user_id ]`, `[ interface, request_method, success, created_at ]`, `[ interface, request_method, user_id, created_at ]`, `[ interface, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success ]`, `[ request_id, request_ip, request_method, user_id ]`, `[ request_id, request_ip, request_method, created_at ]`, `[ request_id, request_ip, success, user_id ]`, `[ request_id, request_ip, success, created_at ]`, `[ request_id, request_ip, user_id, created_at ]`, `[ request_id, request_method, success, user_id ]`, `[ request_id, request_method, success, created_at ]`, `[ request_id, request_method, user_id, created_at ]`, `[ request_id, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id ]`, `[ request_ip, request_method, success, created_at ]`, `[ request_ip, request_method, user_id, created_at ]`, `[ request_ip, success, user_id, created_at ]`, `[ request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method ]`, `[ api_key_id, interface, request_id, request_ip, success ]`, `[ api_key_id, interface, request_id, request_ip, user_id ]`, `[ api_key_id, interface, request_id, request_ip, created_at ]`, `[ api_key_id, interface, request_id, request_method, success ]`, `[ api_key_id, interface, request_id, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_method, created_at ]`, `[ api_key_id, interface, request_id, success, user_id ]`, `[ api_key_id, interface, request_id, success, created_at ]`, `[ api_key_id, interface, request_id, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success ]`, `[ api_key_id, interface, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id ]`, `[ api_key_id, interface, request_ip, success, created_at ]`, `[ api_key_id, interface, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id ]`, `[ api_key_id, interface, request_method, success, created_at ]`, `[ api_key_id, interface, request_method, user_id, created_at ]`, `[ api_key_id, interface, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success ]`, `[ api_key_id, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id ]`, `[ api_key_id, request_id, request_ip, success, created_at ]`, `[ api_key_id, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id ]`, `[ api_key_id, request_id, request_method, success, created_at ]`, `[ api_key_id, request_id, request_method, user_id, created_at ]`, `[ api_key_id, request_id, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success ]`, `[ interface, request_id, request_ip, request_method, user_id ]`, `[ interface, request_id, request_ip, request_method, created_at ]`, `[ interface, request_id, request_ip, success, user_id ]`, `[ interface, request_id, request_ip, success, created_at ]`, `[ interface, request_id, request_ip, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id ]`, `[ interface, request_id, request_method, success, created_at ]`, `[ interface, request_id, request_method, user_id, created_at ]`, `[ interface, request_id, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id ]`, `[ interface, request_ip, request_method, success, created_at ]`, `[ interface, request_ip, request_method, user_id, created_at ]`, `[ interface, request_ip, success, user_id, created_at ]`, `[ interface, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id ]`, `[ request_id, request_ip, request_method, success, created_at ]`, `[ request_id, request_ip, request_method, user_id, created_at ]`, `[ request_id, request_ip, success, user_id, created_at ]`, `[ request_id, request_method, success, user_id, created_at ]`, `[ request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id ]`, `[ interface, request_id, request_ip, request_method, success, created_at ]`, `[ interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ interface, request_id, request_ip, success, user_id, created_at ]`, `[ interface, request_id, request_method, success, user_id, created_at ]`, `[ interface, request_ip, request_method, success, user_id, created_at ]`, `[ request_id, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id ]`, `[ api_key_id, interface, request_id, request_ip, request_method, success, created_at ]`, `[ api_key_id, interface, request_id, request_ip, request_method, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_ip, success, user_id, created_at ]`, `[ api_key_id, interface, request_id, request_method, success, user_id, created_at ]`, `[ api_key_id, interface, request_ip, request_method, success, user_id, created_at ]`, `[ api_key_id, request_id, request_ip, request_method, success, user_id, created_at ]`, `[ interface, request_id, request_ip, request_method, success, user_id, created_at ]` or `[ api_key_id, interface, request_id, request_ip, request_method, success, user_id, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

### FilesCom.PowerShell.Models.ApiRequestLog

The api request log as Files.com returned it.

### FilesCom.PowerShell.FilesPage`1

With -Page or -Continuation, one page of results and the continuation for the next.

## NOTES

Failures are written as error records with IDs such as Files.NotFound or Files.Authentication. See about_FilesCom for connections, paging, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
