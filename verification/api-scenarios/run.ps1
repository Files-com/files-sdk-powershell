<#
.SYNOPSIS
Runs one shared Files.com API scenario through the installed FilesCom module's public commands, as the consumer that
generated/mock-server/with-scenario.rb starts against the simulator it started for the run.

.DESCRIPTION
The scenario comes from cases.json, which the generator renders from verification/scenarios/api.json bound to the schema
and to this module's command catalogue: each operation step names the command and parameters its schema operation has
in this module. Each step runs as the installed module's public command, in a pipeline of its own in a second runspace,
on a connection made with Connect-FilesAccount -HttpMessageHandler: the handler (Transport.cs) forwards every request to
a real network handler and notes what was sent and how it was answered, so the HTTP status a command does not show is
witnessed where it happened. Steps the definition marks for a Files.com site are omitted, each with the definition's own
reason; every other step runs and must pass, or the run fails.

Before any request, it binds the run to the definition's SHA-256, the raw schema capture's (generation.json
swagger.sha256), the simulator's normalized schema (schema_sha256, as readiness reported it), the canonical preset and
its profile, this server's instance and reset epoch, and the installed module, package and PowerShell, and checks that
the installed module has every command and parameter each step needs.

It owns the run folder and the scenario's folder only from a 201 answer to their own creation that returned exactly the
requested path as a directory. When a step fails, nothing after it runs: it deletes the scenario's folder recursively
only if it owned it, and the run folder without recursion only if it owned it, confirming each absent, and records what
it could not resolve apart from the step's failure. A command still running at its deadline is stopped; one that does
not settle even then is left to the launcher, and nothing it may still be using, its runspace, its local inputs or what
the run owns, is used or removed after it. It ends with a strict one-to-one join of every request the transport saw to
the simulator's complete journal, in which each step's entries must be its own operations on its own fixtures, and an
upload's begin, parts and finish all the same upload.

It writes consumer.json to FILES_MOCK_OUT, even when the run cannot start, and exits 0 only when every simulator step,
the cleanup and the join passed. Dot-sourcing the script defines its functions without running a scenario, as
controls.ps1 does.
#>
[CmdletBinding()]
param(
    [string] $Case,
    [string] $CasesPath,
    [string] $TransportAssembly,
    [string] $ModuleManifest,
    [string] $PackagePath,
    [string] $DefinitionPath,
    [string] $SchemaSha256,
    [string] $GenerationPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The selected bounds, in seconds, not measured times: the scenario's work, the cleanup after it, and the whole run with
# its receipt, all within the launcher's consumer timeout of 210 s. Each is an absolute deadline on the run's own clock,
# and every wait takes only what is left before its deadline: the work ends WorkSeconds into the run, the cleanup
# CleanupSeconds after it starts, and nothing, the journal included, after WholeSeconds less the ReceiptSeconds kept for
# writing the receipt. A held request waits at most HoldSeconds for the command's StopProcessing, and a stopped pipeline
# at most StopSeconds to settle, never past the whole run's deadline.
$script:WorkSeconds = 175
$script:CleanupSeconds = 30
$script:WholeSeconds = 205
$script:ReceiptSeconds = 2
$script:HoldSeconds = 30
$script:StopSeconds = 30
$script:ObservationLimit = 4096
$script:Preset = 'api-scenarios-canonical'
$script:ControlPrefix = '/__files_mock/v1'
$script:Invariant = [System.Globalization.CultureInfo]::InvariantCulture

# What the adapter binds for each intent the definition names: the module's own transfer and action commands. A detail,
# parameter or expectation not listed here is refused before any request is sent. Routes gives each operation the
# definition names for the intent its role in the intent's journal, which must name exactly these.
$script:IntentBindings = @{
    'upload' = @{
        Details = @('source', 'max_read', 'cancel', 'parameters'); Expect = @('completed', 'cancelled', 'progress_final', 'min_parts'); Parameters = @{ provided_mtime = 'ModifiedTime' }
        Routes = @{ begin = 'POST /file_actions/begin_upload/{path}'; finish = 'POST /files/{path}' }
    }
    'download' = @{ Details = @('source'); Expect = @('bytes'); Parameters = @{}; Routes = @{ lookup = 'GET /files/{path}' } }
    'copy' = @{ Details = @('from'); Expect = @('completed'); Parameters = @{}; Routes = @{ action = 'POST /file_actions/copy/{path}'; migration = 'GET /file_migrations/{id}' } }
    'move' = @{ Details = @('from'); Expect = @('completed'); Parameters = @{}; Routes = @{ action = 'POST /file_actions/move/{path}'; migration = 'GET /file_migrations/{id}' } }
    'upload-directory' = @{
        Details = @('tree'); Expect = @('completed', 'files', 'empty_folders', 'bytes'); Parameters = @{}
        Routes = @{ folder = 'POST /folders/{path}'; begin = 'POST /file_actions/begin_upload/{path}'; finish = 'POST /files/{path}' }
    }
    'download-directory' = @{ Details = @(); Expect = @('files', 'empty_folders', 'bytes', 'bytes_match'); Parameters = @{}; Routes = @{ list = 'GET /folders/{path}'; lookup = 'GET /files/{path}' } }
}
$script:TransferCommands = [ordered]@{
    'Send-FilesFile' = @('LiteralPath', 'InputStream', 'Length', 'RemotePath', 'ModifiedTime', 'PassThru', 'Connection', 'Confirm')
    'Receive-FilesFile' = @('RemotePath', 'InputObject', 'LiteralPath', 'PassThru', 'Connection', 'Confirm')
    'Send-FilesDirectory' = @('LiteralPath', 'RemotePath', 'PassThru', 'Connection', 'Confirm')
    'Receive-FilesDirectory' = @('RemotePath', 'LiteralPath', 'PassThru', 'Connection', 'Confirm')
    'Wait-FilesFileAction' = @('InputObject', 'Timeout', 'Connection')
}
$script:OperationExpect = @('status', 'error', 'attempts', 'interval_seconds', 'entry', 'listing')
$script:UploadSources = @('local-file', 'seekable-stream', 'forward-only-stream')
$script:DownloadSources = @('listed-entry')
$script:Cancellations = @('after-first-acknowledged-part')
$script:Matchers = @('one_of', 'includes', 'not')

function Get-TextSha256 {
    param([string] $Text)
    [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($Text))).ToLowerInvariant()
}

