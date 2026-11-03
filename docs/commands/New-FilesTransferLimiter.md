---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: New-FilesTransferLimiter
---

# New-FilesTransferLimiter

## SYNOPSIS

Creates a budget of concurrent requests and buffered bytes for uploads to share.

## SYNTAX

### __AllParameterSets

```
New-FilesTransferLimiter [-MaxConcurrentRequests <int>] [-MaxBufferedBytes <long>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

New-FilesTransferLimiter creates a limiter that Send-FilesFile commands share through their -Limiter parameter. Uploads using the same limiter, including ones in other runspaces or with other connections, take turns within its budget of concurrent part requests and bytes held in memory, which limits the part requests they have in flight at once and the file bytes they hold in memory between them. Creating a limiter sends nothing to Files.com.

## EXAMPLES

### Example 1: Share one budget across parallel uploads (PowerShell 7)

```powershell
$limiter = New-FilesTransferLimiter -MaxConcurrentRequests 8 -MaxBufferedBytes 256MB
$connection = Get-FilesConnection
Get-ChildItem ./exports -File | ForEach-Object -Parallel {
    Send-FilesFile -LiteralPath $_.FullName -RemotePath "exports/$($_.Name)" -Connection $using:connection -Limiter $using:limiter
} -ThrottleLimit 4
```

Uploads four files at a time while all parts together stay within 8 requests and 256 MB.

### Example 2: Bound one upload pipeline

```powershell
$limiter = New-FilesTransferLimiter -MaxConcurrentRequests 4 -MaxBufferedBytes 64MB
Get-ChildItem ./exports -File | Send-FilesFile -RemotePath { "exports/$($_.Name)" } -Limiter $limiter
```

Uploads the files one after another, each sending its parts in parallel within 4 requests and 64 MB.

## PARAMETERS

### -MaxBufferedBytes

The most bytes of file contents the sharing uploads hold in memory at the same time. Defaults to the engine's default, 128 MiB. An upload from a stream whose parts are kept in memory holds whole parts here, so it also limits how large such a part can be; a -Spool copy takes its buffer from here too.

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

### -MaxConcurrentRequests

The most part requests the sharing uploads send at the same time. Defaults to the engine's default, 8.

```yaml
Type: System.Int32
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

### FilesCom.PowerShell.FilesTransferLimiter

The new limiter.

## NOTES

A limiter holds no credentials or connections: each upload sends its requests with its own connection. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()

