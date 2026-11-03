<#
Catalogue journeys through the installed FilesCom PowerShell module, run against a Files.com API simulator.

    FILES_SDK_ROOT=/path/to/Modules/FilesCom/1.0.0 FILES_MOCK_URL=http://127.0.0.1:PORT pwsh -NoProfile -NonInteractive -File journeys.ps1

The module is imported from FILES_SDK_ROOT as it is installed; nothing is installed or changed. Each journey resets the
simulator, drives only the module's public commands, and checks the simulator's journal, stored bytes and state through
its control API, independently of the module's own answers. Prints one JSON document, { package, powershell, results,
epochs }, with results mapping a catalogue row ID to "pass" or "FAIL: <reason>". Journeys cover what the module
expresses on its pinned engine; rows it cannot express are not reported. Keys with a colon, such as "C39:copy-move", are
exploratory evidence for part of a row and bind no lane. The pinned engine asks begin_upload for several part URLs at
once (parts) when a parallel upload needs them, which the simulator does not model; parallel journeys therefore send one
part at a time, and the rows that need parts in flight together are not reported.

Every simulator epoch a journey uses is sealed before it can be reset away and before the journey passes: once its
postconditions hold, its journal must be complete and every fault it was given consumed, in the epoch the journey reset
it into. epochs keeps each sealed epoch once, with the rows of its journey: the simulator's journal and faults answers
whole, exactly as it gave them, with the SHA-256 of each answer's bytes, whether or not the checks then held. Both
answers are bounded by the simulator (its journal limit, 100 fault rules, 1,000 schedule entries a rule) and hold no
credential: the simulator numbers identities and never stores them. A journey whose connections cannot be closed
afterwards fails.

Optional: FILES_MOCK_PEER_URL, a second simulator for redirect and region journeys; FILES_CATALOGUE_DECIMAL=1 when the
simulator's schema declares decimal request parameters; FILES_CATALOGUE_ADAPTIVE=1 when it declares the adaptive upload
fields; FILES_CATALOGUE_JOURNEYS, a comma-separated list of the rows to run.

FILES_CATALOGUE_DEADLINE_MS, the Unix time in milliseconds by which the adapter must have finished, is the adapter's
part of a whole-run deadline its caller owns. The adapter then starts no journey with less than 10 seconds left and
reports it as not started, bounds each simulator request by the time left and each module operation by the time left
when its connection was made (a per-command timeout, not a cumulative one), and fails any journey that ends after the
deadline; its caller still has to stop it, and the simulators, when the deadline passes.
#>
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
# Standard output carries only the final JSON document: nothing else may reach the host.
$ProgressPreference = 'SilentlyContinue'
$WarningPreference = 'SilentlyContinue'
$InformationPreference = 'SilentlyContinue'

$root = $env:FILES_SDK_ROOT
$base = "$env:FILES_MOCK_URL".TrimEnd('/')
$peer = "$env:FILES_MOCK_PEER_URL".TrimEnd('/')
$decimals = $env:FILES_CATALOGUE_DECIMAL -eq '1'
$adaptive = $env:FILES_CATALOGUE_ADAPTIVE -eq '1'
$selected = if ($env:FILES_CATALOGUE_JOURNEYS) { $env:FILES_CATALOGUE_JOURNEYS -split ',' } else { $null }
if (-not $root -or -not $base) {
    [Console]::Error.WriteLine('Set FILES_SDK_ROOT to the installed FilesCom module folder and FILES_MOCK_URL to the simulator')
    exit 2
}
$manifest = Join-Path $root 'FilesCom.psd1'
Import-Module -Name $manifest -Force

$http = [System.Net.Http.HttpClient]::new()
$http.Timeout = [System.Threading.Timeout]::InfiniteTimeSpan
$scratch = (New-Item -ItemType Directory -Path (Join-Path ([IO.Path]::GetTempPath()) "files-catalogue-ps-$([guid]::NewGuid().ToString('N'))")).FullName
$results = [ordered]@{}
$epochs = [System.Collections.Generic.List[object]]::new()
# The origins the running journey has reset, each with the epoch it reset it into, and what its sealed epochs held.
$script:unsealed = [ordered]@{}
$script:sealed = [System.Collections.Generic.List[object]]::new()
$script:journeyRows = @()
$deadline = if ($env:FILES_CATALOGUE_DEADLINE_MS) { [DateTimeOffset]::FromUnixTimeMilliseconds([long]$env:FILES_CATALOGUE_DEADLINE_MS) } else { $null }
$journeyReserve = [TimeSpan]::FromSeconds(10)

# The time left before the deadline, at most the limit given; the limit when there is no deadline.
function Get-TimeLeft {
    param([TimeSpan] $Limit)
    if (-not $deadline) {
        return $Limit
    }
    $left = $deadline - [DateTimeOffset]::UtcNow
    if ($left -le [TimeSpan]::Zero) {
        throw 'the run''s deadline has passed'
    }
    if ($left -lt $Limit) { $left } else { $Limit }
}

# The simulator's control API and raw protocol, without the module.
function Send-Raw {
    param([string] $Method, [string] $Url, [object] $Body, [byte[]] $Bytes)
    $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Method), $Url)
    if ($null -ne $Bytes) {
        $request.Content = [System.Net.Http.ByteArrayContent]::new($Bytes)
    }
    elseif ($null -ne $Body) {
        $request.Content = [System.Net.Http.StringContent]::new(($Body | ConvertTo-Json -Depth 20 -Compress), [Text.Encoding]::UTF8, 'application/json')
    }
    $cancel = [System.Threading.CancellationTokenSource]::new((Get-TimeLeft ([TimeSpan]::FromSeconds(60))))
    try {
        $response = $http.SendAsync($request, $cancel.Token).GetAwaiter().GetResult()
        $data = $response.Content.ReadAsByteArrayAsync($cancel.Token).GetAwaiter().GetResult()
    }
    finally {
        $cancel.Dispose()
    }
    [pscustomobject]@{ Status = [int]$response.StatusCode; Headers = $response.Headers; Bytes = $data; Text = [Text.Encoding]::UTF8.GetString($data) }
}

function Invoke-Control {
    param([string] $Method, [string] $Name, [object] $Body, [string] $Origin = $base)
    $answer = Send-Raw -Method $Method -Url "$Origin/__files_mock/v1/$Name" -Body $Body
    if ($answer.Status -ge 300) {
        throw "$Method ${Name}: $($answer.Status) $($answer.Text)"
    }
    $answer.Text | ConvertFrom-Json -Depth 30
}

# Starts a new epoch of the simulator at Origin, sealing the one this journey used there before.
function Reset-Simulator {
    param([hashtable] $Fixtures = @{}, [hashtable] $SimulatorProfile, [string] $Origin = $base)
    if ($script:unsealed.Contains($Origin)) {
        Close-Epoch -Origin $Origin
    }
    $body = @{ fixtures = $Fixtures }
    if ($SimulatorProfile) {
        $body.profile = $SimulatorProfile
    }
    $script:unsealed[$Origin] = (Invoke-Control -Method POST -Name reset -Body $body -Origin $Origin).epoch
}

# Seals the epoch this journey reset Origin into, once its postconditions have held: it must still be that epoch, its
# journal must be complete, every fault rule must have been consumed and every random rule must have failed a request.
# Its journal and faults answers are kept whole in the epoch's record before anything can reset them, and the record is
# kept whether or not the checks then hold, or the answers can be read at all.
function Close-Epoch {
    param([string] $Origin)
    $epoch = $script:unsealed[$Origin]
    $script:unsealed.Remove($Origin)
    $name = if ($Origin -eq $base) { 'primary' } else { 'peer' }
    $record = [ordered]@{ rows = $script:journeyRows; simulator = $name; epoch = $epoch }
    $script:sealed.Add($record)
    $journal = Save-Answer -Record $record -Name journal -Origin $Origin
    $faults = Save-Answer -Record $record -Name faults -Origin $Origin
    $entries = @($journal.entries)
    $operations = [ordered]@{}
    foreach ($group in $entries | Group-Object operation | Sort-Object Name) {
        $operations[$group.Name] = $group.Count
    }
    $unfailed = @($faults.faults | Where-Object { $null -ne $_.PSObject.Properties['random'] -and $null -ne $_.random -and $_.random.faulted -lt 1 })
    $record.summary = [ordered]@{
        journal_epoch = $journal.epoch; entries = $entries.Count; dropped = $journal.dropped; complete = $journal.complete; operations = $operations
        faults_epoch = $faults.epoch; faults_consumed = $faults.consumed; faults_pending = $faults.pending; random_rules = $faults.random; random_rules_unfailed = $unfailed.Count
    }
    Assert-True ($journal.epoch -eq $epoch -and $faults.epoch -eq $epoch) "the $name simulator left epoch $epoch for $($journal.epoch) before it was sealed"
    Assert-True ($journal.complete -eq $true) "the $name journal of epoch $epoch is incomplete: $($journal.dropped) entries were dropped"
    Assert-True ($faults.pending -eq 0) "the $name simulator's epoch $epoch has $($faults.pending) fault rule(s) never consumed"
    Assert-True ($unfailed.Count -eq 0) "the $name simulator's epoch $epoch has $($unfailed.Count) random rule(s) that failed no request"
}

