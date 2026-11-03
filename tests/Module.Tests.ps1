BeforeDiscovery {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
    $script:Commands = Get-Command -Module FilesCom.PowerShell | ForEach-Object { @{ Name = $_.Name } }
}

BeforeAll {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
    $script:CommonParameters = [System.Management.Automation.PSCmdlet]::CommonParameters + [System.Management.Automation.PSCmdlet]::OptionalCommonParameters

    # Runs a script in a new PowerShell process of the same version, returning its output and exit code.
    function Invoke-FreshPowerShell {
        param([string] $Script)
        $output = & (Join-Path $PSHOME 'pwsh') -NoProfile -NonInteractive -Command $Script 2>&1
        [pscustomobject]@{ Output = ($output | Out-String); ExitCode = $LASTEXITCODE }
    }

    # A copy of the built module that a test can change.
    function Copy-Module {
        $copy = Join-Path $TestDrive "module-$([guid]::NewGuid())"
        Copy-Item -LiteralPath (Split-Path -Parent $script:ModuleManifest) -Destination $copy -Recurse
        # An installed module can be read-only, and its copy keeps that, so the copy is made writable to be changed.
        foreach ($item in @(Get-Item -LiteralPath $copy) + @(Get-ChildItem -LiteralPath $copy -Recurse -Force)) {
            $item.Attributes = $item.Attributes -band -bnot [IO.FileAttributes]::ReadOnly
        }
        Join-Path $copy 'FilesCom.PowerShell.psd1'
    }
}

Describe 'Module surface' {
    It 'exports exactly the commands its manifest declares, all with approved verbs' {
        $declared = (Import-PowerShellDataFile -LiteralPath $script:ModuleManifest).CmdletsToExport
        $exported = (Get-Command -Module FilesCom.PowerShell).Name

        ($exported | Sort-Object) | Should -Be ($declared | Sort-Object)
        $exported.Count | Should -BeGreaterThan 0
        @(Get-Command -Module FilesCom.PowerShell | Where-Object Verb -NotIn (Get-Verb).Verb).Count | Should -Be 0
    }

    It '<Name> has help with a synopsis, an example and a description of every parameter' -TestCases $script:Commands {
        $help = Get-Help -Name $Name -Full
        $command = Get-Command -Name $Name
        $documented = @($help.parameters.parameter | Where-Object { $_.description.Text })

        $help.Synopsis | Should -Not -BeNullOrEmpty
        $help.Synopsis | Should -Not -Match "^\s*$Name\s"
        @($help.examples.example).Count | Should -BeGreaterThan 0
        foreach ($parameter in $command.Parameters.Keys | Where-Object { $_ -notin $script:CommonParameters }) {
            $documented.name | Should -Contain $parameter -Because "-$parameter of $Name needs help"
        }
        foreach ($parameter in $help.parameters.parameter.name) {
            $command.Parameters.Keys | Should -Contain $parameter -Because "help documents -$parameter, which $Name must have"
        }
    }

    It 'has the about_FilesCom topic' {
        (Get-Help about_FilesCom | Out-String) | Should -Match 'CONNECTING'
    }
}

