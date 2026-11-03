BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
}

Describe 'Workspace scope' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service -Options @{ WorkspaceId = 7 }
        $service.Respond('GET', '/users/1', 200, '{"id":1}')
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'runs one command in another workspace, including 0, or none, leaving the connection''s workspace for later commands' {
        Get-FilesUser -Id 1 -Connection $connection | Out-Null
        Get-FilesUser -Id 1 -Connection $connection -WorkspaceScope 0 | Out-Null
        Get-FilesUser -Id 1 -Connection $connection -NoWorkspaceScope | Out-Null
        Get-FilesUser -Id 1 -Connection $connection | Out-Null

        $headers = @(Get-ApiRequest $service -Path '/users/1' | ForEach-Object { $_.Header('X-Files-Workspace-Id') })
        $headers | Should -Be @('7', '0', $null, '7')
        $connection.WorkspaceId | Should -Be 7
    }

    It 'acts on an object in the workspace it was read in, including 0 or none, unless the command is given a scope itself' {
        $service.Respond('GET', '/users/2', 200, '{"id":2}')
        $service.Respond('GET', '/users', 200, '[{"id":3}]')
        foreach ($id in 1, 2, 3) {
            foreach ($answer in 1..3) {
                $service.Respond('PATCH', "/users/$id", 200, "{""id"":$id,""name"":""new""}")
            }
        }
        $inZero = Get-FilesUser -Id 1 -Connection $connection -WorkspaceScope 0
        $inNone = Get-FilesUser -Id 2 -Connection $connection -NoWorkspaceScope
        $listedInZero = Get-FilesUser -Connection $connection -WorkspaceScope 0 | Select-Object -First 1

        $inZero | Set-FilesUser -Name 'new' -Confirm:$false | Out-Null
        $inNone | Set-FilesUser -Name 'new' -Confirm:$false | Out-Null
        $listedInZero | Set-FilesUser -Name 'new' -Confirm:$false | Out-Null
        $inZero | Set-FilesUser -Name 'new' -WorkspaceScope 5 -Confirm:$false | Out-Null
        $inZero | Set-FilesUser -Name 'new' -Confirm:$false | Out-Null
        Set-FilesUser -Id 1 -Name 'new' -Connection $connection -Confirm:$false | Out-Null

        $patches = @(Get-ApiRequest $service -Method PATCH | ForEach-Object { $_.Header('X-Files-Workspace-Id') })
        # -WorkspaceScope 5 applies to its own command only: the object stays in the workspace it was read in.
        $patches | Should -Be @('0', $null, '0', '5', '0', '7')
        $connection.WorkspaceId | Should -Be 7
    }

    It 'refuses -WorkspaceScope together with -NoWorkspaceScope before sending anything' {
        { Get-FilesUser -Id 1 -Connection $connection -WorkspaceScope 3 -NoWorkspaceScope -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        $service.Requests.Count | Should -Be 0
    }
}
