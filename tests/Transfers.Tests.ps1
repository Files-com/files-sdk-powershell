BeforeDiscovery {
    # Group tests need a second group the user can give a file, or the superuser; the refusal test needs a group the
    # user is not in, which on macOS a file created under /private/tmp gets from that folder.
    $script:UnixGroups = if ($IsWindows) { @() } else { @((& id -G) -split ' ' | Where-Object { $_ }) }
    $script:CanChangeGroup = -not $IsWindows -and ((& id -u) -eq '0' -or $script:UnixGroups.Count -ge 2)
    $script:ForeignGroupFixture = $IsMacOS -and (Test-Path -LiteralPath '/private/tmp') -and ($script:UnixGroups -notcontains ((& /usr/bin/stat -f '%g' /private/tmp)))
}

BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')

    function Get-FileGroup {
        param([string] $Path)
        if ($IsMacOS) { & /usr/bin/stat -f '%g' $Path } else { & stat -c '%g' $Path }
    }

    # A group the user may give a file, other than the one given.
    function Get-OtherGroup {
        param([string] $Except)
        if ((& id -u) -eq '0') {
            if ($Except -eq '1') { '2' } else { '1' }
        }
        else {
            @((& id -G) -split ' ' | Where-Object { $_ -and $_ -ne $Except })[0]
        }
    }
}

