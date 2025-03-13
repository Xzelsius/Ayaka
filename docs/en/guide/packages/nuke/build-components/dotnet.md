# .NET Build Components

The .NET build components are suitable for .NET based applications.

## Build Context

The build context components extend the build with additional information required during the build.

### IHaveDotNetConfiguration

The `IHaveDotNetConfiguration` build component decorates the build with the .NET configuration to be
used during the build.

| Property              | Description                                        |
|-----------------------|----------------------------------------------------|
| `DotNetConfiguration` | The .NET configuration to be used during the build |

Ayaka knows two `Configuration` values:

* `Debug`: Which is the default if `IsLocalBuild` is `true`
* `Release`: Which is the default if `IsLocalBuild` is `false`

::: code-group

```csharp {3,9} [Usage]
class Build
    : NukeBuild,
        IHaveDotNetConfiguration
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Configuration: {((IHaveDotNetConfiguration)this).DotNetConfiguration}");
        });
}
```

```csharp {7} [Change the value in Code]
class Build
    : NukeBuild,
        IHaveDotNetConfiguration
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configuration IHaveDotNetConfiguration.DotNetConfiguration => Configuration.Debug;

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Configuration: {((IHaveDotNetConfiguration)this).DotNetConfiguration}");
        });
}
```

```bash {2} [Change the value with Parameter]
dotnet nuke \
   --dotnet-configuration "Debug"
```

```bash {1} [Change the value with Environment Variable]
export DOTNET_CONFIGURATION="Debug"
dotnet nuke
```

:::

### IHaveNuGetConfiguration

The `IHaveNuGetConfiguration` build component decorates the build with the NuGet configuration used
when pushing packages to a NuGet feed.

| Property      | Description                                                                  |
|---------------|------------------------------------------------------------------------------|
| `NuGetSource` | The URL of the NuGet feed. Defaults to `https://api.nuget.org/v3/index.json` |
| `NuGetApiKey` | The API key used to authenticate with the NuGet feed                         |

::: code-group

```csharp {3,9} [Usage]
class Build
    : NukeBuild,
        IHaveNuGetConfiguration
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"NuGet Source: {((IHaveNuGetConfiguration)this).NuGetSource}");
        });
}
```

```csharp {7-8} [Change the value in Code]
class Build
    : NukeBuild,
        IHaveNuGetConfiguration
{
    public static int Main() => Execute<Build>(x => x.Default);

    string IHaveNuGetConfiguration.NuGetSource => "https://api.nuget.org/v3/index.json";
    string IHaveNuGetConfiguration.NuGetApiKey => "my-api-key-value";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"NuGet Source: {((IHaveNuGetConfiguration)this).NuGetSource}");
        });
}
```

```bash {2-3} [Change the value with Parameter]
dotnet nuke \
   --nuget-source "https://api.nuget.org/v3/index.json" \
   --nuget-api-key "my-api-key-value"
```

```bash {1-2} [Change the value with Environment Variable]
export NUGET_SOURCE="https://api.nuget.org/v3/index.json"
export NUGET_API_KEY="my-api-key-value"
dotnet nuke
```

:::

## Build Targets

The build target components extend the build with additional targets that can be executed individually
or as part of other targets.

Although, not required by NUKE, Ayaka provides a definition and one or more implementations for each target.
The definitions are named using the `IHave...Target` pattern, for example `IHaveDotNetRestoreTarget`. The
implementations use the `ICan...` pattern, for example `ICanDotNetRestore`.

This allows you to create custom targets that depend on a definition, but not on a strict implementation of it.
For example, an implementation from Ayaka does not suit your needs, and you want to create your own implementation
based on Ayaka's definition.

### ICanDotNetRestore

The `ICanDotNetRestore` build target restores the NuGet packages for the solution specified in the build parameters.

It does so by executing the `dotnet restore` command for the entire solution.

