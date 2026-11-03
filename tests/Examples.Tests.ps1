BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
    # The scripts as the installed module ships them.
    $examples = Join-Path -Path (Split-Path -Parent $script:ModuleManifest) -ChildPath 'examples'
}

Describe 'The shipped examples' {
    It 'are installed with the module, each with its help' {
        foreach ($name in 'Export-UserAccessInventory.ps1', 'Add-FilesProjectMember.ps1', 'Send-FilesNightlyExport.ps1', 'Send-FilesLiveExport.ps1', 'Set-FilesSiteBranding.ps1', 'Export-FilesUploadDiagnostics.ps1', 'README.md') {
            Test-Path -LiteralPath (Join-Path $examples $name) | Should -BeTrue -Because "$name ships with the module"
        }
        foreach ($script in Get-ChildItem -LiteralPath $examples -Filter '*.ps1') {
            (Get-Help $script.FullName).Synopsis | Should -Not -BeNullOrEmpty -Because $script.Name
        }
    }
}

Describe 'Export-UserAccessInventory.ps1' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'exports every user of every page with their direct and group permissions' {
        $service.Pages('/users', '[{"id":1,"username":"ana","email":"ana@example.com","site_admin":true}]', '[{"id":2,"username":"ben","disabled":true}]')
        $service.Pages('/group_users', '[{"group_id":10,"group_name":"Finance","user_id":2}]')
        $service.Pages('/permissions', '[{"id":1,"path":"shared","user_id":1,"permission":"full"}]', '[{"id":2,"path":"finance","group_id":10,"group_name":"Finance","permission":"readonly"}]')
        $path = Join-Path $TestDrive 'inventory.csv'

        & (Join-Path $examples 'Export-UserAccessInventory.ps1') -Path $path -Connection $connection

        $rows = Import-Csv -LiteralPath $path
        $rows.Username | Should -Be @('ana', 'ben')
        $rows[0].Permissions | Should -Be 'shared:full'
        $rows[0].SiteAdmin | Should -Be 'True'
        $rows[1].Groups | Should -Be 'Finance'
        $rows[1].Permissions | Should -Be 'finance:readonly'
        $rows[1].Disabled | Should -Be 'True'
    }

    It 'leaves the previous export and an unrelated .partial file as they were when a page fails' {
        $service.Respond('GET', '/users', 200, '[{"id":1,"username":"ana"}]', @{ 'X-Files-Cursor-Next' = 'next' })
        $service.Respond('GET', '/users', 500, 'not json')
        $service.Pages('/group_users', '[]')
        $service.Pages('/permissions', '[]')
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'failed-export')
        $path = Join-Path $folder.FullName 'inventory.csv'
        Set-Content -LiteralPath $path -Value 'PREVIOUS-EXPORT'
        Set-Content -LiteralPath "$path.partial" -Value 'SOMEONE-ELSES-FILE'

        { & (Join-Path $examples 'Export-UserAccessInventory.ps1') -Path $path -Connection $connection } | Should -Throw -ErrorId 'Files.InvalidResponse*'

        Get-Content -LiteralPath $path | Should -Be 'PREVIOUS-EXPORT'
        Get-Content -LiteralPath "$path.partial" | Should -Be 'SOMEONE-ELSES-FILE'
        @(Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('inventory.csv', 'inventory.csv.partial')
    }

    It 'replaces the previous export when every page was read, leaving an unrelated .partial file alone' {
        $service.Pages('/users', '[{"id":1,"username":"ana"}]')
        $service.Pages('/group_users', '[]')
        $service.Pages('/permissions', '[]')
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'replaced-export')
        $path = Join-Path $folder.FullName 'inventory.csv'
        Set-Content -LiteralPath $path -Value 'PREVIOUS-EXPORT'
        Set-Content -LiteralPath "$path.partial" -Value 'SOMEONE-ELSES-FILE'

        & (Join-Path $examples 'Export-UserAccessInventory.ps1') -Path $path -Connection $connection

        (Import-Csv -LiteralPath $path).Username | Should -Be 'ana'
        Get-Content -LiteralPath "$path.partial" | Should -Be 'SOMEONE-ELSES-FILE'
        @(Get-ChildItem -LiteralPath $folder.FullName -Force).Name | Should -Be @('inventory.csv', 'inventory.csv.partial')
    }
}

