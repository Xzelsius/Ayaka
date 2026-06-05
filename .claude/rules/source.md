---
paths:
  - "src/**/*.cs"
  - "src/**/PublicAPI.*.txt"
---

# src/ — additions to AGENTS.md

The full guide is in `AGENTS.md` (always loaded); this file only adds src-specific detail.

Public API tracking:

- Lines in `PublicAPI.Unshipped.txt` use the analyzer format, e.g.
  `Ayaka.MultiTenancy.AsyncLocalTenantContextAccessor.TenantContext.get -> Ayaka.MultiTenancy.TenantContext?`
- The analyzer error (RS0016 etc.) names the exact symbol text it expects — mirror it
- When creating the files for a new project, copy them from an existing project so the encoding
  (UTF-8 with BOM) is preserved

Breaking changes:

- `Ayaka.Nuke` is the only stable package — a breaking change there needs a `!`/`BREAKING CHANGE:`
  conventional commit **and** the `breaking-change` PR label (the commit drives the version, the
  label drives the changelog)
- The preview `Ayaka.MultiTenancy*` packages may break APIs, but the Public API files must still be
  updated in the same change
