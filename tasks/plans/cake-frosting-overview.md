# Cake Frosting — Migration Target Overview

> Sister document to `ayaka-nuke-overview.md`. Captures what **Cake Frosting** is, how it's used in production, and — critically — where it maps cleanly to Ayaka.Nuke's concerns and where it does not. The next phase will turn this into a concrete migration plan.

---

## 0. Executive summary

**Cake Frosting** is a regular .NET console application that hosts the Cake build engine. You write build logic as C# classes (`FrostingTask<TContext>`), wire dependencies with attributes (`[IsDependentOn(typeof(...))]`), and run it with `dotnet run --project build/Build.csproj -- --target Pack`. Latest released: **`Cake.Frosting 6.1.0` (2026-03-01)** targeting `net8.0`/`net9.0`/`net10.0`. Built on `Microsoft.Extensions.DependencyInjection` (Autofac under the hood) and `Spectre.Console.Cli`.

**Verdict for the Ayaka.Nuke port:**

- **The .NET pipeline (clean / restore / build / test / pack / push / npm / coverage) is a near-mechanical rewrite.** Every NUKE tool wrapper has a Cake equivalent that's either built-in (`Cake.Common.Tools.DotNet`, GitVersion alias, ReportGenerator) or a maintained addin (`Cake.Npm`).
- **The GitHub-API story is a small custom port:** `Cake.GitHub` covers only `CreateRelease`; `Cake.Octokit` doesn't exist. Just port `Ayaka.Nuke.GitHub/GitHubTasks.cs` verbatim, using `Octokit 14.0.0` directly.
- **`dotnet-validate` has no addin and no published example.** Port `DotNetValidateTasks` to a custom `Tool<TSettings>` subclass — well-understood Cake extension pattern.
- **The interface-mixin model does not survive.** This is the only painful part of the migration. Frosting's design is fundamentally different:
  - Tasks are **concrete public non-abstract classes**, discovered via `Assembly.GetExportedTypes()`. C# 8 default interface methods are invisible to discovery.
  - `[IsDependentOn(typeof(X))]` resolves by **task name string**, not type/interface — so `target.DependsOn<IHaveDotNetBuildTarget>()` has **no equivalent**.
  - "Capability detection" flows through the **context object**, not the **build class** (a structural inversion).
  - The closest analog is **composite context interface + shared task assembly** as used by `Grynwald.SharedBuild`, `Cake.Frosting.Issues.Recipe`, and `CreativeCoders.CakeBuild`.

Ayaka's "one interface → one target + one set of properties" ergonomic becomes "one interface on the context → one property; one `[TaskName]` constant + one concrete task class somewhere." Consumer code grows by roughly **15–30 lines per build** vs. NUKE today. That is the unavoidable cost of admission. Everything else is salvageable.

---

## 1. Cake Frosting at a glance

| | |
|---|---|
| **Latest release** | `Cake.Frosting 6.1.0` (2026-03-01); Cake umbrella v6.1.0 |
| **Target frameworks** | `net8.0`, `net9.0`, `net10.0` |
| **Project shape** | Regular `Microsoft.NET.Sdk` console app, `OutputType=Exe` |
| **Entry point** | `Program.Main` → `new CakeHost().UseContext<T>().Run(args)` |
| **Bootstrap** | Plain `dotnet run --project build/Build.csproj -- <args>`. No `nuke` tool, no `.nuke/parameters.json`. Optional `build.{ps1,sh,cmd}` shims for ergonomics. |
| **Addins** | Plain `<PackageReference>` — no `#addin` directive needed (Frosting-only difference vs. script mode) |
| **Tools** | Either `CakeHost.InstallTool(new Uri("dotnet:?package=…&version=…"))` or, idiomatically, `.config/dotnet-tools.json` + `Cake.DotNetLocalTools.Module` |
| **DI container** | `Microsoft.Extensions.DependencyInjection` (Autofac internally). `ConfigureServices(Action<IServiceCollection>)` is the universal escape hatch. |
| **Task discovery** | Reflective: every `public` non-abstract class implementing `IFrostingTask` in every registered assembly (`Assembly.GetEntryAssembly()` + every `AddAssembly(...)`) |
| **Logging** | `ICakeLog` on the context (`context.Log.Information(...)`). Text-based; no structured logging out of the box. `FormattableString` support added in 6.1. |
| **CLI** | Built-in: `--target`, `-v|--verbosity`, `--dryrun`, `--tree`, `--description`. Custom args read ad-hoc via `context.Argument<T>("name", default)`. |
| **License** | MIT |
| **Maintainership** | Active. cake-build org, multiple recent releases, healthy issue triage. |

**Minimum viable project skeleton** (canonical, mirrors `src/Cake.Frosting.Example/`):

```
build/
├── Build.csproj
├── Program.cs
├── BuildContext.cs
└── Tasks/
    ├── CleanTask.cs
    ├── BuildTask.cs
    ├── TestTask.cs
    └── PackTask.cs
build.ps1   # one-liner: dotnet run --project build/Build.csproj -- $args
build.sh    # one-liner: dotnet run --project build/Build.csproj -- "$@"
```

```xml
<!-- build/Build.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <RunWorkingDirectory>$(MSBuildProjectDirectory)/..</RunWorkingDirectory>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Cake.Frosting" Version="6.1.0" />
  </ItemGroup>
</Project>
```

```csharp
// build/Program.cs
public static class Program
{
    public static int Main(string[] args) =>
        new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
}
```

## 2. The core programming model

### 2.1 CakeHost

`CakeHost` is `sealed`. Customization happens via fluent `Use*<T>()` extension methods, every one of which ultimately calls `ConfigureServices(IServiceCollection)` (declared in `src/Cake.Frosting/Extensions/CakeHostExtensions.cs`):

```csharp
new CakeHost()
    .UseStartup<MyStartup>()              // IFrostingStartup.Configure(IServiceCollection)
    .UseContext<BuildContext>()           // custom FrostingContext
    .UseLifetime<BuildLifetime>()         // Setup + Teardown bundle (global)
    .UseSetup<BuildSetup>()               // Setup only
    .UseTeardown<BuildTeardown>()         // Teardown only
    .UseTaskLifetime<TaskLifetime>()      // per-task Setup + Teardown
    .UseTaskSetup<TaskSetup>()
    .UseTaskTeardown<TaskTeardown>()
    .UseModule<MyCakeModule>()            // ICakeModule registration
    .InstallTool(new Uri("dotnet:?package=GitVersion.Tool&version=5.12.0"))
    .UseWorkingDirectory("./src")
    .ConfigureServices(s => s.AddSingleton<IMyService, MyService>())
    .AddAssembly(typeof(MarkerInOtherAsm).Assembly)   // multi-assembly task scan
    .Run(args);
```

