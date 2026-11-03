BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
}

Describe 'Automatic paging' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'writes every object of every page, typed, in order' {
        $service.Pages('/users', (ConvertTo-UserJson 1, 2), (ConvertTo-UserJson 3), (ConvertTo-UserJson 4, 5))

        $users = Get-FilesUser -Connection $connection

        $users.Id | Should -Be @(1, 2, 3, 4, 5)
        $users[0] | Should -BeOfType ([FilesCom.PowerShell.Models.User])
        (Get-ApiRequest $service -Path '/users').Count | Should -Be 3
    }

    It 'continues past an empty page that still has a cursor' {
        $service.Pages('/users', (ConvertTo-UserJson 1), '[]', (ConvertTo-UserJson 2))

        (Get-FilesUser -Connection $connection).Id | Should -Be @(1, 2)
    }

    It 'loads no further page once -First objects are written' {
        $service.Pages('/users', (ConvertTo-UserJson 1, 2), (ConvertTo-UserJson 3, 4), (ConvertTo-UserJson 5))

        (Get-FilesUser -Connection $connection -First 2).Id | Should -Be @(1, 2)
        (Get-ApiRequest $service -Path '/users').Count | Should -Be 1
    }

    It 'loads no further page after Select-Object -First stops the pipeline' {
        $service.Pages('/users', (ConvertTo-UserJson 1, 2), (ConvertTo-UserJson 3, 4))

        (Get-FilesUser -Connection $connection | Select-Object -First 1).Id | Should -Be 1
        (Get-ApiRequest $service -Path '/users').Count | Should -Be 1
    }

    It 'skips objects with -Skip and still counts pages' {
        $service.Pages('/users', (ConvertTo-UserJson 1, 2), (ConvertTo-UserJson 3))

        (Get-FilesUser -Connection $connection -Skip 2).Id | Should -Be @(3)
    }

    It 'stops with an error when a later page fails, after writing the earlier objects' {
        $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 1, 2), @{ 'X-Files-Cursor-Next' = 'next' })
        $service.Respond('GET', '/users', 503, '{"error":"Down","http-code":503,"type":"service-unavailable/site-disabled"}')
        $written = [System.Collections.Generic.List[object]]::new()

        { Get-FilesUser -Connection $connection -ErrorAction Continue | ForEach-Object { $written.Add($_) } } | Should -Throw -ErrorId 'Files.ServiceUnavailable*'
        $written.Id | Should -Be @(1, 2)
    }

    It 'stops with an error, after the earlier objects, when a page has no items and the cursor it was asked for' {
        $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 1), @{ 'X-Files-Cursor-Next' = 'same' })
        $service.Respond('GET', '/users', 200, '[]', @{ 'X-Files-Cursor-Next' = 'same' })
        $written = [System.Collections.Generic.List[object]]::new()

        { Get-FilesUser -Connection $connection -ErrorAction Continue | ForEach-Object { $written.Add($_) } } | Should -Throw -ErrorId 'Files.InvalidResponse*'
        $written.Id | Should -Be @(1)
        @(Get-ApiRequest $service -Path '/users').Count | Should -Be 2
    }

    It 'sends query parameters on every page, including the initial -Cursor' {
        $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 1), @{ 'X-Files-Cursor-Next' = 'second' })
        $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 2))

        Get-FilesUser -Connection $connection -Cursor 'start' -PerPage 1 -Search 'al' | Out-Null

        $requests = @(Get-ApiRequest $service -Path '/users')
        $requests[0].Query['cursor'] | Should -Be 'start'
        $requests[1].Query['cursor'] | Should -Be 'second'
        $requests.ForEach({ $_.Query['per_page'] }) | Should -Be @('1', '1')
        $requests.ForEach({ $_.Query['search'] }) | Should -Be @('al', 'al')
    }
}

Describe 'Manual paging' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $service.Pages('/users', (ConvertTo-UserJson 1), '[]', (ConvertTo-UserJson 2))
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'writes pages with -Page and -Continuation, including an empty page, until the last' {
        $first = Get-FilesUser -Connection $connection -Page
        $second = Get-FilesUser -Continuation $first.Continuation
        $third = Get-FilesUser -Continuation $second.Continuation

        $first | Should -BeOfType ([FilesCom.PowerShell.FilesPage[FilesCom.PowerShell.Models.User]])
        $first.Items.Id | Should -Be @(1)
        $second.Items.Count | Should -Be 0
        $second.HasMore | Should -BeTrue
        $third.Items.Id | Should -Be @(2)
        $third.HasMore | Should -BeFalse
        $third.Continuation | Should -BeNullOrEmpty
    }

    It 'loads a continuation''s page only once' {
        $first = Get-FilesUser -Connection $connection -Page
        Get-FilesUser -Continuation $first.Continuation | Out-Null

        { Get-FilesUser -Continuation $first.Continuation -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        $first.Continuation.IsUsed | Should -BeTrue
        (Get-ApiRequest $service -Path '/users').Count | Should -Be 2
    }

    It 'refuses a continuation of another command or another connection' {
        $first = Get-FilesUser -Connection $connection -Page
        $other = Connect-TestService -Service $service -ApiKey 'other'

        { Get-FilesGroup -Continuation $first.Continuation -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { Get-FilesUser -Continuation $first.Continuation -Connection $other -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        $first.Continuation.IsUsed | Should -BeFalse
        Disconnect-FilesAccount -Connection $other -Confirm:$false
    }

    It 'refuses the continuation of another command that lists the same kind of object, and keeps it for its own command' {
        $service.Pages('/history', '[{"id":1}]', '[{"id":2}]')
        $first = Get-FilesHistory -Connection $connection -Page

        { Get-FilesLoginHistory -Continuation $first.Continuation -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        $first.Continuation.IsUsed | Should -BeFalse
        @(Get-ApiRequest $service -Path '/history').Count | Should -Be 1
        (Get-FilesHistory -Continuation $first.Continuation).Items.Id | Should -Be @(2)
    }

    It 'keeps a continuation usable after its page fails to load' {
        $service.Dispose()
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 1), @{ 'X-Files-Cursor-Next' = 'next' })
        $service.Respond('GET', '/users', 500, 'not json')
        $service.Respond('GET', '/users', 200, (ConvertTo-UserJson 2))
        $first = Get-FilesUser -Connection $connection -Page

        { Get-FilesUser -Continuation $first.Continuation -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidResponse*'
        (Get-FilesUser -Continuation $first.Continuation).Items.Id | Should -Be @(2)
    }

    It 'refuses -First with -Page' {
        { Get-FilesUser -Connection $connection -Page -First 1 -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        $service.Requests.Count | Should -Be 0
    }
}
