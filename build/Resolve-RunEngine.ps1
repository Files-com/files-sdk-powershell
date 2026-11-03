<#
.SYNOPSIS
Checks that a pipeline's schema capture and the FilesCom package built from it join, and gives the engine they make.

.DESCRIPTION
The ordinary CI job builds the module on the FilesCom .NET package its own pipeline built, from the production schema
the same pipeline captured. This takes the files of one such run:

- the capture, with "<capture>.sha256" and the capture's record "<capture>.provenance.json" beside it;
- the package, FilesCom.<version>.nupkg, and its record, filescom-dotnet-package-provenance/1;
- the engine project's packages.lock.json;
- the commit and tree of the source the module is built from, and, when the job has them, its pipeline and commit.

It requires the capture to be a public one whose record describes its exact bytes; the package's record to describe the
package's exact bytes, its lib members, its nuspec and the dependencies the lock pins for it, to embed the capture's
record, to list the recipe's commands, each exiting 0, and to give its generated input manifest's SHA-256; and both to
come from this source's commit and one pipeline, the job's when it has one. The capture and the package come from
different jobs of that pipeline, so their job IDs are not compared. The records are read as capture-schema.sh and
targets/dotnet/package-provenance.rb write them.

With -LocalPack the package was packed outside CI, as package-provenance.rb retain-release records a pack with no CI
identity, and there is no job: -PipelineId and -CommitSha are refused. The capture's and the build's pipeline, job and
commit may then be unknown (null), and stay unknown in the engine; each one the records know must still join as above.
Every other check is the same.

It returns an ordered dictionary: mismatches, each 'what: actual, expected'; and, only when there are none, the engine
the run makes, and the lock with the package's version and content hash in place of the committed ones and nothing else
changed. A missing or malformed input is a mismatch, not an error.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SchemaPath,

    [Parameter(Mandatory)]
    [string] $PackagePath,

    [Parameter(Mandatory)]
    [string] $ProvenancePath,

    [Parameter(Mandatory)]
    [string] $LockPath,

    [Parameter(Mandatory)]
    [string] $SourceCommit,

    [Parameter(Mandatory)]
    [string] $SourceTree,

    [string] $PipelineId,

    [string] $CommitSha,

    [switch] $LocalPack
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.Text.Json

$captureFormat = 'gollum-schema-capture/1'
$captureFields = @('format', 'flavor', 'source', 'sha256', 'bytes', 'captured_at', 'pipeline_id', 'job_id', 'commit_sha')
$recordFormat = 'filescom-dotnet-package-provenance/1'
$packageId = 'FilesCom'
# The CI identities a local pack's records may leave unknown.
$ciFields = @('pipeline_id', 'job_id', 'commit_sha')

$mismatches = [System.Collections.Generic.List[string]]::new()
function Add-Mismatch {
    param([string] $What, [string] $Actual, [string] $Expected)
    $mismatches.Add("$What`: $Actual, expected $Expected")
}

# The value at Names in Object, or $null when any is missing.
function Get-Field {
    param($Object, [string[]] $Names)
    foreach ($name in $Names) {
        if ($null -eq $Object -or $Object -isnot [System.Management.Automation.PSCustomObject] -or -not $Object.PSObject.Properties[$name]) {
            return $null
        }
        $Object = $Object.$name
    }
    $Object
}

function Get-Hex {
    param([byte[]] $Hash)
    [System.Convert]::ToHexString($Hash).ToLowerInvariant()
}

