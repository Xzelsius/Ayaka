# VitePress Build Components

The VitePress build components are suitable for VitePress based sites.

## Build Targets

The build target components extend the build with additional targets that can be executed individually
or as part of other targets.

Although, not required by NUKE, Ayaka provides a definition and one or more implementations for each target.
The definitions are named using the `IHave...Target` pattern, for example `IHaveVitePressBuildTarget`. The
implementations use the `ICan...` pattern, for example `ICanVitePressBuild`.

This allows you to create custom targets that depend on a definition, but not on a strict implementation of it.
For example, an implementation from Ayaka does not suit your needs, and you want to create your own implementation
based on Ayaka's definition.

### ICanVitePressInstall

The `ICanVitePressInstall` build targets installs the dependencies for a VitePress site.

Its does so by executing `npm ci` command in the directory specified by `DocsDirectory` of the
[`IHaveDocumentation`] build context component.

The `ICanVitePressInstall` target implements the `IHaveVitePressInstallTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanVitePressInstall
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveVitePressInstallTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-9} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanVitePressInstall
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configure<NpmCiSettings> ICanVitePressInstall.VitePressInstallSettings
        => install => install
            .SetProcessEnvironmentVariable("MY_VAR", "some-value");

    Target Default => _ => _
        .DependsOn<IHaveVitePressInstallTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

### ICanVitePressLint

The `ICanVitePressLint` build target lints a VitePress site.

It does so by executing the `npm run lint` command in the directory specified by `DocsDirectory` of the
[`IHaveDocumentation`] build context component.

::: info
Since the build target just executes a NPM script, you can use whatever linter you want
as long as it is not interactive.

Interactive stuff is not suitable for build scripts, as they are meant to be run in a CI/CD pipeline.
:::

The `ICanVitePressLint` target implements the `IHaveVitePressLintTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanVitePressLint
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveVitePressLintTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-9} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanVitePressLint
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configure<NpmRunSettings> ICanVitePressLint.VitePressLintSettings
        => lint => lint
            .SetArguments("--no-eslintrc");

    Target Default => _ => _
        .DependsOn<IHaveVitePressLintTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

### ICanVitePressBuild

The `ICanVitePressBuild` build target builds a VitePress site.

It does so by executing the `npm run build` command in the directory specified by `DocsDirectory` of the
[`IHaveDocumentation`] build context component.

::: info
Since the build target just executes a NPM script, you can use whatever command you want
as long as it is not interactive.

Interactive stuff is not suitable for build scripts, as they are meant to be run in a CI/CD pipeline.
:::

By default, the build target expects the command behind `npm run build` to be `vitepress build` and based
on this uses the directory specified by `DocsArtifactsDirectory` of the [`IHaveDocumentationArtifacts`]
build context component as the output directory (`--outDir`).

The `ICanVitePressBuild` target implements the `IHaveVitePressBuildTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanVitePressBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveVitePressBuildTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-9} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanVitePressBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configure<NpmRunSettings> ICanVitePressBuild.VitePressBuildSettings
        => run => run
            .SetArguments("--cors");

    Target Default => _ => _
        .DependsOn<IHaveVitePressBuildTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

[`IHaveDocumentation`]: ./general#ihavedocumentation
[`IHaveDocumentationArtifacts`]: ./general#ihavedocumentationartifacts
