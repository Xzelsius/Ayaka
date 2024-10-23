# General Build Components

The general build components are suitable for all sort of applications, some of them can even
be helpful in deployment pipelines.

## Build Context

The build context components extend the build with additional information required during the build.
They can, for, example, describe where to find certain stuff in the build environment.

### IHaveGitRepository

The `IHaveGitRepository` build component decorates the build with information about the current Git repository.

| Property        | Required | Description                                  |
|-----------------|----------|----------------------------------------------|
| `GitRepository` | Yes      | Information about the current Git repository |

::: info
It uses the `[GitRepository]` functionality from the `Nuke.Common` NuGet package internally,
which loads the information based on the local root directory.
:::

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveGitRepository
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Commit: {((IHaveGitRepository)this).GitRepository.Commit}");
        });
}
```

:::

### IHaveGitVersion

The `IHaveGitVersion` build component decorates the build with information about the version numbers
computed using `GitVersion`.

| Property        | Required | Description                                  |
|-----------------|----------|----------------------------------------------|
| `GitVersion`    | Yes      | The version numbers computed by `GitVersion` |

::: info
It uses the `[GitVersion]` functionality from the `Nuke.Common` NuGet package internally,
which loads the information based on the local Git repository.
:::

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveGitVersion
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"NuGetVersionV2: {((IHaveGitVersion)this).Versioning.NuGetVersionV2}");
        });
}
```

:::

### IHaveSolution

The `IHaveSolution` build component decorates the build with information about the solution configured
in the build parameters.

| Property        | Required | Description                                                        |
|-----------------|----------|--------------------------------------------------------------------|
| `Solution`      | Yes      | The information of the solution configured in the build parameters |

::: info
It uses the `[Solution]` functionality from the `Nuke.Common` package internally,
which loads the information based on the solution file configured in the build parameters.
:::

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveSolution
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Project count: {((IHaveSolution)this).Solution.AllProjects.Count}");
        });
}
```

:::

### IHaveSources

The `IHaveSources` build component decorates the build with information about where to find the source
code of the application.

| Property          | Default               | Description                                    |
|-------------------|-----------------------|------------------------------------------------|
| `SourceDirectory` | `{RootDirectory}/src` | The directory where the source code is located |

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveSources
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Source directory: {((IHaveSources)this).SourceDirectory}");
        });
}
```

```csharp{7} [Change the path in Code]
class Build
    : NukeBuild,
        IHaveSources
{
    public static int Main() => Execute<Build>(x => x.Default);

    AbsolutePath IHaveSources.SourceDirectory => RootDirectory / "my-source-directory";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Source directory: {((IHaveSources)this).SourceDirectory}");
        });
}
```

:::

### IHaveTests

The `IHaveTests` build component decorates the build with information about where to find the test
code of the application.

| Property         | Default                | Description                                  |
|------------------|------------------------|----------------------------------------------|
| `TestsDirectory` | `{RootDirectory}/test` | The directory where the test code is located |

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveTests
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Tests directory: {((IHaveTests)this).TestsDirectory}");
        });
}
```

```csharp{7} [Change the path in Code]
class Build
    : NukeBuild,
        IHaveTests
{
    public static int Main() => Execute<Build>(x => x.Default);

    AbsolutePath IHaveTests.TestsDirectory => RootDirectory / "my-test-directory";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Tests directory: {((IHaveTests)this).TestsDirectory}");
        });
}
```

:::

### IHaveDocumentation

The `IHaveDocumentation` build component decorates the build with information about where to find the
documentation of the application.

| Property        | Default                | Description                                      |
|-----------------|------------------------|--------------------------------------------------|
| `DocsDirectory` | `{RootDirectory}/docs` | The directory where the documentation is located |

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveDocumentation
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Documentation directory: {((IHaveDocumentation)this).DocsDirectory}");
        });
}
```

```csharp{7} [Change the path in Code]
class Build
    : NukeBuild,
        IHaveDocumentation
{
    public static int Main() => Execute<Build>(x => x.Default);

    AbsolutePath IHaveDocumentation.DocsDirectory => RootDirectory / "my-docs-directory";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Documentation directory: {((IHaveDocumentation)this).DocsDirectory}");
        });
}
```

