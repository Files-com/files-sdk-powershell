# Analysis of the customer examples, which must run on Windows PowerShell 5.1 as well as PowerShell 7: the module's own
# script rules and formatting, plus the compatibility rules, checked against the Windows PowerShell 5.1 profile that
# PSScriptAnalyzer ships. They catch PowerShell 7-only syntax, commands, parameters and types, such as Join-String or
# ForEach-Object -Parallel. This is static evidence; the Windows lane runs the examples on 5.1 itself.
@{
    IncludeDefaultRules = $true
    Rules = @{
        PSPlaceOpenBrace = @{ Enable = $true; OnSameLine = $true; NewLineAfter = $true; IgnoreOneLineBlock = $true }
        PSPlaceCloseBrace = @{ Enable = $true; NewLineAfter = $true; IgnoreOneLineBlock = $true; NoEmptyLineBefore = $false }
        PSUseConsistentIndentation = @{ Enable = $true; IndentationSize = 4; Kind = 'space'; PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
        PSUseConsistentWhitespace = @{ Enable = $true; CheckOpenBrace = $true; CheckOpenParen = $true; CheckOperator = $true; CheckSeparator = $true; CheckInnerBrace = $true; CheckPipe = $true; CheckParameter = $true }
        PSUseCorrectCasing = @{ Enable = $true }
        PSAvoidUsingDoubleQuotesForConstantString = @{ Enable = $true }
        PSUseCompatibleSyntax = @{ Enable = $true; TargetVersions = @('5.1', '7.0') }
        PSUseCompatibleCommands = @{ Enable = $true; TargetProfiles = @('win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework') }
        PSUseCompatibleTypes = @{ Enable = $true; TargetProfiles = @('win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework') }
    }
}
