<#
.SYNOPSIS
The control of build/Resolve-RunEngine.ps1: runs it on synthetic schema captures, FilesCom packages and package
records, and checks what it returns.

.DESCRIPTION
    pwsh -NoProfile -NonInteractive -File build/test-run-engine.ps1 -Work <dir> [-Only <case>[,<case>...]]

Each case makes a fixture in <dir>/<case>, as the .NET build keeps a package with its schema capture: a synthetic
public schema capture, schema/swagger_doc-production.json, with its checksum and its record; a synthetic
FilesCom.2.0.1.nupkg, its members stored without compression; generated-input.sha256, a synthetic manifest of the
generated input; and package-provenance.json, the record of what that package was built from.

The capture's record has the keys and values capture-schema.sh writes: format gollum-schema-capture/1, flavor public,
source, sha256, bytes as a number, captured_at, and pipeline_id, job_id and commit_sha as strings. The package's record
has those package-provenance.rb writes: format filescom-dotnet-package-provenance/1, at, producer and paths; package,
with its id, version and target frameworks, its archive's file name, size, SHA-256 and SHA-512, in hexadecimal and in
Base64, and the SHA-256 of each lib member; source, its commit and tree; schema, with the capture's path, SHA-256 and
size, the capture's whole record and that record's SHA-256, and the checksum file's path and SHA-256; generated_input,
with the manifest's path, SHA-256, size and count of files; build, with its commands, a list of each command's name and
exit code, every one 0, and its pipeline, job and commit as strings; join; and not_claimed. Each path it gives is
relative to the record's directory.

Unless the case says otherwise, every hash, size and name either record gives is that of the file it describes, both
records name one synthetic commit and pipeline, and the nuspec's dependency groups are the FilesCom dependencies the
committed engine lock, src/FilesCom.PowerShell.Engine/packages.lock.json, gives netstandard2.0, net8.0 and net10.0. The
case runs the Resolve-RunEngine.ps1 beside this file on the fixture and that lock, and checks what it returns. A
mismatch is known by its label, its text before the first ': '.

- match: as the fixture's pipeline and commit, Resolve-RunEngine.ps1 reports no mismatch, though the capture's job is
  not the build's. The engine gives the package, its version, hashes and size, the source commit and tree, the schema,
  the capture's and the build's pipeline, job and commit, and the hashes of both records, each as the fixture has it.
  The lock is the committed one, but for each FilesCom entry's requested and resolved version and content hash, which
  are the package's.
- match-local: given no pipeline or commit, it reports no mismatch either, and gives an engine and a lock.
- local-unknown: on a local pack, a fixture whose records give the capture's and the build's pipeline, job and commit
  as null, as a pack outside CI leaves them unknown, and given -LocalPack and no pipeline or commit, it reports no
  mismatch. The engine is as in match, but the capture's and the build's pipeline, job and commit are each null, never
  an empty string.
- In each of these cases the fixture or the arguments have one fault, and Resolve-RunEngine.ps1 reports exactly the
  labels named and gives neither engine nor lock:
  - source-commit, another source commit, and ci-commit, another pipeline commit: capture commit, producer commit and
    build commit;
  - source-tree, another source tree: producer tree;
  - pipeline, another pipeline: capture pipeline and build pipeline;
  - build-pipeline, a package built in another pipeline: build pipeline;
  - schema-bytes, a capture changed at one byte: capture checksum, capture SHA-256 and producer schema;
  - flavor, a capture of the internal flavor, and flavor-case, one whose flavor is PUBLIC: capture flavor;
  - capture-record, a capture record changed after the package record was made: producer capture and producer
    capture SHA-256;
  - package-bytes, a package member changed after the package record was made: archive SHA-256, archive SHA-512 and
    package members;
  - member, a package record with another hash for a member: package members;
  - nuspec, a nuspec of another version: package nuspec;
  - file-name, the package under another version's file name: package file;
  - dependencies, a nuspec with a dependency the lock does not have: package dependencies;
  - build-exit, a package record whose pack command exited 1: build commands;
  - commands-shape, a package record whose commands are an object of exit codes by command name, not a list: build
    commands;
  - manifest-shape, a package record that gives the generated input manifest's SHA-256 alone, as manifest_sha256:
    generated manifest;
  - record-format, a package record of another format: record format;
  - local-unknown-needs-switch, that local pack given no -LocalPack: capture fields, capture commit, build commit and
    build job;
  - local-known-mismatch, that local pack with another build commit, given -LocalPack: build commit;
  - local-with-pipeline, that local pack given -LocalPack and a pipeline: local pack.
- missing-record: given no capture record, Resolve-RunEngine.ps1 reports capture record and no other label about that
  record, and gives neither engine nor lock.
- No case changes the committed lock.

First, a check named fixture requires the committed lock to give FilesCom the same dependencies for net48 as for
netstandard2.0: the fixture's nuspec has no net48 group, so its netstandard2.0 group must serve both.

-Only runs just the named cases, after the fixture check. Any other or repeated name, or none, is refused.

