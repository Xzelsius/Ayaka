# Ayaka — Agent Guide

Ayaka is a collection of NuGet packages helping with .NET application development.
MIT licensed, docs at <https://xzelsius.github.io/Ayaka>.

## Packages

| Package                           | Path                                  | Status                                                                                                                                          |
|-----------------------------------|---------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------|
| `Ayaka.Nuke`                      | `src/Ayaka.Nuke`                      | stable — opinionated build components for [NUKE](https://nuke.build) (`IHave*` context interfaces, `ICan*` target interfaces, `*Tasks` classes) |
| `Ayaka.MultiTenancy.Abstractions` | `src/Ayaka.MultiTenancy.Abstractions` | preview                                                                                                                                         |
| `Ayaka.MultiTenancy`              | `src/Ayaka.MultiTenancy`              | preview                                                                                                                                         |
| `Ayaka.MultiTenancy.AspNetCore`   | `src/Ayaka.MultiTenancy.AspNetCore`   | preview                                                                                                                                         |

Everything targets **net10.0 only** — no multi-targeting, by explicit decision. The .NET SDK is pinned
to `10.0.301` in `global.json`.

## Repository map

- `src/` — package projects
- `test/` — `<Project>.Tests` xunit projects (not every package has one)
- `eng/` — shared MSBuild `.props` imported by `src/Directory.Build.props` and `test/Directory.Build.props`
  (target framework, compiler settings, package metadata, analyzers, global usings, dependency versions)
- `docs/` — VitePress site (`docs/en/...` pages, `docs/.vitepress/en.ts` sidebar/nav)
- `artifacts/` — build output (gitignored)
- `build.sh` — the build entry point, used locally and by CI

## Build, test, docs

`build.sh` restores the local .NET tools (GitVersion, dotnet-validate) itself when it needs them.

Fast inner loop (preferred while iterating):

```sh
dotnet build Ayaka.slnx
dotnet test test/Ayaka.MultiTenancy.Tests
dotnet test test/Ayaka.MultiTenancy.Tests --filter "FullyQualifiedName~AsyncLocalTenantContextAccessorTest"
```

Full pipeline (run before declaring work done — this is what CI runs):

```sh
./build.sh              # default: build test pack (Release, versioned by GitVersion)
./build.sh build test   # only the given steps, in the given order
./build.sh docs         # lint + build the VitePress docs
```

Steps: `version`, `build`, `test`, `pack`, `validate`, `docs`. CI runs `build test pack validate` and
`docs`. `validate` (dotnet-validate) only passes on CI builds, because deterministic builds are only
enabled when `GITHUB_ACTIONS` is set. On Windows run it from Git Bash.

Docs directly: `cd docs && npm ci && npm run dev` (live preview), `npm run build`, `npm run lint:js`.

## Public API gate (most common build failure)

Every package project tracks its public API with `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`
(Microsoft.CodeAnalysis.PublicApiAnalyzers; RS0016–RS0050 are **errors**).

- Adding/changing a public API → add the new symbol lines to that project's `PublicAPI.Unshipped.txt`
  (keep the `#nullable enable` header). IDEs with Roslyn support offer a
  code-fix that generates the lines; otherwise add them by hand following the format of existing entries.
- Removing a shipped API → add a `*REMOVED*`-prefixed line to `PublicAPI.Unshipped.txt`.
- **Never edit `PublicAPI.Shipped.txt`** — the after-release automation moves Unshipped → Shipped via
  `eng/scripts/ship-public-apis.sh` (which also applies `*REMOVED*` lines) and an auto-PR.

## Code style (essentials)

Authoritative sources: `.editorconfig` (mechanically enforced, `EnforceCodeStyleInBuild=true`) and
`docs/en/contributing/code-guidelines.md` — read the latter before non-trivial C# work.

Layout & structure:

- Every `.cs` file starts with the header: `// Copyright (c) Raphael Strotz. All rights reserved.`
- File-scoped namespaces, matching the folder structure
- `using` directives **inside** the namespace, `System.*` first
- 4-space indent, Allman braces

Naming:

- Interfaces `IFoo`
- Private fields `_camelCase`
- Type parameters `TFoo`

Idioms:

- `var` when the type is apparent
- Pattern matching preferred: `is null` over `== null`, `is not` over `!(...)`
- No primary constructors on classes or structs — use regular constructors (records are fine)
- Expression bodies for single-line members
- Avoid unnecessary code/clutter — match the surrounding style

Usings:

- `ImplicitUsings` is enabled
- `System.Diagnostics.CodeAnalysis` is a global using everywhere
- Test projects additionally get `Xunit`, `Shouldly`, and `FakeItEasy` as global usings — no
  explicit `using` needed for these

## Tests

xunit + Shouldly + FakeItEasy (see `eng/Packages.props` for versions).

- Test class: `<TypeUnderTest>Test` (singular, e.g. `AsyncLocalTenantContextAccessorTest`) in
  `test/<Project>.Tests` mirroring the source namespace
- Group related cases in nested classes named after the member/scenario under test (e.g.
  `AddMultiTenancy`, `When`)
- Method names are readable sentences in snake_case: `Getting_TenantContext_returns_null_when_no_TenantContext_is_set`
- Assert with Shouldly (`.ShouldBe(...)`), fake with FakeItEasy

## Commits, PRs, releases

- **Conventional commits** — PR titles are validated against `.github/semantic.yml`, so a
  non-matching type fails the PR check; GitVersion also derives the version from them
- Allowed types are a **closed set — exactly these, nothing else**: `chore`, `docs`, `feat`, `fix`,
  `refactor`, `revert`, `style`, `test` (optional `(scope)`)
- The standard Conventional Commits types `ci`, `build`, and `perf` are **not** enabled here — use
  `chore` for CI/workflow, build, tooling, and dependency changes
- Version bump: `feat:` → minor, everything else → patch, `!` or `BREAKING CHANGE:` footer → major
- Changelog sections come from **PR labels**: `breaking-change`, `enhancement`, `bug`, `dependencies`,
  `documentation`
- PRs target `main`; CI must pass (`build-packages` + `build-docs` run the same `./build.sh` steps)
- Releases are manual (`workflow_dispatch`), followed by an automated after-release PR — don't touch
  that machinery casually

## Documentation upkeep

- Public-surface or behavior changes to a package must update its docs under
  `docs/en/guide/packages/<package>/` (and the sidebar in `docs/.vitepress/en.ts` when adding pages)
- CI builds the docs site, so broken docs fail the PR
- Each package also has a `PACKAGE.md` used as the NuGet readme — keep it accurate

## Gotchas

- The MultiTenancy packages carry `<VersionSuffix>preview</VersionSuffix>` — API may still churn, but
  the Public API gate above applies regardless.
- `./build.sh` always builds `Release` with GitVersion-computed versions, so it is slower than the fast
  inner loop. Use plain `dotnet build` / `dotnet test` while iterating.
