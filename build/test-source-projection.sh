#!/usr/bin/env bash
# The control of the ordinary CI job's source projection. It runs the source-projection.sh beside it, as ci.sh does, on
# synthetic normal Git clones, each with an origin it could fetch from and commits holding synthetic marker files at the
# paths the projection leaves out. Then it runs the source and schema steps of the Invoke-OrdinaryCi.ps1 beside it,
# taken from that file and run as they are, on what source-projection.sh made and on the clone itself:
#
#   build/test-source-projection.sh <pwsh> <work> [--only <case>[,<case>...]]
#
# - The guard refuses the clone itself, as it refused the job's checkout, for its remote.origin configuration.
# - The projection of the clone's commit passes the source step, as the pipeline's commit (CI_COMMIT_SHA), and the
#   schema step after it. It has no remote, alternate or partial clone and is shallow at the commit. Every file of the
#   commit is in the index, and the listed ones are checked out as committed. The uncommitted change and the untracked
#   file in the clone are not, and of the files the list leaves out there is neither file nor object.
# - The guard still refuses that projection as another commit, given a remote, or given a protected object.
# - source-projection.sh refuses each of these and makes nothing:
#   - a malformed or foreign commit;
#   - a missing source, or a directory below the top of the clone;
#   - an existing output or a link, which it leaves as they were;
#   - an output with no directory, or one in the clone;
#   - a set GIT_DIR;
#   - a list that is missing, unterminated, or names '..', a path the commit lacks or a link.
# - When a listed object is missing from a partial clone, source-projection.sh refuses to fetch it and removes the
#   projection it began.
# - Each run leaves the clone exactly as it was.
#
# --only runs just the named list cases, each of list-dotdot, list-absent and list-link: its own clone and
# source-projection.sh's refusal of it, four checks each, and no other scenario. <pwsh> is then not used. Any other or
# repeated name, or none, is refused.
#
# <work> must not exist. Needs Linux, bash, git 2.39 or later and PowerShell 7.4 or later. It uses no network and no
# Git configuration but its own, and of this repository it reads only the two files it runs. It prints one line a check
# and exits 0 only when every check passed.
set -euo pipefail
export LC_ALL=C

