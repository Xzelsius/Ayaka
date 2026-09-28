#!/usr/bin/env bash
# Shared helpers for the renovate-sweep / renovate-fix scripts. Source it, don't execute it.

set -euo pipefail

repo_slug() {
    gh repo view --json nameWithOwner --jq .nameWithOwner
}

# Required status check contexts of the given base branch (default: main), one per line.
# Falls back to the contexts configured for this repository if branch protection can't be read.
required_checks() {
    local base="${1:-main}"
    gh api "repos/$(repo_slug)/branches/$base/protection/required_status_checks" --jq '.contexts[]' 2>/dev/null \
        || printf '%s\n' "Semantic PR" "build-docs" "build-packages" \
            "Codacy Diff Coverage" "Codacy Static Code Analysis" "Codacy Coverage Variation"
}

strip_ansi() {
    sed -E 's/\x1b\[[0-9;]*[A-Za-z]//g'
}

# jq function: summarizes a PR's statusCheckRollup against a JSON array of required contexts ($req).
# Yields {ci: pass|fail|pending, failed: [...], pending: [...], stability: pass|pending|absent}.
# shellcheck disable=SC2016
JQ_CI_SUMMARY='
def state:
  if .__typename == "StatusContext" then
    (if .state == "SUCCESS" then "pass" elif (.state == "FAILURE" or .state == "ERROR") then "fail" else "pending" end)
  elif .status != "COMPLETED" then "pending"
  elif (.conclusion == "SUCCESS" or .conclusion == "SKIPPED" or .conclusion == "NEUTRAL") then "pass"
  else "fail" end;
def ci_summary($req):
  (.statusCheckRollup // []) as $checks
  | [ $req[] as $name
      | ([ $checks[] | select((.name // .context) == $name) ] | last) as $c
      | { name: $name, state: (if $c == null then "pending" else ($c | state) end) } ] as $r
  | ([ $checks[] | select(.context == "renovate/stability-days") ] | last) as $s
  | { ci: (if any($r[]; .state == "fail") then "fail" elif any($r[]; .state == "pending") then "pending" else "pass" end),
      failed: [ $r[] | select(.state == "fail") | .name ],
      pending: [ $r[] | select(.state == "pending") | .name ],
      stability: (if $s == null then "absent" else ($s | state) end) };
'
