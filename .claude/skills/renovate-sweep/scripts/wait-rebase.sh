#!/usr/bin/env bash
# Waits until a Renovate branch sits directly on top of origin/main (its head's parent is main's tip).
#
# Usage: wait-rebase.sh <branch> [timeout-seconds=900] [interval-seconds=15]
#
# Exit 0: up to date (prints head and main). Exit 1: timeout. Exit 3: the head is not Renovate's own commit,
# so Renovate won't rebase it (someone pushed on top). Never rebase Renovate's commit yourself instead:
# a Renovate job already in flight can force-push over it.

set -euo pipefail
# shellcheck source=lib.sh
source "$(dirname "$0")/lib.sh"

branch="$1" timeout="${2:-900}" interval="${3:-15}"
slug="$(repo_slug)"
deadline=$(( $(date +%s) + timeout ))

while :; do
    main="$(git ls-remote origin refs/heads/main | cut -f1)"
    head="$(git ls-remote origin "refs/heads/$branch" | cut -f1)"
    [ -z "$head" ] && { echo "branch $branch no longer exists" >&2; exit 1; }
    commit="$(gh api "repos/$slug/commits/$head" --jq '{parent: .parents[0].sha, author: (.author.login // .commit.author.name)}')"
    parent="$(jq -r .parent <<<"$commit")"
    author="$(jq -r .author <<<"$commit")"

    if [ "$parent" = "$main" ]; then
        echo "up to date: head=${head:0:7} main=${main:0:7}"
        exit 0
    fi
    case "$author" in
        renovate*) ;;
        *) echo "head ${head:0:7} is by $author, not Renovate; Renovate won't rebase it" >&2; exit 3 ;;
    esac
    if [ "$(date +%s)" -ge "$deadline" ]; then
        echo "timeout: head=${head:0:7} parent=${parent:0:7} main=${main:0:7}" >&2
        exit 1
    fi
    sleep "$interval"
done
