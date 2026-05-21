# NUKE → Cake Frosting Migration — Options Catalogue

> Pre-decision exploration of how to port `Ayaka.Nuke` to **Cake Frosting** while preserving feature parity, staying idiomatic to Cake, and avoiding deep inheritance or invisible magic.
>
> Companion to [`ayaka-nuke-overview.md`](./ayaka-nuke-overview.md) (the source) and [`cake-frosting-overview.md`](./cake-frosting-overview.md) (the target). This document is **not yet the plan** — it lays out the candidate options at each major decision so we can pick before sequencing the actual port.

---

## 0. Scope & Working Constraints

**Stated, non-negotiable:**

- Distribute as a **reusable NuGet component library** (`Ayaka.Cake` package(s)) consumed by other repos' build hosts.
- Consumers must be able to **customize** — at minimum override settings, ideally swap implementations.
- **No endless complexity or inheritance towers.** Prefer one shallow layer of composition (interface + default class) over generic-template / CRTP / DI-orchestration stacks.
- **Feature parity** with `Ayaka.Nuke`'s current `IHave…` + `IHave…Target` + `ICan…` surface area as catalogued in `ayaka-nuke-overview.md §7`.
- **Align with Cake idioms** where they exist. Prefer built-in aliases (`context.DotNetBuild(...)`) over reimplementations; prefer `dotnet-tools.json` + `LocalToolsModule` over hand-rolled resolution.

**Accepted up front (lossy from NUKE → Cake — covered separately in §14):**

- The "one interface line on the build class → one target + its required context" ergonomic.
- Type-based `.DependsOn<T>()` against an *interface* or *abstract* type.
- Soft ordering (`.Before<T>()` / `.After<T>()` / `.TryDependsOn<T>()`).
- Built-in `[Parameter]` / `[Secret]` / `[Unlisted]` CLI advertisement.
- Auto-generated `build.schema.json` and IDE schema integration.

**Working assumptions:**

- We target Cake `6.x` (currently `6.1.0`, .NET 8/9/10) — already the latest line.
- We can choose to ship one NuGet or several; either is reachable.
- We can keep `GitVersion.yml`, the public-API analyzer policy, `Microsoft.SourceLink.GitHub`, and the existing `build/` MSBuild props verbatim — they are framework-agnostic.
- The migration is a **hard cut** for `Ayaka` itself (self-hosting); for downstream consumers we can publish `Ayaka.Cake` in parallel with the last `Ayaka.Nuke` release and let them migrate at their own pace.

---

## 1. Composition Model

The single most consequential decision. Determines how a consumer "opts in" to a capability and how tasks find the data they need. Maps directly onto the three families in `ayaka-nuke-overview.md §4` (Context / Target-definition / Target-implementation).

The mechanical fact is that **Frosting tasks must be concrete public non-abstract classes** — default interface methods are invisible to its assembly scan. So every option below trades on the same fixed pivot: capability lives on the **context**, and a fixed library task **opens** the context to read it.

### 1A. Context-Capability Interfaces (Pattern A — "SharedBuild style")

**Sketch.** Each `IHave…` from Ayaka.Nuke becomes a sub-interface of `IFrostingContext`. The consumer's `BuildContext` implements only the ones they want. Library tasks accept the broadest context type they can and cast (`context as IHaveGitVersion`) to read capability data.

```csharp
// Library
public interface IAyakaContext : IFrostingContext { }                    // composite root marker
public interface IHaveSolution    : IAyakaContext { FilePath Solution { get; } }
public interface IHaveGitVersion  : IAyakaContext { GitVersion Versioning { get; } }
public interface IHaveCodeCoverage : IAyakaContext { DirectoryPath CoverageDirectory { get; } }
// ... one per Ayaka.Nuke IHave…

public abstract class DefaultAyakaContext : FrostingContext, IAyakaContext, IHaveSolution, IHaveArtifacts /* baseline */
{
    protected DefaultAyakaContext(ICakeContext c) : base(c) { }
    public virtual FilePath Solution { get; } = "./" + ...; // sensible default
    public virtual DirectoryPath ArtifactsDirectory { get; } = "./artifacts";
}

[TaskName(AyakaTasks.DotNetBuild)]
[IsDependentOn(typeof(DotNetRestoreTask))]
public sealed class DotNetBuildTask : FrostingTask<IAyakaContext>
{
    public override void Run(IAyakaContext ctx)
    {
        var solution = ((IHaveSolution)ctx).Solution;                    // required capability
        var version  = (ctx as IHaveGitVersion)?.Versioning;             // opt-in enrichment
        ctx.DotNetBuild(solution.FullPath, settings => { ... });
    }
    public override bool ShouldRun(IAyakaContext ctx) => ctx is IHaveSolution;
}
```

**Consumer:**

```csharp
public sealed class BuildContext(ICakeContext c) : DefaultAyakaContext(c),
    IHaveGitVersion, IHaveCodeCoverage
{
    public override FilePath Solution { get; } = "./Ayaka.sln";
    public GitVersion Versioning { get; } = c.GitVersion();
    public DirectoryPath CoverageDirectory => ArtifactsDirectory.Combine("coverage");
}

new CakeHost().UseAyaka<BuildContext>().Run(args);
```

**Pros.**
- Closest possible mirror of Ayaka's `WhenNotNull(this as IHaveGitVersion, …)` idiom — moves from `this as` to `context as`, otherwise identical shape.
- Consumer adds **one interface line per capability**, just like today (the line moves from the build class to the context class).
- Honest about what's optional vs required (`ShouldRun(ctx is IHaveSolution)`).
- Library tasks stay loose-typed (`FrostingTask<IAyakaContext>`); consumers don't need closed-generic subclasses.

**Cons.**
- Pattern-matching everywhere is mildly verbose vs. NUKE's automatic `IHaveX.X` access.
- `ShouldRun(ctx is IHaveX)` plus `IsDependentOn` means a "missing" capability still puts the task in the dependency graph — it just no-ops. Mostly fine; visible in `--tree`.
- The `IAyakaContext` marker is a small layer of indirection; consumers must inherit `DefaultAyakaContext` *or* implement the marker themselves.

**Fit with constraints.** Strongest match. One layer of inheritance (`DefaultAyakaContext`) plus opt-in capability interfaces — no generics, no DI, no codegen.

### 1B. Single-Context Property Containment (Pattern B)

**Sketch.** One concrete `DefaultAyakaContext` carrying every property. Absence is signalled by `null` / `default`. Consumers subclass and assign.

```csharp
public class DefaultAyakaContext : FrostingContext
{
    public FilePath?      Solution           { get; init; }
    public GitVersion?    Versioning         { get; init; }              // null ⇒ no version stamping
    public DirectoryPath? CoverageDirectory  { get; init; }              // null ⇒ no coverage collection
    public string?        NuGetApiKey        { get; init; }
    ...
}

public sealed class DotNetBuildTask : FrostingTask<DefaultAyakaContext>
{
    public override void Run(DefaultAyakaContext ctx)
    {
        ctx.DotNetBuild(ctx.Solution!.FullPath, s =>
        {
            if (ctx.Versioning is { } v) s.MSBuildSettings.SetVersion(v.SemVer);
        });
    }
    public override bool ShouldRun(DefaultAyakaContext ctx) => ctx.Solution is not null;
}
```

**Pros.**
- Simplest possible model — no interface explosion, single type to subclass.
- Tasks bind to the concrete context with strong typing.
- Easy to grep ("what properties does the context have?").

**Cons.**
- Loses the *type-level* opt-in: every consumer carries the whole property surface even if 90% is unused.
- "Capability" becomes a runtime value-check (`is not null`) rather than a type-check (`is IHaveX`). Slightly less discoverable in IDE.
- Cannot grow third-party capability extensions without modifying the central context class. Goes against open/closed.
- Forces every consumer onto the same context class hierarchy (no marker-only consumers).

**Fit.** Simpler than 1A, but discards the type-system signal that capabilities exist. Acceptable if we conclude the type-level opt-in isn't worth the verbosity.

### 1C. Generic Abstract Task Templates (Pattern D — "CreativeCoders style")

**Sketch.** Library ships abstract `XxxTaskBase<TContext> where TContext : IFrostingContext, IHaveX` templates plus concrete `XxxTask : XxxTaskBase<DefaultAyakaContext>` subclasses bound to the default context. Consumers who want a different context type must declare their own closed-generic subclass.

```csharp
public abstract class DotNetBuildTaskBase<T> : FrostingTask<T>
    where T : FrostingContext, IHaveSolution, IHaveDotNetConfiguration { ... }

[TaskName("DotNet.Build")]
public sealed class DotNetBuildTask : DotNetBuildTaskBase<DefaultAyakaContext> { }

// In consumer that doesn't use DefaultAyakaContext:
[TaskName("DotNet.Build")]
public sealed class DotNetBuildTask : DotNetBuildTaskBase<MyOwnContext> { }    // they must re-declare
```

**Pros.**
- Strongest possible typing inside task bodies — no casts, IDE autocomplete on `context.X`.
- Constraints (`where T : IHaveX`) are enforced at compile time, not runtime.

