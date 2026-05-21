# Ayaka.Nuke — Architecture Overview

> Reference document for the upcoming **NUKE → Cake Frosting** migration. Captures every load-bearing decision in `Ayaka.Nuke` so that the migration target can be evaluated against — and the eventual port can preserve — what actually matters.

---

## 1. Purpose

`Ayaka.Nuke` is an opinionated, **reusable component library on top of NUKE 9 (`Nuke.Common 9.0.4`)**, distributed as the `Ayaka.Nuke` NuGet package. It is consumed by other repositories' NUKE host projects (typically `nukebuild/Build.csproj`) via `PackageReference` or, when self-hosted as in this repo, `ProjectReference`.

The package's domain is the standard build/test/release pipeline for a .NET library:

- Clean → Restore → Build → Test → Pack → Push → Validate
- VitePress documentation install/lint/build
- GitHub release creation (notes + release + asset upload)
- Public-API shipping (`PublicAPI.Unshipped.txt` → `PublicAPI.Shipped.txt`)
- `dotnet-validate` execution

The repo eats its own dog food: `nukebuild/Build.cs` composes the very interfaces the package exports.

## 2. Repository Layout

```
.
├── Ayaka.sln
├── build.{cmd,ps1,sh}        # NUKE bootstrap shims
├── global.json               # SDK pin: 9.0.300, STS, no prerelease, rollForward=latestFeature
├── GitVersion.yml            # ContinuousDeployment + conventional-commit version bump regex
├── .config/dotnet-tools.json # nuke.globaltool 9.0.4
├── .nuke/
│   ├── build.schema.json     # generated parameter+target schema
│   └── parameters.json       # {"Solution":"Ayaka.sln"}
├── build/                    # *MSBuild* shared props (NOT the NUKE host)
│   ├── Analyzers.props
│   ├── Compiler.props
│   ├── Packages.props
│   ├── Product.props
│   ├── SourceLink.props
│   ├── TargetFramework.props # DotNetTargetFramework = net9.0
│   ├── Usings.props
│   └── assets/logo.png
├── nukebuild/                # *NUKE host* project
│   ├── Build.csproj
│   ├── Build.cs              # composition + orchestration targets
│   ├── Build.AfterRelease.cs # after-release branch/commit/PR flow
│   ├── Build.Utilities.cs    # one helper: From<T>() upcast
│   ├── Directory.Build.props
│   └── Directory.Build.targets
├── src/
│   ├── Directory.Build.props # IsPackable=True, IsNuGetPackage=True
│   └── Ayaka.Nuke/           # the component library (this doc's subject)
└── test/
    ├── Directory.Build.props
    └── Ayaka.Nuke.Tests/
```

**Key separation to keep in mind:** `build/` holds shared MSBuild `.props`; `nukebuild/` holds the NUKE host C# project. These are two unrelated concerns that just happen to live near each other.

## 3. Build Bootstrap

Standard NUKE bootstrap, nothing unusual:

- `build.sh` / `build.ps1` / `build.cmd` are the canonical NUKE shims. They look up `global.json`'s SDK, install it into `.nuke/temp/dotnet-{unix|win}/` if missing, set `DOTNET_CLI_TELEMETRY_OPTOUT=1`, `DOTNET_NOLOGO=1`, `DOTNET_MULTILEVEL_LOOKUP=0`, then `dotnet build nukebuild/Build.csproj` and `dotnet run --project nukebuild/Build.csproj -- "$@"`.
- `.nuke/parameters.json` defaults `Solution` to `Ayaka.sln` and references `build.schema.json` (auto-generated).
- `.config/dotnet-tools.json` pins `nuke.globaltool 9.0.4` (so `dotnet nuke` works directly).
- `nukebuild/Build.csproj` references `Nuke.Common 9.0.4` and `PackageDownload`s `dotnet-validate [0.0.1-preview.304]` and `GitVersion.Tool [5.12.0]` with exact-version brackets.
- `GitVersion.yml` uses `ContinuousDeployment` mode. Bump regexes map conventional commits → versions: `feat:` → minor, most others → patch, `!:` / `BREAKING CHANGE:` → major. `main` tagged `preview`, PRs tagged `pr`.
- `nukebuild/Build.csproj` `NoWarn`s the codestyle warnings that don't fit NUKE's idioms (`CS0649;CS0169;CA1050;CA1822;CA2211;IDE1006`) and sets `NukeRootDirectory=..`, `NukeScriptDirectory=..`, `NukeTelemetryVersion=1`.

