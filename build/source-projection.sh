#!/usr/bin/env bash
# Makes the source projection the PowerShell module's ordinary CI job runs on. ci.sh runs it before
# build/Invoke-OrdinaryCi.ps1, which requires it:
#
#   build/source-projection.sh <checkout> <commit> <projection>
#
# <commit> must be the commit <checkout>, the top of a Git working tree, has checked out. <projection> becomes a new
# repository of that commit alone, shallow there, with no remote, alternate or partial clone, so it can fetch nothing.
# It holds the commit, its trees, an index of every file of the commit, and the files the commit's own
# build/ordinary-source-projection.txt lists, written from their objects in the commit. The files the list leaves out
# are known there only by their names and object IDs in the index, marked skip-worktree: their objects are neither read
# nor copied. Invoke-OrdinaryCi.ps1 checks all of this again before anything else.
#
# The checkout is only read, through Git that cannot fetch or follow replacement objects: its working tree, index and
# configuration are left as they are, and uncommitted changes there are not in the projection. <projection> must not
# exist, but its directory must, outside the checkout. On failure the projection made so far is removed, and nothing
# else.
set -euo pipefail
export LC_ALL=C

if [[ $# -ne 3 ]]; then
  echo "usage: source-projection.sh <checkout> <commit> <projection>" >&2
  exit 2
fi
checkout="$1"
commit="$2"
projection="$3"
list='targets/powershell/build/ordinary-source-projection.txt'

fail() {
  echo "source-projection.sh: $*" >&2
  exit 1
}

# Git on the checkout as Invoke-OrdinaryCi.ps1 runs it on the projection: every transport is refused, missing objects
# are not fetched and replacement objects are not used, whatever any Git configuration says.
sealed=(--no-replace-objects -c protocol.allow=never)
for protocol in file git http https ssh ext; do
  sealed+=(-c "protocol.${protocol}.allow=never")
done
source_git() {
  GIT_NO_LAZY_FETCH=1 git -C "${checkout}" "${sealed[@]}" --literal-pathspecs "$@"
}

for name in GIT_DIR GIT_WORK_TREE GIT_INDEX_FILE GIT_OBJECT_DIRECTORY GIT_ALTERNATE_OBJECT_DIRECTORIES \
  GIT_COMMON_DIR; do
  if [[ -n "${!name:-}" ]]; then
    fail "${name} is set, but the checkout and the projection are each a repository of its own"
  fi
done
if [[ ! "${commit}" =~ ^[0-9a-f]{40}$ ]]; then
  fail "'${commit}' is not a full commit ID"
fi

# The checkout: the top of its own working tree, at the commit.
if [[ ! -d "${checkout}" ]]; then
  fail "${checkout} is not a directory"
fi
checkout="$(cd "${checkout}" && pwd -P)"
top="$(source_git rev-parse --show-toplevel 2> /dev/null)" || fail "${checkout} is not in a Git working tree"
if [[ "${top}" != "${checkout}" ]]; then
  fail "${checkout} is not the top of its Git working tree, ${top}"
fi
if [[ "$(source_git rev-parse --show-object-format)" != sha1 ]]; then
  fail "${checkout} is not a SHA-1 repository"
fi
head="$(source_git rev-parse --verify --quiet 'HEAD^{commit}')" || fail "${checkout} has no commit checked out"
if [[ "${head}" != "${commit}" ]]; then
  fail "${checkout} has commit ${head} checked out, not ${commit}"
fi
tree="$(source_git rev-parse --verify --quiet "${commit}^{tree}")" || fail "commit ${commit} has no tree"

# The projection: a new directory, outside the checkout.
name="$(basename -- "${projection}")"
parent="$(dirname -- "${projection}")"
if [[ "${name}" == "." || "${name}" == ".." || "${name}" == "/" || ! -d "${parent}" ]]; then
  fail "${projection} is not a new directory in an existing one"
fi
projection="$(cd "${parent}" && pwd -P)/${name}"
if [[ -e "${projection}" || -L "${projection}" ]]; then
  fail "${projection} already exists"
fi
if [[ "${projection}/" == "${checkout}/"* ]]; then
  fail "${projection} is in the checkout"
fi

# The commit's own list: one '/'-anchored literal path a line, each line ending in LF, and names of letters, digits and
# . _ @ + -, but not '.', '..' or '.git'. Invoke-OrdinaryCi.ps1 checks the list in full; only listed paths that are
# so are ever written.
list_entry="$(source_git ls-tree --full-tree "${commit}" -- "${list}")"
if [[ ! "${list_entry}" =~ ^100(644|755)\ blob\ ([0-9a-f]{40})$'\t'"${list}"$ ]]; then
  fail "commit ${commit} has no file ${list}"
fi
list_object="${BASH_REMATCH[2]}"
text="$(source_git cat-file blob "${list_object}" && echo .)" || fail "${list} in commit ${commit} cannot be read"
text="${text%.}"
if [[ "${#text}" -ne "$(source_git cat-file -s "${list_object}")" || "${text}" != *$'\n' ]]; then
  fail "${list} in commit ${commit} is empty, holds a NUL or does not end its last line with LF"
fi
mapfile -t paths <<< "${text%$'\n'}"
pattern='^(/[A-Za-z0-9._@+-]+)+$'
for i in "${!paths[@]}"; do
  # Any case of .git, which a case-insensitive file system would take for the repository's own.
  folded="${paths[i],,}/"
  if [[ ! "${paths[i]}" =~ ${pattern} || "${folded}" == */./* || "${folded}" == */../* ||
    "${folded}" == */.git/* ]]; then
    fail "'${paths[i]}' in ${list} is not a literal '/'-anchored path"
  fi
  paths[i]="${paths[i]#/}"
done

# Each listed path: a file of the commit, by mode and object.
declare -A modes=() objects=()
while read -r mode type object path; do
  if [[ "${type}" == blob && ("${mode}" == 100644 || "${mode}" == 100755) ]]; then
    modes["${path}"]="${mode}"
    objects["${path}"]="${object}"
  fi
done < <(source_git ls-tree --full-tree "${commit}" -- "${paths[@]}")
for path in "${paths[@]}"; do
  if [[ -z "${objects["${path}"]:-}" ]]; then
    fail "${path}, which ${list} lists, is not a file of commit ${commit}"
  fi
done

# The commit's trees, by object.
trees="$(source_git ls-tree -r -t --full-tree --format='%(objecttype) %(objectname)' "${commit}")"
trees="$(awk '$1 == "tree" { print $2 }' <<< "${trees}")"

created=''
remove_projection() {
  status=$?
  trap - EXIT
  if [[ "${status}" -ne 0 && -n "${created}" ]]; then
    rm -rf -- "${projection}"
  fi
  exit "${status}"
}
trap remove_projection EXIT
mkdir -m 755 -- "${projection}"
created=yes
projection_git() {
  git -C "${projection}" "$@"
}
git init --quiet --template= --object-format=sha1 -- "${projection}"

# The commit, its trees and the listed files' objects, and no other, read from the checkout's repository.
{
  printf '%s\n' "${commit}" "${tree}" "${objects[@]}"
  if [[ -n "${trees}" ]]; then
    printf '%s\n' "${trees}"
  fi
} | source_git pack-objects --stdout -q | projection_git unpack-objects -q ||
  fail "the commit's objects could not be read from ${checkout} without fetching"
# The commit's parents are not in the projection: it is shallow at the commit.
header="$(projection_git cat-file commit "${commit}")"
if [[ $'\n'"${header%%$'\n\n'*}" == *$'\n'"parent "* ]]; then
  printf '%s\n' "${commit}" > "${projection}/.git/shallow"
fi
projection_git update-ref --no-deref HEAD "${commit}"

# Every file of the commit in the index; only the listed ones checked out, from their own objects, byte for byte.
projection_git read-tree "${commit}"
projection_git ls-files -z | projection_git update-index --skip-worktree -z --stdin
projection_git update-index --no-skip-worktree -- "${paths[@]}"
for path in "${paths[@]}"; do
  mkdir -p -- "$(dirname -- "${projection}/${path}")"
  projection_git cat-file blob "${objects["${path}"]}" > "${projection}/${path}"
  if [[ "${modes["${path}"]}" == 100755 ]]; then
    chmod +x -- "${projection}/${path}"
  fi
done

if [[ "$(projection_git rev-parse --verify 'HEAD^{commit}')" != "${commit}" ]]; then
  fail "the projection at ${projection} is not at commit ${commit}"
fi
echo "source-projection.sh: ${projection} holds commit ${commit}: ${#paths[@]} listed files checked out"