:::

### IHaveArtifacts

The `IHaveArtifacts` build component decorates the build with information about where to put various
artifacts created during the build process.

| Property            | Default                     | Description                                   |
|---------------------|-----------------------------|-----------------------------------------------|
| `ArtifactsDirectory`| `{RootDirectory}/artifacts` | The directory where the artifacts are located |

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveArtifacts
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Artifacts directory: {((IHaveArtifacts)this).ArtifactsDirectory}");
        });
}
```

```csharp{7} [Change the path in Code]
class Build
    : NukeBuild,
        IHaveArtifacts
{
    public static int Main() => Execute<Build>(x => x.Default);

    AbsolutePath IHaveArtifacts.ArtifactsDirectory => RootDirectory / "my-artifacts-directory";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Artifacts directory: {((IHaveArtifacts)this).ArtifactsDirectory}");
        });
}
```

```bash {2} [Change the path with Parameter]
dotnet nuke \
  --artifacts "/path/to/artifacts"
```

```bash {1} [Change the path with Environment Variable]
export ARTIFACTS="/path/to/artifacts"
dotnet nuke
```

:::

### IHaveTestArtifacts

The `IHaveTestArtifacts` build component decorates the build with information about where to put test
artifacts created during the build process.

| Property               | Default                             | Description                                      |
|------------------------|-------------------------------------|--------------------------------------------------|
| `TestResultsDirectory` | `{ArtifactsDirectory}/test-results` | The directory where the test results are located |

::: info
Implicitly decorates the build with [`IHaveArtifacts`](./general#ihaveartifacts) to use the `ArtifactsDirectory`.
:::

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveTestArtifacts
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Test results directory: {((IHaveTestArtifacts)this).TestResultsDirectory}");
        });
}
```

```csharp{7} [Change the path in Code]
class Build
    : NukeBuild,
        IHaveTestArtifacts
{
    public static int Main() => Execute<Build>(x => x.Default);

    AbsolutePath IHaveTestArtifacts.TestResultsDirectory => RootDirectory / "my-test-results-directory";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Test results directory: {((IHaveTestArtifacts)this).TestResultsDirectory}");
        });
}
```

:::

### IHaveCodeCoverage

The `IHaveCodeCoverage` build component decorates the build with information about where to put code
coverage artifacts created during the build process.

| Property            | Default                         | Description                                      |
|---------------------|---------------------------------|--------------------------------------------------|
| `CoverageDirectory` | `{ArtifactsDirectory}/coverage` | The directory where the code coverage is located |

::: info
Implicitly decorates the build with [`IHaveArtifacts`](./general#ihaveartifacts) to use the `ArtifactsDirectory`.
:::

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveCodeCoverage
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Coverage directory: {((IHaveCodeCoverage)this).CoverageDirectory}");
        });
}
```

```csharp{7} [Change the path in Code]
class Build
    : NukeBuild,
        IHaveCodeCoverage
{
    public static int Main() => Execute<Build>(x => x.Default);

    AbsolutePath IHaveCodeCoverage.CoverageDirectory => RootDirectory / "my-coverage-directory";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Coverage directory: {((IHaveCodeCoverage)this).CoverageDirectory}");
        });
}
```

:::

### IHavePackageArtifacts

The `IHavePackageArtifacts` build component decorates the build with information about where to put
package artifacts created during the build process.

| Property              | Default                         | Description                                           |
|-----------------------|---------------------------------|-------------------------------------------------------|
| `PackagesDirectory`   | `{ArtifactsDirectory}/packages` | The directory where the package artifacts are located |

::: info
Implicitly decorates the build with [`IHaveArtifacts`](./general#ihaveartifacts) to use the `ArtifactsDirectory`.
:::

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHavePackageArtifacts
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Packages directory: {((IHavePackageArtifacts)this).PackagesDirectory}");
        });
}
```

