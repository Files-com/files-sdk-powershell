---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Wait-FilesFileAction
---

# Wait-FilesFileAction

## SYNOPSIS

Waits until Files.com has finished a copy or move, or until nothing is left at a path after a delete.

## SYNTAX

### Action (Default)

```
Wait-FilesFileAction -InputObject <FileAction> [-PollIntervalSeconds <double>] [-Timeout <timespan>]
 [-Connection <FilesConnection>] [-WorkspaceScope <long>] [-NoWorkspaceScope]
```

### Removed

```
Wait-FilesFileAction [-Path] <string> -Removed [-PollIntervalSeconds <double>] [-Timeout <timespan>]
 [-Connection <FilesConnection>] [-WorkspaceScope <long>] [-NoWorkspaceScope]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Copy-FilesFile, Move-FilesFile and Remove-FilesFile return once Files.com accepts the request, which it may finish in the background. Wait-FilesFileAction waits until it has: for an action from Copy-FilesFile or Move-FilesFile, until its migration is completed, failed or cancelled, and with -Path and -Removed, until Files.com answers that nothing is at the path. It checks every -PollIntervalSeconds, one request each time, retried as the connection's requests are.

An action is checked with the connection, the address and the workspace its command used, so an action from a command that moved to its site's region is checked there. A failed or cancelled migration is Files.FileActionFailed, with the migration's ID and status; what it did before then stays done. Stopping the command, or -Timeout, stops only the wait: the action goes on at Files.com.

## EXAMPLES

### Example 1: Copy a folder and wait for the copy to finish

```powershell
Copy-FilesFile -Path projects/2025 -Destination archive/2025 | Wait-FilesFileAction
```

Writes the completed migration, with the number of files it processed.

### Example 2: Delete a folder, wait until it is gone, then upload again

```powershell
Remove-FilesFile -Path staging -Recursive $true -Confirm:$false
Wait-FilesFileAction -Path staging -Removed -Timeout (New-TimeSpan -Minutes 10)
Send-FilesDirectory ./staging -RemotePath staging
```

Starts the new upload only once Files.com has finished deleting the old folder.

## PARAMETERS

### -Connection

The connection to use, from Connect-FilesAccount. Defaults to the connection of the -InputObject given, and otherwise to the current runspace's default connection.

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

### -InputObject

A copy or move written by Copy-FilesFile or Move-FilesFile. It is checked with the connection, the address and the workspace its command used.

```yaml
Type: FilesCom.PowerShell.Models.FileAction
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Action
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
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

The remote path to wait on, such as a folder deleted with its contents, which Files.com can finish after it answers.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Removed
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PollIntervalSeconds

How long to wait between checks, in seconds; by default 1. Each check is one request.

```yaml
Type: System.Double
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

### -Removed

Waits until Files.com has nothing at -Path.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Removed
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Timeout

The longest to wait. The command then fails with Files.OperationTimeout, and the action goes on at Files.com.

```yaml
Type: System.TimeSpan
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

### FilesCom.PowerShell.Models.FileAction

Actions from Copy-FilesFile and Move-FilesFile, as -InputObject.

## OUTPUTS

### FilesCom.PowerShell.Models.FileMigration

For an action with a migration, the completed migration. Nothing for an action Files.com had completed when it answered, or with -Removed.

## NOTES

A delete Files.com finishes before it answers needs no wait: -Removed then returns after one check. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
