# Utilities

`Ayaka.Nuke` provides a set of utility methods that can improve the readability of your build scripts
or provide other useful functionality.

## When

```csharp [Definition]
public static T When<T>(this T settings, bool condition, Func<T, T> configurator)
```

Invokes a `configurator` method, but only if the passed in `condition` is `true`.

The `configurator` method receives the object the method is called on and should return the same object
it is called on (at least of the same type, if you care for immutability), so multiple calls can be chained.

::: tip
This method is similar to the built-in `.When()` but using a simple `bool` instead of a `Func<T, bool>`.
:::

::: code-group

```csharp {3-8} [Usage for Settings]
Configure<DotNetBuildSettings> ICanDotNetBuild.DotNetBuild
        => dotnet => dotnet
            .When(IsServerBuild, x => x.EnableContinuousIntegrationBuild())
```

```csharp {6} [Usage in Targets]
Target Default => target => target
    .Executes(() =>
    {
        ReportSummary(
            summary => summary
                .When(IsServerBuild, s => s.AddPair("CI", "Yes"))
        );
    });
```

```csharp {3-10} [Chaining multiple calls]
Configure<DotNetTestSettings, Project> ICanDotNetTest.DotNetTestProjectSettings
    => (dotnet, project) => dotnet
        .When(
            GitHubActions.Instance is not null && project.HasPackageReference("GitHubActionsTestLogger"),
            d => d.AddLoggers("GitHubActions;report-warnings=false"))
        .When(
            this is IHaveCodeCoverage && project.HasPackageReference("coverlet.collector"),
            d => d
                .SetDataCollector("XPlat Code Coverage")
                .EnableUseSourceLink())
```

:::

## WhenNotNull

```csharp [Definition]
public static T WhenNotNull<T, TObject>(this T settings, TObject? obj, Func<T, TObject, T> configurator)
```

Invokes a `configurator` method, but only if the passed in `obj` is not `null`.

The `configurator` method receives both, the object the method is called on and the `obj` parameter,
and should return the same object it is called on (at least of the same type, if you care for immutability),
so multiple calls can be chained.

::: tip
This method is especially useful to make otherwise ugly safe casts and `null` checks more readable.
:::

::: code-group

```csharp {3-8} [Usage for Settings]
Configure<DotNetBuildSettings> ICanDotNetBuild.DotNetBuild
        => dotnet => dotnet
            .WhenNotNull(
                this as IHaveGitVersion,
                (d, o) => d
                    .SetAssemblyVersion(o.Versioning.AssemblySemVer)
                    .SetFileVersion(o.Versioning.AssemblySemFileVer)
                    .SetInformationalVersion(o.Versioning.InformationalVersion));
```

```csharp {6-8} [Usage in Targets]
Target Default => target => target
    .Executes(() =>
    {
        ReportSummary(
            summary => summary
                .WhenNotNull(
                    this as IHaveGitVersion,
                    (s, o) => s.AddPair("Version", o.Versioning.NuGetVersionV2))
        );
    });
```

```csharp {3-8} [Chaining multiple calls]
Configure<DotNetPackSettings> ICanDotNetPack.DotNetPackSettings
        => dotnet => dotnet
            .WhenNotNull(
                this as IHaveGitRepository,
                (d, o) => d.SetRepositoryUrl(o.GitRepository.HttpsUrl))
            .WhenNotNull(
                this as IHaveGitVersion,
                (d, o) => d.SetVersion(o.Versioning.NuGetVersionV2));
```

:::