function Get-FileSha256 {
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

# The value at Names inside a parsed JSON object, or $null when a level is missing.
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

function Assert-Same {
    param($Actual, $Expected, [string] $What)
    if ([string] $Actual -cne [string] $Expected) {
        throw "$What is '$Actual', not '$Expected'."
    }
}

# A public property of an object a command wrote, which must exist.
function Get-ObjectMember {
    param($Object, [string] $Name)
    if ($null -eq $Object) {
        throw "the command wrote no object to read $Name from"
    }
    $property = $Object.PSObject.Properties[$Name]
    if (-not $property) {
        throw "the $($Object.GetType().Name) the command wrote has no $Name property"
    }
    , $property.Value
}

function Get-OrdinalSorted {
    param([string[]] $Values)
    $sorted = [string[]] @($Values | Where-Object { $null -ne $_ })
    [System.Array]::Sort($sorted, [System.StringComparer]::Ordinal)
    , $sorted
}

# Each item of a list that a lookup returned whole, as Get-JsonValue and Get-ObjectMember return one, so that @() around
# this gives the list's own items: none, one or several. A missing value, or one that is not a list, is refused, never
# taken as an item.
function Get-ListItem {
    param($List, [string] $What)
    if ($null -eq $List) {
        throw "$What is missing"
    }
    if ($List -is [string] -or $List -is [System.Collections.IDictionary] -or $List -isnot [System.Collections.IEnumerable]) {
        throw "$What is a $($List.GetType().Name), not a list"
    }
    foreach ($item in $List) {
        , $item
    }
}

# Fails unless Actual and Expected hold the same strings, in any order.
function Assert-SameList {
    param([string[]] $Actual, [string[]] $Expected, [string] $What)
    $actualSorted = Get-OrdinalSorted -Values $Actual
    $expectedSorted = Get-OrdinalSorted -Values $Expected
    $differ = $actualSorted.Count -ne $expectedSorted.Count
    for ($index = 0; -not $differ -and $index -lt $actualSorted.Count; $index++) {
        $differ = $actualSorted[$index] -cne $expectedSorted[$index]
    }
    if ($differ) {
        throw "$What are $(ConvertTo-Json -InputObject @($actualSorted) -Compress), not $(ConvertTo-Json -InputObject @($expectedSorted) -Compress)."
    }
}

function Get-StepLabel {
    param($Step)
    "position $($Step.number)"
}

function Get-StepTitle {
    param($Step)
    if ($Step.kind -ceq 'operation') { $Step.operation.route } else { "$($Step.intent) $($Step.fixture)" }
}

# The run's inputs from the launcher, which gives every consumer these. One not set is listed in Missing, which
# Initialize-ScenarioRun refuses, rather than thrown here, so that a run that cannot start still has a context, and a
# receipt wherever FILES_MOCK_OUT names.
function Get-RunInput {
    $names = [ordered]@{
        ServerUrl = 'FILES_MOCK_SERVER_URL'
        ApiKey = 'FILES_MOCK_API_KEY'
        Instance = 'FILES_MOCK_INSTANCE'
        SchemaSha256 = 'FILES_MOCK_SCHEMA_SHA256'
        RunId = 'FILES_MOCK_RUN_ID'
        Scenario = 'FILES_MOCK_SCENARIO'
        ScenarioFile = 'FILES_MOCK_SCENARIO_FILE'
        Out = 'FILES_MOCK_OUT'
    }
    $values = [ordered]@{}
    $missing = [System.Collections.Generic.List[string]]::new()
    foreach ($entry in $names.GetEnumerator()) {
        $value = [System.Environment]::GetEnvironmentVariable($entry.Value)
        if (-not $value) {
            $missing.Add($entry.Value)
            $value = $null
        }
        $values[$entry.Key] = $value
    }
    $values.MaxBodyBytes = [System.Environment]::GetEnvironmentVariable('FILES_MOCK_MAX_BODY_BYTES')
    $values.Missing = $missing.ToArray()
    $values
}

# The scenario's own protocol for the folder a run owns: its fixture with the empty path, and the definition's steps
# that create it (expecting 201 with the folder as the entry), delete it (expecting 204) and confirm it absent (with
# attempts). The run folder and cleanup use the same commands and parameters. An operation's expectation always has its
# status, and an entry, an error or retries only where the definition gives them, so those are read where present.
function Get-NamespaceProtocol {
    param($Scenario)
    $names = @($Scenario.fixtures.Keys | Where-Object { $Scenario.fixtures[$_].path -ceq '' })
    if ($names.Count -ne 1) {
        throw "scenario $($Scenario.id) has $($names.Count) fixtures with the empty path, not one owned folder."
    }
    $namespace = $names[0]
    $onNamespace = {
        param($Step)
        $Step.applies -and $Step.kind -ceq 'operation' -and @($Step.arguments | Where-Object { $_.identity -and $_.value -is [System.Collections.IDictionary] -and $_.value.fixture -ceq $namespace }).Count -eq 1
    }
    $steps = @($Scenario.steps | Where-Object { & $onNamespace $_ })
    $create = @($steps | Where-Object { $entry = Get-JsonValue $_.expect 'entry'; $_.expect.status -eq 201 -and $entry -and $entry.fixture -ceq $namespace }) | Select-Object -First 1
    $delete = @($steps | Where-Object { $_.expect.status -eq 204 }) | Select-Object -First 1
    $absent = @($steps | Where-Object { (Get-JsonValue $_.expect 'attempts') -and (Get-JsonValue $_.expect 'error') }) | Select-Object -First 1
    if (-not $create -or -not $delete -or -not $absent) {
        throw "scenario $($Scenario.id) does not create, delete and confirm absent its owned folder $namespace."
    }
    if (-not @($delete.arguments | Where-Object { $_.wire_name -ceq 'recursive' -and $_.value -is [bool] }).Count) {
        throw "scenario $($Scenario.id) deletes its owned folder without a recursive argument."
    }
    [ordered]@{ Fixture = $namespace; Create = $create; Delete = $delete; Absent = $absent; Type = $Scenario.fixtures[$namespace].type }
}

# A run's context before anything about it is checked: its clock and deadlines, the launcher's inputs as given, and
# empty evidence, so that whatever fails later still has its receipt. Its identity holds only what needs no checking;
# Initialize-ScenarioRun adds each other part once it is checked, so a part that is not there was not established.
function New-ScenarioRun {
    param([hashtable] $Paths)
    [System.Globalization.CultureInfo]::CurrentCulture = $script:Invariant
    [System.Globalization.CultureInfo]::CurrentUICulture = $script:Invariant
    $inputs = Get-RunInput
    [ordered]@{
        Clock = [System.Diagnostics.Stopwatch]::StartNew()
        Deadlines = [ordered]@{ work = $script:WorkSeconds; cleanup = $null; whole = $script:WholeSeconds - $script:ReceiptSeconds }
        Phase = 'setup'
        Paths = $Paths
        Inputs = $inputs
        Identity = [ordered]@{
            runId = $inputs.RunId
            powershell = [ordered]@{
                version = $PSVersionTable.PSVersion.ToString()
                edition = $PSVersionTable.PSEdition
                os = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
                framework = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
                architecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
                culture = [System.Globalization.CultureInfo]::CurrentCulture.Name
            }
            bounds = [ordered]@{
                workSeconds = $script:WorkSeconds; cleanupSeconds = $script:CleanupSeconds; wholeSeconds = $script:WholeSeconds; receiptSeconds = $script:ReceiptSeconds
                holdSeconds = $script:HoldSeconds; stopSeconds = $script:StopSeconds; maxBodyBytes = $inputs.MaxBodyBytes
            }
        }
        Positions = [System.Collections.Generic.List[object]]::new()
        Failure = $null
        CleanupFailure = $null
        Ownership = [ordered]@{
            run = [ordered]@{ path = $null; owned = $false; removed = $false; evidence = $null }
            namespace = [ordered]@{ path = $null; owned = $false; deleted = $false; removed = $false; evidence = $null }
        }
        Cleanup = [System.Collections.Generic.List[object]]::new()
        Unresolved = [System.Collections.Generic.List[string]]::new()
        Listed = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
        StepJournal = @{}
        ExcludedEntries = [System.Collections.Generic.List[long]]::new()
        Journal = $null
        Transport = $null
        Connection = $null
        Runspace = $null
        Work = $null
        # A command that did not settle when stopped (Invoke-PublicCommand), and its pipeline, left to the launcher.
        Unsettled = $null
        UnsettledPipeline = $null
    }
}

# Checks the run's inputs and binds the run to them, adding each part of its identity once checked: the definition, the
# raw capture, the normalized simulation schema and the module generation, joined; this server, its reset and the
# canonical preset, as the launcher saved them; the case; and the installed module, package and transport support.
function Initialize-ScenarioRun {
    param($Run, [string] $CaseId)
    if ($Run.Inputs.Missing.Count) {
        throw "$($Run.Inputs.Missing -join ', ') not set: the consumer runs only under generated/mock-server/with-scenario.rb."
    }
    $Paths = $Run.Paths
    if ($Run.Inputs.Scenario -cne $script:Preset) {
        throw "The launcher loaded the scenario '$($Run.Inputs.Scenario)', not $script:Preset."
    }

    # The definition, the raw capture, the normalized simulation schema and the module generation, joined.
    $manifest = Read-JsonFile -Path $Paths.CasesPath
    Assert-Same -Actual $manifest.format -Expected 'filescom-powershell-api-scenarios/1' -What 'cases.json format'
    $definitionSha256 = Get-FileSha256 -Path $Paths.DefinitionPath
    Assert-Same -Actual (Get-JsonValue $manifest 'definition', 'sha256') -Expected $definitionSha256 -What 'the definition SHA-256 cases.json was rendered from'
    $Run.Identity.definition = [ordered]@{ path = $Paths.DefinitionPath; sha256 = $definitionSha256; rendered = $manifest.definition }
    Assert-Same -Actual $manifest.schema_sha256 -Expected $Paths.SchemaSha256 -What 'the schema SHA-256 cases.json was rendered from (the raw capture)'
    $generation = Read-JsonFile -Path $Paths.GenerationPath
    Assert-Same -Actual (Get-JsonValue $generation 'swagger', 'sha256') -Expected $Paths.SchemaSha256 -What 'the simulator generation swagger.sha256 (the raw capture)'
    $Run.Identity.rawCaptureSha256 = $Paths.SchemaSha256
    Assert-Same -Actual (Get-JsonValue $generation 'schema_sha256') -Expected $Run.Inputs.SchemaSha256 -What 'the simulator generation schema_sha256 (the normalized simulation schema)'
    $Run.Identity.normalizedSchemaSha256 = $Run.Inputs.SchemaSha256
    $Run.Identity.generation = [ordered]@{ path = $Paths.GenerationPath; sha256 = Get-FileSha256 -Path $Paths.GenerationPath }

    # This server, its reset and the canonical preset, as the launcher saved them.
    $ready = Read-JsonFile -Path (Join-Path -Path $run.Inputs.Out -ChildPath 'ready.json')
    $reset = Read-JsonFile -Path (Join-Path -Path $run.Inputs.Out -ChildPath 'reset.json')
    $preset = Read-JsonFile -Path $run.Inputs.ScenarioFile
    Assert-Same -Actual $ready.status -Expected 'ready' -What 'readiness status'
    Assert-Same -Actual $ready.mode -Expected 'simulation' -What 'readiness mode'
    Assert-Same -Actual $ready.instance -Expected $run.Inputs.Instance -What 'readiness instance'
    Assert-Same -Actual $ready.schema_sha256 -Expected $run.Inputs.SchemaSha256 -What 'readiness schema_sha256'
    Assert-Same -Actual $preset.name -Expected $script:Preset -What 'the preset name'
    Assert-Same -Actual (Get-JsonValue $preset 'reset', 'profile', 'api_scenarios', 'definition_sha256') -Expected $definitionSha256 -What 'the preset definition_sha256'
    Assert-Same -Actual $reset.instance -Expected $run.Inputs.Instance -What 'the reset instance'
    Assert-Same -Actual (Get-JsonValue $reset 'api_scenarios', 'definition_sha256') -Expected $definitionSha256 -What 'the reset api_scenarios definition_sha256'
    $Run.Epoch = [long] $reset.epoch
    $Run.Identity.preset = [ordered]@{ name = $script:Preset; path = $Run.Inputs.ScenarioFile; sha256 = Get-FileSha256 -Path $Run.Inputs.ScenarioFile; profile = Get-JsonValue $preset 'reset', 'profile' }
    $Run.Identity.server = [ordered]@{ url = $Run.Inputs.ServerUrl; instance = $Run.Inputs.Instance; contractVersion = $ready.contract_version; simulatorVersion = $ready.simulator_version; limits = $ready.limits }
    $Run.Identity.reset = [ordered]@{ epoch = $Run.Epoch; apiScenarios = $reset.api_scenarios }

    $run.Manifest = $manifest
    $scenarios = @($manifest.scenarios | Where-Object { $_.id -ceq $CaseId })
    if ($scenarios.Count -ne 1) {
        throw "cases.json has $($scenarios.Count) scenarios with the id '$CaseId', not one."
    }
    $run.Scenario = $scenarios[0]
    $run.Protocol = Get-NamespaceProtocol -Scenario $run.Scenario

    # The simulator's operations, by its operation IDs and by the schema's, and the API path prefix they share.
    $run.ReadyOperations = @{}
    $run.BySwagger = @{}
    foreach ($operation in @($ready.operations)) {
        $run.ReadyOperations[[string] $operation.id] = $operation
        if ($operation.swagger_operation_id) {
            $run.BySwagger[[string] $operation.swagger_operation_id] = $operation
        }
    }
    $reference = $run.Protocol.Absent.operation
    $served = $run.BySwagger[[string] $reference.operation_id]
    if (-not $served -or -not ([string] $served.path).EndsWith([string] $reference.path, [System.StringComparison]::Ordinal)) {
        throw "The simulator does not serve $($reference.route) as $($reference.operation_id)."
    }
    $run.ApiPrefix = ([string] $served.path).Substring(0, ([string] $served.path).Length - ([string] $reference.path).Length)
    $Run.Transfers = Get-TransferOperation -Ready $ready

    Import-Module -Name $Paths.ModuleManifest -Force -ErrorAction Stop
    $module = @(Get-Module -Name FilesCom.PowerShell)
    if ($module.Count -ne 1) {
        throw "$($module.Count) FilesCom.PowerShell modules are imported, not the one at $($Paths.ModuleManifest)."
    }
    Assert-Same -Actual $module[0].Version -Expected $manifest.module_version -What 'the installed module version'
    Add-Type -Path $Paths.TransportAssembly
    $Run.Identity.module = [ordered]@{
        manifest = $Paths.ModuleManifest
        version = $module[0].Version.ToString()
        base = $module[0].ModuleBase
        assemblySha256 = Get-FileSha256 -Path (Join-Path -Path $module[0].ModuleBase -ChildPath 'FilesCom.PowerShell.dll')
        buildInfoSha256 = Get-FileSha256 -Path (Join-Path -Path $module[0].ModuleBase -ChildPath 'FilesCom.BuildInfo.json')
    }
    $Run.Identity.package = [ordered]@{ path = $Paths.PackagePath; sha256 = Get-FileSha256 -Path $Paths.PackagePath }
    $Run.Identity.transport = [ordered]@{ path = $Paths.TransportAssembly; sha256 = Get-FileSha256 -Path $Paths.TransportAssembly }
}

# The simulator's storage transfer operations, from its readiness, which lists every one with its method: by operation
# ID, the one that stores an upload part (PUT), which must be the only one, and those that read stored bytes (GET).
function Get-TransferOperation {
    param($Ready)
    $byId = @{}
    foreach ($operation in @(Get-ListItem -List (Get-JsonValue $Ready 'transfers', 'operations') -What 'the readiness transfers.operations')) {
        $byId[[string] $operation.id] = [string] $operation.method
    }
    $uploads = @($byId.Keys | Where-Object { $byId[$_] -ceq 'PUT' })
    if ($uploads.Count -ne 1) {
        throw "The simulator lists $($uploads.Count) storage upload operations, not one."
    }
    [ordered]@{ ById = $byId; UploadPart = [string] $uploads[0]; Reads = Get-OrdinalSorted -Values @($byId.Keys | Where-Object { $byId[$_] -ceq 'GET' }) }
}

# The absolute deadline of the run's current phase, in seconds on its clock: the work's; the cleanup's, CleanupSeconds
# after the cleanup started; or, for the journal after the cleanup, the whole run's. None is after the whole run's.
function Get-PhaseDeadline {
    param($Run)
    $deadline = switch -CaseSensitive ($Run.Phase) {
        'cleanup' { $Run.Deadlines.cleanup }
        'journal' { $Run.Deadlines.whole }
        default { $Run.Deadlines.work }
    }
    [math]::Min([double] $deadline, [double] $Run.Deadlines.whole)
}

# What is left of the current phase's budget, in seconds, which may be nothing or less.
function Get-PhaseRemaining {
    param($Run)
    (Get-PhaseDeadline -Run $Run) - $Run.Clock.Elapsed.TotalSeconds
}

# Starts the cleanup phase, whose deadline is CleanupSeconds from now and never after the whole run's. Starting it again
# keeps that deadline.
function Start-CleanupPhase {
    param($Run)
    $Run.Phase = 'cleanup'
    if ($null -eq $Run.Deadlines.cleanup) {
        $Run.Deadlines.cleanup = [math]::Min($Run.Clock.Elapsed.TotalSeconds + $script:CleanupSeconds, $Run.Deadlines.whole)
    }
}

# How long a stopped pipeline may take to settle, in seconds: StopSeconds, but never past the whole run's deadline.
function Get-StopAllowance {
    param($Run)
    [math]::Max(0, [math]::Min($script:StopSeconds, $Run.Deadlines.whole - $Run.Clock.Elapsed.TotalSeconds))
}

function ConvertTo-WaitMilliseconds {
    param([double] $Seconds)
    [int] [math]::Max(0, [math]::Min([int]::MaxValue, [math]::Floor($Seconds * 1000)))
}

# The whole seconds a control request to the simulator may take: what is left of the current phase, rounded down.
# Fails when less than a second is left, since Invoke-WebRequest takes no shorter timeout and 0 would be none.
function Get-ControlTimeoutSeconds {
    param($Run)
    $seconds = [math]::Floor((Get-PhaseRemaining -Run $Run))
    if ($seconds -lt 1) {
        throw "the $($Run.Phase) budget ran out"
    }
    [int] [math]::Min($seconds, [int]::MaxValue)
}

function Assert-PhaseBudget {
    param($Run)
    if ((Get-PhaseRemaining -Run $Run) -le 0) {
        throw "the $($Run.Phase) budget ran out"
    }
}

# Why a step cannot run with the installed module, or nothing: its command or a parameter is missing, or it says or
# expects something the adapter does not bind.
function Get-StepBindingProblem {
    param($Step)
    if ($Step.kind -ceq 'operation') {
        $command = Get-Command -Name $Step.command.name -Module FilesCom.PowerShell -ErrorAction Ignore
        if (-not $command) {
            return "$($Step.operation.route) binds to $($Step.command.name), which the installed module does not have"
        }
        foreach ($argument in @($Step.arguments)) {
            if (-not $command.Parameters.ContainsKey($argument.parameter)) {
                "$($Step.command.name) has no parameter -$($argument.parameter) for $($argument.wire_name)"
            }
        }
        foreach ($name in @($Step.expect.Keys)) {
            if ($name -cnotin $script:OperationExpect) {
                "it expects $name, which the adapter does not check"
            }
        }
        # An entry and a listing are expected only where the definition gives them.
        $entry = Get-JsonValue $Step.expect 'entry'
        if ($entry) {
            foreach ($field in @($entry.fields)) {
                foreach ($matcher in @($field.matcher.Keys)) {
                    if ($matcher -cnotin $script:Matchers) {
                        "it matches $($field.name) with $matcher, which the adapter does not check"
                    }
                }
            }
        }
        if ((Get-JsonValue $Step.expect 'listing') -and -not ($Step.command.paged -and $command.Parameters.ContainsKey('Page') -and $command.Parameters.ContainsKey('Continuation'))) {
            "$($Step.command.name) does not list one page at a time with -Page and -Continuation"
        }
        foreach ($name in @('Connection') + @(if ($Step.command.mutation) { 'Confirm' })) {
            if (-not $command.Parameters.ContainsKey($name)) {
                "$($Step.command.name) has no -$name"
            }
        }
        return
    }
    $binding = $script:IntentBindings[[string] $Step.intent]
    if (-not $binding) {
        return "the intent $($Step.intent) has no PowerShell binding"
    }
    foreach ($name in @($Step.details.Keys)) {
        if ($name -cnotin $binding.Details) {
            "it says $name, which the $($Step.intent) binding does not take"
        }
    }
    foreach ($name in @($Step.expect.Keys)) {
        if ($name -cnotin $binding.Expect) {
            "it expects $name, which the $($Step.intent) binding does not check"
        }
    }
    $given = Get-JsonValue $Step.details 'parameters'
    if ($given) {
        foreach ($name in @($given.Keys)) {
            if (-not $binding.Parameters.ContainsKey($name)) {
                "it sends $name, which the $($Step.intent) binding does not bind to a parameter"
            }
        }
    }
    # Its journal is checked by the role of each operation the definition names for it, which must be exactly these.
    $routes = @(@($Step.operations) | ForEach-Object { [string] $_.route })
    foreach ($route in @($binding.Routes.Values)) {
        if ($route -cnotin $routes) {
            "the $($Step.intent) does not name $route, which its journal check needs"
        }
    }
    foreach ($route in $routes) {
        if ($route -cnotin @($binding.Routes.Values)) {
            "the $($Step.intent) names $route, which its journal check does not know"
        }
    }
    switch -CaseSensitive ([string] $Step.intent) {
        'upload' {
            if ([string] $Step.details.source -cnotin $script:UploadSources) {
                "it uploads from '$($Step.details.source)', a source the adapter does not bind"
            }
            if ($Step.details.source -ceq 'forward-only-stream' -and -not ([int] $Step.details.max_read -ge 1)) {
                'it uploads from a forward-only stream without a max_read of at least 1'
            }
            $cancel = Get-JsonValue $Step.details 'cancel'
            if ($cancel -and [string] $cancel -cnotin $script:Cancellations) {
                "it cancels '$cancel', which the adapter does not bind"
            }
        }
        'download' {
            $source = Get-JsonValue $Step.details 'source'
            if ($source -and [string] $source -cnotin $script:DownloadSources) {
                "it downloads from '$source', a source the adapter does not bind"
            }
        }
        { $_ -in 'copy', 'move' } {
            $action = $Step.action
            if (-not $action) {
                return "the $($Step.intent) has no command binding"
            }
            $command = Get-Command -Name $action.command -Module FilesCom.PowerShell -ErrorAction Ignore
            if (-not $command) {
                return "$($action.route) binds to $($action.command), which the installed module does not have"
            }
            foreach ($wire in 'path', 'destination') {
                $parameter = Get-JsonValue $action 'parameters', $wire
                if (-not $parameter -or -not $command.Parameters.ContainsKey([string] $parameter)) {
                    "$($action.command) has no parameter for $wire"
                }
            }
        }
        'upload-directory' {
            if (-not $Step.tree) {
                'it uploads a directory without its tree'
            }
        }
        'download-directory' {
            if (-not $Step.tree) {
                'it downloads a directory without the tree it must write'
            }
            elseif (@($Step.tree.files).Count -ne [int] $Step.expect.files -or @($Step.tree.empty_directories).Count -ne [int] $Step.expect.empty_folders) {
                "its folder's tree has $(@($Step.tree.files).Count) files and $(@($Step.tree.empty_directories).Count) empty folders, but it expects $($Step.expect.files) and $($Step.expect.empty_folders)"
            }
        }
    }
}

# Fails, before any request, when any step the simulator runs cannot be bound to the installed module, or a step it
# does not run has no reason.
function Assert-ScenarioBinding {
    param($Run)
    $problems = [System.Collections.Generic.List[string]]::new()
    foreach ($name in $script:TransferCommands.Keys) {
        $command = Get-Command -Name $name -Module FilesCom.PowerShell -ErrorAction Ignore
        if (-not $command) {
            $problems.Add("the installed module has no $name")
            continue
        }
        foreach ($parameter in $script:TransferCommands[$name]) {
            if (-not $command.Parameters.ContainsKey($parameter)) {
                $problems.Add("$name has no -$parameter")
            }
        }
    }
    foreach ($step in @($Run.Scenario.steps)) {
        if (-not $step.applies) {
            if (-not $step.omission) {
                $problems.Add("position $($step.number) is omitted without a reason")
            }
            continue
        }
        foreach ($problem in @(Get-StepBindingProblem -Step $step)) {
            $problems.Add("position $($step.number): $problem")
        }
    }
    if ($problems.Count) {
        throw "The scenario does not bind to the installed module: $($problems -join '; ')."
    }
}

# Makes the run's connection on the observing transport, and the runspace every command runs in, with the installed
# module imported there too.
function Connect-ObservedAccount {
    param($Run)
    $Run.Transport = [FilesCom.ApiScenarios.ObservingHandler]::new([uri] $Run.Inputs.ServerUrl, $Run.ApiPrefix, (Get-TextSha256 -Text $Run.Inputs.ApiKey), $script:ObservationLimit)
    $key = [System.Security.SecureString]::new()
    foreach ($character in $Run.Inputs.ApiKey.ToCharArray()) {
        $key.AppendChar($character)
    }
    $key.MakeReadOnly()
    $Run.Transport.Step = 'connect'
    $Run.Connection = Connect-FilesAccount -ApiKey $key -BaseUrl $Run.Inputs.ServerUrl -HttpMessageHandler $Run.Transport -NoDefault -ErrorAction Stop
    $Run.Runspace = New-CommandRunspace -Run $Run
}

# An open runspace for commands, with the installed module imported.
function New-CommandRunspace {
    param($Run)
    $state = [System.Management.Automation.Runspaces.InitialSessionState]::CreateDefault2()
    $state.ImportPSModule([string[]] @($Run.Paths.ModuleManifest))
    $runspace = [System.Management.Automation.Runspaces.RunspaceFactory]::CreateRunspace($state)
    try {
        $runspace.Open()
    }
    catch {
        $runspace.Dispose()
        throw
    }
    $runspace
}

# Runs one public command, Name with Parameters, as Start-PublicCommand starts it, and waits for it as
# Wait-PublicCommand does, with the same Deadline. With -StopAtBoundary, it stops the pipeline as soon as the
# transport holds the request after the upload's first acknowledged part.
function Invoke-PublicCommand {
    param($Run, [string] $Name, [hashtable] $Parameters, [double] $Deadline = [double]::MaxValue, [switch] $StopAtBoundary)
    $held = if ($StopAtBoundary) { $Run.Transport.HeldWaitHandle } else { $null }
    $command = Start-PublicCommand -Run $Run -Name $Name -Parameters $Parameters -Deadline $Deadline
    Wait-PublicCommand -Run $Run -Command $command -Deadline $Deadline -Held $held
}

# Starts one public command, Name with Parameters, as a pipeline of its own in the run's command runspace with the
# invariant culture, and returns it. It is refused before anything is scheduled, so that it sends nothing, in a runspace
# whose last command has not settled, and, with a TimeoutException, once its deadline has passed: the current phase's,
# or Deadline (in seconds on the run's clock) when that is earlier.
function Start-PublicCommand {
    param($Run, [string] $Name, [hashtable] $Parameters, [double] $Deadline = [double]::MaxValue)
    if ($Run.Unsettled) {
        throw "$Name was not run: $($Run.Unsettled.command), stopped at $($Run.Unsettled.step), has not settled, and its runspace is not used again"
    }
    $until = [math]::Min((Get-PhaseDeadline -Run $Run), $Deadline)
    $now = $Run.Clock.Elapsed.TotalSeconds
    if ($now -ge $until) {
        throw [System.TimeoutException]::new("$Name was not run: its deadline, $([math]::Round($until, 3)) s into the run, had passed at $([math]::Round($now, 3)) s, and nothing was sent")
    }
    $shell = [System.Management.Automation.PowerShell]::Create()
    $shell.Runspace = $Run.Runspace
    $body = 'param($CommandName, $CommandParameters) $culture = [System.Globalization.CultureInfo]::InvariantCulture; [System.Threading.Thread]::CurrentThread.CurrentCulture = $culture; [System.Threading.Thread]::CurrentThread.CurrentUICulture = $culture; & $CommandName @CommandParameters'
    [void] $shell.AddScript($body).AddArgument($Name).AddArgument($Parameters)
    $none = [System.Management.Automation.PSDataCollection[psobject]]::new()
    $none.Complete()
    $written = [System.Management.Automation.PSDataCollection[psobject]]::new()
    # What it writes and its progress records are read as they stand, without waiting: once this PowerShell holds them,
    # their enumerators otherwise wait for its pipeline to end, so one that does not settle would hold its reader too.
    $written.EnumeratorNeverBlocks = $true
    $shell.Streams.Progress.EnumeratorNeverBlocks = $true
    $async = $shell.BeginInvoke($none, $written)
    [pscustomobject]@{ Name = $Name; Shell = $shell; Async = $async; Written = $written }
}

# Waits for a command Start-PublicCommand started until it ends or its deadline passes: the current phase's, or Deadline
# when that is earlier. Then it stops the pipeline, as Ctrl+C does, and waits for it to settle for what
# Get-StopAllowance leaves. Given Held, the transport's HeldWaitHandle, it stops the pipeline the same way as soon as the
# transport holds the request after the upload's first acknowledged part, which calls the command's StopProcessing.
# Each wait takes only what is left before its own deadline, once. It returns what the pipeline wrote, its failure (for
# a pipeline stopped at its deadline, that it did not end in time; what stopping it then threw is kept apart as
# StopFailure), its progress records and final state, and how stopping went. A pipeline that has not settled even then
# is left running: the run records it as Unsettled, and no later command runs in its runspace.
function Wait-PublicCommand {
    param($Run, $Command, [double] $Deadline = [double]::MaxValue, [System.Threading.WaitHandle] $Held)
    $Name = $Command.Name
    $shell = $Command.Shell
    $async = $Command.Async
    $written = $Command.Written
    $until = [math]::Min((Get-PhaseDeadline -Run $Run), $Deadline)
    $outcome = [ordered]@{ Output = @(); Failure = $null; StopFailure = $null; Progress = @(); State = $null; Held = $false; StopRequested = $false; StopSettled = $null; StopMilliseconds = $null; TimedOut = $false; Unsettled = $false }
    $limit = ConvertTo-WaitMilliseconds -Seconds ($until - $Run.Clock.Elapsed.TotalSeconds)
    if ($held) {
        $signalled = [System.Threading.WaitHandle]::WaitAny([System.Threading.WaitHandle[]] @($async.AsyncWaitHandle, $held), $limit)
        $outcome.Held = $signalled -eq 1
        $outcome.TimedOut = $signalled -eq [System.Threading.WaitHandle]::WaitTimeout
    }
    else {
        $outcome.TimedOut = -not $async.AsyncWaitHandle.WaitOne($limit)
    }
    if (($outcome.Held -or $outcome.TimedOut) -and -not $async.IsCompleted) {
        $outcome.StopRequested = $true
        $stopping = [System.Diagnostics.Stopwatch]::StartNew()
        $stop = $shell.BeginStop($null, $null)
        $outcome.StopSettled = $stop.AsyncWaitHandle.WaitOne((ConvertTo-WaitMilliseconds -Seconds (Get-StopAllowance -Run $Run)))
        $outcome.StopMilliseconds = [math]::Round($stopping.Elapsed.TotalMilliseconds, 1)
        if ($outcome.StopSettled) {
            $shell.EndStop($stop)
        }
    }
    elseif ($outcome.TimedOut) {
        # It ended as its deadline passed, before anything stopped it.
        $outcome.TimedOut = $false
    }
    # Whether the pipeline has ended is decided once, here, after its waits: one that has not is left running, unsettled,
    # whenever it ends later.
    $ended = $async.IsCompleted
    if ($ended) {
        try {
            [void] $shell.EndInvoke($async)
        }
        catch {
            $outcome.Failure = $_
        }
    }
    # Everything it has written and its progress records, as they stand: Start-PublicCommand made both readable without
    # waiting.
    $outcome.Output = @($written)
    $outcome.Progress = @($shell.Streams.Progress)
    $outcome.State = [string] $shell.InvocationStateInfo.State
    if ($outcome.TimedOut) {
        $outcome.StopFailure = $outcome.Failure
        $outcome.Failure = "$Name did not end by its deadline, $([math]::Round($until, 1)) s into the run"
    }
    if ($ended) {
        $shell.Dispose()
    }
    else {
        $outcome.Unsettled = $true
        $Run.Unsettled = [ordered]@{ command = $Name; step = $Run.Transport.Step; stopRequested = $outcome.StopRequested; deadline = [math]::Round($until, 3); secondsIntoRun = [math]::Round($Run.Clock.Elapsed.TotalSeconds, 3); localWork = $null }
        $Run.UnsettledPipeline = [ordered]@{ Shell = $shell; Async = $async }
    }
    $outcome
}

# The FilesException behind a failure, or $null: the failure's own exception, or one that it or its inner exceptions wrap.
function Get-FilesException {
    param($Failure)
    $pending = [System.Collections.Generic.Queue[object]]::new()
    if ($Failure -is [System.Management.Automation.ErrorRecord]) {
        $pending.Enqueue($Failure.Exception)
    }
    elseif ($Failure -is [System.Exception]) {
        $pending.Enqueue($Failure)
    }
    $visited = 0
    while ($pending.Count -and $visited -lt 32) {
        $visited++
        $exception = $pending.Dequeue()
        if ($null -eq $exception) {
            continue
        }
        if ($exception -is [FilesCom.PowerShell.FilesException]) {
            return $exception
        }
        if ($exception -is [System.Management.Automation.IContainsErrorRecord] -and $exception.ErrorRecord) {
            $pending.Enqueue($exception.ErrorRecord.Exception)
        }
        if ($exception.InnerException) {
            $pending.Enqueue($exception.InnerException)
        }
    }
    $null
}

function Format-Failure {
    param($Failure)
    if ($null -eq $Failure) {
        return ''
    }
    $exception = Get-FilesException -Failure $Failure
    if ($exception) {
        return "$($exception.Kind) $($exception.HttpStatus) $($exception.ErrorType): $($exception.Message)"
    }
    if ($Failure -is [System.Management.Automation.ErrorRecord]) {
        return $Failure.Exception.Message
    }
    [string] $Failure
}

function Format-Observation {
    param($Observation)
    $path = if ($Observation.Kind -ceq 'api') { $Observation.Path } else { 'a storage address' }
    "$($Observation.Sequence) $($Observation.Method) $path, $($Observation.Outcome) $($Observation.Status)"
}

# Runs Action with the transport's requests labelled Label, and returns what it returned with the requests it sent.
# Each command settles before the next starts, or is the last its run starts, so the requests after the count taken
# first are exactly this step's.
function Invoke-ObservedStep {
    param($Run, [string] $Label, [scriptblock] $Action)
    $Run.Transport.Step = $Label
    $before = $Run.Transport.Sent
    $value = & $Action
    [pscustomobject]@{ Value = $value; Observations = @($Run.Transport.Since($before)) }
}

# What a request's decoded path holds in RoutePath's {path} placeholder after Prefix, with any leading or trailing /
# aside, or $null when the path is not that route's or the route has no {path}.
function Get-RoutePathValue {
    param([string] $Observed, [string] $Prefix, [string] $RoutePath)
    $index = $RoutePath.IndexOf('{path}', [System.StringComparison]::Ordinal)
    if ($index -lt 0) {
        return $null
    }
    $head = $Prefix + $RoutePath.Substring(0, $index)
    $tail = $RoutePath.Substring($index + '{path}'.Length)
    if ($Observed.Length -lt $head.Length + $tail.Length -or -not $Observed.StartsWith($head, [System.StringComparison]::Ordinal) -or -not $Observed.EndsWith($tail, [System.StringComparison]::Ordinal)) {
        return $null
    }
    $Observed.Substring($head.Length, $Observed.Length - $head.Length - $tail.Length).Trim('/')
}

# Whether a request's decoded path is Prefix and RoutePath with its {path} placeholder holding PathValue, with any
# leading or trailing / of that value aside.
function Test-RequestPath {
    param([string] $Observed, [string] $Prefix, [string] $RoutePath, [string] $PathValue)
    if ($RoutePath.IndexOf('{path}', [System.StringComparison]::Ordinal) -lt 0) {
        return $Observed -ceq ($Prefix + $RoutePath)
    }
    $value = Get-RoutePathValue -Observed $Observed -Prefix $Prefix -RoutePath $RoutePath
    $null -ne $value -and $value -ceq $PathValue.Trim('/')
}

# The requests of an operation step: one request to the operation's route and path for each status in Statuses, each
# answered with it, carrying exactly the run's API key, and none to another address or left unanswered.
function Assert-OperationRequest {
    param($Run, $Operation, [object[]] $Observations, [string] $PathValue, [int[]] $Statuses)
    if ($Observations.Count -ne $Statuses.Count) {
        throw "it sent $($Observations.Count) requests, not $($Statuses.Count): $(@($Observations | ForEach-Object { Format-Observation -Observation $_ }) -join '; ')"
    }
    for ($index = 0; $index -lt $Observations.Count; $index++) {
        $observation = $Observations[$index]
        if (-not $observation.SameOrigin) {
            throw "request $($observation.Sequence) went to an address other than the simulator's"
        }
        if ($observation.Kind -cne 'api' -or $observation.Method -cne $Operation.method -or -not (Test-RequestPath -Observed $observation.Path -Prefix $Run.ApiPrefix -RoutePath $Operation.path -PathValue $PathValue)) {
            throw "request $($observation.Sequence) was $(Format-Observation -Observation $observation), not $($Operation.route) for '$PathValue'"
        }
        if ($observation.Outcome -cne 'responded' -or $observation.Status -ne $Statuses[$index]) {
            throw "request $($observation.Sequence) was answered $($observation.Status) ($($observation.Outcome)), not $($Statuses[$index])"
        }
        if ($observation.Credential -cne 'api-key') {
            throw "request $($observation.Sequence) carried '$($observation.Credential)', not the run's API key alone"
        }
    }
}

# The requests of an intent step: each to the simulator, carrying the run's API key to the API and no credential to
# storage, and answered with success; with -AllowHeld, the one request a boundary held unsent aside.
function Assert-SentRequest {
    param([object[]] $Observations, [switch] $AllowHeld)
    if (-not $Observations.Count) {
        throw 'it sent no request'
    }
    foreach ($observation in $Observations) {
        if (-not $observation.SameOrigin) {
            throw "request $($observation.Sequence) went to an address other than the simulator's"
        }
        if ($AllowHeld -and $observation.Outcome -like 'held-*') {
            continue
        }
        if ($observation.Outcome -cne 'responded' -or $observation.Status -lt 200 -or $observation.Status -gt 299) {
            throw "request $(Format-Observation -Observation $observation) was not answered with success"
        }
        $expected = if ($observation.Kind -ceq 'api') { 'api-key' } else { 'none' }
        if ($observation.Credential -cne $expected) {
            throw "request $(Format-Observation -Observation $observation) carried '$($observation.Credential)', not '$expected'"
        }
    }
}

function Get-FixtureRemotePath {
    param($Run, [string] $Name)
    $fixture = $Run.Scenario.fixtures[$Name]
    if (-not $fixture) {
        throw "scenario $($Run.Scenario.id) has no fixture $Name"
    }
    if ($fixture.path -ceq '') { $Run.Namespace } else { "$($Run.Namespace)/$($fixture.path)" }
}

function Resolve-ArgumentValue {
    param($Run, $Argument)
    if ($Argument.value -is [System.Collections.IDictionary]) {
        return Get-FixtureRemotePath -Run $Run -Name $Argument.value.fixture
    }
    , $Argument.value
}

# The command and parameters for an operation step, with Override values (by wire name) in place of the step's own.
function Get-StepCall {
    param($Run, $Step, [hashtable] $Override = @{})
    $parameters = @{ Connection = $Run.Connection; ErrorAction = 'Stop' }
    if ($Step.command.mutation) {
        $parameters.Confirm = $false
    }
    $pathValue = $null
    foreach ($argument in @($Step.arguments)) {
        $value = if ($Override.ContainsKey($argument.wire_name)) { $Override[$argument.wire_name] } else { Resolve-ArgumentValue -Run $Run -Argument $argument }
        $parameters[$argument.parameter] = $value
        if ($argument.identity) {
            $pathValue = [string] $value
        }
    }
    [pscustomobject]@{ Name = $Step.command.name; Parameters = $parameters; Path = $pathValue }
}

# The identity argument's wire name of an operation step, which Override keys name.
function Get-IdentityWireName {
    param($Step)
    $identity = @($Step.arguments | Where-Object { $_.identity })
    if ($identity.Count -ne 1) {
        throw "$($Step.operation.route) has $($identity.Count) identity arguments, not one"
    }
    $identity[0].wire_name
}

# The value of an entry's schema property: the one public property whose name is the schema name without its
# separators, ignoring case.
function Get-EntryFieldValue {
    param($Object, [string] $Property)
    $key = ($Property -replace '[^A-Za-z0-9]', '').ToLowerInvariant()
    $found = @($Object.PSObject.Properties | Where-Object { ($_.Name -replace '[^A-Za-z0-9]', '').ToLowerInvariant() -ceq $key })
    if ($found.Count -ne 1) {
        throw "the entry has $($found.Count) properties for the schema property $Property, not one"
    }
    , $found[0].Value
}

# The UTC instant a date names, or $null for anything that names none: a DateTimeOffset; a DateTime, one of unspecified
# kind taken as UTC; or ISO 8601 text, taken as UTC too when it gives no offset. The module writes dates as DateTime
# values, and ConvertFrom-Json may already have read the definition's ISO 8601 text as one.
function ConvertTo-UtcInstant {
    param($Value)
    if ($Value -is [datetimeoffset]) {
        return $Value.UtcDateTime
    }
    if ($Value -is [datetime]) {
        if ($Value.Kind -eq [System.DateTimeKind]::Unspecified) {
            return [datetime]::SpecifyKind($Value, [System.DateTimeKind]::Utc)
        }
        return $Value.ToUniversalTime()
    }
    $instant = [datetimeoffset]::MinValue
    if ($Value -is [string] -and [datetimeoffset]::TryParse($Value, $script:Invariant, [System.Globalization.DateTimeStyles]::AssumeUniversal, [ref] $instant)) {
        return $instant.UtcDateTime
    }
    $null
}

# Whether a field's value is the expected one: null only for null; where either is a date, the same UTC instant, never
# its display text; a boolean only for the same boolean; anything else, the same text.
function Test-FieldValue {
    param($Value, $Expected)
    if ($null -eq $Expected) {
        return $null -eq $Value
    }
    if ($null -eq $Value) {
        return $false
    }
    if ($Value -is [datetime] -or $Value -is [datetimeoffset] -or $Expected -is [datetime] -or $Expected -is [datetimeoffset]) {
        $actual = ConvertTo-UtcInstant -Value $Value
        $instant = ConvertTo-UtcInstant -Value $Expected
        return $null -ne $actual -and $null -ne $instant -and $actual -eq $instant
    }
    if ($Value -is [bool] -or $Expected -is [bool]) {
        return ($Value -is [bool]) -and ($Expected -is [bool]) -and ($Value -eq $Expected)
    }
    [string] $Value -ceq [string] $Expected
}

function Assert-FieldMatcher {
    param($Value, $Matcher, [string] $What)
    foreach ($name in @($Matcher.Keys)) {
        switch -CaseSensitive ($name) {
            'one_of' {
                $any = $false
                foreach ($candidate in @($Matcher.one_of)) {
                    if (Test-FieldValue -Value $Value -Expected $candidate) {
                        $any = $true
                    }
                }
                if (-not $any) {
                    throw "$What is '$Value', not one of $(ConvertTo-Json -InputObject @($Matcher.one_of) -Compress)"
                }
            }
            'includes' {
                foreach ($item in @($Matcher.includes)) {
                    $has = if ($Value -is [string]) { $Value.Contains([string] $item) } else { @($Value | Where-Object { Test-FieldValue -Value $_ -Expected $item }).Count -gt 0 }
                    if (-not $has) {
                        throw "$What '$Value' does not include '$item'"
                    }
                }
            }
            'not' {
                if (Test-FieldValue -Value $Value -Expected $Matcher.not) {
                    throw "$What is '$Value', which it must not be"
                }
            }
            default {
                throw "$What has the matcher $name, which the adapter does not check"
            }
        }
    }
}

# An entry a command wrote, against the step's expected entry: a fixture's own path, type and (for a file) size, or a
# type and the paths allowed, and each field's matcher.
function Assert-EntryExpectation {
    param($Run, $Entry, $Object)
    if ($Entry.fixture) {
        $fixture = $Run.Scenario.fixtures[$Entry.fixture]
        Assert-Same -Actual (Get-ObjectMember -Object $Object -Name 'Path') -Expected (Get-FixtureRemotePath -Run $Run -Name $Entry.fixture) -What 'the entry path'
        Assert-Same -Actual (Get-ObjectMember -Object $Object -Name 'Type') -Expected $fixture.type -What 'the entry type'
        if ($fixture.type -ceq 'file') {
            Assert-Same -Actual (Get-ObjectMember -Object $Object -Name 'Size') -Expected $fixture.recipe.length -What 'the entry size'
        }
    }
    if ($Entry.type) {
        Assert-Same -Actual (Get-ObjectMember -Object $Object -Name 'Type') -Expected $Entry.type -What 'the entry type'
    }
    if ($Entry.path_one_of) {
        $path = [string] (Get-ObjectMember -Object $Object -Name 'Path')
        if ($path -cnotin @($Entry.path_one_of)) {
            throw "the entry path '$path' is not one of $(ConvertTo-Json -InputObject @($Entry.path_one_of) -Compress)"
        }
    }
    foreach ($field in @($Entry.fields)) {
        Assert-FieldMatcher -Value (Get-EntryFieldValue -Object $Object -Property $field.property) -Matcher $field.matcher -What "the entry's $($field.name)"
    }
}

function Get-RecipeContent {
    param($Recipe)
    $bytes = [FilesCom.ApiScenarios.Recipe]::Bytes([long] $Recipe.multiplier, [long] $Recipe.offset, [long] $Recipe.modulus, [long] $Recipe.length)
    $sha256 = [FilesCom.ApiScenarios.Recipe]::Sha256($bytes)
    if ($sha256 -cne $Recipe.sha256) {
        throw "recipe $($Recipe.number) makes bytes whose SHA-256 is $sha256, not $($Recipe.sha256)"
    }
    , $bytes
}

# A path in the run's own local work folder where nothing is yet.
function Get-LocalPath {
    param($Run, [string] $Name)
    $path = Join-Path -Path $Run.Work -ChildPath $Name
    if (Test-Path -LiteralPath $path) {
        throw "$path already exists"
    }
    $path
}

# An operation step: one request, or the lookups an absence allows, with the status witnessed at the transport, and
# the command's own answer: the expected error, or the expected entry. Its expectation always has its status, and a
# listing, an error, its retries or an entry only where the definition gives them: an error without retries is looked
# up once, and retries the definition gives, a zero interval included, are kept as given.
function Invoke-OperationStep {
    param($Run, $Step)
    $expect = $Step.expect
    if (Get-JsonValue $expect 'listing') {
        return Invoke-ListingStep -Run $Run -Step $Step
    }
    $expectedError = Get-JsonValue $expect 'error'
    $entry = Get-JsonValue $expect 'entry'
    $retries = Get-JsonValue $expect 'attempts'
    $retryInterval = Get-JsonValue $expect 'interval_seconds'
    $call = Get-StepCall -Run $Run -Step $Step
    $attempts = if ($null -ne $retries) { [int] $retries } else { 1 }
    $interval = if ($null -ne $retryInterval) { [double] $retryInterval } else { 0 }
    $statuses = [System.Collections.Generic.List[int]]::new()
    $observations = [System.Collections.Generic.List[object]]::new()
    $object = $null
    for ($attempt = 1; $attempt -le $attempts; $attempt++) {
        if ($attempt -gt 1) {
            if ((Get-PhaseRemaining -Run $Run) -le $interval) {
                throw "the $($Run.Phase) budget ran out before attempt $attempt of $attempts"
            }
            Start-Sleep -Seconds $interval
        }
        Assert-PhaseBudget -Run $Run
        $observed = Invoke-ObservedStep -Run $Run -Label (Get-StepLabel -Step $Step) -Action { Invoke-PublicCommand -Run $Run -Name $call.Name -Parameters $call.Parameters }
        $observations.AddRange([object[]] $observed.Observations)
        $outcome = $observed.Value
        if ($expectedError) {
            $exception = Get-FilesException -Failure $outcome.Failure
            if ($exception -and $exception.HttpStatus -eq $expect.status) {
                Assert-Same -Actual $exception.ErrorType -Expected $expectedError -What "the $($expect.status) error type"
                $statuses.Add([int] $expect.status)
                break
            }
            if ($outcome.Failure) {
                throw "$($call.Name) failed: $(Format-Failure -Failure $outcome.Failure)"
            }
            if ($attempt -eq $attempts) {
                throw "$($call.Name) found '$($call.Path)' in each of $attempts attempts, not $($expect.status) $expectedError"
            }
            # The path is still there; the lookup that found it was a 200.
            $statuses.Add(200)
            continue
        }
        if ($outcome.Failure) {
            throw "$($call.Name) failed: $(Format-Failure -Failure $outcome.Failure)"
        }
        $statuses.Add([int] $expect.status)
        $written = @($outcome.Output)
        if ($entry) {
            if ($written.Count -ne 1) {
                throw "$($call.Name) wrote $($written.Count) objects, not the one entry"
            }
            $object = $written[0]
        }
        elseif ($expect.status -eq 204 -and $written.Count) {
            throw "$($call.Name) wrote $($written.Count) objects for a 204"
        }
    }
    Assert-OperationRequest -Run $Run -Operation $Step.operation -Observations $observations.ToArray() -PathValue $call.Path -Statuses $statuses.ToArray()
    if ($entry) {
        Assert-EntryExpectation -Run $Run -Entry $entry -Object $object
    }

    # The scenario's own folder is owned once its creation is witnessed as a 201 that returned exactly it as a folder,
    # and removed once its non-recursive deletion and absence are.
    $namespace = $Run.Ownership.namespace
    if ($Step.number -eq $Run.Protocol.Create.number) {
        $namespace.owned = $true
        $namespace.evidence = [ordered]@{ position = $Step.number; request = $observations[0].Sequence; status = $observations[0].Status; path = [string] (Get-ObjectMember -Object $object -Name 'Path'); type = [string] (Get-ObjectMember -Object $object -Name 'Type') }
    }
    if ($call.Path -ceq $Run.Namespace -and $expect.status -eq $Run.Protocol.Delete.expect.status -and $Step.operation.route -ceq $Run.Protocol.Delete.operation.route) {
        $namespace.deleted = $true
    }
    if ($call.Path -ceq $Run.Namespace -and $namespace.deleted -and $retries) {
        $namespace.removed = $true
    }
    [ordered]@{ requests = $observations.Count; statuses = $statuses.ToArray(); path = $call.Path }
}

# A listing step: the first page with -Page, then each next page with -Continuation, at most max_pages pages each of at
# most per_page entries, the last with no continuation, holding each expected fixture exactly once and nothing else.
function Invoke-ListingStep {
    param($Run, $Step)
    $listing = $Step.expect.listing
    $call = Get-StepCall -Run $Run -Step $Step
    $perPage = [int] $listing.per_page
    $maxPages = [int] $listing.max_pages
    $parameters = $call.Parameters.Clone()
    $parameters.Page = $true
    $items = [System.Collections.Generic.List[object]]::new()
    $observations = [System.Collections.Generic.List[object]]::new()
    $sizes = [System.Collections.Generic.List[int]]::new()
    while ($true) {
        if ($sizes.Count -ge $maxPages) {
            throw "the listing has more than $maxPages pages"
        }
        Assert-PhaseBudget -Run $Run
        $observed = Invoke-ObservedStep -Run $Run -Label (Get-StepLabel -Step $Step) -Action { Invoke-PublicCommand -Run $Run -Name $call.Name -Parameters $parameters }
        $observations.AddRange([object[]] $observed.Observations)
        if ($observed.Value.Failure) {
            throw "page $($sizes.Count + 1) failed: $(Format-Failure -Failure $observed.Value.Failure)"
        }
        $pages = @($observed.Value.Output)
        if ($pages.Count -ne 1) {
            throw "$($call.Name) wrote $($pages.Count) objects for page $($sizes.Count + 1), not one page"
        }
        # The page's own entries, none, one or several, never the page's array as one entry.
        $pageItems = @(Get-ListItem -List (Get-ObjectMember -Object $pages[0] -Name 'Items') -What "page $($sizes.Count + 1)'s Items")
        if ($pageItems.Count -gt $perPage) {
            throw "page $($sizes.Count + 1) has $($pageItems.Count) entries, more than $perPage"
        }
        $sizes.Add($pageItems.Count)
        $items.AddRange([object[]] $pageItems)
        $continuation = Get-ObjectMember -Object $pages[0] -Name 'Continuation'
        if ($null -eq $continuation) {
            break
        }
        $parameters = @{ Continuation = $continuation; Connection = $Run.Connection; ErrorAction = 'Stop' }
    }
    Assert-OperationRequest -Run $Run -Operation $Step.operation -Observations $observations.ToArray() -PathValue $call.Path -Statuses ([int[]] @(foreach ($size in $sizes) { $Step.expect.status }))
    $paths = @($items | ForEach-Object { [string] (Get-ObjectMember -Object $_ -Name 'Path') })
    $repeated = @($paths | Group-Object -CaseSensitive | Where-Object { $_.Count -gt 1 } | ForEach-Object { $_.Name })
    if ($repeated.Count) {
        throw "the listing has $($repeated -join ', ') more than once"
    }
    if ($null -ne $listing.entries) {
        $expected = @(foreach ($name in @($listing.entries)) { Get-FixtureRemotePath -Run $Run -Name $name })
        $missing = @($expected | Where-Object { $_ -cnotin $paths })
        $extra = @($paths | Where-Object { $_ -cnotin $expected })
        if ($missing.Count -or $extra.Count) {
            throw "the listing lacks [$($missing -join ', ')] and has others [$($extra -join ', ')]"
        }
        foreach ($name in @($listing.entries)) {
            $path = Get-FixtureRemotePath -Run $Run -Name $name
            $item = @($items | Where-Object { [string] $_.Path -ceq $path })[0]
            Assert-Same -Actual (Get-ObjectMember -Object $item -Name 'Type') -Expected $Run.Scenario.fixtures[$name].type -What "the listed $name type"
        }
    }
    # The objects a listing returned, for later steps that use a listed entry.
    foreach ($item in $items) {
        $Run.Listed[[string] $item.Path] = $item
    }
    [ordered]@{ pages = $sizes.Count; entries = $sizes.ToArray(); listed = $paths; requests = $observations.Count; path = $call.Path }
}

# Why an upload stopped at its first acknowledged part was not cancelled as the definition asks, or nothing: the first
# acknowledged part was part 1, the next request was held unsent until the command's StopProcessing cancelled it, the
# pipeline was stopped and settled, and the command wrote nothing.
function Get-CancellationProblem {
    param($Outcome, $Boundary)
    if (-not $Boundary) {
        return 'no request boundary was armed'
    }
    if (-not $Boundary.Acknowledged) {
        'no storage part was acknowledged'
    }
    elseif ($Boundary.AcknowledgedPart -ne 1) {
        "the first acknowledged part was part $($Boundary.AcknowledgedPart), not part 1"
    }
    if (-not $Boundary.Held) {
        'no request after the first acknowledged part was held'
    }
    elseif ($Boundary.HeldOutcome -cne 'held-cancelled-unsent') {
        "the held request ended '$($Boundary.HeldOutcome)', not cancelled by the command's stop before it was sent"
    }
    if (-not $Outcome.StopRequested) {
        'the pipeline was not stopped at the boundary'
    }
    elseif (-not $Outcome.StopSettled) {
        "stopping the pipeline did not settle within $script:StopSeconds s"
    }
    if ($Outcome.State -cne 'Stopped') {
        "the pipeline ended $($Outcome.State), not Stopped"
    }
    if (@($Outcome.Output).Count) {
        'the command wrote output'
    }
}

# The upload's final progress, from the PowerShell progress records its command wrote: the one completed record, which
# repeats the latest record, with the expected acknowledged and total bytes. The command writes at most one record each
# 250 ms and then the latest state, so the completed record is the terminal state.
function Assert-FinalProgress {
    param($Outcome, [string] $RemotePath, $Expect, [long] $Length)
    $activity = "Uploading $RemotePath"
    $records = @($Outcome.Progress | Where-Object { $_.Activity -ceq $activity })
    $completed = @($records | Where-Object { $_.RecordType -eq [System.Management.Automation.ProgressRecordType]::Completed })
    $processing = @($records | Where-Object { $_.RecordType -eq [System.Management.Automation.ProgressRecordType]::Processing })
    if ($completed.Count -ne 1 -or -not $processing.Count) {
        throw "the upload wrote $($processing.Count) progress and $($completed.Count) completed records for '$activity', not at least one and one"
    }
    $final = $completed[0].StatusDescription
    if ($final -cne $processing[-1].StatusDescription) {
        throw "the completed progress record says '$final', not the latest record's '$($processing[-1].StatusDescription)'"
    }
    $match = [regex]::Match($final, '^(?<path>.+): (?<acknowledged>[0-9,]+) of (?<total>[0-9,]+) bytes(?:, [0-9]+ parts at once)?$')
    if (-not $match.Success) {
        throw "the final progress record says '$final', which does not give acknowledged and total bytes"
    }
    $values = [ordered]@{ acknowledged_bytes = [long] ($match.Groups['acknowledged'].Value -replace ',', ''); total_bytes = [long] ($match.Groups['total'].Value -replace ',', '') }
    foreach ($name in @($Expect.Keys)) {
        if ($Expect[$name] -cne 'recipe-length' -or -not $values.Contains($name)) {
            throw "the final progress expects $name '$($Expect[$name])', which the adapter does not check"
        }
        Assert-Same -Actual $values[$name] -Expected $Length -What "the final progress $name"
    }
    [ordered]@{ records = $records.Count; final = $final; acknowledgedBytes = $values.acknowledged_bytes; totalBytes = $values.total_bytes }
}

# An upload intent: Send-FilesFile from a new local file, a seekable stream or a forward-only stream giving at most
# max_read bytes a read, with the recipe's exact bytes, its known length for a stream, and any provided parameters.
function Invoke-UploadStep {
    param($Run, $Step)
    $fixture = $Run.Scenario.fixtures[$Step.fixture]
    $bytes = Get-RecipeContent -Recipe $fixture.recipe
    $remote = Get-FixtureRemotePath -Run $Run -Name $Step.fixture
    $parameters = @{ RemotePath = $remote; PassThru = $true; Confirm = $false; Connection = $Run.Connection; ErrorAction = 'Stop' }
    $given = Get-JsonValue $Step.details 'parameters'
    if ($given) {
        foreach ($name in @($given.Keys)) {
            $instant = [datetimeoffset]::MinValue
            if (-not [datetimeoffset]::TryParse([string] $given[$name], $script:Invariant, [System.Globalization.DateTimeStyles]::AssumeUniversal, [ref] $instant)) {
                throw "$name '$($given[$name])' is not a time"
            }
            $parameters[$script:IntentBindings['upload'].Parameters[$name]] = $instant.UtcDateTime
        }
    }
    $source = [string] $Step.details.source
    $stream = $null
    switch -CaseSensitive ($source) {
        'local-file' {
            $local = Get-LocalPath -Run $Run -Name "position-$($Step.number).bin"
            [FilesCom.ApiScenarios.Recipe]::WriteNewFile($local, $bytes)
            $parameters.LiteralPath = $local
        }
        'seekable-stream' {
            $stream = [System.IO.MemoryStream]::new($bytes, $false)
            $parameters.InputStream = $stream
            $parameters.Length = [long] $bytes.Length
        }
        'forward-only-stream' {
            $stream = [FilesCom.ApiScenarios.ForwardOnlyStream]::new($bytes, [int] $Step.details.max_read)
            $parameters.InputStream = $stream
            $parameters.Length = [long] $bytes.Length
        }
    }
    $cancel = [bool] (Get-JsonValue $Step.details 'cancel')
    $boundary = $null
    if ($cancel) {
        $Run.Transport.ArmFirstAcknowledgedPartBoundary([timespan]::FromSeconds($script:HoldSeconds))
    }
    try {
        $observed = Invoke-ObservedStep -Run $Run -Label (Get-StepLabel -Step $Step) -Action { Invoke-PublicCommand -Run $Run -Name 'Send-FilesFile' -Parameters $parameters -StopAtBoundary:$cancel }
    }
    finally {
        if ($cancel) {
            $boundary = $Run.Transport.DisarmBoundary()
        }
    }
    $outcome = $observed.Value
    $evidence = [ordered]@{ remotePath = $remote; source = $source; bytes = $bytes.Length; sha256 = $fixture.recipe.sha256; requests = $observed.Observations.Count }
    if ($stream -is [FilesCom.ApiScenarios.ForwardOnlyStream]) {
        $evidence.stream = [ordered]@{ maxRead = $stream.MaxRead; bytesRead = $stream.BytesRead; reads = $stream.Reads; maxReturned = $stream.MaxReturned; endReads = $stream.EndReads; unsupportedCalls = $stream.UnsupportedCalls }
    }
    elseif ($stream) {
        $evidence.stream = [ordered]@{ position = $stream.Position }
    }
    # A command that has not settled may still read its stream, which is then left to it.
    if ($stream -and -not $outcome.Unsettled) {
        $stream.Dispose()
    }
    if ($cancel) {
        $evidence.boundary = $boundary
        $evidence.stop = [ordered]@{ held = $outcome.Held; requested = $outcome.StopRequested; settled = $outcome.StopSettled; milliseconds = $outcome.StopMilliseconds; state = $outcome.State }
        $problems = @(Get-CancellationProblem -Outcome $outcome -Boundary $boundary)
        if ($problems.Count) {
            throw "the upload was not cancelled at its first acknowledged part: $($problems -join '; ')"
        }
        Assert-SentRequest -Observations $observed.Observations -AllowHeld
        $Run.StepJournal[[int] $Step.number] = [ordered]@{ kind = 'upload'; paths = @($remote); cancelled = $true; minParts = 1 }
        return $evidence
    }
    if ($outcome.Failure) {
        throw "Send-FilesFile failed: $(Format-Failure -Failure $outcome.Failure)"
    }
    $written = @($outcome.Output)
    if ($written.Count -ne 1 -or $written[0] -isnot [FilesCom.PowerShell.FilesUploadResult]) {
        throw "Send-FilesFile wrote $($written.Count) objects, not one upload result"
    }
    $result = $written[0]
    Assert-Same -Actual ([string] $result.Path).TrimStart('/') -Expected $remote -What 'the upload path'
    Assert-Same -Actual $result.Size -Expected $bytes.Length -What 'the upload size'
    if ($null -eq $result.File) {
        throw 'the upload result has no file'
    }
    if ($stream -is [FilesCom.ApiScenarios.ForwardOnlyStream]) {
        Assert-Same -Actual $evidence.stream.bytesRead -Expected $bytes.Length -What 'the bytes read from the forward-only stream'
        Assert-Same -Actual $evidence.stream.maxReturned -Expected ([math]::Min($stream.MaxRead, $bytes.Length)) -What 'the most bytes one read of the forward-only stream gave'
    }
    elseif ($stream) {
        Assert-Same -Actual $evidence.stream.position -Expected $bytes.Length -What 'the seekable stream position after the upload'
    }
    # The definition asks some uploads for at least min_parts parts and for their final progress; an upload it does not ask
    # stores at least one part, and its progress is not checked.
    $leastParts = Get-JsonValue $Step.expect 'min_parts'
    $minParts = if ($null -ne $leastParts) { [int] $leastParts } else { 1 }
    if (@($result.Parts).Count -lt $minParts) {
        throw "the upload stored $(@($result.Parts).Count) parts, fewer than $minParts"
    }
    $evidence.parts = @($result.Parts).Count
    $finalProgress = Get-JsonValue $Step.expect 'progress_final'
    if ($finalProgress) {
        $evidence.progress = Assert-FinalProgress -Outcome $outcome -RemotePath $remote -Expect $finalProgress -Length $bytes.Length
    }
    Assert-SentRequest -Observations $observed.Observations
    $Run.StepJournal[[int] $Step.number] = [ordered]@{ kind = 'upload'; paths = @($remote); cancelled = $false; minParts = $minParts }
    $evidence
}

# A download intent: Receive-FilesFile of the fixture's path, or of the very object a listing returned for it, to a new
# local file, which must then hold the recipe's exact bytes.
function Invoke-DownloadStep {
    param($Run, $Step)
    $fixture = $Run.Scenario.fixtures[$Step.fixture]
    $remote = Get-FixtureRemotePath -Run $Run -Name $Step.fixture
    $local = Get-LocalPath -Run $Run -Name "position-$($Step.number).bin"
    $parameters = @{ LiteralPath = $local; PassThru = $true; Confirm = $false; Connection = $Run.Connection; ErrorAction = 'Stop' }
    $source = [string] (Get-JsonValue $Step.details 'source')
    if ($source -ceq 'listed-entry') {
        if (-not $Run.Listed.ContainsKey($remote)) {
            throw "no listing before this download returned '$remote'"
        }
        $parameters.InputObject = $Run.Listed[$remote]
    }
    else {
        $parameters.RemotePath = $remote
    }
    $observed = Invoke-ObservedStep -Run $Run -Label (Get-StepLabel -Step $Step) -Action { Invoke-PublicCommand -Run $Run -Name 'Receive-FilesFile' -Parameters $parameters }
    if ($observed.Value.Failure) {
        throw "Receive-FilesFile failed: $(Format-Failure -Failure $observed.Value.Failure)"
    }
    if (@($observed.Value.Output).Count -ne 1) {
        throw "Receive-FilesFile wrote $(@($observed.Value.Output).Count) objects, not one download result"
    }
    if (-not (Test-Path -LiteralPath $local -PathType Leaf)) {
        throw "the download put no file at $local"
    }
    Assert-Same -Actual (Get-Item -LiteralPath $local).Length -Expected $fixture.recipe.length -What 'the downloaded length'
    Assert-Same -Actual ([FilesCom.ApiScenarios.Recipe]::FileSha256($local)) -Expected $fixture.recipe.sha256 -What 'the downloaded SHA-256'
    Assert-SentRequest -Observations $observed.Observations
    $Run.StepJournal[[int] $Step.number] = [ordered]@{ kind = 'download'; paths = @($remote) }
    [ordered]@{ remotePath = $remote; source = $(if ($source) { $source } else { 'path' }); bytes = $fixture.recipe.length; requests = $observed.Observations.Count }
}

# A copy or move intent: the action's own command from the source fixture to the step's fixture, then
# Wait-FilesFileAction on the action it returned, until its migration, if any, has completed.
function Invoke-ActionStep {
    param($Run, $Step)
    # The action's binding has a name of its own: the callback below runs inside Invoke-ObservedStep, whose -Action
    # parameter, the callback itself, would hide a variable here named $action.
    $actionBinding = $Step.action
    $from = Get-FixtureRemotePath -Run $Run -Name $Step.details.from
    $to = Get-FixtureRemotePath -Run $Run -Name $Step.fixture
    $parameters = @{ Connection = $Run.Connection; ErrorAction = 'Stop' }
    if ($actionBinding.mutation) {
        $parameters.Confirm = $false
    }
    $parameters[[string] $actionBinding.parameters.path] = $from
    $parameters[[string] $actionBinding.parameters.destination] = $to
    $observed = Invoke-ObservedStep -Run $Run -Label (Get-StepLabel -Step $Step) -Action {
        $started = Invoke-PublicCommand -Run $Run -Name $actionBinding.command -Parameters $parameters
        $waited = $null
        if (-not $started.Failure -and @($started.Output).Count -eq 1) {
            $timeout = [timespan]::FromSeconds([math]::Max(1, (Get-PhaseRemaining -Run $Run) - 5))
            $waited = Invoke-PublicCommand -Run $Run -Name 'Wait-FilesFileAction' -Parameters @{ InputObject = @($started.Output)[0]; Timeout = $timeout; Connection = $Run.Connection; ErrorAction = 'Stop' }
        }
        [pscustomobject]@{ Started = $started; Waited = $waited }
    }
    $started = $observed.Value.Started
    $waited = $observed.Value.Waited
    if ($started.Failure) {
        throw "$($actionBinding.command) failed: $(Format-Failure -Failure $started.Failure)"
    }
    if (@($started.Output).Count -ne 1) {
        throw "$($actionBinding.command) wrote $(@($started.Output).Count) objects, not one action"
    }
    if ($waited.Failure) {
        throw "Wait-FilesFileAction failed: $(Format-Failure -Failure $waited.Failure)"
    }
    $migrations = @($waited.Output)
    if ($migrations.Count -gt 1) {
        throw "Wait-FilesFileAction wrote $($migrations.Count) migrations"
    }
    $status = if ($migrations.Count) { [string] (Get-ObjectMember -Object $migrations[0] -Name 'Status') } else { $null }
    if ($migrations.Count -and $status -cne 'completed') {
        throw "the $($Step.intent)'s migration ended '$status', not completed"
    }
    Assert-SentRequest -Observations $observed.Observations
    # The wait checks a migration only when the action named one, and then writes it once completed.
    $Run.StepJournal[[int] $Step.number] = [ordered]@{ kind = 'action'; from = $from; migrations = $migrations.Count }
    [ordered]@{ from = $from; to = $to; migration = $status; requests = $observed.Observations.Count }
}

# A directory upload intent: Send-FilesDirectory of a new local tree, the step's tree with each file's recipe bytes and
# its empty folders, which the result must report exactly.
function Invoke-DirectoryUploadStep {
    param($Run, $Step)
    $remote = Get-FixtureRemotePath -Run $Run -Name $Step.fixture
    $folder = $Run.Scenario.fixtures[$Step.fixture]
    $prefix = if ($folder.path) { "$($folder.path)/" } else { '' }
    $relative = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::Ordinal)
    foreach ($name in @($Run.Scenario.fixtures.Keys)) {
        $path = [string] $Run.Scenario.fixtures[$name].path
        if ($path.StartsWith($prefix, [System.StringComparison]::Ordinal) -and $path.Length -gt $prefix.Length) {
            $relative[$path.Substring($prefix.Length)] = $name
        }
    }
    $local = Get-LocalPath -Run $Run -Name "position-$($Step.number)"
    [void] [System.IO.Directory]::CreateDirectory($local)
    foreach ($directory in @($Step.tree.directories)) {
        [void] [System.IO.Directory]::CreateDirectory((Join-Path -Path $local -ChildPath $directory))
    }
    $sizes = @{}
    foreach ($file in @($Step.tree.files)) {
        $recipe = $Run.Scenario.fixtures[$relative[$file]].recipe
        [FilesCom.ApiScenarios.Recipe]::WriteNewFile((Join-Path -Path $local -ChildPath $file), (Get-RecipeContent -Recipe $recipe))
        $sizes[$file] = [long] $recipe.length
    }
    $parameters = @{ LiteralPath = $local; RemotePath = $remote; PassThru = $true; Confirm = $false; Connection = $Run.Connection; ErrorAction = 'Stop' }
    $observed = Invoke-ObservedStep -Run $Run -Label (Get-StepLabel -Step $Step) -Action { Invoke-PublicCommand -Run $Run -Name 'Send-FilesDirectory' -Parameters $parameters }
    if ($observed.Value.Failure) {
        throw "Send-FilesDirectory failed: $(Format-Failure -Failure $observed.Value.Failure)"
    }
    $written = @($observed.Value.Output)
    if ($written.Count -ne 1 -or $written[0] -isnot [FilesCom.PowerShell.FilesDirectoryTransferResult]) {
        throw "Send-FilesDirectory wrote $($written.Count) objects, not one directory result"
    }
    $result = $written[0]
    $files = Get-OrdinalSorted -Values @($result.Files | ForEach-Object { $_.RelativePath })
    Assert-Same -Actual ($files -join ',') -Expected ((Get-OrdinalSorted -Values @($Step.tree.files)) -join ',') -What 'the files uploaded'
    foreach ($entry in @($result.Files)) {
        if ($entry.CompletedEarlier) {
            throw "$($entry.RelativePath) was taken as completed by an earlier run"
        }
        Assert-Same -Actual $entry.Size -Expected $sizes[$entry.RelativePath] -What "the size of $($entry.RelativePath)"
    }
    Assert-Same -Actual ((Get-OrdinalSorted -Values @($result.Directories)) -join ',') -Expected ((Get-OrdinalSorted -Values @($Step.tree.empty_directories)) -join ',') -What 'the empty folders uploaded'
    Assert-Same -Actual @($result.Excluded).Count -Expected 0 -What 'the entries excluded'
    $completed = Get-OrdinalSorted -Values @(foreach ($name in @($Step.expect.completed)) { ([string] $Run.Scenario.fixtures[$name].path).Substring($prefix.Length) })
    Assert-Same -Actual ($files -join ',') -Expected ($completed -join ',') -What 'the files completed'
    Assert-Same -Actual @($result.Files).Count -Expected $Step.expect.files -What 'the number of files'
    Assert-Same -Actual @($result.Directories).Count -Expected $Step.expect.empty_folders -What 'the number of empty folders'
    Assert-Same -Actual $result.Bytes -Expected $Step.expect.bytes -What 'the bytes uploaded'
    Assert-SentRequest -Observations $observed.Observations
    $Run.StepJournal[[int] $Step.number] = [ordered]@{
        kind = 'directory-upload'
        files = @(foreach ($file in @($Step.tree.files)) { "$remote/$file" })
        emptyFolders = @(foreach ($directory in @($Step.tree.empty_directories)) { "$remote/$directory" })
    }
    [ordered]@{ remotePath = $remote; files = $files; emptyFolders = @($result.Directories); bytes = $result.Bytes; requests = $observed.Observations.Count }
}

