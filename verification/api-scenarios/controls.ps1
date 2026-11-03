<#
.SYNOPSIS
Runs the API scenario consumer's focused controls: each gives one of the consumer's own checks (binding, readiness and
paging, ownership, cleanup, cancellation, deadlines, setup receipts, downloaded trees, journal identity and journal
accounting) a real case it must refuse, through the installed FilesCom module's public commands against the simulator
generated/mock-server/with-scenario.rb started for the run.

.DESCRIPTION
The controls use run.ps1's functions, which this script dot-sources, on the same connection, transport and owned
folders as a scenario run, so they exercise the code a scenario relies on, not a copy of it. They run the steps of the
first scenario with both a directory upload and a directory download:

- binding: a step whose command lacks a parameter, a step bound to a command the installed module does not have, and an
  intent that sends a parameter the adapter does not bind, are each refused before any request;
- sparse expectations: every scenario cases.json renders, whose operations give an error, retries, an entry or a
  listing only where the definition does, selects its owned folder's creation, deletion and final absence after that
  deletion, with the attempts and interval the definition gives it, and binds every step it runs, before any request;
- dates: a date the parsed cases.json expects in a field's one_of matches that same instant as the parser made it, as
  a UTC DateTime and as a DateTimeOffset nine hours east, but not one second later, and null matches only where that
  one_of holds null;
- readiness: the storage upload operation is found among the several transfers the simulator's own readiness lists,
  and in a list of it alone, and a list of none, or no list, is refused;
- ownership: creating a folder that already exists is answered with the definition's refusal, which owns nothing, and
  cleanup sends nothing for it, while the folder stays;
- cleanup: removing the owned run folder without recursion while it holds a folder fails, is recorded unresolved and is
  not repeated recursively, and the run folder stays;
- paging: a folder listed one page at a time is read entry by entry from pages holding no entry, one, or several, and
  from pages of one entry each;
- cancellation: an upload whose request after its first acknowledged part is held and never cancelled by a stop, so that
  it ends unsent when the hold expires, is not credited as cancelled;
- deadline: an upload held at its first acknowledged part is stopped at a deadline counted from that hold, which
  cancels the held request unsent, settles within one stop allowance, and fails for its deadline, not for what
  stopping it threw;
- unsettled: an upload in a run of its own, on the same connection and owning a folder that run acquired, is stopped
  as that run's whole deadline arrives while the upload is held at its first acknowledged part. The transport signals
  that the held request saw the stop's cancellation and keeps it from completing, so the upload is recorded unsettled:
  nothing more runs in its runspace, that run's cleanup sends nothing and leaves the folder unresolved, and its local
  input stays. Released, the upload settles here, and the folder, still there, is removed;
- expired: a lookup past an explicit deadline, and one past its run's whole deadline, are refused before anything is
  scheduled, and that run's cleanup, with no time left, sends nothing and leaves both folders it acquired unresolved,
  for its deadline; both are still there, and are removed;
- setup: run.ps1 given a case cases.json does not have exits 1 with a failed consumer.json naming that case, holding
  the identity it checked and no other, and sends no request;
- directories: the folder a real directory download of a real directory upload wrote passes as the definition's tree,
  and with an extra empty folder, an extra file, or its empty folder under another name, is refused;
- journal identity: that directory upload's own journal entries pass, and a begin of another path, a request the
  upload does not make, a finish naming another upload and a part naming no part are each refused;
- journal: a request sent to the simulator without the module's transport leaves a journal entry the strict join
  refuses, and the join otherwise accounts for every request.

It then removes what it owns, the namespace recursively and everything else deepest first and without recursion,
confirming each absent; once the cleanup's deadline has passed it sends nothing more, and what is left stays
unresolved. It joins the journal with only that one bypass entry set aside, writes controls.json to FILES_MOCK_OUT,
even when the controls cannot start, and exits 0 only when every control passed, the cleanup resolved everything and
the join found no other problem.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $CasesPath,

    [Parameter(Mandatory)]
    [string] $TransportAssembly,

    [Parameter(Mandatory)]
    [string] $ModuleManifest,

    [Parameter(Mandatory)]
    [string] $PackagePath,

    [Parameter(Mandatory)]
    [string] $DefinitionPath,

    [Parameter(Mandatory)]
    [string] $SchemaSha256,

    [Parameter(Mandatory)]
    [string] $GenerationPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Kept before run.ps1 is dot-sourced, which declares its own parameters of these names in this scope.
$controlPaths = @{
    CasesPath = $CasesPath
    TransportAssembly = $TransportAssembly
    ModuleManifest = $ModuleManifest
    PackagePath = $PackagePath
    DefinitionPath = $DefinitionPath
    SchemaSha256 = $SchemaSha256
    GenerationPath = $GenerationPath
}
. (Join-Path -Path $PSScriptRoot -ChildPath 'run.ps1')

# How long a control's held request waits before it expires, unsent: shorter than any step needs, since nothing will
# cancel it.
$script:ControlHoldSeconds = 3
# How long the deadline control's upload stays held at its first acknowledged part before its deadline stops it.
$script:ControlDeadlineSeconds = 5

# Runs one control: Action must show the refusal, and throws otherwise. An error fails the control, never the others.
function Invoke-Control {
    param($Run, [string] $Name, [string] $Refusal, [scriptblock] $Action)
    $record = [ordered]@{ name = $Name; refusal = $Refusal; result = 'failed'; observed = $null; error = $null }
    try {
        Assert-PhaseBudget -Run $Run
        $record.observed = & $Action
        $record.result = 'passed'
    }
    catch {
        $record.error = $_.Exception.Message
    }
    $Run.Controls.Add($record)
}

# A deep copy of a parsed JSON value, such as a step or the readiness, to change for a control.
function Copy-JsonValue {
    param($Value)
    ConvertTo-Json -InputObject $Value -Depth 32 | ConvertFrom-Json -AsHashtable
}

# A copy of one journal entry, to change for a control without changing the journal it came from.
function Copy-JournalEntry {
    param($Entry)
    $copy = [ordered]@{}
    foreach ($key in @($Entry.Keys)) {
        $copy[$key] = $Entry[$key]
    }
    $copy
}

# The recipe with the fewest bytes, but some, which a control's upload sends as one part.
function Get-SmallestRecipe {
    param($Run)
    @($Run.Manifest.recipes | Where-Object { [long] $_.length -gt 0 } | Sort-Object -Property { [long] $_.length })[0]
}

# Whether Path is there, by one lookup with the scenario's absence step.
function Test-RemotePresent {
    param($Run, [string] $Path, [string] $Label)
    $step = $Run.Protocol.Absent
    $call = Get-StepCall -Run $Run -Step $step -Override @{ (Get-IdentityWireName -Step $step) = $Path }
    $observed = Invoke-ObservedStep -Run $Run -Label $Label -Action { Invoke-PublicCommand -Run $Run -Name $call.Name -Parameters $call.Parameters }
    if (-not $observed.Value.Failure) {
        return $true
    }
    $exception = Get-FilesException -Failure $observed.Value.Failure
    if ($exception -and $exception.HttpStatus -eq $step.expect.status -and $exception.ErrorType -ceq $step.expect.error) {
        return $false
    }
    throw "looking up '$Path' failed: $(Format-Failure -Failure $observed.Value.Failure)"
}