## 4. The Core Design — Three Interface Families

This is the **single most important pattern** in Ayaka.Nuke. Everything the package ships is one of three kinds of interface, each constrained to `INukeBuild`:

| Family | Suffix | Purpose | Examples |
|---|---|---|---|
| **Context** | `IHave…` (no `Target`) | Adds parameters, paths, or attributed properties to the build | `IHaveSources`, `IHaveArtifacts`, `IHaveSolution`, `IHaveGitRepository`, `IHaveGitVersion`, `IHaveDotNetConfiguration`, `IHaveNuGetConfiguration`, `IHaveGitHubToken` |
| **Target definition** | `IHave…Target` | Declares just `Target Foo { get; }` — no implementation, no behavior | `IHaveCleanTarget`, `IHaveDotNetBuildTarget`, `IHaveGitHubReleaseTarget` |
| **Target implementation** | `ICan…` | Default-interface-method implementation of a `IHave…Target` | `ICanClean`, `ICanDotNetBuild`, `ICanGitHubRelease` |

Two empty root markers, both extending `INukeBuild`:

```csharp
// src/Ayaka.Nuke/IHave.cs
public interface IHave : INukeBuild { }

// src/Ayaka.Nuke/ICan.cs
public interface ICan : INukeBuild { }
```

### Why split definition from implementation?

Documented explicitly across the docs. Consumers depend on the **definition** (`IHaveDotNetBuildTarget`) when wiring `.DependsOn<…>()`. The **implementation** (`ICanDotNetBuild`) can be Ayaka's or the consumer's own — swapped at will without touching the dependency graph.

In practice the consumer build line looks like:

```csharp
class Build : NukeBuild,
    IHaveGitRepository, IHaveGitVersion, IHaveSolution, IHaveSources, IHaveTests,
    IHaveDocumentation, IHaveArtifacts, IHaveTestArtifacts, IHaveCodeCoverage,
    IHavePackageArtifacts, IHaveDocumentationArtifacts, IHaveDotNetConfiguration,
    IHaveNuGetConfiguration, IHaveGitHubToken,
    ICanClean, ICanDotNetRestore, ICanDotNetBuild, ICanDotNetTest, ICanDotNetPack,
    ICanDotNetValidate, ICanDotNetPush, ICanVitePressInstall, ICanVitePressLint,
    ICanVitePressBuild, ICanGitHubRelease, ICanShipPublicApis
{
    public static int Main() => Execute<Build>(x => x.Default);
    Target Default => target => target
        .DependsOn(Compile).DependsOn(Test).DependsOn(Pack);
    Target Compile => target => target
        .DependsOn<IHaveCleanTarget>()
        .DependsOn<IHaveDotNetBuildTarget>();
    // …
}
```

Each interface line both **declares context** (`IHave…` add properties to the build) and **adds targets** (`ICan…` contribute default-interface-method implementations). No DI registration, no inheritance, no codegen.

## 5. Settings Layering Pattern

Every `ICan…` that wraps a tool exposes **two** `Configure<TSettings>` properties:

```csharp
// sealed — consumers MUST NOT lose the baseline
[ExcludeFromCodeCoverage]
sealed Configure<DotNetBuildSettings> DotNetBuildSettingsBase
    => dotnet => dotnet
        .SetProjectFile(Solution)
        .SetConfiguration(DotNetConfiguration)
        .SetNoRestore(SucceededTargets.Contains(DotNetRestore))
        .When(IsServerBuild, x => x.EnableContinuousIntegrationBuild())
        .WhenNotNull(this as IHaveGitVersion, (d, o) => d
            .SetAssemblyVersion(o.Versioning.AssemblySemVer)
            .SetFileVersion(o.Versioning.AssemblySemFileVer)
            .SetInformationalVersion(o.Versioning.InformationalVersion));

// overridable — defaults to identity; the customization seam
[ExcludeFromCodeCoverage]
Configure<DotNetBuildSettings> DotNetBuildSettings
    => dotnet => dotnet;
```

