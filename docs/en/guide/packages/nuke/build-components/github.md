# GitHub Build Components

The GitHub build components are suitable for working with GitHub repositories.

## Build Context

The build context components extend the build with additional information required during the build.

### IHaveGitHubToken

The `IHaveGitHubToken` build component decorates the build with the configuration used for accessing
the GitHub API.

| Property        | Description                                          |
|-----------------|------------------------------------------------------|
| `GitHubBaseUrl` | The base URL for the GitHub API                      |
| `GitHubToken`   | The GitHub token to authenticate with the GitHub API |

::: code-group

```csharp {3,9} [Usage]
class Build
    : NukeBuild,
        IHaveGitHubToken
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"GitHub base URL: {((IHaveGitHubToken)this).GitHubBaseUrl}");
        });
}
```

```bash {2-3} [Change the values with Parameters]
dotnet nuke \
   --github-base-url "https://api.github.com/" \
   --github-token "api-token"
```

```bash {1-2} [Change the values with Environment Variables]
export GITHUB_BASE_URL="https://api.github.com/"
export GITHUB_TOKEN="api-token"
dotnet nuke
```

:::

## Build Targets

The build target components extend the build with additional targets that can be executed individually
or as part of other targets.

Although, not required by NUKE, Ayaka provides a definition and one or more implementations for each target.
The definitions are named using the `IHave...Target` pattern, for example `IHaveGitHubReleaseTarget`. The
implementations use the `ICan...` pattern, for example `ICanGitHubRelease`.

This allows you to create custom targets that depend on a definition, but not on a strict implementation of it.
For example, an implementation from Ayaka does not suit your needs, and you want to create your own implementation
based on Ayaka's definition.

### ICanGitHubRelease

The `ICanGitHubRelease` build target generates release notes and then creates a GitHub release.

It does so by calling the GitHub API using the [`GitHubTasks`] build tasks.

By default, it uses repository information from the [`IHaveGitRepository`] build context
component and version information from the [`IHaveGitVersion`] build context component.

The release's tag and name will use the `v{Major.Minor.Patch}` pattern. If this does not suit your needs, you can
change this by providing custom settings.

The `ICanGitHubRelease` target implements the `IHaveGitHubReleaseTarget` build target definition.

::: code-group

```csharp {3,8} [Usage]
class Build
    : NukeBuild,
        ICanGitHubRelease
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .DependsOn<IHaveGitHubReleaseTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

```csharp {7-9,11-13} [Change the settings in Code]
class Build
    : NukeBuild,
        ICanGitHubRelease
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configure<GitHubReleaseNotesSettings> ICanGitHubRelease.GitHubReleaseNotesSettings
        => notes => notes
            .SetPreviousTag("v0.1.0");

    Configure<GitHubReleaseSettings> ICanGitHubRelease.GitHubReleaseSettings
        => release => release
            .SetDraft(true);

    Target Default => _ => _
        .DependsOn<IHaveGitHubReleaseTarget>()
        .Executes(() =>
        {
            // ...
        });
}
```

:::

[`GitHubTasks`]: ../build-tasks/github#releases
[`IHaveGitRepository`]: ./general#ihavegitrepository
[`IHaveGitVersion`]: ./general#ihavegitversion
