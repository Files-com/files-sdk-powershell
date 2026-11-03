BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')

    # The origin of a test service: its scheme, host and port.
    function Get-Origin {
        param([FilesCom.PowerShell.Tests.FilesTestService] $Service)
        $Service.BaseUrl.TrimEnd('/')
    }

    # Files.com's answer for a site served from another region's address, which it names in data.host.
    function Get-MismatchJson {
        param([string] $Origin)
        "{""error"":""You are attempting to access a site in a different region."",""http-code"":403,""title"":""Lockout Region Mismatch"",""type"":""not-authenticated/lockout-region-mismatch"",""data"":{""host"":""$Origin""}}"
    }
}

Describe 'Region relocation' {
    BeforeEach {
        # $site is where the connection points; $region is where Files.com says the site is served from.
        $site = New-TestService
        $region = New-TestService
        $siteOrigin = Get-Origin $site
        $regionOrigin = Get-Origin $region
    }
    AfterEach {
        Get-FilesConnection -All | Disconnect-FilesAccount -Confirm:$false
        $site.Dispose()
        $region.Dispose()
    }

    It 'moves a command to a listed origin, and keeps the pages and objects it writes there for later commands' {
        $site.Respond('GET', '/users', 403, (Get-MismatchJson $regionOrigin))
        $region.Pages('/users', (ConvertTo-UserJson 1), (ConvertTo-UserJson 2))
        $region.Respond('PATCH', '/users/1', 200, '{"id":1,"username":"user1","notes":"moved"}')
        $connection = Connect-TestService -Service $site -Options @{ MaxNetworkRetries = 1; RegionRelocation = @{ $siteOrigin = $regionOrigin } }

        $page = Get-FilesUser -Page -Connection $connection
        $next = Get-FilesUser -Continuation $page.Continuation
        $updated = $page.Items[0] | Set-FilesUser -Notes 'moved' -Confirm:$false

        $page.Items.Id | Should -Be 1
        $next.Items.Id | Should -Be 2
        $updated.Notes | Should -Be 'moved'
        # Only the first request went to the connection's own address.
        @($site.Requests).Count | Should -Be 1
        @(Get-ApiRequest $region -Method GET -Path '/users').Count | Should -Be 2
        @(Get-ApiRequest $region -Method PATCH -Path '/users/1').Count | Should -Be 1
        $connection.BaseUrl | Should -Be $site.BaseUrl
        $connection.RegionRelocation.Kind | Should -Be 'Listed'
        "$($connection.RegionRelocation)" | Should -Be "$siteOrigin -> $regionOrigin"
    }

    It 'starts every other command at the connection''s own address, and an -InputObject with its own workspace scope too' {
        $site.Respond('GET', '/users/1', 403, (Get-MismatchJson $regionOrigin))
        $site.Respond('PATCH', '/users/1', 403, (Get-MismatchJson $regionOrigin))
        $region.Respond('GET', '/users/1', 200, '{"id":1,"username":"user1"}')
        $region.Respond('PATCH', '/users/1', 200, '{"id":1,"username":"user1"}')
        $connection = Connect-TestService -Service $site -Options @{ MaxNetworkRetries = 1; RegionRelocation = @{ $siteOrigin = $regionOrigin } }

        $user = Get-FilesUser -Id 1 -Connection $connection
        $user | Set-FilesUser -Notes 'x' -WorkspaceScope 5 -Confirm:$false
        Get-FilesUser -Id 1 -Connection $connection | Out-Null

        # The scoped update and the second read each started at the site's address and moved again.
        @(Get-ApiRequest $site -Method PATCH -Path '/users/1').Count | Should -Be 1
        @(Get-ApiRequest $site -Method GET -Path '/users/1').Count | Should -Be 2
        (Get-ApiRequest $region -Method PATCH -Path '/users/1')[0].Header('X-Files-Workspace-Id') | Should -Be '5'
    }

    It 'resends a file''s same bytes to the region it moves to' {
        $content = New-RandomContent 3000
        $site.Respond('PATCH', '/users/7', 403, (Get-MismatchJson $regionOrigin))
        $region.Respond('PATCH', '/users/7', 200, '{"id":7}')
        $connection = Connect-TestService -Service $site -Options @{ MaxNetworkRetries = 1; RegionRelocation = @{ $siteOrigin = $regionOrigin } }

        Set-FilesUser -Id 7 -AvatarFile (New-FilesAttachment -Content $content -FileName 'a.png') -Connection $connection -Confirm:$false | Out-Null

        $moved = @(Get-ApiRequest $region -Method PATCH -Path '/users/7')
        $moved.Count | Should -Be 1
        Test-SameContent -Expected $content -Actual (@($moved[0].FormParts() | Where-Object Name -EQ 'avatar_file')[0].Content) | Should -BeTrue
    }

    It 'runs <Command> -<Destination> on a file read in the region there, not at the connection''s own address' -TestCases @(
        @{ Command = 'Copy-FilesFile'; Action = 'copy'; Destination = 'Destination'; Sent = 'archive/q3.csv' }
        @{ Command = 'Copy-FilesFile'; Action = 'copy'; Destination = 'RemoteServerId'; Sent = '_/RemoteServers/5/archive/q3.csv' }
        @{ Command = 'Copy-FilesFile'; Action = 'copy'; Destination = 'SnapshotId'; Sent = '_/Snapshots/5/archive/q3.csv' }
        @{ Command = 'Copy-FilesFile'; Action = 'copy'; Destination = 'ChildSiteId'; Sent = '_/Sites/5/archive/q3.csv' }
        @{ Command = 'Move-FilesFile'; Action = 'move'; Destination = 'Destination'; Sent = 'archive/q3.csv' }
        @{ Command = 'Move-FilesFile'; Action = 'move'; Destination = 'RemoteServerId'; Sent = '_/RemoteServers/5/archive/q3.csv' }
        @{ Command = 'Move-FilesFile'; Action = 'move'; Destination = 'SnapshotId'; Sent = '_/Snapshots/5/archive/q3.csv' }
        @{ Command = 'Move-FilesFile'; Action = 'move'; Destination = 'ChildSiteId'; Sent = '_/Sites/5/archive/q3.csv' }
    ) {
        param($Command, $Action, $Destination, $Sent)
        $site.Respond('GET', '/file_actions/metadata/reports/q3.csv', 403, (Get-MismatchJson $regionOrigin))
        $region.Respond('GET', '/file_actions/metadata/reports/q3.csv', 200, '{"path":"reports/q3.csv","type":"file"}')
        # The connection's own address can no longer act on the file; the region it was read in can.
        $site.Respond('POST', "/file_actions/$Action/reports/q3.csv", 503, '{"error":"temporarily unavailable"}')
        $region.Respond('POST', "/file_actions/$Action/reports/q3.csv", 200, '{"status":"enqueued","file_migration_id":11}')
        $connection = Connect-TestService -Service $site -Options @{ MaxNetworkRetries = 1; RegionRelocation = @{ $siteOrigin = $regionOrigin } }
        $file = Get-FilesFile -Path 'reports/q3.csv' -Connection $connection
        $send = @{ InputObject = $file; Confirm = $false; ErrorAction = 'Stop' }
        if ($Destination -eq 'Destination') {
            $send.Destination = 'archive/q3.csv'
        }
        else {
            $send[$Destination] = 5
            $send.DestinationPath = 'archive/q3.csv'
        }

        $result = & $Command @send

        $result.FileMigrationId | Should -Be 11
        @(Get-ApiRequest $site -Method POST -Path "/file_actions/$Action/reports/q3.csv").Count | Should -Be 0
        $received = @(Get-ApiRequest $region -Method POST -Path "/file_actions/$Action/reports/q3.csv")
        $received.Count | Should -Be 1
        ($received[0].Body | ConvertFrom-Json).destination | Should -BeExactly $Sent
    }

    It 'starts an external copy at the connection''s own address when given -Path, or a file with its own workspace scope' {
        $site.Respond('GET', '/file_actions/metadata/reports/q3.csv', 403, (Get-MismatchJson $regionOrigin))
        $region.Respond('GET', '/file_actions/metadata/reports/q3.csv', 200, '{"path":"reports/q3.csv","type":"file"}')
        $site.Respond('POST', '/file_actions/copy/reports/q3.csv', 403, (Get-MismatchJson $regionOrigin))
        $region.Respond('POST', '/file_actions/copy/reports/q3.csv', 200, '{"status":"enqueued","file_migration_id":11}')
        $connection = Connect-TestService -Service $site -Options @{ MaxNetworkRetries = 1; RegionRelocation = @{ $siteOrigin = $regionOrigin } }
        $file = Get-FilesFile -Path 'reports/q3.csv' -Connection $connection

        Copy-FilesFile -Path 'reports/q3.csv' -RemoteServerId 5 -DestinationPath 'archive/q3.csv' -Connection $connection -Confirm:$false | Out-Null
        $file | Copy-FilesFile -RemoteServerId 5 -DestinationPath 'archive/q3.csv' -WorkspaceScope 7 -Confirm:$false | Out-Null

        # Each copy started at the site's address and moved again.
        @(Get-ApiRequest $site -Method POST -Path '/file_actions/copy/reports/q3.csv').Count | Should -Be 2
        $moved = @(Get-ApiRequest $region -Method POST -Path '/file_actions/copy/reports/q3.csv')
        $moved.Count | Should -Be 2
        $moved[1].Header('X-Files-Workspace-Id') | Should -Be '7'
    }

    It 'refuses to move <Label>, naming the site''s address and why, with what to do' -TestCases @(
        @{ Label = 'over plain HTTP under Automatic'; Options = @{ MaxNetworkRetries = 1 }; Refusal = 'UntrustedIssuer'; Advice = '-RegionRelocation @{' }
        @{ Label = 'when -RegionRelocation is Disabled'; Options = @{ MaxNetworkRetries = 1; RegionRelocation = 'Disabled' }; Refusal = 'Disabled'; Advice = '-RegionRelocation Automatic' }
        @{ Label = 'without a retry to move with'; Options = @{ MaxNetworkRetries = 0; RegionRelocation = '<pair>' }; Refusal = 'RetriesExhausted'; Advice = '-MaxNetworkRetries' }
    ) {
        param($Label, $Options, $Refusal, $Advice)
        $site.Respond('GET', '/users/1', 403, (Get-MismatchJson $regionOrigin))
        $settings = @{} + $Options
        if ($settings.RegionRelocation -eq '<pair>') { $settings.RegionRelocation = @{ $siteOrigin = $regionOrigin } }
        $connection = Connect-TestService -Service $site -Options $settings

        { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } | Should -Throw -ErrorId 'Files.RegionMismatch*' -Because $Label

        $failure = $Error[0]
        $failure.CategoryInfo.Category | Should -Be 'ResourceUnavailable'
        $failure.Exception.RequestedOrigin | Should -Be $regionOrigin
        "$($failure.Exception.RegionRelocationRefusal)" | Should -Be $Refusal
        $failure.Exception.HttpStatus | Should -Be 403
        $failure.Exception.ErrorType | Should -Be 'not-authenticated/lockout-region-mismatch'
        $failure.ErrorDetails.RecommendedAction | Should -Match ([regex]::Escape($Advice))
        $failure.Exception.Message | Should -Not -Match 'FilesConfiguration|RegionRelocationPolicy'
        $region.Requests.Count | Should -Be 0
    }

    It 'names the pair that would allow an unverifiable move in the module''s own terms' {
        $site.Respond('GET', '/users/1', 403, (Get-MismatchJson $regionOrigin))
        $connection = Connect-TestService -Service $site -Options @{ MaxNetworkRetries = 1 }

        { Get-FilesUser -Id 1 -Connection $connection -ErrorAction Stop } | Should -Throw -ErrorId 'Files.RegionMismatch*'

        $Error[0].Exception.Message | Should -Match ([regex]::Escape("Connect-FilesAccount -RegionRelocation @{ '$siteOrigin' = '$regionOrigin' } allows this move"))
    }

    It 'takes Automatic, Disabled or origin pairs, and refuses anything else before connecting' {
        $automatic = Connect-TestService -Service $site
        $disabled = Connect-TestService -Service $site -Options @{ RegionRelocation = 'disabled' }
        $listed = Connect-TestService -Service $site -Options @{ RegionRelocation = @{ "$siteOrigin/" = $regionOrigin, 'https://files.example.eu' } }

        $automatic.RegionRelocation.Kind | Should -Be 'Automatic'
        $disabled.RegionRelocation.Kind | Should -Be 'Disabled'
        $listed.RegionRelocation.Pairs.Count | Should -Be 2
        $listed.RegionRelocation.Pairs[0].From | Should -Be $siteOrigin
        { Connect-TestService -Service $site -Options @{ RegionRelocation = 'Sometimes' } } | Should -Throw -ErrorId 'ParameterArgumentTransformationError*'
        { Connect-TestService -Service $site -Options @{ RegionRelocation = @{ "$siteOrigin/api" = $regionOrigin } } } | Should -Throw -ErrorId 'ParameterArgumentTransformationError*'
        { Connect-TestService -Service $site -Options @{ RegionRelocation = @{ 'ftp://files.example.com' = $regionOrigin } } } | Should -Throw -ErrorId 'ParameterArgumentTransformationError*'
        $site.Requests.Count | Should -Be 0
    }
}
