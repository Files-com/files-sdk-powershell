BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')

    function New-ErrorJson {
        param([string] $Type, [int] $Status, [string] $Message = 'Failed')
        "{""error"":""$Message"",""http-code"":$Status,""title"":""Title of $Type"",""type"":""$Type"",""instance"":""instance-42"",""model_errors"":{""name"":[""is too long""]},""data"":{""partial_session_id"":""sentinel-partial-session""}}"
    }
}

Describe 'Error records' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'reports <Type> as <Id> with category <Category>' -TestCases @(
        @{ Type = 'not-found/user-not-found'; Status = 404; Id = 'Files.NotFound'; Category = 'ObjectNotFound' }
        @{ Type = 'not-authorized/site-admin-required'; Status = 403; Id = 'Files.PermissionDenied'; Category = 'PermissionDenied' }
        @{ Type = 'not-authenticated/invalid-credentials'; Status = 401; Id = 'Files.Authentication'; Category = 'AuthenticationError' }
        @{ Type = 'rate-limited/too-many-requests'; Status = 429; Id = 'Files.Throttled'; Category = 'LimitsExceeded' }
        @{ Type = 'processing-failure/destination-exists'; Status = 422; Id = 'Files.Conflict'; Category = 'ResourceExists' }
        @{ Type = 'processing-failure/folder-locked'; Status = 423; Id = 'Files.Conflict'; Category = 'ResourceBusy' }
        @{ Type = 'processing-failure/invalid-filename'; Status = 422; Id = 'Files.ProcessingFailure'; Category = 'InvalidOperation' }
        @{ Type = 'bad-request/invalid-filter-field'; Status = 400; Id = 'Files.InvalidArgument'; Category = 'InvalidArgument' }
        @{ Type = 'service-unavailable/site-disabled'; Status = 503; Id = 'Files.ServiceUnavailable'; Category = 'ResourceUnavailable' }
        @{ Type = 'a-type-this-module-does-not-know'; Status = 409; Id = 'Files.ApiError'; Category = 'NotSpecified' }
    ) {
        $service.Respond('GET', '/users/1', $Status, (New-ErrorJson -Type $Type -Status $Status), @{ 'X-Request-Id' = 'request-7' })

        $record = try { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } catch { $_ }

        $record.FullyQualifiedErrorId | Should -BeLike "$Id,*"
        $record.CategoryInfo.Category | Should -Be $Category
        $record.TargetObject | Should -Be 1
        $record.Exception | Should -BeOfType ([FilesCom.PowerShell.FilesException])
        $record.Exception.ErrorType | Should -Be $Type
        $record.Exception.HttpStatus | Should -Be $Status
        $record.Exception.Title | Should -Be "Title of $Type"
        $record.Exception.Instance | Should -Be 'instance-42'
        $record.Exception.RequestId | Should -Be 'request-7'
        $record.Exception.ModelErrors['name'] | Should -Be 'is too long'
    }

    It 'goes on with the next object after a failure that concerns only the current one, and stops with -ErrorAction Stop' {
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
        $service.Respond('GET', '/users/2', 200, '{"id":2}')
        $service.Respond('DELETE', '/users/1', 404, (New-ErrorJson -Type 'not-found/user-not-found' -Status 404))
        $service.Respond('DELETE', '/users/2', 204, $null)
        $users = 1, 2 | ForEach-Object { Get-FilesUser -Id $_ -Connection $connection }

        $users | Remove-FilesUser -Confirm:$false -ErrorVariable failures -ErrorAction SilentlyContinue

        $failures.Count | Should -Be 1
        (Get-ApiRequest $service -Method DELETE).Path | Should -Be @('/api/rest/v1/users/1', '/api/rest/v1/users/2')

        $service.ClearRequests()
        { $users | Remove-FilesUser -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.NotFound*'
        (Get-ApiRequest $service -Method DELETE).Count | Should -Be 1
    }

    It 'stops the command when the credentials fail, without trying the next object' {
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
        $service.Respond('GET', '/users/2', 200, '{"id":2}')
        $service.Respond('DELETE', '/users/1', 401, (New-ErrorJson -Type 'not-authenticated/invalid-credentials' -Status 401))
        $users = 1, 2 | ForEach-Object { Get-FilesUser -Id $_ -Connection $connection }

        { $users | Remove-FilesUser -Confirm:$false -ErrorAction Continue } | Should -Throw -ErrorId 'Files.Authentication*'
        (Get-ApiRequest $service -Method DELETE).Count | Should -Be 1
    }

    It 'reports an unreadable error response as Files.InvalidResponse, and an unreachable site as Files.Connection' {
        $service.Respond('GET', '/users/1', 500, '<html>proxy error</html>')
        { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidResponse*'

        $unreachable = Connect-FilesAccount -ApiKey (ConvertTo-SecureString 'k' -AsPlainText -Force) -BaseUrl 'http://127.0.0.1:9' -MaxNetworkRetries 0 -NoDefault
        { Get-FilesUser -Id 1 -Connection $unreachable -ErrorAction Stop } | Should -Throw -ErrorId 'Files.Connection*'
        Disconnect-FilesAccount -Connection $unreachable -Confirm:$false
    }
}

Describe 'Privacy of output' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'keeps credentials, error data and URL credentials out of errors, verbose and debug output and transcripts' {
        $signed = 'https://someone:sentinel-url-password@storage.example.com/bucket/file?X-Amz-Signature=sentinel-signature'
        $service.Respond('PATCH', '/users/1', 422, "{""error"":""Could not reach $signed"",""http-code"":422,""type"":""processing-failure/remote-server-error"",""data"":{""partial_session_id"":""sentinel-partial-session""}}")
        $transcript = Join-Path $TestDrive 'transcript.txt'
        $password = ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force

        Start-Transcript -Path $transcript | Out-Null
        try {
            $streams = Set-FilesUser -Id 1 -Password $password -Connection $connection -Confirm:$false -Verbose -Debug -ErrorVariable failures -ErrorAction Continue *>&1 | Out-String
            # Shown on the host, so the transcript holds the error records' full detail.
            $failures | Format-List * -Force | Out-Host
            $failures[0].Exception | Format-List * -Force | Out-Host
        }
        finally {
            Stop-Transcript | Out-Null
        }

        $everything = @($streams, ($failures | Format-List * -Force | Out-String), ($failures[0].Exception | Format-List * -Force | Out-String), (Get-Content -Raw $transcript)) -join "`n"
        foreach ($secret in @($script:ApiKeySentinel, $script:PasswordSentinel, 'sentinel-url-password', 'sentinel-signature', 'sentinel-partial-session')) {
            $everything | Should -Not -Match ([regex]::Escape($secret))
        }
        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.ProcessingFailure*'
        $failures[0].Exception.Message | Should -Match 'storage.example.com'
    }

    It 'keeps signed URL credentials out of every detail of Files.com''s error, and keeps the rest of its context' {
        $signed = 'https://synthetic-user:sentinel-url-password@storage.example.invalid/object?sig=sentinel-query-secret#sentinel-fragment'
        $body = @{
            error = 'Remote server failed'
            'http-code' = 422
            type = 'processing-failure/remote-server-error'
            title = "Could not reach $signed"
            instance = $signed
            errors = @($signed, 'Name can''t be blank')
            model_errors = @{ path = @("points to $signed"); name = @('can''t be blank'); $signed = @('bad field') }
        } | ConvertTo-Json -Depth 5 -Compress
        $service.Respond('GET', '/users/1', 422, $body, @{ 'X-Request-Id' = 'request-7Q' })

        $failure = try { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } catch { $_ }
        $shown = @(
            ($failure | Format-List * -Force | Out-String),
            ($failure.Exception | Format-List * -Force | Out-String),
            ($failure.Exception | ConvertTo-Json -Depth 5 -WarningAction SilentlyContinue),
            [System.Management.Automation.PSSerializer]::Serialize($failure, 3)
        ) -join "`n"

        foreach ($secret in 'sentinel-url-password', 'sentinel-query-secret', 'sentinel-fragment', 'synthetic-user') {
            $shown | Should -Not -Match $secret
        }
        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.ProcessingFailure*'
        $failure.Exception.ErrorType | Should -Be 'processing-failure/remote-server-error'
        $failure.Exception.HttpStatus | Should -Be 422
        $failure.Exception.RequestId | Should -Be 'request-7Q'
        $failure.Exception.Title | Should -Match '^Could not reach https://\[redacted\]@storage\.example\.invalid/object\?\[redacted\]#\[redacted\]$'
        $failure.Exception.Instance | Should -Match '^https://\[redacted\]@storage\.example\.invalid/object'
        $failure.Exception.Errors | Should -Contain 'Name can''t be blank'
        $failure.Exception.ModelErrors['name'] | Should -Be @('can''t be blank')
        $failure.Exception.ModelErrors['path'][0] | Should -Match 'storage\.example\.invalid/object'
    }

    It 'keeps signed URL credentials out of the error type and request ID of <Case>, and classifies the error as <Id>' -TestCases @(
        @{ Case = 'an API error'; Body = '{"error":"Remote server failed","http-code":422,"type":"processing-failure/remote-server-error SIGNED"}'; Id = 'Files.ProcessingFailure'; Type = '^processing-failure/remote-server-error https://\[redacted\]@storage\.example\.invalid/object\?\[redacted\]#\[redacted\]$' }
        @{ Case = 'an unreadable response'; Body = 'not json SIGNED'; Id = 'Files.InvalidResponse'; Type = $null }
    ) {
        $signed = 'https://synthetic-user:sentinel-url-password@storage.example.invalid/object?sig=sentinel-query-secret#sentinel-fragment'
        $service.Respond('GET', '/users/1', 422, $Body.Replace('SIGNED', $signed), @{ 'X-Request-Id' = "request $signed" })

        $failure = try { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } catch { $_ }
        $shown = @(
            ($failure | Format-List * -Force | Out-String),
            ($failure.Exception | Format-List * -Force | Out-String),
            ($failure.Exception | Select-Object Kind, ErrorType, HttpStatus, Title, Instance, RequestId, Errors, ModelErrors, Message | ConvertTo-Json -Depth 5),
            [System.Management.Automation.PSSerializer]::Serialize($failure, 3)
        ) -join "`n"

        foreach ($secret in 'sentinel-url-password', 'sentinel-query-secret', 'sentinel-fragment', 'synthetic-user') {
            $shown | Should -Not -Match $secret
        }
        $failure.FullyQualifiedErrorId | Should -BeLike "$Id*"
        $failure.Exception.HttpStatus | Should -Be 422
        $failure.Exception.RequestId | Should -Match '^request https://\[redacted\]@storage\.example\.invalid/object\?\[redacted\]#\[redacted\]$'
        if ($Type) {
            $failure.Exception.ErrorType | Should -Match $Type
        }
        else {
            $failure.Exception.ErrorType | Should -BeNullOrEmpty
        }
    }

    It 'drops the credentials when Files.com redirects a request to another origin' {
        $elsewhere = New-TestService
        try {
            $elsewhere.Respond('GET', '/users/1', 200, '{"id":1,"username":"moved"}')
            $service.Respond('GET', '/users/1', 302, $null, @{ Location = "$($elsewhere.BaseUrl)api/rest/v1/users/1" })

            (Get-FilesUser -Id 1 -Connection $connection).Username | Should -Be 'moved'

            $followed = @(Get-ApiRequest $elsewhere -Path '/users/1')
            $followed.Count | Should -Be 1
            $followed[0].Header('X-FilesAPI-Key') | Should -BeNullOrEmpty
        }
        finally {
            $elsewhere.Dispose()
        }
    }

    It 'describes a connection without its credentials' {
        ($connection | Format-List * -Force | Out-String) + ($connection | ConvertTo-Json) + $connection.ToString() | Should -Not -Match $script:ApiKeySentinel
    }
}
