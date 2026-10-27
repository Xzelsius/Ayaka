#!/usr/bin/env bash
#
# Ships the public API of every project under src/: moves the entries of PublicAPI.Unshipped.txt into
# PublicAPI.Shipped.txt. A *REMOVED* entry deletes the matching line from PublicAPI.Shipped.txt instead.
#
# Only edits files, the after-release workflow commits them. To try it locally, run it, check
# `git diff` and revert with `git checkout -- src`.
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/../.."

header="#nullable enable"
removed_prefix="*REMOVED*"
failed=0

for unshipped in src/*/PublicAPI.Unshipped.txt; do
  project=$(dirname "$unshipped")
  shipped="$project/PublicAPI.Shipped.txt"
  if [[ ! -f $shipped ]]; then
    echo "Skipping $project, it has no PublicAPI.Shipped.txt"
    continue
  fi

  mapfile -t entries < <(tr -d '\r' < "$unshipped" | grep -vx -e "$header" -e '')
  if ((${#entries[@]} == 0)); then
    echo "Nothing to ship for $project"
    continue
  fi

  mapfile -t lines < <(tr -d '\r' < "$shipped" | grep -vx -e '')
  missing=0
  for entry in "${entries[@]}"; do
    if [[ $entry != "$removed_prefix"* ]]; then
      lines+=("$entry")
      continue
    fi

    entry=${entry#"$removed_prefix"}
    kept=()
    found=0
    for line in "${lines[@]}"; do
      if ((found == 0)) && [[ $line == "$entry" ]]; then
        found=1
      else
        kept+=("$line")
      fi
    done
    if ((found == 0)); then
      echo "$shipped has no entry '$entry' to remove" >&2
      missing=1
    fi
    lines=("${kept[@]}")
  done

  if ((missing == 1)); then
    failed=1
    continue
  fi

  # Case-insensitive ordinal, then ordinal, the order the existing files are in
  printf '%s\n' "${lines[@]}" | LC_ALL=C sort -f > "$shipped"
  printf '%s\n' "$header" > "$unshipped"
  echo "Shipped ${#entries[@]} entries for $project"
done

exit "$failed"