Inside the target body they are applied in order:

```csharp
_ = DotNetTasks.DotNetBuild(d => d
    .Apply(DotNetBuildSettingsBase)
    .Apply(DotNetBuildSettings));
```

This is the **single customization seam** consumers use. `sealed` on Base prevents accidental loss; identity on the public one means override-or-don't.

The same pattern repeats for restore, test, pack, push, validate, npm, GitHub release, GitHub release notes. Where a tool has a "per-thing" loop (per test project, per package, per release artifact), a second pair appears: `…ProjectSettingsBase`/`…ProjectSettings`, etc.

## 6. Capability Detection Pattern

Two extension methods live in `src/Ayaka.Nuke/Extensions.cs`:

```csharp
public static T When<T>(this T settings, bool condition, Func<T, T> configurator)
    => condition ? configurator.Invoke(settings) : settings;

public static T WhenNotNull<T, TObject>(this T settings, TObject? obj, Func<T, TObject, T> configurator)
    => obj != null ? configurator.Invoke(settings, obj) : settings;
```

`WhenNotNull(this as IHaveGitVersion, (d, o) => …)` is the idiom for **opt-in cross-component enrichment**: if the consumer build also implements `IHaveGitVersion`, the tool settings get versioning data; otherwise nothing happens. This is how `ICanDotNetBuild` stamps `AssemblyVersion`/`FileVersion`/`InformationalVersion` only when the version capability is present, how `ICanDotNetPack` sets `RepositoryUrl` only when `IHaveGitRepository` is present, how `ICanDotNetTest` adds the XPlat coverage collector only when `IHaveCodeCoverage` is present, etc.

The same pattern appears in `Build.cs` itself (the consumer code), again on `Configure<DotNetPackSettings>`, to switch between `--version` and `--version-prefix` depending on whether GitVersion produced a pre-release label.

`Build.Utilities.cs` adds one helper used inside default interface methods to upcast `this`:

```csharp
T From<T>() where T : INukeBuild => (T)(object)this;
```

## 7. Component Inventory

### 7.1 General (root `Ayaka.Nuke` namespace)

| Interface | Kind | Surface |
|---|---|---|
| `IHave` | marker | extends `INukeBuild` |
| `ICan` | marker | extends `INukeBuild` |
| `IHaveGitRepository` | context | `[GitRepository] [Required] GitRepository` |
| `IHaveGitVersion` | context | `[GitVersion(NoFetch = true)] [Required] GitVersion Versioning` |
| `IHaveSolution` | context | `[Solution] [Required] Solution` |
| `IHaveSources` | context | `AbsolutePath SourceDirectory => RootDirectory / "src"` |
| `IHaveTests` | context | `AbsolutePath TestsDirectory => RootDirectory / "test"` |
| `IHaveDocumentation` | context | `AbsolutePath DocsDirectory => RootDirectory / "docs"` |
| `IHaveArtifacts` | context | `[Parameter("…", Name="Artifacts")] AbsolutePath ArtifactsDirectory => TryGetValue(…) ?? RootDirectory / "artifacts"` |
| `IHaveTestArtifacts : IHaveArtifacts` | context | `TestResultsDirectory => ArtifactsDirectory / "test-results"` |
| `IHaveCodeCoverage : IHaveArtifacts` | context | `CoverageDirectory => ArtifactsDirectory / "coverage"` |
| `IHavePackageArtifacts : IHaveArtifacts` | context | `PackagesDirectory => ArtifactsDirectory / "packages"` |
| `IHaveDocumentationArtifacts : IHaveArtifacts` | context | `DocsArtifactsDirectory => ArtifactsDirectory / "docs"` |
| `IHaveCleanTarget` | definition | `Target Clean { get; }` |
| `ICanClean : ICan, IHaveCleanTarget` | impl | dynamically globs `**/bin`, `**/obj` under sources/tests, `CreateOrCleanDirectory()`s artifacts — each branch gated by `if (this is IHave…)` |

### 7.2 `Ayaka.Nuke.DotNet`

`Configuration : Enumeration` — typed enum with `Debug`, `Release`, implicit `string` conversion, `TypeConverter<Configuration>` registration.