Describe 'Add-FilesProjectMember.ps1' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $members = Join-Path $TestDrive 'members.csv'
        @(
            [pscustomobject]@{ Username = 'ana'; Email = 'ana@example.com'; Folder = 'projects/apollo/ana'; Permission = 'full' }
            [pscustomobject]@{ Username = 'ben'; Email = 'ben@example.com'; Folder = 'projects/apollo/ben'; Permission = 'readonly' }
        ) | Export-Csv -LiteralPath $members -NoTypeInformation
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'previews every row without contacting Files.com at all, and says what it could not check' {
        $service.Pages('/users', '[{"id":7,"username":"ben"}]')

        $results = & (Join-Path $examples 'Add-FilesProjectMember.ps1') -Path $members -Connection $connection -WhatIf

        $results.Status | Should -Be @('Previewed', 'Previewed')
        $results[0].Note | Should -Match 'without contacting Files.com'
        $results[0].Note | Should -Match 'not checked'
        $service.Requests.Count | Should -Be 0
    }

    It 'provisions each row, reuses an existing user, and reports a failed row while provisioning the others' {
        $service.Respond('GET', '/users', 200, '[]')
        $service.Respond('GET', '/users', 200, '[{"id":7,"username":"ben"}]')
        $service.Respond('POST', '/users', 200, '{"id":6,"username":"ana"}')
        $service.Respond('POST', '/folders/projects/apollo/ana', 200, '{"path":"projects/apollo/ana","type":"directory"}')
        $service.Respond('POST', '/folders/projects/apollo/ben', 422, '{"error":"Folder is locked","http-code":422,"type":"processing-failure/folder-locked"}')
        $service.Respond('GET', '/permissions', 200, '[]')
        $service.Respond('POST', '/permissions', 200, '{"id":1,"path":"projects/apollo/ana","user_id":6,"permission":"full"}')

        $results = & (Join-Path $examples 'Add-FilesProjectMember.ps1') -Path $members -Connection $connection -Confirm:$false

        $results[0].Status | Should -Be 'Provisioned'
        $results[1].Status | Should -Be 'Failed'
        $results[1].Error | Should -Match 'Files.Conflict'
        (Get-ApiRequest $service -Method POST -Path '/users').Count | Should -Be 1
        ((Get-ApiRequest $service -Method POST -Path '/permissions').Body | ConvertFrom-Json).user_id | Should -Be 6
        (Get-ApiRequest $service -Method POST -Path '/folders/projects/apollo/ana').Body | Should -Match '"mkdir_parents":true'
    }

    It 'reuses the user, folder and permission an earlier run provisioned when run again' {
        $service.Respond('GET', '/users', 200, '[{"id":6,"username":"ana"}]')
        $service.Respond('GET', '/users', 200, '[{"id":7,"username":"ben"}]')
        $service.Respond('POST', '/folders/projects/apollo/ana', 422, '{"error":"Destination exists","http-code":422,"type":"processing-failure/destination-exists"}')
        $service.Respond('POST', '/folders/projects/apollo/ben', 422, '{"error":"Destination exists","http-code":422,"type":"processing-failure/destination-exists"}')
        $service.Respond('GET', '/permissions', 200, '[{"id":1,"path":"projects/apollo/ana","user_id":6,"permission":"full"}]')
        $service.Respond('GET', '/permissions', 200, '[{"id":2,"path":"projects/apollo","user_id":7,"permission":"readonly"}]')
        $service.Respond('POST', '/permissions', 200, '{"id":3,"path":"projects/apollo/ben","user_id":7,"permission":"readonly"}')

        $results = & (Join-Path $examples 'Add-FilesProjectMember.ps1') -Path $members -Connection $connection -Confirm:$false

        $results.Status | Should -Be @('Provisioned', 'Provisioned')
        (Get-ApiRequest $service -Method POST -Path '/users').Count | Should -Be 0
        $grants = @(Get-ApiRequest $service -Method POST -Path '/permissions')
        $grants.Count | Should -Be 1
        ($grants[0].Body | ConvertFrom-Json).path | Should -Be 'projects/apollo/ben'
        (Get-ApiRequest $service -Method GET -Path '/permissions')[1].Query['user_id'] | Should -Be '7'
    }
}