# Runs the controls, then cleans up what they own, runs the journal control and closes the local work. What the run
# makes, its local work, connection and runspace included, is made inside that lifetime.
function Invoke-ControlRun {
    param($Run)
    $Run.Phase = 'work'
    $Run.RunFolder = "powershell-api-scenario-controls-$($Run.Inputs.RunId)"
    $Run.Namespace = "$($Run.RunFolder)/namespace"
    $Run.Ownership.run.path = $Run.RunFolder
    $Run.Ownership.namespace.path = $Run.Namespace
    $existing = "$($Run.RunFolder)/existing"
    $held = "$($Run.RunFolder)/held.bin"
    # What the controls themselves made inside the run folder, but the namespace, in the order they made it.
    $made = [System.Collections.Generic.List[string]]::new()
    $uploadStep = @($Run.Scenario.steps | Where-Object { $_.applies -and $_.kind -ceq 'intent' -and $_.intent -ceq 'upload-directory' })[0]
    $downloadStep = @($Run.Scenario.steps | Where-Object { $_.applies -and $_.kind -ceq 'intent' -and $_.intent -ceq 'download-directory' -and $uploadStep -and $_.fixture -ceq $uploadStep.fixture })[0]
    # Waits, within the work's budget, until the transport holds the request after the first acknowledged part of the
    # upload Command started, or that upload ends first, and returns whether the request is held.
    $waitForBoundary = {
        param($Command)
        $handles = [System.Threading.WaitHandle[]] @($Command.Async.AsyncWaitHandle, $Run.Transport.HeldWaitHandle)
        [void] [System.Threading.WaitHandle]::WaitAny($handles, (ConvertTo-WaitMilliseconds -Seconds (Get-PhaseRemaining -Run $Run)))
        $Run.Transport.HeldWaitHandle.WaitOne(0)
    }
    # A run of its own for a control: this run's connection, transport, clock and settings, with its own Deadlines,
    # Runspace, local Work and Ownership, no cleanup yet and no command that has not settled.
    $newProbe = {
        param($Deadlines, $Runspace, $Work, $Ownership)
        $probe = [ordered]@{}
        foreach ($key in @($Run.Keys)) {
            $probe[$key] = $Run[$key]
        }
        $probe.Phase = 'work'
        $probe.Deadlines = $Deadlines
        $probe.Runspace = $Runspace
        $probe.Work = $Work
        $probe.Ownership = $Ownership
        $probe.Cleanup = [System.Collections.Generic.List[object]]::new()
        $probe.Unresolved = [System.Collections.Generic.List[string]]::new()
        $probe.CleanupFailure = $null
        $probe.Unsettled = $null
        $probe.UnsettledPipeline = $null
        $probe
    }
    try {
        Assert-ScenarioBinding -Run $Run
        Initialize-LocalWork -Run $Run
        Connect-ObservedAccount -Run $Run

        Invoke-Control -Run $Run -Name 'binding: a parameter the installed command lacks' -Refusal 'the binding check names the missing parameter before any request' -Action {
            $source = @($Run.Scenario.steps | Where-Object { $_.applies -and $_.kind -ceq 'operation' -and @($_.arguments).Count })[0]
            $step = Copy-JsonValue -Value $source
            $step.arguments[0].parameter = "$($step.arguments[0].parameter)Unbound"
            $before = $Run.Transport.Sent
            $problems = @(Get-StepBindingProblem -Step $step)
            if (-not @($problems | Where-Object { $_ -like "*has no parameter -$($step.arguments[0].parameter)*" }).Count) {
                throw "the binding check accepted -$($step.arguments[0].parameter): $($problems -join '; ')"
            }
            if ($Run.Transport.Sent -ne $before) {
                throw 'the binding check sent a request'
            }
            [ordered]@{ problems = $problems }
        }
        Invoke-Control -Run $Run -Name 'binding: an operation bound to a command the installed module does not have' -Refusal 'the binding check names the missing command before any request' -Action {
            $source = @($Run.Scenario.steps | Where-Object { $_.applies -and $_.kind -ceq 'operation' })[0]
            $step = Copy-JsonValue -Value $source
            $step.command.name = 'Get-FilesApiScenarioUnbound'
            $problems = @(Get-StepBindingProblem -Step $step)
            if (-not @($problems | Where-Object { $_ -like '*Get-FilesApiScenarioUnbound, which the installed module does not have*' }).Count) {
                throw "the binding check accepted Get-FilesApiScenarioUnbound: $($problems -join '; ')"
            }
            [ordered]@{ problems = $problems }
        }
        Invoke-Control -Run $Run -Name 'binding: an intent parameter the adapter does not bind' -Refusal 'the binding check names the unbound intent parameter before any request' -Action {
            $source = @($Run.Manifest.scenarios | ForEach-Object { $_.steps } | Where-Object { $_.kind -ceq 'intent' -and (Get-JsonValue $_.details 'parameters') })[0]
            if (-not $source) {
                throw 'the definition has no intent step with parameters to control'
            }
            $step = Copy-JsonValue -Value $source
            $step.details.parameters['unbound_field'] = 1
            $problems = @(Get-StepBindingProblem -Step $step)
            if (-not @($problems | Where-Object { $_ -like '*unbound_field, which the * binding does not bind to a parameter*' }).Count) {
                throw "the binding check accepted unbound_field: $($problems -join '; ')"
            }
            [ordered]@{ problems = $problems }
        }
        Invoke-Control -Run $Run -Name 'sparse expectations: every rendered scenario selects its owned-folder protocol and binds' -Refusal "every scenario cases.json renders, whose operations give an error, retries, an entry or a listing only where the definition does, selects its owned folder's creation, deletion and final absence after that deletion, with the attempts and interval the definition gives it, and binds every step it runs, sending no request" -Action {
            $before = $Run.Transport.Sent
            $selected = [ordered]@{}
            $withoutRetries = 0
            $problems = [System.Collections.Generic.List[string]]::new()
            foreach ($scenario in @($Run.Manifest.scenarios)) {
                $protocol = Get-NamespaceProtocol -Scenario $scenario
                $absent = $protocol.Absent
                # The absence the cleanup waits for is the lookup after the deletion, with the retries the definition gives it.
                if ($absent.number -le $protocol.Delete.number -or -not $absent.expect.Contains('attempts') -or -not $absent.expect.Contains('interval_seconds')) {
                    throw "scenario $($scenario.id) took position $($absent.number) as its owned folder's absence, not the retried lookup after its deletion at position $($protocol.Delete.number)"
                }
                foreach ($step in @($scenario.steps | Where-Object { $_.applies })) {
                    if ($step.kind -ceq 'operation' -and -not $step.expect.Contains('attempts')) {
                        $withoutRetries++
                    }
                    foreach ($problem in @(Get-StepBindingProblem -Step $step)) {
                        $problems.Add("$($scenario.id) position $($step.number): $problem")
                    }
                }
                $selected[[string] $scenario.id] = [ordered]@{
                    create = [ordered]@{ position = $protocol.Create.number; status = $protocol.Create.expect.status }
                    delete = [ordered]@{ position = $protocol.Delete.number; status = $protocol.Delete.expect.status }
                    absent = [ordered]@{ position = $absent.number; status = $absent.expect.status; error = $absent.expect.error; attempts = $absent.expect.attempts; intervalSeconds = $absent.expect.interval_seconds }
                }
            }
            if (-not $withoutRetries) {
                throw 'no scenario cases.json renders has an operation whose expectation omits its retries'
            }
            if ($problems.Count) {
                throw "the rendered scenarios do not bind: $($problems -join '; ')"
            }
            if ($Run.Transport.Sent -ne $before) {
                throw 'selecting the owned-folder protocols and binding the steps sent a request'
            }
            [ordered]@{ scenarios = $selected; operationsWithoutRetries = $withoutRetries }
        }
        Invoke-Control -Run $Run -Name 'dates: an expected date matches its own instant in any form, and no other' -Refusal "a date the parsed cases.json expects in a field's one_of matches that instant as the parser made it, as a UTC DateTime and as a DateTimeOffset nine hours east, refuses one second later, and matches null only where that one_of holds null" -Action {
            # The first field of a rendered scenario matched against a date, as ConvertFrom-Json read it from cases.json.
            $found = $null
            foreach ($scenario in @($Run.Manifest.scenarios)) {
                foreach ($step in @($scenario.steps | Where-Object { $_.applies -and $_.kind -ceq 'operation' })) {
                    $entry = Get-JsonValue $step.expect 'entry'
                    foreach ($field in @(if ($entry) { $entry.fields })) {
                        $candidates = Get-JsonValue $field.matcher 'one_of'
                        foreach ($candidate in @(if ($null -ne $candidates) { $candidates })) {
                            if (-not $found -and ($candidate -is [datetime] -or $candidate -is [datetimeoffset])) {
                                $found = [ordered]@{ scenario = $scenario.id; position = $step.number; field = $field.name; matcher = $field.matcher; date = $candidate }
                            }
                        }
                    }
                }
            }
            if (-not $found) {
                throw 'no field of a rendered scenario is matched against a date as cases.json was parsed'
            }
            $date = $found.date
            if ($date -is [datetime] -and $date.Kind -eq [System.DateTimeKind]::Unspecified) {
                throw "the date $($found.field) expects, $date, was parsed without its time zone"
            }
            $moment = if ($date -is [datetimeoffset]) { $date } else { [datetimeoffset]::new($date) }
            $matcher = $found.matcher
            # Assert-FieldMatcher's refusal of Value against a matcher, or $null when it accepts it.
            $verdict = {
                param($Value, $Against)
                try {
                    Assert-FieldMatcher -Value $Value -Matcher $Against -What "the control's $($found.field)"
                    $null
                }
                catch {
                    $_.Exception.Message
                }
            }
            # The instant the field expects, as the parser made it and as the module may write it.
            $forms = [ordered]@{ 'the date as parsed' = $date; 'a UTC DateTime' = $moment.UtcDateTime; 'a DateTimeOffset nine hours east' = $moment.ToOffset([timespan]::FromHours(9)) }
            foreach ($form in @($forms.Keys)) {
                $answer = & $verdict $forms[$form] $matcher
                if ($answer) {
                    throw "the instant $($found.field) expects, as $form, was refused: $answer"
                }
            }
            $later = & $verdict $moment.AddSeconds(1).UtcDateTime $matcher
            if ($later -notlike '*not one of*') {
                throw "one second after the instant $($found.field) expects was not refused as none of its values: '$later'"
            }
            $nullExpected = $false
            foreach ($candidate in $matcher.one_of) {
                if ($null -eq $candidate) {
                    $nullExpected = $true
                }
            }
            $withNull = & $verdict $null $matcher
            $withoutNull = & $verdict $null ([ordered]@{ one_of = @(foreach ($candidate in $matcher.one_of) { if ($null -ne $candidate) { $candidate } }) })
            if ([bool] $withNull -eq $nullExpected -or -not $withoutNull) {
                throw "null was $(if ($withNull) { 'refused' } else { 'accepted' }) by the one_of, which $(if ($nullExpected) { 'holds' } else { 'does not hold' }) null, and $(if ($withoutNull) { 'refused' } else { 'accepted' }) by it without its null"
            }
            [ordered]@{
                scenario = $found.scenario; position = $found.position; field = $found.field
                parsed = $date.GetType().FullName; kind = $(if ($date -is [datetime]) { [string] $date.Kind } else { $null })
                instant = $moment.UtcDateTime.ToString('o', $script:Invariant); later = $later; nullExpected = $nullExpected
            }
        }
        Invoke-Control -Run $Run -Name 'readiness: the storage upload operation is found among several, one or no listed transfers' -Refusal "among the several transfers the simulator's own readiness lists, and in a list of it alone, the one upload operation is found; a list of none, or no list, is refused" -Action {
            $ready = Read-JsonFile -Path (Join-Path -Path $Run.Inputs.Out -ChildPath 'ready.json')
            $listed = @(Get-ListItem -List (Get-JsonValue $ready 'transfers', 'operations') -What 'the readiness transfers.operations')
            $several = Get-TransferOperation -Ready $ready
            if ($listed.Count -lt 2 -or $several.ById.Count -ne $listed.Count -or $several.UploadPart -cne $Run.Transfers.UploadPart) {
                throw "the readiness lists $($listed.Count) transfers, of which $($several.ById.Count) were read and '$($several.UploadPart)' taken as the upload"
            }
            $alone = Copy-JsonValue -Value $ready
            $alone.transfers.operations = @($listed | Where-Object { $_.method -ceq 'PUT' })
            $single = Get-TransferOperation -Ready $alone
            if ($single.ById.Count -ne 1 -or $single.UploadPart -cne $Run.Transfers.UploadPart) {
                throw "the upload operation listed alone was read as $($single.ById.Count) transfers, with the upload '$($single.UploadPart)'"
            }
            $none = Copy-JsonValue -Value $ready
            $none.transfers.operations = @()
            $unlisted = Copy-JsonValue -Value $ready
            [void] $unlisted.transfers.Remove('operations')
            $shapes = [ordered]@{ 'a list of none' = $none; 'no list' = $unlisted }
            $refusals = [ordered]@{}
            foreach ($shape in @($shapes.Keys)) {
                try {
                    [void] (Get-TransferOperation -Ready $shapes[$shape])
                    $refusals[$shape] = $null
                }
                catch {
                    $refusals[$shape] = $_.Exception.Message
                }
                if (-not $refusals[$shape]) {
                    throw "the readiness with $shape was read as listing the upload operation"
                }
            }
            [ordered]@{ listed = $listed.Count; uploadPart = $several.UploadPart; reads = $several.Reads; refusals = $refusals }
        }

        $acquired = Invoke-FolderAcquisition -Run $Run -Path $Run.RunFolder -Label 'acquire run folder'
        $Run.Ownership.run.evidence = $acquired
        $Run.Ownership.run.owned = $acquired.owned
        if (-not $acquired.owned) {
            throw "the run folder '$($Run.RunFolder)' was not acquired: status $($acquired.status), $($acquired.failure)"
        }

        Invoke-Control -Run $Run -Name 'ownership: a refused creation owns nothing and is not cleaned up' -Refusal 'creating an existing folder is refused, owns nothing, and cleanup sends nothing for it' -Action {
            $first = Invoke-FolderAcquisition -Run $Run -Path $existing -Label 'control ownership create'
            if (-not $first.owned) {
                throw "the control could not make '$existing' to refuse: status $($first.status), $($first.failure)"
            }
            $made.Add($existing)
            $refused = Invoke-FolderAcquisition -Run $Run -Path $existing -Label 'control ownership refused'
            if ($refused.owned) {
                throw 'creating a folder that already exists was taken as ownership'
            }
            if ($refused.status -eq $Run.Protocol.Create.expect.status -or -not $refused.failure) {
                throw "creating a folder that already exists was answered $($refused.status) without a failure"
            }
            $before = $Run.Transport.Sent
            $ownership = [ordered]@{
                run = [ordered]@{ path = $Run.RunFolder; owned = $false; removed = $false }
                namespace = [ordered]@{ path = $existing; owned = $refused.owned; removed = $false }
            }
            $targets = @(Get-CleanupTarget -Ownership $ownership)
            if ($targets.Count -or $Run.Transport.Sent -ne $before) {
                throw 'cleanup would remove a folder whose creation was refused'
            }
            if (-not (Test-RemotePresent -Run $Run -Path $existing -Label 'control ownership lookup')) {
                throw "'$existing' is gone after the refused creation"
            }
            [ordered]@{ created = $first; refused = $refused }
        }
        Invoke-Control -Run $Run -Name 'cleanup: a failed removal without recursion is recorded unresolved, never made recursive' -Refusal 'the run folder, which holds a folder, is not removed by a non-recursive deletion' -Action {
            if ($existing -cnotin $made) {
                throw 'the run folder holds no folder for this control'
            }
            $removal = Invoke-OwnedRemoval -Run $Run -Path $Run.RunFolder -Recursive $false -Label 'control nonrecursive removal'
            if ($removal.absent -or -not $removal.failure) {
                throw "the run folder was removed without recursion while it held '$existing'"
            }
            $deletions = @($removal.requests | Where-Object { $_.method -ceq $Run.Protocol.Delete.operation.method })
            if ($deletions.Count -ne 1) {
                throw "the removal sent $($deletions.Count) deletions, not one"
            }
            if (-not (Test-RemotePresent -Run $Run -Path $Run.RunFolder -Label 'control nonrecursive lookup')) {
                throw 'the run folder is gone after the refused removal'
            }
            [ordered]@{ removal = $removal }
        }
        Invoke-Control -Run $Run -Name 'paging: pages of no entry, one or several are read entry by entry' -Refusal 'a folder listed one page at a time yields each entry of a page holding none, one or several, and of pages of one entry each, never a page as an entry' -Action {
            $source = @($Run.Scenario.steps | Where-Object { $_.applies -and $_.kind -ceq 'operation' -and (Get-JsonValue $_.expect 'listing') -and @($_.arguments | Where-Object { $_.wire_name -ceq 'per_page' }).Count })[0]
            if (-not $source) {
                throw 'the scenario has no listing step with per_page to page with'
            }
            # The scenario's own listing step, of Path with PerPage entries a page, with no expected entries of its own.
            $list = {
                param([string] $Path, [int] $PerPage)
                $step = Copy-JsonValue -Value $source
                foreach ($argument in @($step.arguments)) {
                    if ($argument.identity) {
                        $argument.value = $Path
                    }
                    elseif ($argument.wire_name -ceq 'per_page') {
                        $argument.value = $PerPage
                    }
                }
                $step.expect.listing.per_page = $PerPage
                $step.expect.listing.entries = $null
                Invoke-ListingStep -Run $Run -Step $step
            }
            $pages = "$($Run.RunFolder)/pages"
            $inside = @("$pages/one", "$pages/two")
            $listings = [ordered]@{}
            foreach ($path in @($pages) + $inside) {
                $created = Invoke-FolderAcquisition -Run $Run -Path $path -Label 'control paging create'
                if (-not $created.owned) {
                    throw "the control could not make '$path': status $($created.status), $($created.failure)"
                }
                $made.Add($path)
                if ($path -ceq $pages) {
                    $listings['no entry'] = & $list $pages 1000
                }
                elseif ($path -ceq $inside[0]) {
                    $listings['one entry'] = & $list $pages 1000
                }
                else {
                    $listings['several entries'] = & $list $pages 1000
                    $listings['one entry a page'] = & $list $pages 1
                }
            }
            $expected = [ordered]@{ 'no entry' = @(); 'one entry' = @($inside[0]); 'several entries' = $inside; 'one entry a page' = $inside }
            foreach ($case in @($expected.Keys)) {
                Assert-SameList -Actual @($listings[$case].listed) -Expected @($expected[$case]) -What "the entries listed with $case"
            }
            if (@($listings['no entry'].entries | Where-Object { $_ -ne 0 }).Count -or
                -not @($listings['one entry'].entries | Where-Object { $_ -eq 1 }).Count -or
                -not @($listings['several entries'].entries | Where-Object { $_ -ge 2 }).Count -or
                @($listings['one entry a page'].entries | Where-Object { $_ -gt 1 }).Count -or $listings['one entry a page'].pages -lt 2) {
                throw "the pages held $(@(foreach ($case in @($listings.Keys)) { "$case [$(@($listings[$case].entries) -join ', ')]" }) -join '; ') entries"
            }
            $listings
        }
        Invoke-Control -Run $Run -Name 'cancellation: a held request that no stop cancels is not credited' -Refusal 'an upload whose held request expired unsent is not taken as cancelled at its first acknowledged part' -Action {
            $local = Get-LocalPath -Run $Run -Name 'control-held.bin'
            [FilesCom.ApiScenarios.Recipe]::WriteNewFile($local, (Get-RecipeContent -Recipe (Get-SmallestRecipe -Run $Run)))
            $parameters = @{ LiteralPath = $local; RemotePath = $held; PassThru = $true; Confirm = $false; Connection = $Run.Connection; ErrorAction = 'Stop' }
            $boundary = $null
            $Run.Transport.ArmFirstAcknowledgedPartBoundary([timespan]::FromSeconds($script:ControlHoldSeconds))
            try {
                $observed = Invoke-ObservedStep -Run $Run -Label 'control cancellation' -Action { Invoke-PublicCommand -Run $Run -Name 'Send-FilesFile' -Parameters $parameters -Deadline ($Run.Clock.Elapsed.TotalSeconds + 60) }
            }
            finally {
                $boundary = $Run.Transport.DisarmBoundary()
                # The control's own upload, which may have completed after its held request expired.
                $made.Add($held)
            }
            $problems = @(Get-CancellationProblem -Outcome $observed.Value -Boundary $boundary)
            if (-not $problems.Count) {
                throw 'an upload whose held request expired unsent was credited as cancelled'
            }
            if (-not $boundary.Held -or $boundary.HeldOutcome -cne 'held-expired-unsent') {
                throw "the boundary saw '$($boundary.HeldOutcome)', not a held request that expired unsent"
            }
            [ordered]@{ boundary = $boundary; problems = $problems; state = $observed.Value.State; failure = Format-Failure -Failure $observed.Value.Failure }
        }
        Invoke-Control -Run $Run -Name 'deadline: a command still running at its deadline is stopped once and fails for its deadline' -Refusal 'an upload held past a deadline counted from its hold is stopped there, its held request cancelled unsent, settles within one stop allowance, and fails as not ending by its deadline, not as what stopping it threw' -Action {
            $local = Get-LocalPath -Run $Run -Name 'control-deadline.bin'
            [FilesCom.ApiScenarios.Recipe]::WriteNewFile($local, (Get-RecipeContent -Recipe (Get-SmallestRecipe -Run $Run)))
            $remote = "$($Run.RunFolder)/deadline.bin"
            $made.Add($remote)
            $parameters = @{ LiteralPath = $local; RemotePath = $remote; PassThru = $true; Confirm = $false; Connection = $Run.Connection; ErrorAction = 'Stop' }
            $boundary = $null
            $started = $Run.Clock.Elapsed.TotalSeconds
            # Held far longer than the deadline, so that only the deadline's stop ends the upload.
            $Run.Transport.ArmFirstAcknowledgedPartBoundary([timespan]::FromSeconds($script:HoldSeconds))
            try {
                $observed = Invoke-ObservedStep -Run $Run -Label 'control deadline' -Action {
                    $command = Start-PublicCommand -Run $Run -Name 'Send-FilesFile' -Parameters $parameters
                    # The deadline counts from the hold, so that the upload's start and first part are no part of it.
                    $reached = & $waitForBoundary $command
                    $heldAt = $Run.Clock.Elapsed.TotalSeconds
                    $outcome = Wait-PublicCommand -Run $Run -Command $command -Deadline ($heldAt + $script:ControlDeadlineSeconds)
                    [pscustomobject]@{ Reached = $reached; HeldAt = $heldAt; Outcome = $outcome }
                }
            }
            finally {
                $boundary = $Run.Transport.DisarmBoundary()
            }
            $outcome = $observed.Value.Outcome
            $elapsed = $Run.Clock.Elapsed.TotalSeconds - $observed.Value.HeldAt
            if (-not $observed.Value.Reached) {
                throw "the upload was not held at its first acknowledged part before it ended or the work's budget ran out, in state $($outcome.State)"
            }
            if (-not $outcome.TimedOut -or -not $outcome.StopRequested -or -not $outcome.StopSettled -or $outcome.Unsettled) {
                throw "the upload held past its deadline was timed out $($outcome.TimedOut), stopped $($outcome.StopRequested) and settled $($outcome.StopSettled)"
            }
            if ([string] $outcome.Failure -notlike 'Send-FilesFile did not end by its deadline*') {
                throw "the upload stopped at its deadline failed with '$(Format-Failure -Failure $outcome.Failure)', not for its deadline"
            }
            if ($elapsed -gt $script:ControlDeadlineSeconds + $outcome.StopMilliseconds / 1000 + 1) {
                throw "the upload took $([math]::Round($elapsed, 1)) s from its hold, more than its $($script:ControlDeadlineSeconds) s deadline and one stop of $($outcome.StopMilliseconds) ms"
            }
            if (-not $boundary.Held -or $boundary.HeldOutcome -cne 'held-cancelled-unsent') {
                throw "the request held at the deadline ended '$($boundary.HeldOutcome)', not cancelled unsent by the stop"
            }
            [ordered]@{ secondsToHold = [math]::Round($observed.Value.HeldAt - $started, 3); seconds = [math]::Round($elapsed, 3); stopMilliseconds = $outcome.StopMilliseconds; state = $outcome.State; failure = $outcome.Failure; stoppedWith = Format-Failure -Failure $outcome.StopFailure; boundary = $boundary }
        }
        Invoke-Control -Run $Run -Name 'unsettled: a stopped command that has not settled keeps its runspace, inputs and what the run owns' -Refusal "an upload stopped as its run's whole deadline arrives, while the transport keeps its cancelled request from completing, is recorded unsettled; nothing more runs in its runspace, the cleanup sends nothing and leaves the folder that run acquired unresolved, and its local input stays; released, it settles, and the folder is still there" -Action {
            $probe = $null
            $result = $null
            $boundary = $null
            # What failed while the upload was held, kept until the boundary has reported what its held request did.
            $failure = $null
            $work = Join-Path -Path $Run.Work -ChildPath 'control-unsettled'
            [void] [System.IO.Directory]::CreateDirectory($work)
            $local = Join-Path -Path $work -ChildPath 'unsettled.bin'
            [FilesCom.ApiScenarios.Recipe]::WriteNewFile($local, (Get-RecipeContent -Recipe (Get-SmallestRecipe -Run $Run)))
            # The folder the probe run owns, acquired as a run acquires its own: one creation the transport saw answered
            # with the expected status, returning exactly that folder.
            $folder = "$($Run.RunFolder)/unsettled"
            $acquisition = Invoke-FolderAcquisition -Run $Run -Path $folder -Label 'control unsettled create'
            if (-not $acquisition.owned) {
                throw "the control could not make '$folder': status $($acquisition.status), $($acquisition.failure)"
            }
            $made.Add($folder)
            $remote = "$folder/unsettled.bin"
            $made.Add($remote)
            $parameters = @{ LiteralPath = $local; RemotePath = $remote; PassThru = $true; Confirm = $false; Connection = $Run.Connection; ErrorAction = 'Stop' }
            # The request after the upload's first acknowledged part is held; once the stop cancels it, the transport
            # signals that and keeps it, and so the upload, from completing until it is released below.
            $Run.Transport.ArmFirstAcknowledgedPartBoundary([timespan]::FromSeconds($script:HoldSeconds), $true)
            try {
                # A run of its own, on the same connection and transport but with a runspace of its own, owning the folder.
                $ownership = [ordered]@{
                    run = [ordered]@{ path = $folder; owned = $acquisition.owned; removed = $false; evidence = $acquisition }
                    namespace = [ordered]@{ path = $null; owned = $false; deleted = $false; removed = $false; evidence = $null }
                }
                $deadlines = [ordered]@{ work = Get-PhaseDeadline -Run $Run; cleanup = $null; whole = $Run.Deadlines.whole }
                $probe = & $newProbe $deadlines (New-CommandRunspace -Run $Run) $work $ownership
                $Run.Transport.Step = 'control unsettled'
                $command = Start-PublicCommand -Run $probe -Name 'Send-FilesFile' -Parameters $parameters
                $reached = & $waitForBoundary $command
                # The probe's whole run ends now, with its upload held: its deadline passes, with no time left to settle.
                $probe.Deadlines.work = $probe.Deadlines.whole = $Run.Clock.Elapsed.TotalSeconds
                $outcome = Wait-PublicCommand -Run $probe -Command $command
                $sent = $Run.Transport.Sent
                if (-not $reached) {
                    throw "the upload was not held at its first acknowledged part before it ended or the work's budget ran out, in state $($outcome.State)"
                }
                if (-not $outcome.Unsettled -or -not $probe.Unsettled -or -not $outcome.TimedOut -or -not $outcome.StopRequested -or $outcome.StopSettled) {
                    throw "the upload stopped with no time left to settle was taken as settled: timed out $($outcome.TimedOut), stop requested $($outcome.StopRequested), settled $($outcome.StopSettled) after $($outcome.StopMilliseconds) ms, unsettled $($outcome.Unsettled), in state $($outcome.State), failing '$(Format-Failure -Failure $outcome.Failure)'"
                }
                # The stop reached the held request, which saw its cancellation and is kept from completing.
                if (-not $Run.Transport.CancelledWaitHandle.WaitOne((ConvertTo-WaitMilliseconds -Seconds (Get-StopAllowance -Run $Run)))) {
                    throw 'the stop did not cancel the request the transport held'
                }
                if ($command.Async.IsCompleted) {
                    throw 'the upload completed while the transport kept its cancelled request from completing'
                }
                $refusal = $null
                try {
                    [void] (Invoke-PublicCommand -Run $probe -Name 'Send-FilesFile' -Parameters $parameters)
                }
                catch {
                    $refusal = $_.Exception.Message
                }
                if (-not $refusal -or $refusal -notlike '*has not settled*') {
                    throw "a command was run in the runspace of one that has not settled: '$refusal'"
                }
                Invoke-ScenarioCleanup -Run $probe
                Close-RunWork -Run $probe
                if ($Run.Transport.Sent -ne $sent -or $probe.Cleanup.Count) {
                    throw 'the run whose command has not settled sent a request after it'
                }
                if ($folder -cnotin $probe.Unresolved -or -not $probe.CleanupFailure -or -not $probe.Ownership.run.owned -or $probe.Ownership.run.removed -or
                    -not [object]::ReferenceEquals($probe.Ownership.run.evidence, $acquisition)) {
                    throw 'the run whose command has not settled did not keep the folder it acquired owned and unresolved'
                }
                if (-not (Test-Path -LiteralPath $local -PathType Leaf) -or $null -eq $probe.Runspace -or $probe.Unsettled.localWork -cne $work) {
                    throw "the run whose command has not settled removed that command's input or runspace"
                }
                $result = [ordered]@{ unsettled = $probe.Unsettled; failure = $outcome.Failure; refusal = $refusal; unresolved = $probe.Unresolved.ToArray(); cleanupFailure = $probe.CleanupFailure; acquired = $acquisition; boundary = $null; removal = $null }
            }
            catch {
                $failure = $_.Exception.Message
            }
            finally {
                # Released, the stopped upload settles: it is waited for here, within the work's budget, before anything
                # else uses the transport. Only one that still does not settle is left to the launcher, as the run's own
                # unsettled command.
                $Run.Transport.ReleaseCompletion()
                if ($probe -and $probe.UnsettledPipeline) {
                    $pipeline = $probe.UnsettledPipeline
                    if ($pipeline.Async.AsyncWaitHandle.WaitOne((ConvertTo-WaitMilliseconds -Seconds ([math]::Min($script:StopSeconds, (Get-PhaseRemaining -Run $Run)))))) {
                        try {
                            [void] $pipeline.Shell.EndInvoke($pipeline.Async)
                        }
                        catch {
                            Write-Verbose -Message "The stopped probe upload ended: $($_.Exception.Message)"
                        }
                        $pipeline.Shell.Dispose()
                        $probe.Runspace.Dispose()
                    }
                    else {
                        $Run.Unsettled = $probe.Unsettled
                        $Run.UnsettledPipeline = $pipeline
                    }
                }
                elseif ($probe -and $probe.Runspace) {
                    $probe.Runspace.Dispose()
                }
                $boundary = $Run.Transport.DisarmBoundary()
            }
            # A failure keeps what was observed: the outcome in its message, and what the held request then did.
            if ($failure) {
                throw "$failure; the held request then ended '$($boundary.HeldOutcome)' after $($boundary.HoldMilliseconds) ms, cancelled $($boundary.Cancelled), released $($boundary.CompletionReleased)"
            }
            # Released, the held request ended unsent as cancelled, and the folder the cleanup left is still there: it is
            # removed now, as the run's own.
            if ($Run.Unsettled) {
                throw 'the stopped upload did not settle once its held request was released'
            }
            if (-not $boundary.Cancelled -or $boundary.CompletionReleased -ne $true -or $boundary.HeldOutcome -cne 'held-cancelled-unsent') {
                throw "the held request ended '$($boundary.HeldOutcome)', cancelled $($boundary.Cancelled) and released $($boundary.CompletionReleased), not cancelled unsent once released"
            }
            if (-not (Test-RemotePresent -Run $Run -Path $folder -Label 'control unsettled lookup')) {
                throw "'$folder', left unresolved, is gone"
            }
            $removal = Invoke-OwnedRemoval -Run $Run -Path $folder -Recursive $false -Label 'control unsettled removal'
            if (-not $removal.absent) {
                throw "'$folder' was not removed once the upload settled: $($removal.failure)"
            }
            $probe.Ownership.run.removed = $true
            $result.boundary = $boundary
            $result.removal = $removal
            $result
        }
        Invoke-Control -Run $Run -Name 'expired: a command past its deadline is not started, and a cleanup out of time sends nothing' -Refusal "a lookup past an explicit deadline, and one past its run's whole deadline, are refused before anything is scheduled; that run's cleanup, with no time left, sends nothing and leaves both folders it acquired unresolved, for its deadline, and both are still there" -Action {
            $outer = "$($Run.RunFolder)/expired"
            $inner = "$outer/namespace"
            $acquisitions = [ordered]@{}
            foreach ($path in $outer, $inner) {
                $acquisitions[$path] = Invoke-FolderAcquisition -Run $Run -Path $path -Label 'control expired create'
                if (-not $acquisitions[$path].owned) {
                    throw "the control could not make '$path': status $($acquisitions[$path].status), $($acquisitions[$path].failure)"
                }
                $made.Add($path)
            }
            # A run of its own whose whole deadline has passed, owning both folders as a run owns its folder and namespace.
            $ownership = [ordered]@{
                run = [ordered]@{ path = $outer; owned = $acquisitions[$outer].owned; removed = $false; evidence = $acquisitions[$outer] }
                namespace = [ordered]@{ path = $inner; owned = $acquisitions[$inner].owned; deleted = $false; removed = $false; evidence = $acquisitions[$inner] }
            }
            $now = $Run.Clock.Elapsed.TotalSeconds
            $probe = & $newProbe ([ordered]@{ work = Get-PhaseDeadline -Run $Run; cleanup = $null; whole = $now }) $Run.Runspace $null $ownership
            $lookup = Get-StepCall -Run $Run -Step $Run.Protocol.Absent -Override @{ (Get-IdentityWireName -Step $Run.Protocol.Absent) = $outer }
            $sent = $Run.Transport.Sent
            $refusals = [ordered]@{}
            $refuse = {
                param([string] $Case, $Target, [double] $Deadline)
                try {
                    [void] (Invoke-PublicCommand -Run $Target -Name $lookup.Name -Parameters $lookup.Parameters -Deadline $Deadline)
                    $refusals[$Case] = $null
                }
                catch [System.TimeoutException] {
                    $refusals[$Case] = $_.Exception.Message
                }
            }
            & $refuse 'an explicit deadline' $Run $now
            & $refuse "its run's whole deadline" $probe ([double]::MaxValue)
            Invoke-ScenarioCleanup -Run $probe
            if ($Run.Transport.Sent -ne $sent) {
                throw "$($Run.Transport.Sent - $sent) requests were sent past their deadlines"
            }
            foreach ($case in @($refusals.Keys)) {
                if (-not $refusals[$case]) {
                    throw "a lookup past $case was run"
                }
            }
            if ($outer -cnotin $probe.Unresolved -or $inner -cnotin $probe.Unresolved -or $probe.Cleanup.Count -or [string] $probe.CleanupFailure -notlike '*deadline*had passed*') {
                throw "the cleanup with no time left recorded $($probe.Cleanup.Count) removals, left [$($probe.Unresolved -join ', ')] unresolved and failed with '$($probe.CleanupFailure)'"
            }
            foreach ($role in 'namespace', 'run') {
                if (-not $probe.Ownership[$role].owned -or $probe.Ownership[$role].removed) {
                    throw "the cleanup with no time left gave up the $role folder its run owns"
                }
            }
            # Both folders are still there; they are removed now, deepest first, as the run's own.
            $removals = foreach ($path in $inner, $outer) {
                if (-not (Test-RemotePresent -Run $Run -Path $path -Label 'control expired lookup')) {
                    throw "'$path', left unresolved, is gone"
                }
                $removal = Invoke-OwnedRemoval -Run $Run -Path $path -Recursive $false -Label 'control expired removal'
                if (-not $removal.absent) {
                    throw "'$path' was not removed: $($removal.failure)"
                }
                $removal
            }
            [ordered]@{ refusals = $refusals; unresolved = $probe.Unresolved.ToArray(); cleanupFailure = $probe.CleanupFailure; acquired = $acquisitions; removals = @($removals) }
        }
        Invoke-Control -Run $Run -Name 'setup: a run that cannot start still writes its failed receipt' -Refusal 'run.ps1, given a case cases.json does not have, exits 1 with a failed consumer.json naming that case, holding the identity it checked and no other, and sends no request' -Action {
            $case = 'files.api-scenario-controls-no-such-case'
            $out = Join-Path -Path $Run.Inputs.Out -ChildPath 'control-setup'
            [void] [System.IO.Directory]::CreateDirectory($out)
            foreach ($name in 'ready.json', 'reset.json') {
                Copy-Item -LiteralPath (Join-Path -Path $Run.Inputs.Out -ChildPath $name) -Destination $out
            }
            $before = @((Read-SimulatorJournal -Run $Run).entries).Count
            $start = [System.Diagnostics.ProcessStartInfo]::new([System.Environment]::ProcessPath)
            $arguments = @(
                '-NoProfile', '-NonInteractive', '-File', (Join-Path -Path $PSScriptRoot -ChildPath 'run.ps1'), '-Case', $case,
                '-CasesPath', $Run.Paths.CasesPath, '-TransportAssembly', $Run.Paths.TransportAssembly, '-ModuleManifest', $Run.Paths.ModuleManifest,
                '-PackagePath', $Run.Paths.PackagePath, '-DefinitionPath', $Run.Paths.DefinitionPath, '-SchemaSha256', $Run.Paths.SchemaSha256,
                '-GenerationPath', $Run.Paths.GenerationPath)
            foreach ($argument in $arguments) {
                $start.ArgumentList.Add($argument)
            }
            $start.UseShellExecute = $false
            $start.Environment['FILES_MOCK_OUT'] = $out
            $process = [System.Diagnostics.Process]::Start($start)
            try {
                if (-not $process.WaitForExit((ConvertTo-WaitMilliseconds -Seconds (Get-PhaseRemaining -Run $Run)))) {
                    $process.Kill($true)
                    throw 'run.ps1 given an unknown case did not end within the work budget'
                }
                $exit = $process.ExitCode
            }
            finally {
                $process.Dispose()
            }
            $receipt = Read-JsonFile -Path (Join-Path -Path $out -ChildPath 'consumer.json')
            $after = @((Read-SimulatorJournal -Run $Run).entries).Count
            if ($exit -ne 1 -or $receipt.result -cne 'failed' -or [string] $receipt.firstFailure -notlike "*$case*") {
                throw "run.ps1 given an unknown case exited $exit with a $($receipt.result) receipt whose first failure is '$($receipt.firstFailure)'"
            }
            if ($null -eq (Get-JsonValue $receipt 'identity', 'definition') -or $null -ne (Get-JsonValue $receipt 'identity', 'module') -or $null -ne $receipt.transport) {
                throw "the receipt of a run that could not start holds the identity [$(@($receipt.identity.Keys) -join ', ')] and transport '$($receipt.transport)'"
            }
            if ($after -ne $before) {
                throw "the journal grew from $before to $after entries while run.ps1 could not start"
            }
            [ordered]@{ exit = $exit; firstFailure = $receipt.firstFailure; identity = @($receipt.identity.Keys); journalEntries = $after }
        }
        Invoke-Control -Run $Run -Name 'directories: a downloaded tree with an extra or misnamed entry is refused' -Refusal "the folder a real directory download of a real directory upload wrote passes as the definition's tree, and with an extra empty folder, an extra file or its empty folder under another name is refused" -Action {
            if (-not $uploadStep -or -not $downloadStep) {
                throw 'the scenario has no directory upload and download of one folder to control'
            }
            $namespace = Invoke-FolderAcquisition -Run $Run -Path $Run.Namespace -Label 'acquire namespace'
            $Run.Ownership.namespace.evidence = $namespace
            $Run.Ownership.namespace.owned = $namespace.owned
            if (-not $namespace.owned) {
                throw "the namespace '$($Run.Namespace)' was not acquired: status $($namespace.status), $($namespace.failure)"
            }
            [void] (Invoke-DirectoryUploadStep -Run $Run -Step $uploadStep)
            $download = Invoke-DirectoryDownloadStep -Run $Run -Step $downloadStep
            $local = $download.local
            $empty = @($downloadStep.tree.empty_directories)
            if (-not $empty.Count) {
                throw 'the downloaded tree has no empty folder to rename'
            }
            $refusals = [ordered]@{}
            $check = {
                param([string] $Case)
                try {
                    [void] (Assert-LocalTree -Run $Run -Step $downloadStep -Local $local)
                    $refusals[$Case] = $null
                }
                catch {
                    $refusals[$Case] = $_.Exception.Message
                }
            }
            $extraFolder = Join-Path -Path $local -ChildPath 'extra-empty'
            [void] [System.IO.Directory]::CreateDirectory($extraFolder)
            & $check 'an extra empty folder'
            Remove-Item -LiteralPath $extraFolder
            $extraFile = Join-Path -Path $local -ChildPath 'extra.bin'
            [FilesCom.ApiScenarios.Recipe]::WriteNewFile($extraFile, [byte[]] @(0))
            & $check 'an extra file'
            Remove-Item -LiteralPath $extraFile
            $emptyFolder = Join-Path -Path $local -ChildPath $empty[0]
            $renamed = "$emptyFolder-renamed"
            Move-Item -LiteralPath $emptyFolder -Destination $renamed
            & $check 'its empty folder under another name'
            Move-Item -LiteralPath $renamed -Destination $emptyFolder
            # Restored, the tree passes again.
            [void] (Assert-LocalTree -Run $Run -Step $downloadStep -Local $local)
            foreach ($case in @($refusals.Keys)) {
                if (-not $refusals[$case]) {
                    throw "the downloaded tree with $case was accepted"
                }
            }
            [ordered]@{ local = $local; tree = $downloadStep.tree; refusals = $refusals }
        }
        Invoke-Control -Run $Run -Name 'journal identity: an upload joined to another path, operation, upload or no part is refused' -Refusal "the real directory upload's own journal entries pass, and a begin of another path, a request the upload does not make, a finish naming another upload and a part naming no part are each refused" -Action {
            if (-not $uploadStep -or -not $Run.StepJournal.ContainsKey([int] $uploadStep.number)) {
                throw 'the directory upload did not run'
            }
            $check = $Run.StepJournal[[int] $uploadStep.number]
            $journal = Read-SimulatorJournal -Run $Run
            $pairs = @((Compare-JournalAccounting -Run $Run -Journal $journal -Observations @($Run.Transport.All())).Pairs)
            $label = Get-StepLabel -Step $uploadStep
            $entries = @($pairs | Where-Object { [string] $_.Observation.Step -ceq $label } | ForEach-Object { $_.Entry })
            $own = @(Get-IntentJournalProblem -Run $Run -Step $uploadStep -Entries $entries -Check $check)
            if ($own.Count) {
                throw "the directory upload's own journal entries were refused: $($own -join '; ')"
            }
            # The operations of the upload's begin and finish, as the upload's binding names them.
            $routes = $script:IntentBindings['upload-directory'].Routes
            $operationOf = {
                param([string] $Role)
                $operation = @($uploadStep.operations | Where-Object { $_.route -ceq $routes[$Role] })[0]
                [pscustomobject]@{ Id = Get-SimulatorOperationId -Run $Run -OperationId $operation.operation_id; Path = [string] $operation.path }
            }
            $begin = & $operationOf 'begin'
            $finish = & $operationOf 'finish'
            $begins = @($entries | Where-Object { [string] $_.operation -ceq $begin.Id })
            $finishes = @($entries | Where-Object { [string] $_.operation -ceq $finish.Id })
            $parts = @($entries | Where-Object { [string] $_.operation -ceq $Run.Transfers.UploadPart })
            $lookupId = Get-SimulatorOperationId -Run $Run -OperationId $Run.Protocol.Absent.operation.operation_id
            $lookup = @($pairs | Where-Object { [string] $_.Entry.operation -ceq $lookupId } | ForEach-Object { $_.Entry })[0]
            if (-not $begins.Count -or -not $finishes.Count -or -not $parts.Count -or -not $lookup) {
                throw "the journal holds $($begins.Count) begins, $($finishes.Count) finishes and $($parts.Count) parts of the directory upload, and $(if ($lookup) { 'a' } else { 'no' }) lookup, to change"
            }
            # The upload's entries with Original replaced by Copy.
            $replace = {
                param($Original, $Copy)
                , @($entries | ForEach-Object { if ([object]::ReferenceEquals($_, $Original)) { $Copy } else { $_ } })
            }
            $cases = [ordered]@{}
            $moved = Copy-JournalEntry -Entry $begins[0]
            $moved.path = $Run.ApiPrefix + $begin.Path.Replace('{path}', [System.Uri]::EscapeDataString("$($Run.RunFolder)/unrelated.bin"))
            $cases['a begin of another path'] = [pscustomobject]@{ Entries = (& $replace $begins[0] $moved); Problem = '*unrelated.bin*' }
            $cases['a request the upload does not make'] = [pscustomobject]@{ Entries = @($entries) + @($lookup); Problem = '*which the upload-directory does not make*' }
            $other = @($begins | ForEach-Object { [string] $_.upload } | Where-Object { $_ -cne [string] $finishes[0].upload })[0]
            $mismatched = Copy-JournalEntry -Entry $finishes[0]
            $mismatched.upload = if ($other) { $other } else { "$($finishes[0].upload)0" }
            $cases['a finish naming another upload'] = [pscustomobject]@{ Entries = (& $replace $finishes[0] $mismatched); Problem = '*finished upload*' }
            $partless = Copy-JournalEntry -Entry $parts[0]
            $partless.Remove('part')
            $cases['a part naming no part'] = [pscustomobject]@{ Entries = (& $replace $parts[0] $partless); Problem = '*without naming both its upload and its part*' }
            $refusals = [ordered]@{}
            foreach ($case in @($cases.Keys)) {
                $problems = @(Get-IntentJournalProblem -Run $Run -Step $uploadStep -Entries $cases[$case].Entries -Check $check)
                if (-not @($problems | Where-Object { $_ -like $cases[$case].Problem }).Count) {
                    throw "the journal check did not refuse $($case): $($problems -join '; ')"
                }
                $refusals[$case] = $problems
            }
            [ordered]@{ entries = $entries.Count; refusals = $refusals }
        }
    }
    catch {
        if (-not $Run.Failure) {
            $Run.Failure = $_.Exception.Message
        }
    }
    finally {
        Start-CleanupPhase -Run $Run
        try {
            if ($Run.Unsettled) {
                # Nothing more runs while a command has not settled: what the run owns stays, unresolved.
                Invoke-ScenarioCleanup -Run $Run
                foreach ($path in $made) {
                    $Run.Unresolved.Add($path)
                }
            }
            elseif ($Run.Ownership.run.owned) {
                # Deepest first: the namespace recursively, then what the controls made, in reverse, without recursion and
                # only when a lookup finds it, then the run folder once nothing in it is unresolved.
                $targets = [System.Collections.Generic.List[object]]::new()
                if ($Run.Ownership.namespace.owned -and -not $Run.Ownership.namespace.removed) {
                    $targets.Add([ordered]@{ path = $Run.Namespace; recursive = $true; role = 'namespace' })
                }
                $children = $made.ToArray()
                [System.Array]::Reverse($children)
                foreach ($path in $children) {
                    $targets.Add([ordered]@{ path = $path; recursive = $false; role = $null })
                }
                $targets.Add([ordered]@{ path = $Run.RunFolder; recursive = $false; role = 'run' })
                $next = 0
                try {
                    for (; $next -lt $targets.Count; $next++) {
                        $target = $targets[$next]
                        if ($target.role -ceq 'run' -and $Run.Unresolved.Count) {
                            $Run.Unresolved.Add($target.path)
                            continue
                        }
                        if (-not $target.role -and -not (Test-RemotePresent -Run $Run -Path $target.path -Label 'cleanup control lookup')) {
                            continue
                        }
                        $label = if ($target.role) { "cleanup $($target.role)" } else { 'cleanup control' }
                        $removal = Invoke-OwnedRemoval -Run $Run -Path $target.path -Recursive $target.recursive -Label $label
                        $Run.Cleanup.Add($removal)
                        if (-not $removal.absent) {
                            $Run.Unresolved.Add($target.path)
                        }
                        elseif ($target.role) {
                            $Run.Ownership[$target.role].removed = $true
                        }
                    }
                }
                catch [System.TimeoutException] {
                    # Refused for the cleanup's deadline before it was sent: nothing more is sent, and this target and every
                    # one after it stay unresolved.
                    if (-not $Run.CleanupFailure) {
                        $Run.CleanupFailure = "cleanup stopped: $($_.Exception.Message)"
                    }
                    for (; $next -lt $targets.Count; $next++) {
                        $Run.Unresolved.Add($targets[$next].path)
                    }
                }
            }
        }
        catch {
            $Run.CleanupFailure = "cleanup failed: $($_.Exception.Message)"
        }
        try {
            Invoke-JournalBypassControl -Run $Run
        }
        catch {
            $Run.Journal = [ordered]@{ problems = @("the journal could not be joined: $($_.Exception.Message)") }
        }
        Close-RunWork -Run $Run
    }
}

