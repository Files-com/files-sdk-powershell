# FilesCom.PowerShell module

Manage a [Files.com](https://www.files.com) site from PowerShell: users, groups, permissions, folders, automations,
remote servers and every other resource of the Files.com API, plus parallel file uploads, including streams of
unknown length such as a program's output, and safe downloads.

```powershell
Register-PSResourceRepository -Name NuGetGallery -Uri https://api.nuget.org/v3/index.json
Install-PSResource -Name FilesCom.PowerShell -Repository NuGetGallery -Version <version> -Scope CurrentUser
Import-Module FilesCom.PowerShell -RequiredVersion <version>

Connect-FilesAccount -ApiKey (Get-Secret -Name FilesComApiKey) -BaseUrl https://mysite.files.com
Get-FilesUser | Where-Object SiteAdmin | Select-Object Username, Email, LastLoginAt
Send-FilesFile -Path ./report.csv -RemotePath reports/2026/report.csv -MkdirParents
Receive-FilesFile -RemotePath reports/2026/report.csv -LiteralPath ./report.csv
```

The module is published on NuGet.org, not the PowerShell Gallery, as the binary package `FilesCom.PowerShell`:
register NuGet.org once, then install an exact version. The package holds every .NET assembly the module needs, so it
depends on no other module, and its version is its own, independent of the Files.com .NET SDK's. The
[installation guide](docs/guide.md) covers installing from NuGet.org or from a downloaded `.nupkg`, updating,
uninstalling and troubleshooting.

The module runs on the Files.com .NET SDK, which it ships privately in its own load context, so it never conflicts
with assemblies that other modules, such as Az or AWS.Tools, load.

## Requirements

- Windows, with PowerShell 7.4 or later. Windows PowerShell 5.1 is not supported. The module is not supported on macOS or
  Linux; use the Files.com CLI or a language SDK there.
- Microsoft.PowerShell.PSResourceGet to install the module, which PowerShell 7.4 and later include.
- No script execution: the module's root is its .NET Standard assembly, so `Import-Module FilesCom.PowerShell` runs no
  script and works under any execution policy, including Windows' default `Restricted`.
- A Files.com API key, or a username and password.

## Commands

Every Files.com API operation has a command, named with a standard PowerShell verb and a `Files` noun:

| API | Command |
| --- | --- |
| List users, or get one | `Get-FilesUser`, `Get-FilesUser -Id 5` |
| Create, update, delete | `New-FilesUser`, `Set-FilesUser`, `Remove-FilesUser` |
| Actions | `Unlock-FilesUser`, `Start-FilesAutomation`, `Copy-FilesFile`, `Expand-FilesArchive`, ... |
| Folder contents | `Get-FilesChildItem -Path reports` |

`Get-Command -Module FilesCom.PowerShell` lists them all, and `Get-Help <command> -Full` documents each command,
parameter and example. `Get-Help about_FilesCom` explains connections, paging, errors, previewing changes and
transfers.

## Connecting

`Connect-FilesAccount` connects with an API key, a session ID, or a credential, and makes the connection the default
of the current runspace. Commands can also take any connection with `-Connection`, so a script can work with several
sites at once. Nothing is saved between sessions, and no environment variable or profile is read.

A connection also sets how the engine sends requests: `-MaxNetworkRetries` and the retry delays, network timeouts
such as `-ReadTimeoutSeconds`, a `-Proxy` or an `HttpMessageHandler` of your own, and `-RegionRelocation`: whether a
command moves on when Files.com answers that the site is served from another region's address (`Automatic`, the
default, `Disabled`, or only between listed origins). With `-Verbose`, commands show the engine's messages about their
requests and retries, at or above the connection's `-LogLevel`. `Disconnect-FilesAccount` cancels the operations using
the connection and closes its network connections before it returns; it does not wait for each command to end.

## Pipelines

Commands write typed objects and accept them from the pipeline:

```powershell
# Disable users who have not signed in this year, previewing first.
Get-FilesUser -Filter @{ disabled = $false } |
    Where-Object { $_.LastLoginAt -lt [datetime]::new((Get-Date).Year, 1, 1) } |
    Set-FilesUser -Disabled $true -WhatIf
```

Lists load one page at a time as the pipeline consumes them; `-First` and `Select-Object -First` stop further pages.

File parameters, such as a user's avatar or the site's logo, take a local file from `Get-Item` or an attachment
`New-FilesAttachment` makes from a file, bytes or a stream:

```powershell
Set-FilesUser -Id 42 -AvatarFile (New-FilesAttachment -Path ./avatars/ana.png -ContentType image/png)
```

## Uploads