Describe 'Send-FilesNightlyExport.ps1' {
    BeforeEach {
        $service = New-TestService
        $folder = New-Item -ItemType Directory -Path (Join-Path $TestDrive "exports-$([guid]::NewGuid())")
        $contents = @{}
        foreach ($name in 'a.csv', 'b.csv', 'c.csv') {
            $bytes = [byte[]]::new(9000)
            [System.Random]::new($name.GetHashCode()).NextBytes($bytes)
            [System.IO.File]::WriteAllBytes((Join-Path $folder.FullName $name), $bytes)
            $contents[$name] = $bytes
        }
        $apiKey = ConvertTo-SecureString $script:ApiKeySentinel -AsPlainText -Force
        $global:LASTEXITCODE = 0
    }
    AfterEach {
        $service.Dispose()
    }

    It 'confirms an uncertain completion only by checksum, reports an old same-size file as uncertain, never uploads again, and fails the run' {
        $sha256 = (Get-FileHash -LiteralPath (Join-Path $folder.FullName 'b.csv') -Algorithm SHA256).Hash.ToLowerInvariant()
        $service.ServeUpload('nightly/a.csv', 4096)
        $service.ServeUpload('nightly/b.csv', 4096, $true, 0)
        $service.ServeUpload('nightly/c.csv', 4096, $true, 0)
        $service.Respond('GET', '/file_actions/metadata/nightly/b.csv', 200, "{""path"":""nightly/b.csv"",""type"":""file"",""size"":9000,""sha256"":""$sha256""}")
        # An older version with the same size and no checksum yet: not proof that this upload finished.
        $service.Respond('GET', '/file_actions/metadata/nightly/c.csv', 200, '{"path":"nightly/c.csv","type":"file","size":9000,"mtime":"2020-01-01T00:00:00Z"}')

        $results = & (Join-Path $examples 'Send-FilesNightlyExport.ps1') -ApiKey $apiKey -BaseUrl $service.BaseUrl -LocalFolder $folder.FullName -RemoteFolder 'nightly'
        $exitCode = $LASTEXITCODE

        ($results | Sort-Object File).Status | Should -Be @('Uploaded', 'Uploaded (confirmed by checksum)', 'Uncertain')
        ($results | Where-Object File -EQ 'c.csv').Reason | Should -Match 'not reported a checksum'
        $exitCode | Should -Be 1
        Test-SameContent -Expected $contents['a.csv'] -Actual ($service.Uploaded('nightly/a.csv')) | Should -BeTrue
        foreach ($name in 'a.csv', 'b.csv', 'c.csv') {
            # A new upload begins without a ref; later requests for more parts name the upload's ref.
            @(Get-ApiRequest $service -Method POST -Path "/file_actions/begin_upload/nightly/$name" | Where-Object Body -NotMatch '"ref":').Count | Should -Be 1 -Because "$name is uploaded once"
            @(Get-ApiRequest $service -Method POST -Path "/files/nightly/$name").Count | Should -Be 1 -Because "$name is finished once"
        }
    }

    It 'reports an uncertain completion it cannot check, or whose checksum differs, as uncertain' {
        Remove-Item -LiteralPath (Join-Path $folder.FullName 'c.csv')
        $service.ServeUpload('nightly/a.csv', 4096, $true, 0)
        $service.ServeUpload('nightly/b.csv', 4096, $true, 0)
        $service.Respond('GET', '/file_actions/metadata/nightly/a.csv', 500, 'not json')
        $service.Respond('GET', '/file_actions/metadata/nightly/b.csv', 200, '{"path":"nightly/b.csv","type":"file","size":9000,"sha256":"0000000000000000000000000000000000000000000000000000000000000000"}')

        $results = & (Join-Path $examples 'Send-FilesNightlyExport.ps1') -ApiKey $apiKey -BaseUrl $service.BaseUrl -LocalFolder $folder.FullName -RemoteFolder 'nightly'

        ($results | Sort-Object File).Status | Should -Be @('Uncertain', 'Uncertain')
        ($results | Where-Object File -EQ 'a.csv').Reason | Should -Match 'could not be read'
        ($results | Where-Object File -EQ 'b.csv').Reason | Should -Match 'SHA256 checksum differs'
        $LASTEXITCODE | Should -Be 1
    }

    It 'exits with code 0 when every file was uploaded' {
        foreach ($name in 'a.csv', 'b.csv', 'c.csv') {
            $service.ServeUpload("nightly/$name", 4096)
        }

        $results = & (Join-Path $examples 'Send-FilesNightlyExport.ps1') -ApiKey $apiKey -BaseUrl $service.BaseUrl -LocalFolder $folder.FullName -RemoteFolder 'nightly'

        $results.Status | Should -Be @('Uploaded', 'Uploaded', 'Uploaded')
        $LASTEXITCODE | Should -Be 0
    }
}