**Cons.**
- **Triggers exactly the "endless inheritance layers" the brief rules out** — every consumer must declare ~25 closed-generic subclasses just to register the default tasks under a different context type.
- Duplicate `[TaskName]` between library and consumer subclass throws at engine init — the consumer must filter the library's tasks out, ship their own.
- Doesn't actually win type safety vs. 1A in practice: 1A's `context as IHaveX` produces the same compile-time shape inside the task body after a single cast at the top.

**Fit.** Out, per stated constraint. Listed for completeness so it's a deliberate "no", not an oversight.

### 1D. DI-First Capabilities (Pattern C — "Antda.Build style")

**Sketch.** Capabilities are services registered in `ConfigureServices`; tasks declare them as constructor parameters and Frosting resolves via Autofac.

```csharp
host.ConfigureServices(s =>
{
    s.AddSingleton<IGitVersionProvider, ConventionalGitVersionProvider>();
    s.AddSingleton<INuGetCredentials>(_ => NuGetCredentials.FromEnv());
});

public sealed class DotNetBuildTask(IGitVersionProvider version) : FrostingTask<BuildContext>
{
    public override void Run(BuildContext ctx) { var v = version.Get(); ... }
}
```

**Pros.**
- Clean separation of capability and consumer.
- Best testability per capability in isolation.

**Cons.**
- Maximum boilerplate per capability (interface + impl + registration + ctor injection).
- Two parallel routes for data ("context properties" vs. "DI services") — confusing.
- Frosting's DI surface (`ConfigureServices`) is universal but underused in production; almost no public Frosting library does this.
- Inverts the "context is the integration point" mental model that Cake otherwise leans into.

**Fit.** Wrong tool for this job. Worth knowing exists; not a serious contender for Ayaka.

### 1E. Hybrid — 1A baseline with selective concrete-context tasks

**Sketch.** Default to 1A (capability interfaces). For a small set of tasks where strong typing materially helps (test reporting, public-API shipping), narrow the task's `FrostingTask<T>` generic to a more specific interface that bundles the required capabilities.

```csharp
public interface IDotNetTestContext : IAyakaContext, IHaveSolution, IHaveTestArtifacts, IHaveDotNetConfiguration { }

public sealed class DotNetTestTask : FrostingTask<IDotNetTestContext> { ... }
```

**Pros.**
- Tasks that need many capabilities at once become readable without a cast cascade.
- Still no closed-generic subclasses to maintain.

**Cons.**
- Now there are two flavors of task (broad-typed and narrow-typed). Documentation has to explain both.
- A consumer whose context doesn't implement `IDotNetTestContext` (because they skip one of its members) loses the task — the type system *enforces* opt-in instead of the soft `ShouldRun` form.

**Fit.** Marginal. Probably better to keep all tasks at `FrostingTask<IAyakaContext>` for uniformity and accept the casts. Listed in case a few tasks really benefit.

### Composition model — comparison

| Property | 1A (capability ifaces) | 1B (single context) | 1C (generic templates) | 1D (DI) | 1E (hybrid) |
|---|---|---|---|---|---|
| Type-level opt-in per capability | ✓ | ✗ (runtime null) | ✓ | partial | ✓ |
| Inheritance depth for consumer | 1 (`DefaultAyakaContext`) | 1 | 1 + N task subclasses | 0 | 1 |
| Lines of consumer code per capability | 1–2 | 1 (assignment) | 4–6 (subclass per task) | 3 (svc + ctor) | 1–2 |
| Library task type | `FrostingTask<IAyakaContext>` | `FrostingTask<DefaultAyakaContext>` | abstract + concrete | `FrostingTask<TContext>` | mixed |
| Cast inside task body | yes (`ctx as IHaveX`) | no | no | no | mostly no |
| Capability supplied by 3rd party | ✓ | ✗ | ✓ | ✓ | ✓ |
| Aligns with stated constraints | strong | medium | rejected | weak | medium |

**Decision needed.** Pick between 1A, 1B, and (optionally) 1E. The brief's "no inheritance towers" rules out 1C and 1D.

---

## 2. Library Packaging Shape

How many NuGet artifacts ship from this repo, and how does a consumer pull them in?

### 2A. Single `Ayaka.Cake` package

One assembly. Tasks live in nested namespaces (`Ayaka.Cake.DotNet`, `Ayaka.Cake.GitHub`, …). Consumer opts out via `taskFilter`:

```csharp
new CakeHost().UseAyaka<BuildContext>(t => t.Namespace != "Ayaka.Cake.VitePress").Run(args);
```

**Pros.** Simplest to ship/maintain; one `AddAssembly`; matches `Grynwald.SharedBuild`. **Cons.** Consumer pulls every transitive (Octokit, Cake.Npm, etc.) even if they don't use a subsystem.

### 2B. Multi-package per subsystem

Mirror Ayaka.Nuke's current namespace split → `Ayaka.Cake.Core`, `.DotNet`, `.GitHub`, `.VitePress`, `.PublicApi`, `.DotNetValidate`. Each is an assembly; each needs its own `host.UseAyakaXxx<T>()` extension that calls `.AddAssembly(...)`.

**Pros.** Granular deps; transitive footprint matches the features actually used; clean public API per subsystem. **Cons.** N publishable packages, N versioning streams, N CI gates; consumer chains `.UseAyakaCore<T>().UseAyakaDotNet().UseAyakaGitHub()...`. Cake's task-discovery model is per-assembly, so this is supported but adds host setup ceremony.

### 2C. Single package, internal folder grouping (recommended-feel)

Ship one `Ayaka.Cake` assembly but organize source under `src/Ayaka.Cake/DotNet/`, `src/Ayaka.Cake/GitHub/`, etc., mirroring today's repo layout. `taskFilter` (string-based on namespace or `[TaskName]` prefix) handles opt-out.

**Pros.** Single package surface for consumers; preserves the on-disk mental model of `Ayaka.Nuke` 1:1; one CI release pipeline. **Cons.** Transitive Octokit/Cake.Npm always present (~few MB, acceptable for a build host).

**Decision needed.** 2A and 2C are functionally equivalent — they differ only in folder layout. 2B is the only structurally different option and adds real ceremony to consumer hosts.

---

## 3. Task Naming & Dependency Resolution

Frosting's `[IsDependentOn(typeof(X))]` flattens `X` to `X.GetTaskName()` at registration and resolves by string. So:

- `[IsDependentOn(typeof(ConcreteTask))]` works — even across assemblies.
- `[IsDependentOn(typeof(IAbstractOrInterfaceType))]` does **not** — the abstract/interface is never registered as a task name.
- Two tasks claiming the same `[TaskName]` throws on engine init.

This forecloses Ayaka's `.DependsOn<IHaveDotNetBuildTarget>()` and forces a naming convention.

### 3A. `TaskNames` constants class + concrete-type dependencies

```csharp
public static class AyakaTaskNames
{
    public const string Clean         = "Ayaka.Clean";
    public const string DotNetRestore = "Ayaka.DotNet.Restore";
    public const string DotNetBuild   = "Ayaka.DotNet.Build";
    public const string DotNetTest    = "Ayaka.DotNet.Test";
    public const string DotNetPack    = "Ayaka.DotNet.Pack";
    public const string GitHubRelease = "Ayaka.GitHub.Release";
    ...
}

[TaskName(AyakaTaskNames.DotNetBuild)]
[IsDependentOn(typeof(DotNetRestoreTask))]
public sealed class DotNetBuildTask : FrostingTask<IAyakaContext> { ... }
```

Consumers reference Ayaka tasks either by concrete type or by name (for runtime composition tools that take strings).

**Pros.** Explicit, greppable, no runtime indirection; same shape as `Grynwald.SharedBuild`. **Cons.** Library and consumer must use the same constants — a string typo turns into a registration error. Acceptable risk.

### 3B. Programmatic wiring in `UseAyaka<T>()`

Drop `[IsDependentOn]` entirely; configure the Cake engine directly:

```csharp
public static CakeHost UseAyaka<T>(this CakeHost host) where T : class, IAyakaContext =>
    host.UseContext<T>().AddAssembly(typeof(AyakaCakeExtensions).Assembly)
        .ConfigureServices(s => s.AddSingleton<IConfigurator<ICakeContainerRegistrar>>(new AyakaDependencyConfigurator()));
```

The configurator walks the task graph and wires dependencies at runtime, allowing arbitrary logic (e.g. "depend on `Clean` only if consumer included it").

**Pros.** Maximum flexibility; can implement true `TryDependsOn`-like behavior; no constant collisions. **Cons.** Sticks a layer of bespoke runtime wiring into the host that nobody else in the Cake ecosystem does — high maintenance cost; not idiomatic.

### 3C. Hybrid

Use `[IsDependentOn(typeof(...))]` for the static graph, expose `AyakaTaskNames` constants for cross-cutting reference, and reserve programmatic wiring for the one or two genuinely conditional edges (e.g. "validate after pack only if `IHaveDotNetValidateTarget` is opted in").

**Fit.** 3A is the default. 3C is 3A plus a documented escape hatch for the few places that need it.

**Decision needed.** Almost certainly 3A. The question is whether to admit 3C's escape hatch for soft ordering (§10).

---

## 4. Customization Seam

Once a task runs, *how does the consumer override what it does* without rewriting the task?

### 4A. Virtual properties on the default context

