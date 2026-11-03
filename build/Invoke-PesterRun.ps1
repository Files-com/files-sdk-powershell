<#
.SYNOPSIS
Runs Pester once, in the PowerShell process Test-Module.ps1 starts for one PowerShell version.

.DESCRIPTION
Exits 0 when every discovered test passed, 1 when any failed, was skipped or did not run, and 3 when no test was found.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Pester,

    # Test files or folders, separated by the platform's path separator.
    [Parameter(Mandatory)]
    [string] $TestPath,

    [Parameter(Mandatory)]
    [string] $ResultFile
)

$ErrorActionPreference = 'Stop'
Import-Module -Name $Pester
$configuration = New-PesterConfiguration
$configuration.Run.Path = $TestPath -split [System.IO.Path]::PathSeparator
$configuration.Run.PassThru = $true
$configuration.Output.Verbosity = 'Detailed'
$configuration.TestResult.Enabled = $true
$configuration.TestResult.OutputFormat = 'NUnitXml'
$configuration.TestResult.OutputPath = $ResultFile
$result = Invoke-Pester -Configuration $configuration
if (-not $result -or $result.TotalCount -eq 0) {
    'No test was discovered.'
    exit 3
}
"Discovered $($result.TotalCount); passed $($result.PassedCount); failed $($result.FailedCount); skipped $($result.SkippedCount); not run $($result.NotRunCount)"
if ($result.FailedCount -or $result.SkippedCount -or $result.NotRunCount -or $result.Result -ne 'Passed') {
    exit 1
}
exit 0