```csharp{7} [Change the path in Code]
class Build
    : NukeBuild,
        IHavePackageArtifacts
{
    public static int Main() => Execute<Build>(x => x.Default);

    AbsolutePath IHavePackageArtifacts.PackagesDirectory => RootDirectory / "my-packages-directory";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Packages directory: {((IHavePackageArtifacts)this).PackagesDirectory}");
        });
}
```

:::

### IHaveDocumentationArtifacts

The `IHaveDocumentationArtifacts` build component decorates the build with information about where to put
documentation artifacts created during the build process.

| Property                 | Default                     | Description                                                 |
|--------------------------|-----------------------------|-------------------------------------------------------------|
| `DocsArtifactsDirectory` | `{ArtifactsDirectory}/docs` | The directory where the documentation artifacts are located |

::: info
Implicitly decorates the build with [`IHaveArtifacts`](./general#ihaveartifacts) to use the `ArtifactsDirectory`.
:::

::: code-group

```csharp{3,9} [Usage]
class Build
    : NukeBuild,
        IHaveDocumentationArtifacts
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Docs artifacts directory: {((IHaveDocumentationArtifacts)this).DocsArtifactsDirectory}");
        });
}
```

```csharp{7} [Change the path in Code]
class Build
    : NukeBuild,
        IHaveDocumentationArtifacts
{
    public static int Main() => Execute<Build>(x => x.Default);

    AbsolutePath IHaveDocumentationArtifacts.DocsArtifactsDirectory => RootDirectory / "my-docs-artifacts-directory";

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Docs artifacts directory: {((IHaveDocumentationArtifacts)this).DocsArtifactsDirectory}");
        });
}
```

:::

## Build Targets

The build target components extend the build with additional targets that can be executed individually
or as part of other targets.

Although, not required by NUKE, Ayaka provides a definition and one or more implementations for each target.
The definitions are named using the `IHave...Target` pattern, for example `IHaveCleanTarget`. The implementations
use the `ICan...` pattern, for example `ICanClean`.

This allows you to create custom targets that depend on a definition, but not on a strict implementation of it.
For example, an implementation from Ayaka does not suit your needs, and you want to create your own implementation
based on Ayaka's definition.

### ICanClean

The `ICanClean` build target cleans various directories configured in the build.

Depending on the build context components the build is decorated with, it does clean the following directories:

* `IHaveSources`: The directory located at `SourceDirectory`
* `IHaveTests`: The directory located at `TestsDirectory`
* `IHaveArtifacts`: The directory located at `ArtifactsDirectory`

The `ICanClean` target implements the `IHaveCleanTarget` build target definition.

::: code-group

```csharp{3,8} [Usage]
class Build
    : NukeBuild,
        ICanClean
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveCleanTarget>()
        .Executes(x =>
        {
            // ...
        );
}
```

:::

### ICanShipPublicApis

The `ICanShipPublicApis` build target ships your public APIs.

It does so iterating over all projects specified by `PublicApiProjects` and moving everything from
`PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt`.

By default, `PublicApiProjects` represents all projects within the `SourceDirectory` directory of the
[`IHaveSources`](./general#ihavesources) build context component.

The build target maintains the alphabetical order of the public APIs files and recognizes the `#nullable enable` flag.

The `ICanShipPublicApis` implements the `IHaveShipPublicApiTarget` build target definition.

::: code-group

```csharp{3,8} [Usage]
class Build
    : NukeBuild,
        ICanShipPublicApis
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveShipPublicApisTarget>()
        .Executes(x =>
        {
            // ...
        );
}
```

```csharp {7-10} [Change the projects in Code]
class Build
    : NukeBuild,
        ICanShipPublicApis
{
    public static int Main() => Execute<Build>(x => x.Default);

    IEnumerable<Project> ICanShipPublicApis.PublicApiProjects => new[]
    {
        Solution.GetProject("MyProject")
    };

    Target Default => _ => _
        .DependsOn<IHaveShipPublicApisTarget>()
        .Executes(x =>
        {
            // ...
        );
}
```

:::