```csharp
public class DefaultAyakaContext : FrostingContext
{
    public virtual DotNetMSBuildSettings DotNetMSBuildSettings => new DotNetMSBuildSettings()
        .SetVersion(this is IHaveGitVersion g ? g.Versioning.SemVer : null);
}

public class BuildContext(ICakeContext c) : DefaultAyakaContext(c)
{
    public override DotNetMSBuildSettings DotNetMSBuildSettings => base.DotNetMSBuildSettings
        .SetProperty("ContinuousIntegrationBuild", "true");
}
```

**Pros.** Familiar OOP; ergonomic for simple value overrides. **Cons.** Doesn't compose well — "modify the result of base" requires the consumer to remember `base.X`; easy to forget and silently drop baseline.

### 4B. Configure-delegate properties on the context (closest mirror of NUKE)

Direct port of Ayaka's "sealed Base + virtual Settings" pattern from `ayaka-nuke-overview.md §5`:

```csharp
public class DefaultAyakaContext : FrostingContext
{
    public sealed Action<DotNetBuildSettings> DotNetBuildSettingsBase => s =>
    {
        s.Configuration = (this as IHaveDotNetConfiguration)?.DotNetConfiguration ?? "Release";
        if (this is IHaveGitVersion g) s.MSBuildSettings.SetVersion(g.Versioning.SemVer);
    };

    public virtual Action<DotNetBuildSettings> DotNetBuildSettings => _ => { };
}

// In DotNetBuildTask.Run:
ctx.DotNetBuild(solution, settings =>
{
    ctx.DotNetBuildSettingsBase(settings);
    ctx.DotNetBuildSettings(settings);
});
```

`sealed` on Base enforces that the baseline can't be lost; the public delegate defaults to identity; consumers override only the latter.

**Pros.** Exact behavioural mirror of NUKE; survives the migration with minimal mental retraining; baseline is non-bypassable. **Cons.** `sealed` on instance properties is a less-common C# construct (works, but raises eyebrows); requires every task to call both delegates in order.

### 4C. `IConfigureSettings<T>` services via DI

```csharp
host.ConfigureServices(s => s.AddSingleton<IConfigureSettings<DotNetBuildSettings>, MyExtraBuildConfig>());

// In task body:
foreach (var c in ctx.Services.GetServices<IConfigureSettings<DotNetBuildSettings>>())
    c.Configure(settings);
```

**Pros.** Clean separation, allows multiple stacked configurers, fits if the consumer's settings depend on per-environment DI services. **Cons.** Twice the boilerplate vs 4B; relies on running tasks pulling from `IServiceProvider` which Cake doesn't surface as a first-class API on tasks.

### 4D. Override-by-replacement

Library ships a default `DotNetBuildTask`. Consumer needing custom behaviour:
1. excludes it via `taskFilter` in `UseAyaka<T>(filter: t => t != typeof(DotNetBuildTask))`,
2. ships their own task with the same `[TaskName(AyakaTaskNames.DotNetBuild)]`.

**Pros.** Total control when the consumer needs it. **Cons.** All-or-nothing; can't override just one setting; no baseline reuse. Reasonable as an *escape hatch* for the rare "rewrite the whole task" case.

### 4E. Mix (recommended baseline)

- **4B** as the primary customization seam for settings (mirrors NUKE precisely).
- **4A** for plain values (paths, names) that don't carry chained settings.
- **4D** documented as the escape hatch for total replacement.

**Decision needed.** Confirm 4B is the right default for the "modify tool settings" seam. 4A and 4D are then add-ons that don't conflict.

---

## 5. Settings Layering — Per-thing Loops

A subset of `ICan…` (`ICanDotNetTest`, `ICanDotNetPush`, `ICanDotNetValidate`) iterate over a collection (per test project, per nupkg, per package to validate) and offer **two pairs** of settings: a global pair (Base + Override) and a per-thing pair (`…ProjectSettingsBase` + `…ProjectSettings`).

### 5A. Mirror the four-delegate shape

```csharp
public class DefaultAyakaContext
{
    public sealed Action<DotNetTestSettings> DotNetTestSettingsBase            => ...;
    public virtual Action<DotNetTestSettings> DotNetTestSettings               => _ => { };
    public sealed Action<DotNetTestSettings, FilePath> DotNetTestProjectSettingsBase => ...;
    public virtual Action<DotNetTestSettings, FilePath> DotNetTestProjectSettings   => (_, _) => { };
}

// In task body, per project:
ctx.DotNetTestSettingsBase(s);
ctx.DotNetTestSettings(s);
ctx.DotNetTestProjectSettingsBase(s, project);
ctx.DotNetTestProjectSettings(s, project);
```

**Pros.** Behaviour-preserving 1:1 with NUKE. **Cons.** Four delegates per multi-target tool. Verbose.

### 5B. Collapse to a single pair (Base + Override) plus a per-thing closure inside the task

The per-project specialization moves into the task body's foreach loop where the project is in scope:

```csharp
foreach (var project in ctx.TestProjects)
{
    ctx.DotNetTest(project, s =>
    {
        ctx.DotNetTestSettingsBase(s);
        ctx.DotNetTestSettings(s);
        // per-project tweaks here in the task (not customizable per project from outside)
    });
}
```

**Pros.** Half as many delegates. **Cons.** Per-project consumer customization disappears — that's lost as a customization seam.

### 5C. Single configure method receiving both settings *and* the per-thing token

```csharp
public virtual void ConfigureDotNetTest(DotNetTestSettings s, FilePath project) { ... }
```

**Pros.** One override point covers both global and per-project tweaks. **Cons.** Loses the explicit separation between "always apply" baseline (Base) and "consumer's hook" (Settings) — they fuse, and the consumer must remember to call `base.ConfigureDotNetTest` to keep the baseline.

**Decision needed.** Whether per-thing customization is a feature consumers use enough to keep (`5A`) or whether we collapse it (`5B`/`5C`). A grep on `Build.cs` in `Ayaka` and any known downstream consumers should answer this empirically.

---

## 6. Parameter & Secret Handling

NUKE's `[Parameter("description")] AbsolutePath X { get; }` and `[Parameter, Secret] string? ApiKey { get; }` give automatic CLI surface, env-var fallback, masking, and `--help` listing. Cake provides **none** of this directly.

### 6A. Manual reading in `BuildContext` constructor

```csharp
public BuildContext(ICakeContext c) : base(c)
{
    Solution    = c.Argument<FilePath>("solution", "./Ayaka.sln");
    NuGetSource = c.Argument("nuget-source", "https://api.nuget.org/v3/index.json");
    NuGetApiKey = c.EnvironmentVariable("NUGET_API_KEY")
                 ?? c.Argument<string?>("nuget-api-key", null);
}
```

**Pros.** Zero infrastructure. Greppable. Idiomatic in every reference project surveyed (GitVersion, BenchmarkDotNet, octokit.net). **Cons.** Re-implemented per capability. No `--help` listing. No automatic masking — secret leakage relies on convention.

### 6B. Custom `[Parameter]` / `[Secret]` attributes + reflection in `BuildLifetime.Setup`

Ship `Ayaka.Cake.ParameterAttribute` + a `FrostingLifetime<TContext>` that reflects on the context type, populates `[Parameter]` properties from `context.Argument(...)`, `[Secret]` from env-var-first, and prints them in a `Help` task.

```csharp
[Parameter("Path to the solution file")]                public FilePath Solution { get; set; }
[Parameter("NuGet feed URL"), Default("https://...")]   public string   NuGetSource { get; set; }
[Parameter("NuGet API key"), Secret]                    public string?  NuGetApiKey { get; set; }
```

**Pros.** Restores Ayaka's discoverability and the "advertised CLI surface" feel; `Help` task can dump everything. **Cons.** Reintroduces reflection magic the rest of Cake doesn't have — a maintenance burden Ayaka would own alone. Properties must have setters (no read-only init).

### 6C. Source generator

Same surface as 6B but emit the `Setup` body at compile time, supporting `init`-only properties.

**Pros.** Best ergonomics. **Cons.** Source generator tooling = larger maintenance commitment; another moving piece in the package; harder to debug.

### 6D. Helper extension class (`AyakaArgs`)

```csharp
public static class AyakaArgs
{
    public static T Get<T>(ICakeContext c, string arg, string env, T defaultValue) =>
        c.HasArgument(arg) ? c.Argument<T>(arg)
        : Environment.GetEnvironmentVariable(env) is { Length: > 0 } v ? Convert.ChangeType(v, typeof(T))!
        : defaultValue;
}

// In ctor:
NuGetSource = AyakaArgs.Get(c, "nuget-source", "NUGET_SOURCE", "https://api.nuget.org/v3/index.json");
NuGetApiKey = AyakaArgs.GetSecret(c, "nuget-api-key", "NUGET_API_KEY");   // masks in GHA if present
```

**Pros.** Removes the per-line boilerplate of 6A; centralizes masking + fallback; no attributes, no reflection. **Cons.** Still no `--help` discovery. Naming each arg twice (key + env-var) is mild repetition.

**Decision needed.** Almost certainly 6A or 6D for the default; whether to invest in 6B/6C is a future-discoverability decision that can be deferred. The brief leans against 6B/6C ("no magic / hidden complexity").

---

## 7. Tool Wrapping Approach

Ayaka.Nuke wraps `dotnet`, `dotnet-validate`, `npm`, GitHub APIs, and git. Cake has built-ins for `dotnet` and `npm`; `dotnet-validate` and GitHub need custom work; git can be `StartProcess`.

