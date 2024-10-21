# Ayaka.Nuke

Provides various opinionated build components for simpler build automation using [NUKE](https://nuke.build/).

## Key Features

`Ayaka.Nuke` is all about reusable build components and tasks to make your life easier.

The build components and tasks follow a simple naming convention:

* `IHave...` to extend the build with additional context. For example
  * `IHaveSources`, `IHaveTests`, `IHaveArtifacts`, etc. to provide conventional paths for sources, tests, artifacts and other directories to the build
  * `IHaveGitRepository` to provide information about the current Git repository to the build
  * `IHaveGitVersion` to provide information about the current Git version configuration to the build
  * `IHaveNuGetConfiguration` to have NuGet API client parameters on the build
  * `IHaveGitHubToken` to have GitHub API client parameters on the build
  * and many more
* `ICan...` to extend the build with additional targets. For example
  * `ICanClean` to clean the build directories
  * `ICanDotNet...` to restore, build, test and pack .NET projects
  * `ICanVitePress...` to lint and build a VitePress site
  * `ICanShipPublicApis` to ship public APIs from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt`
  * and many more
* `...Tasks` to provide build tasks that wrap other tools or APIs. For example
  * `GitHubTasks` to create pull requests, generate release notes or create new releases using the GitHub API
  * `DotNetValidateTasks` to validate .NET NuGet packages using `dotnet-validate` tool
  * and many more

::: tip
In a NUKE build project, a build component represents additional build context or targets that can be added to a build,
whereas a build task is a method that can be executed as part of target.
:::

## How to Use

Using any of the build components is as simple as decorating your build class. For example:

```csharp
class Build
    : NukeBuild
        IHaveSources,
        ICanDotNetRestore,
        ICanDotNetBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => target => target
        .DependsOn<IHaveDotNetRestoreTarget>()
        .DependsOn<IHaveDotNetBuildTarget>();
}
```

Using any of the build tasks is the same as calling a static method in .NET

```csharp
await GitHubTasks.CreateRelease(new GitHubReleaseSettings
{
    // ...
});
```

or with a `static using`

```csharp
using static Ayaka.Nuke.GitHub.GitHubTasks;

await CreateRelease(new GitHubReleaseSettings
{
    // ...
});
```