| Interface | Kind | Surface |
|---|---|---|
| `IHaveDotNetConfiguration` | context | `[Parameter] Configuration DotNetConfiguration => TryGetValue(…) ?? (IsLocalBuild ? Debug : Release)` |
| `IHaveDotNetRestoreTarget` | definition | `Target DotNetRestore` |
| `IHaveDotNetBuildTarget` | definition | `Target DotNetBuild` |
| `IHaveDotNetTestTarget` | definition | `Target DotNetTest` |
| `IHaveDotNetPackTarget` | definition | `Target DotNetPack` |
| `IHaveDotNetPushTarget` | definition | `Target DotNetPush` |
| `ICanDotNetRestore : IHaveSolution, IHaveDotNetRestoreTarget` | impl | `dotnet restore` on `Solution`; `TryDependsOn<IHaveCleanTarget>()`; Base = `SetProjectFile(Solution)` |
| `ICanDotNetBuild : IHaveSolution, IHaveDotNetConfiguration, IHaveDotNetRestoreTarget, IHaveDotNetBuildTarget` | impl | `dotnet build`; `DependsOn(DotNetRestore)`; `SetNoRestore(SucceededTargets.Contains(DotNetRestore))`; `EnableContinuousIntegrationBuild()` on CI; version stamping via `WhenNotNull(this as IHaveGitVersion, …)`; reports `Version` to summary |
| `ICanDotNetTest : IHaveTestArtifacts, IHaveSolution, IHaveTests, IHaveDotNetConfiguration, IHaveDotNetBuildTarget, IHaveDotNetTestTarget` | impl | `dotnet test`; per-project loop via `CombineWith(TestProjects, …)`; `degreeOfParallelism: 1`, `completeOnFailure: true`; produces `.trx`; per-project: `AddLoggers("GitHubActions;…")` iff GHA+package ref, `AddLoggers("trx;…")` always, `SetDataCollector("XPlat Code Coverage").EnableUseSourceLink()` iff `IHaveCodeCoverage` + coverlet package ref; copies cobertura via `CopyCoverageFiles` extension (de-dupes by GUID-named parent dir); `ReportTestCount()` post-step uses `XmlTasks.XmlPeek` on every `*.trx` to surface Passed/Failed/Skipped |
| `ICanDotNetPack : IHavePackageArtifacts, IHaveSolution, IHaveDotNetConfiguration, IHaveDotNetBuildTarget, IHaveDotNetPackTarget` | impl | `dotnet pack`; `DependsOn(DotNetBuild)`; `After<IHaveDotNetTestTarget>()` if present; `SetOutputDirectory(PackagesDirectory)`; sets `RepositoryUrl` from `IHaveGitRepository`; sets `Version` from `IHaveGitVersion`; reports package count |
| `ICanDotNetPush : IHavePackageArtifacts, IHaveNuGetConfiguration, IHaveDotNetPackTarget, IHaveDotNetPushTarget` | impl | `dotnet nuget push`; `--skip-duplicate`; source/key from NuGet config; `CombineWith(NuGetPackagesToPush, …)`; `degreeOfParallelism: 5`; per-package + global settings layered |

`TargetDefinitionExtensions.CopyCoverageFiles(testResultsDir, coverageDir)` — internal helper that globs `**/*.cobertura.xml`, filters to those whose parent dir name parses as `Guid` (XPlat duplicates the file once outside the per-test-run directory), and renames the copy `{guid}.cobertura.xml`.

### 7.3 `Ayaka.Nuke.DotNetValidate`

A typed wrapper around the `dotnet-validate` CLI, modeled on NUKE 9's modern `ToolTasks` system:

```csharp
[NuGetTool(Id = "dotnet-validate", Executable = "dotnet-validate.dll")]
public class DotNetValidateTasks : ToolTasks, IRequireNuGetPackage
{
    public const string DotNetValidatePackageId = "dotnet-validate";
    public const string DotNetValidateExecutablePath = "dotnet-validate.dll";
    // Exposes:
    //   DotNetValidate(ArgumentStringHandler …) — raw
    //   DotNetValidateLocalPackage(settings | Configure<…> | CombinatorialConfigure<…>)
    //   DotNetValidateRemotePackage(settings | Configure<…> | CombinatorialConfigure<…>)
}
```

Option types use NUKE 9's `[Command]` + `[Argument]` attribute system:

