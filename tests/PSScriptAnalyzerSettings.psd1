# Analysis of the Pester tests and the catalogue adapter: the same rules and formatting as the module's scripts, except
# rules that misread Pester's structure or the deliberate use of test credentials.
@{
    IncludeDefaultRules = $true
    ExcludeRules = @(
        # Variables set in BeforeAll and BeforeEach are read in It blocks, which the rule cannot see.
        'PSUseDeclaredVarsMoreThanAssignments'
        # The tests make SecureStrings from known sentinel text to check that no output ever contains it.
        'PSAvoidUsingConvertToSecureStringWithPlainText'
        # Test helpers named New-* or Start-* create test objects, not system state.
        'PSUseShouldProcessForStateChangingFunctions'
    )
    Rules = @{
        PSPlaceOpenBrace = @{ Enable = $true; OnSameLine = $true; NewLineAfter = $true; IgnoreOneLineBlock = $true }
        PSPlaceCloseBrace = @{ Enable = $true; NewLineAfter = $true; IgnoreOneLineBlock = $true; NoEmptyLineBefore = $false }
        PSUseConsistentIndentation = @{ Enable = $true; IndentationSize = 4; Kind = 'space'; PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
        PSUseConsistentWhitespace = @{ Enable = $true; CheckOpenBrace = $true; CheckOpenParen = $true; CheckOperator = $true; CheckSeparator = $true; CheckInnerBrace = $true; CheckPipe = $true; CheckParameter = $true }
        PSUseCorrectCasing = @{ Enable = $true }
    }
}
