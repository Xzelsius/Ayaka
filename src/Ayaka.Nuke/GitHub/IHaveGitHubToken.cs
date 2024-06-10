// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common;
using global::Nuke.Common.CI.GitHubActions;

/// <summary>
///     Provides information about the GitHub credentials in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveGitHubToken
    : IHave
{
    /// <summary>
    ///     Gets the base URL for the GitHub API.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [Parameter]
    [ExcludeFromCodeCoverage]
    string? GitHubBaseUrl => TryGetValue(() => GitHubBaseUrl);

    /// <summary>
    ///     Gets the GitHub token used to authenticate with the GitHub API.
    /// </summary>
    [Parameter("The GitHub API token")]
    [Secret]
    [ExcludeFromCodeCoverage]
    string? GitHubToken => TryGetValue(() => GitHubToken)
                           ?? GitHubActions.Instance?.Token;
}
