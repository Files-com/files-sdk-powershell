---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: New-FilesAutomaticPartSizing
---

# New-FilesAutomaticPartSizing

## SYNOPSIS

Creates expert tuning for the part sizes uploads choose where their parts go straight to Amazon S3.

## SYNTAX

### __AllParameterSets

```
New-FilesAutomaticPartSizing [-KnownSizeTiers <FilesPartSizeTier[]>] [-WorkloadBytes <long>]
 [-WorkloadParts <int>] [-UnknownLengthStartSize <long>] [-MinPartSize <long>] [-MaxPartSize <long>]
 [-GrowthFactor <double>] [-GrowthIntervalWindows <int>] [-ProbeGainPercent <double>] [-ProbeWindows <int>]
 [-HoldWindows <int>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

New-FilesAutomaticPartSizing creates tuning for Send-FilesFile -AutomaticPartSizing: the size an upload's parts start with, the bounds it keeps them in, and how it tries larger parts. Creating it sends nothing to Files.com. It applies only where the upload chooses its part sizes, where its parts go straight to Amazon S3 as an ordinary multipart upload, and only to Automatic part sizes; elsewhere uploads use the sizes Files.com plans.

By default the upload chooses a part size from the file's size and tries larger parts while that increases throughput. Tuning is for rollout and diagnostics, such as keeping parts within a proxy's request size limit (-MaxPartSize), sending fewer, larger requests (-MinPartSize), or making a batch of small files share a plan (-WorkloadBytes).

An upload starts with the part size of the first tier that covers its file. If that would give the workload fewer than -WorkloadParts parts, it starts smaller, so that several parts can be sent at once. The size then stays from -MinPartSize to -MaxPartSize and within the engine's limits for S3, every part but the last at least 5 MiB and none larger than 1 GiB; a part is made larger than -MaxPartSize only where that is needed to stay within the number of parts available, and the last part holds what remains. The upload result's PartSizeConstraint names what set the starting size, and LargestPartSize the largest part.

Once adding parts at once no longer raises throughput, the parts not yet read grow by -GrowthFactor; larger parts must show -ProbeGainPercent more throughput within -ProbeWindows windows or the size goes back, and growth waits -HoldWindows windows. Parts already read, being sent or waiting to retry keep their size and bytes.

The settings are checked together when the tuning is created, with the rules the upload checks them by, such as -MinPartSize at most -MaxPartSize and -KnownSizeTiers with each file size once. The object lists every effective value and, in Overrides, the settings you gave.

## EXAMPLES

### Example 1: Keep parts within a proxy's limit

```powershell
$small = New-FilesAutomaticPartSizing -MaxPartSize 16MB
Send-FilesFile ./backup.tar -RemotePath backups/backup.tar -AutomaticPartSizing $small
```

Chooses parts of at most 16 MB, unless the number of parts available needs larger ones.

### Example 2: Send fewer, larger parts

```powershell
$large = New-FilesAutomaticPartSizing -MinPartSize 64MB -KnownSizeTiers @{ 1GB = 64MB; 1TB = 128MB }
Send-FilesFile ./video.mov -AutomaticPartSizing $large -PassThru | Select-Object PartSizeConstraint, LargestPartSize
```

Starts at 64 MB for files up to 1 GB and 128 MB beyond, never below 64 MB, and shows what set the starting size.

### Example 3: Plan a batch of small files together

```powershell
$files = Get-ChildItem ./exports -File
$batch = New-FilesAutomaticPartSizing -WorkloadBytes ($files | Measure-Object Length -Sum).Sum
$files | Send-FilesFile -RemotePath { "exports/$($_.Name)" } -AutomaticPartSizing $batch
```

Sizes each file's parts from the size of the whole batch rather than the file alone.

## PARAMETERS

### -GrowthFactor

How many times larger each growth makes the parts not yet read: more than 1 and at most 16. 2 by default.

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

### -GrowthIntervalWindows

The number of measured windows at a size before parts grow, at least 1. 2 by default.

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

### -HoldWindows

The number of measured windows growth waits after larger parts were given up, at least 0. 4 by default.

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

### -KnownSizeTiers

The part size a file starts with, by the file's size: a hashtable of file sizes and the part sizes files of up to that size start with, such as @{ 256MB = 8MB; 4GB = 16MB }, or the KnownSizeTiers of other tuning. Sizes are whole numbers of bytes and the tiers are put in order of file size; files larger than every tier use the last. The defaults are 8 MiB for files up to 256 MiB, 16 MiB up to 4 GiB, 32 MiB up to 64 GiB, and 64 MiB beyond.

```yaml
Type: FilesCom.PowerShell.FilesPartSizeTier[]
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

### -MaxPartSize

The largest size, in bytes, the upload chooses or grows parts to, at least 1 and at least -MinPartSize. 256 MiB by default. The engine's maximum applies too, and parts can be larger where the file needs them to be to stay within the number of parts available.

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

### -MinPartSize

The smallest size, in bytes, the upload chooses for a part other than the last, at least 0. 0 by default. The engine's minimum, 5 MiB for S3, applies too. When this minimum is what sets a part size, the upload result's PartSizeConstraint is MinPartSize.

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

### -ProbeGainPercent

The throughput gain, in percent, larger parts must show over the size before them to be kept, at least 0. 10 by default.

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

### -ProbeWindows

The number of measured windows larger parts get to show their gain, at least 1. 2 by default.

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

### -UnknownLengthStartSize

The size, in bytes, the parts of a stream of unknown length start with, at least 1. 8 MiB by default; the other bounds apply to it too.

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

### -WorkloadBytes

The size, in bytes, of the whole workload the upload belongs to, such as a batch of files uploaded together. Without it, each file's own size. Only the starting size uses it.

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

### -WorkloadParts

The fewest parts the workload should be divided into, at least 1: a smaller workload gets smaller parts, so that it can send several at once. 16 by default.

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

### FilesCom.PowerShell.FilesAutomaticPartSizing

The tuning, with every effective value and the Overrides you gave.

## NOTES

Without -AutomaticPartSizing, Send-FilesFile chooses part sizes as described above, where its parts go straight to Amazon S3; tuning you give is used as preferences within the engine's limits, every part but the last at least 5 MiB and none larger than 1 GiB, and elsewhere uploads use the sizes Files.com plans. A -Limiter's memory budget bounds the parts held at once. The tuning sets how sizes are chosen; it promises no throughput. See Send-FilesFile for how an upload uses and reports the tuning, and about_FilesCom.

## RELATED LINKS

- [about_FilesCom]()