The `ICanDotNetRestore` target implements the `IHaveDotNetRestoreTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanDotNetRestore
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveDotNetRestoreTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-9} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanDotNetRestore
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configure<DotNetRestoreSettings> ICanDotNetRestore.DotNetRestoreSettings
        => dotnet => dotnet
            .UseLockFile();

    Target Default => _ => _
        .DependsOn<IHaveDotNetRestoreTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

### ICanDotNetBuild

The `ICanDotNetBuild` build target builds the solution specified in the build parameters.

It does so by executing the `dotnet build` command for the entire solution.
By default, it automatically uses the following parameters when building the solution:

* Uses the configuration specified with the [`IHaveDotNetConfiguration`] build context component
* Uses `--no-restore` if the [`IHaveDotNetRestoreTarget`] build target succeeded
* Sets the `ContinuousIntegrationBuild` to `true` if `IsServerBuild` is `true`
* Sets the assembly, file and informational version if the build is decorated with the [`IHaveGitVersion`]
  build context component

Once finished, it reports the `NuGetVersionV2` as `Version` to the target summary if the build is decorated
with the [`IHaveGitVersion`] build context component.

The `ICanDotNetBuild` target implements the `IHaveDotNetBuildTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanDotNetBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveDotNetBuildTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-9} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanDotNetBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configure<DotNetBuildSettings> ICanDotNetBuild.DotNetBuildSettings
        => dotnet => dotnet
            .SetNoIncremental();

    Target Default => _ => _
        .DependsOn<IHaveDotNetBuildTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

### ICanDotNetTest

The `ICanDotNetTest` build target runs the tests in all configured test projects.

It does so by executing the `dotnet test` command for all test projects specified by `TestProjects`.
By default, `TestProjects` represents all projects within the `TestsDirectory` directory of the
[`IHaveTests`] build context component.

The following loggers are used when running the tests:

* `trx;LogFileName={project-name}.trx`: Always used
* `GitHubActions;report-warnings=false`: If building with GitHub Actions and the test project references
   the `GitHubActionsTestLogger` NuGet package

If the build is decorated with the [`IHaveCodeCoverage`] build context component and the test project
references the `coverlet.collector` NuGet package, code coverage is collected using the `XPlat Code Coverage`
collector and source link is enabled.

Once finished, it reports the `Passed`, and optionally the `Failed` and `Skipped` tests, to the target summary.
`Failed` and `Skipped` tests are only reported if there are any.

The `ICanDotNetTest` target implements the `IHaveDotNetTestTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanDotNetTest
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveDotNetTestTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-10,12-15} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanDotNetTest
{
    public static int Main() => Execute<Build>(x => x.Default);

    // Change settings used for all test projects
    Configure<DotNetTestSettings> ICanDotNetTest.DotNetTestSettings
        => dotnet => dotnet
            .Filter("FullyQualifiedName~MyTest");

    // Change settings used for a specific test project
    Configure<DotNetTestSettings, Project> ICanDotNetTest.DotNetTestProjectSettings
        => (dotnet, project) => dotnet
            .SetSettingsFile("/path/to/settings.xml");

    Target Default => _ => _
        .DependsOn<IHaveDotNetTestTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-10} [Change the projects in Code]
class Build
    : NukeBuild,
        ICanDotNetTest
{
    public static int Main() => Execute<Build>(x => x.Default);

    IEnumerable<Project> ICanDotNetTest.TestProjects => new[]
    {
        Solution.GetProject("MyTestProject")
    };

    Target Default => _ => _
        .DependsOn<IHaveDotNetTestTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

### ICanDotNetPack

The `ICanDotNetPack` build target packs all packable project in the solution.

It does so by executing the `dotnet pack` command for the entire solution. Only projects that have
`IsPackable` set to `true` will be affected by this command.

By default, it automatically uses the following parameters when packing the projects:

* Uses the configuration specified with the [`IHaveDotNetConfiguration`] build context component
* Uses `--no-build` if the [`IHaveDotNetBuildTarget`] build target succeeded
* Sets the output directory to the `PackagesDirectory` of the [`IHavePackageArtifacts`] build context component
* Sets `RepositoryUrl` property to the HTTPS URL of the current repository if the build is decorated with the
  [`IHaveGitRepository`] build context component
* Sets the `Version` property to the `NuGetVersionV2` if the build is decorated with the [`IHaveGitVersion`]
  build context component

Once finished, it reports the amount of created packages as `Packages` to the target summary.

The `ICanDotNetPack` target implements the `IHaveDotNetPackTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanDotNetPack
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveDotNetPackTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-9} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanDotNetPack
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configure<DotNetPackSettings> ICanDotNetPack.DotNetPackSettings
        => dotnet => dotnet
            .SetIncludeSymbols(true);

    Target Default => _ => _
        .DependsOn<IHaveDotNetPackTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

### ICanDotNetPush

The `ICanDotNetPush` build target pushes all configured packages to the configured NuGet feed.

It does so by executing the `dotnet nuget push` command for all packages specified by `NuGetPackagesToPush`.