# What a directory download wrote under Local, read from the file system itself, against the step's tree, which
# cases.json derives from the definition's fixtures: exactly the tree's files, each with its fixture's recipe length and
# SHA-256; exactly its folders; exactly its empty folders, empty; and nothing that is neither a file nor a folder.
# Returns each file's length and their total.
function Assert-LocalTree {
    param($Run, $Step, [string] $Local)
    $folder = $Run.Scenario.fixtures[$Step.fixture]
    $prefix = if ($folder.path) { "$($folder.path)/" } else { '' }
    $recipes = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
    foreach ($name in @($Run.Scenario.fixtures.Keys)) {
        $fixture = $Run.Scenario.fixtures[$name]
        $path = [string] $fixture.path
        if ($fixture.type -ceq 'file' -and $path.StartsWith($prefix, [System.StringComparison]::Ordinal) -and $path.Length -gt $prefix.Length) {
            $recipes[$path.Substring($prefix.Length)] = $fixture.recipe
        }
    }
    $files = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::Ordinal)
    $folders = [System.Collections.Generic.List[string]]::new()
    $emptyFolders = [System.Collections.Generic.List[string]]::new()
    $other = [System.Collections.Generic.List[string]]::new()
    $separator = [string] [System.IO.Path]::DirectorySeparatorChar
    foreach ($item in @(Get-ChildItem -LiteralPath $Local -Recurse -Force)) {
        $relative = [System.IO.Path]::GetRelativePath($Local, $item.FullName).Replace($separator, '/')
        if ($item.LinkTarget) {
            $other.Add($relative)
        }
        elseif ($item -is [System.IO.DirectoryInfo]) {
            $folders.Add($relative)
            if (-not @(Get-ChildItem -LiteralPath $item.FullName -Force).Count) {
                $emptyFolders.Add($relative)
            }
        }
        elseif ($item -is [System.IO.FileInfo]) {
            $files[$relative] = $item.FullName
        }
        else {
            $other.Add($relative)
        }
    }
    if ($other.Count) {
        throw "the download wrote $($other -join ', '), which is neither a file nor a folder"
    }
    Assert-SameList -Actual @($files.Keys) -Expected @($Step.tree.files) -What 'the files the download wrote'
    Assert-SameList -Actual $folders.ToArray() -Expected @($Step.tree.directories) -What 'the folders the download wrote'
    Assert-SameList -Actual $emptyFolders.ToArray() -Expected @($Step.tree.empty_directories) -What 'the empty folders the download wrote'
    $sizes = [System.Collections.Generic.Dictionary[string, long]]::new([System.StringComparer]::Ordinal)
    $bytes = [long] 0
    foreach ($file in @($Step.tree.files)) {
        if (-not $recipes.ContainsKey($file)) {
            throw "the tree's file $file is no file fixture of the scenario"
        }
        $recipe = $recipes[$file]
        Assert-Same -Actual (Get-Item -LiteralPath $files[$file]).Length -Expected $recipe.length -What "the length of the downloaded $file"
        Assert-Same -Actual ([FilesCom.ApiScenarios.Recipe]::FileSha256($files[$file])) -Expected $recipe.sha256 -What "the SHA-256 of the downloaded $file"
        $sizes[$file] = [long] $recipe.length
        $bytes += [long] $recipe.length
    }
    [pscustomobject]@{ Sizes = $sizes; Bytes = $bytes }
}