Describe 'Send-FilesLiveExport.ps1' {
    BeforeAll {
        # A program that writes Count deterministic bytes to its standard output, then exits with ExitCode, run by this
        # same PowerShell. With a marker path, it first creates that file; with a PID file, it first writes its process
        # ID there; with HoldSeconds, it keeps running, its output open, that long after writing.
        function Get-ProducerArgument {
            param([int] $Count, [int] $ExitCode = 0, [string] $Marker, [string] $PidFile, [int] $HoldSeconds)
            $script = "`$b = [byte[]]::new($Count); [System.Random]::new(7).NextBytes(`$b); "
            if ($Marker) { $script += "Set-Content -LiteralPath '$Marker' -Value started; " }
            if ($PidFile) { $script += "[System.IO.File]::WriteAllText('$PidFile', [string]`$PID); " }
            $script += "`$o = [Console]::OpenStandardOutput(); `$o.Write(`$b, 0, `$b.Length); `$o.Flush(); "
            if ($HoldSeconds) { $script += "Start-Sleep -Seconds $HoldSeconds; " }
            $script += "exit $ExitCode"
            '-NoProfile -NonInteractive -EncodedCommand ' + [Convert]::ToBase64String([System.Text.Encoding]::Unicode.GetBytes($script))
        }
        function Get-ProducerOutput {
            param([int] $Count)
            $bytes = [byte[]]::new($Count)
            [System.Random]::new(7).NextBytes($bytes)
            $bytes
        }
        $producer = (Get-Process -Id $PID).Path
    }
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $temporary = (New-Item -ItemType Directory -Path (Join-Path $TestDrive "parts-$([guid]::NewGuid().ToString('N'))")).FullName
        $service.ServeUpload('backups/sales.dump.partial', 4096)
        $service.Respond('POST', '/file_actions/move/backups/sales.dump.partial', 200, '{"status":"completed"}')
        $service.Respond('DELETE', '/files/backups/sales.dump.partial', 204, '')
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'uploads a program''s output as it is written, within -MaxLength, and publishes it when the program succeeds' {
        $result = & (Join-Path $examples 'Send-FilesLiveExport.ps1') -FilePath $producer -Arguments (Get-ProducerArgument -Count 20000) -RemotePath 'backups/sales.dump' -MaxLength 100000 -TemporaryDirectory $temporary -Connection $connection

        $result.Status | Should -Be 'Published'
        $result.Size | Should -Be 20000
        $result.ServerPlannedReason | Should -Be 'LengthUnknown'
        Test-SameContent -Expected (Get-ProducerOutput -Count 20000) -Actual ($service.Uploaded('backups/sales.dump.partial')) | Should -BeTrue
        (Get-ApiRequest $service -Method POST -Path '/file_actions/begin_upload/backups/sales.dump.partial')[0].Body | Should -Not -Match '"size"'
        $move = @(Get-ApiRequest $service -Method POST -Path '/file_actions/move/backups/sales.dump.partial')
        $move.Count | Should -Be 1
        ($move[0].Body | ConvertFrom-Json).destination | Should -Be 'backups/sales.dump'
        ($move[0].Body | ConvertFrom-Json).overwrite | Should -BeTrue
        @(Get-ApiRequest $service -Method DELETE).Count | Should -Be 0
        @(Get-ChildItem -LiteralPath $temporary -Force).Count | Should -Be 0
    }

    It 'deletes the uploaded output instead of publishing it when the program fails' {
        { & (Join-Path $examples 'Send-FilesLiveExport.ps1') -FilePath $producer -Arguments (Get-ProducerArgument -Count 9000 -ExitCode 3) -RemotePath 'backups/sales.dump' -MaxLength 100000 -Connection $connection } | Should -Throw '*exited with code 3*'

        @(Get-ApiRequest $service -Method DELETE -Path '/files/backups/sales.dump.partial').Count | Should -Be 1
        @(Get-ApiRequest $service -Method POST -Path '/file_actions/move/backups/sales.dump.partial').Count | Should -Be 0
    }

    It 'stops the upload and the program when the output is longer than -MaxLength, creating nothing' {
        { & (Join-Path $examples 'Send-FilesLiveExport.ps1') -FilePath $producer -Arguments (Get-ProducerArgument -Count 50000) -RemotePath 'backups/sales.dump' -MaxLength 10000 -TemporaryDirectory $temporary -Connection $connection } | Should -Throw -ErrorId 'Files.LimitExceeded*'

        @(Get-ApiRequest $service -Method POST -Path '/files/backups/sales.dump.partial').Count | Should -Be 0
        @(Get-ApiRequest $service -Method POST -Path '/file_actions/move/backups/sales.dump.partial').Count | Should -Be 0
        @(Get-ChildItem -LiteralPath $temporary -Force).Count | Should -Be 0
    }

    It 'stops the program it started, and completes and publishes nothing, when its pipeline is stopped' {
        $pidFile = Join-Path $TestDrive "producer-$([guid]::NewGuid().ToString('N')).pid"
        $arguments = Get-ProducerArgument -Count 10000 -PidFile $pidFile -HoldSeconds 60
        $running = Start-Command -Connection $connection -Arguments (Join-Path $examples 'Send-FilesLiveExport.ps1'), $producer, $arguments, $temporary -Script {
            param($Connection, $Example, $Producer, $Arguments, $Temporary)
            & $Example -FilePath $Producer -Arguments $Arguments -RemotePath 'backups/sales.dump' -MaxLength 100000 -TemporaryDirectory $Temporary -Connection $Connection -Confirm:$false
        }
        $producerId = 0
        try {
            Wait-Until { (Test-Path -LiteralPath $pidFile) -and ([System.IO.File]::ReadAllText($pidFile) -match '^\d+$') }
            $producerId = [int][System.IO.File]::ReadAllText($pidFile)
            # The program has written its output and keeps it open; the upload has sent the full parts and waits for more.
            Wait-Until { @($service.Requests | Where-Object { [uri]::UnescapeDataString($_.Path) -like '/upload/backups/sales.dump.partial/*' }).Count -ge 2 }
            Get-Process -Id $producerId -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty

            $elapsed = Measure-Command { $running.Shell.Stop() }

            $elapsed.TotalSeconds | Should -BeLessThan 5
            $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
            Get-Process -Id $producerId -ErrorAction SilentlyContinue | Should -BeNullOrEmpty
            @(Get-ApiRequest $service -Method POST -Path '/files/backups/sales.dump.partial').Count | Should -Be 0
            @(Get-ApiRequest $service -Method POST -Path '/file_actions/move/backups/sales.dump.partial').Count | Should -Be 0
            @(Get-ChildItem -LiteralPath $temporary -Force).Count | Should -Be 0
        }
        finally {
            $running.Shell.Dispose()
            # A program the script failed to stop is stopped and reaped here, so that no test leaves one running.
            $left = if ($producerId) { Get-Process -Id $producerId -ErrorAction SilentlyContinue }
            if ($left) {
                $left.Kill()
                $left.WaitForExit()
                $left.Dispose()
            }
        }
    }

    It 'starts nothing and sends nothing under -WhatIf' {
        $marker = Join-Path $TestDrive 'producer-started.txt'

        & (Join-Path $examples 'Send-FilesLiveExport.ps1') -FilePath $producer -Arguments (Get-ProducerArgument -Count 100 -Marker $marker) -RemotePath 'backups/sales.dump' -MaxLength 100000 -Connection $connection -WhatIf

        Test-Path -LiteralPath $marker | Should -BeFalse
        $service.Requests.Count | Should -Be 0
    }
}