`Send-FilesFile` sends parts in parallel where Files.com allows, retries failed parts and adapts how many parts it
sends at once to what it measures. Where an upload's parts go straight to Amazon S3 as an ordinary multipart upload,
which the engine recognizes from the part URLs Files.com gives, it starts from defaults tuned for S3 and also chooses
how large the parts are, within its own planning limits for S3; elsewhere it starts from generic defaults and sends
the parts Files.com plans. To S3 it probes the link from one part at a time by default, so a slow link is not flooded;
`-AggressiveStartup`, on `Send-FilesFile` and `Send-FilesDirectory`, starts at the S3 policy's 150 parts at once
instead, and adapts from there. Use it on a known high-throughput network, such as a high-capacity cloud VM, to skip
the initial concurrency probes. The default start is not a lower performance level: it can reach the same eventual
throughput and concurrency ceiling, and only its ramp-up takes longer. Leave it off when gradual startup helps avoid
overwhelming a residential or constrained connection. Both keep concurrency limits and adaptive backoff. For rollout and diagnostics,
`New-FilesAutomaticConcurrency` and `New-FilesAutomaticPartSizing` make expert tuning for `-AutomaticConcurrency` and
`-AutomaticPartSizing`, and `-MaxReadyParts` and `-MaxReadyBytes` bound the parts read ahead. With `-PassThru` the
result's `Metrics` say what the upload measured, and `-Diagnostics` writes a record of every part request and decision
to the Information stream:

```powershell
$tuning = New-FilesAutomaticConcurrency -Profile S3 -StartTarget 4 -MaxTarget 16
Send-FilesFile ./backup.tar -RemotePath backups/backup.tar -AutomaticConcurrency $tuning -Diagnostics -InformationVariable trace -PassThru
```

With `-CheckpointPath`, an interrupted upload, or a download bound to a version, continues where it stopped when the
same command runs again, and `Send-FilesDirectory` and `Receive-FilesDirectory` transfer a whole folder, several files
at once, with `-CheckpointDirectory` to continue it. `Receive-FilesFile` downloads a file whose storage is an Amazon S3
endpoint over HTTPS in ranged parts, all of the version its storage answers the first part with, and fails with
`Files.SourceChanged` rather than mixing versions; from other storage a file comes whole, in one request. A downloaded
file is put in place under its name only once complete, and replaces a file already there only with `-Force`, never a
folder or link. `Test-FilesPath`, `Get-FilesFileContent`, `Set-FilesFileContent` and `Wait-FilesFileAction` use remote
files the way local ones are, and wait for background copies, moves and deletes. `Open-FilesFileReadStream` and
`Open-FilesFileWriteStream` open a remote file as a forward-only stream, read or written a piece at a time with bounded
memory; closing a stream opened to write completes the file unless `Abandon()` was called first.

Timestamp checks are enabled by default. Set `-CheckDownloadModificationTime:$false` on
`Connect-FilesAccount` to disable them for that connection. `Receive-FilesFile` inherits the connection's
setting unless the same Boolean parameter overrides it for that download, including `-OutputStream`:

```powershell
Receive-FilesFile -RemotePath reports/latest.csv -LiteralPath ./latest.csv -CheckDownloadModificationTime:$false
```

The value is fixed when the download starts. Disabling it suppresses automatic date headers and timestamp
comparisons, so changes detectable only by timestamps can go unnoticed. Strong ETag, size, concurrency
and checkpoint checks still apply.

Downloads also keep the first valid `Last-Modified` response date and send it as
`If-Unmodified-Since` on subsequent requests and retries, including after URL renewal, unless a stronger
`If-Match` condition is used. HTTP `412`, or a response with a newer date, stops the download with
`Files.SourceChanged`; run the command again to fetch the current file. Dates are best effort at one-second
precision: preserved modification times, changes within the same second and edits during a response are
not detected. Missing or invalid dates remain compatible. Files Agents may reject an unchanged file with
a future modification time; correct that timestamp or wait until it is no longer in the future. Date checks do not enable parallel parts or
checkpoint resume, which still need a strong version. Bytes already written to a supplied stream cannot
be taken back, and an interrupted unbound response is not replayed after writing any bytes.

## Examples

The module installs complete scripts for common tasks in its `examples` folder: exporting a user and permission
inventory, provisioning project members from a CSV with a preview that contacts no one, uploading a folder
unattended while reporting uncertain completions instead of uploading again, streaming a program's output, such as a
database export, to Files.com within a declared size, published under its final name only when the program succeeds,
applying a site's logo, icons and login background from a folder of images in one change, and exporting an upload's
diagnostics to a report that can be shared with support. They run wherever the module does, and the module's tests run
each of them from the installed package.

## Building and testing this module

The module is generated by the Files.com SDK generator. `build.sh` compiles it, generates its help and packs it;
`test.sh` analyzes its scripts and sources, installs the module from the package, and runs its Pester tests against
that installed module on each PowerShell version given. Both scripts list the pinned tools and environment they need,
and fail when any is missing.

`verification/catalogue/journeys.ps1` is the module's adapter for the SDK scenario catalogue
(`verification/catalogue` in the generator): it drives an installed module's public commands against the Files.com API
simulator and checks the simulator's journal, stored bytes and state, then prints one JSON report of the catalogue rows
it passed. The catalogue runner starts it with `--adapter powershell`.

Each built module carries `ThirdPartyNotices.txt`, naming every package it ships with that package's license, with the
license and notice texts the packages carry or, recorded in `build/notices` with their provenance, their sources
provide, and
`FilesCom.BuildInfo.json`, recording the schema, engine package and dependencies it was built from and the SHA-256 of
each of its files. Two builds of the same sources produce identical module files; the `.nupkg` around them differs
only in the packaging metadata NuGet writes on every pack.