By default, `NuGetPackagesToPush` represents all packages (`*.nupkg`) within the `PackagesDirectory` directory of the
[`IHavePackageArtifacts`] build context component.

::: tip
The `dotnet nuget push` command automatically pushes the symbols package if a symbols package is found in the
same directory as the NuGet package that is being pushed.
:::

The target feed and API key is specified by the `NuGetSource` and `NuGetApiKey` property of the
[`IHaveNuGetConfiguration`] build context component.

The `ICanDotNetPush` target implements the `IHaveDotNetPushTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanDotNetPush
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveDotNetPushTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-10,12-15} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanDotNetPush
{
    public static int Main() => Execute<Build>(x => x.Default);

    // Change settings used for all packages
    Configure<DotNetNuGetPushSettings> ICanDotNetPush.DotNetPushSettings
        => dotnet => dotnet
            .SetTimeout(500);

    // Change settings used for a specific package
    Configure<DotNetNuGetPushSettings, AbsolutePath> ICanDotNetPush.DotNetPushPackageSettings
        => (dotnet, packagePath) => dotnet
            .SetNoSymbols(true);

    Target Default => _ => _
        .DependsOn<IHaveDotNetPushTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-10} [Change the packages in Code]
class Build
    : NukeBuild,
        ICanDotNetPush
{
    public static int Main() => Execute<Build>(x => x.Default);

    IEnumerable<AbsolutePath> ICanDotNetPush.NuGetPackagesToPush => new[]
    {
        PackagesDirectory / "MyPackage.1.0.0.nupkg"
    };

    Target Default => _ => _
        .DependsOn<IHaveDotNetPushTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

### ICanDotNetValidate

The `ICanDotNetValidate` build target validates created NuGet packages.

It does so by executing the `dotnet-validate` CLI command for all packages specified by `NuGetPackagesToValidate`.

`dotnet-validate` is provided by people behind [NuGetPackageExplorer] and makes sure that your packages
meet certain requirements like being deterministic or having symbols.

::: warning
You may need to install the `dotnet-validate` CLI tool before using this target.

The simplest way to do this is by adding `<PackageDownload Include="dotnet-validate" Version="..." />` to your
build project file.

See [Executing CLI Tools] for more information.
:::

::: warning
Please note that `dotnet-validate` with version `0.0.1-preview.304` and below still requires .NET 6 SDK installed
on the build system.

Since the support for .NET 6 ended in Nov 2024, you likely have to install it manually.
:::

By default, `NuGetPackagesToPush` represents all packages (`*.nupkg`) within the `PackagesDirectory` directory of the
[`IHavePackageArtifacts`] build context component.

The `ICanDotNetValidate` target implements the `IHaveDotNetValidateTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanDotNetValidate
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveDotNetValidateTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-12} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanDotNetValidate
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configure<DotNetValidateSettings, AbsolutePath> ICanDotNetValidate.DotNetValidatePackageSettings
        => (dotnet, packagePath) => dotnet
            .SetProcessLogger((type, s) =>
            {
                // ...
            });

    Target Default => _ => _
        .DependsOn<IHaveDotNetValidateTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-10} [Change the packages in Code]
class Build
    : NukeBuild,
        ICanDotNetValidate
{
    public static int Main() => Execute<Build>(x => x.Default);

    IEnumerable<AbsolutePath> ICanDotNetValidate.NuGetPackagesToValidate => new[]
    {
        PackagesDirectory / "MyPackage.1.0.0.nupkg"
    };

    Target Default => _ => _
        .DependsOn<IHaveDotNetValidateTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

[`IHaveGitRepository`]: ./general#ihavegitrepository
[`IHaveGitVersion`]: ./general#ihavegitversion
[`IHaveSources`]: ./general#ihavesource
[`IHaveTests`]: ./general#ihavetests
[`IHaveArtifacts`]: ./general#ihaveartifacts
[`IHavePackageArtifacts`]: ./general#ihavepackageartifacts
[`IHaveCodeCoverage`]: ./general#ihavecodecoverage
[`IHaveDotNetConfiguration`]: ./dotnet#ihavedotnetconfiguration
[`IHaveDotNetRestoreTarget`]: ./dotnet#icandotnetrestore
[`IHaveDotNetBuildTarget`]: ./dotnet#icandotnetbuild
[`IHaveNuGetConfiguration`]: ./dotnet#ihavenugetconfiguration
[NuGetPackageExplorer]: https://github.com/NuGetPackageExplorer/NuGetPackageExplorer
[Executing CLI Tools]: https://nuke.build/docs/common/cli-tools
