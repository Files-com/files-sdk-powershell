BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')

    # The JSON body of the one request the service received for method and path.
    function Get-Body {
        param($Service, $Method, $Path)
        $request = @(Get-ApiRequest $Service -Method $Method -Path $Path)
        $request.Count | Should -Be 1
        $request[0].Body | ConvertFrom-Json -AsHashtable
    }
}

Describe 'Request values' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $service.Respond('PATCH', '/users/1', 200, '{"id":1,"username":"u"}')
        $service.Respond('POST', '/users', 200, '{"id":2,"username":"new"}')
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'refuses a fractional or out-of-range whole number before sending anything' -TestCases @(
        @{ Value = 2.5 }, @{ Value = '2.5' }, @{ Value = [decimal]'9223372036854775808' }, @{ Value = 'ten' }
    ) {
        { Get-FilesUser -Id $Value -Connection $connection -ErrorAction Stop } | Should -Throw
        $service.Requests.Count | Should -Be 0
    }

    It 'accepts a whole number given as text or a whole double' {
        $service.Respond('GET', '/users/3', 200, '{"id":3}')
        (Get-FilesUser -Id '3' -Connection $connection).Id | Should -Be 3
        (Get-FilesUser -Id 3.0 -Connection $connection).Id | Should -Be 3
    }

    It 'sends an explicit $false, and nothing for an omitted Boolean' {
        Set-FilesUser -Id 1 -Disabled $false -Connection $connection -Confirm:$false | Out-Null

        $body = Get-Body -Service $service -Method PATCH -Path '/users/1'
        $body['disabled'] | Should -BeExactly $false
        $body.ContainsKey('site_admin') | Should -BeFalse
        $body.ContainsKey('name') | Should -BeFalse
    }

    It 'sends empty text, and refuses an explicit $null before sending anything' {
        Set-FilesUser -Id 1 -Name '' -Connection $connection -Confirm:$false | Out-Null
        (Get-Body -Service $service -Method PATCH -Path '/users/1')['name'] | Should -BeExactly ''

        $service.ClearRequests()
        { Set-FilesUser -Id 1 -Name $null -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw
        $service.Requests.Count | Should -Be 0
    }

    It 'keeps every digit of a decimal given as text, a decimal or a double, sending it as exact text' -TestCases @(
        @{ Value = '123456789.123456789123'; Expected = '123456789.123456789123' }
        @{ Value = '12.50'; Expected = '12.50' }
        @{ Value = 12.50d; Expected = '12.50' }
        @{ Value = 0.1; Expected = '0.1' }
    ) {
        $service.Respond('PATCH', '/remote_mount_backends/4', 200, '{"id":4,"min_free_cpu":"123456789.123456789123"}')

        $backend = Set-FilesRemoteMountBackend -Id 4 -MinFreeCpu $Value -Connection $connection -Confirm:$false

        (Get-ApiRequest $service -Method PATCH -Path '/remote_mount_backends/4').Body | Should -Match ('"min_free_cpu":"' + [regex]::Escape($Expected) + '"')
        $backend.MinFreeCpu | Should -BeOfType ([decimal])
        $backend.MinFreeCpu | Should -Be ([decimal]::Parse('123456789.123456789123', [cultureinfo]::InvariantCulture))
    }

    It 'writes a decimal the response left out as $null rather than failing' {
        $service.Respond('GET', '/remote_mount_backends/4', 200, '{"id":4}')

        (Get-FilesRemoteMountBackend -Id 4 -Connection $connection).MinFreeMem | Should -BeNullOrEmpty
    }

    It 'sends a date with its instant and offset' -TestCases @(
        @{ Value = [datetimeoffset]'2026-01-31T09:00:00-05:00'; Expected = '2026-01-31T09:00:00-05:00' }
        @{ Value = [datetime]::SpecifyKind([datetime]'2026-01-31T14:00:00', 'Utc'); Expected = '2026-01-31T14:00:00Z' }
        @{ Value = '2026-01-31T14:00:00.25Z'; Expected = '2026-01-31T14:00:00.25Z' }
    ) {
        Set-FilesUser -Id 1 -AuthenticateUntil $Value -Connection $connection -Confirm:$false | Out-Null

        (Get-ApiRequest $service -Method PATCH -Path '/users/1').Body | Should -Match ('"authenticate_until":"' + [regex]::Escape($Expected) + '"')
    }

    It 'refuses a date that does not say whether it is UTC or local time' -TestCases @(
        @{ Value = [datetime]'2026-01-31T14:00:00' }, @{ Value = '2026-01-31T14:00:00' }, @{ Value = '2026-01-31' }
    ) {
        { Set-FilesUser -Id 1 -AuthenticateUntil $Value -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw
        $service.Requests.Count | Should -Be 0
    }

    It 'sends an object parameter with its keys'' case and every value, from a hashtable or PSCustomObject' {
        $service.Respond('POST', '/automations', 200, '{"id":9}')
        # PowerShell hashtables fold key case, so keys differing only in case need an ordinal dictionary.
        $definition = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
        $definition['Nodes'] = @([pscustomobject]@{ id = 'a'; retries = 0 })
        $definition['nodes'] = 'lower'
        $definition['enabled'] = $false
        $definition['label'] = ''
        $definition['missing'] = $null

        New-FilesAutomation -Automation 'create_folder' -Definition $definition -ScheduleDaysOfWeek 1, 3 -Connection $connection -Confirm:$false | Out-Null

        $body = (Get-ApiRequest $service -Method POST -Path '/automations').Body | ConvertFrom-Json -AsHashtable
        $body['definition'].Keys | Should -Contain 'Nodes'
        $body['definition'].Keys | Should -Contain 'nodes'
        $body['definition']['Nodes'][0]['retries'] | Should -BeExactly 0
        $body['definition']['enabled'] | Should -BeExactly $false
        $body['definition']['label'] | Should -BeExactly ''
        $body['definition'].ContainsKey('missing') | Should -BeTrue
        $body['definition']['missing'] | Should -BeNullOrEmpty
        $body['schedule_days_of_week'] | Should -Be @(1, 3)
    }

    It 'sends an object parameter of a list as query pairs' {
        $service.Respond('GET', '/users', 200, '[]')

        Get-FilesUser -Filter @{ username = 'al b'; site_admin = $true } -SortBy @{ username = 'desc' } -Connection $connection | Out-Null

        $query = (Get-ApiRequest $service -Path '/users').Query
        $query['filter[username]'] | Should -BeExactly 'al b'
        $query['filter[site_admin]'] | Should -BeExactly 'true'
        $query['sort_by[username]'] | Should -BeExactly 'desc'
    }

    It 'joins documented list parameters with their delimiter' {
        New-FilesUser -Username 'new' -GroupIds 4, 5 -Tags 'eu', 'finance' -AllowedIps '10.0.0.1', '10.0.0.2' -Connection $connection -Confirm:$false | Out-Null

        $body = Get-Body -Service $service -Method POST -Path '/users'
        $body['group_ids'] | Should -BeExactly '4,5'
        $body['tags'] | Should -BeExactly 'eu,finance'
        $body['allowed_ips'] | Should -BeExactly "10.0.0.1`n10.0.0.2"
    }

    It 'sends a SecureString parameter''s text only in the request' {
        $password = ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force

        $verbose = New-FilesUser -Username 'new' -Password $password -Connection $connection -Confirm:$false -Verbose 4>&1

        (Get-Body -Service $service -Method POST -Path '/users')['password'] | Should -BeExactly $script:PasswordSentinel
        ($verbose | Out-String) | Should -Not -Match $script:PasswordSentinel
    }

    It 'takes every parameter decided as a credential as a SecureString, keeps reviewed identifiers as text, and sends the text only in the request' {
        $service.Respond('POST', '/public_keys', 200, '{"id":7,"title":"deploy"}')
        $passphrase = ConvertTo-SecureString 'sentinel-passphrase-3P7' -AsPlainText -Force
        foreach ($decided in @(
                @{ Command = 'New-FilesPublicKey'; Name = 'GeneratePrivateKeyPassword' }
                @{ Command = 'New-FilesRemoteServer'; Name = 'SharepointClientCertificate' }
                @{ Command = 'Set-FilesRemoteServerCredential'; Name = 'SharepointClientCertificate' }
                @{ Command = 'New-FilesSession'; Name = 'PartialSessionId' }
            )) {
            (Get-Command $decided.Command).Parameters[$decided.Name].ParameterType | Should -Be ([securestring])
        }
        (Get-Command Remove-FilesLock).Parameters['Token'].ParameterType | Should -Be ([string])
        (Get-Command New-FilesSession).Parameters['Otp'].ParameterType | Should -Be ([string])

        $verbose = New-FilesPublicKey -Title 'deploy' -GenerateKeypair $true -GeneratePrivateKeyPassword $passphrase -Connection $connection -Confirm:$false -Verbose 4>&1

        (Get-Body -Service $service -Method POST -Path '/public_keys')['generate_private_key_password'] | Should -BeExactly 'sentinel-passphrase-3P7'
        ($verbose | Out-String) | Should -Not -Match 'sentinel-passphrase-3P7'
    }

    It 'uses remote paths exactly as written' {
        # Non-ASCII letters, an apostrophe and spaces, including a trailing one.
        $path = "reports/$([char]0x00FC)n$([char]0x00EF)code 'quoted' name .txt "
        $service.Respond('GET', "/file_actions/metadata/$path", 200, "{""path"":""reports/x"",""type"":""file""}")

        Get-FilesFile -Path $path -Connection $connection | Out-Null

        [uri]::UnescapeDataString((Get-ApiRequest $service -Method GET)[0].Path) | Should -BeExactly "/api/rest/v1/file_actions/metadata/$path"
    }
}

Describe 'Response values' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'writes dates as UTC, whole numbers as Int64 and other numbers exactly' {
        $service.Respond('GET', '/file_actions/metadata/a.txt', 200, '{"path":"a.txt","size":12345678901234,"mtime":"2026-01-01T10:00:00-05:00","custom_metadata":{"Count":5,"ratio":1.25,"Flag":false,"nested":{"n":[1,2]}}}')

        $file = Get-FilesFile -Path 'a.txt' -Connection $connection

        $file.Size | Should -BeExactly ([long]12345678901234)
        $file.Mtime.Kind | Should -Be 'Utc'
        $file.Mtime | Should -Be ([datetime]::new(2026, 1, 1, 15, 0, 0, [System.DateTimeKind]::Utc))
        $file.CustomMetadata['Count'] | Should -BeOfType ([long])
        $file.CustomMetadata['ratio'] | Should -BeExactly ([decimal]'1.25')
        $file.CustomMetadata['Flag'] | Should -BeExactly $false
        $file.CustomMetadata['nested']['n'] | Should -Be @(1, 2)
    }

    It 'reads decimal properties as exact decimals, nested too, and as $null when Files.com leaves them out' {
        $service.Respond('GET', '/invoices/5', 200, '{"id":5,"amount":12345678901234567.89,"currency":"USD","invoice_line_items":[{"id":1,"amount":0.1000000000000000055511151231},{"id":2}]}')
        $service.Respond('GET', '/remote_mount_backends/3', 200, '{"id":3,"min_free_cpu":12.345678901234567890123456789,"min_free_mem":null}')

        $invoice = Get-FilesInvoice -Id 5 -Connection $connection
        $backend = Get-FilesRemoteMountBackend -Id 3 -Connection $connection

        # [decimal]'...' would go through a double; Parse keeps every digit.
        $invoice.Amount | Should -BeExactly ([decimal]::Parse('12345678901234567.89', [cultureinfo]::InvariantCulture))
        $invoice.Balance | Should -BeNullOrEmpty
        $invoice.InvoiceLineItems[0].Amount | Should -BeExactly ([decimal]::Parse('0.1000000000000000055511151231', [cultureinfo]::InvariantCulture))
        $invoice.InvoiceLineItems[1].Amount | Should -BeNullOrEmpty
        $backend.MinFreeCpu | Should -BeExactly ([decimal]::Parse('12.345678901234567890123456789', [cultureinfo]::InvariantCulture))
        $backend.MinFreeMem | Should -BeNullOrEmpty
    }

    It 'writes history entries as HistoryAction objects, whose action also answers to its wire name' {
        $service.Pages('/history/users/4', '[{"id":9,"path":"reports/a.csv","when":"2026-02-03T04:05:06Z","action":"read","interface":"web","username":"ana","user_id":4}]')

        $entry = Get-FilesUserHistory -UserId 4 -Connection $connection | Select-Object -First 1

        $entry | Should -BeOfType ([FilesCom.PowerShell.Models.HistoryAction])
        $entry.ActionType | Should -Be 'read'
        $entry.Action | Should -Be 'read'
        $entry.Interface | Should -Be 'web'
        $entry.When | Should -Be ([datetime]::new(2026, 2, 3, 4, 5, 6, [System.DateTimeKind]::Utc))
        (Get-Command Get-FilesUserHistory).OutputType.Name | Should -Contain 'FilesCom.PowerShell.Models.HistoryAction'
    }

    It 'returns credentials in results as FilesSecret, which output and serialization never show' {
        $service.Respond('POST', '/api_keys', 200, "{""id"":7,""name"":""automation"",""key"":""$script:ApiKeySentinel""}")

        $key = New-FilesApiKey -Name 'automation' -Connection $connection -Confirm:$false

        $key.Key | Should -BeOfType ([FilesCom.PowerShell.FilesSecret])
        $key.Key.Reveal() | Should -BeExactly $script:ApiKeySentinel
        ($key | Format-List * | Out-String) | Should -Not -Match $script:ApiKeySentinel
        ($key | ConvertTo-Json -Depth 5) | Should -Not -Match $script:ApiKeySentinel
        ($key | Export-Csv -Path (Join-Path $TestDrive 'k.csv') -NoTypeInformation)
        Get-Content (Join-Path $TestDrive 'k.csv') -Raw | Should -Not -Match $script:ApiKeySentinel
        $key | Export-Clixml -Path (Join-Path $TestDrive 'k.xml')
        Get-Content (Join-Path $TestDrive 'k.xml') -Raw | Should -Not -Match $script:ApiKeySentinel
    }

    It 'updates an -InputObject with what Files.com returns and writes that same object' {
        $service.Respond('GET', '/users/1', 200, '{"id":1,"username":"before","name":"Old"}')
        $service.Respond('PATCH', '/users/1', 200, '{"id":1,"username":"before","name":"Normalized Name"}')
        $user = Get-FilesUser -Id 1 -Connection $connection

        $result = $user | Set-FilesUser -Name ' normalized name ' -Confirm:$false

        [object]::ReferenceEquals($result, $user) | Should -BeTrue
        $user.Name | Should -Be 'Normalized Name'
        $body = (Get-ApiRequest $service -Method PATCH -Path '/users/1').Body | ConvertFrom-Json -AsHashtable
        $body['name'] | Should -BeExactly ' normalized name '
        $body.Keys | Should -Not -Contain 'username'
    }

    It 'reads Style responses of the schema''s shape as typed objects, and refuses the same response with only its logo malformed' {
        $valid = '{"id":1,"path":"brand","logo":{"name":"logo.png","uri":"https://example.invalid/logo.png"},"thumbnail":{"name":"thumb.png","uri":"https://example.invalid/thumb.png"}}'
        # The schema's own example gives logo as text, though logo is an Image object; this is that difference alone.
        $malformed = '{"id":1,"path":"brand","logo":"https://example.invalid/logo.png","thumbnail":{"name":"thumb.png","uri":"https://example.invalid/thumb.png"}}'
        $service.Respond('GET', '/styles/brand', 200, $valid)
        $service.Respond('PATCH', '/styles/brand', 200, $valid)
        $service.Respond('GET', '/styles/brand', 200, $malformed)

        $read = Get-FilesStyle -Path 'brand' -Connection $connection
        $updated = Set-FilesStyle -Path 'brand' -LogoClickHref 'https://example.invalid' -Connection $connection -Confirm:$false

        $read.Logo | Should -BeOfType ([FilesCom.PowerShell.Models.Image])
        $read.Logo.Name | Should -Be 'logo.png'
        $read.Thumbnail.Uri | Should -Be 'https://example.invalid/thumb.png'
        $updated.Logo.Name | Should -Be 'logo.png'
        { Get-FilesStyle -Path 'brand' -Connection $connection -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidResponse*'
    }

    It 'reads WebhookTest responses of the schema''s shape as typed objects, and refuses the same response with only its data malformed' {
        # The schema gives data as an object with a dynamic member; its own example gives text instead.
        $service.Respond('POST', '/webhook_tests', 200, '{"code":200,"message":"delivered","status":"ok","data":{"dynamic":{"received":true}},"success":true}')
        $service.Respond('POST', '/webhook_tests', 200, '{"code":200,"message":"delivered","status":"ok","data":"example","success":true}')

        $test = New-FilesWebhookTest -Url 'https://example.invalid/hook' -Connection $connection -Confirm:$false

        $test.Success | Should -BeTrue
        $test.Data | Should -BeOfType ([FilesCom.PowerShell.Models.Auto])
        $test.Data.Dynamic.received | Should -BeTrue
        { New-FilesWebhookTest -Url 'https://example.invalid/hook' -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidResponse*'
    }

    It 'offers a property that .NET naming renamed under its wire name too' {
        $service.Respond('GET', '/permissions', 200, '[{"id":1,"path":"projects","user_id":6,"permission":"full"}]')

        $permission = Get-FilesPermission -Path 'projects' -Connection $connection

        $permission.PermissionType | Should -Be 'full'
        $permission.Permission | Should -Be 'full'
    }
}
