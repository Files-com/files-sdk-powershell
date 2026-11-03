---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Send-FilesFile
---

# Send-FilesFile

## SYNOPSIS

Uploads local files, or bytes from a stream, to Files.com, or through it to a remote server, snapshot or child site.

## SYNTAX

### Path (Default)

```
Send-FilesFile [-Path] <string[]> [[-RemotePath] <string>] [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### LiteralPath

```
Send-FilesFile -LiteralPath <string[]> [-RemotePath <string>] [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### Stream

```
Send-FilesFile [-RemotePath] <string> -InputStream <Stream> [-Length <long>] [-MaxLength <long>]
 [-PartStorage <FilesStreamPartStorage>] [-Spool] [-TemporaryDirectory <string>] [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### RemoteServerPath

```
Send-FilesFile [-Path] <string[]> [[-RemotePath] <string>] -RemoteServerId <long> [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### RemoteServerLiteralPath

```
Send-FilesFile -LiteralPath <string[]> -RemoteServerId <long> [-RemotePath <string>] [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### RemoteServerStream

```
Send-FilesFile [-RemotePath] <string> -InputStream <Stream> -RemoteServerId <long> [-Length <long>]
 [-MaxLength <long>] [-PartStorage <FilesStreamPartStorage>] [-Spool] [-TemporaryDirectory <string>]
 [-MkdirParents] [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### SnapshotPath

```
Send-FilesFile [-Path] <string[]> [[-RemotePath] <string>] -SnapshotId <long> [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### SnapshotLiteralPath

```
Send-FilesFile -LiteralPath <string[]> -SnapshotId <long> [-RemotePath <string>] [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### SnapshotStream

```
Send-FilesFile [-RemotePath] <string> -InputStream <Stream> -SnapshotId <long> [-Length <long>]
 [-MaxLength <long>] [-PartStorage <FilesStreamPartStorage>] [-Spool] [-TemporaryDirectory <string>]
 [-MkdirParents] [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### ChildSitePath

```
Send-FilesFile [-Path] <string[]> [[-RemotePath] <string>] -ChildSiteId <long> [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### ChildSiteLiteralPath

```
Send-FilesFile -LiteralPath <string[]> -ChildSiteId <long> [-RemotePath <string>] [-MkdirParents]
 [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-CheckpointPath <string>]
 [-CheckpointRestart <FilesCheckpointRestart>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

### ChildSiteStream

```
Send-FilesFile [-RemotePath] <string> -InputStream <Stream> -ChildSiteId <long> [-Length <long>]
 [-MaxLength <long>] [-PartStorage <FilesStreamPartStorage>] [-Spool] [-TemporaryDirectory <string>]
 [-MkdirParents] [-ConcurrencyMode <FilesUploadConcurrencyMode>] [-MaxConcurrency <int>]
 [-PerformanceProfile <FilesUploadPerformanceProfile>] [-AggressiveStartup]
 [-PartSizingMode <FilesUploadPartSizingMode>] [-PartSize <long>]
 [-AutomaticConcurrency <FilesAutomaticConcurrency>]
 [-AutomaticPartSizing <FilesAutomaticPartSizing>] [-MaxReadyParts <int>] [-MaxReadyBytes <long>]
 [-Diagnostics] [-DiagnosticLimit <int>] [-Limiter <FilesTransferLimiter>]
 [-ModifiedTime <DateTimeOffset>] [-PassThru] [-Connection <FilesConnection>]
 [-WorkspaceScope <long>] [-NoWorkspaceScope] [-WhatIf] [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Send-FilesFile uploads each local file given with -Path, -LiteralPath or from Get-ChildItem, or the bytes of a stream, to Files.com. The Files.com engine sends the file in parts, several at a time when Files.com allows it, retries a failed part on its own, and completes the upload with one final request. Write-Progress shows the bytes Files.com has accepted.

By default each upload adapts how many parts it sends at once to what it measures: it adds parts while they raise throughput and sends fewer after failures, throttling or rising latency, within -MaxConcurrency when that is given. It starts from the Agent tuning when Files.com supplies direct connection information for the upload (a Files.com Agent or Integration Worker destination), from the scaled S3 policy where its parts go straight to Amazon S3 as an ordinary multipart upload, as described below, and from the generic tuning for any other destination; -PerformanceProfile chooses the tuning instead, S3, Agent or Generic, as a performance preference that never changes the parts Files.com asks for. With the scaled S3 policy, the upload probes the link from 1 part at a time by default; -AggressiveStartup instead starts it at the policy's 150 parts at once, within -MaxConcurrency, on a known high-throughput network. -ConcurrencyMode Static instead sends -MaxConcurrency parts at once, or 4, every time, for integrations that need the same behavior on every run. Either way, when Files.com says parts must be sent one at a time, they are. With -PassThru the result shows how the upload chose: its mode, profile, the most parts it had in flight at once and why it sent parts one at a time, if it did.

Where an upload's parts go straight to Amazon S3 as an ordinary multipart upload, the upload chooses their sizes. The engine recognizes this only from what Files.com gives when the upload starts: an ordinary write whose parts go by PUT to presigned UploadPart URLs of one Amazon S3 general-purpose endpoint over HTTPS, signed for the host alone, with nothing to send but each part's length and bytes. There it plans within its own limits for S3: every part but the last at least 5 MiB, no part larger than 1 GiB, at most 10,000 parts, or fewer where Files.com says fewer are available, and files of at most 5 TiB. These are the engine's limits, within S3's own rules, and no promise that Files.com or the destination accepts any file within them. By default the upload chooses a size from the file's size and tries larger parts while that raises throughput; -PartSize instead sends every part but the last with that many bytes, and -PartSizingMode ServerPlanned uses the sizes Files.com plans. A file larger than 5 TiB, or one that would need more parts than are available, uses the sizes Files.com plans instead; a -PartSize outside the limits, or that would need more parts than are available, fails before anything is sent. Any other destination, such as S3-compatible storage, another kind of S3 endpoint, a Files.com Agent destination or storage Files.com writes itself, uses the sizes Files.com plans, whatever the mode. With -PassThru the result shows where the sizes came from, why they were Files.com's if they were, what limited the size the upload started with, the engine's limits where it could choose, and the largest part.

-InputStream uploads a stream from its current position. With -Length, exactly that many bytes are uploaded. Without it, the command uploads everything the stream gives until it ends, such as a program's output: the stream is never asked for its length or seeked. Each part is kept until Files.com accepts it, so that a retry resends the same bytes: in memory by default, within the transfer limiter's buffer space, or with -PartStorage TemporaryFiles in a private temporary file per part, deleted once the part is accepted or the upload ends. -MaxLength declares the most bytes such a stream may give. A longer stream stops the upload, which is not completed, and the command fails with Files.LimitExceeded; parts already sent are not undone, but the remote file is neither created nor changed. A failure the stream itself raises while it is read, whatever its type, stops the upload the same way and ends the command with that failure, exactly as the stream raised it: it is never reported as a limit. Where the upload chooses part sizes, parts are sized so that -MaxLength, or else the 5 TiB the engine plans for, fits in the parts available, and several are sent at once. Elsewhere Files.com plans each part in turn and they are sent one at a time, in order, and the result's SerialReason and ServerPlannedReason are LengthUnknown. When parts held in memory could not be large enough for the bound to fit, the upload uses Files.com's sizes instead and says MemoryLimit: declare a smaller -MaxLength, or use -PartStorage TemporaryFiles.

-Spool instead copies the whole stream to a private temporary file of at most -MaxLength bytes before anything is sent, and then uploads that file with its size known, like a local file; the file is deleted however the upload ends. A longer stream, a failed read or stopping the command during the copy ends the command before anything is sent. The copy's buffer comes from the -Limiter's buffer space, when one is given. -TemporaryDirectory chooses the existing local folder for these temporary files; by default they go to the user's temporary folder.

For rollout and diagnostics, -AutomaticConcurrency takes expert tuning from New-FilesAutomaticConcurrency, and -AutomaticPartSizing from New-FilesAutomaticPartSizing. Concurrency tuning starts from the defaults of the profile it names and replaces the profile chosen for the destination, whatever the destination; without it, the upload keeps that choice. Part sizing tuning applies where the upload chooses its part sizes, as preferences within the engine's limits. -MaxReadyParts and -MaxReadyBytes bound the parts read ahead and waiting to be sent: by default 2 parts and 64 MiB of their buffers. The byte budget is for waiting parts: a part larger than it is still read ahead once no other part is waiting, so a small -MaxReadyBytes slows reading ahead rather than refusing large parts. None of these changes the parts Files.com asks for.

Settings that would change nothing are refused before anything is read or sent, rather than ignored: -PartSize with another -PartSizingMode, -PartSizingMode Fixed without -PartSize, -AutomaticConcurrency with -ConcurrencyMode Static or with -PerformanceProfile, -AggressiveStartup with -ConcurrencyMode Static, -PerformanceProfile or -AutomaticConcurrency, -AutomaticPartSizing with -PartSize or another -PartSizingMode, -DiagnosticLimit without -Diagnostics, -MaxLength or -Spool with -Length, -Spool without a -MaxLength of at least 1 or with -PartStorage, and -TemporaryDirectory without -Spool or -PartStorage TemporaryFiles. The stream parameters can be used only with -InputStream.

Remote paths are used exactly as written; -RemotePath defaults to the local file's name in the root folder. -Path expands wildcards; -LiteralPath and piped files are used as given. A local file is opened once and every part is read through that handle.

-RemoteServerId, -SnapshotId or -ChildSiteId uploads to that remote server, snapshot or child site instead, through Files.com, as the same upload with every setting described here. Files.com's folder for the destination, _/RemoteServers/<id>, _/Snapshots/<id> or _/Sites/<id>, is the root, and -RemotePath, by default the local file's name, is the path within it. There, as the Files.com SDK does for these destinations, either slash separates folders, and empty, "." and ".." folders and null characters are left out, so the path cannot leave the destination: ../reports/./q3.csv and reports\q3.csv are both reports/q3.csv. A -RemotePath that leaves nothing, such as "..", is refused with Files.InvalidArgument before anything is sent. The confirmation, errors, progress, checkpoint and -PassThru name the whole remote path, such as _/RemoteServers/12/reports/q3.csv.

All the files of one input are found and checked before the first upload starts. A -RemotePath names one file, so when the paths given match more than one file, or two files would upload to what Files.com treats as the same remote path (it compares paths without regard to case or accents), the command uploads nothing and fails with Files.InvalidArgument. To send several files, pipe them and give each its own remote path with a script block, as in Example 2.

An upload that needs more than a limit allows, such as a stream longer than -MaxLength, a part larger than the limiter may buffer, more parts than are available or a -PartSize outside the engine's limits, stops with Files.LimitExceeded. Its exception's UploadLimit says which limit (StreamLength, PartCount, BufferSpace, MemoryPart, SpoolLength or FixedPartSize), and the error's recommended action says what to change. Only an actual limit is reported this way: a failure of your stream, whatever its type, and any other failure keep their own kind.

If the final request fails after it may have reached Files.com, the command stops with Files.TransferCompletionUnknown. The file may or may not have been completed: check it, for example with Get-FilesFile, before uploading again. The error's exception carries the accepted parts in UncertainUpload.

With -CheckpointPath the upload of a local file records its progress in that file, so that when it is stopped, by Ctrl+C, a failure or the process ending, running the same command again continues it: parts Files.com accepted are read again and, while each still has the bytes that were sent, are not sent again; parts planned and not accepted are sent again exactly as planned. -PassThru reports the parts and bytes continued in ResumedParts and ResumedBytes. The engine writes the checkpoint for you only, without credentials or signed URLs, flushes it as the upload goes and deletes it once the upload completes. A checkpoint that records another upload, a file that changed, an upload Files.com no longer knows, or parts that no longer go where, or within the limits, they did when it started is refused with Files.CheckpointRefused before any part is sent, unless -CheckpointRestart StartOver says to upload the file again from its first byte. An upload whose final request was sent is never started over: it is Files.TransferCompletionUnknown, as above. A checkpoint is relied on only where no other user can change it: its folder must be yours or the system's and writable by no one else, and the checkpoint must be a file of yours, not a link; otherwise it is refused, with CheckpointProblem Untrusted.

Each file or stream is one change for -WhatIf and -Confirm; the parts and retries of a confirmed upload are not confirmed separately. Nothing is read from a stream, copied or sent unless the upload goes ahead: not under -WhatIf, nor when a requested confirmation is declined. Stopping the command, as with Ctrl+C, stops the upload wherever it is, and its temporary files are deleted.

Write-Progress shows the bytes Files.com has accepted. For a stream whose length is not known yet it shows them against the bytes read so far, without a percentage or time remaining, and for -Spool it shows the copy first. While a throttling answer pauses new part requests, it says for how long.

With -PassThru, the result's Metrics say what the upload measured: its throughput, the time it waited for its limiter and for parts to be read, how often reading ahead reached -MaxReadyParts or -MaxReadyBytes, how its number of parts at once and its part sizes changed and why, and the process's CPU and memory use, where the platform reports them. -Diagnostics also writes a record of every part request and every concurrency and part size decision to the Information stream, tagged FilesCom.UploadTrace, followed by a summary once the upload has completed or failed; collect them with -InformationVariable. Records hold the remote path, times, decisions and counts only, never storage addresses, headers, credentials or file contents. One upload writes at most -DiagnosticLimit records, 1000 by default; the summary counts any left out, and its Complete is then false. When the pipeline is stopped, no summary is written. Records are written on the pipeline's own thread as the upload goes, never from the upload's threads. Measurements against a local test server say nothing about production throughput.

## EXAMPLES

### Example 1: Upload a file

```powershell
Send-FilesFile -Path ./report.csv -RemotePath reports/2026/report.csv -MkdirParents
```

Uploads report.csv, creating the reports/2026 folders if they are missing.

### Example 2: Upload every file in a folder

```powershell
Get-ChildItem ./exports -File | Send-FilesFile -RemotePath { "exports/$($_.Name)" } -PassThru
```

Uploads each file under exports/ and writes each completed upload.

### Example 3: Upload from a stream

```powershell
$stream = [System.IO.File]::OpenRead('./backup.bin')
try {
    Send-FilesFile -InputStream $stream -Length $stream.Length -RemotePath backups/backup.bin
}
finally {
    $stream.Dispose()
}
```

Uploads the stream's bytes. The command leaves the stream open; the caller closes it.

### Example 4: Upload a program's output within a declared size

```powershell
$start = [System.Diagnostics.ProcessStartInfo]::new('pg_dump', '--format=custom --dbname=sales')
$start.UseShellExecute = $false
$start.RedirectStandardOutput = $true
$process = [System.Diagnostics.Process]::Start($start)
Send-FilesFile -InputStream $process.StandardOutput.BaseStream -RemotePath backups/sales.dump.partial -MaxLength 200GB -PartStorage TemporaryFiles -PassThru |
    Select-Object Path, Size, PartSizeSource, ServerPlannedReason, SerialReason
$process.WaitForExit()
```

Uploads the export as pg_dump writes it, keeping each part in a temporary file until Files.com accepts it, and fails without completing the upload if the export grows beyond 200 GB. The upload completes when pg_dump closes its output, which it also does when it fails, so check its exit code before publishing the file under its final name. The installed example Send-FilesLiveExport.ps1 does that, and deletes the upload when the program fails.

### Example 5: Copy a stream to a temporary file before sending anything

```powershell
Send-FilesFile -InputStream $stream -RemotePath imports/feed.json -Spool -MaxLength 2GB -TemporaryDirectory D:\spool
```

Reads the whole stream into a private temporary file in D:\spool first, so nothing reaches Files.com unless the stream ends within 2 GB, then uploads the file with its size known and deletes it.

### Example 6: Preview uploads

```powershell
Send-FilesFile -Path ./*.csv -WhatIf
```

Lists the files that would be uploaded and where, without sending anything.

### Example 7: Upload with a fixed number of parts at once, of a fixed size

```powershell
Send-FilesFile -Path ./nightly.tar -RemotePath backups/nightly.tar -ConcurrencyMode Static -MaxConcurrency 8 -PartSize 64MB -PassThru |
    Select-Object Path, ConcurrencyMode, PeakConcurrency, SerialReason, PartSizeSource, ServerPlannedReason
```

Sends eight parts at a time whenever Files.com allows parallel parts, each part but the last of 64 MB where the parts go straight to Amazon S3, and shows how the upload behaved. Where Files.com plans the sizes, ServerPlannedReason is NotAllowed.

### Example 8: Tune an automatic upload for a rollout

```powershell
$concurrency = New-FilesAutomaticConcurrency -Profile S3 -StartTarget 4 -MaxTarget 16
$sizing = New-FilesAutomaticPartSizing -MaxPartSize 32MB
Send-FilesFile ./backup.tar -RemotePath backups/backup.tar -AutomaticConcurrency $concurrency -AutomaticPartSizing $sizing -MaxReadyParts 4 -PassThru |
    Select-Object PerformanceProfile, PerformanceProfileSource, PeakConcurrency, LargestPartSize, Metrics
```

Starts at 4 parts at once and grows to at most 16, keeps chosen parts within 32 MB, reads up to 4 parts ahead, and shows what the upload measured.

### Example 9: Capture an upload's decisions

```powershell
Send-FilesFile ./big.bin -Diagnostics -InformationVariable trace
$records = $trace.MessageData
$records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadDecisionTrace] } | Format-Table Elapsed, Decision, PreviousTarget, Target
$records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadTraceSummary] }
```

Lists how the upload changed its number of parts at once, and whether every record was written.

### Example 10: Upload to a remote server

```powershell
Send-FilesFile -Path ./q3.csv -RemoteServerId 12 -RemotePath reports/q3.csv -MkdirParents -PassThru
```

Uploads q3.csv, through Files.com, to reports/q3.csv on the remote server with ID 12: the file _/RemoteServers/12/reports/q3.csv. Without -RemotePath it goes to q3.csv in the server's root folder.

### Example 11: Preview uploading a folder's files to a child site

```powershell
Get-ChildItem -Path ./outgoing -File | Send-FilesFile -ChildSiteId 7 -RemotePath { "incoming/" + $_.Name } -WhatIf
```

Describes each upload to the incoming folder of the child site with ID 7, with its whole remote path, without contacting Files.com. Remove -WhatIf to upload them.

### Example 12: Skip startup probes on a high-throughput network

```powershell
Send-FilesFile ./backup.tar -RemotePath backups/backup.tar -AggressiveStartup -MaxConcurrency 64 -PassThru |
    Select-Object PerformanceProfile, PerformanceProfileSource, PeakConcurrency
```

Where the parts go straight to Amazon S3, starts at 64 parts at once, the scaled S3 policy's start within -MaxConcurrency, rather than probing the link from 1, and adapts from there.

## PARAMETERS

### -AggressiveStartup

Starts an automatic upload whose parts go straight to Amazon S3 as an ordinary multipart upload at the scaled S3 policy's 150 parts at once, within -MaxConcurrency and the -Limiter, skipping the initial concurrency probes. Use it on a known high-throughput network, such as a high-capacity cloud VM. Without it, or with -AggressiveStartup:$false, the upload probes the link from 1 part at a time and adds parts as the parts S3 has answered show they help, as -PerformanceProfile describes, so that a link more parts at once would only overload is not flooded. Either way the upload then adapts with the same policy: failures, throttling and rising latency lower the number, retries and Ctrl+C work as usual, and parts go one at a time when Files.com says they must. Uploads to other destinations start from their own defaults, and the part sizes the upload plans do not depend on it. It cannot be used with -ConcurrencyMode Static, -PerformanceProfile or -AutomaticConcurrency, which choose their own start; -AggressiveStartup:$false can.

The default start is not a lower performance level: it can reach the same eventual throughput and concurrency ceiling, and only its ramp-up takes longer because it probes first. Leave -AggressiveStartup off when gradual startup helps avoid overwhelming a residential or constrained connection. Both starts keep -MaxConcurrency, the -Limiter and adaptive backoff.

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

### -AutomaticConcurrency

Expert tuning, from New-FilesAutomaticConcurrency, for how an automatic upload chooses the number of parts it sends at once. It starts from the defaults of the profile it names and replaces the profile chosen for the destination, whatever the destination; the result's PerformanceProfileSource is then Options. -MaxConcurrency and a -Limiter still apply. It cannot be used with -ConcurrencyMode Static or -PerformanceProfile.

```yaml
Type: FilesCom.PowerShell.FilesAutomaticConcurrency
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

### -AutomaticPartSizing

Expert tuning, from New-FilesAutomaticPartSizing, for the part sizes the upload chooses where its parts go straight to Amazon S3, as preferences within the engine's limits. It cannot be used with -PartSize or with -PartSizingMode Fixed or ServerPlanned, which do not choose sizes.

```yaml
Type: FilesCom.PowerShell.FilesAutomaticPartSizing
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

### -CheckpointPath

A local file in which the upload records its progress, so that when it is stopped, by Ctrl+C, a failure or the process ending, running the same command again with the same connection, remote path and checkpoint continues it: parts Files.com accepted, whose bytes are still what was sent, are not sent again. The engine writes the file for you only, without credentials or signed URLs, and deletes it once the upload completes. Its folder must exist and be one that only you, or the system, can change, and each upload needs a checkpoint of its own: it is refused, with Files.CheckpointRefused, when it records another upload, the file changed, or Files.com no longer knows the upload, unless -CheckpointRestart StartOver says to upload the file again. Only a local file can be continued: a checkpoint cannot be used with -InputStream, or with more than one file.

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
- Name: RemoteServerPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: RemoteServerLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSitePath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -CheckpointRestart

What the upload does when its checkpoint records an upload that cannot be continued, because the file changed, Files.com no longer knows the upload, or it now allows other part limits: Fail, the default, fails with Files.CheckpointRefused before any part is sent; StartOver uploads the file again from its first byte as a new upload. An upload whose final request was sent is never started over. Needs -CheckpointPath.

```yaml
Type: FilesCom.PowerShell.FilesCheckpointRestart
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
- Name: RemoteServerPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: RemoteServerLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSitePath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -ChildSiteId

Uploads to the child site with this ID, through Files.com, instead of a folder of this site: into its folder _/Sites/<id>, at -RemotePath within it.

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: ChildSitePath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteLiteralPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteStream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

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

### -ConcurrencyMode

How each upload decides how many parts to send at once when Files.com allows parallel parts. Automatic, the default, adapts the number to the throughput, failures and throttling it measures, within -MaxConcurrency when that is given. Static sends -MaxConcurrency parts at once, or 4 when it is not given, whatever the upload measures.

```yaml
Type: FilesCom.PowerShell.FilesUploadConcurrencyMode
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
AcceptedValues:
- Automatic
- Static
HelpMessage: ''
```

### -Connection

The connection to use, from Connect-FilesAccount. Defaults to the current runspace's default connection.

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

### -DiagnosticLimit

With -Diagnostics, the most records one upload writes, from 1 to 100000. Defaults to 1000. Records beyond it are left out and counted in the summary's RecordsDropped, and its Complete is false.

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

### -Diagnostics

Writes each upload's detailed records to the Information stream with the tag FilesCom.UploadTrace: a FilesUploadAttemptTrace for every part request, a FilesUploadDecisionTrace for every concurrency decision and a FilesUploadSizingTrace for every part size decision, then a FilesUploadTraceSummary once the upload has completed or failed. Collect them with -InformationVariable; they are not written to the output. Records hold the remote path, times, decisions and counts only.

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

### -InputStream

The stream to upload from its current position: -Length bytes, or without -Length everything it gives until it ends. It is borrowed: the command reads it only once the upload is confirmed, never seeks it, and leaves it open. It may be forward-only, such as a program's output.

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
- Name: RemoteServerStream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotStream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteStream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Length

The number of bytes to upload from -InputStream. If the stream ends sooner, the upload fails and is not completed. Without -Length, the command uploads everything the stream gives until it ends.

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
- Name: RemoteServerStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Limiter

A limiter from New-FilesTransferLimiter whose budget of requests and memory this upload shares with others.

```yaml
Type: FilesCom.PowerShell.FilesTransferLimiter
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

### -LiteralPath

The local files to upload, used exactly as written. Files piped from Get-ChildItem bind here.

```yaml
Type: System.String[]
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
- Name: RemoteServerLiteralPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: SnapshotLiteralPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: ChildSiteLiteralPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -MaxConcurrency

The most parts of each upload to send at the same time, from 1 to 1024: the limit an automatic upload stays within, or the number a static upload sends. By default an automatic upload has only its profile's own limit, and a static upload sends 4. A -Limiter's request budget applies as well, and parts are sent one at a time whenever Files.com requires it.

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

### -MaxLength

For -InputStream without -Length, the most bytes the stream may give. A longer stream stops the upload, which is not completed, and the command fails with Files.LimitExceeded. Where the upload chooses part sizes, parts are sized so that this many bytes fit in the parts available. With -Spool, it is also the most bytes copied to the temporary file. Without it, where the upload chooses part sizes, the stream may give at most the 5 TiB the engine plans for.

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
- Name: RemoteServerStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -MaxReadyBytes

The most bytes of buffers the parts read ahead and waiting to be sent may hold, at least 1. Defaults to the engine's default, 64 MiB. It is a budget for waiting parts, not a limit on the size of one: a larger part is still read ahead once no other part is waiting. The result's Metrics.ReadyLimitWaits counts how often reading ahead stopped at it or at -MaxReadyParts.

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

### -MaxReadyParts

The most parts read ahead and waiting to be sent, at least 1. Defaults to the engine's default, 2. However many parts an automatic upload sends at once, no more than this many wait ready.

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

### -MkdirParents

Creates missing parent folders of the remote path. -MkdirParents:$false asks Files.com explicitly not to create them.

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

### -ModifiedTime

The modification time to record. Defaults to the local file's last write time, or for a stream the time the upload completes.

```yaml
Type: System.DateTimeOffset
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

### -PerformanceProfile

The kind of destination an automatic upload's starting point and limits are tuned for: Generic, which favors stability; S3, tuned for Amazon S3, which starts higher and can grow further on fast uploads; or Agent, for Files.com Agent and Integration Worker destinations, which starts lower and stays lower. Without it, the upload uses Agent when Files.com supplies direct connection information for it, the scaled S3 policy (up to 1024 parts at once) where its parts go straight to Amazon S3 as an ordinary multipart upload, and Generic for any other destination. Given, it is used as it is, whatever the destination: S3 names the conservative S3 preset, which starts at 8 parts at once, up to 64. It is only a performance preference: it never changes the parts Files.com asks for. It cannot be used with -ConcurrencyMode Static.

Without -AggressiveStartup, a single-file upload using the default scaled S3 policy starts with 1 part and probes upward using the parts S3 has answered. A window delivering at least 96 MiB/s moves it to the policy's 150-part start, within -MaxConcurrency. Otherwise it establishes a baseline after its connections warm up, then probes by adding about half as many requests, rounded up. It keeps the added requests while each delivers at least half of what each earlier request delivered, provided the requests have not begun waiting behind each other. It settles back when the gain is too small or the added load increases queueing. It also postpones a probe when requests are already queueing. After settling or a temporary failure, it pauses before trying one extra request again. While it pauses, it may try larger parts, as -PartSizingMode describes; it tries one extra request again only after that trial is judged, with the part size the trial kept. Later successful probes use the same proportional growth; a later window showing a fast link can still move it to the policy's start, and a larger-part trial still under way then returns to the earlier size. A directory upload's files warm-start the same way, once, in the scope they share (Send-FilesDirectory). Explicit tuning starts at the number it specifies, and -AggressiveStartup at the policy's start.

```yaml
Type: FilesCom.PowerShell.FilesUploadPerformanceProfile
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
AcceptedValues:
- Generic
- S3
- Agent
HelpMessage: ''
```

### -PartSize

The size in bytes of every part but the last, which makes the upload's part sizes Fixed, for integrations that need the same parts on every run. Where the parts go straight to Amazon S3 and this size is outside the engine's limits, every part but the last at least 5 MiB and none larger than 1 GiB, or would need more parts than are available, or a stream's parts in memory could not hold it, the upload fails with Files.LimitExceeded before sending any part. Elsewhere the upload uses the sizes Files.com plans and the result's ServerPlannedReason is NotAllowed.

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

### -PartSizingMode

How the upload sizes its parts where they go straight to Amazon S3, within the engine's limits: Automatic, the default, chooses a size from the file's size and tries larger parts for the parts not yet read when sending more at once no longer raises throughput; Fixed sends every part but the last with -PartSize bytes, and needs it; ServerPlanned uses the sizes Files.com plans. Elsewhere every mode uses Files.com's sizes. It is independent of -ConcurrencyMode.

```yaml
Type: FilesCom.PowerShell.FilesUploadPartSizingMode
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
AcceptedValues:
- Automatic
- Fixed
- ServerPlanned
HelpMessage: ''
```

### -PartStorage

Where the upload keeps each part of -InputStream until Files.com accepts it, so that a retry resends the same bytes. Memory, the default, holds parts within the transfer limiter's buffer space, which also limits how large one part can be. TemporaryFiles writes each part to a private temporary file of its own, in -TemporaryDirectory or the user's temporary folder, and deletes it once the part is accepted or the upload ends; parts can then be as large as the upload's part sizes need, using disk space for the parts read ahead and being sent. It cannot be used with -Spool, whose parts are read back from its one temporary file.

```yaml
Type: FilesCom.PowerShell.FilesStreamPartStorage
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
- Name: RemoteServerStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues:
- Memory
- TemporaryFiles
HelpMessage: ''
```

### -PassThru

Writes each completed upload as a FilesUploadResult.

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

The local files to upload. Wildcards are expanded.

```yaml
Type: System.String[]
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
- Name: RemoteServerPath
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotPath
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSitePath
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -RemotePath

The remote path to upload to, used exactly as written. Defaults to the local file's name in the root folder. With -RemoteServerId, -SnapshotId or -ChildSiteId, the path within that destination, by default the local file's name in its root folder, which cannot leave it: either slash separates folders, and empty, "." and ".." folders are left out. For piped files, a script block such as { "reports/" + $_.Name } gives each its own path.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Path
  Position: 1
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: LiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: Stream
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: RemoteServerPath
  Position: 1
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: RemoteServerLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: RemoteServerStream
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotPath
  Position: 1
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: SnapshotLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: SnapshotStream
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSitePath
  Position: 1
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: ChildSiteLiteralPath
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: true
  ValueFromRemainingArguments: false
- Name: ChildSiteStream
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -RemoteServerId

Uploads to the remote server with this ID, through Files.com, instead of a folder of this site: into its folder _/RemoteServers/<id>, at -RemotePath within it.

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: RemoteServerPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: RemoteServerLiteralPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: RemoteServerStream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -SnapshotId

Uploads to the snapshot with this ID, through Files.com, instead of a folder of this site: into its folder _/Snapshots/<id>, at -RemotePath within it.

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: SnapshotPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotLiteralPath
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotStream
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Spool

Copies all of -InputStream, which has no -Length, to a private temporary file of at most -MaxLength bytes before sending anything, then uploads that file with its size known, like a local file, and deletes it however the upload ends. A longer stream, a failed read or stopping the command during the copy ends the command before anything is sent. It needs a -MaxLength of at least 1.

```yaml
Type: System.Management.Automation.SwitchParameter
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
- Name: RemoteServerStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -TemporaryDirectory

The existing local folder for the temporary files of -Spool or -PartStorage TemporaryFiles, used as written. Defaults to the user's temporary folder. Each file has a new random name, is readable only by its owner where the platform allows, and is deleted when the upload no longer needs it.

```yaml
Type: System.String
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
- Name: RemoteServerStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: SnapshotStream
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: ChildSiteStream
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

### System.IO.FileInfo

Files from Get-ChildItem, bound to -LiteralPath by their PSPath.

## OUTPUTS

### None

Nothing, unless -PassThru is given.

### FilesCom.PowerShell.FilesUploadResult

With -PassThru, each completed upload: its remote path, size, parts and the file as Files.com describes it; how it chose how many parts to send at once: ConcurrencyMode, PerformanceProfile, PerformanceProfileSource, PeakConcurrency, SerialReason and PartRetries; how it sized its parts: PartSizingMode, PartSizeSource, ServerPlannedReason, PartSizeConstraint, PartSizeLimits and LargestPartSize; and what it measured, in Metrics.

## NOTES

An interrupted upload of a local file continues with -CheckpointPath (see its description); a stream from -InputStream cannot be continued. The automatic part sizes and concurrency start from defaults that have not yet been measured against Files.com's production destinations. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()

