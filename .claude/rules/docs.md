---
paths:
  - "docs/**"
---

# docs/ — additions to AGENTS.md

The full guide is in `AGENTS.md` (always loaded); this file only adds docs-specific detail.

- The `format-docs` hook auto-fixes ESLint style after every edit — don't hand-fix pure style;
  `npm run lint:js` is the full check CI runs
- The `en/` source segment is rewritten away at build time — `docs/en/guide/packages.md` is served
  at `/Ayaka/guide/packages`; write inter-page links accordingly
- New pages need a sidebar entry in `guideSidebar()` in `docs/.vitepress/en.ts`
