BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')

    # Bytes that differ from file to file: New-RandomContent gives the same ones every time.
    function New-SeededContent {
        param([int] $Count, [int] $Seed)
        $bytes = [byte[]]::new($Count)
        [System.Random]::new($Seed).NextBytes($bytes)
        $bytes
    }

    # Every entry under a local directory, as sorted relative paths with / between names, folders ending in /.
    function Get-LocalEntry {
        param([string] $LiteralPath)
        $root = (Get-Item -LiteralPath $LiteralPath -Force).FullName.TrimEnd([System.IO.Path]::DirectorySeparatorChar)
        @(Get-ChildItem -LiteralPath $root -Recurse -Force | ForEach-Object {
                $relative = $_.FullName.Substring($root.Length + 1).Replace([System.IO.Path]::DirectorySeparatorChar, '/')
                if ($_.PSIsContainer -and -not $_.LinkType) { "$relative/" } else { $relative }
            } | Sort-Object)
    }

    # The requests for a file's bytes that reached the service's storage.
    function Get-StorageRequest {
        param([FilesCom.PowerShell.Tests.FilesTestService] $Service, [string] $RemotePath)
        @($Service.Requests | Where-Object Path -EQ "/tree/$([uri]::EscapeDataString($RemotePath))")
    }

    # Each request for a file's bytes, as its Range and the If-Match it asked for, in the order they arrived.
    function Get-StorageAsk {
        param([FilesCom.PowerShell.Tests.FilesTestService] $Service, [string] $RemotePath)
        @(Get-StorageRequest $Service $RemotePath | ForEach-Object { "$($_.Header('Range')) If-Match=$($_.Header('If-Match'))" })
    }

    function Test-LocalContent {
        param([string] $LiteralPath, [byte[]] $Expected)
        (Test-Path -LiteralPath $LiteralPath -PathType Leaf) -and (Test-SameContent -Expected $Expected -Actual ([System.IO.File]::ReadAllBytes($LiteralPath)))
    }
}

