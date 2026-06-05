<!-- Derived from AGENTS.md — Copilot code review reads only this tree; re-sync after changing AGENTS.md or .claude/rules/. -->

# Ayaka — Copilot instructions

Ayaka is a collection of NuGet packages helping with .NET application development.

`src/` contains the package projects, mirrored by `test/<Project>.Tests` xunit projects (not every
package has one). Everything targets net9.0 only; the SDK is pinned to 9.0.300 in `global.json`.

See `AGENTS.md` at the repo root for the full agent guide.

## Build & test

- `dotnet tool restore` once, then fast loop: `dotnet build Ayaka.sln`, `dotnet test test/<Project>.Tests`
- Full pipeline (what CI runs):
  - `./build.sh` — NUKE targets Compile → Test → Pack
  - `./build.sh Docs` builds the VitePress docs

## Public APIs

- Public APIs are tracked: add new/changed public symbols to the project's `PublicAPI.Unshipped.txt`
  (RS analyzer errors otherwise)
- Never edit `PublicAPI.Shipped.txt` — release automation owns it

## Code style

- Every `.cs` file starts with `// Copyright (c) Raphael Strotz. All rights reserved.`
- File-scoped namespaces matching folders
- `using` directives inside the namespace, `System.*` first
- 4-space indent, Allman braces
- Interfaces `IFoo`
- Private fields `_camelCase`
- Type parameters `TFoo`
- `var` when the type is apparent
- Pattern matching preferred: `is null` over `== null`, `is not` over `!(...)`
- Primary constructors preferred
- Expression bodies for single-line members
- `ImplicitUsings` is enabled, and `System.Diagnostics.CodeAnalysis` is a global using everywhere —
  no explicit `using` needed for either
- Avoid unnecessary code/clutter — match the surrounding style
- Full reference: `.editorconfig` + `docs/en/contributing/code-guidelines.md`

## Tests

- xunit + FluentAssertions + FakeItEasy via global usings (don't add `using Xunit;`)
- Class `<TypeName>Test`, snake_case sentence method names
- Group related cases in nested classes named for the member/scenario under test

## Commits & PRs

- Conventional commits and PR titles; types: `chore`, `docs`, `feat`, `fix`, `refactor`, `revert`,
  `style`, `test`
- Version bump: `feat:` → minor, everything else → patch, `!` or `BREAKING CHANGE:` footer → major
- PR labels drive the changelog: `breaking-change`, `enhancement`, `bug`, `dependencies`,
  `documentation`
- Breaking changes to `Ayaka.Nuke` (the only stable package) need both the `!`/`BREAKING CHANGE:`
  commit and the `breaking-change` PR label

## Documentation

- Public-surface or behavior changes must update `docs/en/guide/packages/<package>/` and the
  project's `PACKAGE.md`
