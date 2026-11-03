---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Get-FilesOutboundConnectionLog
---

# Get-FilesOutboundConnectionLog

## SYNOPSIS

List Outbound Connection Logs.

## SYNTAX

### List (Default)

```
Get-FilesOutboundConnectionLog [<parameters of the List set>]
```

### Page

```
Get-FilesOutboundConnectionLog [<parameters of the Page set>]
```

### Continuation

```
Get-FilesOutboundConnectionLog [<parameters of the Continuation set>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

List Outbound Connection Logs.

Without -Page or -Continuation, the command writes each outbound connection log of every page, loading one page at a time as the pipeline consumes them. -First and -Skip bound what it writes, and stopping the pipeline early, as Select-Object -First does, loads no further page. A failure on any page stops the command, so a partial listing is never taken for a complete one.

## EXAMPLES

### Example 1: List all outbound connection logs

```powershell
Get-FilesOutboundConnectionLog
```

Writes every outbound connection log, loading one page at a time as the pipeline consumes them.

### Example 2: Get one page at a time

```powershell
$page = Get-FilesOutboundConnectionLog -Page
$next = Get-FilesOutboundConnectionLog -Continuation $page.Continuation
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

If set, return records where the specified field is equal to the supplied value. Valid fields are `operation`, `status`, `src_remote_server_id`, `dest_remote_server_id`, `path`, `client_ip` or `created_at`. Valid field combinations are `[ operation ]`, `[ status ]`, `[ src_remote_server_id ]`, `[ dest_remote_server_id ]`, `[ path ]`, `[ client_ip ]`, `[ created_at ]`, `[ operation, status ]`, `[ operation, src_remote_server_id ]`, `[ operation, dest_remote_server_id ]`, `[ operation, path ]`, `[ operation, client_ip ]`, `[ operation, created_at ]`, `[ status, src_remote_server_id ]`, `[ status, dest_remote_server_id ]`, `[ status, path ]`, `[ status, client_ip ]`, `[ status, created_at ]`, `[ src_remote_server_id, dest_remote_server_id ]`, `[ src_remote_server_id, path ]`, `[ src_remote_server_id, client_ip ]`, `[ src_remote_server_id, created_at ]`, `[ dest_remote_server_id, path ]`, `[ dest_remote_server_id, client_ip ]`, `[ dest_remote_server_id, created_at ]`, `[ path, client_ip ]`, `[ path, created_at ]`, `[ client_ip, created_at ]`, `[ operation, status, src_remote_server_id ]`, `[ operation, status, dest_remote_server_id ]`, `[ operation, status, path ]`, `[ operation, status, client_ip ]`, `[ operation, status, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id ]`, `[ operation, src_remote_server_id, path ]`, `[ operation, src_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, created_at ]`, `[ operation, dest_remote_server_id, path ]`, `[ operation, dest_remote_server_id, client_ip ]`, `[ operation, dest_remote_server_id, created_at ]`, `[ operation, path, client_ip ]`, `[ operation, path, created_at ]`, `[ operation, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id ]`, `[ status, src_remote_server_id, path ]`, `[ status, src_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, created_at ]`, `[ status, dest_remote_server_id, path ]`, `[ status, dest_remote_server_id, client_ip ]`, `[ status, dest_remote_server_id, created_at ]`, `[ status, path, client_ip ]`, `[ status, path, created_at ]`, `[ status, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, created_at ]`, `[ src_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, path, created_at ]`, `[ src_remote_server_id, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip ]`, `[ dest_remote_server_id, path, created_at ]`, `[ dest_remote_server_id, client_ip, created_at ]`, `[ path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id ]`, `[ operation, status, src_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, created_at ]`, `[ operation, status, dest_remote_server_id, path ]`, `[ operation, status, dest_remote_server_id, client_ip ]`, `[ operation, status, dest_remote_server_id, created_at ]`, `[ operation, status, path, client_ip ]`, `[ operation, status, path, created_at ]`, `[ operation, status, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, src_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip ]`, `[ operation, dest_remote_server_id, path, created_at ]`, `[ operation, dest_remote_server_id, client_ip, created_at ]`, `[ operation, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ status, src_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip ]`, `[ status, dest_remote_server_id, path, created_at ]`, `[ status, dest_remote_server_id, client_ip, created_at ]`, `[ status, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ src_remote_server_id, path, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip ]`, `[ operation, status, dest_remote_server_id, path, created_at ]`, `[ operation, status, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ status, src_remote_server_id, path, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]` or `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is greater than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ operation ]`, `[ status ]`, `[ src_remote_server_id ]`, `[ dest_remote_server_id ]`, `[ path ]`, `[ client_ip ]`, `[ created_at ]`, `[ operation, status ]`, `[ operation, src_remote_server_id ]`, `[ operation, dest_remote_server_id ]`, `[ operation, path ]`, `[ operation, client_ip ]`, `[ operation, created_at ]`, `[ status, src_remote_server_id ]`, `[ status, dest_remote_server_id ]`, `[ status, path ]`, `[ status, client_ip ]`, `[ status, created_at ]`, `[ src_remote_server_id, dest_remote_server_id ]`, `[ src_remote_server_id, path ]`, `[ src_remote_server_id, client_ip ]`, `[ src_remote_server_id, created_at ]`, `[ dest_remote_server_id, path ]`, `[ dest_remote_server_id, client_ip ]`, `[ dest_remote_server_id, created_at ]`, `[ path, client_ip ]`, `[ path, created_at ]`, `[ client_ip, created_at ]`, `[ operation, status, src_remote_server_id ]`, `[ operation, status, dest_remote_server_id ]`, `[ operation, status, path ]`, `[ operation, status, client_ip ]`, `[ operation, status, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id ]`, `[ operation, src_remote_server_id, path ]`, `[ operation, src_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, created_at ]`, `[ operation, dest_remote_server_id, path ]`, `[ operation, dest_remote_server_id, client_ip ]`, `[ operation, dest_remote_server_id, created_at ]`, `[ operation, path, client_ip ]`, `[ operation, path, created_at ]`, `[ operation, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id ]`, `[ status, src_remote_server_id, path ]`, `[ status, src_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, created_at ]`, `[ status, dest_remote_server_id, path ]`, `[ status, dest_remote_server_id, client_ip ]`, `[ status, dest_remote_server_id, created_at ]`, `[ status, path, client_ip ]`, `[ status, path, created_at ]`, `[ status, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, created_at ]`, `[ src_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, path, created_at ]`, `[ src_remote_server_id, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip ]`, `[ dest_remote_server_id, path, created_at ]`, `[ dest_remote_server_id, client_ip, created_at ]`, `[ path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id ]`, `[ operation, status, src_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, created_at ]`, `[ operation, status, dest_remote_server_id, path ]`, `[ operation, status, dest_remote_server_id, client_ip ]`, `[ operation, status, dest_remote_server_id, created_at ]`, `[ operation, status, path, client_ip ]`, `[ operation, status, path, created_at ]`, `[ operation, status, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, src_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip ]`, `[ operation, dest_remote_server_id, path, created_at ]`, `[ operation, dest_remote_server_id, client_ip, created_at ]`, `[ operation, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ status, src_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip ]`, `[ status, dest_remote_server_id, path, created_at ]`, `[ status, dest_remote_server_id, client_ip, created_at ]`, `[ status, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ src_remote_server_id, path, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip ]`, `[ operation, status, dest_remote_server_id, path, created_at ]`, `[ operation, status, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ status, src_remote_server_id, path, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]` or `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is greater than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ operation ]`, `[ status ]`, `[ src_remote_server_id ]`, `[ dest_remote_server_id ]`, `[ path ]`, `[ client_ip ]`, `[ created_at ]`, `[ operation, status ]`, `[ operation, src_remote_server_id ]`, `[ operation, dest_remote_server_id ]`, `[ operation, path ]`, `[ operation, client_ip ]`, `[ operation, created_at ]`, `[ status, src_remote_server_id ]`, `[ status, dest_remote_server_id ]`, `[ status, path ]`, `[ status, client_ip ]`, `[ status, created_at ]`, `[ src_remote_server_id, dest_remote_server_id ]`, `[ src_remote_server_id, path ]`, `[ src_remote_server_id, client_ip ]`, `[ src_remote_server_id, created_at ]`, `[ dest_remote_server_id, path ]`, `[ dest_remote_server_id, client_ip ]`, `[ dest_remote_server_id, created_at ]`, `[ path, client_ip ]`, `[ path, created_at ]`, `[ client_ip, created_at ]`, `[ operation, status, src_remote_server_id ]`, `[ operation, status, dest_remote_server_id ]`, `[ operation, status, path ]`, `[ operation, status, client_ip ]`, `[ operation, status, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id ]`, `[ operation, src_remote_server_id, path ]`, `[ operation, src_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, created_at ]`, `[ operation, dest_remote_server_id, path ]`, `[ operation, dest_remote_server_id, client_ip ]`, `[ operation, dest_remote_server_id, created_at ]`, `[ operation, path, client_ip ]`, `[ operation, path, created_at ]`, `[ operation, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id ]`, `[ status, src_remote_server_id, path ]`, `[ status, src_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, created_at ]`, `[ status, dest_remote_server_id, path ]`, `[ status, dest_remote_server_id, client_ip ]`, `[ status, dest_remote_server_id, created_at ]`, `[ status, path, client_ip ]`, `[ status, path, created_at ]`, `[ status, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, created_at ]`, `[ src_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, path, created_at ]`, `[ src_remote_server_id, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip ]`, `[ dest_remote_server_id, path, created_at ]`, `[ dest_remote_server_id, client_ip, created_at ]`, `[ path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id ]`, `[ operation, status, src_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, created_at ]`, `[ operation, status, dest_remote_server_id, path ]`, `[ operation, status, dest_remote_server_id, client_ip ]`, `[ operation, status, dest_remote_server_id, created_at ]`, `[ operation, status, path, client_ip ]`, `[ operation, status, path, created_at ]`, `[ operation, status, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, src_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip ]`, `[ operation, dest_remote_server_id, path, created_at ]`, `[ operation, dest_remote_server_id, client_ip, created_at ]`, `[ operation, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ status, src_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip ]`, `[ status, dest_remote_server_id, path, created_at ]`, `[ status, dest_remote_server_id, client_ip, created_at ]`, `[ status, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ src_remote_server_id, path, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip ]`, `[ operation, status, dest_remote_server_id, path, created_at ]`, `[ operation, status, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ status, src_remote_server_id, path, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]` or `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is prefixed by the supplied value. Valid fields are `path`. Valid field combinations are `[ operation ]`, `[ status ]`, `[ src_remote_server_id ]`, `[ dest_remote_server_id ]`, `[ path ]`, `[ client_ip ]`, `[ created_at ]`, `[ operation, status ]`, `[ operation, src_remote_server_id ]`, `[ operation, dest_remote_server_id ]`, `[ operation, path ]`, `[ operation, client_ip ]`, `[ operation, created_at ]`, `[ status, src_remote_server_id ]`, `[ status, dest_remote_server_id ]`, `[ status, path ]`, `[ status, client_ip ]`, `[ status, created_at ]`, `[ src_remote_server_id, dest_remote_server_id ]`, `[ src_remote_server_id, path ]`, `[ src_remote_server_id, client_ip ]`, `[ src_remote_server_id, created_at ]`, `[ dest_remote_server_id, path ]`, `[ dest_remote_server_id, client_ip ]`, `[ dest_remote_server_id, created_at ]`, `[ path, client_ip ]`, `[ path, created_at ]`, `[ client_ip, created_at ]`, `[ operation, status, src_remote_server_id ]`, `[ operation, status, dest_remote_server_id ]`, `[ operation, status, path ]`, `[ operation, status, client_ip ]`, `[ operation, status, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id ]`, `[ operation, src_remote_server_id, path ]`, `[ operation, src_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, created_at ]`, `[ operation, dest_remote_server_id, path ]`, `[ operation, dest_remote_server_id, client_ip ]`, `[ operation, dest_remote_server_id, created_at ]`, `[ operation, path, client_ip ]`, `[ operation, path, created_at ]`, `[ operation, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id ]`, `[ status, src_remote_server_id, path ]`, `[ status, src_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, created_at ]`, `[ status, dest_remote_server_id, path ]`, `[ status, dest_remote_server_id, client_ip ]`, `[ status, dest_remote_server_id, created_at ]`, `[ status, path, client_ip ]`, `[ status, path, created_at ]`, `[ status, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, created_at ]`, `[ src_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, path, created_at ]`, `[ src_remote_server_id, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip ]`, `[ dest_remote_server_id, path, created_at ]`, `[ dest_remote_server_id, client_ip, created_at ]`, `[ path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id ]`, `[ operation, status, src_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, created_at ]`, `[ operation, status, dest_remote_server_id, path ]`, `[ operation, status, dest_remote_server_id, client_ip ]`, `[ operation, status, dest_remote_server_id, created_at ]`, `[ operation, status, path, client_ip ]`, `[ operation, status, path, created_at ]`, `[ operation, status, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, src_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip ]`, `[ operation, dest_remote_server_id, path, created_at ]`, `[ operation, dest_remote_server_id, client_ip, created_at ]`, `[ operation, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ status, src_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip ]`, `[ status, dest_remote_server_id, path, created_at ]`, `[ status, dest_remote_server_id, client_ip, created_at ]`, `[ status, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ src_remote_server_id, path, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip ]`, `[ operation, status, dest_remote_server_id, path, created_at ]`, `[ operation, status, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ status, src_remote_server_id, path, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]` or `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is less than the supplied value. Valid fields are `created_at`. Valid field combinations are `[ operation ]`, `[ status ]`, `[ src_remote_server_id ]`, `[ dest_remote_server_id ]`, `[ path ]`, `[ client_ip ]`, `[ created_at ]`, `[ operation, status ]`, `[ operation, src_remote_server_id ]`, `[ operation, dest_remote_server_id ]`, `[ operation, path ]`, `[ operation, client_ip ]`, `[ operation, created_at ]`, `[ status, src_remote_server_id ]`, `[ status, dest_remote_server_id ]`, `[ status, path ]`, `[ status, client_ip ]`, `[ status, created_at ]`, `[ src_remote_server_id, dest_remote_server_id ]`, `[ src_remote_server_id, path ]`, `[ src_remote_server_id, client_ip ]`, `[ src_remote_server_id, created_at ]`, `[ dest_remote_server_id, path ]`, `[ dest_remote_server_id, client_ip ]`, `[ dest_remote_server_id, created_at ]`, `[ path, client_ip ]`, `[ path, created_at ]`, `[ client_ip, created_at ]`, `[ operation, status, src_remote_server_id ]`, `[ operation, status, dest_remote_server_id ]`, `[ operation, status, path ]`, `[ operation, status, client_ip ]`, `[ operation, status, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id ]`, `[ operation, src_remote_server_id, path ]`, `[ operation, src_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, created_at ]`, `[ operation, dest_remote_server_id, path ]`, `[ operation, dest_remote_server_id, client_ip ]`, `[ operation, dest_remote_server_id, created_at ]`, `[ operation, path, client_ip ]`, `[ operation, path, created_at ]`, `[ operation, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id ]`, `[ status, src_remote_server_id, path ]`, `[ status, src_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, created_at ]`, `[ status, dest_remote_server_id, path ]`, `[ status, dest_remote_server_id, client_ip ]`, `[ status, dest_remote_server_id, created_at ]`, `[ status, path, client_ip ]`, `[ status, path, created_at ]`, `[ status, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, created_at ]`, `[ src_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, path, created_at ]`, `[ src_remote_server_id, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip ]`, `[ dest_remote_server_id, path, created_at ]`, `[ dest_remote_server_id, client_ip, created_at ]`, `[ path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id ]`, `[ operation, status, src_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, created_at ]`, `[ operation, status, dest_remote_server_id, path ]`, `[ operation, status, dest_remote_server_id, client_ip ]`, `[ operation, status, dest_remote_server_id, created_at ]`, `[ operation, status, path, client_ip ]`, `[ operation, status, path, created_at ]`, `[ operation, status, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, src_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip ]`, `[ operation, dest_remote_server_id, path, created_at ]`, `[ operation, dest_remote_server_id, client_ip, created_at ]`, `[ operation, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ status, src_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip ]`, `[ status, dest_remote_server_id, path, created_at ]`, `[ status, dest_remote_server_id, client_ip, created_at ]`, `[ status, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ src_remote_server_id, path, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip ]`, `[ operation, status, dest_remote_server_id, path, created_at ]`, `[ operation, status, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ status, src_remote_server_id, path, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]` or `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

If set, return records where the specified field is less than or equal the supplied value. Valid fields are `created_at`. Valid field combinations are `[ operation ]`, `[ status ]`, `[ src_remote_server_id ]`, `[ dest_remote_server_id ]`, `[ path ]`, `[ client_ip ]`, `[ created_at ]`, `[ operation, status ]`, `[ operation, src_remote_server_id ]`, `[ operation, dest_remote_server_id ]`, `[ operation, path ]`, `[ operation, client_ip ]`, `[ operation, created_at ]`, `[ status, src_remote_server_id ]`, `[ status, dest_remote_server_id ]`, `[ status, path ]`, `[ status, client_ip ]`, `[ status, created_at ]`, `[ src_remote_server_id, dest_remote_server_id ]`, `[ src_remote_server_id, path ]`, `[ src_remote_server_id, client_ip ]`, `[ src_remote_server_id, created_at ]`, `[ dest_remote_server_id, path ]`, `[ dest_remote_server_id, client_ip ]`, `[ dest_remote_server_id, created_at ]`, `[ path, client_ip ]`, `[ path, created_at ]`, `[ client_ip, created_at ]`, `[ operation, status, src_remote_server_id ]`, `[ operation, status, dest_remote_server_id ]`, `[ operation, status, path ]`, `[ operation, status, client_ip ]`, `[ operation, status, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id ]`, `[ operation, src_remote_server_id, path ]`, `[ operation, src_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, created_at ]`, `[ operation, dest_remote_server_id, path ]`, `[ operation, dest_remote_server_id, client_ip ]`, `[ operation, dest_remote_server_id, created_at ]`, `[ operation, path, client_ip ]`, `[ operation, path, created_at ]`, `[ operation, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id ]`, `[ status, src_remote_server_id, path ]`, `[ status, src_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, created_at ]`, `[ status, dest_remote_server_id, path ]`, `[ status, dest_remote_server_id, client_ip ]`, `[ status, dest_remote_server_id, created_at ]`, `[ status, path, client_ip ]`, `[ status, path, created_at ]`, `[ status, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, created_at ]`, `[ src_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, path, created_at ]`, `[ src_remote_server_id, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip ]`, `[ dest_remote_server_id, path, created_at ]`, `[ dest_remote_server_id, client_ip, created_at ]`, `[ path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id ]`, `[ operation, status, src_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, created_at ]`, `[ operation, status, dest_remote_server_id, path ]`, `[ operation, status, dest_remote_server_id, client_ip ]`, `[ operation, status, dest_remote_server_id, created_at ]`, `[ operation, status, path, client_ip ]`, `[ operation, status, path, created_at ]`, `[ operation, status, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, src_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip ]`, `[ operation, dest_remote_server_id, path, created_at ]`, `[ operation, dest_remote_server_id, client_ip, created_at ]`, `[ operation, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ status, src_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip ]`, `[ status, dest_remote_server_id, path, created_at ]`, `[ status, dest_remote_server_id, client_ip, created_at ]`, `[ status, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ src_remote_server_id, path, client_ip, created_at ]`, `[ dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip ]`, `[ operation, status, dest_remote_server_id, path, created_at ]`, `[ operation, status, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ status, src_remote_server_id, path, client_ip, created_at ]`, `[ status, dest_remote_server_id, path, client_ip, created_at ]`, `[ src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, path, created_at ]`, `[ operation, status, src_remote_server_id, dest_remote_server_id, client_ip, created_at ]`, `[ operation, status, src_remote_server_id, path, client_ip, created_at ]`, `[ operation, status, dest_remote_server_id, path, client_ip, created_at ]`, `[ operation, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`, `[ status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]` or `[ operation, status, src_remote_server_id, dest_remote_server_id, path, client_ip, created_at ]`. Give a hashtable or PSCustomObject; keys keep their case and values their types.

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

### FilesCom.PowerShell.Models.OutboundConnectionLog

The outbound connection log as Files.com returned it.

### FilesCom.PowerShell.FilesPage`1

With -Page or -Continuation, one page of results and the continuation for the next.

## NOTES

Failures are written as error records with IDs such as Files.NotFound or Files.Authentication. See about_FilesCom for connections, paging, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
