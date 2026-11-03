#!/usr/bin/env bash
# The PowerShell module's ordinary CI job, build-powershell in .gitlab-ci.yml, runs this after the .NET toolchain:
#
#   source ./.gitlab-ci/dotnet-toolchain.sh
#   ./targets/powershell/ci.sh
#
# It puts each PowerShell that build/tools.json pins in a private directory, from its release archive checked against
# the pinned SHA-256, then runs build/Invoke-OrdinaryCi.ps1 on the newest of them. That script checks that the captured
# production schema FILESCOM_CI_SCHEMA names and the FilesCom package FILESCOM_CI_PACKAGE names, which the pipeline
# built from that capture, join with the package's record FILESCOM_CI_PACKAGE_PROVENANCE names, this commit and this
# pipeline; generates this target from the capture, on that package; builds, checks and packs the module; and runs the
# ordinary test suites on each pinned PowerShell as an unprivileged user. Its evidence, kept as the job's artifacts, goes
# to powershell-ci/ in the checkout.
#
# The script runs from the job's source projection, which build/source-projection.sh first makes in the same private
# directory: a new repository of the checkout's commit with only the files build/ordinary-source-projection.txt lists,
# which can fetch nothing, as the script requires. The checkout itself is only read.
#
# FILESCOM_CI_SCHEMA must be set in CI as elsewhere: nothing fetches the schema or the package. Outside GitLab CI,
# FILESCOM_CI_PWSH_ARCHIVES may name a directory already holding the release archives, which are checked the same way;
# in CI they are fetched. Invoke-OrdinaryCi.ps1 describes its other local inputs.
set -euo pipefail

target_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
project_root="$(git -C "${target_dir}" rev-parse --show-toplevel)"
if [[ "${target_dir}" != "${project_root}/targets/powershell" ]]; then
  echo "ci.sh: run targets/powershell/ci.sh in the generator checkout, not a generated copy (${target_dir})" >&2
  exit 1
fi

export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_GENERATE_ASPNET_CERTIFICATE=false
export POWERSHELL_TELEMETRY_OPTOUT=1 POWERSHELL_UPDATECHECK=Off

for tool in curl dotnet git ruby sha256sum tar; do
  if ! command -v "${tool}" > /dev/null; then
    echo "ci.sh: this image has no ${tool}" >&2
    exit 1
  fi
done

case "$(uname -m)" in
  x86_64) rid="linux-x64" ;;
  aarch64) rid="linux-arm64" ;;
  *)
    echo "ci.sh: build/tools.json pins no PowerShell for $(uname -m)" >&2
    exit 1
    ;;
esac

if [[ "${GITLAB_CI:-}" == "true" && -n "${FILESCOM_CI_PWSH_ARCHIVES:-}" ]]; then
  echo "ci.sh: FILESCOM_CI_PWSH_ARCHIVES is for local checks; in CI the archives are fetched" >&2
  exit 1
fi

work="$(mktemp -d "${TMPDIR:-/tmp}/filescom-powershell-ci.XXXXXX")"
# Removes only this run's private directory, keeping the exit status.
remove_work() {
  status=$?
  trap - EXIT
  rm -rf -- "${work}"
  exit "${status}"
}
trap remove_work EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
# The unprivileged test user reaches the PowerShells and tools through this directory.
chmod 755 "${work}"
mkdir -p "${work}/archives" "${work}/powershell"

# The projection is of the pipeline's commit in CI, which the checkout must have checked out, and of the checked-out
# commit elsewhere.
if [[ "${GITLAB_CI:-}" == "true" ]]; then
  commit="${CI_COMMIT_SHA:-}"
else
  commit="$(git -C "${project_root}" rev-parse --verify 'HEAD^{commit}')"
fi
"${target_dir}/build/source-projection.sh" "${project_root}" "${commit}" "${work}/source"

# Each pinned version and its archive's SHA-256 for this platform, one per line.
pins="$(ruby -rjson -e '
  JSON.parse(File.read(ARGV[0])).fetch("powershell").fetch("versions").each do |version, hashes|
    puts "#{version} #{hashes.fetch(ARGV[1])}"
  end' "${target_dir}/build/tools.json" "${rid}")"
newest=""
while read -r version sha256; do
  name="powershell-${version}-${rid}.tar.gz"
  if [[ -n "${FILESCOM_CI_PWSH_ARCHIVES:-}" ]]; then
    archive="${FILESCOM_CI_PWSH_ARCHIVES}/${name}"
  else
    archive="${work}/archives/${name}"
    curl --fail --silent --show-error --location --proto '=https' --proto-redir '=https' --retry 3 --output "${archive}" \
      "https://github.com/PowerShell/PowerShell/releases/download/v${version}/${name}"
  fi
  echo "${sha256}  ${archive}" | sha256sum --check --strict
  mkdir "${work}/powershell/${version}"
  tar -xzf "${archive}" -C "${work}/powershell/${version}"
  chmod 755 "${work}/powershell/${version}/pwsh"
  # shellcheck disable=SC2016 # PowerShell, not this shell, expands the expression.
  actual="$("${work}/powershell/${version}/pwsh" -NoProfile -NonInteractive -Command '$PSVersionTable.PSVersion.ToString()')"
  if [[ "${actual}" != "${version}" ]]; then
    echo "ci.sh: ${name} runs PowerShell ${actual}, not ${version}" >&2
    exit 1
  fi
  newest="$(printf '%s\n%s\n' "${newest}" "${version}" | sort --version-sort | tail -n 1)"
done <<< "${pins}"

"${work}/powershell/${newest}/pwsh" -NoProfile -NonInteractive \
  -File "${work}/source/targets/powershell/build/Invoke-OrdinaryCi.ps1" \
  -WorkPath "${work}" -EvidencePath "${project_root}/powershell-ci"