<dir> must not exist. The control makes it, and leaves it, with each case's fixture, for inspection. Needs PowerShell
7.4 or later. It uses no network, dotnet or git, and of this repository it runs Resolve-RunEngine.ps1 and reads the
committed lock, and nothing else. It prints one line a check, 'ok <case>: <what>' or
'not ok <case>: <what>: <details>', then how many passed, and exits 0 only when every check passed, 1 when any did
not, and 2 when it refuses its arguments.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Work,

    [string[]] $Only
)

#Requires -Version 7.4
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression

# Synthetic commits and trees, 40 hexadecimal digits each: the fixture's, and others for the cases that give another.
$commit = 'c0' * 20
$tree = 'e0' * 20
$otherCommit = 'c1' * 20
$otherTree = 'e1' * 20

# The capture: a small JSON schema on one line.
$captureText = '{"swagger":"2.0","info":{"title":"fixture","version":"2.0.1","description":"A synthetic schema for the control of Resolve-RunEngine.ps1."},"paths":{}}' + "`n"

# The nuspec's dependency groups, by target framework, each with the FilesCom dependencies the committed lock gives
# this lock framework. Resolve-RunEngine.ps1 checks .NETFramework,Version=v4.8 against the .NETStandard2.0 group, as
# the nuspec has no .NETFramework4.8 one.
$nuspecGroups = [ordered]@{ '.NETStandard2.0' = '.NETStandard,Version=v2.0'; 'net8.0' = 'net8.0'; 'net10.0' = 'net10.0' }

$utf8 = [System.Text.UTF8Encoding]::new($false)
# Every package member is dated alike, so that only its name and bytes make the archive.
$memberTime = [System.DateTimeOffset]::new(2026, 10, 6, 0, 0, 0, [System.TimeSpan]::Zero)
# How many checks passed and failed.
$checks = [ordered]@{ passed = 0; failed = 0 }

# Prints a check's line, 'ok <case>: <what>' or 'not ok <case>: <what>: <details>', and counts it.
function Write-Check {
    param([string] $Case, [string] $What, $Passed, [string] $Details)
    if ($Passed) {
        $checks['passed'] += 1
        Write-Output -InputObject "ok ${Case}: $What"
    }
    else {
        $checks['failed'] += 1
        Write-Output -InputObject "not ok ${Case}: ${What}: $Details"
    }
}