# Sends one request to the simulator's API without the module's transport, then requires the strict join to refuse
# exactly that journal entry, and to find nothing else once it is set aside.
function Invoke-JournalBypassControl {
    param($Run)
    # After the cleanup, within the whole run's budget.
    $Run.Phase = 'journal'
    $route = $Run.Protocol.Absent.operation.path
    $address = "$($Run.Inputs.ServerUrl)$($Run.ApiPrefix)$($route.Replace('{path}', [System.Uri]::EscapeDataString($Run.RunFolder)))"
    Invoke-Control -Run $Run -Name 'journal: a request that bypasses the module transport fails the accounting' -Refusal 'the strict join reports the bypass request journal entry, and only it' -Action {
        $bypass = Invoke-WebRequest -Uri $address -Method Get -Headers @{ 'X-FilesAPI-Key' = $Run.Inputs.ApiKey } -TimeoutSec (Get-ControlTimeoutSeconds -Run $Run) -NoProxy -SkipHttpErrorCheck
        $journal = Read-SimulatorJournal -Run $Run
        $accounting = Compare-JournalAccounting -Run $Run -Journal $journal -Observations @(if ($Run.Transport) { $Run.Transport.All() })
        $last = @($journal.entries | Sort-Object -Property { [long] $_.seq })[-1]
        $expected = [System.Uri]::UnescapeDataString(([uri] $address).AbsolutePath)
        if ($last.method -cne 'GET' -or [System.Uri]::UnescapeDataString([string] $last.path) -cne $expected) {
            throw "the journal's last entry is $($last.method) $($last.path), not the bypass request"
        }
        if (@($accounting.Unmatched).Count -ne 1 -or [long] @($accounting.Unmatched)[0] -ne [long] $last.seq -or @($accounting.Problems).Count -ne 1) {
            throw "the join reported entries [$(@($accounting.Unmatched) -join ', ')] and $(@($accounting.Problems).Count) problems, not the bypass entry $($last.seq) alone"
        }
        $Run.ExcludedEntries.Add([long] $last.seq)
        [ordered]@{ status = [int] $bypass.StatusCode; entry = [long] $last.seq; problems = @($accounting.Problems) }
    }
    [void] (Invoke-JournalAccounting -Run $Run)
}