Describe 'Importing the module' {
    It 'makes no connection and reads no credential from the environment' {
        $result = Invoke-FreshPowerShell "`$env:FILES_API_KEY = 'environment-key'; `$env:FILESCOM_API_KEY = 'environment-key'; Import-Module '$script:ModuleManifest'; 'connections=' + @(Get-FilesConnection -All).Count; try { Get-FilesUser -Id 1 -ErrorAction Stop } catch { `$_.FullyQualifiedErrorId }"

        $result.Output | Should -Match 'connections=0'
        $result.Output | Should -Match 'Files.NoConnection'
    }

    It 'refuses an edition its manifest does not list, before importing any command' {
        $manifest = Copy-Module
        (Get-Content -LiteralPath $manifest -Raw) -replace "CompatiblePSEditions = @\('Core'\)", "CompatiblePSEditions = @('Desktop')" | Set-Content -LiteralPath $manifest

        $result = Invoke-FreshPowerShell "try { Import-Module '$manifest' -ErrorAction Stop } catch { `$_.Exception.Message }; 'commands=' + @(Get-Command -Module FilesCom.PowerShell -ErrorAction SilentlyContinue).Count"

        $result.Output | Should -Match 'supports PowerShell Desktop editions, not Core'
        $result.Output | Should -Match 'commands=0'
    }

    It 'fails the import with a useful message when its engine is missing' {
        $manifest = Copy-Module
        $engineDirectory = if ([Environment]::Version.Major -ge 10) { 'engine-net10.0' } else { 'engine-net8.0' }
        Remove-Item -LiteralPath (Join-Path (Split-Path -Parent $manifest) "core/$engineDirectory/FilesCom.PowerShell.Engine.dll")

        $result = Invoke-FreshPowerShell "try { Import-Module '$manifest' -ErrorAction Stop; 'imported' } catch { `$_.Exception.Message }"

        $result.Output | Should -Match 'engine is missing'
        $result.Output | Should -Not -Match 'imported'
    }

    It 'imports from its binary root without any script, type or format file' {
        $manifest = Import-PowerShellDataFile -LiteralPath $script:ModuleManifest
        $moduleDirectory = Split-Path -Parent $script:ModuleManifest

        $manifest.RootModule | Should -Be 'FilesCom.PowerShell.dll'
        @('ScriptsToProcess', 'TypesToProcess', 'FormatsToProcess', 'NestedModules', 'RequiredModules') | Where-Object { $manifest.ContainsKey($_) } | Should -BeNullOrEmpty
        @(Get-ChildItem -LiteralPath $moduleDirectory -Recurse -File -Include '*.psm1', '*.ps1xml', '*.cdxml') | Should -BeNullOrEmpty
        (Get-Module -Name FilesCom.PowerShell).ModuleType | Should -Be 'Binary'
    }

    It 'installs from its package as FilesCom.PowerShell at the version in _VERSION, imports by that name and runs on the engine it bundles' {
        $version = (Get-Content -LiteralPath (Join-Path $PSScriptRoot '../_VERSION') -Raw).Trim()
        $moduleDirectory = [System.IO.Path]::GetFullPath((Split-Path -Parent $script:ModuleManifest))
        # The folder the package installed into, as PSResourceGet installs it: <modules>/<name>/<version>.
        $modules = Split-Path -Parent (Split-Path -Parent $moduleDirectory)

        $result = Invoke-FreshPowerShell "`$env:PSModulePath = '$modules' + [System.IO.Path]::PathSeparator + `$env:PSModulePath; Import-Module -Name FilesCom.PowerShell -ErrorAction Stop; `$module = Get-Module -Name FilesCom.PowerShell; 'module=' + `$module.Name + ' ' + `$module.Version + ' installed=' + ([System.IO.Path]::GetFullPath(`$module.ModuleBase) -eq '$moduleDirectory'); `$connection = Connect-FilesAccount -ApiKey (ConvertTo-SecureString 'k' -AsPlainText -Force) -BaseUrl 'http://127.0.0.1:9' -MaxNetworkRetries 0 -NoDefault; try { Get-FilesUser -Id 1 -Connection `$connection -ErrorAction Stop } catch { `$_.FullyQualifiedErrorId }"

        $result.Output | Should -Match ([regex]::Escape("module=FilesCom.PowerShell $version installed=True"))
        $result.Output | Should -Match 'Files\.Connection,'
    }

    It 'shows a few properties of its objects by default and keeps every other one available' {
        $service = New-TestService
        try {
            $service.Respond('GET', '/users/1', 200, '{"id":1,"username":"ana","name":"Ana","email":"ana@example.com","time_zone":"UTC"}')
            $connection = Connect-TestService -Service $service
            $user = Get-FilesUser -Id 1 -Connection $connection
            $shown = $user | Out-String
            $connectionShown = $connection | Out-String
            Disconnect-FilesAccount -Connection $connection -Confirm:$false
        }
        finally {
            $service.Dispose()
        }

        $user.PSStandardMembers.DefaultDisplayPropertySet.ReferencedPropertyNames | Should -Contain 'Username'
        $shown | Should -Match 'ana@example.com'
        $shown | Should -Not -Match 'UTC'
        $user.TimeZone | Should -Be 'UTC'
        $connectionShown | Should -Match 'BaseUrl'
        $connectionShown | Should -Match 'IsOpen'
    }

    It 'removes its type settings and closes its connections when removed, and adds them again when imported again' {
        $result = Invoke-FreshPowerShell "Import-Module '$script:ModuleManifest'; 'before=' + [bool](Get-TypeData -TypeName FilesCom.PowerShell.Models.User); Remove-Module FilesCom.PowerShell; 'after=' + [bool](Get-TypeData -TypeName FilesCom.PowerShell.Models.User); Import-Module '$script:ModuleManifest'; 'again=' + [bool](Get-TypeData -TypeName FilesCom.PowerShell.Models.User)"

        $result.Output | Should -Match 'before=True'
        $result.Output | Should -Match 'after=False'
        $result.Output | Should -Match 'again=True'
    }

    It 'loads its engine and the SDK''s dependencies privately, sharing only the contracts and the host''s DiagnosticSource' {
        $service = New-TestService
        try {
            $service.Respond('GET', '/users/1', 200, '{"id":1}')
            $connection = Connect-TestService -Service $service
            Get-FilesUser -Id 1 -Connection $connection | Out-Null
            Disconnect-FilesAccount -Connection $connection -Confirm:$false
        }
        finally {
            $service.Dispose()
        }
        $engineContext = [System.Runtime.Loader.AssemblyLoadContext]::All | Where-Object Name -EQ 'FilesCom.PowerShell.Engine'
        $default = [System.Runtime.Loader.AssemblyLoadContext]::Default

        foreach ($name in 'FilesCom', 'log4net', 'Polly.Core') {
            $engineContext.Assemblies.GetName().Name | Should -Contain $name
            $default.Assemblies.GetName().Name | Should -Not -Contain $name
        }
        $default.Assemblies.GetName().Name | Should -Contain 'FilesCom.PowerShell.Contracts'
        $engineContext.Assemblies.GetName().Name | Should -Not -Contain 'FilesCom.PowerShell.Contracts'
    }
}