# A directory download intent: Receive-FilesDirectory of the fixture's folder to a new local folder, which must then
# hold exactly the step's tree, as Assert-LocalTree reads it from the file system; and the command's result must report
# that same tree, its counts never standing in for it.
function Invoke-DirectoryDownloadStep {
    param($Run, $Step)
    $remote = Get-FixtureRemotePath -Run $Run -Name $Step.fixture
    $folder = $Run.Scenario.fixtures[$Step.fixture]
    $prefix = if ($folder.path) { "$($folder.path)/" } else { '' }
    $local = Get-LocalPath -Run $Run -Name "position-$($Step.number)"
    $parameters = @{ RemotePath = $remote; LiteralPath = $local; PassThru = $true; Confirm = $false; Connection = $Run.Connection; ErrorAction = 'Stop' }
    $observed = Invoke-ObservedStep -Run $Run -Label (Get-StepLabel -Step $Step) -Action { Invoke-PublicCommand -Run $Run -Name 'Receive-FilesDirectory' -Parameters $parameters }
    if ($observed.Value.Failure) {
        throw "Receive-FilesDirectory failed: $(Format-Failure -Failure $observed.Value.Failure)"
    }
    $written = @($observed.Value.Output)
    if ($written.Count -ne 1 -or $written[0] -isnot [FilesCom.PowerShell.FilesDirectoryTransferResult]) {
        throw "Receive-FilesDirectory wrote $($written.Count) objects, not one directory result"
    }
    $result = $written[0]
    $tree = Assert-LocalTree -Run $Run -Step $Step -Local $local
    Assert-Same -Actual $tree.Bytes -Expected $Step.expect.bytes -What "the bytes of the tree's files"
    foreach ($name in @($Step.expect.bytes_match)) {
        $path = [string] $Run.Scenario.fixtures[$name].path
        if (-not $path.StartsWith($prefix, [System.StringComparison]::Ordinal) -or -not $tree.Sizes.ContainsKey($path.Substring($prefix.Length))) {
            throw "the download's tree has no file for $name"
        }
    }
    Assert-SameList -Actual @($result.Files | ForEach-Object { [string] $_.RelativePath }) -Expected @($Step.tree.files) -What 'the files the command reported'
    foreach ($entry in @($result.Files)) {
        if ($entry.CompletedEarlier) {
            throw "$($entry.RelativePath) was taken as completed by an earlier run"
        }
        Assert-Same -Actual $entry.Size -Expected $tree.Sizes[[string] $entry.RelativePath] -What "the size the command reported for $($entry.RelativePath)"
    }
    Assert-SameList -Actual @($result.Directories) -Expected @($Step.tree.empty_directories) -What 'the empty folders the command reported'
    Assert-Same -Actual @($result.Excluded).Count -Expected 0 -What 'the entries excluded'
    Assert-Same -Actual $result.Bytes -Expected $tree.Bytes -What 'the bytes the command reported'
    Assert-Same -Actual @($result.Files).Count -Expected $Step.expect.files -What 'the number of files'
    Assert-Same -Actual @($result.Directories).Count -Expected $Step.expect.empty_folders -What 'the number of empty folders'
    Assert-SentRequest -Observations $observed.Observations
    $Run.StepJournal[[int] $Step.number] = [ordered]@{
        kind = 'directory-download'
        folders = @($remote) + @(foreach ($directory in @($Step.tree.directories)) { "$remote/$directory" })
        files = @(foreach ($file in @($Step.tree.files)) { "$remote/$file" })
    }
    [ordered]@{ remotePath = $remote; local = $local; files = @($result.Files).Count; emptyFolders = @($result.Directories); bytes = $result.Bytes; requests = $observed.Observations.Count }
}

