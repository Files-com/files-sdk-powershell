<#
.SYNOPSIS
Restores the FilesCom package engine.json names from NuGet and pins the build to it.

.DESCRIPTION
The publish workflow runs this before build.sh. It waits until NuGet serves FilesCom at the version engine.json names,
which files-sdk-dotnet's publish workflow may still be publishing, and saves it in -FeedPath as
FilesCom.<version>.nupkg, the feed build.sh reads as FILESCOM_ENGINE_FEED. It then records the package's SHA-256 in
engine.json, and its version and content hash in the engine lock, as build.sh requires. NuGet leaves the signature
nuget.org adds to every package out of a signed package's content hash, so the content hash is the one a restore of the
package records.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $FeedPath,
    [int] $TimeoutMinutes = 60
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Path $PSScriptRoot -Parent
$enginePath = Join-Path -Path $root -ChildPath 'engine.json'
$lockPath = Join-Path -Path $root -ChildPath 'src/FilesCom.PowerShell.Engine/packages.lock.json'
$engine = Get-Content -LiteralPath $enginePath -Raw | ConvertFrom-Json -AsHashtable
$version = $engine['version']
$lowerVersion = $version.ToLowerInvariant()
$url = "https://api.nuget.org/v3-flatcontainer/filescom/$lowerVersion/filescom.$lowerVersion.nupkg"

New-Item -ItemType Directory -Path $FeedPath -Force | Out-Null
$package = Join-Path -Path $FeedPath -ChildPath "FilesCom.$version.nupkg"
$deadline = (Get-Date).AddMinutes($TimeoutMinutes)
while ($true) {
    try {
        Invoke-WebRequest -Uri $url -OutFile $package -UseBasicParsing
        break
    }
    catch {
        if ((Get-Date) -ge $deadline) {
            throw "NuGet did not serve FilesCom $version within $TimeoutMinutes minutes: $_"
        }
        Write-Information "Waiting for NuGet to serve FilesCom $version" -InformationAction Continue
        Start-Sleep -Seconds 30
    }
}

$bytes = [System.IO.File]::ReadAllBytes($package)
$engine['sha256'] = [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
$engine['source'] = $url
Set-Content -LiteralPath $enginePath -Value (ConvertTo-Json -InputObject $engine) -Encoding utf8

$probeProject = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>netstandard2.0</TargetFramework></PropertyGroup>
  <ItemGroup><PackageReference Include="FilesCom" Version="[$version]" /></ItemGroup>
</Project>
"@
$probe = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "filescom-engine-probe-$([guid]::NewGuid())"
try {
    New-Item -ItemType Directory -Path $probe | Out-Null
    $project = Join-Path -Path $probe -ChildPath 'probe.csproj'
    Set-Content -LiteralPath $project -Value $probeProject -Encoding utf8
    $packages = Join-Path -Path $probe -ChildPath 'packages'
    & dotnet restore $project --packages $packages --source $FeedPath --source 'https://api.nuget.org/v3/index.json' | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Restoring FilesCom $version from $FeedPath failed with exit code $LASTEXITCODE."
    }
    $metadata = Get-Content -LiteralPath (Join-Path -Path $packages -ChildPath "filescom/$lowerVersion/.nupkg.metadata") -Raw | ConvertFrom-Json
    $contentHash = $metadata.contentHash
}
finally {
    Remove-Item -LiteralPath $probe -Recurse -Force -ErrorAction SilentlyContinue
}

$lock = Get-Content -LiteralPath $lockPath -Raw | ConvertFrom-Json -AsHashtable
foreach ($framework in $lock['dependencies'].Values) {
    $entry = $framework['FilesCom']
    $entry['requested'] = "[$version, )"
    $entry['resolved'] = $version
    $entry['contentHash'] = $contentHash
}
Set-Content -LiteralPath $lockPath -Value (ConvertTo-Json -InputObject $lock -Depth 32) -Encoding utf8
Write-Information "Pinned the build to FilesCom $version from $url" -InformationAction Continue
