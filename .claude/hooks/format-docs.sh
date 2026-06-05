#!/usr/bin/env bash
# PostToolUse hook: runs `eslint --fix` (antfu config) on files under docs/ edited by Claude Code.
# Fail-open by design — a lint problem must never block an edit.

set -uo pipefail

input=$(cat)

# Extract the edited file path from the hook's stdin JSON.
file=$(jq -r '.tool_input.file_path // empty' <<< "$input" 2>/dev/null) || exit 0
[[ -z "$file" ]] && exit 0

root="${CLAUDE_PROJECT_DIR:-$(pwd)}"
docs="$root/docs"

[[ "$file" != "$docs"/* ]] && exit 0
[[ "$file" == */node_modules/* || "$file" == */.vitepress/cache/* || "$file" == */.vitepress/dist/* ]] && exit 0
[[ ! -f "$file" ]] && exit 0

# Only file types covered by the antfu ESLint config.
case "$file" in
  *.js | *.mjs | *.cjs | *.ts | *.mts | *.cts | *.md | *.json | *.jsonc | *.yml | *.yaml) ;;
  *) exit 0 ;;
esac

# ESLint not installed (npm ci not run yet) — fail open.
[[ ! -x "$docs/node_modules/.bin/eslint" ]] && exit 0

cd "$docs" 2>/dev/null || exit 0

rel="${file#"$docs"/}"

"$docs/node_modules/.bin/eslint" --fix "$rel" >/dev/null 2>&1 || true

exit 0