function Invoke-ScenarioStep {
    param($Run, $Step)
    if ($Step.kind -ceq 'operation') {
        return Invoke-OperationStep -Run $Run -Step $Step
    }
    switch -CaseSensitive ([string] $Step.intent) {
        'upload' { return Invoke-UploadStep -Run $Run -Step $Step }
        'download' { return Invoke-DownloadStep -Run $Run -Step $Step }
        'copy' { return Invoke-ActionStep -Run $Run -Step $Step }
        'move' { return Invoke-ActionStep -Run $Run -Step $Step }
        'upload-directory' { return Invoke-DirectoryUploadStep -Run $Run -Step $Step }
        'download-directory' { return Invoke-DirectoryDownloadStep -Run $Run -Step $Step }
    }
    throw "the intent $($Step.intent) has no PowerShell binding"
}

# Creates Path as a folder with the scenario's own creation command, and owns it only when the transport witnessed
# exactly one request to that route and path answered with the creation's expected status, and the command returned
# exactly Path as a folder. Anything else, including an earlier lookup, another success or a refusal, owns nothing.
function Invoke-FolderAcquisition {
    param($Run, [string] $Path, [string] $Label)
    $create = $Run.Protocol.Create
    $call = Get-StepCall -Run $Run -Step $create -Override @{ (Get-IdentityWireName -Step $create) = $Path }
    $observed = Invoke-ObservedStep -Run $Run -Label $Label -Action { Invoke-PublicCommand -Run $Run -Name $call.Name -Parameters $call.Parameters }
    $requests = @($observed.Observations)
    $evidence = [ordered]@{ path = $Path; owned = $false; requests = @($requests | ForEach-Object { ConvertTo-ObservationRecord -Observation $_ }); status = $null; errorType = $null; returnedPath = $null; returnedType = $null; failure = $null }
    if ($requests.Count -eq 1) {
        $evidence.status = $requests[0].Status
    }
    if ($observed.Value.Failure) {
        $evidence.failure = Format-Failure -Failure $observed.Value.Failure
        $exception = Get-FilesException -Failure $observed.Value.Failure
        if ($exception) {
            $evidence.errorType = $exception.ErrorType
        }
        return $evidence
    }
    $written = @($observed.Value.Output)
    if ($written.Count -eq 1) {
        $evidence.returnedPath = [string] $written[0].Path
        $evidence.returnedType = [string] $written[0].Type
    }
    $evidence.owned = $requests.Count -eq 1 -and $requests[0].SameOrigin -and $requests[0].Kind -ceq 'api' -and $requests[0].Method -ceq $create.operation.method -and
        (Test-RequestPath -Observed $requests[0].Path -Prefix $Run.ApiPrefix -RoutePath $create.operation.path -PathValue $Path) -and
        $requests[0].Outcome -ceq 'responded' -and $requests[0].Status -eq $create.expect.status -and
        $written.Count -eq 1 -and $evidence.returnedPath -ceq $Path -and $evidence.returnedType -ceq $Run.Protocol.Type
    $evidence
}

