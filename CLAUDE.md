# Claude Code instructions

@AGENTS.md

The shared agent guide above is the single source of truth — keep it (not this file) updated when
conventions change. The Copilot tree (`.github/copilot-instructions.md`, `.github/instructions/`)
is a derived copy because Copilot code review cannot read `AGENTS.md` — mirror convention changes
there too.

Claude-specific notes:

- Path-scoped conventions live in `.claude/rules/` and load automatically when you touch matching files.
- PostToolUse hooks auto-fix style after every edit (fail-open, see `.claude/hooks/`):
  - `dotnet format` for `*.cs`
  - `eslint --fix` for files under `docs/` (requires `npm ci` in `docs/` once).
  - Don't hand-fix pure formatting — it's already handled.
- Use `/new-package` to scaffold a new NuGet package (project, tests, PublicAPI files, docs, solution entries).
- Docs rendering: `npm run build` catches dead links and produces readable HTML in
  `docs/.vitepress/dist/`; for visual checks (theme, sidebar, containers, console errors) use
  `/docs-verify`, which drives the site via the Playwright MCP server (one-time browser setup:
  `cd docs && npm run setup:browser`).
- Prefer the fast inner loop (`dotnet build` / `dotnet test`) while iterating;
  run `./build.sh` once before declaring work done.
