---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: New-FilesAutomaticConcurrency
---

# New-FilesAutomaticConcurrency

## SYNOPSIS

Creates expert tuning for how automatic uploads choose the number of parts they send at once.

## SYNTAX

### __AllParameterSets

```
New-FilesAutomaticConcurrency [-Profile] <FilesUploadPerformanceProfile> [-StartTarget <int>] [-MinTarget <int>]
 [-SoftCeiling <int>] [-MaxTarget <int>] [-SoftCeilingUnlockBytes <long>]
 [-SoftCeilingUnlockBytesPerSecond <double>] [-WindowAttempts <int>] [-GrowthIntervalWindows <int>]
 [-GrowthStep <int>] [-ProbeGainPercent <double>] [-PlateauTarget <int>]
 [-ProbeGainPerAddedPartPercent <double>] [-ProbeWindows <int>] [-ShrinkPercent <double>] [-HoldWindows <int>]
 [-LatencyGrowthQueueLimit <double>] [-LatencyBackoffQueueLimit <double>]
 [-BackpressureShrinkPercent <double>] [-BackpressurePauseSeconds <double>]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

New-FilesAutomaticConcurrency creates tuning for Send-FilesFile -AutomaticConcurrency: the number of parts at once an automatic upload starts with, how far and how fast it may add parts, and how much it backs off after failures, throttling or rising latency. Creating it sends nothing to Files.com.

By default each upload uses the Agent profile when Files.com supplies direct connection information for it (a Files.com Agent or Integration Worker destination), the scaled S3 policy where its parts go straight to Amazon S3, and Generic elsewhere; -PerformanceProfile S3 is the conservative S3 preset, which starts at 8. Tuning is for rollout and diagnostics: to hold a new integration to fewer parts at once, to give a known fast S3 destination a higher start, or to reproduce a behavior while investigating it, with Send-FilesFile -Diagnostics showing the decisions it leads to.

A single-file upload using the default scaled S3 policy starts with 1 part and probes upward using the parts S3 has answered. A window delivering at least 96 MiB/s moves it to the policy's 150-part start, within Send-FilesFile -MaxConcurrency. Otherwise it establishes a baseline after its connections warm up, then probes one extra request at a time. It keeps a request while that request delivers at least half of what each earlier request delivered, provided the requests have not begun waiting behind each other. It settles back when the gain is too small or the added load increases queueing. It also postpones a probe when requests are already queueing. After settling or a temporary failure, it pauses before trying one extra request again. Later probes keep the same gradual growth; a later window showing a fast link can still move it to the policy's start. Directory uploads warm-start the same way, once for all their files, in the scope they share. Tuning supplied through -AutomaticConcurrency starts at its -StartTarget instead.

-Profile is required and chooses the defaults of every setting not given: Generic starts at 4 parts at once and reaches at most 8; S3 starts at 8, grows to 16, and past that up to 64 once the upload has moved 1 GiB at 32 MiB a second or more; Agent starts at 2, grows to 4, and up to 8 on the same terms at 16 MiB a second. An upload given this tuning uses it whatever the destination: tuning replaces the profile chosen for the destination rather than adjusting it. The resulting object lists every effective value and, in Overrides, the settings you gave; `New-FilesAutomaticConcurrency -Profile S3 | Format-List` shows a profile's defaults.

The settings are checked together when the tuning is created, with the rules the upload checks them by: the numbers of parts at once must be in the order -MinTarget, -StartTarget, -SoftCeiling, -MaxTarget, with -PlateauTarget from -MinTarget to -MaxTarget; percentages of shrinking are from 0 to 100; -LatencyBackoffQueueLimit is at least -LatencyGrowthQueueLimit; and -BackpressurePauseSeconds is at most 60. A combination the upload would refuse fails here with Files.InvalidArgument, naming the defaults it conflicts with.

Tuning never changes the parts Files.com asks for, and never lets parts be sent in parallel when Files.com says they must be sent one at a time. It chooses only how many part requests are in flight. Send-FilesFile -MaxConcurrency and a -Limiter's request budget still apply on top of it.

## EXAMPLES

### Example 1: Start a new integration with fewer parts at once

```powershell
$gentle = New-FilesAutomaticConcurrency -Profile Generic -StartTarget 2 -SoftCeiling 4 -MaxTarget 4 -PlateauTarget 4
Send-FilesFile ./export.csv -RemotePath exports/export.csv -AutomaticConcurrency $gentle
```

Starts at 2 parts at once and never sends more than 4, growing only while that raises throughput. -PlateauTarget, the Generic default of which is 8, must come down with -MaxTarget.

### Example 2: See a profile's defaults

```powershell
New-FilesAutomaticConcurrency -Profile S3 | Format-List
```

Lists every setting the S3 profile starts from.

### Example 3: Back off longer after throttling while investigating

```powershell
$patient = New-FilesAutomaticConcurrency -Profile Agent -BackpressurePauseSeconds 5 -BackpressureShrinkPercent 75
Send-FilesFile ./big.bin -AutomaticConcurrency $patient -Diagnostics -InformationVariable trace -PassThru
$trace.MessageData | Where-Object Decision -eq CooldownStarted
```

Pauses new part requests for at least 5 seconds after a throttling answer, and shows each pause the upload made.

## PARAMETERS

### -BackpressurePauseSeconds

The shortest time, in seconds from 0 to 60, a throttling answer stops the upload from starting part requests, including retries. A longer Retry-After from Files.com's storage, up to a minute, is honored instead. The profiles' defaults are 1 second, and 2 for Agent.

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

### -BackpressureShrinkPercent

How much, in percent from 0 to 100, a throttling answer (HTTP 429 or 503, or Retry-After on a failure) lowers the number of parts at once.

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

The number of measured windows at a number of parts at once before it grows, at least 1.

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

### -GrowthStep

The number of parts at once each growth adds, at least 1.

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

The number of measured windows growth waits after the number was lowered or a growth was given up, at least 0.

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

### -LatencyBackoffQueueLimit

The estimated number of part requests queued, rather than moving data, above which the number of parts at once is lowered by -ShrinkPercent when throughput has also fallen. At least -LatencyGrowthQueueLimit; 0 turns this off.

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

### -LatencyGrowthQueueLimit

The estimated number of part requests queued, rather than moving data, above which growth waits. The estimate compares each window's latency per byte with the lowest the upload has seen. 0 turns this off.

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

### -MaxTarget

The most parts at once an upload ever sends, from 1 to 1024 and at least -SoftCeiling. Send-FilesFile -MaxConcurrency, when given, limits it further.

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

### -MinTarget

The fewest parts at once failures, throttling or latency can lower an upload to, at least 1.

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

### -PlateauTarget

The number of parts at once, from -MinTarget to -MaxTarget, above which each part added must earn -ProbeGainPerAddedPartPercent of throughput to be kept.

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

### -ProbeGainPerAddedPartPercent

The throughput gain, in percent, each part at once added above -PlateauTarget must earn. A growth from 16 to 18 parts above a plateau of 16 must gain twice this, or -ProbeGainPercent if that is more.

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

### -ProbeGainPercent

The throughput gain, in percent, a grown number of parts at once must show over the number it came from to be kept.

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

The number of measured windows a grown number of parts at once gets to show its gain before it goes back, at least 1.

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

### -Profile

The kind of destination whose defaults the settings not given take: Generic, which favors stability; S3, tuned for Amazon S3; or Agent, for Files.com Agent and Integration Worker destinations. An upload given the tuning uses it whatever the destination, and reports this profile.

```yaml
Type: FilesCom.PowerShell.FilesUploadPerformanceProfile
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
AcceptedValues:
- Generic
- S3
- Agent
HelpMessage: ''
```

### -ShrinkPercent

How much, in percent from 0 to 100, a temporary failure (a connection failure or timeout, 5xx other than 501, or 408) or rising latency lowers the number of parts at once. It falls by at least one part when this is above 0.

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

### -SoftCeiling

The most parts at once growth reaches until the upload has delivered -SoftCeilingUnlockBytes and a window has measured -SoftCeilingUnlockBytesPerSecond, from -StartTarget to -MaxTarget. Equal to -MaxTarget for no soft ceiling.

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

### -SoftCeilingUnlockBytes

The bytes of accepted parts an upload must have delivered before it may grow past -SoftCeiling, at least 1. Time alone never unlocks the soft ceiling.

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

### -SoftCeilingUnlockBytesPerSecond

The throughput, in bytes per second, a measured window must reach before an upload may grow past -SoftCeiling. 0 still requires a measured window.

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

### -StartTarget

The number of parts at once an upload starts with, from -MinTarget to -SoftCeiling.

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

### -WindowAttempts

The fewest successful part requests in one measurement window, at least 1. A window also has at least one per part at once and lasts at least as long as its requests took on average.

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

### FilesCom.PowerShell.FilesAutomaticConcurrency

The tuning, with every effective value, its Profile and the Overrides you gave.

## NOTES

Without -AutomaticConcurrency or -PerformanceProfile, Send-FilesFile chooses the profile from the destination, as described above; tuning you give is used as it is, whatever the destination. -MaxConcurrency, a -Limiter's request slots and its memory budget cap the parts in flight whatever the tuning, and parts are sent one at a time whenever Files.com requires it. The tuning sets how the number of parts at once is chosen; it promises no throughput. See Send-FilesFile for how an upload uses and reports the tuning, and about_FilesCom.

## RELATED LINKS

- [about_FilesCom]()
