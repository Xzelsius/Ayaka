---
applyTo: "src/**/*.cs, src/**/PublicAPI.*.txt"
---

<!-- Derived from AGENTS.md and .claude/rules/ — Copilot code review reads only this tree; re-sync after changing those. -->

# src/ — shipped packages

Public API tracking:

- Every package project tracks its public API: add new/changed public symbols to
  `PublicAPI.Unshipped.txt`, keeping the `#nullable enable` header (files are UTF-8 with BOM —
  copy from an existing project to preserve encoding)
- Lines use the analyzer format, e.g.
  `Ayaka.MultiTenancy.AsyncLocalTenantContextAccessor.TenantContext.get -> Ayaka.MultiTenancy.TenantContext?`
- The analyzer error (RS0016 etc.) names the exact symbol text it expects — mirror it
- Removed shipped symbols get a `*REMOVED*` line in `PublicAPI.Unshipped.txt`
- Never modify `PublicAPI.Shipped.txt` — release automation owns it

Style:

- File header `// Copyright (c) Raphael Strotz. All rights reserved.` on every file
- File-scoped namespace matching the folder
- `using` directives inside the namespace, `System.*` first
- Read `docs/en/contributing/code-guidelines.md` before non-trivial changes

Dependencies:

- Package-specific NuGet dependencies go directly into the csproj with pinned versions (Renovate
  manages them); shared ones come via `eng/Packages.props`

Docs go with code:

- A change to a package's public surface or behavior must also update
  `docs/en/guide/packages/<package>/` (sidebar: `docs/.vitepress/en.ts`) and the project's
  `PACKAGE.md`

Breaking changes:

- `Ayaka.Nuke` is the only stable package — a breaking change there needs a `!`/`BREAKING CHANGE:`
  conventional commit **and** the `breaking-change` PR label (the commit drives the version, the
  label drives the changelog)
- The preview `Ayaka.MultiTenancy*` packages may break APIs, but the Public API files must still
  be updated in the same change
- `nukebuild/Build.csproj` project-references `src/Ayaka.Nuke` — breaking it breaks the repo's own
  build