function Get-Sha256 {
    param([string] $Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

# The lowercase hexadecimal SHA-256 of Byte, or with -Sha512 its SHA-512.
function Get-ByteHash {
    param([byte[]] $Byte, [switch] $Sha512)
    if ($Sha512) {
        $hash = [System.Security.Cryptography.SHA512]::HashData($Byte)
    }
    else {
        $hash = [System.Security.Cryptography.SHA256]::HashData($Byte)
    }
    [System.Convert]::ToHexString($hash).ToLowerInvariant()
}

# Writes Text to Path in UTF-8, without a byte order mark.
function Write-TextFile {
    param([string] $Path, [string] $Text)
    [System.IO.File]::WriteAllText($Path, $Text, $utf8)
}

# Byte with the byte at Index changed: as many bytes, but not the same ones.
function ConvertTo-ChangedByte {
    param([byte[]] $Byte, [int] $Index)
    $changed = $Byte.Clone()
    $changed[$Index] = $changed[$Index] -bxor 1
    , $changed
}

# Writes a zip archive at Path of Member's entries, name to bytes, in order. Each is stored without compression and
# dated alike, so that changing a member's bytes for as many others leaves the archive's size as it was.
function Write-Archive {
    param([string] $Path, [System.Collections.IDictionary] $Member)
    $stream = [System.IO.FileStream]::new($Path, [System.IO.FileMode]::Create)
    try {
        $archive = [System.IO.Compression.ZipArchive]::new($stream, [System.IO.Compression.ZipArchiveMode]::Create)
        try {
            foreach ($item in $Member.GetEnumerator()) {
                $entry = $archive.CreateEntry($item.Key, [System.IO.Compression.CompressionLevel]::NoCompression)
                $entry.LastWriteTime = $memberTime
                $entryStream = $entry.Open()
                try {
                    $entryStream.Write($item.Value, 0, $item.Value.Length)
                }
                finally {
                    $entryStream.Dispose()
                }
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

# A FilesCom nuspec of Version, with a dependency group for each of Group's target frameworks: id to version.
function ConvertTo-Nuspec {
    param([string] $Version, [System.Collections.IDictionary] $Group)
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">')
    $lines.Add('  <metadata>')
    $lines.Add('    <id>FilesCom</id>')
    $lines.Add("    <version>$Version</version>")
    $lines.Add('    <authors>Files.com</authors>')
    $lines.Add('    <description>A synthetic package for the control of Resolve-RunEngine.ps1.</description>')
    $lines.Add('    <dependencies>')
    foreach ($framework in $Group.GetEnumerator()) {
        $line = '      <group targetFramework="{0}">' -f $framework.Key
        $lines.Add($line)
        foreach ($dependency in $framework.Value.GetEnumerator()) {
            $line = '        <dependency id="{0}" version="{1}" />' -f $dependency.Key, $dependency.Value
            $lines.Add($line)
        }
        $lines.Add('      </group>')
    }
    $lines.Add('    </dependencies>')
    $lines.Add('  </metadata>')
    $lines.Add('</package>')
    ($lines -join "`n") + "`n"
}

# A fixture in Path that Resolve-RunEngine.ps1 must accept as the fixture's commit, tree and pipeline: the capture with
# its checksum and record, the package, the generated input manifest and the package's record, every hash, size and
# name in either record that of the file it describes. A fault given here is made before the package record, which is
# made to match it: -Flavor, the capture record's flavor; -NuspecVersion, the version the nuspec gives;
# -ExtraDependency, a dependency added to the nuspec's net8.0 group; -EditRecord, a change to the package record just
# before it is written. -NoCaptureRecord removes the capture's record once the package record is made. -UnknownCi
# makes the pipeline, job and commit of both records, the capture's and the build's, null, as a pack outside CI leaves
# them, before -EditRecord. Returns the paths made, the capture record and the package's members, for a case to change
# after.
function Build-Fixture {
    param(
        [string] $Path,
        [string] $Flavor = 'public',
        [string] $NuspecVersion = '2.0.1',
        [switch] $ExtraDependency,
        [scriptblock] $EditRecord,
        [switch] $NoCaptureRecord,
        [switch] $UnknownCi
    )
    $null = [System.IO.Directory]::CreateDirectory($Path)

    # The capture, its checksum and its record, in schema/ as the package record names them.
    $schemaDirectory = Join-Path -Path $Path -ChildPath 'schema'
    $null = [System.IO.Directory]::CreateDirectory($schemaDirectory)
    $schemaPath = Join-Path -Path $schemaDirectory -ChildPath 'swagger_doc-production.json'
    $capture = $utf8.GetBytes($captureText)
    [System.IO.File]::WriteAllBytes($schemaPath, $capture)
    $captureSha256 = Get-ByteHash -Byte $capture
    Write-TextFile -Path "$schemaPath.sha256" -Text "$captureSha256  swagger_doc-production.json`n"
    $captureRecord = [ordered]@{
        format = 'gollum-schema-capture/1'
        flavor = $Flavor
        source = 's3://fixture-bucket/swagger_doc-production.json'
        sha256 = $captureSha256
        bytes = $capture.Length
        captured_at = '2026-10-06T16:00:00Z'
        pipeline_id = '12345'
        job_id = '101'
        commit_sha = $commit
    }
    if ($UnknownCi) {
        foreach ($key in 'pipeline_id', 'job_id', 'commit_sha') {
            $captureRecord[$key] = $null
        }
    }
    $captureRecordPath = "$schemaPath.provenance.json"
    Write-TextFile -Path $captureRecordPath -Text (ConvertTo-Json -InputObject $captureRecord -Depth 32)

    # The package: its nuspec, and for each target framework a synthetic assembly and documentation file.
    $groups = [ordered]@{}
    foreach ($group in $nuspecGroups.GetEnumerator()) {
        $dependencies = [ordered]@{}
        foreach ($dependency in $committedLock['dependencies'][$group.Value]['FilesCom']['dependencies'].GetEnumerator()) {
            $dependencies[$dependency.Key] = $dependency.Value
        }
        if ($ExtraDependency -and $group.Key -eq 'net8.0') {
            $dependencies['Extra.Fixture'] = '1.0.0'
        }
        $groups[$group.Key] = $dependencies
    }
    $members = [ordered]@{
        'FilesCom.nuspec' = $utf8.GetBytes((ConvertTo-Nuspec -Version $NuspecVersion -Group $groups))
        '[Content_Types].xml' = $utf8.GetBytes('<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types" />')
    }
    foreach ($framework in 'net10.0', 'net8.0', 'netstandard2.0') {
        $members["lib/$framework/FilesCom.dll"] = $utf8.GetBytes("synthetic FilesCom assembly for $framework")
        $members["lib/$framework/FilesCom.xml"] = $utf8.GetBytes("<doc><assembly><name>FilesCom</name></assembly><members /><!-- $framework --></doc>")
    }
    $libMembers = [ordered]@{}
    foreach ($member in $members.GetEnumerator()) {
        if ($member.Key.StartsWith('lib/', [System.StringComparison]::Ordinal)) {
            $libMembers[$member.Key] = Get-ByteHash -Byte $member.Value
        }
    }
    $packagePath = Join-Path -Path $Path -ChildPath 'FilesCom.2.0.1.nupkg'
    Write-Archive -Path $packagePath -Member $members
    $archive = [System.IO.File]::ReadAllBytes($packagePath)

    # The manifest of the generated input: one synthetic file's line.
    $projectSha256 = Get-ByteHash -Byte ($utf8.GetBytes('<Project Sdk="Microsoft.NET.Sdk" />'))
    $manifest = $utf8.GetBytes("$projectSha256  sdk/FilesCom/FilesCom.csproj`n")
    [System.IO.File]::WriteAllBytes((Join-Path -Path $Path -ChildPath 'generated-input.sha256'), $manifest)

    # The record of what the package was built from. Its build commands are a list, each with its name and exit code.
    $record = [ordered]@{
        format = 'filescom-dotnet-package-provenance/1'
        at = '2026-10-06T16:10:00Z'
        producer = 'targets/dotnet/build.sh with package-provenance.rb, after every step of its recipe succeeded'
        paths = 'relative to the directory holding this file'
        package = [ordered]@{
            id = 'FilesCom'
            version = '2.0.1'
            target_frameworks = @('net10.0', 'net8.0', 'netstandard2.0')
            archive = [ordered]@{
                path = 'FilesCom.2.0.1.nupkg'
                sha256 = Get-ByteHash -Byte $archive
                bytes = $archive.Length
                sha512_hex = Get-ByteHash -Byte $archive -Sha512
                sha512_base64 = [System.Convert]::ToBase64String([System.Security.Cryptography.SHA512]::HashData($archive))
            }
            lib_members = $libMembers
        }
        source = [ordered]@{ commit = $commit; tree = $tree }
        schema = [ordered]@{
            path = 'schema/swagger_doc-production.json'
            sha256 = $captureSha256
            bytes = $capture.Length
            capture_provenance = $captureRecord
            capture_provenance_sha256 = Get-Sha256 -Path $captureRecordPath
            capture_sha256_file = [ordered]@{
                path = 'schema/swagger_doc-production.json.sha256'
                sha256 = Get-Sha256 -Path "$schemaPath.sha256"
            }
        }
        generated_input = [ordered]@{
            manifest = [ordered]@{
                path = 'generated-input.sha256'
                sha256 = Get-ByteHash -Byte $manifest
                bytes = $manifest.Length
                files = 1
            }
        }
        build = [ordered]@{
            commands = @(
                [ordered]@{ name = 'format sdk/FilesCom/FilesCom.csproj'; 'exit' = 0 }
                [ordered]@{ name = 'build sdk/FilesCom/FilesCom.csproj'; 'exit' = 0 }
                [ordered]@{ name = 'pack sdk/FilesCom/FilesCom.csproj'; 'exit' = 0 }
                [ordered]@{ name = 'build examples/client-journey/client-journey.csproj'; 'exit' = 0 }
            )
            ci = [ordered]@{ pipeline_id = '12345'; job_id = '202'; commit_sha = $commit }
        }
        join = [ordered]@{ pipeline = 'same'; source_commit = 'same'; schema_sha256 = 'same' }
        not_claimed = @(
            'publication or release of this package'
            'source or schema metadata embedded in the package or its DLLs'
            'a build other than this recipe''s'
        )
    }
    if ($UnknownCi) {
        $record.build.ci = [ordered]@{ pipeline_id = $null; job_id = $null; commit_sha = $null }
    }
    if ($EditRecord) {
        $null = & $EditRecord -Record $record
    }
    $recordPath = Join-Path -Path $Path -ChildPath 'package-provenance.json'
    Write-TextFile -Path $recordPath -Text (ConvertTo-Json -InputObject $record -Depth 32)
    if ($NoCaptureRecord) {
        Remove-Item -LiteralPath $captureRecordPath
    }
    [ordered]@{
        SchemaPath = $schemaPath
        CaptureRecordPath = $captureRecordPath
        CaptureRecord = $captureRecord
        PackagePath = $packagePath
        Member = $members
        ProvenancePath = $recordPath
    }
}

# Runs Resolve-RunEngine.ps1 on a fixture and the committed lock, as SourceCommit and SourceTree, with PipelineId,
# CommitSha and -LocalPack only when given, and with PackagePath, when given, in place of the fixture's package.
# Returns its output, as an array, and the message of what it threw, if it threw.
function Invoke-Consumer {
    param(
        [System.Collections.IDictionary] $Fixture,
        [string] $SourceCommit = $commit,
        [string] $SourceTree = $tree,
        [string] $PipelineId,
        [string] $CommitSha,
        [string] $PackagePath,
        [switch] $LocalPack
    )
    $argument = @{
        SchemaPath = $Fixture.SchemaPath
        PackagePath = $Fixture.PackagePath
        ProvenancePath = $Fixture.ProvenancePath
        LockPath = $lockPath
        SourceCommit = $SourceCommit
        SourceTree = $SourceTree
    }
    if ($PackagePath) {
        $argument['PackagePath'] = $PackagePath
    }
    if ($PipelineId) {
        $argument['PipelineId'] = $PipelineId
    }
    if ($CommitSha) {
        $argument['CommitSha'] = $CommitSha
    }
    if ($LocalPack) {
        $argument['LocalPack'] = $true
    }
    try {
        [ordered]@{ output = @(& (Join-Path -Path $PSScriptRoot -ChildPath 'Resolve-RunEngine.ps1') @argument); thrown = $null }
    }
    catch {
        [ordered]@{ output = @(); thrown = $_.Exception.Message }
    }
}

# What a value is, for a check's details: null, or its type.
function Get-ValueKind {
    param($Value)
    if ($null -eq $Value) { 'null' } else { $Value.GetType().FullName }
}

# The value at Name, a path of keys, in nested dictionaries; $null where a key is missing.
function Get-Field {
    param($Object, [string[]] $Name)
    foreach ($key in $Name) {
        if ($Object -isnot [System.Collections.IDictionary] -or @($Object.Keys) -cnotcontains $key) {
            return $null
        }
        $Object = $Object[$key]
    }
    , $Object
}

# Why a run of Resolve-RunEngine.ps1 did not give one ordered dictionary with the keys mismatches, engine and lock, its
# mismatches an array of strings; nothing when it did.
function Get-ShapeProblem {
    param([System.Collections.IDictionary] $Run)
    if ($null -ne $Run.thrown) {
        return "it threw: $($Run.thrown)"
    }
    if ($Run.output.Count -ne 1 -or $Run.output[0] -isnot [System.Collections.Specialized.OrderedDictionary]) {
        return "it returned $($Run.output.Count) objects: $(@($Run.output | ForEach-Object { Get-ValueKind -Value $_ }) -join ', ')"
    }
    $keys = @($Run.output[0].Keys)
    $absent = @(@('mismatches', 'engine', 'lock') | Where-Object { $_ -cnotin $keys })
    if ($absent.Count) {
        return "it has no $($absent -join ', '); its keys are $($keys -join ', ')"
    }
    $mismatches = $Run.output[0]['mismatches']
    if ($mismatches -isnot [System.Array] -or @($mismatches | Where-Object { $_ -isnot [string] }).Count) {
        return "its mismatches are $(Get-ValueKind -Value $mismatches), not an array of strings"
    }
}

# The label of each mismatch: its text before the first ': ', or all of it when it has none.
function Get-Label {
    param([string[]] $Mismatch)
    foreach ($entry in $Mismatch) {
        $at = $entry.IndexOf(': ', [System.StringComparison]::Ordinal)
        if ($at -ge 0) { $entry.Substring(0, $at) } else { $entry }
    }
}

# Labels, once each, in ordinal order and joined with '; '; or 'none' when there are none.
function Join-Label {
    param([string[]] $Label)
    $sorted = [System.Collections.Generic.SortedSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($item in $Label) {
        $null = $sorted.Add($item)
    }
    if ($sorted.Count) { @($sorted) -join '; ' } else { 'none' }
}

# Checks a run of Resolve-RunEngine.ps1: that it gave its one result and left the committed lock as it was; that the
# labels of its mismatches are exactly Expected, or, with -Including, include Expected and none of Absent; and that it
# gave an engine and a lock when it reported no mismatch, and neither when it reported any.
function Write-RunCheck {
    param(
        [string] $Case,
        [System.Collections.IDictionary] $Run,
        [string[]] $Expected,
        [switch] $Including,
        [string[]] $Absent
    )
    $problem = Get-ShapeProblem -Run $Run
    Write-Check -Case $Case -What 'Resolve-RunEngine.ps1 returns one ordered dictionary of mismatches, engine and lock' -Passed (-not $problem) -Details $problem
    Write-Check -Case $Case -What 'the committed lock is as it was' -Passed ((Get-Sha256 -Path $lockPath) -ceq $lockSha256) -Details 'its SHA-256 changed'
    if ($problem) {
        return
    }
    $result = $Run.output[0]
    $labels = @(Get-Label -Mismatch $result['mismatches'])
    $reported = Join-Label -Label $labels
    if ($Including) {
        $missing = @($Expected | Where-Object { $_ -cnotin $labels })
        Write-Check -Case $Case -What "reports $(Join-Label -Label $Expected)" -Passed (-not $missing.Count) -Details "it reports $reported"
        $unwanted = @($Absent | Where-Object { $_ -cin $labels })
        Write-Check -Case $Case -What "reports none of $(Join-Label -Label $Absent)" -Passed (-not $unwanted.Count) -Details "it reports $reported"
    }
    else {
        $wanted = Join-Label -Label $Expected
        $what = 'reports no mismatch'
        if ($Expected) {
            $what = "reports exactly $wanted"
        }
        Write-Check -Case $Case -What $what -Passed ($reported -ceq $wanted) -Details "it reports $reported"
    }
    $given = "it gives engine $(Get-ValueKind -Value $result['engine']) and lock $(Get-ValueKind -Value $result['lock'])"
    if ($result['mismatches'].Count) {
        Write-Check -Case $Case -What 'gives neither engine nor lock, having reported a mismatch' -Passed ($null -eq $result['engine'] -and $null -eq $result['lock']) -Details $given
    }
    else {
        Write-Check -Case $Case -What 'gives an engine and a lock, having reported no mismatch' -Passed ($result['engine'] -is [System.Collections.Specialized.OrderedDictionary] -and $result['lock'] -is [string]) -Details $given
    }
}

# A dependencies map, id to version, as text: 'id version' pairs in ordinal order.
function ConvertTo-DependencyText {
    param([System.Collections.IDictionary] $Dependency)
    $pairs = [string[]] @(foreach ($entry in $Dependency.GetEnumerator()) { "$($entry.Key) $($entry.Value)" })
    [System.Array]::Sort($pairs, [System.StringComparer]::Ordinal)
    $pairs -join ', '
}

# The frameworks of a parsed lock that have a FilesCom entry, in the lock's order.
function Get-FilesComFramework {
    param($Lock)
    $frameworks = Get-Field -Object $Lock -Name 'dependencies'
    if ($frameworks -is [System.Collections.IDictionary]) {
        foreach ($framework in $frameworks.GetEnumerator()) {
            if ($framework.Value -is [System.Collections.IDictionary] -and @($framework.Value.Keys) -ccontains 'FilesCom') {
                $framework.Key
            }
        }
    }
}

# Checks the engine and lock of a run that matched: each of the engine's values as the fixture has it; and the lock as
# the committed one, but for the version and content hash of each FilesCom entry, which are the package's. -UnknownCi,
# for a fixture made with it, has the capture's and the build's pipeline, job and commit each null.
function Write-EngineCheck {
    param([string] $Case, [System.Collections.IDictionary] $Run, [System.Collections.IDictionary] $Fixture, [switch] $UnknownCi)
    if (Get-ShapeProblem -Run $Run) {
        return
    }
    $engine = $Run.output[0]['engine']
    $lockText = $Run.output[0]['lock']
    if ($engine -isnot [System.Collections.IDictionary] -or $lockText -isnot [string]) {
        return
    }
    $archive = [System.IO.File]::ReadAllBytes($Fixture.PackagePath)
    $capture = [System.IO.File]::ReadAllBytes($Fixture.SchemaPath)

    # Each value, by its path in the engine. A string must be a string; a number may be any number of that value; null
    # must be given, and null, not an empty string.
    $fields = [ordered]@{
        'package' = 'FilesCom'
        'version' = '2.0.1'
        'sha256' = Get-ByteHash -Byte $archive
        'sha512' = Get-ByteHash -Byte $archive -Sha512
        'bytes' = [long] $archive.Length
        'source' = "commit:$commit"
        'source_tree' = $tree
        'schema_sha256' = Get-ByteHash -Byte $capture
        'capture/sha256' = Get-ByteHash -Byte $capture
        'capture/bytes' = [long] $capture.Length
        'capture/source' = 's3://fixture-bucket/swagger_doc-production.json'
        'capture/captured_at' = '2026-10-06T16:00:00Z'
        'capture/pipeline_id' = '12345'
        'capture/job_id' = '101'
        'capture/commit_sha' = $commit
        'capture/record_sha256' = Get-Sha256 -Path $Fixture.CaptureRecordPath
        'build/pipeline_id' = '12345'
        'build/job_id' = '202'
        'build/commit_sha' = $commit
        'record_sha256' = Get-Sha256 -Path $Fixture.ProvenancePath
    }
    if ($UnknownCi) {
        foreach ($key in 'capture/pipeline_id', 'capture/job_id', 'capture/commit_sha', 'build/pipeline_id', 'build/job_id', 'build/commit_sha') {
            $fields[$key] = $null
        }
    }
    foreach ($field in $fields.GetEnumerator()) {
        $path = $field.Key.Split('/')
        $actual = Get-Field -Object $engine -Name $path
        if ($null -eq $field.Value) {
            $parent = Get-Field -Object $engine -Name $path[0]
            $same = $parent -is [System.Collections.IDictionary] -and @($parent.Keys) -ccontains $path[1] -and $null -eq $actual
        }
        else {
            $same = $null -ne $actual -and $actual -isnot [System.Array] -and ($actual -is [string]) -eq ($field.Value -is [string]) -and $actual -ceq $field.Value
        }
        Write-Check -Case $Case -What "engine $($field.Key) is $(if ($null -eq $field.Value) { 'null' } else { $field.Value })" -Passed $same -Details "it is $(ConvertTo-Json -InputObject $actual -Compress -Depth 8)"
    }
    $bytes = Get-Field -Object $engine -Name 'bytes'
    Write-Check -Case $Case -What 'engine bytes is a [long]' -Passed ($bytes -is [long]) -Details "it is $(Get-ValueKind -Value $bytes)"

    # The lock: the committed one, but for the version and content hash of each FilesCom entry, which are the package's.
    $returned = ConvertFrom-Json -InputObject $lockText -AsHashtable
    $frameworks = @(Get-FilesComFramework -Lock $committedLock)
    $given = @(Get-FilesComFramework -Lock $returned) -join ', '
    Write-Check -Case $Case -What "the lock has a FilesCom entry for $($frameworks -join ', ')" -Passed ($given -ceq ($frameworks -join ', ')) -Details "it has one for $given"
    $pinNames = @('requested', 'resolved', 'contentHash')
    $pinned = "[2.0.1, ) 2.0.1 $([System.Convert]::ToBase64String([System.Security.Cryptography.SHA512]::HashData($archive)))"
    foreach ($framework in $frameworks) {
        $entry = Get-Field -Object $returned -Name 'dependencies', $framework, 'FilesCom'
        $pin = @(foreach ($name in $pinNames) { [string] (Get-Field -Object $entry -Name $name) }) -join ' '
        Write-Check -Case $Case -What "the lock's $framework FilesCom entry requests and resolves 2.0.1, with the package's SHA-512" -Passed ($pin -ceq $pinned) -Details "it has $pin"
        if ($entry -is [System.Collections.IDictionary]) {
            foreach ($name in $pinNames) {
                $entry[$name] = $committedLock['dependencies'][$framework]['FilesCom'][$name]
            }
        }
    }
    $same = (ConvertTo-Json -InputObject $returned -Compress -Depth 32) -ceq (ConvertTo-Json -InputObject $committedLock -Compress -Depth 32)
    Write-Check -Case $Case -What 'the lock is otherwise the committed one' -Passed $same -Details 'it differs from it elsewhere'
}

# The cases, by name. Each makes its fixture in $caseDirectory, runs Resolve-RunEngine.ps1 on it and checks the result.
$cases = [ordered]@{
    'match' = {
        $fixture = Build-Fixture -Path $caseDirectory
        $run = Invoke-Consumer -Fixture $fixture -PipelineId '12345' -CommitSha $commit
        Write-RunCheck -Case $caseName -Run $run -Expected @()
        Write-EngineCheck -Case $caseName -Run $run -Fixture $fixture
    }
    'match-local' = {
        $fixture = Build-Fixture -Path $caseDirectory
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected @()
    }
    'local-unknown' = {
        $fixture = Build-Fixture -Path $caseDirectory -UnknownCi
        $run = Invoke-Consumer -Fixture $fixture -LocalPack
        Write-RunCheck -Case $caseName -Run $run -Expected @()
        Write-EngineCheck -Case $caseName -Run $run -Fixture $fixture -UnknownCi
    }
    'source-commit' = {
        $fixture = Build-Fixture -Path $caseDirectory
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture -SourceCommit $otherCommit) -Expected 'capture commit', 'producer commit', 'build commit'
    }
    'ci-commit' = {
        $fixture = Build-Fixture -Path $caseDirectory
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture -CommitSha $otherCommit) -Expected 'capture commit', 'producer commit', 'build commit'
    }
    'source-tree' = {
        $fixture = Build-Fixture -Path $caseDirectory
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture -SourceTree $otherTree) -Expected 'producer tree'
    }
    'pipeline' = {
        $fixture = Build-Fixture -Path $caseDirectory
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture -PipelineId '99999' -CommitSha $commit) -Expected 'capture pipeline', 'build pipeline'
    }
    'build-pipeline' = {
        $fixture = Build-Fixture -Path $caseDirectory -EditRecord { param($Record) $Record.build.ci.pipeline_id = '12346' }
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'build pipeline'
    }
    'schema-bytes' = {
        # One letter of the title changes, after the checksum and both records were made.
        $fixture = Build-Fixture -Path $caseDirectory
        $capture = [System.IO.File]::ReadAllBytes($fixture.SchemaPath)
        [System.IO.File]::WriteAllBytes($fixture.SchemaPath, (ConvertTo-ChangedByte -Byte $capture -Index $captureText.IndexOf('fixture', [System.StringComparison]::Ordinal)))
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'capture checksum', 'capture SHA-256', 'producer schema'
    }
    'flavor' = {
        $fixture = Build-Fixture -Path $caseDirectory -Flavor 'internal'
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'capture flavor'
    }
    'flavor-case' = {
        $fixture = Build-Fixture -Path $caseDirectory -Flavor 'PUBLIC'
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'capture flavor'
    }
    'capture-record' = {
        $fixture = Build-Fixture -Path $caseDirectory
        $fixture.CaptureRecord['captured_at'] = '2026-10-06T16:00:01Z'
        Write-TextFile -Path $fixture.CaptureRecordPath -Text (ConvertTo-Json -InputObject $fixture.CaptureRecord -Depth 32)
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'producer capture', 'producer capture SHA-256'
    }
    'package-bytes' = {
        # The archive is made again with one member changed, as long as it was, after the package record was made.
        $fixture = Build-Fixture -Path $caseDirectory
        $fixture.Member['lib/net8.0/FilesCom.dll'] = ConvertTo-ChangedByte -Byte $fixture.Member['lib/net8.0/FilesCom.dll'] -Index 0
        Write-Archive -Path $fixture.PackagePath -Member $fixture.Member
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'archive SHA-256', 'archive SHA-512', 'package members'
    }
    'member' = {
        $fixture = Build-Fixture -Path $caseDirectory -EditRecord { param($Record) $Record.package.lib_members['lib/net8.0/FilesCom.dll'] = 'ab' * 32 }
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'package members'
    }
    'nuspec' = {
        $fixture = Build-Fixture -Path $caseDirectory -NuspecVersion '2.0.2'
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'package nuspec'
    }
    'file-name' = {
        $fixture = Build-Fixture -Path $caseDirectory
        $renamed = Join-Path -Path $caseDirectory -ChildPath 'FilesCom.9.9.9.nupkg'
        [System.IO.File]::Copy($fixture.PackagePath, $renamed)
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture -PackagePath $renamed) -Expected 'package file'
    }
    'dependencies' = {
        $fixture = Build-Fixture -Path $caseDirectory -ExtraDependency
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'package dependencies'
    }
    'build-exit' = {
        $fixture = Build-Fixture -Path $caseDirectory -EditRecord {
            param($Record)
            foreach ($command in $Record.build.commands) {
                if ($command['name'] -ceq 'pack sdk/FilesCom/FilesCom.csproj') {
                    $command['exit'] = 1
                }
            }
        }
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'build commands'
    }
    'commands-shape' = {
        # The commands as an object of exit codes by command name, which is not how the record gives them.
        $fixture = Build-Fixture -Path $caseDirectory -EditRecord {
            param($Record)
            $Record.build.commands = [ordered]@{ restore = [ordered]@{ exit_code = 0 }; build = [ordered]@{ exit_code = 0 }; pack = [ordered]@{ exit_code = 0 } }
        }
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'build commands'
    }
    'manifest-shape' = {
        # The manifest's SHA-256 alone, as manifest_sha256, which is not how the record gives it.
        $fixture = Build-Fixture -Path $caseDirectory -EditRecord {
            param($Record)
            $Record.generated_input = [ordered]@{ manifest_sha256 = $Record.generated_input.manifest.sha256 }
        }
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'generated manifest'
    }
    'record-format' = {
        $fixture = Build-Fixture -Path $caseDirectory -EditRecord { param($Record) $Record.format = 'filescom-dotnet-package-provenance/0' }
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'record format'
    }
    'local-unknown-needs-switch' = {
        $fixture = Build-Fixture -Path $caseDirectory -UnknownCi
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'capture fields', 'capture commit', 'build commit', 'build job'
    }
    'local-known-mismatch' = {
        $fixture = Build-Fixture -Path $caseDirectory -UnknownCi -EditRecord { param($Record) $Record.build.ci.commit_sha = $otherCommit }
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture -LocalPack) -Expected 'build commit'
    }
    'local-with-pipeline' = {
        $fixture = Build-Fixture -Path $caseDirectory -UnknownCi
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture -LocalPack -PipelineId '12345') -Expected 'local pack'
    }
    'missing-record' = {
        # Without its record, nothing about the capture's record but its absence is reported.
        $fixture = Build-Fixture -Path $caseDirectory -NoCaptureRecord
        $recordLabels = @('capture fields', 'capture format', 'capture flavor', 'capture source', 'capture SHA-256', 'capture size', 'capture commit', 'capture pipeline')
        Write-RunCheck -Case $caseName -Run (Invoke-Consumer -Fixture $fixture) -Expected 'capture record' -Including -Absent $recordLabels
    }
}

