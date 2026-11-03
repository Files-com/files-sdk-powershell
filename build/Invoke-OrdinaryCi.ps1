<#
.SYNOPSIS
Builds, checks and tests the FilesCom module as its ordinary CI job does.

.DESCRIPTION
ci.sh runs this on the newest pinned PowerShell, with every pinned PowerShell in -WorkPath/powershell/<version>. On
Linux, it:

- requires the checkout to be the job's source projection: one commit's files that build/ordinary-source-projection.txt
  lists and no others, in a repository that can fetch nothing more;
- takes the production schema capture FILESCOM_CI_SCHEMA names, with its checksum and record beside it, and the FilesCom
  package FILESCOM_CI_PACKAGE names, which the same pipeline built from that capture, with the package's record
  FILESCOM_CI_PACKAGE_PROVENANCE names;
- requires them to join, as build/Resolve-RunEngine.ps1 checks: a public capture and a package whose records describe
  their exact bytes, the package's lib members, nuspec and locked dependencies, from this commit and this pipeline;
- exposes the package to the build, in a feed of its own, only once they join;
- generates this target and the Files.com API simulator (mock-server) from the capture in one generator run, requires
  the generator to have made nothing from sources the projection leaves out and the simulator to record the capture's
  exact bytes, and makes the generated target's engine.json and engine lock name the package, in place of the committed
  ones;
- runs build.sh, which restores the pinned tools, builds, documents, imports and packs the module, and requires the
  module's record of its schema and engine to be the ones checked;
- analyzes the module's scripts, examples and ordinary test suites and its C# sources (build/Invoke-Analysis.ps1);
- runs the ordinary test suites with build/Test-Module.ps1 on each pinned PowerShell in turn, as an unprivileged user
  in two groups, and requires every suite to have run tests, all of which passed. That user is the job's own when the
  job does not run as root and its user is in two groups; when the job runs as root, it is a user in two groups made
  for the run. The job fails, naming its user and groups, when neither is so;
- after each PowerShell's tests, runs the generator's shared API scenarios (verification/scenarios/api.json) through
  the installed module's public commands on that PowerShell, as the same user, each against a fresh simulator, with
  build/Invoke-ApiScenarios.ps1, and requires every simulator step of each to pass, with a complete journal joined to
  every request the module sent, and the consumer's controls to refuse what they must.

The generated contract suite, which needs the shared mock server, the region suite and the catalogue adapter are not
part of this job, and its result says nothing about them. Their sources are not in the projection, and the job fails if
they, or the files the generator would make from them, exist. The API scenarios consumer (verification/api-scenarios)
is separate from all three. The evidence goes to -EvidencePath: summary.json, the schema and engine records, the module
package and its file hashes, each PowerShell's test results and API scenario runs (canonical/<version>), and the logs
of each step.

Nothing is fetched. In GitLab CI, the three inputs are the files the pipeline's earlier jobs left, and they must come
from the job's own pipeline (CI_PIPELINE_ID) and commit (CI_COMMIT_SHA). Elsewhere they may name the files of one
pipeline, checked the same way, with no pipeline of the job's own to bind. Or, for a local check of the package
engine.json pins, FILESCOM_CI_ENGINE_ARCHIVE and FILESCOM_CI_ENGINE_PROVENANCE name it and its record instead of
FILESCOM_CI_PACKAGE and FILESCOM_CI_PACKAGE_PROVENANCE, and both are checked against engine.json, with the schema it
names.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $WorkPath,

    [Parameter(Mandatory)]
    [string] $EvidencePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Every suite but the generated contract suite and the region suite.
$ordinarySuites = @('Attachments', 'Cancellation', 'Connections', 'Directories', 'Errors', 'Examples', 'Module', 'Paging', 'Runtime', 'Transfers', 'Values', 'WhatIf', 'Workspace') |
    ForEach-Object { "$_.Tests.ps1" }

# The job's source projection: the commit's files that this list names, one '/'-anchored literal path a line, the list
# among them. It leaves out the sources of the generated contract suite, the region suite and the catalogue adapter,
# which, like the files the generator would make from them, must not be in the job.
$projectionList = 'targets/powershell/build/ordinary-source-projection.txt'
$protectedSources = @('targets/powershell/tests/__Contract.Tests.ps1', 'targets/powershell/tests/Regions.Tests.ps1', 'targets/powershell/verification/catalogue/journeys.ps1')
$protectedOutputs = @('generated/powershell/tests/Contract.Tests.ps1', 'generated/powershell/tests/Regions.Tests.ps1', 'generated/powershell/verification/catalogue/journeys.ps1')
# The generator makes this target, and the simulator the API scenarios run against, from every file it finds under these
# directories.
$generatorInputs = @('targets/powershell', 'targets/common', 'targets/mock-server')

# The engine's inputs: the package the job's pipeline built and its record, for the job; or, for a local check only, the
# package engine.json pins and its record.
$runInputs = @('FILESCOM_CI_PACKAGE', 'FILESCOM_CI_PACKAGE_PROVENANCE')
$pinnedInputs = @('FILESCOM_CI_ENGINE_ARCHIVE', 'FILESCOM_CI_ENGINE_PROVENANCE')
# The engine project's lock, which pins the package and its dependencies.
$engineLock = 'src/FilesCom.PowerShell.Engine/packages.lock.json'

# The record of the package engine.json pins: the package itself names no commit. A local check uses that package only
# when its record has this SHA-256 and names engine.json's package, version, source tree and schema, and this producer
# commit.
$pinnedRecord = [ordered]@{
    Sha256 = '29c4bd99651b1c80d8c4b256ecf5ecb59fd0ec0dc09a990c818187b98ba37d9d'
    Format = 'filescom-dotnet-package-provenance/1'
    ProducerCommit = '4848f6246e5fd1af6fd5b064b4c08c29a13d2547'
}

# When the job runs as root: the unprivileged user the tests run as, and the names of its two groups, made for the run.
$testUserId = 65534
$testGroupNames = @('filescom-ci-tests', 'filescom-ci-other')

$target = (Resolve-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$projectRoot = (Resolve-Path -LiteralPath (Join-Path -Path $target -ChildPath '../..')).Path
$generated = Join-Path -Path $projectRoot -ChildPath 'generated/powershell'
$mockServer = Join-Path -Path $projectRoot -ChildPath 'generated/mock-server'
$pwsh = Join-Path -Path $PSHOME -ChildPath 'pwsh'
$inCi = $env:GITLAB_CI -eq 'true'
$engine = Get-Content -LiteralPath (Join-Path -Path $target -ChildPath 'engine.json') -Raw | ConvertFrom-Json
$pins = Get-Content -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath 'tools.json') -Raw | ConvertFrom-Json
$hostVersions = @($pins.powershell.versions.PSObject.Properties.Name | Sort-Object { [version] $_ })

