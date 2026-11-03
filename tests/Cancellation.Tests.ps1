BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
}

Describe 'Stopping commands' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'cancels the request in progress when the pipeline stops, and returns once it has stopped' {
        $service.Hang('GET', '/users/1')
        $running = Start-Command -Connection $connection -Script { param($Connection) Get-FilesUser -Id 1 -Connection $Connection }
        Wait-Until { $service.ActiveRequests -eq 1 }

        $elapsed = Measure-Command { $running.Shell.Stop() }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        Wait-Until { $service.AbortedRequests -eq 1 }
        $running.Shell.Dispose()

        $service.Respond('GET', '/users/2', 200, '{"id":2}')
        (Get-FilesUser -Id 2 -Connection $connection).Id | Should -Be 2
    }

    It 'cancels an upload while a part is being sent, and never sends its final request' {
        $local = Join-Path $TestDrive 'stalled.bin'
        [System.IO.File]::WriteAllBytes($local, (New-RandomContent 12000))
        $service.ServeUpload('stalled.bin', 4096)
        $service.StallPart('stalled.bin', 2)
        $running = Start-Command -Connection $connection -Arguments $local -Script {
            param($Connection, $Local)
            Send-FilesFile -LiteralPath $Local -RemotePath 'stalled.bin' -Connection $Connection -Confirm:$false
        }
        Wait-Until { @($service.Requests | Where-Object Path -EQ '/upload/stalled.bin/2').Count -eq 1 -and $service.ActiveRequests -ge 1 }

        $elapsed = Measure-Command { $running.Shell.Stop() }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        Wait-Until { $service.AbortedRequests -ge 1 }
        $running.Shell.Dispose()
        @(Get-ApiRequest $service -Method POST -Path '/files/stalled.bin').Count | Should -Be 0
    }

    It 'stops an upload whose records are being captured, writes none after, and leaves another upload to complete' {
        $content = New-RandomContent 60000
        $local = Join-Path $TestDrive 'traced.bin'
        [System.IO.File]::WriteAllBytes($local, $content)
        $service.ServeUpload('stopped.bin', 4096)
        $service.StallPart('stopped.bin', 4)
        $service.ServeUpload('survivor.bin', 4096)
        $service.DelayParts('survivor.bin', 40)
        $upload = { param($Connection, $Local, $Remote) Send-FilesFile -LiteralPath $Local -RemotePath $Remote -Diagnostics -Connection $Connection -PassThru -Confirm:$false }
        $stopped = Start-Command -Connection $connection -Arguments $local, 'stopped.bin' -Script $upload
        Wait-Until { $stopped.Shell.Streams.Information.Count -ge 3 -and @($service.Requests | Where-Object Path -EQ '/upload/stopped.bin/4').Count -eq 1 }
        $survivor = Start-Command -Connection $connection -Arguments $local, 'survivor.bin' -Script $upload
        Wait-Until { @($service.Requests | Where-Object Path -Like '/upload/survivor.bin/*').Count -ge 1 }

        $elapsed = Measure-Command { $stopped.Shell.Stop() }
        $written = $stopped.Shell.Streams.Information.Count
        $result = $survivor.Shell.EndInvoke($survivor.Handle)

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $stopped.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        $stopped.Shell.Streams.Information.Count | Should -Be $written
        @($stopped.Shell.Streams.Information.MessageData | Where-Object { $_ -is [FilesCom.PowerShell.FilesUploadTraceSummary] }).Count | Should -Be 0
        @(Get-ApiRequest $service -Method POST -Path '/files/stopped.bin').Count | Should -Be 0
        $result.Path | Should -Be 'survivor.bin'
        Test-SameContent -Expected $content -Actual ($service.Uploaded('survivor.bin')) | Should -BeTrue
        $summary = @($survivor.Shell.Streams.Information.MessageData)[-1]
        $summary.Outcome | Should -Be 'Completed'
        $summary.Complete | Should -BeTrue
        $stopped.Shell.Dispose()
        $survivor.Shell.Dispose()
    }

    It 'cancels a download while its bytes arrive, and leaves no local or partial file' {
        $service.ServeDownload('reports/big.csv', (New-RandomContent 50000))
        $service.StallDownload('reports/big.csv', 1000)
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'cancelled-download')
        $destination = Join-Path $folder.FullName 'big.csv'
        $running = Start-Command -Connection $connection -Arguments $destination -Script {
            param($Connection, $Destination)
            Receive-FilesFile -RemotePath 'reports/big.csv' -LiteralPath $Destination -Connection $Connection -Confirm:$false
        }
        Wait-Until { @($service.Requests | Where-Object Path -Like '/download/*').Count -eq 1 -and $service.ActiveRequests -eq 1 }

        $elapsed = Measure-Command { $running.Shell.Stop() }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        Wait-Until { $service.AbortedRequests -eq 1 }
        $running.Shell.Dispose()
        # Without a checkpoint the engine keeps nothing: no file, and no stage or private folder.
        @(Get-ChildItem -LiteralPath $folder.FullName -Force) | Should -BeNullOrEmpty
    }

    It 'stops paging when stopped during a later page, after writing the earlier objects' {
        $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 1, 2), @{ 'X-Files-Cursor-Next' = 'next' })
        $service.Hang('GET', '/users')
        $running = Start-Command -Connection $connection -Script { param($Connection) Get-FilesUser -Connection $Connection }
        Wait-Until { @(Get-ApiRequest $service -Path '/users').Count -eq 2 -and $service.ActiveRequests -eq 1 }

        $running.Shell.Stop()

        $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        Wait-Until { $service.AbortedRequests -eq 1 }
        $running.Shell.Dispose()
    }

    It 'stops the operations of a connection when another runspace disconnects it' {
        $service.Hang('GET', '/users/1')
        $running = Start-Command -Connection $connection -Script { param($Connection) try { Get-FilesUser -Id 1 -Connection $Connection -ErrorAction Stop } catch { $_ } }
        Wait-Until { $service.ActiveRequests -eq 1 }

        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $result = @($running.Shell.EndInvoke($running.Handle))[0]

        $result.FullyQualifiedErrorId | Should -BeLike 'Files.ConnectionClosed*'
        # The connection closed after the command had started, which is what the message says.
        $result.Exception.Message | Should -Match 'was closed while the command ran'
        Wait-Until { $service.AbortedRequests -eq 1 }
        $running.Shell.Dispose()
    }

    It 'bounds the whole command by the operation timeout, across its pages, not each request' {
        $bounded = Connect-FilesAccount -ApiKey (ConvertTo-SecureString $script:ApiKeySentinel -AsPlainText -Force) -BaseUrl $service.BaseUrl -MaxNetworkRetries 0 -OperationTimeout ([TimeSpan]::FromMilliseconds(600)) -NoDefault
        try {
            $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 1), @{ 'X-Files-Cursor-Next' = 'two' }, 400)
            $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 2), @{ 'X-Files-Cursor-Next' = 'three' }, 400)
            $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 3), $null, 400)
            $written = [System.Collections.Generic.List[object]]::new()

            $elapsed = Measure-Command {
                { Get-FilesUser -Connection $bounded -ErrorAction Stop | ForEach-Object { $written.Add($_) } } | Should -Throw -ErrorId 'Files.OperationTimeout*'
            }

            $written.Id | Should -Be @(1)
            $elapsed.TotalMilliseconds | Should -BeLessThan 1200
            @(Get-ApiRequest $service -Path '/users').Count | Should -BeLessOrEqual 2
        }
        finally {
            Disconnect-FilesAccount -Connection $bounded -Confirm:$false
        }
    }

    It 'stops a command that runs longer than the connection''s operation timeout' {
        $service.Hang('GET', '/users/1')
        $bounded = Connect-TestService -Service $service -Options @{ OperationTimeout = [TimeSpan]::FromMilliseconds(300) }

        $elapsed = Measure-Command { { Get-FilesUser -Id 1 -Connection $bounded -ErrorAction Stop } | Should -Throw -ErrorId 'Files.OperationTimeout*' }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        Disconnect-FilesAccount -Connection $bounded -Confirm:$false
    }
}