```csharp
[Serializable]
[Command(Type = typeof(DotNetValidateTasks),
         Command = nameof(DotNetValidateTasks.DotNetValidateLocalPackage),
         Arguments = "package local")]
public class DotNetValidateLocalPackageSettings : ToolOptions
{
    [Argument(Format = "{value}", Position = 1)]
    public string PackagePath => Get<string>(() => PackagePath);
}
```

Each option type has a matching `*Extensions` class with `[Builder]`-attributed `Set…`/`Reset…` fluent methods.

| Interface | Kind | Surface |
|---|---|---|
| `IHaveDotNetValidateTarget` | definition | `Target DotNetValidate` |
| `ICanDotNetValidate : IHavePackageArtifacts, IHaveDotNetValidateTarget` | impl | `OnlyWhenStatic(() => IsServerBuild)` — CI-only; `NuGetPackagesToValidate` defaults to `PackagesDirectory.GlobFiles("*.nupkg")`; `CombineWith(packages, …)`; `degreeOfParallelism: 1`, `completeOnFailure: true` |

### 7.4 `Ayaka.Nuke.NuGet`

Just configuration. No targets.

| Interface | Kind | Surface |
|---|---|---|
| `IHaveNuGetConfiguration` | context | `const DefaultNuGetSource = "https://api.nuget.org/v3/index.json"`; `[Parameter] string NuGetSource => TryGetValue(…) ?? DefaultNuGetSource`; `[Parameter, Secret] string? NuGetApiKey` |

### 7.5 `Ayaka.Nuke.GitHub`

The most substantial subsystem. Octokit-backed.

**`IHaveGitHubToken`** — `[Parameter] string? GitHubBaseUrl`; `[Parameter, Secret] string? GitHubToken => TryGetValue(…) ?? GitHubActions.Instance?.Token` (auto-fallback to GHA-provided token).

**Option hierarchy** (each with a matching `*Extensions` class of `Set…`/`Reset…` builders):

- `GitHubSettings : Options` — `RepositoryOwner`, `RepositoryName`, `Token`, `BaseUrl?`
- `GitHubReleaseSettings : GitHubSettings` — `Tag`, `TargetCommitish?`, `Name?`, `Body?`, `Draft?`, `PreRelease?`, `GenerateReleaseNotes?`, `ArtifactPaths?` (collection)
- `GitHubReleaseNotesSettings : GitHubSettings` — `Tag`, `TargetCommitish?`, `PreviousTag?`, `ConfigFile?`
- `GitHubPullRequestSettings : GitHubSettings` — `Head`, `Base`, `Title`, `Body?`, `Draft?`

**`GitHubTasks`** — static, Octokit-backed:

- `CreateRelease(settings | Configure<…>)` — idempotent: lists existing releases first, warns and returns if a release with `settings.Tag` exists, otherwise creates `NewRelease(Tag) { TargetCommitish, Name, Body, Draft, Prerelease, GenerateReleaseNotes }`, then loops `ArtifactPaths` and uploads each via `UploadAsset` as `application/octet-stream`.
- `GenerateReleaseNotes(settings | Configure<…>)` — returns `(Name, Body)` from `GenerateReleaseNotesRequest`.
- `CreatePullRequest(settings | Configure<…>)` — `NewPullRequest(Title, Head, Base) { Body, Draft }`.
- Helper `CreateGitHubClient(token, baseUrl?)` sets the assembly name as `ProductHeaderValue`.

| Interface | Kind | Surface |
|---|---|---|
| `IHaveGitHubReleaseTarget` | definition | `Target GitHubRelease` |
| `ICanGitHubRelease : IHaveGitRepository, IHaveGitVersion, IHaveGitHubToken, IHaveGitHubReleaseTarget` | impl | `Requires(() => GitHubToken)`; uses `GitRepository.GetGitHubOwner()/GetGitHubName()`; tag and name default to `v{Versioning.MajorMinorPatch}`, target commitish = `Versioning.Sha`; calls `GenerateReleaseNotes` then `CreateRelease(.SetBody(body))` — body chained between Base and Settings to allow override |

### 7.6 `Ayaka.Nuke.VitePress`

Three thin `npm` wrappers running inside `DocsDirectory`. `SetProcessOutputLogging(Verbosity == Verbosity.Verbose)` — output only logs on verbose, so build output stays clean.

