BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')

    function Connect-WithSetting {
        [CmdletBinding()]
        param(
            [Parameter(Position = 0)] [FilesCom.PowerShell.Tests.FilesTestService] $Service,
            [Parameter(Position = 1)] [hashtable] $Options = @{}
        )
        $arguments = @{ ApiKey = (ConvertTo-SecureString $script:ApiKeySentinel -AsPlainText -Force); BaseUrl = $Service.BaseUrl; NoDefault = $true }
        foreach ($option in $Options.Keys) {
            $arguments[$option] = $Options[$option]
        }
        Connect-FilesAccount @arguments
    }
}

Describe 'Retries and network timeouts' {
    BeforeEach {
        $service = New-TestService
    }
    AfterEach {
        Get-FilesConnection -All | Disconnect-FilesAccount -Confirm:$false
        $service.Dispose()
    }

    It 'retries a failed request as -MaxNetworkRetries allows and reports each retry on the Verbose stream' {
        $service.Respond('GET', '/users/1', 503, '{"error":"Try again","http-code":503,"type":"service-unavailable"}')
        $service.Respond('GET', '/users/1', 503, '{"error":"Try again","http-code":503,"type":"service-unavailable"}')
        $service.Respond('GET', '/users/1', 200, '{"id":1,"username":"alice"}')
        $connection = Connect-WithSetting $service @{ MaxNetworkRetries = 2; RetryDelaySeconds = 0; MaxRetryDelaySeconds = 0 }

        $records = @(Get-FilesUser -Id 1 -Connection $connection -Verbose 4>&1)

        ($records | Where-Object { $_ -is [FilesCom.PowerShell.Models.User] }).Username | Should -Be 'alice'
        $verbose = @($records | Where-Object { $_ -is [System.Management.Automation.VerboseRecord] } | ForEach-Object Message)
        @($verbose | Where-Object { $_ -like 'FilesCom: Retrying*retry 1 of 2*' }).Count | Should -Be 1
        @($verbose | Where-Object { $_ -like 'FilesCom: Retrying*retry 2 of 2*' }).Count | Should -Be 1
        @(Get-ApiRequest $service -Path '/users/1').Count | Should -Be 3
        $connection.MaxNetworkRetries | Should -Be 2
    }

    It 'never sends again a request whose connection dropped after it was sent and that may have changed something, and says so' {
        $service.Drop('POST', '/folders/reports')
        # A delete has no body, which a connection pool would otherwise send again by itself when its connection closes.
        $service.Drop('DELETE', '/users/5')
        $service.Drop('GET', '/users/1')
        $connection = Connect-WithSetting $service @{ MaxNetworkRetries = 2; RetryDelaySeconds = 0; MaxRetryDelaySeconds = 0 }

        $failure = try { New-FilesFolder -Path 'reports' -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
        $deleted = try { Remove-FilesUser -Id 5 -Connection $connection -Confirm:$false -ErrorAction Stop } catch { $_ }
        $read = try { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } catch { $_ }

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.Connection*'
        $failure.Exception.RequestMayHaveBeenProcessed | Should -BeTrue
        $failure.ErrorDetails.RecommendedAction | Should -Match 'Check whether it did'
        @(Get-ApiRequest $service -Method POST -Path '/folders/reports').Count | Should -Be 1
        $deleted.FullyQualifiedErrorId | Should -BeLike 'Files.Connection*'
        $deleted.Exception.RequestMayHaveBeenProcessed | Should -BeTrue
        @(Get-ApiRequest $service -Method DELETE -Path '/users/5').Count | Should -Be 1
        # A read changes nothing, so it is sent again, and its failure is not in doubt.
        $read.FullyQualifiedErrorId | Should -BeLike 'Files.Connection*'
        $read.Exception.RequestMayHaveBeenProcessed | Should -BeFalse
        @(Get-ApiRequest $service -Path '/users/1').Count | Should -BeGreaterThan 1
    }

    It 'fails a request that stalls longer than -ReadTimeoutSeconds with Files.NetworkTimeout, once its retries are spent' {
        $service.Hang('GET', '/users/1')
        $connection = Connect-WithSetting $service @{ ReadTimeoutSeconds = 1; MaxNetworkRetries = 1; RetryDelaySeconds = 0 }

        $elapsed = Measure-Command { $failure = try { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } catch { $_ } }

        $failure.FullyQualifiedErrorId | Should -BeLike 'Files.NetworkTimeout*'
        $failure.CategoryInfo.Category | Should -Be 'OperationTimeout'
        $failure.ErrorDetails.RecommendedAction | Should -Match 'ReadTimeoutSeconds'
        @(Get-ApiRequest $service -Path '/users/1').Count | Should -Be 2
        $elapsed.TotalSeconds | Should -BeLessThan 10
        $connection.ReadTimeout | Should -Be ([TimeSpan]::FromSeconds(1))
    }

    It 'shows the engine''s settings on the connection, and null for those left to the engine' {
        $connection = Connect-WithSetting $service @{ ConnectTimeoutSeconds = 7; WriteTimeoutSeconds = 9 }

        $connection.ConnectTimeout | Should -Be ([TimeSpan]::FromSeconds(7))
        $connection.WriteTimeout | Should -Be ([TimeSpan]::FromSeconds(9))
        $connection.ReadTimeout | Should -BeNullOrEmpty
        $connection.MaxNetworkRetries | Should -BeNullOrEmpty
        $connection.Proxy | Should -BeNullOrEmpty
        $connection.UsesSharedHandler | Should -BeFalse
        $connection.DiagnosticDebug | Should -BeFalse
    }

    It 'refuses a <Name> of <Value> before connecting' -TestCases @(
        @{ Name = 'ReadTimeoutSeconds'; Value = 0 }
        @{ Name = 'ConnectTimeoutSeconds'; Value = 2147484 }
        @{ Name = 'RetryDelaySeconds'; Value = -1 }
        @{ Name = 'MaxRetryDelaySeconds'; Value = [double]::NaN }
    ) {
        { Connect-WithSetting $service @{ $Name = $Value } } | Should -Throw -ErrorId 'ParameterArgumentValidationError*'
        Get-FilesConnection -All | Should -BeNullOrEmpty
    }
}

Describe 'Engine diagnostics' {
    BeforeEach {
        $service = New-TestService
    }
    AfterEach {
        Get-FilesConnection -All | Disconnect-FilesAccount -Confirm:$false
        $service.Dispose()
    }

    It 'writes the engine''s messages for the command''s own requests to its Verbose stream, without credentials or queries' {
        $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 1), $null, 2000)
        $service.Respond('GET', '/users/2', 200, '{"id":2}')
        $connection = Connect-WithSetting $service
        # Another runspace's request, sent while this command waits for its own response.
        $other = Start-Command -Connection $connection -Script { param($Connection) Start-Sleep -Milliseconds 500; Get-FilesUser -Id 2 -Connection $Connection -Verbose 4>&1 }

        $records = @(Get-FilesUser -Connection $connection -Search 'sentinel-search' -Verbose 4>&1)
        $otherVerbose = @($other.Shell.EndInvoke($other.Handle) | Where-Object { $_ -is [System.Management.Automation.VerboseRecord] } | ForEach-Object Message) -join "`n"
        $other.Shell.Dispose()

        $verbose = @($records | Where-Object { $_ -is [System.Management.Automation.VerboseRecord] } | ForEach-Object Message) -join "`n"
        # The engine names each request by its method and resource only, with no site, ID, query or credentials, so
        # each command's Verbose stream holds exactly its own one request.
        ([regex]::Matches($verbose, 'FilesCom: Sending GET /users')).Count | Should -Be 1
        $verbose | Should -Not -Match '127\.0\.0\.1|/users/'
        $verbose | Should -Not -Match 'sentinel-search'
        $verbose | Should -Not -Match $script:ApiKeySentinel
        ([regex]::Matches($otherVerbose, 'FilesCom: Sending GET /users')).Count | Should -Be 1
        (Get-ApiRequest $service -Path '/users/2').Count | Should -Be 1
        (Get-ApiRequest $service -Path '/users').Query['search'] | Should -Be 'sentinel-search'
    }

    It 'writes the engine''s full DEBUG messages, which include responses, only for a connection made with -DiagnosticDebug' {
        $service.Respond('GET', '/users/1', 200, '{"id":1,"username":"debug-marker-5T"}')
        $service.ServeDownload('reports/debug.csv', [byte[]](1..10))
        $plain = Connect-WithSetting $service
        $diagnosed = Connect-WithSetting $service @{ DiagnosticDebug = $true }

        # As -Debug would, without its prompt in a non-interactive host.
        $DebugPreference = 'Continue'
        $plainDebug = @(Get-FilesUser -Id 1 -Connection $plain 5>&1 | Where-Object { $_ -is [System.Management.Automation.DebugRecord] } | ForEach-Object Message) -join "`n"
        $diagnosedDebug = @(Get-FilesUser -Id 1 -Connection $diagnosed 5>&1 | Where-Object { $_ -is [System.Management.Automation.DebugRecord] } | ForEach-Object Message) -join "`n"

        $download = @(Receive-FilesFile -RemotePath 'reports/debug.csv' -OutputStream ([System.IO.MemoryStream]::new()) -Connection $diagnosed -Confirm:$false -Verbose 5>&1 4>&1)
        $downloadDebug = @($download | Where-Object { $_ -is [System.Management.Automation.DebugRecord] } | ForEach-Object Message) -join "`n"
        $downloadVerbose = @($download | Where-Object { $_ -is [System.Management.Automation.VerboseRecord] } | ForEach-Object Message) -join "`n"

        $plainDebug | Should -Not -Match 'FilesCom:'
        $diagnosedDebug | Should -Match 'FilesCom: .*debug-marker-5T'
        $diagnosedDebug | Should -Not -Match $script:ApiKeySentinel
        # The explicit diagnostics are not redacted; the Verbose stream of the same command is.
        $downloadDebug | Should -Match 'FilesCom: .*X-Files-Signature=sentinel-signed-url-secret'
        $downloadVerbose | Should -Match 'FilesCom: '
        $downloadVerbose | Should -Not -Match 'sentinel-signed-url-secret'
        $diagnosed.DiagnosticDebug | Should -BeTrue
    }

    It 'writes only the engine''s messages at the connection''s -LogLevel or above, and refuses -DiagnosticDebug with another level' {
        # A retry is information, and a request that fails in the end a warning.
        $service.Respond('GET', '/users/1', 503, '{"error":"Try again","http-code":503,"type":"service-unavailable"}')
        $service.Respond('GET', '/users/1', 401, '{"error":"Sign in","http-code":401,"type":"not-authenticated/authentication-required"}')
        $warnings = Connect-WithSetting $service @{ LogLevel = 'Warning'; MaxNetworkRetries = 1; RetryDelaySeconds = 0; MaxRetryDelaySeconds = 0 }
        $silent = Connect-WithSetting $service @{ LogLevel = 'None'; MaxNetworkRetries = 1; RetryDelaySeconds = 0; MaxRetryDelaySeconds = 0 }

        $warningVerbose = @(& { try { Get-FilesUser -Id 1 -Connection $warnings -Verbose } catch { $null = $_ } } 4>&1 | Where-Object { $_ -is [System.Management.Automation.VerboseRecord] } | ForEach-Object Message) -join "`n"
        $service.Respond('GET', '/users/1', 503, '{"error":"Try again","http-code":503,"type":"service-unavailable"}')
        $service.Respond('GET', '/users/1', 401, '{"error":"Sign in","http-code":401,"type":"not-authenticated/authentication-required"}')
        $silentVerbose = @(& { try { Get-FilesUser -Id 1 -Connection $silent -Verbose } catch { $null = $_ } } 4>&1 | Where-Object { $_ -is [System.Management.Automation.VerboseRecord] } | ForEach-Object Message) -join "`n"

        $warningVerbose | Should -Not -Match 'FilesCom: (Sending|Retrying)'
        $warningVerbose | Should -Match 'FilesCom: .*HTTP 401'
        $silentVerbose | Should -Not -Match 'FilesCom:'
        $warnings.LogLevel | Should -Be 'Warning'
        { Connect-WithSetting $service @{ LogLevel = 'Warning'; DiagnosticDebug = $true } -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
    }

    It 'writes a sign-in''s full DEBUG messages only when -DiagnosticDebug asks and only to its own command, and keeps its password and session ID off its Verbose stream, an ordinary sign-in''s output and another connection''s' {
        $service.Respond('POST', '/sessions', 200, '{"id":"session-sentinel-8D2","language":"en"}', $null, 1000)
        $service.Respond('GET', '/users/5', 200, '{"id":5}', $null, 800)
        $service.Respond('DELETE', '/sessions', 204, $null)
        $credential = [pscredential]::new('alice', (ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force))
        $plain = Connect-WithSetting $service
        # Another connection's command, with the same stream preferences, runs while the sign-in waits for its answer.
        $other = Start-Command -Connection $plain -Script { param($Connection) $DebugPreference = 'Continue'; Get-FilesUser -Id 5 -Connection $Connection -Verbose 5>&1 4>&1 }

        $DebugPreference = 'Continue'
        $diagnosed = @(Connect-FilesAccount -Credential $credential -BaseUrl $service.BaseUrl -DiagnosticDebug -NoDefault -Verbose 5>&1 4>&1)
        $ordinary = @(Connect-FilesAccount -Credential $credential -BaseUrl $service.BaseUrl -NoDefault -Verbose 5>&1 4>&1)
        $otherText = @($other.Shell.EndInvoke($other.Handle) | ForEach-Object { "$_" }) -join "`n"
        $other.Shell.Dispose()

        $debug = @($diagnosed | Where-Object { $_ -is [System.Management.Automation.DebugRecord] } | ForEach-Object Message) -join "`n"
        $verbose = @($diagnosed | Where-Object { $_ -is [System.Management.Automation.VerboseRecord] } | ForEach-Object Message) -join "`n"
        $ordinaryDebug = @($ordinary | Where-Object { $_ -is [System.Management.Automation.DebugRecord] } | ForEach-Object Message) -join "`n"
        $ordinaryText = @($ordinary | Where-Object { $_ -isnot [FilesCom.PowerShell.FilesConnection] } | ForEach-Object { "$_" }) -join "`n"
        ($diagnosed | Where-Object { $_ -is [FilesCom.PowerShell.FilesConnection] }).DiagnosticDebug | Should -BeTrue
        # The engine's DEBUG messages about the sign-in's request, whatever they hold, reach the Debug stream of the
        # sign-in that asked for them, and no other command's.
        $debug | Should -Match 'FilesCom: .*/sessions'
        $ordinaryDebug | Should -Not -Match 'FilesCom:'
        $otherText | Should -Not -Match 'FilesCom: .*/sessions'
        $verbose | Should -Match 'FilesCom: Sending POST /sessions'
        $verbose | Should -Not -Match $script:PasswordSentinel
        $verbose | Should -Not -Match 'session-sentinel-8D2'
        $ordinaryText | Should -Not -Match $script:PasswordSentinel
        $ordinaryText | Should -Not -Match 'session-sentinel-8D2'
        $otherText | Should -Not -Match $script:PasswordSentinel
        $otherText | Should -Not -Match 'session-sentinel-8D2'
    }
}