if [[ $# -ne 2 && ! ($# -eq 4 && "$3" == --only) ]]; then
  echo "usage: test-source-projection.sh <pwsh> <work> [--only <case>[,<case>...]]" >&2
  exit 2
fi
selection=()
if [[ $# -eq 4 ]]; then
  if [[ "$4" =~ ^[a-z-]+(,[a-z-]+)*$ ]]; then
    IFS=, read -r -a selection <<< "$4"
  fi
  if [[ ${#selection[@]} -eq 0 || -n "$(printf '%s\n' "${selection[@]}" | sort | uniq -d)" ]]; then
    echo "test-source-projection.sh: --only needs one or more of list-dotdot, list-absent and list-link, each once" >&2
    exit 2
  fi
  for name in "${selection[@]}"; do
    case "${name}" in
      list-dotdot | list-absent | list-link) ;;
      *)
        echo "test-source-projection.sh: --only: '${name}' is not list-dotdot, list-absent or list-link" >&2
        exit 2
        ;;
    esac
  done
fi
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" > /dev/null 2>&1 && pwd -P)"
producer="${here}/source-projection.sh"
guard="${here}/Invoke-OrdinaryCi.ps1"
pwsh="$1"
mkdir -- "$2"
work="$(cd "$2" && pwd -P)"

# Git with no configuration but this control's, and a fixed identity and time.
export HOME="${work}/home" XDG_CONFIG_HOME="${work}/home/.config" GIT_CONFIG_NOSYSTEM=1
export GIT_AUTHOR_NAME=control GIT_AUTHOR_EMAIL=control@example.invalid GIT_AUTHOR_DATE='2026-10-06T00:00:00Z'
export GIT_COMMITTER_NAME=control GIT_COMMITTER_EMAIL=control@example.invalid GIT_COMMITTER_DATE='2026-10-06T00:00:00Z'
unset GIT_DIR GIT_WORK_TREE GIT_INDEX_FILE GIT_OBJECT_DIRECTORY GIT_ALTERNATE_OBJECT_DIRECTORIES GIT_COMMON_DIR CI_COMMIT_SHA
mkdir "${HOME}"

passed=0
failed=0
# Records a check: its name, then a command that succeeds when it holds.
check() {
  local name="$1"
  shift
  if "$@"; then
    passed=$((passed + 1))
    echo "pass  ${name}"
  else
    failed=$((failed + 1))
    echo "FAIL  ${name}"
  fi
}

# The guard's sources, as Invoke-OrdinaryCi.ps1 names them, and other files the list leaves out; and the listed files.
protected=(targets/powershell/tests/__Contract.Tests.ps1 targets/powershell/tests/Regions.Tests.ps1
  targets/powershell/verification/catalogue/journeys.ps1)
excluded=("${protected[@]}" verification/catalogue/schemas/private-synthetic.json.gz docs/unlisted.md)
listed=(README.md targets/common/shared/data.json targets/powershell/build/ordinary-source-projection.txt
  targets/powershell/build/tool.sh targets/powershell/module/Synthetic.psm1)
list=targets/powershell/build/ordinary-source-projection.txt

# A synthetic origin with two commits and a normal clone of it, at <work>/<name>, detached at the second commit as a
# CI checkout is. The variant changes the list: ok, dotdot, absent, unterminated, none or link.
make_clone() {
  local name="$1" variant="$2" origin="${work}/$1-origin" path
  git init --quiet --template= --initial-branch=main -- "${origin}"
  printf 'synthetic public readme\n' > "${origin}/README.md"
  git -C "${origin}" add -A
  git -C "${origin}" commit --quiet -m first
  for path in "${listed[@]}" "${excluded[@]}"; do
    mkdir -p -- "$(dirname -- "${origin}/${path}")"
  done
  printf '{"synthetic": true}\n' > "${origin}/targets/common/shared/data.json"
  printf '#!/bin/sh\necho synthetic tool\n' > "${origin}/targets/powershell/build/tool.sh"
  chmod +x "${origin}/targets/powershell/build/tool.sh"
  printf 'function Get-Synthetic { "synthetic" }\n' > "${origin}/targets/powershell/module/Synthetic.psm1"
  for path in "${excluded[@]}"; do
    printf 'synthetic excluded marker: %s\n' "${path}" > "${origin}/${path}"
  done
  case "${variant}" in
    ok) printf '/%s\n' "${listed[@]}" > "${origin}/${list}" ;;
    dotdot) printf '/%s\n' "${listed[@]}" targets/../README.md > "${origin}/${list}" ;;
    absent) printf '/%s\n' "${listed[@]}" targets/powershell/build/absent.txt > "${origin}/${list}" ;;
    unterminated)
      printf '/%s\n' "${listed[@]:0:4}" > "${origin}/${list}"
      printf '/%s' "${listed[4]}" >> "${origin}/${list}"
      ;;
    none) ;;
    link)
      ln -s tool.sh "${origin}/targets/powershell/build/link.sh"
      printf '/%s\n' "${listed[@]}" targets/powershell/build/link.sh > "${origin}/${list}"
      ;;
  esac
  git -C "${origin}" add -A
  git -C "${origin}" commit --quiet -m second
  git clone --quiet -- "${origin}" "${work}/${name}"
  git -C "${work}/${name}" checkout --quiet --detach
}

# Every name, type, mode and byte under a directory, its .git included, as one hash.
state() {
  (
    cd "$1"
    find . -print0 | sort -z | xargs -0 stat -c '%n %F %a %s'
    find . -type f -print0 | sort -z | xargs -0 sha256sum
  ) | sha256sum
}

# The guard: Invoke-OrdinaryCi.ps1's own functions and definitions of the projection, and its source and schema steps,
# taken from the file and run as they are on -Root, in CI as the pipeline's commit CI_COMMIT_SHA. It prints
# 'STEP <name> passed <details as JSON>' or 'STEP <name> failed <error>', and stops at the first failure.
cat > "${work}/guard.ps1" << 'GUARD'
param([string] $Guard, [string] $Root, [string] $Out, [string] $Schema)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$errors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile($Guard, [ref] $null, [ref] $errors)
if ($errors.Count) {
    throw "$Guard does not parse."
}
$names = @('projectionList', 'protectedSources', 'protectedOutputs', 'generatorInputs')
$definitions = @($ast.EndBlock.Statements | Where-Object {
        $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] -or
        ($_ -is [System.Management.Automation.Language.AssignmentStatementAst] -and $_.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and $_.Left.VariablePath.UserPath -in $names)
    })
