<#
.SYNOPSIS
Builds the FilesCom module: compiles it, lays it out, generates its help, checks it and packs it.

.DESCRIPTION
Requires the .NET SDK that global.json pins, the tools Restore-Tools.ps1 installs, and FILESCOM_ENGINE_FEED naming a
directory that holds the exact FilesCom package engine.json pins. Every step fails the build when its tool is missing
or its check fails.

The module goes to -OutputPath/FilesCom.PowerShell/<version>, and its package to -OutputPath/packages.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ToolsPath,

    [string] $OutputPath = (Join-Path -Path $PSScriptRoot -ChildPath '../out'),

    # Adds Windows PowerShell 5.1 to the module: its binaries, its manifest edition and its minimum version. Use it only
    # for builds that run the Desktop compatibility gate until that gate has passed.
    [switch] $IncludeDesktopEdition
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$version = (Get-Content -LiteralPath (Join-Path -Path $root -ChildPath '_VERSION') -Raw).Trim()
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$moduleRoot = Join-Path -Path $OutputPath -ChildPath "FilesCom.PowerShell/$version"
$staging = Join-Path -Path $OutputPath -ChildPath 'staging'

function Invoke-Checked {
    param([string] $FilePath, [string[]] $ArgumentList)
    & $FilePath @ArgumentList
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath $($ArgumentList -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Import-Tool {
    param([string] $Name, [string] $From)
    $pins = Get-Content -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath 'tools.json') -Raw | ConvertFrom-Json
    $pin = $pins.modules | Where-Object name -EQ $Name
    $manifest = Join-Path -Path $From -ChildPath "$Name/$($pin.version)/$Name.psd1"
    if (-not (Test-Path -LiteralPath $manifest)) {
        throw "$Name $($pin.version) is not in $From. Run build/Restore-Tools.ps1 first."
    }
    Import-Module -Name $manifest -RequiredVersion $pin.version -Force -ErrorAction Stop
    $manifest
}

function Assert-EnginePackage {
    $engine = Get-Content -LiteralPath (Join-Path -Path $root -ChildPath 'engine.json') -Raw | ConvertFrom-Json
    if (-not $env:FILESCOM_ENGINE_FEED) {
        throw 'FILESCOM_ENGINE_FEED must name the directory holding the FilesCom package engine.json pins.'
    }
    $package = Join-Path -Path $env:FILESCOM_ENGINE_FEED -ChildPath "FilesCom.$($engine.version).nupkg"
    if (-not (Test-Path -LiteralPath $package)) {
        throw "$package does not exist."
    }
    $actual = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash
    if ($actual -ne $engine.sha256) {
        throw "$package has SHA-256 $actual, not $($engine.sha256) as engine.json pins."
    }
    $engine
}

function Copy-AssemblyFile {
    param([string] $From, [string] $To, [string[]] $Exclude = @())
    New-Item -ItemType Directory -Path $To -Force | Out-Null
    Get-ChildItem -LiteralPath $From -Filter '*.dll' -File |
        Where-Object Name -NotIn $Exclude |
        Copy-Item -Destination $To
}

# An engine build's output: its assemblies, the deps.json its load context resolves them with, and the operating
# system specific builds under runtimes/. The contracts load beside the commands instead.
function Copy-Engine {
    param([string] $From, [string] $To)
    Copy-AssemblyFile -From $From -To $To -Exclude @('FilesCom.PowerShell.Contracts.dll')
    Get-ChildItem -LiteralPath $From -Filter '*.deps.json' -File | Copy-Item -Destination $To
    $runtimes = Join-Path -Path $From -ChildPath 'runtimes'
    if (Test-Path -LiteralPath $runtimes) {
        Copy-Item -LiteralPath $runtimes -Destination $To -Recurse
    }
}

# The Desktop allowlist: each assembly the module ships beside its Windows PowerShell commands, by file, name, version
# and public key token. The loader answers binding requests only for these.
function Write-DesktopAllowlist {
    param([string] $Directory)
    $lines = @('# File	Assembly	Version	PublicKeyToken')
    foreach ($file in Get-ChildItem -LiteralPath $Directory -Filter '*.dll' -File | Sort-Object Name) {
        $name = [System.Reflection.AssemblyName]::GetAssemblyName($file.FullName)
        $token = $name.GetPublicKeyToken()
        $tokenText = if ($token -and $token.Length) { -join ($token | ForEach-Object { $_.ToString('x2') }) } else { 'null' }
        $lines += "$($file.Name)`t$($name.Name)`t$($name.Version)`t$tokenText"
    }
    Set-Content -LiteralPath (Join-Path -Path $Directory -ChildPath 'FilesCom.PowerShell.Dependencies.txt') -Value $lines -Encoding utf8
}

# The packages the module ships: each package the locked engine restore resolved runtime files from, for the engine
# builds laid out in the module, with its license and copyright as its own package declares them.
function Get-ShippedPackage {
    param([hashtable] $Targets)
    $assets = Get-Content -LiteralPath (Join-Path -Path $root -ChildPath 'src/FilesCom.PowerShell.Engine/obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
    # NuGet takes each package from the first of these folders that holds it: the global packages folder, then the
    # fallback folders, in the order the restore recorded them.
    $packageFolders = [string[]] @($assets['packageFolders'].Keys)
    $packages = [ordered]@{}
    $targetNames = [string[]] @($Targets.Keys)
    [System.Array]::Sort($targetNames, [System.StringComparer]::Ordinal)
    foreach ($targetName in $targetNames) {
        foreach ($library in $assets['targets'][$targetName].GetEnumerator()) {
            $runtimeFiles = @(foreach ($group in 'runtime', 'runtimeTargets') {
                    if ($library.Value[$group]) { $library.Value[$group].Keys }
                }) | Where-Object { $_ -notlike '*/_._' }
            if ($library.Value['type'] -ne 'package' -or -not $runtimeFiles) {
                continue
            }
            if (-not $packages.Contains($library.Key)) {
                $details = $assets['libraries'][$library.Key]
                $nuspecName = $details['files'] | Where-Object { $_ -like '*.nuspec' }
                $directory = $packageFolders | ForEach-Object { Join-Path -Path $_ -ChildPath $details['path'] } |
                    Where-Object { $nuspecName -and (Test-Path -LiteralPath (Join-Path -Path $_ -ChildPath $nuspecName) -PathType Leaf) } |
                    Select-Object -First 1
                if (-not $directory) {
                    throw "The engine restore resolved $($library.Key), but none of its package folders ($($packageFolders -join ', ')) holds $($details['path'])/$nuspecName."
                }
                $nuspec = [xml](Get-Content -LiteralPath (Join-Path -Path $directory -ChildPath $nuspecName) -Raw)
                $metadata = { param($Name) $nuspec.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='$Name']") }
                $license = & $metadata 'license'
                $licenseUrl = & $metadata 'licenseUrl'
                $copyright = & $metadata 'copyright'
                $projectUrl = & $metadata 'projectUrl'
                $packages[$library.Key] = [ordered]@{
                    id = $library.Key.Split('/')[0]
                    version = $library.Key.Split('/')[1]
                    sha512 = $details['sha512']
                    license = if ($license) { $license.InnerText } elseif ($licenseUrl) { $licenseUrl.InnerText } else { 'not declared' }
                    copyright = if ($copyright) { $copyright.InnerText } else { $null }
                    projectUrl = if ($projectUrl) { $projectUrl.InnerText } else { $null }
                    usedBy = [System.Collections.Generic.List[string]]::new()
                    provenance = $null
                    noticeFiles = @(Get-ChildItem -LiteralPath $directory -File | Where-Object Name -Match '^(license|notice|third-party-notices)' | Sort-Object Name)
                }
            }
            $packages[$library.Key].usedBy.Add($Targets[$targetName])
        }
    }
    # By package ID, then version.
    # Texts a package's source provides where its NuGet package does not, recorded with their provenance.
    $recorded = Get-Content -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath 'notices/notices.json') -Raw | ConvertFrom-Json
    foreach ($entry in $recorded.packages) {
        $package = $packages.Values | Where-Object id -EQ $entry.id
        if (-not $package) {
            continue
        }
        if ($package.version -ne $entry.version) {
            throw "build/notices records $($entry.id) $($entry.version), but the module ships $($package.version). Record its texts from $($entry.source) at the shipped version."
        }
        foreach ($file in $entry.files.PSObject.Properties) {
            $text = Get-Item -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath "notices/$($entry.id)/$($file.Name)")
            if ((Get-FileHash -LiteralPath $text.FullName -Algorithm SHA256).Hash -ne $file.Value) {
                throw "build/notices/$($entry.id)/$($file.Name) is not the text recorded from $($entry.source) at $($entry.commit)."
            }
            $package.noticeFiles += $text
            $package.provenance = "$($entry.source) $($entry.tag) ($($entry.commit))"
        }
    }
    foreach ($package in $packages.Values | Where-Object { $_.license -eq 'Apache-2.0' -and -not $_.noticeFiles }) {
        throw "$($package.id) $($package.version) is licensed Apache-2.0 but ships no license text; record it in build/notices."
    }
    $keys = [string[]] @($packages.Keys)
    $sortKeys = [string[]] @($keys | ForEach-Object { $_.Replace('/', [char] 1) })
    [System.Array]::Sort($sortKeys, $keys, [System.StringComparer]::Ordinal)
    foreach ($key in $keys) {
        $packages[$key]
    }
}