# The JSON object in the file at Path, or $null when it is missing or holds anything else.
function Get-JsonObject {
    param([string] $Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    try {
        $parsed = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    }
    catch {
        return $null
    }
    if ($parsed -is [System.Management.Automation.PSCustomObject]) { $parsed } else { $null }
}

function ConvertTo-Canonical {
    param($Value)
    ConvertTo-Json -InputObject $Value -Compress -Depth 32
}

# A local pack has no job: a pipeline or commit given for one is refused, and not used.
if ($LocalPack -and ($PipelineId -or $CommitSha)) {
    Add-Mismatch -What 'local pack' -Actual "pipeline '$PipelineId', commit '$CommitSha'" -Expected 'none: a local pack has no CI job'
    $PipelineId = $null
    $CommitSha = $null
}

# Each commit the job must come from: the source's, and the pipeline's when the job has one.
$commits = @($SourceCommit) + @(if ($CommitSha) { $CommitSha })
function Test-Commit {
    param([string] $What, $Actual, [switch] $CiIdentity)
    # A local pack may leave a CI identity unknown; the producer's own source commit is never unknown.
    if ($CiIdentity -and $LocalPack -and [string]::IsNullOrEmpty([string] $Actual)) {
        return
    }
    foreach ($commit in $commits) {
        if ([string] $Actual -cne $commit) {
            Add-Mismatch -What $What -Actual $Actual -Expected $commit
            return
        }
    }
}

# A CI identity as the records give it, or $null when they leave it unknown.
function Get-Known {
    param($Value)
    if ([string]::IsNullOrEmpty([string] $Value)) { $null } else { [string] $Value }
}

# The capture: its bytes, its checksum file and its record.
$capture = $null
$captureSha256 = $null
$captureRecord = $null
$captureRecordPath = "$SchemaPath.provenance.json"
if (Test-Path -LiteralPath $SchemaPath -PathType Leaf) {
    $bytes = [System.IO.File]::ReadAllBytes($SchemaPath)
    $captureSha256 = Get-Hex -Hash ([System.Security.Cryptography.SHA256]::HashData($bytes))
    $capture = [ordered]@{ sha256 = $captureSha256; bytes = [long] $bytes.Length }
}
else {
    Add-Mismatch -What 'capture file' -Actual '(missing)' -Expected "a file at $SchemaPath"
}
$checksumPath = "$SchemaPath.sha256"
$checksum = if (Test-Path -LiteralPath $checksumPath -PathType Leaf) { (Get-Content -LiteralPath $checksumPath -Raw) -split '\s+' | Where-Object { $_ } | Select-Object -First 1 }
if (-not $checksum -or [string] $checksum -cne $captureSha256) {
    Add-Mismatch -What 'capture checksum' -Actual $(if ($checksum) { $checksum } else { '(missing)' }) -Expected $captureSha256
}
$captureRecord = Get-JsonObject -Path $captureRecordPath
if (-not $captureRecord) {
    Add-Mismatch -What 'capture record' -Actual '(missing or not a JSON object)' -Expected "a $captureFormat record at $captureRecordPath"
}
else {
    $absent = @($captureFields | Where-Object { [string]::IsNullOrEmpty([string] (Get-Field -Object $captureRecord -Names $_)) -and -not ($LocalPack -and $_ -in $ciFields) })
    if ($absent.Count) {
        Add-Mismatch -What 'capture fields' -Actual "$($absent -join ', ') absent" -Expected ($captureFields -join ', ')
    }
    $value = { param([string] $Name) [string] (Get-Field -Object $captureRecord -Names $Name) }
    if ((& $value 'format') -cne $captureFormat) {
        Add-Mismatch -What 'capture format' -Actual (& $value 'format') -Expected $captureFormat
    }
    # The capture's flavor, as capture-schema.sh names it: public, never internal.
    if ((& $value 'flavor') -cne 'public') {
        Add-Mismatch -What 'capture flavor' -Actual (& $value 'flavor') -Expected 'public'
    }
    $sourceUri = $null
    if (-not [uri]::TryCreate((& $value 'source'), [System.UriKind]::Absolute, [ref] $sourceUri) -or $sourceUri.Scheme -notin 's3', 'https') {
        Add-Mismatch -What 'capture source' -Actual (& $value 'source') -Expected 'an s3:// or https:// URI'
    }
    if ($capture) {
        if ((& $value 'sha256') -cne $capture.sha256) {
            Add-Mismatch -What 'capture SHA-256' -Actual (& $value 'sha256') -Expected $capture.sha256
        }
        if ((& $value 'bytes') -cne [string] $capture.bytes) {
            Add-Mismatch -What 'capture size' -Actual (& $value 'bytes') -Expected $capture.bytes
        }
    }
    Test-Commit -What 'capture commit' -Actual (& $value 'commit_sha') -CiIdentity
    if ($PipelineId -and (& $value 'pipeline_id') -cne $PipelineId) {
        Add-Mismatch -What 'capture pipeline' -Actual (& $value 'pipeline_id') -Expected $PipelineId
    }
}

# The package: its bytes, its nuspec and lib members.
$archive = $null
$nuspec = $null
$members = [ordered]@{}
if (Test-Path -LiteralPath $PackagePath -PathType Leaf) {
    $bytes = [System.IO.File]::ReadAllBytes($PackagePath)
    $sha512 = [System.Security.Cryptography.SHA512]::HashData($bytes)
    $archive = [ordered]@{
        sha256 = Get-Hex -Hash ([System.Security.Cryptography.SHA256]::HashData($bytes))
        sha512 = Get-Hex -Hash $sha512
        sha512Base64 = [System.Convert]::ToBase64String($sha512)
        bytes = [long] $bytes.Length
    }
    try {
        $zip = [System.IO.Compression.ZipArchive]::new([System.IO.MemoryStream]::new($bytes, $false), [System.IO.Compression.ZipArchiveMode]::Read)
        try {
            $nuspecs = @($zip.Entries | Where-Object { $_.FullName -cmatch '^[^/]+\.nuspec$' })
            if ($nuspecs.Count -eq 1) {
                $settings = [System.Xml.XmlReaderSettings]::new()
                $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
                $settings.XmlResolver = $null
                $reader = [System.Xml.XmlReader]::Create($nuspecs[0].Open(), $settings)
                try {
                    $nuspec = [System.Xml.XmlDocument]::new()
                    $nuspec.Load($reader)
                }
                finally {
                    $reader.Dispose()
                }
            }
            foreach ($entry in $zip.Entries | Where-Object { $_.FullName.StartsWith('lib/', [System.StringComparison]::Ordinal) -and -not $_.FullName.EndsWith('/') }) {
                $stream = $entry.Open()
                try {
                    $members[$entry.FullName] = Get-Hex -Hash ([System.Security.Cryptography.SHA256]::HashData($stream))
                }
                finally {
                    $stream.Dispose()
                }
            }
        }
        finally {
            $zip.Dispose()
        }
    }
    catch {
        $archive.unreadable = $_.Exception.Message
    }
}
if (-not $archive -or $archive.Contains('unreadable')) {
    Add-Mismatch -What 'package archive' -Actual $(if ($archive) { $archive.unreadable } else { '(missing)' }) -Expected "a NuGet package at $PackagePath"
}
# The text of the nuspec's metadata element Name, or $null.
$metadata = {
    param([string] $Name)
    $node = if ($nuspec) { $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='$Name']") }
    if ($node) { $node.InnerText }
}

# The engine project's lock: the frameworks that pin the package, and each one's dependencies.
$lock = $null
try {
    $lock = Get-Content -LiteralPath $LockPath -Raw | ConvertFrom-Json -AsHashtable
}
catch {
    $lock = $null
}
$pinned = [ordered]@{}
if ($lock -is [System.Collections.IDictionary] -and $lock['dependencies'] -is [System.Collections.IDictionary]) {
    foreach ($framework in $lock['dependencies'].Keys) {
        $packages = $lock['dependencies'][$framework]
        $entry = if ($packages -is [System.Collections.IDictionary]) { $packages[$packageId] }
        if ($entry -is [System.Collections.IDictionary]) {
            $pinned[$framework] = $entry
        }
    }
}
if (-not $pinned.Count) {
    Add-Mismatch -What 'lock' -Actual "no $packageId in $LockPath" -Expected "a packages.lock.json that pins $packageId"
}

# The package's record.
$record = Get-JsonObject -Path $ProvenancePath
if (-not $record) {
    Add-Mismatch -What 'record' -Actual '(missing or not a JSON object)' -Expected "a $recordFormat record at $ProvenancePath"
}
else {
    $value = { param([string[]] $Names) [string] (Get-Field -Object $record -Names $Names) }
    $version = & $value 'package', 'version'
    $fileName = "$packageId.$version.nupkg"
    if ((& $value 'format') -cne $recordFormat) {
        Add-Mismatch -What 'record format' -Actual (& $value 'format') -Expected $recordFormat
    }
    if ((& $value 'package', 'id') -cne $packageId) {
        Add-Mismatch -What 'package name' -Actual (& $value 'package', 'id') -Expected $packageId
    }
    if ($version -cnotmatch '^[0-9]+(\.[0-9]+){1,3}(-[0-9A-Za-z.-]+)?$') {
        Add-Mismatch -What 'package version' -Actual $version -Expected 'a version'
    }
    foreach ($name in [System.IO.Path]::GetFileName($PackagePath), [System.IO.Path]::GetFileName((& $value 'package', 'archive', 'path'))) {
        if ($name -cne $fileName) {
            Add-Mismatch -What 'package file' -Actual $name -Expected $fileName
        }
    }
    if ($archive) {
        if ((& $value 'package', 'archive', 'bytes') -cne [string] $archive.bytes) {
            Add-Mismatch -What 'archive size' -Actual $archive.bytes -Expected (& $value 'package', 'archive', 'bytes')
        }
        if ((& $value 'package', 'archive', 'sha256') -cne $archive.sha256) {
            Add-Mismatch -What 'archive SHA-256' -Actual $archive.sha256 -Expected (& $value 'package', 'archive', 'sha256')
        }
        if ((& $value 'package', 'archive', 'sha512_hex') -cne $archive.sha512 -or (& $value 'package', 'archive', 'sha512_base64') -cne $archive.sha512Base64) {
            Add-Mismatch -What 'archive SHA-512' -Actual $archive.sha512 -Expected (& $value 'package', 'archive', 'sha512_hex')
        }
    }
    if ($archive -and -not $archive.Contains('unreadable')) {
        $nuspecId = [string] (& $metadata 'id')
        $nuspecVersion = [string] (& $metadata 'version')
        if (-not $nuspec -or $nuspecId -cne $packageId -or $nuspecVersion -cne $version) {
            Add-Mismatch -What 'package nuspec' -Actual $(if ($nuspec) { "$nuspecId $nuspecVersion" } else { 'not exactly one nuspec' }) -Expected "$packageId $version"
        }
        $recorded = [ordered]@{}
        $libMembers = Get-Field -Object $record -Names 'package', 'lib_members'
        if ($libMembers -is [System.Management.Automation.PSCustomObject]) {
            foreach ($property in $libMembers.PSObject.Properties) {
                $recorded[$property.Name] = [string] $property.Value
            }
        }
        $paths = @(@($recorded.Keys) + @($members.Keys) | Sort-Object -Unique -CaseSensitive)
        $differing = @($paths | Where-Object { -not $recorded.Contains($_) -or -not $members.Contains($_) -or $recorded[$_] -cne $members[$_] })
        if (-not $recorded.Count -or $differing.Count) {
            Add-Mismatch -What 'package members' -Actual "$(@($members.Keys).Count) lib members, $($differing.Count) not as recorded ($($differing -join ', '))" -Expected "the $($recorded.Count) recorded"
        }
        # Each pinning framework's dependencies, against the nuspec group NuGet takes them from.
        if ($nuspec -and $pinned.Count) {
            $groups = @{}
            foreach ($group in $nuspec.SelectNodes("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='dependencies']/*[local-name()='group']")) {
                $lines = [string[]] @($group.SelectNodes("*[local-name()='dependency']") | ForEach-Object { "$($_.GetAttribute('id')) $($_.GetAttribute('version'))" })
                [System.Array]::Sort($lines, [System.StringComparer]::Ordinal)
                $groups[$group.GetAttribute('targetFramework')] = $lines -join '; '
            }
            foreach ($framework in $pinned.Keys) {
                $groupName = switch -Regex ($framework) {
                    '^\.NETStandard,Version=v(.+)$' { ".NETStandard$($Matches[1])" }
                    '^\.NETFramework,Version=v(.+)$' { if ($groups.ContainsKey(".NETFramework$($Matches[1])")) { ".NETFramework$($Matches[1])" } else { '.NETStandard2.0' } }
                    default { $framework }
                }
                $dependencies = $pinned[$framework]['dependencies']
                $lines = [string[]] @(if ($dependencies -is [System.Collections.IDictionary]) { $dependencies.GetEnumerator() | ForEach-Object { "$($_.Key) $($_.Value)" } })
                [System.Array]::Sort($lines, [System.StringComparer]::Ordinal)
                $expected = $lines -join '; '
                if (-not $groups.ContainsKey($groupName) -or $groups[$groupName] -cne $expected) {
                    Add-Mismatch -What 'package dependencies' -Actual "$framework from group $groupName`: $(if ($groups.ContainsKey($groupName)) { $groups[$groupName] } else { '(absent)' })" -Expected $expected
                }
            }
        }
    }
    Test-Commit -What 'producer commit' -Actual (& $value 'source', 'commit')
    if ((& $value 'source', 'tree') -cne $SourceTree) {
        Add-Mismatch -What 'producer tree' -Actual (& $value 'source', 'tree') -Expected $SourceTree
    }
    if ((& $value 'schema', 'sha256') -cne $captureSha256) {
        Add-Mismatch -What 'producer schema' -Actual (& $value 'schema', 'sha256') -Expected $captureSha256
    }
    if ($captureRecord) {
        $embedded = Get-Field -Object $record -Names 'schema', 'capture_provenance'
        if ($null -eq $embedded -or (ConvertTo-Canonical -Value $embedded) -cne (ConvertTo-Canonical -Value $captureRecord)) {
            Add-Mismatch -What 'producer capture' -Actual 'the embedded capture record' -Expected "the record at $captureRecordPath"
        }
        $captureRecordSha256 = (Get-FileHash -LiteralPath $captureRecordPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ((& $value 'schema', 'capture_provenance_sha256') -cne $captureRecordSha256) {
            Add-Mismatch -What 'producer capture SHA-256' -Actual (& $value 'schema', 'capture_provenance_sha256') -Expected $captureRecordSha256
        }
        $capturePipeline = [string] (Get-Field -Object $captureRecord -Names 'pipeline_id')
        # A local pack's pipeline joins the capture's only when both are known.
        if (-not $LocalPack -or ($capturePipeline -and (& $value 'build', 'ci', 'pipeline_id'))) {
            foreach ($pipeline in @($capturePipeline) + @(if ($PipelineId) { $PipelineId })) {
                if ((& $value 'build', 'ci', 'pipeline_id') -cne $pipeline) {
                    Add-Mismatch -What 'build pipeline' -Actual (& $value 'build', 'ci', 'pipeline_id') -Expected $pipeline
                    break
                }
            }
        }
    }
    Test-Commit -What 'build commit' -Actual (& $value 'build', 'ci', 'commit_sha') -CiIdentity
    if (-not $LocalPack -and -not (& $value 'build', 'ci', 'job_id')) {
        Add-Mismatch -What 'build job' -Actual '(missing)' -Expected 'the build job''s ID'
    }
    # The recipe's commands, in the order they ran: each a name and its exit, which must be 0. Taken from the property
    # itself, so that an array of one stays an array.
    $commands = $null
    $build = Get-Field -Object $record -Names 'build'
    if ($build -is [System.Management.Automation.PSCustomObject] -and $build.PSObject.Properties['commands']) {
        $commands = $build.commands
    }
    $failed = @(if ($commands -is [System.Array]) {
            foreach ($command in $commands) {
                $name = [string] (Get-Field -Object $command -Names 'name')
                $exit = Get-Field -Object $command -Names 'exit'
                if (-not $name -or ($exit -isnot [long] -and $exit -isnot [int]) -or $exit -ne 0) {
                    if ($name) { $name } else { '(unnamed)' }
                }
            }
        })
    if ($commands -isnot [System.Array] -or -not $commands.Count -or $failed.Count) {
        Add-Mismatch -What 'build commands' -Actual $(if ($failed.Count) { "$($failed -join ', ') did not exit 0" } elseif ($commands -is [System.Array]) { '(none recorded)' } else { 'not a list of commands' }) -Expected 'the recipe''s commands, each exiting 0'
    }
    if ((& $value 'generated_input', 'manifest', 'sha256') -cnotmatch '^[0-9a-f]{64}$') {
        Add-Mismatch -What 'generated manifest' -Actual (& $value 'generated_input', 'manifest', 'sha256') -Expected 'the SHA-256 of the generated input manifest'
    }
}

$engine = $null
$lockText = $null
if (-not $mismatches.Count) {
    $ci = { param([string] $Name) Get-Known (Get-Field -Object $record -Names 'build', 'ci', $Name) }
    $engine = [ordered]@{
        package = $packageId
        version = $version
        sha256 = $archive.sha256
        sha512 = $archive.sha512
        bytes = $archive.bytes
        source = "commit:$SourceCommit"
        source_tree = $SourceTree
        schema_sha256 = $captureSha256
        capture = [ordered]@{
            sha256 = $captureSha256
            bytes = $capture.bytes
            source = [string] $captureRecord.source
            # As the record writes it: ConvertFrom-Json would make it a date.
            captured_at = [System.Text.Json.JsonDocument]::Parse((Get-Content -LiteralPath $captureRecordPath -Raw)).RootElement.GetProperty('captured_at').ToString()
            pipeline_id = Get-Known $captureRecord.pipeline_id
            job_id = Get-Known $captureRecord.job_id
            commit_sha = Get-Known $captureRecord.commit_sha
            record_sha256 = $captureRecordSha256
        }
        build = [ordered]@{ pipeline_id = & $ci 'pipeline_id'; job_id = & $ci 'job_id'; commit_sha = & $ci 'commit_sha' }
        record_sha256 = (Get-FileHash -LiteralPath $ProvenancePath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    foreach ($entry in $pinned.Values) {
        $entry['requested'] = "[$version, )"
        $entry['resolved'] = $version
        $entry['contentHash'] = $archive.sha512Base64
    }
    $lockText = ConvertTo-Json -InputObject $lock -Depth 32
}

[ordered]@{ mismatches = [string[]] $mismatches.ToArray(); engine = $engine; lock = $lockText }
