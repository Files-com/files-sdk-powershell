<#
.SYNOPSIS
Streams a program's output, such as a database export, straight to Files.com within a declared size, and publishes it
under its final name only when the program succeeds.

.DESCRIPTION
Runs the program and uploads its standard output as the program writes it, with no local copy of the whole export.
Its length is not known in advance, so Send-FilesFile reads it part by part and stops at -MaxLength bytes. Each part
waits for Files.com in a private temporary file of its own, deleted once the part is accepted, so memory does not
limit how large the parts can be.

The output is uploaded to "<RemotePath>.partial". An upload of a program's output completes when the program closes
its output, which a program that fails partway also does, so only its exit code shows whether the export is whole: on
exit code 0 the script moves the partial file over -RemotePath; otherwise it deletes the partial file and fails,
leaving any previous export at -RemotePath as it was.

When the output is longer than -MaxLength, the upload stops without completing, the script stops the program and
fails with the error ID Files.LimitExceeded, and nothing is created on Files.com. When the upload fails in any other
way the script also stops the program and fails with that error; if the failure is Files.TransferCompletionUnknown, a
partial file may remain, which the next run replaces. Runs of the same export share the partial file's name, so they
must not overlap.

Stopping the script, as with Ctrl+C or Stop-Job, stops the upload, which is not completed, and the program; nothing is
published. The script owns the program it starts: however the script ends, it stops the program if it is still running,
waits for it to exit and releases it and its redirected output. Processes the program starts itself are the program's
to stop. If the script is stopped after the upload completed but before the move, the partial file remains, and the
next run replaces it.

The script writes one result with the export's size, its parts, how Files.com let the upload size them, and the
move's Status: Published, or Moving when Files.com finishes the move in the background, with the FileMigrationId to
follow with Get-FilesFileMigration.

With -WhatIf the program is not started and nothing is sent. The script runs on PowerShell 7.4 or later.

.EXAMPLE
$connection = Connect-FilesAccount -ApiKey (Get-Secret -Name FilesComApiKey) -BaseUrl https://mysite.files.com -NoDefault
./Send-FilesLiveExport.ps1 -FilePath pg_dump -Arguments '--format=custom --dbname=sales' -RemotePath backups/sales.dump -MaxLength 200GB -Connection $connection
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    # The program that writes the export to its standard output.
    [Parameter(Mandatory)]
    [string] $FilePath,

    # The program's command line arguments, as one string.
    [string] $Arguments = '',

    # The remote path to publish the export at.
    [Parameter(Mandatory)]
    [string] $RemotePath,

    # The most bytes the export may hold.
    [Parameter(Mandatory)]
    [ValidateRange(1, [long]::MaxValue)]
    [long] $MaxLength,

    # The existing local folder for the parts' temporary files; defaults to the user's temporary folder.
    [string] $TemporaryDirectory,

    # The connection to use.
    [Parameter(Mandatory)]
    [FilesCom.PowerShell.FilesConnection] $Connection
)

$ErrorActionPreference = 'Stop'
$partial = "$RemotePath.partial"
if (-not $PSCmdlet.ShouldProcess($RemotePath, "Upload the output of $FilePath")) {
    return
}

$start = New-Object -TypeName System.Diagnostics.ProcessStartInfo
$start.FileName = $FilePath
$start.Arguments = $Arguments
$start.UseShellExecute = $false
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$process = $null
try {
    $process = [System.Diagnostics.Process]::Start($start)
    # The program's error output is read meanwhile, so that a program writing much of it never waits for a reader.
    $errorOutput = $process.StandardError.ReadToEndAsync()
    $upload = @{
        InputStream = $process.StandardOutput.BaseStream
        RemotePath = $partial
        MaxLength = $MaxLength
        PartStorage = 'TemporaryFiles'
        MkdirParents = $true
        Connection = $Connection
    }
    if ($TemporaryDirectory) {
        $upload.TemporaryDirectory = $TemporaryDirectory
    }
    $result = Send-FilesFile @upload -PassThru -Confirm:$false
    # Waiting in short steps lets the script be stopped while a program that closed its output keeps running.
    while (-not $process.WaitForExit(200)) {
    }
    if ($process.ExitCode -ne 0) {
        Remove-FilesFile -Path $partial -Connection $Connection -Confirm:$false
        throw "$FilePath exited with code $($process.ExitCode), so its output was deleted instead of published. $($errorOutput.Result.Trim())"
    }

    $move = Move-FilesFile -Path $partial -Destination $RemotePath -Overwrite $true -Connection $Connection -Confirm:$false
    [pscustomobject]@{
        RemotePath = $RemotePath
        Size = $result.Size
        Parts = $result.Parts.Count
        PartSizeSource = $result.PartSizeSource
        ServerPlannedReason = $result.ServerPlannedReason
        Status = if ($move.Status -eq 'pending') { 'Moving' } else { 'Published' }
        FileMigrationId = $move.FileMigrationId
    }
}
finally {
    # Runs however the script ends, including when its pipeline is stopped, which no catch block sees. A program still
    # running may be waiting to write output that no one will read.
    if ($process) {
        if (-not $process.HasExited) {
            try {
                $process.Kill()
            }
            catch [System.InvalidOperationException] {
                # It exited meanwhile, which .NET Framework reports this way.
                $process.Refresh()
            }
        }
        $process.WaitForExit()
        $process.StandardOutput.Dispose()
        $process.StandardError.Dispose()
        $process.Dispose()
    }
}
