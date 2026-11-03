<#
.SYNOPSIS
Uploads every file of a local folder to a remote folder, unattended, and reports what happened to each.

.DESCRIPTION
Connects with an API key and uploads the folder's files one after another. Each file's parts go in parallel within one
shared budget of requests and memory. The script writes one result per file, whose Status is one of:

- Uploaded: Files.com confirmed the upload.
- Uploaded (confirmed by checksum): the answer to the upload's final request was lost, but Files.com reports a SHA-256
  or MD5 checksum for the remote file that equals the local file's, so the remote file holds exactly these bytes.
- Uncertain: the answer to the final request was lost and the remote file could not be shown to hold these bytes. It
  may be an older version, still being processed, or missing; the Reason says what was found. The script never uploads
  such a file again by itself, which could duplicate or overwrite work Files.com already accepted: check the file, for
  example with Get-FilesFile, before uploading it again.
- Failed: the upload failed.

The script exits with code 1 when any file failed or is uncertain, so a scheduler reports the run as unsuccessful. It
runs on PowerShell 7.4 or later.

.EXAMPLE
./Send-FilesNightlyExport.ps1 -ApiKey (Get-Secret -Name FilesComApiKey) -BaseUrl https://mysite.files.com -LocalFolder D:\exports -RemoteFolder exports/nightly
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [securestring] $ApiKey,

    [Parameter(Mandatory)]
    [string] $BaseUrl,

    [Parameter(Mandatory)]
    [string] $LocalFolder,

    [Parameter(Mandatory)]
    [string] $RemoteFolder
)

$ErrorActionPreference = 'Stop'

# Why the remote file cannot be shown to hold the local file's bytes, or nothing when its checksum proves it does.
function Get-ContentMismatch {
    param($File, [string] $RemotePath, $Connection)
    try {
        $remote = Get-FilesFile -Path $RemotePath -Connection $Connection -ErrorAction Stop
    }
    catch {
        return "the remote file could not be read: $($_.Exception.Message)"
    }
    foreach ($algorithm in 'SHA256', 'MD5') {
        $reported = if ($algorithm -eq 'SHA256') { $remote.Sha256 } else { $remote.Md5 }
        if ($reported) {
            if ($reported -eq (Get-FileHash -LiteralPath $File.FullName -Algorithm $algorithm).Hash) {
                return $null
            }
            return "the remote file's $algorithm checksum differs from the local file's"
        }
    }
    return "Files.com has not reported a checksum for the remote file yet (remote size $($remote.Size), modified $($remote.Mtime))"
}

$connection = Connect-FilesAccount -ApiKey $ApiKey -BaseUrl $BaseUrl -OperationTimeout (New-TimeSpan -Hours 2) -NoDefault
$limiter = New-FilesTransferLimiter -MaxConcurrentRequests 8
try {
    $results = foreach ($file in Get-ChildItem -LiteralPath $LocalFolder -File) {
        $remotePath = "$RemoteFolder/$($file.Name)"
        $result = [pscustomobject]@{ File = $file.Name; Size = $file.Length; Status = 'Uploaded'; Reason = $null }
        try {
            Send-FilesFile -LiteralPath $file.FullName -RemotePath $remotePath -MkdirParents -Connection $connection -Limiter $limiter -Confirm:$false -ErrorAction Stop
        }
        catch {
            if ($_.FullyQualifiedErrorId -like 'Files.TransferCompletionUnknown*') {
                $mismatch = Get-ContentMismatch -File $file -RemotePath $remotePath -Connection $connection
                $result.Status = if ($mismatch) { 'Uncertain' } else { 'Uploaded (confirmed by checksum)' }
                $result.Reason = $mismatch
            }
            else {
                $result.Status = 'Failed'
                $result.Reason = $_.Exception.Message
            }
        }
        $result
    }
}
finally {
    Disconnect-FilesAccount -Connection $connection -Confirm:$false
}

$results
if (@($results | Where-Object { $_.Status -in 'Failed', 'Uncertain' }).Count) {
    exit 1
}
