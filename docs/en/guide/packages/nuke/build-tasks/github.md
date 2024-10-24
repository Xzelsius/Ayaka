# GitHub Build Tasks

The GitHub builds tasks are methods that are meant be executed as part of a target.

It interacts with the GitHub API using the `Octokit` NuGet package provided by the people behind
[Octokit].

## Releases

`GitHubTasks` provides build tasks to interact with GitHub releases.

### Create Release

Using `CreateRelease()` you can create a new release on GitHub

This method takes a `GitHubReleaseSettings` object as an argument. The settings object has the following
properties which can be set:

| Parameter              | Required | Description                                                                                                                                           |
|------------------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------|
| `RepositoryOwner`      | `true`   | The account owner of the repository                                                                                                                   |
| `RepositoryName`       | `true`   | The name of the repository without the .git extension                                                                                                 |
| `Token`                | `true`   | The GitHub token used to authenticate with the GitHub API                                                                                             |
| `BaseUrl`              | `false`  | The base URL for the GitHub API. Defaults to the Octokit default value                                                                                |
| `Tag`                  | `true`   | The tag name for the release                                                                                                                          |
| `TargetCommitish`      | `false`  | The commitish value that will be the target for the release's tag                                                                                     |
| `Name`                 | `false`  | The name of the release. Required if `GenerateReleaseNotes` is `false`                                                                                |
| `Body`                 | `false`  | The text describing the contents of the release. Required if `GenerateReleaseNotes` is `false`                                                        |
| `Draft`                | `false`  | `true` to create a draft (unpublished) release, `false` to create a published one                                                                     |
| `PreRelease`           | `false`  | `true` to identify the release as a prerelease, `false` to identify it as a full release                                                              |
| `GenerateReleaseNotes` | `false`  | `true` to generate release notes automatically by GitHub. If `Body` is specified, the content will be pre-pended to the automatically generated notes |
| `ArtifactPaths`        | `false`  | The optional artifact paths to upload to the release                                                                                                  |

::: code-group

```csharp [Usage]
GitHubTasks.CreateRelease(
    new GitHubReleaseSettings()
        .SetRepositoryOwner("owner")
        .SetRepositoryName("repo")
        .SetToken("api-token")
        .SetTag("v1.0.0")
        .SetTargetCommitish("ca82a6dff817ec66f44342007202690a93763949")
        .SetName("Release v1.0.0")
        .SetBody("This is the release notes for v1.0.0"));
```

:::

### Generate Release Notes

Using `GenerateReleaseNotes()` you can generate release notes before creating a release on GitHub.

This method takes a `GitHubReleaseNotesSettings` object as an argument. The settings object has the following
properties which can be set:

| Parameter              | Required | Description                                                                                                                                                                                                                    |
|------------------------|----------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `RepositoryOwner`      | `true`   | The account owner of the repository                                                                                                                                                                                            |
| `RepositoryName`       | `true`   | The name of the repository without the .git extension                                                                                                                                                                          |
| `Token`                | `true`   | The GitHub token used to authenticate with the GitHub API                                                                                                                                                                      |
| `BaseUrl`              | `false`  | The base URL for the GitHub API. Defaults to the Octokit default value                                                                                                                                                         |
| `Tag`                  | `true`   | The tag name for the release                                                                                                                                                                                                   |
| `TargetCommitish`      | `false`  | The commitish value that will be the target for the release's tag                                                                                                                                                              |
| `PreviousTag`          | `false`  | The name of the previous tag to use as the starting point for the release notes                                                                                                                                                |
| `ConfigFile`           | `false`  | The optional path to a file in the repository containing the configuration settings used for generating the release notes. Defaults to `.github/release.yaml`. If that is not presenet, the default configuration will be used |

::: info
This will generate the same release notes as if you would create a release with `GenerateReleaseNotes`
set to `true`, but allows you to use a different configuration.
:::

::: code-group

```csharp [Usage]
GitHubTasks.GenerateReleaseNotes(
    new GitHubReleaseNotesSettings()
        .SetRepositoryOwner("owner")
        .SetRepositoryName("repo")
        .SetToken("api-token")
        .SetTag("v1.0.0")
        .SetTargetCommitish("ca82a6dff817ec66f44342007202690a93763949");
```

:::

## Pull Requests

`GitHubTasks` provides build tasks to interact with GitHub pull requests.

### Create Pull Request

Using `CreatePullRequest()` you can create a new pull request on GitHub.

This method takes a `GitHubPullRequestSettings` object as an argument. The settings object has the following
properties which can be set:

| Parameter              | Required | Description                                                                            |
|------------------------|----------|----------------------------------------------------------------------------------------|
| `RepositoryOwner`      | `true`   | The account owner of the repository                                                    |
| `RepositoryName`       | `true`   | The name of the repository without the .git extension                                  |
| `Token`                | `true`   | The GitHub token used to authenticate with the GitHub API                              |
| `BaseUrl`              | `false`  | The base URL for the GitHub API. Defaults to the Octokit default value                 |
| `Head`                 | `true`   | The name of the branch where your changes are implemented                              |
| `Base`                 | `true`   | The name of the branch you want your changes pulled into                               |
| `Title`                | `true`   | The title of the pull request                                                          |
| `Body`                 | `false`  | The body of the pull request                                                           |
| `Draft`                | `false`  | `true` to create a draft (unpublished) pull request, `false` to create a published one |

::: code-group

```csharp [Usage]
GitHubTasks.CreatePullRequest(
    new GitHubPullRequestSettings()
        .SetRepositoryOwner("owner")
        .SetRepositoryName("repo")
        .SetToken("api-token")
        .SetHead("feature-branch")
        .SetBase("main")
        .SetTitle("feat: add this super feature"));
```

:::

[Octokit]: https://github.com/octokit/octokit.net
