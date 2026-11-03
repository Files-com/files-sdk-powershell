# Installing and using FilesCom.PowerShell

FilesCom.PowerShell is a binary PowerShell module for Files.com. It is published on NuGet.org, not the PowerShell
Gallery, as a package whose ID is also the module's name: `FilesCom.PowerShell`. The package holds every .NET assembly
the module needs, including the Files.com .NET SDK, so it depends on no other PowerShell module; NuGet.org's v3 feed
does not install a PowerShell resource's dependencies automatically, which is why they are all in the package. The
module's version is its own, independent of the version of the Files.com .NET SDK it ships.

In the commands below, replace `<version>` with the exact version you want, as listed on the
[package's page](https://www.nuget.org/packages/FilesCom.PowerShell).

## Prerequisites

- Windows, with PowerShell 7.4 or later. `$PSVersionTable.PSVersion` shows the version of the current session. The
  module is not supported on macOS or Linux; use the Files.com CLI or a language SDK there.
- Windows PowerShell 5.1 is not supported. The module's manifest requires PowerShell 7.4 and lists only the Core
  edition, so Windows PowerShell does not import it; use `pwsh`.
- Microsoft.PowerShell.PSResourceGet, which provides `Install-PSResource` and the other commands used here. PowerShell
  7.4 and later include it: `Get-Module -ListAvailable Microsoft.PowerShell.PSResourceGet` shows it, and where it is
  missing, `Install-Module Microsoft.PowerShell.PSResourceGet` adds it.
- Access to `https://api.nuget.org`, or a package downloaded on another computer: see
  [Installing from a downloaded package](#installing-from-a-downloaded-package).
- A Files.com API key, kept in a secret vault, such as with Microsoft.PowerShell.SecretManagement, or a username and
  password.

## Installing from NuGet.org

Register NuGet.org as a PSResourceGet repository, once per user:

```powershell
Register-PSResourceRepository -Name NuGetGallery -Uri https://api.nuget.org/v3/index.json
```

`Get-PSResourceRepository` lists the repositories already registered; if NuGet.org is there under another name, give
that name to `-Repository` below. Microsoft describes NuGet.org as a PSResourceGet repository in
[Supported repositories](https://learn.microsoft.com/powershell/gallery/powershellget/supported-repositories).

Optionally, look the package up first. Use its exact name: wildcard searches and searches by command name are limited
on NuGet.org and may not find it.

```powershell
Find-PSResource -Name FilesCom.PowerShell -Repository NuGetGallery
```

Install an exact version for the current user:

```powershell
Install-PSResource -Name FilesCom.PowerShell -Repository NuGetGallery -Version <version> -Scope CurrentUser
```

A repository registered this way is untrusted, so `Install-PSResource` asks before it installs from it. Answer once you
have checked the name, version and owners on the package's page. Trusting the repository, with
`Set-PSResourceRepository -Name NuGetGallery -Trusted` or `Install-PSResource -TrustRepository`, stops that question
for every package from NuGet.org; whether to do so is your decision.

Import the version you installed, list its commands and read its help:

```powershell
Import-Module FilesCom.PowerShell -RequiredVersion <version>
Get-Command -Module FilesCom.PowerShell
Get-Help <command> -Full
Get-Help about_FilesCom
```

`Get-Help <command> -Full`, such as `Get-Help Send-FilesFile -Full`, documents a command's parameters and examples, and
`about_FilesCom` explains connections, paging, errors, previewing changes and transfers. Importing the module runs no
script: its root is a binary assembly. The example scripts run only where the execution policy allows them.

## Installing from a downloaded package

Where registering NuGet.org is not wanted, or `api.nuget.org` cannot be reached, download the package itself: open
`https://www.nuget.org/packages/FilesCom.PowerShell/<version>` and choose **Download package**. The file is named like
`filescom.powershell.<version>.nupkg`. Then either install it with PSResourceGet from a local folder, or extract it
once.

### Install it from a local folder

Put the package in a folder of its own, register that folder as a repository, and install from it. PSResourceGet then
installs, updates and uninstalls the module as it does one from NuGet.org:

```powershell
$packages = Join-Path $HOME 'FilesComPackages'
New-Item -ItemType Directory -Path $packages
Move-Item -LiteralPath ./filescom.powershell.<version>.nupkg -Destination $packages
Register-PSResourceRepository -Name FilesComLocal -Uri $packages
Install-PSResource -Name FilesCom.PowerShell -Repository FilesComLocal -Version <version> -Scope CurrentUser
Import-Module FilesCom.PowerShell -RequiredVersion <version>
```

This repository is untrusted too, so the install asks first. For a later version, download it into the same folder and
install it the same way.

### Extract it once

A `.nupkg` is a zip archive. Copy it to a `.zip` file and expand it into a folder named `FilesCom.PowerShell/<version>`,
the version exactly as the package names it, in your per-user folder of `$env:PSModulePath`:

```powershell
$userModules = ($env:PSModulePath -split [System.IO.Path]::PathSeparator)[0]
$target = Join-Path $userModules 'FilesCom.PowerShell' '<version>'
Copy-Item -LiteralPath ./filescom.powershell.<version>.nupkg -Destination ./filescom.powershell.<version>.zip
Expand-Archive -LiteralPath ./filescom.powershell.<version>.zip -DestinationPath $target
Import-Module FilesCom.PowerShell -RequiredVersion <version>
```

The first entry of `$env:PSModulePath` is the per-user folder unless your profile or environment changes it, so check
`$userModules` before expanding: by default it is under `Documents\PowerShell\Modules`. The archive also holds NuGet's
own files, such as
`[Content_Types].xml` and `_rels`, which PowerShell does not use. PSResourceGet does not manage a module installed this
way: to update or remove it, replace or delete its version folder.

## Updating

Versions install side by side. Install the new version exactly, or let PSResourceGet install the newest one, then list
what is installed:

```powershell
Install-PSResource -Name FilesCom.PowerShell -Repository NuGetGallery -Version <version> -Scope CurrentUser
Update-PSResource -Name FilesCom.PowerShell -Repository NuGetGallery -Scope CurrentUser
Get-InstalledPSResource -Name FilesCom.PowerShell
```

Then start a new PowerShell session. A session that imported the module keeps its .NET assemblies loaded, and they
cannot be unloaded, so neither `Remove-Module` nor a second `Import-Module` switches versions. In the new session,
choose the version:

```powershell
Import-Module FilesCom.PowerShell -RequiredVersion <version>
```

Without `-RequiredVersion`, PowerShell loads the highest version installed.

## Uninstalling

Close the PowerShell sessions that imported the version, then remove it:

```powershell
Uninstall-PSResource -Name FilesCom.PowerShell -Version <version> -Scope CurrentUser
```

Repeat for each version `Get-InstalledPSResource -Name FilesCom.PowerShell` lists. A module extracted by hand is
removed by deleting its version folder. `Unregister-PSResourceRepository -Name NuGetGallery` removes the repository
once no other package needs it.

## Troubleshooting

**The package is not found.** Check that:

- NuGet.org is registered: `Get-PSResourceRepository` lists `NuGetGallery` with the URI above.
- The command names it with `-Repository NuGetGallery`. The module is not in the default PowerShell Gallery, and
  `Install-Module`, from the older PowerShellGet, keeps its own list of repositories, which does not include
  `NuGetGallery`: use `Install-PSResource`.
- The name is exactly `FilesCom.PowerShell`, not `FilesCom`, and the version is one the package's page lists.

**A search finds nothing.** Wildcard searches, such as `Find-PSResource -Name FilesCom*`, and searches by command name
are limited on NuGet.org. Search, and install, by the exact name.

**Windows PowerShell 5.1.** The module is not supported there and does not import. `$PSVersionTable.PSEdition` is
`Desktop` in Windows PowerShell; install and use the module from `pwsh`, PowerShell 7.4 or later, where it is `Core`.

**Another version is already loaded.** A session uses the version it imported first: its assemblies stay loaded after
`Remove-Module`. `Get-Module FilesCom.PowerShell` shows the version loaded; start a new session and import the version
you want.

**A proxy, or no internet access.** Where `api.nuget.org` cannot be reached, download the package on a computer that
can reach NuGet.org and install it as described in
[Installing from a downloaded package](#installing-from-a-downloaded-package).

## First steps

Connect with an API key from a secret vault, using Microsoft.PowerShell.SecretManagement and a vault you have
registered:

```powershell
$apiKey = Get-Secret -Name FilesComApiKey
Connect-FilesAccount -ApiKey $apiKey -BaseUrl https://mysite.files.com
```

Without a vault, type the key at a prompt, which keeps it out of the command history:

```powershell
Connect-FilesAccount -ApiKey (Read-Host -AsSecureString 'Files.com API key') -BaseUrl https://mysite.files.com
```

List a folder, which changes nothing:

```powershell
Get-FilesChildItem -Path reports | Select-Object DisplayName, Type
```

Preview an upload, upload the file, then download it again:

```powershell
Send-FilesFile -Path ./report.csv -RemotePath reports/2026/report.csv -MkdirParents -WhatIf
Send-FilesFile -Path ./report.csv -RemotePath reports/2026/report.csv -MkdirParents
Receive-FilesFile -RemotePath reports/2026/report.csv -LiteralPath ./report-copy.csv
```

`-WhatIf` describes the upload without contacting Files.com. `Receive-FilesFile` fails rather than replace something
already at `./report-copy.csv`; `-Force` lets it replace a regular file. `Disconnect-FilesAccount` closes the
connection when you are done.

## More

- [Introduction](https://developers.files.com/powershell/overview/introduction),
  [Installation](https://developers.files.com/powershell/overview/installation),
  [Authentication](https://developers.files.com/powershell/overview/authentication) and
  [Transfers](https://developers.files.com/powershell/overview/transfers) on developers.files.com.
- [Command reference](https://developers.files.com/powershell/commands): every command, also shown by
  `Get-Help <command> -Full`.
- `Get-Help about_FilesCom`: connections, paging, errors, previewing changes and transfers.
- The installed module's `examples` folder: complete scripts for common tasks.
  `Get-ChildItem (Join-Path (Get-Module FilesCom.PowerShell).ModuleBase examples)` lists them once the module is
  imported, and `Get-Help <path to script> -Full` documents each one.