Two important properties:

- **Only the last `Use*` registration wins** — registrations are not additive. Significant when a reusable library composes its own `UseAyaka<T>()` extension that calls `UseContext<T>()` and `UseLifetime<L>()`.
- **`CakeHost` is sealed.** No "custom host" subclass. To customize, register services and modules.

### 2.2 FrostingContext

`FrostingContext` is a 6-line class inheriting `CakeContextAdapter`, which forwards every `ICakeContext` member to a wrapped instance (`FileSystem`, `Environment`, `Globber`, `Log`, `Arguments`, `ProcessRunner`, `Registry`, `Tools`, `Data`, `Configuration`):

```csharp
public class FrostingContext : CakeContextAdapter, IFrostingContext
{
    public FrostingContext(ICakeContext context) : base(context) { }
}
```

You subclass it to hold per-build state, read arguments in the constructor, and have services injected by DI. The context is registered as a **singleton**, so it's the natural place to pass data between tasks (there's no built-in `OutputCollection`):

```csharp
public class BuildContext : FrostingContext
{
    public string Configuration { get; }
    public DirectoryPath ArtifactsDirectory { get; }
    public GitVersion Version { get; }
    public IGitClient Git { get; }                          // DI-injected

    public BuildContext(ICakeContext context, IGitClient git) : base(context)
    {
        Git = git;
        Configuration       = context.Argument("configuration", "Release");
        ArtifactsDirectory  = context.Argument<DirectoryPath>("artifacts", "./artifacts");
        Version             = context.GitVersion();
    }
}
```

`UseContext<TContext>()` registers `TContext` as the resolved `IFrostingContext`. Tasks declare their context type as the generic argument of `FrostingTask<T>`; at task execution Frosting casts the singleton context to `T` (the resolved type must be `IsAssignableTo` the declared one).

### 2.3 FrostingTask&lt;TContext&gt;

Two base classes:

```csharp
public abstract class FrostingTask<T> : IFrostingTask where T : ICakeContext
{
    public virtual void Run(T context) { }
    public virtual bool ShouldRun(T context) => true;
    public virtual void OnError(Exception exception, T context) { }
    public virtual void Finally(T context) { }
}

public abstract class AsyncFrostingTask<T> : IFrostingTask where T : ICakeContext
{
    public virtual Task RunAsync(T context) => Task.CompletedTask;
    // ShouldRun / OnError / Finally as above
}
```

Attributes:

```csharp
[TaskName("Build")]                        // overrides the class-name-derived task name
[TaskDescription("Compiles the solution")] // shown by --description
[IsDependentOn(typeof(RestoreTask))]       // hard forward dependency, multiple allowed
[IsDependeeOf(typeof(PackTask))]           // hard reverse dependency
[ContinueOnError]                          // build continues if I throw
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => !context.SkipBuild;
    public override void Run(BuildContext context) { /* ... */ }
}
```

**Discovery rules** (from `src/Cake.Frosting/CakeHost.cs`):

```csharp
foreach (var type in assembly.GetExportedTypes())          // ONLY public types
    if (typeof(IFrostingTask).IsAssignableFrom(type) &&
        type.IsClass && !type.IsAbstract)                  // concrete classes only
        result.Add(type);
```