# Reads one of the simulator's control answers into an epoch's record: its status, its exact text and the SHA-256 of its
# bytes, which anyone can recompute from that text as UTF-8. A failed read is recorded before its error is raised.
function Save-Answer {
    param([System.Collections.IDictionary] $Record, [string] $Name, [string] $Origin)
    try {
        $answer = Send-Raw -Method GET -Url "$Origin/__files_mock/v1/$Name"
    }
    catch {
        $Record["${Name}_error"] = $_.Exception.Message
        throw
    }
    $digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($answer.Bytes)).ToLowerInvariant()
    $Record["${Name}_status"] = $answer.Status
    $Record["${Name}_sha256"] = $digest
    $Record["${Name}_answer"] = $answer.Text
    Assert-True ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($answer.Text))).ToLowerInvariant() -eq $digest) "the $Name answer is not UTF-8 text"
    Assert-True ($answer.Status -lt 300) "GET ${Name}: $($answer.Status) $($answer.Text)"
    $answer.Text | ConvertFrom-Json -Depth 30
}

function Add-Fault {
    param([hashtable] $Rule)
    Invoke-Control -Method POST -Name faults -Body $Rule | Out-Null
}

# The journal since the last reset; an incomplete journal is missing evidence.
function Get-Journal {
    param([string] $Origin = $base)
    $journal = Invoke-Control -Method GET -Name journal -Origin $Origin
    if ($journal.complete -ne $true) {
        throw 'the journal is incomplete'
    }
    @($journal.entries)
}

function Select-Operation {
    param([object[]] $Entries, [string] $Operation)
    @($Entries | Where-Object { $_.operation -eq $Operation })
}

function Get-Encoded {
    param([string] $RemotePath)
    ($RemotePath -split '/' | ForEach-Object { [uri]::EscapeDataString($_) }) -join '/'
}

# Stores a file through the upload protocol, without the module.
function Save-Raw {
    param([string] $RemotePath, [byte[]] $Data)
    $begin = Send-Raw -Method POST -Url "$base/api/rest/v1/file_actions/begin_upload/$(Get-Encoded $RemotePath)" -Body @{}
    $part = @($begin.Text | ConvertFrom-Json)[0]
    $stored = Send-Raw -Method PUT -Url $part.upload_uri -Bytes $Data
    $etag = "$($stored.Headers.ETag)".Trim('"')
    $finished = Send-Raw -Method POST -Url "$base/api/rest/v1/files/$(Get-Encoded $RemotePath)" -Body @{ action = 'end'; ref = $part.ref; etags = @(@{ etag = $etag; part = 1 }) }
    Assert-Equal $finished.Status 201 -Message "stored $RemotePath"
}

# The bytes the simulator holds, fetched without the module.
function Get-Stored {
    param([string] $RemotePath)
    $file = (Send-Raw -Method GET -Url "$base/api/rest/v1/files/$(Get-Encoded $RemotePath)").Text | ConvertFrom-Json
    , (Send-Raw -Method GET -Url $file.download_uri).Bytes
}

function Get-Held {
    (Invoke-Control -Method GET -Name ready).state
}

# Deterministic bytes that differ at every position: each 32-byte block is the SHA-256 of the seed and the block's
# number, so a block swapped, repeated, shifted or left out changes the content. Zeros and bytes that are not UTF-8
# occur throughout; a size that is not a multiple of 32 ends in part of a block.
function New-TestContent {
    param([int] $Size, [string] $Seed = 'files-catalogue')
    $bytes = [byte[]]::new($Size)
    $prefix = [Text.Encoding]::UTF8.GetBytes("$Seed/")
    $block = [byte[]]::new($prefix.Length + 8)
    [Array]::Copy($prefix, $block, $prefix.Length)
    for ($number = 0L; $number * 32 -lt $Size; $number++) {
        [BitConverter]::GetBytes($number).CopyTo($block, $prefix.Length)
        $digest = [Security.Cryptography.SHA256]::HashData($block)
        [Array]::Copy($digest, 0, $bytes, $number * 32, [Math]::Min(32, $Size - $number * 32))
    }
    , $bytes
}

function Write-LocalFile {
    param([string] $Name, [byte[]] $Data)
    $path = Join-Path $scratch $Name
    [IO.File]::WriteAllBytes($path, $Data)
    $path
}

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) {
        throw $Message
    }
}

# A value as JSON with the members of every object in ordinal order: the journal's members come in no set order.
function ConvertTo-Canonical {
    param($Value)
    $parsed = ConvertFrom-Json -InputObject (ConvertTo-Json -InputObject $Value -Depth 20 -Compress) -AsHashtable -NoEnumerate
    ConvertTo-Json -InputObject (Get-Sorted $parsed) -Depth 20 -Compress
}

function Get-Sorted {
    param($Value)
    if ($Value -is [System.Collections.IDictionary]) {
        $keys = [string[]]@($Value.Keys)
        [Array]::Sort($keys, [StringComparer]::Ordinal)
        $sorted = [ordered]@{}
        foreach ($key in $keys) {
            $sorted[$key] = Get-Sorted $Value[$key]
        }
        return $sorted
    }
    if ($Value -is [System.Collections.IList]) {
        return , @(foreach ($item in $Value) { Get-Sorted $item })
    }
    $Value
}

function Assert-Equal {
    param($Actual, $Expected, [string] $Message)
    $left = ConvertTo-Canonical $Actual
    $right = ConvertTo-Canonical $Expected
    if ($left -cne $right) {
        throw "${Message}: expected $right, got $left"
    }
}

function Assert-SameContent {
    param([byte[]] $Actual, [byte[]] $Expected, [string] $Message)
    Assert-True ($null -ne $Actual) "${Message}: no bytes"
    $a = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Actual))
    $e = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Expected))
    if ($Actual.Length -ne $Expected.Length -or $a -ne $e) {
        throw "${Message}: $($Actual.Length) bytes with SHA-256 $a, expected $($Expected.Length) with $e"
    }
}

# The failure a command raises, or null.
function Get-Failure {
    param([scriptblock] $Command)
    try {
        & $Command | Out-Null
        $null
    }
    catch {
        $_
    }
}

function Connect-Simulator {
    param([string] $ApiKey = 'sentinel-ps-key-a', [hashtable] $Options = @{}, [string] $Origin = $base)
    $arguments = @{ BaseUrl = $Origin; NoDefault = $true; RetryDelaySeconds = 0.01; MaxRetryDelaySeconds = 0.05 }
    if ($ApiKey) {
        $arguments.ApiKey = ConvertTo-SecureString -String $ApiKey -AsPlainText -Force
    }
    if ($deadline) {
        # Every module operation ends by the deadline, less the time left to seal and clean up.
        $arguments.OperationTimeout = [TimeSpan]::FromSeconds([Math]::Max(1, ((Get-TimeLeft ([TimeSpan]::MaxValue)) - $journeyReserve).TotalSeconds))
    }
    foreach ($option in $Options.Keys) {
        $arguments[$option] = $Options[$option]
    }
    Connect-FilesAccount @arguments
}

function Invoke-Journey {
    param([string[]] $Ids, [scriptblock] $Body)
    if ($selected -and -not ($Ids | Where-Object { $selected -contains $_ })) {
        return
    }
    if ($deadline -and ($deadline - [DateTimeOffset]::UtcNow) -lt $journeyReserve) {
        foreach ($id in $Ids) {
            $results[$id] = 'FAIL: not started: the run''s deadline leaves no time for it'
        }
        return
    }
    $script:unsealed = [ordered]@{}
    $script:sealed = [System.Collections.Generic.List[object]]::new()
    $script:journeyRows = @($Ids)
    $failure = $null
    try {
        & $Body
        foreach ($origin in @($script:unsealed.Keys)) {
            Close-Epoch -Origin $origin
        }
    }
    catch {
        $failure = "line $($_.InvocationInfo.ScriptLineNumber): $($_.Exception.Message)"
        # Keep what the failed journey's epochs held too, and say what else was wrong with them.
        foreach ($origin in @($script:unsealed.Keys)) {
            try {
                Close-Epoch -Origin $origin
            }
            catch {
                $failure += "; then: $($_.Exception.Message)"
            }
        }
    }
    try {
        Get-FilesConnection -All | Disconnect-FilesAccount -Confirm:$false
    }
    catch {
        $failure = (@($failure) + "the journey's connections could not be closed: $($_.Exception.Message)" | Where-Object { $_ }) -join '; '
    }
    if (-not $failure -and $deadline -and [DateTimeOffset]::UtcNow -gt $deadline) {
        $failure = 'it ended after the run''s deadline'
    }
    foreach ($id in $Ids) {
        $results[$id] = if ($failure) { "FAIL: $failure" } else { 'pass' }
    }
    $epochs.AddRange($script:sealed)
}

