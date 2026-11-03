BeforeDiscovery {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
    # Every generated command that changes something, from the module's own metadata.
    $script:ChangingCommands = Get-Command -Module FilesCom.PowerShell |
        Where-Object { $_.Parameters.ContainsKey('WhatIf') -and $_.Name -notin 'Connect-FilesAccount', 'Disconnect-FilesAccount', 'Send-FilesFile', 'Receive-FilesFile', 'Out-FilesLocalFile', 'Send-FilesDirectory', 'Set-FilesFileContent' } |
        ForEach-Object { @{ Name = $_.Name } }
}

BeforeDiscovery {
    # Changes to credentials, keys, sign-in and access rights, and site security policy, which confirm as deletions do.
    $script:SecurityChanges = @(
        'New-FilesUser', 'Set-FilesUser', 'Unlock-FilesUser', 'New-FilesPermission', 'New-FilesGroup', 'Set-FilesGroup',
        'New-FilesGroupUser', 'Set-FilesGroupUser', 'New-FilesApiKey', 'Set-FilesApiKey', 'Remove-FilesCurrentApiKey',
        'New-FilesPublicKey', 'Set-FilesGpgKey', 'Set-FilesSite', 'Set-FilesPartner', 'Set-FilesRemoteServerCredential'
    ) | ForEach-Object { @{ Name = $_ } }
}

BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')

    # Runs a script in a runspace without a user interface, which cannot answer a confirmation, so any prompt declines.
    function Invoke-WithoutPrompt {
        param([scriptblock] $Script, $Connection)
        $shell = [powershell]::Create()
        try {
            $shell.AddScript('param($Manifest) Import-Module -Name $Manifest').AddArgument($script:ModuleManifest).AddStatement() | Out-Null
            $shell.AddScript($Script).AddArgument($Connection) | Out-Null
            $shell.Invoke() | Out-Null
        }
        finally {
            $shell.Dispose()
        }
    }

    # A value of the parameter's type, for filling mandatory parameters.
    function New-SampleValue {
        param([System.Management.Automation.ParameterMetadata] $Parameter)
        $type = $Parameter.ParameterType
        if ($type -eq [string]) { return 'sample' }
        if ($type -eq [long] -or $type -eq [Nullable[long]]) { return 1 }
        if ($type -eq [bool] -or $type -eq [Nullable[bool]]) { return $true }
        if ($type -eq [double] -or $type -eq [Nullable[double]] -or $type -eq [decimal] -or $type -eq [Nullable[decimal]]) { return 1.5 }
        if ($type -eq [DateTimeOffset] -or $type -eq [Nullable[DateTimeOffset]]) { return [DateTimeOffset]::UtcNow }
        if ($type -eq [string[]]) { return @('sample') }
        if ($type -eq [long[]]) { return @(1) }
        if ($type -eq [System.Collections.IDictionary]) { return @{ key = 'value' } }
        if ($type -eq [System.Collections.IDictionary[]]) { return @(@{ key = 'value' }) }
        if ($type -eq [securestring]) { return (ConvertTo-SecureString 'sample' -AsPlainText -Force) }
        throw "No sample value for $type"
    }
}

