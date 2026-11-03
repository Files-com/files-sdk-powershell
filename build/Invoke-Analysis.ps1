<#
.SYNOPSIS
Checks the module's PowerShell scripts and C# sources without changing them.

.DESCRIPTION
Runs PSScriptAnalyzer over the manifest, build scripts, examples, tests and catalogue adapter with their settings,
compares each script with what Invoke-Formatter would make of it, and runs dotnet format in verify mode over the C#
projects. The examples are also checked against the Windows PowerShell 5.1 compatibility profile. Fails when a tool is
missing, when no file is analyzed, or on any finding or formatting difference.

With -TestScript, only the named scripts in tests are analyzed in place of every script in tests and verification;
everything else is checked as without it. The ordinary CI job (ci.sh) names its test suites and their shared files so.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ToolsPath,

    # File names in tests, such as Module.Tests.ps1. A name that is not a file there fails the check.
    [string[]] $TestScript
)

# Not under strict mode: PSScriptAnalyzer 1.25.0 fails with a null reference when its caller sets it.
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$pins = Get-Content -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath 'tools.json') -Raw | ConvertFrom-Json
$analyzer = Join-Path -Path $ToolsPath -ChildPath "PSScriptAnalyzer/$(($pins.modules | Where-Object name -EQ 'PSScriptAnalyzer').version)/PSScriptAnalyzer.psd1"
if (-not (Test-Path -LiteralPath $analyzer)) {
    throw "PSScriptAnalyzer is not in $ToolsPath. Run build/Restore-Tools.ps1 first."
}
Import-Module -Name $analyzer -ErrorAction Stop

$testPaths = if ($TestScript) { @($TestScript | ForEach-Object { "tests/$_" }) } else { @('tests', 'verification') }
$groups = @(
    @{ Settings = Join-Path -Path $root -ChildPath 'PSScriptAnalyzerSettings.psd1'; Paths = @('module', 'build') }
    @{ Settings = Join-Path -Path $root -ChildPath 'build/ExamplesAnalyzerSettings.psd1'; Paths = @('examples') }
    @{ Settings = Join-Path -Path $root -ChildPath 'tests/PSScriptAnalyzerSettings.psd1'; Paths = $testPaths }
)
$problems = [System.Collections.Generic.List[string]]::new()
$scripts = foreach ($group in $groups) {
    foreach ($path in $group.Paths) {
        $item = Join-Path -Path $root -ChildPath $path
        if ($TestScript -and $path -like 'tests/*' -and -not (Test-Path -LiteralPath $item -PathType Leaf)) {
            throw "$item is not a test script to analyze."
        }
        Get-ChildItem -LiteralPath $item -Include '*.ps1', '*.psm1', '*.psd1' -File -Recurse |
            ForEach-Object { [pscustomobject]@{ File = $_; Settings = $group.Settings } }
    }
}
$analyzed = @($scripts).Count

# PSScriptAnalyzer 1.25.0 runs its rules in parallel, and rules that look commands up share one runspace: now and then
# its analysis fails inside with a null reference, or with a pipeline already running in that runspace, differently from
# run to run and sometimes several times in a row. A file whose analysis fails that way is analyzed again after a short
# pause, up to five times in all; any other failure, or the same one five times, fails the check.
function Invoke-FileAnalysis {
    param($Script)
    for ($attempt = 1; ; $attempt++) {
        try {
            return @(Invoke-ScriptAnalyzer -Path $Script.File.FullName -Settings $Script.Settings)
        }
        catch {
            $concurrency = $_.Exception -is [System.NullReferenceException] -or
            ($_.Exception -is [System.Management.Automation.PSInvalidOperationException] -and $_.Exception.Message -like '*pipeline is already running*')
            if (-not $concurrency -or $attempt -eq 5) {
                throw
            }
            Start-Sleep -Milliseconds 250
        }
    }
}

# Every analysis runs before any formatting check, which keeps Invoke-Formatter's state out of the analyses.
foreach ($script in $scripts) {
    foreach ($finding in Invoke-FileAnalysis -Script $script) {
        $problems.Add("$($script.File.FullName):$($finding.Line): $($finding.RuleName): $($finding.Message)")
    }
}
foreach ($script in $scripts) {
    $text = Get-Content -LiteralPath $script.File.FullName -Raw
    if ($text -and (Invoke-Formatter -ScriptDefinition $text -Settings $script.Settings) -ne $text) {
        $problems.Add("$($script.File.FullName): formatting differs from Invoke-Formatter's")
    }
}
if ($analyzed -eq 0) {
    throw 'No PowerShell script was found to analyze.'
}

foreach ($project in @('src/FilesCom.PowerShell/FilesCom.PowerShell.csproj', 'src/FilesCom.PowerShell.CoreLoader/FilesCom.PowerShell.CoreLoader.csproj', 'tests/FilesCom.PowerShell.TestService/FilesCom.PowerShell.TestService.csproj')) {
    & dotnet format (Join-Path -Path $root -ChildPath $project) --verify-no-changes --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        $problems.Add("${project}: dotnet format found changes to make")
    }
}

if ($problems.Count) {
    $problems | ForEach-Object { Write-Error -Message $_ -ErrorAction Continue }
    throw "Analysis found $($problems.Count) problems in $analyzed scripts and the C# projects."
}
"Analyzed $analyzed scripts and the C# projects: no findings."