Describe 'Set-FilesSiteBranding.ps1' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $folder = (New-Item -ItemType Directory -Path (Join-Path $TestDrive "branding-$([guid]::NewGuid().ToString('N'))")).FullName
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'sends every image it finds, with its media type, in one request, and nothing for the branding it has no image for' {
        $logo = New-RandomContent 2000
        $icon = New-RandomContent 300
        $background = New-RandomContent 5000
        [System.IO.File]::WriteAllBytes((Join-Path $folder 'logo.png'), $logo)
        [System.IO.File]::WriteAllBytes((Join-Path $folder 'icon16.ico'), $icon)
        [System.IO.File]::WriteAllBytes((Join-Path $folder 'login-background.jpg'), $background)
        Set-Content -LiteralPath (Join-Path $folder 'notes.txt') -Value 'not an image'
        $service.Respond('PATCH', '/site', 200, '{"name":"Acme"}')

        $result = & (Join-Path $examples 'Set-FilesSiteBranding.ps1') -Path $folder -Connection $connection -Confirm:$false

        $sent = @(Get-ApiRequest $service -Method PATCH -Path '/site')
        $sent.Count | Should -Be 1
        $parts = @($sent[0].FormParts() | Where-Object FileName)
        ($parts.Name | Sort-Object) | Should -Be @('icon16_file', 'login_page_background_image_file', 'logo_file')
        $expectedParts = @(
            @{ Name = 'logo_file'; FileName = 'logo.png'; ContentType = 'image/png'; Content = $logo }
            @{ Name = 'icon16_file'; FileName = 'icon16.ico'; ContentType = 'image/x-icon'; Content = $icon }
            @{ Name = 'login_page_background_image_file'; FileName = 'login-background.jpg'; ContentType = 'image/jpeg'; Content = $background }
        )
        foreach ($expected in $expectedParts) {
            $part = $parts | Where-Object Name -EQ $expected.Name
            $part.FileName | Should -Be $expected.FileName
            $part.ContentType | Should -Be $expected.ContentType
            Test-SameContent -Expected $expected.Content -Actual $part.Content | Should -BeTrue -Because $expected.Name
        }
        $result.Image | Should -Be @('logo', 'icon16', 'login-background')
        $result.Status | Should -Be @('Applied', 'Applied', 'Applied')
    }

    It 'previews under -WhatIf without contacting Files.com, and sends nothing when an image is doubled or not an image' {
        [System.IO.File]::WriteAllBytes((Join-Path $folder 'logo.png'), (New-RandomContent 100))

        $preview = & (Join-Path $examples 'Set-FilesSiteBranding.ps1') -Path $folder -Connection $connection -WhatIf
        [System.IO.File]::WriteAllBytes((Join-Path $folder 'logo.svg'), (New-RandomContent 100))
        { & (Join-Path $examples 'Set-FilesSiteBranding.ps1') -Path $folder -Connection $connection -Confirm:$false } | Should -Throw '*more than one logo image (logo.png, logo.svg)*'
        Remove-Item -LiteralPath (Join-Path $folder 'logo.svg')
        [System.IO.File]::WriteAllBytes((Join-Path $folder 'icon32.bmp'), (New-RandomContent 100))
        { & (Join-Path $examples 'Set-FilesSiteBranding.ps1') -Path $folder -Connection $connection -Confirm:$false } | Should -Throw '*icon32.bmp is not an image this script sends*'

        $preview.Image | Should -Be 'logo'
        $preview.Status | Should -Be 'Previewed'
        $service.Requests.Count | Should -Be 0
    }
}