Describe 'Changes under -WhatIf' {
    BeforeAll {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
    }
    AfterAll {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It '<Name> -WhatIf sends nothing' -TestCases $script:ChangingCommands {
        $command = Get-Command -Name $Name
        $set = $command.ParameterSets | Where-Object IsDefault
        if (-not $set) { $set = $command.ParameterSets[0] }
        $arguments = @{ Connection = $connection; WhatIf = $true; ErrorAction = 'Stop' }
        foreach ($parameter in $set.Parameters | Where-Object { $_.IsMandatory }) {
            $arguments[$parameter.Name] = New-SampleValue $command.Parameters[$parameter.Name]
        }
        $service.ClearRequests()

        & $Name @arguments | Out-Null

        $service.Requests.Count | Should -Be 0
    }

    It '<Name> confirms at high impact' -TestCases $script:SecurityChanges {
        $cmdlet = (Get-Command -Name $Name).ImplementingType.GetCustomAttributes([System.Management.Automation.CmdletAttribute], $true)[0]
        $cmdlet.ConfirmImpact | Should -Be 'High'
    }

    It 'asks before granting admin rights by default, sending nothing unless confirmed, and keeps secrets out of the question' {
        $service.Respond('PATCH', '/users/1', 200, '{"id":1,"site_admin":true}')
        $service.ClearRequests()

        Invoke-WithoutPrompt -Connection $connection -Script {
            param($Connection)
            Set-FilesUser -Id 1 -SiteAdmin $true -Password (ConvertTo-SecureString 'sentinel-password-9K4' -AsPlainText -Force) -Connection $Connection
        }
        $service.Requests.Count | Should -Be 0
        $transcript = Join-Path $TestDrive 'what-if.txt'
        Start-Transcript -Path $transcript | Out-Null
        try {
            Set-FilesUser -Id 1 -SiteAdmin $true -Password (ConvertTo-SecureString $script:PasswordSentinel -AsPlainText -Force) -Connection $connection -WhatIf
        }
        finally {
            Stop-Transcript | Out-Null
        }
        $question = Get-Content -LiteralPath $transcript -Raw
        $question | Should -Match 'What if: .*User 1'
        $question | Should -Not -Match ([regex]::Escape($script:PasswordSentinel))
        $service.Requests.Count | Should -Be 0

        Set-FilesUser -Id 1 -SiteAdmin $true -Connection $connection -Confirm:$false | Out-Null
        (Get-ApiRequest $service -Method PATCH -Path '/users/1').Count | Should -Be 1
    }

    It 'describes a folder download under -WhatIf without creating its local directory or checkpoint folder, by path or for a piped folder' {
        $service.Respond('GET', '/file_actions/metadata/reports', 200, '{"path":"reports","type":"directory"}')
        $folder = Get-FilesFile -Path 'reports' -Connection $connection
        $local = Join-Path $TestDrive 'what-if-local/reports'
        $checkpoints = Join-Path $TestDrive 'what-if-checkpoints'
        $service.ClearRequests()
        $transcript = Join-Path $TestDrive 'what-if-directory.txt'
        Start-Transcript -Path $transcript | Out-Null
        try {
            Receive-FilesDirectory -RemotePath 'reports' -LiteralPath $local -CheckpointDirectory $checkpoints -Force -Connection $connection -WhatIf
            $folder | Receive-FilesDirectory -LiteralPath $local -CheckpointDirectory $checkpoints -WhatIf
            Receive-FilesDirectory -RemotePath '' -LiteralPath $local -Connection $connection -WhatIf
        }
        finally {
            Stop-Transcript | Out-Null
        }

        $question = Get-Content -LiteralPath $transcript -Raw
        ([regex]::Matches($question, "What if: .*folder 'reports' and everything in it")).Count | Should -Be 2
        $question | Should -Match 'replacing regular files there'
        $question | Should -Match "What if: .*the site's root folder and everything in it"
        $service.Requests.Count | Should -Be 0
        Test-Path -LiteralPath (Join-Path $TestDrive 'what-if-local') | Should -BeFalse
        Test-Path -LiteralPath $checkpoints | Should -BeFalse
    }

    It 'makes an ordinary change without asking' {
        $service.Respond('PATCH', '/file_comments/1', 200, '{"id":1,"body":"hello"}')
        $service.ClearRequests()

        Invoke-WithoutPrompt -Connection $connection -Script {
            param($Connection)
            Set-FilesFileComment -Id 1 -Body 'hello' -Connection $Connection
        }

        (Get-ApiRequest $service -Method PATCH -Path '/file_comments/1').Count | Should -Be 1
    }

    It 'asks before a deletion by default, sending nothing unless confirmed' {
        $service.Respond('DELETE', '/users/1', 204, $null)
        $service.ClearRequests()
        $shell = [powershell]::Create()
        try {
            # A runspace without a user interface cannot ask, so the deletion is declined.
            $shell.AddScript({
                    param($Manifest, $Connection)
                    Import-Module -Name $Manifest
                    Remove-FilesUser -Id 1 -Connection $Connection
                }).AddArgument($script:ModuleManifest).AddArgument($connection) | Out-Null
            $shell.Invoke() | Out-Null
        }
        finally {
            $shell.Dispose()
        }
        $service.Requests.Count | Should -Be 0

        Remove-FilesUser -Id 1 -Connection $connection -Confirm:$false
        (Get-ApiRequest $service -Method DELETE -Path '/users/1').Count | Should -Be 1
    }
}