# Runs a program and fails unless it succeeds. With -LogPath, its output, errors included, is also kept in that file.
function Invoke-Checked {
    param([string] $FilePath, [string[]] $ArgumentList, [string] $LogPath)
    if ($LogPath) {
        & $FilePath @ArgumentList 2>&1 | Tee-Object -FilePath $LogPath | Out-Host
    }
    else {
        & $FilePath @ArgumentList | Out-Host
    }
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath $($ArgumentList -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Get-Sha256 {
    param([string] $Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

# A single-quoted PowerShell literal of each value, for a -Command another PowerShell process runs.
function ConvertTo-Literal {
    param([string[]] $Value)
    $quoted = @($Value | ForEach-Object { "'" + $_.Replace("'", "''") + "'" })
    if ($quoted.Count -eq 1) { $quoted[0] } else { '@(' + ($quoted -join ', ') + ')' }
}

# The user and groups this job runs as, by ID and, where the system has them, by name.
function Get-JobIdentity {
    [ordered]@{
        uid = [string] (& id -u)
        user = [string] (& id -un 2>$null)
        groups = [string] (& id -G)
        groupNames = [string] (& id -Gn 2>$null)
    }
}

# Who the tests run as, for a job with this identity: 'job user', the job's own, when it is not root and in at least two
# groups, which Transfers' group-ownership tests need; 'made for the run' when the job runs as root, which can make such
# a user. Fails, naming the job's user and groups, otherwise: only root could give the tests a user in two groups.
function Get-TestUserMode {
    param($Identity)
    if ($Identity.uid -eq '0') {
        return 'made for the run'
    }
    if (@($Identity.groups -split ' ' | Where-Object { $_ }).Count -ge 2) {
        return 'job user'
    }
    throw "The job runs as user '$($Identity.user)' (uid $($Identity.uid)) in groups '$($Identity.groupNames)' ($($Identity.groups)), which is not root and in fewer than two groups. The tests need an unprivileged user in two groups, for Transfers' group-ownership tests, and only root could make one here."
}

# For a local check of the package engine.json pins: its record, checked against the pins here and in engine.json, and
# the archive against both: each mismatch, as 'what: actual, expected'. The record gives the package's id and version,
# its archive's path, size and SHA-256, the commit and tree it was built from, and its schema's SHA-256.
function Get-EngineMismatch {
    param([string] $ArchivePath, [string] $ProvenancePath)
    $fileName = "$($engine.package).$($engine.version).nupkg"
    $record = Get-Content -LiteralPath $ProvenancePath -Raw | ConvertFrom-Json
    $value = {
        param($Object, [string[]] $Names)
        foreach ($name in $Names) {
            if ($null -eq $Object -or -not $Object.PSObject.Properties[$name]) {
                return '(missing)'
            }
            $Object = $Object.$name
        }
        [string] $Object
    }
    $checks = [ordered]@{
        'record SHA-256' = @((Get-Sha256 -Path $ProvenancePath), $pinnedRecord.Sha256)
        'record format' = @((& $value $record 'format'), $pinnedRecord.Format)
        'package name' = @((& $value $record 'package', 'id'), $engine.package)
        'package version' = @((& $value $record 'package', 'version'), $engine.version)
        'package file' = @([System.IO.Path]::GetFileName((& $value $record 'package', 'archive', 'path')), $fileName)
        'package SHA-256' = @((& $value $record 'package', 'archive', 'sha256'), $engine.sha256)
        'archive SHA-256' = @((Get-Sha256 -Path $ArchivePath), $engine.sha256)
        'archive size' = @([string] (Get-Item -LiteralPath $ArchivePath).Length, (& $value $record 'package', 'archive', 'bytes'))
        'producer commit' = @((& $value $record 'source', 'commit'), $pinnedRecord.ProducerCommit)
        'producer tree' = @((& $value $record 'source', 'tree'), $engine.source_tree)
        'producer schema' = @((& $value $record 'schema', 'sha256'), $engine.schema_sha256)
    }
    foreach ($check in $checks.GetEnumerator()) {
        if ($check.Value[0] -cne $check.Value[1]) {
            "$($check.Key): $($check.Value[0]), expected $($check.Value[1])"
        }
    }
}

# Each ordinary suite's tests in a Test-Module result file. Fails unless the file shows no failure, error, skip or test
# not run, every ordinary suite and no other, and in each suite at least one test, every one of which ran and passed.
function Assert-TestResult {
    param([string] $Path, [string[]] $Suites)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Path was not written."
    }
    [xml] $results = Get-Content -LiteralPath $Path -Raw
    $summary = $results.DocumentElement
    $problems = [System.Collections.Generic.List[string]]::new()
    foreach ($attribute in 'failures', 'errors', 'not-run', 'inconclusive', 'ignored', 'skipped', 'invalid') {
        if ([int] $summary.GetAttribute($attribute)) {
            $problems.Add("$attribute $($summary.GetAttribute($attribute))")
        }
    }
    $counts = [ordered]@{}
    foreach ($file in $results.SelectNodes('/test-results/test-suite/results/test-suite')) {
        $cases = @($file.SelectNodes('.//test-case'))
        $passed = @($cases | Where-Object { $_.GetAttribute('executed') -eq 'True' -and $_.GetAttribute('result') -eq 'Success' })
        $counts[[System.IO.Path]::GetFileName($file.GetAttribute('name'))] = [ordered]@{ tests = $cases.Count; passed = $passed.Count }
    }
    foreach ($suite in $Suites) {
        if (-not $counts.Contains($suite)) {
            $problems.Add("$suite ran no test")
        }
        elseif (-not $counts[$suite].tests -or $counts[$suite].passed -ne $counts[$suite].tests) {
            $problems.Add("$suite passed $($counts[$suite].passed) of $($counts[$suite].tests) tests")
        }
    }
    foreach ($name in $counts.Keys | Where-Object { $_ -notin $Suites }) {
        $problems.Add("$name is not an ordinary suite")
    }
    $total = 0
    foreach ($count in $counts.Values) {
        $total += $count.tests
    }
    if ([int] $summary.GetAttribute('total') -ne $total) {
        $problems.Add("the file counts $($summary.GetAttribute('total')) tests, its suites $total")
    }
    if ($problems.Count) {
        throw "$Path does not show every ordinary suite passing: $($problems -join '; ')."
    }
    $counts
}

# Items, joined with Separator: the first ten, and how many more there are.
function Join-Item {
    param([string[]] $Item, [string] $Separator = ', ')
    $shown = @($Item | Select-Object -First 10) -join $Separator
    if ($Item.Count -gt 10) { "$shown$Separator... $($Item.Count - 10) more" } else { $shown }
}

# Whether anything is at Path: a file, a directory, or a link, even one to nothing.
function Test-Entry {
    param([string] $Path)
    [System.IO.FileInfo]::new($Path).Exists -or [System.IO.Directory]::Exists($Path)
}

# Runs git on the project's repository so that it cannot reach a remote, whatever any Git configuration says: every
# transport is refused, missing objects are not fetched and replacement objects are not used. -InputLine is its standard
# input, a line each. Returns its output split at the NULs -z puts between items, and fails unless it exits with one of
# -Allowed, which $LASTEXITCODE then holds.
function Invoke-SourceGit {
    param([string[]] $ArgumentList, [int[]] $Allowed = @(0), [string[]] $InputLine)
    $sealed = @('--no-replace-objects', '-c', 'protocol.allow=never')
    foreach ($protocol in 'file', 'git', 'http', 'https', 'ssh', 'ext') {
        $sealed += @('-c', "protocol.$protocol.allow=never")
    }
    $lazyFetch = $env:GIT_NO_LAZY_FETCH
    $env:GIT_NO_LAZY_FETCH = '1'
    try {
        if ($PSBoundParameters.ContainsKey('InputLine')) {
            $output = $InputLine | & git -C $projectRoot @sealed @ArgumentList
        }
        else {
            $output = & git -C $projectRoot @sealed @ArgumentList
        }
    }
    finally {
        $env:GIT_NO_LAZY_FETCH = $lazyFetch
    }
    if ($LASTEXITCODE -notin $Allowed) {
        throw "git $($ArgumentList -join ' ') failed with exit code $LASTEXITCODE."
    }
    @(($output -join "`n") -split "`0" | Where-Object { $_ })
}

# Which of Paths, relative to the project, are regular files that no link leads to. find, which does not follow a link
# it is given, tells them from links, directories and special files, which hashing could wait on forever; and no
# directory above one may be a link.
function Get-RegularFile {
    param([string[]] $Paths)
    $isLink = [System.Collections.Generic.Dictionary[string, bool]]::new([System.StringComparer]::Ordinal)
    $linked = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($path in $Paths) {
        for ($parent = [System.IO.Path]::GetDirectoryName($path); $parent; $parent = [System.IO.Path]::GetDirectoryName($parent)) {
            if (-not $isLink.ContainsKey($parent)) {
                $item = Get-Item -LiteralPath (Join-Path -Path $projectRoot -ChildPath $parent) -Force -ErrorAction Ignore
                $isLink[$parent] = $null -ne $item -and ($item -isnot [System.IO.DirectoryInfo] -or [bool] $item.LinkTarget)
            }
            if ($isLink[$parent]) {
                [void] $linked.Add($path)
            }
        }
    }
    $prefix = $projectRoot.TrimEnd('/') + '/'
    $regular = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($file in @(& find @($Paths | ForEach-Object { $prefix + $_ }) -prune -type f -print 2>$null)) {
        if (-not $linked.Contains($file.Substring($prefix.Length))) {
            [void] $regular.Add($file.Substring($prefix.Length))
        }
    }
    , $regular
}

# The paths the projection list at Path names, without their leading '/'. Fails unless each of its lines ends in LF
# alone and is a literal '/'-anchored path of names made of letters, digits and . _ @ + -, but not '.', '..' or '.git';
# the lines are in ordinal order, each once; and they name the list itself and no protected source.
function Get-ProjectionList {
    param([string] $Path)
    # One character a byte, so no byte can hide in a decoding.
    $text = [System.Text.Encoding]::Latin1.GetString([System.IO.File]::ReadAllBytes($Path))
    if (-not $text.EndsWith("`n")) {
        throw "$projectionList is empty or does not end its last line with LF."
    }
    $lines = $text.Substring(0, $text.Length - 1).Split("`n")
    $problems = [System.Collections.Generic.List[string]]::new()
    $previous = ''
    foreach ($line in $lines) {
        $names = $line.Split('/')
        if ($names[0] -or $names.Count -lt 2 -or @($names | Select-Object -Skip 1 | Where-Object { $_ -cnotmatch '^[A-Za-z0-9._@+-]+$' -or $_ -in '.', '..', '.git' }).Count) {
            $problems.Add("'$line' is not a literal '/'-anchored path")
        }
        elseif ($line -ceq $previous) {
            $problems.Add("'$line' is listed more than once")
        }
        elseif ([string]::CompareOrdinal($previous, $line) -gt 0) {
            $problems.Add("'$line' is out of ordinal order")
        }
        $previous = $line
    }
    if (-not $problems.Count) {
        $paths = @($lines | ForEach-Object { $_.Substring(1) })
        if ($projectionList -cnotin $paths) {
            $problems.Add('it does not list itself')
        }
        foreach ($path in @($protectedSources | Where-Object { $_ -in $paths })) {
            $problems.Add("it lists $path, a protected source")
        }
    }
    if ($problems.Count) {
        throw "$projectionList is not a list of the job's source projection: $(Join-Item -Item $problems -Separator '; ')."
    }
    $paths
}

$summary = [ordered]@{
    format = 'filescom-powershell-ordinary-ci/1'
    started = [System.DateTime]::UtcNow.ToString('o')
    result = 'running'
    steps = [System.Collections.Generic.List[object]]::new()
}

# Runs one step, timed, and records what it returns, or how it failed, in summary.json.
function Invoke-Step {
    param([string] $Name, [scriptblock] $Action)
    Write-Information "Step: $Name" -InformationAction Continue
    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $details = & $Action
        $summary.steps.Add([ordered]@{ name = $Name; result = 'passed'; seconds = [math]::Round($watch.Elapsed.TotalSeconds, 3); details = $details })
    }
    catch {
        $summary.steps.Add([ordered]@{ name = $Name; result = 'failed'; seconds = [math]::Round($watch.Elapsed.TotalSeconds, 3); error = $_.Exception.Message })
        throw
    }
}

if ((Test-Path -LiteralPath $EvidencePath) -and @(Get-ChildItem -LiteralPath $EvidencePath -Force).Count) {
    throw "$EvidencePath already holds files; give an empty or new directory."
}
New-Item -ItemType Directory -Path $EvidencePath -Force | Out-Null
$EvidencePath = (Resolve-Path -LiteralPath $EvidencePath).Path
$WorkPath = (Resolve-Path -LiteralPath $WorkPath).Path
$logs = New-Item -ItemType Directory -Path (Join-Path -Path $EvidencePath -ChildPath 'logs')

try {
    Invoke-Step -Name 'runner' -Action {
        $isSet = { param([string] $Name) [bool] [System.Environment]::GetEnvironmentVariable($Name) }
        if ($inCi) {
            if (& $isSet 'FILESCOM_TOOL_CACHE') {
                throw 'FILESCOM_TOOL_CACHE is for local checks; in CI that input is fetched.'
            }
            foreach ($name in $pinnedInputs) {
                if (& $isSet $name) {
                    throw "$name is for local checks; in CI the module is built on the package its pipeline built."
                }
            }
            foreach ($name in 'CI_PIPELINE_ID', 'CI_COMMIT_SHA') {
                if (-not (& $isSet $name)) {
                    throw "$name is not set, so the job cannot tell which pipeline's capture and package it may use."
                }
            }
        }
        $runSet = @($runInputs | Where-Object { & $isSet $_ })
        $pinnedSet = @($pinnedInputs | Where-Object { & $isSet $_ })
        $script:engineInput = if ($runSet.Count -eq $runInputs.Count -and -not $pinnedSet.Count) {
            'run'
        }
        elseif (-not $inCi -and $pinnedSet.Count -eq $pinnedInputs.Count -and -not $runSet.Count) {
            'pinned'
        }
        else {
            throw "Set $($runInputs -join ' and ') to the package the pipeline built from FILESCOM_CI_SCHEMA and its record$(if (-not $inCi) { ", or, for a local check of the package engine.json pins, $($pinnedInputs -join ' and ') to it and its record" }); $(if ($runSet.Count + $pinnedSet.Count) { "only $((@($runSet) + @($pinnedSet)) -join ', ') is set" } else { 'none is set' })."
        }
        $identity = Get-JobIdentity
        $script:testUserMode = Get-TestUserMode -Identity $identity
        $installed = @(Get-ChildItem -LiteralPath (Join-Path -Path $WorkPath -ChildPath 'powershell') -Directory | ForEach-Object Name | Sort-Object { [version] $_ })
        if (($installed -join ' ') -ne ($hostVersions -join ' ')) {
            throw "build/tools.json pins PowerShell $($hostVersions -join ', '), but $WorkPath/powershell holds $($installed -join ', ')."
        }
        $os = Get-Content -LiteralPath '/etc/os-release' -Raw | ConvertFrom-StringData
        $memory = (Select-String -LiteralPath '/proc/meminfo' -Pattern '^MemTotal:\s+(\d+) kB').Matches[0].Groups[1].Value
        $cgroupLimit = if (Test-Path -LiteralPath '/sys/fs/cgroup/memory.max') { (Get-Content -LiteralPath '/sys/fs/cgroup/memory.max' -Raw).Trim() } else { 'not available' }
        [ordered]@{
            identity = $identity
            testUser = $script:testUserMode
            engineInput = $script:engineInput
            commit = (& git -C $projectRoot rev-parse HEAD)
            tree = (& git -C $projectRoot rev-parse 'HEAD^{tree}')
            os = $os.PRETTY_NAME.Trim('"')
            architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
            processors = [System.Environment]::ProcessorCount
            memoryBytes = [long] $memory * 1024
            cgroupMemoryMax = $cgroupLimit
            dotnet = (& dotnet --version)
            powershell = @($hostVersions | ForEach-Object { [ordered]@{ version = $_; path = Join-Path -Path $WorkPath -ChildPath "powershell/$_/pwsh" } })
            pipeline = $env:CI_PIPELINE_ID
            job = $env:CI_JOB_ID
            image = $env:CI_JOB_IMAGE
        }
    }

    # The job runs only on its source projection: a checkout of one commit with the files the projection list names and
    # no others, in a repository that cannot fetch. Before anything else, and so before any check could fetch, the
    # repository must have no remote or partial clone left. The files the list leaves out are known only by their names
    # and object IDs in the commit: none is read, hashed or fetched.
    Invoke-Step -Name 'source' -Action {
        foreach ($name in 'GIT_DIR', 'GIT_WORK_TREE', 'GIT_INDEX_FILE', 'GIT_OBJECT_DIRECTORY', 'GIT_ALTERNATE_OBJECT_DIRECTORIES', 'GIT_COMMON_DIR') {
            if ([System.Environment]::GetEnvironmentVariable($name)) {
                throw "$name is set, but the job's source is the repository at $projectRoot and no other."
            }
        }
        if (@(Invoke-SourceGit -ArgumentList @('rev-parse', '--show-cdup')).Count) {
            throw "$projectRoot is not the top of its own Git repository."
        }
        $routes = @(Invoke-SourceGit -ArgumentList @('config', '--list', '--name-only', '-z') | ForEach-Object { $_ -split "`n" } | Where-Object { $_ -match '^(remote\..*|extensions\.partialclone)$' })
        if ($routes.Count) {
            throw "The repository could still fetch what the projection leaves out: no Git configuration may set $(Join-Item -Item $routes)."
        }
        $commit = Invoke-SourceGit -ArgumentList @('rev-parse', '--verify', 'HEAD^{commit}')
        $tree = Invoke-SourceGit -ArgumentList @('rev-parse', '--verify', 'HEAD^{tree}')
        if ($inCi -and $commit -cne $env:CI_COMMIT_SHA) {
            throw "The checkout is commit $commit, not the pipeline's commit '$env:CI_COMMIT_SHA'."
        }

        # The commit's files and the index's, by name: mode, type and object; and tag (H checked out, S not), mode,
        # object and stage.
        $inCommit = [System.Collections.Generic.Dictionary[string, string[]]]::new([System.StringComparer]::Ordinal)
        foreach ($entry in Invoke-SourceGit -ArgumentList @('ls-tree', '-r', '-z', '--full-tree', $commit)) {
            $metadata, $path = $entry.Split("`t", 2)
            $inCommit[$path] = $metadata.Split(' ')
        }
        $inIndex = [System.Collections.Generic.Dictionary[string, string[]]]::new([System.StringComparer]::Ordinal)
        foreach ($entry in Invoke-SourceGit -ArgumentList @('ls-files', '--stage', '-t', '-z')) {
            $metadata, $path = $entry.Split("`t", 2)
            $inIndex[$path] = $metadata.Split(' ')
        }

        # The list checked against is the commit's own, byte for byte.
        $listObject = if ($inCommit.ContainsKey($projectionList)) { $inCommit[$projectionList][2] }
        if (-not $listObject -or -not (Get-RegularFile -Paths @($projectionList)).Contains($projectionList) -or (Invoke-SourceGit -ArgumentList @('hash-object', '--no-filters', $projectionList)) -cne $listObject) {
            throw "$projectionList, the list of the job's source projection, is not in the checkout as in commit $commit."
        }
        $listed = @(Get-ProjectionList -Path (Join-Path -Path $projectRoot -ChildPath $projectionList))
        $isListed = [System.Collections.Generic.HashSet[string]]::new([string[]] $listed, [System.StringComparer]::Ordinal)

        $problems = [System.Collections.Generic.List[string]]::new()
        $note = {
            param([string] $What, [string[]] $Paths)
            if ($Paths.Count) {
                $problems.Add("$What`: $(Join-Item -Item $Paths)")
            }
        }
        & $note 'in the index, but not as in the commit' @($inIndex.Keys | Where-Object { -not $inCommit.ContainsKey($_) -or $inIndex[$_][1] -cne $inCommit[$_][0] -or $inIndex[$_][2] -cne $inCommit[$_][2] -or $inIndex[$_][3] -cne '0' })
        & $note 'in the commit, but not in the index' @($inCommit.Keys | Where-Object { -not $inIndex.ContainsKey($_) })
        & $note 'listed, but not a file of the commit' @($listed | Where-Object { -not $inCommit.ContainsKey($_) -or $inCommit[$_][0] -notin '100644', '100755' })
        & $note 'listed, but not checked out' @($listed | Where-Object { -not $inIndex.ContainsKey($_) -or $inIndex[$_][0] -cne 'H' })
        & $note 'checked out, but not listed' @($inIndex.Keys | Where-Object { -not $isListed.Contains($_) -and $inIndex[$_][0] -cne 'S' })
        # The generator reads every file under its input directories, so the commit may have none there that is neither
        # listed nor protected.
        & $note 'among the generator''s inputs in the commit, but not listed' @($inCommit.Keys | Where-Object {
                $path = $_
                -not $isListed.Contains($path) -and $path -cnotin $protectedSources -and @($generatorInputs | Where-Object { $path.StartsWith("$_/", [System.StringComparison]::Ordinal) }).Count
            })

        $regular = Get-RegularFile -Paths $listed
        & $note 'listed, but not a regular file reached without a link' @($listed | Where-Object { -not $regular.Contains($_) })
        $unlisted = @(foreach ($directory in $generatorInputs) {
                $item = Get-Item -LiteralPath (Join-Path -Path $projectRoot -ChildPath $directory) -Force -ErrorAction Ignore
                if ($item -and ($item -isnot [System.IO.DirectoryInfo] -or $item.LinkTarget)) {
                    $directory
                }
                elseif ($item) {
                    Get-ChildItem -LiteralPath $item.FullName -Recurse -Force |
                        Where-Object { $_.LinkTarget -or ($_ -is [System.IO.FileInfo] -and -not $isListed.Contains([System.IO.Path]::GetRelativePath($projectRoot, $_.FullName))) } |
                        ForEach-Object { [System.IO.Path]::GetRelativePath($projectRoot, $_.FullName) }
                }
            })
        & $note 'among the generator''s inputs on disk, but not a listed file' $unlisted
        & $note 'not listed, but on disk' @($inIndex.Keys | Where-Object { -not $isListed.Contains($_) -and $_ -cnotin $protectedSources -and (Test-Entry -Path (Join-Path -Path $projectRoot -ChildPath $_)) })
        & $note 'protected, but on disk' @($protectedSources | Where-Object { Test-Entry -Path (Join-Path -Path $projectRoot -ChildPath $_) })

        # Only listed files are hashed, each against its object in the commit.
        $hashed = @($listed | Where-Object { $regular.Contains($_) -and $inCommit.ContainsKey($_) })
        $objects = @(@(Invoke-SourceGit -ArgumentList @('hash-object', '--no-filters', '--stdin-paths') -InputLine $hashed) -split "`n" | Where-Object { $_ })
        if ($objects.Count -ne $hashed.Count) {
            throw "git hash-object gave $($objects.Count) objects for $($hashed.Count) listed files."
        }
        & $note 'listed, but not as in the commit' @(for ($i = 0; $i -lt $hashed.Count; $i++) { if ($objects[$i] -cne $inCommit[$hashed[$i]][2]) { $hashed[$i] } })

        # Nor may the repository hold a protected source's object: only whether it exists is asked.
        $protected = @(foreach ($path in $protectedSources) {
                $object = if ($inCommit.ContainsKey($path)) { $inCommit[$path][2] }
                $inRepository = $false
                if ($object) {
                    $null = Invoke-SourceGit -ArgumentList @('cat-file', '-e', $object) -Allowed 0, 1
                    $inRepository = $LASTEXITCODE -eq 0
                }
                [ordered]@{ path = $path; object = $object; onDisk = Test-Entry -Path (Join-Path -Path $projectRoot -ChildPath $path); inRepository = $inRepository }
            })
        & $note 'protected, but in the repository' @($protected | Where-Object { $_.inRepository } | ForEach-Object { "$($_.path) ($($_.object))" })

        if ($problems.Count) {
            throw "The checkout is not the job's source projection: $($problems -join '; ')."
        }
        $script:sourceCommit = $commit
        $script:sourceTree = $tree
        [ordered]@{
            commit = $commit
            tree = $tree
            shallow = Invoke-SourceGit -ArgumentList @('rev-parse', '--is-shallow-repository')
            list = [ordered]@{ path = $projectionList; object = $listObject; sha256 = Get-Sha256 -Path (Join-Path -Path $projectRoot -ChildPath $projectionList); paths = $listed.Count }
            commitFiles = $inCommit.Count
            checkedOut = $listed.Count
            asInCommit = $hashed.Count
            protected = $protected
            fetch = 'no remote or partial clone in any Git configuration'
        }
    }

    # The schema comes only from the file FILESCOM_CI_SCHEMA names, in CI as elsewhere; nothing downloads one. The
    # capture's checksum and record beside it are kept with it, for the engine step to check.
    Invoke-Step -Name 'schema' -Action {
        if (-not $env:FILESCOM_CI_SCHEMA -or -not (Test-Path -LiteralPath $env:FILESCOM_CI_SCHEMA -PathType Leaf)) {
            throw "FILESCOM_CI_SCHEMA must name a file holding the captured production schema, but it is '$env:FILESCOM_CI_SCHEMA'. The job does not fetch the schema."
        }
        $capture = Join-Path -Path $EvidencePath -ChildPath 'swagger_doc-production.json'
        Copy-Item -LiteralPath $env:FILESCOM_CI_SCHEMA -Destination $capture
        $source = $env:FILESCOM_CI_SCHEMA
        $sha256 = Get-Sha256 -Path $capture
        if ($script:engineInput -eq 'run') {
            foreach ($sibling in '.sha256', '.provenance.json') {
                if (Test-Path -LiteralPath "$source$sibling" -PathType Leaf) {
                    Copy-Item -LiteralPath "$source$sibling" -Destination "$capture$sibling"
                }
            }
        }
        else {
            Set-Content -LiteralPath "$capture.sha256" -Value "$sha256  swagger_doc-production.json" -Encoding utf8
            if ($sha256 -ne $engine.schema_sha256) {
                throw "The production schema is $sha256, but engine.json says the engine was generated from $($engine.schema_sha256). Commands generated from this schema would not match the engine: rebind the engine to the current schema first."
            }
        }
        $script:schemaPath = $capture
        [ordered]@{ sha256 = $sha256; source = $source }
    }

    # The engine is the package the job's pipeline built from this capture, once build/Resolve-RunEngine.ps1 finds that
    # they join; or, for a local check, the package engine.json pins, once it and its record match the pins.
    Invoke-Step -Name 'engine' -Action {
        $download = New-Item -ItemType Directory -Path (Join-Path -Path $WorkPath -ChildPath 'engine-download')
        $provenance = Join-Path -Path $EvidencePath -ChildPath 'engine-provenance.json'
        if ($script:engineInput -eq 'run') {
            $inputs = [ordered]@{ archive = $env:FILESCOM_CI_PACKAGE; record = $env:FILESCOM_CI_PACKAGE_PROVENANCE }
            $archive = Join-Path -Path $download.FullName -ChildPath ([System.IO.Path]::GetFileName($inputs.archive))
            Copy-Item -LiteralPath $inputs.record -Destination $provenance
            Copy-Item -LiteralPath $inputs.archive -Destination $archive
            $run = & (Join-Path -Path $PSScriptRoot -ChildPath 'Resolve-RunEngine.ps1') -SchemaPath $script:schemaPath -PackagePath $archive -ProvenancePath $provenance -LockPath (Join-Path -Path $target -ChildPath $engineLock) -SourceCommit $script:sourceCommit -SourceTree $script:sourceTree -PipelineId $env:CI_PIPELINE_ID -CommitSha $env:CI_COMMIT_SHA
            if ($run.mismatches.Count) {
                throw "The schema capture and the engine package do not join, so the build does not use them: $($run.mismatches -join '; ')."
            }
            $script:engine = $run.engine
            $script:engineLockText = $run.lock
        }
        else {
            $inputs = [ordered]@{ archive = $env:FILESCOM_CI_ENGINE_ARCHIVE; record = $env:FILESCOM_CI_ENGINE_PROVENANCE }
            $archive = Join-Path -Path $download.FullName -ChildPath "$($engine.package).$($engine.version).nupkg"
            Copy-Item -LiteralPath $inputs.record -Destination $provenance
            Copy-Item -LiteralPath $inputs.archive -Destination $archive
            $mismatches = @(Get-EngineMismatch -ArchivePath $archive -ProvenancePath $provenance)
            if ($mismatches.Count) {
                throw "The engine package or its record does not match the pins, so the build does not use it: $($mismatches -join '; ')."
            }
            $script:engine = $engine
        }
        # The build's feed holds this one checked package and nothing else.
        $feed = New-Item -ItemType Directory -Path (Join-Path -Path $WorkPath -ChildPath 'engine-feed')
        Move-Item -LiteralPath $archive -Destination $feed.FullName
        $script:engineFeed = $feed.FullName
        $record = Get-Content -LiteralPath $provenance -Raw | ConvertFrom-Json
        [ordered]@{
            input = $script:engineInput
            archive = [ordered]@{ path = $inputs.archive; sha256 = $script:engine.sha256; bytes = $record.package.archive.bytes }
            record = [ordered]@{ path = $inputs.record; sha256 = Get-Sha256 -Path $provenance }
            producer = [ordered]@{ commit = $record.source.commit; tree = $record.source.tree; schema = $record.schema.sha256 }
            engine = if ($script:engineInput -eq 'run') { $script:engine } else { 'as engine.json pins' }
        }
    }

    Invoke-Step -Name 'generate' -Action {
        if (-not $env:BUNDLE_PATH) {
            $env:BUNDLE_PATH = Join-Path -Path $projectRoot -ChildPath 'vendor/bundle'
        }
        $env:BUNDLE_FROZEN = 'true'
        Push-Location -LiteralPath $projectRoot
        try {
            Invoke-Checked -FilePath 'bundle' -ArgumentList @('install') -LogPath (Join-Path -Path $logs.FullName -ChildPath 'bundle-install.log')
            # One run makes the simulator, with its own setup, and this target, both from the checked capture.
            Invoke-Checked -FilePath 'bundle' -ArgumentList @('exec', 'ruby', 'files-sdk-generator.rb', '--target', 'mock-server,powershell', '--file', $script:schemaPath) -LogPath (Join-Path -Path $logs.FullName -ChildPath 'generate.log')
        }
        finally {
            Pop-Location
        }
        # The projection has no source for these, so the generator must not have made them, and none may be left over.
        $made = @($protectedOutputs | Where-Object { Test-Entry -Path (Join-Path -Path $projectRoot -ChildPath $_) })
        if ($made.Count) {
            throw "After generation the job has $($made -join ', '), which the source projection has no source for."
        }
        $inventory = Get-Content -LiteralPath (Join-Path -Path $generated -ChildPath 'docs/command-inventory.json') -Raw | ConvertFrom-Json
        if ($inventory.schema_sha256 -ne $script:engine.schema_sha256) {
            throw "The commands were generated from schema $($inventory.schema_sha256), not $($script:engine.schema_sha256)."
        }
        # The simulator's generation record names the raw capture's exact bytes (swagger.sha256), as the API scenarios'
        # cases do; its normalized simulation schema (schema_sha256) is the one its readiness reports.
        $captureSha256 = Get-Sha256 -Path $script:schemaPath
        $simulator = Get-Content -LiteralPath (Join-Path -Path $mockServer -ChildPath 'lib/simulation/generation.json') -Raw | ConvertFrom-Json
        if ($simulator.swagger.sha256 -cne $captureSha256) {
            throw "The simulator was generated from schema $($simulator.swagger.sha256), not the capture $captureSha256."
        }
        $cases = Get-Content -LiteralPath (Join-Path -Path $generated -ChildPath 'verification/api-scenarios/cases.json') -Raw | ConvertFrom-Json
        if ($cases.schema_sha256 -cne $captureSha256) {
            throw "The API scenario cases were generated from schema $($cases.schema_sha256), not the capture $captureSha256."
        }
        $engineFiles = 'as committed'
        if ($script:engineInput -eq 'run') {
            # The generated target builds on the run's engine: its engine.json names the package, and its engine lock pins
            # the package's version and content hash, with every other locked input as committed. Only these generated
            # copies change, and the lock only when the generator copied the committed one the engine was checked against.
            $generatedEngine = Join-Path -Path $generated -ChildPath 'engine.json'
            $generatedLock = Join-Path -Path $generated -ChildPath $engineLock
            if ((Get-Sha256 -Path $generatedLock) -ne (Get-Sha256 -Path (Join-Path -Path $target -ChildPath $engineLock))) {
                throw "The generated $engineLock is not the committed one the engine was checked against."
            }
            $runEngine = [ordered]@{
                comment = 'The FilesCom package this run built the module on: the one its pipeline built from the schema capture the module was generated from, as build/Resolve-RunEngine.ps1 checked. The ordinary CI job wrote this file in place of the committed engine.json.'
                package = $script:engine.package
                version = $script:engine.version
                sha256 = $script:engine.sha256
                source = $script:engine.source
                source_tree = $script:engine.source_tree
                schema_sha256 = $script:engine.schema_sha256
            }
            Set-Content -LiteralPath $generatedEngine -Value (ConvertTo-Json -InputObject $runEngine) -Encoding utf8
            Set-Content -LiteralPath $generatedLock -Value $script:engineLockText -Encoding utf8
            Copy-Item -LiteralPath $generatedEngine -Destination (Join-Path -Path $EvidencePath -ChildPath 'engine.json')
            Copy-Item -LiteralPath $generatedLock -Destination (Join-Path -Path $EvidencePath -ChildPath 'engine.packages.lock.json')
            $engineFiles = [ordered]@{ engine = Get-Sha256 -Path $generatedEngine; lock = Get-Sha256 -Path $generatedLock }
        }
        [ordered]@{
            files = @(Get-ChildItem -LiteralPath $generated -File -Recurse).Count
            inventorySchema = $inventory.schema_sha256
            absent = $protectedOutputs
            engineFiles = $engineFiles
            simulator = [ordered]@{ swaggerSha256 = $simulator.swagger.sha256; schemaSha256 = $simulator.schema_sha256 }
            apiScenarios = [ordered]@{ definitionSha256 = $cases.definition.sha256; scenarios = @($cases.scenarios).Count }
        }
    }

    Invoke-Step -Name 'build' -Action {
        $env:FILESCOM_ENGINE_FEED = $script:engineFeed
        $env:FILESCOM_TOOLS = Join-Path -Path $WorkPath -ChildPath 'tools'
        $env:FILESCOM_PWSH = $pwsh
        $env:NUGET_PACKAGES = Join-Path -Path $WorkPath -ChildPath 'nuget/build'
        Invoke-Checked -FilePath 'bash' -ArgumentList @((Join-Path -Path $generated -ChildPath 'build.sh')) -LogPath (Join-Path -Path $logs.FullName -ChildPath 'build.log')
        $version = (Get-Content -LiteralPath (Join-Path -Path $generated -ChildPath '_VERSION') -Raw).Trim()
        $moduleRoot = Join-Path -Path $generated -ChildPath "out/FilesCom.PowerShell/$version"
        $package = Get-Item -LiteralPath (Join-Path -Path $generated -ChildPath "out/packages/FilesCom.PowerShell.$version.nupkg")
        $buildInfo = Get-Content -LiteralPath (Join-Path -Path $moduleRoot -ChildPath 'FilesCom.BuildInfo.json') -Raw | ConvertFrom-Json
        if ($buildInfo.engine.sha256 -ne $script:engine.sha256 -or $buildInfo.engine.version -ne $script:engine.version -or $buildInfo.engine.source -ne $script:engine.source -or $buildInfo.schemaSha256 -ne $script:engine.schema_sha256) {
            throw "The module's BuildInfo records engine $($buildInfo.engine.version) $($buildInfo.engine.sha256) from $($buildInfo.engine.source) and schema $($buildInfo.schemaSha256), not the engine and schema the job checked."
        }
        $paths = [string[]] @(Get-ChildItem -LiteralPath $moduleRoot -File -Recurse | ForEach-Object { [System.IO.Path]::GetRelativePath($moduleRoot, $_.FullName) })
        [System.Array]::Sort($paths, [System.StringComparer]::Ordinal)
        $payload = @($paths | ForEach-Object { "$(Get-Sha256 -Path (Join-Path -Path $moduleRoot -ChildPath $_))  $_" })
        Set-Content -LiteralPath (Join-Path -Path $EvidencePath -ChildPath 'module-payload.sha256') -Value $payload -Encoding utf8
        Copy-Item -LiteralPath $package.FullName -Destination $EvidencePath
        [ordered]@{
            package = [ordered]@{ file = $package.Name; sha256 = Get-Sha256 -Path $package.FullName; bytes = $package.Length }
            buildInfoSha256 = Get-Sha256 -Path (Join-Path -Path $moduleRoot -ChildPath 'FilesCom.BuildInfo.json')
            files = $payload.Count
            engine = $buildInfo.engine
        }
    }

    $testScripts = @($ordinarySuites) + @('TestSupport.ps1', 'PSScriptAnalyzerSettings.psd1')
    Invoke-Step -Name 'analysis' -Action {
        $analysis = Join-Path -Path $generated -ChildPath 'build/Invoke-Analysis.ps1'
        Invoke-Checked -FilePath $pwsh -ArgumentList @('-NoProfile', '-NonInteractive', '-Command', "& $(ConvertTo-Literal $analysis) -ToolsPath $(ConvertTo-Literal $env:FILESCOM_TOOLS) -TestScript $(ConvertTo-Literal $testScripts)") -LogPath (Join-Path -Path $logs.FullName -ChildPath 'analysis.log')
        [ordered]@{ testScripts = $testScripts }
    }

    Invoke-Step -Name 'unprivileged user' -Action {
        $user = Join-Path -Path $WorkPath -ChildPath 'test-user'
        $environment = @(
            "HOME=$user/home", "TMPDIR=$user/tmp", "DOTNET_CLI_HOME=$user/dotnet", "NUGET_PACKAGES=$user/nuget",
            "XDG_CONFIG_HOME=$user/config", "XDG_CACHE_HOME=$user/cache", "XDG_DATA_HOME=$user/data")
        foreach ($setting in $environment) {
            New-Item -ItemType Directory -Path $setting.Split('=', 2)[1] -Force | Out-Null
        }
        $results = New-Item -ItemType Directory -Path (Join-Path -Path $EvidencePath -ChildPath 'tests')
        # The API scenarios' runs, each PowerShell's in a folder of its own, which the same user writes.
        $canonical = New-Item -ItemType Directory -Path (Join-Path -Path $EvidencePath -ChildPath 'canonical')
        # The tests get a fixed environment: no CI token or other job secret reaches them.
        $testEnvironment = @(foreach ($name in 'PATH', 'LANG', 'LC_ALL', 'DOTNET_ROOT', 'DOTNET_CLI_TELEMETRY_OPTOUT', 'DOTNET_NOLOGO', 'DOTNET_SKIP_FIRST_TIME_EXPERIENCE', 'POWERSHELL_TELEMETRY_OPTOUT', 'POWERSHELL_UPDATECHECK') {
                $value = [System.Environment]::GetEnvironmentVariable($name)
                if ($value) { "$name=$value" }
            }) + @('DOTNET_GENERATE_ASPNET_CERTIFICATE=false') + $environment
        if ($script:testUserMode -eq 'made for the run') {
            foreach ($name in $testGroupNames) {
                Invoke-Checked -FilePath 'groupadd' -ArgumentList @($name)
            }
            $groupIds = @($testGroupNames | ForEach-Object { (& getent group $_).Split(':')[2] })
            foreach ($path in $user, $generated, $results.FullName, $canonical.FullName, $mockServer) {
                Invoke-Checked -FilePath 'chown' -ArgumentList @('-R', "${testUserId}:$($groupIds[0])", $path)
            }
            $script:testLauncher = @('setpriv', "--reuid=$testUserId", "--regid=$($groupIds[0])", "--groups=$($groupIds[1])", '--inh-caps=-all', '--no-new-privs', '--', 'env', '-i') + $testEnvironment
            $expected = [ordered]@{ uid = "$testUserId"; groups = $groupIds -join ' ' }
        }
        else {
            # The job's own user, already unprivileged and in two groups, owns everything the tests write.
            $script:testLauncher = @('env', '-i') + $testEnvironment
            $expected = [ordered]@{ uid = [string] (& id -u); groups = [string] (& id -G) }
        }
        Invoke-Checked -FilePath 'chmod' -ArgumentList @('700', "$user/tmp")
        $launcherArguments = @($script:testLauncher | Select-Object -Skip 1)
        $userId = [string] (& $script:testLauncher[0] ($launcherArguments + @('id', '-u')))
        $userGroups = [string] (& $script:testLauncher[0] ($launcherArguments + @('id', '-G')))
        if ($userId -ne $expected.uid -or $userId -eq '0' -or $userGroups -ne $expected.groups -or @($userGroups -split ' ').Count -lt 2) {
            throw "The tests would run as uid '$userId' in groups '$userGroups', not unprivileged uid $($expected.uid) in groups $($expected.groups)."
        }
        [ordered]@{ mode = $script:testUserMode; uid = $userId; groups = $userGroups }
    }

    $testPaths = @($ordinarySuites | ForEach-Object { Join-Path -Path $generated -ChildPath "tests/$_" })
    foreach ($hostVersion in $hostVersions) {
        Invoke-Step -Name "tests on PowerShell $hostVersion" -Action {
            $results = Join-Path -Path $EvidencePath -ChildPath "tests/$hostVersion"
            $testModule = Join-Path -Path $generated -ChildPath 'build/Test-Module.ps1'
            $hostPath = Join-Path -Path $WorkPath -ChildPath "powershell/$hostVersion/pwsh"
            $command = "& $(ConvertTo-Literal $testModule) -ToolsPath $(ConvertTo-Literal $env:FILESCOM_TOOLS) -PowerShell $(ConvertTo-Literal $hostPath) -ModulePath $(ConvertTo-Literal (Join-Path -Path $generated -ChildPath 'out')) -ResultsPath $(ConvertTo-Literal $results) -TestPath $(ConvertTo-Literal $testPaths)"
            Invoke-Checked -FilePath $script:testLauncher[0] -ArgumentList (@($script:testLauncher | Select-Object -Skip 1) + @($pwsh, '-NoProfile', '-NonInteractive', '-Command', $command)) -LogPath (Join-Path -Path $logs.FullName -ChildPath "tests-$hostVersion.log")
            [ordered]@{ results = "tests/$hostVersion/pester-$hostVersion.xml"; suites = Assert-TestResult -Path (Join-Path -Path $results -ChildPath "pester-$hostVersion.xml") -Suites $ordinarySuites }
        }
        # The shared API scenarios, through the module Test-Module.ps1 just installed from the package, on the same
        # PowerShell and as the same user. The launcher and its simulator get the generator's bundle by name besides the
        # tests' fixed environment, and the consumer only the launcher's FILES_MOCK_* values more: no CI token or other
        # job secret reaches either.
        Invoke-Step -Name "API scenarios on PowerShell $hostVersion" -Action {
            $version = (Get-Content -LiteralPath (Join-Path -Path $generated -ChildPath '_VERSION') -Raw).Trim()
            $outPath = Join-Path -Path $EvidencePath -ChildPath "canonical/$hostVersion"
            $hostPath = Join-Path -Path $WorkPath -ChildPath "powershell/$hostVersion/pwsh"
            $scenarios = @(
                $hostPath, '-NoProfile', '-NonInteractive', '-File', (Join-Path -Path $generated -ChildPath 'build/Invoke-ApiScenarios.ps1'),
                '-ModuleManifest', (Join-Path -Path $generated -ChildPath "out/installed/FilesCom.PowerShell/$version/FilesCom.PowerShell.psd1"),
                '-PackagePath', (Join-Path -Path $generated -ChildPath "out/packages/FilesCom.PowerShell.$version.nupkg"),
                '-MockServer', $mockServer,
                '-ScenarioRoot', (Join-Path -Path $generated -ChildPath 'verification/api-scenarios'),
                '-DefinitionPath', (Join-Path -Path $projectRoot -ChildPath 'verification/scenarios/api.json'),
                '-SchemaSha256', (Get-Sha256 -Path $script:schemaPath),
                '-OutPath', $outPath)
            Invoke-Checked -FilePath $script:testLauncher[0] -ArgumentList (@($script:testLauncher | Select-Object -Skip 1) + @("BUNDLE_PATH=$env:BUNDLE_PATH") + $scenarios) -LogPath (Join-Path -Path $logs.FullName -ChildPath "api-scenarios-$hostVersion.log")
            $runs = Get-Content -LiteralPath (Join-Path -Path $outPath -ChildPath 'summary.json') -Raw | ConvertFrom-Json
            if ($runs.result -cne 'passed') {
                throw "The API scenarios on PowerShell $hostVersion did not pass: see canonical/$hostVersion/summary.json."
            }
            [ordered]@{ summary = "canonical/$hostVersion/summary.json"; runs = @($runs.runs | ForEach-Object { [ordered]@{ name = $_.name; result = $_.result } }) }
        }
    }
    $summary.result = 'passed'
}
catch {
    $summary.result = 'failed'
    $summary.error = $_.Exception.Message
    throw
}
finally {
    $summary.finished = [System.DateTime]::UtcNow.ToString('o')
    $summary.resources = [ordered]@{}
    foreach ($name in 'memory.peak', 'memory.events') {
        $file = "/sys/fs/cgroup/$name"
        $summary.resources[$name] = if (Test-Path -LiteralPath $file) { (Get-Content -LiteralPath $file -Raw).Trim() } else { 'not available' }
    }
    $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path -Path $EvidencePath -ChildPath 'summary.json') -Encoding utf8
}