# Looks Path up with the scenario's own absence step until the expected error answers, at most its attempts, its
# interval apart, within the phase's budget.
function Wait-RemoteAbsence {
    param($Run, [string] $Path, [string] $Label)
    $step = $Run.Protocol.Absent
    $attempts = [int] $step.expect.attempts
    $interval = [double] $step.expect.interval_seconds
    $statuses = [System.Collections.Generic.List[object]]::new()
    for ($attempt = 1; $attempt -le $attempts; $attempt++) {
        # The next lookup comes its interval after this one, only when that is still within the phase.
        if ((Get-PhaseRemaining -Run $Run) -le $(if ($attempt -gt 1) { $interval } else { 0 })) {
            return [ordered]@{ absent = $false; attempts = $attempt - 1; statuses = $statuses.ToArray(); failure = "the $($Run.Phase) budget ran out" }
        }
        if ($attempt -gt 1) {
            Start-Sleep -Seconds $interval
        }
        $call = Get-StepCall -Run $Run -Step $step -Override @{ (Get-IdentityWireName -Step $step) = $Path }
        $observed = Invoke-ObservedStep -Run $Run -Label $Label -Action { Invoke-PublicCommand -Run $Run -Name $call.Name -Parameters $call.Parameters }
        $statuses.Add(@($observed.Observations | ForEach-Object { $_.Status }))
        $exception = Get-FilesException -Failure $observed.Value.Failure
        if ($exception -and $exception.HttpStatus -eq $step.expect.status -and $exception.ErrorType -ceq $step.expect.error) {
            return [ordered]@{ absent = $true; attempts = $attempt; statuses = $statuses.ToArray(); failure = $null }
        }
        if ($observed.Value.Failure) {
            return [ordered]@{ absent = $false; attempts = $attempt; statuses = $statuses.ToArray(); failure = Format-Failure -Failure $observed.Value.Failure }
        }
    }
    [ordered]@{ absent = $false; attempts = $attempts; statuses = $statuses.ToArray(); failure = "still present after $attempts lookups" }
}

# Deletes a positively owned Path with the scenario's own deletion command, recursively only when asked, and confirms
# it absent.
function Invoke-OwnedRemoval {
    param($Run, [string] $Path, [bool] $Recursive, [string] $Label)
    $delete = $Run.Protocol.Delete
    $call = Get-StepCall -Run $Run -Step $delete -Override @{ (Get-IdentityWireName -Step $delete) = $Path; recursive = $Recursive }
    $observed = Invoke-ObservedStep -Run $Run -Label $Label -Action { Invoke-PublicCommand -Run $Run -Name $call.Name -Parameters $call.Parameters }
    $evidence = [ordered]@{ path = $Path; recursive = $Recursive; requests = @($observed.Observations | ForEach-Object { ConvertTo-ObservationRecord -Observation $_ }); failure = $null; absence = $null; absent = $false }
    if ($observed.Value.Failure) {
        $evidence.failure = Format-Failure -Failure $observed.Value.Failure
        return $evidence
    }
    $evidence.absence = Wait-RemoteAbsence -Run $Run -Path $Path -Label "$Label absence"
    $evidence.absent = $evidence.absence.absent
    $evidence
}

# What a run positively owns and has not removed, deepest first: the scenario's folder, recursively, after a step
# stopped the scenario before its own deletion; then the run folder, without recursion.
function Get-CleanupTarget {
    param($Ownership)
    if ($Ownership.namespace.owned -and -not $Ownership.namespace.removed) {
        [ordered]@{ path = $Ownership.namespace.path; recursive = $true; role = 'namespace' }
    }
    if ($Ownership.run.owned -and -not $Ownership.run.removed) {
        [ordered]@{ path = $Ownership.run.path; recursive = $false; role = 'run' }
    }
}