| Interface | Kind | Surface |
|---|---|---|
| `IHaveVitePressInstallTarget` | definition | `Target VitePressInstall` |
| `IHaveVitePressLintTarget` | definition | `Target VitePressLint` |
| `IHaveVitePressBuildTarget` | definition | `Target VitePressBuild` |
| `ICanVitePressInstall : IHaveDocumentation, IHaveVitePressInstallTarget` | impl | `NpmTasks.NpmCi(SetProcessWorkingDirectory(DocsDirectory))` |
| `ICanVitePressLint : IHaveDocumentation, IHaveVitePressInstallTarget, IHaveVitePressLintTarget` | impl | `NpmRun` with `SetCommand("lint")`; `DependsOn<IHaveVitePressInstallTarget>`; `TryBefore<IHaveVitePressBuildTarget>()` |
| `ICanVitePressBuild : IHaveDocumentation, IHaveDocumentationArtifacts, IHaveVitePressInstallTarget, IHaveVitePressBuildTarget` | impl | `NpmRun` with `SetCommand("build").SetArguments("--outDir", DocsArtifactsDirectory)`; `DependsOn<IHaveVitePressInstallTarget>`; `TryDependsOn<IHaveVitePressLintTarget>()` |

The consumer overrides `Configure<NpmRunSettings> ICanVitePressLint.VitePressLintSettings` to switch the npm script (e.g., `"lint:js"`) — exactly what `nukebuild/Build.cs` does.

### 7.7 `Ayaka.Nuke.PublicApi`

| Interface | Kind | Surface |
|---|---|---|
| `IHaveShipPublicApisTarget` | definition | `Target ShipPublicApis` |
| `ICanShipPublicApis : IHaveSolution, IHaveSources, IHaveShipPublicApisTarget` | impl | iterates `PublicApiProjects` (default: all projects under `SourceDirectory`); for each: reads `PublicAPI.Shipped.txt` + `PublicAPI.Unshipped.txt`, moves every line from Unshipped → Shipped preserving the leading `#nullable enable` directive, sorts both with `IgnoreCaseWhenPossibleComparer`, rewrites both files as UTF-8 |

`IgnoreCaseWhenPossibleComparer` is `internal sealed`: ordinal case-insensitive comparison first, falls back to ordinal case-sensitive as a tiebreaker. Singleton via `Instance`.

## 8. Convention-over-Configuration Defaults

| Default | Source |
|---|---|
| `/src`, `/test`, `/docs`, `/artifacts` directories | `IHave…` properties |
| `artifacts/test-results`, `artifacts/coverage`, `artifacts/packages`, `artifacts/docs` | derived `IHave…` properties |
| `Debug` locally / `Release` on CI | `IHaveDotNetConfiguration` |
| `https://api.nuget.org/v3/index.json` | `IHaveNuGetConfiguration.DefaultNuGetSource` |
| Release tag/name = `v{Major.Minor.Patch}` | `ICanGitHubRelease` base settings |
| TRX logger always; `GitHubActionsTestLogger` only if project package-references it | `ICanDotNetTest` |
| Coverlet XPlat coverage only if project package-references coverlet.collector AND build implements `IHaveCodeCoverage` | `ICanDotNetTest` |
| `EnableContinuousIntegrationBuild()` only on CI | `ICanDotNetBuild` |
| `--no-restore` / `--no-build` if upstream target succeeded | `SucceededTargets.Contains(…)` |
| `dotnet nuget push --skip-duplicate` | `ICanDotNetPush` |
| `dotnet-validate` only on CI | `OnlyWhenStatic(() => IsServerBuild)` |

Every parameter-bound property is overridable through CLI (`--nuget-source …`), env var (`NUGET_SOURCE=…`), or `.nuke` profile.

## 9. Public API Discipline

Encoded in `build/Compiler.props`:

```xml
<PropertyGroup Condition="'$(IsNuGetPackage)' == 'True'">
  <UsePublicApiAnalyzers Condition="'$(UsePublicApiAnalyzers)' == ''">True</UsePublicApiAnalyzers>
  <WarningsAsErrors>$(WarningsAsErrors);RS0016;RS0017;RS0022;RS0024;RS0025;RS0026;RS0027;RS0036;RS0037;RS0041;RS0048;RS0050</WarningsAsErrors>
</PropertyGroup>
```

