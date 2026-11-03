#!/usr/bin/env bash
# Checks the scripts and sources, then runs the module's tests against the module build.sh built, on each PowerShell
# given. Requires the environment build.sh does, plus:
#   - FILESCOM_MOCK_URL: a running Files.com mock server (the mock-server target) for the generated contract tests;
#   - FILESCOM_TEST_PWSH: the PowerShell executables to test on, separated by ':'. Defaults to the one build.sh uses.
# The generator passes "true" or "false" (initial run); it does not change what runs.
set -euo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
PWSH="${FILESCOM_PWSH:-pwsh}"
command -v dotnet >/dev/null || { echo "test.sh: dotnet is not on PATH" >&2; exit 1; }
command -v "${PWSH}" >/dev/null || { echo "test.sh: PowerShell (${PWSH}) was not found; set FILESCOM_PWSH" >&2; exit 1; }
: "${FILESCOM_TOOLS:?test.sh: set FILESCOM_TOOLS to the directory build.sh restored the tools into}"

: "${FILESCOM_MOCK_URL:?test.sh: set FILESCOM_MOCK_URL to a running Files.com mock server}"

"${PWSH}" -NoProfile -NonInteractive -File "${DIR}/build/Invoke-Analysis.ps1" -ToolsPath "${FILESCOM_TOOLS}"
IFS=':' read -r -a hosts <<< "${FILESCOM_TEST_PWSH:-${PWSH}}"
for host in "${hosts[@]}"; do
  "${PWSH}" -NoProfile -NonInteractive -File "${DIR}/build/Test-Module.ps1" -ToolsPath "${FILESCOM_TOOLS}" -PowerShell "${host}"
done