# The loaded package: the module's name and version, and the SHA-256 of every module file the run loaded or read, by
# its path under the module folder; with the engine and schema its build records.
function Get-Identity {
    $real = (Resolve-Path -LiteralPath $root).ProviderPath.TrimEnd([IO.Path]::DirectorySeparatorChar)
    $modules = [ordered]@{}
    $loaded = @([AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { -not $_.IsDynamic -and $_.Location } | ForEach-Object Location)
    $files = @($loaded + (Join-Path $real 'FilesCom.psd1') + (Join-Path $real 'FilesCom.BuildInfo.json')) | Where-Object { $_.StartsWith($real + [IO.Path]::DirectorySeparatorChar) } | Sort-Object -Unique
    foreach ($file in $files) {
        $relative = [IO.Path]::GetRelativePath($real, $file).Replace('\', '/')
        $modules[$relative] = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $build = Get-Content -LiteralPath (Join-Path $real 'FilesCom.BuildInfo.json') -Raw | ConvertFrom-Json
    $module = Import-PowerShellDataFile -LiteralPath (Join-Path $real 'FilesCom.psd1')
    [ordered]@{ name = 'FilesCom'; version = $module.ModuleVersion; root = $real; modules = $modules; engine = $build.engine; schema_sha256 = $build.schemaSha256 }
}

try {
    Invoke-Journey 'SIM-LIFECYCLE-001', 'MOCK-REC-002' {
        Reset-Simulator
        $c = Connect-Simulator
        $alice = New-FilesUser -Username alice -Name Alice -Connection $c -Confirm:$false
        $bob = New-FilesUser -Username bob -Connection $c -Confirm:$false
        Assert-Equal @($alice.Id, $bob.Id) @(1, 2) -Message 'ids'
        $found = Get-FilesUser -Id 1 -Connection $c
        Assert-Equal @($found.Username, $found.Name) @('alice', 'Alice') -Message 'found'
        $found | Set-FilesUser -Name 'Alice B' -Confirm:$false | Out-Null
        Assert-Equal (Get-FilesUser -Id 1 -Connection $c).Name 'Alice B' -Message 'updated'
        $bob | Remove-FilesUser -Confirm:$false
        $gone = Get-Failure { Get-FilesUser -Id 2 -Connection $c }
        Assert-True ($gone -and $gone.FullyQualifiedErrorId -like 'Files.NotFound*') "a deleted user is not found: $gone"
        Assert-Equal (New-FilesUser -Username carol -Connection $c -Confirm:$false).Id 3 -Message 'ids are not reused'
        $wire = (Select-Operation (Get-Journal) 'users.create')[0].wire
        Assert-Equal $wire ([ordered]@{ username = 'string'; name = 'string' }) -Message 'wire types'
    }

    Invoke-Journey 'SIM-PAGE-001', 'GOL-PAGE-001' {
        Reset-Simulator -Fixtures @{ users = @(1..5 | ForEach-Object { @{ username = "user$_" } }) }
        $c = Connect-Simulator
        $listed = @(Get-FilesUser -PerPage 2 -Connection $c | ForEach-Object Username)
        Assert-Equal $listed @('user1', 'user2', 'user3', 'user4', 'user5') -Message 'every user once'
        $pages = Select-Operation (Get-Journal) 'users.list'
        Assert-Equal $pages.Count 3 -Message 'pages'
        for ($i = 1; $i -lt $pages.Count; $i++) {
            Assert-Equal $pages[$i].cursor_sha256 $pages[$i - 1].next_cursor_sha256 -Message 'each page sends the cursor the one before returned'
        }
        Assert-True ($null -eq $pages[2].PSObject.Properties['next_cursor_sha256'] -or $null -eq $pages[2].next_cursor_sha256) 'the last page has no cursor'
        $page = Get-FilesUser -PerPage 2 -Page -Connection $c
        $manual = @($page.Items.Username)
        while ($page.Continuation) {
            $page = Get-FilesUser -Continuation $page.Continuation
            $manual += @($page.Items.Username)
        }
        Assert-Equal $manual $listed -Message 'manual pages'
    }

    # The rest of GOL-PAGE-002, a repeated cursor ending with an error, is not the pinned engine's: it ends the listing
    # when an empty page repeats its cursor.
    Invoke-Journey 'GOL-PAGE-002:advancing' {
        Reset-Simulator -Fixtures @{ users = @(1..3 | ForEach-Object { @{ username = "user$_" } }) }
        Add-Fault @{ operation = 'users.list'; match = @{ continuation = $true }; kind = 'empty_page' }
        Assert-Equal @(Get-FilesUser -PerPage 2 -Connection (Connect-Simulator) | ForEach-Object Username) @('user1', 'user2', 'user3') -Message 'an empty page with a new cursor does not end the listing'
        Assert-Equal @((Select-Operation (Get-Journal) 'users.list').status) @(200, 200, 200) -Message 'three pages'
    }

    Invoke-Journey 'GOL-ERR-001', 'MOCK-FAULT-001' {
        Reset-Simulator -Fixtures @{ users = @(@{ username = 'alice' }) }
        $c = Connect-Simulator -Options @{ MaxNetworkRetries = 0 }
        Add-Fault @{ operation = 'users.find'; status = 404; type = 'not-found'; message = 'sentinel missing object' }
        $injected = Get-Failure { Get-FilesUser -Id 1 -Connection $c }
        Assert-True ($injected.FullyQualifiedErrorId -like 'Files.NotFound*') "injected 404: $injected"
        Add-Fault @{ operation = 'users.find'; status = 422; type = 'not-found'; message = 'sentinel missing object' }
        $other = Get-Failure { Get-FilesUser -Id 1 -Connection $c }
        Assert-True ($other.FullyQualifiedErrorId -like 'Files.NotFound*') "the type, not the status, classifies: $other"
        Assert-Equal @($other.Exception.ErrorType, $other.Exception.HttpStatus) @('not-found', 422) -Message 'the type and status are kept'
        $missing = Get-Failure { Get-FilesUser -Id 99 -Connection $c }
        Assert-True ($missing.FullyQualifiedErrorId -like 'Files.NotFound*') "a real 404: $missing"
    }

    # The rest of MOCK-FAULT-002, the storage answers, waits for the next engine: the pinned engine saves the web frontend
    # page a download is answered with as the file (C37), and does not renew a URL a part is refused as expired with
    # (GOL-XFER-004).
    Invoke-Journey 'MOCK-FAULT-002:api-unstructured' {
        Reset-Simulator
        Add-Fault @{ operation = 'users.list'; kind = 'unstructured'; status = 502 }
        $gateway = Get-Failure { Get-FilesUser -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 0 }) }
        Assert-True ($null -ne $gateway -and $gateway.FullyQualifiedErrorId -like 'Files.*' -and $gateway.FullyQualifiedErrorId -notlike 'Files.NotFound*') "a non-JSON failure is a module error: $gateway"
        Assert-Equal @((Select-Operation (Get-Journal) 'users.list').status) @(502) -Message 'the web tier''s answer'
    }

    # GOL-ERR-002 also expects an explicit "request may have been processed" signal and an uncertain mutation sent
    # once, which the pinned engine does not give: it has no such signal, and it sends a create whose connection was
    # lost again within the retry budget. MOCK-FAULT-005 is reported with no retries; GOL-ERR-002 is not.
    Invoke-Journey 'MOCK-FAULT-005' {
        foreach ($case in @(@{ Kind = 'drop_before'; Created = 0 }, @{ Kind = 'drop_after'; Created = 1 })) {
            Reset-Simulator
            Add-Fault @{ operation = 'users.create'; kind = $case.Kind }
            $lost = Get-Failure { New-FilesUser -Username $case.Kind -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 0 }) -Confirm:$false }
            Assert-True ($lost.FullyQualifiedErrorId -like 'Files.Connection*') "$($case.Kind): $lost"
            Assert-Equal (Select-Operation (Get-Journal) 'users.create').Count 1 -Message "$($case.Kind): sent once"
            Assert-Equal (Get-Held).users $case.Created -Message "$($case.Kind): users held"
        }
    }

    Invoke-Journey 'GOL-RETRY-001' {
        Reset-Simulator -Fixtures @{ users = @(@{ username = 'alice' }) }
        Add-Fault @{ operation = 'users.find'; status = 503; random = @{ seed = 'retry'; rate = 1; max_faults = 2 } }
        Assert-Equal (Get-FilesUser -Id 1 -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 3 })).Username 'alice' -Message 'retried to success'
        Assert-Equal @((Select-Operation (Get-Journal) 'users.find').status) @(503, 503, 200) -Message 'two retries within the budget'
        Reset-Simulator -Fixtures @{ users = @(@{ username = 'alice' }) }
        Add-Fault @{ operation = 'users.find'; status = 503 }
        Assert-True ($null -ne (Get-Failure { Get-FilesUser -Id 1 -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 0 }) })) 'a zero budget fails'
        Assert-Equal @((Select-Operation (Get-Journal) 'users.find').status) @(503) -Message 'a zero budget sends once'
    }

    # The rest of GOL-RETRY-002 is not observable on the simulator: a request whose connection fails for bad TLS or an
    # invalid protocol never reaches its journal.
    Invoke-Journey 'GOL-RETRY-002:structured-and-redirects' {
        Reset-Simulator -Fixtures @{ users = @(@{ username = 'alice' }) }
        $retrying = Connect-Simulator -Options @{ MaxNetworkRetries = 3 }
        Add-Fault @{ operation = 'users.find'; status = 422; type = 'bad-request'; message = 'sentinel refused' }
        Assert-True ((Get-Failure { Get-FilesUser -Id 1 -Connection $retrying }).FullyQualifiedErrorId -like 'Files.*') 'a structured 422'
        Assert-True ((Get-Failure { Get-FilesUser -Id 99 -Connection $retrying }).FullyQualifiedErrorId -like 'Files.NotFound*') 'a 404'
        Assert-True ($null -ne (Get-Failure { Get-FilesUser -Search a -Connection $retrying })) 'filtering is not simulated'
        Assert-Equal @((Select-Operation (Get-Journal) 'users.find').status) @(422, 404) -Message 'neither 4xx was retried'
        Assert-Equal @((Select-Operation (Get-Journal) 'users.list').status) @(501) -Message 'the 501 was not retried'
        # A redirect loop fails at the redirect limit with as many requests whatever the retry budget.
        $looped = foreach ($budget in 0, 3) {
            Reset-Simulator -Fixtures @{ users = @(@{ username = 'alice' }) }
            Add-Fault @{ operation = 'users.list'; kind = 'redirect'; location = $base; random = @{ seed = 'loop'; rate = 1; max_faults = 100 } }
            $failure = Get-Failure { Get-FilesUser -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = $budget }) }
            Assert-True ($failure.FullyQualifiedErrorId -like 'Files.*') "budget ${budget}: the loop fails: $failure"
            $requests = Select-Operation (Get-Journal) 'users.list'
            Assert-True (@($requests | Where-Object status -NE 307).Count -eq 0) "budget ${budget}: every answer redirected"
            $requests.Count
        }
        Assert-True ($looped[0] -gt 1 -and $looped[0] -eq $looped[1]) "the redirects stop at the limit and are not sent again: $($looped -join ' and ') requests"
    }

    Invoke-Journey 'MOCK-FAULT-007' {
        # A seeded rule chooses the same keys whatever order their requests arrive in.
        $chosen = foreach ($order in @(1..8), @(8..1)) {
            Reset-Simulator -Fixtures @{ users = @(1..8 | ForEach-Object { @{ username = "user$_" } }) }
            Add-Fault @{ operation = 'users.find'; status = 503; random = @{ seed = 'replay'; rate = 0.5; key = @('id') } }
            $c = Connect-Simulator -Options @{ MaxNetworkRetries = 0 }
            foreach ($id in $order) {
                Get-Failure { Get-FilesUser -Id $id -Connection $c } | Out-Null
            }
            $requests = Select-Operation (Get-Journal) 'users.find'
            Assert-Equal @($requests.id) $order -Message 'one request per id, in order'
            , @($requests | Where-Object status -EQ 503 | ForEach-Object id | Sort-Object)
        }
        Assert-Equal $chosen[1] $chosen[0] -Message 'the same ids fail in either order'
        Assert-True ($chosen[0].Count -gt 0 -and $chosen[0].Count -lt 8) "some ids fail and some do not: $($chosen[0] -join ',')"
        # A budget is spent in arrival order, whatever the keys.
        Reset-Simulator -Fixtures @{ users = @(1..3 | ForEach-Object { @{ username = "user$_" } }) }
        Add-Fault @{ operation = 'users.find'; status = 503; random = @{ seed = 'budget'; rate = 1; key = @('id'); max_faults = 2 } }
        $c = Connect-Simulator -Options @{ MaxNetworkRetries = 0 }
        foreach ($id in 3, 1, 2) {
            Get-Failure { Get-FilesUser -Id $id -Connection $c } | Out-Null
        }
        Assert-Equal @(Select-Operation (Get-Journal) 'users.find' | ForEach-Object { "$($_.id):$($_.status)" }) @('3:503', '1:503', '2:200') -Message 'the first two arrivals fail'
        Assert-Equal @((Invoke-Control -Method GET -Name faults).faults[0].random.schedule.outcome) @('fault', 'fault', 'budget-exhausted') -Message 'the schedule'
        # A rule keyed on the session names it by its number and never reports it.
        Reset-Simulator
        Add-Fault @{ operation = 'users.list'; status = 503; random = @{ seed = 'session'; rate = 1; key = @('session'); max_faults = 1 } }
        $session = Connect-Simulator -ApiKey $null -Options @{ SessionId = (ConvertTo-SecureString -String 'sentinel-ps-session-7' -AsPlainText -Force); MaxNetworkRetries = 0 }
        Get-Failure { Get-FilesUser -Connection $session } | Out-Null
        $faults = Invoke-Control -Method GET -Name faults
        Assert-Equal $faults.faults[0].random.schedule[0].key ([ordered]@{ session = 1 }) -Message 'the schedule names the session by its number'
        Assert-Equal (Select-Operation (Get-Journal) 'users.list')[0].credentials ([ordered]@{ session = 1 }) -Message 'so does the journal'
        foreach ($text in (Send-Raw -Method GET -Url "$base/__files_mock/v1/journal").Text, (Send-Raw -Method GET -Url "$base/__files_mock/v1/faults").Text) {
            Assert-True (-not $text.Contains('sentinel-ps-session-7')) 'the session itself is never reported'
        }
    }

    Invoke-Journey 'GOL-AUTH-001', 'GOL-AUTH-002', 'GOL-WS-001', 'NET-XFER-03', 'MOCK-OBS-001' {
        Reset-Simulator
        $a = Connect-Simulator -ApiKey 'sentinel-ps-key-a' -Options @{ WorkspaceId = 123 }
        $b = Connect-Simulator -ApiKey 'sentinel-ps-key-b'
        $s = Connect-Simulator -ApiKey $null -Options @{ SessionId = (ConvertTo-SecureString -String 'sentinel-ps-session' -AsPlainText -Force) }
        $zero = Connect-Simulator -ApiKey 'sentinel-ps-key-a' -Options @{ WorkspaceId = 0 }
        foreach ($connection in $a, $b, $s, $a, $zero) {
            Get-FilesUser -Connection $connection | Out-Null
        }
        $entries = Select-Operation (Get-Journal) 'users.list'
        Assert-Equal @($entries.credentials) @([ordered]@{ api_key = 1; workspace_id = '123' }, [ordered]@{ api_key = 2 }, [ordered]@{ session = 3 }, [ordered]@{ api_key = 1; workspace_id = '123' }, [ordered]@{ api_key = 1; workspace_id = '0' }) -Message 'each request carries its own connection''s credential and workspace'
        Assert-True (@($entries | Where-Object { $_.user_agent -notmatch '^Files\.com DOTNET SDK v2\.0\.0-api\.6' }).Count -eq 0) "user agent: $($entries[0].user_agent)"
        # Independent connections sharing one limiter upload at the same time, each with its own credential and workspace.
        Reset-Simulator
        $limiter = New-FilesTransferLimiter -MaxConcurrentRequests 2 -MaxBufferedBytes 8MB
        $sources = @{ a = (Write-LocalFile 'limiter-a.bin' (New-TestContent 300000 -Seed a)); b = (Write-LocalFile 'limiter-b.bin' (New-TestContent 300001 -Seed b)) }
        $jobs = @(@{ Connection = $a; Local = $sources.a; Remote = 'shared/a.bin' }, @{ Connection = $b; Local = $sources.b; Remote = 'shared/b.bin' }) | ForEach-Object -Parallel {
            Import-Module -Name $using:manifest
            $upload = $_
            Send-FilesFile -LiteralPath $upload.Local -RemotePath $upload.Remote -Limiter $using:limiter -Connection $upload.Connection -PassThru -Confirm:$false
        } -ThrottleLimit 2
        Assert-Equal @($jobs.Path | Sort-Object) @('shared/a.bin', 'shared/b.bin') -Message 'both uploads completed'
        # Credentials are numbered by first appearance since the reset, and the uploads raced: each upload's requests
        # must carry one number, its own, with a's workspace only on a's. Read before the bytes are fetched here.
        $api = @((Get-Journal) | Where-Object { $_.operation -like 'files.*' })
        Assert-SameContent (Get-Stored 'shared/a.bin') (New-TestContent 300000 -Seed a) -Message 'a''s bytes'
        Assert-SameContent (Get-Stored 'shared/b.bin') (New-TestContent 300001 -Seed b) -Message 'b''s bytes'
        $fromA = @($api | Where-Object { [uri]::UnescapeDataString($_.path) -like '*/shared/a.bin' })
        $fromB = @($api | Where-Object { [uri]::UnescapeDataString($_.path) -like '*/shared/b.bin' })
        $credentialsA = @($fromA | ForEach-Object { ConvertTo-Canonical $_.credentials } | Sort-Object -Unique)
        $credentialsB = @($fromB | ForEach-Object { ConvertTo-Canonical $_.credentials } | Sort-Object -Unique)
        Assert-True ($credentialsA.Count -eq 1 -and $credentialsB.Count -eq 1) "each upload's requests carry one credential: a $($credentialsA -join ' '), b $($credentialsB -join ' ')"
        Assert-True ($fromA.Count -ge 2 -and $fromB.Count -ge 2 -and $fromA[0].credentials.api_key -ne $fromB[0].credentials.api_key) 'a''s and b''s credentials differ'
        Assert-Equal $fromA[0].credentials.workspace_id '123' -Message 'a''s workspace'
        Assert-True ($null -eq $fromB[0].credentials.PSObject.Properties['workspace_id']) 'b sends no workspace'
        $text = (Send-Raw -Method GET -Url "$base/__files_mock/v1/journal").Text
        foreach ($sentinel in 'sentinel-ps-key-a', 'sentinel-ps-key-b', 'sentinel-ps-session') {
            Assert-True (-not $text.Contains($sentinel)) "the journal holds no credential ($sentinel)"
        }
    }

    Invoke-Journey 'GOL-AUTH-003', 'GOL-WS-002' {
        Reset-Simulator -Fixtures @{ users = @(1..3 | ForEach-Object { @{ username = "user$_" } }) }
        $a = Connect-Simulator -ApiKey 'sentinel-ps-key-a' -Options @{ WorkspaceId = 123 }
        $b = Connect-Simulator -ApiKey 'sentinel-ps-key-b' -Options @{ WorkspaceId = 7 }
        Get-FilesUser -Connection $a | Out-Null
        $found = Get-FilesUser -Id 1 -Connection $b
        $found | Set-FilesUser -Name 'via b' -Confirm:$false | Out-Null
        Get-FilesUser -PerPage 2 -Connection $b | Out-Null
        Send-FilesFile -InputStream ([IO.MemoryStream]::new((New-TestContent 5))) -Length 5 -RemotePath 'context/b.bin' -Connection $b -Confirm:$false
        Get-FilesUser -Connection $a -WorkspaceScope 456 | Out-Null
        Get-FilesUser -Connection $a | Out-Null
        $api = @((Get-Journal) | Where-Object { $_.operation -notlike 'transfers.*' })
        Assert-Equal $api[0].credentials ([ordered]@{ api_key = 1; workspace_id = '123' }) -Message 'a''s first request'
        $fromB = @($api[1..($api.Count - 3)])
        Assert-True ($fromB.Count -ge 6 -and @($fromB | Where-Object { $_.credentials.api_key -ne 2 -or $_.credentials.workspace_id -ne '7' }).Count -eq 0) "b's object, pages and upload carry b's credential and workspace: $(($fromB | ForEach-Object { "$($_.operation)" }) -join ',')"
        Assert-Equal @($api[-2].credentials.workspace_id, $api[-1].credentials.workspace_id) @('456', '123') -Message 'a per-command workspace applies to that command only'
    }

    Invoke-Journey 'GOL-PATH-001' {
        Reset-Simulator
        $c = Connect-Simulator
        $paths = 'folder/item?recursive=true', 'a:b/file.txt', 'folder/a#b.txt', 'folder/100%25.txt', "folder/$([char]0xe9) +.txt"
        for ($i = 0; $i -lt $paths.Count; $i++) {
            $data = New-TestContent ($i + 3)
            Send-FilesFile -InputStream ([IO.MemoryStream]::new($data)) -Length $data.Length -RemotePath $paths[$i] -Connection $c -Confirm:$false
            Assert-SameContent (Get-Stored $paths[$i]) $data -Message "the bytes at $($paths[$i])"
            Assert-Equal (Get-FilesFile -Path $paths[$i] -Connection $c).Path $paths[$i] -Message 'the path comes back literally'
        }
        Assert-True (@((Get-Journal) | Where-Object { $_.path -like '*/null' }).Count -eq 0) 'no request for a null path'
    }

    Invoke-Journey 'GOL-PATH-002' {
        Reset-Simulator -Fixtures @{ groups = @('a', 'b', 'c' | ForEach-Object { @{ name = $_ } }) } -SimulatorProfile @{ list_cursors = 'special-characters' }
        $c = Connect-Simulator
        Assert-Equal @(Get-FilesGroup -PerPage 1 -Connection $c | ForEach-Object Name) @('a', 'b', 'c') -Message 'groups'
        $pages = Select-Operation (Get-Journal) 'groups.list'
        Assert-Equal @($pages.status) @(200, 200, 200) -Message 'no cursor was refused'
        for ($i = 1; $i -lt $pages.Count; $i++) {
            Assert-Equal $pages[$i].cursor_sha256 $pages[$i - 1].next_cursor_sha256 -Message 'each page sends the cursor the one before returned'
        }
        $scalar = "a+b &x=y%25#$([char]0xe9)"
        Reset-Simulator -Fixtures @{ responses = @{ 'automation_runs.find_node' = @{ "3/$scalar" = @{ node_id = $scalar } } } }
        Assert-Equal (Get-FilesAutomationRunNode -Id 3 -NodeId $scalar -Connection (Connect-Simulator)).NodeId $scalar -Message 'the exact scalar'
    }

    Invoke-Journey 'API-ATTACHMENTS-COMPANIONS-002' {
        Reset-Simulator
        $content = [byte[]](@(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A) + (New-TestContent 300))
        $c = Connect-Simulator
        $behavior = @{ Path = 'attach'; Behavior = 'webhook'; Value = @{ urls = @('https://example.test/hook'); method = 'POST' }; Recursive = $false; Description = ''; Connection = $c; Confirm = $false }
        # The module has no null for a text parameter: it refuses one before any request rather than leave it out.
        $nullName = Get-Failure { New-FilesBehavior @behavior -Name $null -AttachmentFile (New-FilesAttachment -Content $content -FileName 'n.png') }
        Assert-True ($nullName.FullyQualifiedErrorId -like 'ParameterArgumentValidationError*') "a null is refused: $nullName"
        Assert-Equal @(Get-Journal).Count 0 -Message 'nothing was sent'
        $created = New-FilesBehavior @behavior -AttachmentFile (New-FilesAttachment -Content $content -FileName 'a%2Fb\c.png' -ContentType image/png)
        Assert-Equal @($created.Path, $created.Recursive, $created.Description) @('attach', $false, '') -Message 'the companions came back'
        Assert-Equal $created.Value.urls @('https://example.test/hook') -Message 'the object companion came back'
        $creates = Select-Operation (Get-Journal) 'behaviors.create'
        Assert-Equal $creates.Count 1 -Message 'one mutation'
        Assert-Equal $creates[0].wire ([ordered]@{ path = 'string'; behavior = 'string'; value = 'object'; recursive = 'boolean'; description = 'string'; attachment_file = 'object' }) -Message 'wire types'
        $attachment = $creates[0].attachments[0]
        $digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($content)).ToLowerInvariant()
        Assert-Equal @($attachment.param, $attachment.raw_filename, $attachment.content_type, $attachment.size, $attachment.sha256, $attachment.encoding) @('attachment_file', 'a%2Fb\c.png', 'image/png', $content.Length, $digest, 'encoded_content') -Message 'the attachment'
        Assert-True (-not (Send-Raw -Method GET -Url "$base/__files_mock/v1/journal").Text.Contains([Convert]::ToBase64String($content).Substring(0, 40))) 'the bytes are not journaled'
    }

    Invoke-Journey 'API-ATTACHMENTS-001' {
        # A multipart file part from a borrowed stream's position, sent again unchanged when its request is retried.
        Reset-Simulator -Fixtures @{ users = @(@{ username = 'alice' }) }
        $bytes = New-TestContent 4096
        $stream = [IO.MemoryStream]::new($bytes)
        $stream.Position = 96
        Add-Fault @{ operation = 'users.update'; match = @{ id = 1 }; status = 503; retry_after = 0 }
        Set-FilesUser -Id 1 -Notes 'avatar' -AvatarFile (New-FilesAttachment -InputStream $stream -FileName 'ana b.png' -ContentType image/png) -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 1 }) -Confirm:$false | Out-Null
        $updates = Select-Operation (Get-Journal) 'users.update'
        Assert-Equal @($updates.status) @(503, 200) -Message 'the retried update'
        $expected = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([byte[]]$bytes[96..4095])).ToLowerInvariant()
        $sent = $updates[1].attachments[0]
        Assert-Equal @($sent.param, $sent.raw_filename, $sent.content_type, $sent.size, $sent.sha256, $sent.encoding) @('avatar_file', 'ana b.png', 'image/png', 4000, $expected, 'multipart') -Message 'the attachment'
        Assert-Equal $updates[1].wire.notes 'string' -Message 'the companion'
        Assert-True ($stream.CanRead) 'the borrowed stream is left open'
    }

    if ($peer) {
        Invoke-Journey 'GOL-AUTH-004' {
            Reset-Simulator
            Reset-Simulator -Origin $peer
            Add-Fault @{ operation = 'users.list'; kind = 'redirect'; location = $peer }
            Get-Failure { Get-FilesUser -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 0 }) } | Out-Null
            $arrived = Get-Journal -Origin $peer
            Assert-True ($arrived.Count -ge 1) 'the redirect was followed'
            Assert-True (@($arrived | Where-Object { $null -ne $_.PSObject.Properties['credentials'] -and $null -ne $_.credentials }).Count -eq 0) 'no credential crossed to the other origin'
        }

        Invoke-Journey 'C07:listed-pair' {
            # A listed-pair region move, from a fault naming the other simulator; the Automatic move stays a real gate.
            Reset-Simulator
            Reset-Simulator -Fixtures @{ users = @(@{ username = 'moved' }) } -Origin $peer
            Add-Fault @{ operation = 'users.find'; status = 403; type = 'not-authenticated/lockout-region-mismatch'; data = @{ host = $peer } }
            $c = Connect-Simulator -Options @{ MaxNetworkRetries = 1; RegionRelocation = @{ $base = $peer } }
            Assert-Equal (Get-FilesUser -Id 1 -Connection $c).Username 'moved' -Message 'answered by the region'
            Assert-Equal @((Get-Journal -Origin $peer).credentials.api_key) @(1) -Message 'the region got the connection''s credential once'
        }
    }

    if ($decimals) {
        Invoke-Journey 'GOL-TYPE-001' {
            Reset-Simulator
            $value = '12345678901234567890.1234567'
            $created = New-FilesRemoteMountBackend -CanaryFilePath c -RemoteServerMountId 1 -RemoteServerId 1 -MinFreeCpu $value -Connection (Connect-Simulator) -Confirm:$false
            Assert-Equal ([string]$created.MinFreeCpu) $value -Message 'returned exactly'
            Assert-Equal (Select-Operation (Get-Journal) 'remote_mount_backends.create')[0].wire.min_free_cpu 'string' -Message 'sent as a string'
            $held = (Send-Raw -Method GET -Url "$base/api/rest/v1/remote_mount_backends/$($created.Id)").Text
            Assert-True ($held.Contains("`"min_free_cpu`":`"$value`"")) $held
        }
    }

    Invoke-Journey 'GOL-XFER-001', 'SIM-BYTES-001', 'SIM-EMPTY-001', 'GOL-AUTH-004-STORAGE' {
        Reset-Simulator
        $c = Connect-Simulator
        foreach ($size in 0, 17, 1048576, 1048581) {
            $data = New-TestContent $size
            Send-FilesFile -LiteralPath (Write-LocalFile "basic-$size.bin" $data) -RemotePath "gollum/basic-$size.bin" -Connection $c -Confirm:$false
            Assert-SameContent (Get-Stored "gollum/basic-$size.bin") $data -Message "$size bytes"
        }
        $entries = Get-Journal
        Assert-Equal (Select-Operation $entries 'files.finalize_upload').Count 4 -Message 'one finalize per file'
        $operations = @($entries.operation)
        Assert-True ($operations.IndexOf('files.begin_upload') -lt $operations.IndexOf('transfers.upload_part')) 'begin_upload before parts'
        Assert-True (@(Select-Operation $entries 'transfers.upload_part' | Where-Object { $null -ne $_.PSObject.Properties['credentials'] -and $null -ne $_.credentials }).Count -eq 0) 'storage requests carry no credential'
        Assert-True (@(Select-Operation $entries 'files.begin_upload' | Where-Object { $_.credentials.api_key -ne 1 }).Count -eq 0) 'API requests carry the connection''s'
    }

    Invoke-Journey 'GOL-XFER-002' {
        Reset-Simulator
        foreach ($size in 0, 17, 1048576) {
            Save-Raw "gollum/down-$size.bin" (New-TestContent $size)
        }
        $c = Connect-Simulator
        foreach ($size in 0, 17, 1048576) {
            $destination = Join-Path $scratch "down-$size.bin"
            Receive-FilesFile -RemotePath "gollum/down-$size.bin" -LiteralPath $destination -Connection $c -Confirm:$false
            Assert-SameContent ([IO.File]::ReadAllBytes($destination)) (New-TestContent $size) -Message "$size bytes"
        }
        Assert-True (@(Select-Operation (Get-Journal) 'transfers.download' | Where-Object { $null -ne $_.PSObject.Properties['credentials'] -and $null -ne $_.credentials }).Count -eq 0) 'downloads carry no credential'
    }

    Invoke-Journey 'GOL-XFER-003', 'SIM-PARTS-001', 'NET-UP-08', 'MOCK-XFER-001' {
        Reset-Simulator -SimulatorProfile @{ upload = @{ mode = 'serial'; partsize = 8 } }
        Add-Fault @{ operation = 'transfers.upload_part'; match = @{ part = 2 }; status = 503 }
        $data = New-TestContent 21
        $result = Send-FilesFile -LiteralPath (Write-LocalFile 'serial.bin' $data) -RemotePath 'parts/serial.bin' -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 1 }) -PassThru -Confirm:$false
        Assert-SameContent (Get-Stored 'parts/serial.bin') $data -Message 'bytes'
        $parts = @(Select-Operation (Get-Journal) 'transfers.upload_part' | ForEach-Object { "$($_.part):$($_.status)" })
        Assert-Equal $parts @('1:200', '2:503', '2:200', '3:200') -Message 'one part at a time, in order; only the failed part sent again'
        Assert-Equal "$($result.SerialReason)" 'ServerRequiresSerialParts' -Message 'why the parts went one at a time'
        Assert-Equal @($result.Parts | ForEach-Object Length) @(8, 8, 5) -Message 'the issued part sizes'
    }

    Invoke-Journey 'MOCK-XFER-003' {
        Reset-Simulator -SimulatorProfile @{ upload = @{ mode = 'serial'; partsize = 8; retry_parts = $false } }
        Add-Fault @{ operation = 'transfers.upload_part'; match = @{ part = 2 }; status = 503 }
        $failed = Get-Failure { Send-FilesFile -LiteralPath (Write-LocalFile 'no-retry.bin' (New-TestContent 21)) -RemotePath 'parts/no-retry.bin' -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 3 }) -Confirm:$false }
        Assert-True ($null -ne $failed) 'the upload fails'
        $entries = Get-Journal
        Assert-Equal @(Select-Operation $entries 'transfers.upload_part' | ForEach-Object { "$($_.part):$($_.status)" }) @('1:200', '2:503') -Message 'the failed part is not sent again'
        Assert-Equal (Select-Operation $entries 'files.finalize_upload').Count 0 -Message 'no finalize'
    }

    # UP-003 asks for every supported S3-compatible provider; the simulator signs URLs in one form.
    Invoke-Journey 'UP-003:simulated-s3', 'GOL-LOG-001', 'MOCK-XFER-009' {
        Reset-Simulator -SimulatorProfile @{ signed_urls = @{ lifetime = 900 }; upload = @{ mode = 'serial'; partsize = 8 } }
        Add-Fault @{ operation = 'transfers.upload_part'; match = @{ part = 2 }; status = 503 }
        $c = Connect-Simulator -Options @{ MaxNetworkRetries = 1 }
        $data = New-TestContent 21
        $verbose = @(Send-FilesFile -LiteralPath (Write-LocalFile 'signed.bin' $data) -RemotePath 'signed/data.bin' -Connection $c -Confirm:$false -Verbose 4>&1)
        $destination = Join-Path $scratch 'signed-download.bin'
        $verbose += @(Receive-FilesFile -RemotePath 'signed/data.bin' -LiteralPath $destination -Connection $c -Confirm:$false -Verbose 4>&1)
        Assert-SameContent ([IO.File]::ReadAllBytes($destination)) $data -Message 'downloaded bytes'
        Assert-True (@(Select-Operation (Get-Journal) 'transfers.upload_part' | Where-Object status -EQ 403).Count -eq 0) 'signed URLs are sent with their exact query'
        $lines = @($verbose | ForEach-Object { "$_" })
        Assert-True ($lines.Count -gt 0) 'the engine''s messages reached Verbose'
        Assert-True (@($lines | Where-Object { $_ -match 'SIMULATEDSENTINELKEY|X-Amz-Signature|X-Amz-Credential' }).Count -eq 0) ($lines -join ' | ')
        $expired = Get-Failure { Receive-FilesFile -RemotePath 'signed/missing.bin' -LiteralPath (Join-Path $scratch 'missing.bin') -Connection $c -Confirm:$false }
        Assert-True ("$expired" -notmatch 'SIMULATEDSENTINELKEY|X-Amz-Signature') "an error: $expired"
    }

    Invoke-Journey 'GOL-LOG-002' {
        # The explicit DEBUG contract: with -DiagnosticDebug the debug stream carries each request's and response's body
        # exactly, and signed URLs whole; without it neither the debug nor the ordinary Verbose stream carries any.
        Reset-Simulator -SimulatorProfile @{ signed_urls = @{ lifetime = 900 }; upload = @{ mode = 'serial'; partsize = 8 } }
        $source = Write-LocalFile 'debug.bin' (New-TestContent 21)
        $payload = [ordered]@{ username = 'sentinel-debug-user'; name = 'Sentinel Debug Name'; notes = 'sentinel-debug-notes' }
        $DebugPreference = 'Continue'
        $VerbosePreference = 'Continue'
        $plainConnection = Connect-Simulator
        $plain = @(& {
                New-FilesUser -Username $payload.username -Name $payload.name -Notes $payload.notes -Connection $plainConnection -Confirm:$false
                Send-FilesFile -LiteralPath $source -RemotePath 'debug/plain.bin' -Connection $plainConnection -Confirm:$false
            } 5>&1 4>&1 | Where-Object { $_ -is [System.Management.Automation.DebugRecord] -or $_ -is [System.Management.Automation.VerboseRecord] } | ForEach-Object Message)
        $VerbosePreference = 'SilentlyContinue'
        $diagnosedConnection = Connect-Simulator -Options @{ DiagnosticDebug = $true }
        $created = @(New-FilesUser -Username "$($payload.username)-2" -Name $payload.name -Notes $payload.notes -Connection $diagnosedConnection -Confirm:$false 5>&1 | Where-Object { $_ -is [System.Management.Automation.DebugRecord] } | ForEach-Object Message)
        $uploaded = @(Send-FilesFile -LiteralPath $source -RemotePath 'debug/diagnosed.bin' -Connection $diagnosedConnection -Confirm:$false 5>&1 | Where-Object { $_ -is [System.Management.Automation.DebugRecord] } | ForEach-Object Message)
        $DebugPreference = 'SilentlyContinue'
        Assert-True ($plain.Count -gt 0) 'the ordinary run wrote its Verbose messages'
        foreach ($secret in @($payload.Values) + 'X-Amz-Signature') {
            Assert-True (@($plain | Where-Object { $_.Contains($secret) }).Count -eq 0) "without -DiagnosticDebug no stream carries $secret"
        }
        $request = @($created | Where-Object { $_ -match '^FilesCom: content: ' } | ForEach-Object { $_ -replace '^FilesCom: content: ', '' })
        $response = @($created | Where-Object { $_ -match '^FilesCom: Response \(HTTP 201\): ' } | ForEach-Object { $_ -replace '^FilesCom: Response \(HTTP 201\): ', '' })
        Assert-Equal $request.Count 1 -Message 'one request body'
        Assert-Equal ($request[0] | ConvertFrom-Json) ([ordered]@{ username = "$($payload.username)-2"; name = $payload.name; notes = $payload.notes }) -Message 'the request body, exactly'
        $stored = (Send-Raw -Method GET -Url "$base/api/rest/v1/users/2").Text | ConvertFrom-Json
        Assert-Equal @($response | ForEach-Object { $_ | ConvertFrom-Json }) @($stored) -Message 'the response body, exactly as the simulator holds the user'
        Assert-True (@($uploaded | Where-Object { $_ -match 'X-Amz-Signature=' }).Count -gt 0) "with -DiagnosticDebug signed URLs are whole: $($uploaded.Count) lines"
        Assert-Equal @((Select-Operation (Get-Journal) 'users.create').status) @(201, 201) -Message 'both users were created'
    }

    Invoke-Journey 'NET-UP-14:serial' {
        # 503 and 429 answers with Retry-After pause new parts at least that long, the upload shrinks and recovers, and
        # each part is stored once. One part at a time: the recovery of parts in flight together is not simulated.
        Reset-Simulator -SimulatorProfile @{ upload = @{ mode = 'serial'; partsize = 4096 } }
        Add-Fault @{ operation = 'transfers.upload_part'; match = @{ part = 3 }; status = 503; retry_after = 1 }
        Add-Fault @{ operation = 'transfers.upload_part'; match = @{ part = 6 }; status = 429; retry_after = 1 }
        $data = New-TestContent 40960
        $quick = New-FilesAutomaticConcurrency -Profile Generic -BackpressurePauseSeconds 0.1
        $result = Send-FilesFile -LiteralPath (Write-LocalFile 'throttled.bin' $data) -RemotePath 'parts/throttled.bin' -AutomaticConcurrency $quick -Diagnostics -InformationVariable trace -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 5 }) -PassThru -Confirm:$false
        Assert-SameContent (Get-Stored 'parts/throttled.bin') $data -Message 'bytes'
        $parts = @(Select-Operation (Get-Journal) 'transfers.upload_part')
        Assert-Equal @($parts | ForEach-Object { "$($_.part):$($_.status)" }) @('1:200', '2:200', '3:503', '3:200', '4:200', '5:200', '6:429', '6:200', '7:200', '8:200', '9:200', '10:200') -Message 'each throttled part sent again, then the rest'
        foreach ($throttled in 3, 6) {
            $attempts = @($parts | Where-Object part -EQ $throttled)
            # Whole milliseconds since the reset: the answer was sent after applied_ms, the next attempt arrived at arrived_ms.
            $gap = $attempts[1].arrived_ms - $attempts[0].applied_ms
            Assert-True ($gap -ge 999) "part $throttled waited out Retry-After: $gap ms"
        }
        $decisions = @($trace.MessageData | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadDecisionTrace] })
        Assert-Equal @($decisions | Where-Object { "$($_.Decision)" -eq 'CooldownStarted' } | ForEach-Object { $_.Cooldown.TotalSeconds }) @(1.0, 1.0) -Message 'each cooldown is Retry-After, over the 0.1 s pause'
        Assert-Equal @($decisions | Where-Object { "$($_.Decision)" -eq 'ShrankAfterThrottling' }).Count 2 -Message 'each throttle shrank the target'
        Assert-Equal @($result.Metrics.Throttles, "$($result.PerformanceProfileSource)") @(2, 'Options') -Message 'the result counts the throttles'
    }

    Invoke-Journey 'NET-UP-15' {
        # Normal results carry cheap metrics; detailed records come only when asked, and match the journal.
        Reset-Simulator -SimulatorProfile @{ upload = @{ mode = 'serial'; partsize = 4096 } }
        Add-Fault @{ operation = 'transfers.upload_part'; match = @{ path = 'observe/traced.bin'; part = 3 }; status = 500 }
        $c = Connect-Simulator -Options @{ MaxNetworkRetries = 2 }
        $data = New-TestContent 20000
        $quiet = Send-FilesFile -LiteralPath (Write-LocalFile 'quiet.bin' $data) -RemotePath 'observe/quiet.bin' -Connection $c -InformationVariable quietRecords -PassThru -Confirm:$false
        Assert-Equal @($quietRecords).Count 0 -Message 'no records unless asked'
        Assert-True ($null -ne $quiet.Metrics -and $quiet.Metrics.ThroughputBytesPerSecond -gt 0) 'the result carries metrics'
        Send-FilesFile -LiteralPath (Write-LocalFile 'traced.bin' $data) -RemotePath 'observe/traced.bin' -Connection $c -Diagnostics -InformationVariable records -Confirm:$false
        $attempts = @($records.MessageData | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadAttemptTrace] } | Sort-Object PartNumber, Attempt | ForEach-Object { "$($_.PartNumber):$($_.StatusCode)" })
        # The simulator numbers uploads from 1 after the reset: the traced upload is the second.
        $journaled = @(Select-Operation (Get-Journal) 'transfers.upload_part' | Where-Object upload -EQ 2 | ForEach-Object { "$($_.part):$($_.status)" })
        Assert-Equal $attempts $journaled -Message 'each attempt record matches the journal'
        Assert-Equal $journaled @('1:200', '2:200', '3:500', '3:200', '4:200', '5:200') -Message 'the journal'
        $summary = @($records.MessageData)[-1]
        Assert-True ($summary -is [FilesCom.PowerShell.FilesUploadTraceSummary] -and $summary.Complete -and "$($summary.Outcome)" -eq 'Completed') 'the records are complete'
        Assert-True (-not ((@($records.MessageData) | Out-String -Width 400) -match 'sentinel-ps-key|X-Amz-Signature')) 'no credential or signature in the records'
    }

    if ($adaptive) {
        Invoke-Journey 'NET-UP-05' {
            # The server's description of the destination selects distinct defaults; nothing is guessed from addresses.
            $expected = @{ s3 = @('S3', 64); agent_fiw = @('Agent', 8); generic = @('Generic', 8) }
            foreach ($class in 's3', 'agent_fiw', 'generic') {
                Reset-Simulator -SimulatorProfile @{ upload = @{ mode = 'parallel'; partsize = 4096; upload_target_class = $class } }
                $result = Send-FilesFile -LiteralPath (Write-LocalFile "class-$class.bin" (New-TestContent 8192)) -RemotePath "classes/$class.bin" -Connection (Connect-Simulator) -PassThru -Confirm:$false
                Assert-Equal @("$($result.PerformanceProfile)", $result.Metrics.MaxTarget) $expected[$class] -Message "the $class defaults"
                Assert-Equal "$($result.PerformanceProfileSource)" 'Server' -Message 'from the server''s description'
            }
        }

        Invoke-Journey 'NET-UP-10' {
            # Without variable limits the issued part size is mandatory; within them, sizes are the upload's choice and a
            # fixed size outside them is refused before any part.
            Reset-Simulator -SimulatorProfile @{ upload = @{ mode = 'serial'; partsize = 8 } }
            $small = New-FilesAutomaticPartSizing -KnownSizeTiers @{ 1MB = 5 } -WorkloadParts 1
            $c = Connect-Simulator
            $mandatory = Send-FilesFile -LiteralPath (Write-LocalFile 'mandatory.bin' (New-TestContent 21)) -RemotePath 'limits/mandatory.bin' -AutomaticPartSizing $small -Connection $c -PassThru -Confirm:$false
            Assert-Equal @($mandatory.Parts | ForEach-Object Length) @(8, 8, 5) -Message 'the issued size, whatever the tuning prefers'
            Assert-Equal @("$($mandatory.PartSizeSource)", "$($mandatory.ServerPlannedReason)") @('ServerPlanned', 'NotAllowed') -Message 'why'
            Reset-Simulator -SimulatorProfile @{ upload = @{ mode = 'serial'; partsize = 1024; variable_part_limits = @{ min_nonfinal_bytes = 1024; max_part_bytes = 8192; max_parts = 100; max_file_bytes = 1048576 } } }
            $chosen = Send-FilesFile -LiteralPath (Write-LocalFile 'chosen.bin' (New-TestContent 30000)) -RemotePath 'limits/chosen.bin' -PartSize 3000 -Connection $c -PassThru -Confirm:$false
            Assert-Equal @($chosen.Parts | Select-Object -SkipLast 1 | ForEach-Object Length | Sort-Object -Unique) @(3000) -Message 'the chosen size within the limits'
            Assert-SameContent (Get-Stored 'limits/chosen.bin') (New-TestContent 30000) -Message 'bytes'
            $refused = Get-Failure { Send-FilesFile -LiteralPath (Write-LocalFile 'refused.bin' (New-TestContent 30000)) -RemotePath 'limits/refused.bin' -PartSize 9000 -Connection $c -Confirm:$false }
            Assert-True ($refused.FullyQualifiedErrorId -like 'Files.LimitExceeded*' -and "$($refused.Exception.UploadLimit)" -eq 'FixedPartSize') "a size outside the limits: $refused"
            # The simulator numbers uploads from 1 after the reset: the refused upload, if begun, is the second.
            Assert-Equal @(Select-Operation (Get-Journal) 'transfers.upload_part' | Where-Object upload -EQ 2).Count 0 -Message 'refused before any part'
        }

        # NET-UP-11's resume identities wait for the next engine's resumable uploads (T4).
        Invoke-Journey 'NET-UP-11:known-size' {
            # Parts not yet read grow while a retried part keeps its number, offset and length, which the simulator enforces.
            Reset-Simulator -SimulatorProfile @{ upload = @{ mode = 'parallel'; partsize = 1024; part_offset_query = $true; variable_part_limits = @{ min_nonfinal_bytes = 1024; max_part_bytes = 1048576; max_parts = 1000; max_file_bytes = 104857600 } } }
            Add-Fault @{ operation = 'transfers.upload_part'; match = @{ part = 2 }; status = 500 }
            $one = New-FilesAutomaticConcurrency -Profile Generic -MinTarget 1 -StartTarget 1 -SoftCeiling 1 -MaxTarget 1 -PlateauTarget 1 -WindowAttempts 1
            $growing = New-FilesAutomaticPartSizing -KnownSizeTiers @{ 100MB = 16384 } -WorkloadParts 1 -GrowthIntervalWindows 1 -ProbeWindows 1 -HoldWindows 0
            $data = New-TestContent 400000
            $result = Send-FilesFile -LiteralPath (Write-LocalFile 'planned.bin' $data) -RemotePath 'plans/planned.bin' -AutomaticConcurrency $one -AutomaticPartSizing $growing -Connection (Connect-Simulator -Options @{ MaxNetworkRetries = 2 }) -PassThru -Confirm:$false
            Assert-SameContent (Get-Stored 'plans/planned.bin') $data -Message 'bytes'
            $sizes = @($result.Parts | ForEach-Object Length)
            Assert-True ($sizes[0] -eq 16384 -and $sizes -contains 32768) "later parts grew: $($sizes -join ',')"
            $second = @(Select-Operation (Get-Journal) 'transfers.upload_part' | Where-Object part -EQ 2)
            Assert-Equal @($second.status) @(500, 200) -Message 'part 2 was retried'
            Assert-Equal @($second.bytes | Select-Object -Last 1) @(16384) -Message 'with the length it was first sent with'
        }
    }

    Invoke-Journey 'C39:copy-move' {
        # The six copy and move destination helpers; uploads to these destinations wait for the transfer engine.
        Reset-Simulator
        $c = Connect-Simulator
        foreach ($kind in @(@{ Parameter = 'RemoteServerId'; Folder = 'RemoteServers'; Scope = 'remote_server'; Id = 7 }, @{ Parameter = 'SnapshotId'; Folder = 'Snapshots'; Scope = 'snapshot'; Id = 8 }, @{ Parameter = 'ChildSiteId'; Folder = 'Sites'; Scope = 'child_site'; Id = 9 })) {
            Save-Raw "src/copy-$($kind.Scope).bin" (New-TestContent 5)
            Save-Raw "src/move-$($kind.Scope).bin" (New-TestContent 6)
            $copy = @{ Path = "src/copy-$($kind.Scope).bin"; DestinationPath = 'copied.bin'; Connection = $c; Confirm = $false; $kind.Parameter = $kind.Id }
            $move = @{ Path = "src/move-$($kind.Scope).bin"; DestinationPath = 'moved.bin'; Connection = $c; Confirm = $false; $kind.Parameter = $kind.Id }
            Copy-FilesFile @copy | Out-Null
            Move-FilesFile @move | Out-Null
            Assert-SameContent (Get-Stored "_/$($kind.Folder)/$($kind.Id)/copied.bin") (New-TestContent 5) -Message "$($kind.Parameter): copied bytes"
            Assert-SameContent (Get-Stored "src/copy-$($kind.Scope).bin") (New-TestContent 5) -Message "$($kind.Parameter): the copy source stays"
            Assert-SameContent (Get-Stored "_/$($kind.Folder)/$($kind.Id)/moved.bin") (New-TestContent 6) -Message "$($kind.Parameter): moved bytes"
            Assert-Equal (Send-Raw -Method GET -Url "$base/api/rest/v1/files/src/move-$($kind.Scope).bin").Status 404 -Message "$($kind.Parameter): the move source is gone"
            # Only the module's requests carry a credential; the raw reads here carry none.
            $sent = @((Get-Journal) | Where-Object { $null -ne $_.PSObject.Properties['destination_scope'] -and $_.destination_scope.kind -eq $kind.Scope -and $null -ne $_.credentials })
            Assert-Equal @($sent | ForEach-Object { "$($_.operation):$($_.destination_scope.id):$($_.credentials.api_key):$($_.status)" }) @("files.copy:$($kind.Id):1:201", "files.move:$($kind.Id):1:201") -Message "$($kind.Parameter): the destination scope"
        }
    }
}
finally {
    Remove-Item -LiteralPath $scratch -Recurse -Force -ErrorAction SilentlyContinue
}

[ordered]@{ package = (Get-Identity); powershell = [ordered]@{ version = "$($PSVersionTable.PSVersion)"; edition = $PSVersionTable.PSEdition }; results = $results; epochs = @($epochs) } | ConvertTo-Json -Depth 8
exit $(if (@($results.Values | Where-Object { $_ -ne 'pass' }).Count -eq 0) { 0 } else { 1 })
