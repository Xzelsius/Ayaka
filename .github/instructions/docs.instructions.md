---
applyTo: "docs/**"
---

<!-- Derived from AGENTS.md and .claude/rules/ — Copilot code review reads only this tree; re-sync after changing those. -->

# docs/ — VitePress documentation site

Tooling & CI gate:

- `cd docs && npm run lint:js && npm run build` — the exact gate CI runs; broken links fail the
  build
- ESLint uses the antfu config plus `style/semi` — statements end with semicolons; it also lints
  fenced code blocks in Markdown, JSON, and YAML
- `npm run lint:js:fix` auto-fixes most style findings

Structure:

- Pages live under `docs/en/`; the `en/` segment is rewritten away at build time —
  `docs/en/guide/packages.md` is served at `/Ayaka/guide/packages` (base `/Ayaka/`, clean URLs)
- Sidebar and nav live in `docs/.vitepress/en.ts` (`guideSidebar()`); shared config in
  `docs/.vitepress/shared.ts`
- New pages need a sidebar entry in `en.ts`; new packages also get a row in
  `docs/en/guide/packages.md`
- Use VitePress containers (`::: warning`, `::: tip`, `::: info`) like the existing pages do
