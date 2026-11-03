# Shared setup for the module's Pester tests. The module under test is the built package named by
# FILESCOM_MODULE_MANIFEST; the only stand-in is FilesTestService, a Files.com site on a loopback port, or in memory.

$script:ModuleManifest = $env:FILESCOM_MODULE_MANIFEST
$script:TestServiceAssembly = $env:FILESCOM_TEST_SERVICE
if (-not $script:ModuleManifest -or -not (Test-Path -LiteralPath $script:ModuleManifest)) {
    throw 'FILESCOM_MODULE_MANIFEST must name the built FilesCom.PowerShell.psd1.'
}
if (-not $script:TestServiceAssembly -or -not (Test-Path -LiteralPath $script:TestServiceAssembly)) {
    throw 'FILESCOM_TEST_SERVICE must name the built FilesCom.PowerShell.TestService.dll.'
}

Import-Module -Name $script:ModuleManifest -Force -ErrorAction Stop
Add-Type -Path $script:TestServiceAssembly

# Credentials whose text must never appear in any output, error or log.
$script:ApiKeySentinel = 'sentinel-api-key-7Q2'
$script:PasswordSentinel = 'sentinel-password-9K4'

function New-TestService {
    [OutputType([FilesCom.PowerShell.Tests.FilesTestService])]
    param()
    [FilesCom.PowerShell.Tests.FilesTestService]::new()
}

# A connection to the test service, made without becoming the runspace default unless -AsDefault is given. With
# -InMemory, every request goes to the service through its in-memory handler, with -HttpMessageHandler, instead of its
# socket: the storage its versioned downloads, folders and S3 uploads name is then Amazon S3's, answered in memory.
function Connect-TestService {
    param(
        [Parameter(Mandatory)]
        [FilesCom.PowerShell.Tests.FilesTestService] $Service,
        [string] $ApiKey = $script:ApiKeySentinel,
        [switch] $AsDefault,
        [switch] $InMemory,
        [hashtable] $Options = @{}
    )
    $arguments = @{ ApiKey = (ConvertTo-SecureString -String $ApiKey -AsPlainText -Force); BaseUrl = $Service.BaseUrl; MaxNetworkRetries = 0; NoDefault = (-not $AsDefault) }
    if ($InMemory) {
        $arguments.HttpMessageHandler = $Service.CreateHandler()
    }
    foreach ($option in $Options.Keys) {
        $arguments[$option] = $Options[$option]
    }
    Connect-FilesAccount @arguments
}

# Whether two byte arrays hold the same bytes.
# Starts a script with the module and connection in another runspace, as a user's pipeline would run. The script's
# parameters are the connection followed by -Arguments.
function Start-Command {
    param([scriptblock] $Script, $Connection, [object[]] $Arguments = @())
    $shell = [powershell]::Create()
    # The module is imported on its own first: disposing a stopped batch of statements at once can make PowerShell read
    # a variable of the closed runspace on a pool thread, which ends the process.
    $shell.AddScript('param($Manifest) Import-Module -Name $Manifest').AddArgument($script:ModuleManifest).Invoke() | Out-Null
    $shell.Commands.Clear()
    $shell.AddScript($Script).AddArgument($Connection) | Out-Null
    foreach ($argument in $Arguments) {
        $shell.AddArgument($argument) | Out-Null
    }
    [pscustomobject]@{ Shell = $shell; Handle = $shell.BeginInvoke() }
}

function Wait-Until {
    param([scriptblock] $Condition, [int] $TimeoutSeconds = 10)
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    while (-not (& $Condition)) {
        if ([DateTime]::UtcNow -gt $deadline) { throw "Timed out waiting for $Condition" }
        Start-Sleep -Milliseconds 20
    }
}

# Count bytes that are the same on every run, as one byte array.
function New-RandomContent {
    [OutputType([byte[]])]
    param([int] $Count)
    $bytes = [byte[]]::new($Count)
    [System.Random]::new(42).NextBytes($bytes)
    # The comma outputs the array itself: a bare $bytes would send each byte down the pipeline on its own, and the
    # caller would collect them, one object per byte, into a new array.
    , $bytes
}

function Test-SameContent {
    param([byte[]] $Expected, [byte[]] $Actual)
    # SHA256.Create rather than HashData, which .NET Framework, under Windows PowerShell 5.1, does not have.
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $Expected.Length -eq $Actual.Length -and [System.Convert]::ToBase64String($sha.ComputeHash($Expected)) -eq [System.Convert]::ToBase64String($sha.ComputeHash($Actual))
    }
    finally {
        $sha.Dispose()
    }
}

# The requests the service received for a path relative to /api/rest/v1, compared after percent-decoding.
function Get-ApiRequest {
    param(
        [Parameter(Mandatory)]
        [FilesCom.PowerShell.Tests.FilesTestService] $Service,
        [string] $Method,
        [string] $Path
    )
    $Service.Requests | Where-Object {
        (-not $Method -or $_.Method -eq $Method) -and (-not $Path -or [uri]::UnescapeDataString($_.Path) -eq "/api/rest/v1$Path")
    }
}

function ConvertTo-UserJson {
    param([long[]] $Id)
    '[' + (($Id | ForEach-Object { "{""id"":$_,""username"":""user$_""}" }) -join ',') + ']'
}
