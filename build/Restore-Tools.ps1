<#
.SYNOPSIS
Installs the pinned build and test tools into a private directory.

.DESCRIPTION
Reads build/tools.json and places each PowerShell module it pins in -ToolsPath/<Name>/<Version>, after checking the
package's SHA-256. Packages come from -PackageCachePath when it holds them, and otherwise from the PowerShell Gallery.
Nothing is installed for the user or the machine, and no module path or setting changes.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ToolsPath,

    [string] $PackageCachePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$pins = Get-Content -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath 'tools.json') -Raw | ConvertFrom-Json
foreach ($tool in $pins.modules) {
    $destination = Join-Path -Path $ToolsPath -ChildPath "$($tool.name)/$($tool.version)"
    if (Test-Path -LiteralPath (Join-Path -Path $destination -ChildPath "$($tool.name).psd1")) {
        continue
    }

    $packageName = "$($tool.name).$($tool.version).nupkg"
    $package = if ($PackageCachePath) { Join-Path -Path $PackageCachePath -ChildPath $packageName }
    $downloaded = $false
    try {
        if (-not $package -or -not (Test-Path -LiteralPath $package)) {
            $package = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "filescom-tool-$([guid]::NewGuid())-$packageName"
            $downloaded = $true
            # The Gallery can return transient HTTP errors, including 404 for an existing pinned package.
            Invoke-WebRequest -Uri "https://www.powershellgallery.com/api/v2/package/$($tool.name)/$($tool.version)" -OutFile $package -UseBasicParsing -MaximumRetryCount 3 -RetryIntervalSec 5
        }
        $actual = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash
        if ($actual -ne $tool.sha256) {
            throw "$packageName has SHA-256 $actual, not the pinned $($tool.sha256)."
        }
        New-Item -ItemType Directory -Path $destination -Force | Out-Null
        # A .nupkg is a zip archive; Expand-Archive accepts only the .zip extension.
        [System.IO.Compression.ZipFile]::ExtractToDirectory($package, $destination)
        foreach ($packagingEntry in @('[Content_Types].xml', '_rels', 'package', "$($tool.name).nuspec")) {
            $entry = Join-Path -Path $destination -ChildPath $packagingEntry
            if (Test-Path -LiteralPath $entry) {
                Remove-Item -LiteralPath $entry -Recurse -Force
            }
        }
    }
    finally {
        if ($downloaded -and (Test-Path -LiteralPath $package)) {
            Remove-Item -LiteralPath $package -Force -ErrorAction SilentlyContinue
        }
    }
}
