<#
.SYNOPSIS
Runs the module's Pester tests against the module installed from its built package, in one PowerShell version.

.DESCRIPTION
Installs the module from the package build.sh made, as PowerShellGet and PSResourceGet do, into a private folder under
-ModulePath, builds the test service for the edition of the PowerShell given by -PowerShell (its .NET build for
PowerShell 7, its .NET Framework build for Windows PowerShell 5.1), then runs every *.Tests.ps1 in tests/ with the
pinned Pester, in a fresh process of that PowerShell, against that installed module. Fails when the package or Pester is
missing, when the tests given include the generated contract tests and no mock server is named, when no test is found,
when any test fails or is skipped, or when the run exceeds -TimeoutMinutes. Writes NUnit results to -ResultsPath.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ToolsPath,

    [Parameter(Mandatory)]
    [string] $PowerShell,

    [string] $ModulePath = (Join-Path -Path $PSScriptRoot -ChildPath '../out'),

    [string] $ResultsPath = (Join-Path -Path $PSScriptRoot -ChildPath '../out/test-results'),

    [string[]] $TestPath = @(Join-Path -Path $PSScriptRoot -ChildPath '../tests'),

    # The Files.com mock server the generated contract tests run against, required when they are among the tests.
    [string] $MockUrl = $env:FILESCOM_MOCK_URL,

    [int] $TimeoutMinutes = 20
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$version = (Get-Content -LiteralPath (Join-Path -Path $root -ChildPath '_VERSION') -Raw).Trim()
$package = Join-Path -Path ([System.IO.Path]::GetFullPath($ModulePath)) -ChildPath "packages/FilesCom.PowerShell.$version.nupkg"
if (-not (Test-Path -LiteralPath $package)) {
    throw "$package does not exist. Build the module first."
}
# Installing a package extracts its payload and leaves out NuGet's own parts.
$installed = Join-Path -Path ([System.IO.Path]::GetFullPath($ModulePath)) -ChildPath "installed/FilesCom.PowerShell/$version"
if (Test-Path -LiteralPath $installed) {
    Remove-Item -LiteralPath $installed -Recurse -Force
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($package)
try {
    foreach ($entry in $archive.Entries) {
        $name = [uri]::UnescapeDataString($entry.FullName)
        if ($name -eq '[Content_Types].xml' -or $name -like '_rels/*' -or $name -like 'package/*' -or $name -like '*.nuspec' -or $name.EndsWith('/')) {
            continue
        }
        $target = [System.IO.Path]::GetFullPath((Join-Path -Path $installed -ChildPath $name))
        if (-not $target.StartsWith($installed + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::Ordinal)) {
            throw "$package has an entry outside the module folder: $name."
        }
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target)
    }
}
finally {
    $archive.Dispose()
}
$manifest = Join-Path -Path $installed -ChildPath 'FilesCom.PowerShell.psd1'
if (-not (Test-Path -LiteralPath $manifest)) {
    throw "$package holds no FilesCom.PowerShell.psd1."
}
$paths = @($TestPath | ForEach-Object { (Resolve-Path -LiteralPath $_).Path })
$contractTests = @(foreach ($path in $paths) {
        if (Test-Path -LiteralPath $path -PathType Container) {
            Get-ChildItem -LiteralPath $path -Filter 'Contract.Tests.ps1' -File -Recurse
        }
        elseif ((Split-Path -Path $path -Leaf) -eq 'Contract.Tests.ps1') {
            $path
        }
    })
if ($contractTests.Count -and -not $MockUrl) {
    throw 'The contract tests need a running Files.com mock server: pass -MockUrl or set FILESCOM_MOCK_URL.'
}
$pins = Get-Content -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath 'tools.json') -Raw | ConvertFrom-Json
$pester = Join-Path -Path $ToolsPath -ChildPath "Pester/$(($pins.modules | Where-Object name -EQ 'Pester').version)/Pester.psd1"
if (-not (Test-Path -LiteralPath $pester)) {
    throw "Pester is not in $ToolsPath. Run build/Restore-Tools.ps1 first."
}

# The test service loads into the tests' process, so it is built for that PowerShell's runtime.
$hostVersion, $hostEdition = & $PowerShell -NoProfile -NonInteractive -Command '$PSVersionTable.PSVersion.ToString(); $PSVersionTable.PSEdition'
$serviceFramework = if ($hostEdition -eq 'Desktop') { 'net48' } else { 'net8.0' }
$service = Join-Path -Path ([System.IO.Path]::GetFullPath($ModulePath)) -ChildPath "testservice/$serviceFramework"
dotnet build (Join-Path -Path $root -ChildPath 'tests/FilesCom.PowerShell.TestService/FilesCom.PowerShell.TestService.csproj') --configuration Release --framework $serviceFramework --output $service --disable-build-servers -p:UseSharedCompilation=false -nodeReuse:false | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw 'Building the test service failed.'
}

New-Item -ItemType Directory -Path $ResultsPath -Force | Out-Null
$resultFile = Join-Path -Path ([System.IO.Path]::GetFullPath($ResultsPath)) -ChildPath "pester-$hostVersion.xml"
$environment = @{ FILESCOM_MODULE_MANIFEST = $manifest; FILESCOM_TEST_SERVICE = (Join-Path -Path $service -ChildPath 'FilesCom.PowerShell.TestService.dll') }
if ($MockUrl) {
    $environment.FILESCOM_MOCK_URL = $MockUrl
}
foreach ($name in $environment.Keys) {
    Set-Item -Path "env:$name" -Value $environment[$name]
}
$paths = $paths -join [System.IO.Path]::PathSeparator
$runner = Join-Path -Path $PSScriptRoot -ChildPath 'Invoke-PesterRun.ps1'
# ArgumentList quotes each argument, so paths with spaces arrive intact.
$start = [System.Diagnostics.ProcessStartInfo]::new($PowerShell)
foreach ($argument in @('-NoProfile', '-NonInteractive', '-File', $runner, '-Pester', $pester, '-TestPath', $paths, '-ResultFile', $resultFile)) {
    $start.ArgumentList.Add($argument)
}
$start.UseShellExecute = $false
$process = [System.Diagnostics.Process]::Start($start)
if (-not $process.WaitForExit($TimeoutMinutes * 60 * 1000)) {
    $process.Kill($true)
    throw "The tests on PowerShell $hostVersion did not finish within $TimeoutMinutes minutes."
}
if ($process.ExitCode -ne 0) {
    throw "The tests on PowerShell $hostVersion failed (exit code $($process.ExitCode)). Results: $resultFile"
}
Write-Information "The tests passed on PowerShell $hostVersion. Results: $resultFile" -InformationAction Continue
