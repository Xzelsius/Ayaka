---
name: new-package
description: |
  Scaffold a new Ayaka NuGet package - src project, test project, PublicAPI files, PACKAGE.md, docs pages,
  VitePress sidebar, and solution entries
argument-hint: "[PackageName e.g. Ayaka.Caching]"
---

Scaffold a new Ayaka package named $ARGUMENTS (ask for the name if not provided; it must start
with `Ayaka.`). Conventions live in `AGENTS.md` and `.claude/rules/` — this file is just the
procedure.

Use the kebab-case of the feature part for docs paths (e.g. `Ayaka.MultiTenancy` → `multi-tenancy`).

1. **src project** — `src/<Name>/<Name>.csproj`, copying the shape of
   `src/Ayaka.MultiTenancy/Ayaka.MultiTenancy.csproj`:
   - `<TargetFramework>$(DotNetTargetFramework)</TargetFramework>`
   - `<VersionSuffix>preview</VersionSuffix>` — new packages start as preview
   - `<PackageDescription>... $(PackageDescriptionAppendix)</PackageDescription>`
   - `<PackageTags>$(PackageCommonTags);<feature-tags></PackageTags>`
   - package-specific NuGet dependencies go directly into this csproj with pinned versions
     (Renovate manages them); shared ones come via `eng/Packages.props`
2. **Public API files** — copy `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` from an
   existing src project (preserves the required encoding); both contain only `#nullable enable`
3. **PACKAGE.md** — NuGet readme; follow `src/Ayaka.MultiTenancy/PACKAGE.md`
   (About / Key Features / How to Use / Additional Documentation / Feedback & Contributing)
4. **test project** — `test/<Name>.Tests/<Name>.Tests.csproj`, copying the shape of
   `test/Ayaka.MultiTenancy.Tests/Ayaka.MultiTenancy.Tests.csproj`, with a `ProjectReference` to
   the new src project; add `InternalsVisibleTo` in the src csproj if tests need internals
5. **Solution** — add both projects to their solution folders:
   - `dotnet sln Ayaka.sln add src/<Name>/<Name>.csproj --solution-folder src`
   - `dotnet sln Ayaka.sln add test/<Name>.Tests/<Name>.Tests.csproj --solution-folder test`
6. **Docs** —
   - `docs/en/guide/packages/<kebab>/index.md`, starting with the `::: warning` preview note like
     `docs/en/guide/packages/multi-tenancy/index.md`
   - a sidebar section in `guideSidebar()` in `docs/.vitepress/en.ts`
   - a table row + link reference in `docs/en/guide/packages.md`
7. **Verify** — `dotnet build Ayaka.sln`, then `./build.sh` (Pack runs dotnet-validate over the
   new package), then `/docs-verify guide/packages/<kebab>` for the new page and sidebar entry

Do not invent initial APIs — leave the package empty or minimal unless the user specified contents.
