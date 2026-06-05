---
name: docs-verify
description: |
  Visually verify the VitePress docs - build or serve the site, screenshot pages via Playwright MCP,
  check console errors and sidebar/nav rendering
argument-hint: "[page path, e.g. guide/packages/multi-tenancy]"
---

Verify how the docs render. Target page: $ARGUMENTS (default: the pages changed in the working tree).

Static checks first (no browser needed):

1. `cd docs && npm run lint:js && npm run build` — the same lint + build gate CI runs; the build
   fails on dead links, and the SSG output in `docs/.vitepress/dist/` is plain HTML you can read
   directly for content checks.
2. Skip the browser entirely for pure prose/link changes — step 1 covers those.

Visual checks (Playwright MCP server `playwright`):

3. Serve the site (in the background):
   - while iterating: `npm run dev` → <http://localhost:5173/Ayaka/>
   - verifying the real thing: `npm run build && npm run preview` → <http://localhost:4173/Ayaka/>
4. URL shape: base is `/Ayaka/`, `cleanUrls` is on, and `en/` is rewritten away —
   `docs/en/guide/packages.md` → `/Ayaka/guide/packages`.
5. For each target page: navigate, screenshot (desktop 1280px and mobile 375px), and check:
   - the sidebar shows the page / new entries (`docs/.vitepress/en.ts`)
   - custom containers (`::: warning` etc.) and code groups render
   - no console errors
   - dark mode if theme CSS was touched (toggle via the navbar button)
6. Screenshots land in `artifacts/playwright/` — reference them by path in your summary.
7. Kill the background server when done.

Troubleshooting:

- MCP server won't start → `cd docs && npm ci` (the server runs from `docs/node_modules`)
- Browser missing → `cd docs && npm run setup:browser` (installs the Chromium matching the pinned
  `@playwright/mcp` version)
- Images missing on a page → check whether their host is in `--allowed-origins` in `.mcp.json`;
  the allowlist must cover every host fetched at render time (badges, external figures), not
  navigation links
