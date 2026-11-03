#!/usr/bin/env bash
# Builds, checks and packs the FilesCom module. Requires:
#   - the .NET SDK global.json pins, as `dotnet` on PATH;
#   - PowerShell 7.4 or later, as `pwsh` on PATH or named by FILESCOM_PWSH;
#   - FILESCOM_ENGINE_FEED: a directory holding the FilesCom package engine.json pins;
#   - FILESCOM_TOOLS: a directory for the pinned tools (FILESCOM_TOOL_CACHE may hold their packages for offline use).
set -euo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
PWSH="${FILESCOM_PWSH:-pwsh}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 POWERSHELL_TELEMETRY_OPTOUT=1 POWERSHELL_UPDATECHECK=Off

command -v dotnet >/dev/null || { echo "build.sh: dotnet is not on PATH" >&2; exit 1; }
command -v "${PWSH}" >/dev/null || { echo "build.sh: PowerShell (${PWSH}) was not found; set FILESCOM_PWSH" >&2; exit 1; }
: "${FILESCOM_TOOLS:?build.sh: set FILESCOM_TOOLS to a directory for the pinned tools}"

"${PWSH}" -NoProfile -NonInteractive -File "${DIR}/build/Restore-Tools.ps1" -ToolsPath "${FILESCOM_TOOLS}" ${FILESCOM_TOOL_CACHE:+-PackageCachePath "${FILESCOM_TOOL_CACHE}"}
"${PWSH}" -NoProfile -NonInteractive -File "${DIR}/build/Build-Module.ps1" -ToolsPath "${FILESCOM_TOOLS}" "$@"