Describe 'Send-FilesFile' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service -Options @{ MaxNetworkRetries = 1 }
        $content = New-RandomContent 10000
        $local = Join-Path $TestDrive 'report data.bin'
        [System.IO.File]::WriteAllBytes($local, $content)
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'uploads a file''s exact bytes in parallel parts and writes the result with -PassThru' {
        $service.ServeUpload('reports/report data.bin', 4096, $true)

        $result = Send-FilesFile -Path $local -RemotePath 'reports/report data.bin' -MkdirParents -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $content -Actual ($service.Uploaded('reports/report data.bin')) | Should -BeTrue
        $result.Size | Should -Be 10000
        $result.Parts.Count | Should -Be 3
        $result.ParallelParts | Should -BeTrue
        $result.File.Path | Should -Be 'reports/report data.bin'
        (Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/reports/report data.bin')[0].Body | Should -Match '"mkdir_parents":true'
    }

    It 'sends mkdir_parents only when -MkdirParents is given, including an explicit $false' {
        $service.ServeUpload('a.bin', 100000)
        Send-FilesFile -Path $local -RemotePath 'a.bin' -Connection $connection -Confirm:$false
        Send-FilesFile -Path $local -RemotePath 'a.bin' -MkdirParents:$false -Connection $connection -Confirm:$false

        $bodies = @(Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/a.bin').Body
        $bodies[0] | Should -Not -Match 'mkdir_parents'
        $bodies[1] | Should -Match '"mkdir_parents":false'
    }

    It 'retries a failed part and completes the upload' {
        $service.ServeUpload('retry.bin', 4096)
        $service.FailPart('retry.bin', 2, 503)

        $result = Send-FilesFile -Path $local -RemotePath 'retry.bin' -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $content -Actual ($service.Uploaded('retry.bin')) | Should -BeTrue
        ($result.Parts | Where-Object Number -EQ 2).Attempts | Should -Be 2
    }

    It 'gets a new URL for a part whose storage refuses it as expired and sends it the same bytes, but stops at an ordinary 403' {
        $service.ServeUpload('renewed.bin', 4096, $false)
        $service.ExpirePartGrant('renewed.bin', 2)
        $service.ServeUpload('denied.bin', 4096, $false)
        $service.FailPart('denied.bin', 2, 403)

        $result = Send-FilesFile -Path $local -RemotePath 'renewed.bin' -Connection $connection -PassThru -Confirm:$false
        $denied = try { Send-FilesFile -Path $local -RemotePath 'denied.bin' -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }

        Test-SameContent -Expected $content -Actual ($service.Uploaded('renewed.bin')) | Should -BeTrue
        $result.Size | Should -Be 10000
        # The expired URL was answered once; Files.com issued the part again, and the same bytes went to the new URL.
        $service.GrantsIssued('renewed.bin', 2) | Should -Be 2
        $sent = @($service.Requests | Where-Object { $_.Method -eq 'PUT' -and $_.Path -eq '/upload/renewed.bin/2' })
        @($sent | ForEach-Object { $_.Query['grant'] }) | Should -Be @('1', '2')
        Test-SameContent -Expected $sent[0].BodyBytes -Actual $sent[1].BodyBytes | Should -BeTrue
        # A 403 that does not say the URL expired is a refusal: no new URL, no second request, and the upload fails.
        $denied | Should -Not -BeNullOrEmpty
        @(Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/denied.bin' | Where-Object Body -Match '"part":2').Count | Should -Be 1
        @($service.Requests | Where-Object { $_.Method -eq 'PUT' -and $_.Path -eq '/upload/denied.bin/2' }).Count | Should -Be 1
        @(Get-ApiRequest $service -Method POST -Path '/files/denied.bin').Count | Should -Be 0
    }

    It 'sends a static upload''s parts -MaxConcurrency at a time and reports how the upload chose' {
        $service.ServeUpload('static.bin', 1000, $true)
        $service.DelayParts('static.bin', 150)

        $result = Send-FilesFile -Path $local -RemotePath 'static.bin' -ConcurrencyMode Static -MaxConcurrency 3 -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $content -Actual ($service.Uploaded('static.bin')) | Should -BeTrue
        $service.PeakParts('static.bin') | Should -Be 3
        $result.ConcurrencyMode | Should -Be 'Static'
        $result.PeakConcurrency | Should -Be 3
        $result.SerialReason | Should -Be 'None'
        $result.PartRetries | Should -Be 0
    }

    It 'adapts an upload''s concurrency by default, stays within -MaxConcurrency, and reports its profile' {
        $service.ServeUpload('automatic.bin', 1000, $true)
        $service.ServeUpload('capped.bin', 1000, $true)
        $service.DelayParts('automatic.bin', 50)
        $service.DelayParts('capped.bin', 50)

        $automatic = Send-FilesFile -Path $local -RemotePath 'automatic.bin' -Connection $connection -PassThru -Confirm:$false
        $capped = Send-FilesFile -Path $local -RemotePath 'capped.bin' -MaxConcurrency 1 -PerformanceProfile S3 -Connection $connection -PassThru -Confirm:$false

        $automatic.ConcurrencyMode | Should -Be 'Automatic'
        $automatic.PerformanceProfile | Should -Be 'Generic'
        $automatic.PeakConcurrency | Should -Be $service.PeakParts('automatic.bin')
        $capped.ConcurrencyMode | Should -Be 'Automatic'
        $capped.PerformanceProfile | Should -Be 'S3'
        $service.PeakParts('capped.bin') | Should -Be 1
        $capped.PeakConcurrency | Should -Be 1
        Test-SameContent -Expected $content -Actual ($service.Uploaded('capped.bin')) | Should -BeTrue
    }

    It 'sends parts one at a time when Files.com requires it, whatever -MaxConcurrency says, and reports why' {
        $service.ServeUpload('serial.bin', 1000, $false)
        $service.DelayParts('serial.bin', 50)

        $result = Send-FilesFile -Path $local -RemotePath 'serial.bin' -ConcurrencyMode Static -MaxConcurrency 4 -Connection $connection -PassThru -Confirm:$false

        $service.PeakParts('serial.bin') | Should -Be 1
        $result.PeakConcurrency | Should -Be 1
        $result.ParallelParts | Should -BeFalse
        $result.SerialReason | Should -Be 'ServerRequiresSerialParts'
        Test-SameContent -Expected $content -Actual ($service.Uploaded('serial.bin')) | Should -BeTrue
    }

    It 'refuses a profile for a static upload and more than 1024 parts at once, before sending anything' {
        { Send-FilesFile -Path $local -RemotePath 'x.bin' -ConcurrencyMode Static -PerformanceProfile S3 -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { Send-FilesFile -Path $local -RemotePath 'x.bin' -MaxConcurrency 1025 -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'ParameterArgumentValidationError*'
        $service.Requests.Count | Should -Be 0
    }

    It 'uploads a borrowed stream and leaves it open' {
        $service.ServeUpload('stream.bin', 4096)
        $stream = [System.IO.MemoryStream]::new($content)

        Send-FilesFile -InputStream $stream -Length $content.Length -RemotePath 'stream.bin' -Connection $connection -Confirm:$false

        Test-SameContent -Expected $content -Actual ($service.Uploaded('stream.bin')) | Should -BeTrue
        $stream.CanRead | Should -BeTrue
        $stream.Position | Should -Be $content.Length
        $stream.Dispose()
    }

    It 'gives each piped file its own remote path' {
        $second = Join-Path $TestDrive 'second.bin'
        [System.IO.File]::WriteAllBytes($second, (New-RandomContent 10))
        $service.ServeUpload('in/report data.bin', 100000)
        $service.ServeUpload('in/second.bin', 100000)

        Get-Item $local, $second | Send-FilesFile -RemotePath { "in/$($_.Name)" } -Connection $connection -Confirm:$false

        $service.Uploaded('in/second.bin').Length | Should -Be 10
        $service.Uploaded('in/report data.bin').Length | Should -Be 10000
    }

    It 'uploads nothing when the files given would go to one remote path, and names them' {
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'batch')
        $a = Join-Path $folder.FullName 'a.txt'
        $b = Join-Path $folder.FullName 'b.txt'
        [System.IO.File]::WriteAllText($a, 'AAA')
        [System.IO.File]::WriteAllText($b, 'BBB')
        $otherFolder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'other')
        $sameName = Join-Path $otherFolder.FullName 'A.TXT'
        [System.IO.File]::WriteAllText($sameName, 'CCC')
        $service.ServeUpload('same.txt', 4096)

        { Send-FilesFile -LiteralPath $a, $b -RemotePath 'same.txt' -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { Send-FilesFile -Path $a, (Join-Path $folder.FullName '*.txt') -RemotePath 'same.txt' -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        # Without -RemotePath each file keeps its name, and Files.com treats a.txt and A.TXT as the same path.
        { Send-FilesFile -LiteralPath $a, $sameName -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw '*would all upload to*'

        $service.Requests.Count | Should -Be 0
    }

    It 'sends nothing under -WhatIf' {
        Send-FilesFile -Path $local -Connection $connection -WhatIf
        $service.Requests.Count | Should -Be 0
    }

    It 'reports an upload whose completion is unknown, with what Files.com accepted, and sends the final request once' {
        $service.ServeUpload('lost.bin', 4096, $true, 0)

        { Send-FilesFile -Path $local -RemotePath 'lost.bin' -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.TransferCompletionUnknown*'

        $failure = $Error[0]
        $failure.Exception.UncertainUpload.Path | Should -Be 'lost.bin'
        $failure.Exception.UncertainUpload.Parts.Count | Should -Be 3
        $failure.ErrorDetails.RecommendedAction | Should -Match 'Check the remote file'
        @(Get-ApiRequest $service -Method POST -Path '/files/lost.bin').Count | Should -Be 1
    }

    It 'reports progress from the pipeline thread and completes it' {
        $service.ServeUpload('progress.bin', 4096)
        $shell = [powershell]::Create()
        try {
            $shell.AddScript({
                    param($Manifest, $Local, $Connection)
                    Import-Module -Name $Manifest
                    Send-FilesFile -Path $Local -RemotePath 'progress.bin' -Connection $Connection -Confirm:$false
                }).AddArgument($script:ModuleManifest).AddArgument($local).AddArgument($connection) | Out-Null
            $shell.Invoke() | Out-Null
            $records = @($shell.Streams.Progress)
        }
        finally {
            $shell.Dispose()
        }

        $records.Count | Should -BeGreaterThan 0
        $records[-1].RecordType | Should -Be 'Completed'
    }

    It 'continues an interrupted upload from its -CheckpointPath without sending accepted parts again, and removes the checkpoint once complete' {
        $service.ServeUpload('resume.bin', 4096, $false)
        $service.FailPart('resume.bin', 3, 400)
        $checkpoint = Join-Path $TestDrive 'resume.checkpoint'

        { Send-FilesFile -Path $local -RemotePath 'resume.bin' -CheckpointPath $checkpoint -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw
        Test-Path -LiteralPath $checkpoint | Should -BeTrue
        $service.ClearRequests()

        $result = Send-FilesFile -Path $local -RemotePath 'resume.bin' -CheckpointPath $checkpoint -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $content -Actual ($service.Uploaded('resume.bin')) | Should -BeTrue
        $result.ResumedParts | Should -Be 2
        $result.ResumedBytes | Should -Be 8192
        @($service.Requests | Where-Object Method -EQ 'PUT').Path | Should -Be @('/upload/resume.bin/3')
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
    }

    It 'refuses a checkpoint whose file changed, or that is not a checkpoint, before sending anything, and starts over with -CheckpointRestart StartOver' {
        $service.ServeUpload('changed.bin', 4096, $false)
        $service.FailPart('changed.bin', 3, 400)
        $checkpoint = Join-Path $TestDrive 'changed.checkpoint'
        { Send-FilesFile -Path $local -RemotePath 'changed.bin' -CheckpointPath $checkpoint -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw
        # The same size, so only the bytes of the parts Files.com accepted show the change.
        $changed = New-RandomContent 10000
        [array]::Reverse($changed)
        [System.IO.File]::WriteAllBytes($local, $changed)
        $service.ClearRequests()

        $failure = try { Send-FilesFile -Path $local -RemotePath 'changed.bin' -CheckpointPath $checkpoint -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.CheckpointRefused*'
        $failure.CategoryInfo.Category | Should -Be 'InvalidData'
        $failure.Exception.CheckpointProblem | Should -Be 'SourceChanged'
        $failure.Exception.CheckpointPath | Should -Be $checkpoint
        $failure.Exception.Message | Should -Not -Match 'UploadOptions|CheckpointRestartPolicy'
        $failure.ErrorDetails.RecommendedAction | Should -Match '-CheckpointRestart StartOver'
        $service.Requests.Count | Should -Be 0

        $result = Send-FilesFile -Path $local -RemotePath 'changed.bin' -CheckpointPath $checkpoint -CheckpointRestart StartOver -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $changed -Actual ($service.Uploaded('changed.bin')) | Should -BeTrue
        $result.ResumedParts | Should -Be 0

        Set-Content -LiteralPath $checkpoint -Value 'not a checkpoint'
        $service.ClearRequests()
        $failure = try { Send-FilesFile -Path $local -RemotePath 'changed.bin' -CheckpointPath $checkpoint -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
        $failure.Exception.CheckpointProblem | Should -Be 'Unreadable'
        $service.Requests.Count | Should -Be 0
    }

    It 'uploads the first piped file with -CheckpointPath and refuses a later one, saying the first may have completed' {
        $second = Join-Path $TestDrive 'second.bin'
        [System.IO.File]::WriteAllBytes($second, (New-RandomContent 10))
        $checkpoint = Join-Path $TestDrive 'shared.checkpoint'
        $service.ServeUpload('in/report data.bin', 100000)
        $service.ServeUpload('in/second.bin', 100000)

        $failure = try { Get-Item $local, $second | Send-FilesFile -RemotePath { "in/$($_.Name)" } -CheckpointPath $checkpoint -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }

        # The first file is uploaded before the second arrives; the second is refused rather than reusing the checkpoint.
        Test-SameContent -Expected $content -Actual ($service.Uploaded('in/report data.bin')) | Should -BeTrue
        @(Get-ApiRequest $service -Method POST -Path '/files/in/report data.bin').Count | Should -Be 1
        @(Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/in/second.bin').Count | Should -Be 0
        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        $failure.Exception.Message | Should -Not -Match 'Nothing was uploaded'
        $failure.Exception.Message | Should -Match 'may have completed'
        $failure.Exception.Message | Should -Match ([regex]::Escape($second))
    }

    It 'refuses a -CheckpointPath for several files of one input or that is a folder, and -CheckpointRestart without it, before sending anything' {
        $second = Join-Path $TestDrive 'second.bin'
        [System.IO.File]::WriteAllBytes($second, (New-RandomContent 10))
        $checkpoint = Join-Path $TestDrive 'several.checkpoint'

        $failure = try { Send-FilesFile -LiteralPath $local, $second -CheckpointPath $checkpoint -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
        { Send-FilesFile -Path $local -CheckpointPath $TestDrive -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { Send-FilesFile -Path $local -CheckpointPath (Join-Path $TestDrive 'missing/a.checkpoint') -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { Send-FilesFile -Path $local -CheckpointRestart StartOver -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        $failure.Exception.Message | Should -Match 'Nothing was uploaded'
        $service.Requests.Count | Should -Be 0
        Test-Path -LiteralPath (Join-Path $TestDrive 'missing') | Should -BeFalse
    }

    It 'refuses a local folder and a missing local file as per-file errors' {
        Send-FilesFile -LiteralPath (Join-Path $TestDrive 'missing.bin') -Connection $connection -Confirm:$false -ErrorVariable failures -ErrorAction SilentlyContinue
        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
        $service.Requests.Count | Should -Be 0
    }
}

Describe 'Send-FilesFile to a remote server, snapshot or child site' {
    BeforeAll {
        # The part requests of an upload to RemotePath: the test service's part URLs hold the remote path as one segment.
        function Get-PartRequest {
            param([string] $RemotePath)
            @($service.Requests | Where-Object { $_.Method -eq 'PUT' -and [uri]::UnescapeDataString($_.Path) -like "/upload/$RemotePath/*" })
        }
    }
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service -Options @{ MaxNetworkRetries = 1 }
        $content = New-RandomContent 10000
        $local = Join-Path $TestDrive 'report data.bin'
        [System.IO.File]::WriteAllBytes($local, $content)
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'uploads a local file and a borrowed stream to a <Label> as the same parallel upload, in its folder, by the file''s name unless given a path, which cannot leave it' -TestCases @(
        @{ Label = 'remote server'; Parameter = 'RemoteServerId'; Root = '_/RemoteServers/5' }
        @{ Label = 'snapshot'; Parameter = 'SnapshotId'; Root = '_/Snapshots/5' }
        @{ Label = 'child site'; Parameter = 'ChildSiteId'; Root = '_/Sites/5' }
    ) {
        $destination = @{ $Parameter = 5; Connection = $connection; Confirm = $false }
        $service.ServeUpload("$Root/report data.bin", 4096, $true)
        $service.ServeUpload("$Root/in/nested/dir/x.bin", 4096)
        $service.FailPart("$Root/in/nested/dir/x.bin", 2, 503)
        $service.ServeUpload("$Root/moved/s.bin", 4096)
        $stream = [System.IO.MemoryStream]::new($content)

        $named = Send-FilesFile -Path $local -MkdirParents -PassThru @destination
        $messy = Send-FilesFile -LiteralPath $local -RemotePath 'in\..\nested//dir/./x.bin' -PassThru @destination
        $streamed = Send-FilesFile -InputStream $stream -Length $content.Length -RemotePath '../../moved/s.bin' -PassThru @destination

        # The accepted .NET SDK's own external-destination helpers address the same paths (ExternalDestinationTest).
        $named.File.Path | Should -Be "$Root/report data.bin"
        $messy.File.Path | Should -Be "$Root/in/nested/dir/x.bin"
        $streamed.File.Path | Should -Be "$Root/moved/s.bin"
        foreach ($path in "$Root/report data.bin", "$Root/in/nested/dir/x.bin", "$Root/moved/s.bin") {
            Test-SameContent -Expected $content -Actual ($service.Uploaded($path)) | Should -BeTrue
            @(Get-ApiRequest $service -Method POST -Path "/files/$path").Count | Should -Be 1
        }
        # The same upload as to a folder of the site: parallel parts, a retried part, mkdir_parents, a borrowed stream.
        $named.ParallelParts | Should -BeTrue
        (Get-PartRequest "$Root/report data.bin").Count | Should -Be 3
        ($messy.Parts | Where-Object Number -EQ 2).Attempts | Should -Be 2
        (Get-ApiRequest $service -Method POST -Path "/file_actions/begin_upload/$Root/report data.bin")[0].Body | Should -Match '"mkdir_parents":true'
        $stream.CanRead | Should -BeTrue
        # No other endpoint: only the upload's own requests.
        @($service.Requests | Where-Object Method -NE 'PUT' | ForEach-Object { ([uri]::UnescapeDataString($_.Path) -split '/_/')[0] } | Sort-Object -Unique) | Should -Be @('/api/rest/v1/file_actions/begin_upload', '/api/rest/v1/files')
    }

    It 'previews an upload to a remote server with its whole path and sends nothing, and refuses a path that names nothing within it, or two files that would go to one path, before sending anything' {
        $other = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'other')
        $sameName = Join-Path $other.FullName 'REPORT DATA.BIN'
        [System.IO.File]::WriteAllBytes($sameName, $content)
        $transcript = Join-Path $TestDrive 'external-what-if.txt'
        Start-Transcript -Path $transcript | Out-Null
        try {
            Send-FilesFile -Path $local -RemoteServerId 5 -RemotePath 'reports/../q3.bin' -Connection $connection -WhatIf
        }
        finally {
            Stop-Transcript | Out-Null
        }

        $refusals = foreach ($within in '..', './/.\') {
            try { Send-FilesFile -Path $local -RemoteServerId 5 -RemotePath $within -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
        }
        $collision = try { Send-FilesFile -LiteralPath $local, $sameName -SnapshotId 3 -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
        $twoDestinations = try { Send-FilesFile -Path $local -RemoteServerId 5 -ChildSiteId 7 -Connection $connection -WhatIf } catch { $_ }

        Get-Content -LiteralPath $transcript -Raw | Should -Match ([regex]::Escape("to remote server 5, as file '_/RemoteServers/5/reports/q3.bin'"))
        foreach ($refusal in $refusals) {
            $refusal.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
            $refusal.Exception.Message | Should -Match 'names no file within remote server 5'
        }
        $collision.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        $collision.Exception.Message | Should -Match ([regex]::Escape("would all upload to '_/Snapshots/3/report data.bin'"))
        $twoDestinations.FullyQualifiedErrorId | Should -BeLike 'AmbiguousParameterSet*'
        $service.Requests.Count | Should -Be 0
    }

    It 'reports an uncertain completion with the whole remote path and sends the final request once, and continues an interrupted upload from its checkpoint' {
        $service.ServeUpload('_/Sites/7/lost.bin', 4096, $true, 0)
        $service.ServeUpload('_/Snapshots/3/resume.bin', 4096, $false)
        $service.FailPart('_/Snapshots/3/resume.bin', 3, 400)
        $checkpoint = Join-Path $TestDrive 'external.checkpoint'

        $uncertain = try { Send-FilesFile -Path $local -ChildSiteId 7 -RemotePath 'lost.bin' -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
        $finalRequests = @(Get-ApiRequest $service -Method POST -Path '/files/_/Sites/7/lost.bin').Count
        $interrupted = try { Send-FilesFile -Path $local -SnapshotId 3 -RemotePath 'resume.bin' -CheckpointPath $checkpoint -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
        $service.ClearRequests()
        $resumed = Send-FilesFile -Path $local -SnapshotId 3 -RemotePath 'resume.bin' -CheckpointPath $checkpoint -Connection $connection -PassThru -Confirm:$false

        $uncertain.FullyQualifiedErrorId | Should -BeLike 'Files.TransferCompletionUnknown*'
        $uncertain.TargetObject | Should -Be '_/Sites/7/lost.bin'
        $uncertain.Exception.UncertainUpload.Path | Should -Be '_/Sites/7/lost.bin'
        $finalRequests | Should -Be 1
        $interrupted | Should -Not -BeNullOrEmpty
        $resumed.ResumedParts | Should -Be 2
        (Get-PartRequest '_/Snapshots/3/resume.bin').Count | Should -Be 1
        Test-SameContent -Expected $content -Actual ($service.Uploaded('_/Snapshots/3/resume.bin')) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
    }

    It 'stops an upload to a child site when the pipeline stops, and does not complete it' {
        $service.ServeUpload('_/Sites/7/stopped.bin', 4096)
        $service.StallPart('_/Sites/7/stopped.bin', 2)
        $running = Start-Command -Connection $connection -Arguments $local -Script {
            param($Connection, $Local)
            Send-FilesFile -Path $Local -ChildSiteId 7 -RemotePath 'stopped.bin' -Connection $Connection -Confirm:$false
        }
        Wait-Until { (Get-PartRequest '_/Sites/7/stopped.bin' | Where-Object { [uri]::UnescapeDataString($_.Path) -like '*/2' }).Count -eq 1 -and $service.ActiveRequests -ge 1 }

        $elapsed = Measure-Command { $running.Shell.Stop() }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        $running.Shell.Dispose()
        @(Get-ApiRequest $service -Method POST -Path '/files/_/Sites/7/stopped.bin').Count | Should -Be 0
    }
}

Describe 'Send-FilesFile part sizes and streams of unknown length' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service -Options @{ MaxNetworkRetries = 1 }
        # The same site in memory, where the parts of uploads given SendPartsToS3 go straight to Amazon S3.
        $inMemory = Connect-TestService -Service $service -InMemory -Options @{ MaxNetworkRetries = 1 }
        $content = New-RandomContent 10000
        $local = Join-Path $TestDrive 'data.bin'
        [System.IO.File]::WriteAllBytes($local, $content)
        $temporary = (New-Item -ItemType Directory -Path (Join-Path $TestDrive "temporary-$([guid]::NewGuid().ToString('N'))")).FullName
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $inMemory -Confirm:$false
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'chooses part sizes within the engine''s limits for an ordinary multipart upload to Amazon S3, sends each part as exactly its bytes, and reports how it sized them' {
        $large = New-RandomContent 12MB
        [System.IO.File]::WriteAllBytes($local, $large)
        $service.ServeUpload('chosen.bin', 4096)
        $service.SendPartsToS3('chosen.bin')

        $result = Send-FilesFile -Path $local -RemotePath 'chosen.bin' -Connection $inMemory -PassThru -Confirm:$false

        Test-SameContent -Expected $large -Actual ($service.Uploaded('chosen.bin')) | Should -BeTrue
        $result.PartSizingMode | Should -Be 'Automatic'
        $result.PartSizeSource | Should -Be 'Automatic'
        $result.ServerPlannedReason | Should -Be 'None'
        # A 12 MiB file would prefer parts smaller than the 5 MiB S3 requires of every part but the last.
        $result.PartSizeConstraint | Should -Be 'ServerMinimum'
        $result.PartSizeLimits.MinNonfinalBytes | Should -Be 5MB
        $result.PartSizeLimits.MaxPartBytes | Should -Be 1GB
        $result.PartSizeLimits.MaxParts | Should -Be 10000
        $result.PartSizeLimits.MaxFileBytes | Should -Be 5TB
        $result.LargestPartSize | Should -Be 5MB
        # Each part is the file's bytes from its offset, sent to S3 as its part number without an offset, and the upload
        # completes with every part's ETag, in order, as storage answered them.
        @($result.Parts.ForEach('Offset')) | Should -Be @(0, 5MB, 10MB)
        @($result.Parts.ForEach('Length')) | Should -Be @(5MB, 5MB, 2MB)
        $service.PartLengths('chosen.bin') | Should -Be @(5MB, 5MB, 2MB)
        $parts = @($service.Requests | Where-Object Path -EQ '/upload/chosen.bin')
        @($parts | ForEach-Object { $_.Header('Host') } | Sort-Object -Unique) | Should -Be @([FilesCom.PowerShell.Tests.FilesTestService]::S3Host)
        @($parts | ForEach-Object { $_.Query['partNumber'] } | Sort-Object) | Should -Be @('1', '2', '3')
        @($parts | Where-Object { $_.Query.ContainsKey('part_offset') }).Count | Should -Be 0
        $result.Metrics.SendsPartOffsets | Should -BeFalse
        $finish = (Get-ApiRequest $service -Method POST -Path '/files/chosen.bin').Body | ConvertFrom-Json
        $finish.size | Should -Be 12MB
        @($finish.etags | ForEach-Object { "$($_.part)=$($_.etag)" }) | Should -Be @('1="etag-1"', '2="etag-2"', '3="etag-3"')
    }

    It 'sends every part but the last with -PartSize bytes' {
        $large = New-RandomContent 15MB
        [System.IO.File]::WriteAllBytes($local, $large)
        $service.ServeUpload('fixed.bin', 4096)
        $service.SendPartsToS3('fixed.bin')

        $result = Send-FilesFile -Path $local -RemotePath 'fixed.bin' -PartSize 6MB -Connection $inMemory -PassThru -Confirm:$false

        Test-SameContent -Expected $large -Actual ($service.Uploaded('fixed.bin')) | Should -BeTrue
        @($result.Parts.ForEach('Length')) | Should -Be @(6MB, 6MB, 3MB)
        $service.PartLengths('fixed.bin') | Should -Be @(6MB, 6MB, 3MB)
        $result.PartSizingMode | Should -Be 'Fixed'
        $result.PartSizeSource | Should -Be 'Fixed'
        $result.ServerPlannedReason | Should -Be 'None'
    }

    It 'refuses a -PartSize outside the part sizes the engine plans for the destination before sending any part, as a limit error in the module''s terms' {
        $large = New-RandomContent 12MB
        [System.IO.File]::WriteAllBytes($local, $large)
        $service.ServeUpload('too-small.bin', 4096)
        $service.SendPartsToS3('too-small.bin')

        { Send-FilesFile -Path $local -RemotePath 'too-small.bin' -PartSize 4MB -Connection $inMemory -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.LimitExceeded*'

        $failure = $Error[0]
        $failure.CategoryInfo.Category | Should -Be 'LimitsExceeded'
        $failure.Exception.Message | Should -Match 'fixed part size of 4194304 bytes is outside .*from 5242880 to 1073741824 bytes'
        $failure.Exception.UploadLimit | Should -Be 'FixedPartSize'
        $failure.ErrorDetails.RecommendedAction | Should -Match '-PartSize'
        @($service.Requests | Where-Object Path -Like '/upload/*').Count | Should -Be 0
        @(Get-ApiRequest $service -Method POST -Path '/files/too-small.bin').Count | Should -Be 0
    }

    It 'uses the sizes Files.com plans when asked, or when the parts do not go straight to Amazon S3, and says why' {
        # Files.com plans 6 MiB parts for a 12 MiB file whose parts go straight to Amazon S3.
        $large = New-RandomContent 12MB
        $largeFile = Join-Path $TestDrive 'requested.bin'
        [System.IO.File]::WriteAllBytes($largeFile, $large)
        $service.ServeUpload('requested.bin', 6MB)
        $service.SendPartsToS3('requested.bin')
        $service.ServeUpload('planned.bin', 4096)
        $service.ServeUpload('planned-fixed.bin', 4096)

        $requested = Send-FilesFile -Path $largeFile -RemotePath 'requested.bin' -PartSizingMode ServerPlanned -Connection $inMemory -PassThru -Confirm:$false
        $planned = Send-FilesFile -Path $local -RemotePath 'planned.bin' -Connection $connection -PassThru -Confirm:$false
        $plannedFixed = Send-FilesFile -Path $local -RemotePath 'planned-fixed.bin' -PartSize 3000 -Connection $connection -PassThru -Confirm:$false

        @($requested.Parts.ForEach('Length')) | Should -Be @(6MB, 6MB)
        $service.PartLengths('requested.bin') | Should -Be @(6MB, 6MB)
        $requested.PartSizingMode | Should -Be 'ServerPlanned'
        $requested.PartSizeSource | Should -Be 'ServerPlanned'
        $requested.ServerPlannedReason | Should -Be 'Requested'
        $requested.PartSizeLimits.MaxPartBytes | Should -Be 1GB
        Test-SameContent -Expected $large -Actual ($service.Uploaded('requested.bin')) | Should -BeTrue
        $planned.PartSizeSource | Should -Be 'ServerPlanned'
        $planned.ServerPlannedReason | Should -Be 'NotAllowed'
        $planned.PartSizeLimits | Should -BeNullOrEmpty
        $plannedFixed.PartSizingMode | Should -Be 'Fixed'
        $plannedFixed.PartSizeSource | Should -Be 'ServerPlanned'
        $plannedFixed.ServerPlannedReason | Should -Be 'NotAllowed'
        @($plannedFixed.Parts.ForEach('Length')) | Should -Be @(4096, 4096, 1808)
        # No part is sent with its offset: the destination places parts by their numbers.
        @($service.Requests | Where-Object { $_.Path -like '/upload/planned.bin/*' }).Count | Should -Be 3
        @($service.Requests | Where-Object { $_.Path -like '/upload/*' -and $_.Query.ContainsKey('part_offset') }).Count | Should -Be 0
    }

    It 'tunes an automatic upload for an ordinary multipart upload to Amazon S3, and generically for any other destination, unless -PerformanceProfile is given' {
        foreach ($remote in 's3.bin', 'agent.bin') {
            $service.ServeUpload($remote, 4096)
            $service.SendPartsToS3($remote)
        }
        $service.ServeUpload('generic.bin', 4096)
        $service.ServeUpload('preferred.bin', 4096)

        $s3 = Send-FilesFile -Path $local -RemotePath 's3.bin' -Connection $inMemory -PassThru -Confirm:$false
        $agent = Send-FilesFile -Path $local -RemotePath 'agent.bin' -PerformanceProfile Agent -Connection $inMemory -PassThru -Confirm:$false
        $generic = Send-FilesFile -Path $local -RemotePath 'generic.bin' -Connection $connection -PassThru -Confirm:$false
        $preferred = Send-FilesFile -Path $local -RemotePath 'preferred.bin' -PerformanceProfile S3 -Connection $connection -PassThru -Confirm:$false

        "$($s3.PerformanceProfile) $($s3.PerformanceProfileSource)" | Should -Be 'S3 Server'
        "$($agent.PerformanceProfile) $($agent.PerformanceProfileSource)" | Should -Be 'Agent Options'
        "$($generic.PerformanceProfile) $($generic.PerformanceProfileSource)" | Should -Be 'Generic Server'
        "$($preferred.PerformanceProfile) $($preferred.PerformanceProfileSource)" | Should -Be 'S3 Options'
        # A profile tunes only how many parts are sent at once: the other destination's parts are still Files.com's.
        $preferred.PartSizeSource | Should -Be 'ServerPlanned'
        $preferred.ServerPlannedReason | Should -Be 'NotAllowed'
    }

    It 'uploads a forward-only stream without -Length one part at a time, leaves it open, and reports why' {
        $service.ServeUpload('live.bin', 4096)
        $service.DelayParts('live.bin', 50)
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)

        $result = Send-FilesFile -InputStream $stream -RemotePath 'live.bin' -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $content -Actual ($service.Uploaded('live.bin')) | Should -BeTrue
        $result.Size | Should -Be 10000
        $result.SerialReason | Should -Be 'LengthUnknown'
        $result.PartSizeSource | Should -Be 'ServerPlanned'
        $result.ServerPlannedReason | Should -Be 'LengthUnknown'
        $service.PeakParts('live.bin') | Should -Be 1
        (Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/live.bin')[0].Body | Should -Not -Match '"size"'
        (Get-ApiRequest $service -Method POST -Path '/files/live.bin')[0].Body | Should -Match '"size":10000'
        $stream.IsDisposed | Should -BeFalse
        $stream.BytesRead | Should -Be 10000
    }

    It 'sizes the parts of a stream without -Length so that -MaxLength fits in the parts available, where its parts go straight to Amazon S3' {
        # Files.com makes two parts available, and the limiter holds parts of at most 8 MiB: for 16 MiB to fit, the first
        # part must hold 8 MiB, not the 5 MiB the tuning starts with.
        $service.ServeUpload('bounded-live.bin', 4096)
        $service.SendPartsToS3('bounded-live.bin', 2)
        $large = New-RandomContent 10MB
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($large, 65536)
        $limiter = New-FilesTransferLimiter -MaxBufferedBytes 8MB
        $sizing = New-FilesAutomaticPartSizing -UnknownLengthStartSize 5MB

        $result = Send-FilesFile -InputStream $stream -RemotePath 'bounded-live.bin' -MaxLength 16MB -Limiter $limiter -AutomaticPartSizing $sizing -Connection $inMemory -PassThru -Confirm:$false

        Test-SameContent -Expected $large -Actual ($service.Uploaded('bounded-live.bin')) | Should -BeTrue
        $result.PartSizeSource | Should -Be 'Automatic'
        $result.ServerPlannedReason | Should -Be 'None'
        # The size it started with was within the limits; the bound raised the first part.
        $result.PartSizeConstraint | Should -Be 'None'
        $result.SerialReason | Should -Be 'None'
        @($result.Parts.ForEach('Length')) | Should -Be @(8MB, 2MB)
        $service.PartLengths('bounded-live.bin') | Should -Be @(8MB, 2MB)
        (Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/bounded-live.bin')[0].Body | Should -Not -Match '"size"'
        (Get-ApiRequest $service -Method POST -Path '/files/bounded-live.bin')[0].Body | Should -Match '"size":10485760'
        $stream.IsDisposed | Should -BeFalse
    }

    It 'stops a stream that gives more than -MaxLength without completing the upload' {
        $service.ServeUpload('overflow.bin', 4096)
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)

        { Send-FilesFile -InputStream $stream -RemotePath 'overflow.bin' -MaxLength 6000 -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.LimitExceeded*'

        $Error[0].Exception.Message | Should -Match 'longer than the 6000 bytes the upload allows it \(-MaxLength'
        $Error[0].Exception.Message | Should -Not -Match 'UploadOptions'
        $Error[0].Exception.UploadLimit | Should -Be 'StreamLength'
        $Error[0].ErrorDetails.RecommendedAction | Should -Match '-MaxLength'
        $stream.BytesRead | Should -BeLessOrEqual 6001
        @(Get-ApiRequest $service -Method POST -Path '/files/overflow.bin').Count | Should -Be 0
        $stream.IsDisposed | Should -BeFalse
    }

    It 'falls back to the sizes Files.com plans when parts in memory cannot hold -MaxLength in the parts available, and chooses them with -PartStorage TemporaryFiles' {
        # Two parts of at most the limiter's 8 MiB in memory cannot hold 32 MiB; on disk, parts can be as large as the
        # engine's limits allow. A part on disk holds only the engine's 128 KiB copy buffer in memory. Files.com plans
        # 5 MiB parts, of which the 6 MiB stream fills one.
        $source = New-RandomContent 6MB
        foreach ($remote in 'memory.bin', 'files.bin') {
            $service.ServeUpload($remote, 5MB)
            $service.SendPartsToS3($remote, 2)
        }
        $limiter = New-FilesTransferLimiter -MaxBufferedBytes 8MB

        $memory = Send-FilesFile -InputStream ([FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($source, 65536)) -RemotePath 'memory.bin' -MaxLength 32MB -Limiter $limiter -Connection $inMemory -PassThru -Confirm:$false
        $files = Send-FilesFile -InputStream ([FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($source, 65536)) -RemotePath 'files.bin' -MaxLength 32MB -PartStorage TemporaryFiles -TemporaryDirectory $temporary -Limiter $limiter -Connection $inMemory -PassThru -Confirm:$false

        $memory.PartSizeSource | Should -Be 'ServerPlanned'
        $memory.ServerPlannedReason | Should -Be 'MemoryLimit'
        $memory.SerialReason | Should -Be 'LengthUnknown'
        @($memory.Parts.ForEach('Length')) | Should -Be @(5MB, 1MB)
        $service.PartLengths('memory.bin') | Should -Be @(5MB, 1MB)
        $files.PartSizeSource | Should -Be 'Automatic'
        $files.ServerPlannedReason | Should -Be 'None'
        # The engine's 8 MiB start for a stream is within its limits.
        $files.PartSizeConstraint | Should -Be 'None'
        @($files.Parts.ForEach('Length')) | Should -Be @(6MB)
        $service.PartLengths('files.bin') | Should -Be @(6MB)
        Test-SameContent -Expected $source -Actual ($service.Uploaded('memory.bin')) | Should -BeTrue
        Test-SameContent -Expected $source -Actual ($service.Uploaded('files.bin')) | Should -BeTrue
        @(Get-ChildItem -LiteralPath $temporary -Force).Count | Should -Be 0
    }

    It 'keeps each part of a stream in its own temporary file only until the part is accepted' {
        $service.ServeUpload('on-disk.bin', 4096)
        $service.DelayParts('on-disk.bin', 400)
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)
        $running = Start-Command -Connection $connection -Arguments $stream, $temporary -Script {
            param($Connection, $Stream, $Temporary)
            Send-FilesFile -InputStream $Stream -RemotePath 'on-disk.bin' -PartStorage TemporaryFiles -TemporaryDirectory $Temporary -Connection $Connection -Confirm:$false
        }

        Wait-Until { @(Get-ChildItem -LiteralPath $temporary -Force).Count -ge 1 }
        $running.Shell.EndInvoke($running.Handle) | Out-Null
        $running.Shell.Streams.Error.Count | Should -Be 0
        $running.Shell.Dispose()

        Test-SameContent -Expected $content -Actual ($service.Uploaded('on-disk.bin')) | Should -BeTrue
        @(Get-ChildItem -LiteralPath $temporary -Force).Count | Should -Be 0
    }

    It 'copies a stream with -Spool before sending anything, uploads it with its size known, and deletes the copy' {
        $service.ServeUpload('spooled.bin', 4096)
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)

        $result = Send-FilesFile -InputStream $stream -RemotePath 'spooled.bin' -Spool -MaxLength 20000 -TemporaryDirectory $temporary -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $content -Actual ($service.Uploaded('spooled.bin')) | Should -BeTrue
        (Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/spooled.bin')[0].Body | Should -Match '"size":10000'
        $result.SerialReason | Should -Be 'None'
        $result.ServerPlannedReason | Should -Be 'NotAllowed'
        @(Get-ChildItem -LiteralPath $temporary -Force).Count | Should -Be 0
        $stream.IsDisposed | Should -BeFalse
    }

    It 'sends nothing at all when a spooled stream is longer than -MaxLength or fails to read, and deletes the copy' {
        $service.ServeUpload('spool-overflow.bin', 4096)
        $service.ServeUpload('spool-broken.bin', 4096)

        { Send-FilesFile -InputStream ([FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)) -RemotePath 'spool-overflow.bin' -Spool -MaxLength 5000 -TemporaryDirectory $temporary -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.LimitExceeded*'
        $Error[0].Exception.Message | Should -Match 'nothing was uploaded'
        $Error[0].Exception.UploadLimit | Should -Be 'SpoolLength'
        Send-FilesFile -InputStream ([FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700, 3000)) -RemotePath 'spool-broken.bin' -Spool -MaxLength 20000 -TemporaryDirectory $temporary -Connection $connection -Confirm:$false -ErrorVariable broken -ErrorAction SilentlyContinue
        $broken[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'

        $service.Requests.Count | Should -Be 0
        @(Get-ChildItem -LiteralPath $temporary -Force).Count | Should -Be 0
    }

    It 'refuses a stream part larger than the shared limiter may buffer before reading it' {
        $service.ServeUpload('tight.bin', 8192)
        $limiter = New-FilesTransferLimiter -MaxBufferedBytes 4096
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)

        { Send-FilesFile -InputStream $stream -RemotePath 'tight.bin' -Limiter $limiter -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.LimitExceeded*'

        $Error[0].Exception.Message | Should -Match 'New-FilesTransferLimiter -MaxBufferedBytes'
        $Error[0].Exception.Message | Should -Not -Match 'TransferLimiter\.MaxBufferedBytes'
        $Error[0].Exception.UploadLimit | Should -Be 'BufferSpace'
        $Error[0].ErrorDetails.RecommendedAction | Should -Match '-MaxBufferedBytes'
        $stream.BytesRead | Should -Be 0
        @(Get-ApiRequest $service -Method POST -Path '/files/tight.bin').Count | Should -Be 0
    }

    It 'passes on the stream''s own failure unchanged, never as a limit, with <Label>' -TestCases @(
        @{ Label = '-Length'; Arguments = @{ Length = 10000 } }
        @{ Label = '-MaxLength'; Arguments = @{ MaxLength = 100000 } }
        @{ Label = '-Spool'; Arguments = @{ Spool = $true; MaxLength = 100000 } }
    ) {
        param($Label, $Arguments)
        # The same exception type the engine reports its limits with, raised by the caller's own stream mid-way.
        $service.ServeUpload('source-failure.bin', 4096)
        $producerFailure = [System.InvalidOperationException]::new('The producer lost its database connection.')
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700, 5000, 0, $producerFailure)
        $send = @{ InputStream = $stream; RemotePath = 'source-failure.bin'; Connection = $connection; Confirm = $false; ErrorAction = 'Stop' } + $Arguments

        { Send-FilesFile @send } | Should -Throw -ErrorId 'Files.Unexpected*' -Because $Label

        $failure = $Error[0]
        [object]::ReferenceEquals($failure.Exception, $producerFailure) | Should -BeTrue
        $failure.CategoryInfo.Category | Should -Be 'NotSpecified'
        $failure.ErrorDetails | Should -BeNullOrEmpty
        $stream.IsDisposed | Should -BeFalse
        @(Get-ApiRequest $service -Method POST -Path '/files/source-failure.bin').Count | Should -Be 0
    }

    It 'reports an upload that needs more parts than Files.com makes available as a limit, and a plan without part sizes as Files.com''s answer, before reading or sending anything' {
        # A 20 MiB stream whose parts are held whole within the limiter's 8 MiB needs three parts; Files.com makes two
        # available.
        $service.ServeUpload('many-parts.bin', 4096)
        $service.SendPartsToS3('many-parts.bin', 2)
        $service.ServeUpload('no-size.bin', 0)
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new((New-RandomContent 20MB), 65536)
        $limiter = New-FilesTransferLimiter -MaxBufferedBytes 8MB

        { Send-FilesFile -InputStream $stream -Length 20MB -RemotePath 'many-parts.bin' -Limiter $limiter -Connection $inMemory -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.LimitExceeded*'
        $Error[0].Exception.UploadLimit | Should -Be 'PartCount'
        $stream.BytesRead | Should -Be 0
        # Parts Files.com plans without a size are Files.com's mistake, not a limit of the upload.
        { Send-FilesFile -Path $local -RemotePath 'no-size.bin' -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidResponse*'
        $Error[0].Exception.UploadLimit | Should -BeNullOrEmpty

        @($service.Requests | Where-Object Path -Like '/upload/*').Count | Should -Be 0
        @(Get-ApiRequest $service -Method POST -Path '/files/many-parts.bin').Count + @(Get-ApiRequest $service -Method POST -Path '/files/no-size.bin').Count | Should -Be 0
    }

    It 'passes on the stream''s own CanRead failure unchanged on its <Call> call, with <Label>' -TestCases @(
        @{ Label = '-Length'; Call = 'second'; Before = 1; Arguments = @{ Length = 10000 } }
        @{ Label = '-MaxLength'; Call = 'second'; Before = 1; Arguments = @{ MaxLength = 100000 } }
        @{ Label = '-Spool'; Call = 'second'; Before = 1; Arguments = @{ Spool = $true; MaxLength = 100000 } }
        @{ Label = '-Length'; Call = 'third'; Before = 2; Arguments = @{ Length = 10000 } }
        @{ Label = '-MaxLength'; Call = 'third'; Before = 2; Arguments = @{ MaxLength = 100000 } }
        @{ Label = '-Spool'; Call = 'third'; Before = 2; Arguments = @{ Spool = $true; MaxLength = 100000 } }
    ) {
        param($Label, $Call, $Before, $Arguments)
        # The command asks CanRead itself before it confirms anything; the engine asks next, then Stream's own reads.
        $service.ServeUpload('capability.bin', 4096)
        $capabilityFailure = [System.InvalidOperationException]::new("The producer's pipe broke ($Call call).")
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)
        $stream.CanReadFailure = $capabilityFailure
        $stream.CanReadCallsBeforeFailure = $Before
        $send = @{ InputStream = $stream; RemotePath = 'capability.bin'; Connection = $connection; Confirm = $false; ErrorAction = 'Stop' } + $Arguments

        { Send-FilesFile @send } | Should -Throw -ErrorId 'Files.Unexpected*' -Because "$Label, $Call call"

        [object]::ReferenceEquals($Error[0].Exception, $capabilityFailure) | Should -BeTrue
        $Error[0].CategoryInfo.Category | Should -Be 'NotSpecified'
        @(Get-ApiRequest $service -Method POST -Path '/files/capability.bin').Count | Should -Be 0
    }

    It 'never asks a forward-only stream for its length or position, or to seek, with <Label>' -TestCases @(
        @{ Label = '-Length'; Arguments = @{ Length = 10000 } }
        @{ Label = '-MaxLength'; Arguments = @{ MaxLength = 100000 } }
        @{ Label = '-Spool'; Arguments = @{ Spool = $true; MaxLength = 100000 } }
    ) {
        param($Label, $Arguments)
        $service.ServeUpload('forward.bin', 4096)
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)
        $send = @{ InputStream = $stream; RemotePath = 'forward.bin'; Connection = $connection; Confirm = $false } + $Arguments

        Send-FilesFile @send

        $stream.SeekingCalls | Should -Be 0 -Because $Label
        Test-SameContent -Expected $content -Actual ($service.Uploaded('forward.bin')) | Should -BeTrue
        $stream.IsDisposed | Should -BeFalse
    }

    It 'keeps streams of unknown length that share a limiter within its request budget' {
        # Parts that go straight to Amazon S3 are sized by the engine, so a stream of unknown length sends them in
        # parallel: each 20 MiB stream has three.
        $large = New-RandomContent 20MB
        foreach ($remote in 'first.bin', 'second.bin') {
            $service.ServeUpload($remote, 4096)
            $service.SendPartsToS3($remote)
            $service.DelayParts($remote, 100)
        }
        $limiter = New-FilesTransferLimiter -MaxConcurrentRequests 2
        $script = {
            param($Connection, $Stream, $RemotePath, $Limiter)
            Send-FilesFile -InputStream $Stream -RemotePath $RemotePath -MaxLength 40MB -ConcurrencyMode Static -MaxConcurrency 4 -Limiter $Limiter -Connection $Connection -Confirm:$false
        }

        $first = Start-Command -Connection $inMemory -Script $script -Arguments ([FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($large, 65536)), 'first.bin', $limiter
        $second = Start-Command -Connection $inMemory -Script $script -Arguments ([FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($large, 65536)), 'second.bin', $limiter
        foreach ($running in $first, $second) {
            $running.Shell.EndInvoke($running.Handle) | Out-Null
            $running.Shell.Streams.Error.Count | Should -Be 0
            $running.Shell.Dispose()
        }

        # Each upload alone would send its 3 parts at once.
        $service.PeakPartsOfAllUploads | Should -Be 2
        $service.PartLengths('first.bin').Count | Should -Be 3
        Test-SameContent -Expected $large -Actual ($service.Uploaded('first.bin')) | Should -BeTrue
        Test-SameContent -Expected $large -Actual ($service.Uploaded('second.bin')) | Should -BeTrue
    }

    It 'shows a stream''s progress without a percentage until its length is known, and a spooled stream''s copy' {
        # A slow producer: each part is accepted while the next is still being read, so the total is not yet known.
        $service.ServeUpload('progress-live.bin', 4096)
        $service.ServeUpload('progress-spool.bin', 100000)
        $shell = [powershell]::Create()
        try {
            $shell.AddScript({
                    param($Manifest, $Connection, $Live, $Spooled)
                    Import-Module -Name $Manifest
                    Send-FilesFile -InputStream $Live -RemotePath 'progress-live.bin' -Connection $Connection -Confirm:$false
                    Send-FilesFile -InputStream $Spooled -RemotePath 'progress-spool.bin' -Spool -MaxLength 20000 -Connection $Connection -Confirm:$false
                }).AddArgument($script:ModuleManifest).AddArgument($connection).AddArgument([FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700, -1, 80)).AddArgument([FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 1000, -1, 60)) | Out-Null
            $shell.Invoke() | Out-Null
            $records = @($shell.Streams.Progress)
            $shell.Streams.Error.Count | Should -Be 0
        }
        finally {
            $shell.Dispose()
        }

        $unknown = @($records | Where-Object StatusDescription -Like 'progress-live.bin: * bytes read so far, total unknown*')
        $unknown.Count | Should -BeGreaterThan 0
        $unknown.PercentComplete | Sort-Object -Unique | Should -Be -1
        $unknown.SecondsRemaining | Sort-Object -Unique | Should -Be -1
        @($records | Where-Object StatusDescription -Like 'progress-spool.bin: copying the stream to a temporary file before sending it, * bytes so far').Count | Should -BeGreaterThan 0
        @($records | Where-Object RecordType -EQ 'Completed').Count | Should -Be 2
    }

    It 'refuses settings that would change nothing or contradict each other, before reading the stream or sending anything' -TestCases @(
        @{ Label = '-PartSize with another sizing mode'; Arguments = @{ PartSize = 3000; PartSizingMode = 'ServerPlanned' } }
        @{ Label = '-PartSizingMode Fixed without -PartSize'; Arguments = @{ PartSizingMode = 'Fixed' } }
        @{ Label = '-MaxLength with -Length'; Arguments = @{ Length = 10000; MaxLength = 20000 } }
        @{ Label = '-Spool with -Length'; Arguments = @{ Length = 10000; Spool = $true; MaxLength = 20000 } }
        @{ Label = '-Spool without -MaxLength'; Arguments = @{ Spool = $true } }
        @{ Label = '-Spool with -MaxLength 0'; Arguments = @{ Spool = $true; MaxLength = 0 } }
        @{ Label = '-Spool with -PartStorage'; Arguments = @{ Spool = $true; MaxLength = 20000; PartStorage = 'TemporaryFiles' } }
        @{ Label = '-TemporaryDirectory without temporary files'; Arguments = @{ TemporaryDirectory = '<temporary>' } }
        @{ Label = '-TemporaryDirectory that does not exist'; Arguments = @{ PartStorage = 'TemporaryFiles'; TemporaryDirectory = '<missing>' } }
        @{ Label = '-AutomaticConcurrency with -ConcurrencyMode Static'; Arguments = @{ AutomaticConcurrency = '<concurrency>'; ConcurrencyMode = 'Static' } }
        @{ Label = '-AutomaticConcurrency with -PerformanceProfile'; Arguments = @{ AutomaticConcurrency = '<concurrency>'; PerformanceProfile = 'S3' } }
        @{ Label = '-AggressiveStartup with -ConcurrencyMode Static'; Arguments = @{ AggressiveStartup = $true; ConcurrencyMode = 'Static' } }
        @{ Label = '-AggressiveStartup with -PerformanceProfile'; Arguments = @{ AggressiveStartup = $true; PerformanceProfile = 'S3' } }
        @{ Label = '-AggressiveStartup with -AutomaticConcurrency'; Arguments = @{ AggressiveStartup = $true; AutomaticConcurrency = '<concurrency>' } }
        @{ Label = '-AutomaticPartSizing with -PartSize'; Arguments = @{ AutomaticPartSizing = '<sizing>'; PartSize = 3000 } }
        @{ Label = '-AutomaticPartSizing with -PartSizingMode ServerPlanned'; Arguments = @{ AutomaticPartSizing = '<sizing>'; PartSizingMode = 'ServerPlanned' } }
        @{ Label = '-DiagnosticLimit without -Diagnostics'; Arguments = @{ DiagnosticLimit = 5 } }
    ) {
        param($Label, $Arguments)
        if ($Arguments.TemporaryDirectory -eq '<temporary>') { $Arguments.TemporaryDirectory = $temporary }
        if ($Arguments.TemporaryDirectory -eq '<missing>') { $Arguments.TemporaryDirectory = Join-Path $temporary 'missing' }
        if ($Arguments.AutomaticConcurrency -eq '<concurrency>') { $Arguments.AutomaticConcurrency = New-FilesAutomaticConcurrency -Profile Generic }
        if ($Arguments.AutomaticPartSizing -eq '<sizing>') { $Arguments.AutomaticPartSizing = New-FilesAutomaticPartSizing }
        $service.ServeUpload('refused.bin', 4096)
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)

        { Send-FilesFile -InputStream $stream -RemotePath 'refused.bin' @Arguments -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*' -Because $Label

        $stream.BytesRead | Should -Be 0
        $service.Requests.Count | Should -Be 0
    }

    It 'binds the stream settings only with -InputStream' {
        { Send-FilesFile -Path $local -RemotePath 'x.bin' -PartStorage TemporaryFiles -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'AmbiguousParameterSet*'
        { Send-FilesFile -Path $local -RemotePath 'x.bin' -MaxLength 10 -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'AmbiguousParameterSet*'
        $service.Requests.Count | Should -Be 0
    }

    It 'reads nothing and sends nothing under -WhatIf' {
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)

        Send-FilesFile -InputStream $stream -RemotePath 'preview.bin' -Spool -MaxLength 20000 -Connection $connection -WhatIf
        Send-FilesFile -InputStream $stream -RemotePath 'preview.bin' -MaxLength 20000 -PartStorage TemporaryFiles -Connection $connection -WhatIf
        $concurrency = New-FilesAutomaticConcurrency -Profile S3
        $sizing = New-FilesAutomaticPartSizing -MinPartSize 1024
        Send-FilesFile -InputStream $stream -RemotePath 'preview.bin' -AutomaticConcurrency $concurrency -AutomaticPartSizing $sizing -MaxReadyParts 4 -Diagnostics -Connection $connection -WhatIf -InformationVariable previewed

        @($previewed).Count | Should -Be 0
        $stream.BytesRead | Should -Be 0
        $stream.IsDisposed | Should -BeFalse
        $service.Requests.Count | Should -Be 0
    }

    It 'stops a stream upload when the pipeline stops, deletes its part files, and does not complete it' {
        $service.ServeUpload('stopped-live.bin', 4096)
        $service.StallPart('stopped-live.bin', 2)
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)
        $running = Start-Command -Connection $connection -Arguments $stream, $temporary -Script {
            param($Connection, $Stream, $Temporary)
            Send-FilesFile -InputStream $Stream -RemotePath 'stopped-live.bin' -PartStorage TemporaryFiles -TemporaryDirectory $Temporary -Connection $Connection -Confirm:$false
        }
        Wait-Until { @($service.Requests | Where-Object Path -EQ '/upload/stopped-live.bin/2').Count -eq 1 -and $service.ActiveRequests -ge 1 }

        $elapsed = Measure-Command { $running.Shell.Stop() }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        $running.Shell.Dispose()
        @(Get-ApiRequest $service -Method POST -Path '/files/stopped-live.bin').Count | Should -Be 0
        @(Get-ChildItem -LiteralPath $temporary -Force).Count | Should -Be 0
        $stream.IsDisposed | Should -BeFalse
    }
}

Describe 'Send-FilesFile expert tuning, read-ahead budget and diagnostics' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service -Options @{ MaxNetworkRetries = 1 }
        # The same site in memory, where the parts of uploads given SendPartsToS3 go straight to Amazon S3.
        $inMemory = Connect-TestService -Service $service -InMemory -Options @{ MaxNetworkRetries = 1 }
        $content = New-RandomContent 200000
        $local = Join-Path $TestDrive 'tuned.bin'
        [System.IO.File]::WriteAllBytes($local, $content)
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $inMemory -Confirm:$false
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'keeps the profile of an ordinary multipart upload to Amazon S3, started at the policy''s start only with -AggressiveStartup, unless given concurrency tuning, which replaces it whatever the destination' {
        foreach ($remote in 'described.bin', 'off.bin', 'aggressive.bin', 'tuned.bin', 'tuned-off.bin', 'profiled-off.bin', 'static-off.bin') {
            $service.ServeUpload($remote, 16384)
            $service.SendPartsToS3($remote)
        }
        $tuning = New-FilesAutomaticConcurrency -Profile Agent -SoftCeiling 3 -MaxTarget 3 -PlateauTarget 3

        $described = Send-FilesFile -LiteralPath $local -RemotePath 'described.bin' -Connection $inMemory -PassThru -Confirm:$false
        $off = Send-FilesFile -LiteralPath $local -RemotePath 'off.bin' -AggressiveStartup:$false -Connection $inMemory -PassThru -Confirm:$false
        $aggressive = Send-FilesFile -LiteralPath $local -RemotePath 'aggressive.bin' -AggressiveStartup -Connection $inMemory -PassThru -Confirm:$false
        $tuned = Send-FilesFile -LiteralPath $local -RemotePath 'tuned.bin' -AutomaticConcurrency $tuning -Connection $inMemory -PassThru -Confirm:$false
        # -AggressiveStartup:$false is the default start, which the tuning -AggressiveStartup is refused with accepts.
        $tunedOff = Send-FilesFile -LiteralPath $local -RemotePath 'tuned-off.bin' -AutomaticConcurrency $tuning -AggressiveStartup:$false -Connection $inMemory -PassThru -Confirm:$false
        $profiledOff = Send-FilesFile -LiteralPath $local -RemotePath 'profiled-off.bin' -PerformanceProfile S3 -AggressiveStartup:$false -Connection $inMemory -PassThru -Confirm:$false
        $staticOff = Send-FilesFile -LiteralPath $local -RemotePath 'static-off.bin' -ConcurrencyMode Static -AggressiveStartup:$false -Connection $inMemory -PassThru -Confirm:$false

        foreach ($default in $described, $off) {
            $default.PerformanceProfile | Should -Be 'S3'
            $default.PerformanceProfileSource | Should -Be 'Server'
            # The scaled S3 policy an ordinary multipart upload to Amazon S3 gets by default, not the conservative S3 preset.
            # Its one part cannot fill the second window that establishes the warm baseline. The warm start begins at 1,
            # which is its lowest target unless it reaches the policy's start.
            $default.Metrics.MaxTarget | Should -Be 1024
            $default.Metrics.Target | Should -Be 1
            $default.Metrics.MinTarget | Should -Be 1
        }
        # -AggressiveStartup keeps the policy chosen for the destination and starts at its 150, with its own minimum of 8.
        $aggressive.PerformanceProfile | Should -Be 'S3'
        $aggressive.PerformanceProfileSource | Should -Be 'Server'
        $aggressive.Metrics.MaxTarget | Should -Be 1024
        $aggressive.Metrics.Target | Should -Be 150
        $aggressive.Metrics.MinTarget | Should -Be 8
        Test-SameContent -Expected $content -Actual ($service.Uploaded('aggressive.bin')) | Should -BeTrue
        foreach ($result in $tuned, $tunedOff) {
            $result.PerformanceProfile | Should -Be 'Agent'
            $result.PerformanceProfileSource | Should -Be 'Options'
            $result.Metrics.MinTarget | Should -Be 1
            $result.Metrics.MaxTarget | Should -Be 3
        }
        $tuning.Overrides | Should -Be @('SoftCeiling', 'MaxTarget', 'PlateauTarget')
        Test-SameContent -Expected $content -Actual ($service.Uploaded('tuned.bin')) | Should -BeTrue
        # The conservative S3 preset, up to 64 parts at once, and a static upload.
        $profiledOff.PerformanceProfile | Should -Be 'S3'
        $profiledOff.PerformanceProfileSource | Should -Be 'Options'
        $profiledOff.Metrics.MaxTarget | Should -Be 64
        $staticOff.ConcurrencyMode | Should -Be 'Static'
    }

    It 'starts at the tuning''s number of parts at once and never sends more than its maximum, or -MaxConcurrency' {
        $service.ServeUpload('start.bin', 8192)
        $service.DelayParts('start.bin', 100)
        $service.ServeUpload('capped.bin', 8192)
        $service.DelayParts('capped.bin', 100)
        $tuning = New-FilesAutomaticConcurrency -Profile Generic -StartTarget 3 -SoftCeiling 3 -MaxTarget 3 -PlateauTarget 3

        $started = Send-FilesFile -LiteralPath $local -RemotePath 'start.bin' -AutomaticConcurrency $tuning -Connection $connection -PassThru -Confirm:$false
        $capped = Send-FilesFile -LiteralPath $local -RemotePath 'capped.bin' -AutomaticConcurrency $tuning -MaxConcurrency 2 -Connection $connection -PassThru -Confirm:$false

        $service.PeakParts('start.bin') | Should -Be 3
        $started.PeakConcurrency | Should -Be 3
        $service.PeakParts('capped.bin') | Should -Be 2
        $capped.Metrics.MaxTarget | Should -Be 2
        Test-SameContent -Expected $content -Actual ($service.Uploaded('capped.bin')) | Should -BeTrue
    }

    It 'starts parts at the tuning''s tier for the file''s size, and reports when its minimum set the size' {
        $large = New-RandomContent 20MB
        $file = Join-Path $TestDrive 'tiered.bin'
        [System.IO.File]::WriteAllBytes($file, $large)
        foreach ($remote in 'tiers.bin', 'minimum.bin') {
            $service.ServeUpload($remote, 4096)
            $service.SendPartsToS3($remote)
        }
        $tiers = New-FilesAutomaticPartSizing -KnownSizeTiers @{ 100MB = 6MB; 1MB = 5MB } -WorkloadParts 1
        # 20 MiB over the default 16 workload parts prefers 1.25 MiB parts, below the tier's 5 MiB and the 5 MiB S3 requires;
        # the minimum sets 7 MiB.
        $minimum = New-FilesAutomaticPartSizing -KnownSizeTiers @{ 100MB = 5MB } -MinPartSize 7MB

        $tiered = Send-FilesFile -LiteralPath $file -RemotePath 'tiers.bin' -AutomaticPartSizing $tiers -Connection $inMemory -PassThru -Confirm:$false
        $floored = Send-FilesFile -LiteralPath $file -RemotePath 'minimum.bin' -AutomaticPartSizing $minimum -Diagnostics -InformationVariable trace -Connection $inMemory -PassThru -Confirm:$false

        $tiers.KnownSizeTiers.MaxFileBytes | Should -Be @(1048576, 104857600)
        @($tiered.Parts | Select-Object -SkipLast 1 | Where-Object Length -NE 6MB).Count | Should -Be 0
        @($tiered.Parts.ForEach('Length')) | Should -Be @(6MB, 6MB, 6MB, 2MB)
        $tiered.PartSizeConstraint | Should -Be 'None'
        @($floored.Parts | Select-Object -SkipLast 1 | Where-Object Length -NE 7MB).Count | Should -Be 0
        @($floored.Parts.ForEach('Length')) | Should -Be @(7MB, 7MB, 6MB)
        $floored.PartSizeConstraint | Should -Be 'MinPartSize'
        $chosen = @($trace.MessageData | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadSizingTrace] })[0]
        $chosen.Decision | Should -Be 'Chosen'
        $chosen.Constraint | Should -Be 'MinPartSize'
        Test-SameContent -Expected $large -Actual ($service.Uploaded('tiers.bin')) | Should -BeTrue
        Test-SameContent -Expected $large -Actual ($service.Uploaded('minimum.bin')) | Should -BeTrue
    }

    It 'grows the parts not yet read, and keeps the size and bytes of parts already read or retried' {
        $large = New-RandomContent 40MB
        $file = Join-Path $TestDrive 'growing.bin'
        [System.IO.File]::WriteAllBytes($file, $large)
        $service.ServeUpload('growth.bin', 16384)
        $service.SendPartsToS3('growth.bin')
        $service.FailPart('growth.bin', 2, 500)
        # One part at a time is the ceiling from the start, so adding parts at once cannot help and sizes may grow.
        $one = New-FilesAutomaticConcurrency -Profile Generic -MinTarget 1 -StartTarget 1 -SoftCeiling 1 -MaxTarget 1 -PlateauTarget 1 -WindowAttempts 1
        $growing = New-FilesAutomaticPartSizing -KnownSizeTiers @{ 100MB = 5MB } -WorkloadParts 1 -GrowthIntervalWindows 1 -ProbeWindows 1 -HoldWindows 0

        $result = Send-FilesFile -LiteralPath $file -RemotePath 'growth.bin' -AutomaticConcurrency $one -AutomaticPartSizing $growing -Diagnostics -InformationVariable trace -Connection $inMemory -PassThru -Confirm:$false

        $sizing = @($trace.MessageData | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadSizingTrace] })
        $sizing[0].Decision | Should -Be 'Chosen'
        $sizing[0].Size | Should -Be 5MB
        # A growth whose probe the engine rejects goes back to the smaller size and may grow again.
        @($sizing | Where-Object { $_.Decision -eq 'Grew' -and $_.PreviousSize -eq 5MB -and $_.Size -eq 10MB }).Count | Should -BeGreaterThan 0
        $result.Parts[0].Length | Should -Be 5MB
        @($result.Parts | ForEach-Object Length) | Should -Contain 10MB
        $result.Metrics.PartSizeGrows | Should -BeGreaterThan 0
        # The retried part was sent again with the same size and the same bytes, and every part is the file's bytes from
        # where the ones before it end.
        $retried = @($trace.MessageData | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadAttemptTrace] -and $_.PartNumber -eq 2 })
        $retried.Attempt | Should -Be @(1, 2)
        $retried.PartBytes | Should -Be @($result.Parts[1].Length, $result.Parts[1].Length)
        $attempts = $service.AttemptDigests('growth.bin', 2)
        $attempts.Count | Should -Be 2
        $attempts[1] | Should -Be $attempts[0]
        $service.PartLengths('growth.bin') | Should -Be @($result.Parts.ForEach('Length'))
        $end = 0
        foreach ($part in $result.Parts) {
            $part.Offset | Should -Be $end
            $end += $part.Length
        }
        Test-SameContent -Expected $large -Actual ($service.Uploaded('growth.bin')) | Should -BeTrue
    }

    It 'reads ahead within -MaxReadyParts, and within -MaxReadyBytes still reads one larger part at a time' {
        foreach ($remote in 'default.bin', 'ahead.bin', 'bytes.bin') {
            $service.ServeUpload($remote, 8192)
            $service.DelayParts($remote, 30)
        }

        $default = Send-FilesFile -LiteralPath $local -RemotePath 'default.bin' -Connection $connection -PassThru -Confirm:$false
        $ahead = Send-FilesFile -LiteralPath $local -RemotePath 'ahead.bin' -MaxReadyParts 64 -Connection $connection -PassThru -Confirm:$false
        $bytes = Send-FilesFile -LiteralPath $local -RemotePath 'bytes.bin' -MaxReadyParts 64 -MaxReadyBytes 1 -Connection $connection -PassThru -Confirm:$false

        # By default reading ahead stops at 2 waiting parts; 64 lets every part of this file wait; 1 byte stops it after each part.
        $default.Metrics.ReadyLimitWaits | Should -BeGreaterThan 0
        $ahead.Metrics.ReadyLimitWaits | Should -Be 0
        $bytes.Metrics.ReadyLimitWaits | Should -BeGreaterThan 0
        @($bytes.Parts | Select-Object -SkipLast 1 | Where-Object Length -NE 8192).Count | Should -Be 0
        Test-SameContent -Expected $content -Actual ($service.Uploaded('bytes.bin')) | Should -BeTrue
    }

    It 'pauses new part requests for the tuning''s backpressure pause after throttling, then recovers with the same bytes' {
        $service.ServeUpload('throttled.bin', 8192)
        $service.FailPart('throttled.bin', 2, 503)
        $patient = New-FilesAutomaticConcurrency -Profile Generic -BackpressurePauseSeconds 0.4

        $result = Send-FilesFile -LiteralPath $local -RemotePath 'throttled.bin' -AutomaticConcurrency $patient -Diagnostics -InformationVariable trace -Connection $connection -PassThru -Confirm:$false

        $records = $trace.MessageData
        $cooldown = @($records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadDecisionTrace] -and $_.Decision -eq 'CooldownStarted' })
        $cooldown.Count | Should -Be 1
        $cooldown[0].Cooldown | Should -Be ([timespan]::FromSeconds(0.4))
        @($records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadDecisionTrace] -and $_.Decision -eq 'ShrankAfterThrottling' }).Count | Should -Be 1
        $attempts = @($records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadAttemptTrace] -and $_.PartNumber -eq 2 })
        $attempts.StatusCode | Should -Be @(503, 200)
        $attempts[0].Throttled | Should -BeTrue
        # A timing proxy for the pause: the retry's trace time less its request duration, after the pause started.
        # The trace time is taken after the response is handled, so this includes that bookkeeping rather than the
        # exact send time, and can miss a retry sent slightly early.
        $attempts[1].Elapsed - $attempts[1].Duration - $cooldown[0].Elapsed | Should -BeGreaterOrEqual $cooldown[0].Cooldown
        $result.Metrics.Throttles | Should -Be 1
        ($result.Parts | Where-Object Number -EQ 2).Attempts | Should -Be 2
        Test-SameContent -Expected $content -Actual ($service.Uploaded('throttled.bin')) | Should -BeTrue
    }

    It 'refuses tuning the upload would refuse when it is made, naming the defaults it conflicts with' -TestCases @(
        @{ Label = 'a start above the profile''s soft ceiling'; Command = 'New-FilesAutomaticConcurrency'; Arguments = @{ Profile = 'Generic'; StartTarget = 12 }; Message = '-StartTarget 12 cannot be above SoftCeiling 8, the Generic default' }
        @{ Label = 'a maximum below the profile''s plateau'; Command = 'New-FilesAutomaticConcurrency'; Arguments = @{ Profile = 'S3'; StartTarget = 6; SoftCeiling = 6; MaxTarget = 6 }; Message = 'PlateauTarget 16, the S3 default, cannot be above -MaxTarget 6' }
        @{ Label = 'a latency backoff below its growth limit'; Command = 'New-FilesAutomaticConcurrency'; Arguments = @{ Profile = 'Generic'; LatencyGrowthQueueLimit = 5; LatencyBackoffQueueLimit = 3 }; Message = '-LatencyBackoffQueueLimit 3 cannot be below -LatencyGrowthQueueLimit 5' }
        @{ Label = 'a minimum part above the default maximum'; Command = 'New-FilesAutomaticPartSizing'; Arguments = @{ MinPartSize = 300MB }; Message = '-MinPartSize 314572800 cannot be above MaxPartSize 268435456, the default' }
        @{ Label = 'a growth factor of 1'; Command = 'New-FilesAutomaticPartSizing'; Arguments = @{ GrowthFactor = 1 }; Message = '-GrowthFactor 1 must be more than 1' }
    ) {
        param($Label, $Command, $Arguments, $Message)
        $failure = $null
        try {
            & $Command @Arguments -ErrorAction Stop
        }
        catch {
            $failure = $_
        }

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*' -Because $Label
        $failure.Exception.Message | Should -BeLike "$Message*"
    }

    It 'keeps sizes as whole numbers of bytes, and refuses a fraction or a file size given twice' {
        $exact = New-FilesAutomaticConcurrency -Profile S3 -SoftCeilingUnlockBytes 9007199254740993
        $sizing = New-FilesAutomaticPartSizing -WorkloadBytes 9007199254740993 -KnownSizeTiers ([ordered]@{ 4GB = 16MB; 256MB = 8MB })
        $tiers = $sizing.KnownSizeTiers

        $exact.SoftCeilingUnlockBytes | Should -Be ([long]9007199254740993)
        $sizing.WorkloadBytes | Should -Be ([long]9007199254740993)
        "$($tiers[0]); $($tiers[1])" | Should -Be '268435456 -> 8388608; 4294967296 -> 16777216'
        { New-FilesAutomaticPartSizing -KnownSizeTiers @{ 1MB = 1.5 } -ErrorAction Stop } | Should -Throw -ErrorId 'ParameterArgumentTransformationError*'
        { New-FilesAutomaticPartSizing -KnownSizeTiers (@($tiers) + $tiers[0]) -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        # The tuning is a snapshot: its tiers and overrides cannot be changed in place.
        { $tiers[0] = $tiers[1] } | Should -Throw
        { $sizing.Overrides[0] = 'MaxPartSize' } | Should -Throw
        { New-FilesAutomaticConcurrency -Profile Agent -BackpressurePauseSeconds 61 -ErrorAction Stop } | Should -Throw -ErrorId 'ParameterArgumentValidationError*'
    }

    It 'writes each part request and decision as a record on the Information stream, then a summary, and nothing unless asked' {
        $large = New-RandomContent 12MB
        $file = Join-Path $TestDrive 'traced.bin'
        [System.IO.File]::WriteAllBytes($file, $large)
        $service.ServeUpload('quiet.bin', 16384)
        # Its parts go to S3 at URLs presigned with PartSignature.
        $service.ServeUpload('traced.bin', 16384)
        $service.SendPartsToS3('traced.bin')
        $service.FailPart('traced.bin', 2, 503)

        Send-FilesFile -LiteralPath $local -RemotePath 'quiet.bin' -Connection $connection -InformationVariable quiet -Confirm:$false
        $output = @(Send-FilesFile -LiteralPath $file -RemotePath 'traced.bin' -Diagnostics -InformationVariable trace -Connection $inMemory -PassThru -Confirm:$false)

        @($quiet).Count | Should -Be 0
        $output.Count | Should -Be 1
        $output[0] | Should -BeOfType ([FilesCom.PowerShell.FilesUploadResult])
        @($trace.Tags | Select-Object -Unique) | Should -Be @('FilesCom.UploadTrace')
        $records = @($trace.MessageData)
        $attempts = @($records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadAttemptTrace] })
        $attempts.Count | Should -Be ($output[0].Parts.Count + 1)
        $throttled = @($attempts | Where-Object StatusCode -EQ 503)
        $throttled.PartNumber | Should -Be 2
        $throttled.Throttled | Should -BeTrue
        @($attempts | Where-Object Accepted).Count | Should -Be $output[0].Parts.Count
        @($records | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadSizingTrace] -and $_.Decision -eq 'Chosen' }).Count | Should -Be 1
        $summary = $records[-1]
        $summary | Should -BeOfType ([FilesCom.PowerShell.FilesUploadTraceSummary])
        $summary.Outcome | Should -Be 'Completed'
        $summary.RecordsWritten | Should -Be ($records.Count - 1)
        $summary.RecordsDropped | Should -Be 0
        $summary.Complete | Should -BeTrue
        # Records hold numbers, times and decisions: no storage address, signature or credential.
        $text = ($records | ConvertTo-Json -Depth 4) + ($trace | Out-String) + ($records | Format-List * | Out-String)
        $text | Should -Not -Match ([regex]::Escape([FilesCom.PowerShell.Tests.FilesTestService]::PartSignature))
        $text | Should -Not -Match ([regex]::Escape($script:ApiKeySentinel))
        $text | Should -Not -Match '/upload/'
        Test-SameContent -Expected $large -Actual ($service.Uploaded('traced.bin')) | Should -BeTrue
    }

    It 'names the warm start of a default upload straight to S3 in its decision records' {
        $content = New-RandomContent 200MB
        $file = Join-Path $TestDrive 'warm.bin'
        [System.IO.File]::WriteAllBytes($file, $content)
        # Its parts go to S3, so without tuning of its own it takes the scaled S3 policy and warm-starts at 1 part at a
        # time. Forty 5 MiB parts leave enough answers for the sampler's bounded window even when response times vary.
        # The 500 ms delay keeps the first target below the fast-link threshold. Its first raise is from 1 to 2;
        # further valid decisions depend on the measured windows and need not have a fixed count.
        $service.ServeUpload('warm.bin', 16384)
        $service.SendPartsToS3('warm.bin')
        $service.DelayParts('warm.bin', 500)

        Send-FilesFile -LiteralPath $file -RemotePath 'warm.bin' -Diagnostics -InformationVariable trace -Connection $inMemory -Confirm:$false

        $decisions = @($trace.MessageData | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadDecisionTrace] })
        $warm = @($decisions | Where-Object { $_.Decision -in 'WarmStartRaised', 'WarmStartFastLink', 'WarmStartSettled' })
        $warm.Count | Should -BeGreaterThan 0
        $warm[0].Decision | Should -Be 'WarmStartRaised'
        $warm[0].PreviousTarget | Should -Be 1
        $warm[0].Target | Should -Be 2
        @($decisions | Where-Object Decision -EQ 'Other').Count | Should -Be 0
        Test-SameContent -Expected $content -Actual ($service.Uploaded('warm.bin')) | Should -BeTrue
    }

    It 'keeps at most -DiagnosticLimit records of an upload and says how many it left out' {
        $service.ServeUpload('limited.bin', 8192)

        Send-FilesFile -LiteralPath $local -RemotePath 'limited.bin' -Diagnostics -DiagnosticLimit 3 -InformationVariable trace -Connection $connection -Confirm:$false

        $records = @($trace.MessageData)
        $records.Count | Should -Be 4
        $summary = $records[-1]
        $summary.RecordsWritten | Should -Be 3
        $summary.Limit | Should -Be 3
        $summary.RecordsDropped | Should -BeGreaterThan 0
        $summary.Complete | Should -BeFalse
    }

    It 'writes the summary of an upload that fails, and then its failure unchanged' {
        $service.ServeUpload('refused.bin', 8192)
        $service.FailPart('refused.bin', 2, 400)

        $failure = $null
        try {
            Send-FilesFile -LiteralPath $local -RemotePath 'refused.bin' -Diagnostics -InformationVariable trace -Connection $connection -Confirm:$false -ErrorAction Stop
        }
        catch {
            $failure = $_
        }

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidResponse*'
        $summary = @($trace.MessageData)[-1]
        $summary | Should -BeOfType ([FilesCom.PowerShell.FilesUploadTraceSummary])
        $summary.Outcome | Should -Be 'Failed'
        @(Get-ApiRequest $service -Method POST -Path '/files/refused.bin').Count | Should -Be 0
    }
}

Describe 'Receive-FilesFile' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        # The same site in memory, where versioned downloads come from Amazon S3.
        $inMemory = Connect-TestService -Service $service -InMemory
        $content = New-RandomContent 50000
        $service.ServeDownload('reports/q3.csv', $content)
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $inMemory -Confirm:$false
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'writes the exact bytes to a local file and describes the download with -PassThru' {
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'exact')
        $destination = Join-Path $folder.FullName 'q3.csv'

        $result = Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $destination -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        $result.Size | Should -Be 50000
        $result.LocalPath | Should -Be $destination
        $result.RemotePath | Should -Be 'reports/q3.csv'
        $result.ReceivedBytes | Should -Be 50000
        # The file's storage is the service's own address, over HTTP, which the engine binds no download to: it read the
        # file whole, bound to no version of it.
        $result.VersionBound | Should -BeFalse
        $result.Parts | Should -Be 1
        $result.ResumedBytes | Should -Be 0
        $result.CompletedEarlier | Should -BeFalse
        $result.File | Should -BeNullOrEmpty
        # The engine's stage is gone once the file is in place.
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('q3.csv')
    }

    It 'fetches a file whole, in one request, from storage other than Amazon S3 over HTTPS, even storage that answers ranges, and starts an interrupted download over from its first byte' {
        $versioned = New-RandomContent 50000
        $service.ServeVersionedDownload('reports/http.bin', $versioned, 'v1')
        $whole = Join-Path $TestDrive 'http.bin'
        $destination = Join-Path $TestDrive 'interrupted.csv'
        $checkpoint = Join-Path $TestDrive 'interrupted.checkpoint'

        $result = Receive-FilesFile -RemotePath 'reports/http.bin' -LiteralPath $whole -PartSize 16384 -Connection $connection -PassThru -Confirm:$false
        $wholeRequests = @($service.Requests | Where-Object { $_.Path -like '/download/*' })
        $service.HoldDownload('reports/q3.csv', 1000)
        $running = Start-Command -Connection $connection -Arguments $destination, $checkpoint -Script {
            param($Connection, $Destination, $Checkpoint)
            Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $Destination -CheckpointPath $Checkpoint -Connection $Connection -Confirm:$false
        }
        Wait-Until { $service.IsDownloadHeld('reports/q3.csv') }
        $running.Shell.Stop()
        $running.Shell.Dispose()
        $service.ReleaseDownload('reports/q3.csv')
        Wait-Until { $service.ActiveRequests -eq 0 }
        $service.ClearRequests()
        $continued = Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $destination -CheckpointPath $checkpoint -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $versioned -Actual ([System.IO.File]::ReadAllBytes($whole)) | Should -BeTrue
        $result.VersionBound | Should -BeFalse
        $result.Parts | Should -Be 1
        $wholeRequests.Count | Should -Be 1
        $wholeRequests[0].Header('Range') | Should -BeNullOrEmpty
        $wholeRequests[0].Header('If-Match') | Should -BeNullOrEmpty
        # The bytes the stopped run had received are not used: the file comes again, whole, from its first byte.
        $continued.VersionBound | Should -BeFalse
        $continued.ResumedBytes | Should -Be 0
        $continued.ReceivedBytes | Should -Be 50000
        $storage = @($service.Requests | Where-Object { $_.Path -like '/download/*' })
        $storage.Count | Should -Be 1
        $storage[0].Header('Range') | Should -BeNullOrEmpty
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
    }

    It 'reports progress from the pipeline thread and completes it' {
        $destination = Join-Path $TestDrive 'progress.csv'
        $shell = [powershell]::Create()
        try {
            $shell.AddScript({
                    param($Manifest, $Destination, $Connection)
                    Import-Module -Name $Manifest
                    Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $Destination -Connection $Connection -Confirm:$false
                }).AddArgument($script:ModuleManifest).AddArgument($destination).AddArgument($connection) | Out-Null
            $shell.Invoke() | Out-Null
            $records = @($shell.Streams.Progress)
        }
        finally {
            $shell.Dispose()
        }

        $records.Count | Should -BeGreaterThan 0
        $records[-1].RecordType | Should -Be 'Completed'
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
    }

    It 'publishes the bytes it downloaded when another process swaps its private staging folder in the folder, and leaves the substitute alone' {
        $service.HoldDownload('reports/q3.csv', 1000)
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'swapped-entry')
        $destination = Join-Path $folder.FullName 'raced.csv'
        $running = Start-Command -Connection $connection -Arguments $destination -Script {
            param($Connection, $Destination)
            Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $Destination -Connection $Connection -PassThru -Confirm:$false
        }
        Wait-Until { $service.IsDownloadHeld('reports/q3.csv') -and @(Get-ChildItem -LiteralPath $folder.FullName -Force -Directory).Count -eq 1 }
        # The engine stages the file in a private folder of its own beside the destination.
        $staging = Get-ChildItem -LiteralPath $folder.FullName -Force -Directory
        $swapped = $true
        try {
            Rename-Item -LiteralPath $staging.FullName -NewName 'moved-staging' -ErrorAction Stop
            New-Item -ItemType Directory -Path $staging.FullName | Out-Null
            Set-Content -LiteralPath (Join-Path $staging.FullName 'substitute.txt') -Value 'SUBSTITUTED-CONTENT'
        }
        catch {
            $swapped = $false
        }
        $service.ReleaseDownload('reports/q3.csv')
        $result = $running.Shell.EndInvoke($running.Handle)
        $running.Shell.Dispose()

        if (-not $IsWindows) {
            $swapped | Should -BeTrue
        }
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        $result[0].Size | Should -Be 50000
        if ($swapped) {
            Get-Content -LiteralPath (Join-Path $staging.FullName 'substitute.txt') | Should -Be 'SUBSTITUTED-CONTENT'
        }
    }

    It 'keeps its stage private from other users, and refuses to put in place what another process of the same user puts at its stage''s name' {
        $service.HoldDownload('reports/q3.csv', 1000)
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'substituted-bytes')
        $destination = Join-Path $folder.FullName 'substituted.csv'
        $running = Start-Command -Connection $connection -Arguments $destination -Script {
            param($Connection, $Destination)
            Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $Destination -Connection $Connection -PassThru -Confirm:$false
        }
        Wait-Until { $service.IsDownloadHeld('reports/q3.csv') -and @(Get-ChildItem -LiteralPath $folder.FullName -Force -Directory | Get-ChildItem -Force -File).Count -eq 1 }
        $staging = Get-ChildItem -LiteralPath $folder.FullName -Force -Directory
        $stage = Get-ChildItem -LiteralPath $staging.FullName -Force -File
        if (-not $IsWindows) {
            [System.IO.File]::GetUnixFileMode($staging.FullName) | Should -Be ([System.IO.UnixFileMode]'UserRead, UserWrite, UserExecute')
            [System.IO.File]::GetUnixFileMode($stage.FullName) | Should -Be ([System.IO.UnixFileMode]'UserRead, UserWrite')
        }
        # On Windows the open stage cannot be moved; elsewhere a process of the same user can move it and put a file at its name.
        $replaced = $true
        try {
            Move-Item -LiteralPath $stage.FullName -Destination "$($stage.FullName).original" -ErrorAction Stop
            Set-Content -LiteralPath $stage.FullName -Value 'SUBSTITUTED-CONTENT'
        }
        catch {
            $replaced = $false
        }
        $service.ReleaseDownload('reports/q3.csv')
        $output = @($running.Shell.EndInvoke($running.Handle))
        $failures = @($running.Shell.Streams.Error)
        $running.Shell.Dispose()

        if (-not $IsWindows) {
            $replaced | Should -BeTrue
        }
        if ($replaced) {
            # Nothing that looks like success: no result, and one error that says the downloaded file is not in place.
            $output | Should -BeNullOrEmpty
            $failures.Count | Should -Be 1
            $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
            $failures[0].CategoryInfo.Category | Should -Be 'WriteError'
            $failures[0].Exception.PublicationProblem | Should -Be 'StageReplaced'
            $failures[0].Exception.DownloadPlaced | Should -BeFalse
            $failures[0].Exception.DownloadRetained | Should -BeFalse
            $failures[0].Exception.LocalPath | Should -Be $destination
            $failures[0].ErrorDetails.RecommendedAction | Should -Not -Match '-Force'
            Test-Path -LiteralPath $destination | Should -BeFalse
            # What the other process put at the stage's name, and the stage it moved aside, are left as they are.
            Get-Content -LiteralPath $stage.FullName | Should -Be 'SUBSTITUTED-CONTENT'
            (Get-Item -LiteralPath "$($stage.FullName).original").Length | Should -Be 50000
        }
        else {
            $failures | Should -BeNullOrEmpty
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        }
    }

    It 'puts the file nowhere, and says the folder moved, when the destination folder is replaced during the download' {
        $parent = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'replaced-folder')
        $folder = Join-Path $parent.FullName 'into'
        New-Item -ItemType Directory -Path $folder | Out-Null
        $destination = Join-Path $folder 'report.csv'
        $service.HoldDownload('reports/q3.csv', 1000)
        $running = Start-Command -Connection $connection -Arguments $destination -Script {
            param($Connection, $Destination)
            Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $Destination -Connection $Connection -PassThru -Confirm:$false
        }
        Wait-Until { $service.IsDownloadHeld('reports/q3.csv') }
        $moved = $true
        try {
            Rename-Item -LiteralPath $folder -NewName 'into-original' -ErrorAction Stop
            New-Item -ItemType Directory -Path $folder | Out-Null
        }
        catch {
            $moved = $false
        }
        $service.ReleaseDownload('reports/q3.csv')
        $output = @($running.Shell.EndInvoke($running.Handle))
        $failures = @($running.Shell.Streams.Error)
        $running.Shell.Dispose()

        # Windows holds the destination folder, and those above it, from the start of the download.
        $moved | Should -Be (-not $IsWindows)
        if ($moved) {
            $output | Should -BeNullOrEmpty
            $failures.Count | Should -Be 1
            $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
            $failures[0].CategoryInfo.Category | Should -Be 'ResourceUnavailable'
            $failures[0].Exception.PublicationProblem | Should -Be 'DestinationMoved'
            $failures[0].Exception.DownloadPlaced | Should -BeFalse
            $failures[0].Exception.DownloadRetained | Should -BeFalse
            $failures[0].Exception.LocalPath | Should -Be $destination
            # Neither the folder at the path nor the one the download opened holds the file or a stage.
            @(Get-ChildItem -LiteralPath $folder -Force) | Should -BeNullOrEmpty
            @(Get-ChildItem -LiteralPath (Join-Path $parent.FullName 'into-original') -Force) | Should -BeNullOrEmpty
        }
        else {
            $failures | Should -BeNullOrEmpty
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        }
    }

    It 'gives a downloaded file the permissions any new file gets, and keeps those of a file it replaces' {
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'permissions')
        $new = Join-Path $folder.FullName 'new.csv'
        $replaced = Join-Path $folder.FullName 'replaced.csv'
        $reference = Join-Path $folder.FullName 'reference.txt'
        Set-Content -LiteralPath $reference -Value 'made by PowerShell'
        Set-Content -LiteralPath $replaced -Value 'old'
        if (-not $IsWindows) {
            [System.IO.File]::SetUnixFileMode($replaced, [System.IO.UnixFileMode]'UserRead, UserWrite, GroupRead')
        }

        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $new -Connection $connection -Confirm:$false
        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $replaced -Connection $connection -Force -Confirm:$false

        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($replaced)) | Should -BeTrue
        if (-not $IsWindows) {
            [System.IO.File]::GetUnixFileMode($new) | Should -Be ([System.IO.File]::GetUnixFileMode($reference))
            [System.IO.File]::GetUnixFileMode($replaced) | Should -Be ([System.IO.UnixFileMode]'UserRead, UserWrite, GroupRead')
        }
    }

    It 'lets Out-FilesLocalFile write into a folder it may add files to but not list' {
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'add-only')
        if ($IsWindows) {
            $user = [System.Security.Principal.WindowsIdentity]::GetCurrent().User
            $original = [System.IO.FileSystemAclExtensions]::GetAccessControl([System.IO.DirectoryInfo]$folder)
            $addOnly = [System.Security.AccessControl.DirectorySecurity]::new()
            $addOnly.SetAccessRuleProtection($true, $false)
            $addOnly.AddAccessRule([System.Security.AccessControl.FileSystemAccessRule]::new($user, 'CreateFiles, ReadAttributes, Synchronize', 'Allow'))
            $addOnly.AddAccessRule([System.Security.AccessControl.FileSystemAccessRule]::new($user, 'FullControl', 'ObjectInherit', 'InheritOnly', 'Allow'))
            [System.IO.FileSystemAclExtensions]::SetAccessControl([System.IO.DirectoryInfo]$folder, $addOnly)
        }
        else {
            [System.IO.File]::SetUnixFileMode($folder.FullName, [System.IO.UnixFileMode]'UserWrite, UserExecute')
        }
        try {
            'report' | Out-FilesLocalFile -LiteralPath (Join-Path $folder.FullName 'report.txt') -WarningVariable warnings

            { Get-ChildItem -LiteralPath $folder.FullName -ErrorAction Stop } | Should -Throw
        }
        finally {
            if ($IsWindows) {
                [System.IO.FileSystemAclExtensions]::SetAccessControl([System.IO.DirectoryInfo]$folder, $original)
            }
            else {
                [System.IO.File]::SetUnixFileMode($folder.FullName, [System.IO.UnixFileMode]'UserRead, UserWrite, UserExecute')
            }
        }
        Get-Content -LiteralPath (Join-Path $folder.FullName 'report.txt') | Should -Be 'report'
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('report.txt')
        $warnings | Should -BeNullOrEmpty
    }

    # Where it can, the engine opens the destination folder for searching only, so a folder that may not be listed is fine.
    if (-not $IsWindows) {
        It 'downloads into a folder it may add files to but not list, with the exact bytes and nothing else left there' {
            $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'add-only-download')
            [System.IO.File]::SetUnixFileMode($folder.FullName, [System.IO.UnixFileMode]'UserWrite, UserExecute')
            try {
                $result = Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath (Join-Path $folder.FullName 'q3.csv') -Connection $connection -Confirm:$false -PassThru -ErrorVariable failures -ErrorAction SilentlyContinue
                { Get-ChildItem -LiteralPath $folder.FullName -ErrorAction Stop } | Should -Throw
            }
            finally {
                [System.IO.File]::SetUnixFileMode($folder.FullName, [System.IO.UnixFileMode]'UserRead, UserWrite, UserExecute')
            }

            $failures | Should -BeNullOrEmpty
            $result.Size | Should -Be 50000
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes((Join-Path $folder.FullName 'q3.csv'))) | Should -BeTrue
            (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('q3.csv')
        }
    }

    It 'publishes files that a folder listing shows, and keeps a file it replaces as visible or hidden as it was' {
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'visibility')
        $new = Join-Path $folder.FullName 'new.csv'
        $visible = Join-Path $folder.FullName 'visible.csv'
        # Hidden by its attribute on Windows, by its name elsewhere.
        $hidden = Join-Path $folder.FullName '.hidden.csv'
        Set-Content -LiteralPath $visible -Value 'old'
        Set-Content -LiteralPath $hidden -Value 'old'
        if ($IsWindows) {
            (Get-Item -LiteralPath $hidden -Force).Attributes = 'Hidden, Archive'
        }

        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $new -Connection $connection -Confirm:$false
        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $visible -Force -Connection $connection -Confirm:$false
        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $hidden -Force -Connection $connection -Confirm:$false

        (Get-ChildItem -LiteralPath $folder.FullName).Name | Sort-Object | Should -Be @('new.csv', 'visible.csv')
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Sort-Object | Should -Be @('.hidden.csv', 'new.csv', 'visible.csv')
        if ($IsWindows) {
            (Get-Item -LiteralPath $new -Force).Attributes.HasFlag([System.IO.FileAttributes]::Hidden) | Should -BeFalse
            (Get-Item -LiteralPath $visible -Force).Attributes.HasFlag([System.IO.FileAttributes]::Hidden) | Should -BeFalse
            (Get-Item -LiteralPath $hidden -Force).Attributes.HasFlag([System.IO.FileAttributes]::Hidden) | Should -BeTrue
        }
        foreach ($path in $new, $visible, $hidden) {
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($path)) | Should -BeTrue
        }
    }

    # Folders other users may change, and sticky ones, are macOS and Linux matters.
    if (-not $IsWindows) {
        It 'leaves nothing of its own, and warns of nothing, in a folder other users may change or a sticky one' {
            $service.ServeDownload('reports/q4.csv', $content)
            $service.Respond('GET', '/file_actions/metadata/reports/q3.csv', 200, '{"path":"reports/q3.csv","type":"file"}')
            $service.Respond('GET', '/file_actions/metadata/reports/q4.csv', 200, '{"path":"reports/q4.csv","type":"file"}')
            $files = @('reports/q3.csv', 'reports/q4.csv') | ForEach-Object { Get-FilesFile -Path $_ -Connection $connection }
            $shared = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'shared')
            $sticky = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'sticky')
            [System.IO.File]::SetUnixFileMode($shared.FullName, [System.IO.UnixFileMode]'UserRead, UserWrite, UserExecute, GroupRead, GroupWrite, GroupExecute, OtherRead, OtherWrite, OtherExecute')
            [System.IO.File]::SetUnixFileMode($sticky.FullName, [System.IO.UnixFileMode]'UserRead, UserWrite, UserExecute, GroupRead, GroupWrite, GroupExecute, OtherRead, OtherWrite, OtherExecute, StickyBit')

            $files | Receive-FilesFile -LiteralPath $shared.FullName -Confirm:$false -WarningVariable sharedWarnings
            $files | Receive-FilesFile -LiteralPath $sticky.FullName -Confirm:$false -WarningVariable stickyWarnings

            (Get-ChildItem -LiteralPath $shared.FullName -Force).Name | Sort-Object | Should -Be @('q3.csv', 'q4.csv')
            (Get-ChildItem -LiteralPath $sticky.FullName -Force).Name | Sort-Object | Should -Be @('q3.csv', 'q4.csv')
            $sharedWarnings | Should -BeNullOrEmpty
            $stickyWarnings | Should -BeNullOrEmpty
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes((Join-Path $shared.FullName 'q4.csv'))) | Should -BeTrue
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes((Join-Path $sticky.FullName 'q4.csv'))) | Should -BeTrue
        }
    }

    # Access control entries that do not show in a folder's mode are macOS's.
    if ($IsMacOS) {
        It 'keeps access rules a folder passes on out of its private staging folder and stage, and leaves nothing behind' {
            $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'access-rules')
            & /bin/chmod +a 'user:nobody allow add_file,add_subdirectory,delete_child,file_inherit,directory_inherit' $folder.FullName
            $service.HoldDownload('reports/q3.csv', 1000)
            try {
                $running = Start-Command -Connection $connection -Arguments (Join-Path $folder.FullName 'q3.csv') -Script {
                    param($Connection, $Destination)
                    Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $Destination -Connection $Connection -Confirm:$false
                }
                Wait-Until { $service.IsDownloadHeld('reports/q3.csv') -and @(Get-ChildItem -LiteralPath $folder.FullName -Force -Directory | Get-ChildItem -Force -File).Count -eq 1 }
                $staging = Get-ChildItem -LiteralPath $folder.FullName -Force -Directory
                $rules = @(& /bin/ls -lde $staging.FullName) + @(& /bin/ls -le $staging.FullName)
                $service.ReleaseDownload('reports/q3.csv')
                $running.Shell.EndInvoke($running.Handle) | Out-Null
                $warnings = @($running.Shell.Streams.Warning)
                $failures = @($running.Shell.Streams.Error)
                $running.Shell.Dispose()
            }
            finally {
                & /bin/chmod -N $folder.FullName
            }

            $rules.Count | Should -BeGreaterThan 1
            $rules -join "`n" | Should -Not -Match 'nobody'
            $failures | Should -BeNullOrEmpty
            $warnings | Should -BeNullOrEmpty
            (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('q3.csv')
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes((Join-Path $folder.FullName 'q3.csv'))) | Should -BeTrue
        }
    }

    # Groups and access control lists beyond the mode are Unix and macOS matters.
    if ($CanChangeGroup) {
        It 'gives a file that replaces another the replaced file''s group and permissions, not the folder''s, and a new file the folder''s' {
            $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'groups')
            $folderGroup = Get-FileGroup $folder.FullName
            $otherGroup = Get-OtherGroup -Except $folderGroup
            $downloaded = Join-Path $folder.FullName 'downloaded.csv'
            $written = Join-Path $folder.FullName 'written.txt'
            $reference = Join-Path $folder.FullName 'reference.txt'
            Set-Content -LiteralPath $reference -Value 'made by PowerShell'
            foreach ($path in $downloaded, $written) {
                Set-Content -LiteralPath $path -Value 'old'
                & chgrp $otherGroup $path
                [System.IO.File]::SetUnixFileMode($path, [System.IO.UnixFileMode]'UserRead, UserWrite, GroupRead')
            }

            Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $downloaded -Force -Connection $connection -Confirm:$false
            'new' | Out-FilesLocalFile -LiteralPath $written -Force
            Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath (Join-Path $folder.FullName 'fresh.csv') -Connection $connection -Confirm:$false

            $otherGroup | Should -Not -Be $folderGroup
            foreach ($path in $downloaded, $written) {
                Get-FileGroup $path | Should -Be $otherGroup
                [System.IO.File]::GetUnixFileMode($path) | Should -Be ([System.IO.UnixFileMode]'UserRead, UserWrite, GroupRead')
            }
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($downloaded)) | Should -BeTrue
            Get-Content -LiteralPath $written | Should -Be 'new'
            Get-FileGroup (Join-Path $folder.FullName 'fresh.csv') | Should -Be (Get-FileGroup $reference)
            [System.IO.File]::GetUnixFileMode((Join-Path $folder.FullName 'fresh.csv')) | Should -Be ([System.IO.File]::GetUnixFileMode($reference))
        }
    }

    if ($IsMacOS) {
        It 'carries over the access control list of the file it replaces, with its effect' {
            $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'acl-replacement')
            $path = Join-Path $folder.FullName 'restricted.csv'
            Set-Content -LiteralPath $path -Value 'old'
            & /bin/chmod +a 'user:nobody allow read' $path
            # Denies the owner what the mode allows, so the effect shows without another user.
            & /bin/chmod +a "user:$([Environment]::UserName) deny read" $path
            $before = @(& /bin/ls -le $path | Select-Object -Skip 1)
            try {
                Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $path -Force -Connection $connection -Confirm:$false

                @(& /bin/ls -le $path | Select-Object -Skip 1) | Should -Be $before
                { [System.IO.File]::ReadAllBytes($path) } | Should -Throw
            }
            finally {
                & /bin/chmod -N $path
            }
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($path)) | Should -BeTrue
            (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('restricted.csv')
        }
    }

    if ($ForeignGroupFixture) {
        It 'replaces nothing, and leaves no stage, when it cannot give the new file the replaced file''s group' {
            $fixture = New-Item -ItemType Directory -Path "/private/tmp/filescom-tests-$([guid]::NewGuid().ToString('N'))"
            try {
                $source = Join-Path $fixture.FullName 'locked.csv'
                Set-Content -LiteralPath $source -Value 'KEEP'
                [System.IO.File]::SetUnixFileMode($source, [System.IO.UnixFileMode]'UserRead, UserWrite, GroupRead')
                # The stage takes this folder's group, the user's own, so it differs from the replaced file's.
                $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'foreign-group')
                & chgrp (& id -g) $folder.FullName
                $path = Join-Path $folder.FullName 'locked.csv'
                # A rename keeps the group /private/tmp gave the file, which the user is not in.
                Move-Item -LiteralPath $source -Destination $path
                $group = Get-FileGroup $path

                $failure = try { Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $path -Force -Connection $connection -Confirm:$false -ErrorAction Stop; $null } catch { $_ }

                $group | Should -Be (& /usr/bin/stat -f '%g' /private/tmp)
                Get-FileGroup $folder.FullName | Should -Not -Be $group
                $failure.FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
                $failure.CategoryInfo.Category | Should -Be 'PermissionDenied'
                $failure.Exception.PublicationProblem | Should -Be 'AccessNotKept'
                $failure.Exception.Message | Should -Match 'group .*not replaced'
                Get-Content -LiteralPath $path | Should -Be 'KEEP'
                Get-FileGroup $path | Should -Be $group
                [System.IO.File]::GetUnixFileMode($path) | Should -Be ([System.IO.UnixFileMode]'UserRead, UserWrite, GroupRead')
                (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('locked.csv')
            }
            finally {
                Remove-Item -LiteralPath $fixture.FullName -Recurse -Force
            }
        }
    }

    It 'downloads a file from Amazon S3 bound to the version its first range answers with, in ranged parts of -PartSize, each later one asking for that version' {
        $versioned = New-RandomContent 50000
        $service.ServeVersionedDownload('reports/versioned.bin', $versioned, 'v1')
        $destination = Join-Path $TestDrive 'versioned.bin'

        $result = Receive-FilesFile -RemotePath 'reports/versioned.bin' -LiteralPath $destination -PartSize 16384 -MaxConcurrency 2 -Connection $inMemory -PassThru -Confirm:$false

        Test-SameContent -Expected $versioned -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        $result.VersionBound | Should -BeTrue
        $result.Parts | Should -Be 4
        $result.Size | Should -Be 50000
        @($service.RangesServed('reports/versioned.bin') | Sort-Object { [long]($_ -split '-')[0] }) | Should -Be @('0-16383', '16384-32767', '32768-49151', '49152-49999')
        # The first range, asked for alone and unconditionally, gives the version; every later one asks for it.
        $storage = @($service.Requests | Where-Object { $_.Path -like '/download/*' })
        @($storage | ForEach-Object { $_.Header('Host') } | Sort-Object -Unique) | Should -Be @([FilesCom.PowerShell.Tests.FilesTestService]::S3Host)
        $storage[0].Header('Range') | Should -Be 'bytes=0-16383'
        $storage[0].Header('If-Match') | Should -BeNullOrEmpty
        @($storage | Select-Object -Skip 1 | ForEach-Object { $_.Header('If-Match') } | Sort-Object -Unique) | Should -Be @('"v1"')
    }

    It 'publishes nothing, and leaves no stage, when the version being downloaded is gone, and refuses the web app''s answer' {
        $versioned = New-RandomContent 50000
        $service.ServeVersionedDownload('reports/changed.bin', $versioned, 'v1')
        $service.HoldRange('reports/changed.bin', 16384)
        $service.ServeDownload('reports/app.bin', $content, @{ 'X-Files-Frontend-App' = 'true' })
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'refused')
        $changed = Join-Path $folder.FullName 'changed.bin'
        $app = Join-Path $folder.FullName 'app.bin'
        $running = Start-Command -Connection $inMemory -Arguments $changed -Script {
            param($Connection, $Destination)
            Receive-FilesFile -RemotePath 'reports/changed.bin' -LiteralPath $Destination -PartSize 16384 -MaxConcurrency 1 -Connection $Connection -Confirm:$false
        }
        # The file changes once its first range has bound the download to version v1.
        Wait-Until { $service.IsRangeHeld('reports/changed.bin', 16384) }
        $reversed = [byte[]] $versioned.Clone()
        [System.Array]::Reverse($reversed)
        $service.ChangeVersion('reports/changed.bin', $reversed, 'v2')
        $service.ReleaseRange('reports/changed.bin', 16384)
        $running.Shell.EndInvoke($running.Handle) | Out-Null
        $failure = @($running.Shell.Streams.Error)[0]
        $running.Shell.Dispose()
        $refusal = try { Receive-FilesFile -RemotePath 'reports/app.bin' -LiteralPath $app -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }

        # The part after the first asked for v1, which storage no longer had.
        @($service.Requests | Where-Object { $_.Header('Range') -eq 'bytes=16384-32767' } | ForEach-Object { $_.Header('If-Match') }) | Should -Be @('"v1"')
        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.SourceChanged*'
        $failure.CategoryInfo.Category | Should -Be 'ResourceUnavailable'
        $failure.ErrorDetails.RecommendedAction | Should -Match 'again'
        $refusal.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidResponse*'
        Test-Path -LiteralPath $changed | Should -BeFalse
        Test-Path -LiteralPath $app | Should -BeFalse
        @(Get-ChildItem -LiteralPath $folder.FullName -Force).Count | Should -Be 0
    }

    It 'refuses -PartSize and -MaxConcurrency outside what the engine allows before sending anything' {
        { Receive-FilesFile -RemotePath 'reports/q3.csv' -OutputStream ([System.IO.MemoryStream]::new()) -MaxConcurrency 65 -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'ParameterArgumentValidationError*'
        { Receive-FilesFile -RemotePath 'reports/q3.csv' -OutputStream ([System.IO.MemoryStream]::new()) -PartSize 0 -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'ParameterArgumentValidationError*'
        $service.Requests.Count | Should -Be 0
    }

    It 'leaves an existing file untouched and sends nothing without -Force, refusing it under -WhatIf too, and replaces it with -Force' {
        $destination = Join-Path $TestDrive 'existing.csv'
        Set-Content -LiteralPath $destination -Value 'keep me'

        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $destination -Connection $connection -Confirm:$false -ErrorVariable failures -ErrorAction SilentlyContinue
        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $destination -Connection $connection -WhatIf -ErrorVariable +failures -ErrorAction SilentlyContinue

        $failures.Count | Should -Be 2
        foreach ($failure in $failures) {
            $failure.FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
            $failure.CategoryInfo.Category | Should -Be 'ResourceExists'
            $failure.Exception.PublicationProblem | Should -Be 'DestinationExists'
            $failure.Exception.LocalPath | Should -Be $destination
            $failure.ErrorDetails.RecommendedAction | Should -Match '-Force'
        }
        Get-Content -LiteralPath $destination | Should -Be 'keep me'
        $service.Requests.Count | Should -Be 0

        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $destination -Force -Connection $connection -Confirm:$false
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
    }

    It 'replaces only a regular file with -Force, refusing a folder at the name before sending anything and leaving it as it is' {
        $parent = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'folder-at-name')
        $occupied = New-Item -ItemType Directory -Path (Join-Path $parent.FullName 'q3.csv')
        Set-Content -LiteralPath (Join-Path $occupied.FullName 'inside.txt') -Value 'INSIDE'

        $forced = try { Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $parent.FullName -Force -Connection $connection -Confirm:$false -ErrorAction Stop; $null } catch { $_ }
        $plain = try { Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $parent.FullName -Connection $connection -Confirm:$false -ErrorAction Stop; $null } catch { $_ }

        $forced.FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
        $forced.CategoryInfo.Category | Should -Be 'InvalidType'
        $forced.Exception.PublicationProblem | Should -Be 'DestinationNotRegularFile'
        $forced.Exception.LocalPath | Should -Be $occupied.FullName
        $plain.Exception.PublicationProblem | Should -Be 'DestinationExists'
        Get-Content -LiteralPath (Join-Path $occupied.FullName 'inside.txt') | Should -Be 'INSIDE'
        (Get-ChildItem -LiteralPath $parent.FullName -Force).Name | Should -Be @('q3.csv')
        $service.Requests.Count | Should -Be 0
    }

    # Symbolic links need no privilege on macOS and Linux.
    if (-not $IsWindows) {
        It 'never replaces or follows a link at the name, even with -Force, and refuses it before sending anything' {
            $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'link-at-name')
            $target = Join-Path $folder.FullName 'target.txt'
            Set-Content -LiteralPath $target -Value 'TARGET'
            $link = Join-Path $folder.FullName 'q3.csv'
            $dangling = Join-Path $folder.FullName 'dangling.csv'
            [System.IO.File]::CreateSymbolicLink($link, $target) | Out-Null
            [System.IO.File]::CreateSymbolicLink($dangling, (Join-Path $folder.FullName 'nowhere.csv')) | Out-Null

            $forced = try { Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $link -Force -Connection $connection -Confirm:$false -ErrorAction Stop; $null } catch { $_ }
            $plain = try { Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $dangling -Connection $connection -Confirm:$false -ErrorAction Stop; $null } catch { $_ }

            $forced.Exception.PublicationProblem | Should -Be 'DestinationNotRegularFile'
            $plain.Exception.PublicationProblem | Should -Be 'DestinationExists'
            Get-Content -LiteralPath $target | Should -Be 'TARGET'
            (Get-Item -LiteralPath $link -Force).LinkTarget | Should -Be $target
            Test-Path -LiteralPath (Join-Path $folder.FullName 'nowhere.csv') | Should -BeFalse
            $service.Requests.Count | Should -Be 0
        }
    }

    It 'leaves a file that appears at the name during the download as it is, and no stage' {
        $service.HoldDownload('reports/q3.csv', 1000)
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'appeared')
        $destination = Join-Path $folder.FullName 'q3.csv'
        $running = Start-Command -Connection $connection -Arguments $destination -Script {
            param($Connection, $Destination)
            Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $Destination -Connection $Connection -PassThru -Confirm:$false
        }
        Wait-Until { $service.IsDownloadHeld('reports/q3.csv') }
        Set-Content -LiteralPath $destination -Value 'APPEARED'
        $service.ReleaseDownload('reports/q3.csv')
        $result = $running.Shell.EndInvoke($running.Handle)
        $failures = @($running.Shell.Streams.Error)
        $running.Shell.Dispose()

        $result | Should -BeNullOrEmpty
        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
        $failures[0].Exception.PublicationProblem | Should -Be 'DestinationExists'
        $failures[0].Exception.DownloadPlaced | Should -BeFalse
        $failures[0].Exception.DownloadRetained | Should -BeFalse
        Get-Content -LiteralPath $destination | Should -Be 'APPEARED'
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('q3.csv')
    }

    It 'writes each piped file it can, and reports the one whose name is taken without stopping the others' {
        foreach ($name in 'a.csv', 'b.csv', 'c.csv') {
            $service.ServeDownload("reports/$name", $content)
            $service.Respond('GET', "/file_actions/metadata/reports/$name", 200, "{""path"":""reports/$name"",""type"":""file""}")
        }
        $files = 'a.csv', 'b.csv', 'c.csv' | ForEach-Object { Get-FilesFile -Path "reports/$_" -Connection $connection }
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'several')
        Set-Content -LiteralPath (Join-Path $folder.FullName 'b.csv') -Value 'TAKEN'

        $results = $files | Receive-FilesFile -LiteralPath $folder.FullName -PassThru -Confirm:$false -ErrorVariable failures -ErrorAction SilentlyContinue

        @($results).RemotePath | Should -Be @('reports/a.csv', 'reports/c.csv')
        $failures.Count | Should -Be 1
        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
        $failures[0].TargetObject | Should -Be 'reports/b.csv'
        Get-Content -LiteralPath (Join-Path $folder.FullName 'b.csv') | Should -Be 'TAKEN'
        foreach ($name in 'a.csv', 'c.csv') {
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes((Join-Path $folder.FullName $name))) | Should -BeTrue
        }
    }

    It 'writes into an existing folder under the remote file''s name, taking the file from the pipeline' {
        $service.Respond('GET', '/file_actions/metadata/reports/q3.csv', 200, '{"path":"reports/q3.csv","type":"file"}')
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'downloads')

        Get-FilesFile -Path 'reports/q3.csv' -Connection $connection | Receive-FilesFile -LiteralPath $folder.FullName -Confirm:$false

        (Get-Item (Join-Path $folder.FullName 'q3.csv')).Length | Should -Be 50000
    }

    It 'leaves no local file and no partial file when the download fails, and keeps the signed URL out of the error' {
        $service.FailDownload('reports/q3.csv', 403)
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'failed')
        $destination = Join-Path $folder.FullName 'failed.csv'

        { Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath $destination -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw

        @(Get-ChildItem -LiteralPath $folder.FullName -Force) | Should -BeNullOrEmpty
        ($Error[0] | Format-List * -Force | Out-String) + ($Error[0].Exception | Format-List * -Force | Out-String) | Should -Not -Match 'sentinel-signed-url-secret'
    }

    It 'writes into a borrowed stream and leaves it open' {
        $buffer = [System.IO.MemoryStream]::new()

        Receive-FilesFile -RemotePath 'reports/q3.csv' -OutputStream $buffer -Connection $connection -Confirm:$false

        $buffer.CanWrite | Should -BeTrue
        Test-SameContent -Expected $content -Actual ($buffer.ToArray()) | Should -BeTrue
    }

    It 'writes nothing under -WhatIf, not even a checkpoint' {
        $checkpoint = Join-Path $TestDrive 'whatif.checkpoint'

        Receive-FilesFile -RemotePath 'reports/q3.csv' -LiteralPath (Join-Path $TestDrive 'whatif.csv') -CheckpointPath $checkpoint -Connection $connection -WhatIf

        $service.Requests.Count | Should -Be 0
        Test-Path -LiteralPath (Join-Path $TestDrive 'whatif.csv') | Should -BeFalse
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
    }
}

