BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')

    # The request a command sent to the route, whose body is multipart form data or JSON.
    function Get-SentRequest {
        param([FilesCom.PowerShell.Tests.FilesTestService] $Service, [string] $Method, [string] $Path)
        @(Get-ApiRequest $Service -Method $Method -Path $Path)
    }
}

Describe 'File parameters' {
    BeforeEach {
        $service = New-TestService
        $connection = Connect-TestService -Service $service
        $content = New-RandomContent 5000
        $local = Join-Path $TestDrive "avatar-$([guid]::NewGuid().ToString('N')).png"
        [System.IO.File]::WriteAllBytes($local, $content)
        $service.Respond('PATCH', '/users/7', 200, '{"id":7,"username":"ana"}')
    }
    AfterEach {
        Disconnect-FilesAccount -Connection $connection -Confirm:$false
        $service.Dispose()
    }

    It 'sends <Command> -<Parameter> as <Wire> in the same request as the other parameters' -TestCases @(
        @{ Command = 'New-FilesBehavior'; Parameter = 'AttachmentFile'; Wire = 'attachment_file'; Method = 'POST'; Route = '/behaviors'; Arguments = @{ Path = 'inbox'; Behavior = 'webhook' } }
        @{ Command = 'Set-FilesBehavior'; Parameter = 'AttachmentFile'; Wire = 'attachment_file'; Method = 'PATCH'; Route = '/behaviors/3'; Arguments = @{ Id = 3 } }
        @{ Command = 'New-FilesBundle'; Parameter = 'WatermarkAttachmentFile'; Wire = 'watermark_attachment_file'; Method = 'POST'; Route = '/bundles'; Arguments = @{ Paths = 'reports' } }
        @{ Command = 'Set-FilesBundle'; Parameter = 'WatermarkAttachmentFile'; Wire = 'watermark_attachment_file'; Method = 'PATCH'; Route = '/bundles/3'; Arguments = @{ Id = 3 } }
        @{ Command = 'New-FilesUser'; Parameter = 'AvatarFile'; Wire = 'avatar_file'; Method = 'POST'; Route = '/users'; Arguments = @{ Username = 'ana' } }
        @{ Command = 'Set-FilesUser'; Parameter = 'AvatarFile'; Wire = 'avatar_file'; Method = 'PATCH'; Route = '/users/7'; Arguments = @{ Id = 7 } }
        @{ Command = 'Set-FilesStyle'; Parameter = 'File'; Wire = 'file'; Method = 'PATCH'; Route = '/styles/branding'; Arguments = @{ Path = 'branding' } }
        @{ Command = 'Set-FilesSite'; Parameter = 'Icon16File'; Wire = 'icon16_file'; Method = 'PATCH'; Route = '/site'; Arguments = @{} }
        @{ Command = 'Set-FilesSite'; Parameter = 'Icon32File'; Wire = 'icon32_file'; Method = 'PATCH'; Route = '/site'; Arguments = @{} }
        @{ Command = 'Set-FilesSite'; Parameter = 'Icon48File'; Wire = 'icon48_file'; Method = 'PATCH'; Route = '/site'; Arguments = @{} }
        @{ Command = 'Set-FilesSite'; Parameter = 'Icon128File'; Wire = 'icon128_file'; Method = 'PATCH'; Route = '/site'; Arguments = @{} }
        @{ Command = 'Set-FilesSite'; Parameter = 'LogoFile'; Wire = 'logo_file'; Method = 'PATCH'; Route = '/site'; Arguments = @{} }
        @{ Command = 'Set-FilesSite'; Parameter = 'BundleWatermarkAttachmentFile'; Wire = 'bundle_watermark_attachment_file'; Method = 'PATCH'; Route = '/site'; Arguments = @{} }
        @{ Command = 'Set-FilesSite'; Parameter = 'LoginPageBackgroundImageFile'; Wire = 'login_page_background_image_file'; Method = 'PATCH'; Route = '/site'; Arguments = @{} }
    ) {
        param($Command, $Parameter, $Wire, $Method, $Route, $Arguments)
        $service.Respond($Method, $Route, 200, '{"id":3}')
        $invocation = @{} + $Arguments
        $invocation[$Parameter] = New-FilesAttachment -Content $content -FileName 'brand.png' -ContentType 'image/png'

        & $Command @invocation -Connection $connection -Confirm:$false | Out-Null

        $sent = Get-SentRequest -Service $service -Method $Method -Path $Route
        $sent.Count | Should -Be 1 -Because "$Command sends one request"
        $file = @($sent[0].FormParts() | Where-Object Name -EQ $Wire)
        $file.Count | Should -Be 1
        $file[0].FileName | Should -Be 'brand.png'
        $file[0].ContentType | Should -Be 'image/png'
        Test-SameContent -Expected $content -Actual $file[0].Content | Should -BeTrue
    }

    It 'sends a local file, and the other parameters exactly, as multipart form data' {
        Set-FilesUser -Id 7 -AvatarFile (Get-Item -LiteralPath $local) -Notes '' -ReceiveAdminAlerts $false -Connection $connection -Confirm:$false | Out-Null

        $sent = Get-SentRequest -Service $service -Method PATCH -Path '/users/7'
        $sent[0].Header('Content-Type') | Should -BeLike 'multipart/form-data;*'
        $parts = $sent[0].FormParts()
        $avatar = @($parts | Where-Object Name -EQ 'avatar_file')[0]
        $avatar.FileName | Should -Be (Split-Path -Leaf $local)
        Test-SameContent -Expected $content -Actual $avatar.Content | Should -BeTrue
        # An empty text and $false go as they would without a file.
        @($parts | Where-Object Name -EQ 'notes')[0].Text | Should -Be ''
        @($parts | Where-Object Name -EQ 'receive_admin_alerts')[0].Text | Should -Be 'false'
    }

    It 'sends JSON with the file as encoded_content when a form cannot carry the file name unchanged' {
        $attachment = New-FilesAttachment -Content $content -FileName 'quote"d.png'

        Set-FilesUser -Id 7 -AvatarFile $attachment -ReceiveAdminAlerts $false -Connection $connection -Confirm:$false | Out-Null

        $sent = Get-SentRequest -Service $service -Method PATCH -Path '/users/7'
        $sent[0].Header('Content-Type') | Should -BeLike 'application/json*'
        $body = $sent[0].Body | ConvertFrom-Json
        $body.avatar_file.filename | Should -Be 'quote"d.png'
        Test-SameContent -Expected $content -Actual ([Convert]::FromBase64String($body.avatar_file.encoded_content)) | Should -BeTrue
        $body.receive_admin_alerts | Should -BeFalse
    }

    It 'reads a borrowed stream that can seek from its position, and leaves it open' {
        $stream = [System.IO.MemoryStream]::new($content)
        $stream.Position = 1000

        Set-FilesUser -Id 7 -AvatarFile (New-FilesAttachment -InputStream $stream -FileName 'tail.bin') -Connection $connection -Confirm:$false | Out-Null

        $avatar = @((Get-SentRequest -Service $service -Method PATCH -Path '/users/7')[0].FormParts() | Where-Object Name -EQ 'avatar_file')[0]
        Test-SameContent -Expected $content[1000..4999] -Actual $avatar.Content | Should -BeTrue
        $stream.CanRead | Should -BeTrue
        $stream.Dispose()
    }

    It 'copies a stream that cannot seek within -MaxLength, and sends nothing when it holds more' {
        $within = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)
        $over = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)

        Set-FilesUser -Id 7 -AvatarFile (New-FilesAttachment -InputStream $within -FileName 'live.bin' -MaxLength 5000) -Connection $connection -Confirm:$false | Out-Null
        { Set-FilesUser -Id 7 -AvatarFile (New-FilesAttachment -InputStream $over -FileName 'live.bin' -MaxLength 4999) -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'

        $sent = Get-SentRequest -Service $service -Method PATCH -Path '/users/7'
        $sent.Count | Should -Be 1
        Test-SameContent -Expected $content -Actual (@($sent[0].FormParts() | Where-Object Name -EQ 'avatar_file')[0].Content) | Should -BeTrue
        $within.IsDisposed | Should -BeFalse
        $over.IsDisposed | Should -BeFalse
    }

    It 'holds a stream that cannot seek to its documented 64 MiB when no -MaxLength is given, and sends nothing when longer' {
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new([byte[]]::new(64MB + 1), 1MB)

        { Set-FilesUser -Id 7 -AvatarFile (New-FilesAttachment -InputStream $stream -FileName 'big.bin') -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'

        [FilesCom.PowerShell.FilesAttachment]::DefaultMaxLength | Should -Be 64MB
        (Get-SentRequest -Service $service -Method PATCH -Path '/users/7').Count | Should -Be 0
        $stream.IsDisposed | Should -BeFalse
    }

    It 'passes on a failure of the attachment''s stream exactly as it was raised, and sends nothing' {
        $sourceFailure = [System.InvalidOperationException]::new('The camera lost its image buffer.')
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700, 2000, 0, $sourceFailure)

        { Set-FilesUser -Id 7 -AvatarFile (New-FilesAttachment -InputStream $stream -FileName 'photo.png') -Connection $connection -Confirm:$false -ErrorAction Stop } | Should -Throw -ErrorId 'Files.Unexpected*'

        [object]::ReferenceEquals($Error[0].Exception, $sourceFailure) | Should -BeTrue
        $Error[0].ErrorDetails | Should -BeNullOrEmpty
        (Get-SentRequest -Service $service -Method PATCH -Path '/users/7').Count | Should -Be 0
        $stream.IsDisposed | Should -BeFalse
    }

    It 'reads nothing under -WhatIf, and refuses a missing local file before asking to confirm' {
        $stream = [FilesCom.PowerShell.Tests.ForwardOnlyStream]::new($content, 700)
        $missing = [System.IO.FileInfo] (Join-Path $TestDrive 'missing.png')

        Set-FilesUser -Id 7 -AvatarFile (New-FilesAttachment -InputStream $stream -FileName 'x.png') -Connection $connection -WhatIf
        Set-FilesUser -Id 7 -AvatarFile $missing -Connection $connection -Confirm:$false -ErrorVariable failures -ErrorAction SilentlyContinue

        $stream.BytesRead | Should -Be 0
        $failures[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'
        $service.Requests.Count | Should -Be 0
    }

    It 'makes attachments only of names and media types a request can carry, and of one existing file' {
        $second = Join-Path $TestDrive 'second.png'
        [System.IO.File]::WriteAllBytes($second, [byte[]](1, 2, 3))

        { New-FilesAttachment -Content $content -FileName "line`nbreak.png" -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { New-FilesAttachment -Content $content -FileName 'x.png' -ContentType 'not a type' -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        { New-FilesAttachment -Path (Join-Path $TestDrive '*.png') -ErrorAction Stop } | Should -Throw -ErrorId 'Files.InvalidArgument*'
        # As for Send-FilesFile: -Path resolves as PowerShell does, -LiteralPath is checked by the module.
        { New-FilesAttachment -Path (Join-Path $TestDrive 'nothing-here.png') -ErrorAction Stop } | Should -Throw -ErrorId 'PathNotFound*'
        New-FilesAttachment -LiteralPath (Join-Path $TestDrive 'nothing-here.png') -ErrorVariable missing -ErrorAction SilentlyContinue
        $missing[0].FullyQualifiedErrorId | Should -BeLike 'Files.LocalFile*'

        $made = New-FilesAttachment -LiteralPath $second -ContentType 'image/png'
        $made.SourceKind | Should -Be 'File'
        $made.FileName | Should -Be 'second.png'
        $made.LocalPath | Should -Be (Get-Item -LiteralPath $second).FullName
        (New-FilesAttachment -InputStream ([System.IO.MemoryStream]::new()) -FileName 's.bin' -MaxLength 10).MaxLength | Should -Be 10
    }

    It 'stops a request carrying a file when the pipeline stops, and leaves the stream open' {
        $service.Hang('PATCH', '/users/8')
        $stream = [System.IO.MemoryStream]::new($content)
        $running = Start-Command -Connection $connection -Arguments $stream -Script {
            param($Connection, $Stream)
            Set-FilesUser -Id 8 -AvatarFile (New-FilesAttachment -InputStream $Stream -FileName 'a.bin') -Connection $Connection -Confirm:$false
        }
        Wait-Until { @(Get-ApiRequest $service -Method PATCH -Path '/users/8').Count -eq 1 }

        $elapsed = Measure-Command { $running.Shell.Stop() }

        $elapsed.TotalSeconds | Should -BeLessThan 5
        $running.Shell.InvocationStateInfo.State | Should -Be 'Stopped'
        $running.Shell.Dispose()
        $stream.CanRead | Should -BeTrue
        $stream.Dispose()
    }
}