Describe 'Receive-FilesDirectory' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        # The same site in memory, where its folders' files come from Amazon S3, each bound to a version.
        $inMemory = Connect-TestService -Service $service -InMemory
        $work = (New-Item -ItemType Directory -Path (Join-Path $TestDrive "work-$([guid]::NewGuid().ToString('n'))")).FullName
        [byte[]] $alpha = New-SeededContent 2500 1
        [byte[]] $big = New-SeededContent 5000 2
        [byte[]] $deep = New-SeededContent 1200 3
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $inMemory -Confirm:$false
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'downloads every file of a folder to its relative path with exact bytes, creates its empty folders, and describes it with -PassThru' {
        $service.ServeTree('reports', @{ 'a.bin' = $alpha; 'big.bin' = $big; 'sub/deeper/c.bin' = $deep }, @('empty', 'sub/also-empty'), 2)
        # Missing, as are the folders above it: the command creates them.
        $local = Join-Path $work 'new/reports'

        $result = Receive-FilesDirectory -RemotePath 'reports' -LiteralPath $local -MaxConcurrentFiles 2 -PartSize 1000 -Limiter (New-FilesTransferLimiter -MaxConcurrentRequests 2) -Connection $inMemory -PassThru -Confirm:$false

        Test-LocalContent -LiteralPath (Join-Path $local 'a.bin') -Expected $alpha | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $local 'big.bin') -Expected $big | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $local 'sub/deeper/c.bin') -Expected $deep | Should -BeTrue
        # Nothing else: no stage or private folder is left behind.
        Get-LocalEntry $local | Should -Be @('a.bin', 'big.bin', 'empty/', 'sub/', 'sub/also-empty/', 'sub/deeper/', 'sub/deeper/c.bin')
        $result.RemoteDirectory | Should -Be 'reports'
        $result.LocalDirectory | Should -Be $local
        @($result.Files.RelativePath) | Should -Be @('a.bin', 'big.bin', 'sub/deeper/c.bin')
        @($result.Files.CompletedEarlier) | Should -Be @($false, $false, $false)
        ($result.Files | Where-Object RelativePath -EQ 'sub/deeper/c.bin').LocalPath | Should -Be (Join-Path $local 'sub/deeper/c.bin')
        ($result.Files | Where-Object RelativePath -EQ 'sub/deeper/c.bin').RemotePath | Should -Be 'reports/sub/deeper/c.bin'
        @($result.Directories | Sort-Object) | Should -Be @('empty', 'sub/also-empty')
        $result.Excluded.Count | Should -Be 0
        $result.Bytes | Should -Be 8700
        # Each file came in -PartSize ranges of the version its first range answered with: that range asked for alone and
        # unconditionally, every later one for that version. The listing's second page was read.
        @($service.TreeRangesServed('reports/big.bin') | Sort-Object { [long]($_ -split '-')[0] }) | Should -Be @('0-999', '1000-1999', '2000-2999', '3000-3999', '4000-4999')
        foreach ($file in 'reports/a.bin', 'reports/big.bin', 'reports/sub/deeper/c.bin') {
            $asks = Get-StorageAsk $service $file
            $asks[0] | Should -Be 'bytes=0-999 If-Match=' -Because $file
            @($asks | Select-Object -Skip 1 | ForEach-Object { ($_ -split ' ')[1] } | Sort-Object -Unique) | Should -Be @('If-Match="v1"') -Because $file
        }
        @(Get-ApiRequest $service -Method GET -Path '/folders/reports' | Where-Object { $_.Query['cursor'] -eq 'page-2' }).Count | Should -Be 1
    }

    It 'downloads the site''s root folder with an empty -RemotePath, and a piped folder with its own connection and workspace' {
        $service.ServeTree('', @{ 'top.bin' = $alpha; 'reports/r.bin' = $deep })
        $service.Respond('GET', '/file_actions/metadata/reports', 200, '{"path":"reports","type":"directory","display_name":"reports"}')
        $root = Join-Path $work 'root'
        $piped = Join-Path $work 'piped'

        $whole = Receive-FilesDirectory -RemotePath '' -LiteralPath $root -Connection $connection -PassThru -Confirm:$false
        $service.ClearRequests()
        $folder = Get-FilesFile -Path 'reports' -WorkspaceScope 5 -Connection $connection
        $part = $folder | Receive-FilesDirectory -LiteralPath $piped -PassThru -Confirm:$false

        $whole.RemoteDirectory | Should -Be ''
        @($whole.Files.RelativePath) | Should -Be @('reports/r.bin', 'top.bin')
        Test-LocalContent -LiteralPath (Join-Path $root 'top.bin') -Expected $alpha | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $root 'reports/r.bin') -Expected $deep | Should -BeTrue
        @($part.Files.RelativePath) | Should -Be @('r.bin')
        Test-LocalContent -LiteralPath (Join-Path $piped 'r.bin') -Expected $deep | Should -BeTrue
        # The piped folder's requests ran in the workspace it was read in.
        @($service.Requests | Where-Object { $_.Path -like '/api/*' } | ForEach-Object { $_.Header('X-Files-Workspace-Id') } | Sort-Object -Unique) | Should -Be @('5')
    }

    It 'continues an interrupted download with the same -CheckpointDirectory: completed files are CompletedEarlier and not fetched again, and the unfinished file resumes' {
        $service.ServeTree('media', @{ 'a.bin' = $alpha; 'big.bin' = $big; 'sub/c.bin' = $deep })
        $service.FailTreeDownload('media/big.bin', 3000, 503)
        $local = Join-Path $work 'media'
        $checkpoints = Join-Path $work 'checkpoints'
        $arguments = @{ RemotePath = 'media'; LiteralPath = $local; CheckpointDirectory = $checkpoints; MaxConcurrentFiles = 1; MaxConcurrency = 1; PartSize = 1000; Connection = $inMemory; Confirm = $false }

        $failure = try { Receive-FilesDirectory @arguments -ErrorAction Stop } catch { $_ }

        $failure | Should -Not -BeNullOrEmpty
        ($failure | Format-List * -Force | Out-String) + ($failure.Exception | Format-List * -Force | Out-String) | Should -Not -Match 'sentinel-signed-url-secret'
        # The file completed before the failure stays; the unfinished one is not under its name, and the next never started.
        Test-LocalContent -LiteralPath (Join-Path $local 'a.bin') -Expected $alpha | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $local 'big.bin') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $local 'sub/c.bin') | Should -BeFalse
        @($service.TreeRangesServed('media/big.bin')) | Should -Be @('0-999', '1000-1999', '2000-2999')
        @(Get-StorageRequest $service 'media/sub/c.bin').Count | Should -Be 0

        $service.ClearTreeFailure('media/big.bin')
        $service.ClearRequests()
        $result = Receive-FilesDirectory @arguments -PassThru

        ($result.Files | Where-Object RelativePath -EQ 'a.bin').CompletedEarlier | Should -BeTrue
        ($result.Files | Where-Object RelativePath -EQ 'big.bin').CompletedEarlier | Should -BeFalse
        ($result.Files | Where-Object RelativePath -EQ 'sub/c.bin').CompletedEarlier | Should -BeFalse
        # The completed file was not fetched again: storage confirmed its version with its first byte, asked for as that
        # version. The unfinished one, confirmed the same way, fetched only what it lacked.
        Get-StorageAsk $service 'media/a.bin' | Should -Be @('bytes=0-0 If-Match="v1"')
        @(Get-ApiRequest $service -Method GET -Path '/files/media/a.bin').Count | Should -Be 1
        @($service.TreeRangesServed('media/big.bin')) | Should -Be @('0-999', '1000-1999', '2000-2999', '0-0', '3000-3999', '4000-4999')
        Test-LocalContent -LiteralPath (Join-Path $local 'a.bin') -Expected $alpha | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $local 'big.bin') -Expected $big | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $local 'sub/c.bin') -Expected $deep | Should -BeTrue
        Get-LocalEntry $local | Should -Be @('a.bin', 'big.bin', 'sub/', 'sub/c.bin')
    }

    # On Windows the local directory, and those above it, are held from before the listing, so they cannot be moved.
    if (-not $IsWindows) {
        It 'does nothing in a local directory that is replaced while the remote folder is listed, says it moved, and keeps its checkpoint folder' {
            $service.ServeTree('moving', @{ 'a.bin' = $alpha; 'big.bin' = $big })
            $local = Join-Path $work 'moving'
            $checkpoints = Join-Path $work 'checkpoints'
            New-Item -ItemType Directory -Path $local | Out-Null
            $service.HoldRoute('GET', '/folders/moving')
            $running = Start-Command -Connection $connection -Arguments $local, $checkpoints -Script {
                param($Connection, $Local, $Checkpoints)
                Receive-FilesDirectory -RemotePath 'moving' -LiteralPath $Local -CheckpointDirectory $Checkpoints -Connection $Connection -PassThru -Confirm:$false
            }
            Wait-Until { $service.IsRouteHeld('GET', '/folders/moving') }
            Rename-Item -LiteralPath $local -NewName 'moving-opened'
            New-Item -ItemType Directory -Path $local | Out-Null
            $service.ReleaseRoute('GET', '/folders/moving')
            $output = @($running.Shell.EndInvoke($running.Handle))
            $failures = @($running.Shell.Streams.Error)
            $running.Shell.Dispose()

            $output | Should -BeNullOrEmpty
            $failures.Count | Should -Be 1
            $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
            $failures[0].CategoryInfo.Category | Should -Be 'ResourceUnavailable'
            $failures[0].Exception.PublicationProblem | Should -Be 'DestinationMoved'
            $failures[0].Exception.DownloadPlaced | Should -BeFalse
            $failures[0].Exception.DownloadRetained | Should -BeFalse
            $failures[0].Exception.LocalPath | Should -Be $local
            $failures[0].Exception.CheckpointPath | Should -BeNullOrEmpty
            # The refusal concerns the directory, not a file of it.
            $failures[0].Exception.Message | Should -Not -Match 'completed before it'
            Get-LocalEntry $local | Should -BeNullOrEmpty
            Get-LocalEntry (Join-Path $work 'moving-opened') | Should -BeNullOrEmpty
            @(Get-StorageRequest $service 'moving/a.bin').Count + @(Get-StorageRequest $service 'moving/big.bin').Count | Should -Be 0
            Test-Path -LiteralPath $checkpoints -PathType Container | Should -BeTrue
        }

        It 'reports a file an earlier run completed as in place where its folder moved to, not completed earlier, when the folder moves while its version is confirmed, and recognizes it once the folder is back' {
            $service.ServeTree('media', @{ 'a.bin' = $alpha; 'big.bin' = $big })
            $service.FailTreeDownload('media/big.bin', 3000, 503)
            $local = Join-Path $work 'media'
            $checkpoints = Join-Path $work 'checkpoints'
            $arguments = @{ RemotePath = 'media'; LiteralPath = $local; CheckpointDirectory = $checkpoints; MaxConcurrentFiles = 1; MaxConcurrency = 1; PartSize = 1000; Connection = $inMemory; Confirm = $false }
            $null = try { Receive-FilesDirectory @arguments -ErrorAction Stop } catch { $_ }
            $service.ClearTreeFailure('media/big.bin')
            $service.ClearRequests()
            $service.HoldRoute('GET', '/files/media/a.bin')
            $running = Start-Command -Connection $inMemory -Arguments $local, $checkpoints -Script {
                param($Connection, $Local, $Checkpoints)
                Receive-FilesDirectory -RemotePath 'media' -LiteralPath $Local -CheckpointDirectory $Checkpoints -MaxConcurrentFiles 1 -MaxConcurrency 1 -PartSize 1000 -Connection $Connection -PassThru -Confirm:$false
            }
            Wait-Until { $service.IsRouteHeld('GET', '/files/media/a.bin') }
            $opened = Join-Path $work 'media-opened'
            Rename-Item -LiteralPath $local -NewName 'media-opened'
            New-Item -ItemType Directory -Path $local | Out-Null
            $service.ReleaseRoute('GET', '/files/media/a.bin')
            $output = @($running.Shell.EndInvoke($running.Handle))
            $failures = @($running.Shell.Streams.Error)
            $running.Shell.Dispose()

            # Not success, and not a file that is missing: the file is in place, in the folder the download opened.
            $output | Should -BeNullOrEmpty
            $failures.Count | Should -Be 1
            $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.PublicationUncertain*'
            $failures[0].CategoryInfo.Category | Should -Be 'WriteError'
            $failures[0].Exception.PublicationProblem | Should -Be 'DestinationMoved'
            $failures[0].Exception.DownloadPlaced | Should -BeTrue
            $failures[0].Exception.DownloadRetained | Should -BeFalse
            $failures[0].Exception.LocalPath | Should -Be (Join-Path $local 'a.bin')
            # A folder download's record is kept in its checkpoint folder, which the error does not name as a file.
            $failures[0].Exception.CheckpointPath | Should -BeNullOrEmpty
            $failures[0].ErrorDetails.RecommendedAction | Should -Match 'no longer at the path'
            Test-LocalContent -LiteralPath (Join-Path $opened 'a.bin') -Expected $alpha | Should -BeTrue
            Get-LocalEntry $local | Should -BeNullOrEmpty
            @(Get-ChildItem -LiteralPath $checkpoints -Force).Count | Should -BeGreaterThan 0
            # The completed file's version was confirmed with its first byte, not fetched again; the next file never started.
            Get-StorageAsk $service 'media/a.bin' | Should -Be @('bytes=0-0 If-Match="v1"')
            @(Get-StorageRequest $service 'media/big.bin').Count | Should -Be 0

            Remove-Item -LiteralPath $local
            Rename-Item -LiteralPath $opened -NewName 'media'
            $service.ClearRequests()
            $result = Receive-FilesDirectory @arguments -PassThru

            ($result.Files | Where-Object RelativePath -EQ 'a.bin').CompletedEarlier | Should -BeTrue
            ($result.Files | Where-Object RelativePath -EQ 'big.bin').CompletedEarlier | Should -BeFalse
            Get-StorageAsk $service 'media/a.bin' | Should -Be @('bytes=0-0 If-Match="v1"')
            @(Get-ApiRequest $service -Method GET -Path '/files/media/a.bin').Count | Should -Be 1
            # Every range once, after the first byte that confirmed the version: the unfinished file kept what it had
            # through the run that failed, and resumed.
            @($service.TreeRangesServed('media/big.bin')) | Should -Be @('0-999', '1000-1999', '2000-2999', '0-0', '3000-3999', '4000-4999')
            Test-LocalContent -LiteralPath (Join-Path $local 'a.bin') -Expected $alpha | Should -BeTrue
            Test-LocalContent -LiteralPath (Join-Path $local 'big.bin') -Expected $big | Should -BeTrue
            Get-LocalEntry $local | Should -Be @('a.bin', 'big.bin')
        }

        It 'keeps the bytes of a file whose folder moves before the file is put in place, advises running it again with parameters it takes, and then puts the file in place without fetching it again' {
            $service.ServeTree('media', @{ 'a.bin' = $alpha; 'big.bin' = $big })
            $local = Join-Path $work 'media'
            $checkpoints = Join-Path $work 'checkpoints'
            $arguments = @{ RemotePath = 'media'; LiteralPath = $local; CheckpointDirectory = $checkpoints; MaxConcurrentFiles = 1; MaxConcurrency = 1; PartSize = 1000; Connection = $inMemory; Confirm = $false }
            # big.bin's details are asked for once its folder is open, before any of its bytes; a.bin is already in place.
            $service.HoldRoute('GET', '/files/media/big.bin')
            $running = Start-Command -Connection $inMemory -Arguments $local, $checkpoints -Script {
                param($Connection, $Local, $Checkpoints)
                Receive-FilesDirectory -RemotePath 'media' -LiteralPath $Local -CheckpointDirectory $Checkpoints -MaxConcurrentFiles 1 -MaxConcurrency 1 -PartSize 1000 -Connection $Connection -PassThru -Confirm:$false
            }
            Wait-Until { $service.IsRouteHeld('GET', '/files/media/big.bin') }
            $opened = Join-Path $work 'media-opened'
            Rename-Item -LiteralPath $local -NewName 'media-opened'
            New-Item -ItemType Directory -Path $local | Out-Null
            $service.ReleaseRoute('GET', '/files/media/big.bin')
            $output = @($running.Shell.EndInvoke($running.Handle))
            $failures = @($running.Shell.Streams.Error)
            $running.Shell.Dispose()

            $output | Should -BeNullOrEmpty
            $failures.Count | Should -Be 1
            $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
            $failures[0].Exception.PublicationProblem | Should -Be 'DestinationMoved'
            $failures[0].Exception.DownloadPlaced | Should -BeFalse
            $failures[0].Exception.DownloadRetained | Should -BeTrue
            $failures[0].Exception.LocalPath | Should -Be (Join-Path $local 'big.bin')
            # An error about one file of the folder names that file's own checkpoint, in the checkpoint folder.
            Split-Path -Parent $failures[0].Exception.CheckpointPath | Should -Be $checkpoints
            Test-Path -LiteralPath $failures[0].Exception.CheckpointPath -PathType Leaf | Should -BeTrue
            $advised = @([regex]::Matches($failures[0].ErrorDetails.RecommendedAction, '(?<![\w-])-(\w+)') | ForEach-Object { $_.Groups[1].Value })
            $advised | Should -Contain 'CheckpointDirectory'
            @($advised | Where-Object { $_ -notin (Get-Command Receive-FilesDirectory).Parameters.Keys }) | Should -BeNullOrEmpty
            Get-LocalEntry $local | Should -BeNullOrEmpty
            Test-LocalContent -LiteralPath (Join-Path $opened 'a.bin') -Expected $alpha | Should -BeTrue
            @($service.TreeRangesServed('media/big.bin')).Count | Should -Be 5

            Remove-Item -LiteralPath $local
            Rename-Item -LiteralPath $opened -NewName 'media'
            $service.ClearRequests()
            $result = Receive-FilesDirectory @arguments -PassThru

            ($result.Files | Where-Object RelativePath -EQ 'a.bin').CompletedEarlier | Should -BeTrue
            # Neither file was fetched again: storage confirmed the version of each with its first byte.
            Get-StorageAsk $service 'media/a.bin' | Should -Be @('bytes=0-0 If-Match="v1"')
            Get-StorageAsk $service 'media/big.bin' | Should -Be @('bytes=0-0 If-Match="v1"')
            Test-LocalContent -LiteralPath (Join-Path $local 'big.bin') -Expected $big | Should -BeTrue
            Get-LocalEntry $local | Should -Be @('a.bin', 'big.bin')
        }
    }

    It 'leaves resumable state when stopped: running it again continues the file from where it stopped' {
        $service.ServeTree('media', @{ 'a.bin' = $alpha; 'big.bin' = $big })
        $service.HoldTreeDownload('media/big.bin', 3000)
        $local = Join-Path $work 'media'
        $checkpoints = Join-Path $work 'checkpoints'
        $running = Start-Command -Connection $inMemory -Arguments $local, $checkpoints -Script {
            param($Connection, $Local, $Checkpoints)
            Receive-FilesDirectory -RemotePath 'media' -LiteralPath $Local -CheckpointDirectory $Checkpoints -MaxConcurrentFiles 1 -MaxConcurrency 1 -PartSize 1000 -Connection $Connection -Confirm:$false
        }
        Wait-Until { $service.IsTreeDownloadHeld('media/big.bin') }

        $elapsed = Measure-Command { $running.Shell.Stop() }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        $running.Shell.Dispose()
        Wait-Until { $service.AbortedRequests -eq 1 }
        Test-Path -LiteralPath (Join-Path $local 'big.bin') | Should -BeFalse

        $result = Receive-FilesDirectory -RemotePath 'media' -LiteralPath $local -CheckpointDirectory $checkpoints -MaxConcurrency 1 -PartSize 1000 -Connection $inMemory -PassThru -Confirm:$false

        # Every range was answered once, after the first byte that confirmed the version: the parts written before the
        # stop were not fetched again.
        @($service.TreeRangesServed('media/big.bin')) | Should -Be @('0-999', '1000-1999', '2000-2999', '0-0', '3000-3999', '4000-4999')
        ($result.Files | Where-Object RelativePath -EQ 'a.bin').CompletedEarlier | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $local 'big.bin') -Expected $big | Should -BeTrue
        Get-LocalEntry $local | Should -Be @('a.bin', 'big.bin')
    }

    It 'stops at a file whose name is taken without -Force, with an error naming that file, and keeps the files before it and every other file' {
        $service.ServeTree('shared', @{ 'a.bin' = $alpha; 'b.bin' = $big; 'c.bin' = $deep })
        $local = New-Item -ItemType Directory -Path (Join-Path $work 'shared')
        $taken = Join-Path $local.FullName 'b.bin'
        Set-Content -LiteralPath $taken -Value 'keep me'
        Set-Content -LiteralPath (Join-Path $local.FullName 'notes.txt') -Value 'mine'

        Receive-FilesDirectory -RemotePath 'shared' -LiteralPath $local.FullName -MaxConcurrentFiles 1 -Connection $connection -Confirm:$false -ErrorVariable failures -ErrorAction SilentlyContinue

        $failures.Count | Should -Be 1
        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
        $failures[0].CategoryInfo.Category | Should -Be 'ResourceExists'
        $failures[0].TargetObject | Should -Be $taken
        $failures[0].Exception.LocalPath | Should -Be $taken
        $failures[0].Exception.PublicationProblem | Should -Be 'DestinationExists'
        $failures[0].Exception.Message | Should -Match ([regex]::Escape($taken))
        $failures[0].Exception.Message | Should -Not -Match 'a\.bin'
        $failures[0].ErrorDetails.RecommendedAction | Should -Match '-Force'
        Test-LocalContent -LiteralPath (Join-Path $local.FullName 'a.bin') -Expected $alpha | Should -BeTrue
        Get-Content -LiteralPath $taken | Should -Be 'keep me'
        Get-Content -LiteralPath (Join-Path $local.FullName 'notes.txt') | Should -Be 'mine'
        Get-LocalEntry $local.FullName | Should -Be @('a.bin', 'b.bin', 'notes.txt')
        # The taken name was refused before its bytes were fetched, and no file started after it.
        @(Get-StorageRequest $service 'shared/b.bin').Count | Should -Be 0
        @(Get-StorageRequest $service 'shared/c.bin').Count | Should -Be 0
    }

    It 'with -Force replaces a regular file, but never a folder or link at a file''s name, and never follows a link where a folder belongs' {
        $service.ServeTree('shared', @{ 'a.bin' = $alpha; 'sub/b.bin' = $deep })
        $regular = New-Item -ItemType Directory -Path (Join-Path $work 'regular')
        Set-Content -LiteralPath (Join-Path $regular.FullName 'a.bin') -Value 'old'
        $folder = New-Item -ItemType Directory -Path (Join-Path $work 'folder')
        New-Item -ItemType Directory -Path (Join-Path $folder.FullName 'a.bin') | Out-Null
        Set-Content -LiteralPath (Join-Path $folder.FullName 'a.bin/inside.txt') -Value 'inside'
        $link = New-Item -ItemType Directory -Path (Join-Path $work 'link')
        $target = Join-Path $work 'target.txt'
        Set-Content -LiteralPath $target -Value 'target'
        New-Item -ItemType SymbolicLink -Path (Join-Path $link.FullName 'a.bin') -Target $target | Out-Null
        $linkedFolder = New-Item -ItemType Directory -Path (Join-Path $work 'linked-folder')
        $outside = New-Item -ItemType Directory -Path (Join-Path $work 'outside')
        New-Item -ItemType SymbolicLink -Path (Join-Path $linkedFolder.FullName 'sub') -Target $outside.FullName | Out-Null
        $download = @{ RemotePath = 'shared'; Force = $true; MaxConcurrentFiles = 1; Connection = $connection; Confirm = $false; ErrorAction = 'SilentlyContinue' }

        Receive-FilesDirectory @download -LiteralPath $regular.FullName -ErrorVariable regularFailures
        Receive-FilesDirectory @download -LiteralPath $folder.FullName -ErrorVariable folderFailures
        Receive-FilesDirectory @download -LiteralPath $link.FullName -ErrorVariable linkFailures
        Receive-FilesDirectory @download -LiteralPath $linkedFolder.FullName -ErrorVariable linkedFolderFailures

        $regularFailures.Count | Should -Be 0
        Test-LocalContent -LiteralPath (Join-Path $regular.FullName 'a.bin') -Expected $alpha | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $regular.FullName 'sub/b.bin') -Expected $deep | Should -BeTrue
        foreach ($refused in @(@{ Failures = $folderFailures; Name = Join-Path $folder.FullName 'a.bin' }, @{ Failures = $linkFailures; Name = Join-Path $link.FullName 'a.bin' })) {
            $refused.Failures.Count | Should -Be 1
            $refused.Failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
            $refused.Failures[0].Exception.PublicationProblem | Should -Be 'DestinationNotRegularFile'
            $refused.Failures[0].TargetObject | Should -Be $refused.Name
        }
        Get-Content -LiteralPath (Join-Path $folder.FullName 'a.bin/inside.txt') | Should -Be 'inside'
        (Get-Item -LiteralPath (Join-Path $link.FullName 'a.bin') -Force).LinkType | Should -Be 'SymbolicLink'
        Get-Content -LiteralPath $target | Should -Be 'target'
        $linkedFolderFailures.Count | Should -Be 1
        $linkedFolderFailures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
        $linkedFolderFailures[0].Exception.Message | Should -Match 'symbolic link'
        (Get-Item -LiteralPath (Join-Path $linkedFolder.FullName 'sub') -Force).LinkType | Should -Be 'SymbolicLink'
        @(Get-ChildItem -LiteralPath $outside.FullName -Force).Count | Should -Be 0
    }

    It 'refuses a checkpoint folder inside the local directory, a path of another provider, and a file as the local directory, before sending anything or creating a folder' {
        $service.ServeTree('media', @{ 'a.bin' = $alpha })
        $local = Join-Path $work 'media'
        $file = Join-Path $work 'file.txt'
        Set-Content -LiteralPath $file -Value 'a file'
        $download = @{ RemotePath = 'media'; Connection = $connection; Confirm = $false; ErrorAction = 'Stop' }
        Push-Location -LiteralPath $work
        try {
            $inside = try { Receive-FilesDirectory @download -LiteralPath $local -CheckpointDirectory (Join-Path $local 'checkpoints') } catch { $_ }
            $same = try { Receive-FilesDirectory @download -LiteralPath $local -CheckpointDirectory $local } catch { $_ }
            # Paths of another provider, which would otherwise be read as names relative to the process folder.
            $checkpointProvider = try { Receive-FilesDirectory @download -LiteralPath $local -CheckpointDirectory 'Env:FILESCOM_DIRECTORY_CHECKPOINT' } catch { $_ }
            $localProvider = try { Receive-FilesDirectory @download -LiteralPath 'Env:FILESCOM_DIRECTORY_LOCAL' } catch { $_ }
            $notFolder = try { Receive-FilesDirectory @download -LiteralPath $file } catch { $_ }
            $restartAlone = try { Receive-FilesDirectory @download -LiteralPath $local -CheckpointRestart StartOver } catch { $_ }
        }
        finally {
            Pop-Location
        }
        { Receive-FilesDirectory @download -LiteralPath $local -MaxConcurrentFiles 65 } | Should -Throw -ErrorId 'ParameterArgumentValidationError*'
        { Receive-FilesDirectory @download -LiteralPath $local -PartSize 0 } | Should -Throw -ErrorId 'ParameterArgumentValidationError*'

        foreach ($refusal in $inside, $same, $checkpointProvider, $localProvider, $notFolder, $restartAlone) {
            $refusal.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        }
        $inside.Exception.Message | Should -Match 'inside'
        $service.Requests.Count | Should -Be 0
        Test-Path -LiteralPath $local | Should -BeFalse
        Get-LocalEntry $work | Should -Be @('file.txt')
        @(Get-ChildItem -LiteralPath ([System.IO.Directory]::GetCurrentDirectory()) -Filter 'FILESCOM_DIRECTORY_*' -Force).Count | Should -Be 0
    }

    It 'takes -LiteralPath and -CheckpointDirectory as relative or PowerShell-drive paths' {
        $service.ServeTree('media', @{ 'a.bin' = $alpha })
        New-PSDrive -Name FilesDirs -PSProvider FileSystem -Root $work | Out-Null
        Push-Location -LiteralPath $work
        try {
            $relative = Receive-FilesDirectory -RemotePath 'media' -LiteralPath 'relative' -CheckpointDirectory 'relative-checkpoints' -Connection $connection -PassThru -Confirm:$false
            $drive = Receive-FilesDirectory -RemotePath 'media' -LiteralPath 'FilesDirs:\drive' -CheckpointDirectory 'FilesDirs:\drive-checkpoints' -Connection $connection -PassThru -Confirm:$false
        }
        finally {
            Pop-Location
            Remove-PSDrive -Name FilesDirs
        }

        $relative.LocalDirectory | Should -Be (Join-Path $work 'relative')
        $drive.LocalDirectory | Should -Be (Join-Path $work 'drive')
        Test-LocalContent -LiteralPath (Join-Path $work 'relative/a.bin') -Expected $alpha | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $work 'drive/a.bin') -Expected $alpha | Should -BeTrue
    }

    It 'gives piped folders directories of their own with a script block, and refuses one that would share a directory or checkpoint folder with an earlier folder' {
        $service.ServeTree('projects', @{ 'alpha/a.bin' = $alpha; 'beta/b.bin' = $deep; 'gamma/c.bin' = $big })
        $folders = @(Get-FilesChildItem -Path 'projects' -Connection $connection | Where-Object Type -EQ 'directory')
        $own = Join-Path $work 'own'
        $shared = Join-Path $work 'shared'

        $results = @($folders | Receive-FilesDirectory -LiteralPath { Join-Path $own $_.DisplayName } -CheckpointDirectory { Join-Path $work "checkpoints-$($_.DisplayName)" } -PassThru -Confirm:$false)
        $sameDirectory = try { $folders | Receive-FilesDirectory -LiteralPath $shared -Confirm:$false -ErrorAction Stop } catch { $_ }
        $sameCheckpoint = try { $folders | Receive-FilesDirectory -LiteralPath { Join-Path $work "again-$($_.DisplayName)" } -CheckpointDirectory (Join-Path $work 'one-checkpoint') -Confirm:$false -ErrorAction Stop } catch { $_ }

        @($results.RemoteDirectory) | Should -Be @('projects/alpha', 'projects/beta', 'projects/gamma')
        Test-LocalContent -LiteralPath (Join-Path $own 'alpha/a.bin') -Expected $alpha | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $own 'beta/b.bin') -Expected $deep | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $own 'gamma/c.bin') -Expected $big | Should -BeTrue
        # The first folder was downloaded before the second was refused; the third never started.
        $sameDirectory.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        $sameDirectory.Exception.Message | Should -Match 'projects/beta'
        Get-LocalEntry $shared | Should -Be @('a.bin')
        $sameCheckpoint.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        $sameCheckpoint.Exception.Message | Should -Match 'projects/beta'
        Test-LocalContent -LiteralPath (Join-Path $work 'again-alpha/a.bin') -Expected $alpha | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $work 'again-beta') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $work 'again-gamma') | Should -BeFalse
    }

    It 'writes a piped folder''s file failure as that folder''s error and goes on with the next folder' {
        $service.ServeTree('projects', @{ 'alpha/a.bin' = $alpha; 'beta/b.bin' = $deep })
        $folders = @(Get-FilesChildItem -Path 'projects' -Connection $connection | Where-Object Type -EQ 'directory')
        $own = Join-Path $work 'own'
        New-Item -ItemType Directory -Path (Join-Path $own 'alpha') | Out-Null
        Set-Content -LiteralPath (Join-Path $own 'alpha/a.bin') -Value 'keep me'

        $results = @($folders | Receive-FilesDirectory -LiteralPath { Join-Path $own $_.DisplayName } -PassThru -Confirm:$false -ErrorVariable failures -ErrorAction SilentlyContinue)

        $failures.Count | Should -Be 1
        $failures[0].TargetObject | Should -Be (Join-Path $own 'alpha/a.bin')
        @($results.RemoteDirectory) | Should -Be @('projects/beta')
        Get-Content -LiteralPath (Join-Path $own 'alpha/a.bin') | Should -Be 'keep me'
        Test-LocalContent -LiteralPath (Join-Path $own 'beta/b.bin') -Expected $deep | Should -BeTrue
    }

    It 'refuses a piped object that is not a folder before sending anything for it' {
        $service.Respond('GET', '/file_actions/metadata/reports/q3.csv', 200, '{"path":"reports/q3.csv","type":"file"}')
        $file = Get-FilesFile -Path 'reports/q3.csv' -Connection $connection
        $service.ClearRequests()

        $refusal = try { $file | Receive-FilesDirectory -LiteralPath (Join-Path $work 'q3') -Confirm:$false -ErrorAction Stop } catch { $_ }

        $refusal.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        $refusal.Exception.Message | Should -Match 'Receive-FilesFile'
        $service.Requests.Count | Should -Be 0
        Test-Path -LiteralPath (Join-Path $work 'q3') | Should -BeFalse
    }

    It 'refuses to continue a file whose kept bytes were cut short, fetching nothing, unless -CheckpointRestart StartOver downloads it again from its first byte' {
        $service.ServeTree('media', @{ 'a.bin' = $alpha; 'big.bin' = $big })
        $service.FailTreeDownload('media/big.bin', 3000, 503)
        $local = Join-Path $work 'media'
        $arguments = @{ RemotePath = 'media'; LiteralPath = $local; CheckpointDirectory = (Join-Path $work 'checkpoints'); MaxConcurrentFiles = 1; MaxConcurrency = 1; PartSize = 1000; Connection = $inMemory; Confirm = $false; ErrorAction = 'Stop' }
        { Receive-FilesDirectory @arguments } | Should -Throw
        $service.ClearTreeFailure('media/big.bin')
        # The only other file in the local directory is the unfinished file's kept stage.
        $stage = @(Get-ChildItem -LiteralPath $local -Recurse -Force -File | Where-Object Name -NE 'a.bin')
        $stage.Count | Should -Be 1
        $truncating = [System.IO.File]::Open($stage[0].FullName, 'Open', 'ReadWrite')
        try {
            $truncating.SetLength(1000)
        }
        finally {
            $truncating.Dispose()
        }

        $refusal = try { Receive-FilesDirectory @arguments } catch { $_ }
        $refetched = @($service.TreeRangesServed('media/big.bin')).Count
        $result = Receive-FilesDirectory @arguments -CheckpointRestart StartOver -PassThru

        $refusal.FullyQualifiedErrorId | Should -BeLike 'Files.CheckpointRefused*'
        $refusal.ErrorDetails.RecommendedAction | Should -Match 'StartOver'
        $refetched | Should -Be 3
        # Started over: every range again, from the first byte.
        @($service.TreeRangesServed('media/big.bin') | Select-Object -Skip 3) | Should -Be @('0-999', '1000-1999', '2000-2999', '3000-3999', '4000-4999')
        ($result.Files | Where-Object RelativePath -EQ 'a.bin').CompletedEarlier | Should -BeTrue
        Test-LocalContent -LiteralPath (Join-Path $local 'big.bin') -Expected $big | Should -BeTrue
        Get-LocalEntry $local | Should -Be @('a.bin', 'big.bin')
    }

    It 'fetches at most -MaxConcurrentFiles files and -MaxConcurrency parts of each at once, within a shared -Limiter' {
        $service.ServeTree('media', @{ 'a.bin' = $alpha; 'b.bin' = $deep; 'c.bin' = $deep; 'd.bin' = $deep; 'big.bin' = $big })
        $service.DelayTreeDownloads(150)
        $download = @{ RemotePath = 'media'; PartSize = 1000; Connection = $inMemory; Confirm = $false }

        Receive-FilesDirectory @download -LiteralPath (Join-Path $work 'files') -MaxConcurrentFiles 2 -MaxConcurrency 1
        $filesAtOnce = $service.TakeTreeDownloadPeak()
        Receive-FilesDirectory @download -LiteralPath (Join-Path $work 'parts') -MaxConcurrentFiles 1 -MaxConcurrency 3
        $partsAtOnce = $service.TakeTreeDownloadPeak()
        Receive-FilesDirectory @download -LiteralPath (Join-Path $work 'limited') -MaxConcurrentFiles 4 -MaxConcurrency 4 -Limiter (New-FilesTransferLimiter -MaxConcurrentRequests 1)
        $limited = $service.TakeTreeDownloadPeak()

        # -MaxConcurrentFiles and -MaxConcurrency are ceilings. The engine's automatic transfer policy, the default,
        # starts files and parts as the target permits, and a short transfer may use fewer at once than allowed. So each
        # peak is at least 1 and never above its ceiling; the delay makes the downloads overlap, so more would show.
        $filesAtOnce | Should -BeGreaterOrEqual 1
        $filesAtOnce | Should -BeLessOrEqual 2
        $partsAtOnce | Should -BeGreaterOrEqual 1
        $partsAtOnce | Should -BeLessOrEqual 3
        $limited | Should -Be 1
        Test-LocalContent -LiteralPath (Join-Path $work 'limited/big.bin') -Expected $big | Should -BeTrue
    }

    if (-not $IsWindows) {
        It 'refuses a checkpoint folder other users can change, fetching no file' {
            $service.ServeTree('media', @{ 'a.bin' = $alpha })
            $checkpoints = New-Item -ItemType Directory -Path (Join-Path $work 'shared-checkpoints')
            & chmod 777 $checkpoints.FullName

            $refusal = try { Receive-FilesDirectory -RemotePath 'media' -LiteralPath (Join-Path $work 'media') -CheckpointDirectory $checkpoints.FullName -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }

            $refusal.FullyQualifiedErrorId | Should -BeLike 'Files.CheckpointRefused*'
            $refusal.Exception.CheckpointProblem | Should -Be 'Untrusted'
            @($service.Requests | Where-Object { $_.Path -like '/tree/*' -or $_.Path -like '/api/rest/v1/files/*' }).Count | Should -Be 0
            @(Get-ChildItem -LiteralPath $checkpoints.FullName -Force).Count | Should -Be 0
        }
    }

    It 'reports progress from the pipeline thread and completes it' {
        $service.ServeTree('media', @{ 'a.bin' = $alpha; 'big.bin' = $big })
        $local = Join-Path $work 'progress'
        $shell = [powershell]::Create()
        try {
            $shell.AddScript({
                    param($Manifest, $Local, $Connection)
                    Import-Module -Name $Manifest
                    Receive-FilesDirectory -RemotePath 'media' -LiteralPath $Local -PartSize 1000 -Connection $Connection -Confirm:$false
                }).AddArgument($script:ModuleManifest).AddArgument($local).AddArgument($connection) | Out-Null
            $shell.Invoke() | Out-Null
            $records = @($shell.Streams.Progress)
        }
        finally {
            $shell.Dispose()
        }

        $records.Count | Should -BeGreaterThan 1
        $records[0].Activity | Should -Be 'Downloading media'
        $records[-1].RecordType | Should -Be 'Completed'
        $records[-2].PercentComplete | Should -Be 100
        $records[-2].StatusDescription | Should -Match '^media: .*7.500 of 7.500 bytes'
        Test-LocalContent -LiteralPath (Join-Path $local 'big.bin') -Expected $big | Should -BeTrue
    }
}
