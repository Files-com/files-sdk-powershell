<#
.SYNOPSIS
Uploads a file with its diagnostics captured and exports what the upload decided and measured as a JSON report.

.DESCRIPTION
Uploads -Path to -RemotePath with Send-FilesFile -Diagnostics, then writes a JSON report of the upload: its size, time
and throughput; the most parts it sent at once and where its tuning came from; each kind of concurrency decision it made,
such as growing or shrinking after throttling, and the pauses throttling caused; its part size decisions; and every part
it sent more than once, with the status of each attempt. The report also says whether every record was captured.

The report holds only the fields named here: the remote path, times, counts, statuses and decisions. It never holds the
connection, credentials, storage addresses or file contents, so it can be shared with support. Out-FilesLocalFile writes
it in one complete step: a file already at -OutputPath is replaced only once the report is complete, and nothing else in
its folder is changed. An upload that fails is reported too, with the error's ID, before the script fails with that
error.

The upload's outcome is never lost to the report. If the report cannot be written after an upload fails, the script
writes that as a non-terminating ReportNotWritten error, even under -ErrorAction Stop, and then fails with the upload's
own error, such as Files.TransferCompletionUnknown for an upload that may have completed. If the report cannot be
written after an upload completes, the script fails with a ReportNotWritten error whose TargetObject is the upload's
result.

-WhatIf previews the upload without reading the file, contacting Files.com or writing a report. Measurements describe the
connection the upload had: a report made against a test or local server says nothing about production throughput. The
script runs on PowerShell 7.4 or later.

.EXAMPLE
./Export-FilesUploadDiagnostics.ps1 -Path ./backup.tar -RemotePath backups/backup.tar -OutputPath ./backup-upload.json

.EXAMPLE
$tuning = New-FilesAutomaticConcurrency -Profile Agent -BackpressurePauseSeconds 5
./Export-FilesUploadDiagnostics.ps1 -Path ./backup.tar -RemotePath backups/backup.tar -OutputPath ./agent.json -AutomaticConcurrency $tuning
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    # The local file to upload.
    [Parameter(Mandatory)]
    [string] $Path,

    # The remote path to upload it to.
    [Parameter(Mandatory)]
    [string] $RemotePath,

    # The JSON report to write.
    [Parameter(Mandatory)]
    [string] $OutputPath,

    # Expert concurrency tuning, from New-FilesAutomaticConcurrency, to diagnose.
    [FilesCom.PowerShell.FilesAutomaticConcurrency] $AutomaticConcurrency,

    # Expert part sizing tuning, from New-FilesAutomaticPartSizing, to diagnose.
    [FilesCom.PowerShell.FilesAutomaticPartSizing] $AutomaticPartSizing,

    # The most records to capture; the report says whether any were left out.
    [ValidateRange(1, 100000)]
    [int] $DiagnosticLimit = 10000,

    # The connection to use; defaults to the current default connection.
    [FilesCom.PowerShell.FilesConnection] $Connection
)

$upload = @{ LiteralPath = $Path; RemotePath = $RemotePath; Diagnostics = $true; DiagnosticLimit = $DiagnosticLimit; PassThru = $true; InformationVariable = 'trace' }
foreach ($name in 'AutomaticConcurrency', 'AutomaticPartSizing', 'Connection') {
    if ($PSBoundParameters.ContainsKey($name)) {
        $upload[$name] = $PSBoundParameters[$name]
    }
}

$failure = $null
$result = $null
try {
    $result = Send-FilesFile @upload -ErrorAction Stop
}
catch {
    $failure = $_
}
$records = @($trace | ForEach-Object { $_.MessageData })
$summary = $records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadTraceSummary] } | Select-Object -Last 1
if (-not $summary) {
    # Nothing was uploaded: previewed with -WhatIf, declined, or refused before it started.
    if ($failure) {
        $PSCmdlet.ThrowTerminatingError($failure)
    }
    return
}