Every NuGet-packaged project must carry `PublicAPI.Shipped.txt` + `PublicAPI.Unshipped.txt`. New APIs land in Unshipped on PR. After a release, `ICanShipPublicApis` (driven by `Build.AfterRelease.cs`) moves them to Shipped on a generated `after-release` branch and opens a PR.

## 10. Test Surface

`test/Ayaka.Nuke.Tests/` is **deliberately narrow**. The whole framework — every default `IHave…` member, every `ICan…` body — carries `[ExcludeFromCodeCoverage]` because it's wiring, not logic. The tests cover only the parts where behavior actually exists:

| Test file | What it verifies |
|---|---|
| `ExtensionsTest` | both branches of `When` and `WhenNotNull` |
| `DotNetValidateLocalPackageSettingsExtensionsTest`, `…Remote…Test` | every `Set…` returns a *new* instance (immutability), every `Reset…` clears, originals untouched |
| `GitHubSettingsTest`, `GitHubPullRequest…Test`, `GitHubReleaseNotes…Test`, `GitHubRelease…Test` | same Set/Reset semantics for every option type, plus `AddArtifactPath`/`ClearArtifactPaths` collection behavior |
| `IgnoreCaseWhenPossibleComparer` | sort order; case-sensitive fallback for entries equal under ordinal-ignore-case |

The test project sets `EnableUnsafeBinaryFormatterSerialization=true` with a comment: *"Until NUKE decides to switch to JSON for cloning settings"* — NUKE 9 still uses `BinaryFormatter` for `Options.Modify`-style clones, and .NET 9 disables it by default.

## 11. Self-Hosting & Release Flow

`nukebuild/Build.cs` declares orchestration targets that just compose interface-supplied ones:

| Target | Composes |
|---|---|
| `Default` | `Compile`, `Test`, `Pack` |
| `Compile` | `IHaveCleanTarget`, `IHaveDotNetBuildTarget` |
| `Test` | `IHaveCleanTarget`, `IHaveDotNetTestTarget` |
| `Pack` | `IHaveCleanTarget`, `IHaveDotNetPackTarget`, `IHaveDotNetValidateTarget` |
| `Publish` | `IHaveDotNetPushTarget` |
| `Docs` | `IHaveVitePressBuildTarget` |
| `CreateRelease` | `IHaveGitHubReleaseTarget` |
| `AfterRelease` | `CreateAfterReleasePullRequest` |

Two notable consumer-side overrides:

- `ICanDotNetPack.DotNetPackSettings` is overridden to switch between `--version` and `--version-prefix` based on GitVersion's `PreReleaseLabel`. This lets tagged commits (no pre-release label) emit `--version-prefix` so individual packages can carry their own preview suffix; pre-release commits emit `--version` to force the GitVersion semver everywhere.
- `ICanVitePressLint.VitePressLintSettings` sets the npm command to `lint:js` (overriding the default `lint`).

### After-release flow (`Build.AfterRelease.cs`)

```
CreateAfterReleasePullRequest
└── CommitAndPushAfterReleaseChanges
    ├── CreateAfterReleaseBranch                       # before ShipPublicApis
    │   - require clean working copy
    │   - git fetch origin
    │   - assert no remote `after-release` exists
    │   - git checkout -b after-release origin/main
    └── IHaveShipPublicApisTarget                      # writes PublicAPI.*.txt
        - configure git user from --git-user-email/--git-user-name
        - git add **/PublicAPI.*.txt
        - if nothing staged: skipPullRequest = true; return
        - else: git commit -m "chore: ship public apis"; git push -u origin after-release
    # Then, OnlyWhenDynamic(() => !skipPullRequest):
    - GitHubTasks.CreatePullRequest(after-release → main,
        title="chore: after release changes",
        body=…)
```

The flow uses NUKE's standard `Nuke.Common.Tools.Git.GitTasks` for the git ops and Ayaka's own `GitHubTasks` for the PR, with `OfficialGitHubTasks.GetGitHubOwner/Name(GitRepository)` for owner/name extraction.

## 12. External Dependencies