Describe 'The module''s identity and notices' {
    BeforeAll {
        $script:ModuleDirectory = Split-Path -Parent $script:ModuleManifest
        $script:BuildInfo = Get-Content -LiteralPath (Join-Path $script:ModuleDirectory 'FilesCom.BuildInfo.json') -Raw | ConvertFrom-Json -AsHashtable

        function Get-RelativeModulePath {
            param([string] $Path)
            [System.IO.Path]::GetRelativePath($script:ModuleDirectory, $Path).Replace('\', '/')
        }
    }

    It 'records the SHA-256 of every other file in the module' {
        $files = @(Get-ChildItem -LiteralPath $script:ModuleDirectory -Recurse -File | Where-Object Name -NE 'FilesCom.BuildInfo.json')

        @($script:BuildInfo.files.Keys | Sort-Object) | Should -Be @($files | ForEach-Object { Get-RelativeModulePath $_.FullName } | Sort-Object)
        foreach ($file in $files) {
            $path = Get-RelativeModulePath $file.FullName
            $script:BuildInfo.files[$path] | Should -Be (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() -Because $path
        }
    }

    It 'records the module version and the engine package, and each assembly the engine loads is a recorded file' {
        $service = New-TestService
        try {
            $service.Respond('GET', '/users/1', 200, '{"id":1}')
            $connection = Connect-TestService -Service $service
            Get-FilesUser -Id 1 -Connection $connection | Out-Null
            Disconnect-FilesAccount -Connection $connection -Confirm:$false
        }
        finally {
            $service.Dispose()
        }
        $engineContext = [System.Runtime.Loader.AssemblyLoadContext]::All | Where-Object Name -EQ 'FilesCom.PowerShell.Engine'

        $script:BuildInfo.module.version | Should -Be (Import-PowerShellDataFile -LiteralPath $script:ModuleManifest).ModuleVersion
        $script:BuildInfo.packages.id | Should -Contain $script:BuildInfo.engine.package
        ($script:BuildInfo.packages | Where-Object id -EQ $script:BuildInfo.engine.package).version | Should -Be $script:BuildInfo.engine.version
        foreach ($assembly in $engineContext.Assemblies) {
            $path = Get-RelativeModulePath $assembly.Location
            $script:BuildInfo.files[$path] | Should -Be (Get-FileHash -LiteralPath $assembly.Location -Algorithm SHA256).Hash.ToLowerInvariant() -Because $path
        }
    }

    It 'names the package and license of every third-party assembly it ships' {
        $notices = Get-Content -LiteralPath (Join-Path $script:ModuleDirectory 'ThirdPartyNotices.txt') -Raw
        $ownAssemblies = 'FilesCom.PowerShell.dll', 'FilesCom.PowerShell.Contracts.dll', 'FilesCom.PowerShell.CoreLoader.dll', 'FilesCom.PowerShell.Engine.dll'

        foreach ($package in $script:BuildInfo.packages) {
            $notices | Should -Match "(?m)^$([regex]::Escape("$($package.id) $($package.version)"))\r?\n  License: $([regex]::Escape($package.license))"
        }
        # Apache-2.0 requires its text and the package's NOTICE; log4net's package carries neither, so its source provides them.
        if ($script:BuildInfo.packages | Where-Object license -EQ 'Apache-2.0') {
            $notices | Should -Match 'Apache License\s+Version 2\.0, January 2004'
            $notices | Should -Match 'Apache log4net\s+Copyright 2004-2025 The Apache Software Foundation'
            $notices | Should -Match 'License and notice texts from https://github\.com/apache/logging-log4net rel/3\.5\.0'
        }
        foreach ($assembly in Get-ChildItem -LiteralPath (Join-Path $script:ModuleDirectory 'core') -Recurse -Filter '*.dll' -File | Where-Object Name -NotIn $ownAssemblies) {
            $directory = Get-RelativeModulePath ($assembly.DirectoryName -replace '[\\/]runtimes[\\/].*$', '')
            $owners = @($script:BuildInfo.packages | Where-Object { $_.usedBy -contains $directory -and $assembly.BaseName.StartsWith($_.id, [StringComparison]::OrdinalIgnoreCase) })
            $owners.Count | Should -BeGreaterThan 0 -Because "$(Get-RelativeModulePath $assembly.FullName) needs its package's notice"
        }
    }
}
