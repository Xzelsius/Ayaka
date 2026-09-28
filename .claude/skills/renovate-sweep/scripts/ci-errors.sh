#!/usr/bin/env bash
# Deduplicated errors of a PR's failing CI run(s).
#
# Usage: ci-errors.sh <pr-number>
#        ci-errors.sh --run <run-id>
#
# The failing runs are found through the checks' detailsUrl (`gh run list --branch` misses some Renovate branches).
# The last line is "PATTERN: known" when compiler, MSBuild, test or lint errors were found, and "PATTERN: none"
# otherwise. A failure without a known pattern is the only case where rerunning the failed jobs is allowed.

set -euo pipefail
# shellcheck source=lib.sh
source "$(dirname "$0")/lib.sh"

if [ "${1:-}" = "--run" ]; then
    runs=("$2")
else
    mapfile -t runs < <(gh pr view "$1" --json statusCheckRollup --jq '
        .statusCheckRollup[]
        | select(.__typename == "CheckRun")
        | select(.conclusion == "FAILURE" or .conclusion == "TIMED_OUT" or .conclusion == "CANCELLED" or .conclusion == "STARTUP_FAILURE")
        | .detailsUrl' | sed -nE 's#.*/actions/runs/([0-9]+)/.*#\1#p' | sort -u)
fi

if [ "${#runs[@]}" -eq 0 ]; then
    echo "No failing GitHub Actions run found."
    echo "PATTERN: none"
    exit 0
fi

known=0
for run in "${runs[@]}"; do
    echo "== run $run: $(gh run view "$run" --json jobs --jq '
        [ .jobs[] | select(.conclusion == "failure")
          | "\(.name) -> \([ .steps[] | select(.conclusion == "failure") | .name ] | join(", "))" ] | join("; ")')"

    log="$(gh run view "$run" --log-failed 2>&1 | strip_ansi)"

    # MSBuild/compiler: "path/File.cs(16,17): error CS0400: message [project]" or "CSC : error CS1705: ...",
    # also code-less target errors like "X.targets(355,5): error : message".
    build="$(grep -oE '([A-Za-z0-9_.-]+\.[A-Za-z]+\([0-9]+(,[0-9]+)?\)|CSC|MSBUILD) ?: (error|warning)( [A-Z]+[0-9]+)? ?:[^[]*(\[[^]]*\])?' <<<"$log" \
        | grep -E ': error' | sed -E 's#\[([^]]*[/\\])?([^]/\\]+)\]$#(\2)#; s/[[:space:]]+$//' | sort | uniq -c | sort -rn || true)"
    # dotnet test: "  Failed Namespace.Class.Method [12 ms]"
    tests="$(grep -oE '^[[:space:]]*Failed [A-Za-z0-9_.+<>,`]+ \[[^]]*\]' <<<"$log" | sed -E 's/^[[:space:]]+//' | sort -u || true)"
    # ESLint / VitePress: "  12:5  error  message  rule" and dead links
    lint="$(grep -E '^[[:space:]]+[0-9]+:[0-9]+[[:space:]]+error[[:space:]]|[Dd]ead link' <<<"$log" | sed -E 's/^.*Z //' | sort -u || true)"

    if [ -n "$build" ]; then known=1; echo "-- build errors (occurrences in the log, location: code: message (project))"; echo "$build"; fi
    if [ -n "$tests" ]; then known=1; echo "-- failed tests"; echo "$tests"; fi
    if [ -n "$lint" ]; then known=1; echo "-- lint / docs errors"; echo "$lint"; fi
    if [ -z "$build$tests$lint" ]; then
        echo "-- no known error pattern; last error annotations:"
        grep -E '##\[error\]' <<<"$log" | grep -v 'Process completed with exit code' | sed -E 's/^.*##\[error\]//' | sort -u | tail -10 || true
    fi
done

if [ "$known" -eq 1 ]; then echo "PATTERN: known"; else echo "PATTERN: none"; fi