| Package | Version | Pin | Used by |
|---|---|---|---|
| `Nuke.Common` | 9.0.4 | normal | the entire framework |
| `Octokit` | 14.0.0 | normal | `GitHubTasks` |
| `GitVersion.Tool` | 5.12.0 | `PackageDownload`, exact | `[GitVersion]` attribute |
| `dotnet-validate` | 0.0.1-preview.304 | `PackageDownload`, exact | `DotNetValidateTasks` |
| `Microsoft.CodeAnalysis.PublicApiAnalyzers` | 3.3.4 | `PrivateAssets=all` | applied to every NuGet-packaged project via `build/Analyzers.props` |
| `Microsoft.SourceLink.GitHub` | 8.0.0 | `PrivateAssets=all` | applied to every NuGet-packaged project via `build/SourceLink.props` |

`build/Packages.props` and `build/Analyzers.props` together pin everything that gets injected into source/test projects.

## 13. Migration-Critical Observations

Sorted by how load-bearing each thing is for `Ayaka.Nuke`'s value proposition. The migration plan needs answers for the top half; the bottom half can absorb compromises.

### Must survive

1. **Distribution as a NuGet package consumed by other repos.** Ayaka.Nuke's whole point is reuse across projects. Anything that only works in a single repo is a non-starter.
2. **Three-tier separation: context / target definition / target implementation.** Consumers should be able to depend on the *definition* without locking in the *implementation*, and to swap an Ayaka-provided implementation for their own.
3. **Settings layering (sealed Base + overridable Settings).** Consumers customize without ever losing the mandatory baseline.
4. **Capability detection** — "if this build also provides X, enrich behavior with X's data" (e.g., version stamping iff GitVersion present; coverage collection iff `IHaveCodeCoverage` present + project references `coverlet.collector`). This is what makes individual components composable.
5. **Convention-defaulted, CLI/env-overridable parameters** with a typed surface and `[Secret]` support for tokens/keys.
6. **Per-test-project capability detection** (does the project package-reference `GitHubActionsTestLogger`? `coverlet.collector`? add behavior conditionally).
7. **The full release-and-ship-APIs orchestration** including the auto-generated post-release PR. This is a complete, working pipeline that other repos plug into.

### Important but flexible

8. **Tool wrapping idiom** (typed `Options`/`Settings` with `Set…`/`Reset…` fluent builders). The shape matters less than the discipline: typed settings, immutable updates, fluent chaining.
9. **`dotnet-validate` integration.** Whatever the next system, it needs a clean way to wrap a dotnet CLI tool that's installed via the build project itself (no global install).
10. **`OnlyWhenStatic`/`OnlyWhenDynamic` / `Before` / `After` / `TryDependsOn` semantics.** Conditional execution and ordering must be expressible. The exact attribute/method names don't matter.
11. **Schema/help generation** (NUKE auto-generates `build.schema.json` from parameter and target metadata). Nice-to-have; can be substituted by docs.

### Incidental (NUKE-specific implementation detail)

12. The use of **default interface methods** to attach target bodies. Cake Frosting almost certainly will not use this mechanism; the migration needs a substitute pattern that achieves the same *result* — composable behavior without inheritance.
13. The `[Parameter]` / `[Secret]` attribute binding. Cake has its own argument-parsing model.
14. NUKE's `ITool` / `ToolTasks` / `[NuGetTool]` / `IRequireNuGetPackage` machinery. Cake has `Tool<T>` and addins.
15. The auto-generated `build.schema.json` and IDE integration.
16. The exact way targets reference each other (`DependsOn<T>()` taking a type). Cake uses `[IsDependentOn(typeof(T))]` — different but expressively similar.

---

## TL;DR for the migration design

`Ayaka.Nuke` is a **tiny, DI-free, interface-mixin component framework for NUKE 9 builds**, plus a small set of `Tasks` static classes that wrap external tools and APIs. The three-interface split (context / definition / implementation) is the whole intellectual content; everything else is conventional MSBuild/NuGet/.NET tooling. The migration target needs an answer for:

> "How does a consumer build add one symbol (an interface name, an attribute, a base class — *something*) and get a default target body + the context properties it requires, with the option to override the settings?"

Anything that requires the consumer to copy-paste a working target body into their own build project loses the value proposition.
