<#
.SYNOPSIS
Runs the shared API scenarios and the API scenario consumer's controls through the installed FilesCom module, each
against a fresh Files.com API simulator, on the PowerShell that runs this script.

.DESCRIPTION
The ordinary CI job runs this on each pinned PowerShell after its tests, as the unprivileged test user, with the module
installed from the package it built (build/Invoke-OrdinaryCi.ps1). It:

- requires the simulator (-MockServer, generated in the same job) to have been generated from the raw schema capture
  -SchemaSha256 names (its generation.json swagger.sha256), and takes its normalized schema (schema_sha256) as the
  SHA-256 each simulator must report;
- requires the definition (-DefinitionPath), the canonical preset and cases.json (-ScenarioRoot, generated in the same
  job) to name the same definition SHA-256, and cases.json the same raw capture;
- compiles the consumer's transport support (Transport.cs) for this PowerShell before any scenario starts;
- runs each scenario cases.json lists, then the controls, with the simulator's with-scenario.rb and the
  api-scenarios-canonical preset, one fresh simulator each, in turn, with the selected bounds;
- writes summary.json to -OutPath, with each run's launcher outcome, journal and consumer receipt, and fails unless
  every run passed: the launcher exited 0, the consumer passed every simulator position and omitted each site-only one,
  and its journal joined.

Nothing is fetched. The simulator binds to loopback only, and its consumer gets only the launcher's FILES_MOCK_* values
besides this process's environment.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ModuleManifest,

    [Parameter(Mandatory)]
    [string] $PackagePath,

    [Parameter(Mandatory)]
    [string] $MockServer,

    [Parameter(Mandatory)]
    [string] $ScenarioRoot,

    [Parameter(Mandatory)]
    [string] $DefinitionPath,

    [Parameter(Mandatory)]
    [string] $SchemaSha256,

    [Parameter(Mandatory)]
    [string] $OutPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The selected bounds, not measured times: the launcher's startup, consumer and settlement timeouts, the consumer's own
# work, cleanup and whole-run budgets within its timeout (run.ps1), and the simulator's request body limit, at which the
# definition's 6 MiB + 3 byte file goes as parts of 5 MiB and 1 MiB + 3 bytes.
$bounds = [ordered]@{
    startupSeconds = 30
    consumerSeconds = 210
    settleSeconds = 5
    consumerWorkSeconds = 175
    consumerCleanupSeconds = 30
    consumerWholeSeconds = 205
    maxBodyBytes = 16777216
}
$preset = 'api-scenarios-canonical'