### 7A. Built-ins-first

| Tool | Approach |
|---|---|
| `dotnet` (restore/build/test/pack/nuget push) | `context.DotNet*` aliases from `Cake.Common.Tools.DotNet` |
| `npm` (VitePress) | `Cake.Npm` package, `NpmCi` + `NpmRunScript` aliases |
| Coverage (XPlat collector) | Built into `DotNetTestSettings.Collectors` |
| `dotnet-validate` | Custom `Tool<DotNetValidateLocalPackageSettings>` subclass under `Cake.Core.Tooling` |
| GitHub releases / PRs / notes | `Octokit 14.0.0` directly in `AsyncFrostingTask` (port `GitHubTasks.cs` verbatim) |
| Git ops (after-release branch/commit/push) | `context.StartProcess("git", new ProcessSettings { Arguments = ... })` |
| ReportGenerator | `context.ReportGenerator(...)` (already a Cake built-in) |
| `XmlPeek` for TRX parsing | `context.XmlPeek(...)` |

**Pros.** Maximum idiomatic Cake. Aliases are well-tested and version-aligned with Cake itself. **Cons.** Loses Ayaka's typed `Set…/Reset…` builders on `DotNetBuildSettings` etc. — Cake's settings are mutable POCOs, so we use `s.Property = ...` directly.

### 7B. Re-implement Ayaka's typed builders on top of Cake settings

Ship `Ayaka.Cake.DotNet` extension methods that mirror `Ayaka.Nuke.DotNet`'s fluent style:

```csharp
new DotNetBuildSettings()
    .SetConfiguration("Release")
    .SetNoRestore(true)
    .WhenNotNull(ctx as IHaveGitVersion, (s, g) => s.SetMSBuildVersion(g.Versioning.SemVer));
```

**Pros.** Smoother carryover for any code (consumer or test) that uses the fluent style today. **Cons.** Reinvents shape Cake already provides; consumers learning Cake see two parallel surfaces and have to choose.

**Decision needed.** 7A is the default. Worth porting `When` / `WhenNotNull` (already two lines) but skipping per-property fluent builders for Cake-native settings.

---

## 8. External Tool Resolution

How does the build acquire `gitversion`, `dotnet-validate`, `reportgenerator` at runtime?

### 8A. `.config/dotnet-tools.json` + `Cake.DotNetLocalTools.Module`

```csharp
new CakeHost()
    .UseModule<LocalToolsModule>()
    .InstallToolsFromManifest(".config/dotnet-tools.json")
    .UseAyaka<BuildContext>()
    .Run(args);
```

Drop `nuke.globaltool`; the manifest carries `gitversion.tool`, `dotnet-validate`, `dotnet-reportgenerator-globaltool`.

**Pros.** Modern Cake idiom; consumers already familiar with `dotnet tool restore`; works on ARM; one source of truth (the manifest). **Cons.** None significant.

### 8B. `CakeHost.InstallTool` URIs per tool

```csharp
host.InstallTool(new Uri("dotnet:?package=GitVersion.Tool&version=5.12.0"))
    .InstallTool(new Uri("dotnet:?package=dotnet-validate&version=0.0.1-preview.304"));
```

**Pros.** Explicit per tool; no module dependency. **Cons.** Duplicated versioning vs the manifest; consumers calling `UseAyaka<T>()` would need each `InstallTool` to be hidden inside that extension, which then forces the version to be hardcoded in our library — versions you can't override per-consumer. Bad trade.

### 8C. Keep `PackageDownload` on the build csproj

Current Ayaka.Nuke approach. Works but doesn't integrate with Cake's tool resolver; we'd resolve paths manually.

**Pros.** Familiar. **Cons.** Doesn't match Cake conventions; gives no UX benefit over 8A.

**Decision needed.** 8A is the clear default. The only call is whether `UseAyaka<T>()` should *also* call `InstallToolsFromManifest` (convenience) or leave it to the consumer (explicit).

---

## 9. Bootstrap & Host-Project Layout

### 9A. Minimal shims + single project

```
build/
├── Build.csproj
├── Program.cs
├── BuildContext.cs
├── Tasks/
│   ├── Compile.cs
│   ├── Test.cs
│   ├── Pack.cs
│   └── ...
└── Lifetime.cs
build.{cmd,ps1,sh}    # one-liners: dotnet run --project build/Build.csproj -- "$@"
```

**Pros.** Smallest footprint. Single CI build target. Mirrors the current `nukebuild/` shape. **Cons.** No SDK auto-install — fresh clones rely on `global.json` + a pre-installed .NET SDK.

### 9B. Keep NUKE-style SDK auto-installer in the shims

Port the `build.sh`/`build.ps1` SDK-install logic so a clean machine still works without manual SDK setup.

**Pros.** Best DX for fresh clones / CI runners without preconfigured .NET. **Cons.** Shims become non-trivial again; another piece of infrastructure to maintain.

### 9C. Multi-project layout (GitVersion-style)

```
build/
├── Common/    Common.csproj     # shared library (BuildContext, Lifetime, helpers)
├── Build/     Build.csproj      # phase: build/test
├── Pack/      Pack.csproj
├── Release/   Release.csproj
└── Docs/      Docs.csproj
```

Each phase has its own `Program.cs` (3–5 lines) and `Tasks/`. The build invocation chooses the project.

**Pros.** Strong phase isolation; each phase can declare its own minimal task set. Used successfully by GitVersion and DacTools. **Cons.** Five `.csproj` files to maintain; consumers asking "where is X done?" face more navigation. Fights the "no endless complexity" constraint a little.

### 9D. Single project, organized internally (recommended-feel)

9A's project shape, but with explicit `Tasks/Core/`, `Tasks/DotNet/`, `Tasks/GitHub/`, etc. folders inside `build/` matching the library's namespace split.

**Pros.** Single project / single CI gate; readable structure. **Cons.** None over 9A.

**Decision needed.** Choose between 9A/9D vs 9C. The shim question (9A vs 9B) is independent and likely "keep them minimal".

---

## 10. Optional / Soft-Ordered Dependencies

Ayaka uses three semantics that don't map directly:

| Ayaka.Nuke usage | Where | What it does |
|---|---|---|
| `.TryDependsOn<IHaveCleanTarget>()` | `ICanDotNetRestore` | depend on Clean *if* the consumer included it |
| `.TryBefore<IHaveVitePressBuildTarget>()` | `ICanVitePressLint` | run before Build *if* present, no trigger |
| `.After<IHaveDotNetTestTarget>()` | `ICanDotNetPack` | run after Test *if* it ran, no trigger |

### 10A. Hard deps + `ShouldRun` gates

Always declare `[IsDependentOn(typeof(CleanTask))]`; `CleanTask.ShouldRun` returns `false` if the consumer hasn't opted in.

**Pros.** Static, predictable graph. Visible in `--tree`. **Cons.** Tasks the consumer doesn't want still appear in the graph (no-op). For `Before`/`After` soft ordering, hard-depping flips the trigger semantics — `Pack` would *always* trigger `Test`, which is wrong.

### 10B. Programmatic conditional wiring at host setup

`UseAyaka<T>()` inspects the closed `TContext` type, sees which `IHaveX` it implements, and registers `IsDependentOn` edges via Cake engine APIs at startup.

**Pros.** Behaviourally identical to `TryDependsOn`. **Cons.** Bespoke runtime wiring layer; brittle as the Cake engine evolves.

### 10C. `taskFilter` as opt-out

Default registration includes the optional dep; consumer who wants to drop it filters the task out. Inverts the polarity (opt-out instead of opt-in).

**Pros.** No infrastructure. **Cons.** Doesn't help soft ordering; only helps optional deps.

### 10D. Live with the loss for soft ordering

For `.Before` / `.After` specifically: re-express as hard deps where they're truly required (`PackTask` should depend on `TestTask` in the orchestration), and drop the "soft" cases where they aren't (`LintTask` can sit as a sibling, the consumer orders manually).

A literal grep across `Ayaka.Nuke` shows only three uses (clean→restore, lint→build, pack→test). All three can be re-expressed:

- `restore TryDependsOn clean` → 10C: clean is in the graph by default, consumer filters out if they don't want it.
- `lint TryBefore build` → 10D: ship a parent orchestration task `Verify` that depends on `[Lint, Build]`. Consumer who wants both wires the order via the orchestration node, not via soft edges.
- `pack After test` → 10A: hard-dep `pack → test`. Trade-off accepted: running `pack` always also runs `test`. The current "soft" version was already a defensive measure for downstream consumers; making it hard is arguably correct.

**Decision needed.** 10D is the lowest-overhead path. 10B is available if a real consumer pain point emerges later.

---

## 11. Component-Level Migration Sketches

Per-namespace notes assuming we choose **1A (capability interfaces) + 2C (single package) + 3A (TaskNames constants) + 4B (Configure delegates) + 7A (Cake built-ins) + 8A (LocalToolsModule)**. Each is a paragraph, not a final design.

### 11.1 General / `Ayaka.Nuke` root → `Ayaka.Cake`