# ThirdPartyNotices.txt lists every shipped package with its license, then gives each license or notice file those
# packages carry once. FilesCom.BuildInfo.json records the module's identity: the schema, engine and packages it was
# built from and the SHA-256 of each of its files.
function Write-ModuleIdentity {
    param($Engine, [hashtable] $Targets)
    $inventory = Get-Content -LiteralPath (Join-Path -Path $root -ChildPath 'docs/command-inventory.json') -Raw | ConvertFrom-Json
    if ($inventory.schema_sha256 -ne $Engine.schema_sha256) {
        throw "The commands were generated from schema $($inventory.schema_sha256), but engine.json pins an engine generated from $($Engine.schema_sha256)."
    }
    $packages = @(Get-ShippedPackage -Targets $Targets)

    $notices = [System.Collections.Generic.List[string]]::new()
    $notices.Add("Third-party software in the FilesCom PowerShell module $version")
    $notices.Add('')
    $notices.Add('The module ships these packages privately, as dependencies of the FilesCom .NET SDK. Each is used under the')
    $notices.Add('license its package declares. The license and notice files the packages carry, or their sources provide')
    $notices.Add('where noted, follow the list.')
    $texts = [ordered]@{}
    foreach ($package in $packages) {
        $notices.Add('')
        $notices.Add("$($package.id) $($package.version)")
        $notices.Add("  License: $($package.license)")
        if ($package.copyright) { $notices.Add("  $($package.copyright)") }
        if ($package.projectUrl) { $notices.Add("  $($package.projectUrl)") }
        if ($package.provenance) { $notices.Add("  License and notice texts from $($package.provenance)") }
        foreach ($file in $package.noticeFiles) {
            $text = (Get-Content -LiteralPath $file.FullName -Raw).Replace("`r`n", "`n").Trim()
            if (-not $texts.Contains($text)) {
                $texts[$text] = [System.Collections.Generic.List[string]]::new()
            }
            $texts[$text].Add("$($file.Name) from $($package.id) $($package.version)")
        }
    }
    foreach ($text in $texts.GetEnumerator()) {
        $notices.Add('')
        $notices.Add(('=' * 118))
        $notices.Add($text.Value -join ', ')
        $notices.Add(('=' * 118))
        $notices.Add($text.Key)
    }
    Set-Content -LiteralPath (Join-Path -Path $moduleRoot -ChildPath 'ThirdPartyNotices.txt') -Value $notices -Encoding utf8

    $paths = [string[]] @(Get-ChildItem -LiteralPath $moduleRoot -Recurse -File | ForEach-Object { [System.IO.Path]::GetRelativePath($moduleRoot, $_.FullName).Replace('\', '/') })
    [System.Array]::Sort($paths, [System.StringComparer]::Ordinal)
    $files = [ordered]@{}
    foreach ($path in $paths) {
        $files[$path] = (Get-FileHash -LiteralPath (Join-Path -Path $moduleRoot -ChildPath $path) -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $identity = [ordered]@{
        module = [ordered]@{ name = 'FilesCom.PowerShell'; version = $version }
        schemaSha256 = $inventory.schema_sha256
        engine = [ordered]@{ package = $Engine.package; version = $Engine.version; sha256 = $Engine.sha256; source = $Engine.source }
        packages = @($packages | ForEach-Object { [ordered]@{ id = $_.id; version = $_.version; sha512 = $_.sha512; license = $_.license; usedBy = @($_.usedBy) } })
        files = $files
    }
    Set-Content -LiteralPath (Join-Path -Path $moduleRoot -ChildPath 'FilesCom.BuildInfo.json') -Value ($identity | ConvertTo-Json -Depth 5) -Encoding utf8
}

$engine = Assert-EnginePackage
Write-Information "Building FilesCom.PowerShell $version on FilesCom .NET SDK $($engine.version)" -InformationAction Continue

if (Test-Path -LiteralPath $OutputPath) {
    Remove-Item -LiteralPath $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $moduleRoot, $staging -Force | Out-Null

# Compile. Restores are locked to the committed packages.lock.json files. Every engine build compiles, including the
# Windows PowerShell one when this build does not ship it, so that edition cannot fall behind.
$commands = Join-Path -Path $root -ChildPath 'src/FilesCom.PowerShell/FilesCom.PowerShell.csproj'
$engineProject = Join-Path -Path $root -ChildPath 'src/FilesCom.PowerShell.Engine/FilesCom.PowerShell.Engine.csproj'
$coreLoader = Join-Path -Path $root -ChildPath 'src/FilesCom.PowerShell.CoreLoader/FilesCom.PowerShell.CoreLoader.csproj'
$dotnetCommon = @('--configuration', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false', '-nodeReuse:false')
foreach ($project in @($commands, $coreLoader)) {
    Invoke-Checked dotnet (@('restore', $project, '--locked-mode', '--disable-build-servers'))
}
foreach ($project in @($commands, $engineProject, $coreLoader)) {
    Invoke-Checked dotnet (@('build', $project, '--no-restore') + $dotnetCommon)
}

# Lay out the module: the .NET Standard commands and contracts at its root, which both editions import; on
# PowerShell 7 the load-context loader and each engine build with its own dependency closure under core; on Windows
# PowerShell the engine and its closure under desktop.
$commandsOutput = Join-Path -Path $root -ChildPath 'src/FilesCom.PowerShell/bin/Release/netstandard2.0'
$engineOutput = Join-Path -Path $root -ChildPath 'src/FilesCom.PowerShell.Engine/bin/Release'
Copy-AssemblyFile -From $commandsOutput -To $moduleRoot -Exclude @('FilesCom.PowerShell.Engine.dll')
Copy-AssemblyFile -From (Join-Path -Path $root -ChildPath 'src/FilesCom.PowerShell.CoreLoader/bin/Release/net8.0') -To (Join-Path -Path $moduleRoot -ChildPath 'core')
foreach ($framework in @('net8.0', 'net10.0')) {
    Copy-Engine -From (Join-Path -Path $engineOutput -ChildPath $framework) -To (Join-Path -Path $moduleRoot -ChildPath "core/engine-$framework")
}
if ($IncludeDesktopEdition) {
    $desktop = Join-Path -Path $moduleRoot -ChildPath 'desktop'
    Copy-AssemblyFile -From (Join-Path -Path $engineOutput -ChildPath 'net48') -To $desktop -Exclude @('FilesCom.PowerShell.Contracts.dll')
    Write-DesktopAllowlist -Directory $desktop
}
# PowerShell's own assemblies come from the host that imports the module; a copy in the module would be loaded instead
# of, or beside, the host's.
$hostAssemblies = @(Get-ChildItem -LiteralPath $moduleRoot -Recurse -File -Include 'System.Management.Automation.dll', 'Microsoft.PowerShell.*.dll')
if ($hostAssemblies.Count) {
    throw "The module must not ship PowerShell's own assemblies, but has $($hostAssemblies.FullName -join ', ')."
}
Copy-Item -LiteralPath (Join-Path -Path $root -ChildPath 'module/FilesCom.PowerShell.psd1') -Destination $moduleRoot
Copy-Item -LiteralPath (Join-Path -Path $root -ChildPath 'LICENSE') -Destination $moduleRoot
Copy-Item -LiteralPath (Join-Path -Path $root -ChildPath 'module/en-US') -Destination $moduleRoot -Recurse
# The customer examples ship with the module, runnable from its examples folder; the tests run them from there.
$examples = New-Item -ItemType Directory -Path (Join-Path -Path $moduleRoot -ChildPath 'examples')
Get-ChildItem -LiteralPath (Join-Path -Path $root -ChildPath 'examples') -File | Where-Object { $_.Extension -eq '.ps1' -or $_.Name -eq 'README.md' } | Copy-Item -Destination $examples.FullName
$manifestPath = Join-Path -Path $moduleRoot -ChildPath 'FilesCom.PowerShell.psd1'
if ($IncludeDesktopEdition) {
    $manifestText = Get-Content -LiteralPath $manifestPath -Raw
    Set-Content -LiteralPath $manifestPath -Value ($manifestText -replace "CompatiblePSEditions = @\('Core'\)", "CompatiblePSEditions = @('Core', 'Desktop')" -replace "PowerShellVersion = '7\.4'", "PowerShellVersion = '5.1'") -NoNewline -Encoding utf8
}

# Help: each command's Markdown supplies its text, and the compiled command its syntax and parameter details, so the
# MAML Get-Help reads can never describe parameters the command does not have. Any parameter or section left without
# text fails the build.
$platyPSManifest = Import-Tool -Name 'Microsoft.PowerShell.PlatyPS' -From $ToolsPath
$helpScript = {
    param($ManifestPath, $PlatyPS, $DocsPath, $MamlPath)
    $ErrorActionPreference = 'Stop'
    Import-Module -Name $PlatyPS
    Import-Module -Name $ManifestPath
    $help = foreach ($command in Get-Command -Module FilesCom.PowerShell) {
        $markdown = Join-Path -Path $DocsPath -ChildPath "$($command.Name).md"
        if (-not (Test-Path -LiteralPath $markdown)) {
            throw "$($command.Name) has no help in $DocsPath."
        }
        $commandHelp = Update-CommandHelp -Path $markdown
        # PlatyPS marks text it could not find with placeholders such as {{ Fill Name Description }}.
        $placeholder = '\{\{ ?(Fill|Insert|Add) '
        $unfilled = @($commandHelp.Parameters | Where-Object { -not $_.Description -or $_.Description -match $placeholder } | ForEach-Object Name)
        if ($unfilled.Count -or -not $commandHelp.Synopsis -or $commandHelp.Synopsis -match $placeholder -or -not $commandHelp.Examples) {
            throw "$($command.Name) help is incomplete: synopsis, examples or parameters $($unfilled -join ', ')."
        }
        $commandHelp
    }
    $help | Export-MamlCommandHelp -OutputFolder $MamlPath -Force | Out-Null
}
$mamlOutput = Join-Path -Path $staging -ChildPath 'maml'
& (Join-Path -Path $PSHOME -ChildPath 'pwsh') -NoProfile -NonInteractive -Command $helpScript -args $manifestPath, $platyPSManifest, (Join-Path -Path $root -ChildPath 'docs/commands'), $mamlOutput
if ($LASTEXITCODE -ne 0) {
    throw 'Generating the command help failed.'
}
$maml = @(Get-ChildItem -LiteralPath $mamlOutput -Filter '*.xml' -File -Recurse)
if ($maml.Count -ne 1) {
    throw "PlatyPS produced $($maml.Count) MAML files instead of one."
}
# Get-Help reads a binary module's help from the culture folder beside its assembly.
Copy-Item -LiteralPath $maml[0].FullName -Destination (Join-Path -Path $moduleRoot -ChildPath 'en-US/FilesCom.PowerShell.dll-Help.xml')

$engineTargets = @{ 'net8.0' = 'core/engine-net8.0'; 'net10.0' = 'core/engine-net10.0' }
if ($IncludeDesktopEdition) {
    $engineTargets['net48'] = 'desktop'
}
Write-ModuleIdentity -Engine $engine -Targets $engineTargets

# Check the manifest, then import the module in a fresh process and compare its commands with the manifest's.
# Importing must run no script, so it also works where the execution policy lets none run: the root module is the
# commands assembly, and the manifest names no script, type or format file to process.
Test-ModuleManifest -Path $manifestPath | Out-Null
$manifestData = Import-PowerShellDataFile -LiteralPath $manifestPath
$scriptSettings = @('ScriptsToProcess', 'TypesToProcess', 'FormatsToProcess', 'NestedModules', 'RequiredModules') | Where-Object { $manifestData.ContainsKey($_) }
$importScripts = @(Get-ChildItem -LiteralPath $moduleRoot -Recurse -File -Include '*.psm1', '*.ps1xml', '*.cdxml')
if ($manifestData.RootModule -ne 'FilesCom.PowerShell.dll' -or $scriptSettings -or $importScripts.Count) {
    throw "Importing the module must run no script, but its manifest has RootModule '$($manifestData.RootModule)' and $($scriptSettings -join ', '), and it ships $($importScripts.Name -join ', ')."
}
$exported = & (Join-Path -Path $PSHOME -ChildPath 'pwsh') -NoProfile -NonInteractive -Command "Import-Module '$manifestPath' -ErrorAction Stop; (Get-Command -Module FilesCom.PowerShell).Name"
if ($LASTEXITCODE -ne 0) {
    throw 'The built module could not be imported.'
}
$declared = (Import-PowerShellDataFile -LiteralPath $manifestPath).CmdletsToExport
$missing = @($declared | Where-Object { $_ -notin $exported })
if ($missing.Count -or @($exported).Count -ne @($declared).Count) {
    throw "The module exports $(@($exported).Count) commands but its manifest declares $(@($declared).Count); missing: $($missing -join ', ')."
}

# Package the module for PSResourceGet and the Gallery. Signing and its file catalog are the release step's, on the
# release signing host; this package is unsigned.
Import-Tool -Name 'Microsoft.PowerShell.PSResourceGet' -From $ToolsPath | Out-Null
$packages = Join-Path -Path $OutputPath -ChildPath 'packages'
New-Item -ItemType Directory -Path $packages -Force | Out-Null
Compress-PSResource -Path $moduleRoot -DestinationPath $packages
Remove-Item -LiteralPath $staging -Recurse -Force
Write-Information "Built $moduleRoot and $((Get-ChildItem -LiteralPath $packages -Filter '*.nupkg').FullName)" -InformationAction Continue