Describe 'Receive-FilesFile -CheckpointPath' {
    BeforeAll {
        # Starts downloading reports/v.bin with a checkpoint in another runspace, as a user's pipeline would run it: four
        # parts, fetched one at a time so that which of them a run wrote is certain.
        function Start-CheckpointedDownload {
            param($Connection, [string] $Destination, [string] $Checkpoint)
            Start-Command -Connection $Connection -Arguments $Destination, $Checkpoint -Script {
                param($Connection, $Destination, $Checkpoint)
                Receive-FilesFile -RemotePath 'reports/v.bin' -LiteralPath $Destination -CheckpointPath $Checkpoint -PartSize 16384 -MaxConcurrency 1 -Connection $Connection -PassThru -Confirm:$false
            }
        }

        # Each request for the file's bytes, as its Range and the If-Match it asked for, in the order they arrived.
        function Get-StorageAsk {
            param([FilesCom.PowerShell.Tests.FilesTestService] $Service)
            @($Service.Requests | Where-Object { $_.Path -like '/download/*' } | ForEach-Object { "$($_.Header('Range')) If-Match=$($_.Header('If-Match'))" })
        }
    }
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        # The same site in memory, where reports/v.bin comes from Amazon S3, bound to a version.
        $inMemory = Connect-TestService -Service $service -InMemory
        $content = New-RandomContent 50000
        $service.ServeVersionedDownload('reports/v.bin', $content, 'v1')
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive "durable-$([guid]::NewGuid().ToString('n'))")
        $destination = Join-Path $folder.FullName 'v.bin'
        $checkpoints = New-Item -ItemType Directory -Path (Join-Path $TestDrive "checkpoints-$([guid]::NewGuid().ToString('n'))")
        $checkpoint = Join-Path $checkpoints.FullName 'v.checkpoint'
        $download = @{ RemotePath = 'reports/v.bin'; LiteralPath = $destination; CheckpointPath = $checkpoint; PartSize = 16384; MaxConcurrency = 1; Connection = $inMemory; Confirm = $false }
        # The same content backwards: another version of the file.
        $changed = [byte[]] $content.Clone()
        [System.Array]::Reverse($changed)
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $inMemory -Confirm:$false
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'continues a version-bound download that failed, fetching only the parts it had not written once storage confirms the version, and removes the checkpoint once complete' {
        $service.FailRange('reports/v.bin', 32768, 503)

        $failure = try { Receive-FilesFile @download -ErrorAction Stop; $null } catch { $_ }
        $firstRun = @($service.RangesServed('reports/v.bin'))
        $firstAsks = Get-StorageAsk $service
        $kept = Test-Path -LiteralPath $checkpoint
        $published = Test-Path -LiteralPath $destination
        $service.ClearRequests()
        $result = Receive-FilesFile @download -PassThru

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.*'
        $firstRun | Should -Be @('0-16383', '16384-32767')
        $firstAsks | Should -Be @('bytes=0-16383 If-Match=', 'bytes=16384-32767 If-Match="v1"', 'bytes=32768-49151 If-Match="v1"')
        $kept | Should -BeTrue
        $published | Should -BeFalse
        # Storage confirms the version with its first byte, asked for as that version, before the parts not yet written.
        @($service.RangesServed('reports/v.bin') | Select-Object -Skip $firstRun.Count) | Should -Be @('0-0', '32768-49151', '49152-49999')
        Get-StorageAsk $service | Should -Be @('bytes=0-0 If-Match="v1"', 'bytes=32768-49151 If-Match="v1"', 'bytes=49152-49999 If-Match="v1"')
        $result.ResumedBytes | Should -Be 32768
        # The parts not yet written, and the byte that confirmed the version.
        $result.ReceivedBytes | Should -Be 17233
        $result.VersionBound | Should -BeTrue
        $result.CompletedEarlier | Should -BeFalse
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('v.bin')
    }

    It 'pauses when the pipeline stops, keeping the parts it wrote with the checkpoint, and continues when run again' {
        $service.HoldRange('reports/v.bin', 32768)
        $running = Start-CheckpointedDownload -Connection $inMemory -Destination $destination -Checkpoint $checkpoint
        Wait-Until { $service.IsRangeHeld('reports/v.bin', 32768) }

        $elapsed = Measure-Command { $running.Shell.Stop() }
        $state = $running.Shell.InvocationStateInfo.State
        $running.Shell.Dispose()
        $kept = Test-Path -LiteralPath $checkpoint
        $published = Test-Path -LiteralPath $destination
        $service.ReleaseRange('reports/v.bin', 32768)
        Wait-Until { $service.ActiveRequests -eq 0 }
        $before = @($service.RangesServed('reports/v.bin')).Count
        $result = Receive-FilesFile @download -PassThru

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $state | Should -Be 'Stopped'
        $kept | Should -BeTrue
        $published | Should -BeFalse
        # Storage confirms the version with its first byte before the parts not yet written.
        @($service.RangesServed('reports/v.bin') | Select-Object -Skip $before) | Should -Be @('0-0', '32768-49151', '49152-49999')
        $result.ResumedBytes | Should -Be 32768
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
    }

    It 'starts over from the first byte, as the file is now, when its version changed since the run that wrote the checkpoint' {
        $service.FailRange('reports/v.bin', 32768, 503)
        $null = try { Receive-FilesFile @download -ErrorAction Stop } catch { $_ }
        $kept = Test-Path -LiteralPath $checkpoint
        $service.ChangeVersion('reports/v.bin', $changed, 'v2')
        $service.ClearRequests()
        $before = @($service.RangesServed('reports/v.bin')).Count

        $result = Receive-FilesFile @download -PassThru

        $kept | Should -BeTrue
        # The run asked storage whether the version the checkpoint records is still the file's, with its first byte asked
        # for as that version; storage no longer had it, so the download started over, bound to the version it has now.
        Get-StorageAsk $service | Should -Be @('bytes=0-0 If-Match="v1"', 'bytes=0-16383 If-Match=', 'bytes=16384-32767 If-Match="v2"', 'bytes=32768-49151 If-Match="v2"', 'bytes=49152-49999 If-Match="v2"')
        $result.ResumedBytes | Should -Be 0
        @($service.RangesServed('reports/v.bin') | Select-Object -Skip $before) | Should -Be @('0-16383', '16384-32767', '32768-49151', '49152-49999')
        Test-SameContent -Expected $changed -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('v.bin')
    }

    It 'fails with Files.SourceChanged and puts nothing in place when the version changes during the download, and gets the new version when run again' {
        $service.HoldRange('reports/v.bin', 32768)
        $running = Start-CheckpointedDownload -Connection $inMemory -Destination $destination -Checkpoint $checkpoint
        Wait-Until { $service.IsRangeHeld('reports/v.bin', 32768) }
        $service.ChangeVersion('reports/v.bin', $changed, 'v2')
        $service.ReleaseRange('reports/v.bin', 32768)
        $running.Shell.EndInvoke($running.Handle) | Out-Null
        $failures = @($running.Shell.Streams.Error)
        $running.Shell.Dispose()
        $published = Test-Path -LiteralPath $destination
        $kept = Test-Path -LiteralPath $checkpoint
        $service.ClearRequests()

        $result = Receive-FilesFile @download -PassThru

        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.SourceChanged*'
        $failures[0].CategoryInfo.Category | Should -Be 'ResourceUnavailable'
        $published | Should -BeFalse
        $kept | Should -BeTrue
        # The next run asked storage for the first byte of the version the checkpoint records, which it no longer had.
        (Get-StorageAsk $service)[0] | Should -Be 'bytes=0-0 If-Match="v1"'
        $result.ResumedBytes | Should -Be 0
        Test-SameContent -Expected $changed -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
    }

    It 'refuses a checkpoint whose stage no longer holds what it records before fetching anything, and starts over with -CheckpointRestart StartOver' {
        $service.FailRange('reports/v.bin', 32768, 503)
        $null = try { Receive-FilesFile @download -ErrorAction Stop } catch { $_ }
        # The stage is the one file in the engine's private folder beside the destination.
        $stage = Get-ChildItem -LiteralPath $folder.FullName -Force -Directory | Get-ChildItem -Force -File
        $truncating = [System.IO.File]::Open($stage.FullName, 'Open', 'ReadWrite')
        try {
            $truncating.SetLength(1000)
        }
        finally {
            $truncating.Dispose()
        }
        $service.ClearRequests()

        $refused = try { Receive-FilesFile @download -ErrorAction Stop; $null } catch { $_ }
        $requests = $service.Requests.Count
        $kept = Test-Path -LiteralPath $checkpoint
        $before = @($service.RangesServed('reports/v.bin')).Count
        $result = Receive-FilesFile @download -CheckpointRestart StartOver -PassThru

        $refused.FullyQualifiedErrorId | Should -BeLike 'Files.CheckpointRefused*'
        $refused.CategoryInfo.Category | Should -Be 'InvalidData'
        $refused.Exception.CheckpointProblem | Should -Be 'Unreadable'
        $refused.Exception.CheckpointPath | Should -Be $checkpoint
        $refused.ErrorDetails.RecommendedAction | Should -Match 'StartOver'
        $requests | Should -Be 0
        $kept | Should -BeTrue
        $result.ResumedBytes | Should -Be 0
        @($service.RangesServed('reports/v.bin') | Select-Object -Skip $before) | Should -Be @('0-16383', '16384-32767', '32768-49151', '49152-49999')
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('v.bin')
    }

    It 'never discards a file that is not a checkpoint, even with -CheckpointRestart StartOver' {
        Set-Content -LiteralPath $checkpoint -Value 'NOT A CHECKPOINT'

        $refusals = foreach ($restart in 'Fail', 'StartOver') {
            try { Receive-FilesFile @download -CheckpointRestart $restart -ErrorAction Stop; $null } catch { $_ }
        }

        @($refusals).Count | Should -Be 2
        foreach ($refused in $refusals) {
            $refused.FullyQualifiedErrorId | Should -BeLike 'Files.CheckpointRefused*'
            $refused.Exception.CheckpointProblem | Should -Be 'Unreadable'
        }
        Get-Content -LiteralPath $checkpoint | Should -Be 'NOT A CHECKPOINT'
        Test-Path -LiteralPath $destination | Should -BeFalse
        $service.Requests.Count | Should -Be 0
    }

    It 'keeps what it downloaded with the checkpoint when a file appears at the name during the download, and puts it in place with -Force without fetching it again' {
        $service.HoldRange('reports/v.bin', 32768)
        $running = Start-CheckpointedDownload -Connection $inMemory -Destination $destination -Checkpoint $checkpoint
        Wait-Until { $service.IsRangeHeld('reports/v.bin', 32768) }
        Set-Content -LiteralPath $destination -Value 'APPEARED'
        $service.ReleaseRange('reports/v.bin', 32768)
        $running.Shell.EndInvoke($running.Handle) | Out-Null
        $failures = @($running.Shell.Streams.Error)
        $running.Shell.Dispose()
        $appeared = Get-Content -LiteralPath $destination
        $before = @($service.RangesServed('reports/v.bin')).Count

        $result = Receive-FilesFile @download -Force -PassThru

        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
        $failures[0].Exception.PublicationProblem | Should -Be 'DestinationExists'
        $failures[0].Exception.DownloadPlaced | Should -BeFalse
        $failures[0].Exception.DownloadRetained | Should -BeTrue
        $failures[0].Exception.CheckpointPath | Should -Be $checkpoint
        $failures[0].ErrorDetails.RecommendedAction | Should -Match 'kept with the checkpoint'
        $appeared | Should -Be 'APPEARED'
        # Only the first byte, which confirmed the kept version.
        @($service.RangesServed('reports/v.bin') | Select-Object -Skip $before) | Should -Be @('0-0')
        $result.ResumedBytes | Should -Be 50000
        $result.Parts | Should -Be 0
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('v.bin')
    }

    # On Windows the stage and the destination folder are held open, so neither can be moved during the download.
    if (-not $IsWindows) {
        It 'refuses what another process puts at the stage''s name even with -Force, leaving the file it would replace as it was, and downloads anew when run again' {
            Set-Content -LiteralPath $destination -Value 'EXISTING'
            $service.HoldRange('reports/v.bin', 32768)
            $running = Start-Command -Connection $inMemory -Arguments $destination, $checkpoint -Script {
                param($Connection, $Destination, $Checkpoint)
                Receive-FilesFile -RemotePath 'reports/v.bin' -LiteralPath $Destination -CheckpointPath $Checkpoint -Force -PartSize 16384 -MaxConcurrency 1 -Connection $Connection -PassThru -Confirm:$false
            }
            Wait-Until { $service.IsRangeHeld('reports/v.bin', 32768) -and @(Get-ChildItem -LiteralPath $folder.FullName -Force -Directory | Get-ChildItem -Force -File).Count -eq 1 }
            $stage = Get-ChildItem -LiteralPath $folder.FullName -Force -Directory | Get-ChildItem -Force -File
            Move-Item -LiteralPath $stage.FullName -Destination "$($stage.FullName).original"
            Set-Content -LiteralPath $stage.FullName -Value 'SUBSTITUTED-CONTENT'
            $service.ReleaseRange('reports/v.bin', 32768)
            $output = @($running.Shell.EndInvoke($running.Handle))
            $failures = @($running.Shell.Streams.Error)
            $running.Shell.Dispose()
            $existing = Get-Content -LiteralPath $destination
            $before = @($service.RangesServed('reports/v.bin')).Count

            $result = Receive-FilesFile @download -Force -PassThru

            $output | Should -BeNullOrEmpty
            $failures.Count | Should -Be 1
            $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
            $failures[0].Exception.PublicationProblem | Should -Be 'StageReplaced'
            $failures[0].Exception.DownloadPlaced | Should -BeFalse
            # The checkpoint recorded the stage that was taken away, so nothing is kept to continue.
            $failures[0].Exception.DownloadRetained | Should -BeFalse
            $failures[0].Exception.CheckpointPath | Should -Be $checkpoint
            $failures[0].ErrorDetails.RecommendedAction | Should -Not -Match 'kept with the checkpoint'
            $existing | Should -Be 'EXISTING'
            # The run after it fetches every part again, and puts the exact bytes in place.
            @($service.RangesServed('reports/v.bin') | Select-Object -Skip $before) | Should -Be @('0-16383', '16384-32767', '32768-49151', '49152-49999')
            $result.ResumedBytes | Should -Be 0
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
            Test-Path -LiteralPath $checkpoint | Should -BeFalse
        }

        It 'keeps what it downloaded in the folder it opened when the destination folder is replaced, and puts it in place without fetching it again once that folder is back' {
            $service.HoldRange('reports/v.bin', 32768)
            $running = Start-CheckpointedDownload -Connection $inMemory -Destination $destination -Checkpoint $checkpoint
            Wait-Until { $service.IsRangeHeld('reports/v.bin', 32768) }
            $opened = "$($folder.FullName)-opened"
            Rename-Item -LiteralPath $folder.FullName -NewName (Split-Path -Leaf $opened)
            New-Item -ItemType Directory -Path $folder.FullName | Out-Null
            $service.ReleaseRange('reports/v.bin', 32768)
            $output = @($running.Shell.EndInvoke($running.Handle))
            $failures = @($running.Shell.Streams.Error)
            $running.Shell.Dispose()
            $inReplacement = @(Get-ChildItem -LiteralPath $folder.FullName -Force)
            $kept = @(Get-ChildItem -LiteralPath $opened -Force -Directory)
            $keptMode = @($kept | ForEach-Object { [System.IO.File]::GetUnixFileMode($_.FullName) })
            Remove-Item -LiteralPath $folder.FullName
            Rename-Item -LiteralPath $opened -NewName (Split-Path -Leaf $folder.FullName)
            $before = @($service.RangesServed('reports/v.bin')).Count

            $result = Receive-FilesFile @download -PassThru

            $output | Should -BeNullOrEmpty
            $failures.Count | Should -Be 1
            $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
            $failures[0].CategoryInfo.Category | Should -Be 'ResourceUnavailable'
            $failures[0].Exception.PublicationProblem | Should -Be 'DestinationMoved'
            $failures[0].Exception.DownloadPlaced | Should -BeFalse
            $failures[0].Exception.DownloadRetained | Should -BeTrue
            $failures[0].Exception.CheckpointPath | Should -Be $checkpoint
            $failures[0].ErrorDetails.RecommendedAction | Should -Match 'once that folder is at the path again'
            $inReplacement | Should -BeNullOrEmpty
            # The stage waits, private, in the folder the download opened.
            $kept.Count | Should -Be 1
            $keptMode | Should -Be @([System.IO.UnixFileMode]'UserRead, UserWrite, UserExecute')
            # Only the first byte, which confirmed the kept version.
            @($service.RangesServed('reports/v.bin') | Select-Object -Skip $before) | Should -Be @('0-0')
            $result.ResumedBytes | Should -Be 50000
            $result.Parts | Should -Be 0
            Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
            Test-Path -LiteralPath $checkpoint | Should -BeFalse
            (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('v.bin')
        }
    }

    It 'completes a download whose file an earlier run already put in place, without -Force and without fetching it again' {
        $service.HoldRange('reports/v.bin', 32768)
        $running = Start-CheckpointedDownload -Connection $inMemory -Destination $destination -Checkpoint $checkpoint
        Wait-Until { $service.IsRangeHeld('reports/v.bin', 32768) }
        Set-Content -LiteralPath $destination -Value 'APPEARED'
        $service.ReleaseRange('reports/v.bin', 32768)
        $running.Shell.EndInvoke($running.Handle) | Out-Null
        $running.Shell.Dispose()
        # What a run that put the file in place but could not record it leaves: a checkpoint of the complete stage, and
        # that very file at its name.
        Remove-Item -LiteralPath $destination
        Get-ChildItem -LiteralPath $folder.FullName -Force -Directory | Get-ChildItem -Force -File | Move-Item -Destination $destination
        $service.ClearRequests()

        $result = Receive-FilesFile @download -PassThru

        $result.CompletedEarlier | Should -BeTrue
        $result.Size | Should -Be 50000
        $service.Requests.Count | Should -Be 0
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('v.bin')
    }

    It 'keeps a paused download when a file has since appeared at the name, refusing it without fetching anything, and continues it with -Force' {
        $service.FailRange('reports/v.bin', 32768, 503)
        $null = try { Receive-FilesFile @download -ErrorAction Stop } catch { $_ }
        Set-Content -LiteralPath $destination -Value 'APPEARED'
        $service.ClearRequests()

        $refused = try { Receive-FilesFile @download -ErrorAction Stop; $null } catch { $_ }
        $requests = $service.Requests.Count
        $appeared = Get-Content -LiteralPath $destination
        $before = @($service.RangesServed('reports/v.bin')).Count
        $result = Receive-FilesFile @download -Force -PassThru

        $refused.Exception.PublicationProblem | Should -Be 'DestinationExists'
        $refused.Exception.DownloadRetained | Should -BeTrue
        $requests | Should -Be 0
        $appeared | Should -Be 'APPEARED'
        $result.ResumedBytes | Should -Be 32768
        # Storage confirms the version with its first byte before the parts not yet written.
        @($service.RangesServed('reports/v.bin') | Select-Object -Skip $before) | Should -Be @('0-0', '32768-49151', '49152-49999')
        Test-SameContent -Expected $content -Actual ([System.IO.File]::ReadAllBytes($destination)) | Should -BeTrue
    }

    It 'uses -CheckpointPath for the first piped file only, and refuses the next before downloading it, saying the first may have completed' {
        $service.ServeDownload('reports/q3.csv', $content)
        $service.Respond('GET', '/file_actions/metadata/reports/v.bin', 200, '{"path":"reports/v.bin","type":"file"}')
        $service.Respond('GET', '/file_actions/metadata/reports/q3.csv', 200, '{"path":"reports/q3.csv","type":"file"}')
        $files = 'reports/v.bin', 'reports/q3.csv' | ForEach-Object { Get-FilesFile -Path $_ -Connection $connection }

        $failure = try { $files | Receive-FilesFile -LiteralPath $folder.FullName -CheckpointPath $checkpoint -PassThru -Confirm:$false -OutVariable results -ErrorAction Stop | Out-Null; $null } catch { $_ }

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        $failure.Exception.Message | Should -Match "'reports/q3.csv' and any later file were not downloaded"
        @($results).LocalPath | Should -Be @($destination)
        (Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('v.bin')
        Test-Path -LiteralPath $checkpoint | Should -BeFalse
    }

    It 'refuses checkpoint settings it cannot use before sending anything: with -OutputStream, -CheckpointRestart alone, or a folder or another provider''s path' {
        $stream = [System.IO.MemoryStream]::new()

        { Receive-FilesFile -RemotePath 'reports/v.bin' -OutputStream $stream -CheckpointPath $checkpoint -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'AmbiguousParameterSet*'
        { Receive-FilesFile -RemotePath 'reports/v.bin' -OutputStream $stream -CheckpointRestart StartOver -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'AmbiguousParameterSet*'
        { Receive-FilesFile -RemotePath 'reports/v.bin' -LiteralPath $destination -CheckpointRestart StartOver -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { Receive-FilesFile -RemotePath 'reports/v.bin' -LiteralPath $destination -CheckpointPath $checkpoints.FullName -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { Receive-FilesFile -RemotePath 'reports/v.bin' -LiteralPath $destination -CheckpointPath 'Env:PATH' -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'

        $service.Requests.Count | Should -Be 0
        $stream.Length | Should -Be 0
        Test-Path -LiteralPath $destination | Should -BeFalse
    }
}

Describe 'Copying and moving to external destinations' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $service.Respond('POST', '/file_actions/copy/reports/q3.csv', 200, '{"status":"enqueued","file_migration_id":11}')
        $service.Respond('POST', '/file_actions/move/reports/q3.csv', 200, '{"status":"enqueued","file_migration_id":12}')
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'sends a copy to a <Label> as the same copy operation, to its path under that destination' -TestCases @(
        @{ Label = 'remote server'; Parameter = 'RemoteServerId'; Root = '_/RemoteServers' }
        @{ Label = 'snapshot'; Parameter = 'SnapshotId'; Root = '_/Snapshots' }
        @{ Label = 'child site'; Parameter = 'ChildSiteId'; Root = '_/Sites' }
    ) {
        $arguments = @{ Path = 'reports/q3.csv'; $Parameter = 5; DestinationPath = 'archive/q3.csv'; Connection = $connection; Confirm = $false }

        $action = Copy-FilesFile @arguments

        $action.FileMigrationId | Should -Be 11
        $body = (Get-ApiRequest $service -Method POST -Path '/file_actions/copy/reports/q3.csv').Body | ConvertFrom-Json
        $body.destination | Should -BeExactly "$Root/5/archive/q3.csv"
    }

    It 'moves a file from the pipeline to a remote server with its connection' {
        $service.Respond('GET', '/file_actions/metadata/reports/q3.csv', 200, '{"path":"reports/q3.csv","type":"file"}')

        Get-FilesFile -Path 'reports/q3.csv' -Connection $connection | Move-FilesFile -RemoteServerId 8 -DestinationPath 'in' -Confirm:$false | Out-Null

        ((Get-ApiRequest $service -Method POST -Path '/file_actions/move/reports/q3.csv').Body | ConvertFrom-Json).destination | Should -BeExactly '_/RemoteServers/8/in'
    }

    It 'sends nothing under -WhatIf' {
        Copy-FilesFile -Path 'reports/q3.csv' -SnapshotId 3 -Connection $connection -WhatIf
        $service.Requests.Count | Should -Be 0
    }
}

Describe 'Out-FilesLocalFile' {
    It 'writes every line as one file, once all input has arrived' {
        $path = Join-Path $TestDrive 'lines.txt'

        $file = 'first', '', 'third' | Out-FilesLocalFile -LiteralPath $path -PassThru

        Get-Content -LiteralPath $path | Should -Be @('first', '', 'third')
        $file.FullName | Should -Be $path
        @(Get-ChildItem -LiteralPath $TestDrive -Force -Filter '*.filescom-partial') | Should -BeNullOrEmpty
    }

    It 'leaves an existing file exactly as it was, and no stage, when a command before it fails' {
        $path = Join-Path $TestDrive 'report.csv'
        Set-Content -LiteralPath $path -Value 'PREVIOUS-REPORT'

        { & { 'new line'; throw 'a page could not be read' } | Out-FilesLocalFile -LiteralPath $path -Force } | Should -Throw '*could not be read*'

        Get-Content -LiteralPath $path | Should -Be 'PREVIOUS-REPORT'
        @(Get-ChildItem -LiteralPath $TestDrive -Force -Filter '*.filescom-partial') | Should -BeNullOrEmpty
    }

    It 'refuses to replace an existing file without -Force, before reading any input, and replaces it with -Force' {
        $path = Join-Path $TestDrive 'existing.txt'
        Set-Content -LiteralPath $path -Value 'KEEP'

        { 'new' | Out-FilesLocalFile -LiteralPath $path } | Should -Throw -ErrorId 'Files.LocalFile*'
        Get-Content -LiteralPath $path | Should -Be 'KEEP'

        'new' | Out-FilesLocalFile -LiteralPath $path -Force
        Get-Content -LiteralPath $path | Should -Be 'new'
    }

    It 'creates nothing under -WhatIf' {
        $path = Join-Path $TestDrive 'preview.txt'

        'line' | Out-FilesLocalFile -LiteralPath $path -WhatIf

        Test-Path -LiteralPath $path | Should -BeFalse
        @(Get-ChildItem -LiteralPath $TestDrive -Force -Filter '*.filescom-partial') | Should -BeNullOrEmpty
    }
}

Describe 'Send-FilesDirectory' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $tree = New-Item -ItemType Directory -Path (Join-Path $TestDrive "tree-$([guid]::NewGuid().ToString('n'))")
        $one = New-RandomContent 100
        $two = New-RandomContent 200
        New-Item -ItemType Directory -Path (Join-Path $tree.FullName 'sub') | Out-Null
        [System.IO.File]::WriteAllBytes((Join-Path $tree.FullName 'one.bin'), $one)
        [System.IO.File]::WriteAllBytes((Join-Path $tree.FullName 'sub/two.bin'), $two)
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'uploads every file under a folder to the folder of its name, leaves out links, and describes it with -PassThru' {
        New-Item -ItemType SymbolicLink -Path (Join-Path $tree.FullName 'link.bin') -Target (Join-Path $tree.FullName 'one.bin') | Out-Null
        $service.ServeUpload("$($tree.Name)/one.bin", 1048576)
        $service.ServeUpload("$($tree.Name)/sub/two.bin", 1048576)

        $result = Send-FilesDirectory -LiteralPath $tree.FullName -MaxConcurrentFiles 2 -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $one -Actual ($service.Uploaded("$($tree.Name)/one.bin")) | Should -BeTrue
        Test-SameContent -Expected $two -Actual ($service.Uploaded("$($tree.Name)/sub/two.bin")) | Should -BeTrue
        $result.RemoteDirectory | Should -Be $tree.Name
        $result.Bytes | Should -Be 300
        @($result.Files.RelativePath | Sort-Object) | Should -Be @('one.bin', 'sub/two.bin')
        $result.Excluded | Should -Be @('link.bin')
        @($service.Requests | Where-Object { $_.Path -like '*link.bin*' }).Count | Should -Be 0
    }

    It 'skips the files it completed when run again with -CheckpointDirectory, and uploads the rest' {
        $checkpoints = Join-Path $TestDrive "checkpoints-$($tree.Name)"
        $service.ServeUpload("$($tree.Name)/one.bin", 1048576)
        $service.ServeUpload("$($tree.Name)/sub/two.bin", 1048576)
        $service.FailPart("$($tree.Name)/sub/two.bin", 1, 400)

        { Send-FilesDirectory -LiteralPath $tree.FullName -MaxConcurrentFiles 1 -CheckpointDirectory $checkpoints -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw
        $service.ClearRequests()
        $result = Send-FilesDirectory -LiteralPath $tree.FullName -CheckpointDirectory $checkpoints -Connection $connection -PassThru -Confirm:$false

        Test-SameContent -Expected $two -Actual ($service.Uploaded("$($tree.Name)/sub/two.bin")) | Should -BeTrue
        ($result.Files | Where-Object RelativePath -EQ 'one.bin').CompletedEarlier | Should -BeTrue
        ($result.Files | Where-Object RelativePath -EQ 'sub/two.bin').CompletedEarlier | Should -BeFalse
        @(Get-ApiRequest $service -Method POST -Path "/file_actions/begin_upload/$($tree.Name)/one.bin").Count | Should -Be 0
    }

    It 'takes -CheckpointDirectory as a relative or PowerShell-drive file system path, even one not made yet, and refuses another provider''s' {
        $service.ServeUpload("$($tree.Name)/one.bin", 1048576)
        $service.ServeUpload("$($tree.Name)/sub/two.bin", 1048576)
        $work = New-Item -ItemType Directory -Path (Join-Path $TestDrive "work-$($tree.Name)")
        New-PSDrive -Name FilesCkpt -PSProvider FileSystem -Root $work.FullName | Out-Null
        Push-Location -LiteralPath $work.FullName
        try {
            # A path of another provider: Env: resolves to a bare name, which would otherwise be read relative to the process folder.
            $refusal = try { Send-FilesDirectory -LiteralPath $tree.FullName -CheckpointDirectory 'Env:FILESCOM_R1_CHECKPOINT' -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
            $refusal.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
            $service.Requests.Count | Should -Be 0
            @(Get-ChildItem -LiteralPath $work.FullName -Force).Count | Should -Be 0
            @(Get-ChildItem -LiteralPath ([System.IO.Directory]::GetCurrentDirectory()) -Filter 'FILESCOM_R1_CHECKPOINT' -Force).Count | Should -Be 0

            $relative = Send-FilesDirectory -LiteralPath $tree.FullName -CheckpointDirectory 'relative-checkpoints' -Connection $connection -PassThru -Confirm:$false
            $drive = Send-FilesDirectory -LiteralPath $tree.FullName -CheckpointDirectory 'FilesCkpt:\drive-checkpoints' -Connection $connection -PassThru -Confirm:$false
        }
        finally {
            Pop-Location
            Remove-PSDrive -Name FilesCkpt
        }

        $relative.Bytes | Should -Be 300
        $drive.Bytes | Should -Be 300
        Test-SameContent -Expected $two -Actual ($service.Uploaded("$($tree.Name)/sub/two.bin")) | Should -BeTrue
    }

    It 'sends nothing under -WhatIf or for a missing folder' {
        Send-FilesDirectory -LiteralPath $tree.FullName -RemotePath 'x' -Connection $connection -WhatIf

        { Send-FilesDirectory -LiteralPath (Join-Path $TestDrive 'missing') -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        $service.Requests.Count | Should -Be 0
    }

    It 'starts the files whose parts go straight to Amazon S3 at the scaled policy''s start only with -AggressiveStartup' {
        # By default, as with -AggressiveStartup:$false, the files' shared target warm-starts at 1 part at a time: with each
        # part held for 500 ms, it cannot raise until three parts have been answered, more than these files leave to send in
        # parallel. -AggressiveStartup starts the shared target at 150, so both files' parts are in flight at once.
        $inMemory = Connect-TestService -Service $service -InMemory
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive "fast-$([guid]::NewGuid().ToString('n'))")
        $contents = @{}
        foreach ($name in 'a.bin', 'b.bin') {
            $contents[$name] = New-RandomContent 6MB
            [System.IO.File]::WriteAllBytes((Join-Path $folder.FullName $name), $contents[$name])
            foreach ($remote in 'gradual', 'fast') {
                $service.ServeUpload("$remote/$name", 16384)
                $service.SendPartsToS3("$remote/$name")
                $service.DelayParts("$remote/$name", 500)
            }
        }
        try {
            Send-FilesDirectory -LiteralPath $folder.FullName -RemotePath 'gradual' -AggressiveStartup:$false -Connection $inMemory -Confirm:$false
            $gradualPeak = $service.PeakPartsOfAllUploads
            Send-FilesDirectory -LiteralPath $folder.FullName -RemotePath 'fast' -AggressiveStartup -Connection $inMemory -Confirm:$false
        }
        finally {
            Disconnect-FilesAccount -Connection $inMemory -Confirm:$false
        }

        $gradualPeak | Should -Be 1
        $service.PeakPartsOfAllUploads | Should -BeGreaterThan 1
        foreach ($remote in 'gradual', 'fast') {
            foreach ($name in 'a.bin', 'b.bin') {
                Test-SameContent -Expected $contents[$name] -Actual ($service.Uploaded("$remote/$name")) | Should -BeTrue
            }
        }
    }
}

Describe 'Remote files used as local ones' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        # The same site in memory, where versioned downloads come from Amazon S3.
        $inMemory = Connect-TestService -Service $service -InMemory
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $inMemory -Confirm:$false
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'tells whether Files.com has a file or folder at a path, with -PathType' {
        $service.Respond('GET', '/file_actions/metadata/a.txt', 200, '{"path":"a.txt","type":"file"}')
        $service.Respond('GET', '/file_actions/metadata/docs', 200, '{"path":"docs","type":"directory"}')

        Test-FilesPath -Path 'a.txt' -Connection $connection | Should -BeTrue
        Test-FilesPath -Path 'a.txt' -PathType Folder -Connection $connection | Should -BeFalse
        Test-FilesPath -Path 'docs' -PathType Folder -Connection $connection | Should -BeTrue
        Test-FilesPath -Path 'docs' -PathType File -Connection $connection | Should -BeFalse
        Test-FilesPath -Path 'missing.txt' -Connection $connection | Should -BeFalse
    }

    It 'reads a file as text without its byte order mark, or as bytes with -AsByteStream' {
        $bytes = [System.Text.Encoding]::UTF8.GetPreamble() + [System.Text.Encoding]::UTF8.GetBytes("h$([char]0xE9)llo")
        $service.ServeDownload('notes.txt', [byte[]] $bytes)

        Get-FilesFileContent -Path 'notes.txt' -Connection $connection | Should -BeExactly "h$([char]0xE9)llo"
        $read = Get-FilesFileContent -Path 'notes.txt' -AsByteStream -Connection $connection
        $read.GetType() | Should -Be ([byte[]])
        Test-SameContent -Expected $bytes -Actual $read | Should -BeTrue
    }

    It 'writes text as UTF-8, or bytes, as the whole file, and nothing under -WhatIf' {
        $service.ServeUpload('out/notes.txt', 1048576)
        $service.ServeUpload('out/data.bin', 1048576)

        Set-FilesFileContent -Path 'out/notes.txt' -Value "h$([char]0xE9)llo" -MkdirParents -Connection $connection -Confirm:$false
        Set-FilesFileContent -Path 'out/data.bin' -Value ([byte[]](1, 2, 3)) -Connection $connection -Confirm:$false

        [System.Text.Encoding]::UTF8.GetString($service.Uploaded('out/notes.txt')) | Should -BeExactly "h$([char]0xE9)llo"
        $service.Uploaded('out/data.bin') | Should -Be @(1, 2, 3)
        (Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/out/notes.txt')[0].Body | Should -Match '"mkdir_parents":true'
        $service.ClearRequests()
        Set-FilesFileContent -Path 'out/notes.txt' -Value 'changed' -Connection $connection -WhatIf
        $service.Requests.Count | Should -Be 0
    }

    It 'writes all the strings or bytes piped to it as one file, with one upload, and each array''s bytes as they were when piped' {
        $service.ServeUpload('lines.txt', 1048576)
        $service.ServeUpload('parts.bin', 1048576)
        $newline = [System.Environment]::NewLine

        'line one', 'line two', 'line three' | Set-FilesFileContent -Path 'lines.txt' -Connection $connection -Confirm:$false

        [System.Text.Encoding]::UTF8.GetString($service.Uploaded('lines.txt')) | Should -BeExactly "line one${newline}line two${newline}line three"
        @(Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/lines.txt').Count | Should -Be 1

        # Two byte arrays, then an array the pipeline sends one byte at a time.
        & { , [byte[]](1, 2); , [byte[]](3); [byte[]](4, 5) } | Set-FilesFileContent -Path 'parts.bin' -Connection $connection -Confirm:$false

        $service.Uploaded('parts.bin') | Should -Be @(1, 2, 3, 4, 5)
        @(Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/parts.bin').Count | Should -Be 1

        # A producer may change an array after piping it, or reuse it for the next value.
        $service.ServeUpload('reused.bin', 1048576)
        $service.ServeUpload('changed.bin', 1048576)
        & { $buffer = [byte[]](1, 2); , $buffer; $buffer[0] = 3; , $buffer } | Set-FilesFileContent -Path 'reused.bin' -Connection $connection -Confirm:$false
        & { $buffer = [byte[]](1, 2); , $buffer; $buffer[0] = 3 } | Set-FilesFileContent -Path 'changed.bin' -Connection $connection -Confirm:$false

        $service.Uploaded('reused.bin') | Should -Be @(1, 2, 3, 2)
        $service.Uploaded('changed.bin') | Should -Be @(1, 2)
        foreach ($path in 'reused.bin', 'changed.bin') {
            @(Get-ApiRequest $service -Method POST -Path "/file_actions/begin_upload/$path").Count | Should -Be 1
        }
    }

    It 'writes several strings given to -Value as the lines piping them writes' {
        $service.ServeUpload('given.txt', 1048576)

        Set-FilesFileContent -Path 'given.txt' -Value 'line one', 'line two' -Connection $connection -Confirm:$false

        [System.Text.Encoding]::UTF8.GetString($service.Uploaded('given.txt')) | Should -BeExactly "line one$([System.Environment]::NewLine)line two"
    }

    It 'sends nothing when a piped value is refused, or when a terminating error or a stop ends the pipeline early' {
        $service.ServeUpload('kept.txt', 1048576)
        $write = @{ Path = 'kept.txt'; Connection = $connection; Confirm = $false }

        $mixed = try { 'text', [byte[]](1) | Set-FilesFileContent @write } catch { $_ }
        $number = try { 'text', 42 | Set-FilesFileContent @write } catch { $_ }
        $nothing = try { 'text', $null | Set-FilesFileContent @write } catch { $_ }
        $failed = try { & { 'text'; throw 'the source failed' } | Set-FilesFileContent @write } catch { $_ }
        $marker = Join-Path $TestDrive 'first-value-given'
        $running = Start-Command -Connection $connection -Arguments $marker -Script {
            param($Connection, $Marker)
            & { param($Given) 'text'; New-Item -ItemType File -Path $Given | Out-Null; while ($true) { Start-Sleep -Milliseconds 20 } } $Marker | Set-FilesFileContent -Path 'kept.txt' -Connection $Connection -Confirm:$false
        }
        Wait-Until { Test-Path -LiteralPath $marker }
        $running.Shell.Stop()
        $running.Shell.Dispose()

        foreach ($refusal in $mixed, $number, $nothing) {
            $refusal.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
            $refusal.TargetObject | Should -Be 'kept.txt'
        }
        $failed.Exception.Message | Should -Be 'the source failed'
        $service.Requests.Count | Should -Be 0
    }

    It 'writes nothing when nothing is piped, an empty file for an empty value, and previews one upload of all the content with -WhatIf' {
        $service.ServeUpload('empty.txt', 1048576)

        @() | Set-FilesFileContent -Path 'empty.txt' -Connection $connection -Confirm:$false
        $sentForNothing = $service.Requests.Count
        '' | Set-FilesFileContent -Path 'empty.txt' -Connection $connection -Confirm:$false
        $emptyUploads = @(Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/empty.txt').Count
        $emptyCompletions = @(Get-ApiRequest $service -Method POST -Path '/files/empty.txt').Count
        $service.ClearRequests()
        $transcript = Join-Path $TestDrive 'set-content-what-if.txt'
        Start-Transcript -Path $transcript | Out-Null
        try {
            'line one', 'line two' | Set-FilesFileContent -Path 'preview.txt' -Connection $connection -WhatIf
        }
        finally {
            Stop-Transcript | Out-Null
        }

        $sentForNothing | Should -Be 0
        $emptyUploads | Should -Be 1
        $emptyCompletions | Should -Be 1
        $service.Uploaded('empty.txt').Length | Should -Be 0
        $total = [System.Text.Encoding]::UTF8.GetByteCount("line one$([System.Environment]::NewLine)line two")
        $previews = [regex]::Matches((Get-Content -LiteralPath $transcript -Raw), "What if: .*'preview\.txt'")
        $previews.Count | Should -Be 1
        $previews[0].Value | Should -Match "\b$total bytes\b"
        $service.Requests.Count | Should -Be 0
    }

    It 'opens a remote file to read as a forward-only stream that reads it after the command returns, in the command''s workspace, and stops its download when disposed early' {
        [byte[]] $content = New-RandomContent 5000
        $service.ServeVersionedDownload('streamed.bin', $content, 'v1')
        $service.ServeVersionedDownload('held.bin', $content, 'v1')
        $service.HoldRange('held.bin', 2000)
        $buffer = [byte[]]::new(700)

        $stream = Open-FilesFileReadStream -Path 'streamed.bin' -PartSize 1000 -MaxConcurrency 1 -WorkspaceScope 5 -Connection $inMemory
        $read = [System.IO.MemoryStream]::new()
        $first = $stream.Read($buffer, 0, 300)
        $read.Write($buffer, 0, $first)
        while (($count = $stream.ReadAsync($buffer, 0, $buffer.Length).GetAwaiter().GetResult()) -gt 0) {
            $read.Write($buffer, 0, $count)
        }
        $seek = try { $stream.Seek(0, 'Begin') } catch { $_.Exception.InnerException }
        $stream.Dispose()
        $afterDispose = try { $stream.Read($buffer, 0, 1) } catch { $_.Exception.InnerException }
        $early = Open-FilesFileReadStream -Path 'held.bin' -PartSize 1000 -MaxConcurrency 1 -Connection $inMemory
        $got = 0
        while ($got -lt 2000) { $got += $early.Read($buffer, 0, [Math]::Min($buffer.Length, 2000 - $got)) }
        Wait-Until { $service.IsRangeHeld('held.bin', 2000) }
        $elapsed = Measure-Command { $early.Dispose() }
        $service.ReleaseRange('held.bin', 2000)
        Wait-Until { $service.ActiveRequests -eq 0 }

        $stream.GetType().FullName | Should -Be 'FilesCom.PowerShell.FilesRemoteReadStream'
        $stream.GetType().Assembly.GetName().Name | Should -Be 'FilesCom.PowerShell.Contracts'
        $stream.Path | Should -Be 'streamed.bin'
        Test-SameContent -Expected $content -Actual $read.ToArray() | Should -BeTrue
        @($service.RangesServed('streamed.bin')) | Should -Be @('0-999', '1000-1999', '2000-2999', '3000-3999', '4000-4999')
        @($service.Requests | Where-Object { $_.Path -like '/api/*streamed.bin*' } | ForEach-Object { $_.Header('X-Files-Workspace-Id') } | Sort-Object -Unique) | Should -Be @('5')
        $seek | Should -BeOfType [System.NotSupportedException]
        $stream.CanRead | Should -BeFalse
        $afterDispose | Should -BeOfType [System.ObjectDisposedException]
        $elapsed.TotalSeconds | Should -BeLessThan 5
        # Stopped at the held part: nothing after it was asked for.
        @($service.RangesServed('held.bin') | Where-Object { $_ -in '3000-3999', '4000-4999' }).Count | Should -Be 0
    }

    It 'reports a download failure after opening as the reader reaches it, as a FilesException without the signed URL, reading synchronously or not, and a file it cannot open as the command''s error' {
        [byte[]] $content = New-RandomContent 5000
        $buffer = [byte[]]::new(700)
        $failures = foreach ($async in $false, $true) {
            $remote = "failing-$async.bin"
            $service.ServeVersionedDownload($remote, $content, 'v1')
            # The request for the bytes from 3000 on waits until it is released, and then fails: the stream is open and
            # its reader has read the 3000 bytes before that point by the time the download fails.
            $service.HoldRange($remote, 3000)
            $service.FailRange($remote, 3000, 403)
            $stream = $null
            try {
                $stream = Open-FilesFileReadStream -Path $remote -PartSize 1000 -MaxConcurrency 1 -Connection $inMemory
                $next = if ($async) { { param($Count) $stream.ReadAsync($buffer, 0, $Count).GetAwaiter().GetResult() } } else { { param($Count) $stream.Read($buffer, 0, $Count) } }
                $received = [System.IO.MemoryStream]::new()
                $total = 0
                while ($total -lt 3000 -and ($count = & $next ([Math]::Min($buffer.Length, 3000 - $total))) -gt 0) {
                    $received.Write($buffer, 0, $count)
                    $total += $count
                }
                Wait-Until { $service.IsRangeHeld($remote, 3000) }
                $service.ReleaseRange($remote, 3000)
                try {
                    while (($count = & $next $buffer.Length) -gt 0) {
                        $received.Write($buffer, 0, $count)
                        $total += $count
                    }
                    [pscustomobject]@{ Total = $total; Received = $received.ToArray(); Failure = $null }
                }
                catch {
                    [pscustomobject]@{ Total = $total; Received = $received.ToArray(); Failure = $_.Exception.InnerException }
                }
            }
            finally {
                # Answers the held request even when a step above failed, so that nothing is left waiting on it.
                $service.ReleaseRange($remote, 3000)
                if ($null -ne $stream) {
                    $stream.Dispose()
                }
            }
        }
        $opened = Open-FilesFileReadStream -Path 'missing.bin' -Connection $connection -ErrorVariable refused -ErrorAction SilentlyContinue

        foreach ($outcome in $failures) {
            $outcome.Total | Should -Be 3000
            Test-SameContent -Expected $content[0..2999] -Actual $outcome.Received | Should -BeTrue
            $outcome.Failure | Should -BeOfType [FilesCom.PowerShell.FilesException]
            $outcome.Failure.InnerException | Should -BeNullOrEmpty
            ($outcome.Failure | Format-List * -Force | Out-String) | Should -Not -Match 'sentinel-signed-url-secret'
        }
        $opened | Should -BeNullOrEmpty
        $refused[0].FullyQualifiedErrorId | Should -BeLike 'Files.NotFound*'
        $refused[0].TargetObject | Should -Be 'missing.bin'
    }

    It 'writes a remote file through a stream after the command returns, in the command''s workspace, and completes it once when the stream is closed' {
        [byte[]] $content = New-RandomContent 5000
        $service.ServeUpload('written.bin', 4096)

        $stream = Open-FilesFileWriteStream -Path 'written.bin' -WorkspaceScope 5 -Connection $connection -Confirm:$false
        $stream.Write($content, 0, 3000)
        $stream.WriteAsync($content, 3000, 2000).GetAwaiter().GetResult() | Out-Null
        $stream.Dispose()
        $afterDispose = try { $stream.Write($content, 0, 1) } catch { $_.Exception.InnerException }

        $stream.GetType().Assembly.GetName().Name | Should -Be 'FilesCom.PowerShell.Contracts'
        Test-SameContent -Expected $content -Actual $service.Uploaded('written.bin') | Should -BeTrue
        @(Get-ApiRequest $service -Method POST -Path '/files/written.bin').Count | Should -Be 1
        @($service.Requests | Where-Object { $_.Path -like '/api/*written.bin*' } | ForEach-Object { $_.Header('X-Files-Workspace-Id') } | Sort-Object -Unique) | Should -Be @('5')
        $stream.CanWrite | Should -BeFalse
        $afterDispose | Should -BeOfType [System.ObjectDisposedException]
    }

    It 'reports an upload failure once, from the write that finds it or from closing the stream, as a FilesException, keeps an uncertain completion uncertain, and completes no refused file' {
        [byte[]] $content = New-RandomContent 5000
        $service.ServeUpload('refused.bin', 4096)
        $service.FailPart('refused.bin', 1, 400)
        $service.ServeUpload('refused-early.bin', 4096)
        $service.FailPart('refused-early.bin', 1, 400)
        $service.ServeUpload('lost.bin', 4096, $true, 0)

        $refused = Open-FilesFileWriteStream -Path 'refused.bin' -Connection $connection -Confirm:$false
        $refused.Write($content, 0, 5000)
        $atClose = try { $refused.Dispose() } catch { $_.Exception.InnerException }
        $early = Open-FilesFileWriteStream -Path 'refused-early.bin' -Connection $connection -Confirm:$false
        $early.Write($content, 0, 5000)
        Wait-Until { @($service.Requests | Where-Object { $_.Method -eq 'PUT' -and $_.Path -eq '/upload/refused-early.bin/1' }).Count -eq 1 }
        $atWrite = [ref] $null
        Wait-Until { try { $early.Write($content, 0, 1); $false } catch { $atWrite.Value = $_.Exception.InnerException; $true } }
        $closedAfter = try { $early.Dispose(); $null } catch { $_.Exception.InnerException }
        $lost = Open-FilesFileWriteStream -Path 'lost.bin' -Connection $connection -Confirm:$false
        $lost.Write($content, 0, 5000)
        $uncertain = try { $lost.Dispose() } catch { $_.Exception.InnerException }

        foreach ($failure in $atClose, $atWrite.Value, $uncertain) {
            $failure | Should -BeOfType [FilesCom.PowerShell.FilesException]
            $failure.InnerException | Should -BeNullOrEmpty
            ($failure | Format-List * -Force | Out-String) | Should -Not -Match 'sentinel-signed-url-secret'
        }
        $closedAfter | Should -BeNullOrEmpty
        $early.CanWrite | Should -BeFalse
        $uncertain.Kind | Should -Be 'TransferCompletionUnknown'
        @(Get-ApiRequest $service -Method POST -Path '/files/refused.bin').Count | Should -Be 0
        @(Get-ApiRequest $service -Method POST -Path '/files/refused-early.bin').Count | Should -Be 0
    }

    It 'abandons an upload with Abandon, before closing, after a part failed or while a write waits for the upload, sending nothing more, completing nothing and keeping the writing code''s error, and holds a bounded amount while the upload is behind' {
        [byte[]] $content = New-RandomContent 5000
        $service.ServeUpload('abandoned.bin', 4096)
        $service.ServeUpload('faulted.bin', 4096)
        $service.FailPart('faulted.bin', 1, 400)
        $service.ServeUpload('behind.bin', 65536)
        $service.StallPart('behind.bin', 1)

        $abandoned = Open-FilesFileWriteStream -Path 'abandoned.bin' -Connection $connection -Confirm:$false
        $abandoned.Write($content, 0, 5000)
        $abandoned.Abandon()
        $writeAfter = try { $abandoned.Write($content, 0, 1) } catch { $_.Exception.InnerException }
        $closed = try { $abandoned.Dispose(); $null } catch { $_.Exception.InnerException }
        # The code writing the stream fails once a part it wrote was refused, and abandons the upload as it ends.
        $faulted = Open-FilesFileWriteStream -Path 'faulted.bin' -Connection $connection -Confirm:$false
        $faulted.Write($content, 0, 5000)
        Wait-Until { @($service.Requests | Where-Object { $_.Method -eq 'PUT' -and $_.Path -eq '/upload/faulted.bin/1' }).Count -eq 1 -and $service.ActiveRequests -eq 0 }
        # Lets the engine take in the refused part before the upload is abandoned.
        Start-Sleep -Milliseconds 500
        $producer = try {
            try {
                throw 'the producer failed'
            }
            catch {
                $faulted.Abandon()
                throw
            }
            finally {
                $faulted.Dispose()
            }
        }
        catch {
            $_
        }
        $behind = Open-FilesFileWriteStream -Path 'behind.bin' -Connection $connection -Confirm:$false
        $written = [ref] 0L
        $writer = [powershell]::Create()
        $writer.AddScript({
                param($Stream, $Written)
                $chunk = [byte[]]::new(65536)
                try {
                    for ($i = 0; $i -lt 256; $i++) {
                        $Stream.Write($chunk, 0, $chunk.Length)
                        $Written.Value += $chunk.Length
                    }
                    'finished'
                }
                catch {
                    $_.Exception.InnerException.Kind
                }
            }).AddArgument($behind).AddArgument($written) | Out-Null
        $handle = $writer.BeginInvoke()
        Wait-Until { $written.Value -ge 1MB }
        Start-Sleep -Milliseconds 500
        $heldBehind = $written.Value
        $waiting = -not $handle.IsCompleted
        $behind.Abandon()
        $unblocked = $true
        try {
            Wait-Until { $handle.IsCompleted }
        }
        catch {
            # Stops the stalled part, so that the writer ends and the test fails rather than hangs.
            $unblocked = $false
            $service.Dispose()
        }
        $outcome = @($writer.EndInvoke($handle))
        $writer.Dispose()
        $behindClosed = try { $behind.Dispose(); $null } catch { $_.Exception.InnerException }

        $abandoned.IsAbandoned | Should -BeTrue
        $writeAfter.Kind | Should -Be 'Cancelled'
        $closed | Should -BeNullOrEmpty
        @(Get-ApiRequest $service -Method POST -Path '/files/abandoned.bin').Count | Should -Be 0
        $producer.Exception.Message | Should -Be 'the producer failed'
        $faulted.IsAbandoned | Should -BeTrue
        $faulted.CanWrite | Should -BeFalse
        @(Get-ApiRequest $service -Method POST -Path '/files/faulted.bin').Count | Should -Be 0
        $waiting | Should -BeTrue
        # Of 16 MiB, only what the upload's 1 MiB pipe and the parts it holds take while it is behind.
        $heldBehind | Should -BeLessThan 4MB
        $unblocked | Should -BeTrue
        $outcome | Should -Be @('Cancelled')
        $behindClosed | Should -BeNullOrEmpty
        @(Get-ApiRequest $service -Method POST -Path '/files/behind.bin').Count | Should -Be 0
    }

    It 'opens nothing to write under -WhatIf, and reports Files.ConnectionClosed when the connection closes before a stream is closed or while it is read' {
        [byte[]] $content = New-RandomContent 5000
        $service.ServeUpload('closing.bin', 4096)
        $service.ServeVersionedDownload('closing-read.bin', $content, 'v1')
        $service.HoldRange('closing-read.bin', 2000)
        $buffer = [byte[]]::new(700)
        $service.ClearRequests()

        $preview = Open-FilesFileWriteStream -Path 'preview.bin' -Connection $connection -WhatIf
        $previewRequests = $service.Requests.Count
        $closing = Connect-TestService -Service $service -InMemory
        $writing = Open-FilesFileWriteStream -Path 'closing.bin' -Connection $closing -Confirm:$false
        $writing.Write($content, 0, 5000)
        $reading = Open-FilesFileReadStream -Path 'closing-read.bin' -PartSize 1000 -MaxConcurrency 1 -Connection $closing
        Wait-Until { $service.IsRangeHeld('closing-read.bin', 2000) }
        $total = 0
        $readBeforeClose = $null
        try { while ($total -lt 2000 -and ($count = $reading.Read($buffer, 0, [Math]::Min($buffer.Length, 2000 - $total))) -gt 0) { $total += $count } } catch { $readBeforeClose = $_ }
        Disconnect-FilesAccount -Connection $closing -Confirm:$false
        $closedWrite = try { $writing.Dispose(); $null } catch { $_.Exception.InnerException }
        $closedRead = try { while (($count = $reading.Read($buffer, 0, $buffer.Length)) -gt 0) { $total += $count }; $null } catch { $_.Exception.InnerException }
        $reading.Dispose()
        $service.ReleaseRange('closing-read.bin', 2000)
        if ($readBeforeClose) { throw $readBeforeClose }

        $preview | Should -BeNullOrEmpty
        $previewRequests | Should -Be 0
        $closedWrite.Kind | Should -Be 'ConnectionClosed'
        @(Get-ApiRequest $service -Method POST -Path '/files/closing.bin').Count | Should -Be 0
        $total | Should -Be 2000
        $closedRead.Kind | Should -Be 'ConnectionClosed'
    }

    It 'stops opening a file to read, and its download, when the pipeline stops' {
        [byte[]] $content = New-RandomContent 5000
        $service.ServeVersionedDownload('slow.bin', $content, 'v1')
        $service.HoldRange('slow.bin', 0)
        $running = Start-Command -Connection $inMemory -Script {
            param($Connection)
            Open-FilesFileReadStream -Path 'slow.bin' -PartSize 1000 -MaxConcurrency 1 -Connection $Connection
        }
        Wait-Until { $service.IsRangeHeld('slow.bin', 0) }

        $elapsed = Measure-Command { $running.Shell.Stop() }
        $state = $running.Shell.InvocationStateInfo.State
        $running.Shell.Dispose()
        $service.ReleaseRange('slow.bin', 0)
        Wait-Until { $service.ActiveRequests -eq 0 }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $state | Should -Be 'Stopped'
        # The download stopped with the command: no part after the held one was asked for.
        @($service.RangesServed('slow.bin') | Where-Object { $_ -ne '0-999' }).Count | Should -Be 0
    }

    It 'stops the download, or abandons the upload, of a stream it opened when it fails before passing the stream on or the command it passes it to fails, reporting that failure, and leaves the connection, its other streams and other connections working' {
        [byte[]] $content = New-RandomContent 5000
        $service.ServeVersionedDownload('unwritten.bin', $content, 'v1')
        $service.HoldRange('unwritten.bin', 1000)
        $service.ServeVersionedDownload('passed.bin', $content, 'v1')
        $service.HoldRange('passed.bin', 1000)
        $service.ServeUpload('passed-write.bin', 4096)
        $service.ServeVersionedDownload('bystander.bin', $content, 'v1')
        $service.ServeVersionedDownload('after.bin', $content, 'v1')
        $other = Connect-TestService -Service $service -InMemory
        $bystander = Open-FilesFileReadStream -Path 'bystander.bin' -PartSize 1000 -MaxConcurrency 1 -Connection $inMemory
        # Writing the command's Verbose message fails once the file is open, before the command writes the stream.
        $receiver = [FilesCom.PowerShell.Tests.FailingReceiver]::new('the verbose stream failed')
        $shell = [powershell]::Create()
        $shell.AddScript('param($Manifest) Import-Module -Name $Manifest').AddArgument($script:ModuleManifest).Invoke() | Out-Null
        $shell.Commands.Clear()
        $shell.Streams.Verbose.add_DataAdding($receiver.Handler[System.Management.Automation.DataAddingEventArgs]())
        $shell.AddScript({
                param($Connection)
                Open-FilesFileReadStream -Path 'unwritten.bin' -PartSize 1000 -MaxConcurrency 1 -Connection $Connection -Verbose
            }).AddArgument($inMemory) | Out-Null
        $handle = $shell.BeginInvoke()
        try {
            Wait-Until { $receiver.IsReached -and $service.IsRangeHeld('unwritten.bin', 1000) }
            # Lets the open return the stream to the command.
            Start-Sleep -Milliseconds 300
        }
        finally {
            $receiver.Release()
        }
        $output = @($shell.EndInvoke($handle))
        $reported = @($shell.Streams.Error)
        $shell.Dispose()
        $service.ReleaseRange('unwritten.bin', 1000)
        $passedRead = [ref] $null
        $readFailure = try {
            Open-FilesFileReadStream -Path 'passed.bin' -PartSize 1000 -MaxConcurrency 1 -Connection $inMemory | ForEach-Object { $passedRead.Value = $_; throw 'the next command failed' }
        }
        catch {
            $_
        }
        $service.ReleaseRange('passed.bin', 1000)
        $passedWrite = [ref] $null
        $writeFailure = try {
            Open-FilesFileWriteStream -Path 'passed-write.bin' -Connection $inMemory -Confirm:$false | ForEach-Object { $passedWrite.Value = $_; $_.Write($content, 0, 5000); throw 'the next command failed' }
        }
        catch {
            $_
        }
        $closedLater = try { $passedWrite.Value.Dispose(); $null } catch { $_.Exception.InnerException }
        Wait-Until { $service.ActiveRequests -eq 0 }
        # Leaves a download that went on the time to ask for its next part.
        Start-Sleep -Milliseconds 300
        $readBystander = [System.IO.MemoryStream]::new()
        $bystander.CopyTo($readBystander)
        $bystander.Dispose()
        $after = Open-FilesFileReadStream -Path 'after.bin' -Connection $other
        $read = [System.IO.MemoryStream]::new()
        $after.CopyTo($read)
        $after.Dispose()
        $otherOpen = $other.IsOpen
        Disconnect-FilesAccount -Connection $other -Confirm:$false

        $output.Count | Should -Be 0
        $reported.Count | Should -Be 1
        $reported[0].FullyQualifiedErrorId | Should -BeLike 'Files.Unexpected,*'
        $reported[0].Exception.Message | Should -Be 'the verbose stream failed'
        $reported[0].TargetObject | Should -Be 'unwritten.bin'
        $readFailure.Exception.Message | Should -Be 'the next command failed'
        $passedRead.Value.CanRead | Should -BeFalse
        # The downloads stopped with the commands: no part after the held one was asked for.
        foreach ($path in 'unwritten.bin', 'passed.bin') {
            @($service.RangesServed($path) | Where-Object { $_ -notin '0-999', '1000-1999' }).Count | Should -Be 0
        }
        $writeFailure.Exception.Message | Should -Be 'the next command failed'
        $passedWrite.Value.IsAbandoned | Should -BeTrue
        $closedLater | Should -BeNullOrEmpty
        @(Get-ApiRequest $service -Method POST -Path '/files/passed-write.bin').Count | Should -Be 0
        # The connection, a stream it opened before, and another connection go on working.
        $inMemory.IsOpen | Should -BeTrue
        Test-SameContent -Expected $content -Actual $readBystander.ToArray() | Should -BeTrue
        $otherOpen | Should -BeTrue
        Test-SameContent -Expected $content -Actual $read.ToArray() | Should -BeTrue
    }
}

Describe 'Wait-FilesFileAction' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'waits for a copy from the pipeline until its migration completes, and writes the migration' {
        $service.Respond('POST', '/file_actions/copy/a.txt', 200, '{"status":"enqueued","file_migration_id":11}')
        $service.Respond('GET', '/file_migrations/11', 200, '{"id":11,"status":"processing"}')
        $service.Respond('GET', '/file_migrations/11', 200, '{"id":11,"status":"completed","files_moved":1}')

        $migration = Copy-FilesFile -Path 'a.txt' -Destination 'b.txt' -Connection $connection -Confirm:$false | Wait-FilesFileAction -PollIntervalSeconds 0.05

        $migration | Should -BeOfType ([FilesCom.PowerShell.Models.FileMigration])
        $migration.Status | Should -Be 'completed'
        @(Get-ApiRequest $service -Method GET -Path '/file_migrations/11').Count | Should -Be 2
    }

    It 'reports a migration that failed with its ID, as an error that does not stop other records' {
        $service.Respond('POST', '/file_actions/copy/a.txt', 200, '{"status":"enqueued","file_migration_id":12}')
        $service.Respond('GET', '/file_migrations/12', 200, '{"id":12,"status":"failed"}')

        Copy-FilesFile -Path 'a.txt' -Destination 'b.txt' -Connection $connection -Confirm:$false | Wait-FilesFileAction -PollIntervalSeconds 0.05 -ErrorVariable failures -ErrorAction SilentlyContinue

        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.FileActionFailed*'
        $failures[0].Exception.FileMigrationId | Should -Be 12
        $failures[0].Exception.FileMigrationStatus | Should -Be 'failed'
        $failures[0].ErrorDetails.RecommendedAction | Should -Match 'Get-FilesFileMigration -Id 12'
    }

    It 'waits with -Removed until Files.com has nothing at a path, and fails with Files.OperationTimeout after -Timeout' {
        $service.Respond('GET', '/file_actions/metadata/old', 200, '{"path":"old","type":"directory"}')
        $service.Respond('GET', '/file_actions/metadata/old', 404, '{"error":"Not Found","http-code":404,"type":"not-found"}')
        $service.Respond('GET', '/file_actions/metadata/busy', 200, '{"path":"busy","type":"directory"}')

        Wait-FilesFileAction -Path 'old' -Removed -PollIntervalSeconds 0.05 -Connection $connection | Should -BeNullOrEmpty
        $elapsed = Measure-Command { $failure = try { Wait-FilesFileAction -Path 'busy' -Removed -PollIntervalSeconds 0.05 -Timeout '00:00:00.5' -Connection $connection -ErrorAction Stop } catch { $_ } }

        @(Get-ApiRequest $service -Path '/file_actions/metadata/old').Count | Should -Be 2
        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.OperationTimeout*'
        $failure.ErrorDetails.RecommendedAction | Should -Match '-Timeout'
        $elapsed.TotalSeconds | Should -BeLessThan 10
        { Wait-FilesFileAction -Path 'busy' -Removed -Timeout 0 -Connection $connection -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
    }
}
