---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Disconnect-FilesAccount
---

# Disconnect-FilesAccount

## SYNOPSIS

Closes a connection, revoking the session it owns.

## SYNTAX

### __AllParameterSets

```
Disconnect-FilesAccount [[-Connection] <FilesConnection>] [-KeepSession] [-PassThru] [-WhatIf]
 [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Disconnect-FilesAccount closes a connection. Operations still running with it are cancelled, and commands given it afterward fail with Files.ConnectionClosed. The command returns once the engine has finished with what those operations were using, such as their requests, responses and temporary files, and has closed the network connections it made for the connection. It does not wait for each running command to end: each command ends on its own, usually with Files.Cancelled, though one whose response had already arrived can still succeed, and an upload whose final request had been sent may have completed the file. A stream a command borrowed stays open and its caller's. Stopping Disconnect-FilesAccount, as with Ctrl+C, stops only its wait. A -HttpMessageHandler the connection borrowed is never disposed. Objects fetched with it stay usable as data.

A connection made by signing in with -Credential owns its session, and Disconnect-FilesAccount revokes that session on Files.com before closing the connection. If revoking fails, the connection is still closed locally and the command writes a separate error saying the session may remain valid until it expires. API keys and session IDs you supplied are never revoked.

Removing the FilesCom.PowerShell module closes the connections of the runspace that removes it, locally, without revoking any session.

## EXAMPLES

### Example 1: Close the default connection

```powershell
Disconnect-FilesAccount
```

Closes the runspace's default connection and revokes its session if the connection signed in.

### Example 2: Close a connection but keep its session

```powershell
Disconnect-FilesAccount -Connection $connection -KeepSession
```

Closes the connection locally; the session it created stays valid until it expires.

### Example 3: Preview disconnecting

```powershell
Get-FilesConnection -All | Disconnect-FilesAccount -WhatIf
```

Describes which connections would be closed and which sessions revoked.

## PARAMETERS

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

### -Connection

The connection to close. Defaults to the current runspace's default connection.

```yaml
Type: FilesCom.PowerShell.FilesConnection
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 0
  IsRequired: false
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -KeepSession

Closes the connection without revoking a session it created, which then stays valid until it expires.

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

### -PassThru

Writes the closed connection.

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

### -WhatIf

Runs the command in a mode that only reports what would happen without performing the actions.

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

### FilesCom.PowerShell.FilesConnection

A connection to close.

## OUTPUTS

### None

Nothing, unless -PassThru is given.

### FilesCom.PowerShell.FilesConnection

With -PassThru, the closed connection.

## NOTES

Revoking an owned session is a request to Files.com and supports -WhatIf and -Confirm like any other change. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()