function Get-Sha256 {
    param([string] $Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Read-JsonFile {
    param([string] $Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Path does not exist."
    }
    Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable
}

function Get-JsonValue {
    param($Object, [string[]] $Names)
    foreach ($name in $Names) {
        if ($Object -isnot [System.Collections.IDictionary] -or -not $Object.Contains($name)) {
            return $null
        }
        $Object = $Object[$name]
    }
    , $Object
}

# A run ID with-scenario.rb accepts: letters, digits, '.', '_' and '-', at most 64 characters.
function Get-RunId {
    param([string] $Name)
    $id = "powershell-$($PSVersionTable.PSVersion)-$Name" -replace '[^A-Za-z0-9._-]', '-'
    if ($id.Length -gt 64) { $id.Substring(0, 64) } else { $id }
}

if ((Test-Path -LiteralPath $OutPath) -and @(Get-ChildItem -LiteralPath $OutPath -Force).Count) {
    throw "$OutPath already holds files; give an empty or new directory."
}
[void] [System.IO.Directory]::CreateDirectory($OutPath)
$OutPath = (Resolve-Path -LiteralPath $OutPath).Path

$summary = [ordered]@{
    format = 'filescom-powershell-api-scenarios/1'
    started = [System.DateTime]::UtcNow.ToString('o')
    result = 'running'
    powershell = [ordered]@{
        version = $PSVersionTable.PSVersion.ToString()
        path = [System.Environment]::ProcessPath
        os = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        framework = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
    }
    bounds = $bounds
    identity = [ordered]@{}
    runs = [System.Collections.Generic.List[object]]::new()
}

try {
    # The schema, the simulator and the definition, joined before anything runs.
    $generationPath = Join-Path -Path $MockServer -ChildPath 'lib/simulation/generation.json'
    $generation = Read-JsonFile -Path $generationPath
    if ([string] (Get-JsonValue $generation 'swagger', 'sha256') -cne $SchemaSha256) {
        throw "The simulator was generated from schema $(Get-JsonValue $generation 'swagger', 'sha256'), not the capture $SchemaSha256."
    }
    $normalized = [string] (Get-JsonValue $generation 'schema_sha256')
    if ($normalized -cnotmatch '^[0-9a-f]{64}$') {
        throw "The simulator's generation record gives no normalized schema SHA-256, but '$normalized'."
    }
    $definitionSha256 = Get-Sha256 -Path $DefinitionPath
    $presetPath = Join-Path -Path $MockServer -ChildPath "scenarios/$preset.json"
    $presetDefinition = [string] (Get-JsonValue (Read-JsonFile -Path $presetPath) 'reset', 'profile', 'api_scenarios', 'definition_sha256')
    if ($presetDefinition -cne $definitionSha256) {
        throw "The $preset preset names the definition $presetDefinition, not $DefinitionPath's $definitionSha256."
    }
    $casesPath = Join-Path -Path $ScenarioRoot -ChildPath 'cases.json'
    $cases = Read-JsonFile -Path $casesPath
    if ([string] (Get-JsonValue $cases 'definition', 'sha256') -cne $definitionSha256 -or [string] $cases.schema_sha256 -cne $SchemaSha256) {
        throw "cases.json was rendered from definition $(Get-JsonValue $cases 'definition', 'sha256') and schema $($cases.schema_sha256), not $definitionSha256 and $SchemaSha256."
    }
    $scenarios = @($cases.scenarios)
    if (-not $scenarios.Count) {
        throw 'cases.json lists no scenario.'
    }
    $launcher = Join-Path -Path $MockServer -ChildPath 'with-scenario.rb'
    if (-not (Test-Path -LiteralPath $launcher -PathType Leaf)) {
        throw "$launcher does not exist."
    }
    $ruby = @(Get-Command -Name ruby -CommandType Application -ErrorAction Stop)[0].Source
    $summary.identity = [ordered]@{
        definition = [ordered]@{ path = $DefinitionPath; sha256 = $definitionSha256 }
        rawCaptureSha256 = $SchemaSha256
        normalizedSchemaSha256 = $normalized
        generation = [ordered]@{ path = $generationPath; sha256 = Get-Sha256 -Path $generationPath }
        preset = [ordered]@{ path = $presetPath; sha256 = Get-Sha256 -Path $presetPath }
        cases = [ordered]@{ path = $casesPath; sha256 = Get-Sha256 -Path $casesPath }
        module = [ordered]@{ manifest = $ModuleManifest; sha256 = Get-Sha256 -Path $ModuleManifest }
        package = [ordered]@{ path = $PackagePath; sha256 = Get-Sha256 -Path $PackagePath }
        ruby = $ruby
    }

    # The consumer's support, compiled for this PowerShell's .NET before any scenario's deadline starts.
    $support = Join-Path -Path $OutPath -ChildPath 'support'
    [void] [System.IO.Directory]::CreateDirectory($support)
    $transportSource = Join-Path -Path $ScenarioRoot -ChildPath 'Transport.cs'
    $transport = Join-Path -Path $support -ChildPath 'FilesCom.ApiScenarios.Transport.dll'
    Add-Type -Path $transportSource -OutputAssembly $transport -OutputType Library
    $summary.identity.transport = [ordered]@{ source = $transportSource; sourceSha256 = Get-Sha256 -Path $transportSource; assembly = $transport; assemblySha256 = Get-Sha256 -Path $transport }

    # Only the launcher's FILES_MOCK_* values reach the consumer, with the body limit selected for every run.
    foreach ($name in @([System.Environment]::GetEnvironmentVariables().Keys | Where-Object { $_ -like 'FILES_MOCK_*' })) {
        [System.Environment]::SetEnvironmentVariable($name, $null)
    }
    [System.Environment]::SetEnvironmentVariable('FILES_MOCK_MAX_BODY_BYTES', [string] $bounds.maxBodyBytes)

    $pwsh = [System.Environment]::ProcessPath
    $common = @(
        '-CasesPath', $casesPath, '-TransportAssembly', $transport, '-ModuleManifest', $ModuleManifest, '-PackagePath', $PackagePath,
        '-DefinitionPath', $DefinitionPath, '-SchemaSha256', $SchemaSha256, '-GenerationPath', $generationPath)
    $plan = @(foreach ($scenario in $scenarios) {
            [ordered]@{ name = [string] $scenario.id; script = 'run.ps1'; arguments = @('-Case', [string] $scenario.id) + $common; receipt = 'consumer.json'; positions = $scenario.positions }
        }) + @([ordered]@{ name = 'controls'; script = 'controls.ps1'; arguments = $common; receipt = 'controls.json'; positions = $null })

    foreach ($item in $plan) {
        $out = Join-Path -Path $OutPath -ChildPath $item.name
        $arguments = @(
            $launcher, '--scenario', $preset, '--out', $out, '--run-id', (Get-RunId -Name $item.name), '--schema-sha256', $normalized,
            '--startup-timeout', [string] $bounds.startupSeconds, '--consumer-timeout', [string] $bounds.consumerSeconds, '--settle-timeout', [string] $bounds.settleSeconds,
            '--', $pwsh, '-NoProfile', '-NonInteractive', '-File', (Join-Path -Path $ScenarioRoot -ChildPath $item.script)) + $item.arguments
        Write-Information -MessageData "API scenarios: $($item.name)" -InformationAction Continue
        & $ruby @arguments 2>&1 | Tee-Object -FilePath (Join-Path -Path $OutPath -ChildPath "$($item.name).launcher.log") | Out-Host
        $exit = $LASTEXITCODE
        $record = [ordered]@{ name = $item.name; out = $item.name; launcherExit = $exit; consumer = $null; journal = $null; missingEvidence = $null; receipt = $null; result = 'failed'; problems = [System.Collections.Generic.List[string]]::new() }
        $runPath = Join-Path -Path $out -ChildPath 'run.json'
        if (Test-Path -LiteralPath $runPath -PathType Leaf) {
            $launched = Read-JsonFile -Path $runPath
            $record.consumer = $launched.consumer
            $record.journal = $launched.journal
            $record.missingEvidence = $launched.missing_evidence
            if ($launched.setup_error) {
                $record.problems.Add("setup: $($launched.setup_error)")
            }
        }
        else {
            $record.problems.Add('the launcher wrote no run.json')
        }
        if ($exit -ne 0) {
            $record.problems.Add("the launcher exited $exit")
        }
        $receiptPath = Join-Path -Path $out -ChildPath $item.receipt
        if (Test-Path -LiteralPath $receiptPath -PathType Leaf) {
            $receipt = Read-JsonFile -Path $receiptPath
            # The journal's problems one by one: Get-JsonValue returns the list whole, and @() would only wrap it.
            $journalProblems = Get-JsonValue $receipt 'journal', 'problems'
            $record.receipt = [ordered]@{
                file = $item.receipt; result = $receipt.result; firstFailure = $receipt.firstFailure; positions = $receipt.positions; unresolved = $receipt.unresolved
                unsettled = $receipt.unsettled; journalProblems = @(if ($null -ne $journalProblems) { $journalProblems })
            }
            if ($receipt.result -cne 'passed') {
                $record.problems.Add("the consumer's receipt says $($receipt.result): $($receipt.firstFailure)")
            }
            if ($item.positions) {
                $counts = $receipt.positions
                if ([int] $counts.passed -ne [int] $item.positions.simulator -or [int] $counts.omitted -ne [int] $item.positions.site_only) {
                    $record.problems.Add("$($counts.passed) of $($item.positions.simulator) simulator positions passed and $($counts.omitted) of $($item.positions.site_only) site-only positions were omitted")
                }
            }
        }
        else {
            $record.problems.Add("the consumer wrote no $($item.receipt)")
        }
        if (-not $record.problems.Count) {
            $record.result = 'passed'
        }
        $summary.runs.Add($record)
    }
    $failed = @($summary.runs | Where-Object { $_.result -cne 'passed' })
    $summary.result = if ($failed.Count) { 'failed' } else { 'passed' }
    if ($failed.Count) {
        throw "$($failed.Count) of $($summary.runs.Count) API scenario runs failed: $(@($failed | ForEach-Object { "$($_.name): $($_.problems -join '; ')" }) -join ' | ')"
    }
}
catch {
    $summary.result = 'failed'
    if (-not $summary.Contains('error')) {
        $summary.error = $_.Exception.Message
    }
    throw
}
finally {
    $summary.finished = [System.DateTime]::UtcNow.ToString('o')
    ConvertTo-Json -InputObject $summary -Depth 16 | Set-Content -LiteralPath (Join-Path -Path $OutPath -ChildPath 'summary.json') -Encoding utf8
}