- `IHave`, `ICan` markers drop entirely (no DIM model).
- All `IHave…` context interfaces port 1:1 to interfaces deriving `IAyakaContext`.
- `IHaveCleanTarget` etc. (the "definition" tier) disappear; their job is now done by the `[TaskName]` constants.
- `ICanClean` becomes `CleanTask : FrostingTask<IAyakaContext>` with `Run` that calls `context.CleanDirectories("**/{bin,obj}")` + `context.EnsureDirectoryExists(artifactsDir)`. Each branch (clean `obj` etc.) is gated by `ctx is IHaveSources` / `IHaveTests` / `IHaveArtifacts` mirroring the current `if (this is IHaveX)` blocks.
- `Extensions.When` / `WhenNotNull` stay verbatim — they're pure helpers, no NUKE dependency.

### 11.2 `Ayaka.Nuke.DotNet` → `Ayaka.Cake.DotNet`

- `Configuration : Enumeration` becomes a plain `static class Configurations { public const string Debug = "Debug"; public const string Release = "Release"; }` or a simple enum + extension; `Enumeration` was NUKE-specific.
- `IHaveDotNetConfiguration` → carries a `string` (or enum) `DotNetConfiguration` property; default `Debug` locally / `Release` on CI computed in `DefaultAyakaContext`.
- `DotNetRestoreTask` / `DotNetBuildTask` / `DotNetTestTask` / `DotNetPackTask` / `DotNetPushTask` all become concrete `FrostingTask<IAyakaContext>` classes.
- The version-stamping behaviour (`SetAssemblyVersion` / `SetFileVersion` / `SetInformationalVersion`) ports into `DotNetMSBuildSettings` set up in `DotNetBuildSettingsBase`.
- The per-project loop in `DotNetTestTask` becomes a `foreach (var project in ctx.TestProjects)` with per-project settings shaped by §5's chosen layering.
- `CopyCoverageFiles` (the GUID-named-parent-dir de-duplication) ports verbatim as a static helper.
- `ReportTestCount` becomes a small post-step inside `DotNetTestTask.Run` that uses `context.XmlPeek` over `*.trx`.

### 11.3 `Ayaka.Nuke.DotNetValidate` → `Ayaka.Cake.DotNetValidate`

- The `[NuGetTool]`/`ToolTasks` machinery is NUKE-specific. Port to a custom `Tool<DotNetValidate*Settings>` subclass under `Cake.Core.Tooling`, registering via a `[CakeAliasCategory]`-decorated extension on `ICakeContext`.
- The `[Argument(Format="…", Position=…)]` attribute pattern → `ProcessArgumentBuilder` calls inside `EvaluateToolArguments`.
- `IHaveDotNetValidateTarget` → just a `[TaskName(AyakaTaskNames.DotNetValidate)]` constant + a `DotNetValidateTask`.
- The CI-only gate (`OnlyWhenStatic(() => IsServerBuild)`) → `ShouldRun(IAyakaContext ctx) => ctx.BuildSystem().IsRunningOnGitHubActions` or equivalent helper.
- `dotnet-validate` is acquired via `.config/dotnet-tools.json` (replacing `PackageDownload`).

### 11.4 `Ayaka.Nuke.NuGet` → `Ayaka.Cake.NuGet`

- Pure configuration, no targets — same shape after port.
- `IHaveNuGetConfiguration { string NuGetSource; string? NuGetApiKey; }` — values read in `BuildContext` ctor via 6A or 6D.
- Default source constant unchanged.

### 11.5 `Ayaka.Nuke.GitHub` → `Ayaka.Cake.GitHub`

- **Port `GitHubTasks.cs` verbatim** — it's already Octokit-backed and Cake-agnostic. Octokit 14.0.0 is the current latest.
- `GitHubSettings` / `GitHubReleaseSettings` / `GitHubReleaseNotesSettings` / `GitHubPullRequestSettings` and their `*Extensions` builders move over unchanged (POCO surface, no NUKE deps in the classes).
- `IHaveGitHubReleaseTarget` → `GitHubReleaseTask : AsyncFrostingTask<IAyakaContext>` reading `IHaveGitVersion` + `IHaveGitRepository` + `IHaveGitHubToken` from the context.
- The `Requires(() => GitHubToken)` semantic ports as `ShouldRun(ctx) => !string.IsNullOrEmpty(ctx.GitHubToken)` plus a startup assertion in lifetime.

### 11.6 `Ayaka.Nuke.VitePress` → `Ayaka.Cake.VitePress`

- Three tasks wrapping `Cake.Npm`:
  - `VitePressInstallTask` → `context.NpmCi(s => s.FromPath(ctx.DocsDirectory))` — note that `Cake.Npm 5.1.0` exposes `WorkingDirectory` via the inherited `ToolSettings`, not a `FromPath` setting; check the actual API.
  - `VitePressLintTask` → `context.NpmRunScript("lint", s => s.WithLogLevel(NpmLogLevel.Error))` or with `WorkingDirectory` set.
  - `VitePressBuildTask` → `context.NpmRunScript("build", ...)` with `--outDir` argument.