Describe 'Transport' {
    BeforeEach {
        $service = New-TestService
    }
    AfterEach {
        Get-FilesConnection -All | Disconnect-FilesAccount -Confirm:$false
        $service.Dispose()
    }

    It 'identifies the module and the engine it ships, by their exact versions, in the User-Agent' {
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
        $build = Get-Content -Raw -LiteralPath (Join-Path (Split-Path -Parent $script:ModuleManifest) 'FilesCom.BuildInfo.json') | ConvertFrom-Json
        $connection = Connect-TestService -Service $service

        Get-FilesUser -Id 1 -Connection $connection | Out-Null

        # The module's and the engine's exact products, then the SDK's one metadata comment and nothing after it: the
        # coarse family of this host's operating system, as .NET's public checks tell it, and the version of the .NET
        # runtime this PowerShell runs on.
        $userAgent = (Get-ApiRequest $service -Path '/users/1')[0].Header('User-Agent')
        $products = "FilesCom-PowerShell/$($build.module.version) Files.com DOTNET SDK v$($build.engine.version)"
        $metadata = ' \(files-sdk-meta/1; os=(?<os>windows|macos|linux|android|ios|freebsd|other|unknown); runtime=dotnet/(?<runtime>[0-9][A-Za-z0-9.+_-]{0,47})\)'
        $pattern = '\A' + [regex]::Escape($products) + '(?<metadata>' + $metadata + ')\z'
        $os = if ([System.OperatingSystem]::IsAndroid()) { 'android' }
        elseif ([System.OperatingSystem]::IsIOS()) { 'ios' }
        elseif ([System.OperatingSystem]::IsWindows()) { 'windows' }
        elseif ([System.OperatingSystem]::IsMacOS()) { 'macos' }
        elseif ([System.OperatingSystem]::IsFreeBSD()) { 'freebsd' }
        elseif ([System.OperatingSystem]::IsLinux()) { 'linux' }
        else { 'other' }

        $userAgent | Should -MatchExactly $pattern
        $match = [regex]::Match($userAgent, $pattern)
        $match.Groups['os'].Value | Should -BeExactly $os
        $match.Groups['runtime'].Value | Should -BeExactly ([System.Environment]::Version.ToString())
        [System.Text.Encoding]::UTF8.GetByteCount($match.Groups['metadata'].Value) | Should -BeLessOrEqual 128
        [System.Text.Encoding]::UTF8.GetByteCount($userAgent) | Should -BeLessOrEqual 190
    }

    It 'sends the connection''s requests through -Proxy, answering its challenge with -ProxyCredential' {
        $service.RequiredProxyCredential = 'proxy-user:sentinel-proxy-password'
        $service.Respond('GET', '/users/1', 200, '{"id":1,"username":"proxied"}')
        $proxyCredential = [pscredential]::new('proxy-user', (ConvertTo-SecureString 'sentinel-proxy-password' -AsPlainText -Force))
        $connection = Connect-FilesAccount -ApiKey (ConvertTo-SecureString $script:ApiKeySentinel -AsPlainText -Force) -BaseUrl 'http://files.invalid/' -Proxy $service.BaseUrl -ProxyCredential $proxyCredential -MaxNetworkRetries 0 -NoDefault -WarningAction SilentlyContinue

        (Get-FilesUser -Id 1 -Connection $connection).Username | Should -Be 'proxied'

        $sent = @(Get-ApiRequest $service -Path '/users/1')
        ($sent.AbsoluteTarget -replace '\?.*$', '') | Select-Object -Unique | Should -Be 'http://files.invalid/api/rest/v1/users/1'
        $sent[-1].Header('Proxy-Authorization') | Should -Be ('Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('proxy-user:sentinel-proxy-password')))
        $connection.Proxy | Should -Be ([uri]$service.BaseUrl)
        ($connection | Format-List * -Force | Out-String) + ($connection | ConvertTo-Json) | Should -Not -Match 'sentinel-proxy-password'
    }

    It 'refuses -Proxy <Case> before connecting' -TestCases @(
        @{ Case = 'with -HttpMessageHandler'; Options = @{ Proxy = 'http://proxy.invalid:8080'; HttpMessageHandler = [System.Net.Http.SocketsHttpHandler]@{ AllowAutoRedirect = $false } } }
        @{ Case = 'credentials without a proxy'; Options = @{ ProxyCredential = [pscredential]::new('u', (ConvertTo-SecureString 'p' -AsPlainText -Force)) } }
        @{ Case = 'with user information'; Options = @{ Proxy = 'http://u:p@proxy.invalid:8080' } }
        @{ Case = 'that is not http or https'; Options = @{ Proxy = 'socks5://proxy.invalid:1080' } }
    ) {
        { Connect-WithSetting $service $Options -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        Get-FilesConnection -All | Should -BeNullOrEmpty
    }

    It 'sends requests through a caller''s -HttpMessageHandler, which closing the connection never disposes' {
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
        $handler = [System.Net.Http.SocketsHttpHandler]@{ AllowAutoRedirect = $false }
        try {
            $connection = Connect-WithSetting $service @{ HttpMessageHandler = $handler }
            Get-FilesUser -Id 1 -Connection $connection | Out-Null
            Disconnect-FilesAccount -Connection $connection -Confirm:$false

            $client = [System.Net.Http.HttpClient]::new($handler, $false)
            $afterwards = $client.GetAsync("$($service.BaseUrl)api/rest/v1/users/1").GetAwaiter().GetResult()

            [int]$afterwards.StatusCode | Should -Be 200
            $connection.UsesSharedHandler | Should -BeTrue
            @(Get-ApiRequest $service -Path '/users/1').Count | Should -Be 2
        }
        finally {
            $handler.Dispose()
        }
    }

    It 'refuses a handler that follows redirects itself, since that would send credentials to other sites' {
        $handler = [System.Net.Http.SocketsHttpHandler]::new()
        try {
            $failure = try { Connect-WithSetting $service @{ HttpMessageHandler = $handler } -ErrorAction Stop } catch { $_ }

            $failure.FullyQualifiedErrorId | Should -BeLike 'Files.InvalidArgument*'
            $failure.Exception.Message | Should -Match 'AllowAutoRedirect'
            Get-FilesConnection -All | Should -BeNullOrEmpty
            $service.Requests.Count | Should -Be 0
        }
        finally {
            $handler.Dispose()
        }
    }
}

Describe 'Closing connections' {
    BeforeEach {
        $service = New-TestService
    }
    AfterEach {
        Get-FilesConnection -All | Disconnect-FilesAccount -Confirm:$false
        $service.Dispose()
    }

    It 'closes the connection''s network connections before Disconnect-FilesAccount returns, however often it connects' {
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
        foreach ($attempt in 1..3) {
            $connection = Connect-WithSetting $service
            Get-FilesUser -Id 1 -Connection $connection | Out-Null
            $service.OpenConnections | Should -BeGreaterThan 0

            Disconnect-FilesAccount -Connection $connection -Confirm:$false

            Wait-Until { $service.OpenConnections -eq 0 } -TimeoutSeconds 5
            { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } | Should -Throw -ErrorId 'Files.ConnectionClosed*'
        }
    }

    It 'closes the network connections of a runspace''s connections when the runspace closes' {
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
        $runspace = [runspacefactory]::CreateRunspace()
        $runspace.Open()
        try {
            $shell = [powershell]::Create($runspace)
            $shell.AddScript({
                    param($Manifest, $Url)
                    Import-Module -Name $Manifest
                    $connection = Connect-FilesAccount -ApiKey (ConvertTo-SecureString 'inner-key' -AsPlainText -Force) -BaseUrl $Url -NoDefault
                    Get-FilesUser -Id 1 -Connection $connection | Out-Null
                }).AddArgument($script:ModuleManifest).AddArgument($service.BaseUrl).Invoke() | Out-Null
            $shell.Dispose()
            $service.OpenConnections | Should -BeGreaterThan 0
        }
        finally {
            $runspace.Dispose()
        }

        Wait-Until { $service.OpenConnections -eq 0 } -TimeoutSeconds 5
    }
}

Describe 'Commands stopped by their own output' {
    BeforeEach {
        $service = New-TestService
    }
    AfterEach {
        Get-FilesConnection -All | Disconnect-FilesAccount -Confirm:$false
        $service.Dispose()
    }

    It 'cancels a request the Verbose stream stops, settles it before returning, and leaves other commands on the connection running' {
        $service.Hang('PATCH', '/users/77')
        $service.Respond('GET', '/users/5', 200, '{"id":5,"username":"companion"}', $null, 1500)
        $connection = Connect-WithSetting $service
        $companion = Start-Command -Connection $connection -Script { param($Connection) Get-FilesUser -Id 5 -Connection $Connection }
        Wait-Until { @(Get-ApiRequest $service -Path '/users/5').Count -eq 1 }

        $VerbosePreference = 'Stop'
        $failure = try { Set-FilesUser -Id 77 -Name 'synthetic-update' -Connection $connection -Confirm:$false; $null } catch { $_ }
        $VerbosePreference = 'SilentlyContinue'
        $companionResult = $companion.Shell.EndInvoke($companion.Handle)
        $companionErrors = @($companion.Shell.Streams.Error)
        $companion.Shell.Dispose()

        $failure.FullyQualifiedErrorId | Should -BeLike 'ActionPreferenceStop*'
        $failure.Exception | Should -BeOfType ([System.Management.Automation.ActionPreferenceStopException])
        # Sent or not before the stop, the change is never left running: a request that reached Files.com was abandoned.
        Wait-Until { @(Get-ApiRequest $service -Method PATCH -Path '/users/77' | Where-Object { -not $_.ClientAborted }).Count -eq 0 } -TimeoutSeconds 3
        @(Get-ApiRequest $service -Method PATCH -Path '/users/77').Count | Should -BeLessOrEqual 1
        $companionResult.Username | Should -Be 'companion'
        $companionErrors | Should -BeNullOrEmpty
        (Get-FilesUser -Id 5 -Connection $connection).Id | Should -Be 5
        $connection.IsOpen | Should -BeTrue
    }

    It 'cancels an upload whose progress stops the command, and never sends its final request' {
        $local = Join-Path $TestDrive 'stopped.bin'
        [System.IO.File]::WriteAllBytes($local, (New-RandomContent 5000))
        $service.ServeUpload('stopped.bin', 1000, $true)
        $service.DelayParts('stopped.bin', 300)
        $connection = Connect-WithSetting $service

        $ProgressPreference = 'Stop'
        $failure = try { Send-FilesFile -LiteralPath $local -RemotePath 'stopped.bin' -Connection $connection -Confirm:$false; $null } catch { $_ }
        $ProgressPreference = 'SilentlyContinue'

        $failure.FullyQualifiedErrorId | Should -BeLike 'ActionPreferenceStop*'
        Wait-Until { $service.ActiveRequests -eq 0 } -TimeoutSeconds 3
        Start-Sleep -Milliseconds 700
        @(Get-ApiRequest $service -Method POST -Path '/files/stopped.bin' | Where-Object { $_.Body -match '"action":"end"' }) | Should -BeNullOrEmpty
        $connection.IsOpen | Should -BeTrue
    }
}