$reportFailure = $null
try {
    # Each field is chosen here, as a string or a number: no object of the upload is written as it is.
    $decisions = [ordered]@{}
    foreach ($record in $records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadDecisionTrace] }) {
        $decisions["$($record.Decision)"] = 1 + [int]$decisions["$($record.Decision)"]
    }
    $attempts = @($records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadAttemptTrace] })
    $retried = foreach ($part in $attempts | Group-Object PartNumber | Where-Object Count -GT 1) {
        [ordered]@{
            PartNumber = [long]$part.Name
            Attempts = @($part.Group | Sort-Object Attempt | ForEach-Object { [ordered]@{ Attempt = $_.Attempt; StatusCode = $_.StatusCode; Throttled = $_.Throttled; Seconds = [math]::Round($_.Duration.TotalSeconds, 3) } })
        }
    }
    $report = [ordered]@{
        RemotePath = $summary.Path
        Outcome = "$($summary.Outcome)"
        ErrorId = if ($failure) { $failure.FullyQualifiedErrorId } else { $null }
        Size = if ($result) { $result.Size } else { $null }
        ElapsedSeconds = if ($result) { [math]::Round($result.Elapsed.TotalSeconds, 3) } else { $null }
        ThroughputBytesPerSecond = if ($result) { [math]::Round($result.Metrics.ThroughputBytesPerSecond) } else { $null }
        PeakConcurrency = if ($result) { $result.PeakConcurrency } else { $null }
        PerformanceProfile = if ($result) { "$($result.PerformanceProfile)" } else { $null }
        PerformanceProfileSource = if ($result) { "$($result.PerformanceProfileSource)" } else { $null }
        PartSizeConstraint = if ($result) { "$($result.PartSizeConstraint)" } else { $null }
        LargestPartSize = if ($result) { $result.LargestPartSize } else { $null }
        Throttles = @($attempts | Where-Object Throttled).Count
        PartRetries = @($attempts | Where-Object Attempt -GT 1).Count
        ReadyLimitWaits = if ($result) { $result.Metrics.ReadyLimitWaits } else { $null }
        Decisions = $decisions
        CooldownSeconds = @($records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadDecisionTrace] -and "$($_.Decision)" -eq 'CooldownStarted' } | ForEach-Object { $_.Cooldown.TotalSeconds })
        PartSizes = @($records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadSizingTrace] } | ForEach-Object { [ordered]@{ Decision = "$($_.Decision)"; PreviousSize = $_.PreviousSize; Size = $_.Size; Constraint = "$($_.Constraint)" } })
        RetriedParts = @($retried)
        Records = [ordered]@{ Written = $summary.RecordsWritten; Dropped = $summary.RecordsDropped; Complete = $summary.Complete }
    }
    # The whole report or nothing: a file already at -OutputPath is replaced only once the report is complete.
    $report | ConvertTo-Json -Depth 6 | Out-FilesLocalFile -LiteralPath $OutputPath -Force -WhatIf:$false -Confirm:$false -ErrorAction Stop
}
catch {
    $reportFailure = $_
}

if ($reportFailure) {
    $reason = $reportFailure.Exception.Message.TrimEnd('.') + '.'
    if ($failure) {
        # The upload's error must stay the error the caller gets: it may say the upload could have completed.
        $unwritten = [System.Management.Automation.ErrorRecord]::new($reportFailure.Exception, 'ReportNotWritten', [System.Management.Automation.ErrorCategory]::WriteError, $OutputPath)
        $unwritten.ErrorDetails = [System.Management.Automation.ErrorDetails]::new("The report was not written to '$OutputPath': $reason The upload's own error follows.")
        $secondary = if ($ErrorActionPreference -eq 'Stop') { 'Continue' } else { $ErrorActionPreference }
        Write-Error -ErrorRecord $unwritten -ErrorAction $secondary
        $PSCmdlet.ThrowTerminatingError($failure)
    }
    $unwritten = [System.Management.Automation.ErrorRecord]::new($reportFailure.Exception, 'ReportNotWritten', [System.Management.Automation.ErrorCategory]::WriteError, $result)
    $unwritten.ErrorDetails = [System.Management.Automation.ErrorDetails]::new("The upload to '$($result.Path)' completed with $($result.Size) bytes, but its report was not written to '$OutputPath': $reason The upload's result is the error's TargetObject.")
    $PSCmdlet.ThrowTerminatingError($unwritten)
}
[pscustomobject]@{ RemotePath = $report.RemotePath; Outcome = $report.Outcome; Throttles = $report.Throttles; PartRetries = $report.PartRetries; Complete = $summary.Complete; Report = $OutputPath }
if ($failure) {
    $PSCmdlet.ThrowTerminatingError($failure)
}
