BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
}

Describe 'Connect-FilesAccount' {
    BeforeEach {
        $service = New-TestService
        Set-FilesConnection -Clear
    }
    AfterEach {
        Get-FilesConnection -All | Disconnect-FilesAccount -Confirm:$false
        $service.Dispose()
    }

    It 'makes the new connection the default, and commands send its API key' {
        $service.Respond('GET', '/users/5', 200, '{"id":5,"username":"alice"}')
        $connection = Connect-TestService -Service $service -AsDefault

        (Get-FilesConnection).Id | Should -Be $connection.Id
        $user = Get-FilesUser -Id 5

        $user.Username | Should -Be 'alice'
        (Get-ApiRequest $service -Path '/users/5').Header('X-FilesAPI-Key') | Should -Be $script:ApiKeySentinel
    }

    It 'keeps the current default with -NoDefault' {
        $first = Connect-TestService -Service $service -AsDefault
        $second = Connect-TestService -Service $service

        (Get-FilesConnection).Id | Should -Be $first.Id
        (Get-FilesConnection -All).Id | Should -Contain $second.Id
    }

    It 'sends nothing and makes no connection under -WhatIf' {
        $before = @(Get-FilesConnection -All).Count
        $result = Connect-FilesAccount -ApiKey (ConvertTo-SecureString 'k' -AsPlainText -Force) -BaseUrl $service.BaseUrl -WhatIf

        $result | Should -BeNullOrEmpty
        @(Get-FilesConnection -All).Count | Should -Be $before
        $service.Requests.Count | Should -Be 0
    }

    It 'signs in with a credential and one-time password, and revokes that owned session when disconnecting' {
        $service.Respond('POST', '/sessions', 200, '{"id":"session-sentinel-5T1","language":"en"}')
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
        $service.Respond('DELETE', '/sessions', 204, $null)
        $credential = [pscredential]::new('alice', (ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force))

        $connection = Connect-FilesAccount -Credential $credential -OneTimePassword '123456' -BaseUrl $service.BaseUrl -MaxNetworkRetries 0 -NoDefault
        Get-FilesUser -Id 1 -Connection $connection | Out-Null
        Disconnect-FilesAccount -Connection $connection -Confirm:$false

        $signIn = Get-ApiRequest $service -Method POST -Path '/sessions'
        $signIn.Body | Should -Match '"username":"alice"'
        $signIn.Body | Should -Match '"otp":"123456"'
        $signIn.Header('X-FilesAPI-Auth') | Should -BeNullOrEmpty
        (Get-ApiRequest $service -Path '/users/1').Header('X-FilesAPI-Auth') | Should -Be 'session-sentinel-5T1'
        (Get-ApiRequest $service -Method DELETE -Path '/sessions').Header('X-FilesAPI-Auth') | Should -Be 'session-sentinel-5T1'
        $connection.AuthenticationMethod | Should -Be 'Credential'
        $connection.Username | Should -Be 'alice'
        $connection.IsOpen | Should -BeFalse
    }

    It 'finishes a two-factor sign-in from its challenge, keeping the partial login out of sight, and revokes the owned session' {
        $partial = 'sentinel-partial-login-4V8'
        $service.Respond('POST', '/sessions', 401, "{""error"":""2FA Authentication error: enter the code from your app."",""http-code"":401,""type"":""not-authenticated/two-factor-authentication-error"",""title"":""Two Factor Authentication Error"",""data"":{""partial_session_id"":""$partial"",""two_factor_authentication_methods"":[""totp""]}}")
        $service.Respond('POST', '/sessions', 200, '{"id":"session-sentinel-5T1","language":"en"}')
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
        $service.Respond('DELETE', '/sessions', 204, $null)
        $credential = [pscredential]::new('alice', (ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force))

        $failures = @(try { Connect-FilesAccount -Credential $credential -BaseUrl $service.BaseUrl -MaxNetworkRetries 0 -NoDefault -ErrorAction Stop } catch { $_ })
        $challenge = $failures[0].TargetObject
        $shown = @(
            ($failures | Format-List * -Force | Out-String),
            ($failures[0].Exception | Format-List * -Force | Out-String),
            ($challenge | Format-List * -Force | Out-String),
            ($challenge | ConvertTo-Json -Depth 5),
            [System.Management.Automation.PSSerializer]::Serialize($challenge),
            $challenge.ToString()
        ) -join "`n"
        $connection = $challenge | Connect-FilesAccount -OneTimePassword '654321' -NoDefault
        Get-FilesUser -Id 1 -Connection $connection | Out-Null
        Disconnect-FilesAccount -Connection $connection -Confirm:$false

        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.TwoFactorRequired*'
        $failures[0].CategoryInfo.Category | Should -Be 'AuthenticationError'
        $challenge.Username | Should -Be 'alice'
        $challenge.Methods | Should -Be @('totp')
        $shown | Should -Not -Match $partial
        $shown | Should -Not -Match $script:PasswordSentinel
        $completion = @(Get-ApiRequest $service -Method POST -Path '/sessions')[1]
        $completion.Body | Should -Match "`"partial_session_id`":`"$partial`""
        $completion.Body | Should -Match '"otp":"654321"'
        $completion.Body | Should -Match '"username":"alice"'
        $completion.Body | Should -Not -Match $script:PasswordSentinel
        (Get-ApiRequest $service -Method DELETE -Path '/sessions').Header('X-FilesAPI-Auth') | Should -Be 'session-sentinel-5T1'
        $connection.AuthenticationMethod | Should -Be 'Credential'
        $challenge.IsCompleted | Should -BeTrue
        { Connect-FilesAccount -Challenge $challenge -OneTimePassword '111111' -NoDefault -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        @(Get-ApiRequest $service -Method POST -Path '/sessions').Count | Should -Be 2
    }

    It 'lets one command at a time finish a challenge: another sends nothing, and a refused code can be tried again' {
        $partial = 'sentinel-partial-login-4V8'
        $service.Respond('POST', '/sessions', 401, "{""error"":""2FA required"",""http-code"":401,""type"":""not-authenticated/two-factor-authentication-error"",""data"":{""partial_session_id"":""$partial"",""two_factor_authentication_methods"":[""totp""]}}")
        $service.Respond('POST', '/sessions', 401, '{"error":"Invalid code","http-code":401,"type":"not-authenticated/two-factor-authentication-error"}', $null, 1500)
        $service.Respond('POST', '/sessions', 200, '{"id":"session-sentinel-6R3","language":"en"}')
        $service.Respond('DELETE', '/sessions', 204, $null)
        $credential = [pscredential]::new('alice', (ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force))
        $challenge = $(try { Connect-FilesAccount -Credential $credential -BaseUrl $service.BaseUrl -MaxNetworkRetries 0 -NoDefault -ErrorAction Stop } catch { $_ }).TargetObject

        # The first attempt, with a code Files.com refuses, is still waiting for its answer.
        $first = Start-Command -Connection $challenge -Script { param($Challenge) try { Connect-FilesAccount -Challenge $Challenge -OneTimePassword '000000' -NoDefault -ErrorAction Stop } catch { $_.FullyQualifiedErrorId } }
        Wait-Until { $challenge.IsInProgress -and $service.ActiveRequests -eq 1 }
        $competing = try { Connect-FilesAccount -Challenge $challenge -OneTimePassword '654321' -NoDefault -ErrorAction Stop } catch { $_ }
        $sentWhileInProgress = @(Get-ApiRequest $service -Method POST -Path '/sessions').Count
        $firstResult = $first.Shell.EndInvoke($first.Handle)
        $first.Shell.Dispose()
        $refusedState = $challenge.IsInProgress, $challenge.IsCompleted
        $connection = Connect-FilesAccount -Challenge $challenge -OneTimePassword '654321' -NoDefault
        Disconnect-FilesAccount -Connection $connection -Confirm:$false

        $competing.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
        $competing.Exception.Message | Should -Match 'Another command is finishing'
        $sentWhileInProgress | Should -Be 2
        $firstResult | Should -BeLike 'Files.Authentication*'
        $refusedState | Should -Be @($false, $false)
        $challenge.IsCompleted | Should -BeTrue
        $posts = @(Get-ApiRequest $service -Method POST -Path '/sessions')
        $posts.Count | Should -Be 3
        $posts[2].Body | Should -Match '"otp":"654321"'
        (Get-ApiRequest $service -Method DELETE -Path '/sessions').Header('X-FilesAPI-Auth') | Should -Be 'session-sentinel-6R3'
    }

    It 'cannot finish a sign-in with a challenge read back from serialized data' {
        $service.Respond('POST', '/sessions', 401, '{"error":"2FA required","http-code":401,"type":"not-authenticated/two-factor-authentication-error","data":{"partial_session_id":"sentinel-partial-login-4V8","two_factor_authentication_methods":["sms"]}}')
        $credential = [pscredential]::new('alice', (ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force))
        $failures = @(try { Connect-FilesAccount -Credential $credential -BaseUrl $service.BaseUrl -MaxNetworkRetries 0 -NoDefault -ErrorAction Stop } catch { $_ })
        $saved = [System.Management.Automation.PSSerializer]::Serialize($failures[0].TargetObject)
        $readBack = [System.Management.Automation.PSSerializer]::Deserialize($saved)

        $saved | Should -Not -Match 'sentinel-partial-login-4V8'
        { Connect-FilesAccount -Challenge $readBack -OneTimePassword '654321' -NoDefault -ErrorAction Stop } | Should -Throw
        @(Get-ApiRequest $service -Method POST -Path '/sessions').Count | Should -Be 1
    }

    It 'never revokes an API key, and closes locally' {
        $connection = Connect-TestService -Service $service
        Disconnect-FilesAccount -Connection $connection -Confirm:$false

        $connection.IsOpen | Should -BeFalse
        $service.Requests.Count | Should -Be 0
    }

    It 'closes locally and reports separately when revoking the session fails' {
        $service.Respond('POST', '/sessions', 200, '{"id":"session-2"}')
        $service.Respond('DELETE', '/sessions', 500, 'not json')
        $credential = [pscredential]::new('bob', (ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force))
        $connection = Connect-FilesAccount -Credential $credential -BaseUrl $service.BaseUrl -MaxNetworkRetries 0 -NoDefault

        Disconnect-FilesAccount -Connection $connection -Confirm:$false -ErrorVariable failures -ErrorAction SilentlyContinue

        $connection.IsOpen | Should -BeFalse
        $failures.Count | Should -Be 1
        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.InvalidResponse*'
        $failures[0].ErrorDetails.Message | Should -Match 'is closed, but Files.com may not have revoked its session'
    }

    It 'fails with Files.NoConnection when there is no connection to use' {
        { Get-FilesUser -Id 1 -ErrorAction Stop } | Should -Throw -ErrorId 'Files.NoConnection*'
    }

    It 'refuses a closed connection with Files.ConnectionClosed, without rebinding objects to another connection' {
        $service.Respond('GET', '/users/9', 200, '{"id":9,"username":"old"}')
        $closing = Connect-TestService -Service $service
        $user = Get-FilesUser -Id 9 -Connection $closing
        Disconnect-FilesAccount -Connection $closing -Confirm:$false
        Connect-TestService -Service $service -AsDefault | Out-Null

        $failure = try { $user | Set-FilesUser -Name 'new' -Confirm:$false -ErrorAction Stop } catch { $_ }

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.ConnectionClosed*'
        $failure.Exception.Message | Should -Match 'is closed\. Connect again with Connect-FilesAccount\.$'
        $failure.ErrorDetails.RecommendedAction | Should -Match 'Connect again with Connect-FilesAccount'
        (Get-ApiRequest $service -Method PATCH).Count | Should -Be 0
    }

    It 'refuses a connection already closed, before asking to confirm anything, whether it comes from an object, -Connection, the default or a continuation' {
        $service.Respond('GET', '/users/9', 200, '{"id":9,"username":"old"}')
        $service.Pages('/users', (ConvertTo-UserJson 1), (ConvertTo-UserJson 2))
        $closing = Connect-TestService -Service $service -AsDefault
        $user = Get-FilesUser -Id 9 -Connection $closing
        $page = Get-FilesUser -Connection $closing -Page
        # Closed from another runspace, so this runspace still has it selected as its default.
        $other = Start-Command -Connection $closing -Script { param($Connection) Disconnect-FilesAccount -Connection $Connection -Confirm:$false }
        $other.Shell.EndInvoke($other.Handle) | Out-Null
        $other.Shell.Dispose()
        $service.ClearRequests()

        # With -WhatIf, a command that reached its confirmation would describe the change and end without an error.
        $failures = @(
            try { $user | Set-FilesUser -Name 'new' -WhatIf -ErrorAction Stop } catch { $_ }
            try { Set-FilesUser -Id 9 -Name 'new' -Connection $closing -WhatIf -ErrorAction Stop } catch { $_ }
            try { Set-FilesUser -Id 9 -Name 'new' -WhatIf -ErrorAction Stop } catch { $_ }
            try { Get-FilesUser -Continuation $page.Continuation -ErrorAction Stop } catch { $_ }
        )

        $failures.Count | Should -Be 4
        foreach ($failure in $failures) {
            $failure.FullyQualifiedErrorId | Should -BeLike 'Files.ConnectionClosed*'
            $failure.Exception.Message | Should -Match "^The connection to $([regex]::Escape($service.BaseUrl)) is closed\. Connect again with Connect-FilesAccount\.$"
            $failure.ErrorDetails.RecommendedAction | Should -Match 'Connect again with Connect-FilesAccount'
        }
        $service.Requests.Count | Should -Be 0
        $closing.IsOpen | Should -BeFalse
    }

    It 'refuses an object from one connection given with a different -Connection' {
        $service.Respond('GET', '/users/3', 200, '{"id":3}')
        $one = Connect-TestService -Service $service
        $other = Connect-TestService -Service $service -ApiKey 'other-key'
        $user = Get-FilesUser -Id 3 -Connection $one

        { $user | Remove-FilesUser -Connection $other -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        (Get-ApiRequest $service -Method DELETE).Count | Should -Be 0
    }

    It 'rejects a site address with credentials in it' {
        { Connect-FilesAccount -ApiKey (ConvertTo-SecureString 'k' -AsPlainText -Force) -BaseUrl 'https://user:pass@example.files.com' -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
    }
}

Describe 'Connections across runspaces' {
    BeforeEach {
        $service = New-TestService
    }
    AfterEach {
        $service.Dispose()
    }

    It 'lets ForEach-Object -Parallel use a connection given with $using:, each request with its credentials' {
        1..4 | ForEach-Object { $service.Respond('GET', "/users/$_", 200, "{""id"":$_}") }
        $connection = Connect-TestService -Service $service

        $ids = 1..4 | ForEach-Object -Parallel {
            Import-Module -Name $using:ModuleManifest
            (Get-FilesUser -Id $_ -Connection $using:connection).Id
        } -ThrottleLimit 4

        ($ids | Sort-Object) | Should -Be @(1, 2, 3, 4)
        (Get-ApiRequest $service -Method GET | ForEach-Object { $_.Header('X-FilesAPI-Key') } | Select-Object -Unique) | Should -Be $script:ApiKeySentinel
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
    }

    It 'keeps each runspace''s default separate, and closes a runspace''s connections when it closes' {
        $runspace = [runspacefactory]::CreateRunspace()
        $runspace.Open()
        try {
            $shell = [powershell]::Create($runspace)
            $inner = $shell.AddScript({
                    param($Manifest, $Url)
                    Import-Module -Name $Manifest
                    Connect-FilesAccount -ApiKey (ConvertTo-SecureString 'inner-key' -AsPlainText -Force) -BaseUrl $Url
                }).AddArgument($script:ModuleManifest).AddArgument($service.BaseUrl).Invoke()[0]
            $shell.Dispose()

            $inner.IsOpen | Should -BeTrue
            (Get-FilesConnection).Id | Should -Not -Be $inner.Id
        }
        finally {
            $runspace.Dispose()
        }

        $inner.IsOpen | Should -BeFalse
    }
}