# Removes what the run owns and has not removed, within the cleanup's deadline. While a command has not settled, it may
# still change what the run owns, and nothing more runs in its runspace: what the run owns then stays, unresolved, for
# the launcher to stop that command's process. Once the deadline has passed, no removal starts: what the run still owns
# stays unresolved, and the cleanup's failure, unless it already has one, is that deadline.
function Invoke-ScenarioCleanup {
    param($Run)
    Start-CleanupPhase -Run $Run
    $targets = @(Get-CleanupTarget -Ownership $Run.Ownership)
    if ($Run.Unsettled) {
        foreach ($target in $targets) {
            $Run.Unresolved.Add($target.path)
        }
        if ($targets.Count -and -not $Run.CleanupFailure) {
            $Run.CleanupFailure = "cleanup did not run: $($Run.Unsettled.command), stopped at $($Run.Unsettled.step), has not settled"
        }
        return
    }
    foreach ($target in $targets) {
        if ($Run.Unresolved.Count) {
            # What is inside it is unresolved, so a non-recursive removal could only fail; it stays unresolved too.
            $Run.Unresolved.Add($target.path)
            continue
        }
        try {
            $removal = Invoke-OwnedRemoval -Run $Run -Path $target.path -Recursive $target.recursive -Label "cleanup $($target.role)"
        }
        catch [System.TimeoutException] {
            # Refused for the deadline before it was sent: this target stays unresolved, and so does every one after it.
            if (-not $Run.CleanupFailure) {
                $Run.CleanupFailure = "cleanup stopped: $($_.Exception.Message)"
            }
            $Run.Unresolved.Add($target.path)
            continue
        }
        $Run.Cleanup.Add($removal)
        if ($removal.absent) {
            $Run.Ownership[$target.role].removed = $true
        }
        else {
            $Run.Unresolved.Add($target.path)
        }
    }
}

# The simulator's journal, read from its control endpoint, which the journal does not record, within what is left of
# the current phase.
function Read-SimulatorJournal {
    param($Run)
    $response = Invoke-WebRequest -Uri "$($Run.Inputs.ServerUrl)$script:ControlPrefix/journal" -Method Get -TimeoutSec (Get-ControlTimeoutSeconds -Run $Run) -NoProxy -SkipHttpErrorCheck
    if ($response.StatusCode -ne 200) {
        throw "the journal answered $($response.StatusCode)"
    }
    $response.Content | ConvertFrom-Json -AsHashtable
}

# Joins every request the transport saw to the simulator's journal, one to one: each journal entry of this epoch, in
# sequence order, to the earliest unjoined request with its method, decoded path and status, which must carry its
# User-Agent and the credential its kind allows, under no fault. The journal must be complete and numbered without a
# gap; a request a boundary held unsent has no entry; any other request without an entry, or unanswered, and any
# entry without a request, except an ExcludedEntries one, is a problem.
function Compare-JournalAccounting {
    param($Run, $Journal, [object[]] $Observations, [long[]] $ExcludedEntries = @())
    $problems = [System.Collections.Generic.List[string]]::new()
    if ($Journal.complete -ne $true -or [long] $Journal.dropped -ne 0) {
        $problems.Add("the journal is not complete (complete $($Journal.complete), dropped $($Journal.dropped))")
    }
    if ([long] $Journal.epoch -ne $Run.Epoch) {
        $problems.Add("the journal is of epoch $($Journal.epoch), not the reset's $($Run.Epoch)")
    }
    Assert-Same -Actual $Journal.instance -Expected $Run.Inputs.Instance -What 'the journal instance'
    if ($Run.Transport -and $Run.Transport.Dropped) {
        $problems.Add("the transport kept only the first $($Run.Transport.Limit) requests")
    }
    $agent = "FilesCom-PowerShell/$($Run.Identity.module.version)"
    $entries = @($Journal.entries | Sort-Object -Property { [long] $_.seq })
    for ($index = 0; $index -lt $entries.Count; $index++) {
        if ([long] $entries[$index].seq -ne $index + 1) {
            $problems.Add("the journal's entry $($index + 1) is numbered $($entries[$index].seq)")
            break
        }
    }
    $pending = [System.Collections.Generic.List[object]]::new()
    $held = 0
    foreach ($observation in $Observations) {
        if ($observation.Outcome -like 'held-*') {
            $held++
        }
        elseif ($observation.Outcome -cne 'responded') {
            $problems.Add("request $(Format-Observation -Observation $observation) was not answered")
        }
        else {
            $pending.Add($observation)
        }
    }
    $pairs = [System.Collections.Generic.List[object]]::new()
    $unmatched = [System.Collections.Generic.List[long]]::new()
    foreach ($entry in $entries) {
        $sequence = [long] $entry.seq
        if ($ExcludedEntries -contains $sequence) {
            continue
        }
        if ([long] $entry.epoch -ne $Run.Epoch) {
            $problems.Add("journal entry $sequence is of epoch $($entry.epoch)")
        }
        $path = [System.Uri]::UnescapeDataString([string] $entry.path)
        $index = -1
        for ($candidate = 0; $candidate -lt $pending.Count; $candidate++) {
            $observation = $pending[$candidate]
            if ($observation.Method -ceq $entry.method -and $observation.Path -ceq $path -and $observation.Status -eq $entry.status) {
                $index = $candidate
                break
            }
        }
        if ($index -lt 0) {
            $unmatched.Add($sequence)
            $shown = if ($path.StartsWith("$($Run.ApiPrefix)/", [System.StringComparison]::Ordinal)) { $path } else { 'a storage address' }
            $problems.Add("journal entry $sequence ($($entry.method) $shown, $($entry.status)) has no request the transport saw")
            continue
        }
        $observation = $pending[$index]
        $pending.RemoveAt($index)
        $pairs.Add([pscustomobject]@{ Entry = $entry; Observation = $observation })
        if ([string] $entry.user_agent -cne $observation.UserAgent -or -not $observation.UserAgent.StartsWith($agent, [System.StringComparison]::Ordinal)) {
            $problems.Add("journal entry $sequence has User-Agent '$($entry.user_agent)', the transport '$($observation.UserAgent)', not the module's $agent")
        }
        $credentials = Get-JsonValue $entry 'credentials'
        $apiKey = Get-JsonValue $credentials 'api_key'
        $session = Get-JsonValue $credentials 'session'
        if ($observation.Kind -ceq 'api') {
            if ($observation.Credential -cne 'api-key' -or $null -eq $apiKey -or [string] $apiKey -ceq 'unnumbered' -or $null -ne $session) {
                $problems.Add("journal entry $sequence carried $(ConvertTo-Json -InputObject $credentials -Compress), the transport '$($observation.Credential)', not the run's API key alone")
            }
        }
        elseif ($observation.Credential -cne 'none' -or $null -ne $apiKey -or $null -ne $session) {
            $problems.Add("journal entry $sequence, a storage request, carried a credential")
        }
        if ($null -ne $entry.fault_id) {
            $problems.Add("journal entry $sequence was answered under the fault $($entry.fault_id)")
        }
        if (-not $entry.operation) {
            $problems.Add("journal entry $sequence was not a simulated operation")
        }
    }
    foreach ($observation in $pending) {
        $problems.Add("the transport saw request $(Format-Observation -Observation $observation), which the journal does not have")
    }
    [pscustomobject]@{ Problems = $problems.ToArray(); Pairs = $pairs.ToArray(); Entries = $entries.Count; Matched = $pairs.Count; Held = $held; Unmatched = $unmatched.ToArray(); Excluded = @($ExcludedEntries) }
}

function Get-SimulatorOperationId {
    param($Run, [string] $OperationId)
    $served = $Run.BySwagger[$OperationId]
    if ($served) { [string] $served.id } else { $null }
}

# Each passed step's journal entries: an operation step's are all its own schema operation; an intent's are exactly
# what that intent makes, as Get-IntentJournalProblem checks them against what the step recorded.
function Get-StepJournalProblem {
    param($Run, [object[]] $Pairs)
    $byStep = @{}
    foreach ($pair in $Pairs) {
        $label = [string] $pair.Observation.Step
        if (-not $byStep.ContainsKey($label)) {
            $byStep[$label] = [System.Collections.Generic.List[object]]::new()
        }
        $byStep[$label].Add($pair.Entry)
    }
    foreach ($record in @($Run.Positions | Where-Object { $_.result -ceq 'passed' })) {
        $step = @($Run.Scenario.steps | Where-Object { $_.number -eq $record.position })[0]
        $label = Get-StepLabel -Step $step
        $entries = if ($byStep.ContainsKey($label)) { $byStep[$label].ToArray() } else { @() }
        if ($step.kind -ceq 'operation') {
            $expected = Get-SimulatorOperationId -Run $Run -OperationId $step.operation.operation_id
            foreach ($entry in $entries) {
                if ([string] $entry.operation -cne $expected) {
                    "position $($step.number): journal entry $($entry.seq) is $($entry.operation), not $expected ($($step.operation.operation_id))"
                }
            }
            continue
        }
        $check = $Run.StepJournal[[int] $step.number]
        if (-not $check) {
            "position $($step.number): the $($step.intent) recorded nothing to check its journal entries against"
            continue
        }
        Get-IntentJournalProblem -Run $Run -Step $step -Entries $entries -Check $check
    }
}

# Why a passed intent step's journal entries are not exactly what that intent makes, or nothing. Each entry must be an
# operation the definition names for the intent, in the role its binding gives it (IntentBindings Routes), on the step's
# own fixture or, for a directory, on a path of the step's tree; or a storage transfer of what those named: a part of an
# upload the step began, or bytes of a version its lookups found. Anything else is a problem, as is an upload, part or
# version the simulator's journal does not name. Check is what the step recorded in StepJournal.
function Get-IntentJournalProblem {
    param($Run, $Step, [object[]] $Entries, $Check)
    $where = "position $($Step.number)"
    $binding = $script:IntentBindings[[string] $Step.intent]
    $roles = @{}
    $templates = @{}
    $byRole = @{}
    foreach ($role in @($binding.Routes.Keys)) {
        $byRole[$role] = [System.Collections.Generic.List[object]]::new()
        $operation = @($Step.operations | Where-Object { $_.route -ceq $binding.Routes[$role] })[0]
        if (-not $operation) {
            "$($where): the $($Step.intent) names no $($binding.Routes[$role])"
            continue
        }
        $roles[[string] (Get-SimulatorOperationId -Run $Run -OperationId $operation.operation_id)] = $role
        $templates[$role] = [string] $operation.path
    }
    $parts = [System.Collections.Generic.List[object]]::new()
    $reads = [System.Collections.Generic.List[object]]::new()
    foreach ($entry in $Entries) {
        $id = [string] $entry.operation
        if ($id -ceq $Run.Transfers.UploadPart) {
            $parts.Add($entry)
            continue
        }
        if ($id -cin $Run.Transfers.Reads) {
            $reads.Add($entry)
            continue
        }
        if (-not $id -or -not $roles.ContainsKey($id)) {
            "$($where): journal entry $($entry.seq) is $id, which the $($Step.intent) does not make"
            continue
        }
        $role = $roles[$id]
        $path = Get-RoutePathValue -Observed ([System.Uri]::UnescapeDataString([string] $entry.path)) -Prefix $Run.ApiPrefix -RoutePath $templates[$role]
        $byRole[$role].Add([pscustomobject]@{ Entry = $entry; Path = $path })
    }
    $of = @{}
    foreach ($role in @($byRole.Keys)) {
        $of[$role] = $byRole[$role].ToArray()
    }
    switch -CaseSensitive ([string] $Check.kind) {
        'upload' {
            Get-UploadJournalProblem -Where $where -Paths @($Check.paths) -Begins $of['begin'] -Finishes $of['finish'] -Parts $parts.ToArray() -MinParts ([int] $Check.minParts) -Cancelled ([bool] $Check.cancelled)
        }
        'directory-upload' {
            Get-RequestPathProblem -Where $where -What 'created the folder' -Requests $of['folder'] -Expected @($Check.emptyFolders) -Each
            Get-UploadJournalProblem -Where $where -Paths @($Check.files) -Begins $of['begin'] -Finishes $of['finish'] -Parts $parts.ToArray() -MinParts 1 -Cancelled $false
        }
        'download' {
            Get-DownloadJournalProblem -Where $where -Paths @($Check.paths) -Lookups $of['lookup'] -Reads $reads.ToArray()
        }
        'directory-download' {
            # The download lists its folder and every folder inside it, a folder at a time.
            Get-RequestPathProblem -Where $where -What 'listed' -Requests $of['list'] -Expected @($Check.folders) -Each
            Get-DownloadJournalProblem -Where $where -Paths @($Check.files) -Lookups $of['lookup'] -Reads $reads.ToArray()
        }
        'action' {
            $actions = @($of['action'])
            Get-RequestPathProblem -Where $where -What $(if ($Step.intent -ceq 'copy') { 'copied' } else { 'moved' }) -Requests $actions -Expected @($Check.from) -Each
            if ($actions.Count -ne 1) {
                "$($where): the journal shows $($actions.Count) $($Step.intent) actions, not one"
            }
            # The wait checks a migration only when the action named one, and then that one alone.
            $migrations = @($of['migration'])
            $ids = @($migrations | ForEach-Object { [string] $_.Entry.id } | Sort-Object -Unique)
            if ([int] $Check.migrations -eq 0 -and $migrations.Count) {
                "$($where): the wait checked migration $($ids -join ', '), but reported none"
            }
            elseif ([int] $Check.migrations -gt 0 -and ($migrations.Count -eq 0 -or $ids.Count -ne 1)) {
                "$($where): the wait checked migrations [$($ids -join ', ')], not the one migration it reported"
            }
        }
        default {
            "$($where): the $($Step.intent) recorded a journal check of kind '$($Check.kind)', which the adapter does not know"
        }
    }
    if ([string] $Check.kind -cnotin 'upload', 'directory-upload' -and $parts.Count) {
        "$($where): the $($Step.intent) stored upload parts, in journal entries $(@($parts | ForEach-Object { $_.seq }) -join ', ')"
    }
    if ([string] $Check.kind -cnotin 'download', 'directory-download' -and $reads.Count) {
        "$($where): the $($Step.intent) read stored bytes, in journal entries $(@($reads | ForEach-Object { $_.seq }) -join ', ')"
    }
}

# Problems with requests of one role (objects with the journal Entry and its route's {path} value, Path) whose paths
# must be among Expected, and, with -Each, include every one of them.
function Get-RequestPathProblem {
    param([string] $Where, [string] $What, [object[]] $Requests, [string[]] $Expected, [switch] $Each)
    foreach ($request in @($Requests)) {
        if ($null -eq $request.Path -or $request.Path -cnotin $Expected) {
            "$($Where): journal entry $($request.Entry.seq) $What '$($request.Path)', which is not $(if (@($Expected).Count -eq 1) { "'$(@($Expected)[0])'" } else { "a path of the step's tree" })"
        }
    }
    if ($Each) {
        $seen = @(@($Requests) | ForEach-Object { $_.Path })
        foreach ($path in @($Expected)) {
            if ($path -cnotin $seen) {
                "$($Where): no journal entry $What '$path'"
            }
        }
    }
}