. ([scriptblock]::Create(($definitions | ForEach-Object { $_.Extent.Text }) -join "`n"))
$projectRoot = (Resolve-Path -LiteralPath $Root).Path
$inCi = $true
# The schema step as for a local check of the package engine.json pins, whose schema this one stands in for.
$engineInput = 'pinned'
$engine = [pscustomobject]@{ schema_sha256 = (Get-FileHash -LiteralPath $Schema -Algorithm SHA256).Hash.ToLowerInvariant() }
$env:FILESCOM_CI_SCHEMA = $Schema
$EvidencePath = (New-Item -ItemType Directory -Path $Out).FullName
foreach ($stepName in 'source', 'schema') {
    $steps = @($ast.FindAll({ param($node) $node -is [System.Management.Automation.Language.CommandAst] -and $node.GetCommandName() -eq 'Invoke-Step' -and $node.Extent.Text -match "-Name '$stepName'" }, $true))
    $actions = @($steps | ForEach-Object { $_.CommandElements } | Where-Object { $_ -is [System.Management.Automation.Language.ScriptBlockExpressionAst] })
    if ($actions.Count -ne 1) {
        throw "$Guard has no single $stepName step."
    }
    try {
        $details = & ([scriptblock]::Create($actions[0].ScriptBlock.EndBlock.Extent.Text))
        "STEP $stepName passed $($details | ConvertTo-Json -Depth 8 -Compress)"
    }
    catch {
        "STEP $stepName failed $($_.Exception.Message -replace '\s*\r?\n\s*', ' ')"
        break
    }
}
GUARD
printf '{"synthetic": "schema"}\n' > "${work}/schema.json"
# Runs the guard on a root as a pipeline's commit, its output in <work>/<name>.txt.
run_guard() {
  CI_COMMIT_SHA="$3" "${pwsh}" -NoProfile -NonInteractive -File "${work}/guard.ps1" -Guard "${guard}" -Root "$1" \
    -Out "${work}/$2-evidence" -Schema "${work}/schema.json" > "${work}/$2.txt" 2>&1 || true
}
guard_says() {
  grep -qF -- "$2" "${work}/$1.txt"
}

# Runs source-projection.sh, which must refuse with the message, make nothing at the output and leave the clone
# observed as it was: refused <name> <message> <source> <commit> <output> <observed clone> [<variable>=<value>...]
refused() {
  local name="$1" message="$2" source="$3" commit="$4" output="$5" observed="$6" before status=0
  shift 6
  before="$(state "${observed}")"
  env "$@" "${producer}" "${source}" "${commit}" "${output}" > "${work}/${name}.out" 2> "${work}/${name}.err" || status=$?
  check "${name}: refused" test "${status}" -ne 0
  check "${name}: names why" grep -qF -- "${message}" "${work}/${name}.err"
  check "${name}: made nothing" test ! -e "${output}" -a ! -L "${output}"
  check "${name}: the clone as it was" test "$(state "${observed}")" == "${before}"
}

# A list case, list-<variant>: a clone whose list has that fault, which source-projection.sh must refuse for it.
list_case() {
  local name="$1" variant="${1#list-}" message
  case "${variant}" in
    dotdot) message="'/targets/../README.md' in ${list} is not a literal '/'-anchored path" ;;
    absent) message="targets/powershell/build/absent.txt, which ${list} lists, is not a file of commit" ;;
    unterminated) message="does not end its last line with LF" ;;
    none) message="has no file ${list}" ;;
    link) message="targets/powershell/build/link.sh, which ${list} lists, is not a file of commit" ;;
  esac
  make_clone "${variant}" "${variant}"
  refused "${name}" "${message}" "${work}/${variant}" "$(git -C "${work}/${variant}" rev-parse HEAD)" "${work}/out-${name}" \
    "${work}/${variant}"
}

