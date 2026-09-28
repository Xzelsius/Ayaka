#!/usr/bin/env bash
# Watches a PR until its required checks have finished, it is merged, or its head changes.
#
# Usage: watch-pr.sh <pr> <expected-head-sha> [timeout-seconds=1800] [interval-seconds=15]
#
# Exit 0: merged, or all required checks finished (prints each result plus review and merge state).
# Exit 2: the head changed (someone pushed, e.g. Renovate raced a push). Exit 1: timeout.

set -euo pipefail
# shellcheck source=lib.sh
source "$(dirname "$0")/lib.sh"

pr="$1" expected="$2" timeout="${3:-1800}" interval="${4:-15}"
required="$(required_checks main | jq -R . | jq -s .)"
deadline=$(( $(date +%s) + timeout ))

while :; do
    json="$(gh pr view "$pr" --json state,headRefOid,statusCheckRollup,reviewDecision,mergeStateStatus,autoMergeRequest)"
    state="$(jq -r .state <<<"$json")"
    head="$(jq -r .headRefOid <<<"$json")"

    if [ "$state" = "MERGED" ]; then echo "MERGED"; exit 0; fi
    if [ "$state" = "CLOSED" ]; then echo "CLOSED" >&2; exit 2; fi
    case "$head" in
        "$expected"*) ;;
        *) echo "head changed: ${head:0:7} (expected ${expected:0:7})" >&2; exit 2 ;;
    esac

    summary="$(jq --argjson req "$required" "$JQ_CI_SUMMARY"' ci_summary($req)' <<<"$json")"
    if [ "$(jq -r .ci <<<"$summary")" != "pending" ]; then
        jq -r --argjson s "$summary" '"ci=\($s.ci) failed=\($s.failed | join(",")) review=\(.reviewDecision) merge=\(.mergeStateStatus) autoMerge=\(.autoMergeRequest != null)"' <<<"$json"
        exit 0
    fi
    if [ "$(date +%s)" -ge "$deadline" ]; then
        echo "timeout; still pending: $(jq -r '.pending | join(", ")' <<<"$summary")" >&2
        exit 1
    fi
    sleep "$interval"
done
