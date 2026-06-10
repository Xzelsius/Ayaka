#!/usr/bin/env bash
# PostToolUse hook: runs `dotnet format` on C# files edited by Claude Code.
# Fail-open by design — a formatting problem must never block an edit.

set -uo pipefail

input=$(cat)

# Extract the edited file path from the hook's stdin JSON.
file=$(jq -r '.tool_input.file_path // empty' <<< "$input" 2>/dev/null) || exit 0
[[ -z "$file" ]] && exit 0
[[ "$file" != *.cs ]] && exit 0
[[ "$file" == */bin/* || "$file" == */obj/* ]] && exit 0
[[ ! -f "$file" ]] && exit 0

root="${CLAUDE_PROJECT_DIR:-$(pwd)}"
cd "$root" 2>/dev/null || exit 0

# dotnet format expects workspace-relative --include paths.
rel="${file#"$root"/}"

dotnet format Ayaka.slnx --include "$rel" --no-restore --verbosity quiet >/dev/null 2>&1 || true

exit 0
