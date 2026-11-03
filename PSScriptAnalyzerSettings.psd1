# Analysis of the module's own scripts: its loader, build scripts and examples. Every default rule applies, with the
# formatting the scripts are written in.
@{
    IncludeDefaultRules = $true
    Rules = @{
        PSPlaceOpenBrace = @{ Enable = $true; OnSameLine = $true; NewLineAfter = $true; IgnoreOneLineBlock = $true }
        PSPlaceCloseBrace = @{ Enable = $true; NewLineAfter = $true; IgnoreOneLineBlock = $true; NoEmptyLineBefore = $false }
        PSUseConsistentIndentation = @{ Enable = $true; IndentationSize = 4; Kind = 'space'; PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
        PSUseConsistentWhitespace = @{ Enable = $true; CheckOpenBrace = $true; CheckOpenParen = $true; CheckOperator = $true; CheckSeparator = $true; CheckInnerBrace = $true; CheckPipe = $true; CheckParameter = $true }
        PSUseCorrectCasing = @{ Enable = $true }
        PSAvoidUsingDoubleQuotesForConstantString = @{ Enable = $true }
    }
}