# -Only: the named cases, each once.
$selected = @($cases.Keys)
if ($PSBoundParameters.ContainsKey('Only')) {
    $selected = @(foreach ($item in $Only) { ([string] $item).Split(',') })
    $repeated = @($selected | Group-Object -CaseSensitive | Where-Object { $_.Count -gt 1 })
    $unknown = @($selected | Where-Object { $_ -cnotin $cases.Keys })
    if (-not $selected.Count -or $repeated.Count -or $unknown.Count) {
        [System.Console]::Error.WriteLine("test-run-engine.ps1: -Only needs one or more of $($cases.Keys -join ', '), each once")
        exit 2
    }
}

# <dir>: a new directory, for this run alone.
$Work = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Work)
if ([System.IO.FileInfo]::new($Work).Exists -or [System.IO.Directory]::Exists($Work)) {
    [System.Console]::Error.WriteLine("test-run-engine.ps1: $Work already exists; give a path that does not")
    exit 2
}
$null = [System.IO.Directory]::CreateDirectory($Work)

# The committed engine lock, which Resolve-RunEngine.ps1 is given as it is and no case may change.
$lockPath = (Resolve-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath '../src/FilesCom.PowerShell.Engine/packages.lock.json')).Path
$lockSha256 = Get-Sha256 -Path $lockPath
$committedLock = Get-Content -LiteralPath $lockPath -Raw | ConvertFrom-Json -AsHashtable

# The fixture's nuspec has no net48 group, so Resolve-RunEngine.ps1 checks the lock's net48 FilesCom entry against its
# netstandard2.0 group, which matches only while the lock gives FilesCom the same dependencies for both.
$net48 = ConvertTo-DependencyText -Dependency $committedLock['dependencies']['.NETFramework,Version=v4.8']['FilesCom']['dependencies']
$netstandard = ConvertTo-DependencyText -Dependency $committedLock['dependencies']['.NETStandard,Version=v2.0']['FilesCom']['dependencies']
Write-Check -Case 'fixture' -What 'the committed lock gives FilesCom the same dependencies for net48 as for netstandard2.0' -Passed ($net48 -and $net48 -ceq $netstandard) -Details "net48 has $net48; netstandard2.0 has $netstandard"

foreach ($caseName in $selected) {
    $caseDirectory = Join-Path -Path $Work -ChildPath $caseName
    try {
        & $cases[$caseName]
    }
    catch {
        Write-Check -Case $caseName -What 'the case runs to its end' -Passed $false -Details $_.Exception.Message
    }
}

Write-Output -InputObject "passed $($checks.passed) of $($checks.passed + $checks.failed)"
if ($checks.failed) {
    exit 1
}
exit 0