So: abstract base task classes are **not** discovered (they're templates — see §4.4). Generic open types are **not** discovered (the consumer must supply a closed-generic subclass). Internal types are **not** discovered.

**Naming**: `[TaskName]` value if present, otherwise the class's simple name (so `BuildTask` becomes the task name `"BuildTask"`, not `"Build"` — declare `[TaskName("Build")]` if you want the friendly name).

**Duplicate task names throw** at registration (`CakeEngine.cs`). Naming discipline becomes mandatory when shipping a reusable task library.

### 2.4 Lifetime hooks

Four hook surfaces. A `Lifetime` is just sugar for both setup and teardown:

| Scope | Bundle base | Setup-only | Teardown-only |
|---|---|---|---|
| Per-build (global) | `FrostingLifetime<T>` | `FrostingSetup<T>` | `FrostingTeardown<T>` |
| Per-task | `FrostingTaskLifetime<T>` | `FrostingTaskSetup<T>` | `FrostingTaskTeardown<T>` |

```csharp
public class BuildLifetime : FrostingLifetime<BuildContext>
{
    public override void Setup(BuildContext context, ISetupContext info)
    {
        context.Log.Information("About to run target '{0}'", info.TargetTask?.Name);
        // typical use: compute version, validate env, build a Projects[] registry
    }
    public override void Teardown(BuildContext context, ITeardownContext info)
    {
        if (!info.Successful)
            context.Log.Error("Build FAILED: {0}", info.ThrownException?.Message);
    }
}
```

Wired via `.UseLifetime<BuildLifetime>()`. Ordering:

```
Build Setup           → IFrostingLifetime.Setup
  ├─ TaskSetup        → IFrostingTaskLifetime.Setup
  ├─   ShouldRun
  ├─   Run / RunAsync
  ├─   OnError        (only on exception)
  ├─   Finally        (always)
  └─ TaskTeardown     → IFrostingTaskLifetime.Teardown
Build Teardown        → IFrostingLifetime.Teardown
```

`Setup` is only called if there's at least one task to run; `Teardown` always pairs with `Setup`.

### 2.5 Arguments, environment, secrets

Arguments: read ad-hoc on `ICakeContext`. Casing-insensitive. No type-system surface for "advertised parameters" the way NUKE's `[Parameter]` provides:

```csharp
bool   hasIt = context.HasArgument("verbose");
string cfg   = context.Argument("configuration", "Release");
int    count = context.Argument<int>("count", 5);
string must  = context.Argument<string>("apikey");                  // throws if missing
var    many  = context.Arguments<string>("rid", "linux-x64");       // repeatable
```

Type coercion via `System.ComponentModel.TypeConverter` — `DirectoryPath`, `FilePath`, `enum`, `Guid`, `bool`, all work.

Environment variables: same shape, on `ICakeContext` via the `EnvironmentAliases` (file name has a typo: `EnviromentAliases.cs`):

```csharp
string? token   = context.EnvironmentVariable("GITHUB_TOKEN");      // null if missing
int     build   = context.EnvironmentVariable<int>("BUILD_NUMBER", 42);
bool    hasIt   = context.HasEnvironmentVariable("CI");
```

Idiomatic "arg with env-var fallback" (lifted from `octokit/octokit.net/build/Lifetime.cs`):

```csharp
private static string? GetEnvOrArg(ICakeContext c, string env, string arg)
    => c.EnvironmentVariable(env) is { Length: > 0 } v ? v : c.Argument<string?>(arg, null);
```

**Secrets: no built-in primitive.** No `[Secret]` attribute, no prompt-on-missing UX, no auto-mask in logs. Idioms in production:

- Use environment variables, not CLI args (don't leak via shell history / process listings).
- If on GitHub Actions, register a mask: `context.GitHubActions().Commands.AddMask(token);`
- Wrap in an `EnvVar` value-object with `AssertHasValue()` (BenchmarkDotNet pattern).
- Centralize all token lookup in a `Credentials` record loaded once during lifetime setup (GitVersion pattern).
- Use `args.AppendSecret(token)` when building `ProcessSettings.Arguments` so it shows as `[REDACTED]` in CI logs (MvvmCross).

This is the **weakest UX gap** vs. NUKE's `[Secret]`. Plan to bake a thin wrapper into the `BuildContext`.

### 2.6 Dependencies and ordering — the narrow vocabulary

| NUKE                                    | Cake Frosting                                    |
|-----------------------------------------|--------------------------------------------------|
| `.DependsOn<T>()`                       | `[IsDependentOn(typeof(T))]` (hard)              |
| Inverse dep (declared on B for A)       | `[IsDependeeOf(typeof(A))]` on B (hard)          |
| `.TryDependsOn<T>()` (skip if T absent) | **No equivalent.** Make T's `ShouldRun` return false. |
| `.Before<T>()` (soft ordering, no trigger) | **No equivalent.** Closest is `IsDependeeOf` (but hard trigger). |
| `.After<T>()` (soft ordering, no trigger)  | **No equivalent.** Closest is `IsDependentOn` (hard trigger). |
| `.OnlyWhenStatic(() => …)`              | `override bool ShouldRun(T context)` |
| `.OnlyWhenDynamic(() => …)`             | Same — there's only one form |
| `.ProceedAfterFailure()`                | `[ContinueOnError]` |
| `.Unlisted`                             | **No equivalent.** Either make it `internal` (kills discovery) or accept it shows in `--description`/`--tree`. |
| `[Parameter("…")]` (advertised CLI surface) | Manual: `context.Argument(...)` in context ctor. No `--help` listing of custom args. |
| `[Secret]`                              | **No equivalent.** Env var + manual masking. |
| Targets are interfaces (mixins, DIM)    | **No equivalent.** Tasks must be concrete classes. |

The two missing pieces with biggest blast radius: **no soft ordering** (`Before`/`After`) and **no interface-typed dependencies**. See §4.

### 2.7 Calling Cake aliases from Frosting

Cake's "aliases" are extension methods on `ICakeContext` decorated with `[CakeMethodAlias]` / `[CakePropertyAlias]`. In Frosting you just `using` the namespace and call them on the context — there's no `#addin`:

```csharp
using Cake.Common.IO;                       // CleanDirectory, CopyFile, GetFiles
using Cake.Common.Tools.DotNet;             // DotNetBuild, DotNetTest, DotNetPack, DotNetRestore
using Cake.Common.Tools.DotNet.Pack;        // DotNetPackSettings
using Cake.Common.Tools.ReportGenerator;    // ReportGenerator
using Cake.Common.Xml;                      // XmlPeek
using Cake.Common.Diagnostics;              // Information(), Warning() shortcuts
using Cake.Common;                          // EnvironmentVariable, Argument, IsRunningOnWindows

public override void Run(BuildContext context)
{
    context.CleanDirectory(context.ArtifactsDirectory);
    context.DotNetBuild("./Ayaka.sln", new DotNetBuildSettings { Configuration = "Release" });
    context.Log.Information("Done.");
}
```

Third-party addins (e.g., `Cake.Npm`, `Cake.Coverlet`) become a `<PackageReference>` and contribute their extension methods on `ICakeContext`.

---

## 3. Feature mapping — Ayaka.Nuke surface → Cake Frosting

| Ayaka.Nuke concern | Frosting answer | Notes |
|---|---|---|
| **`dotnet` CLI** (`Cake.Common.Tools.DotNet`) | Built-in | Direct 1:1 mapping. `DotNetRestore/Build/Test/Pack/NuGetPush` aliases on `ICakeContext`, matching `*Settings` POCOs and fluent extensions. `.slnx` support added in 6.1.0. |
| **GitVersion** | Built-in alias `context.GitVersion(GitVersionSettings)` returning a POCO with `SemVer`/`NuGetVersionV2`/`AssemblySemVer`/`AssemblySemFileVer`/`InformationalVersion`/`Sha`/`ShortSha`/`PreReleaseLabel`/`MajorMinorPatch` — **field-for-field match with NUKE's `GitVersion`**. Requires `GitVersion.Tool` installed (via tool manifest or `InstallTool`). Existing `GitVersion.yml` keeps working. |
| **Octokit / GitHub releases / PRs / release notes** | **Use `Octokit 14.0.0` directly** from an `AsyncFrostingTask`. Port `Ayaka.Nuke/GitHub/GitHubTasks.cs` verbatim. `Cake.GitHub` covers only `CreateRelease` (no `GenerateReleaseNotes`, no `CreatePullRequest`) and was last released Aug 2024. `Cake.Octokit` does not exist. |
| **`dotnet-validate`** | **No addin.** Port `Ayaka.Nuke.DotNetValidate.DotNetValidateTasks` to a custom `Tool<DotNetValidate*Settings>` subclass under `Cake.Core.Tooling`. Alternative: `context.DotNetTool("validate package local <path>", new DotNetToolSettings { ... })` if you don't need typed settings. |
| **`npm` (VitePress install/lint/build)** | `Cake.Npm` 5.1.0 (active, last push 2026-04-28). Aliases `NpmCi`, `NpmRunScript`, `NpmInstall`. Working directory via the inherited `ToolSettings.WorkingDirectory` property (no `FromPath` setting). |
| **Code coverage** | Built-in: `DotNetTestSettings { Collectors = ["XPlat Code Coverage"] }` + `coverlet.collector` package in test projects. `Cake.Coverlet` only needed if you want to drive `coverlet.console` directly. |
| **HTML coverage reports** | Built-in `context.ReportGenerator(...)` alias (`Cake.Common.Tools.ReportGenerator`); requires `dotnet-reportgenerator-globaltool` on the manifest. |
| **TRX test count reporting** (Ayaka's `ReportTestCount`) | `context.XmlPeek(trxFile, "/x:TestRun/x:Results/x:UnitTestResult/@outcome", new XmlPeekSettings { Namespaces = { ["x"] = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010" } })`. Port the counting logic to a Frosting task. |
| **`Microsoft.CodeAnalysis.PublicApiAnalyzers` shipping** | **No addin.** Port `ICanShipPublicApis` logic (including `IgnoreCaseWhenPossibleComparer`) verbatim into a `FrostingTask`. Use `context.FileReadLines`/`context.FileWriteLines`. **No reviewed OSS Frosting project ships this** — it's an open opportunity (BenchmarkDotNet does the analogous thing for `AnalyzerReleases.Shipped.md` in `MoveAnalyzerRulesTask`). |
| **`Microsoft.SourceLink.GitHub`** | Unchanged — still an MSBuild-time `<PackageReference>` in `Directory.Build.props`. Frosting is irrelevant here. |
| **Git operations** (for after-release branch/commit/push) | **Avoid `Cake.Git`** — pinned x64-only per its NuGet page, blocks ARM CI runners. Use `context.StartProcess("git", new ProcessSettings { Arguments = "…" })` for the few git commands needed; matches what Ayaka does today with `Nuke.Common.Tools.Git.GitTasks` under the hood. |
| **Tool resolution** (NUKE `[NuGetTool]` / `IRequireNuGetPackage`) | `Cake.DotNetLocalTools.Module` 3.0.12 + `.config/dotnet-tools.json` (already present in the Ayaka repo). Add `.UseModule<LocalToolsModule>().InstallToolsFromManifest(".config/dotnet-tools.json")` to the host. Alternative: `host.InstallTool(new Uri("dotnet:?package=…&version=…"))` per tool. |
| **Schema/help generation** | None for custom args. Built-in `--description` lists every public task. For per-task argument help, port the **`TaskArgumentAttribute`** pattern from GitVersion's `build/common/Utilities/`. |
| **Build host bootstrap (`build.sh`/`build.ps1`/`build.cmd`)** | Keep the shims; replace bodies with `dotnet run --project build/Build.csproj -- "$@"`. The SDK-install logic can stay or be deleted depending on how much auto-bootstrap you want. |
| **`[Parameter]` advertised parameters** | No equivalent. Read in `BuildContext` constructor; document via README and/or a `Help` task that prints known keys. |
| **`OnlyWhenStatic(IsServerBuild)` for CI-only tasks** | `override bool ShouldRun(T) => context.BuildSystem().IsRunningOnGitHubActions` (or your own `context.IsCI` property). |
| **`[ExcludeFromCodeCoverage]` on framework wiring** | Equally useful in Frosting — same C# attribute. Apply to default `BuildContext` properties and any plumbing classes. |

### Composite mapping per Ayaka subsystem

| Ayaka namespace | Frosting equivalent |
|---|---|
| `Ayaka.Nuke` (general `IHave*`) | Properties on the `BuildContext` (or sub-interfaces of an `IAyakaContext`) — see §4 |
| `Ayaka.Nuke.ICanClean` | `CleanTask : FrostingTask<TContext>` — `context.CleanDirectories("**/{bin,obj}")` + `context.EnsureDirectoryExists(artifacts)` |
| `Ayaka.Nuke.DotNet.*` | Built-in `Cake.Common.Tools.DotNet` aliases + a `BuildContext` carrying `DotNet*Settings` defaults |
| `Ayaka.Nuke.DotNetValidate.*` | Custom `Tool<DotNetValidate*Settings>` subclass + an `Ayaka.Cake.DotNetValidate` namespace |
| `Ayaka.Nuke.GitHub.*` | Direct Octokit usage in `AsyncFrostingTask` — port `GitHubTasks`/`GitHubSettings` 1:1 |
| `Ayaka.Nuke.NuGet.IHaveNuGetConfiguration` | Properties on context (`NuGetSource`, `NuGetApiKey`) read from arg + env |
| `Ayaka.Nuke.VitePress.*` | `Cake.Npm` 5.1.0 — three tasks with `WorkingDirectory = "./docs"` |
| `Ayaka.Nuke.PublicApi.*` | Custom Frosting task; reuse `IgnoreCaseWhenPossibleComparer` |

---

## 4. The composition problem (the hard part)

Ayaka.Nuke's distinguishing value — the **interface-mixin model** — does not translate. This section is the honest diagnosis.

### 4.1 What Ayaka relies on

Three .NET features that Frosting's design doesn't engage with:

1. **C# 8 default interface methods.** Ayaka uses DIM on `ICan…` interfaces to *attach the target body to the build class* via mere interface implementation. Frosting registers `IFrostingTask` *classes*, not interface members — DIM bodies are invisible to discovery.
2. **NUKE's reflection-driven target discovery** walks the build class's interface tree and harvests `Target` members. Frosting walks assemblies for `IFrostingTask` types — different mechanism, different surface.
3. **Type-based dependency resolution** (`.DependsOn<IHaveDotNetBuildTarget>()`). Frosting's `[IsDependentOn(typeof(X))]` immediately flattens `X` to a name string (`FrostingEngine.cs`), then resolves by name in Cake.Core's task table. **You cannot depend on an interface or abstract base.**

### 4.2 The closest existing analog: composite context interface + shared task assembly

Used by all three substantial reusable Frosting libraries in the wild:

- **`Grynwald.SharedBuild`** (`ap0llo/shared-build`) — 10+ public consumer repos; closest to Ayaka's intent.
- **`Cake.Frosting.Issues.Recipe`** — same pattern, narrower scope.
- **`CreativeCoders.CakeBuild`** — same pattern plus generic abstract task templates.

The shape, simplified to the relevant bits:

```csharp
// ─── In the shared library Ayaka.Cake ───

// 1. Sub-capability interfaces (Ayaka's IHave… analogs, but on the *context*)
public interface IHaveSolution    : IFrostingContext { FilePath Solution { get; } }
public interface IHaveArtifacts   : IFrostingContext { DirectoryPath ArtifactsDirectory { get; } }
public interface IHaveGitVersion  : IFrostingContext { GitVersion Versioning { get; } }
public interface IHaveCodeCoverage : IFrostingContext { DirectoryPath CoverageDirectory { get; } }

// 2. Composite root interface that consumers implement
public interface IAyakaContext : IFrostingContext { /* may be pure marker */ }

// 3. Default context with virtuals (à la SharedBuild's DefaultBuildContext)
public abstract class DefaultAyakaContext : FrostingContext, IAyakaContext, IHaveSolution, IHaveArtifacts
{
    protected DefaultAyakaContext(ICakeContext c) : base(c) { }
    public virtual FilePath Solution => "./" + Path.GetFileName(c.Environment.WorkingDirectory.FullPath) + ".sln";
    public virtual DirectoryPath ArtifactsDirectory => "./artifacts";
}

// 4. Tasks typed against the interfaces they actually need
[TaskName(AyakaTaskNames.DotNetBuild)]
[IsDependentOn(typeof(DotNetRestoreTask))]
public sealed class DotNetBuildTask : FrostingTask<IFrostingContext>
{
    public override bool ShouldRun(IFrostingContext context) => context is IHaveSolution;
    public override void Run(IFrostingContext context)
    {
        var solution = ((IHaveSolution)context).Solution;
        var versioning = (context as IHaveGitVersion)?.Versioning;          // capability detect
        context.DotNetBuild(solution.FullPath, new DotNetBuildSettings
        {
            Configuration = "Release",
            MSBuildSettings = versioning is null ? null : new DotNetMSBuildSettings()
                .SetVersion(versioning.SemVer)
        });
    }
}

// 5. Single-call consumer API (the SharedBuild pattern)
public static class AyakaCakeHostExtensions
{
    public static CakeHost UseAyaka<TContext>(this CakeHost host, Func<Type, bool>? taskFilter = null)
        where TContext : class, IAyakaContext
        => host.UseContext<TContext>()
               .AddAssembly(typeof(AyakaCakeHostExtensions).Assembly, taskFilter);
}

// ─── In the consumer repo ───

public sealed class BuildContext(ICakeContext c) : DefaultAyakaContext(c),
    IHaveGitVersion, IHaveCodeCoverage
{
    public override FilePath Solution { get; } = "./Ayaka.sln";
    public GitVersion Versioning { get; } = c.GitVersion();
    public DirectoryPath CoverageDirectory => ArtifactsDirectory.Combine("coverage");
}

public static int Main(string[] args) => new CakeHost().UseAyaka<BuildContext>().Run(args);
```

The architectural inversion: in Ayaka the **build class** declares its capabilities; in Frosting the **context** does. Capability detection moves from `this as IHaveGitVersion` (inside a DIM body on the build) to `context as IHaveGitVersion` (inside a `Run` body on a task). Behaviorally identical; ergonomically more verbose.

### 4.3 Capability detection — three patterns in the wild

In descending order of fidelity to Ayaka's `this as IHaveX`:

**Pattern A — `this as` cast on the context (closest to Ayaka).**  Used by CreativeCoders.CakeBuild (`CakeBuildContext.GetSettings<T>` falls back to interface cast + property reflection). The context exposes sub-capabilities; tasks `context as IHaveX` to detect.

**Pattern B — Property containment with feature flags (most common in production).** Used by Grynwald.SharedBuild and friends. The context has concrete properties (`Git`, `GitHub`, `BuildSettings`); tasks consult them in `ShouldRun`. Capability is a *value*, not a *type*. Less elegant than Pattern A but easier to reason about.

**Pattern C — DI service resolution.** Used by Antda.Build. Capabilities are services registered in `ConfigureServices` and constructor-injected into tasks. Most "enterprise" feel; most boilerplate.

For Ayaka, **Pattern A** preserves the most of the original code shape. Pattern B is a fallback if specific properties don't fit the interface model.

### 4.4 Generic abstract task templates (complementary)

CreativeCoders ships abstract `XxxTask<T> where T : IXxxContext` templates and pre-bound `XxxTask : XxxTask<DefaultContext>` concrete subclasses. Consumers who want defaults consume the concrete class; consumers who need to override do the closed-generic subclass dance:

```csharp
public abstract class DotNetBuildTaskBase<T> : FrostingTask<T>
    where T : FrostingContext, IHaveSolution, IHaveDotNetConfiguration
{
    public override void Run(T context) { /* uses context.Solution, context.Configuration */ }
}

// In the consumer:
[TaskName("DotNetBuild")]
[IsDependentOn(typeof(DotNetRestoreTask))]
public sealed class DotNetBuildTask : DotNetBuildTaskBase<MyAyakaContext> { }
```

Works fully — Frosting walks `BaseType.GenericTypeArguments` to discover the context type. The price: **the consumer must declare every concrete subclass** to get the task registered.

### 4.5 The `[IsDependentOn]` problem

Ayaka's `.DependsOn<IHaveDotNetBuildTarget>()` lets a target depend on an *interface* — i.e., on the *abstraction*, not the implementation. Frosting cannot do this:

- `[IsDependentOn(typeof(X))]` is converted to the string `X.GetTaskName()` and resolved by name.
- If `X` is abstract, the abstract type isn't registered as a task — the *concrete subclass* is, under its own name.
- If `X` is an interface, the lookup string is the interface name — also not a registered task.

**Workarounds in order of preference:**

1. **Convention via constants class.** Ship `Ayaka.Cake.TaskNames` with `public const string DotNetBuild = "DotNetBuild";`. Library tasks use `[TaskName(TaskNames.DotNetBuild)]`. Cross-library deps still use `[IsDependentOn(typeof(ConcreteTaskInSameAsm))]`, but the *names* are stable so the consumer can reference them programmatically if needed.
2. **Programmatic dependency wiring at host setup** — drop attributes for the shared layer; have `UseAyaka<T>()` configure the engine directly. Maximum flexibility, much more code.
3. **Ship default concrete tasks with default dependencies; let consumers `taskFilter` them out** (the SharedBuild approach). Override means: exclude Ayaka's concrete task from `AddAssembly` and ship your own with the same `[TaskName]` constant.

**Honest assessment:** Ayaka's interface-typed dependency story has **no clean Frosting equivalent.** Pick a `TaskNames` convention and accept that swap-out happens at the *type* boundary (filter out theirs, ship yours), not at the *interface implementation* boundary.

### 4.6 Lost ergonomics — explicit list

| Ayaka.Nuke today | Frosting equivalent | Cost |
|---|---|---|
| Add one interface to the build class → get one target + its required context | Implement one interface on the context (one property) + ship one concrete `FrostingTask` class somewhere | ~3–5 lines per capability per consumer |
| Auto-populated `[GitVersion]` / `[Solution]` / `[GitRepository]` properties | Call `context.GitVersion()` / `context.Argument(...)` in the context constructor | One line per property in the ctor |
| `[Parameter]`/`[Secret]` advertise CLI surface to `--help` | Manual; build your own `Help` task | One-time investment |
| `.OnlyWhenStatic(...)` / `.OnlyWhenDynamic(...)` | `override bool ShouldRun(T)` | None — equivalent |
| `.TryDependsOn<T>()` (skip if T absent) | Make T's `ShouldRun` return false; or omit T from the assembly via `taskFilter` | Mild |
| `.Before<T>()` / `.After<T>()` (soft ordering) | None — only hard `[IsDependentOn]`/`[IsDependeeOf]` | Significant if Ayaka relies on this — but a survey of `Ayaka.Nuke` source shows it uses `.TryDependsOn<…>()`, `.TryBefore<…>()`, `.After<…>()` only in narrow places (clean-before-build, lint-before-build, pack-after-test). All can be re-expressed via hard deps + `ShouldRun`. |
| `.Unlisted` | None — make it `internal` (kills task discovery) or accept it shows in `--description` | Cosmetic |
| Auto `build.schema.json` from parameter+target metadata | None | Document via README |

### 4.7 The minimum viable consumer

```csharp
// build/Program.cs  (≈3 lines)
new CakeHost().UseAyaka<BuildContext>().Run(args);

// build/BuildContext.cs  (≈15–30 lines depending on capability count)
public sealed class BuildContext(ICakeContext c) : DefaultAyakaContext(c),
    IHaveGitVersion, IHaveCodeCoverage, IHaveDocumentation, IHaveGitHubToken
{
    public override FilePath Solution { get; } = "./MyApp.sln";
    public GitVersion Versioning { get; } = c.GitVersion();
    public string GitHubToken { get; } = c.EnvironmentVariable("GITHUB_TOKEN") ?? "";
}
```

That's the consumer surface. Compare to Ayaka today: ~25 interface names on the `Build` class declaration. Frosting consumer is ~15–30 lines of context-class code. **More verbose, but contained to one file.**

---

## 5. Addin and tooling inventory

Versions and maintenance status verified May 2026.

| Concern | Package | Latest | Repo | Status | Frosting use |
|---|---|---|---|---|---|
| Build host | `Cake.Frosting` | 6.1.0 (2026-03-01) | cake-build/cake | Active | The base SDK |
| Aliases | `Cake.Common` | 6.1.0 | cake-build/cake | Active | Transitively pulled in by Cake.Frosting; `DotNetBuild`, `GitVersion`, `ReportGenerator`, `XmlPeek`, `EnvironmentVariable`, `Argument`, etc. |
| Tool manifest | `Cake.DotNetLocalTools.Module` | 3.0.12 (2022-11-24) | cake-contrib/Cake.DotNetLocalTools.Module | Feature-complete; works on 6.1 | `.UseModule<LocalToolsModule>().InstallToolsFromManifest(".config/dotnet-tools.json")` |
| GitVersion | (built-in `Cake.Common`) | — | — | Active | Requires `GitVersion.Tool` on the manifest |
| GitVersion (alt) | `Cake.GitVersioning` | 3.9.50 (2025-11-05) | dotnet/Nerdbank.GitVersioning | Active | Use only if migrating to Nerdbank.GitVersioning; POCO surface differs |
| GitHub releases | `Cake.GitHub` | 1.0.0 (2024-08-13) | cake-contrib/Cake.GitHub | Dormant; covers only `CreateRelease` | **Not recommended for Ayaka.** Use Octokit directly. |
| GitHub API | `Octokit` | 14.0.0 (2025-01-08) | octokit/octokit.net | Very active | **Recommended.** Already used by Ayaka.Nuke today — port directly. |
| npm | `Cake.Npm` | 5.1.0 (2025-01-23) | cake-contrib/Cake.Npm | Active (2026-04-28) | Three VitePress tasks; use `WorkingDirectory` not `FromPath` |
| Coverage | (built-in `Cake.Common`) + `coverlet.collector` 10.0.1 | — | — | Active | `DotNetTestSettings.Collectors = ["XPlat Code Coverage"]` |
| Coverage (alt) | `Cake.Coverlet` | 6.0.1 (2026-03-09) | Romanx/Cake.Coverlet | Active | Only if you need `coverlet.console` directly — not necessary for Ayaka. |
| HTML reports | (built-in `Cake.Common`) | — | — | Active | `ReportGenerator` alias; requires `dotnet-reportgenerator-globaltool` |
| Git ops | `Cake.Git` | 5.0.1 (2024-12-16) | cake-contrib/Cake_Git | **x64 only — ARM unsupported.** | **Avoid.** Use `context.StartProcess("git", …)` instead. |
| dotnet-validate | — | — | — | No addin exists | Port `DotNetValidateTasks` to custom `Tool<TSettings>` subclass |
| Public API tracking | — | — | — | No addin exists | Port `ICanShipPublicApis` logic verbatim into a `FrostingTask` |
| VitePress | — | — | — | No addin exists | Three tasks wrapping `Cake.Npm` |
| Conventional commits | — | — | — | No addin (GitVersion handles it) | — |
| SourceLink | `Microsoft.SourceLink.GitHub` | 10.0.300 | dotnet/sourcelink | Active | Unchanged — MSBuild package in `Directory.Build.props` |

**Recommended `.config/dotnet-tools.json` for Ayaka after migration:**

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "gitversion.tool":                    { "version": "5.12.0", "commands": ["dotnet-gitversion"] },
    "dotnet-validate":                    { "version": "0.0.1-preview.304", "commands": ["dotnet-validate"] },
    "dotnet-reportgenerator-globaltool":  { "version": "5.4.13", "commands": ["reportgenerator"] }
  }
}
```

(Note: the `nuke.globaltool` line drops out.)

---

## 6. Real-world reference projects

Surveyed 30+ candidates; 10 representative builds reviewed in depth. **Top picks** for borrowing patterns, in priority order:

### 1. `ap0llo/shared-build` (`Grynwald.SharedBuild`) — closest existing analog to Ayaka.Nuke

- 10+ public consumers (`ap0llo/changelog`, `ap0llo/mddocs`, `ap0llo/markdown-generator`, `ap0llo/xmldocs`, `ap0llo/Cake.Asciidoctor`, `ap0llo/Cake.GitLab`, `ap0llo/cake-addin-generator`, …).
- **`UseSharedBuild<TContext>(taskFilter)` extension** is exactly the API shape Ayaka should ship.
- **`IBuildContext` interface + `DefaultBuildContext` with virtual everything** is the canonical interface-based-with-override pattern.
- **`IPushTarget` collection** lets consumers declare named NuGet feeds with activation predicates.
- **`PrintBuildContextTask` with `IPrintableObject.PrintToLog(ICakeLog)`** for diagnostic dumps.
- Builds itself by including its own sources via `<Compile Include="..\src\SharedBuild\**\*.cs" />` — neat bootstrap.

### 2. `GitTools/GitVersion`'s `build/` — best end-to-end pipeline

- **Multi-project layout**: `build/CI.slnx` containing `common.csproj` (library) + 7 stage executables (`build`, `release`, `publish`, `docs`, `docker`, `config`, `artifacts`). Each stage has its own `BuildContext`, `BuildLifetime`, `Program.cs`, `Tasks/`.
- **C# 13 `extension(...)` blocks** (`build/common/Utilities/CakeHostExtensions.cs`) for registering host helpers.
- **`Directory.Build.props` with global usings** so task files are `using`-free.
- **Central NuGet pinning** via `build/Directory.Packages.props`.
- **`Wrapper + Internal` task split** for CI-only guards — public task names stay clean in pipeline composition; internal tasks own the `ShouldRun` guards.
- **OIDC-based NuGet publishing** (`build/publish/Tasks/PublishNuget.cs`) — trusted publishing with no static API key.
- **`TaskArgumentAttribute` family** for self-documenting per-task CLI arguments.
- **Public-API shipping is a PowerShell script** (`src/mark-shipped.ps1`) — not a Frosting task. Opportunity for Ayaka to do better.

### 3. `dotnet/BenchmarkDotNet`'s `build/`

- **Runner-class pattern**: tasks delegate to `BuildRunner`/`ReleaseRunner`/`UnitTestRunner`/`DocumentationRunner`/`GitRunner`. Eager construction in `BuildContext.ctor`. Task bodies stay ~5 lines.
- **`MoveAnalyzerRulesTask`** for the `AnalyzerReleases.Shipped.md` workflow — directly analogous to `ICanShipPublicApis`. This is the template for Ayaka's port.
- **Direct `Octokit 14.0.0`** for releases (`ReleaseRunner.PublishGitHubRelease()`).
- **`EnvVar` value-object** with `AssertHasValue()`/`SetEmpty()` — typed env access.
- **All tasks in one `Program.cs`** — but readable because each is a few lines + runner delegation.

### 4. `DrBarnabus/Mimic` (and `DrBarnabus/DacTools`)

- Same multi-project layout as GitVersion in miniature (~150 lines per project).
- **Generic `BuildLifetimeBase<TContext>` where `TContext : BuildContextBase`** — single source of truth for repo/CI/branch detection across stages.
- **Pre-release/release/stable detection logic** ready to copy (`build/Publish/Tasks/PublishNuGet.ShouldRun`).
- Smaller starting point than GitVersion if the 8-project layout feels heavy.

### 5. `ap0llo/changelog`'s `build/`

- The canonical **consumer** example. Shows what Ayaka's downstream repos should look like after migration:
  ```csharp
  // build/Program.cs (~5 lines)
  return new CakeHost()
      .UseModule<AzurePipelinesModule>()
      .UseModule<LocalToolsModule>()
      .InstallToolsFromManifest(".config/dotnet-tools.json")
      .UseSharedBuild<BuildContext>(t => t != typeof(GenerateChangeLogTask))
      .Run(args);

  public class BuildContext : DefaultBuildContext { /* PushTargets and overrides */ }
  ```

Other notable but lower-priority references: `octokit/octokit.net` (clean simple single-project), `MonoGame/MonoGame` (subsystem-folder layout, `HttpClient` via DI), `MvvmCross/MvvmCross` (single-file build, `AppendSecret`), `testcontainers/testcontainers-dotnet` (global usings file, `Parameters` POCO), `darlov/Antda.Build` (DI-heavy alternative library), `yavorfingarov/Yf.Cake.Layers` (abstract-base-class-per-stage pattern).

---

## 7. Patterns to borrow (consolidated)

From the cross-project survey:

**Layout**
- `build/` next to the solution root.
- For library + docs + release: multi-project (GitVersion / DacTools / Mimic). `build/Common/` (library) + `build/Build/` + `build/Pack/` + `build/Release/` + `build/Docs/`, all referenced from `build/CI.slnx`.
- `build/Directory.Build.props` with global usings.
- `build/Directory.Packages.props` with central package management.

**Task organization**
- One task per file; class name matches `[TaskName(nameof(X))]`.
- No "Task" suffix (GitVersion, Mimic, SharedBuild) — though it's common elsewhere (Antda, MonoGame).
- Pipeline parent tasks as bodyless class declarations: `public class Package : FrostingTask<BuildContext>;` (C# 13).
- **Wrapper + Internal split**: public bodyless task `[IsDependentOn(typeof(XInternal))]`, internal task with the `ShouldRun` guards and `Run` body. Allows local invocation without CI gates.

**Helpers**
- Static `Constants` / `Arguments` / `EnvVars` / `Paths` / `Tools` classes at the bottom of the dependency tree.
- `CakeHostExtensions` with C# 13 `extension(CakeHost host)` blocks.
- `IPrintableObject` interface + indented logger for dumping the entire context tree (`PrintBuildContextTask`).

**Secrets**
- Centralize in a `Credentials` record loaded in lifetime setup.
- `EnvVar` value-object with `AssertHasValue()`.
- `args.AppendSecret(token)` for `ProcessSettings.Arguments`.

**Versioning**
- GitVersion CLI via `Cake.DotNetLocalTools.Module` reading `.config/dotnet-tools.json`.
- MSBuild settings populated in the lifetime: `SetVersion`/`SetAssemblyVersion`/`SetFileVersion`/`SetInformationalVersion`.
- A `BuildVersion` record wrapping the gitversion POCO into project-specific shape.

**NuGet**
- `DotNetPack` with `Deterministic=true`, `ContinuousIntegrationBuild=true` on CI.
- `IncludeSymbols=true`, `SymbolPackageFormat=snupkg`.
- `DotNetNuGetPush` with `SkipDuplicate=true`.
- Per-package iteration via `context.GetFiles("artifacts/packages/*.nupkg")`.
- Different sources by version stream (preview → GitHub Packages / release → nuget.org).
- OIDC-first publishing when available (GitVersion pattern).

**GitHub releases**
- Use **Octokit directly** from `AsyncFrostingTask` (best fit for Ayaka — preserves the existing `GitHubTasks` surface).
- Or use **`Cake.GitHub`** if `CreateRelease` is all you need (Ayaka needs more, so skip).
- Or use **`GitReleaseManager.Tool`** via `GitReleaseManagerCreate`/`AddAssets`/`Close` (heavier; introduces another tool dependency).

---

## 8. Known gaps and open opportunities

After the survey, several Ayaka.Nuke features have **no Frosting equivalent in the OSS ecosystem**. Each is an opportunity for the migrated `Ayaka.Cake` to be uniquely valuable:

1. **Public-API shipping (`PublicAPI.Shipped.txt`/`PublicAPI.Unshipped.txt`).** No Frosting library does this. GitVersion uses PowerShell; BenchmarkDotNet does the analogous thing for `AnalyzerReleases.Shipped.md` in `MoveAnalyzerRulesTask`. Porting `ICanShipPublicApis` (with `IgnoreCaseWhenPossibleComparer`) would be ~80 lines and genuinely unique.
2. **`dotnet-validate` wrapper.** No addin, no published example. Port `DotNetValidateTasks` to a custom `Tool<TSettings>` subclass.
3. **VitePress task suite.** Multiple projects use Wyam or Docfx; none ship a polished VitePress install/lint/build task trio.
4. **Coherent GitHub-release flow** (notes + release + asset upload, with idempotency). Existing options are fragmented across three packages; SharedBuild's `CreateGitHubReleaseTask` is closest but tied to Nerdbank.GitVersioning.
5. **OIDC-first NuGet push** as a reusable task. Only GitVersion's build does this; not extracted.
6. **`[Parameter]`/`[Secret]` ergonomics.** A small library that adds attribute-based parameter declarations on the `BuildContext` (read in ctor; advertised via `--help`) would be useful in the wider Frosting ecosystem.

If Ayaka.Cake ships all of (1)–(5) with the SharedBuild-style consumer ergonomic, it occupies a clear niche: **"the .NET-library build kit for Cake Frosting that GitVersion's build is, but as a library."**

---

## 9. Honest limitations summary

To be re-read whenever the migration plan suggests something rosy:

- **`CakeHost` is sealed.** No custom host. Compose via DI.
- **Tasks must be public, non-abstract, concrete classes.** Abstract base templates work but consumers need a concrete subclass. Open generic task types are not discovered.
- **`[IsDependentOn(typeof(X))]` resolves by name, not type.** Cannot depend on an interface or abstract base. Naming discipline is mandatory.
- **No soft ordering.** `.Before<T>()` / `.After<T>()` have no equivalent.
- **No `TryDependsOn`.** Every `[IsDependentOn]` is hard. Skip with `ShouldRun` or filter out at host wiring.
- **No `[Parameter]` / `[Secret]`.** No advertised CLI surface for custom args. Build your own help generator if you want one.
- **No `Unlisted`.** Public tasks show in `--description` and `--tree`.
- **No structured logging.** `ICakeLog` is format-string text. Layer your own if needed.
- **No interface-mixin model for tasks.** Composition is via context interfaces + concrete task classes + `AddAssembly`.
- **Argument-name aliasing is manual.** No built-in `-c → --configuration`. Chain `context.Argument(...)` calls.
- **`Cake.Git` is x64-only.** Use `StartProcess("git", …)` for git ops.
- **Public-API shipping, dotnet-validate, VitePress have no addins.** Custom work required.
- **Consumer surface grows.** Ayaka's "~25 interface names on the build class" becomes "~15–30 lines of context-class code per consumer." That difference is the price of admission and won't go away.

---

## 10. What this means for the migration plan (preview, not the plan itself)

The next document will be the migration plan proper. Anchor decisions to make first:

1. **Library distribution shape:** ship `Ayaka.Cake` as a single NuGet package on the Grynwald.SharedBuild model. Public surface = `IAyakaContext` + sub-`IHave…` interfaces + `DefaultAyakaContext` with virtual everything + concrete `FrostingTask` classes + `UseAyaka<TContext>(taskFilter?)` extension method.
2. **`TaskNames` convention:** define every task name as a constant in `Ayaka.Cake.TaskNames`; library tasks decorate with `[TaskName(TaskNames.X)]`; consumers reference the constants for any custom dependency wiring.
3. **Versioning:** keep `GitVersion.yml` and `GitVersion.Tool 5.12.0`; switch the alias call from NUKE's attribute injection to `context.GitVersion()` in the context constructor.
4. **GitHub operations:** port `Ayaka.Nuke.GitHub` verbatim using `Octokit 14.0.0`. Skip `Cake.GitHub`.
5. **`dotnet-validate`:** port `DotNetValidateTasks` to a `Tool<DotNetValidate*Settings>` subclass; keep the existing `PackageDownload` mechanism for tool acquisition.
6. **Public-API shipping:** port `ICanShipPublicApis` + `IgnoreCaseWhenPossibleComparer` to a `FrostingTask` 1:1. The release-flow PR automation in `Build.AfterRelease.cs` ports to a separate `AfterReleaseTask` chain using `StartProcess("git", …)`.
7. **Capability detection:** Pattern A (`context as IHaveX`) for everything Ayaka's `ICan…` does with `WhenNotNull(this as IHaveX, …)`.
8. **Self-hosted build:** layout the host build as `build/Common/` + `build/Build/` + `build/Pack/` + `build/Release/` + `build/Docs/` à la GitVersion. Each stage's `Program.cs` is 3–5 lines.
9. **Bootstrap shims:** keep `build.{ps1,sh,cmd}`; replace bodies with `dotnet run --project build/Build/Build.csproj -- "$@"`.

These decisions form the spine of the actual migration plan. The next document will sequence them into concrete commits and call out the risks at each step.
