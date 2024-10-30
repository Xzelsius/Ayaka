## About

Provides various opinionated build components for simpler build automation using [NUKE](https://nuke.build/).

## Key Features

Provides build components like:

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

Additionally, provides build tasks that wrap other tools or APIs like:

* `GitHubTasks` to create pull requests, generate release notes or create new releases
* `DotNetValidateTasks` to validate .NET NuGet packages using `dotnet-validate` tool

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

_Note: You can either use `ICanDotNetRestore` or `IHaveDotNetRestoreTarget` to use the target with `.DependsOn()`._

## Additional Documentation

See [the documentation](https://xzelsius.github.io/Ayaka/guide/packages/nuke) for more details.

## Feedback & Contributing

`Ayaka` is an open-source project and welcomes contributions. If you have any ideas, improvements or issues, please open an issue
or a pull request at [the GitHub repository](https://github.com/Xzelsius/Ayaka).