# Problems with the uploads of Paths among one step's journal entries. Each path is begun as one upload, which every
# begin of it names; unless cancelled, it is finished once, by a finish naming that same upload; every part stored names
# its upload and its part, and is a part of one of the step's uploads; and each upload stores at least MinParts parts,
# or, cancelled at its first acknowledged part, part 1 alone, and is not finished.
function Get-UploadJournalProblem {
    param([string] $Where, [string[]] $Paths, [object[]] $Begins, [object[]] $Finishes, [object[]] $Parts, [int] $MinParts, [bool] $Cancelled)
    Get-RequestPathProblem -Where $Where -What 'began an upload of' -Requests $Begins -Expected $Paths -Each
    Get-RequestPathProblem -Where $Where -What 'finished an upload of' -Requests $Finishes -Expected $Paths
    $uploads = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::Ordinal)
    foreach ($begin in @($Begins)) {
        $upload = Get-JsonValue $begin.Entry 'upload'
        if ($null -eq $upload) {
            "$($Where): journal entry $($begin.Entry.seq) began an upload without naming it"
        }
        elseif ($null -eq $begin.Path -or $begin.Path -cnotin $Paths) {
            continue
        }
        elseif ($uploads.ContainsKey($begin.Path) -and $uploads[$begin.Path] -cne [string] $upload) {
            "$($Where): journal entry $($begin.Entry.seq) began upload $upload of '$($begin.Path)', which upload $($uploads[$begin.Path]) had begun"
        }
        else {
            $uploads[$begin.Path] = [string] $upload
        }
    }
    $owners = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::Ordinal)
    foreach ($path in @($uploads.Keys)) {
        if ($owners.ContainsKey($uploads[$path])) {
            "$($Where): upload $($uploads[$path]) was begun for both '$($owners[$uploads[$path]])' and '$path'"
        }
        else {
            $owners[$uploads[$path]] = $path
        }
    }
    $stored = @{}
    foreach ($part in @($Parts)) {
        $upload = Get-JsonValue $part 'upload'
        $number = Get-JsonValue $part 'part'
        if ($null -eq $upload -or $null -eq $number) {
            "$($Where): journal entry $($part.seq) stored a part without naming both its upload and its part"
            continue
        }
        if (-not $owners.ContainsKey([string] $upload)) {
            "$($Where): journal entry $($part.seq) stored part $number of upload $upload, which the step did not begin"
            continue
        }
        if ([int] $part.status -ge 200 -and [int] $part.status -le 299) {
            if (-not $stored.ContainsKey([string] $upload)) {
                $stored[[string] $upload] = [System.Collections.Generic.List[int]]::new()
            }
            $stored[[string] $upload].Add([int] $number)
        }
    }
    foreach ($path in @($Paths)) {
        if (-not $uploads.ContainsKey($path)) {
            continue
        }
        $upload = $uploads[$path]
        $numbers = @(if ($stored.ContainsKey($upload)) { $stored[$upload].ToArray() })
        $finished = @(@($Finishes) | Where-Object { $_.Path -ceq $path })
        if ($Cancelled) {
            if ($numbers.Count -ne 1 -or $numbers[0] -ne 1) {
                "$($Where): the cancelled upload of '$path' stored parts [$($numbers -join ', ')], not part 1 alone"
            }
            if ($finished.Count) {
                "$($Where): the cancelled upload of '$path' was finished"
            }
            continue
        }
        $distinct = @($numbers | Sort-Object -Unique)
        if ($distinct.Count -lt $MinParts) {
            "$($Where): the upload of '$path' stored $($distinct.Count) parts, fewer than $MinParts"
        }
        if ($finished.Count -ne 1) {
            "$($Where): the upload of '$path' was finished $($finished.Count) times, not once"
        }
        foreach ($finish in $finished) {
            $named = Get-JsonValue $finish.Entry 'upload'
            if ($null -eq $named -or [string] $named -cne $upload) {
                "$($Where): journal entry $($finish.Entry.seq) finished upload '$named' of '$path', not upload $upload, which its begin opened"
            }
        }
    }
}

# Problems with the downloads of Paths among one step's journal entries: lookups only of those paths, each naming the
# version it found, and storage reads only of versions those lookups found.
function Get-DownloadJournalProblem {
    param([string] $Where, [string[]] $Paths, [object[]] $Lookups, [object[]] $Reads)
    Get-RequestPathProblem -Where $Where -What 'looked up' -Requests $Lookups -Expected $Paths
    $versions = [System.Collections.Generic.List[string]]::new()
    foreach ($lookup in @($Lookups)) {
        $version = Get-JsonValue $lookup.Entry 'version'
        if ($null -eq $version) {
            "$($Where): journal entry $($lookup.Entry.seq) looked up '$($lookup.Path)' without naming the version it found"
        }
        elseif ($lookup.Path -cin $Paths) {
            $versions.Add([string] $version)
        }
    }
    foreach ($read in @($Reads)) {
        $version = Get-JsonValue $read 'version'
        if ($null -eq $version -or [string] $version -cnotin $versions) {
            "$($Where): journal entry $($read.seq) read version '$version', which no lookup of the step found"
        }
    }
}

# A request, as the receipt records it: a storage address only by its SHA-256 and part number.
function ConvertTo-ObservationRecord {
    param($Observation)
    [ordered]@{
        sequence = $Observation.Sequence
        step = $Observation.Step
        method = $Observation.Method
        kind = $Observation.Kind
        path = $(if ($Observation.Kind -ceq 'api') { $Observation.Path } else { 'storage' })
        pathSha256 = Get-TextSha256 -Text $Observation.Path
        sameOrigin = $Observation.SameOrigin
        queryNames = @($Observation.QueryNames)
        credential = $Observation.Credential
        userAgent = $Observation.UserAgent
        requestBytes = $Observation.RequestBytes
        status = $Observation.Status
        outcome = $Observation.Outcome
        error = $Observation.Error
        storagePart = $Observation.StoragePart
        startedMs = [math]::Round($Observation.StartedMilliseconds, 1)
        endedMs = [math]::Round($Observation.EndedMilliseconds, 1)
    }
}

function Invoke-JournalAccounting {
    param($Run)
    $journal = Read-SimulatorJournal -Run $Run
    $observations = @(if ($Run.Transport) { $Run.Transport.All() })
    $accounting = Compare-JournalAccounting -Run $Run -Journal $journal -Observations $observations -ExcludedEntries $Run.ExcludedEntries.ToArray()
    $stepProblems = @(Get-StepJournalProblem -Run $Run -Pairs $accounting.Pairs)
    $Run.Journal = [ordered]@{
        epoch = $journal.epoch
        complete = $journal.complete
        dropped = $journal.dropped
        entries = $accounting.Entries
        matched = $accounting.Matched
        heldUnsent = $accounting.Held
        excluded = @($accounting.Excluded)
        unmatched = @($accounting.Unmatched)
        uploadParts = $journal.upload_parts
        problems = @($accounting.Problems) + $stepProblems
    }
    $accounting
}

# Makes the run's own local work folder, which is the run's to remove only once it has made it.
function Initialize-LocalWork {
    param($Run)
    $work = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "files-api-scenarios-$($Run.Inputs.RunId)"
    if (Test-Path -LiteralPath $work) {
        throw "$work already exists"
    }
    [void] [System.IO.Directory]::CreateDirectory($work)
    $Run.Work = $work
}

# Removes the run's local work folder, unless a command that has not settled may still be using what is in it: the
# folder is then left, and named in the run's Unsettled record.
function Clear-LocalWork {
    param($Run)
    if ($Run.Unsettled) {
        $Run.Unsettled.localWork = $Run.Work
        return
    }
    if ($Run.Work -and (Test-Path -LiteralPath $Run.Work)) {
        Remove-Item -LiteralPath $Run.Work -Recurse -Force
    }
}

# Closes the run's command runspace, unless a command that has not settled still runs in it, and removes its local work
# as Clear-LocalWork does. A failure is recorded as the cleanup's when the cleanup has none.
function Close-RunWork {
    param($Run)
    try {
        if ($Run.Runspace -and -not $Run.Unsettled) {
            $Run.Runspace.Dispose()
            $Run.Runspace = $null
        }
        Clear-LocalWork -Run $Run
    }
    catch {
        if (-not $Run.CleanupFailure) {
            $Run.CleanupFailure = "closing the run's local work failed: $($_.Exception.Message)"
        }
    }
}

# The run's verdict and receipt, written to FILES_MOCK_OUT as Name. Passed only when every simulator position passed,
# every site-only position was omitted with its reason, nothing failed, everything owned was removed, the whole run
# kept within its budget and the journal joined without a problem.
function Write-ScenarioReceipt {
    param($Run, [string] $Name, [hashtable] $Extra = @{})
    $problems = [System.Collections.Generic.List[string]]::new()
    foreach ($item in @($Run.Failure, $Run.CleanupFailure)) {
        if ($item) {
            $problems.Add([string] $item)
        }
    }
    if ($Run.Unresolved.Count) {
        $problems.Add("unresolved: $($Run.Unresolved -join ', ')")
    }
    if ($Run.Unsettled) {
        $problems.Add("$($Run.Unsettled.command), stopped at $($Run.Unsettled.step), did not settle: its runspace, local work and what the run owns are left to the launcher")
    }
    if (-not $Run.Journal) {
        $problems.Add('the journal was not joined')
    }
    elseif (@($Run.Journal.problems).Count) {
        $problems.Add("the journal accounting found $(@($Run.Journal.problems).Count) problems")
    }
    $seconds = $Run.Clock.Elapsed.TotalSeconds
    if ($seconds -gt $script:WholeSeconds) {
        $problems.Add("the run took $([math]::Round($seconds, 1)) s, more than $script:WholeSeconds s")
    }
    $counts = [ordered]@{ total = 0; simulator = 0; siteOnly = 0; passed = 0; failed = 0; notRun = 0; omitted = 0 }
    if ($Run.Contains('Scenario') -and $Run.Scenario) {
        $counts.total = [int] $Run.Scenario.positions.total
        $counts.simulator = [int] $Run.Scenario.positions.simulator
        $counts.siteOnly = [int] $Run.Scenario.positions.site_only
        foreach ($record in $Run.Positions) {
            switch -CaseSensitive ($record.result) {
                'passed' { $counts.passed++ }
                'failed' { $counts.failed++ }
                'not-run' { $counts.notRun++ }
                'omitted' { $counts.omitted++ }
            }
        }
        if (-not $Extra.ContainsKey('controls') -and ($counts.passed -ne $counts.simulator -or $counts.omitted -ne $counts.siteOnly)) {
            $problems.Add("$($counts.passed) of $($counts.simulator) simulator positions passed and $($counts.omitted) of $($counts.siteOnly) site-only positions were omitted")
        }
    }
    $result = if ($problems.Count) { 'failed' } else { 'passed' }
    $receipt = [ordered]@{
        format = 'filescom-powershell-api-scenarios-run/1'
        scenario = $(if ($Run.Contains('Scenario') -and $Run.Scenario) { $Run.Scenario.id } else { $null })
        result = $result
        firstFailure = $Run.Failure
        problems = $problems.ToArray()
        positions = $counts
        steps = $Run.Positions.ToArray()
        identity = $Run.Identity
        ownership = $Run.Ownership
        cleanup = $Run.Cleanup.ToArray()
        cleanupFailure = $Run.CleanupFailure
        unresolved = $Run.Unresolved.ToArray()
        unsettled = $Run.Unsettled
        journal = $Run.Journal
        transport = $(if ($Run.Transport) { [ordered]@{ requests = $Run.Transport.Sent; dropped = $Run.Transport.Dropped; observations = @($Run.Transport.All() | ForEach-Object { ConvertTo-ObservationRecord -Observation $_ }) } } else { $null })
        seconds = [math]::Round($seconds, 3)
    }
    foreach ($key in $Extra.Keys) {
        $receipt[$key] = $Extra[$key]
    }
    # Written to the folder FILES_MOCK_OUT names; a run without one has nowhere to write it, and fails.
    try {
        if (-not $Run.Inputs.Out -or -not (Test-Path -LiteralPath $Run.Inputs.Out -PathType Container)) {
            throw "FILES_MOCK_OUT names no folder ('$($Run.Inputs.Out)')"
        }
        ConvertTo-Json -InputObject $receipt -Depth 24 | Set-Content -LiteralPath (Join-Path -Path $Run.Inputs.Out -ChildPath $Name) -Encoding utf8
    }
    catch {
        Write-Error -Message "$Name could not be written: $($_.Exception.Message). The run's first failure: $($Run.Failure)" -ErrorAction Continue
        return 'failed'
    }
    $result
}

# Runs the scenario's steps in order, stopping at the first failure, then cleans up what it owns, joins the journal and
# closes its local work. What it makes for the run, its local work, connection and runspace included, is made inside
# that lifetime, so a failure making any of them is followed by the same cleanup as a failed step.
function Invoke-ScenarioRun {
    param($Run)
    $Run.Phase = 'work'
    $Run.RunFolder = "powershell-api-scenarios-$($Run.Inputs.RunId)"
    $Run.Namespace = "$($Run.RunFolder)/$($Run.Scenario.id)"
    $Run.Ownership.run.path = $Run.RunFolder
    $Run.Ownership.namespace.path = $Run.Namespace
    $failed = $false
    try {
        Assert-ScenarioBinding -Run $Run
        Initialize-LocalWork -Run $Run
        Connect-ObservedAccount -Run $Run
        $acquired = Invoke-FolderAcquisition -Run $Run -Path $Run.RunFolder -Label 'acquire run folder'
        $Run.Ownership.run.evidence = $acquired
        $Run.Ownership.run.owned = $acquired.owned
        if (-not $acquired.owned) {
            throw "the run folder '$($Run.RunFolder)' was not acquired: status $($acquired.status), $($acquired.failure)"
        }
        foreach ($step in @($Run.Scenario.steps)) {
            if (-not $step.applies) {
                $Run.Positions.Add([ordered]@{ position = $step.number; title = Get-StepTitle -Step $step; backend = $step.backend; result = 'omitted'; reason = $step.omission })
                continue
            }
            $record = [ordered]@{ position = $step.number; title = Get-StepTitle -Step $step; backend = $step.backend; result = 'not-run'; evidence = $null; error = $null }
            $Run.Positions.Add($record)
            if ($failed) {
                continue
            }
            try {
                Assert-PhaseBudget -Run $Run
                $record.evidence = Invoke-ScenarioStep -Run $Run -Step $step
                $record.result = 'passed'
            }
            catch {
                $record.result = 'failed'
                $record.error = $_.Exception.Message
                $failed = $true
                $Run.Failure = "position $($step.number) ($(Get-StepTitle -Step $step)) failed: $($_.Exception.Message)"
            }
        }
    }
    catch {
        if (-not $Run.Failure) {
            $Run.Failure = $_.Exception.Message
        }
    }
    finally {
        try {
            Invoke-ScenarioCleanup -Run $Run
        }
        catch {
            $Run.CleanupFailure = "cleanup failed: $($_.Exception.Message)"
        }
        $Run.Phase = 'journal'
        try {
            [void] (Invoke-JournalAccounting -Run $Run)
        }
        catch {
            $Run.Journal = [ordered]@{ problems = @("the journal could not be joined: $($_.Exception.Message)") }
        }
        Close-RunWork -Run $Run
    }
}

# Runs one case and writes its receipt, a failed one too when the run cannot start, with the first cause.
function Invoke-ScenarioMain {
    param([hashtable] $Paths, [string] $CaseId)
    $run = New-ScenarioRun -Paths $Paths
    $result = 'failed'
    try {
        Initialize-ScenarioRun -Run $run -CaseId $CaseId
        Invoke-ScenarioRun -Run $run
    }
    catch {
        if (-not $run.Failure) {
            $run.Failure = if ($run.Phase -ceq 'setup') { "the run could not start: $($_.Exception.Message)" } else { $_.Exception.Message }
        }
    }
    finally {
        $result = Write-ScenarioReceipt -Run $run -Name 'consumer.json'
    }
    Write-Information -MessageData "Scenario $CaseId $result." -InformationAction Continue
    if ($result -ceq 'passed') { 0 } else { 1 }
}

if ($MyInvocation.InvocationName -ne '.') {
    $exitCode = Invoke-ScenarioMain -CaseId $Case -Paths @{
        CasesPath = $CasesPath
        TransportAssembly = $TransportAssembly
        ModuleManifest = $ModuleManifest
        PackagePath = $PackagePath
        DefinitionPath = $DefinitionPath
        SchemaSha256 = $SchemaSha256
        GenerationPath = $GenerationPath
    }
    exit $exitCode
}