# Runs the controls on the first scenario with both a directory upload and a directory download, and writes their
# receipt, a failed one too when they cannot start, with the first cause.
function Invoke-ControlsMain {
    param([hashtable] $Paths)
    $run = New-ScenarioRun -Paths $Paths
    $run.Controls = [System.Collections.Generic.List[object]]::new()
    $result = 'failed'
    try {
        $manifest = Read-JsonFile -Path $Paths.CasesPath
        $case = @($manifest.scenarios | Where-Object {
                $intents = @($_.steps | Where-Object { $_.kind -ceq 'intent' } | ForEach-Object { $_.intent })
                'upload-directory' -cin $intents -and 'download-directory' -cin $intents
            })[0]
        if (-not $case) {
            throw 'cases.json has no scenario with both a directory upload and a directory download for the controls to run'
        }
        Initialize-ScenarioRun -Run $run -CaseId ([string] $case.id)
        Invoke-ControlRun -Run $run
    }
    catch {
        if (-not $run.Failure) {
            $run.Failure = if ($run.Phase -ceq 'setup') { "the controls could not start: $($_.Exception.Message)" } else { $_.Exception.Message }
        }
    }
    finally {
        $failed = @($run.Controls | Where-Object { $_.result -cne 'passed' } | ForEach-Object { $_.name })
        if ($failed.Count -and -not $run.Failure) {
            $run.Failure = "controls failed: $($failed -join '; ')"
        }
        $result = Write-ScenarioReceipt -Run $run -Name 'controls.json' -Extra @{ kind = 'controls'; controls = $run.Controls.ToArray(); positions = 'not applicable: the controls run no scenario position' }
    }
    Write-Information -MessageData "Controls $result." -InformationAction Continue
    if ($result -ceq 'passed') { 0 } else { 1 }
}

$exitCode = Invoke-ControlsMain -Paths $controlPaths
exit $exitCode
