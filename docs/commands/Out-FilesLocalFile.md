---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Out-FilesLocalFile
---

# Out-FilesLocalFile

## SYNOPSIS

Writes lines of text to a local file in one complete step, so a failed or stopped pipeline never leaves a partial file.

## SYNTAX

### __AllParameterSets

```
Out-FilesLocalFile [-LiteralPath] <string> -InputObject <string> [-Force] [-PassThru] [-WhatIf]
 [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Out-FilesLocalFile writes each input string as one line of a local file, in UTF-8 without a byte order mark, such as a CSV report built from Files.com objects with ConvertTo-Csv. The lines go to a hidden stage that the command creates and owns in the file's folder, and the file appears at its path only once every line has arrived. If a command earlier in the pipeline fails, for example because a page of a list could not be read, or the pipeline is stopped, the command removes its stage and nothing else: a file already at the path stays exactly as it was, and no partial file is left behind.

An existing file is replaced only with -Force. The command first checks that the existing entry is a regular file, refusing a folder, a symbolic link or anything else, then replaces that entry with the finished file in one step: the replacement never writes through a symbolic link, never replaces a folder, and only ever publishes the file the command wrote. The check and the replacement cannot be one step, so another process allowed to change the folder could put a different entry, such as a symbolic link, at that name in between; the replacement then replaces that entry itself, never what it points to. The command publishes the stage it wrote even if another process renames or replaces files in the folder meanwhile, and removes only what it created.

A file that replaces another gives no one access the replaced file did not, other than you as its new owner. It takes that file's group and access control list, and its permissions when the file is yours; when it is another user's file, its group and other permissions, with the owner permissions a new file gets. If the command cannot give the new file that group or access control list, for example because you are not a member of the group, it replaces nothing and fails, and the existing file stays exactly as it was. On Windows the new file takes the replaced file's access control list, and a hidden file it replaces stays hidden. A new file gets the group and permissions any new file in the folder gets, and is never hidden.

## EXAMPLES

### Example 1: Export every user to CSV, all or nothing

```powershell
Get-FilesUser | Select-Object Id, Username, Email | ConvertTo-Csv -NoTypeInformation | Out-FilesLocalFile -LiteralPath ./users.csv -Force
```

Writes users.csv only if every page of users was read; a failure leaves the previous users.csv as it was.

## PARAMETERS

### -Force

Replaces a regular file that already exists at the path, giving the new file that file's group, access control list and permissions as described above. Without it, an existing file makes the command fail before it reads any input. Another process that changes the folder between the command's check and its replacement can have a different entry at that name replaced, never followed.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: False
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

A line of text to write. Each input string becomes one line.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -LiteralPath

The local file to write, used exactly as written. Its folder must exist.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PassThru

Writes the finished file as a FileInfo object.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: False
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

Asks for confirmation before creating or replacing the file.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: False
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

Shows the file that would be created or replaced, without creating anything.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: False
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

### System.String

The lines to write.

## OUTPUTS

### System.IO.FileInfo

The finished file, with -PassThru.

## NOTES

The command sends nothing to Files.com. On Windows the stage is a hidden file beside the destination that no other process can rename or delete while it is written, and the finished file is not hidden. See about_FilesCom.

## RELATED LINKS

- [about_FilesCom]()
- [Receive-FilesFile]()