# --only: the selected list cases, and nothing else.
if [[ ${#selection[@]} -gt 0 ]]; then
  for name in "${selection[@]}"; do
    list_case "${name}"
  done
  echo "passed ${passed} of $((passed + failed))"
  [[ "${failed}" -eq 0 ]]
  exit
fi

# The clone, detached at its second commit, with an uncommitted change and an untracked file.
make_clone main ok
clone="${work}/main"
commit="$(git -C "${clone}" rev-parse HEAD)"
tree="$(git -C "${clone}" rev-parse 'HEAD^{tree}')"
parent="$(git -C "${clone}" rev-parse 'HEAD^')"
printf 'an uncommitted change\n' >> "${clone}/README.md"
printf 'untracked\n' > "${clone}/targets/powershell/build/untracked.txt"
before="$(state "${clone}")"

# The guard refuses the clone as it refused the job's checkout.
run_guard "${clone}" clone "${commit}"
check 'the guard refuses the clone for its origin' guard_says clone \
  "STEP source failed The repository could still fetch what the projection leaves out: no Git configuration may set remote.origin.url, remote.origin.fetch."

# The projection passes the guard's source step as the pipeline's commit, and its schema step.
projection="${work}/projection"
status=0
"${producer}" "${clone}" "${commit}" "${projection}" > "${work}/projection.out" 2> "${work}/projection.err" || status=$?
check 'source-projection.sh makes the projection' test "${status}" -eq 0
run_guard "${projection}" projection "${commit}"
check 'the guard passes the projection: source step' grep -q '^STEP source passed ' "${work}/projection.txt"
check 'the guard passes the projection: schema step' grep -q '^STEP schema passed ' "${work}/projection.txt"
for field in "\"commit\":\"${commit}\"" "\"tree\":\"${tree}\"" '"shallow":"true"' '"commitFiles":10' '"checkedOut":5' \
  '"asInCommit":5' '"fetch":"no remote or partial clone in any Git configuration"'; do
  check "the guard's source details: ${field}" guard_says projection "${field}"
done
for path in "${protected[@]}"; do
  object="$(git -C "${clone}" rev-parse "${commit}:${path}")"
  check "the guard's source details: ${path} neither on disk nor in the repository" guard_says projection \
    "{\"path\":\"${path}\",\"object\":\"${object}\",\"onDisk\":false,\"inRepository\":false}"
done

# What the projection holds, asked of Git directly.
in_projection() {
  GIT_NO_LAZY_FETCH=1 git -C "${projection}" -c protocol.allow=never "$@"
}
check 'the projection is at the commit' test "$(in_projection rev-parse HEAD)" == "${commit}"
check 'the projection is shallow at the commit' test "$(cat "${projection}/.git/shallow")" == "${commit}"
check 'the projection has no alternate' test ! -e "${projection}/.git/objects/info/alternates"
check 'the projection has no remote or partial clone' \
  test -z "$(in_projection config --list --name-only | grep -E '^(remote\.|extensions\.partialclone)' || true)"
expected="$(for path in "${listed[@]}"; do git -C "${clone}" rev-parse "${commit}:${path}"; done | sort)"
check 'the projection holds exactly the listed blobs' test "$(in_projection cat-file --batch-all-objects \
  --batch-check='%(objecttype) %(objectname)' | awk '$1 == "blob" { print $2 }' | sort)" == "${expected}"
check 'the projection holds one commit' test "$(in_projection cat-file --batch-all-objects \
  --batch-check='%(objecttype)' | grep -c '^commit$')" -eq 1
for path in "${excluded[@]}"; do
  object="$(git -C "${clone}" rev-parse "${commit}:${path}")"
  check "left out, no object: ${path}" test -z "$(in_projection cat-file --batch-check='%(objectname)' <<< "${object}" |
    grep -v ' missing$' || true)"
  check "left out, no file: ${path}" test ! -e "${projection}/${path}" -a ! -L "${projection}/${path}"
done
check 'the index: every file of the commit, the listed ones checked out' \
  test "$(in_projection ls-files -t | cut -c1 | sort | uniq -c | tr -s ' ')" == "$(printf ' 5 H\n 5 S')"
check 'README.md as committed, without the uncommitted change' \
  cmp -s <(git -C "${clone}" cat-file blob "${commit}:README.md") "${projection}/README.md"
check 'the untracked file is not in the projection' test ! -e "${projection}/targets/powershell/build/untracked.txt"
check 'the executable file is executable' test -x "${projection}/targets/powershell/build/tool.sh"
check 'the clone as it was' test "$(state "${clone}")" == "${before}"

# The guard is as strict on the projection: another pipeline commit, a remote, a protected object.
run_guard "${projection}" other-commit "${parent}"
check 'the guard refuses the projection as another commit' guard_says other-commit \
  "STEP source failed The checkout is commit ${commit}, not the pipeline's commit '${parent}'."
git -C "${projection}" config remote.origin.url "${work}/main-origin"
run_guard "${projection}" remote "${commit}"
check 'the guard refuses the projection given a remote' guard_says remote \
  'STEP source failed The repository could still fetch what the projection leaves out: no Git configuration may set remote.origin.url.'
git -C "${projection}" config --unset remote.origin.url
object="$(git -C "${clone}" rev-parse "${commit}:${protected[0]}")"
git -C "${clone}" cat-file blob "${object}" | git -C "${projection}" hash-object -w --stdin > /dev/null
run_guard "${projection}" protected-object "${commit}"
check 'the guard refuses the projection given a protected object' guard_says protected-object \
  "protected, but in the repository: ${protected[0]} (${object})"

# source-projection.sh refuses, making nothing.
refused malformed-commit "'HEAD' is not a full commit ID" "${clone}" HEAD "${work}/out-1" "${clone}"
refused foreign-commit "has commit ${commit} checked out, not ${parent}" "${clone}" "${parent}" "${work}/out-2" "${clone}"
refused missing-source "${work}/missing is not a directory" "${work}/missing" "${commit}" "${work}/out-3" "${clone}"
refused below-top "is not the top of its Git working tree" "${clone}/targets" "${commit}" "${work}/out-4" "${clone}"
refused no-directory "is not a new directory in an existing one" "${clone}" "${commit}" "${work}/no-such-directory/out" "${clone}"
refused in-clone "is in the checkout" "${clone}" "${commit}" "${clone}/out" "${clone}"
refused git-dir "GIT_DIR is set" "${clone}" "${commit}" "${work}/out-5" "${clone}" GIT_DIR="${clone}/.git"
for variant in dotdot absent unterminated none link; do
  list_case "list-${variant}"
done

# An existing output, and a link, are left as they were.
for kind in directory link; do
  output="${work}/existing-${kind}"
  mkdir "${work}/existing-${kind}-target"
  printf 'kept\n' > "${work}/existing-${kind}-target/sentinel"
  if [[ "${kind}" == directory ]]; then
    mv "${work}/existing-${kind}-target" "${output}"
    kept="${output}"
  else
    ln -s "${work}/existing-${kind}-target" "${output}"
    kept="${work}/existing-${kind}-target"
  fi
  status=0
  "${producer}" "${clone}" "${commit}" "${output}" > /dev/null 2> "${work}/existing-${kind}.err" || status=$?
  check "existing ${kind}: refused" test "${status}" -ne 0
  check "existing ${kind}: names why" grep -qF -- "${output} already exists" "${work}/existing-${kind}.err"
  check "existing ${kind}: left as it was" test "$(ls -A "${kept}")" == sentinel -a "$(cat "${kept}/sentinel")" == kept
done
check 'the link still leads where it did' test "$(readlink "${work}/existing-link")" == "${work}/existing-link-target"

# A partial clone missing a listed object, which its origin holds: nothing is fetched, and the projection begun is
# removed.
make_clone partial ok
object="$(git -C "${work}/partial" rev-parse 'HEAD:targets/common/shared/data.json')"
rm -- "${work}/partial/.git/objects/${object:0:2}/${object:2}"
git -C "${work}/partial" config core.repositoryformatversion 1
git -C "${work}/partial" config extensions.partialClone origin
git -C "${work}/partial" config remote.origin.promisor true
refused partial-clone "could not be read from ${work}/partial without fetching" "${work}/partial" \
  "$(git -C "${work}/partial" rev-parse HEAD)" "${work}/out-11" "${work}/partial"

echo "passed ${passed} of $((passed + failed))"
[[ "${failed}" -eq 0 ]]
