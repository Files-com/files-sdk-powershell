BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
}

Describe 'Download modification time checks' {
    It 'keeps date checking <Enabled> for <Label>' -TestCases @(
        @{ Label = 'the default connection'; Client = $null; Override = $null; Enabled = $true; Stream = $false }
        @{ Label = 'a disabled connection'; Client = $false; Override = $null; Enabled = $false; Stream = $false }
        @{ Label = 'a disabled file override'; Client = $true; Override = $false; Enabled = $false; Stream = $false }
        @{ Label = 'an enabled file override'; Client = $false; Override = $true; Enabled = $true; Stream = $false }
        @{ Label = 'a disabled stream override'; Client = $true; Override = $false; Enabled = $false; Stream = $true }
    ) {
        param($Client, $Override, $Enabled, $Stream)
        $service = New-TestService
        $connection = $null
        $destination = [System.IO.MemoryStream]::new()
        try {
            $content = [System.Text.Encoding]::UTF8.GetBytes('0123456789')
            $service.ServeDatedDownload('dated.bin', $content, 'Wed, 21 Oct 2015 07:28:00 GMT', 'Wed, 21 Oct 2015 07:28:01 GMT')
            $connectionOptions = @{}
            if ($null -ne $Client) { $connectionOptions.CheckDownloadModificationTime = $Client }
            $connection = Connect-TestService -Service $service -InMemory -Options $connectionOptions
            $path = Join-Path $TestDrive (([guid]::NewGuid().ToString('N')) + '.bin')
            $arguments = @{ RemotePath = 'dated.bin'; Connection = $connection; PartSize = 4; PassThru = $true; Confirm = $false; ErrorAction = 'Stop' }
            if ($Stream) { $arguments.OutputStream = $destination } else { $arguments.LiteralPath = $path }
            if ($null -ne $Override) { $arguments.CheckDownloadModificationTime = $Override }
            if ($Enabled) {
                { Receive-FilesFile @arguments } | Should -Throw -ErrorId 'Files.SourceChanged*'
                Test-Path -LiteralPath $path | Should -BeFalse
            }
            else {
                $result = Receive-FilesFile @arguments
                $actual = if ($Stream) { $destination.ToArray() } else { [System.IO.File]::ReadAllBytes($path) }
                [System.Text.Encoding]::UTF8.GetString($actual) | Should -Be '9876543210'
                $result.VersionBound | Should -BeFalse
            }
            $requests = @($service.Requests | Where-Object { $_.Path -like '/download/*' })
            $requests.Count | Should -Be 2
            $requests[0].Header('Range') | Should -Be 'bytes=0-3'
            $requests[1].Header('Range') | Should -BeNullOrEmpty
            if ($Enabled) { $requests[1].Header('If-Unmodified-Since') | Should -Be 'Wed, 21 Oct 2015 07:28:00 GMT' }
            else { $requests[1].Header('If-Unmodified-Since') | Should -BeNullOrEmpty }
        }
        finally {
            $destination.Dispose()
            if ($connection) { Disconnect-FilesAccount -Connection $connection -Confirm:$false }
            $service.Dispose()
        }
    }
}
