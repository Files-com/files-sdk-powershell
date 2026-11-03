---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: New-FilesAttachment
---

# New-FilesAttachment

## SYNOPSIS

Makes an attachment for a file parameter from a local file, bytes or a stream.

## SYNTAX

### Path (Default)

```
New-FilesAttachment [-Path] <string> [-FileName <string>] [-ContentType <string>]
```

### LiteralPath

```
New-FilesAttachment -LiteralPath <string> [-FileName <string>] [-ContentType <string>]
```

### Content

```
New-FilesAttachment -Content <byte[]> -FileName <string> [-ContentType <string>]
```

### Stream

```
New-FilesAttachment -InputStream <Stream> -FileName <string> [-ContentType <string>] [-MaxLength <long>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

New-FilesAttachment makes a FilesAttachment, the value of a file parameter such as Set-FilesUser -AvatarFile, Set-FilesSite -LogoFile or New-FilesBehavior -AttachmentFile: a local file, bytes in memory or a stream of yours, with the name and media type to send it as. A local file from Get-Item converts to an attachment without it, sent under its own name.

Making an attachment reads and sends nothing. The command given it checks that its local file exists before any confirmation prompt, and opens the file, or reads the stream, only when it goes ahead with the change: never under -WhatIf, and not when a requested confirmation is declined. It sends the file in the same request as its other parameters, whose values go exactly as they would without it. When it sends the request again, after a failure that may be temporary or a move to the site's region, it sends the same bytes. A local file is opened once, and closed when the command ends; a file whose length changes meanwhile fails the command before its request is sent again.

A stream is borrowed: commands read it from its position when they send it and leave it open. A stream that can seek is read in place, from that position each time the request is sent, and must keep its length meanwhile. One that cannot seek, such as a program's output, is first copied to a private temporary file, deleted when the command ends, so that its length is known; if it holds more than -MaxLength bytes, 64 MiB by default, the command fails with Files.InvalidArgument and nothing is sent. A failure to read the file or stream ends the command with that failure, as it was raised. Bytes are not copied: leave them unchanged until the command given the attachment ends.

## EXAMPLES

### Example 1: Set a user's avatar from a local file

```powershell
Set-FilesUser -Id 42 -AvatarFile (New-FilesAttachment -Path ./avatars/ana.png -ContentType image/png)
```

Sends ana.png as the user's avatar. (Get-Item ./avatars/ana.png) also works, sent as application/octet-stream.

### Example 2: Send bytes made in memory

```powershell
$logo = [System.IO.File]::ReadAllBytes('./brand/logo.png')
Set-FilesSite -LogoFile (New-FilesAttachment -Content $logo -FileName logo.png -ContentType image/png) -WhatIf
```

Previews the change without reading or sending anything; without -WhatIf, sends the bytes as logo.png.

### Example 3: Send a stream within a limit

```powershell
$stream = [System.IO.File]::OpenRead('./watermark.png')
try {
    $watermark = New-FilesAttachment -InputStream $stream -FileName watermark.png -ContentType image/png
    Set-FilesBundle -Id 7 -WatermarkAttachmentFile $watermark
}
finally {
    $stream.Dispose()
}
```

Sends the stream, which can seek, in place; the command leaves it open and the script closes it.

## PARAMETERS

### -Content

The bytes to attach. They are not copied: leave them unchanged until the command given the attachment ends.

```yaml
Type: System.Byte[]
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Content
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```
### -ContentType

The media type to send the file with, such as image/png. Defaults to application/octet-stream.

```yaml
Type: System.String
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
### -FileName

The name to send the file with, exactly as given: not empty, and without control characters. Defaults to the local file's name.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Path
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: LiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Content
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Stream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```
### -InputStream

The stream to attach, from its position when the command given the attachment sends it. It is borrowed: the command reads it and leaves it open.

```yaml
Type: System.IO.Stream
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Stream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```
### -LiteralPath

The local file to attach, used exactly as written. A file piped from Get-ChildItem binds here.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases:
- PSPath
ParameterSets:
- Name: LiteralPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```
### -MaxLength

For -InputStream, the most bytes a stream that cannot seek may hold, from 1: it is copied to a private temporary file before the request is sent, and nothing is sent if it holds more. Defaults to 64 MiB. A stream that can seek is read in place and needs no limit.

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Stream
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

The local file to attach. Wildcards are expanded and must match exactly one file.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: true
Aliases: []
ParameterSets:
- Name: Path
  Position: 0
  IsRequired: true
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

### System.IO.FileInfo

Files from Get-ChildItem, bound to -LiteralPath by their PSPath.

## OUTPUTS

### FilesCom.PowerShell.FilesAttachment

The attachment: its FileName, ContentType, SourceKind (File, Bytes or Stream), the LocalPath of a local file, the Length of bytes and the MaxLength of a stream.

## NOTES

The file parameters are those of Set-FilesSite (Icon16File, Icon32File, Icon48File, Icon128File, LogoFile, BundleWatermarkAttachmentFile, LoginPageBackgroundImageFile), New-FilesUser and Set-FilesUser (AvatarFile), New-FilesBehavior and Set-FilesBehavior (AttachmentFile), New-FilesBundle and Set-FilesBundle (WatermarkAttachmentFile) and Set-FilesStyle (File). See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()
