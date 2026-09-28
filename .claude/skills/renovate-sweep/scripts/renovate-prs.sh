#!/usr/bin/env bash
# Inventory of open Renovate PRs (or the given PR numbers) with their classification, as a JSON array.
#
# Usage: renovate-prs.sh [pr-number ...]
# Run it from the main checkout: it reads origin/main to decide whether a dependency is consumer-facing.
#
# Per PR: number, title, url, state, branch, head, deps [{name, from, to, update}], updateType (highest of the
# deps: major > minor > patch > digest/pin/lockfile), zeroVer, ecosystem, consumerFacing, ci {ci, failed,
# pending, stability}, behindBy, mergeState, reviewDecision, autoMerge, modifiedByOthers.

set -euo pipefail
# shellcheck source=lib.sh
source "$(dirname "$0")/lib.sh"

git fetch origin main --quiet
slug="$(repo_slug)"
main_sha="$(git rev-parse origin/main)"
required="$(required_checks main | jq -R . | jq -s .)"

if [ "$#" -gt 0 ]; then
    prs=("$@")
else
    mapfile -t prs < <(gh pr list --state open --author app/renovate --limit 100 --json number --jq '.[].number')
fi

# A dependency is consumer-facing when a shipped package pulls it in (a src/ project or the non-test group of
# eng/Packages.props), or when it is the .NET SDK. Majors of these may force a major Ayaka release.
consumer_facing() {
    local dep="$1"
    [ "$(printf '%s' "$dep" | tr '[:upper:]' '[:lower:]')" = "dotnet-sdk" ] && return 0
    git grep -q -i -F "Include=\"$dep\"" origin/main -- 'src/*.csproj' && return 0
    git show origin/main:eng/Packages.props 2>/dev/null \
        | awk '/!= .True./ { p = 1 } p && /<\/ItemGroup>/ { p = 0 } p' \
        | grep -q -i -F "Include=\"$dep\"" && return 0
    return 1
}

# PR body → TSV rows (name, from, to, update) from Renovate's "| Package | ... |" table.
parse_table() {
    awk -F'|' '
        function trim(s) { gsub(/^[ \t]+|[ \t]+$/, "", s); return s }
        /^\| *Package *\|/ {
            for (i = 1; i <= NF; i++) { h = trim($i); if (h == "Update") uc = i; if (h == "Change") cc = i }
            intable = 1; next
        }
        intable && /^\|[-| ]+\|$/ { next }
        intable && /^\|/ {
            name = $2
            if (match(name, /\[[^]]+\]/)) name = substr(name, RSTART + 1, RLENGTH - 2)
            change = $cc; from = ""; to = ""
            if (match(change, /`[^`]+`/)) { from = substr(change, RSTART + 1, RLENGTH - 2); change = substr(change, RSTART + RLENGTH) }
            if (match(change, /`[^`]+`/)) { to = substr(change, RSTART + 1, RLENGTH - 2) }
            update = (uc > 0) ? trim($uc) : ""
            printf "%s\t%s\t%s\t%s\n", trim(name), from, to, update
            next
        }
        intable { exit }
    '
}

# shellcheck disable=SC2016
jq_classify='
def ver: ltrimstr("v") | split(".") | map((capture("^(?<n>[0-9]+)").n | tonumber)? // 0);
def utype:
  if .update != "" then (.update | ascii_downcase | if . == "pindigest" then "pin" else . end)
  elif (.from | test("^[0-9a-f]{7,40}$")) and (.to | test("^[0-9a-f]{7,40}$")) then "digest"
  else (.from | ver) as $f | (.to | ver) as $t
    | if ($f[0] // 0) != ($t[0] // 0) then "major"
      elif ($f[1] // 0) != ($t[1] // 0) then "minor"
      else "patch" end
  end;
def rank: {"major": 4, "minor": 3, "patch": 2, "digest": 1, "pin": 1, "lockfile": 1}[.] // 0;
'

out="[]"
for pr in "${prs[@]}"; do
    json="$(gh pr view "$pr" --json number,title,url,state,headRefName,headRefOid,labels,body,mergeStateStatus,reviewDecision,statusCheckRollup,autoMergeRequest,commits)"
    body="$(jq -r .body <<<"$json")"

    deps="[]"
    while IFS=$'\t' read -r name from to update; do
        [ -z "$name" ] && continue
        cf=false
        consumer_facing "$name" && cf=true
        deps="$(jq --arg n "$name" --arg f "$from" --arg t "$to" --arg u "$update" --argjson cf "$cf" \
            "$jq_classify"' . + [ {name: $n, from: $f, to: $t, update: $u, consumerFacing: $cf} | .update = utype ]' <<<"$deps")"
    done < <(parse_table <<<"$body")

    head="$(jq -r .headRefOid <<<"$json")"
    behind="$(gh api "repos/$slug/compare/$main_sha...$head" --jq .behind_by 2>/dev/null || echo null)"

    entry="$(jq --argjson deps "$deps" --argjson req "$required" --argjson behind "$behind" \
        "$jq_classify$JQ_CI_SUMMARY"'
        (.title | ascii_downcase) as $t
        | ([ .labels[].name ] | map(select(. == "nuget" or . == "npm" or . == "github-actions")) | first) as $eco
        | {
            number, title, url, state,
            branch: .headRefName,
            head: .headRefOid,
            deps: $deps,
            updateType: (
                if ($t | test("lock file maintenance")) then "lockfile"
                elif ($t | test("^[^:]+: pin ")) then "pin"
                elif ($deps | length) == 0 then (if ($t | test("digest")) then "digest" else "unknown" end)
                else ($deps | max_by(.update | rank) | .update) end),
            zeroVer: ($deps | any(.[]; (.update == "major" or .update == "minor" or .update == "patch")
                and (.from | ver | .[0]) == 0 and (.to | ver | .[0]) == 0)),
            ecosystem: ($eco // "unknown"),
            consumerFacing: ($deps | any(.[]; .consumerFacing)),
            ci: ci_summary($req),
            behindBy: $behind,
            mergeState: .mergeStateStatus,
            reviewDecision,
            autoMerge: (.autoMergeRequest != null),
            modifiedByOthers: ([ .commits[].authors[].login ] | any(.[]; . != "renovate[bot]"))
          }' <<<"$json")"
    out="$(jq --argjson e "$entry" '. + [$e]' <<<"$out")"
done

jq . <<<"$out"