Describe 'Export-FilesUploadDiagnostics.ps1' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service -Options @{ MaxNetworkRetries = 1 }
        # The same site in memory, where the parts of uploads given SendPartsToS3 go straight to Amazon S3.
        $inMemory = Connect-TestService -Service $service -InMemory -Options @{ MaxNetworkRetries = 1 }
        $content = New-RandomContent 100000
        $folder = (New-Item -ItemType Directory -Path (Join-Path $TestDrive "diagnostics-$([guid]::NewGuid().ToString('N'))")).FullName
        $local = Join-Path $folder 'data.bin'
        [System.IO.File]::WriteAllBytes($local, $content)
        $report = Join-Path $folder 'report.json'
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $inMemory -Confirm:$false
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'exports the upload''s throttling, retried parts and part sizes to a report holding only the fields it names' {
        # 12 MiB goes to S3 in parts the engine sizes, at URLs presigned with PartSignature.
        $content = New-RandomContent 12MB
        [System.IO.File]::WriteAllBytes($local, $content)
        $service.ServeUpload('diag/data.bin', 8192)
        $service.SendPartsToS3('diag/data.bin')
        $service.FailPart('diag/data.bin', 2, 503)
        $patient = New-FilesAutomaticConcurrency -Profile Generic -BackpressurePauseSeconds 0.2
        # A report from an earlier run is replaced; a file of someone else's beside it is not touched.
        [System.IO.File]::WriteAllText($report, 'an earlier report')
        [System.IO.File]::WriteAllText("$report.partial", 'not the script''s')

        $result = & (Join-Path $examples 'Export-FilesUploadDiagnostics.ps1') -Path $local -RemotePath 'diag/data.bin' -OutputPath $report -AutomaticConcurrency $patient -Connection $inMemory -Confirm:$false

        $text = Get-Content -LiteralPath $report -Raw
        $exported = $text | ConvertFrom-Json
        $exported.Outcome | Should -Be 'Completed'
        $exported.PerformanceProfileSource | Should -Be 'Options'
        $exported.Throttles | Should -Be 1
        $exported.Decisions.CooldownStarted | Should -Be 1
        $exported.CooldownSeconds | Should -Be 0.2
        $exported.RetriedParts.Count | Should -Be 1
        $exported.RetriedParts[0].PartNumber | Should -Be 2
        $exported.RetriedParts[0].Attempts.StatusCode | Should -Be @(503, 200)
        $exported.PartSizes[0].Decision | Should -Be 'Chosen'
        $exported.Records.Complete | Should -BeTrue
        @($exported.PSObject.Properties.Name) | Should -Be @('RemotePath', 'Outcome', 'ErrorId', 'Size', 'ElapsedSeconds', 'ThroughputBytesPerSecond', 'PeakConcurrency', 'PerformanceProfile', 'PerformanceProfileSource', 'PartSizeConstraint', 'LargestPartSize', 'Throttles', 'PartRetries', 'ReadyLimitWaits', 'Decisions', 'CooldownSeconds', 'PartSizes', 'RetriedParts', 'Records')
        $text | Should -Not -Match ([regex]::Escape([FilesCom.PowerShell.Tests.FilesTestService]::PartSignature))
        $text | Should -Not -Match ([regex]::Escape($script:ApiKeySentinel))
        $text | Should -Not -Match '/upload/'
        $result.Throttles | Should -Be 1
        $result.Report | Should -Be $report
        Test-SameContent -Expected $content -Actual ($service.Uploaded('diag/data.bin')) | Should -BeTrue
        [System.IO.File]::ReadAllText("$report.partial") | Should -Be 'not the script''s'
        @(Get-ChildItem -LiteralPath $folder -Force | ForEach-Object Name | Sort-Object) | Should -Be @('data.bin', 'report.json', 'report.json.partial')
    }

    It 'reports an upload that fails before failing with its error, and writes nothing under -WhatIf' {
        $service.ServeUpload('diag/refused.bin', 8192)
        $service.FailPart('diag/refused.bin', 2, 400)
        $preview = Join-Path $folder 'preview.json'

        { & (Join-Path $examples 'Export-FilesUploadDiagnostics.ps1') -Path $local -RemotePath 'diag/refused.bin' -OutputPath $report -Connection $connection -Confirm:$false } | Should -Throw -ErrorId 'Files.InvalidResponse*'
        & (Join-Path $examples 'Export-FilesUploadDiagnostics.ps1') -Path $local -RemotePath 'diag/preview.bin' -OutputPath $preview -Connection $connection -WhatIf

        $exported = Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
        $exported.Outcome | Should -Be 'Failed'
        $exported.ErrorId | Should -BeLike 'Files.InvalidResponse*'
        $exported.Size | Should -BeNullOrEmpty
        Test-Path -LiteralPath $preview | Should -BeFalse
        @(Get-ApiRequest $service -Path '/file_actions/begin_upload/diag/preview.bin').Count | Should -Be 0
    }

    It 'keeps the upload''s own error when the report cannot be written, and fails a completed upload whose report cannot be written with its result' {
        $service.ServeUpload('diag/uncertain.bin', 8192, $true, 0)
        $service.ServeUpload('diag/stored.bin', 8192)
        $unwritable = Join-Path $folder 'missing/report.json'

        $caught = $null
        try {
            & (Join-Path $examples 'Export-FilesUploadDiagnostics.ps1') -Path $local -RemotePath 'diag/uncertain.bin' -OutputPath $unwritable -Connection $connection -Confirm:$false -ErrorAction Stop -ErrorVariable problems 2>$null
        }
        catch {
            $caught = $_
        }
        $caught.FullyQualifiedErrorId | Should -BeLike 'Files.TransferCompletionUnknown*'
        @($problems | Where-Object { $_.FullyQualifiedErrorId -like 'ReportNotWritten*' }).Count | Should -Be 1

        $caught = $null
        try {
            & (Join-Path $examples 'Export-FilesUploadDiagnostics.ps1') -Path $local -RemotePath 'diag/stored.bin' -OutputPath $unwritable -Connection $connection -Confirm:$false
        }
        catch {
            $caught = $_
        }
        $caught.FullyQualifiedErrorId | Should -BeLike 'ReportNotWritten*'
        $caught.TargetObject.Path | Should -Be 'diag/stored.bin'
        $caught.TargetObject.Size | Should -Be $content.Length
        Test-SameContent -Expected $content -Actual ($service.Uploaded('diag/stored.bin')) | Should -BeTrue
        Test-Path -LiteralPath (Split-Path -Parent $unwritable) | Should -BeFalse
    }
}