- Consumer override for the lint npm-script name (currently `"lint:js"` in Ayaka's `Build.cs`) → 4B Configure-delegate on the context.

### 11.7 `Ayaka.Nuke.PublicApi` → `Ayaka.Cake.PublicApi`

- `ShipPublicApisTask : FrostingTask<IAyakaContext>` — body ports from `ICanShipPublicApis` verbatim using `System.IO`.
- `IgnoreCaseWhenPossibleComparer` keeps its internal sealed shape.
- No external dependency change.

### 11.8 After-Release flow (`Build.AfterRelease.cs`)

Today this lives in `nukebuild/`, not the library. Two ports possible:

- **Keep in self-hosting build only** — the after-release dance is repo-specific orchestration; ship the *primitives* (`CreatePullRequestTask` + `ShipPublicApisTask`) in the library, let `Ayaka` itself wire its own `AfterRelease` chain in `build/`. Aligned with §15.1 of the cake-frosting overview (`octokit/octokit.net` keeps the rough equivalent in-repo).
- **Ship the chain as a library task graph** — `Ayaka.Cake.AfterReleaseTask` orchestrates `CreateBranch → ShipPublicApis → CommitPush → CreatePR`. Consumers get the whole thing by depending on one task.

The first is simpler and avoids speculative reuse. Pick that unless we know of a downstream consumer that wants the chain.

---

## 12. Per-Test-Project Capability Detection

Ayaka.Nuke inspects each test project's `<PackageReference>` set to decide whether to enable `GitHubActionsTestLogger`, `coverlet.collector` etc. — NUKE's project model surfaces this.

### 12A. Project-file XML scan via `context.XmlPeek`

In `DotNetTestTask.Run`, for each test project, peek `Project/ItemGroup/PackageReference/@Include` and switch loggers/collectors accordingly.

**Pros.** Behaviour preserved. **Cons.** Reading `.csproj` XML directly skips transitive dependencies and `Directory.Packages.props` central package management. Works for direct refs only; matches what Ayaka does today.

### 12B. Settings-based opt-in on the context

Replace per-project detection with a global `IHaveCodeCoverage` / `IHaveGitHubActionsTestLogger` capability declared on the consumer's context. Tasks check `ctx is IHaveCodeCoverage`, applies XPlat collector uniformly to all test projects.

**Pros.** No XML scanning; clean type-level model. **Cons.** Loses Ayaka's per-project conditional behaviour — if some test projects have coverlet and some don't, the simple model collects-or-doesn't uniformly.

### 12C. Hybrid: opt-in at the context plus per-project autodetect

Apply the collector/logger when the context opts in *and* the project references the relevant package — `context as IHaveCodeCoverage && ProjectHasPackage(project, "coverlet.collector")`.

**Pros.** Preserves current Ayaka behaviour with a cleaner enable signal. **Cons.** Two checks; still does the XML scan.

**Decision needed.** 12A is closest to current; 12B is the cleanest break; 12C is the faithful middle. Resolved by examining how much per-project variation the Ayaka test suite actually exhibits today.

---

## 13. Schema / Help / Discoverability

No Cake equivalent of NUKE's auto-generated `build.schema.json` or rich `--help` listing custom parameters.

**13A.** Document parameters in `README.md` / `CONTRIBUTING.md`. Built-in `--description` lists tasks. (Recommended baseline.)
**13B.** Ship a `HelpTask` that prints every `[Parameter]`-decorated property using the 6B reflection (only viable if we choose 6B).
**13C.** Hand-write a JSON schema for editor integration. Nice-to-have; not load-bearing.

**Decision needed.** Almost certainly 13A. Revisit if we adopt 6B.

---

## 14. Hard Incompatibilities — with proposed mitigations

For each NUKE feature with no clean Cake equivalent, the mitigation that's closest in behaviour.

| # | NUKE feature | Cake reality | Mitigation |
|---|---|---|---|
| 1 | `.DependsOn<IHaveDotNetBuildTarget>()` (depend on an interface) | Resolves by task name string; can only point at concrete types | `AyakaTaskNames` constants + concrete `[IsDependentOn(typeof(ConcreteTask))]`; consumer overrides by `taskFilter` + replacement task with the same `[TaskName]` |
| 2 | DIM-attached target bodies | Cake walks `Assembly.GetExportedTypes()` for `IFrostingTask` *classes* | Tasks are concrete classes; capability lives on the **context** (1A) |
| 3 | `.Before<T>()` / `.After<T>()` soft ordering | No soft ordering primitive | Re-express as hard deps (§10D) or programmatic wiring (§10B); accept one or two extra triggers as the cost |
| 4 | `.TryDependsOn<T>()` | No conditional `IsDependentOn` | Default registration includes the optional dep; consumer filters out via `taskFilter` (§10C); or `ShouldRun = false` for no-op (§10A) |
| 5 | `[Parameter]` auto-help | No advertised CLI surface | 6A/6D manual reading + README docs (§13A); optional 6B for `Help` task |
| 6 | `[Secret]` auto-masking | No built-in masking | Env-var first; explicit GHA mask call (`context.GitHubActions().Commands.AddMask(...)`) in `BuildLifetime.Setup` |
| 7 | `[Unlisted]` | All public tasks visible to `--description`/`--tree` | Make truly internal helpers `internal` (skips discovery); accept that user-facing utility tasks are visible |
| 8 | `OnlyWhenStatic` vs `OnlyWhenDynamic` distinction | Only `ShouldRun` | One form covers both — no functional loss, just less expressive metadata |
| 9 | Auto `build.schema.json` | None | README docs; optional hand-written schema (§13C) |
| 10 | `[NuGetTool]` / `IRequireNuGetPackage` machinery | Different model | `Cake.DotNetLocalTools.Module` + `.config/dotnet-tools.json` (§8A) |

**None of these is an absolute blocker.** Each has a workaround that costs ergonomics, not correctness. Items #1, #3, and #5 are the most user-visible regressions; the rest are cosmetic or covered by trivial substitutions.

**Highlighted for discussion (none truly absolute, but worth a deliberate choice):**

- **Soft ordering (#3, #4).** Decide whether we accept "PR builds always run Test" as the cost, or invest in programmatic wiring (10B) to preserve fidelity.
- **Parameter discoverability (#5).** Decide whether `--help` listing custom parameters is worth the reflection layer (6B) or whether README documentation is fine.

---

## 14.5 Implementation Protocol — absolute rules

The migration is collaborative and gated. These rules govern every action in §15 and override any default agent behaviour. They are absolute.

1. **Dedicated branch.** All migration work happens on a new branch (`cake`, created off `main`). No work lands directly on `main`. The branch is created as the very first action — before any file changes, before Phase 1's content.
2. **Commit granularity.** Each phase produces at least one commit. Multiple commits per phase are allowed and encouraged when the work splits into logically distinct chunks. Keep commits reasonable in size — small enough that the diff is reviewable, large enough that each commit represents a meaningful unit.
3. **Every commit must compile.** No "half-implemented" intermediate commits. The repo must build successfully at every point in history on the `cake` branch. If a logical chunk can't be completed without breaking the build, it isn't yet a commit-sized chunk.
4. **Logical chunking, not file-count chunking.** Commits group changes by *concern*. Example: "rename `build/` → `eng/`" is one commit; "scaffold `Ayaka.Cake` project" is another. Unrelated changes never share a commit.
5. **Commit messages.** Meaningful subject line + body. Follow Conventional Commits **without the scope** — `docs: …`, `chore: …`, `feat: …`, `refactor: …`, never `docs(migration): …`. Subject: one-line imperative describing the change (`docs: capture NUKE → Cake migration plan`, not `Update files`). Body: why the change is being made and what's covered. Reference the phase number from this document.
6. **Pre-commit review gate.** For *every* commit, in order:
    1. Stage the intended changes (`git add` only the files belonging to this commit — never `-A`).
    2. Show the staged diff (`git diff --cached`) or a clear summary of what's in the index, plus the proposed commit message.
    3. Explicitly ask for go.
    4. **Wait for approval before running `git commit`.** No exceptions.
7. **Plan updates after each commit or at sensible intervals.** Update this document to reflect what was actually done vs what the plan said. Capture: scope confirmed, any deviations, anything surprising. The plan tracks reality, not aspiration.
8. **Phase boundary = hard stop.** End of every phase requires:
    - Plan update reflecting the phase's actual outcome.
    - Explicit approval from the user before any Phase N+1 work begins.
    - No spillover. If something belongs in Phase N+1, it waits.
9. **No autonomous initiative.** Outside the steps above, no new migration work starts without an explicit "go". Exiting plan mode, finishing a previous turn, or being idle does not authorize the next action.
10. **Reversibility first.** Where a step is destructive (e.g. dropping `nukebuild/`), confirm separately before executing — even if it's the literal next step in the phase. The plan's existence in a doc isn't standing approval for any individual destructive action.
11. **CI signal via draft PR + local builds.** Ayaka's `ci.yml` triggers only on push-to-`main`, tags `v*`, and PRs to `main` — so `cake` produces no CI signal by itself.
    - **Inner loop (during a phase):** local `dotnet build` / `dotnet test` runs validate each commit before staging. Fast feedback; no remote signal needed for routine work.
    - **Outer loop (phase boundaries):** a single long-lived draft PR `cake → main` provides the remote CI signal. Opened via `gh pr create --draft` immediately after Phase 1's first commit (planning artefacts) is pushed; stays draft until the migration is complete. Every push to `cake` triggers CI via the PR.
    - **Phase boundary gate:** "green CI" at a phase boundary means the draft PR's required checks are green on the phase's last commit. That is the prerequisite for requesting approval to start the next phase.
    - The draft PR's description tracks the migration's overall progress and references this plan; commit-level review still goes through rule 6.
12. **No surprises — stop and consult.** Whenever something turns up that isn't anticipated by this plan and isn't trivially solvable, pause and surface it before proceeding. The user dislikes surprises and would rather decide than be informed after the fact.
    - **Qualifies as "stop and consult":** unexpected files or repo state; an API or tool that doesn't behave as the overview docs describe; an architectural ambiguity that §16 doesn't cover; a deviation from the planned phase scope; a transitive dependency conflict; anything that would mean a non-trivial deviation from the plan; anything I would normally "just figure out" autonomously.
    - **Does not qualify (proceed normally):** typos / formatting fixes in code I'm writing; mechanical resolution of a name clash with an obvious correct answer; routine compiler errors from a missing using/import.
    - When in doubt, stop. The cost of pausing is low; the cost of a surprise the user has to undo is high.
13. **Version alignment when adding packages.** The repo does not use Central Package Management; instead, Renovate keeps versions in sync across `*.csproj` and `eng/Packages.props`. Whenever a new `PackageReference` is added (or a version is touched):
    - First grep the repo for existing usages of that package (`grep -rn 'Include="PackageName"' --include='*.csproj' --include='*.props'`). If found elsewhere, the new declaration must match the existing version exactly.
    - If the package is net-new to the repo, the first introduction sets the version. Prefer the latest stable on NuGet at the time of introduction, and call it out in the commit message so Renovate's future bumps are anchored.
    - Test-only packages added to a test csproj that already get inherited via `eng/Packages.props` (with `Condition="'$(IsTestProject)' == 'True'"`) should not be duplicated in the csproj — let inheritance do the work.

---

## 15. Migration Phases

The migration runs as a checkpoint-driven plan, not a linear port. Each phase ends with the draft PR's CI checks green on the phase's last commit (per §14.5 rule 11); local builds (`dotnet build` / `dotnet test`) are the inner-loop signal during a phase. Until Phase 8, `nukebuild/` remains authoritative for Ayaka's own releases; the new `build/` runs in parallel as a dogfood target.

All phases are subject to §14.5. In particular: every commit goes through the review gate, every phase boundary is a hard stop awaiting approval.

### Phase 1 — Bootstrap

Stand up the new package's infrastructure so subsequent phases have somewhere to land.

- **Create branch `cake`** off `main`. No file changes yet — this is the workspace for the entire migration.
- **First commit: capture the planning artefacts.** Commit the three planning documents under `tasks/plans/` (`ayaka-nuke-overview.md`, `cake-frosting-overview.md`, `nuke-to-cake.md`) as the very first commit on the branch. They are currently untracked; landing them immediately makes the migration plan durable and history-visible before any code changes start.
- **Push the branch and open a draft PR `cake → main`** via `gh pr create --draft` once the first commit is pushed. This is the long-lived PR that provides CI signal for the entire migration (per §14.5 rule 11). PR title and body summarise the migration scope and link this plan; the body will be updated at each phase boundary to reflect progress.
- **Rename `build/` → `eng/`** as the next commit. `git mv build eng`, then update:
    - 15 MSBuild import lines across `src/Directory.Build.props` (7), `test/Directory.Build.props` (6), and `nukebuild/Directory.Build.props` (2 — still load-bearing until Phase 8).
    - `Ayaka.sln` — rename the solution folder labelled "build" to "eng" and update the ~9 `build\*.props` / `build\assets\logo.png` solution-item paths to `eng\…`.
    - `eng/Product.props`'s logo reference is `$(MSBuildThisFileDirectory)`-relative and follows automatically.
    - Done before any new code lands so subsequent phases have a free `build/` for the Cake host. See §16.8 for rationale.
- Scaffold `src/Ayaka.Cake/Ayaka.Cake.csproj` — class library, `<TargetFramework>$(DotNetTargetFramework)</TargetFramework>` (i.e. `net9.0` today, follows `eng/TargetFramework.props`), `Cake.Frosting 6.1.0`, `Cake.DotNetLocalTools.Module` (latest stable — verify version on first introduction, see §14.5 rule 12). Inherits all `eng/` props via `src/Directory.Build.props`. Added as a project entry under the `src` solution folder in `Ayaka.sln`. Picked up automatically by `dotnet nuke`'s solution walk — no CI change needed.
- Add the composition shell: `IAyakaContext` marker interface, abstract `DefaultAyakaContext : FrostingContext`, `AyakaTaskNames` static class (constants empty for now). No `IHave…` interfaces yet.
- Add `UseAyaka<TContext>()` extension method — wires `UseContext<T>()`, `AddAssembly`, `UseModule<LocalToolsModule>()`, and `InstallToolsFromManifest(".config/dotnet-tools.json")`.
- Port the type-only primitives that have no NUKE references: `Extensions.When` / `WhenNotNull`, `IgnoreCaseWhenPossibleComparer`. (GitHub option types and `GitHubTasks` defer to Phase 6.)
- Set up `test/Ayaka.Cake.Tests/` project with `IsTestProject=True` so it inherits the existing test stack from `eng/Packages.props` (xUnit 2.9.3, FluentAssertions 7.2.0, FakeItEasy 8.3.0, coverlet.collector, GitHubActionsTestLogger, Microsoft.NET.Test.Sdk). One trivial assertion so CI exercises it. Added to `Ayaka.sln` under the `test` solution folder.
- **No CI wiring needed.** `ci.yml`'s `build-packages` job runs `dotnet nuke`, which walks `Ayaka.sln` and picks up `Ayaka.Cake` + `Ayaka.Cake.Tests` automatically. The Phase 1 draft PR exercises this on push.

**Exit criteria.** `dotnet pack src/Ayaka.Cake` produces a valid (empty) NuGet package. `Ayaka.Cake.Tests` runs green locally and on the draft PR's CI. `dotnet nuke` still completes successfully (existing pipeline not regressed).

#### Phase 1 outcome — 2026-05-21 (commits `4dfda07`…`e0482b1`)

Held to plan on the major decisions (branch shape, six logical commits matching the six bullets, all chosen options from §16). Six deviations / additions worth recording:

1. **Plan amendment to §14.5 rule 5.** Folded a Conventional Commits convention ("type: subject" with no scope) into the planning-artefacts commit before commit #1 landed. The rule now reads explicitly; all six Phase 1 commits follow it.

2. **`Cake.DotNetLocalTools.Module 3.0.12` — verified despite README framing.** The module's README claims it tracks Cake 3.0 / net6.0–net7.0 only; the repo is dormant since 2022-11-24. Empirical verification: GitTools/GitVersion ships the same combination `Cake.Frosting 6.1.0 + Cake.DotNetLocalTools.Module 3.0.12` in production (`build/Directory.Packages.props`). Restore + build pass cleanly under net9.0. Plan §8A stands as written; the version is anchored for Renovate in `src/Ayaka.Cake/Ayaka.Cake.csproj`.

3. **`dotnet sln add` rewrites platform configurations.** The .NET 9 SDK's `dotnet sln add` expands `SolutionConfigurationPlatforms` to include `Debug/Release × Any CPU/x64/x86` (~80 unrelated lines per add) instead of inheriting the existing AnyCPU-only style. Both Phase 1 sln additions (`Ayaka.Cake`, `Ayaka.Cake.Tests`) used hand-merged entries with fresh GUIDs to preserve AnyCPU-only. This will recur on every future `dotnet sln add` — convention forward: hand-merge sln entries unless we adopt a different sln tool. **Carry-forward for Phase 3+:** continue hand-merging when adding `build/Build.csproj` and any other projects.

4. **PublicAPI tracking files required, not optional.** `eng/Compiler.props` enables `UsePublicApiAnalyzers` when `IsNuGetPackage=True` and adds `RS0016` etc. to `WarningsAsErrors`. Phase 1 added `PublicAPI.Shipped.txt` (empty header) and `PublicAPI.Unshipped.txt` (9 symbols across the composition shell + primitives) to `src/Ayaka.Cake/`. The plan didn't enumerate this explicitly but it follows from the existing repo convention. Phase 5+ ports will each need to extend `PublicAPI.Unshipped.txt` per public symbol added.

5. **Tests ported verbatim, not "one trivial assertion".** The plan's bullet said "One trivial assertion so CI exercises it"; the tests were instead ported verbatim from `test/Ayaka.Nuke.Tests/ExtensionsTest.cs` and `test/Ayaka.Nuke.Tests/PublicApi/IgnoreCaseWhenPossibleComparer.cs` (only namespaces, usings, and the comparer's sample data adjusted). 6 Facts (vs the plan's 1), full coverage of both primitives. **Convention forward:** Phase 5+ ports must port matching `test/Ayaka.Nuke.Tests/**` files alongside their source, not write fresh replacements.

6. **GUID strategy.** Project GUIDs in `Ayaka.sln`: `{38D7E984-9008-4242-8C6A-80914207D0BC}` for `Ayaka.Cake` (inherited from the one `dotnet sln add` chose before the revert), `{F6E7A7C7-835E-4199-A25C-5D50006BBEA9}` for `Ayaka.Cake.Tests` (freshly `uuidgen`-generated). Both use the standard csproj project-type GUID `{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}` (matching every other src/test project in the solution).

§16 decisions all hold unchanged. Phase 1 closes with the six commits pushed and the draft PR's CI green on the last one.

### Phase 2 — Foundation: basic `IHave…` + `Clean` + `DotNet.Restore` + `DotNet.Build`

Validate the architecture end-to-end with the smallest meaningful slice of real work.

- Define the minimal `IHave…` interfaces needed for the slice: `IHaveSolution`, `IHaveDotNetConfiguration`, `IHaveArtifacts`, `IHaveSources`, optionally `IHaveGitVersion`.
- Implement the 4B/5A customization seam on `DefaultAyakaContext` for the two tools in scope: `DotNetRestoreSettingsBase`+`DotNetRestoreSettings`, `DotNetBuildSettingsBase`+`DotNetBuildSettings`. No per-thing pair needed yet (Restore/Build aren't per-project loops).
- Port `CleanTask : FrostingTask<IAyakaContext>` — smallest task, exercises `ctx is IHaveX` gating and the file layout for `Tasks/`.
- Port `DotNetRestoreTask` + `DotNetBuildTask`. `[IsDependentOn(typeof(DotNetRestoreTask))]` on Build.
- Update `AyakaTaskNames` with the three constants used so far.
- Cover each task with a thin integration test that runs it against a fixture solution.

**Exit criteria.** A test harness can construct a `BuildContext`, call `Clean → Restore → Build`, and produce a built solution. The chosen 4B/5A shape has been exercised once.

### Phase 3 — Dogfood: Ayaka's `build/` starts consuming `Ayaka.Cake`

Real-world stress test before porting more features. Discover ergonomics issues while only three tasks are in flight.

- Create `build/` (alongside `nukebuild/` — not replacing it). Single-project layout per §16.8.
- Author `build/BuildContext.cs` implementing the capability interfaces Ayaka needs for Clean/Restore/Build. Read params via `AyakaArgs` helper.
- `build/Program.cs` calls `new CakeHost().UseAyaka<BuildContext>().Run(args)`.
- Wire CI to run the new `build/` for the dotnet pipeline only (not yet covering test/pack/release) on every push, in addition to `nukebuild/`. The new pipeline is allowed to fail without blocking; its purpose is signal.
- Drop `.config/dotnet-tools.json` with `dotnet-reportgenerator-globaltool` and `gitversion.tool` entries (additive — `nukebuild/` may resolve these differently).

**Exit criteria.** `dotnet run --project build/Build.csproj -- --target=DotNet.Build` produces the same binaries `nukebuild/` does. Two pipelines run side-by-side in CI.

### Phase 4 — Checkpoint: reconcile §16 against reality

Pure review phase, not implementation. Surface and resolve any architectural drift before continuing.

- Audit Phase 2 + 3 against the §16 decisions. For each, record: held / adjusted / rejected.
- Common candidates for revision after first contact: the per-thing 5A layering verbosity, whether `IHaveX` casts are tolerable at scale, whether `AyakaArgs` covers all parameter shapes encountered, whether the `IDotNetTestContext`-style composite interfaces from §1E are actually warranted.
- Update §16 in place with revisions (struck-through original + new decision + one-line "why changed").
- Update the §11 component sketches if any of them now read differently.
- This phase produces zero shipping code. Output is an updated plan and a go/no-go for Phase 5+.

**Exit criteria.** §16 reflects what was actually built, not what was originally proposed. Any decisions that didn't survive contact are documented.

### Phase 5 — Complete the `dotnet` pipeline

With the architecture validated, port the remaining dotnet tasks. Each one stays a single commit/PR.

- `DotNetTestTask` — first task exercising 5A's four-delegate per-thing shape; first task exercising 12C hybrid (context opt-in + per-csproj `XmlPeek`). Ports `CopyCoverageFiles` and `ReportTestCount` helpers verbatim.
- `DotNetPackTask` — second 5A per-thing exercise (per `.nupkg`).
- `DotNetPushTask` — uses the §6 `AyakaArgs.GetSecret` for `NuGetApiKey`; GHA masking applied in `BuildLifetime.Setup`.
- Promote `build/` in CI from advisory to required for the dotnet pipeline.

**Exit criteria.** `build/` runs the full dotnet pipeline (Clean → Restore → Build → Test → Pack → Push) green. `nukebuild/`'s dotnet pipeline is now redundant but still present.

### Phase 6 — `DotNetValidate` + GitHub release

- Port the GitHub option types (`GitHubSettings`, `GitHubReleaseSettings`, `GitHubReleaseNotesSettings`, `GitHubPullRequestSettings`) + their `*Extensions` builders verbatim — POCO surface, no NUKE deps.
- Port `GitHubTasks.cs` verbatim (Octokit-only, Cake-agnostic).
- Implement `DotNetValidateTask` as a custom `Tool<DotNetValidateLocalPackageSettings>` under `Cake.Core.Tooling`. `dotnet-validate` acquired via the local-tool manifest.
- Implement `GitHubReleaseTask : AsyncFrostingTask<IAyakaContext>`, reading `IHaveGitVersion` + `IHaveGitRepository` + `IHaveGitHubToken` from the context.

**Exit criteria.** Release-related tasks runnable from `build/`. CI's release-on-tag flow can switch to the new pipeline as a manual dispatch.

### Phase 7 — VitePress + PublicApi

- `VitePressInstallTask` / `VitePressLintTask` / `VitePressBuildTask` — npm-script wrappers via `Cake.Npm`. Lint script name overridable via 4B Configure-delegate on the context.
- `ShipPublicApisTask : FrostingTask<IAyakaContext>` — body ports from `ICanShipPublicApis` verbatim. Per §16.10, this *is* a library-level reusable task.

**Exit criteria.** Every task that's a candidate for the library has been ported. `Ayaka.Cake`'s public surface is feature-complete for 1.0.0.

### Phase 8 — Self-host cutover

Switch Ayaka's own releases from `nukebuild/` to `build/`, port the AfterRelease orchestration (kept in `build/`, not the library — per §16.10), and drop NUKE artefacts.

- Port `Build.AfterRelease.cs` into `build/Tasks/AfterRelease/`. Library provides `ShipPublicApisTask`; orchestration (branch creation, commit, push, PR) stays in `build/`.
- Promote `build/` in CI from "required for dotnet pipeline" to "the only pipeline" — release jobs use it.
- Drop `nukebuild/`, `.nuke/`, `nuke.globaltool`, `build.{cmd,ps1,sh}` shims. Update `global.json` / `.config/dotnet-tools.json` to reflect the final state.
- Rename `test/Ayaka.Nuke.Tests/` → keep as-is until Phase 9 (still validating `Ayaka.Nuke` while it ships).

**Exit criteria.** Ayaka's own CI uses only `build/`. Releases of `Ayaka.Nuke` itself still ship from this repo via the new pipeline.

### Phase 9 — Documentation

- Author `README.md` / `docs/` for `Ayaka.Cake`: getting-started, the `UseAyaka<T>()` story, the `IHaveX` model, the 4B Configure-delegate seam, the `AyakaArgs` parameter pattern, per-task reference.
- Migration guide for downstream consumers: side-by-side `IHave…` → context-implements-`IHave…` mappings, `[Parameter]` → `AyakaArgs.Get` conversions, `DependsOn<T>` → `AyakaTaskNames` constants.
- `CONTRIBUTING.md` updates for the new build.
- **Keep `Ayaka.Nuke` docs in place.** They are removed together with the library at the .NET 10 cutover (§16.15 T+2).

**Exit criteria.** A new consumer can adopt `Ayaka.Cake` from docs alone. The README is comparable in coverage to `Ayaka.Nuke`'s.

### Release

Per §16.15: ship `Ayaka.Cake 1.0.0` alongside the next `Ayaka.Nuke` release. Mark `Ayaka.Nuke` as `<PackageDeprecated>` after that release with README pointers. Drop entirely at the .NET 10 upgrade.

---

## 16. Decisions

Recorded from the design discussion. Each call references the option chosen in the catalogue above.

1. **§1 Composition.** **1A + 1E hybrid.** Default to capability interfaces (`IHaveX` deriving `IAyakaContext`); tasks bind to `FrostingTask<IAyakaContext>` and cast `ctx as IHaveX`. For task families that need many capabilities at once (e.g. `DotNetTest`, `ShipPublicApis`), introduce narrow composite interfaces (`IDotNetTestContext : IAyakaContext, IHaveSolution, IHaveTestArtifacts, …`) to avoid cast cascades — documented as the second flavor.
2. **§2 Packaging.** **2C — single `Ayaka.Cake` package**, source organized in `src/Ayaka.Cake/{DotNet,GitHub,VitePress,PublicApi,DotNetValidate,NuGet}/` mirroring today's `Ayaka.Nuke.*` layout. One CI release pipeline; transitive deps always present.
3. **§3 Dependency wiring.** **3A.** `AyakaTaskNames` constants + `[IsDependentOn(typeof(ConcreteTask))]`. No programmatic wiring layer.
4. **§4 Customization seam.** **4B.** Configure-delegate properties on the context — `sealed Action<T> XSettingsBase` (baseline, non-bypassable) + `virtual Action<T> XSettings` (consumer hook, defaults to identity). Direct behavioural mirror of NUKE's sealed-Base + virtual-Override.
5. **§5 Settings layering.** **5A — full mirror.** For per-thing loops (`DotNetTest`, `DotNetPack`, `DotNetValidate`): four delegates per tool — global `Base`+`Settings` pair plus per-thing `…ProjectSettingsBase`+`…ProjectSettings` pair. Verbose but behaviour-preserving.
6. **§6 Parameter / Secret.** **6D — `AyakaArgs` helper class.** Centralizes arg + env-var fallback + GHA masking. No `[Parameter]` reflection layer. Cake's `--help` lists tasks via `[TaskDescription]` only — parameter discovery is **not** advertised through `--help` (Cake has no native mechanism, and we don't build one). Parameters documented in `README.md` / `CONTRIBUTING.md` instead.
7. **§8 External tools.** **8A + auto-call.** `UseAyaka<T>()` wires `LocalToolsModule` and calls `InstallToolsFromManifest(".config/dotnet-tools.json")` itself. Consumer only needs to drop a manifest.
8. **§9 Host layout.** **9D-flex — single project, internal folders defined per repo.** `build/Build.csproj` is single-project; folder structure inside `build/Tasks/` is not a universal rule, decided case-by-case per repo. Shim scripts: **9A** (drop them entirely) — rely on `dotnet run --project build/Build.csproj` as Cake's default expects.
    - **Repo-level rename: `build/` → `eng/`.** Today's `build/` holds MSBuild shared infra (`Analyzers.props`, `Compiler.props`, `Packages.props`, `Product.props`, `SourceLink.props`, `TargetFramework.props`, `Usings.props`, `assets/logo.png`). It moves to `eng/` to free `build/` for the Cake Frosting host, aligning with the .NET Foundation convention (`dotnet/runtime`, `aspnetcore`, `sdk`, `efcore`, `roslyn`, `maui`, `wpf`, `winforms`, and Polly). Mechanical scope: `git mv build eng`, then update 15 import lines across `src/Directory.Build.props` (7), `test/Directory.Build.props` (6), and `nukebuild/Directory.Build.props` (2 — still load-bearing until Phase 8, so the update can't be deferred). `eng/Product.props`'s logo reference is `$(MSBuildThisFileDirectory)`-relative and follows automatically. No CI / script touchpoints.
9. **§10 Soft ordering.** **Mixed 10A/10C/10D per case.** Each of the three current uses gets the cheapest viable treatment: `ShouldRun` gates where harmless (10A), `taskFilter` opt-out where it fits (10C), hard-dep where the order is genuinely required (10D). No bespoke runtime wiring layer.
10. **§11.8 After-Release flow.** **Split.** `ShipPublicApisTask` ships in `Ayaka.Cake` (reusable primitive). The PR-creation / branch-and-push orchestration stays in `Ayaka`'s own `build/` (repo-specific). Library ships primitives; the chain itself is not bundled as a library task graph.
11. **§12 Per-test-project detection.** **12C — hybrid.** Apply loggers/collectors when the context implements `IHaveCodeCoverage` (or equivalent) **and** the project's `.csproj` references the relevant package via `XmlPeek`. Faithful to current Ayaka.Nuke behaviour.
12. **§13 Discoverability.** **13A — README docs only.** Built-in `--description` for tasks; parameters documented in `README.md` / `CONTRIBUTING.md`. No `Help` task (follows from 6D — no reflection layer to power it).
13. **§16.11 Naming.** `Ayaka.Cake` (single package) + sub-namespaces `Ayaka.Cake.DotNet`, `.GitHub`, `.VitePress`, `.PublicApi`, `.DotNetValidate`, `.NuGet` mirroring today's `Ayaka.Nuke.*` split.
14. **GitVersion.** Keep `GitVersion.yml` + `GitVersion.Tool 5.12.0`. Migration to GitVersion 6+ is a separate change, not bundled with the Cake migration.
15. **Migration drop policy — phased deprecation.**
    - **T+0** (next release): Ayaka.Cake 1.0.0 ships alongside the next Ayaka.Nuke release. Both supported.
    - **T+1**: Ayaka.Nuke marked `<PackageDeprecated>` on NuGet; README points at Ayaka.Cake. No new features in Nuke; critical fixes only if needed.
    - **T+2** (.NET 10 upgrade): Ayaka.Nuke dropped entirely. The .NET 10 bump is the natural cutover — downstream consumers will already need to touch their builds.
